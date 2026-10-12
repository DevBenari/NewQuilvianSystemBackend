# FIX-EPS-006-08 - Verifikasi slice dan batas handoff

Tanggal: 2026-10-12. Status: SEBAGIAN - validasi source/client selesai, verifikasi transaksi nyata belum dilakukan.

Masukan: [rencana disetujui](../../../docs/plan-repair/plan-repair-006-admisi-transfer-igd-rawat-jalan.md), [kontrak EPS-006 1.0](../../../contracts/admission-transfer-repair-006.md). Wewenang: pengguna meminta seluruh rencana dikerjakan sampai tuntas pada 12 Oktober 2026; backend MHamzah, frontend HamzahV2. Tidak mencakup penerapan migration, commit/push atau deployment.

## Perubahan dan source

Suite unit bermakna menguji payload dua asal, kartu penjamin, deposit, source negatif, consent milik episode, pagination65 dan pemulihan key/body setelah timeout. Browser headed memakai server localhost3726 terpisah dan mock HTTP; data pasien/kasir nyata tidak diubah.

Berkas utama: tests/unit/inpatient-transfer-repair-006.test.mjs; tests/unit/inpatient-bed-admission-helper.test.mjs; tests/unit/inpatient-admission-deposit.test.mjs; tests/e2e/inpatient-transfer-repair-006.spec.mjs. Daftar source frontend relatif terhadap repository QuilvianSystemFrontendDev; backend relatif terhadap NewQuilvianSystemBackend.

## Acceptance dan bukti

Build, lint, QBE, unit dan browser dicatat terpisah. Uji dua petugas, stale bed/rollback, receipt dan clinical lifecycle pada PostgreSQL masih membutuhkan lingkungan berwenang yang sudah memakai migration.

Validasi bersama: backend build konfigurasi Repair006 tanpa apphost: 0 error, 270 warning pada project; QBE strict WorkingTree: PASS, 0 violation. Frontend unit terarah: 28/28 PASS; build production PASS; lint:errors PASS; browser headed HTTP mock: 6/6 PASS. Rincian hasil final dicatat pada matriks berikut; tidak memakai self-link sebagai bukti.


## Matriks verifikasi 12 Oktober 2026

| Pemeriksaan | Hasil aktual | Batas bukti |
| --- | --- | --- |
| Backend build Repair006 | PASS: 0 error, 270 warning | Kompilasi; tidak menjalankan startup atau database update |
| QBE Strict WorkingTree | PASS: 16 file, 0 violation, 0 review, 0 info | Konformansi source; bukan pengujian transaksi |
| Frontend production build | PASS: kompilasi, 488 halaman, postbuild standalone | Next.js 16.2.12; konfigurasi environment existing tidak ditimpa |
| Frontend lint:errors | PASS: seluruh repository; scoped no-undef juga PASS | Run final menggunakan source terkini. Kegagalan parsing sementara pada run sebelumnya tidak muncul pada lint ulang terarah maupun seluruh repository |
| Unit terarah 7 berkas admisi | PASS: 28/28 | Payload, penjamin, deposit, source guard, consent episode, paging dan unknown intent/replay |
| Unit seluruh repository | FAIL: 2701 PASS, 26 FAIL dari 2727 | Kegagalan di 20 berkas lain, daftar di bawah; tidak dinyatakan sebagai baseline bersih yang sudah terbukti |
| Playwright headed EPS-006 | PASS: 6/6, 48,9 detik pada build dan run final terbaru | HTTP mock di localhost3726; tidak membuktikan persistence PostgreSQL atau penerimaan kasir nyata |
| Transaksi PostgreSQL, concurrency dan ledger nyata | NOT_RUN | Migration belum diterapkan dan tidak ada wewenang database terpisah |
| Akun operasional dengan scope unit aktual | NOT_RUN | Mock permission menguji UI allowed/denied; bukan sign-off akun rumah sakit |
| Regresi browser pasien baru/lama/kamar pulih | NOT_RUN | Unit admisi terkait dijalankan; browser cabang lain belum dibuktikan |

Perintah yang dapat diulang dari repository masing-masing:

