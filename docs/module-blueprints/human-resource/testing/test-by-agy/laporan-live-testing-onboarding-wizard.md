# Laporan Live Testing — Multi-Step Onboarding Pegawai Baru 7 Tahap (`FIN-HRD-ONB-WIZ-06`)

| Atribut | Nilai |
| --- | --- |
| **Modul & Fitur** | Human Resource — Penerimaan & Onboarding (Wizard 7 Tahap + 7 Submenu Lifecycle) |
| **Dokumen Acuan** | [`INSTRUKSI_AGENT_HRD_ONBOARDING_WIZARD.md`](../../docs/instruksi/INSTRUKSI_AGENT_HRD_ONBOARDING_WIZARD.md) (`FIN-HRD-ONB-WIZ-01..06`) |
| **Laporan Terkait** | [FIN-HRD-ONB-WIZ-01-02 (BE)](../../task/report/backend/FIN-HRD-ONB-WIZ-01-02.md) · [FIN-HRD-ONB-WIZ-03-05 (FE)](../../task/report/frontend/FIN-HRD-ONB-WIZ-03-05.md) |
| **Tanggal Pengujian** | 26 November 2026 |
| **Pelaksana Uji** | Agent Pengujian Quilvian (Backend .NET + PostgreSQL Dev `QuilvianNewDevHamzah` + SQLite In-Memory xUnit + Playwright Headed Chromium) |
| **Hasil Keseluruhan** | 🟡 **LULUS DENGAN CATATAN LINGKUNGAN** (`13/13` UAT Browser `PASS`, `0` 5xx, `0` pageError, `3/3` xUnit `PASS`, BE API check `ok: true`; alur data kasus detail di browser memakai `ROUTE_INJECTED` karena DB dev `0` `HrdOnboardingCase`) |

---

## 1. Ringkasan Eksekutif & Klasifikasi Kejujuran Bukti

Pengujian dilakukan menyeluruh pada lapisan **Backend API**, **Logika Workflow & Keamanan Database**, **Unit Helper Frontend**, **Audit CSS/Build Produksi**, serta **Antarmuka Browser Nyata (Chromium Headed)**. Sesuai aturan integritas bukti, setiap hasil uji diklasifikasikan secara jujur menjadi tiga kategori:

| Kode Bukti | Arti Klasifikasi | Cakupan pada Laporan Ini |
| --- | --- | --- |
| `LIVE_BE` | Permintaan HTTP sungguhan melewati Frontend Next.js & Backend .NET langsung ke PostgreSQL `QuilvianNewDevHamzah` (tanpa injeksi mock data). | Login sesi nyata, muat 7 submenu Penerimaan & Onboarding, RBAC penolakan akun non-HRD (`Novi`), cek endpoint `401`/`200`/`403`/`404`, `coordinator-options` (`30` opsi), dan Swagger UI. |
| `ROUTE_INJECTED` | UI Next.js, CSS Module, React Hook, validasi form, dan `ConfirmModal` nyata dijalankan di Chromium, dengan respons API kasus onboarding disimulasikan melalui `page.route` *(karena DB dev masih memiliki `0` kasus onboarding dan demi menjaga `0` penulisan ke DB)*. | Interaksi lengkap Wizard Tahap 1 s.d. 7 pada kasus **Ns. Rina Putri Santoso, S.Kep.** (`HND-2026-0014`), guard `Nyatakan Mandiri` Submenu 5, round-trip `?onboardingCaseId=` Submenu 6, konflik `409`/`VR-ONB-12`, kasus `Cancelled`, serta layout ponsel `390x844`. |
| `SQLITE_XUNIT` | Eksekusi langsung `HrdOnboardingCaseService` & service pendamping C# di EF Core SQLite in-memory (`OnbRunner.csproj`) terhadap biner `b1cb2333`. | Pembuktian atomisitas database, isolasi unit antar ruangan, `VR-ONB-03`, `ONB-G04` (tanpa bypass HRD), `VR-ONB-10`, `VR-ONB-12`, `VR-ONB-16`, *magic bytes* PDF, *Rehire*, siklus manajer, sanitasi error, dan status siap penjadwalan. |

