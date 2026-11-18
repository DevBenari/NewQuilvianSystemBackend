# Laporan Perubahan Backend — `BE-HRD-FIX-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-FIX-004` |
| Judul | Rentang jam tarif lembur dicocokkan dengan jam UTC |
| Slice | Perbaikan cacat di luar roadmap; temuan sejenis butir (1) pada [BE-HRD-FIX-003](BE-HRD-FIX-003.md) bagian 5 |
| Roadmap | Tidak ada kartu. Dicatat pada `roadmap/backend-roadmap.md` bagian 24 di bawah `BE-HRD-358` |
| Trace | Pratinjau tarif lembur mandiri (`self-services/human-resource/overtime-requests/validate-preview`, draf, ubah, ajukan); pratinjau detail rencana lembur manajer; pratinjau dan perhitungan realisasi (`corporate/human-resource/overtime-management/realizations/requests/{id}/preview`, `/calculate`); master tarif `TimeBand` `Night`/`Custom` (`StartTime`/`EndTime`) |
| Contract version | `v26` tidak berubah. Bentuk request/response tetap; hanya jam pembanding rentang tarif yang dibetulkan |
| Dependency | `BE-HRD-FIX-003` ✅ (temuan; helper tanggal WIB di layanan lembur mandiri disatukan ke helper baru) |
| Klasifikasi | `MEDIUM` — skor 4: satu repository (0), sekitar 8 berkas diperiksa (1), 4 berkas source diubah (1), helper waktu bersama + tiga layanan lembur yang menentukan pengali bayaran (1), tanpa perubahan API (0), tanpa schema (0), tanpa keamanan (0), ditambah 1 untuk verifikasi provider-nyata |
| Task mode | `BACKEND` (perbaikan bug atas instruksi pengguna 17 November 2026: "perbaiki bug tarif lembur jam UTC") |
| Target tulis | `Helpers/AppDateTimeHelper.cs`; `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceService.cs`; `Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimePlanningService.cs`, `OvertimeAttendanceMatchingService.cs`; uji baru `Tests/QuilvianSystemBackend.HumanResourceTests/OvertimeRateClockWibTests.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357`, `353`–`356`, `FIX-002`, `FIX-003` yang belum di-commit; `AppDateTimeHelper.cs`, `OvertimePlanningService.cs`, `OvertimeAttendanceMatchingService.cs` bersih sebelum task ini |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — uji baru 6/6, proyek uji HR 246/246; kontrol negatif tiga mutasi semuanya tertangkap; lewat API devYoga pratinjau 22.00–23.00 WIB memilih tarif malam 2×, 14.00–15.00 memilih tarif hari kerja 1,5×; lembur 22.00–23.00 disetujui dengan pengali 2 dan pratinjau realisasi 60 menit tarif malam |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `SelfServices`, `Corporate`; helper bersama `Helpers` |
| Module / Submodule | `HumanResource` / lembur mandiri, `OvertimeManagement` (rencana, pencocokan presensi realisasi) |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru. `AppDateTimeHelper` = helper platform bersama (penambahan metode, tanpa mengubah metode lama) |
| Keberlakuan | `TOUCHED LEGACY` — perbaikan minimal tanpa refactor (legacy ratchet) |
| QBE yang berlaku | `QBE-SVC-001` (aturan bisnis tetap di service), konvensi waktu WIB proyek (`AppDateTimeHelper`) |
| Tidak berlaku | `QBE-API-001`/`QBE-DTO-001` (kontrak tidak berubah), `QBE-DB-*` (tanpa schema), `QBE-NAM-*` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

Master tarif lembur `TimeBand` `Night`/`Custom` menyimpan `StartTime`/`EndTime` sebagai jam dinding yang diisi petugas (WIB, mis.
22.00–06.00). Ketiga titik di bawah membandingkannya dengan `TimeOnly.FromDateTime(...)` atas nilai UTC, sehingga rentang
tarif meleset 7 jam.

| Titik | Akibat |
| --- | --- |
| `OvertimeSelfServiceService` — `OccurrenceTime` pratinjau tarif pengajuan mandiri | Lembur 22.00 WIB (= 15.00Z) tidak mendapat tarif malam; lembur 05.00–12.59 WIB (= 22.00–05.59Z) justru mendapat tarif malam. `RateMultiplierSnapshot` pada detail pengajuan ikut salah |
| `OvertimePlanningService` — `OccurrenceTime` pratinjau tarif detail rencana manajer | Sama, untuk rencana lembur |
| `OvertimeAttendanceMatchingService.BuildRateBreakdown` — jam tiap menit (`cursor`) saat memecah menit realisasi per tarif | Rincian tarif realisasi (dasar serah-terima payroll) memakai tarif yang salah untuk menit yang jatuh di rentang jam tarif |

Dampak hanya terjadi bila kebijakan punya tarif `Night`/`Custom`; tarif `AllDay` dan rentang menit (`FirstHour`/`NextHour`) tidak
terpengaruh.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Helpers/AppDateTimeHelper.cs` | Metode baru `UtcToLocal(DateTime utc)`: saat UTC (nilai tersimpan; `Unspecified` dari provider dianggap UTC) → jam dinding zona aplikasi (`Asia/Jakarta`). Metode lama tidak berubah. CRLF dipertahankan; +9 |
| `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceService.cs` | `OccurrenceTime = TimeOnly.FromDateTime(AppDateTimeHelper.UtcToLocal(startAt))`. `ToWibDate` (dari `FIX-003`) kini memakai helper yang sama; ruas `WibTimeZone` privat dihapus. CRLF dipertahankan |
| `Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimePlanningService.cs` | `OccurrenceTime` dalam jam WIB; `using` helper. CRLF dipertahankan; +3/−1 |
| `Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimeAttendanceMatchingService.cs` | `ResolveRate(..., TimeOnly.FromDateTime(AppDateTimeHelper.UtcToLocal(cursor)))`; `using` helper. CRLF dipertahankan; +3/−1 |
| `Tests/QuilvianSystemBackend.HumanResourceTests/OvertimeRateClockWibTests.cs` (baru) | 6 uji (3 teori × 2 kasus) lewat service nyata: kebijakan dengan tarif `AllDay` 1,5× dan `Night` 22.00–06.00 2,0× |

Tidak ada perubahan kontrak, schema, hak akses, atau validasi master tarif. Validasi tumpang-tindih rentang jam pada master tarif
(`OvertimeRateResolverService.ApplicabilityRangesOverlap`) membandingkan jam master dengan jam master, sehingga tidak terdampak.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `OvertimeRateClockWibTests` — lembur mandiri, `AfterShift` sesudah shift Pagi | 22.00–23.00 WIB → `OTR-MALAM`; 14.00–15.00 WIB → `OTR-HARIAN` | `PASS` | `dotnet test --filter OvertimeRateClockWibTests` 6/6 |
| — rencana lembur manajer (`PreviewDetailAsync`) | 22.00–23.00 WIB → `OTR-MALAM`; 10.00–11.00 WIB (= 03.00Z) → `OTR-HARIAN` | `PASS` | idem |
| — realisasi (`OvertimeAttendanceMatchingService.PreviewAsync`, kebijakan tanpa wajib presensi) | 22.00–23.00 WIB → 60 menit `OTR-MALAM`; 10.00–11.00 WIB → 60 menit `OTR-HARIAN` | `PASS` | idem |
| `OvertimeDateWibTests` (`FIX-003`) sesudah `ToWibDate` disatukan ke helper | 3/3 | `PASS` | idem |
| Kontrol negatif (tiga putaran terpisah; berkas dipulihkan dari salinan dan dibandingkan byte demi byte tiap putaran) | Lembur mandiri dikembalikan ke jam UTC → kasus 22.00 gagal (kasus 14.00 = 07.00Z memang bukan jam malam di kedua tafsiran); rencana → 2 kasus gagal; realisasi → 2 kasus gagal | `PASS` | Keluaran `dotnet test` per mutasi |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`) | 246/246 lulus (240 + 6 baru) | `PASS` | Keluaran `dotnet test` |
| API devYoga (PostgreSQL) — data uji sementara `UJI-A52-ASAP` (pola uji asap yang disetujui pengguna) ditambah tarif malam uji 22.00–06.00 2×; startup "Permission registry valid" (2457) | Pratinjau gilang 17 Nov 22.00–23.00 WIB `AfterShift` → "Tarif Malam", pengali 2, tanpa isu; 14.00–15.00 → "Tarif Hari Kerja", pengali 1,5 | `PASS` | `at-fix004.mjs`, `at-log.jsonl` ("fix004 …") |
| — Lembur 22.00–23.00: draf → ajukan → dr. Bima menyetujui | `ApprovedForWork`/`Completed`; `RateMultiplierSnapshot` detail = 2,0000 | `PASS` | idem |
| — Pratinjau realisasi oleh superadmin | `200`; rincian "Tarif Malam 2× 60 menit"; isu `ATTENDANCE_INCOMPLETE` (lembur belum terjadi, belum ada presensi — sesuai harapan) | `PASS` | idem |
| Pemulihan devYoga | Izin gilang (32 butir) dan dr. Bima (45 butir) dikirim ulang dan dibaca identik; workflow, langkah, kebijakan, 2 tarif, pengajuan, instance, riwayat, komentar, aksi persetujuan uji dihapus lunak — 0 baris uji aktif tersisa | `PASS` | `at-log.jsonl` ("fix004 Sisa aktif") |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tarif berentang jam dipilih menurut jam WIB pada pratinjau lembur mandiri | Terpenuhi | Harness + devYoga |
| Sama pada pratinjau rencana lembur manajer | Terpenuhi | Harness |
| Sama pada rincian tarif realisasi | Terpenuhi | Harness + devYoga |
| Tidak ada regresi | Terpenuhi | 246/246 |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Data lama | Pengajuan/rencana yang sudah tersimpan dengan `RateMultiplierSnapshot` keliru tidak dihitung ulang otomatis oleh perbaikan ini (perilaku hitung ulang realisasi yang sudah tersimpan tidak diperiksa). devYoga belum punya tarif `Night`/`Custom` di luar data uji (dihapus lunak), sehingga tidak ada data terdampak di sana; lingkungan lain belum diperiksa |
| Temuan sejenis yang tersisa (dari `FIX-003`, tidak diubah) | (2) `OvertimeActualCalculationService.cs` mengisi `ActualStartDate`/`ActualEndDate` dari tanggal UTC. (3) `OvertimePlanningService.cs` mengisi `PlannedEndDate` dari tanggal UTC bila tidak dikirim, dan `NormalizeUtc`-nya masih membaca jam tanpa zona sebagai UTC. (4) `OvertimeAttendanceMatchingService.cs` memakai tanggal UTC untuk jendela pencarian presensi. Juga `RequestedStartTime`/`RequestedEndTime` pada pengajuan dan rencana disimpan sebagai jam UTC (tampilan) |
| Keputusan pemilik (dari `FIX-003`, belum diputuskan) | Tanggal lembur sesudah shift malam |
| Risiko tersisa | Temuan sejenis di atas |
| Perubahan sampingan | `NONE`. Mutasi kontrol negatif dipulihkan; data uji devYoga dihapus lunak |
| Interupsi | `NONE` |
| Status Git | 4 berkas source diubah (`OvertimeSelfServiceService.cs` juga berisi perubahan `FIX-003`), 1 berkas uji baru, laporan ini (baru), dokumen roadmap/traceability/laporan `FIX-003`. Tidak ada commit |
