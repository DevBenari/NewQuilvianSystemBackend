# Laporan Live Testing — Finishing Menu 03 Rekrutmen & Seleksi

| Field | Nilai |
| --- | --- |
| Menu & Rute Uji | Sumber Daya Manusia → Siklus Kepegawaian → **Rekrutmen & Seleksi** (`/hr/recruitment/{candidates,candidates/[id],candidates/[id]/edit,candidates/[id]/apply,candidates/[id]/documents/create,screenings,screenings/[id],evaluations,evaluations/hr/[id],evaluations/rubrics,requisitions/create,vacancies/create,offers/create,mcu-clearance/create,mcu-clearance/[id],talent-pool/create,handoffs/create}`) |
| Task Terkait | `BE-HRD-FIN-REC-001..020` & `FE-HRD-FIN-REC-001..027` (`AC-01` s.d. `AC-12`) |
| Waktu Uji (WIB) | 26 November 2026 (~12:36 – 12:42 WIB) |
| Lingkungan FE | Next.js dev server `http://localhost:3000` (konsisten dengan produksi `npm run build` `942/942` halaman sukses) |
| Lingkungan BE & DB | Binary backend terbaru terisolasi `https://localhost:7196` (`be-build-out\QuilvianSystemBackend.exe`, kompilasi `0 Error(s)`, `QBE Strict 0 VIOLATION`) terhubung ke DB dev `QuilvianNewDevHamzah` |
| Peramban | Google Chrome mode headed visual (`slowMo: 40ms`), desktop `1366x860` & mobile `390x844` via Playwright/CDP (`fin-rec-1126-uat-browser.mjs`) |
| Akun Uji | `SuperAdmin` (`superadmin@admin.com`) & `Asesor Klinis` (`uat.pkb.asesor@rsmmc.local`) — tanpa kredensial tertulis |
| Bukti Otomasi & Screenshot | [`uat-results.json`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/test-with-agy/screenshots/fin-rec-1126/uat-results.json), folder `QuilvianSystemFrontendDev/test-with-agy/screenshots/fin-rec-1126/` |

---

## 1. Sumber Bukti & Pembatasan Jujur (`LIVE_BE` vs `ROUTE_INJECTED` vs `SQLITE_XUNIT`)

1. **`LIVE_BE` (8 skenario: `UAT-REC-01, 02, 03, 04, 06, 08, 11, 12`)**:
   - Seluruh pembacaan direktori pelamar (`9` pelamar nyata di `QuilvianNewDevHamzah`), Candidate Workspace `CAN-20261126-0002` (`APP-20261126-0002`, `Perawat IGD`, `GET /applications/842c0d02-298f-407d-a369-ec7ef274a6d5/journey` HTTP `200 OK` 6 tahap), 3 sub-halaman kandidat (`/edit`, `/apply`, `/documents/create`), worklist screenings & evaluations (`GET /evaluations/eligible-applications` HTTP `200 OK`), halaman Master Rubrik, 6 halaman dedicated `/create`, viewport mobile `390x844 px`, serta penolakan RBAC negatif untuk akun `Asesor Klinis` dieksekusi langsung terhadap backend `https://localhost:7196` & DB dev nyata dengan **0 mutasi tulis DB**.
2. **`ROUTE_INJECTED` (4 skenario: `UAT-REC-05, 07, 09, 10`)**:
   - Pencegatan `POST` (`/screenings/evaluate` dan `/evaluations/{id}/submit-hr`) agar tidak memodifikasi PostgreSQL dev, simulasi screening dengan dokumen Ijazah belum lengkap -> lengkap, simulasi detail MCU dengan `isMedicalDetailsMasked: true`, serta injeksi galat HTTP `500` untuk verifikasi pemulihan *Coba Lagi* dilakukan secara transparan melalui Playwright `page.route`.
