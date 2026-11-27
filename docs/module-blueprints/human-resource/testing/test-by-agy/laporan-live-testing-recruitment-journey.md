# Laporan Live Testing — Submenu Pelamar (Recruitment Journey End-to-End 6 Tahap)

| Field | Nilai |
| --- | --- |
| Menu | Sumber Daya Manusia → Siklus Kepegawaian → Rekrutmen & Seleksi → **Pelamar** (`/hr/recruitment/candidates/[id]`, deep-link ke `/hr/recruitment/evaluations/{hr|clinical}/[id]` & `/hr/recruitment/offers/[id]`) |
| Task | `FIN-HRD-REC-JRN-06` (memvalidasi `FIN-HRD-REC-JRN-01..05`) |
| Waktu uji (WIB) | 25 November 2026 (~22:27 – 22:36 WIB) |
| Lingkungan FE | Next.js dev server `http://localhost:3000` (build produksi terkonfirmasi `npm run build` `935/935` halaman sukses) |
| Lingkungan BE & DB | Build backend terbaru terisolasi `https://localhost:7194` (`QuilvianSystemBackend.dll`, kompilasi akhir `0 Error(s)`) + DB dev `QuilvianNewDevHamzah` (`7` kandidat terdaftar, `0` lamaran & `0` lowongan) |
| Peramban | Google Chrome mode headed visual (`slowMo: 60ms`), desktop `1366x860` & mobile `390x844` lewat CDP port `9344` |
| Akun uji (tanpa kredensial tertulis) | (1) `SuperAdmin` (`superadmin@admin.com`, izin penuh) dan (2) `Asesor Klinis` (`uat.pkb.asesor@rsmmc.local`, hanya `CandidateEvaluation:Read/Clinical` tanpa `Candidate:Read`) |
| Skrip & Bukti | `test-with-agy/scripts/jrn-1125-be-api-test.mjs` (`3/3 PASS`), `test-with-agy/scripts/jrn-1125-uat-browser.mjs` (`11/11 PASS`, `0` HTTP 5xx, `0` pageError), `test-with-agy/screenshots/jrn-1125/` |

---

## 1. Ringkasan Eksekutif & Batasan Jujur Lingkungan

Seluruh skenario `JRN-BE-01..03` dan `RJ-UAT-01..18` (terkelompok dalam 11 skenario otomasi browser) dieksekusi dengan transparansi penuh sumber bukti:
1. **`LIVE_BE` (Nyata terhadap DB dev `QuilvianNewDevHamzah` via server terisolasi `7194`)**:
   - Pembukaan Direktori Kandidat (`7` pelamar nyata) serta verifikasi sidebar sub-menu Rekrutmen tetap bersih tanpa tambahan menu baru.
   - Pembukaan pelamar nyata (`95f0f1c1-...` / `CAN-20261125-0008`, `0` lamaran): panel Recruitment Journey empty state tampil informatif, NIK pada Hero tersamar (`999721••••••0001`), dan **0 request tulis (`POST`/`PUT`/`DELETE`)** terjadi saat GET.
   - Kontrak API `GET /applications/{id}/journey` menolak GUID kosong (`400`), GUID tidak ditemukan (`404`), dan akun `Asesor` tanpa `Candidate:Read` (`403`) serta UI halaman detail memblokir `Asesor` dengan pesan izin aman.
2. **`ROUTE_INJECTED` (Injeksi terukur lewat Playwright `page.route` atas ketiadaan lamaran di DB dev)**:
   - Karena DB dev memiliki `0` lowongan & `0` lamaran nyata (`QuilvianNewDevHamzah`), payload 6 tahap perjalanan (`APP-20261125-0101` s.d. `0106`, `APP-FT-20261125-0001`), pencegatan `POST` aksi inline (`/screenings/evaluate`, `/handoffs/generate`), serta simulasi gangguan server `500` diinjeksikan lewat `page.route` tanpa menuliskan satu baris pun ke database.
3. **`SQLITE_XUNIT` (Verifikasi aturan bisnis BE ujung-ke-ujung pada 5 skenario workflow)**:
   - Dilengkapi oleh `5/5` xUnit `RecruitmentJourneyWorkflowTests` pada DB SQLite in-memory untuk memastikan guard sequence, veto terisolasi per lamaran, `FitWithRestrictions` blocking, `Unfit` revocation, kursi headcount, rehire dedup, dan pemulihan kasus onboarding benar-benar ditegakkan di layanan C#.

---

## 2. Matriks Hasil Pengujian Browser (`11/11 PASS`)

