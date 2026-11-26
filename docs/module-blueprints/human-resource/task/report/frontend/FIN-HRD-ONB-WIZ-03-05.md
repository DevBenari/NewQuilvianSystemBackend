# Laporan Task Tracked Frontend — Multi-Step Onboarding Pegawai Baru 7 Tahap (`FIN-HRD-ONB-WIZ-03..05`)

| Atribut | Nilai |
| --- | --- |
| **Label Kerja Lokal** | `FIN-HRD-ONB-WIZ-03`, `FIN-HRD-ONB-WIZ-04`, `FIN-HRD-ONB-WIZ-05` *(slice `S-C4-ONB` — Penerimaan & Onboarding)* |
| **Hubungan Task Canonical** | Pelengkap integrasi layar lintas `FE-HRD-059..062`, `FE-HRD-066`, `FE-HRD-068..070` *(status canonical tetap utuh)* |
| **Sumber Instruksi** | [`INSTRUKSI_AGENT_HRD_ONBOARDING_WIZARD.md`](../../../docs/instruksi/INSTRUKSI_AGENT_HRD_ONBOARDING_WIZARD.md) |
| **Repositori & Branch** | `QuilvianSystemFrontendDev` — branch `HamzahV2` |
| **Commit Referensi + Perubahan Lokal** | `7dfae4cdb` + 4 penyempurnaan lanjutan belum ter-commit pada Submenu 5, Submenu 6, dan tautan deep-link wizard |
| **Tanggal Penyelesaian & Uji** | 26 November 2026 |
| **Status Akhir** | ✅ **IMPLEMENTED & VERIFIED** *(`lint:errors` `0`, `eslint --max-warnings=0` `0`, `check-css-usage` `0/10`, unit test `11/11`, `npm run build` `935/935`, Chromium UAT `13/13`)* |

---

## 1. Keadaan yang Ditemukan di Awal & Perbaikan Lanjutan

Layar `/hr/lifecycle/onboarding-cases/[id]` sebelumnya belum menyediakan antarmuka panduan bertahap (*multi-step wizard*) yang menyatukan seluruh data kasus dari serah terima hingga verifikasi siap kerja dalam satu halaman berstruktur. Selain implementasi wizard utama pada commit `7dfae4cdb`, inspeksi dan pengujian live menemukan beberapa celah nyata yang langsung diperbaiki:

1. **Penanganan pesan konflik dari API di `LifecycleService.handleError`**: Sebelumnya `handleError` membuang properti `response` dan `request` pada objek error yang dilemparkan, sehingga ketika backend mengirim `409 Conflict` atau `422` pada dialog konfirmasi (`ConfirmModal`), UI hanya menampilkan pesan umum `"Aksi onboarding gagal diproses."`. Diperbaiki dengan tetap meneruskan `.response` dan `.request` pada `Error` yang dilemparkan.
2. **Filter pada Tahap 5 (`Orientasi & Pendampingan`)**: Komponen `DataTable` tidak merender prop `toolbar`, sehingga tombol `Reset Filter` dipindahkan eksplisit di atas tabel bersama perapihan lebar kolom tabel agar tetap bersih di layar desktop dan ponsel.
3. **Guard Tombol *Nyatakan Mandiri* di Submenu 5 (`ClinicalPreceptorshipDetailView`)**: Sebelumnya tombol `Nyatakan Mandiri` dapat tampil pada status `1` (`Preceptor Ditunjuk` / *Pending Onboarding*). Diperbaiki pada `clinical-preceptorship-detail-view.jsx` sehingga `canDeclareIndependent` hanya aktif bila `canManage && info?.preceptorshipStatus === 2` (`Didampingi`), sementara `Ganti Preceptor` tetap tersedia pada status `[1, 2]`.
4. **Dukungan `?onboardingCaseId=&returnTo=` pada Submenu 6 (`OnboardingAccessRequestsView`)**: Halaman `/hr/lifecycle/onboarding-access-requests` belum membaca parameter query `onboardingCaseId` dan `returnTo`. Halaman `page.jsx` dibungkus `<Suspense fallback={null}>`, `OnboardingAccessRequestsView` membaca kedua parameter via `useSearchParams()` untuk memfilter permintaan akses kasus terkait sekaligus menampilkan tombol *Kembali ke Detail Kasus* pada `Hero.actions` (saat `safeReturnPath` berawalan `"/hr/"`), dan tautan dari Tahap 6 wizard (`wizard-open-access-queue-btn`) disesuaikan agar menyertakan kedua parameter tersebut.

