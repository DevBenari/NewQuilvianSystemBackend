# Laporan Perubahan Backend — `BE-RWI-224`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Judul | BE-RWI-224 — Kelas layanan dari riwayat penempatan |
| Slice / Trace | PLAN-REPAIR-DOK-006; FIX-DOK-006-02/06; A-01–A-05; K-01 |
| Roadmap | [backend-roadmap-kelas-perawatan.md](../../../roadmap/backend-roadmap-kelas-perawatan.md) |
| Contract version | [RWI-KELAS-006-1.0](../../../../dokter-rawat-inap/contracts/kelas-perawatan-repair-006.md), disetujui pemilik; SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112` |
| Approval | Instruksi seluruh task sampai tuntas dan jawaban K-01/K-02 pada 2026-10-12 |
| Dependency | Tidak ada |
| Klasifikasi | HEAVY — repo 2, baca 2, ubah 1, logika 1, API 2, DB 1, auth 0, UI 0 = 9 |
| Task mode / Target tulis | CROSS-REPO; source backend scope task dan dokumentasi blueprint pemilik; tidak ada DB/deploy |
| Model | Codex berbasis GPT-6 |
| Commit backend / frontend rujukan | Basis `d908988248f6652ed7f2ce84aae9eb3e38dd36a0` / `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`. Source repair di-commit pemilik pada `8551291f` / `2d0a6890e`; penutupan diverifikasi terhadap HEAD `4ec02370` / `f4495aa14` (merge IGD/Finance, tidak menyentuh berkas repair) ditambah perbaikan penutupan pada `EncounterInsuranceService.cs` yang belum di-commit |
| Tanggal / Status | 2026-10-13 / ✅ SELESAI — divalidasi ulang pada sesi penutupan 13 Oktober 2026, termasuk perbaikan label kelas |

## 1. Masalah yang diperbaiki

Kelas snapshot encounter tetap kelas awal sesudah transfer. Pembayaran/konfirmasi dapat memilih kelas manual. Nilai tersebut tidak mewakili kelas penempatan pada waktu setiap layanan.

## 2. Proses bisnis

Petugas memilih bed. OpenAdmissionRequest.SelectedBedId membuat server membaca ulang kelas/unit kamar, sementara booking menyelaraskan kelas draft. Konfirmasi tidak dapat mengganti kelas manual. Saat pasien ditempatkan, periode kamar menjadi sumber kelas layanan. Resolver memakai interval inklusif awal/eksklusif akhir; riwayat transfer berakhir sah, koreksi pengganti dan baris dihapus dikecualikan. Sebelum placement hasil null; rawat jalan/IGD tetap kelas encounter.

## 3. Perubahan yang dikerjakan

Berkas yang diperiksa: source pada tabel, caller Clinical/Pharmacy/Billing/admisi, blueprint/issue/plan, AGENTS.md kedua repo, aturan global dan backend, engineering contract serta registry canonical. Tidak memakai aturan `agents/rules/` legacy sebagai wewenang.

| Berkas | Perubahan |
| --- | --- |
| [InpServiceClassResolver.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpServiceClassResolver.cs) | Resolver read-only kelas menurut timestamp/riwayat placement; tidak memakai IsSuperseded sebagai filter transfer. |
| [InpEpisodeService.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs) | Validasi SelectedBedId dan kelas/unit server; UpdateAdmission kelas dari reservasi atau nilai server, bukan input manual. |
| [InpBedOccupancyService.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs) | ROOM_CLASS_MISSING 422; booking menyelaraskan kelas draft; placement tanpa kelas tidak fallback ke episode. |
| [InpatientEpisodeDtos.cs](../../../../../../../Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs) | SelectedBedId opsional pada OpenAdmissionRequest, tanpa perubahan schema. |
| [EncounterInsuranceService.cs](../../../../../../../Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs) | Seluruh konteks tunai/asuransi/perusahaan/kandidat membawa kelas efektif dan ServiceOccurredAt. **Perbaikan penutupan 13 Oktober 2026:** pada konteks asuransi dan perusahaan, `PatientClassName` sebelumnya masih diambil dari kelas awal encounter dan bisa jatuh ke `paymentSource.ClassNameSnapshot` (kelas polis). Kini keduanya memakai `serviceClass.Name`, sama dengan konteks tunai dan kandidat. |
| [Program.cs](../../../../../../../Program.cs) | Registrasi scoped InpServiceClassResolver saja; perubahan lain yang sudah ada dipertahankan. |

### Dampak kontrak, database, keamanan dan preflight

| Aspek | Hasil |
| --- | --- |
| Area / Module / Submodule / prefix / keberlakuan | Health Services / Inpatient Management (Inp, ACTIVE, NEW CODE resolver / TOUCHED LEGACY lainnya); Clinical Management (Cli, ACTIVE, TOUCHED LEGACY) |
| Registry | Pemilik terdaftar; tidak membuat entity/folder persisted baru. QBE-MOD-002/003 tidak memerlukan pendaftaran baru |
| QBE ID berlaku | QBE-MOD-001, QBE-SVC-001, QBE-API-001, QBE-DTO-001, QBE-VAL-001, QBE-DEL-001, QBE-AUD-001; permission endpoint existing ditinjau QBE-PERM-001 |
| Kontrak API | POST episodes menambah SelectedBedId opsional. PUT episodes mempertahankan/menurunkan kelas server. 400 untuk konteks tidak sah, 422 pada booking/placement kamar tanpa kelas, 409 lifecycle/konflik existing. |
| Database / migration | Query/perilaku existing saja; tidak mengubah schema/entity, tidak membuat migration, tidak menjalankan database |
| Keamanan/Auth | Metadata Access existing dipertahankan; tidak menambah role hardcode atau melemahkan permission |
| Arsitektur / audit | Resolver baru `InpServiceClassResolver` di service module Inpatient, DI scoped; AsNoTracking pada pembacaan. Write existing (booking menyelaraskan kelas draft) mempertahankan transaksi, actor Create/Update, dan lifecycle. Tidak membuat generic repository |

Checker repository hanya mencakup subset QBE otomatis. Review source/kontrak melengkapi aturan review-only. Registry suite canonical berbeda pada entri Finance/Nutrition yang tidak disentuh; ownership Clinical/Pharmacy/Inpatient/Billing yang dipakai task tetap cocok.

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| POST / PUT | /api/v1/health-services/inpatient-management/episodes / {id} | Buka/perbarui draft sesuai kamar | InpatientEpisode : Create / Update |

## 5. Verifikasi

| Perintah/skenario | Hasil | Klasifikasi/bukti |
| --- | --- | --- |
| `dotnet restore QuilvianSystemBackend.csproj --nologo` | Berhasil; 5 peringatan paket ImageSharp NU1902/NU1903 existing | PASS, 2026-10-12 |
| `dotnet build QuilvianSystemBackend.csproj --no-restore --nologo -o "$env:TEMP/quilvian-kelas-be-build"` | 0 error, 286 warning; output diisolasi karena exe aplikasi berjalan mengunci output default | PASS, 2026-10-13 sesi implementasi, sebelum commit `8551291f` |
| Build penutupan sesudah perbaikan label kelas: perintah yang sama ditambah `-p:UseSharedCompilation=false` | `Build succeeded`, 0 error, 281 warning, 2 menit 16 detik. Source = HEAD `4ec02370` + perbaikan `EncounterInsuranceService.cs`. Selisih jumlah warning tidak dianalisis per butir; HEAD sudah memuat merge IGD/Finance. Warning pada `InpBedOccupancyService.cs`/`InpEpisodeService.Assignments.cs` (CS8604, CS1573) berada di luar baris delta | PASS, 2026-10-13; log `%TEMP%/quilvian-kelas-be-closure.log` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <13 berkas source repair> -Mode ReportOnly` | 13 berkas; 0 VIOLATION, 0 REVIEW, 0 INFO; `Final result: PASS` | PASS, dijalankan ulang sesudah perbaikan |
| Review pemakai `PatientClassName` | Nilai konteks hanya dipakai sebagai `PatientClassNameSnapshot` pada resep (`PrescriptionController`), tindakan (`PatientProcedureController`), dan rekonsiliasi obat; tidak diekspos ke response API | PASS — dasar perbaikan aman |
| Review success/validation/authorization/failure/transaksi/kontrak | Diff dibaca ulang pada sesi penutupan: interval `[StartDateTime, EndDateTime)`, `SupersededByCorrectionId == null`, cabang non-rawat-inap, validasi `SelectedBedId`, kelas reservasi pada `UpdateAdmission`, `ROOM_CLASS_MISSING` 422. Tidak mengklaim eksekusi API dengan data pasien | PASS — source/contract review |
| Review diff/scope | Perubahan Billing checkout dan modul lain yang ikut di-commit dipisahkan dari delta repair; tidak stage/commit/push | PASS; `git diff --check` worktree dan rentang `d9089882..HEAD` area repair |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`). Tidak membuat atau menjalankan runner backend alternatif.

MANUAL TEST: NOT FEASIBLE — inventaris computer-use sesi ini tidak memiliki apps/browser aktif. Proses aplikasi berjalan masih memuat binary lama; build hasil task tidak dideploy atau direstart. Verifikasi runtime data riil tidak diklaim.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Resolver periode kamar dan konteks Clinical | Terpenuhi pada source/kontrak | InpServiceClassResolver + assignment PatientClassId pada keempat cabang GetContextAsync/GetCandidateContextAsync. PatientClassName baru seragam memakai kelas layanan setelah perbaikan penutupan 13 Oktober 2026; sebelum itu cabang asuransi/perusahaan masih memakai nama kelas awal/polis |
| Sebelum penempatan null; transfer berakhir tetap sah; koreksi diganti dikecualikan | Terpenuhi pada source/kontrak | Where StartDateTime <= serviceAt && (EndDateTime == null || serviceAt < EndDateTime), !IsDelete, SupersededByCorrectionId == null |
| Rawat jalan/IGD tetap memakai kelas kunjungan | Terpenuhi pada source/kontrak | Cabang EncounterType != Inpatient mengembalikan kelas encounter |
| Contoh kelas kamar 1 dan item dijamin kelas 2 tanpa menyembunyikan item | Terpenuhi pada source/kontrak | Konteks resolver diberikan ke filter BE-RWI-223; daftar master tidak diubah |
| Admisi mengikuti kamar tanpa kelas palsu | Terpenuhi pada source/kontrak | OpenAdmission server menimpa PatientClassId dari kamar pilihan; booking dan UpdateAdmission mengikuti kamar; EvaluatePlacementEligibility menolak kamar tanpa kelas |
| Build, QBE, laporan tracked, traceability | Terpenuhi | Build penutupan 0 error dan QBE PASS pada bagian 5; [roadmap](../../../roadmap/backend-roadmap-kelas-perawatan.md) dan [traceability](../../../roadmap/requirement-traceability-kelas-perawatan.md) ditandai ✅ 13 Oktober 2026 |

Tidak ada acceptance source yang sengaja dibiarkan. Pengecualian verifikasi runtime dinyatakan sebagai NOT FEASIBLE, bukan PASS.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning build existing (termasuk ImageSharp, nullable/legacy, EF1002 coordinator); bukan diperbaiki pada scope kelas |
| Masalah/risiko tersisa | Klien lama tanpa SelectedBedId tetap didukung pada pembukaan; UI baru selalu mengirim bed. Kelas awal draft belum menjadi kelas aktual layanan sebelum placement. Riwayat kelas awal tidak ditulis ulang saat transfer. Sesudah perbaikan label, resep/tindakan yang dibuat sebelum placement menyimpan `PatientClassNameSnapshot` kosong, sesuai aturan kontrak 4 bahwa kelas belum ditetapkan. Snapshot lama tidak diubah. |
| Perubahan sampingan | BOM UTF-8 tertambah di awal `EncounterInsuranceService.cs` dan `Program.cs`. Tidak memengaruhi kompilasi; tidak dikembalikan karena sudah di-commit pemilik |
| Interupsi | Lanjut setelah interupsi dengan status/diff dan SHA HEAD diverifikasi; tidak mengulang implementasi |
| Status Git | Source utama di-commit pemilik pada `8551291f`. Perbaikan penutupan `M Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs` (2 baris) beserta laporan/register belum di-commit; commit dan push dilakukan pemilik |
| Langkah berikutnya | Commit perbaikan penutupan, lalu jalankan aplikasi hasil build pada lingkungan yang disetujui untuk UAT; tidak ada kebutuhan migration repair ini |

### Riwayat status

| Tanggal | Status | Dasar |
| --- | --- | --- |
| 2026-10-13 | DALAM_PENYELESAIAN_VALIDASI | Source dan build sesi implementasi |
| 2026-10-13 | ✅ SELESAI | Penutupan PLAN-REPAIR-DOK-006: audit ulang menemukan label kelas cabang asuransi/perusahaan belum mengikuti kelas layanan; diperbaiki, build 0 error, QBE PASS, register diperbarui |
