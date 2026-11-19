# Laporan Perubahan Backend — `BE-HRD-FIX-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-FIX-003` |
| Judul | Tanggal lembur mandiri dibandingkan dalam UTC |
| Slice | Perbaikan cacat di luar roadmap; temuan uji asap cuti/lembur 17 November 2026 (dicatat di [BE-HRD-FIX-002](BE-HRD-FIX-002.md) bagian 5) |
| Roadmap | Tidak ada kartu. Dicatat pada `roadmap/backend-roadmap.md` bagian 24 di bawah `BE-HRD-358` |
| Trace | `POST self-services/human-resource/overtime-requests/validate-preview`, `POST self-services/human-resource/overtime-requests` (draf), ubah, dan ajukan — semuanya memakai evaluasi yang sama; konvensi waktu WIB (`AppDateTimeHelper.ToUtc`) |
| Contract version | `v26` tidak berubah. Bentuk request/response tetap; hanya penafsiran tanggal yang dibetulkan |
| Dependency | `BE-HRD-FIX-002` ✅ (uji asap lembur lewat workflow yang menemukan cacat ini) |
| Klasifikasi | `SMALL` — skor 2: satu repository (0), sekitar 6 berkas diperiksa (1), 1 berkas source diubah (0), satu layanan lembur mandiri (0), tanpa perubahan API (0), tanpa schema (0), tanpa keamanan (0), ditambah 1 untuk verifikasi provider-nyata |
| Task mode | `BACKEND` (perbaikan bug atas instruksi pengguna 17 November 2026: "perbaiki bug tanggal lembur UTC") |
| Target tulis | `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceService.cs`; uji baru `Tests/QuilvianSystemBackend.HumanResourceTests/OvertimeDateWibTests.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357`, `353`–`356`, `FIX-002` yang belum di-commit; `OvertimeSelfServiceService.cs` bersih sebelum task ini |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — uji baru 3/3, proyek uji HR 240/240; kontrol negatif tiga mutasi semuanya tertangkap; lewat API devYoga lembur 05.00–07.00 WIB divalidasi tanpa isu, diajukan, dan disetujui (`ApprovedForWork`/`Completed`) dengan jam tersimpan benar |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `SelfServices` |
| Module / Submodule | `HumanResource` / lembur mandiri (`OvertimeSelfService`) |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY` — perbaikan minimal tanpa refactor (legacy ratchet) |
| QBE yang berlaku | `QBE-SVC-001` (aturan bisnis tetap di service), konvensi waktu WIB proyek (`AppDateTimeHelper`) |
| Tidak berlaku | `QBE-API-001`/`QBE-DTO-001` (kontrak tidak berubah), `QBE-DB-*` (tanpa schema), `QBE-NAM-*` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat | Akibat |
| --- | --- |
| **Cek tanggal dalam UTC.** Evaluasi lembur membandingkan `OvertimeDate` (tanggal kalender WIB) dengan `DateOnly.FromDateTime(startAt)`, padahal `startAt` sudah UTC | Lembur yang mulai 00.00–06.59 WIB (tanggal UTC = kemarin) **selalu ditolak** `OVERTIME_DATE_MISMATCH`, mis. BeforeShift 05.00–07.00 sebelum shift Pagi. Sebaliknya, tanggal mulai yang memang salah (mis. tanggal 18, mulai 19 pukul 05.00 WIB = 18 pukul 22.00Z) lolos cek tanggal |
| **`PlannedEndDate` dalam UTC.** `DateOnly.FromDateTime(endAt)` | Lembur yang lewat tengah malam WIB tetapi belum lewat tengah malam UTC (mis. 23.00–01.00 WIB) tercatat selesai pada tanggal yang sama |
| **Jam tanpa zona dibaca UTC.** `NormalizeUtc` memberi `DateTimeKind.Utc` begitu saja pada nilai `Unspecified`, berbeda dari `AppDateTimeHelper.ToUtc` yang membacanya sebagai jam dinding WIB | Klien yang mengirim `"2026-11-18T19:00:00"` tanpa zona tersimpan 02.00 WIB keesokan harinya. Klien yang mengirim `Z`/`+07:00` (seperti FE dan skrip uji) tidak terdampak |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceService.cs` | Helper baru `ToWibDate(utc)` (konversi ke `Asia/Jakarta` lalu ambil tanggal). Cek tanggal memakai `ToWibDate(startAt) != request.OvertimeDate`; `PlannedEndDate` memakai `ToWibDate(endAt)`. `NormalizeUtc` kini `DateTime.SpecifyKind(AppDateTimeHelper.ToUtc(value), DateTimeKind.Utc)` (Utc tetap, Local dikonversi, Unspecified dibaca WIB). `using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;`. CRLF dipertahankan (1401 CR = 1401 baris); +14/−8 |
| `Tests/QuilvianSystemBackend.HumanResourceTests/OvertimeDateWibTests.cs` (baru) | 3 uji pratinjau lewat service nyata (resolver jadwal, kebijakan, tarif, period guard; integrasi workflow tidak dipakai pratinjau): fixture shift Pagi 07.00–14.00, kebijakan + tarif + definisi workflow lembur aktif |