---

## 2. Proses Bisnis dari Sisi Pengguna (Skenario RS MMC Jakarta)

Berikut alur pengalaman pengguna HRD, Kepala Unit ICU, dan petugas akses IT saat memandu **Ns. Rina Putri Santoso, S.Kep.** (*Perawat Pelaksana ICU*) dari serah terima hingga dinyatakan *Siap Kerja*:

1. **Masuk ke Wizard Kasus Onboarding**: Pengguna membuka menu *Kasus Onboarding* (`/hr/lifecycle/onboarding-cases`) dan memilih satu kasus (`/hr/lifecycle/onboarding-cases/[id]`). *Hero* halaman hanya menampilkan identitas ringkas kasus serta navigasi kembali/muat ulang **tanpa tombol mutasi** di area Hero.
2. **Navigasi 7 Tahap Terpadu**: Di bawah Hero terdapat stepper 7 tahap (`Konfirmasi Penerimaan` → `Data Pegawai` → `Perjanjian Kerja` → `Penempatan Pegawai` → `Orientasi & Pendampingan` → `Permintaan Akses IT` → `Verifikasi Siap Kerja`) yang sinkron dengan parameter URL `?stage=...`. Jika URL diberi `?stage=tidak-valid`, UI menampilkan alert peringatan aman dan otomatis mengarah ke tahap aktif yang relevan.
3. **Panel Kanan Kontekstual (*Side Panel*)**: Panel sisi kanan selalu menampilkan ringkasan progres butir wajib, daftar penghalang (*blockers*), serta tombol aksi yang diizinkan (`Catat Hadir Hari Pertama`, `Batalkan Hadir Hari Pertama`, `Ganti Koordinator`, `Periksa Butir Otomatis`, `Sahkan Siap Kerja`, atau `Batalkan Bergabung`).
4. **Menyelesaikan Tahap 1 s.d. 7 Secara Runtut**:
   - **Tahap 1**: HRD melihat data `HND-2026-0014` dan alert pembatasan kerja medis MCU bila ada, lalu mencatat tanggal hadir pertama melalui `ConfirmModal`.
   - **Tahap 2**: HRD meninjau ringkasan 9 kelompok data pegawai (*Identitas & Kontak* s.d. *Rekening Bank & Pajak*), dengan NIK otomatis tersamar (`3174********0009`), info pemisahan master data (`HRD-DEC-087`), serta tautan langsung ke *Employee 360*.
   - **Tahap 3**: Nominal gaji hanya muncul bagi pemilik izin (*ViewAmount*); pengguna lain melihat teks tersamar (`Disembunyikan`). Setelah kontrak berstatus `Signed`/`Active` dan memiliki dokumen scan, klik `Periksa Butir Otomatis` mengubah status kontrak onboarding menjadi sah.
   - **Tahap 4**: Meninjau unit kerja (`Instalasi Rawat Intensif (ICU)`), kode uraian tugas (`JD-PRW-ICU-01`), penanggung jawab atasan langsung, serta indikasi konflik manajer bila ada.
   - **Tahap 5**: Daftar checklist dapat disaring berdasarkan pencarian, lapis template, status, maupun penanggung jawab, serta dilengkapi `Reset Filter`. Item milik Kepala Unit seperti `UNIT-VENT` menampilkan pengunci `Khusus Kepala Unit` bagi HRD. Pada item wajib lampiran (`GEN-EVIDENCE`), tombol `Simpan` di modal dinonaktifkan hingga file PDF dipilih, lalu unduhan bukti terverifikasi bekerja di sisi browser.
   - **Tahap 6**: HRD atau IT dapat menekan `Coba Lagi` pada akun ESS yang gagal aktivasi, menandai tiket SIMRS selesai, atau mengeklik tautan antrean ke `/hr/lifecycle/onboarding-access-requests?onboardingCaseId=...&returnTo=...` dan kembali ke wizard melalui *Kembali ke Detail Kasus*.
   - **Tahap 7**: Jika terdapat pelanggaran empat mata (`VR-ONB-12`), alert merah tampil jelas dan aksi pengesahan ditahan. Saat bebas penghalang, HRD penyetuju mengesahkan *Siap Kerja*; jika terjadi konflik concurrency `409`, pesan spesifik tampil jelas di modal agar pengguna memuat ulang state terbaru sebelum menyetujui. Setelah kasus berstatus *Siap Kerja (Aktif)*, item opsional seperti `GEN-OPT` tetap dapat diubah statusnya, sedangkan seluruh mutasi lain pada kasus `Cancelled` langsung dikunci rapat.