| ID Uji | Prio | Klasifikasi | Skenario | Hasil Aktual Teramati | Tangkapan Layar |
| --- | --- | --- | --- | --- | --- |
| `RJ-UAT-01` | P0 | `LIVE_BE` | Direktori Kandidat & keutuhan sidebar Rekrutmen | Halaman termuat (`7` tombol Rincian, `200 OK`); 0 submenu baru pada sidebar Rekrutmen | `01-directory-and-sidebar.png` |
| `RJ-UAT-02` | P0 | `LIVE_BE` | Pelamar nyata `0` lamaran: empty state + masking NIK + 0 POST | Empty state "Belum Ada Perjalanan Lamaran" tampil, NIK `999721••••••0001`, 0 tulis DB | `02-live-candidate-empty-masked-nik.png` |
| `RJ-UAT-03` | P1 | `ROUTE_INJECTED` | Parameter `?applicationId=` tidak valid | Alert fallback tampil dan lamaran aktif tetap dimuat tanpa crash | `03-invalid-query-fallback.png` |
| `RJ-UAT-04-06` | P0 | `ROUTE_INJECTED` | 6 tahap interaktif, URL sync (`?stage=`), switcher multi-lamaran, & race protection | 6 card stepper berganti ke `?stage=registration`, switcher cepat (`0101` delay 450ms → `0102`) mempertahankan `APP-20261125-0102` | `04-multi-app-6stages-url-sync.png` |
| `RJ-UAT-07-08` | P0 | `ROUTE_INJECTED` | Deep-link Evaluasi + Veto Klinis memblokir Offer | Alert *Veto Mutlak Komite Klinis*, tombol *Buat Penawaran* tidak muncul, dan halaman evaluasi menampilkan *Kembali ke Journey* | `05-clinical-veto-blocked.png` |
| `RJ-UAT-09` | P1 | `ROUTE_INJECTED` | Aksi inline *Nilai Seleksi Berkas* (`ConfirmModal`) tanpa mutasi DB | `POST /screenings/evaluate` mengirim `applicationId` + `notes` valid (dicegat, 0 DB write) | `06-inline-evaluate-screening-modal.png` |
| `RJ-UAT-10-12` | P0 | `ROUTE_INJECTED` | MCU `FitWithRestrictions` (`BLOCKED`), MCU `Unfit` (offer `Revoked`), & deep link Offer | Handoff tertahan saat FWR, Offer tercabut saat Unfit, detail offer memiliki *Kembali ke Journey* | `07-mcu-fit-with-restrictions.png` |
| `RJ-UAT-13-14` | P0 | `ROUTE_INJECTED` | Rehire dipakai ulang + pemulihan partial handoff & Fast-Track Talent Pool | Badge *Profil Pegawai Lama Dipakai Ulang*, klik *Pulihkan Kasus Onboarding* mengirim `POST /handoffs/generate` -> muncul *Buka Kasus Onboarding*, & Fast-Track tampil melewati Tahap 2–3 | `08-rehire-recovered-handoff.png` |
| `RJ-UAT-15-17` | P1 | `ROUTE_INJECTED` | Pemulihan error 500 (*Coba Lagi*) & RBAC masking (gaji, catatan medis, `RESTRICTED`) | Alert ramah tanpa teks Axios mentah pulih pasca klik *Coba Lagi*; gaji, dokter MCU, dan Tahap 6 tersamar `Dirahasiakan` / `Akses Terbatas` | `09-rbac-masked-mcu-stage.png` |
| `RJ-UAT-18` | P0 | `ROUTE_INJECTED` | Responsif ponsel `390x844 px` | `docScrollWidth 375px <= innerWidth 390px` (`0` overflow horizontal), stepper tersusun vertikal rapi | `10-mobile-390px-journey.png` |
| `RJ-UAT-16` | P0 | `LIVE_BE` | Akun Asesor tanpa `Candidate:Read` mengakses detail pelamar | Ditahan pesan `"Anda belum punya hak untuk melihat kandidat."`, 0 bocoran data & 0 HTTP 5xx | `11-asesor-forbidden-live.png` |

---

## 3. Perbaikan Selama Siklus Uji (`Inspect -> Polish -> Retest`)

1. **Pembersihan duplikasi blocker tahap & perataan form modal (`FIN-UI-POLISH`)**:
   - Pada tangkapan layar awal Stage 6 (`NeedsOnboardingRecovery`), alert blocker tampil dua kali berdekatan (header + dalam detail tahap), serta label form di dalam `ConfirmModal` terpusat (`text-align: center`). Dilakukan deduplikasi blocker tahap terhadap blocker global pada `candidate-journey-panel.jsx` dan penambahan `.modalLeftAlign` di `candidate-journey.module.css`, diverifikasi ulang lewat `check-css-usage.mjs` + `eslint --max-warnings=0` + `npm run build` + screenshot UAT terbaru.
2. **Penyelarasan skrip uji (`TIMEOUT & FIXTURE`)**:
   - Navigasi pertama kali Next.js dev route `/evaluations/hr/[id]` dan `/offers/[id]` melebihi 15 detik (`setDefaultTimeout`), ditingkatkan `defaultNavigationTimeout` menjadi `90s`; pembungkus `data-testid="journey-invalid-query-alert"` ditambahkan serta fixture kronologi disinkronkan via `syncStage0Identity`. Uji ulang penuh menghasilkan **`11/11 PASS` (`0` FAIL)**.