Tidak ada perubahan kontrak, schema, hak akses, atau layanan lain. Cek kategori (`BeforeShift`/`AfterShift`) dan tumpang-tindih jam kerja
sudah membandingkan instan UTC, sehingga tidak perlu diubah.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `OvertimeDateWibTests` — lembur 05.00–07.00 WIB `BeforeShift` | Tanpa `OVERTIME_DATE_MISMATCH`, patuh kebijakan, `OvertimeDate` = `PlannedEndDate` = hari kerja | `PASS` | `dotnet test --filter OvertimeDateWibTests` 3/3 |
| — jam tanpa zona `05:00` | `PlannedStartAt` = 22.00Z hari sebelumnya (dibaca WIB); tanpa isu tanggal/kategori | `PASS` | idem |
| — regresi: tanggal 18, mulai 19 pukul 05.00 WIB | Tetap `OVERTIME_DATE_MISMATCH`; `AfterShift` 14.00–16.00 patuh; 23.00–01.00 WIB → `PlannedEndDate` = hari berikutnya | `PASS` | idem |
| Kontrol negatif (tiga putaran terpisah, berkas dipulihkan dan dibandingkan byte demi byte tiap putaran) | Cek tanggal dikembalikan ke UTC → 3/3 gagal; `NormalizeUtc` dikembalikan → uji jam tanpa zona gagal; `PlannedEndDate` dikembalikan → uji regresi gagal | `PASS` | Keluaran `dotnet test` per mutasi |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`) | 240/240 lulus (237 + 3 baru) | `PASS` | Keluaran `dotnet test` |
| API devYoga sebelum perbaikan (uji asap `FIX-002`) | Pratinjau 05.00–07.00 WIB 18 Nov → `OVERTIME_DATE_MISMATCH` | Bukti cacat | `at-log.jsonl` ("asap2 …") |
| API devYoga sesudah perbaikan — data uji sementara `UJI-A52-ASAP` (pola uji asap yang disetujui pengguna: workflow satu langkah penyetuju dr. Bima, kebijakan + tarif lembur global, izin `MyOvertime` sementara pada jabatan gilang dan `ApprovalInbox` pada jabatan dr. Bima); startup "Permission registry valid" (2457) | Pratinjau 05.00–07.00 WIB 18 Nov `BeforeShift` → `200`, **tanpa isu**. Kontrol: tanggal 18, mulai 19 pukul 05.00 WIB → `OVERTIME_DATE_MISMATCH` (+ `CATEGORY_SCHEDULE_MISMATCH`, `REGULAR_WORK_OVERLAP`). Regresi 19.00–21.00 → valid | `PASS` | `at-fix003.mjs`, `at-log.jsonl` ("fix003 …") |
| — Lembur A 05.00–07.00 WIB: draf → ajukan → dr. Bima menyetujui dari kotak persetujuan | `201` → `200` → `200`; `ApprovedForWork`/`Completed`; tersimpan `OvertimeDate` 2026-11-18, `PlannedStartAt` 2026-11-17 22.00Z, `PlannedEndAt` 2026-11-18 00.00Z (= 05.00–07.00 WIB) | `PASS` | idem |
| — Lembur B 19.00–21.00: ajukan → tarik | `Cancelled`/`Withdrawn` | `PASS` | idem |
| Pemulihan devYoga | Izin gilang (32 butir) dan dr. Bima (45 butir) dikirim ulang dan dibaca identik; workflow, langkah, kebijakan, tarif, pengajuan, instance, riwayat, komentar, aksi persetujuan uji dihapus lunak — 0 baris uji aktif tersisa | `PASS` | `at-log.jsonl` ("fix003 Sisa aktif") |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Lembur yang mulai 00.00–06.59 WIB tidak lagi ditolak karena tanggal | Terpenuhi | Harness + devYoga |
| Tanggal yang memang tidak cocok tetap ditolak | Terpenuhi | Harness + devYoga (kontrol) |
| Jam tanpa zona dibaca WIB, sesuai konvensi proyek | Terpenuhi | Harness |
| Tidak ada regresi | Terpenuhi | 240/240 |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Koreksi catatan `FIX-002` | Catatan "akibat ikutan: lembur AfterShift sesudah shift malam tidak dapat diajukan pada tanggal shift" **bukan akibat cacat UTC**: shift Malam 18 Nov berakhir 19 Nov 07.00 WIB (= 00.00Z, tanggal UTC dan WIB sama-sama 19). Perilaku itu tetap ada sesudah perbaikan ini — lihat baris berikut |
| Keputusan pemilik (belum diputuskan) | **Tanggal lembur sesudah shift malam.** Aturan saat ini: `OvertimeDate` = tanggal WIB jam mulai, dan cek `AfterShift` memakai jadwal pada `OvertimeDate`. Lembur sesudah shift Malam 18 Nov (mulai 19 Nov 07.00) ditolak bila tanggal diisi 18 (`OVERTIME_DATE_MISMATCH`) dan ditolak bila diisi 19 (`CATEGORY_SCHEDULE_MISMATCH`, karena jadwal 19 Nov tidak berakhir 07.00). Perlu diputuskan apakah lembur dicatat pada tanggal shift; bila ya, aturan tanggal dan pencarian jadwal perlu task terpisah |
| Dampak klien | Layar lembur mandiri FE (`QuilvianSystemFrontendDev`, branch `YogaV2`) masih mengirim `startTime`/`endTime`, belum memakai kontrak `plannedStartAt`/`plannedEndAt` ini, sehingga belum ada klien yang terdampak perubahan penafsiran jam tanpa zona |
| Temuan sejenis (tidak diubah, di luar cakupan) | Penelusuran `DateOnly/TimeOnly.FromDateTime(...)` atas nilai UTC di layanan lembur: (1) **diperbaiki di [`BE-HRD-FIX-004`](BE-HRD-FIX-004.md)** — `OvertimeSelfServiceService` sendiri mengirim `OccurrenceTime = TimeOnly.FromDateTime(startAt)` (jam UTC) ke `OvertimeRateResolverService`, sehingga tarif berentang jam (`UsesClockRange`, mis. tarif malam) dicocokkan 7 jam meleset — tidak berdampak selama tarif yang dipakai `AllDay`/rentang menit; `OvertimePlanningService.cs:932` sama. (2) `OvertimeActualCalculationService.cs:203,218–219` mengisi `ActualStartDate`/`ActualEndDate` dari tanggal UTC. (3) `OvertimePlanningService.cs:1047` mengisi `PlannedEndDate` dari tanggal UTC bila tidak dikirim. (4) `OvertimeAttendanceMatchingService.cs:98–99,958–959` memakai tanggal UTC untuk jendela pencarian presensi. Masing-masing perlu task perbaikan terpisah |
| Risiko tersisa | Temuan sejenis (2)–(4) di atas belum diperbaiki |
| Perubahan sampingan | `NONE`. Mutasi kontrol negatif dipulihkan; data uji devYoga dihapus lunak |
| Interupsi | `NONE` |
| Status Git | 1 berkas source diubah, 1 berkas uji baru, laporan ini (baru), dokumen roadmap/traceability/laporan `FIX-002`. Tidak ada commit |
