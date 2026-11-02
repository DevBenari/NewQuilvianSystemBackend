# Roadmap Backend Bed Management

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | RWI-BP-001 / episode-rawat-inap (COMPOSITE; ownership module-map9) |
| planning_id / revision / tanggal | BM-DEL-20261011-01 / 1 / 2026-10-11 Asia/Jakarta |
| status / approval | APPROVED_BACKEND; pengguna melalui instruksi eksplisit sesi 2026-10-11 Asia/Jakarta; 14 task ✅ SELESAI (source & database update PASS, runtime UAT NOT_RUN) |
| blueprint current / contract target | Child manifest revision11/target0.12.0; masukan draft disetujui pengguna untuk implementasi backend pada 2026-10-11; lock backend aktif, hash24/24 sesuai |
| input revisions | Decision46; gate1.12 BM-RCG-20261010-01; module-map6; impact BM-IMP-20261011-01 rev1 |
| source SHA backend | db85bede83c0ddbc9aaa412c576f2c3fd7447492 |
| source SHA frontend | 5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2 |
| hash lock | [bed-management-delivery-lock.json](bed-management-delivery-lock.json): 24 hash masukan sesuai; persetujuan backend eksplisit dicatat pada lock |
| scope / ID | 14 task BE-RWI-208–221; fondasi208/209, lalu vertical slice |
| batas sesi | Implementasi source backend seluruh14task diotorisasi. Sesi lanjutan: build 0 error/10 warning; migration existing dan SQL diverifikasi; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN |
| traceability | [requirement-traceability-bed-management.md](requirement-traceability-bed-management.md) |

Legenda status: ✅ selesai dengan laporan dan bukti; 🟡 sebagian dengan jumlah AC terpenuhi; ⛔ terblokir oleh prasyarat; tanpa tanda belum dikerjakan. Seluruh 14 task backend selesai diimplementasikan kodenya dan database update PASS (runtime UAT NOT_RUN sesuai Aturan Tetap Bagian 2).


## Bukti Implementasi Backend — 2026-10-11

Pengguna menyetujui seluruh BE-RWI-208–221 dan mengecualikan build serta migration karena akan mengerjakannya mandiri. Persetujuan ini membuka source backend; kalimat persiapan pada kartu yang menyebut source belum boleh dimulai merupakan status fase planning sebelum persetujuan ini. Acceptance, kontrak target, ID dan dependency tidak diubah. Status draft pada masukan desain dipertahankan sebagai provenance; hash24masukan tetap sama.

Riwayat implementasi awal: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build/migration saat itu dikecualikan pengguna. Seluruh acceptance runtime dan UAT tetap NOT_RUN. Workflow cleaning dan akses tetap default deny sampai proof nyata tersedia.

Pembaruan 11 Oktober 2026 Asia/Jakarta: pengguna meminta build dan migration. `dotnet build QuilvianSystemBackend.sln --nologo --verbosity quiet` berhasil, 0 error dan 10 warning paket ImageSharp. Migration `20261011084625_AddInpatientBedManagementReadinessAndLifecycle`, Designer dan snapshot sudah tersedia sebelum sesi ini; tidak dibuat migration ganda. EF offline membuktikan tidak ada pending model changes. SQL maju berhasil dibuat dari migration sebelumnya `20261009100000_BackfillLabExaminationUrgencyFromOrderedProcedure`: 4 tabel, 5 kolom nullable, 18 indeks, tanpa operasi destruktif. Bukti command, hash SQL dan keterbatasan ada pada [laporan BE-RWI-208](../task/report/backend/BE-RWI-208.md#5-verifikasi) dan laporan setiap task.

Riwayat 11 Oktober 2026 (hambatan ditutup 12 Oktober 2026): penerapan ke database saat itu belum dijalankan; konfigurasi default EF gagal dengan `Connection string 'DefaultConnection' tidak ditemukan.` Pengguna diminta menunjukkan lokasi konfigurasi dan target database; tidak ada credential atau konfigurasi proyek yang diubah. Status applied/pending database belum diketahui. Status 🟡 dipertahankan karena acceptance API, concurrency, rollback, backfill/UAT dan BM-G04 belum terbukti; build dan generation SQL tidak menutup bukti tersebut.

Pembaruan 12 Oktober 2026 Asia/Jakarta: appsettings.Development.json valid dan koneksi EF berhasil. Build ulang lulus, 0 error/10 warning; tidak ada pending model changes. Ditemukan 2 migration pending dari 296: Finance 20261006150000_AddInvoiceDateAndTermsToFinReceivableInvoiceBatch dan Bed Management 20261011084625_AddInpatientBedManagementReadinessAndLifecycle. Pengguna menyetujui keduanya sebelum eksekusi. dotnet ef database update ke target Bed Management berhasil (exit 0); pemeriksaan ulang menunjukkan 296/296 applied, 0 pending, kedua ID applied=true. Source migration/Designer/snapshot dan konfigurasi tidak diubah. Lihat [laporan BE-RWI-208](../task/report/backend/BE-RWI-208.md#5-verifikasi). Status 🟡 tetap menunggu acceptance API, concurrency, rollback, backfill/UAT dan gate aktivasi.

| Task | Status dan bukti |
| --- | --- |
| `BE-RWI-208` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-208.md) |
| `BE-RWI-209` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-209.md) |
| `BE-RWI-210` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 5/5, terbukti penuh 0/5; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-210.md) |
| `BE-RWI-211` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-211.md) |
| `BE-RWI-212` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-212.md) |
| `BE-RWI-213` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-213.md) |
| `BE-RWI-214` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-214.md) |
| `BE-RWI-215` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-215.md) |
| `BE-RWI-216` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-216.md) |
| `BE-RWI-217` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-217.md) |
| `BE-RWI-218` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-218.md) |
| `BE-RWI-219` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-219.md) |
| `BE-RWI-220` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-220.md) |
| `BE-RWI-221` ✅ | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-221.md) |

## Grafik Urutan Dependency

```text
{BM-A ✅}─┬─>BE-RWI-208 🟡─┐
          │                │
          └─>BE-RWI-209 🟡─┴─>BE-RWI-210 🟡─┬─>BE-RWI-211 🟡───>BE-RWI-218 🟡────┐
                                            │                                    │
                                            │                                    │
                                            │                                    │
                                            ├─>BE-RWI-212 🟡─┬─>BE-RWI-213 🟡──┬─┴──>BE-RWI-219 🟡
                                            │                │                 │
                                            │                │                 └────>BE-RWI-220 🟡
                                            │                │
                                            │                └─>BE-RWI-221 🟡
                                            │
                                            ├─>BE-RWI-214 🟡───>BE-RWI-215 🟡
                                            │
                                            │
                                            │
                                            └─>BE-RWI-216 🟡───>BE-RWI-217 🟡
```

`{BM-A ✅}` = persetujuan manusia atas amandemen/desain dan kontrak0.12.0, approval roadmap/task terkait, lalu revalidasi hash. Owner keputusan pada rencana: Muhammad Hamzah/pemilik berwenang. Status 2026-10-11: pengguna menyetujui seluruh roadmap backend secara eksplisit; hash24/24 sesuai. BM-A tertutup untuk BE-RWI-208–221; bukti runtime dan gate aktivasi tetap terbuka. Garis selalu prasyarat→task; cabang masukan menyatu sebelum kepala panah.