3. **`SQLITE_XUNIT` (`8/8` Tes Integrasi Workflow Backend)**:
   - Melengkapi ketiadaan mutasi DB langsung untuk guard transaksi atomik apply + auto-screening, veto terisolasi per lamaran, validasi `FastTrackPromoteAsync`, serta recovery Handoff onboarding melalui `RecruitmentJourneyWorkflowTests` di SQLite in-memory.

---

## 2. Matriks Hasil Pengujian Browser (`12/12 PASS`, `0` HTTP 5xx, `0` PageError)

| ID Uji | Prio | Klasifikasi | Skenario Uji | Hasil Aktual Teramati | Tangkapan Layar |
| --- | --- | --- | --- | --- | --- |
| `UAT-REC-01` | P0 | `LIVE_BE` | Direktori Kandidat pisahkan *Status Pelamar* vs *Lamaran & Tahap Aktif*, masking NIK, tombol `Workspace`, keutuhan sidebar | NIK tersamar (`9999********0002`), sidebar Rekrutmen utuh, navigasi Workspace `candidateId=bcc67c56-0af5-4adf-99b6-f508273d489a` | `01-candidates-directory.png` |
| `UAT-REC-02` | P0 | `LIVE_BE` | Candidate Workspace (Gambar 1): 6-step stepper, 5 tab ARIA (`?tab=`), invalid `applicationId` fallback, & `Panduan Berikutnya` | Stepper 6 tahap, `?tab=documents` ternormalisasi ke tab `Dokumen`, alert fallback `applicationId=invalid-id-999` tampil aman | `02-candidate-workspace-ringkasan.png`, `03-candidate-workspace-dokumen.png` |
| `UAT-REC-03` | P1 | `LIVE_BE` | Halaman dedicated kandidat (`/edit`, `/apply`, `/documents/create`) terbuka penuh tanpa modal | Ketiga halaman tampil penuh; input `nikMasked` terkunci (`disabled`) di `/edit` | `04-candidate-upload-doc-page.png` |
| `UAT-REC-04` | P0 | `LIVE_BE` | Worklist Seleksi Administrasi default `Pending` + `SummaryGrid` | Worklist & ringkasan termuat tanpa error (`200 OK`) | `05-screenings-worklist.png` |
| `UAT-REC-05` | P0 | `ROUTE_INJECTED` | Pemeriksaan Seleksi Administrasi (Gambar 2): Checklist Dokumen, gate dokumen wajib, `Simpan` (`decision=1`) & `Lolos` (`decision=2`) | Ratio `2/4` -> `4/4`; tombol `Lolos Administrasi` terkunci saat `Ijazah` belum ada & aktif saat lengkap; payload `decision: 1` dan `decision: 2` ter-intercept valid | `06-screening-detail-gambar2.png` |
| `UAT-REC-06` | P0 | `LIVE_BE` | Worklist Evaluasi (`Antrean Siap Dinilai`) & Master Rubrik (`/evaluations/rubrics`) | Antrean live & gerbang kebijakan `BLOCKED_SCHEMA_AND_POLICY_GATE` tampil bersih | `07-evaluations-worklist.png`, `08-evaluations-rubrics-gate.png` |
| `UAT-REC-07` | P0 | `ROUTE_INJECTED` | Wawancara HR (Gambar 3): Indikator `1–5` → Ring skor `87`, Simpan Draft lokal & Finalisasi | Rating `[4,5,4,4,4,5]` menghitung tepat `87`, draft tersimpan lokal, finalisasi mengirim `/submit-hr` (`score=87`, `recommendation=1`) | `09-evaluation-hr-gambar3-87.png` |
| `UAT-REC-08` | P0 | `LIVE_BE` | Seluruh 6 rute halaman dedicated pembuatan Menu 03 (`/requisitions/create`, `/vacancies/create`, `/offers/create`, `/mcu-clearance/create`, `/talent-pool/create`, `/handoffs/create`) | Keenam halaman dedicated render formulir penuh dengan tombol `submit` tanpa `ConfirmModal` | `10-dedicated-create-handoff.png` |
| `UAT-REC-09` | P0 | `ROUTE_INJECTED` | Kerahasiaan Medis MCU (`/mcu-clearance/[id]`): masking catatan dokter & sertifikat bagi non-pemeriksa medis | Alert privasi medis & nilai `Dirahasiakan` tampil saat `isMedicalDetailsMasked=true` | `11-mcu-masked-privacy.png` |
| `UAT-REC-10` | P0 | `ROUTE_INJECTED` | Pemulihan galat API `500` menampilkan alert ramah & tombol `Coba Lagi` | Bebas pesan teks mentah `Request failed with status code`; klik `Coba Lagi` memulihkan Workspace penuh | — |
| `UAT-REC-11` | P0 | `LIVE_BE` | Responsif ponsel `390 x 844 px` tanpa horizontal overflow | `documentElement.scrollWidth = 375 <= innerWidth = 390` (`0px` overflow horizontal) | `12-mobile-390px-workspace.png` |
| `UAT-REC-12` | P0 | `LIVE_BE` | RBAC Negatif akun `Asesor Klinis` mengakses halaman kandidat tanpa izin `Candidate:Read` | Alert ramah hak akses tampil, `0` kebocoran data kandidat & `0` HTTP `5xx` | `13-asesor-rbac-gate.png` |

