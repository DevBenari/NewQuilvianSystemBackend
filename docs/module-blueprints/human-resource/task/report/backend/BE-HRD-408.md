# Laporan Perubahan Backend — `BE-HRD-408`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-408` |
| Judul | State machine pembatalan SK; worker tidak mendahului keputusan |
| Slice | `S-FIN-M07` / Perpindahan Unit (alias `FIN-HRD-07-BE-002`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-408` |
| Trace | `HRD-DEC-057`; instruksi Menu 07 `FIN-HRD-07-BE-002` |
| Contract version | Kontrak SK Kepegawaian dan penempatan existing (`BE-HRD-024`, `BE-HRD-027`); tidak ada endpoint baru |
| Dependency | `BE-HRD-407` ✅ |
| Klasifikasi | `LIGHT` — verifikasi; penguatan bersama `BE-HRD-411` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/HrdPersonnelActionOrderService.cs` (lewat `BE-HRD-411`); laporan, roadmap, traceability |
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

Audit 19 November menyatakan alur sudah ada tetapi belum berbukti build. Penelusuran ulang menemukan satu celah waktu: worker membaca daftar SK `Scheduled` tanpa permohonan pembatalan, lalu menerapkannya satu per satu. Permohonan pembatalan yang masuk di antara dua langkah itu tidak terlihat oleh worker.

## 2. Proses bisnis

1. SK `Draft`/`PendingApproval` dapat dibatalkan langsung (`/cancel-request` atau `/cancel`).
2. SK `Scheduled` (belum efektif): petugas A mengajukan permohonan pembatalan; petugas B (bukan A) menyetujui lewat `/cancel`. `/cancel` tanpa permohonan → ditolak.
3. SK yang sudah masuk atau lewat tanggal efektif tidak dapat dibatalkan; SK `Applied` tidak dapat diubah — koreksi lewat SK baru berreferensi (counter-event).
4. Worker tidak menerapkan SK yang punya permohonan pembatalan; kini diperiksa ulang di dalam transaksi per SK, sehingga permohonan yang baru masuk tetap dihormati.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`CancelOrderAsync`, `RequestCancelOrderAsync`, `ProcessScheduledOrdersAsync`, `GetHospitalBusinessDate`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs` | Worker membaca ulang status dan permohonan pembatalan di dalam transaksi per SK (dikerjakan bersama `BE-HRD-411`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak berubah |
| Database | Tanpa migration |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Personnel Action Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/cancel-request` | Ajukan pembatalan | hak existing |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/cancel` | Setujui pembatalan (aktor berbeda) | hak existing |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning nullable dari build pertama diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | 3 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: `/cancel` SK `Scheduled` tanpa permohonan | Ditolak | `PASS` | `CancelOrderAsync` |
| Penelusuran: pemohon menyetujui pembatalan sendiri | Ditolak (SoD) | `PASS` | `CancelOrderAsync` |
| Penelusuran: permohonan masuk saat worker berjalan | SK dilewati worker | `PASS` | baca ulang di transaksi |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `/cancel` tanpa permintaan gagal | Terpenuhi | `CancelOrderAsync` |
| (2) Pemohon tidak dapat menyetujui sendiri | Terpenuhi | guard `CancellationRequestedByUserId` |
| (3) Worker tidak menerapkan SK yang pembatalannya menunggu | Terpenuhi | filter daftar + baca ulang per SK |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Pembuatan counter-event SK berreferensi untuk membalik SK `Applied` belum punya alur khusus; SK `Applied` tetap tidak dapat dibatalkan (sesuai outcome) |
| Langkah berikutnya | `BE-HRD-411` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