---

## 3. Perubahan yang Dikerjakan & Kepatuhan Standar UI

### 3.1 Daftar File Frontend

| File | Peran dalam Implementasi |
| --- | --- |
| `src/app/hr/lifecycle/onboarding-cases/[id]/page.jsx` | Entry route wizard detail kasus onboarding dengan pembungkus `<Suspense>`. |
| `src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx` | Komponen utama halaman wizard yang merangkai Hero, `InformationAlert` fallback query, stepper 7 tahap, panel tahap aktif, dan panel aksi samping. |
| `src/components/view/hr/lifecycle/onboarding-wizard/onboarding-wizard-stepper.jsx` | Komponen stepper 7 tahap responsif dengan badge status terstandardisasi. |
| `src/components/view/hr/lifecycle/onboarding-wizard/onboarding-wizard-stages-early.jsx` | Konten Tahap 1 (*Konfirmasi Penerimaan*), Tahap 2 (*Data Pegawai* 9 kelompok), Tahap 3 (*Perjanjian Kerja* + masking gaji), dan Tahap 4 (*Penempatan Pegawai*). |
| `src/components/view/hr/lifecycle/onboarding-wizard/onboarding-wizard-stages-late.jsx` | Konten Tahap 5 (*Orientasi & Pendampingan* beserta `Reset Filter`, upload bukti PDF, unduh, & role lock), Tahap 6 (*Permintaan Akses IT* + tautan `?onboardingCaseId=&returnTo=`), dan Tahap 7 (*Verifikasi Siap Kerja* & blokir empat mata). |
| `src/components/view/hr/lifecycle/onboarding-wizard/onboarding-wizard-side-panel.jsx` | Panel sisi kanan untuk progres butir wajib, penghalang aktif, dan pemicu aksi mutasi. |
| `src/components/view/hr/lifecycle/onboarding-wizard/onboarding-wizard-dialogs.jsx` | Seluruh dialog modal konfirmasi berbasis `ConfirmModal` (hadir hari pertama, batal hadir, ganti koordinator, periksa otomatis, selesai/batal butir, bukti PDF, retry/selesai akses, siap kerja, batal bergabung). |
| `src/lib/hooks/hr/lifecycle/use-onboarding-case-wizard.js` | Custom hook pengelolaan data kasus wizard, sinkronisasi URL `?stage=`, loading/error state, serta handler aksi mutasi & FormData upload. |
| `src/utils/hr/lifecycle/onboarding-wizard-utils.js` | Fungsi helper murni resolusi tahap default/query, mapping `StatusBadge`, filter checklist, penentu editabilitas opsional pasca-ready, prev/next stage, dan pembentukan `returnTo` href. |
| `src/lib/constants/hr/lifecycle/lifecycle-constants.js` & `src/lib/services/hr/lifecycle-service.js` | Konstanta route lifecycle serta pemanggilan API backend dan pemeliharaan `.response`/`.request` pada `LifecycleService.handleError`. |
| `src/style/hr/lifecycle/onboarding-case-wizard.module.css` | Stylesheet CSS Module berbasis design token resmi Quilvian tanpa warna hex/rgb/rgba mentah maupun utilitas Tailwind. |
| `src/components/view/hr/lifecycle/clinical-preceptorship-detail-view.jsx` | Pembatasan `canDeclareIndependent` hanya pada `preceptorshipStatus === 2` (*Didampingi*). |
| `src/app/hr/lifecycle/onboarding-access-requests/page.jsx` & `src/components/view/hr/lifecycle/onboarding-access-requests-view.jsx` | Pembungkus `<Suspense>`, pembacaan `?onboardingCaseId=&returnTo=`, pemfilteran antrean akses kasus, dan tombol *Kembali ke Detail Kasus* di `Hero.actions`. |
| `tests/unit/onboarding-wizard-utils.test.mjs` | `6` skenario unit test terisolasi untuk utilitas wizard onboarding. |

