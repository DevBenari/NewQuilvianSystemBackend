# Laporan Perubahan Backend — `BE-HRD-411`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-411` |
| Judul | Worker penerapan SK terjadwal: tanggal bisnis, idempotensi, observabilitas |
| Slice | `S-FIN-M07` / Perpindahan Unit (alias `FIN-HRD-07-BE-005`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-411` |
| Trace | `HRD-DEC-050`, `HRD-DEC-057`; instruksi Menu 07 `FIN-HRD-07-BE-005` |
| Contract version | Kontrak SK Kepegawaian dan penempatan existing (`BE-HRD-024`, `BE-HRD-027`); tidak ada endpoint baru |
| Dependency | `BE-HRD-408` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/HrdPersonnelActionOrderService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build dan QBE lulus; uji worker `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Tanggal bisnis sudah dihitung dari zona waktu `SE Asia Standard Time`/`Asia/Jakarta` (cadangan terakhir +7 jam hanya bila zona tidak tersedia). Celah: **dua tick atau dua instance worker** dapat mengambil SK yang sama dari daftar dan menerapkannya dua kali (penempatan ganda), karena tidak ada klaim per SK.

## 2. Proses bisnis

1. Worker menghitung tanggal bisnis RS (WIB). SK `Scheduled` dengan tanggal efektif ≤ tanggal bisnis dan tanpa permohonan pembatalan masuk daftar.
2. Per SK: periode absensi tanggal efektif sudah `Closed` → penerapan ditunda dan dicatat di `Notes`.
3. Di dalam transaksi per SK diambil kunci per SK, lalu status dan permohonan pembatalan dibaca ulang; bila sudah bukan `Scheduled` atau ada permohonan pembatalan → dilewati.
4. Penerapan gagal pada satu SK dicatat di `Notes` tanpa menggagalkan SK lain.

**Contoh tanggal.** 19 November 2026 17.01 UTC = 20 November 00.01 WIB → SK bertanggal efektif 20 November diterapkan pada tick tersebut.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ProcessScheduledOrdersAsync`, `GetHospitalBusinessDate`, `ApplyOrderAssignmentsAsync`, worker pemanggil.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs` | Klaim per SK (kunci + baca ulang status/pembatalan) di dalam transaksi worker |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Tanpa migration |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

Tidak ada endpoint — proses latar `ProcessScheduledOrdersAsync`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning nullable dari build pertama diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | 3 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: 00.01 WIB vs UTC hari sebelumnya | Tanggal bisnis = tanggal WIB | `PASS` | `GetHospitalBusinessDate` |
| Penelusuran: dua worker serentak | SK kedua dilewati sesudah baca ulang | `PASS` | kunci per SK |
| Penelusuran: SK efektif besok | Tidak diterapkan; profil tidak berubah | `PASS` | filter `EffectiveDate <= businessToday` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) WIB 00:01 vs UTC hari sebelumnya benar | Terpenuhi | konversi zona waktu |
| (2) Dua tick/worker tidak menerapkan dua kali | Terpenuhi | kunci + baca ulang per SK |
| (3) Mirror profil tidak berubah sebelum efektif | Terpenuhi | penerapan hanya pada SK jatuh tempo |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Penundaan karena periode absensi `Closed` masih berupa catatan `Notes`, belum status eskalasi tersendiri |
| Langkah berikutnya | `BE-HRD-412` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