```powershell
# Backend: tidak memerlukan aplikasi yang sedang berjalan dihentikan.
dotnet build QuilvianSystemBackend.csproj -c Repair006 --no-restore -p:UseAppHost=false -v:q
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -JsonOutputPath "$env:TEMP/quilvian-eps006-qbe-final.json"
# Frontend: npm.cmd dipakai karena execution policy PowerShell.
npm.cmd run lint:errors
npm.cmd run build
node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-transfer-repair-006.test.mjs tests/unit/inpatient-admission-transfer.test.mjs tests/unit/inpatient-bed-admission-helper.test.mjs tests/unit/inpatient-admission-consent.test.mjs tests/unit/inpatient-admission-deposit.test.mjs tests/unit/inpatient-admission-encounter.test.mjs tests/unit/inpatient-admission-payment.test.mjs
# Playwright membutuhkan server lokal dan SESSION_SIGNING_SECRET khusus fixture yang sama.
node node_modules/@playwright/test/cli.js test tests/e2e/inpatient-transfer-repair-006.spec.mjs --headed --workers=1 --reporter=line
```

Log lokal berada di `%TEMP%`: `quilvian-eps006-backend-build-final.log`, `quilvian-eps006-qbe-final.json`, `quilvian-eps006-fe-build-final4.log`, `quilvian-eps006-fe-lint-final3.log`, `quilvian-eps006-fe-scoped-unit-final2.log`, `quilvian-eps006-fe-unit-final.log`, dan `quilvian-eps006-fe-e2e-final5.log`. Laporan ini menyimpan hasilnya agar bukti tidak bergantung pada umur log sementara.

### Kegagalan suite penuh di luar slice ini

26 assertion gagal pada 20 berkas unit berikut: hemodialysis-navigation-and-privacy-audit, hemodialysis-sidebar-navigation, inpatient-admission-cancellation, inpatient-billing-status-badge, inpatient-final-consistency, inpatient-nursing-billing-summary, inpatient-nursing-finishing-roadmap, inpatient-nursing-procedure-and-ancillary, inpatient-nursing-transfer-and-unavailable, inpatient-nursing-workspace-revision, inpatient-nursing-workspace, inpatient-physician-entry, inpatient-physician-post-op-marker, inpatient-procedure-parity, menu-permission-filter, outpatient-prescription-parity, petty-cash-finance-separation, post-op-summary, surgery-booking, ward-pre-op (semuanya `tests/unit/<nama>.test.mjs`). Assertion terkait struktur/navigasi/label pada fitur tersebut; perubahan source atau test fitur tersebut tidak dilakukan untuk membuat suite ini hijau. Working tree memiliki pekerjaan pengguna lain, sehingga hasil ini tidak membuktikan penyebab masing-masing kegagalan sudah ada sebelum task.

### Bukti terhadap alur bisnis

| Alur/risiko | Bukti tersedia | Yang masih perlu dibuktikan |
| --- | --- | --- |
| A: penerimaan permintaan IGD/Poli | Source reader dua jenis dan mock antrean 65/pencarian | Query union/translation dan sumber nyata pada PostgreSQL |
| B/C: administrasi wali dan satu booking | Payload Cash/Insurance/Company, satu endpoint transfer, generic writer dilewati | Penjamin tersimpan, satu episode/reservation, receipt Rp500.000 dan rollback owner |
| D: persetujuan dan selesai | Guard signature/consentId/episodeId; pembacaan consent menentukan tombol selesai | Penyimpanan persetujuan dengan akun operasional dan dokumen hasil nyata |
| Timeout/replay | Stable key/body persist lalu replay pada unit service | Dua request konkuren, timeout sesudah commit, replay deposit pada PostgreSQL |
| Clinical lifecycle | Source result terstruktur; transfer tidak menutup kunjungan asal | Pembacaan ulang clinical/source/arrival pada data uji berwenang |

### Skenario browser yang lulus

1. Antrean 65: halaman kedua dan pencarian server menemukan pasien ke-65; pilih sumber Poli dan refresh tetap memuat nomor kunjungan serta tombol lanjut.
2. Hak Read tidak tersedia: pesan akses tampil dan tidak ada GET transfer.
3. Sumber dibatalkan: buka URL dokter langsung ditahan dan tidak ada POST.
4. HTTP 500 antrean: pesan gagal tampil, bukan antrean kosong.
5. IGD: pembayaran/deposit nol, dokter tanpa writer umum, refresh mempertahankan catatan, pilih/pesan bed, tepat satu POST transfer dengan versi8/key, lalu consent episode hasil mengendalikan selesai.
6. Poli: alur yang sama tanpa disposisi IGD fiktif.