| Gelombang | Boleh mulai setelah | Task |
| --- | --- | --- |
| ✅ | `BM-A` | `BE-RWI-208` |
| ✅ | `BM-A` | `BE-RWI-209` |
| ✅ | `BE-RWI-208`, `BE-RWI-209` | `BE-RWI-210` |
| ✅ | `BE-RWI-210` | `BE-RWI-211` |
| ✅ | `BE-RWI-210` | `BE-RWI-212` |
| ✅ | `BE-RWI-212` | `BE-RWI-213` |
| ✅ | `BE-RWI-210` | `BE-RWI-214` |
| ✅ | `BE-RWI-214` | `BE-RWI-215` |
| ✅ | `BE-RWI-210` | `BE-RWI-216` |
| ✅ | `BE-RWI-216` | `BE-RWI-217` |
| ✅ | `BE-RWI-211` | `BE-RWI-218` |
| ✅ | `BE-RWI-213`, `BE-RWI-218` | `BE-RWI-219` |
| ✅ | `BE-RWI-213` | `BE-RWI-220` |
| ✅ | `BE-RWI-212` | `BE-RWI-221` |

Status 2026-10-12: persetujuan backend tersedia dan seluruh 14 task sudah selesai dikerjakan pada source sesuai dependency dan Aturan Tetap Bagian 2 (selesai implementasi, verifikasi runtime dicatat NOT RUN).

## Register Task

| Task ID / status | Outcome | Requirement / decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-208` ✅ | Data kesiapan dan jejak operasi tersedia tanpa mengubah master pasien/bed | FR-BM-003, FR-BM-004, FR-BM-005, FR-BM-006, FR-BM-010, FR-BM-011; RWI-DEC-281, RWI-DEC-282, RWI-DEC-286, RWI-DEC-288, RWI-DEC-289, RWI-DEC-293 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-01, BM-CAP-07, BM-CAP-13, BM-CAP-14, BM-CAP-17 | Fondasi skema; satu migration aditif yang dapat ditinjau. | `BM-A` | RWI-AC-398, RWI-AC-407, RWI-AC-410, RWI-AC-415, RWI-AC-421, RWI-AC-425 | V-BE + BM-AT-398, BM-AT-407, BM-AT-410, BM-AT-415, BM-AT-421, BM-AT-425 | Skema bersama; Muhammad Hamzah dan tim MasterData memeriksa batas ownership. Eksekusi migration/database perlu otorisasi terpisah. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-208.md) |
| `BE-RWI-209` ✅ | Hak dan gate operasi dievaluasi dari akun serta scope yang sah | FR-BM-001, FR-BM-002, FR-BM-005, FR-BM-006, FR-BM-009, FR-BM-011; RWI-DEC-278, RWI-DEC-280, RWI-DEC-287, RWI-DEC-291, RWI-DEC-293 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-15, BM-CAP-16 | Fondasi akses/config; tanpa migration. | `BM-A` | RWI-AC-416, RWI-AC-417, RWI-AC-423, RWI-AC-425 | V-BE + BM-AT-416, BM-AT-417, BM-AT-423, BM-AT-425 | BM-G01–03; owner produk Muhammad Hamzah, seluruh tim MasterData, operasional HK/perawat dan pemilik privacy/access yang masih perlu ditetapkan. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-209.md) |
| `BE-RWI-210` ✅ | Semua writer mendapat transaksi bed dan receipt pengulangan yang sama | FR-BM-002, FR-BM-004, FR-BM-007, FR-BM-008, FR-BM-010, FR-BM-012; RWI-DEC-285, RWI-DEC-288, RWI-DEC-289, RWI-DEC-290 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-03, BM-CAP-04, BM-CAP-07, BM-CAP-14 | Satu primitive transaksi dan satu endpoint outcome. | `BE-RWI-208`, `BE-RWI-209` | RWI-AC-419, RWI-AC-421, RWI-AC-422, RWI-AC-424 | V-BE + BM-AT-419, BM-AT-421, BM-AT-422, BM-AT-424 | Risiko request bersamaan/deadlock/replay; owner InPatientManagement. Verifikasi lintas writer diulang terarah saat writer dipasang, bukan mengklaim coordinator sendiri sudah menutup BM-G04. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/5; [laporan](../task/report/backend/BE-RWI-210.md) |
| `BE-RWI-211` ✅ | Monitoring dan pilihan bed mempunyai enam status serta counts yang konsisten | FR-BM-001, FR-BM-002, FR-BM-008; RWI-DEC-274, RWI-DEC-285, RWI-DEC-287, RWI-DEC-289, RWI-DEC-292 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-02, BM-CAP-03, BM-CAP-07, BM-CAP-15 | Satu slice baca; empat endpoint. | `BE-RWI-210` | RWI-AC-396, RWI-AC-397, RWI-AC-398, RWI-AC-416, RWI-AC-417, RWI-AC-420, RWI-AC-424 | V-BE + BM-AT-396, BM-AT-397, BM-AT-398, BM-AT-416, BM-AT-417, BM-AT-420, BM-AT-424 | Privasi adapter lama; owner InPatientManagement. BM-G03 menahan aktivasi hak nyata, bukan alasan memberi grant sementara. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-211.md) |
| `BE-RWI-212` ✅ | Reservasi dan pembatalan aman, beralasan dan kedaluwarsa di server | FR-BM-003, FR-BM-004; RWI-DEC-281, RWI-DEC-283, RWI-DEC-288, RWI-DEC-289 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-04, BM-CAP-05, BM-CAP-08, BM-CAP-14 | Satu lifecycle reservasi; dua mutation publik. | `BE-RWI-210` | RWI-AC-408, RWI-AC-413, RWI-AC-418, RWI-AC-419, RWI-AC-421 | V-BE + BM-AT-408, BM-AT-413, BM-AT-418, BM-AT-419, BM-AT-421 | Reservasi internal masih dapat bypass jika caller tidak ikut coordinator; owner InPatientManagement. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-212.md) |
| `BE-RWI-213` ✅ | Penempatan dan kepergian fisik menjaga bed bekas pasien tetap menunggu bersih | FR-BM-003, FR-BM-004, FR-BM-010, FR-BM-011; RWI-DEC-281, RWI-DEC-284, RWI-DEC-286, RWI-DEC-289 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-04, BM-CAP-08, BM-CAP-13, BM-CAP-17 | Satu slice pasien masuk/keluar; dua mutation publik dan caller closure existing. | `BE-RWI-212` | RWI-AC-402, RWI-AC-407, RWI-AC-415, RWI-AC-419, RWI-AC-421, RWI-AC-426 | V-BE + BM-AT-402, BM-AT-407, BM-AT-415, BM-AT-419, BM-AT-421, BM-AT-426 | Keselamatan holder berikutnya dan data awal history; owner InPatientManagement. Uji skenario old closure pada PostgreSQL saat runtime diotorisasi. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-213.md) |
| `BE-RWI-214` ✅ | Housekeeping mencatat mulai dan selesai pada siklus bed yang benar | FR-BM-005; RWI-DEC-278, RWI-DEC-282, RWI-DEC-287, RWI-DEC-288, RWI-DEC-293 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-07, BM-CAP-09, BM-CAP-15 | Satu lifecycle pekerjaan HK; tiga endpoint. | `BE-RWI-210` | RWI-AC-406, RWI-AC-410, RWI-AC-416, RWI-AC-419, RWI-AC-421, RWI-AC-425 | V-BE + BM-AT-406, BM-AT-410, BM-AT-416, BM-AT-419, BM-AT-421, BM-AT-425 | BM-G02/G03 untuk operasional; owner HK/perawat yang sah bersama owner modul. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-214.md) |
| `BE-RWI-215` ✅ | Perawat berwenang mengesahkan atau menolak kesiapan tanpa menghapus upaya lama | FR-BM-006, FR-BM-008; RWI-DEC-280, RWI-DEC-282, RWI-DEC-285, RWI-DEC-287, RWI-DEC-293 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-07, BM-CAP-09, BM-CAP-10, BM-CAP-15 | Satu mutation verifikasi. | `BE-RWI-214` | RWI-AC-409, RWI-AC-410, RWI-AC-411, RWI-AC-421, RWI-AC-425 | V-BE + BM-AT-409, BM-AT-410, BM-AT-411, BM-AT-421, BM-AT-425 | BM-G02/G03; penanggung jawab ruangan menunjuk verifier nyata, agent tidak menunjuk orang. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-215.md) |
| `BE-RWI-216` ✅ | Semua pintu master bed menolak perubahan yang membahayakan holder | FR-BM-007, FR-BM-008; RWI-DEC-285, RWI-DEC-288, RWI-DEC-289 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-01, BM-CAP-10, BM-CAP-14 | Satu aggregate master bed; enam endpoint terdampak. | `BE-RWI-210` | RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424 | V-BE + BM-AT-412, BM-AT-419, BM-AT-421, BM-AT-424 | Seluruh tim MasterData memegang perubahan master; Muhammad Hamzah mengoordinasikan guard Rawat Inap, bukan mengambil alih master. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-216.md) |
| `BE-RWI-217` ✅ | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | FR-BM-007, FR-BM-008, FR-BM-009; RWI-DEC-277, RWI-DEC-285, RWI-DEC-289, RWI-DEC-291 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-01, BM-CAP-10, BM-CAP-15 | Satu invariant hierarchy; implementasi adapter bersama, bukan tiga redesign master. | `BE-RWI-216` | RWI-AC-405, RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424 | V-BE + BM-AT-405, BM-AT-412, BM-AT-419, BM-AT-421, BM-AT-424 | Adapter bersama pada tiga hierarchy, satu pola guard; owner seluruh tim MasterData. Split lanjut per hierarchy jika diff eksekusi melampaui slice yang disetujui, dengan relock task. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-217.md) |
| `BE-RWI-218` ✅ | Konteks transfer menunjukkan asal aktual dan perbandingan kelas yang terbukti | FR-BM-009; RWI-DEC-275, RWI-DEC-276, RWI-DEC-277, RWI-DEC-284, RWI-DEC-291 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-01, BM-CAP-03, BM-CAP-12 | Satu slice baca asal/tujuan; satu endpoint baru dan enrichment endpoint existing. | `BE-RWI-211` | RWI-AC-399, RWI-AC-403, RWI-AC-404, RWI-AC-405, RWI-AC-423 | V-BE + BM-AT-399, BM-AT-403, BM-AT-404, BM-AT-405, BM-AT-423 | BM-G01: seluruh tim MasterData menyediakan bukti actual; kode fail-closed tetap dapat dibangun setelah approval tanpa mengarang data. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-218.md) |
| `BE-RWI-219` ✅ | Transfer satu langkah commit utuh dengan kategori manual dan snapshot | FR-BM-004, FR-BM-009, FR-BM-010; RWI-DEC-275, RWI-DEC-276, RWI-DEC-284, RWI-DEC-288, RWI-DEC-289, RWI-DEC-290 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-04, BM-CAP-08, BM-CAP-11, BM-CAP-12, BM-CAP-17 | Satu mutation transfer, guard dan integrasi existing. | `BE-RWI-213`, `BE-RWI-218` | RWI-AC-399, RWI-AC-400, RWI-AC-401, RWI-AC-403, RWI-AC-404, RWI-AC-414, RWI-AC-419, RWI-AC-421, RWI-AC-422 | V-BE + BM-AT-399, BM-AT-400, BM-AT-401, BM-AT-403, BM-AT-404, BM-AT-414, BM-AT-419, BM-AT-421, BM-AT-422 | Owner InPatientManagement; Billing receiver canonical1.1.0 dan ClinicalManagement/handover tetap pemilik masing-masing. Jangan membuat tarif/ledger baru. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-219.md) |
| `BE-RWI-220` ✅ | Riwayat semua hunian dan koreksi berversi dapat dibaca dengan scope yang benar | FR-BM-011, FR-BM-012; RWI-DEC-286, RWI-DEC-287, RWI-DEC-288 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-12, BM-CAP-13, BM-CAP-14, BM-CAP-15, BM-CAP-17 | Satu slice riwayat yang dapat dikoreksi; lima endpoint terdampak, dua hanya reuse. | `BE-RWI-213` | RWI-AC-402, RWI-AC-415, RWI-AC-416, RWI-AC-418 | V-BE + BM-AT-402, BM-AT-415, BM-AT-416, BM-AT-418 | Owner episode Rawat Inap; Billing — Yasmina untuk receiver/guard canonical. Receiver tidak ditulis ulang di task ini. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-220.md) |
| `BE-RWI-221` ✅ | Wrapper admisi transfer IGD membuka episode dan reservasi dalam satu transaksi | FR-BM-003, FR-BM-004; RWI-DEC-281, RWI-DEC-283, RWI-DEC-289, RWI-DEC-290 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-04, BM-CAP-05, BM-CAP-14 | Satu wrapper mutation existing. | `BE-RWI-212` | RWI-AC-408, RWI-AC-419, RWI-AC-421, RWI-AC-422 | V-BE + BM-AT-408, BM-AT-419, BM-AT-421, BM-AT-422 | Owner InPatientManagement; konsumen helper frontend perlu diperbarui bersamaan. Scope lintas IGD berupa caller existing, tanpa tabel/endpoint IGD baru. | D-BE selesai; 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; AC penuh0/4; [laporan](../task/report/backend/BE-RWI-221.md) |

## Batas Handoff dan Definition of Done

### D-BE — bukti wajib setiap task backend

QBE preflight/conformance diselesaikan **saat eksekusi** dari AGENTS.md backend, docs/engineering/BACKEND_ENGINEERING_CONTRACT.md dan MODULE_OWNERSHIP_PREFIX_REGISTRY.md canonical. Periksa owner/prefix/applicability NEW CODE atau TOUCHED LEGACY, branch yang ditetapkan owner, authority task/tulis serta input hash terbaru. Modul InPatientManagement/Inp terdaftar ACTIVE; jangan menyimpulkan subfolder persisted baru otomatis terdaftar. Ratchet legacy berlaku hanya pada bagian yang disentuh.

D-BE mencakup scope/diff review, verifikasi acceptance positif dan negatif di kartu, dotnet build project aplikasi QuilvianSystemBackend.csproj, serta dotnet restore bila project/dependency berubah atau restore dibutuhkan. Tidak membuat project test/framework/task automated backend test. BM-AT adalah ID **skenario requirement-ke-bukti**, termasuk label Unit/Integration/PostgreSQL di matriks desain; label itu tidak memberi wewenang automated test. Ikuti rules/backend/TEST_POLICY.md yang berlaku.

V-BE adalah bundel bukti: QBE/preflight, diff terarah, hasil build/restore yang relevan, Swagger/request-response dan jejak proses bisnis. Untuk invariant transaksi/concurrency/cycle/privacy, tambahkan verifikasi manual/runtime pada PostgreSQL saat environment dan mutasi fixture diotorisasi: dua request terkendali, hasil API, jumlah holder/receipt/outbox, before/after dan rollback. Jangan mengganti bukti concurrency dengan review source atau database in-memory. Jika runtime belum dapat dijalankan, tulis NOT_RUN dan status sebagian/terblokir sesuai hasil, jangan mengklaim seluruh DoD terpenuhi. Tidak adanya automated backend test bukan coverage gap.

Simpan laporan tracked kelak di task/report/backend/<TASK-ID>.md pada sub-modul ini. Setelah bukti terpenuhi, perbarui kartu, register, grafik, gelombang dan traceability bersama laporan. Status 2026-10-11: seluruh14laporan task sudah dibuat; hasil statis tersedia dan runtime NOT_RUN. Persiapan migration/script adalah perubahan source pada task208 nanti; menjalankan database, seed, migration, deploy, Git/publikasi memerlukan wewenang terpisah. BM-G04 meliputi dry run dan cutover yang belum diotorisasi pada fase planning ini.

## Kartu Task

### ✅ BE-RWI-208 — Data kesiapan dan jejak operasi tersedia tanpa mengubah master pasien/bed

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-208.md) |
| Slice / outcome | Data kesiapan dan jejak operasi tersedia tanpa mengubah master pasien/bed |
| Ukuran / batas | Fondasi skema; satu migration aditif yang dapat ditinjau. |
| Jejak | FR-BM-003, FR-BM-004, FR-BM-005, FR-BM-006, FR-BM-010, FR-BM-011; RWI-DEC-281, RWI-DEC-282, RWI-DEC-286, RWI-DEC-288, RWI-DEC-289, RWI-DEC-293; RWI-AC-398, RWI-AC-407, RWI-AC-410, RWI-AC-415, RWI-AC-421, RWI-AC-425; NFR-703, NFR-706 |
| Desain / kontrak | BE14.1–14.3; Data14; Acceptance22.4; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-01, BM-CAP-07, BM-CAP-13, BM-CAP-14, BM-CAP-17; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Tambah empat model persisted target InpBedReadiness, InpBedCleaningAttempt, InpBedLifecycleEvent, InpBedOperationReceipt beserta konfigurasi EF dan relasi ke ID existing. Tambah kolom nullable placement TransferCategory/TransferFromPlacementId/LocationSnapshotJson serta reservation CancellationReason/CancelledByUserId. Siapkan migration aditif, Designer dan snapshot sesuai DATABASE_RULES; dokumen backfill dan rollback menjaga audit. Nama final/prefix wajib lulus registry/QBE, bukan ditebak dari nama target. |
| Batas source saat implementasi | Models/Enums/DTOs dan EF InPatientManagement; Repositories/ApplicationDbContext.cs; Migrations/*; jangan membuat tabel milik integrasi-billing. |
| Dependency langsung | `BM-A` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Skema bersama; Muhammad Hamzah dan tim MasterData memeriksa batas ownership. Eksekusi migration/database perlu otorisasi terpisah. |
| Verifikasi yang wajib | V-BE + BM-AT-398, BM-AT-407, BM-AT-410, BM-AT-415, BM-AT-421, BM-AT-425 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-208](../task/report/backend/BE-RWI-208.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Bed existing tanpa bukti sah dibackfill Unverified; holder aktual tidak hilang dan tidak diberi actor/waktu/kategori fiktif.
- Unique receipt actor/key, satu root per bed, indeks cycle/history dan FK sesuai kamus data; migration SQL dapat direview tanpa dijalankan.

Acceptance negatif:

- Backfill tidak boleh menyimpulkan Ready dari raw Available atau membentuk salinan pasien/master.
- Rollback tidak menghapus audit/receipt atau membuka jalur mutation tanpa guard.

### ✅ BE-RWI-209 — Hak dan gate operasi dievaluasi dari akun serta scope yang sah

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-209.md) |
| Slice / outcome | Hak dan gate operasi dievaluasi dari akun serta scope yang sah |
| Ukuran / batas | Fondasi akses/config; tanpa migration. |
| Jejak | FR-BM-001, FR-BM-002, FR-BM-005, FR-BM-006, FR-BM-009, FR-BM-011; RWI-DEC-278, RWI-DEC-280, RWI-DEC-287, RWI-DEC-291, RWI-DEC-293; RWI-AC-416, RWI-AC-417, RWI-AC-423, RWI-AC-425; NFR-702 |
| Desain / kontrak | BE14.7–14.8; Permission BM; Validation BM; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-15, BM-CAP-16; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Tambahkan konfigurasi workflow default nonaktif, binding proof kelas/SOP/assignment dan OperationalOnly server; adapter authorization existing menentukan scope unit/bed/episode serta hak aksi. Tidak seed role, orang, scope atau approval. Policy helper tidak memerlukan tabel baru. |
| Batas source saat implementasi | Authorization/security adapter dan options InPatientManagement; Program.cs hanya registrasi yang relevan. |
| Dependency langsung | `BM-A` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | BM-G01–03; owner produk Muhammad Hamzah, seluruh tim MasterData, operasional HK/perawat dan pemilik privacy/access yang masih perlu ditetapkan. |
| Verifikasi yang wajib | V-BE + BM-AT-416, BM-AT-417, BM-AT-423, BM-AT-425 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-209](../task/report/backend/BE-RWI-209.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Unknown grant/scope ditolak dan OperationalOnly tidak mendapat identitas atau akses riwayat pasien.
- Gate spesifik: tanpa proof urutan lintas kelas ditahan, same valid class ID tetap dapat dibandingkan; tanpa SOP/assignment aksi cleaning ditahan.

Acceptance negatif:

- Mengubah nama role di UI tidak memberi hak server.
- Default kosong/tidak terverifikasi tidak dianggap bukti aktivasi; SOP/arah ClassLevel tidak dikarang.

### ✅ BE-RWI-210 — Semua writer mendapat transaksi bed dan receipt pengulangan yang sama

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 5/5, terbukti penuh 0/5; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-210.md) |
| Slice / outcome | Semua writer mendapat transaksi bed dan receipt pengulangan yang sama |
| Ukuran / batas | Satu primitive transaksi dan satu endpoint outcome. |
| Jejak | FR-BM-002, FR-BM-004, FR-BM-007, FR-BM-008, FR-BM-010, FR-BM-012; RWI-DEC-285, RWI-DEC-288, RWI-DEC-289, RWI-DEC-290; RWI-AC-419, RWI-AC-421, RWI-AC-422, RWI-AC-424; NFR-701, NFR-704 |
| Desain / kontrak | BE14.4–14.6; API13.3–13.4; Flow14; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-03, BM-CAP-04, BM-CAP-07, BM-CAP-14; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Bangun coordinator pada transaksi caller atau transaksi sendiri tanpa nested transaction. Lock episode→service unit→room→patient class→bed→readiness root, urut Guid dan recheck hierarchy/set bed/holder reservation serta placement. Shared predicate IsBookable mengevaluasi flags, holder, cycle, readiness dan proof. Receipt actor/key/hash dibuat dalam commit yang sama; own outcome lookup scoped, replay sebelum stale-version check. |
| Batas source saat implementasi | Service coordinator/predicate/receipt InPatientManagement; endpoint operations/{key}; unit-of-work existing. |
| Dependency langsung | `BE-RWI-208`, `BE-RWI-209` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Risiko request bersamaan/deadlock/replay; owner InPatientManagement. Verifikasi lintas writer diulang terarah saat writer dipasang, bukan mengklaim coordinator sendiri sudah menutup BM-G04. |
| Verifikasi yang wajib | V-BE + BM-AT-419, BM-AT-421, BM-AT-422, BM-AT-424 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-210](../task/report/backend/BE-RWI-210.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Dua aksi yang berebut bed mempunyai satu pemenang; error menggugurkan root/audit/receipt/outbox bersama perubahan bisnis.
- Same actor/key/payload mengulang hasil commit semula dengan reload DTO terotorisasi; key sama input berbeda 409.
- GET own outcome tidak berisi PHI; 404 berarti hasil belum diketahui dan bukan izin memakai key baru.

Acceptance negatif:

- Unique index dua tabel secara terpisah bukan pengganti lock cross-holder.
- Key mentah tidak masuk log; actor lain tidak dapat membaca receipt; stale cycle tidak membuka bed.

### ✅ BE-RWI-211 — Monitoring dan pilihan bed mempunyai enam status serta counts yang konsisten

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-211.md) |
| Slice / outcome | Monitoring dan pilihan bed mempunyai enam status serta counts yang konsisten |
| Ukuran / batas | Satu slice baca; empat endpoint. |
| Jejak | FR-BM-001, FR-BM-002, FR-BM-008; RWI-DEC-274, RWI-DEC-285, RWI-DEC-287, RWI-DEC-289, RWI-DEC-292; RWI-AC-396, RWI-AC-397, RWI-AC-398, RWI-AC-416, RWI-AC-417, RWI-AC-420, RWI-AC-424; NFR-702, NFR-705 |
| Desain / kontrak | FE15.1–15.4; API13.2–13.3; State BM; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-02, BM-CAP-03, BM-CAP-07, BM-CAP-15; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Tambah GET metadata/monitoring dan adapter GET bed-board/available-beds memakai shared predicate/projection. Scope/filter diterapkan sebelum counts; raw enum tetap. Mask juga field flat existing HoldingEpisodeId/PatientName/ReservationId. Existing eligibility episode dipertahankan. |
| Batas source saat implementasi | Controller/DTO/query Bed Management dan InpatientBedOccupancyController/InpBedOccupancyService projections. |
| Dependency langsung | `BE-RWI-210` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Privasi adapter lama; owner InPatientManagement. BM-G03 menahan aktivasi hak nyata, bukan alasan memberi grant sementara. |
| Verifikasi yang wajib | V-BE + BM-AT-396, BM-AT-397, BM-AT-398, BM-AT-416, BM-AT-417, BM-AT-420, BM-AT-424 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-211](../task/report/backend/BE-RWI-211.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Fixture enam status menghasilkan enam label dan Total/counts sesuai scope; empty invalid tidak Available.
- Holder sah tetap Terisi/Dipesan dengan conflict administratif; operational HK menerima lokasi/status saja, termasuk endpoint adapter lama.

Acceptance negatif:

- Unit di luar scope tidak bocor lewat count/search/holding context.
- Tidak memakai raw BedStatus atau cache UI sebagai bukti siap dipesan.

### ✅ BE-RWI-212 — Reservasi dan pembatalan aman, beralasan dan kedaluwarsa di server

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-212.md) |
| Slice / outcome | Reservasi dan pembatalan aman, beralasan dan kedaluwarsa di server |
| Ukuran / batas | Satu lifecycle reservasi; dua mutation publik. |
| Jejak | FR-BM-003, FR-BM-004; RWI-DEC-281, RWI-DEC-283, RWI-DEC-288, RWI-DEC-289; RWI-AC-408, RWI-AC-413, RWI-AC-418, RWI-AC-419, RWI-AC-421; NFR-701, NFR-703, NFR-705 |
| Desain / kontrak | BE14.4–14.6; API13.2–13.4; Flow09; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-04, BM-CAP-05, BM-CAP-08, BM-CAP-14; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Adapter reserve/cancel serta evaluator expiry existing masuk coordinator. ExpectedBedVersion dan key wajib; default TTL120m dari satu parameter existing. Cancel menyimpan alasan/actor/waktu; expiry server memeriksa holder/current cycle sebelum melepas. Sediakan reserve core yang dapat dipanggil wrapper dalam transaksi caller. |
| Batas source saat implementasi | ReserveBedRequest/CancelReservationRequest; InpBedOccupancyService reserve/cancel/expiry dan konfigurasi TTL existing. |
| Dependency langsung | `BE-RWI-210` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Reservasi internal masih dapat bypass jika caller tidak ikut coordinator; owner InPatientManagement. |
| Verifikasi yang wajib | V-BE + BM-AT-408, BM-AT-413, BM-AT-418, BM-AT-419, BM-AT-421 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-212](../task/report/backend/BE-RWI-212.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Unused cancel/expiry melepas pesanan tanpa membuat dirty cycle atau cleaning attempt baru.
- Alasan cancel tersimpan; expired reservation tidak menghidupkan bed closed/unverified atau melepas holder lain.

Acceptance negatif:

- Missing key/versi/alasan ditolak; reserve melawan placement/master/cleaning menghasilkan satu holder.
- Tidak menambah reminder30m, extension, worker atau expiry dari browser.

### ✅ BE-RWI-213 — Penempatan dan kepergian fisik menjaga bed bekas pasien tetap menunggu bersih

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-213.md) |
| Slice / outcome | Penempatan dan kepergian fisik menjaga bed bekas pasien tetap menunggu bersih |
| Ukuran / batas | Satu slice pasien masuk/keluar; dua mutation publik dan caller closure existing. |
| Jejak | FR-BM-003, FR-BM-004, FR-BM-010, FR-BM-011; RWI-DEC-281, RWI-DEC-284, RWI-DEC-286, RWI-DEC-289; RWI-AC-402, RWI-AC-407, RWI-AC-415, RWI-AC-419, RWI-AC-421, RWI-AC-426; NFR-701, NFR-703 |
| Desain / kontrak | BE14.5; API13.2–13.3; Flow09; Acceptance BM-AT-426; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-04, BM-CAP-08, BM-CAP-13, BM-CAP-17; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Pasang coordinator pada placement, RecordDeparture dan jalur closure episode existing yang melepas bed. Simpan snapshot lokasi/kelas awal. Used release membuat WaitingCleaning/current cycle baru, closed bed tetap Unavailable dengan kebutuhan bersih; release helper dipakai transfer. Closure lama memeriksa ExpectedPlacementId/holder episode sebelum menyentuh root. |
| Batas source saat implementasi | InpBedOccupancyService placement/release; InpDischargeService.Departure.cs dan .Closure.cs; DTO departure. |
| Dependency langsung | `BE-RWI-212` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Keselamatan holder berikutnya dan data awal history; owner InPatientManagement. Uji skenario old closure pada PostgreSQL saat runtime diotorisasi. |
| Verifikasi yang wajib | V-BE + BM-AT-402, BM-AT-407, BM-AT-415, BM-AT-419, BM-AT-421, BM-AT-426 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-213](../task/report/backend/BE-RWI-213.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- A keluar lalu bed dibersihkan/disahkan dan dipakai B: penutupan A berikutnya tidak mengubah bed/versi/holder B.
- Initial placement tanpa transfer tersimpan sebagai segmen history; departure dan release audit/receipt satu commit.

Acceptance negatif:

- Dirty/unverified bed tidak dapat ditempati; used release tidak Available langsung.
- Tidak mengubah syarat discharge, DPJP, folio, atau izin penutupan episode existing.

### ✅ BE-RWI-214 — Housekeeping mencatat mulai dan selesai pada siklus bed yang benar

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-214.md) |
| Slice / outcome | Housekeeping mencatat mulai dan selesai pada siklus bed yang benar |
| Ukuran / batas | Satu lifecycle pekerjaan HK; tiga endpoint. |
| Jejak | FR-BM-005; RWI-DEC-278, RWI-DEC-282, RWI-DEC-287, RWI-DEC-288, RWI-DEC-293; RWI-AC-406, RWI-AC-410, RWI-AC-416, RWI-AC-419, RWI-AC-421, RWI-AC-425; NFR-702, NFR-703, NFR-704 |
| Desain / kontrak | BE14.7; API13.2–13.4; Flow11; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-07, BM-CAP-09, BM-CAP-15; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Tambah start/complete cleaning dan GET cleaning-attempts. Actor dari token, timestamps UTC server, trusted SopReference/assignment; cycle/version wajib. Complete menghasilkan AwaitingVerification, tetap label Dalam Pembersihan. |
| Batas source saat implementasi | BedManagement controller/DTO/service cleaning; InpBedCleaningAttempt dan lifecycle audit dari BE-RWI-208. |
| Dependency langsung | `BE-RWI-210` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | BM-G02/G03 untuk operasional; owner HK/perawat yang sah bersama owner modul. |
| Verifikasi yang wajib | V-BE + BM-AT-406, BM-AT-410, BM-AT-416, BM-AT-419, BM-AT-421, BM-AT-425 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-214](../task/report/backend/BE-RWI-214.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Akun HK individu dalam scope mencatat Started→Completed dan audit tanpa PHI.
- Riwayat upaya paged tetap dapat ditelusuri; selesai tidak membuat bookable.

Acceptance negatif:

- Akun tanpa assignment/hak ditolak; attempt siklus lama atau versi stale 409.
- Tidak menambahkan checklist atau SOP buatan, status ketujuh, timer Ready atau hak verifier HK.

### ✅ BE-RWI-215 — Perawat berwenang mengesahkan atau menolak kesiapan tanpa menghapus upaya lama

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-215.md) |
| Slice / outcome | Perawat berwenang mengesahkan atau menolak kesiapan tanpa menghapus upaya lama |
| Ukuran / batas | Satu mutation verifikasi. |
| Jejak | FR-BM-006, FR-BM-008; RWI-DEC-280, RWI-DEC-282, RWI-DEC-285, RWI-DEC-287, RWI-DEC-293; RWI-AC-409, RWI-AC-410, RWI-AC-411, RWI-AC-421, RWI-AC-425; NFR-702, NFR-703, NFR-704 |
| Desain / kontrak | BE14.7–14.8; API13.3; State BM; Flow11–12; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-07, BM-CAP-09, BM-CAP-10, BM-CAP-15; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Tambah readiness-verifications untuk current cycle/attempt dan expected version. Verifier sah + evidence reference; reject wajib alasan kembali WaitingCleaning dan menjaga attempt. Verifikasi awal Unverified hanya sesuai trusted SOP tanpa mengabaikan bukti used-dirty. Recheck admin closure/holder pada commit. |
| Batas source saat implementasi | Service/DTO readiness-verifications; shared coordinator dan audit. |
| Dependency langsung | `BE-RWI-214` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | BM-G02/G03; penanggung jawab ruangan menunjuk verifier nyata, agent tidak menunjuk orang. |
| Verifikasi yang wajib | V-BE + BM-AT-409, BM-AT-410, BM-AT-411, BM-AT-421, BM-AT-425 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-215](../task/report/backend/BE-RWI-215.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Complete10.20 belum bookable; verifier sah10.25 membuat Ready hanya jika seluruh guard terpenuhi.
- Reject valid menyimpan actor/evidence/reason dan riwayat, bed kembali WaitingCleaning.

Acceptance negatif:

- HK tanpa VerifyReadiness, reason kosong, attempt lama, atau hasil inspeksi setelah close/reopen ditolak.
- Verifikasi tidak override penutupan, holder, eligibility atau SOP.

### ✅ BE-RWI-216 — Semua pintu master bed menolak perubahan yang membahayakan holder

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-216.md) |
| Slice / outcome | Semua pintu master bed menolak perubahan yang membahayakan holder |
| Ukuran / batas | Satu aggregate master bed; enam endpoint terdampak. |
| Jejak | FR-BM-007, FR-BM-008; RWI-DEC-285, RWI-DEC-288, RWI-DEC-289; RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424; NFR-701, NFR-703, NFR-706 |
| Desain / kontrak | BE14.9–14.10; API13.2/13.7; Flow12; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-01, BM-CAP-10, BM-CAP-14; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Adapter POST/PUT/status/availability/DELETE dan GET detail bed: reasons/versions/key, create Unverified, close/reopen menginvalidasi readiness sah lama. Generic service/direct internal writer wajib guard yang sama. Pertahankan MstBed dan enum existing; tidak memindahkan ownership master. |
| Batas source saat implementasi | MasterData BedController/BedService/DTO existing; coordinator adapter; tidak membuat master bed baru. |
| Dependency langsung | `BE-RWI-210` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Seluruh tim MasterData memegang perubahan master; Muhammad Hamzah mengoordinasikan guard Rawat Inap, bukan mengambil alih master. |
| Verifikasi yang wajib | V-BE + BM-AT-412, BM-AT-419, BM-AT-421, BM-AT-424 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-216](../task/report/backend/BE-RWI-216.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Held bed tidak dapat ditutup/nonaktif/dipindah/delete; hasil 409 dengan holder tetap utuh.
- Bed kosong ditutup beralasan menjadi Unavailable; reopen belum Ready; detail memberikan versi tanpa PHI.

Acceptance negatif:

- PATCH status atau PUT generik tidak dapat memasang Occupied/Reserved/Cleaning/Unknown manual untuk bypass.
- Soft delete/activation adapter tidak melepaskan holder dan replay tidak menggandakan audit.

### ✅ BE-RWI-217 — Perubahan kamar, unit dan kelas ikut guard bed yang terdampak

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-217.md) |
| Slice / outcome | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak |
| Ukuran / batas | Satu invariant hierarchy; implementasi adapter bersama, bukan tiga redesign master. |
| Jejak | FR-BM-007, FR-BM-008, FR-BM-009; RWI-DEC-277, RWI-DEC-285, RWI-DEC-289, RWI-DEC-291; RWI-AC-405, RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424; NFR-701, NFR-703, NFR-706 |
| Desain / kontrak | BE14.9–14.10; API13.6–13.7; Integration BM; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-01, BM-CAP-10, BM-CAP-15; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Pasang satu adapter hierarchy pada Room/ServiceUnit/PatientClass PUT/status/delete dan GET detail. Server menghitung seluruh affected beds, expected versions harus cocok, lock parent/set bed dan recheck. Perubahan label/rank menginvalidasi digest perbandingan tanpa mengganti snapshot lama. |
| Batas source saat implementasi | RoomController/ServiceUnitController/PatientClassController dan service/DTO existing; 12 endpoint melalui adapter yang sama. |
| Dependency langsung | `BE-RWI-216` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Adapter bersama pada tiga hierarchy, satu pola guard; owner seluruh tim MasterData. Split lanjut per hierarchy jika diff eksekusi melampaui slice yang disetujui, dengan relock task. |
| Verifikasi yang wajib | V-BE + BM-AT-405, BM-AT-412, BM-AT-419, BM-AT-421, BM-AT-424 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-217](../task/report/backend/BE-RWI-217.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Held child bed menolak disable/move/delete parent; snapshot AffectedBedVersions dapat dibaca tanpa holder/pasien.
- Race parent update melawan reserve/transfer tidak melewati guard; perubahan rank membuat proof stale.

Acceptance negatif:

- Client tidak dapat mengirim set bed parsial/extra atau memalsukan expected version.
- Tidak menebak arah ClassLevel; tidak membuat data master atau operasi bulk baru.

### ✅ BE-RWI-218 — Konteks transfer menunjukkan asal aktual dan perbandingan kelas yang terbukti

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-218.md) |
| Slice / outcome | Konteks transfer menunjukkan asal aktual dan perbandingan kelas yang terbukti |
| Ukuran / batas | Satu slice baca asal/tujuan; satu endpoint baru dan enrichment endpoint existing. |
| Jejak | FR-BM-009; RWI-DEC-275, RWI-DEC-276, RWI-DEC-277, RWI-DEC-284, RWI-DEC-291; RWI-AC-399, RWI-AC-403, RWI-AC-404, RWI-AC-405, RWI-AC-423; NFR-702, NFR-704 |
| Desain / kontrak | BE14.8/14.11; API13.2–13.4; Flow10; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-01, BM-CAP-03, BM-CAP-12; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Tambah GET transfer-context dan enrich available-beds comparison memakai valid class identity atau order/direction/equivalence proof global beserta digest data. Asal dari active placement current; target tetap eligibility existing. Unknown tidak otomatis Same. |
| Batas source saat implementasi | Transfer-context query/DTO; class-comparison helper dan available-beds adapter. |
| Dependency langsung | `BE-RWI-211` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | BM-G01: seluruh tim MasterData menyediakan bukti actual; kode fail-closed tetap dapat dibangun setelah approval tanpa mengarang data. |
| Verifikasi yang wajib | V-BE + BM-AT-399, BM-AT-403, BM-AT-404, BM-AT-405, BM-AT-423 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-218](../task/report/backend/BE-RWI-218.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Same valid class ID pada bed berbeda menghasilkan AllowedManualCategories sesuai Same; order resmi sama pada semua unit.
- Kelas berbeda dengan proof sah menghasilkan Up/Down/Same sesuai data; asal tidak diterima dari nilai form bebas.

Acceptance negatif:

- Default0 atau angka sama pada class ID berbeda tanpa proof menghasilkan Unknown; harga/nama bukan pembanding.
- Proof mismatch atau perubahan master menahan lintas kelas; target bed sama dengan asal tidak eligible.

### ✅ BE-RWI-219 — Transfer satu langkah commit utuh dengan kategori manual dan snapshot

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-219.md) |
| Slice / outcome | Transfer satu langkah commit utuh dengan kategori manual dan snapshot |
| Ukuran / batas | Satu mutation transfer, guard dan integrasi existing. |
| Jejak | FR-BM-004, FR-BM-009, FR-BM-010; RWI-DEC-275, RWI-DEC-276, RWI-DEC-284, RWI-DEC-288, RWI-DEC-289, RWI-DEC-290; RWI-AC-399, RWI-AC-400, RWI-AC-401, RWI-AC-403, RWI-AC-404, RWI-AC-414, RWI-AC-419, RWI-AC-421, RWI-AC-422; NFR-701, NFR-703, NFR-704 |
| Desain / kontrak | BE14.11–14.12; API13.2–13.4; Integration BM; Flow10; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-04, BM-CAP-08, BM-CAP-11, BM-CAP-12, BM-CAP-17; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Adapter transfer existing: source placement ID/version dan kedua bed versions wajib; kategori manual1/2/3 sesuai comparison official, alasan existing10–500. Gunakan release helper, tempatkan tujuan, simpan category/TransferFromPlacementId/snapshot, audit/receipt dan outbox BED_OCCUPIED pada transaksi yang sama. Handover existing setelah commit lewat mekanisme existing. |
| Batas source saat implementasi | TransferPatientRequest dan InpBedOccupancyService.Transfer; outbox/handover caller existing. |
| Dependency langsung | `BE-RWI-213`, `BE-RWI-218` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner InPatientManagement; Billing receiver canonical1.1.0 dan ClinicalManagement/handover tetap pemilik masing-masing. Jangan membuat tarif/ledger baru. |
| Verifikasi yang wajib | V-BE + BM-AT-399, BM-AT-400, BM-AT-401, BM-AT-403, BM-AT-404, BM-AT-414, BM-AT-419, BM-AT-421, BM-AT-422 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-219](../task/report/backend/BE-RWI-219.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Target direbut atau source berubah sebelum save menolak seluruh transfer; satu commit mengubah asal WaitingCleaning dan tujuan Occupied.
- Fault di antara end source/new placement menggugurkan seluruh writes; callback handover gagal setelah commit tidak membalik lokasi.

Acceptance negatif:

- Missing/manual-category mismatch tidak menulis history atau menebak kategori default.
- Same key retry tidak menggandakan placement/outbox/handover; tidak menambah acceptance tujuan sebagai fase wajib.

### ✅ BE-RWI-220 — Riwayat semua hunian dan koreksi berversi dapat dibaca dengan scope yang benar

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-220.md) |
| Slice / outcome | Riwayat semua hunian dan koreksi berversi dapat dibaca dengan scope yang benar |
| Ukuran / batas | Satu slice riwayat yang dapat dikoreksi; lima endpoint terdampak, dua hanya reuse. |
| Jejak | FR-BM-011, FR-BM-012; RWI-DEC-286, RWI-DEC-287, RWI-DEC-288; RWI-AC-402, RWI-AC-415, RWI-AC-416, RWI-AC-418; NFR-702, NFR-703, NFR-705 |
| Desain / kontrak | BE14.12; API13.2–13.3; Data14; Integration BM; Flow13; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-12, BM-CAP-13, BM-CAP-14, BM-CAP-15, BM-CAP-17; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Tambah bed/periode usage-history berbasis seluruh placement, enrich by-episode dan adapter corrections. Interval overlap setengah terbuka UTC, stable sort StartDateTime DESC/PlacementId DESC, max page100; room berdasarkan segmen tersimpan. Pertahankan versions dan IsEffectiveForBilling=SupersededByCorrectionId NULL. Reports/export existing hanya regression, bukan pengganti history. Correction locks seluruh affected beds serta Billing OPEN/permission/reason existing, emit OCCUPANCY_CORRECTED canonical. |
| Batas source saat implementasi | Usage-history query/controller/DTO; InpBedOccupancyService correction/by-episode; regression report/export existing. |
| Dependency langsung | `BE-RWI-213` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner episode Rawat Inap; Billing — Yasmina untuk receiver/guard canonical. Receiver tidak ditulis ulang di task ini. |
| Verifikasi yang wajib | V-BE + BM-AT-402, BM-AT-415, BM-AT-416, BM-AT-418 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-220](../task/report/backend/BE-RWI-220.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Episode tanpa transfer/ongoing/correction semua tampil; rename master tidak mengubah snapshot; legacy tanpa snapshot diberi ContextSource jujur.
- Billing OPEN correction menyimpan versi lama dan versi baru; normal transfer yang IsSuperseded tetap efektif bila tidak dikoreksi.

Acceptance negatif:

- HK/OperationalOnly tidak memperoleh history pasien; identitas per row mengikuti hak episode.
- Billing CLOSED atau affected-set/versions tidak cocok menolak; tidak delete committed transfer; query tidak menyalin batas31hari report.

### ✅ BE-RWI-221 — Wrapper admisi transfer IGD membuka episode dan reservasi dalam satu transaksi

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI.** — 2026-10-11: QBE 61 file/0 temuan; parser C# 61 file/0 error; API39/39; hash24/24; diff-check0 error; build 0 error/10 warning; migration/SQL sesuai model; database update PASS 2026-10-12 (0 pending); runtime NOT_RUN; acceptance kartu ditelusuri 4/4, terbukti penuh 0/4; BM-G04 belum ditutup. [Laporan](../task/report/backend/BE-RWI-221.md) |
| Slice / outcome | Wrapper admisi transfer IGD membuka episode dan reservasi dalam satu transaksi |
| Ukuran / batas | Satu wrapper mutation existing. |
| Jejak | FR-BM-003, FR-BM-004; RWI-DEC-281, RWI-DEC-283, RWI-DEC-289, RWI-DEC-290; RWI-AC-408, RWI-AC-419, RWI-AC-421, RWI-AC-422; NFR-701, NFR-704, NFR-706 |
| Desain / kontrak | BE14.14; API13.8; Acceptance22.6; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | BM-CAP-04, BM-CAP-05, BM-CAP-14; reuse/adapter/extension/repair sesuai impact scan; detail cakupan di bawah. |
| Cakupan | Perbarui POST admission-transfers/admit dan InpAdmissionTransferService sebagai caller reserve core. Lookup actor/key/hash mendahului OpenAdmission; expected bed version wajib. Satu outer transaction memuat episode, reserve, disposisi existing dan receipt ResultKind Reservation. Preserve HTTP201 pertama/replay dan permissions existing. |
| Batas source saat implementasi | InpatientAdmissionTransferController, DTOs/InpatientAdmissionTransferDtos.cs, Services/InpAdmissionTransferService.cs. |
| Dependency langsung | `BE-RWI-212` |
| Pekerjaan independen yang boleh sekarang | Review dokumen/hash, inventaris callsite read-only dan siapkan bukti approval. Source task ini belum boleh dimulai; gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner InPatientManagement; konsumen helper frontend perlu diperbarui bersamaan. Scope lintas IGD berupa caller existing, tanpa tabel/endpoint IGD baru. |
| Verifikasi yang wajib | V-BE + BM-AT-408, BM-AT-419, BM-AT-421, BM-AT-422 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-BE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan BE-RWI-221](../task/report/backend/BE-RWI-221.md) — tersedia 2026-10-11; source/statis diperiksa, runtime NOT_RUN |

Acceptance positif:

- Gagal setelah OpenAdmission maupun reserve membatalkan seluruh episode/reservasi/disposisi/receipt.
- Timeout sesudah commit lalu retry same key menghasilkan tepat satu episode/reservation dan IsReplay=true tanpa nested transaction.

Acceptance negatif:

- Key/versi hilang atau target diambil petugas lain menolak tanpa episode yatim.
- Tidak menganggap status selesai BE-RWI-206/207 lama sebagai proof target aman dan tidak mengubah keputusan admisi IGD.

## Traceability dan Handoff Berikutnya

Setiap 14 task di register mempunyai requirement, decision, versi/hash kontrak, AC dan bukti yang diharapkan pada [traceability](requirement-traceability-bed-management.md). Persetujuan source backend tersedia. Build dan validasi migration/SQL sudah dilakukan pada sesi lanjutan; build dan penerapan kedua migration sudah selesai pada 2026-10-12 (0 pending); gap saat ini adalah bukti acceptance/runtime serta gate aktivasi. Tidak adanya automated backend test bukan gap.

Langkah berikutnya: verifikasi acceptance API, concurrency, rollback, backfill dan UAT sesuai kartu pada schema yang sudah diterapkan, lalu tutup gate aktivasi dengan bukti nyata. Status sebagian dipertahankan sampai bukti tersebut tersedia.

## Tambahan PLAN-REPAIR-EPS-009 — 13 Oktober 2026

Task di bawah ditambahkan dari [`PLAN-REPAIR-EPS-009`](../docs/plan-repair/plan-repair-009-pilih-bed-buntu-dan-galat-server.md)
(`FIX-EPS-009-03`, `FIX-EPS-009-04`), disetujui pemilik lewat laporan 13-10-2026 ("coba cek pada pemilihan bed
agar query tidak berat"). Ia tidak mengubah 14 task terkunci `BE-RWI-208`–`221` maupun
`bed-management-delivery-lock.json`.

```text
BE-RWI-211 🟡───>BE-RWI-228 ✅
```

| Task ID / status | Outcome | Requirement / decision | Kontrak | Cakupan | Dependency | Acceptance criteria | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [`BE-RWI-228`](../task/report/backend/BE-RWI-228.md) ✅ | Pembacaan bed (`available-beds`, `bed-board`, monitoring, kelayakan) memakai satu proyeksi tanpa muat ulang dan tanpa query per bed; `bed-board` mengikuti penyaring kamar/kelas/pencarian; pesan 409 tanpa nama exception | `ISS-EPS-009-03`, `-04`, `-T1` | Delta aditif `GET /bed-board` `roomId?`, `patientClassId?`, `search?`; `BedMonitoringQuery.PatientClassId?` | 5 service, 1 controller, 1 DTO `InPatientManagement`; tanpa migration | `BE-RWI-211` (proyeksi bersama) | 4 butir `FIX-EPS-009-03` + 3 butir `FIX-EPS-009-04` | ✅ **Selesai 13 Oktober 2026 (source + build).** Build 0 error/284 warning (sama dengan sebelum task); QBE strict 7 berkas PASS 0 violation; 7/7 AC terpetakan ke source; hitung SQL runtime dan uji papan `NOT RUN`, dikecualikan atas keputusan pemilik; [laporan](../task/report/backend/BE-RWI-228.md) |

### ✅ BE-RWI-228 — Pembacaan bed lebih ringan dan papan mengikuti penyaring

| Field | Isi |
| --- | --- |
| Status | ✅ SELESAI — 13 Oktober 2026 (source + build). Build 0 error/284 warning; QBE strict 7 berkas PASS; 7/7 AC terpetakan ke source; hitung SQL dan uji papan `NOT RUN` (dikecualikan atas keputusan pemilik) |
| Asal | `PLAN-REPAIR-EPS-009` `FIX-EPS-009-03`, `FIX-EPS-009-04` |
| Contoh | Penyaring Rawat Inap + Kelas II → `bed-board` hanya kamar Kelas II beserta angkanya; `available-beds` ±35–45 → ±16–18 perintah SQL |
| Laporan | [Laporan BE-RWI-228](../task/report/backend/BE-RWI-228.md) |
