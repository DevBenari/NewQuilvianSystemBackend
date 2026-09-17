# Laporan Perubahan Backend — `BE-RWI-115`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-115` |
| Judul | Pencatatan pemberian obat |
| Slice | Gelombang 3 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-115` |
| Trace | `FR-KEP-065`, `FR-KEP-067`, `FR-KEP-068`; `VAL-KEP-29f`, `VAL-KEP-30a`–`f`, `VAL-KEP-32a`–`c`, `VAL-KEP-33a`/`b`; state matrix 5.5; api-contract 7.11 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-114` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — rekam pemberian obat, revisi, keserentakan |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — tujuh kriteria terpetakan; `dotnet build` lolos; uji kiriman ulang `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / PharmacyManagement` |
| Registry / prefix | `Phm` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-SVC-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DEL-001`, `QBE-LOG-001`, `QBE-CODE-006` |
| Hak akses | `MedicationAdministration : Create` (mencatat), `: Update` (koreksi, evaluasi PRN) |
| Database | Nol migration tambahan (tabel dari K4) |

## 1. Masalah yang diperbaiki

Tanpa jalur pencatatan yang menegakkan isian, alasan, dan pelaksana dari akun login, catatan pemberian obat dapat
kosong, tercatat atas nama orang lain, tergandakan oleh tombol yang tertekan dua kali, atau ditimpa tanpa jejak.

## 2. Proses bisnis

1. Ns. Siti mencatat Ceftriaxone 08.00 → `PATCH /{id}/record` `DoseStatus=Administered`, 1 g, IV, 08.05. Server
   mengunci baris (`FOR UPDATE`), menegakkan episode berjalan dan penempatan unit, mengisi pencatat dari akun login.
2. 20.00 `Held` "pasien muntah, lapor DPJP" → alasan wajib.
3. **Jalur tidak normal:**
   - 2 g padahal resep 1 g tanpa catatan penyimpangan → `400` "Dosis atau waktu berbeda dari jadwal. Isi catatan penyimpangan."
     Aturan yang sama berlaku bila waktu di luar ±60 menit dari jadwal.
   - Dua perawat menyimpan dosis yang sama bersamaan → yang kedua `409` "Dosis ini sudah dicatat."
   - Tombol tertekan dua kali dengan `Idempotency-Key` sama → `200` dosis yang sama, `isReplay = true`.
   - Butir sudah dihentikan dan jadwal ≥ waktu henti → `409` "Resep obat ini sudah dihentikan dokter."
   - Butir insulin sliding scale → `409` "Insulin sliding scale dicatat lewat layar sliding scale."
   - Waktu pemberian > 5 menit di masa depan → `400`.
4. PRN: `POST /as-needed` — butir wajib `IsAsNeeded`, indikasi wajib; `PrnEvaluationDueAt` terisi bila interval dikonfigurasi.
   Evaluasi efek → `PATCH /{id}/prn-evaluation`.
5. Butir berfrekuensi tanpa jadwal: `POST /unscheduled` dengan alasan wajib (disimpan pada `DeviationNote`); ditolak
   `409` bila kode frekuensinya ternyata sudah punya jadwal.
6. Koreksi: `PUT /{id}/correct` — alasan dan `ExpectedRevisionNumber` wajib; nilai lama disimpan pada
   `PhmMedicationAdministrationRevision`, `RevisionNumber` naik. Dosis tidak pernah kembali `Due` dan tidak pernah dihapus.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Validation matrix 6.5, state matrix 5.5, api-contract 7.11, kamus data 11.12–11.13.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.Recording.cs` | Baru — `RecordAsync`, `RecordAsNeededAsync`, `RecordUnscheduledAsync`, `CorrectAsync`, `RecordPrnEvaluationAsync`, penguncian baris, kiriman ulang |
| `Areas/HealthServices/PharmacyManagement/Controllers/MedicationAdministrationController.cs` | Endpoint pencatatan, PRN, tanpa jadwal, koreksi, evaluasi PRN |
| `Areas/HealthServices/PharmacyManagement/DTOs/MedicationAdministrationDtos.cs` | Request pencatatan, koreksi, evaluasi, revisi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai 7.11. Delta: `RecordAsNeededAdministrationRequest` menerima `DeviationNote` opsional (`VAL-KEP-30c` berlaku pula pada PRN); koreksi dosis high-alert yang belum diberikan menjadi diberikan ditolak `409 HIGH_ALERT_CORRECTION_REQUIRES_DOUBLE_CHECK` supaya cek ganda tidak terlewati |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `StatusReason`, `DeviationNote`, `CorrectionReason`, `PrnIndication` tidak masuk payload logger |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Medication Administration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/record` | Mencatat hasil dosis `Due` | `MedicationAdministration : Create` |
| `POST` | `/as-needed` | Pemberian PRN | `MedicationAdministration : Create` |
| `POST` | `/unscheduled` | Pemberian butir tanpa jadwal terkonfigurasi | `MedicationAdministration : Create` |
| `PUT` | `/{id}/correct` | Koreksi beralasan dengan revisi | `MedicationAdministration : Update` |
| `PATCH` | `/{id}/prn-evaluation` | Evaluasi efek obat PRN | `MedicationAdministration : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Verifikasi kontrak API 7.11 terhadap DTO dan controller | Seluruh endpoint dan isian ada | `PASS` | Bagian 4 |
| Penelusuran kriteria 1–7 | Terpetakan | `PASS` | Bagian 6 |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Uji kiriman ulang dengan keluaran ditempel | Tidak dijalankan | `NOT RUN` | Build dan migration lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `Administered`/`Held`/`Refused`/`Missed` bersyarat isian dan alasan | Terpenuhi | `RecordAsync` — `VAL-KEP-30a`/`b` |
| 2. Pelaksana dari akun login | Terpenuhi | `RecordedByUserId = actorUserId`, `RecordedByEmployeeId` dari penjaga |
| 3. Kiriman ulang tidak menggandakan | Terpenuhi di source | `FindReplayAsync` + unique `IdempotencyKey` + kunci baris |
| 4. PRN indikasi dan evaluasi; tanpa jadwal beralasan | Terpenuhi | `RecordAsNeededAsync`, `RecordPrnEvaluationAsync`, `RecordUnscheduledAsync` |
| 5. Koreksi beralasan menyimpan revisi | Terpenuhi | `CorrectAsync` → `PhmMedicationAdministrationRevision` |
| 6. Dosis tidak pernah dihapus | Terpenuhi | Nol jalur `DELETE`; nol `Remove` pada tabel MAR |
| 7. Catatan penyimpangan saat berbeda dari jadwal | Terpenuhi | `IsDeviation` — dosis ≠ rencana atau ±60 menit |
| DoD: `dotnet build`, uji kiriman ulang | `dotnet build` lolos; **uji kiriman ulang dikecualikan atas keputusan pemilik 17 September 2026** | `PASS` sebagian; sisanya `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `Missed` hanya dicatat perawat; sistem tidak menandai `Missed` otomatis (`AC-MVP-027`) |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` |
| Langkah berikutnya | `BE-RWI-116`, `BE-RWI-117`, `BE-RWI-122` |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |
