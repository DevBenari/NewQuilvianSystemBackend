# Laporan Perubahan Backend — `BE-IGD-067`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-067` |
| Judul | Lini masa SOAP per encounter untuk kunjungan IGD |
| Slice | `EPIC IGD-14` / `MVP-9` — catatan dokter per kunjungan |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-098`; `AT-IGD-203` (bagian baca riwayat); DoD PRD §10.4 butir 2, 4 |
| Keputusan | `IGD-DEC-226` (baca catatan per encounter); `IGD-DEC-221` (kunjungan IGD adalah encounter tanpa antrean) |
| Contract version | API **`0.15.0`** §10.1 nomor 4, §10.4; validation **`0.14.0`** §12.4 aturan 14; permission/audit **`0.8.0`** §9.1 baris `DoctorConsultation` |
| Dependency | `BE-IGD-065` ✅ (baseline saringan kunjungan IGD) |
| Klasifikasi | `MEDIUM` — Controller endpoint baru pada controller yang ada, DTO respons baru, reuse entity dan mapper yang ada |
| Task mode | `BACKEND` — izin implementasi diberikan Rizki |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `DoctorConsultationController.cs`, `DoctorConsultationDtos.cs`, laporan ini, roadmap, requirement traceability |
| Model | Claude Opus 5.5 / Antigravity |
| Tanggal | 8 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 8 Oktober 2026: implementasi selesai dan dibuild pemilik; 3 dari 6 acceptance terbukti** (4 lewat diff, 5, 6). Dua berkas diubah; nol migration; nol komentar baru; CRLF tanpa BOM. Build pemilik: `dotnet build` 0 error, 235 warning (identik baseline; dilaporkan 8 Oktober 2026). Menunggu putaran uji 1 bersama `FE-IGD-047` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `Cln` / `DoctorConsultation`, `BUSINESS DOMAIN / MODULE` — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Applicability | `NEW CODE` pada controller dan DTO yang ada (aditif); nol entity baru, nol migration |
| QBE yang berlaku | `QBE-API-001` (route `encounters/{encounterId:guid}/soap-timeline`, envelope `ApiResponse<EncounterSoapTimelineResponse>`), `QBE-DTO-001` (DTO keluar tanpa ekspos entity), `QBE-PERM-001` (`[AccessAction("Read", ...)]`, `[AccessPermission("DoctorConsultation", "Read")]`), `QBE-SVC-001` (efisiensi kueri LINQ) |
| Checker | `powershell -NoProfile -ExecutionPolicy Bypass -File tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` — **VIOLATION: 0, REVIEW: 0, INFO: 0, Final result: PASS** |

---

## 1. Masalah yang Diperbaiki

Sebelumnya, lini masa catatan SOAP dokter hanya dapat dimuat berdasarkan `episodeId` rawat inap (`GET episodes/{episodeId}/soap-timeline`). Pasien IGD yang belum atau tidak dirawat inap tidak memiliki `InpEpisode`, sehingga layar dokter IGD (`FE-IGD-047`) tidak dapat memuat riwayat catatan SOAP satu kunjungan. Endpoint baru `GET encounters/{encounterId}/soap-timeline` menyediakan lini masa SOAP berbasis encounter yang tervalidasi merupakan bagian dari kunjungan IGD aktif atau selesai.

---

## 2. Perubahan yang Dikerjakan

### 2.1 Berkas yang Diubah

1. `Areas/HealthServices/ClinicalManagement/DTOs/DoctorConsultationDtos.cs`:
   - Menambahkan DTO `EncounterSoapTimelineResponse` beranggotakan: `EncounterId`, `EmergencyVisitId`, `PatientId`, `TotalCount`, dan `Items` (berisi list of `SoapTimelineItemResponse`).
2. `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs`:
   - Menambahkan endpoint `[HttpGet("encounters/{encounterId:guid}/soap-timeline")]` (`GetSoapTimelineByEncounter`).
   - Pengecekan validitas:
     - 404 Not Found bila encounter tidak ada: *"Encounter pasien tidak ditemukan."*
     - 400 Bad Request bila encounter bukan milik kunjungan IGD: *"Lini masa catatan dokter per encounter hanya berlaku untuk kunjungan IGD."*
     - 200 OK dengan `EncounterSoapTimelineResponse` berisi riwayat konsultasi dokter berstatus aktif (non-deleted).

---

## 3. Kriteria Penerimaan & Status

| # | Kriteria | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Encounter kunjungan IGD dengan catatan SOAP → 200, `totalCount` sesuai, items `SoapTimelineItemResponse` | 🟡 | Menunggu uji API putaran 1 |
| 2 | Encounter kunjungan IGD tanpa catatan → 200, items kosong | 🟡 | Menunggu uji API putaran 1 |
| 3 | Encounter bukan milik kunjungan IGD → 400; encounter tidak ada → 404 | 🟡 | Menunggu uji API putaran 1 |
| 4 | Regresi: `GET episodes/{episodeId}/soap-timeline` rawat inap tidak berubah | ✅ | Diff penalaran: endpoint episode lama tidak disentuh |
| 5 | Diff dua berkas pada Cakupan; nol komentar baru; CRLF tanpa BOM | ✅ | `git diff` & script byte check |
| 6 | Build 0 error; warning dilaporkan | ✅ | Build pemilik 8 Oktober 2026: 0 error, 235 warning (identik baseline 235 warning) |

---

## 4. Perintah Kompilasi Backend untuk Pemilik

```powershell
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```
