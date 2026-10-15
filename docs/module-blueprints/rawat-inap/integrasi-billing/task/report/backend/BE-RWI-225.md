# Laporan Perubahan Backend — `BE-RWI-225`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Judul | BE-RWI-225 — Kelas dan aturan cakupan per komponen tagihan |
| Slice / Trace | PLAN-REPAIR-DOK-006; FIX-DOK-006-02/04; K-01/K-03 |
| Roadmap | [backend-roadmap-kelas-perawatan.md](../../../roadmap/backend-roadmap-kelas-perawatan.md) |
| Contract version | [RWI-KELAS-006-1.0](../../../../dokter-rawat-inap/contracts/kelas-perawatan-repair-006.md), disetujui pemilik; SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112` |
| Approval | Instruksi seluruh task sampai tuntas dan jawaban K-01/K-02 pada 2026-10-12 |
| Dependency | BE-RWI-224 dan matcher BE-RWI-223, source sudah tersedia; status akhir pada roadmap pemilik |
| Klasifikasi | HEAVY — repo 2, baca 2, ubah 1, logika 2, API 1, DB 1, auth 0, UI 0 = 9 |
| Task mode / Target tulis | CROSS-REPO; source backend scope task dan dokumentasi blueprint pemilik; tidak ada DB/deploy |
| Model | Codex berbasis GPT-6 |
| Commit backend / frontend rujukan | Basis `d908988248f6652ed7f2ce84aae9eb3e38dd36a0` / `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`. Source repair di-commit pemilik pada `8551291f` / `2d0a6890e`; penutupan diverifikasi terhadap HEAD `4ec02370` / `f4495aa14` (merge IGD/Finance, tidak menyentuh berkas repair) |
| Tanggal / Status | 2026-10-13 / ✅ SELESAI — divalidasi ulang pada sesi penutupan 13 Oktober 2026 |

## 1. Masalah yang diperbaiki

Memilih kelas sekali dari encounter/waktu kalkulasi membuat layanan sebelum transfer dinilai dengan kelas lain. Urutan aturan klinis dan Billing belum sama.

## 2. Proses bisnis

Billing membentuk komponen dari item dan SourceOccurredAt. Adapter menyelesaikan kelas setiap timestamp, menyaring aturan kelas dan masa berlaku per komponen, lalu memakai urutan aturan bersama. Cakupan perusahaan juga per layanan. Bridge tarif menggunakan kelas pada waktu sumber; retur menggunakan waktu item asli. Snapshot kalkulasi tersimpan tidak ditulis ulang.

## 3. Perubahan yang dikerjakan

Berkas yang diperiksa: source pada tabel, caller Clinical/Pharmacy/Billing/admisi, blueprint/issue/plan, AGENTS.md kedua repo, aturan global dan backend, engineering contract serta registry canonical. Tidak memakai aturan `agents/rules/` legacy sebagai wewenang.

| Berkas | Perubahan |
| --- | --- |
| [BillingCoverageAdapter.cs](../../../../../../../Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs) | ServiceOccurredAt/PatientClassId internal; enrich per komponen, filter class/date, pengurutan bersama, normalisasi BenefitPlanCode. |
| [BillingCalculationService.cs](../../../../../../../Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs) | ITEM/TAX membawa item.SourceOccurredAt. |
| [BillingClinicalChargeBridgeService.cs](../../../../../../../Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs) | Delta sempit resolusi kelas untuk permintaan tarif sumber; perubahan checkout lain dipertahankan. |
| [BillingClinicalChargeBridgeService.DrugReturn.cs](../../../../../../../Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.DrugReturn.cs) | Kelas tarif retur menurut SourceOccurredAt item asal. |
| [CompanyGuarantorCoverageService.cs](../../../../../../../Areas/HealthServices/ClinicalManagement/Services/CompanyGuarantorCoverageService.cs) | Class/date rules per komponen, preview memakai resolver timestamp, khusus kelas diutamakan. |

### Dampak kontrak, database, keamanan dan preflight

| Aspek | Hasil |
| --- | --- |
| Area / Module / Submodule / prefix / keberlakuan | Health Services / Billing Management / Billing (Bil, ACTIVE, TOUCHED LEGACY); Clinical Management (Cli, ACTIVE, TOUCHED LEGACY) |
| Registry | Pemilik terdaftar; tidak membuat entity/folder persisted baru. QBE-MOD-002/003 tidak memerlukan pendaftaran baru |
| QBE ID berlaku | QBE-MOD-001, QBE-SVC-001, QBE-VAL-001, QBE-DEL-001, QBE-AUD-001 |
| Kontrak API | DTO komponen internal bertambah optional properties; endpoint publik Billing dan permission tidak berubah. |
| Database / migration | Query/perilaku existing saja; tidak mengubah schema/entity, tidak membuat migration, tidak menjalankan database |
| Keamanan/Auth | Metadata Access existing dipertahankan; tidak menambah role hardcode atau melemahkan permission |
| Arsitektur / audit | Adapter dan service existing memakai `InpServiceClassResolver` (BE-RWI-224) lewat DI scoped dan urutan `InsuranceCoverageService.OrderCoverageRules` (BE-RWI-223). Pembacaan AsNoTracking; kelas di-cache per timestamp dalam satu kalkulasi. Lifecycle dan transaksi kalkulasi existing dipertahankan. Tidak membuat generic repository |

Checker repository hanya mencakup subset QBE otomatis. Review source/kontrak melengkapi aturan review-only. Registry suite canonical berbeda pada entri Finance/Nutrition yang tidak disentuh; ownership Clinical/Pharmacy/Inpatient/Billing yang dipakai task tetap cocok.

## 4. Dokumentasi endpoint

NOT APPLICABLE — task mengubah adapter internal, tidak menambah/mengubah route publik.

## 5. Verifikasi

| Perintah/skenario | Hasil | Klasifikasi/bukti |
| --- | --- | --- |
| `dotnet restore QuilvianSystemBackend.csproj --nologo` | Berhasil; 5 peringatan paket ImageSharp NU1902/NU1903 existing | PASS, 2026-10-12 |
| `dotnet build QuilvianSystemBackend.csproj --no-restore --nologo -o "$env:TEMP/quilvian-kelas-be-build"` | 0 error, 286 warning; output diisolasi karena exe aplikasi berjalan mengunci output default | PASS, 2026-10-13 sesi implementasi, sebelum commit `8551291f` |
| Build penutupan: perintah yang sama ditambah `-p:UseSharedCompilation=false` | `Build succeeded`, 0 error, 281 warning, 2 menit 16 detik. Source = HEAD `4ec02370` ditambah perbaikan label kelas BE-RWI-224. Selisih jumlah warning tidak dianalisis per butir; HEAD sudah memuat merge IGD/Finance. Tidak ada warning pada baris delta task ini | PASS, 2026-10-13; log `%TEMP%/quilvian-kelas-be-closure.log` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <13 berkas source repair> -Mode ReportOnly` | 13 berkas; 0 VIOLATION, 0 REVIEW, 0 INFO; `Final result: PASS` | PASS, dijalankan ulang pada sesi penutupan |
| Review success/validation/authorization/failure/transaksi/kontrak | Diff dibaca ulang pada sesi penutupan: enrich `ServiceOccurredAt`/`PatientClassId` per komponen di `ResolveAsync`, filter kelas/masa per komponen sebelum `Matches`, bridge memakai `effect.OccurredAt`, retur memakai `item.SourceOccurredAt`. Tidak mengklaim eksekusi kalkulasi dengan data pasien | PASS — source/contract review |
| Review diff/scope | Perubahan Billing checkout dan modul lain yang ikut di-commit dipisahkan dari delta repair; tidak stage/commit/push | PASS; `git diff --check` worktree dan rentang `d9089882..HEAD` area repair |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`). Tidak membuat atau menjalankan runner backend alternatif.

MANUAL TEST: NOT FEASIBLE — inventaris computer-use sesi ini tidak memiliki apps/browser aktif. Proses aplikasi berjalan masih memuat binary lama; build hasil task tidak dideploy atau direstart. Verifikasi runtime data riil tidak diklaim.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Komponen item/pajak memakai SourceOccurredAt, bukan waktu kalkulasi | Terpenuhi pada source/kontrak | BuildCoverageComponents mengirim waktu item pada ITEM dan TAX; ResolveAsync cache kelas berdasarkan timestamp komponen |
| Aturan kelas dan urutan sama pada Clinical/Billing | Terpenuhi pada source/kontrak | CalculateInsuranceCoverageDecision menyaring kelas null/sama dan masa berlaku per komponen, memakai InsuranceCoverageService.OrderCoverageRules |
| Snapshot kalkulasi lama tidak diubah | Terpenuhi pada source/kontrak | Resolver/adapter tidak menulis riwayat; hanya keputusan kalkulasi baru. Lifecycle kalkulasi existing dipertahankan |
| Contoh kelas kamar 1 dan aturan item kelas 2 tidak menanggung | Terpenuhi pada source/kontrak | Aturan kelas 2 terbuang sebelum Matches; tidak ada rule lain berarti outcome bagian penjamin nol |
| Tarif bridge/retur tidak memakai kelas saat hitung ulang | Terpenuhi pada source/kontrak | ResolveAsync pada effect.OccurredAt dan item.SourceOccurredAt |
| Build, QBE, laporan tracked, traceability | Terpenuhi | Build 0 error dan QBE PASS pada bagian 5; [roadmap](../../../roadmap/backend-roadmap-kelas-perawatan.md) dan [traceability](../../../roadmap/requirement-traceability-kelas-perawatan.md) ditandai ✅ 13 Oktober 2026 |

Tidak ada acceptance source yang sengaja dibiarkan. Pengecualian verifikasi runtime dinyatakan sebagai NOT FEASIBLE, bukan PASS.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning build existing (termasuk ImageSharp, nullable/legacy, EF1002 coordinator); bukan diperbaiki pada scope kelas |
| Masalah/risiko tersisa | Administrasi/agregat tanpa timestamp layanan memakai CalculatedAt secara eksplisit. Rumus limit agregat, pajak, pembulatan dan buku tarif asuransi existing tidak disatukan oleh delta kelas ini; preview bukan janji nominal final semua kasus. |
| Perubahan sampingan | BOM UTF-8 tertambah di awal `BillingCalculationService.cs`, `BillingClinicalChargeBridgeService.cs`, dan `BillingClinicalChargeBridgeService.DrugReturn.cs`. Berkas bridge juga diubah pekerjaan checkout Billing pada commit yang sama, sehingga asal BOM-nya tidak dapat dipisahkan. Tidak memengaruhi kompilasi; tidak dikembalikan |
| Interupsi | Lanjut setelah interupsi dengan status/diff dan SHA HEAD diverifikasi; tidak mengulang implementasi |
| Status Git | Source task ini sudah di-commit pemilik pada `8551291f`. Sesi penutupan hanya mengubah laporan/register; commit dan push dilakukan pemilik |
| Langkah berikutnya | UAT kalkulasi Billing untuk pasien yang pindah kamar kelas 1 → kelas 2 pada lingkungan yang disetujui; tidak ada kebutuhan migration repair ini |

### Riwayat status

| Tanggal | Status | Dasar |
| --- | --- | --- |
| 2026-10-13 | DALAM_PENYELESAIAN_VALIDASI | Source dan build sesi implementasi |
| 2026-10-13 | ✅ SELESAI | Penutupan PLAN-REPAIR-DOK-006: diff Billing dibaca ulang, build penutupan 0 error, QBE PASS, register diperbarui |