---

## 2. Lingkungan Uji & Temuan Operasional Server Lokal

- **Frontend Live**: `http://localhost:3000` (berjalan bersama hasil kompilasi `npm run build` Next.js `16.2.12 (Turbopack)`, `935/935` rute).
- **Backend Terisolasi yang Diuji (`b1cb2333`)**: Biner segar hasil build `NewQuilvianSystemBackend` commit `b1cb2333` dijalankan sementara pada `https://localhost:7194` (untuk browser UAT) dan `http://127.0.0.1:7195` (untuk API checker), lalu dihentikan kembali dengan bersih setelah pengujian selesai.
- **Temuan Penting pada Backend Lokal Eksisting (`https://localhost:7184`, PID `8376`)**: Saat pemeriksaan awal, host backend yang sudah berjalan di port `7184` ternyata masih memakai berkas `NewQuilvianSystemBackend\bin\Debug\net9.0\QuilvianSystemBackend.dll` tanggal `11/25` sehingga mengembalikan HTTP `400` pada `GET /onboarding-cases/coordinator-options`. Setelah dijalankan biner terbaru dari commit `b1cb2333` di port uji terisolasi, endpoint yang sama merespons HTTP `200` dengan `30` pilihan koordinator. **Tindakan diperlukan**: pengembang perlu melakukan rebuild & restart service backend pada port `7184`.
- **Kondisi Database Dev (`QuilvianNewDevHamzah`)**: Berisi `0` baris `HrdOnboardingCase`, `0` `HrdCandidateHiringHandoff`, `0` `MstOnboardingTemplateTask`, dan akun non-SuperAdmin (`hartini`, `yudha`, `endang`, `novi`) belum memiliki grant `SysAccessPolicy` untuk `WorkforceOnboardingCase` di DB dev.

---

## 3. Verifikasi Backend & Frontend Sebelum Sesi Browser

| Lapisan | Alat / Perintah | Hasil Terukur |
| --- | --- | --- |
| Build Backend | `dotnet build NewQuilvianSystemBackend.csproj --no-restore -p:SkipMigrationMetadata=true` | `Build succeeded. 0 Error(s)` |
| Konformansi QBE | `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path ...\HrdOnboardingCaseService.cs` | `Files evaluated: 1, VIOLATION: 0, REVIEW: 0, INFO: 0, Final result: PASS` |
| Suite Unit/Integrasi BE (`SQLITE_XUNIT`) | `dotnet test OnbRunner.csproj` (`OnboardingWizardTests.cs`) | `3/3 PASS` (`0 Warning, 0 Error`, durasi `1.5468m`) |
| Cek API Live BE (`LIVE_BE`) | `node ...\be-live-api-check.mjs` terhadap biner `b1cb2333` | `ok: true` (`401` tanpa login, `200` SuperAdmin 7 submenu & `30` koordinator, `404` ID acak, `403` akun tanpa izin DB, Swagger UI `200`) |
| Audit CSS & Anti-Regresi FE | `node check-css-usage.mjs` + grep Tailwind/neutral/hex pada 10 JSX + CSS Module | `OK — 10 berkas diperiksa, 0 temuan.` + grep `0` temuan |
| Linter & Build FE | `npx eslint --max-warnings=0` (15 file), `npm run lint:errors`, `npm run build` | `0` error, `0` warning, build `935/935` rute + standalone siap (`EXIT=0`) |
| Unit Test Helper FE | `node --import ./tests/helpers/register.mjs --test` (`onboarding-wizard-utils` & `recruitment-journey-utils`) | `11/11 PASS` (`2 suites, 0 fail`) |

---

## 4. Matriks Pengujian Live Browser Chromium (`13/13 PASS`)