---

## 3. Siklus Temuan Bug, Perbaikan & Uji Ulang (`Inspect -> Fix -> Retest`)

Pengujian berulang peramban langsung menemukan serta memverifikasi perbaikan masalah berikut sebelum laporan akhir ditandatangani:

1. **Bug Kritis P0 `UAT-REC-05` (`decision: undefined` pada `screening-detail-view.jsx`)**:
   - **Gejala**: Pengujian intercept payload `POST /screenings/evaluate` menemukan `body.decision` bernilai `undefined` (hilang dari JSON) meskipun toast UI tampak sukses.
   - **Akar Masalah**: `SCREENING_DECISION_VALUES` pada `recruitment-constants.js` menggunakan kunci PascalCase (`{ Pending: 1, Passed: 2, Rejected: 3 }`), sedangkan `screening-detail-view.jsx` membaca `.PENDING`, `.REJECTED`, dan `.PASSED`.
   - **Perbaikan & Retest**: Mengoreksi keempat referensi enum ke `.Pending`/`.Passed`/`.Rejected` di `screening-detail-view.jsx`, menambah unit test penjamin kunci PascalCase di `tests/unit/recruitment-workspace-utils.test.mjs` (`17/17 PASS`), dan menguji ulang pada Chrome (`UAT-REC-05 PASS`: kedua payload `decision=1` dan `decision=2` terbukti terkirim).
2. **Kerapihan Tabel Checklist/Rubrik & Dropdown Rekomendasi (`Gambar 2` & `Gambar 3`)**:
   - Mengosongkan properti visual `label: ""` disertai `aria-label` pada `BaseNativeSelectField` di dalam baris tabel `screening-detail-view.jsx` dan `CandidateEvaluationWorkspace.jsx` agar label tidak bertumpuk vertikal di dalam kolom, serta memakai `BaseNativeSelectField` untuk daftar pendek statis `Rekomendasi` di panel sisi kanan evaluasi agar bebas terpotong kontainer.
3. **Penyesuaian Teknis Skrip Uji (`fin-rec-1126-uat-browser.mjs`)**:
   - Memperpanjang `defaultNavigationTimeout` menjadi `180s` untuk memberi ruang kompilasi awal rute App Router baru oleh `next dev`, serta mengganti pembacaan teks `<svg><text>` dari `.innerText()` (`SVGElement` error) menjadi `.textContent()`. Seluruh **12 skenario** pada eksekusi ulang akhir lulus (`12/12 PASS`, `0 FAIL`, `0 http5xx`, `0 pageErrors`).