### 3.2 Gerbang Kepatuhan UI (`UI GATE: PASS`)

| Pemeriksaan UI | Hasil Aktual | Status |
| --- | --- | :---: |
| `node check-css-usage.mjs` pada 10 berkas JSX onboarding | `OK — 10 berkas diperiksa, 0 temuan.` | ✅ **PASS** |
| Anti-regresi Tailwind, `status="neutral"`, warna hex/rgb/rgba mentah (10 JSX + CSS Module) | Hasil grep kosong (`0` temuan) | ✅ **PASS** |
| Pemisahan tombol mutasi dari `Hero` & penggunaan `<Suspense>` pada `useSearchParams()` | Hero bebas tombol mutasi; rute `?stage=` & `?onboardingCaseId=` dibungkus `<Suspense>` | ✅ **PASS** |
| Responsivitas viewport mobile `390x844` | `scrollWidth <= clientWidth` (`375/375px` tanpa horizontal page overflow) | ✅ **PASS** |

Screenshot verifikasi visual tersimpan di `QuilvianSystemFrontendDev/test-with-agy/screenshots/onb-wiz-1126/`:
`01-submenu-onboarding-cases-list.png`, `02-stage1-acceptance-invalid-fallback.png`, `03-stage2-employee-9groups.png`, `04-stage3-contract-valid.png`, `05-stage4-placement.png`, `06-stage5-modal-evidence-pdf.png`, `06b-stage5-orientation-complete.png`, `07-stage6-access-completed.png`, `08a-stage7-four-eyes-blocker.png`, `08b-stage7-readiness-completed.png`, `09-cancelled-case-locked.png`, `10-mobile-390px-wizard.png`, `10b-submenu5-preceptor-guard.png`, `11-rbac-novi-denied.png`.

---

## 4. State yang Ditangani di Layar

1. **State Memuat (*Loading*)**: Menampilkan indikator loading terstruktur di halaman detail wizard dan dialog modal saat memuat atau mengirim mutasi tanpa tampilan kosong atau kedipan layout.
2. **State Kosong (*Empty*)**: Daftar tabel pada 7 submenu Penerimaan & Onboarding menangani kondisi `0` baris di DB dev secara bersih, dan filter checklist pada Tahap 5 menyediakan pemulihan cepat via `Reset Filter`.
3. **State Gagal & Konflik (*Error / Concurrency*)**:
   - Query `?stage=tidak-valid` menampilkan `InformationAlert` fallback yang jelas tanpa error 404/crash.
   - Error `409 Conflict` / `422` dari backend terbaca utuh di dalam dialog konfirmasi berkat perbaikan `LifecycleService.handleError`.
   - Selama uji live & injeksi Chromium, `http5xx = 0` dan `pageErrors = 0`.
4. **State Tanpa Hak Akses (`RBAC`)**: Akun non-HRD (`Novi`) saat membuka `/hr/lifecycle/onboarding-cases/[id]` diarahkan pada tampilan penolakan **Akses Terbatas** dengan `0` tombol mutasi wizard.
5. **State Kasus Dibatalkan (`Cancelled`)**: Menampilkan banner pembatalan dengan alasan/catatan dan menghapus seluruh tombol mutasi pada wizard.

---

## 5. Endpoint yang Dikonsumsi

| Tag Swagger Backend | Endpoint yang Dipanggil `LifecycleService` | Penggunaan di Antarmuka |
| --- | --- | --- |
| `Onboarding Cases` | `GET /v1/corporate/human-resource/onboarding-cases` & `GET .../{id}` | Daftar submenu Kasus Onboarding & muat detail kasus wizard. |
| `Onboarding Cases` | `GET .../coordinator-options`, `PUT .../{id}/coordinator` | Pilihan koordinator & aksi ganti koordinator kasus. |
| `Onboarding Cases` | `POST .../{id}/first-day-attendance`, `DELETE .../{id}/first-day-attendance` | Pencatatan & pembatalan kehadiran hari pertama di Tahap 1. |
| `Onboarding Cases` | `POST .../{id}/check-automatic-items` | Pemeriksaan ulang keabsahan kontrak `Signed+Scan` di Tahap 3. |
| `Onboarding Cases` | `POST .../{id}/items/{itemId}/complete` *(FormData)*, `.../uncomplete`, `GET .../evidence` | Penyelesaian item manual/berbukti PDF, pembatalan butir, & unduhan bukti di Tahap 5. |
| `Onboarding Cases` | `POST .../{id}/declare-ready`, `POST .../{id}/cancel-join` | Pengesahan *Siap Kerja* (Tahap 7) & pembatalan bergabung sebelum hari pertama. |
| `Onboarding Access Requests` | `GET /onboarding-access-requests?onboardingCaseId=...`, `POST .../{id}/retry`, `POST .../{id}/complete` | Aksi aktivasi/penyelesaian Tahap 6 & filter antrean pada Submenu 6. |
| `Clinical Preceptorship` | `GET /clinical-preceptorships/{id}` | Detail Submenu 5 beserta guard tampilan `Nyatakan Mandiri`. |