Eksekusi dijalankan dengan Playwright Headed Chromium via skrip `QuilvianSystemFrontendDev/test-with-agy/scripts/uat-onboarding-wizard.mjs` pada resolusi desktop `1366x860` dan mobile `390x844`. Ringkasan hasil akhir pada `browser-uat-results.json`: **`13/13 PASS`**, **`http5xx = 0`**, **`pageErrors = 0`**, dan **`59` pemanggilan API terekam**.

| ID Uji | Mode Bukti | Skenario & Asersi Nyata yang Diuji | Hasil | Bukti Screenshot (`screenshots/onb-wiz-1126/`) |
| --- | :---: | --- | :---: | --- |
| `SUBMENU-REG-07` | `LIVE_BE` | Login akun SuperAdmin, mengunjungi ketujuh submenu *Penerimaan & Onboarding* (`/hr/lifecycle/worklist`, `/hr/recruitment/handoffs`, `/hr/lifecycle/onboarding-cases`, `/hr/lifecycle/onboarding-templates`, `/hr/lifecycle/clinical-preceptorships`, `/hr/lifecycle/onboarding-access-requests`, `/hr/lifecycle/settings`), memastikan seluruhnya memuat bersih tanpa `5xx` atau `pageerror`. | ✅ PASS | [`01-submenu-onboarding-cases-list.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/01-submenu-onboarding-cases-list.png) |
| `WIZ-01-STAGE1` | `ROUTE_INJECTED` | Navigasi ke `/onboarding-cases/{id}?stage=tidak-valid`: Hero terverifikasi **tidak memiliki tombol mutasi**, alert fallback query invalid muncul, stepper 7 tahap lengkap (`acceptance`..`readiness`), dan info `HND-2026-0014` serta peringatan *Pembatasan Kerja Medis* tampil di Tahap 1. | ✅ PASS | [`02-stage1-acceptance-invalid-fallback.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/02-stage1-acceptance-invalid-fallback.png) |
| `WIZ-02-ATTENDANCE` | `ROUTE_INJECTED` | Eksekusi alur modal: `Catat Hadir Hari Pertama` (verifikasi payload `attendedDate` & `rowVersion`), `Batalkan Hadir` (`VR-ONB-17`), `Ganti Koordinator` menjadi `Yudha Hermawan`, dan `Catat Ulang Hadir`. | ✅ PASS | *(tercatat pada `uatLog`)* |
| `WIZ-03-STAGE2-EMP` | `ROUTE_INJECTED` | Tahap 2 (*Data Pegawai*): memastikan persis `9` kartu kelompok data pegawai ter-render (`employee-group-*`), NIK tersamar `3174********0009`, info *Pemisahan Master Data* (`HRD-DEC-087`), & tautan Employee 360 tampil. | ✅ PASS | [`03-stage2-employee-9groups.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/03-stage2-employee-9groups.png) |
| `WIZ-04-STAGE3-CTR` | `ROUTE_INJECTED` | Tahap 3 (*Perjanjian Kerja*): verifikasi nominal gaji `6.500.000` muncul saat `canViewAmount = true` dan berubah menjadi `Disembunyikan` saat `canViewAmount = false`, lalu aksi `Periksa Butir Otomatis` meluluskan kontrak `Signed` + dokumen scan. | ✅ PASS | [`04-stage3-contract-valid.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/04-stage3-contract-valid.png) |
| `WIZ-05-STAGE4-PLC` | `ROUTE_INJECTED` | Tahap 4 (*Penempatan Pegawai*): memastikan unit kerja ICU, kode uraian tugas `JD-PRW-ICU-01`, dan nama atasan langsung `Ns. Endang Purwanti` tampil utuh. | ✅ PASS | [`05-stage4-placement.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/05-stage4-placement.png) |
| `WIZ-06-STAGE5-ORI` | `ROUTE_INJECTED` | Tahap 5 (*Orientasi & Pendampingan*): butir `UNIT-VENT` terkunci berlabel `Khusus Kepala Unit`; tombol `Simpan` pada modal `GEN-EVIDENCE` dinonaktifkan saat berkas kosong dan aktif saat file PDF dipilih (`daftar-hadir-orientasi.pdf`) disertai pengiriman `FormData` & tampilan tautan unduh; filter teks `"Ventilator"` menyisakan `1` baris dan `Reset Filter` memulihkan tabel. | ✅ PASS | [`06-stage5-modal-evidence-pdf.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/06-stage5-modal-evidence-pdf.png), [`06b-stage5-orientation-complete.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/06b-stage5-orientation-complete.png) |
| `WIZ-07-STAGE6-ACC` | `ROUTE_INJECTED` | Tahap 6 (*Permintaan Akses IT*): klik `Coba Lagi` akun ESS mengubah status menjadi `Portal Aktif`, tanda tiket SIMRS selesai, klik tautan antrean menuju `/onboarding-access-requests?onboardingCaseId=...`, lalu kembali ke Tahap 6 melalui `Kembali ke Detail Kasus`. | ✅ PASS | [`07-stage6-access-completed.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/07-stage6-access-completed.png) |
| `WIZ-08-STAGE7-RDY` | `ROUTE_INJECTED` | Tahap 7 (*Verifikasi Siap Kerja*): simulasi blokir *Konflik Empat Mata (`VR-ONB-12`)*, lalu saat bersih menembak konflik `409 Conflict` yang menampilkan pesan `"telah berubah oleh pengguna lain"` di dalam modal, kemudian pengesahan ulang sukses menjadi `Siap Kerja (Aktif)`, dan item opsional `GEN-OPT` tetap dapat diselesaikan pasca-ready (`HRD-DEC-088`). | ✅ PASS | [`08a-stage7-four-eyes-blocker.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/08a-stage7-four-eyes-blocker.png), [`08b-stage7-readiness-completed.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/08b-stage7-readiness-completed.png) |
| `WIZ-09-CANCELLED` | `ROUTE_INJECTED` | Kasus sebelum hari pertama: mengeksekusi modal `Batalkan Bergabung` dengan alasan *Mengundurkan Diri*, banner *Kasus Onboarding Dibatalkan* tampil dan seluruh tombol aksi mutasi hilang. | ✅ PASS | [`09-cancelled-case-locked.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/09-cancelled-case-locked.png) |
| `WIZ-10-MOBILE-390` | `ROUTE_INJECTED` | Pemeriksaan viewport `390x844` pada `/onboarding-cases/{id}?stage=orientation`: `documentElement.scrollWidth <= clientWidth` (`375/375px`), stepper & tabel tanpa horizontal page overflow. | ✅ PASS | [`10-mobile-390px-wizard.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/10-mobile-390px-wizard.png) |
| `WIZ-11-PRECEPTOR-GUARD` | `ROUTE_INJECTED` | Submenu 5 (`/hr/lifecycle/clinical-preceptorships/{id}`): pada status `1` (`Preceptor Ditunjuk`), tombol `Nyatakan Mandiri` tersembunyi (`0`) sedangkan `Ganti Preceptor` tetap tampil (`1`); pada status `2` (`Didampingi`), tombol `Nyatakan Mandiri` muncul (`1`). | ✅ PASS | [`10b-submenu5-preceptor-guard.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/10b-submenu5-preceptor-guard.png) |
| `RBAC-NOVI-LIVE` | `LIVE_BE` | Login sungguhan akun non-HRD (`Novi`) menuju `/hr/lifecycle/onboarding-cases/{id}`: sistem menolak secara bersih dengan pesan **Akses Terbatas** dan `0` tombol mutasi wizard di DOM. | ✅ PASS | [`11-rbac-novi-denied.png`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/11-rbac-novi-denied.png) |

---

## 5. Cakupan Aturan Bisnis Kunci (`SQLITE_XUNIT` + `LIVE_BE` + `ROUTE_INJECTED`)

Gabungan ketiga lapisan uji membuktikan seluruh aturan kritis pada [`INSTRUKSI_AGENT_HRD_ONBOARDING_WIZARD.md`](../../docs/instruksi/INSTRUKSI_AGENT_HRD_ONBOARDING_WIZARD.md) telah diimplementasikan dan teruji:

1. **Keamanan & Isolasi Peran/Unit**: Akun tidak terautentikasi ditolak (`401`), akun tanpa grant DB ditolak (`403` pada API & *Akses Terbatas* pada UI), Kepala Unit IGD diblokir saat membaca/mengunduh bukti kasus ICU (`403`), worklist/preceptorship/akses request terlingkup unit, serta HRD dan Kepala Unit saling terisolasi pada checklist orientasi tanpa *bypass* (`ONB-G04`).
2. **Privasi Data Pegawai & Kontrak**: Masking NIK (`3174********0009`), masking nominal gaji bagi user tanpa izin `ViewAmount`, serta sanitasi pesan kegagalan aktivasi akun agar pengecualian database tidak bocor ke UI.
3. **Integritas Siklus & Bukti**: Validasi *magic bytes* `%PDF` pada item `EvidenceRequired`, penolakan preceptor nonaktif dan siklus hirarki atasan langsung (`422`), isolasi kontrak lama kandidat *Rehire*, keharusan scan dokumen pada kontrak `Signed`, penolakan hadir sebelum `PlannedStartDate` (`VR-ONB-10`), penahanan empat mata `VR-ONB-12`, concurrency `409 RowVersion`, pengembalian kursi pada `CancelJoin`, dan editabilitas butir opsional pasca-ready (`HRD-DEC-088`).
4. **Kesesuaian Antarmuka 7 Submenu & Desain Token**: Hero bersih dari mutasi, `<Suspense>` pada seluruh pembaca `useSearchParams()`, `0` temuan CSS Checker, `0` Tailwind, `0` error ESLint, serta bersih pada viewport `390x844`.

---

## 6. Perbaikan yang Ditemukan Saat Uji & Telah Diuji Ulang

Selama siklus pengujian dan peninjauan ulang, **4 perbaikan nyata** ditemukan dan langsung diperbaiki sebelum rangkaian uji akhir (`13/13 PASS` dan `npm run build` `935/935`) dijalankan ulang hingga tuntas:
1. Perbaikan `LifecycleService.handleError` agar meneruskan properti `.response`/`.request` ke `ConfirmModal` saat backend mengembalikan `409 Conflict` (terbukti pada `WIZ-08-STAGE7-RDY`).
2. Pemindahan `Reset Filter` ke atas `DataTable` serta perapihan kolom pada Tahap 5 (terbukti pada `WIZ-06-STAGE5-ORI` dan `WIZ-10-MOBILE-390`).
3. Pembatasan tombol `Nyatakan Mandiri` hanya untuk status `Didampingi` (`=== 2`) di `ClinicalPreceptorshipDetailView` (terbukti pada `WIZ-11-PRECEPTOR-GUARD`).
4. Dukungan `?onboardingCaseId=&returnTo=` di dalam `<Suspense>` pada Submenu 6 beserta tautan pulang dari Tahap 6 wizard (terbukti pada `WIZ-07-STAGE6-ACC`).

---

## 7. Celah Tersisa yang Bersifat Operasional (Bukan Cacat Kode)

- **Server Lokal Port `7184` Perlu Restart/Rebuild**: Agar API lokal di `https://localhost:7184` menggunakan kode `db900f14` + `b1cb2333`, proses backend lama perlu di-rebuild dan dijalankan ulang.
- **Database Dev `QuilvianNewDevHamzah` Kosong untuk Skenario End-to-End DB**: DB tim/dev belum memiliki baris serah terima/kasus onboarding nyata (`0` baris) serta belum memiliki seed grant `WorkforceOnboardingCase` pada role operasional HRD/Kepala Unit selain SuperAdmin. Saat data & otorisasi tersebut tersedia di lingkungan staging/dev, pengujian end-to-end persisten penuh ke PostgreSQL dapat langsung mengonfirmasi jalur yang saat ini berlabel `ROUTE_INJECTED` + `SQLITE_XUNIT`.
