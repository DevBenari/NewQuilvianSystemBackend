# Laporan Perubahan Backend — `BE-RWI-195`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-195` |
| Judul | Ringkasan ruang kerja, kelengkapan, dan peringatan Detail Episode |
| Slice | Slice B; gelombang `RWA-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-195` |
| Trace | `FR-RWA-001` s.d. `008`; `RWI-DEC-226`, `234`, `245`, `250`, `256`, `257`, `258`; `RWI-AC-344` s.d. `346`, `378`, `379`; G-30; API 12.1, 12.2, 12.3; state 10.2, 10.6; `INT-RWA-07`, `11`, `13` |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-193` ✅ |
| Klasifikasi | `HEAVY` — menyentuh Detail Episode (layar paling sering dibuka) |
| Task mode | `BACKEND` |
| Target tulis | `InpAdmissionCompletenessEvaluator.cs`, `InpAdmissionWorkspaceQueryService.cs`, `InpEpisodeService.cs`, `InpEpisodeService.Reads.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi API dua pasien dan uji kegagalan sumber **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` (`InpEpisodeService` konstruktor dan detail) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Petugas butuh satu ringkasan: siapa pasiennya, dokumen mana yang belum lengkap, dan apakah deposit kurang.
Detail Episode harus memperingatkan tanpa rupiah dan tidak boleh gagal karena sumber lain.

## 2. Proses bisnis

1. **Wajib dihitung.**
   - Selalu: Serah Terima, Gelang (dicetak minimal sekali), IPD (dicetak minimal sekali), Nilai
     Kepercayaan.
   - Bersyarat: Selisih Biaya bila penjamin utama asuransi/perusahaan; Pelunasan Deposit bila Billing
     mencatat kekurangan > 0.
   - Tidak dihitung: General Consent (*fail-closed*), Privasi, Label, dan Estimasi Biaya (sampai
     `BE-RWI-203`).
   - Contoh: Tn. Budi (asuransi, kurang Rp 3.000.000) wajib 6; Ny. Wati (tunai, deposit cukup) wajib 4.
2. Sumber aturan yang gagal membuat butirnya "tidak dapat dihitung" (lencana `Uncountable`), tidak ikut
   pembilang maupun penyebut.
3. **Menu** urutan V1 tanpa Assessment Edukasi dan MP Benefit (8 tampil; Estimasi Biaya tersembunyi):
   General Consent (`PrintOnly`), Serah Terima, Gelang & Label, Permintaan Privasi, IPD, Pelunasan
   Deposit, Selisih Biaya, Nilai Kepercayaan.
4. **Ketersediaan.** Episode `Draft` → `Availability = NotYetAdmitted`. `Closed`/`Cancelled` → `ReadOnly`
   beserta `ReadOnlyReason`.
5. **Rupiah.** `/summary` tanpa rupiah. `/summary/amounts` (pemegang `ViewAmount`) mengembalikan:
   - status deposit `NotRequired`/`Sufficient`/`Shortfall`/`Unavailable`, dengan minimum, diterima,
     dan kekurangan;
   - jatuh tempo surat lengkap yang terlewati.
6. **Detail Episode** (`GET episodes/{id}`), hanya untuk `Admitted`/`DischargePending`, menambah:
   - "Dokumen admisi belum lengkap: *n* (*nama*)";
   - "Pelunasan deposit jatuh tempo *tgl jam* terlewati — lihat kasir";
   - kegagalan apa pun → "Kelengkapan dokumen admisi tidak dapat dihitung", dan detail tetap `200`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpEpisodeService.GetEpisodeDetailAsync`/`GetDetailResponseAsync`, `IInpBillingDepositAdapter`, `OperatingRoomCaseService`,
state 10.2, frontend 14.2 (urutan menu).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpAdmissionCompletenessEvaluator.cs` | Baru — `EvaluateAsync`, `BuildEpisodeWarningsAsync`, `BuildWarnings` |
| `Services/InpAdmissionWorkspaceQueryService.cs` | `GetSummaryAsync`, `GetSummaryAmountsAsync` |
| `Services/InpEpisodeService.cs` | Konstruktor menerima `InpAdmissionCompletenessEvaluator` |
| `Services/InpEpisodeService.Reads.cs` | `GetEpisodeDetailAsync` menambah peringatan untuk `Admitted`/`DischargePending` |
| `Controllers/InpatientAdmissionDocumentController.cs` | `GET /summary`, `/summary/amounts` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.1 (`Warnings` Detail Episode bertambah); 12.2 dua endpoint |
| Database | Tidak ada |
| Keamanan/Auth | Rupiah hanya lewat `ViewAmount` (`RWI-DEC-258`) |

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/admission-workspace/summary` | Header, sumber, menu, kelengkapan, peringatan tanpa rupiah | `InpatientAdmissionDocument : Read` |
| `GET` | `…/admission-workspace/summary/amounts` | Status deposit berupiah, jatuh tempo terlewati | `InpatientAdmissionDocument : ViewAmount` |
| `GET` | `api/v1/health-services/inpatient-management/episodes/{id}` | `Warnings` bertambah (tanpa rupiah) | `InpatientEpisode : Read` (tetap) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight; dependency melingkar | Evaluator tidak bergantung balik pada `InpEpisodeService`; tidak ada `new InpEpisodeService(` | `PASS` | Pemeriksaan konstruktor dan pencarian |
| Gagal aman Detail Episode | `BuildEpisodeWarningsAsync` menangkap seluruh kegagalan selain pembatalan | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Dua pasien samaran; kegagalan sumber | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Asuransi + kekurangan → 6; tunai + cukup → 4; GC/Privasi/Label/Estimasi tidak dihitung | Terpenuhi (source) | `EvaluateAsync` |
| 2. Menu urutan V1, lencana state 10.2 | Terpenuhi (source) | Delapan menu; Estimasi tersembunyi |
| 3. `NotYetAdmitted` untuk `Draft`, `ReadOnly` untuk `Closed`/`Cancelled` | Terpenuhi (source) | `GetSummaryAsync` |
| 4. `/summary` tanpa rupiah; `/amounts` `403` tanpa `ViewAmount`; Billing gagal → `Unavailable`, butir deposit `Uncountable` | Terpenuhi (source) | DTO tanpa isian rupiah; atribut; evaluator |
| 5. Peringatan Detail Episode tanpa rupiah; sumber gagal → kalimat "tidak dapat dihitung", detail `200` | Terpenuhi (source) | `InpEpisodeService.Reads.cs`; evaluator |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Detail Episode kini membaca penjamin, deposit, dan profil rumah sakit untuk episode berjalan. Panggilan ini menambah waktu baca (`NFR-RWA-16`) |
| Masalah yang diketahui | Delta kontrak (aditif):<br>• `Sources.OperatingRoom` selalu `NotYetAvailable` selama aturan Estimasi belum dikirim;<br>• `Completeness.UncountableNames[]` tambahan |
| Risiko tersisa | Kinerja Detail Episode belum diukur |
| Perubahan sampingan | Konstruktor `InpEpisodeService` bertambah satu parameter (dibentuk DI) |
| Interupsi | `NONE` |
| Status Git | `??` evaluator; `M` `InpEpisodeService.cs`, `InpEpisodeService.Reads.cs` |
| Langkah berikutnya | Uji pengguna dua pasien; ukur waktu `GET episodes/{id}` |
