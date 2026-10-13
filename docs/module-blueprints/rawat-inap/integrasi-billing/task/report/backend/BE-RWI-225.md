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
| Commit backend / frontend rujukan | `d908988248f6652ed7f2ce84aae9eb3e38dd36a0` / `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`; worktree perubahan lain dipertahankan |
| Tanggal / Status | 2026-10-13 / DALAM_PENYELESAIAN_VALIDASI |

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
| Arsitektur / audit | Resolver baru di module service, DI scoped; AsNoTracking pada pembacaan. Write existing mempertahankan transaksi, Create/Update actor dan lifecycle. Tidak membuat generic repository |

Checker repository hanya mencakup subset QBE otomatis. Review source/kontrak melengkapi aturan review-only. Registry suite canonical berbeda pada entri Finance/Nutrition yang tidak disentuh; ownership Clinical/Pharmacy/Inpatient/Billing yang dipakai task tetap cocok.

## 4. Dokumentasi endpoint

NOT APPLICABLE — task mengubah adapter internal, tidak menambah/mengubah route publik.

## 5. Verifikasi

| Perintah/skenario | Hasil | Klasifikasi/bukti |
| --- | --- | --- |
| `dotnet restore QuilvianSystemBackend.csproj --nologo` | Berhasil; 5 peringatan paket ImageSharp NU1902/NU1903 existing | PASS, 2026-10-12 |
| `dotnet build QuilvianSystemBackend.csproj --no-restore --nologo -o "$env:TEMP/quilvian-kelas-be-build"` | 0 error, 286 warning; output diisolasi karena exe aplikasi berjalan mengunci output default | PASS, 2026-10-13; log sementara `quilvian-kelas-be-final.log` |
| `Invoke-QbeConformanceCheck.ps1 -Path <13 berkas source repair> -Mode ReportOnly` | 13 berkas; 0 VIOLATION, 0 REVIEW, 0 INFO | PASS; pemeriksaan akhir dicatat pada penutupan register |
| Review success/validation/authorization/failure/transaksi/kontrak | Setiap kriteria di bagian 6 ditelusuri ke guard/query/caller source. Tidak mengklaim eksekusi API dengan data pasien | PASS — source/contract review |
| Review diff/scope | Perubahan checkout Billing dan perubahan modul lain pada workspace dipertahankan; tidak stage/commit/push | PASS; `git diff --check` |

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
| Build, QBE, laporan tracked, traceability | Menunggu penutupan register | Hasil di bagian 5; laporan ini dan roadmap pemilik diperbarui setelah validasi akhir |

Tidak ada acceptance source yang sengaja dibiarkan. Pengecualian verifikasi runtime dinyatakan sebagai NOT FEASIBLE, bukan PASS.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning build existing (termasuk ImageSharp, nullable/legacy, EF1002 coordinator); bukan diperbaiki pada scope kelas |
| Masalah/risiko tersisa | Administrasi/agregat tanpa timestamp layanan memakai CalculatedAt secara eksplisit. Rumus limit agregat, pajak, pembulatan dan buku tarif asuransi existing tidak disatukan oleh delta kelas ini; preview bukan janji nominal final semua kasus. |
| Perubahan sampingan | NONE oleh task ini; perubahan lain di worktree tidak dibatalkan |
| Interupsi | Lanjut setelah interupsi dengan status/diff dan SHA HEAD diverifikasi; tidak mengulang implementasi |
| Status Git | Source pada tabel M/baru, kontrak/roadmap/laporan repair baru; kedua repo memiliki perubahan lain. Belum commit |
| Langkah berikutnya | Jalankan aplikasi hasil build pada lingkungan yang disetujui untuk UAT; tidak ada kebutuhan migration repair ini |