Saat uji awal, perpindahan query langkah melalui router berhenti sebelum review/pembayaran. Cabang transfer diperbaiki memakai History API Next.js; alur positif sesudah perbaikan lulus. Kendala fixture (klik label bed alih-alih tombol Pilih, locator nomor kunjungan yang tidak memasukkan label) diperbaiki pada test. Cold start server pernah melampaui timeout navigasi; timeout test kini60 detik. Tidak menyamakan kegagalan fixture atau cold start dengan hasil akhir aplikasi.

Screenshot hasil lokal: `QuilvianSystemFrontendDev/test-results/tests-e2e-inpatient-transf-d0d6c-a-memakai-endpoint-transfer/transfer-completed.png` (IGD) dan `tests-e2e-inpatient-transf-3fad4-a-memakai-endpoint-transfer/transfer-completed.png` (Poli). Screenshot membuktikan UI data uji, bukan dokumen klinis yang sah. Footer existing dapat menutupi sebagian konten saat scrolling; style dari pekerjaan pengguna dipertahankan dan sign-off visual lintas viewport belum dinyatakan.

## Governance dan dampak

Backend Governance Preflight: HealthServices/InPatientManagement Inp, RegistrationManagement Reg, BillingManagement Bil ACTIVE. Arketipe transaksi/worklist; tidak membuat master/ledger baru. QBE-SVC-001, API-001, PERM-001, VAL-001, DTO-001, TXN-001, PAGE-001, LOG-001, CFG-002, CODE-004. Owner service existing, metadata permission/API cocok; transaksi dan receipt coordinator dipertahankan.

Frontend UI gate: REUSE 5, COMPOSE 1, NEW 0; BaseButton, StatusBadge, InformationAlert, DataFilter, FilterSelect, Pagination dan langkah admisi existing. CSS dan perubahan user sebelumnya dipertahankan. Default deferCreation/transfer false menjaga jalur langsung; source transfer tidak memanggil generic episode writer.

AUTOMATED TEST: NOT APPLICABLE untuk backend - repository tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md). Pengujian frontend menggunakan runner node:test dan Playwright existing, tanpa dependency baru.

## Migration, keterbatasan dan handoff

Migration 20261012041938_AddAdmissionTransferOrigin + Designer/snapshot siap; Up hanya tiga kolom nullable, dua index dan satu FK Restrict. Database update tidak dijalankan. Backend hasil perubahan memerlukan schema migration ini sebelum dipakai untuk uji aplikasi; query encounter membaca kolom asal yang baru. Belum ada bukti commit/rollback/konkurensi PostgreSQL, receipt nyata atau perjalanan pasien nyata. Setelah pemilik menerapkan migration pada database uji berwenang, jalankan matriks transaksi pada rencana bagian 8 dan baca ulang source, episode, penjamin, reservation serta movement deposit. Klaim selesai administratif baru sah sesudah consent tersimpan, terpisah dari arrival/departure fisik.

Database handoff mengikuti [AGENTS.md](../../../../../../../AGENTS.md): "Menjalankan migration atau `Update-Database` terhadap database apa pun memerlukan instruksi eksplisit terpisah." Preferensi pemilik juga menetapkan pemilik menerapkan migration sendiri. Source dan migration siap direview; seluruh status runtime yang belum terbukti tetap SEBAGIAN/NOT_RUN.

Urutan handoff: pemilik memilih database uji dan menerapkan migration yang disiapkan; jalankan backend hasil build; siapkan permintaan IGD/surat Poli, kartu penjamin, bed dan shift/metode kasir yang sah; jalankan matriks bagian8 rencana (termasuk dua petugas, stale bed, rollback, timeout/replay); baca ulang semua efek lewat API owner. Deployment/sign-off tetap menunggu bukti tersebut. Tidak ada commit, push atau deploy pada pekerjaan ini.

Kebersihan diff: backend git diff --check PASS; frontend diff source perbaikan PASS. Pemeriksaan seluruh diff frontend menemukan trailing blank line pada tiga berkas pekerjaan pengguna lain (visit-type-step, patient-entry-choice-step, base-form-control.module.css); berkas tersebut tidak dirapikan oleh task ini. Artefak tracked test-results/.last-run.json dipulihkan setelah runner selesai; screenshot hasil uji tetap tersedia. Server test milik task sudah dihentikan, server pengguna tidak dihentikan.