---

## 6. Verifikasi (Setelah 4 Perbaikan Akhir)

1. **`node check-css-usage.mjs` pada 10 JSX onboarding**: `OK — 10 berkas diperiksa, 0 temuan.`
2. **`npx eslint --max-warnings=0` pada 15 file FE terkait**: `0 errors, 0 warnings`.
3. **`npm run lint:errors`**: lulus tanpa error (`exit 0`).
4. **Unit Test Helper (`node --import ./tests/helpers/register.mjs --test`)**:
   - `tests/unit/onboarding-wizard-utils.test.mjs` (`6` skenario) + `tests/unit/recruitment-journey-utils.test.mjs` (`5` skenario) → **`11 tests, 2 suites, 11 pass, 0 fail`**.
5. **Build Produksi (`npm run build`)**:
   - `FINAL_FE_EXIT=0`, Next.js `16.2.12 (Turbopack)`, `935/935` rute ter-generate, `prepare-standalone.mjs` mencetak `"Standalone runtime siap dijalankan."`.
6. **Live Browser UAT Chromium Headed (`uat-onboarding-wizard.mjs`)**:
   - **`13/13 PASS | HTTP_5XX=0 | PAGE_ERRORS=0`** (`59` captured API calls) mencakup `SUBMENU-REG-07` (`LIVE_BE`), `WIZ-01-STAGE1` s.d. `WIZ-11-PRECEPTOR-GUARD` (`ROUTE_INJECTED`), dan `RBAC-NOVI-LIVE` (`LIVE_BE`).

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| Halaman `/hr/lifecycle/onboarding-cases/[id]` tampil 7 tahap terstruktur + sinkronisasi URL `?stage=` & `<Suspense>` | ✅ | `WIZ-01-STAGE1`, unit test `onboarding-wizard-utils.test.mjs`, `npm run build` `935/935` |
| Masking NIK & gaji non-ViewAmount, info 9 kelompok data, serta penegakan peran item Tahap 5 | ✅ | `WIZ-03-STAGE2-EMP`, `WIZ-04-STAGE3-CTR`, `WIZ-06-STAGE5-ORI` |
| Upload PDF FormData pada item bukti wajib & pesan spesifik error `409`/`422` pada `ConfirmModal` | ✅ | Perbaikan `LifecycleService.handleError` + `WIZ-06-STAGE5-ORI` + `WIZ-08-STAGE7-RDY` |
| Guard `Nyatakan Mandiri` Submenu 5 (`status === 2`) & round-trip `?onboardingCaseId=&returnTo=` Submenu 6 | ✅ | 4 edit pasca-`7dfae4cdb` + `WIZ-07-STAGE6-ACC` & `WIZ-11-PRECEPTOR-GUARD` |
| Kepatuhan CSS Token (`0` Tailwind, `0` hex/rgb, `0` `status="neutral"`), responsif 390px, & RBAC non-HRD | ✅ | `check-css-usage` `0/10`, `WIZ-10-MOBILE-390`, `RBAC-NOVI-LIVE` |
| Laporan tracked, frontend roadmap, & traceability diperbarui | ✅ | Dokumen ini, [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md), [requirement-traceability.md](../../../roadmap/requirement-traceability.md) |

---

## 8. Catatan Penutup

- Empat perbaikan frontend pasca-commit `7dfae4cdb` masih berada di *working tree* lokal `QuilvianSystemFrontendDev` (tidak di-commit otomatis sesuai aturan operasional).
- Berkas `test-with-agy/scripts/uat-onboarding-wizard.mjs` dan screenshot verifikasi berada di direktori kerja frontend sebagai artefak pengujian lokal.
