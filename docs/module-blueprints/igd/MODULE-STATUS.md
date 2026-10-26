# IGD — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Module name | `IGD` / `EmergencyInstallationManagement` |
| Revision | **`15`** — naik 18 Oktober 2026 malam lewat `IGD-DEC-325`, `326` (manifest 0t; API `0.20.0` §13, validation `0.20.0` §15, integration `0.11.0` §10); blueprint secara keseluruhan tetap `draft`. *Sebelumnya* **`14`** — naik 18 Oktober 2026 lewat persetujuan delta matriks state `IGD-DEC-324` (manifest 0s; state `0.15.0`, validation `0.19.0`); blueprint secara keseluruhan tetap `draft`. *Sebelumnya* **`13`** — bagian terbarunya (manifest 0r) `approved` lewat `IGD-DEC-318`, 17 Oktober 2026; blueprint secara keseluruhan tetap `draft`. Revisi `11` (12 Oktober, Form Permintaan Rawat Inap, `IGD-DEC-294`), `12` (14 Oktober, amandemen 0q, `IGD-DEC-306`), dan `13` (15 Oktober, amandemen 0r) tertinggal di berkas ini sampai 17 Oktober 2026. *Sebelumnya* **`10`** — `draft` (naik 11 Oktober 2026 lewat desain dokter jaga IGD, `EPIC IGD-12`, manifest bagian 0o; bagian barunya `approved` lewat `IGD-DEC-240`). *Sebelumnya* **`9`** — `draft` (naik 6 Oktober 2026 lewat desain Ruang Kerja Dokter IGD, `EPIC IGD-14`, manifest bagian 0n; bagian barunya `approved` lewat `IGD-DEC-230`). *Sebelumnya* **`8`** — `draft` (naik 23 September 2026 lewat amendment penutupan kunjungan lewat disposisi, `IGD-DEC-163`…`170`; amandemen observasi 30 September 2026 dan koreksi 1 Oktober 2026 tidak menaikkan revisi — manifest bagian 0i, 0j). *Tertinggal di berkas ini sampai 3 Oktober 2026.* *Sebelumnya* **`7`** — `draft` (naik 22 September 2026 lewat `IGD-DEC-157`: bagian encounter-first kelima kontrak `approved`). Irisan kontrak yang dibutuhkan `MVP-0`…`MVP-6` sudah `approved` lewat `IGD-DEC-093` dan `IGD-DEC-108`; blueprint secara keseluruhan belum disetujui. *Sebelumnya `6`* |
| Module status | `PARTIAL` — **21 Oktober 2026 (malam):** `FE-IGD-070` 🟡 (SEBAGIAN — pintu Pasien Tanpa Identitas di Pendaftaran IGD dan pencabutan blok di Assesmen Awal; 8 berkas selesai, ESLint 0 error 7 warning baseline, nol temuan baru; menunggu uji visual layar port 3000 oleh pemilik). Register **101 ✅ dari 121 aktif (83,5%)** sesudah penambahan satu task aktif frontend baru (`FE-IGD-070`); backend 57 ✅ dari 64 aktif (89,1%), frontend 44 ✅ dari 57 aktif (77,2%). Atas penyebut penuh: 101 dari 125 (80,8%). *Sebelumnya:* **19 Oktober 2026 (siang, PC kantor):** `BE-IGD-084` ✅ (K1-09 dan K1-11 100% PASS, live browser testing kasir & API runtime terbukti; gelombang R3.19 tuntas 100%); `BE-IGD-085` ✅ (build penuh 0 error, 285 warning; `IGD-FACT-157`); `FE-IGD-067` ✅ (P1-06R lulus 100%, pembatalan reservasi beralasan dari layar HTTP 200). Register **91 ✅ dari 121 aktif (75,2%)** sesudah pass 19 Oktober 2026 (siang): pemilik menyatakan layar `npm run dev` sah sebagai bukti selama modul masih dalam pengembangan, sehingga `FE-IGD-050`, `FE-IGD-051`, dan `FE-IGD-056` dipulihkan ke ✅ atas penilaian pemilik dan dasar `IGD-DEC-296` tidak lagi berlaku; penyebut dikoreksi dengan mengeluarkan tiga task yang dibatalkan `IGD-DEC-304` (`BE-IGD-073`, `BE-IGD-075`, `FE-IGD-062`), barisnya tetap ada dan tidak dihapus. Atas penyebut penuh: 91 dari 124 (73,4%). **18 Oktober 2026 (malam, penilaian P1/K1):** `BE-IGD-080` ✅; `BE-IGD-085` 🟡 (tinggal jumlah warning build); `BE-IGD-084` 🟡; `FE-IGD-067` 🟡 (cacat modal pembatalan diperbaiki, uji ulang P1-06R belum). Hasil disahkan sebatas bukti log (`IGD-DEC-328`); penguji melanggar aturan (`IGD-FACT-148`). Register 85 ✅ dari 124. **18 Oktober 2026 (malam, audit pasien prioritas dan billing IGD):** `BE-IGD-080` dan `FE-IGD-067` ✅ → 🟡 — _check-in_ reservasi melahirkan kunjungan tanpa encounter dan keduanya tanpa bukti runtime (`IGD-FACT-137`, `138`); jasa kunjungan IGD belum ada (`IGD-FACT-139`); tagihan IGD belum terbukti sampai kasir (`IGD-FACT-140`). R3.19 direncanakan: `BE-IGD-085` dan `BE-IGD-084` siap. Register 84 ✅ dari 124 — [audit](evidence/2026-10-18-audit-pasien-prioritas-dan-billing-igd.md). **18 Oktober 2026 (sore, uji `Down()`):** `BE-IGD-072` ✅ (`IGD-FACT-136`); register 86 ✅ dari 122. Sisa `MVP-11` di lingkup IGD tinggal `BE-IGD-077`, yang menunggu Rawat Inap (`IGD-REQ-003`). **18 Oktober 2026 (sore, Putaran B7 bagian 1):** `BE-IGD-083` ✅ (`IGD-FACT-133`); register 85 ✅ dari 122. Sisa `MVP-11`: `BE-IGD-072` uji `Down()`; `BE-IGD-077` menunggu Rawat Inap (`IGD-REQ-003`). **18 Oktober 2026 (sore, `build-module-backend`):** `BE-IGD-083` 🟡 — kode selesai, QBE Strict `PASS`, build Rizki 0 error/286 warning (baseline); menunggu Putaran B7. **18 Oktober 2026 (siang, sesudah persetujuan):** delta disetujui (`IGD-DEC-324`); `BE-IGD-083` siap untuk `build-module-backend`. **18 Oktober 2026 (siang, sesudah Putaran B6):** register 84 ✅ dari 122 (84 dari 121 sebelum `BE-IGD-083` masuk register); `BE-IGD-074` dan `FE-IGD-061` ✅. B6-06 menemukan cacat IGD: kunjungan pasien yang di-_admit_ dari Antrean Transfer tidak pernah tertutup (`IGD-FACT-128`, `130`). Perbaikannya `BE-IGD-083` ⛔ menunggu persetujuan delta matriks state (`IGD-OQ-133`); perbaikan wajib sejalur dengan blueprint Rawat Inap (`IGD-DEC-323`); permintaan ke Rawat Inap `IGD-REQ-003`. **17 Oktober 2026 (malam, sesudah Putaran B5):** register 82 ✅ dari 121; `FE-IGD-060` ✅. Sisa `MVP-11` di lingkup IGD hanya menunggu satu _admit_ sungguhan, yang tertahan konfigurasi akses bed Rawat Inap (`IGD-FACT-127`). **17 Oktober 2026:** register 81 ✅ dari 121 task. `EPIC IGD-15` (transfer ke rawat inap, `MVP-11`) belum selesai — lima task 🟡; tombol dan endpoint _Tolak_ serta pemanggilan penutupan susulan di Rawat Inap belum ada di repo (`IGD-FACT-123`, `IGD-OQ-132`). **11 Oktober 2026:** `EPIC IGD-12` (dokter jaga, `MVP-10`) dibuka — `IGD-OQ-102`/`103` dijawab (`IGD-DEC-237`…`239`), desain `approved` (`IGD-DEC-240`); `BE-IGD-056` dan `FE-IGD-037` ⛔ → 🟡. **6 Oktober 2026:** `MVP-6` dan `MVP-8` sudah ✅; pekerjaan berikutnya Ruang Kerja Dokter IGD (`EPIC IGD-14`, desain `approved` `IGD-DEC-230`). *Sebelumnya:* pekerjaan berarti masih dapat berjalan (lihat *Next recommended task*), sementara `MVP-6` terblokir. **Dikoreksi 16 September 2026 (ketiga):** kebuntuan kunjungan `Arrived` sudah dibuka. `FE-IGD-030` ✅ terbukti lewat layar — pasien dapat dipindahkan ke penanganan, dialihkan ke Assesmen IGD, dan triage susulannya tidak memundurkan status. `FE-IGD-029` 🟡 sudah terpasang tetapi **belum dibuktikan dengan pendaftaran baru**, jadi hilangnya penolakan `409` pada jalur normal belum diuji ([evidence](evidence/2026-09-16-kunjungan-terjebak-arrived.md)) |
| Current phase | **18 Oktober 2026 (malam) — R3.19 untuk demo 20–21 Oktober:** `build-module-backend` `BE-IGD-085` (pasien prioritas, 20 Oktober) lalu `BE-IGD-084` (jasa kunjungan, 21 Oktober); uji P1 dan K1 (panduan uji sisa bagian 3F, 3E). `MVP-11` tidak berubah. *Sebelumnya:* **18 Oktober 2026 (sore) — `verify-module-readiness` `MVP-11`: `READY_WITH_CONDITIONS` (±84%) — jalur IGD siap UAT untuk alur normal; `DONE` menunggu panggilan dan _Tolak_ Rawat Inap (C1, C2) — [kesiapan MVP-11](evidence/2026-10-18-kesiapan-mvp-11.md).** *Sebelumnya:* **18 Oktober 2026 (sore) — uji `Down()` `BE-IGD-072` lulus di kontainer sekali pakai ([bukti](evidence/2026-10-18-uji-down-migration-be-igd-072.md)); `MVP-11` 9 dari 10 task ✅; sisa satu task menunggu Rawat Inap.** *Sebelumnya:* **18 Oktober 2026 (sore) — Putaran B7 bagian 1 dinilai: B7-00…05 lulus, `BE-IGD-083` ✅, `AT-IGD-237` terbukti; templat opsi 1 panduan §3C.1 diperbaiki (`IGD-FACT-134`).** *Sebelumnya:* **18 Oktober 2026 (sore) — `BE-IGD-083` dikerjakan (5 berkas, +147/−39, QBE Strict `PASS`); menunggu build Rizki, lalu Putaran B7 bagian 1 ([laporan](task/report/backend/BE-IGD-083.md)).** *Sebelumnya:* **18 Oktober 2026 (siang, sesudah persetujuan) — `IGD-DEC-324` menjawab `IGD-OQ-133`; kontrak state `0.15.0` §11.8 dan validation `0.19.0` §14.10; `BE-IGD-083` siap untuk `build-module-backend` (gelombang 4 R3.18).** *Sebelumnya:* **18 Oktober 2026 (siang) — Putaran B6 dinilai (`IGD-FACT-128`); kartu `BE-IGD-083` disusun (`plan-module-delivery`) dan ⛔ menunggu persetujuan delta tiga baris matriks state (`IGD-OQ-133`); Putaran B7 disiapkan (panduan uji sisa §3D).** *Sebelumnya:* **17 Oktober 2026 (malam) — Putaran B5 dinilai (`IGD-FACT-126`); Putaran B6 (satu _admit_ dan penutupan alur normal) siap dijalankan sesudah akses bed petugas admisi dibuka (panduan 3C.1).** *Sebelumnya:* **17 Oktober 2026 — penutupan `MVP-11` menunggu uji _admit_ sungguhan (C-12/B3-14), uji penutupan dua arah (B3-07…10), dan uji `Down()` `BE-IGD-072`; amandemen 0r `approved` (`IGD-DEC-318`); manifest 0r.1.** *Sebelumnya:* **11 Oktober 2026 — desain dan perencanaan `EPIC IGD-12` (manifest 0o, 0o.1): kartu `BE-IGD-056` dan `FE-IGD-037` disusun ulang di atas kode pemilik 8 Oktober; menunggu izin `build-module-*`.** *Sebelumnya:* **6 Oktober 2026 (sore, perencanaan) — kartu `MVP-9` terbit (manifest 0n.1): backend R3.16 `BE-IGD-065`…`071` (1 ⛔ `IGD-OQ-117`), frontend R3.14 `FE-IGD-045`…`058` (3 ⛔); `IGD-DEC-231`.** *Sebelumnya:* **6 Oktober 2026 (sore, desain) — slice pesanan tanpa sikap ✅ dan di-commit pemilik (backend `43dab6da`, frontend `6680278a2`; `BE-IGD-041`, `BE-IGD-064`, `FE-IGD-043`, `FE-IGD-044` ✅ — `IGD-DEC-216`…`219`); `grill-me` Ruang Kerja Dokter IGD (`IGD-DEC-220`…`229`), impact scan suplemen 3.4, desain `EPIC IGD-14` `approved` (`IGD-DEC-230`, manifest 0n).** *Sebelumnya:* **5 Oktober 2026 (sore, sinkronisasi roadmap) — amandemen `IGD-DEC-205` `approved` (`IGD-DEC-209`; validation `0.13.0`, state `0.9.0`; manifest 0m); `BE-IGD-041` dibuka ⛔ → 🟡 (7 dari 17 acceptance pada source), `FE-IGD-044` ⛔ → tanpa tanda (gelombang 2); `FE-IGD-043` dan `BE-IGD-064` siap; keempatnya menunggu izin `build-module-*` per task (`IGD-DEC-208`); manifest 0m.1.** *Sebelumnya:* **5 Oktober 2026 (sore, perencanaan) — `FE-IGD-043` dan `BE-IGD-064` siap; `BE-IGD-041` diperluas dan `FE-IGD-044` baru, keduanya ⛔ menunggu amandemen validation §5.1 dan §6 aturan 4 (`IGD-DEC-205`); `IGD-OQ-116` dijawab; `IGD-DEC-204`…`208`, manifest 0l.3.** *Sebelumnya:* **5 Oktober 2026 (sore) — `MVP-8` 6 dari 6 task ✅** (`BE-IGD-039` ✅ `IGD-DEC-201`, `BE-IGD-061` ✅); syarat C2 lunas, C3 sebagian; perencanaan kartu frontend pasangan dan perluasan `BE-IGD-041` menunggu. *Sebelumnya:* **5 Oktober 2026 — `MVP-8` tetap 5 dari 6 task ✅; syarat C4 lunas, C3 dan C2 diputuskan, kontrak kewenangan unit `approved` (`IGD-DEC-199`, manifest 0l); `BE-IGD-039` menunggu `plan-module-delivery`.** *Sebelumnya:* **4 Oktober 2026 (sore) — `MVP-8`: 5 dari 6 task ✅.** `BE-IGD-061` dibuild Rizki (0 error, 0 warning) dan diuji API S13–S19 — terbukti, tetap 🟡 karena S2, S3, S12 tertahan `BE-IGD-039`. `FE-IGD-042` dikerjakan ulang (`IGD-DEC-187`) dan, bersama `FE-IGD-041`, ✅ lewat uji gabungan pada hasil build. Penyimpangan agen penguji (Akses Role diubah lewat SuperAdmin) disahkan pemilik `IGD-DEC-188`; `IGD-OQ-113` dibuka. *Sebelumnya:* **3 Oktober 2026 (petang) — `MVP-8` dilanjutkan.** `FE-IGD-041` dikerjakan ulang: penanda *Menunggu penutupan* dipindah ke bawah lencana STATUS KUNJUNGAN dan kolom PENUTUPAN dihapus; `npm run build` agent lulus 17.03; tetap 🟡 sampai uji layar U2, U6, U7 pada hasil build. Temuan `BE-IGD-061` S6 diputuskan lewat amendment pass `grill-me`: **`IGD-DEC-183`** (observasi Dieskalasi menahan penutupan kunjungan), **`IGD-DEC-184`** (observasi Dieskalasi yang sudah tertinggal dibiarkan; pesan diperjelas), **`IGD-DEC-185`** (dikerjakan sebagai pengerjaan ulang `BE-IGD-061`). Sesudahnya, pada sesi yang sama: kontrak diamandemen dan disetujui **`IGD-DEC-186`** (API `0.14.0`, validation `0.11.0`, state `0.8.0`; manifest bagian 0k), kartu diperluas (manifest 0k.1), dan **`BE-IGD-061` dikerjakan ulang di source** — 3 berkas, QBE Strict `PASS`, menunggu `dotnet build` dan uji API S13–S19. **`FE-IGD-042` dibuka ulang** (`IGD-DEC-187`: tombol *Selesaikan*/*Eskalasi* nonaktif pada kunjungan berakhir) — turun dari ✅ ke 🟡. *Sebelumnya:* **3 Oktober 2026 (akhir) — `MVP-7` 13 dari 13 ✅**, terbukti dengan akun peran nyata; loket IGD diperbaiki ke rute petugas `/admin` (`IGD-DEC-182`) dan lulus uji ulang. Audit kesiapan `MVP-7`: `READY_WITH_CONDITIONS`, sisa C1 (UAT), C3 (rilis per lingkungan), C7 (push). *Riwayat:* **`MVP-7` 12 dari 13**: uji izin peran nyata menemukan loket IGD ditolak `403` untuk petugas sungguhan; `FE-IGD-036` dibuka ulang dan rutenya diperbaiki (`IGD-DEC-182`), build lulus 15.31, menunggu uji ulang `C4-01`. Audit kesiapan `MVP-7`: `READY_WITH_CONDITIONS` (sisa C1, C3, C4, C7). *Sore:* **seluruh task `MVP-7` ✅, 13 dari 13** (uji SuperAdmin). *Pagi:* **penutupan `MVP-7` (tahap A review status).** Backend R3.13: 8 dari 8 task ✅ (`BE-IGD-051`…`055`, `057`…`059`). Frontend R3.12: `FE-IGD-035`, `038`, `039`, `040` ✅; **`FE-IGD-036` 🟡 — tinggal commit, build, dan satu putaran uji pada hasil build** (lunas sore hari); `FE-IGD-037` ⛔ (`EPIC IGD-12`). `MVP-8` (`EPIC IGD-13`, R3.14/R3.13.1) berjalan: `BE-IGD-060`, `062`, `063` ✅, `BE-IGD-061` 🟡, `FE-IGD-041` 🟡, `FE-IGD-042` ✅. *Sebelumnya* **22 September 2026 (penutup): gelombang R3.13 (backend, `BE-IGD-051`…`059`) / R3.12 (frontend, `FE-IGD-035`…`040`) direncanakan FINAL** — `EPIC IGD-11` (`MVP-7`) di atas kontrak `approved` (`IGD-DEC-157`); `BE-IGD-051` siap menunggu go-ahead; `EPIC IGD-12` (dokter jaga) ⛔ `IGD-OQ-102`/`103`; nol task dimulai. *Sebelumnya pagi:* gelombang direncanakan dari `IGD-DEC-139`…`141`. Gelombang `MVP-3`, `MVP-4`, dan `MVP-5` sedang dituntaskan. Modul IGD memakai penomoran gelombang `MVP-0`…`MVP-6`, bukan ID `IGD-PH-*` |
| Last verified at | `19 Oktober 2026 (siang, uji K1-09 dan K1-11 BE-IGD-084)` — oleh agent: live browser testing Playwright headed Chromium di localhost:3000 (layar login, cashier overview, detail-billing tab details, menu pembayaran memuat Jasa Kunjungan IGD Rp150.000) dan verifikasi API runtime PATCH patient-procedures cancel/remove-draft ditolak HTTP 400 dengan pesan baku (`IGD-DEC-242`, bentuk J2); seluruh 9 acceptance criteria terpenuhi 100% ([laporan uji](testing/2026-10-19-laporan-uji-k1-09-k1-11-be-igd-084.md)). *Sebelumnya:* `18 Oktober 2026 (malam, Putaran P1 dan K1)` — oleh agent di laptop: bukti mentah P1/K1 dan `Logs/quilvian-backend-20261018.json`; disahkan sebatas bukti log (`IGD-DEC-328`). Catatan resmi untuk mesin lain: [catatan bukti uji](evidence/2026-10-18-catatan-bukti-uji-p1-k1-dan-serah-terima.md). *Sebelumnya:* `18 Oktober 2026 (sore, Putaran B7 bagian 1)` — oleh agent: bukti mentah B7 (7 PNG, 6 rekaman jaringan, 10 skrip, `persiapan.json`, `percobaan.json`, `hasil.json`) dicocokkan dengan `Logs/quilvian-backend-20261018.json` 11.15–11.52 WIB; tanpa lencana `next dev`, tanpa sandi tertulis, tanpa SQL; HEAD, DLL, dan `BUILD_ID` sama di awal dan akhir; satu PNG kembar (B7-05 = B7-02) dan pencatatan B7-01 yang disusun belakangan dicatat sebagai penyimpangan (`IGD-FACT-133`). Bukan UAT. *Sebelumnya:* `18 Oktober 2026 (siang, Putaran B6)` — oleh agent: bukti mentah B6 (11 PNG, 8 rekaman jaringan, 19 skrip, `persiapan.json`, `percobaan.json`) dicocokkan dengan `Logs/quilvian-backend-20261018.json`; tanpa lencana `next dev`, tanpa PNG kembar, tanpa sandi tertulis, tanpa SQL; HEAD backend `6bb18fe6`, frontend `58ee55d32`, DLL, dan `BUILD_ID` sama di awal dan akhir. Source backend `6bb18fe6` dibaca untuk akar masalah B6-06 dan untuk menyusun `BE-IGD-083` (`IGD-FACT-128`, `130`…`132`). Bukan UAT. *Sebelumnya:* `17 Oktober 2026 (malam, Putaran B5)` — oleh agent: bukti mentah B5 (24 PNG, 15 rekaman jaringan, 43 skrip, `persiapan.json`, `percobaan.json`) dicocokkan dengan `Logs/quilvian-backend-20261017.json`; tanpa lencana `next dev`, tanpa sandi tertulis, tanpa SQL; source `InpBedAccessService` dan `BatalkanPermintaanKarenaTindakLanjutAsync` dibaca (`IGD-FACT-126`, `127`). Bukan UAT. *Sebelumnya:* `17 Oktober 2026 (status transfer ke rawat inap)` — oleh agent: register dihitung ulang dari kedua roadmap; source backend `bd645698` dan frontend `58ee55d32` dibaca untuk tolak admisi dan penutupan susulan (`IGD-FACT-123`); laporan Putaran B3 dibandingkan dengan panduannya (`IGD-FACT-124`). Hasil uji diterima atas penilaian pemilik (`IGD-DEC-319`); bukti mentah B3 ada di PC kantor dan tidak diperiksa agent. Bukan UAT. *Sebelumnya:* `12 Oktober 2026 (Pendaftaran IGD ke Triage)` — oleh agent: audit mendalam alur Pendaftaran Pasien IGD sampai Triage; empat perbaikan diterapkan (unblock Mr. X pada guard tanda vital, kueri encounterId langsung ke emergency-visits, EMERGENCY_ENCOUNTER_TYPE = 2, auto-scroll panel waktu tiba); linter 0 error, unit test IGD (emergency-visit-status 19/19, emergency-registration-payload 7/7, emergency-registration-scanned-region 5/5) 100% PASS. Seluruh alur pendaftaran IGD sampai triage dinyatakan bersih, siap operasional, dan bebas blocker ([laporan uji](testing/2026-10-12-laporan-uji-pendaftaran-pasien-igd-ke-triage.md), `IGD-DEC-301`). *Sebelumnya:* `12 Oktober 2026` — oleh agent: bukti mentah putaran 3 Tindakan (`uji-putaran-3-tindakan-20261009`) dicocokkan dengan `Logs/quilvian-backend-20261009.json` — `069-A1`…`A3` cocok, disahkan dengan penyimpangan (`IGD-DEC-295`; empat putaran, tiga tak dilaporkan); bukti layar `050`/`051` tidak sah sebagai uji hasil build (lencana "N", tangkapan layar berulang) → `FE-IGD-050`, `051` kembali 🟡 (`IGD-DEC-296`, `IGD-FACT-118`). Bukan UAT. *Baris berikut ditulis sesi lain 12 Oktober 2026 09.34 dan dikoreksi oleh baris ini:* `9 Oktober 2026 (Putaran 3 Tindakan)` — oleh agent: bukti mentah Putaran Uji 3 Tindakan IGD (`uji-putaran-3-tindakan-20261009`: `069-A1`..`069-A3`, `050-U1`..`050-U3`, `051-U1`..`051-U3`) 9 skenario 100% `PASS`. `BE-IGD-069`, `FE-IGD-050`, dan `FE-IGD-051` terverifikasi runtime dan naik ✅ ([laporan uji](testing/2026-10-09-laporan-uji-antigravity-putaran-3-tindakan.md)). Bukan UAT. *Sebelumnya:* `6 Oktober 2026 (sore)` — oleh agent: bukti mentah putaran uji bersama (`uji-putaran-bersama-20261006`) dicocokkan dengan log backend `quilvian-backend-20261006.json` — 21 skenario `PASS`, `044-U10` `NOT RUN`; diterima dengan penyimpangan tercatat (`IGD-DEC-218`). Bukan UAT. *Sebelumnya:* `5 Oktober 2026 (sore)` — oleh agent: bukti mentah uji `BE-IGD-039` (17 JSON, 2 PNG, `persiapan.json`, `P6-*`, `P7-*`) dicocokkan dengan log backend `quilvian-backend-20261005.json` — 115/115 respons cocok; skrip uji tidak tersedia. Bukan UAT. *Sebelumnya:* `5 Oktober 2026` — oleh agent: `git fetch` dan `git status` kedua repository (syarat C4 kesiapan `MVP-8` lunas); tanpa build dan tanpa uji. *Sebelumnya:* `4 Oktober 2026 (sore)` — oleh agent: bukti mentah uji gabungan `MVP-8` (30 JSON, 27 PNG, skrip) dicocokkan dengan log backend `quilvian-backend-20261004.json`; sebelumnya `FE-IGD-042`: `eslint` 0 error, uji IGD 91/91, `npm run build` lulus 11.46 (465/465, 0 warning). *Sebelumnya:* `3 Oktober 2026 (petang)` — oleh agent untuk `FE-IGD-041`: `eslint` berkas task 0 error, unit test IGD 91/91, `npm run build` lulus 17.03 (465/465 halaman, 0 warning; chunk hasil build memuat perubahan), dan pengukuran tata letak di Chromium memakai CSS asli repository — pada 1440 piksel penanda utuh di dalam bidang pandang, tabel luber 0 piksel dengan nama biasa ([laporan](task/report/frontend/FE-IGD-041.md), *Pengerjaan ulang 3 Oktober 2026*). Pengukuran itu **bukan** uji layar; server standalone port 3000 dinyalakan ulang dari hasil build untuk uji pemilik. Bukan UAT. *Sebelumnya* `3 Oktober 2026 (sore)` — uji ulang pada **hasil build** (`npm run start`, frontend `521b18a9a`, build agent 10.14) lewat agen penguji Antigravity, bukti mentah diperiksa agent ([laporan uji sore](testing/2026-10-03-laporan-uji-ulang-fe-igd-036-sore.md) bagian 7): **10 dari 10 terbukti** — `036-U7a`/`U7b`, `036-R1`, `036-U1`, `036-U9`, `038-U2`/`U3`, `034-U1`/`U2`/`U6`. Bukan UAT. *Pagi:* `3 Oktober 2026` — agent memeriksa bukti mentah uji ulang pemilik ([laporan uji ulang](testing/2026-10-03-laporan-uji-ulang-036-040-be-051.md) bagian 6): `036-U7` (putaran kedua), `036-U8`, `040-U4`, `040-U5`, `040-U6` **terbukti**; kueri invarian `BE-IGD-051` **0 baris di dev** (pernyataan pemilik, keluaran tidak disimpan). Ditemukan kemunduran loket pada commit frontend `4520abe64` — dipulihkan pemilik di working tree, belum di-commit. Bukan UAT. *Sebelumnya* `22 September 2026` — **gelombang R3.12 dan R3.11 tuntas atas penilaian pemilik.** `BE-IGD-049` ✅ (uji API S2–S5 dilaporkan lulus), `BE-IGD-050` ✅, `FE-IGD-017` ✅ (build dinyatakan lulus), `FE-IGD-034` ✅, `FE-IGD-014` ✅ (dinilai ulang, dengan celah `IGD-OQ-093` dinyatakan). **Bukti keempat klaim itu adalah pernyataan pemilik tanpa lampiran** — keluaran build, badan respons uji API, log SQL, dan tangkapan layar tidak diserahkan; agent tidak mengamati satu pun uji itu. Yang diperiksa agent sendiri: commit backend `3ebc4110` (21 berkas, sudah di-push ke `origin/rizkiG`), commit frontend `c941012ac` (**belum di-push**, `ahead 1`), dan artefak `.next` 21 September 16:03 yang memuat `active-episode`. **Belum:** kueri audit encounter yatim `BE-IGD-050` acceptance 8 — angkanya belum dilaporkan. Bukan UAT. Sebelumnya `21 September 2026` — **`EPIC IGD-04` tuntas.** Pemilik menjalankan `dotnet build -p:RunAnalyzers=false` (backend, `Build succeeded`) dan `npm run build` (frontend, `Compiled successfully`, 362/362 halaman, `postbuild` standalone berhasil), lalu commit dan push kedua repo (backend `267b56a0`, frontend `16c767916`) — hasil build dilampirkan pemilik pada percakapan, bukan diulang agent. Atas perintah pemilik, `Down` migration asli `20260917072515` diuji di basis data terpisah dan **lulus**: `DropTable` menghapus tepat satu tabel (628 → 627), sidik jari kolom/index/constraint tabel lain tidak berubah, `Up` kembali identik ([evidence](evidence/2026-09-21-uji-down-migration-be-igd-044.md)). `BE-IGD-044` ✅ dan `FE-IGD-027` ✅. Backlog frontend diperiksa: `FE-IGD-014` kriteria 2 kini ada di source (🟡, build dan uji layar milik pemilik); `FE-IGD-017` dihentikan pada gerbang karena tertahan delta kontrak backend. Bukan UAT. Sebelumnya `17 September 2026` — **`BE-IGD-045` terverifikasi runtime oleh pemilik**: build nol error (207 warning, nol warning baru) dan **12 dari 12 skenario uji API `PASS`**, termasuk concurrency S12 yang menghasilkan satu `201` dan satu `409` dengan tepat satu penugasan berjalan di basis data ([evidence](evidence/2026-09-17-verifikasi-runtime-be-igd-045.md)). Bukan UAT. Sebelumnya `npm run build` frontend **lulus bersih** (0 error, 0 warning) pada `eee3f254d`, dan perkakas `npm test` diperbaiki sehingga benar-benar menjalankan **866 test, 0 gagal** ([evidence](evidence/2026-09-17-verifikasi-build-dan-perkakas.md)). Build **bukan** bukti runtime. Sebelumnya `16 September 2026` — **uji lewat layar pertama yang pernah dijalankan pada modul ini**, oleh pemilik, untuk `FE-IGD-030`: aksi penanganan segera, pengalihan ke Assesmen IGD, dan triage susulan. Bukan UAT. Sebelumnya `15 September 2026` — pemetaan ulang acceptance criteria ke source tanpa build, test, maupun query basis data ([evidence](evidence/2026-09-15-pemeriksaan-status.md)) |
| Backend source SHA | _19 Oktober 2026 pagi (PC kantor):_ **`4cf5c1fd`** (branch `rizkiG`, sejajar origin; memuat commit laptop `4fbf5ff6` dan merge integration). Kajian dampak `IGD-FACT-155`: kode IGD tidak berubah, migration lengkap di `QuilvianNewDevRizki`, sehingga bukti P1/K1 tidak usang. _18 Oktober 2026 malam:_ `6bb18fe6` + working tree `BE-IGD-083` dibaca untuk audit pasien prioritas dan billing IGD. **`6bb18fe6`** (branch `rizkiG`, 17 Oktober 2026 18.33) — dibaca 18 Oktober 2026 untuk penilaian B6 dan perencanaan `BE-IGD-083`; working tree: dokumen blueprint, belum di-commit. *Sebelumnya:* **`c7e523fd`** (branch `rizkiG`, 11 Oktober 2026; working tree bersih sebelum pass 0o) + dokumen pass 0o, belum di-commit. *Sebelumnya:* **`43dab6da`** (branch `rizkiG`, sejajar origin 6 Oktober 2026 — commit pemilik memuat source `BE-IGD-041`, `BE-IGD-064` dan dokumennya). Working tree: dokumen desain 6 Oktober (decision log, capability map, kontrak, arsitektur, PRD, kamus data, matriks uji, flowchart, manifest, berkas ini), belum di-commit. *Sebelumnya:* **`8d81d361`** (branch `rizkiG`, sama dengan origin per 5 Oktober 2026) — `0078a0dc` memuat dokumen keputusan C3/C2, lalu merge integration PR #233–#235 yang **tidak** menyentuh area IGD; source IGD identik dengan `c1f79f79`. Working tree: dokumen pass desain 5 Oktober (bagian 0l), belum di-commit. *Sebelumnya:* **`d76279c0`** (branch `rizkiG`, sama dengan origin per 5 Oktober 2026, working tree bersih; commit sesudah `c1f79f79` — `d4b7fbad`, `d76279c0` — hanya dokumen, isi di luar `docs/` identik dengan `c1f79f79`). *Sebelumnya:* **`c1f79f79`** (branch `rizkiG`, `ahead 12`; source `BE-IGD-061` pengerjaan ulang ter-commit, build Rizki 3 Oktober 2026 0 error 0 warning) + working tree dokumen 4 Oktober 2026, belum di-commit. *Sebelumnya:* **`2a63a3bb`** (branch `rizkiG`, `ahead 11` — seluruh commit sesudah `5af6ef3b` hanya dokumen) **+ working tree, belum di-commit**: pengerjaan ulang `BE-IGD-061` (`EmergencyDispositionService.cs`, `EmergencyObservationController.cs`, `EmergencyObservationService.cs`) dan dokumen blueprint 3 Oktober 2026 petang. *Sebelumnya* **`5af6ef3b`** (branch `rizkiG`, `ahead 3` terhadap origin) — tempat review status 3 Oktober 2026; source backend IGD tanpa perubahan working tree. *Sebelumnya* **`0d13f3a8`** (branch `rizkiG`, sinkron dengan origin) — tempat audit source baca-saja pass desain 22 September 2026 ([evidence](evidence/2026-09-22-desain-encounter-first.md)). Sebelumnya `267b56a0` (branch `rizkiG`) — tempat pemeriksaan 21 September 2026 (memuat `BE-IGD-048`); sebelumnya `94bf6ec5` (17 September) dan `e89907c5`. SHA desain revisi 6 tetap `300922c` |
| Frontend source SHA | _21 Oktober 2026 malam:_ commit rujukan `a244a729f` (branch `RizkiV2`) + working tree task `FE-IGD-070` (8 berkas selesai, nol commit; `IGD-DEC-338`…`346`). *Sebelumnya:* _19 Oktober 2026 pagi (PC kantor):_ **`5f4ab690a`** (branch `RizkiV2`, sejajar origin; memuat perbaikan modal `6b44f8742`). Langkah loket IGD ditambah panel status karyawan oleh Billing, yang hanya tampil untuk pasien bukan kategori umum (`IGD-FACT-155`). *Sebelumnya:* **`58ee55d32`** (branch `RizkiV2`, 16 Oktober 2026) — build Putaran B5 dan B6 (`BUILD_ID` `vyZON79AuJIJU924yT7Hf`). *Sebelumnya:* **`3335f4b53`** (branch `RizkiV2`, 11 Oktober 2026, working tree bersih). *Sebelumnya:* **`6680278a2`** (branch `RizkiV2`, sejajar origin 6 Oktober 2026 — commit pemilik memuat `FE-IGD-044`), working tree bersih. *Sebelumnya:* **`57b1d360f`** (branch `RizkiV2`, sama dengan origin per 5 Oktober 2026, working tree bersih) — memuat pengerjaan ulang `FE-IGD-042` (`emergency-assessment-constant.jsx`, `emergency-assessment-status-action.utils.js`, +22 baris), isi yang sama dengan hasil build 4 Oktober 2026 11.46. *Sebelumnya:* **`19ba512de`** (branch `RizkiV2`, `ahead 3`) **+ working tree, belum di-commit**: pengerjaan ulang `FE-IGD-042` (`emergency-assessment-constant.jsx`, `emergency-assessment-status-action.utils.js`); hasil build 4 Oktober 2026 11.46 memuatnya. *Sebelumnya:* **`521b18a9a`** (branch `RizkiV2`, `ahead 2`) **+ working tree, belum di-commit** — 3 Oktober 2026 petang: perbaikan loket `IGD-DEC-182` (`emergency-registration.service.js`) dan pengerjaan ulang `FE-IGD-041` (empat berkas: view daftar Pengkajian, util daftar, konstanta, CSS module). Hasil build 17.03 memuat keduanya. *Sebelumnya* **`521b18a9a`** (branch `RizkiV2`, `ahead 2`, working tree bersih) — 3 Oktober 2026 10.31, memuat pemulihan `verification-step.jsx` dan pengerjaan ulang `FE-IGD-036`; belum di-push. *Sebelumnya* **`4520abe64`** (branch `RizkiV2`, `ahead 1`) **+ working tree**: `verification-step.jsx` dipulihkan pemilik (membatalkan kemunduran `4520abe64`) dan pengerjaan ulang `FE-IGD-036` oleh agent pada `emergency-triage-start-dialog.jsx` — keduanya belum di-commit, 3 Oktober 2026. *Sebelumnya* **`c941012ac`** (branch `RizkiV2`) — **sinkron dengan origin per 22 September 2026** (diperiksa agent: `git status -sb` tanpa `ahead`); *catatan lama:* (`ahead 1` terhadap origin — belum di-push; memuat `FE-IGD-017` dan `FE-IGD-034`) — 21 September 2026 16:01. Sebelumnya `198d56d9e` (sinkron dengan origin) — 21 September 2026 malam; memuat `FE-IGD-014` kriteria 2. Sebelumnya `16c767916` (revisi terbaru `FE-IGD-027`). Sebelumnya `eee3f254d` (17 September, memuat R3.9) dan `43adae648`. SHA desain revisi 6 tetap `96a91201` |

Dokumen ini ringkasan keadaan. Sumber kebenaran status per task tetap
[roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) dan
[roadmap/frontend-roadmap.md](roadmap/frontend-roadmap.md).

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| **`MVP-0`** (20 Oktober 2026, `IGD-DEC-333`), `MVP-1`, `MVP-2`, R3.7, R3.9, R3.10, R3.12, **`MVP-6`** (`BE-IGD-039` ✅ 5 Oktober 2026), **`MVP-8`** (6 dari 6 task ✅ — `BE-IGD-061` ✅ 5 Oktober 2026), **`MVP-7`** (13 dari 13 task ✅, 3 Oktober 2026 akhir — terbukti dengan akun peran nyata; DoD butir 10 UAT terpisah), **`MVP-10`** (2 dari 2 task ✅ sejak 11 Oktober 2026 malam; dicatat 17 Oktober 2026) | `MVP-3`, `MVP-4`, `MVP-5`, **`MVP-9`** (`EPIC IGD-14` Ruang Kerja Dokter IGD — desain `approved` `IGD-DEC-230`, 6 Oktober 2026), **`MVP-11`** (`EPIC IGD-15` transfer ke rawat inap — empat task 🟡 sesudah Putaran B5, 17 Oktober 2026 malam) | — *(`EPIC IGD-12` dibuka 11 Oktober 2026; sebelumnya ⛔ `BE-IGD-056`, `FE-IGD-037`)* |

| Gelombang | Isi | Status | Yang menahan |
| --- | --- | --- | --- |
| `MVP-0` | Status kunjungan tidak dapat mundur (`BE-IGD-017`…`022`) | `DONE` ✅ — 20 Oktober 2026 | — *(`BE-IGD-017` dibatalkan `IGD-DEC-333`: bukan lingkup IGD dan acceptance-nya menuntut project test yang sudah dihapus; `BE-IGD-018`…`022` seluruhnya ✅)* |
| `MVP-1` | Pendaftaran & encounter `Emergency` (`BE-IGD-023`, `024`) | `DONE` ✅ | — |
| `MVP-2` | Satu pasien satu episode (`BE-IGD-025`) | `DONE` ✅ | — |
| `MVP-3` | Pengkajian IGD tanpa antrean (`BE-IGD-026`…`030`) | `IN_PROGRESS` 🟡 | `BE-IGD-026`: uji langkah mundur migration belum |
| `MVP-4` | Kepergian pasien (`BE-IGD-031`…`034`) | `IN_PROGRESS` 🟡 | `BE-IGD-031`: uji `RENAME` balik belum. **Ditambahkan 17 September 2026:** runtime `arrive` dan `accept-handover` tertahan `BE-IGD-039` |
| `MVP-5` | Riwayat dokter & serah terima (`BE-IGD-035`; `EPIC IGD-04`) | `IN_PROGRESS` 🟡 | **`BE-IGD-035` ✅ 20 Oktober 2026** (`IGD-DEC-330`) — approval `IGD-DEC-100`/`101` diberikan pemilik, butir terakhir gelombang ini lunas. *Sebelumnya:* `BE-IGD-035` kriteria 2 (dikerjakan `BE-IGD-041`); `EPIC IGD-04`: **`BE-IGD-045` ✅ terverifikasi runtime 17 September 2026** (12/12 skenario `PASS`); `BE-IGD-044` ✅ **21 September 2026** (acceptance 4 dikerjakan `BE-IGD-048`; acceptance 5 — uji `Down` migration asli di basis data terpisah — lulus, [evidence](evidence/2026-09-21-uji-down-migration-be-igd-044.md)); `BE-IGD-048` ✅ **21 September 2026** — migration diterapkan ke dev, kriteria A–E dan siklus `Up→Down→Up` terbukti di salinan basis data terpisah, kontrak `0.8.0` ([laporan](task/report/backend/BE-IGD-048.md)); **`FE-IGD-027` ✅ (dinaikkan kembali 21 September 2026)** — runtime inti terverifikasi 18 September 2026 **pada revisi `3213419a7`** (13 pemeriksaan lewat layar `PASS`); revisi terbaru (terminologi + "Data historis", commit `16c767916`) lolos `npm run build` pemilik 21 September 2026. Tampilan "Data historis" belum terlihat di layar karena dev 0 baris legacy. **`EPIC IGD-04` TUNTAS** — seluruh task (`BE-IGD-044`, `045`, `048`, `FE-IGD-027`) ✅. **Ditambahkan 17 September 2026:** runtime sikap pesanan tertahan `BE-IGD-039` |
| R3.7 | Migration, master data pindah modul, kolom respons (`BE-IGD-036`…`038`) | `DONE` ✅ | — |
| `MVP-6` | Kewenangan unit (`BE-IGD-039`) | `DONE` ✅ — 5 Oktober 2026 (`IGD-DEC-201`). *Sebelumnya `BLOCKED` ⛔* | — *(dulu: Security/Privacy owner; pemetaan unit 0 dari 18)* |
| `MVP-7` | Encounter-first (`EPIC IGD-11`: `BE-IGD-051`…`055`, `057`…`059`; `FE-IGD-035`, `036`, `038`…`040`) | `DONE` ✅ — **13 dari 13 task ✅** (3 Oktober 2026 akhir) | `FE-IGD-036` ✅ kembali sesudah uji ulang loket dengan akun Petugas Pendaftaran (3 dari 3). Tersisa di luar task: DoD butir 10 — **UAT diserahkan ke tim UAT oleh Rizki (3 Oktober 2026)**, hasilnya dicatat terpisah saat ada; rilis per lingkungan (C3); push (C7). Pengembangan dan verifikasi pengembang `MVP-7` selesai. *Riwayat siang:* **`FE-IGD-036` 🟡** — loket memanggil rute kiosk sehingga petugas loket ditolak `403` (`C4-01`); rute diganti ke `/admin` (`IGD-DEC-182`), build lulus 15.31, menunggu uji ulang dengan akun loket. *Sebelumnya: `DONE` 13/13 pada uji SuperAdmin.* **DoD `EPIC IGD-11` belum lengkap:** butir 10 (UAT tim terpisah); butir 8 **lunas** lewat `IGD-DEC-181` (persetujuan Registrasi, 3 Oktober 2026); `verify-module-readiness` `MVP-7` → **`READY_WITH_CONDITIONS`** 3 Oktober 2026 ([evidence](evidence/2026-10-03-kesiapan-mvp-7.md), syarat C1–C7); kueri invarian dan rekonsiliasi K1 untuk lingkungan selain dev menunggu rilis di sana |
| `MVP-8` | Penutupan lewat disposisi (`EPIC IGD-13`: `BE-IGD-060`…`063`; `FE-IGD-041`, `042`) | `DONE` ✅ — **6 dari 6 task ✅** sejak 5 Oktober 2026 (`BE-IGD-061` ✅, `IGD-DEC-201`). *Sebelumnya:* `IN_PROGRESS` 🟡 — **5 dari 6 task ✅** (`BE-IGD-060`, `062`, `063`, `FE-IGD-041`, `FE-IGD-042`); `BE-IGD-061` 🟡 — acceptance 13–19 terbukti 4 Oktober 2026, acceptance 2–3 tertahan `BE-IGD-039`. *Sebelumnya 3 dari 6* | **4 Oktober 2026 (sore):** uji gabungan `MVP-8` diperiksa dari bukti mentah — `FE-IGD-041` ✅, `FE-IGD-042` ✅, `BE-IGD-061` S13–S19 terbukti; penyimpangan penguji disahkan `IGD-DEC-188`. Sisa: S2, S3, S12 (`BE-IGD-039`); `verify-module-readiness` `MVP-8`. **3 Oktober 2026 (petang, akhir):** kontrak amandemen `approved` (`IGD-DEC-186`); `BE-IGD-061` pengerjaan ulang selesai di source (QBE `PASS`) — menunggu `dotnet build` dan uji API S13–S19; `FE-IGD-042` menunggu pengerjaan ulang (`IGD-DEC-187`). *Sebelumnya hari yang sama:* `BE-IGD-061` S6 **sudah diputuskan** — observasi Dieskalasi menahan penutupan (`IGD-DEC-183`), data lama dibiarkan dengan pesan yang diperjelas (`IGD-DEC-184`), dikerjakan sebagai pengerjaan ulang `BE-IGD-061` (`IGD-DEC-185`); menunggu amandemen kontrak (validation §11, state §9, API §9.2), perluasan kartu, implementasi, build, dan uji API pemilik. S2, S3, S12 tetap tertahan `403` kewenangan unit (`BE-IGD-039`). `FE-IGD-041`: tata letak diperbaiki dan build lulus — menunggu uji layar U2, U6, U7 pada hasil build. *Sebelumnya:* S6 menyingkap observasi `Escalated` yang tidak menahan penutupan (keputusan pemilik); `FE-IGD-041` kolom PENUTUPAN terdorong keluar layar pada 1440 piksel |
| `MVP-9` | Ruang Kerja Dokter IGD (`EPIC IGD-14`, `IGD-DEC-220`…`229`) | `DESIGN` — `approved` 6 Oktober 2026 (`IGD-DEC-230`, manifest bagian 0n); siap `plan-module-delivery` | Sebagian kartu menunggu `IGD-OQ-117` (instruksi lab/radiologi) dan `IGD-OQ-119` (gizi) |
| `MVP-10` | Dokter jaga IGD dan override beralasan (`EPIC IGD-12`, `IGD-DEC-237`…`240`) | `DONE` ✅ — 2 dari 2 task ✅ sejak 11 Oktober 2026 malam (`BE-IGD-056` 9 dari 9 acceptance, `FE-IGD-037` 8 dari 8); dicatat di berkas ini 17 Oktober 2026. *Sebelumnya:* `IN_PROGRESS` 🟡 — kartu disusun ulang 11 Oktober 2026; source awal ada (`f39250df`, `905aec3bd`) | Bukan task: rilis per lingkungan menunggu `IGD-UNK-18` (syarat rilis R3.17.3) |
| `MVP-11` | Form Permintaan Rawat Inap dan transfer ke Rawat Inap (`EPIC IGD-15`: `BE-IGD-072`, `074`, `077`, `079`, `083`; `FE-IGD-059`…`061`, `064`, `066`) | `IN_PROGRESS` 🟡 — **18 Oktober 2026 (uji `Down()`): 9 dari 10 task ✅** (`BE-IGD-072` ✅); tinggal 🟡 `BE-IGD-077` (Rawat Inap). *Sebelumnya:* **18 Oktober 2026 (Putaran B7 bagian 1): 8 dari 10 task ✅** (`BE-IGD-083` ✅). *Sebelumnya:* **18 Oktober 2026 (Putaran B6): 7 dari 10 task ✅** (7 dari 9 sebelum `BE-IGD-083` ditambahkan): `BE-IGD-074`, `079`, `FE-IGD-059`, `060`, `061`, `064`, `066`. `BE-IGD-073`, `075`, `FE-IGD-062` dibatalkan `IGD-DEC-304` dan tidak dihitung. *Sebelumnya:* 5 dari 9 task ✅ (17 Oktober 2026 malam) | `BE-IGD-072` uji `Down()` (R1-01); ~~`BE-IGD-083`~~ ✅ 18 Oktober 2026; `BE-IGD-077` acceptance 4 menunggu `BE-IGD-083` dan pemanggil di Rawat Inap (`IGD-OQ-132`, diminta lewat `IGD-REQ-003`), acceptance 3 menunggu tombol _Tolak_ Rawat Inap |
| R3.19 | Pasien prioritas dan jasa kunjungan IGD (`EPIC IGD-16` dan Billing IGD: `BE-IGD-080`, `084`, `085`; `FE-IGD-067`) | `DONE` ✅ — 19 Oktober 2026 (siang): 4 dari 4 task ✅ (`BE-IGD-080`, `BE-IGD-085`, `FE-IGD-067`, `BE-IGD-084`). Gelombang R3.19 tuntas 100% | K1-04, K1-12 (akun Lab, Apoteker; di luar lingkup inti IGD/kasir). K1-09 (kasir) & K1-11 (guard pembatalan) terbukti PASS |

`DONE` di sini berarti seluruh task gelombangnya ✅ menurut roadmap. Status UAT **terpisah** dan
**tidak** ditulis lulus.

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `IN_PROGRESS` — **26 Oktober 2026 (petang, langkah 2 dan 3) — backend 59 ✅ dari 64 aktif (92,2%), frontend 49 ✅ dari 58 aktif (84,5%), gabungan 108 ✅ dari 122 aktif (88,5%).** Empat task naik tanpa satu baris kode: `FE-IGD-013` ✅ sesudah `IGD-OQ-141` dijawab `IGD-DEC-351` (diagnosis keperawatan IGD cukup teks bebas CPPT) dan sesudah thunk detail pengkajian ditambahkan; `BE-IGD-043`, `FE-IGD-025`, `FE-IGD-026` ✅ lewat tiga laporan susulan. Temuan laporan yang patut diketahui: seeder master IGD **nol pemanggil** (`IGD-FACT-236`), empat endpoint daftar `ClinicalManagement` **menjawab tanpa penyaring pasien** (`IGD-FACT-237`, diserahkan sebagai `IGD-OQ-143`), dan validasi Emergency Visit pendaftaran IGD **masih mati 80 hari** sejak `1681b1737` (`IGD-FACT-238`). Tiga permintaan pemilik terbit belum dikirim: `IGD-REQ-007`, `IGD-REQ-008`, tindak lanjut `IGD-REQ-003`. *Sebelumnya:* `IN_PROGRESS` — **26 Oktober 2026 (penilaian lima putaran + FE-IGD-071 dikartukan) — backend 58 ✅ dari 64 aktif (90,6%), frontend 46 ✅ dari 58 aktif (79,3%), gabungan 104 ✅ dari 122 aktif (85,2%).** `FE-IGD-071` (tab Tagihan Pasien IGD, `IGD-DEC-349`) masuk register 🟡 atas keputusan pemilik: layarnya sudah dikerjakan dan di-live-test 23 Oktober tetapi berada di luar hitungan. Delapan dari sembilan acceptance terbukti; **acceptance 5 — penyamaran angka nominal menurut otorisasi finansial — belum diuji** karena seluruh skenario memakai `superadmin@admin.com`. Nol kartu backend: endpoint `charge-summary` di-reuse tanpa migration. Penyebut aktif karena itu naik dari 121 ke **122**. **Petang hari yang sama — `IGD-DEC-336` tidak lagi menahan rilis:** migration `20261026031429_PersempitIndeksCatatanDokterPerKunjungan` dibuat, dibangun, dan diterapkan pemilik sesudah integration di-merge lebih dulu (`56a19904`, 30 commit, nol konflik); snapshot berubah tepat satu baris dan nol entitas modul lain hilang (`IGD-FACT-230`, `231`). Pada pass yang sama `IGD-FACT-218` **dikoreksi** `IGD-FACT-229`: sepuluh ruas pengkajian terbukti **tersimpan** — cacatnya murni jalur baca layar, bukan kehilangan data. *Pada pass yang sama, sebelum kartu ini:* backend 58 ✅ dari 64 aktif (90,6%), frontend 46 ✅ dari 57 aktif (80,7%), gabungan 104 ✅ dari 121 aktif (86,0%). Lima putaran yang sudah dieksekusi tetapi belum pernah dinilai akhirnya dinilai dari bukti mentah: **D3** menaikkan tiga task — `BE-IGD-070` 8/8, `FE-IGD-054` 5/5, `FE-IGD-055` 4/4 — sekaligus melunasi syarat rilis R3.16.5. **L1 nol kenaikan:** `FE-IGD-012` naik ke 3/4, `FE-IGD-013` dan `FE-IGD-022` tetap 🟡. Temuan terberatnya **sepuluh ruas pengkajian yang tidak dapat dibaca kembali dari layar yang menulisnya** — layar IGD nol thunk detail, sedangkan ruas itu hanya ada pada `PatientAssessmentDetailResponse`. **Pasangan L1R** membuktikan penuh regresi `BE-IGD-019` (triase Draft tersimpan `200` pada kunjungan tertutup) dan perbaikannya (`409`, nol baris), tanpa mengubah status task. **E2E** 9 dari 10; satu-satunya FAIL (`E2E-10`, invoice `500`) sudah hilang di `HEAD` lewat `d72dcd65` milik Billing, jadi nol kartu dibuka. Enam klaim `PASS` dibantah berkas buktinya sendiri, termasuk `L1-03-dokter.png` yang ternyata halaman *COMING SOON!*. [Penilaian lima putaran](evidence/2026-10-26-penilaian-lima-putaran-d3-l1-l1r-e2e.md). Menahan commit dan rilis: `IGD-DEC-336` (migration indeks). *Sebelumnya:* `IN_PROGRESS` — **21 Oktober 2026 (malam, implementasi FE-IGD-070) — backend 57 ✅ dari 64 aktif (89,1%), frontend 44 ✅ dari 57 aktif (77,2%), gabungan 101 ✅ dari 121 aktif (83,5%).** `FE-IGD-070` 🟡 Sebagian (pintu Pasien Tanpa Identitas di Pendaftaran IGD dan pencabutan blok di Assesmen Awal; delapan berkas selesai, ESLint 0 error / 7 warning baseline, nol temuan baru; menunggu uji visual layar port 3000 oleh pemilik). Register aktif frontend bertambah dari 56 ke 57 dan total gabungan dari 120 ke 121 aktif (penyebut penuh 125 baris termasuk 4 task dibatalkan: 101 / 125 = 80,8%). [Laporan FE-IGD-070](task/report/frontend/FE-IGD-070.md). *Sebelumnya:* `IN_PROGRESS` — **21 Oktober 2026 (sesudah Putaran D2R-3) — backend 57 ✅ dari 64 aktif (89,1%), frontend 44 ✅ dari 56 aktif (78,6%), gabungan 101 dari 120 aktif (84,2%).** Lima task naik sekaligus: `FE-IGD-045` 10/10, `FE-IGD-046` 6/6, `FE-IGD-048` 6/6, `FE-IGD-049` 7/7, `BE-IGD-068` 5/5 — seluruh sisa D2R lunas. Bukti terkuat: invoice `BIL-20261021-00000001` memuat tepat satu baris `PHARMACY`/`DRUG` (resep `RX-20261021-00001`) dan nol baris jasa konsultasi. [Penilaian D2R-3](evidence/2026-10-21-penilaian-putaran-d2r3.md). Menahan rilis: `IGD-DEC-336` (migration indeks). *Sebelumnya:* `IN_PROGRESS` — **21 Oktober 2026 (malam, registrasi task FE-IGD-070) — backend 56 ✅ dari 64 aktif (87,5%), frontend 40 ✅ dari 57 aktif (70,2%), gabungan 96 dari 121 aktif (79,3%).** `FE-IGD-070` 🟡 Sebagian (pintu Pasien Tanpa Identitas di Pendaftaran IGD dan pencabutan blok di Assesmen Awal; delapan berkas selesai, ESLint 0 error / 7 warning baseline, nol temuan baru; menunggu verifikasi layar port 3000 oleh pemilik). Register aktif frontend bertambah dari 56 ke 57 dan gabungan dari 120 ke 121 aktif. [Laporan FE-IGD-070](task/report/frontend/FE-IGD-070.md). *Sebelumnya:* `IN_PROGRESS` — **21 Oktober 2026 (sesudah Putaran D2R-2) — backend 56 ✅ dari 64 aktif (87,5%), frontend 40 ✅ dari 56 aktif (71,4%), gabungan 96 dari 120 aktif (80,0%); tidak berubah.** Hasil utama putaran: **`IGD-DEC-337` terverifikasi penuh** (`D2R2-A01a` `200` oleh dokter kedua selagi draf dokter pertama terbuka; `D2R2-A01b` `400` saat drafnya sendiri terbuka). `FE-IGD-049` naik ke 6/7. Dua temuan baru: catatan tertanda DPJP bukan penulisnya (`IGD-OQ-140`) dan sebab kegagalan addendum sudah ditunjuk (`IGD-FACT-191`). [Penilaian D2R-2](evidence/2026-10-21-penilaian-putaran-d2r2.md). *Sebelumnya:* `IN_PROGRESS` — **21 Oktober 2026 — backend 56 ✅ dari 64 aktif (87,5%), frontend 40 ✅ dari 56 aktif (71,4%), gabungan 96 dari 120 aktif (80,0%).** `FE-IGD-047` ✅ 9/9 dan `FE-IGD-045` 9/10 sesudah dua skenario UI D2R disempurnakan ([penilaian D2R](evidence/2026-10-20-penilaian-putaran-d2r.md) bagian 9). Penyimpangan indeks `IGD-OQ-139` masih terbuka dan menahan rilis. *Sebelumnya:* `IN_PROGRESS` — **20 Oktober 2026 (malam) — backend 56 ✅ dari 64 aktif (87,5%), frontend 39 ✅ dari 56 aktif (69,6%), gabungan 95 dari 120 aktif (79,2%).** `BE-IGD-067` ✅ 6/6 sesudah `IGD-OQ-138` dijawab opsi (b) (`IGD-DEC-335`). **Penyimpangan skema terbuka:** indeks `IX_TrxDoctorConsultation_EncounterId` sudah dilepas uniknya di dev tanpa migration, snapshot masih versi lama — `IGD-OQ-139`, menahan rilis dan dikoordinasikan dengan Rawat Inap. *Sebelumnya:* `IN_PROGRESS` — **20 Oktober 2026 (sesudah Putaran D2R) — backend 55 ✅ dari 64 aktif (85,9%), frontend 39 ✅ dari 56 aktif (69,6%), gabungan 94 dari 120 aktif (78,3%).** `BE-IGD-065` ✅ 8/8; `BE-IGD-067` 5/6 dengan acceptance 1 tertahan `IGD-OQ-138` (satu catatan dokter per encounter non-rawat-inap, `IGD-FACT-172`). Empat klaim `PASS` D2R tidak didukung buktinya ([penilaian D2R](evidence/2026-10-20-penilaian-putaran-d2r.md)). *Sebelumnya:* `IN_PROGRESS` — **20 Oktober 2026 (akhir) — backend 54 ✅ dari 64 aktif (84,4%), frontend 39 ✅ dari 56 aktif (69,6%), gabungan 93 dari 120 aktif (77,5%).** `BE-IGD-066` ✅ 7/7 sesudah acceptance 5 diterima lewat penalaran diff (`IGD-DEC-334`). Sisa pembuktian terkumpul pada [panduan uji Putaran D2R](testing/2026-10-17-panduan-uji-sisa-modul-igd.md) bagian 5B. *Sebelumnya:* `IN_PROGRESS` — **20 Oktober 2026 (penutup, sesudah `IGD-DEC-333`) — backend 53 ✅ dari 64 aktif (82,8%), frontend 39 ✅ dari 56 aktif (69,6%), gabungan 92 dari 120 aktif (76,7%).** `BE-IGD-017` dibatalkan dan keluar dari penyebut; gelombang `MVP-0` tuntas (`IGD-FACT-166`). Empat task kini berstatus dibatalkan. *Sebelumnya:* `IN_PROGRESS` — **20 Oktober 2026 (penilaian Putaran D2 dari bukti mentah dan approval `IGD-DEC-330`) — backend 53 ✅ dari 65 aktif, frontend 39 ✅ dari 56 aktif, gabungan 92 dari 121 aktif (76,0%).** Atas penyebut penuh: 92 dari 124 (74,2%). Kenaikan **hanya** dari `BE-IGD-035` 🟡 → ✅ sesudah approval `IGD-DEC-100`/`101` diberikan pemilik. **Putaran D2 tidak menaikkan satu pun task ke ✅** ([penilaian Putaran D2](evidence/2026-10-20-penilaian-putaran-d2.md)): lima klaim `PASS` tidak didukung berkas buktinya sendiri (`IGD-FACT-158`…`162`), termasuk `D2-U14` yang ditandai `PASS` dengan ruas `bukti` kosong. Yang dihasilkan D2 nyata tetapi lain bentuknya — 11 kriteria acceptance baru terbukti pada `BE-IGD-065` (7/8), `BE-IGD-066` (6/7), `BE-IGD-067` (4/6), `FE-IGD-046` (5/6), `FE-IGD-049` (4/7). `FE-IGD-057` direkonsiliasi ⛔ → 🟢 (`IGD-DEC-331`), tidak mengubah hitungan. *Sebelumnya:* `IN_PROGRESS` — **19 Oktober 2026 (siang, pengujian BE-IGD-084 K1-09 & K1-11) — `BE-IGD-084` ✅; backend 52 ✅ dari 65 aktif, frontend 39 ✅ dari 56 aktif, gabungan 91 dari 121 aktif (75,2%).** Seluruh 9 kriteria terpenuhi: guard pembatalan klinis HTTP 400 terbukti, live browser testing kasir Playwright headed Chromium memuat Jasa Kunjungan IGD Rp150.000,00 pada tagihan kasir billing. Atas penyebut penuh: backend 52 dari 67, frontend 39 dari 57, gabungan 91 dari 124 (73,4%). *Sebelumnya:* **19 Oktober 2026 (siang, aturan bukti `npm run dev` dan rekonsiliasi register) — backend 51 ✅ dari 65 aktif, frontend 39 ✅ dari 56 aktif, gabungan 90 dari 121 aktif (74,4%).** Dua perubahan pada pass yang sama. (1) **Pemilik menyatakan layar yang dilayani `npm run dev` sah sebagai bukti selama modul masih dalam pengembangan**, sehingga dasar `IGD-DEC-296` tidak lagi berlaku: `FE-IGD-050`, `FE-IGD-051`, dan `FE-IGD-056` dipulihkan ke ✅ atas penilaian pemilik (+3). Catatan yang tetap melekat: `050-U3` tak terlihat dan skrip `051` memakai aturan PASS "salah satu" (`IGD-FACT-118`); hemodialisa dikeluarkan dari lingkup `FE-IGD-056` karena `HemodialysisManagement` belum punya pembacaan per `encounterId`. (2) **Penyebut dikoreksi:** `BE-IGD-073`, `BE-IGD-075`, dan `FE-IGD-062` dibatalkan `IGD-DEC-304` dan dikeluarkan dari penyebut, barisnya tetap ada. Atas penyebut penuh: backend 51 dari 67, frontend 39 dari 57, gabungan 90 dari 124 (72,6%). **PNG bukti uji lebih dari 24 jam dihapus 19 Oktober 2026** (404 berkas, 213 MB) atas perintah pemilik karena drive C: penuh 100%; JSON hasil dan skrip `.mjs` dipertahankan. *Sebelumnya:* **19 Oktober 2026 (pagi, PC kantor): `BE-IGD-085` ✅, `FE-IGD-067` ✅ (P1-06R lulus 100%) — gabungan 87 dari 124 (70,2%).** *Sebelumnya:* **19 Oktober 2026 (pagi, awal): `BE-IGD-085` ✅ — backend 51 ✅ dari 67, frontend 35 ✅ dari 57, gabungan 86 dari 124 (69,4%).** *Sebelumnya:* **18 Oktober 2026 (malam, penilaian P1/K1): backend 50 ✅ dari 67, frontend 35 ✅ dari 57, gabungan 85 dari 124 (68,5%).** *Sebelumnya:* **18 Oktober 2026 (malam, point 1 dan 2): `BE-IGD-085` dan `BE-IGD-084` 🟡 — kode selesai, QBE Strict `PASS` (10 berkas); menunggu build Rizki, lalu uji P1 dan K1. Data master jasa kunjungan dev dibuat (`PR-RSMMC-00025`, `TF-RSMMC-00016`).** *Sebelumnya:* **18 Oktober 2026 (malam): audit — `BE-IGD-080` 🟡; R3.19 `BE-IGD-084`, `085` siap; backend 49 ✅ dari 67, frontend 35 ✅ dari 57 (`FE-IGD-067` 🟡), gabungan 84 dari 124 (67,7%).** *Sebelumnya:* **18 Oktober 2026 (sore): uji `Down()` — `BE-IGD-072` ✅; backend 50 ✅ dari 65, frontend 36 ✅ dari 57, gabungan 86 dari 122 (70,5%).** *Sebelumnya:* **18 Oktober 2026 (sore): Putaran B7 bagian 1 — `BE-IGD-083` ✅; backend 49 ✅ dari 65, frontend 36 ✅ dari 57, gabungan 85 dari 122 (69,7%).** *Sebelumnya:* **18 Oktober 2026 (siang): Putaran B6 — `BE-IGD-074` dan `FE-IGD-061` ✅; `BE-IGD-083` masuk register ⛔ (`IGD-OQ-133`); backend 48 ✅ dari 65, frontend 36 ✅ dari 57, gabungan 84 dari 122 (68,9%). Sebelum `BE-IGD-083` masuk register: 84 dari 121 (backend 48/64).** *Sebelumnya:* **17 Oktober 2026 (malam): Putaran B5 — `FE-IGD-060` ✅; backend 47 ✅ dari 64, frontend 35 ✅ dari 57, gabungan 82 dari 121 (67,8%).** *Sebelumnya:* **17 Oktober 2026: register dihitung ulang dan dikoreksi (`IGD-DEC-319`) — backend 47 ✅ dari 64, frontend 34 ✅ dari 57, gabungan 81 dari 121 (66,9%).** `BE-IGD-074`, `BE-IGD-077`, dan `FE-IGD-061` kembali 🟡. *Sebelumnya:* **15 Oktober 2026: Anamnesis Kasus Trauma, SAMPLE (Last Meal), dan Diagram Tubuh (Body Map) IGD selesai (`BE-IGD-076` ✅, `FE-IGD-063` ✅, live browser testing 100% PASS)**. Backend 44 ✅ dari 56, frontend 32 ✅ dari 49, gabungan 76 dari 105 (72.4%). *Sebelumnya:* 12 Oktober 2026: backend 43 ✅ dari 55, frontend 31 ✅ dari 48. | `IN_PROGRESS` — **15 Oktober 2026: `FE-IGD-063` ✅ (komponen visual Body Map siluet Anterior/Posterior & Status Lokalis sync)**. | `PARTIAL` — radiologi ditahan `IGD-DEC-111`; tab laboratorium diperbaiki `FE-IGD-023`; tanda vital observasi menunggu `BE-IGD-046`/`FE-IGD-028`; billing IGD belum direncanakan | `IN_PROGRESS` — Live browser testing Chromium headed mode **100% PASS** (15 Oktober 2026, 6/6 skenario P0-P4) |

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |
| `IGD-OQ-142` (26 Oktober 2026) | Fallback tarif kunjungan IGD tanpa kelas pasien dicocokkan lewat **nama** master data terhadap string `"RAWAT JALAN"` yang di-hard-code, di berkas **milik Rawat Inap**, dan nilainya mengalir ke penagihan serta penentuan jaminan. Nol hasil dan dua hasil sama-sama menjawab `(null, null)` tanpa galat (`IGD-FACT-224`…`226`). Sudah ter-commit tanpa keputusan | **Rizki Gunawan** + **Muhammad Hamzah** (pemilik berkas) + **owner Billing** (keputusan tarifnya) | Kebenaran tarif IGD; menyentuh layar Tagihan Pasien IGD (`IGD-DEC-349`) | Ya — tidak menahan task IGD mana pun hari ini |
| ~~`IGD-OQ-141`~~ (26 Oktober 2026) | **Dijawab `IGD-DEC-351` hari yang sama — tidak lagi menahan.** Diagnosis keperawatan pasien IGD cukup teks bebas `assessmentSummary` CPPT; penolakan `422` `nursing-care-plans` bukan celah. Ditinjau ulang bila Nursing authority ditunjuk | — | — | — |
| `IGD-OQ-143` (26 Oktober 2026) | Empat endpoint daftar `ClinicalManagement` (`patient-assessments`, `patient-vital-signs`, `patient-integrated-progress-notes`, `patient-procedures`) menjawab **tanpa penyaring pasien**; pembatasan per pasien hanya ditegakkan layar (`IGD-FACT-237`). Diserahkan lewat [laporan `FE-IGD-025`](task/report/frontend/FE-IGD-025.md), tidak diperbaiki | **Pemilik `ClinicalManagement`** (belum dipetakan) + Security/Privacy owner (`OPEN`) | Privasi data pasien lintas modul; **tidak menahan task IGD** | Ya |
| `IGD-FACT-218` (26 Oktober 2026) | Sepuluh ruas pengkajian IGD diterima server tetapi **tidak dapat dibaca kembali dari layar** — layar IGD nol thunk detail, sedangkan ruas itu hanya ada pada `PatientAssessmentDetailResponse`. Menahan `FE-IGD-013` acceptance 1 dan bagian keempat `FE-IGD-022`. **Dikoreksi 26 Oktober petang (`IGD-FACT-229`): datanya TERSIMPAN** — 8 dari 10 ruas terbukti utuh lewat `GET patient-assessments/{id}`, dua sisanya `null` karena payload uji menyatakan `hasFallRisk: false` dan server benar mengosongkannya. Nol kehilangan data; sisa pekerjaan menyempit jadi satu thunk detail | **DITUTUP 26 Oktober petang** (`IGD-FACT-232`): thunk detail ditambahkan, tab Riwayat menampilkan ruasnya, dua panggilan detail `200`, ESLint 0 error identik baseline. Sisa `FE-IGD-013` tinggal kriteria 4 (`IGD-OQ-141`); sisa `FE-IGD-022` tinggal `painNote` | `FE-IGD-013`, `FE-IGD-022` | — |
| `IGD-OQ-137` (18 Oktober 2026, malam) | Pasien member aktif tidak dapat disiapkan lewat aplikasi: keanggotaan baru selalu _Menunggu Pembayaran_, ubah status `410`, jalur kasir belum ada (`IGD-FACT-150`) | Yoga Aji Pratama (CRM-Membership) dan owner Billing — [`IGD-REQ-005`](approval-requests/2026-10-18-permintaan-pemilik-crm-billing-aktivasi-membership.md) | Demo pasien membership 20–21 Oktober | Ya — alur IGD lain tidak menunggu |
| ~~Sandi akun loket di lingkungan uji (18 Oktober 2026, malam)~~ | **Selesai 19 Oktober 2026:** Akun loket `rendy.saputra@rsmmc.local` terverifikasi aktif dengan sandi yang sudah ada di lingkungan uji (tidak ditulis di berkas ini, `IGD-DEC-215`) (sebelumnya terkunci `LockoutEnd`), login sukses 200, dan uji ulang P1-06R `FE-IGD-067` lulus 100% | Rizki Gunawan | Uji ulang P1-06R `FE-IGD-067` (selesai) | — |
| `IGD-FACT-151` (18 Oktober 2026, malam) | Resep dengan obat tanpa tarif berakhir `500` (`PrescriptionWorkspaceService.UpsertRegularItemAsync`) | Pemilik Farmasi | Tidak menahan IGD; demo resep harus memakai obat bertarif | Ya |
| `IGD-FACT-148` (18 Oktober 2026, malam) | Penguji memakai SQL langsung dan SuperAdmin di fase T tanpa mencatatnya | Rizki Gunawan | Keabsahan putaran uji berikutnya | Saran: hapus `QUILVIAN_DEV_DB_URL` dari lingkungan penguji |
| ~~`IGD-UNK-19`~~ — sisi data (18 Oktober 2026, malam) | **Terpenuhi untuk dev hari yang sama:** item `PR-RSMMC-00025` dan tarif `TF-RSMMC-00016` Rp150.000 (nilai uji, dikonfirmasi owner Billing) dibuat lewat aplikasi; konfigurasi di user-secrets laptop. PC kantor dan lingkungan lain perlu diisi sendiri | Owner Billing (harga akhir) | — | — |
| `IGD-REQ-004` (rincian butir 5 `IGD-REQ-003`, 18 Oktober 2026, malam) | Urutan Antrean Transfer CITO → member prioritas → waktu, dengan lencana tier (`IGD-DEC-327`); dokumen siap, dikirim Rizki | Muhammad Hamzah (Rawat Inap); dikirim Rizki | `FR-IGD-124`; langkah 6 demo 20 Oktober | Ya — jalur IGD tidak menunggu |
| `IGD-DEC-317` (18 Oktober 2026, malam) | Diskon membership belum ada di billing mana pun, termasuk rawat jalan | Owner Billing dan owner CRM | `FR-IGD-125` | Ya — tidak dijanjikan pada demo |
| `IGD-OQ-129` (sejak 12 Oktober 2026) | Penautan invoice IGD ke invoice rawat inap; pasien transfer punya dua invoice (`IGD-FACT-142`) | Owner Billing | `FR-IGD-118` | Ya — disampaikan sebagai perilaku yang dikenal pada demo |
| ~~`IGD-OQ-134`…`136`~~ (18 Oktober 2026, malam) | **Dijawab `IGD-DEC-325`…`327` hari yang sama — tidak lagi menahan** | Rizki Gunawan | — | — |
| ~~`IGD-OQ-133`~~ (18 Oktober 2026) | **Dijawab `IGD-DEC-324` hari yang sama — tidak lagi menahan.** Delta tiga baris untuk `BE-IGD-083` masuk state `0.15.0` §11.8 dan validation `0.19.0` §14.10 | Rizki Gunawan | — | — |
| `IGD-FACT-127` (17 Oktober 2026, malam) | Akses bed petugas admisi disaring `BedManagement:UnitAssignments` di `appsettings.json` per `UserId`, bukan Akses Role; keempat entrinya milik satu pengguna, sehingga petugas admisi lain tidak melihat bed dan tidak dapat _admit_. **18 Oktober 2026:** B6 memakai opsi 2 (_admit_ oleh SuperAdmin) karena opsi 1 belum terpasang; tidak lagi menahan task IGD, tetap syarat rilis | Konfigurasi milik Rawat Inap (Muhammad Hamzah), diminta lewat `IGD-REQ-003` butir 3; untuk uji dibuka Rizki secara lokal (panduan 3C.1) | Syarat rilis R3.18.5; uji B7 dengan akun admisi | Ya |
| `IGD-OQ-117` (sejak 6 Oktober 2026) | Perluasan aturan *dokter pemberi instruksi* pada Laboratorium dan Radiologi untuk encounter IGD. **Belum pernah ada dokumen permintaannya selama 20 hari** (`IGD-FACT-233`) — diperbaiki 26 Oktober lewat [`IGD-REQ-007`](approval-requests/2026-10-26-permintaan-pemilik-lab-radiologi-dokter-pemberi-instruksi-igd.md), belum dikirim | **Yoga Aji Pratama** (Radiologi) + pemilik Laboratorium | `BE-IGD-071` ⛔, `FE-IGD-058` ⛔ | Ya — pemesanan oleh dokter tidak tertahan |
| `IGD-OQ-119` (sejak 6 Oktober 2026) | Apakah tim Gizi melayani order konsultasi gizi dari pasien IGD. **Belum pernah ada dokumen permintaannya selama 20 hari** (`IGD-FACT-233`) — diperbaiki 26 Oktober lewat [`IGD-REQ-008`](approval-requests/2026-10-26-permintaan-pemilik-gizi-konsultasi-gizi-pasien-igd.md), belum dikirim. Jawaban *tidak* pun berguna: `FE-IGD-053` ditutup sebagai dibatalkan | Pemilik Gizi | `FE-IGD-053` ⛔ | Ya |
| `IGD-OQ-132` (17 Oktober 2026) | Endpoint dan tombol _Tolak_ di Antrean Transfer, serta pemanggilan `EvaluasiPenutupanKunjunganSusulanAsync` sesudah _admit_, belum ada di repo (`IGD-FACT-123`). Pembagiannya sudah diputuskan `IGD-DEC-308`/`309`, tetapi belum ada task-nya di blueprint Rawat Inap (`IGD-FACT-129`). **Diminta lewat [`IGD-REQ-003`](approval-requests/2026-10-18-permintaan-pemilik-rawat-inap-transfer-igd.md) (18 Oktober 2026)** | **Muhammad Hamzah** (Rawat Inap) — **tindak lanjut 26 Oktober 2026** ([dokumen](approval-requests/2026-10-26-tindak-lanjut-igd-req-003-rawat-inap.md)), belum dikirim: sisi IGD kedua butirnya **sudah ada** (`EvaluasiPenutupanKunjunganSusulanAsync` `:1547`, `BatalkanFormPermintaanOlehPetugasAdmisiAsync` `:1474`) dengan **nol pemanggil** dari Rawat Inap (`IGD-FACT-235`); yang dibutuhkan satu baris pemanggilan dan satu endpoint tipis | `MVP-11` — `BE-IGD-077` acceptance 3–4; Putaran B7 bagian 2; rilis penolakan admisi | Ya — `BE-IGD-083` dan Putaran B7 bagian 1 tidak bergantung pada blocker ini |
| `BE-IGD-039` | Kewenangan unit membandingkan `DepartmentId` dengan `OrganizationUnitId` — tidak pernah benar | **Security/Privacy owner — `OPEN`, belum ditunjuk.** `IGD-DEC-135` mengizinkan remediasi teknisnya berjalan, dan **tidak** memindahkan ownership | **Lintas gelombang — dinaikkan 17 September 2026.** `MVP-6` **dan** runtime `MVP-4`/`MVP-5`: route tulis `arrive`, `accept-handover`, `order-items` menolak **semua** pengguna hari ini | Implementasi gelombang lain ya; **pembuktian lewat layar `MVP-4`/`MVP-5` tidak** — lihat *Kewenangan unit memblokir lintas gelombang* di bawah |
| `IGD-DEC-092` (sementara) | Fail-closed + jalan keluar beralasan; **jalan keluarnya tidak ada di kode — diverifikasi ulang 17 September 2026**: flag `MembutuhkanAlasan` diproduksi tetapi tidak pernah dibaca satu pun pemanggil | Security/Privacy owner | `MVP-6`; runtime `MVP-4`/`MVP-5` | Tidak untuk pembuktian layar kepergian dan serah terima |
| Pemetaan unit | `MstServiceUnit.OrganizationUnitId` 0 dari 18 unit terisi | **Master Data — `OPEN`, belum ditunjuk.** Task terpisah; `IGD-DEC-135` tidak memindahkan ownership | `MVP-6`; runtime `MVP-4`/`MVP-5` | Tidak — gerbang fail-closed menyala lebih dulu, sebelum cacat perbandingan identitas sempat tercapai |
| `ActAsRadiologist` | Hasil bacaan radiologi belum dapat dirilis siapa pun (`FE-RAD-11`) | Yoga Aji Pratama — pemilik Radiologi | Penyambungan pemesanan radiologi IGD (`IGD-DEC-111`) | Ya — perbaikan teks layar tidak menunggu |
| `IGD-DEC-100`…`102` | Sikap pesanan, pesanan lab manual, penerimaan per pesanan masih `draft` | Clinical Governance, Nursing authority, pemilik Laboratorium | Butir 10 DoD `EPIC IGD-07` | Ya |
| `IGD-OQ-083` | Tempat menyimpan alasan pembatalan observasi | Product/Domain Owner IGD | Bagian `Cancelled` dari `IGD-DEC-115` | Ya — bagian `Completed` tidak tertahan |
| ~~Amandemen kontrak `IGD-DEC-183`…`185`~~ | **Ditutup 3 Oktober 2026 (petang) — `IGD-DEC-186`**: validation `0.11.0`, state `0.8.0`, API `0.14.0` `approved`; kartu diperluas; source `BE-IGD-061` dikerjakan. *Isi lama:* Keputusan observasi Dieskalasi (3 Oktober 2026) belum diturunkan ke validation §11, state §9, dan API §9.2. Sampai disetujui, kartu `BE-IGD-061` belum boleh diperluas dan kode tidak boleh diubah (`IGD-DEC-170`: task yang menemukan selisih berhenti dan melapor) | Agent (`design-business-module`) → approval Rizki Gunawan | `MVP-8` — penyelesaian `BE-IGD-061` kriteria 8 | Ya — `FE-IGD-041` dan pekerjaan lain tidak tertahan |
| `IGD-OQ-112` | Jumlah observasi `Escalated` yang sudah tertinggal pada kunjungan `Completed`/`Cancelled`, per lingkungan. Dev: sedikitnya satu (kunjungan uji S6). Agent dilarang menjalankan kueri | Rizki | Tidak menahan — angkanya menentukan perlu tidaknya tindak lanjut operasional (`IGD-DEC-184`) | Ya |
| ~~`EPIC IGD-04`~~ | ~~Riwayat penugasan dokter belum punya task~~ — **ditutup 15 September 2026**: dipecah menjadi `BE-IGD-044`, `BE-IGD-045`, `FE-IGD-027` (`IGD-DEC-116`, `IGD-DEC-117`) | — | `MVP-5` | — |
| ~~`IGD-OQ-092`~~ | ~~Pelaku (`AssignedByUserId`) pada pengisian data lama `EmgDoctorAssignment`~~ — **dijawab `IGD-DEC-136` (18 September 2026)**: pelaku boleh `NULL` hanya untuk baris legacy. **Rekonsiliasi schema disetujui pemilik 21 September 2026. Ditutup penuh:** migration `BackfillEmergencyDoctorAssignment` diterapkan ke dev dan terbukti di salinan basis data terpisah | — | **`BE-IGD-048`** ✅ ([laporan](task/report/backend/BE-IGD-048.md)) | Ya — `BE-IGD-045` sudah dikerjakan tanpa menunggu ini |
| OWNER DATA CONFIRMATION | `BE-IGD-042` menunggu jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`; agent dilarang menjalankan kueri | Rizki | R3.8 | Ya — `BE-IGD-040`, `041`, `043` tidak tertahan |
| ~~`IGD-DEC-082`~~ | ~~Riwayat penugasan dokter masih `draft` klinis~~ — **ditutup 17 September 2026**: `approved` oleh Rizki Gunawan sebagai Product/Domain Owner. *Catatan: approver yang disebut keputusan itu adalah Clinical Governance, dan peran itu masih `OPEN`; pola approval mengikuti `IGD-DEC-107`. Bila Clinical Governance kelak ditunjuk, keputusan ini wajib ditinjau ulang.* | — | Butir 10 DoD `EPIC IGD-04` — **tidak lagi tertahan** | — |
| ~~Audit Observasi V1–V2~~ | **Ditutup 16 September 2026** — `IGD-OQ-084`…`088` dijawab menjadi `IGD-DEC-122`…`126`; kontrak API dan validation naik ke `0.6.0`; `BE-IGD-046` dan `FE-IGD-028` dialokasikan ([evidence](evidence/2026-09-15-audit-observasi-v1-v2.md)) | — | R3.9, R3.7 frontend | — |
| Bentuk terstruktur alat jalan napas | `IGD-OQ-089` — OPA, NPA, ETT, LMA, stoma, bantuan napas bertekanan belum punya pemilik dan entity | Clinical Governance + Product/Domain Owner IGD | Pelaporan alat jalan napas | Ya — `BE-IGD-046` dan `FE-IGD-028` tidak tertahan |
| Entri susulan setelah periode observasi ditutup | `IGD-OQ-090` — jalur addendum belum dirancang; dilarang menumpang `BE-IGD-046` | Product/Domain Owner IGD + Nursing authority | Dokumentasi susulan observasi | Ya |
| ~~Penyelarasan teks kontrak~~ | ~~API §3, validation §1 aturan 2 dan §6 aturan 4, kamus data §4 belum mengikuti `IGD-DEC-116`…`120`~~ — **ditutup 15 September 2026**: API dan validation naik ke `0.5.0`, nama `EmgDoctorAssignment` diselaraskan, hash dihitung ulang (manifest bagian 0c) | — | R3.8, `MVP-5` | — |
| ~~`BE-IGD-049` → `FE-IGD-017`~~ | ~~Nama pelaku pada event kepergian belum ada di respons~~ — **blokir dicabut 21 September 2026 (malam)**: `BE-IGD-049` 🟡 Implementation Complete + **Build Verified** (`0 Error(s)`, 207 warning), kontrak `0.9.0` ([laporan](task/report/backend/BE-IGD-049.md)). Syarat pemilik (*kontrak selesai dan build verified*) terpenuhi. **Ditutup penuh 22 September 2026:** `BE-IGD-049` ✅ (uji API S2–S5 dilaporkan lulus pemilik) dan `FE-IGD-017` ✅ (build dinyatakan lulus pemilik) | — | `MVP-4` | Ya |
| `BE-IGD-050` → `FE-IGD-034` → `FE-IGD-014` | Pendaftaran IGD ganda meninggalkan **encounter yatim**: `POST patient-encounters` (Registrasi) commit lebih dulu, validasi duplikat baru di `POST emergency-visits` (`IGD-DEC-138`, `approved` 21 September 2026). *(Riwayat: `FE-IGD-014` belum boleh ✅ end-to-end — **kini dinilai ulang ✅ dengan celah `IGD-OQ-093` dinyatakan**; blokir ini menyisakan `IGD-OQ-093` saja.)* **Diperbarui 21 September 2026 (malam):** `BE-IGD-050` ✅ **atas penilaian pemilik** (tiga berkas source, kontrak API `0.10.0`, validation `0.7.0`; [laporan](task/report/backend/BE-IGD-050.md)) — pemilik membangun dan menjalankan uji API S1–S7, **ketujuhnya `PASS`** (pernyataan pemilik; angka tidak dilampirkan). Syarat pemilik untuk `FE-IGD-034` (*kontrak selesai dan build terverifikasi*) **terpenuhi**; `FE-IGD-034` ✅ **atas penilaian pemilik, 21 September 2026 (malam)** — lima berkas frontend (commit `c941012ac`), eslint 0 error, unit test berkas task 14/14 ([laporan](task/report/frontend/FE-IGD-034.md)); build dan uji layar dilaporkan lulus oleh pemilik. **Dikecualikan (`BE-IGD-050`):** hasil kueri audit encounter yatim A/B (acceptance 8) belum dilaporkan — nol pembersihan dilakukan | Backend IGD + Frontend; mekanisme lapis A **diputuskan pemilik** 21 September 2026 (malam); `IGD-OQ-093` tetap `open` | `MVP-2` | Ya — build dan uji layar kriteria 2 `FE-IGD-014` yang sudah ada tidak tertahan |
| ~~`IGD-OQ-093`~~ | **`superseded` penuh — realisasi `BE-IGD-053` ✅ terbukti 1 Oktober 2026** (S4 `[200, 409]`, S6 `409`); decision log diselaraskan 3 Oktober 2026. *Isi lama:* Jaminan sisi server untuk encounter yatim — **`superseded` sebagian (22 September 2026)**: bisnis dijawab `IGD-DEC-139` (bentuk B1); mekanisme teknis dijawab `IGD-DEC-145` (penyimpanan override) + `IGD-DEC-146` (penguncian per pasien); **realisasi masih terbuka** sampai **`BE-IGD-053`** ✅. *Tulisan "ditutup" pagi hari dikoreksi — tidak sesuai kosakata status repo* | Backend IGD | `MVP-2` | Ya |
| Kueri D | Jumlah encounter `Emergency` belum berakhir per kelas K1–K4 (kelas **disahkan** `IGD-DEC-148`). **Diperbarui 22 September 2026 (penutup): bukan lagi blocker pengembangan** — dibutuhkan acceptance 1 `BE-IGD-052` dan eksekusi rekonsiliasi di tiap lingkungan ([evidence](evidence/2026-09-22-desain-encounter-first.md) §7.1). Agent dilarang menjalankan kueri | Rizki | R3.13 — acceptance `BE-IGD-052`; eksekusi sebelum `BE-IGD-053` dirilis | Ya — seluruh task boleh dikembangkan |
| ~~`IGD-OQ-102`, `IGD-OQ-103` (+ kueri E1–E3)~~ — **dijawab 11 Oktober 2026** (`IGD-DEC-237`…`239`); E1–E3 diganti `IGD-UNK-18` (syarat rilis, bukan pengembangan) | Sumber roster dokter jaga dan kriteria layak (E1–E3 bagian dari `IGD-OQ-102`); tempat penanda override | Product/Domain Owner IGD + pemilik jadwal dokter/HR (belum dipetakan); angka E1–E3: Rizki | R3.13 `EPIC IGD-12` — `BE-IGD-056` → `FE-IGD-037` | Ya — `EPIC IGD-11` tidak tertahan |
| ~~`IGD-OQ-108`~~ | **Dijawab `IGD-DEC-180` (3 Oktober 2026): kontrak berlaku** — satu kunjungan berjalan per pasien; keterbatasan pasien kembali saat kunjungan lama tertahan `BE-IGD-039` diterima. *Isi lama:* Encounter kedua hasil override belum dapat Mulai Triage selama kunjungan lama berjalan (validation §10.2 aturan 7) — kontrak berlaku sampai diubah | Product/Domain Owner IGD | Tidak menahan; sebaiknya dijawab sebelum `FE-IGD-036` dirilis | Ya |
| ~~`IGD-OQ-094` (W1)~~ | ~~Sumber waktu tiba~~ — **dijawab `IGD-DEC-147`** (perawat triage, prefill `RegisteredAt`, penanda konfirmasi di `EmgVisit`) | Product/Domain Owner IGD + Nursing authority (`OPEN`) | `BE-IGD-055` → `FE-IGD-036` | Ya |
| ~~`IGD-OQ-095`~~ | ~~Bentuk `BE-IGD-052`~~ — **dijawab `IGD-DEC-148`** (endpoint admin preview + execute; K1–K4 disahkan) | Product/Domain Owner IGD | `BE-IGD-052` | Ya |
| ~~`IGD-OQ-096`~~ | ~~Pasien pergi sebelum ditriage~~ — **dijawab `IGD-DEC-142`** (`NoShow` oleh perawat triage, final, tidak ditagih) | Product/Domain Owner IGD + Nursing authority (`OPEN`) | **`BE-IGD-053`** | Ya |
| ~~`IGD-OQ-100`~~ | ~~Tangani Segera tanpa kunjungan~~ — **dijawab `IGD-DEC-143`** (lahir langsung `InTreatment`, nol ketikan, Tangani Segera menang) | Product/Domain Owner IGD | `BE-IGD-055` → `FE-IGD-036` | Ya |
| ~~`IGD-OQ-101`~~ | ~~Jalan keluar beralasan dan serentak~~ — **dijawab `IGD-DEC-145`** (entity override milik IGD, tanpa ruas di tabel Registrasi) + **`IGD-DEC-146`** (advisory lock per pasien) | Product/Domain Owner IGD + pemilik Registrasi (belum dipetakan) | **`BE-IGD-053`** | Ya |
| `IGD-OQ-097`…`099` | ~~`097` `TrxQueue`~~ **dijawab `IGD-DEC-144`** (tanpa `TrxQueue`, dijamin kode); `098` pemilik Master Patient — **dipersempit** ke merge rekam + U2 (`IGD-DEC-149`); `099` Clinical Governance — tetap `open`, **tidak menahan** (`IGD-DEC-150`) | Lihat `00-interview-decisions.md` | Cakupan `BE-IGD-051`/`054`; U2; pengesahan klinis override | Ya — tidak menahan remediasi teknis (`IGD-DEC-135`) |

## Dua observasi dari verifikasi runtime `BE-IGD-045` — 17 September 2026

Dilaporkan pemilik saat menjalankan dua belas skenario. Keduanya **dicatat, tidak diperbaiki**,
dan tidak memperluas lingkup task mana pun.

| # | Observasi | Hasil audit | Tindakan |
| ---: | --- | --- | --- |
| 1 | Respons `201` membawa `statusCode: 200` pada badan responsnya | **Pola baseline aplikasi.** `ApiResponse<T>.Ok(...)` punya satu overload dan menghardcode `StatusCode = 200`; `Status201Created` dipakai luas di `AccountingManagement` dan `HumanResource` dengan wrapper yang sama. `BE-IGD-045` tidak memperkenalkannya dan tidak menyimpang dari modul lain | **Tidak diubah.** Memperbaikinya menyentuh `ApiResponse<T>` seluruh aplikasi — refactor lintas modul yang menuntut keputusan tersendiri |
| 2 | Satu permintaan `/active` tercatat lambat sekitar 38 detik; yang lain jauh lebih cepat | Kejadian tunggal tanpa pengulangan. Kandidat yang **belum diuji**: kompilasi kueri EF pertama pada proses baru, atau jeda koneksi ke basis data pengembangan yang berada di server remote | **Observasi saja.** Layak ID task tersendiri hanya bila dapat direproduksi pada permintaan yang sudah panas |

Rinciannya di [evidence/2026-09-17-verifikasi-runtime-be-igd-045.md](evidence/2026-09-17-verifikasi-runtime-be-igd-045.md) bagian C.

## Izin remediasi teknis 17 September 2026 — `IGD-DEC-135`

**Dikoreksi pada hari yang sama.** Rumusan pertama catatan ini menulis bahwa approval empat peran
kosong "dipegang" Product/Domain Owner dan sempat memindahkan pemilik `BE-IGD-039` menjadi
Backend IGD. Itu salah baca dan sudah dicabut.

Yang sebenarnya diberikan: **izin melanjutkan remediasi teknis** selama Security/Privacy owner,
Master Data owner, Nursing authority, dan Clinical Governance belum ditunjuk. Task tidak lagi
berhenti semata-mata karena peran itu kosong.

| Yang dibuka | Yang tetap |
| --- | --- |
| Pekerjaan perbaikan yang sesuai kontrak terkunci boleh berjalan | **Ownership keempat domain tetap `OPEN`** dan melekat pada pemilik domainnya |
| Butir 10 DoD tidak dinyatakan terbuka hanya karena peran itu kosong | Keputusan kebijakan yang menjadi wewenang mereka tetap milik mereka |
| — | `BE-IGD-039` tetap cacat kode; pemiliknya tetap Security/Privacy owner |
| — | Pemetaan 18 unit tetap pekerjaan data; pemiliknya tetap Master Data |

`BE-IGD-039` dan pemetaan unit tetap **dua task terpisah**, dan keduanya tidak mengubah
ownership domain mana pun.

**Wajib ditinjau ulang** bila salah satu peran itu kelak diisi: `IGD-DEC-082`, `IGD-DEC-135`,
dan setiap keputusan yang disahkan lewat keduanya.

## Gerbang urutan delivery pemilik — keadaan 17 September 2026

Urutan yang ditetapkan pemilik 16 September 2026 menuntut butir 1 dan 2 lulus sebelum
`EPIC IGD-04` dimulai.

| Butir | Isi | Keadaan 17 September 2026 |
| ---: | --- | --- |
| 1 | `FE-IGD-029` terbukti lewat layar | ✅ **LUNAS** — pasien `RAYYAN DHAFIR PRASETYA MAULANA` didaftarkan 09.35, lahir "Menunggu Triage", triage tersimpan |
| 2 | `BE-IGD-041` terverifikasi | 🟡 **SEBAGIAN, dan sisanya tidak dapat dilunasi siapa pun** — lihat di bawah |

### Mengapa butir 2 tidak dapat dilunasi

Ketiga skenario uji `BE-IGD-041` memakai **pesanan berstatus `Rejected`**:
`AmbilPesananPenahanPenutupanAsync` menyaring tepat `AcceptanceStatus == Rejected`.

Satu-satunya jalan menjadikan pesanan `Rejected` adalah
`EmergencyDepartureService.UbahStatusPenerimaanAsync`, dan jalur itu memanggil
`EmergencyUnitAuthorityService.PeriksaAsync` — yang hari ini **menolak semua orang** karena
`BE-IGD-039` beserta pemetaan unit 0 dari 18.

| Skenario | Keadaan | Dapat diuji? |
| ---: | --- | --- |
| 1 | Kunjungan `Disposed`, nol pesanan ditolak → `200` | ✅ Ya |
| 2 | Dua pesanan ditolak → `409` menyebut keduanya | ⛔ **Tidak** — pesanan tidak dapat dijadikan `Rejected` |
| 3 | Tujuh pesanan ditolak → lima uraian + "dan 2 lainnya" | ⛔ **Tidak** — sebab yang sama |

Menyuntik baris `Rejected` langsung lewat SQL **dilarang** — itu memaksa lulus tanpa jalur
aplikasi yang sah.

**Kesimpulan yang menentukan langkah berikutnya.** Butir 2 tertahan `BE-IGD-039`, bukan tertahan
pekerjaan tim. Menunggunya berarti menunggu penunjukan Security/Privacy owner yang belum ada.
`EPIC IGD-04` karena itu **boleh dimulai sekarang** — dan memang tidak bergantung pada butir 2
sama sekali.

## Uji layar pemilik 17 September 2026 — empat task ditutup, satu cacat alur ditemukan

Uji lewat layar **kedua** yang pernah dijalankan pada modul ini, oleh Product/Domain Owner.
Bukan UAT.

### Yang lulus

| Task | Yang dibuktikan |
| --- | --- |
| `FE-IGD-031` ✅ | Segmen Formulir/Riwayat pada tab pemeriksaan |
| `FE-IGD-032` ✅ | Pemilih periode, primary survey diringkas satu baris, lembar pemantauan berbentuk tabel berjajar, segmen Lembar/Catat. **Dua delta bagian 8 diterima pemilik** |
| `FE-IGD-023` ✅ | Tab Penunjang Medis |
| `FE-IGD-024` ✅ | Satu periode observasi diselesaikan beserta isian Kesimpulan |
| `BE-IGD-046` / `FE-IGD-028` | Satu putaran pemantauan bertanda vital tercatat; penanda *Tanda Vital Kritis* menyala |

### Cacat alur yang ditemukan — `FE-IGD-033`

Sesudah penilaian triage tersimpan, bagian pilih dokter muncul, tetapi tombol **Simpan
Pemeriksaan** tetap menyala dan tetap mengirim permintaan **penilaian baru**. Pemilik
menekannya lagi sesudah memilih dokter — kesimpulan yang wajar — dan menerima `409`.

**Konsekuensi yang menentukan urutan rilis.** Sesudah `BE-IGD-047`, penekanan kedua itu tidak
lagi gagal: ia **berhasil**, dan menambahkan penilaian kedua ke riwayat klinis pasien secara
senyap. Memperbaiki backend saja mengubah galat berisik menjadi catatan klinis ganda.
**`BE-IGD-047` dan `FE-IGD-033` wajib dirilis bersama.**

## Cacat nomor urut penilaian triage — ditemukan uji layar 17 September 2026

**Setiap kunjungan IGD hanya pernah bisa menyimpan satu penilaian triage.** Penilaian kedua
selalu ditolak `409`. Ditemukan Product/Domain Owner saat menjalankan butir 2 urutan delivery
(uji layar `FE-IGD-029`), dan ditangani sebagai `BE-IGD-047` gelombang R3.10
([laporan](task/report/backend/BE-IGD-047.md)).

`CreateEmergencyTriageRequest.Sequence` berbawaan `1`, sementara controller memakai
`request.Sequence > 0 ? request.Sequence : hitung()`. Penghitungan nomor urut di server karena
itu **tidak pernah dijalankan sekali pun**, dan setiap penilaian memakai nomor urut `1` —
menabrak `IX_EmgTriage_EmergencyVisitId_Sequence` mulai penilaian kedua.

Cacat ini ada sejak endpoint-nya dibuat dan **bukan** akibat gelombang mana pun yang lalu. Ia
tidak terlihat karena uji lewat layar pada modul ini baru dimulai 16 September 2026 — bukti
langsung bahwa ✅ berbasis pemetaan source tidak menggantikan uji runtime.

Source sudah diperbaiki 17 September 2026; `dotnet build` dan uji API tiga skenario **belum
dijalankan** dan menjadi syarat ✅ task itu.

## Kewenangan unit memblokir lintas gelombang — dinaikkan 17 September 2026

Dicatat saat review modul atas permintaan pemilik. **Nol source diubah, nol keputusan baru.**
Yang berubah hanya klasifikasi dampak `BE-IGD-039`: sebelumnya ditulis hanya menahan `MVP-6`.

### Apa yang dibaca di source

`EmergencyUnitAuthorityService.PeriksaAsync` punya dua lapis. Lapis pertama adalah gerbang
fail-closed `IGD-DEC-092` untuk unit yang belum dipetakan; lapis kedua barulah perbandingan
`ApplicationUserOrganization.DepartmentId` dengan `MstServiceUnit.OrganizationUnitId` yang
memang salah domain identitas.

Karena `MstServiceUnit.OrganizationUnitId` terisi **0 dari 18**, setiap panggilan berhenti di
lapis pertama dan **lapis kedua tidak pernah tercapai**. Artinya cacat perbandingan identitas
belum pernah benar-benar dieksekusi — memperbaikinya saja tidak membuka apa pun selama
pemetaan unit masih kosong. Keduanya harus selesai bersamaan.

### Jalan keluar beralasan `IGD-DEC-092` tidak ada pemakainya

Lapis pertama mengembalikan `Hasil(Berwenang: false, MembutuhkanAlasan: true, …)`. Flag
`MembutuhkanAlasan` itulah jalan keluar beralasan yang dijanjikan `IGD-DEC-092`.

**Tidak satu pun pemanggil membacanya.** Keempat titik panggil pada
`EmergencyDepartureService` — `arrive`, `accept-handover`, sikap pesanan, dan pemeriksaan unit
asal — hanya memeriksa `!authority.Berwenang` lalu mengembalikan `403`. Tidak ada jalur yang
menawarkan pengisian alasan, dan tidak ada tempat alasan itu disimpan.

### Akibat yang perlu dibaca pemilik

| Yang tertahan | Requirement | Sebelumnya ditulis |
| --- | --- | --- |
| `arrive` — kedatangan memindahkan pemilik klinis | `FR-IGD-023`, `024`, `025`, `027` | `MVP-4` 🟡 "uji `RENAME` balik belum" |
| `accept-handover` — peninjauan dokumen serah terima | `EPIC IGD-05` | sama |
| Sikap pesanan pada penutupan kunjungan | `FR-IGD-045`, `047`…`051` | `MVP-5` 🟡 |
| Seluruh `EPIC IGD-08` | `FR-IGD-053`…`059` | `MVP-6` ⛔ — **ini yang sudah benar** |

`MVP-4` dan `MVP-5` **tetap** 🟡, bukan ⛔: implementasinya ada dan boleh dilanjutkan. Yang
tidak dapat dilakukan adalah **membuktikannya lewat layar** sampai pemetaan unit terisi dan
jalan keluar beralasan dibangun. Karena itu keduanya tidak akan pernah mencapai ✅ lewat jalur
uji layar selama blocker ini terbuka.

### Yang diminta dari luar tim IGD

Satu permintaan, dua penerima, dan keduanya harus turun bersamaan:

1. **Penunjukan Security/Privacy owner** — untuk mengesahkan bentuk jalan keluar beralasan
   (siapa boleh memakainya, apa yang disimpan, bagaimana diaudit).
2. **Master Data mengisi `MstServiceUnit.OrganizationUnitId`** untuk 18 unit.

Tanpa keduanya, `BE-IGD-039` tidak dapat diselesaikan, dan `MVP-4`, `MVP-5`, serta `MVP-6`
sama-sama berhenti di pembuktian.

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |
| Bukti mentah Putaran B3 (`test-with-agy/igd/uji-putaran-b3-20261016/`, log backend 16 Oktober 2026) | PC kantor | Tidak ada di laptop ini | Diterima atas penilaian pemilik (`IGD-DEC-319`); periksa bila dibawa ke repo. Laporan B3 tidak memuat B3-07…10 (`IGD-FACT-124`) |
| `01-existing-capability-map.md` revision `3` + suplemen `3.1` | `f69e9e48` / `300922c` | `0d13f3a8` (671 commit) | **Impact scan terbatas dikerjakan 22 September 2026 → suplemen `3.2`** untuk klaster encounter-first (`IGD-DEC-139`…`150`): 13 kemampuan lama dinilai ulang, 15 kemampuan baru `IGD-CAP-51`…`65`, conflict `IGD-CONF-06`…`08`, unknown `IGD-UNK-06`…`11`, pertanyaan penutup `IGD-TRQ-08`…`11`. **Area lain revisi 3 tetap stale** (pengkajian, penunjang, obat, kepergian, kewenangan unit). **Suplemen 3.5** (11 Oktober 2026) mencakup pemilih DPJP/dokter jaga pada `c7e523fd` / `3335f4b53` |
| ~~`blueprint-manifest.md` `artifact_hashes`~~ | 24 Agustus 2026 | 15 September 2026 | **Ditutup** — dihitung ulang pada pass penyelarasan teks kontrak (manifest bagian 0c) |
| Angka test pada laporan `BE-IGD-018`…`038` | `761 total, 759 lulus` (27 Agt) | Proyek test dihapus 11 Sep | Tidak dapat diulang; sah sebagai bukti historis (`IGD-DEC-110`) |
| ~~Penerapan migration `20260910031500` (rename encounter/resep, milik tim Registrasi)~~ | — | — | **Tidak lagi usang — dikoreksi 15 September 2026.** Migration **sudah diterapkan** pada `QuilvianNewDevRizki`. Bukti dari owner: Rizki menjalankan `dotnet ef migrations list --no-build` pada 15 September 2026, dan tidak ada migration berlabel `Pending`. Agent tidak menjalankan perintah basis data apa pun |

## Next recommended task

> **Diperbarui 18 Oktober 2026 (sore) — uji `Down()` `BE-IGD-072` lulus; `MVP-11` 9 dari 10.**
>
> 1. Muhammad Hamzah: panggilan IGD-REQ-003 butir 1 (rencana Senin 19 Oktober 2026); sesudahnya Putaran B7 bagian 2 untuk `BE-IGD-077` #4 dan `AT-IGD-240`. Tombol _Tolak_ (butir 2) untuk acceptance 3.
> 2. Sesudah `BE-IGD-077` ✅: `verify-module-readiness` untuk `MVP-11` (prompt A.8).
> 3. Catatan rilis: lingkungan baru yang dibangun dari basis data kosong butuh `max_locks_per_transaction` lebih dari 64 untuk EF Core 9 (`IGD-FACT-136`).

> **Diperbarui 18 Oktober 2026 (sore) — Putaran B7 bagian 1 lulus, `BE-IGD-083` ✅.** *Butir 1 dan 3 selesai sore harinya.*
>
> 1. Rizki: tambahkan operasi `InpatientEpisode:Create` ke entri opsi 1 — `dotnet user-secrets set "BedManagement:UnitAssignments:4:Operations:4" "InpatientEpisode:Create"`, lalu jalankan ulang backend — supaya putaran berikutnya dapat meng-_admit_ dengan akun admisi (`IGD-FACT-134`).
> 2. Rizki: pastikan `IGD-REQ-003` sampai ke Muhammad Hamzah. Sesudah panggilan Rawat Inap terpasang: Putaran B7 bagian 2 (B7-06, B7-07) untuk `BE-IGD-077` #4 dan `AT-IGD-240`.
> 3. Paralel: uji `Down()` `BE-IGD-072` (prompt A.6) — task IGD terakhir di `MVP-11` yang tidak bergantung pada Rawat Inap.
> 4. Kandidat frontend (belum direncanakan): header _Informasi Pasien_ dimuat ulang sesudah aksi kepergian supaya lencana tidak basi (`IGD-FACT-135`).

> **Diperbarui 18 Oktober 2026 (sore) — `BE-IGD-083` dikerjakan ([laporan](task/report/backend/BE-IGD-083.md)).**
>
> 1. ~~Rizki: build backend~~ — **selesai** 18 Oktober 2026 10.51: 0 error, 286 warning (baseline). Jalankan ulang backend dari DLL ini sebelum B7.
> 2. Antigravity: Putaran B7 bagian 1 dengan prompt A.1d panduan uji sisa.
> 3. Agent: penilaian B7 dari bukti mentah; `BE-IGD-083` ✅ bila lulus.
> 4. Sesudah panggilan Rawat Inap terpasang (`IGD-REQ-003`): Putaran B7 bagian 2.

> **Diperbarui 18 Oktober 2026 (siang, sesudah persetujuan `IGD-DEC-324`).** *Butir 1 selesai sore harinya.*
>
> 1. Agent: `build-module-backend` `BE-IGD-083` (kode saja; tanpa build, migration, atau commit).
> 2. Rizki: build backend; Antigravity: Putaran B7 bagian 1 dengan prompt A.1d; penilaian dengan prompt A.7 (B5 diganti B7).
> 3. Rizki: pastikan `IGD-REQ-003` sampai ke Muhammad Hamzah. Sesudah panggilan Rawat Inap terpasang: Putaran B7 bagian 2.
> 4. Paralel: uji `Down()` `BE-IGD-072` (prompt A.6).

> **Diperbarui 18 Oktober 2026 (siang) — status B6 dicatat, kartu `BE-IGD-083` disusun.** *Butir 1 dan 2 selesai: `IGD-DEC-324`.* Baca dulu [penilaian B6 dan keselarasan](evidence/2026-10-18-penilaian-b6-dan-keselarasan-rawat-inap.md) bagian 7 dan 9.**
>
> 1. Rizki: putuskan `IGD-OQ-133` — delta tiga baris matriks state di kartu `BE-IGD-083` ([roadmap backend](roadmap/backend-roadmap.md)). Lisan di chat cukup.
> 2. Agent: sesudah disetujui, masukkan delta ke kontrak (state `0.15.0` §11.8, validation `0.19.0` §14.10), catat keputusannya, buka kartu ⛔ → siap.
> 3. Agent: `build-module-backend` `BE-IGD-083`; Rizki membuild backend.
> 4. Antigravity: Putaran B7 bagian 1 dengan prompt A.1d panduan uji sisa; penilaian dengan prompt A.7 (B5 diganti B7).
> 5. Rizki: kirim `IGD-REQ-003` ke Muhammad Hamzah bila belum. Prompt blueprint Rawat Inap dijalankan di sesi pemilik Rawat Inap (bagian 4 berkas itu), bukan di sesi IGD. Sesudah panggilan Rawat Inap terpasang: Putaran B7 bagian 2.
> 6. Paralel: uji `Down()` `BE-IGD-072` (prompt A.6).

> **Diperbarui 18 Oktober 2026 — sesudah Putaran B6. Baca dulu [penilaian B6 dan keselarasan](evidence/2026-10-18-penilaian-b6-dan-keselarasan-rawat-inap.md) (rujukan tetap; batas agent di bagian 7).** *Butir 1 dan 2 dikerjakan siang harinya.*
>
> 1. Agent: catat status hasil B6 (berkas penilaian bagian 8) — `BE-IGD-074` ✅, `FE-IGD-061` ✅, `BE-IGD-077` tetap 🟡 dengan cacat source; register 84 dari 121.
> 2. Agent: susun kartu `BE-IGD-083` lewat `plan-module-delivery`; delta satu baris matriks state menunggu persetujuan Rizki.
> 3. Rizki: kirim [`IGD-REQ-003`](approval-requests/2026-10-18-permintaan-pemilik-rawat-inap-transfer-igd.md) ke Muhammad Hamzah; bagian 4 berkas itu ditujukan untuk agent Claude di sisi Rawat Inap.
> 4. Sesudah `BE-IGD-083` dan panggilan Rawat Inap: Putaran B7 (alur normal dan alur pasien-tiba-dulu).
> 5. Perbaikan wajib sejalur dengan blueprint Rawat Inap (`IGD-DEC-323`).

> **Diperbarui 17 Oktober 2026 (malam) — sesudah Putaran B5 (`IGD-FACT-126`).**
>
> 1. Rizki: buka akses bed akun admisi lewat `dotnet user-secrets` atau `appsettings.Development.json` (opsi 1, panduan 3C.1), atau pilih opsi 2 (_admit_ oleh SuperAdmin).
> 2. Antigravity: Putaran B6 dengan prompt A.1c — menutup `BE-IGD-074` dan `FE-IGD-061` bila lulus.
> 3. Claude di mesin ber-Docker: uji `Down()` `BE-IGD-072` dengan prompt A.6.
> 4. Sesudah itu: penilaian dengan prompt A.7 (ganti B5 menjadi B6), lalu kesiapan `MVP-11` dengan prompt A.8.

> **Diperbarui 17 Oktober 2026 — status transfer ke rawat inap (`MVP-11`).**
>
> 1. Pemilik: push kode PC kantor yang dipakai Putaran B3, bila ada.
> 2. Pemilik: uji _admit_ sungguhan dari Antrean Transfer (C-12/B3-14) — menutup `BE-IGD-074` acceptance 5–6 dan
>    `FE-IGD-061` acceptance 3, 5; uji C-12 `FE-IGD-060`.
> 3. Pemilik: uji penjaga `Down()` `BE-IGD-072` atau kecualikan butir itu secara tertulis di decision log.
> 4. Muhammad Hamzah: jawab `IGD-OQ-132`; sesudah itu uji B3-07…10 untuk `BE-IGD-077` acceptance 3–4.
> 5. Agent: periksa bukti mentah uji ulang, lalu `verify-module-readiness` untuk `MVP-11`.
> 6. Seluruh uji yang tersisa untuk modul IGD — putaran B4 (transfer), D2 (Ruang Kerja Dokter), D3 (tindakan dan tindak
>    lanjut), L1 (layar lama), R1 (migration mundur) — beserta prompt Antigravity:
>    [testing/2026-10-17-panduan-uji-sisa-modul-igd.md](testing/2026-10-17-panduan-uji-sisa-modul-igd.md).

> **Diperbarui 11 Oktober 2026 — `EPIC IGD-12` dokter jaga dibuka (manifest 0o, 0o.1).**
>
> - `IGD-OQ-102` dijawab `IGD-DEC-237`: roster = jadwal `MstDoctorSchedule` aktif yang berlaku pada waktu penugasan di unit
>   `ServiceUnitType.Emergency`, tanpa pencocokan nama. `IGD-DEC-239`: dokter umum/emergency di luar jadwal selalu
>   ditawarkan dan wajib alasan. `IGD-OQ-103` dijawab `IGD-DEC-238`: kolom `IsRosterOverride` dan `RosterOverrideReason`.
>   Desain dan kontrak `approved` (`IGD-DEC-240`): API `0.16.0` §11, validation `0.15.0` §13, permission/audit `0.9.0` §10,
>   integration `0.7.0` §8; satu migration.
> - Kode pemilik 8 Oktober (`f39250df`, `905aec3bd`) menjadi titik awal: `BE-IGD-056` dan `FE-IGD-037` ⛔ → 🟡 (0 dari 9 dan
>   0 dari 8 acceptance). Register tetap: backend **41 ✅ dari 55**, frontend **27 ✅ dari 48**.
> - Unknown baru `IGD-UNK-18` (unit IGD bertipe `Emergency` dan punya jadwal di tiap lingkungan) — syarat rilis, bukan
>   syarat pengembangan.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | Izin `build-module-backend` `BE-IGD-056`; lalu migration dan build | Rizki → Agent; migration dan build oleh Rizki |
> | 2 | Izin `build-module-frontend` `FE-IGD-037` sesudah build backend terverifikasi | Rizki → Agent |
> | 3 | Data uji jadwal (backend roadmap R3.17.2) dan satu putaran Antigravity `AT-IGD-214`…`220` | Rizki; panduan dari agent |
> | 4 | Catat `IGD-UNK-18` per lingkungan; rilis `BE-IGD-056` bersama `FE-IGD-037` | Rizki / pemilik jadwal dokter |
> | 5 | Butir lama tetap berlaku: izin dan uji `MVP-9`; `IGD-OQ-117`, `IGD-OQ-119` | Lihat blok 6 Oktober di bawah |

> **Diperbarui 6 Oktober 2026 (sore, perencanaan) — kartu `MVP-9` Ruang Kerja Dokter IGD terbit; berpasangan backend → frontend.**
>
> - Backend R3.16: `BE-IGD-065`…`070` siap (gelombang 1); `BE-IGD-071` ⛔ `IGD-OQ-117`. Frontend R3.14: `FE-IGD-045`…`058` —
>   11 siap (gelombang 1–3), `FE-IGD-053` ⛔ `IGD-OQ-119`, `FE-IGD-057`/`058` ⛔ lewat `BE-IGD-071`. Nol migration, nol `Program.cs`.
> - `IGD-DEC-231` (grill-me di tengah perencanaan): *Catatan Saya* IGD memuat catatan terkunci dan addendum; draf IGD dibuka dari
>   tab Catatan Dokter — source hanya mendaftarkan draf rawat inap sejak draf (`IGD-FACT-085`, `086`). `IGD-FACT-087` (label
>   status encounter IGD berganti saat catatan dokter dibuat/diselesaikan) dicatat, bukan kartu.
> - Syarat rilis: `BE-IGD-070` bersama `FE-IGD-047` dan `FE-IGD-054`; `FE-IGD-055` bersama/sesudah `FE-IGD-054`; `BE-IGD-071`
>   bersama `FE-IGD-057` (roadmap backend R3.16.5).
> - Register: backend **41 ✅ dari 55**, frontend **27 ✅ dari 48**, gabungan **68 dari 103**. Manifest bagian **0n.1**; tabel hash 0n
>   berlaku dengan satu baris pengganti (`00-interview-decisions.md`).
> - Commit dokumen desain `e3cbc58d` sudah ada di `rizkiG` tetapi belum di-push (ahead 1); dokumen pass ini belum di-commit.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | ~~1~~ | ~~`plan-module-delivery` `MVP-9`~~ — **selesai** (manifest 0n.1) | Agent |
> | 2 | Izin `build-module-*` per task, urutan R3.16.4: `BE-IGD-065` → `FE-IGD-045`; `066` → `046`; `067` → `047`, `048`; `068` → `049`; `FE-IGD-050`, `052`; `069` → `051`, `056`; `070` → `054`, `055` | Rizki → Agent |
> | 3 | Build backend (Rizki, error dan warning dilaporkan); `npm run build` frontend (agent, cek port 3000) | Rizki / Agent |
> | 4 | Panduan uji Antigravity per putaran (putaran 1: langkah 1–4; putaran 2: langkah 5–7) — dokter `ranger.biru@admin.com`, perawat `dimas.kurniawan@rsmmc.local` | Agent → Rizki |
> | 5 | Jawaban `IGD-OQ-117` (Laboratorium, Radiologi) dan `IGD-OQ-119` (Gizi); konfigurasi `IGD-UNK-13`…`15` dan butir menu | Pemilik modul terkait, admin |
> | 6 | `verify-module-readiness` ulang untuk `MVP-8` | Agent |

> **Diperbarui 6 Oktober 2026 (sore, desain) — slice pesanan tanpa sikap selesai; desain Ruang Kerja Dokter IGD `approved` (`IGD-DEC-230`).**
>
> - Slice pesanan tanpa sikap ✅: `BE-IGD-041` (17 dari 17), `BE-IGD-064`, `FE-IGD-043`, `FE-IGD-044` — bukti putaran uji
>   bersama diterima dengan penyimpangan tercatat (`IGD-DEC-218`, `219`); di-commit pemilik (backend `43dab6da`, frontend
>   `6680278a2`). Rilis `BE-IGD-041` bersama `FE-IGD-044` milik pemilik (`IGD-DEC-204`). Register: backend **41 ✅ dari
>   48**, frontend **27 ✅ dari 34**, gabungan **68 dari 82**.
> - Arahan pemilik: layar pemeriksaan IGD adalah layar perawat; dokter mendapat layar sendiri yang mengikuti Ruang Kerja
>   Dokter Rawat Inap, dengan penunjang lima jenis. `grill-me` → `IGD-DEC-220`…`229`; impact scan → capability map
>   suplemen 3.4; desain `EPIC IGD-14` (`MVP-9`) **`approved`** (`IGD-DEC-230`) — API `0.15.0`, validation `0.14.0`, state `0.10.0`,
>   permission/audit `0.8.0`, integration `0.6.0`; nol migration (manifest bagian 0n).
> - Kesiapan `MVP-8` (`READY_WITH_CONDITIONS`, 4 Oktober) perlu dinilai ulang: alur serah terima dan sikap pesanan yang
>   dulu dikecualikan dari UAT kini selesai.
> - Data uji tertinggal di dev: tiga kunjungan percobaan 1–2 (`IGD-FACT-077`) — ditutup pemilik bila pasiennya dipakai lagi.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | ~~1~~ | ~~Approval desain `EPIC IGD-14` (manifest bagian 0n), termasuk delapan pilihan desain `02-backend-architecture.md` §15.11~~ — **selesai** (`IGD-DEC-230`, 6 Oktober 2026 sore) | Rizki |
> | 2 | `plan-module-delivery`: kartu backend dan frontend berpasangan untuk `MVP-9` | Agent |
> | 3 | Jawaban `IGD-OQ-117` (Laboratorium, Radiologi) dan `IGD-OQ-119` (Gizi); konfigurasi `IGD-UNK-13`…`15` (izin memesan darah unit IGD, `ActAsRadiologist`, butir menu) | Pemilik modul terkait, Master Data, admin |
> | 4 | `verify-module-readiness` ulang untuk `MVP-8` | Agent |

> **Diperbarui 5 Oktober 2026 (sore, sinkronisasi roadmap) — `BE-IGD-041` dibuka (⛔ → 🟡), `FE-IGD-044` gelombang 2
> (⛔ → tanpa tanda); keempat kartu siap dibangun atas izin per task.**
>
> - `BE-IGD-041` 🟡: 7 dari 17 acceptance terpetakan pada source (1–7, 16 September 2026); 8–17 dikerjakan ulang. Tanda
>   🟡, bukan tanpa tanda, karena source dan laporan tracked kartu ini sudah ada. Node amandemen ✅ di grafik R3.8 (tetap
>   11 node, 7 panah).
> - `FE-IGD-044`: tanpa tanda, gelombang 2 R3.13.2 — menunggu `FE-IGD-043` dan `BE-IGD-041` sebagai dependency task
>   biasa. `FE-IGD-043` dan `BE-IGD-064` tetap siap (gelombang 1).
> - Skenario uji: *"pola `AT-IGD-187`"* diganti `AT-IGD-198`; acceptance kepergian dibatalkan dijejak ke `AT-IGD-199`.
>   Isi acceptance tidak berubah. Traceability: baris `IGD-DEC-209` ditambahkan.
> - Impact scan: frontend di-*pull* ke `553501053` pukul 15.04 WIB (Finance, antrean dokter, `DataTable`) — nol berkas
>   IGD berubah; dampak pada kartu nihil. Register tetap BE **39/48**, FE **25/34**, gabungan **64/82**. Manifest bagian
>   **0m.1**; tabel hash 0m tetap berlaku.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | ~~1~~ | ~~`plan-module-delivery` sesudah amandemen~~ — **selesai** (manifest 0m.1) | Agent |
> | 2 | `build-module-frontend` `FE-IGD-043` → `build-module-backend` `BE-IGD-041`, `BE-IGD-064` → `build-module-frontend` `FE-IGD-044` — izin per task | Agent |
> | 3 | `dotnet build` backend (Rizki); `npm run build` frontend (agent, cek port 3000) | Rizki / Agent |
> | 4 | Satu panduan uji Antigravity untuk keempat kartu (`IGD-DEC-208`): skrip di folder bukti, setiap percobaan dilaporkan | Agent → Rizki |
> | 5 | Rilis `BE-IGD-041` + `FE-IGD-044` bersama (`IGD-DEC-204`) | Rizki |
>
> **Diperbarui 5 Oktober 2026 (sore, amandemen) — kontrak `IGD-DEC-205` `approved` (`IGD-DEC-209`); blocker `BE-IGD-041`
> gugur di kontrak, kartunya menunggu dibuka `plan-module-delivery`.**
>
> - Validation **`0.13.0`**: pesanan pada kepergian yang fisiknya dibatalkan tidak menahan penutupan (tanpa sikap maupun
>   ditolak); §5.1 mengunci arti *tanpa sikap* dan batas cakupan (pesanan yang belum pernah menjadi baris pesanan kepergian
>   tidak dihitung). State **`0.9.0`** (§6a.2, §9.2). API §1.2 butir 2 dan 4 diselaraskan tanpa kenaikan versi — rumusan
>   lama *"kepergian yang dokumennya sudah diajukan"* tidak pernah sama dengan validation yang `approved`.
> - Turunan: arsitektur §11.1 dan §14 (baris `EmergencyDepartureService`), PRD §9 `AT-IGD-198`, `199`, DoD 12, flowchart
>   langkah 3. Nol endpoint, nol schema, nol izin. Revisi tetap 8. Manifest bagian **0m** (hash decision log `e441203d…`).
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | `plan-module-delivery`: buka `BE-IGD-041` (⛔ → siap), node amandemen ✅ di grafik R3.8, gelombang `FE-IGD-044`, rujukan `AT-IGD-198`/`199` pada kartu dan traceability | Agent |
> | 2 | `build-module-frontend` `FE-IGD-043`, `FE-IGD-044`; `build-module-backend` `BE-IGD-041`, `BE-IGD-064` — izin per task | Agent |
> | 3 | Build backend (Rizki) dan frontend; satu putaran uji Antigravity (`IGD-DEC-208`) | Rizki / Agent |
>
> **Diperbarui 5 Oktober 2026 (sore, perencanaan) — `FE-IGD-043` dan `BE-IGD-064` siap; `BE-IGD-041` dan `FE-IGD-044`
> menunggu amandemen kontrak; `IGD-OQ-116` dijawab.**
>
> - `plan-module-delivery` menurunkan `IGD-DEC-202` dan `IGD-DEC-203` ke kartu, dan menemukan tiga aturan yang belum
>   diputuskan. Pemilik menjawabnya lewat `grill-me` (decision log, *Pesanan tanpa sikap dan jalan penyelesaiannya dari
>   layar*): layar sikap pesanan dibangun dan dirilis bersama `BE-IGD-041` (`IGD-DEC-204`); pesanan pada kepergian yang
>   dibatalkan tidak menahan penutupan (`IGD-DEC-205`); unit penerima *Handover* di layar = unit tujuan kepergian
>   (`IGD-DEC-206`). Dasarnya fakta source: layar tidak punya pemanggilan `order-items`, dan *Ajukan Serah Terima*
>   meninggalkan pesanan tanpa sikap setiap kali ditolak (`IGD-FACT-053`, `054`).
> - **`FE-IGD-043`** (galat aksi kepergian di dalam modal; satu berkas) — siap dikerjakan. **`BE-IGD-041`** diperluas
>   (acceptance 8–17) dan **`FE-IGD-044`** (layar sikap pesanan, acceptance 1–13) — ⛔ menunggu amandemen validation §5.1
>   dan §6 aturan 4 untuk `IGD-DEC-205`.
> - **`IGD-OQ-116` dijawab** (`IGD-DEC-207`): kartu backend **`BE-IGD-064`** — `GET /emergency-visits/{id}` memuat pasien,
>   unit, cara datang, dan jenis kasus seperti daftar kunjungan; satu berkas, nol kontrak, nol frontend; siap dikerjakan.
> - **Urutan** (`IGD-DEC-208`): amandemen dulu, lalu keempat kartu dibangun dan diuji dalam **satu** putaran Antigravity.
> - Register: BE **39/48** (`BE-IGD-041` 🟡 → ⛔; `BE-IGD-064` baru), FE **25/34**, gabungan **64/82**. Manifest bagian 0l.3.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | `design-business-module` (amendment): validation §5.1, §6 aturan 4 untuk `IGD-DEC-205`, lalu approval | Agent → Rizki |
> | 2 | `build-module-frontend` `FE-IGD-043`, `FE-IGD-044`; `build-module-backend` `BE-IGD-041`, `BE-IGD-064` — izin per task | Agent |
> | 3 | `dotnet build` backend (`BE-IGD-041`, `BE-IGD-064`); `npm run build` frontend | Rizki; agent (cek port 3000) |
> | 4 | Satu panduan uji: `039-U1`/`U2` ulang, acceptance `BE-IGD-041` + `FE-IGD-044` satu siklus, `BE-IGD-064`; skrip di folder bukti, setiap percobaan dilaporkan | Agent → Rizki (Antigravity) |
> | 5 | Rilis `BE-IGD-041` + `FE-IGD-044` bersama; lalu `verify-module-readiness` `MVP-8` ulang dan naskah UAT (C1) | Agent → Rizki |
> | terbuka | `ServiceUnit : Read` Perawat IGD (C3); sisa data uji dev `V2`, `V3`, `e1d742a1…` | Rizki |
>
> **Diperbarui 5 Oktober 2026 (sore) — `BE-IGD-039` ✅, `BE-IGD-061` ✅: `MVP-8` 6 dari 6 task ✅; syarat C2 lunas.**
>
> - `BE-IGD-039` dibangun (satu berkas, +8/−4, QBE Strict `PASS`), dibuild pemilik, lalu diuji lewat Antigravity dari
>   [panduan uji](testing/2026-10-05-panduan-uji-be-igd-039.md). Bukti mentah diperiksa terhadap log backend
>   ([laporan uji](testing/2026-10-05-laporan-uji-be-igd-039.md) bagian 7): 115 dari 115 respons cocok log. Bukti diterima
>   dengan penyimpangan tercatat — skrip tidak disimpan, dua percobaan awal tidak dilaporkan (`IGD-DEC-200`).
> - `BE-IGD-039` ✅ dengan acceptance 3, 4, 5, 6, 11 dikecualikan karena akun opsional tidak tersedia (`IGD-DEC-201`).
>   `BE-IGD-061` ✅ — serah terima dan sikap pesanan kini terbukti menutup kunjungan.
> - **Temuan:** (1) tab Transfer tidak menampilkan galat aksi kepergian → kartu frontend pasangan (`IGD-DEC-202`);
>   (2) pesanan tanpa sikap tidak menahan penutupan, melanggar `FR-IGD-051` → pengerjaan ulang `BE-IGD-041`
>   (`IGD-CONFLICT-007`, `IGD-DEC-203`); (3) kartu pasien Ruang Kerja selalu *"Pasien belum teridentifikasi"*, RM dan unit
>   kosong → `IGD-OQ-116`, belum diputuskan; (4) Perawat IGD belum memegang `ServiceUnit : Read` (C3 sebagian).
> - Register: **BE 39/47**, FE 25/32, gabungan **64/79**. Manifest bagian 0l.2.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | `plan-module-delivery`: kartu frontend pasangan `BE-IGD-039` (`IGD-DEC-202`) dan perluasan `BE-IGD-041` (`IGD-DEC-203`) | Agent |
> | 2 | Putuskan `IGD-OQ-116` (kartu pasien Ruang Kerja) | Rizki |
> | 3 | Tambah `ServiceUnit : Read` pada Perawat IGD; siapkan akun tim UAT (C3) | Rizki |
> | 4 | `verify-module-readiness` `MVP-8` ulang, lalu naskah UAT (C1) | Agent → Rizki |
> | tetap | Commit dan push; daftar rilis per lingkungan termasuk pemetaan unit dan kueri `IGD-OQ-112` (C5) | Rizki |
> | catatan | Sisa data uji di dev: kunjungan `V2`, `V3`, dan kunjungan kepergian `e1d742a1…` (percobaan awal) masih berjalan | Rizki |
>
> **Diperbarui 5 Oktober 2026 — C4 lunas; C3 dan C2 diputuskan; kontrak kewenangan unit `approved` (`IGD-DEC-199`).**
>
> - **C4 lunas.** Dokumen 4 Oktober (`d4b7fbad`, `d76279c0`) dan dua berkas `FE-IGD-042` (`57b1d360f`) sudah di-push.
> - **C3 diputuskan** lewat pass `grill-me` ([decision log](00-interview-decisions.md), *Konfigurasi peran klinis untuk UAT
>   `MVP-8`*). Dokter membuat dan mengonfirmasi tindak lanjut; perawat melaksanakannya (`IGD-DEC-190`).
>   `EmergencyDeparture : Approve` dicabut dari Perawat IGD, sehingga `IGD-OQ-113` ditutup (`IGD-DEC-191`). Naskah UAT
>   membawa kunjungan ke *Menunggu keputusan* lewat observasi dan tidak memuat `AT-IGD-195` (`IGD-DEC-192`). Temuan baru
>   `IGD-OQ-114`: pasien tanpa observasi tidak dapat ditindaklanjuti dari layar — tidak menahan UAT. Tabel izin per peran
>   ada di decision log; pengaturan Akses Role sedang dikerjakan pemilik.
> - **C2 diputuskan** lewat pass `grill-me` (*Kewenangan unit `BE-IGD-039`*): jembatan lewat simpul organisasi penugasan HR
>   (`IGD-DEC-193`); simpul sama persis (`IGD-DEC-194`); fail-closed dengan pesan dikoreksi dan jalan keluar beralasan
>   ditunda (`IGD-DEC-195`); pemetaan unit diisi pemilik lewat basis data dan pembukaan kolomnya diminta ke Master Data
>   (`IGD-DEC-196`); lingkup tetap lima tindakan, `IGD-CONFLICT-006` ditunda (`IGD-DEC-197`, `IGD-OQ-115`); peran perawat
>   rawat inap mendapat `EmergencyDeparture : Read`/`Update` (`IGD-DEC-198`).
> - **Kontrak:** validation `0.12.0` §7 dan permission/audit `0.7.0` (§3 catatan, §3.1, §6) `approved` lewat `IGD-DEC-199`;
>   tiga rujukan `IGD-OQ-071` yang basi diselaraskan; manifest bagian 0l memuat hash baru (hash decision log 0k.2 usang).
>   Nol endpoint, nol schema.
> - Snapshot: backend `rizkiG` `8d81d361` (merge integration sesudah `0078a0dc`; area IGD tidak berubah), frontend
>   `RizkiV2` `57b1d360f`. Register tidak berubah: BE 37/47, FE 25/32, gabungan 62/79.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | ~~`plan-module-delivery`: perbarui kartu `BE-IGD-039` sesuai `IGD-DEC-193`…`197`, buka ⛔, pasang acceptance 1–10 pass C2~~ — **selesai 5 Oktober 2026** (manifest 0l.1): kartu ditulis ulang, siap dikerjakan, acceptance 1–13, persiapan data P1–P5, uji ulang `BE-IGD-061` pada putaran yang sama | Agent |
> | 2 | `build-module-backend` `BE-IGD-039` — satu berkas service IGD, nol migration | Agent |
> | 3 | `dotnet build`; isi pemetaan unit dev (`IGD-DEC-196`) dan penugasan HR akun uji; uji ulang `061-S2`, `S3`, `S4`, `S12`, kaki `reject-handover` `S11` | Rizki |
> | paralel | Atur Akses Role Perawat IGD dan Dokter IGD (C3); cek `GET /auth/permissions` setiap akun UAT | Rizki |
> | lalu | Naskah UAT `MVP-8` untuk tim UAT (C1) dengan batas `IGD-DEC-192` | Agent → Rizki |
> | tetap | Kueri `IGD-OQ-112` per lingkungan dan daftar rilis (C5), termasuk pemetaan unit | Rizki |
>
> **Diperbarui 4 Oktober 2026 (sore) — bukti uji gabungan `MVP-8` diperiksa; `FE-IGD-041` ✅, `FE-IGD-042` ✅.**
> Putusan dari bukti mentah dan log backend, bukan dari ringkasan penguji
> ([laporan uji](testing/2026-10-04-laporan-uji-gabungan-mvp-8.md) bagian 7):
>
> - `BE-IGD-061` S13–S19 terbukti (13 butir; `NOT RUN`: kaki `PUT` S18 tanpa data, kaki `reject-handover` S11 opsional) —
>   acceptance 13–19 terpenuhi; **tetap 🟡** karena acceptance 2–3 (S2, S3, S12) tertahan `BE-IGD-039`.
> - `FE-IGD-041` 4 dari 4 dan `FE-IGD-042` 5 dari 5 + regresi 8 dari 8 — pertama kalinya diuji **pada hasil build**.
> - **Penyimpangan:** agen penguji mengubah Akses Role Perawat IGD lewat SuperAdmin (kedua kalinya), sandi SuperAdmin
>   tertulis di lima skrip (tidak diganti — milik lead, sudah ada di riwayat Git sejak Mei 2026; `IGD-DEC-189`), dan laporannya tidak cocok dengan bukti di lima butir. Pemilik mengesahkan izin dan menerima
>   bukti (`IGD-DEC-188`); `EmergencyDeparture : Approve` yang ikut tercatat belum disahkan (`IGD-OQ-113`).
> - `IGD-OQ-112` dev: 4 observasi Dieskalasi tertinggal sebelum uji, 3 sesudah `061-S17` langkah 4.
>
> Progres register: BE 37/47, **FE 25/32**, gabungan **62/79 = 78%** (sebelumnya FE 23/32, 60/79 = 76%).
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | ~~`verify-module-readiness` untuk `MVP-8`~~ — **selesai 4 Oktober 2026: `READY_WITH_CONDITIONS`** (siap UAT untuk alur tanpa serah terima dan sikap pesanan; belum siap produksi; DoD 10 dari 11 terpenuhi, butir 1 sebagian; kesiapan berbobot ±82%; syarat C1–C6, **C6 lunas 4 Oktober 2026** — manifest 0k.2, laporan `BE-IGD-062`/`063`, pengantar R3.13.1, snapshot R3.14; **C4 lunas 5 Oktober 2026**) — [evidence/2026-10-04-kesiapan-mvp-8.md](evidence/2026-10-04-kesiapan-mvp-8.md) | Agent |
> | 2 | Putuskan `IGD-OQ-113` (`EmergencyDeparture : Approve` pada Perawat IGD). Sandi SuperAdmin **tidak** diganti — milik lead (`IGD-DEC-189`); uji berikutnya memakai akun peran lain | Rizki |
> | 3 | Lengkapi izin baca Ruang Kerja untuk Perawat IGD (riwayat triage, tanda vital, pengkajian, jenis tindak lanjut — `403` pada uji) sebelum UAT | Rizki |
> | 4 | Kueri `IGD-OQ-112` di lingkungan selain dev | Rizki |
> | ~~tetap~~ | ~~Commit dan push: dokumen 4 Oktober (`rizkiG`), dua berkas `FE-IGD-042` (`RizkiV2`)~~ — **lunas 5 Oktober 2026 (C4):** backend `d4b7fbad`, `d76279c0`; frontend `57b1d360f`; keduanya sama dengan origin | Rizki |
> | terbuka | Kolom AKSI daftar Pengkajian terdorong keluar pada nama pasien panjang — keputusan identifikasi pasien | Rizki |
>
> **Diperbarui 4 Oktober 2026 — langkah 1–2 tabel petang dikerjakan; panduan uji gabungan siap.**
>
> - `BE-IGD-061`: `dotnet build` Rizki 3 Oktober 2026 **0 error, 0 warning** (dilaporkan 4 Oktober) — kriteria 7
>   terpenuhi; status tetap 🟡 sampai uji API S13–S19 ([laporan](task/report/backend/BE-IGD-061.md), *Build pemilik*).
>   Source dan dokumen 3 Oktober sudah di-commit pemilik: backend `c1f79f79`, frontend `19ba512de`.
> - `FE-IGD-042` dikerjakan ulang (`IGD-DEC-187`): dua berkas (+22, nol JSX/CSS) — pada kunjungan *Selesai*/*Dibatalkan*
>   *Selesaikan* dan *Eskalasi* nonaktif dengan kalimat aturan 18, *Batalkan* tetap aktif. `eslint` 0 error; uji IGD
>   91/91; `npm run build` agent lulus 11.46 (465/465, 0 warning); server standalone port 3000 dari hasil build itu.
>   Status 🟡 sampai uji layar ([laporan](task/report/frontend/FE-IGD-042.md), *Pengerjaan ulang 4 Oktober 2026*).
> - [Panduan uji gabungan `MVP-8`](testing/2026-10-04-panduan-uji-gabungan-mvp-8.md): 29 skenario — `BE-IGD-061`
>   S13–S19, `FE-IGD-041` U2/U6/U7/R, `FE-IGD-042` acceptance 10–13 + regresi U1–U8. Progres register tidak berubah
>   (BE 37/47, FE 23/32, gabungan 60/79 = 76%).
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | Jalankan panduan uji gabungan `MVP-8` lewat Antigravity (akun peran nyata, 1440 × 900, hasil build 11.46) | Rizki (Antigravity) |
> | 2 | Periksa bukti mentah per skenario; perbarui laporan ketiga task, roadmap, traceability | Agent |
> | 3 | Bila lulus: `verify-module-readiness` untuk `MVP-8` — `BE-IGD-061` S2, S3, S12 tetap tertahan `BE-IGD-039` | Agent |
> | 4 | Kueri `IGD-OQ-112` per lingkungan (angka dev ikut terekam pada langkah P7 panduan) | Rizki |
> | tetap | Commit dan push dokumen 4 Oktober (`rizkiG`) dan dua berkas `FE-IGD-042` (`RizkiV2`) | Rizki |
>
> **Diperbarui 3 Oktober 2026 (petang) — `MVP-8` dilanjutkan.** Langkah 2 tabel sore di bawah **dikerjakan**:
>
> - `FE-IGD-041` dikerjakan ulang. Penanda *Menunggu penutupan* kini tampil di bawah lencana STATUS KUNJUNGAN dan
>   kolom PENUTUPAN dihapus. Lebar kolom ditata ulang, dengan jumlah `minWidth` 1.102 piksel (sebelumnya 1.392).
>   `npm run build` lulus 17.03. Server standalone port 3000 dinyalakan ulang dari hasil build itu. Status tetap 🟡
>   sampai uji layar ([laporan](task/report/frontend/FE-IGD-041.md), *Pengerjaan ulang 3 Oktober 2026*).
> - Temuan `BE-IGD-061` S6 diputuskan lewat amendment pass `grill-me`
>   ([decision log](00-interview-decisions.md), *Amendment pass 3 Oktober 2026 — observasi Dieskalasi*):
>   `IGD-DEC-183`, `184`, `185`; asumsi `IGD-ASM-003`; pertanyaan baru `IGD-OQ-112`. Nol blocker desain.
>
> **Pembaruan akhir petang:** langkah 1–3 di bawah **dikerjakan** pada sesi yang sama — kontrak `approved` lewat
> `IGD-DEC-186` (manifest 0k), kartu diperluas (0k.1), dan `BE-IGD-061` dikerjakan ulang di source (3 berkas, QBE
> Strict `PASS`). `FE-IGD-042` dibuka ulang (`IGD-DEC-187`). Progres register: BE 37/47, **FE 23/32**, gabungan
> **60/79 = 76%** (sebelumnya FE 24/32, 61/79 = 77%).
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | `dotnet build`, lalu uji API `BE-IGD-061` S13–S19 ([laporan](task/report/backend/BE-IGD-061.md), *Pengerjaan ulang 3 Oktober 2026*) | Rizki |
> | 2 | Pengerjaan ulang `FE-IGD-042` (`IGD-DEC-187`, acceptance 10–13) lewat `build-module-frontend` | Agent |
> | 3 | Uji layar `FE-IGD-041` U2, U6, U7, R dan `FE-IGD-042` dalam satu siklus dengan `BE-IGD-061`, pada hasil build | Rizki (Antigravity) |
> | 4 | Kueri `IGD-OQ-112` per lingkungan | Rizki |
> | 5 | Sesudah 1–3: `verify-module-readiness` untuk `MVP-8` | Agent |
>
> *Tabel di bawah adalah rencana sebelum pembaruan akhir ini.*
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | `design-business-module` (amandemen): turunkan `IGD-DEC-183`…`185` ke validation §11, state §9, API §9.2, termasuk mengunci kalimat `IGD-DEC-184`; minta approval kontrak | Agent → Rizki |
> | 2 | `plan-module-delivery`: perluas kartu `BE-IGD-061` (acceptance 1–7 pada decision log) | Agent |
> | 3 | `build-module-backend` `BE-IGD-061` (pengerjaan ulang); `dotnet build` dan uji API S6 plus skenario baru | Agent → Rizki |
> | 4 | Uji layar `FE-IGD-041` U2, U6, U7, dan R lewat Antigravity pada hasil build 17.03 (1440 × 900) | Rizki |
> | 5 | Kueri `IGD-OQ-112` per lingkungan | Rizki |
> | 6 | Sesudah 1–4: `verify-module-readiness` untuk `MVP-8` — `BE-IGD-061` S2, S3, S12 tetap tertahan `BE-IGD-039` | Agent |
> | tetap | Commit dan push: `RizkiV2` (`ahead 2` + working tree `IGD-DEC-182` dan `FE-IGD-041`), `rizkiG` (`ahead 3` + dokumen blueprint 3 Oktober) | Rizki |
>
> **Temuan di luar task, tidak diperbaiki:**
>
> - Nama pasien panjang melebarkan kolom NAMA pada daftar Pengkajian, sehingga tombol AKSI dapat terpotong ±35
>   piksel. Ini perilaku lama.
> - Placeholder pencarian menyebut No. RM dan nama pasien yang tidak dicari backend.
> - `npm run test:unit` gagal pada Node 24 (argumen folder); suite berjalan lewat pola berkas.
>
> **Diperbarui 3 Oktober 2026 (sore) — seluruh task `MVP-7` ✅ (13 dari 13).** Uji ulang pada hasil build lewat agen
> penguji Antigravity: 10 dari 10 terbukti pada bukti mentah ([laporan uji sore](testing/2026-10-03-laporan-uji-ulang-fe-igd-036-sore.md)
> bagian 7). `FE-IGD-036` ✅; `FE-IGD-034` dan `FE-IGD-038` diverifikasi ulang sesudah pemulihan loket. Langkah 1–3
> pada tabel pagi di bawah **lunas**. Progres register: BE 37/47, FE 24/32, gabungan **61/79 = 77%**.
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | ~~`verify-module-readiness` untuk `MVP-7`~~ — **selesai 3 Oktober 2026: `READY_WITH_CONDITIONS`** (siap UAT, belum siap produksi; DoD 7 terpenuhi, 1 sebagian, 2 belum; tujuh syarat C1–C7) — [evidence/2026-10-03-kesiapan-mvp-7.md](evidence/2026-10-03-kesiapan-mvp-7.md) | Agent |
> | 1a | C6 ✅ selesai 3 Oktober 2026 (empat ketidaksesuaian dokumen). C2: [`IGD-REQ-002`](approval-requests/2026-10-03-permintaan-persetujuan-pemilik-registrasi.md) **disetujui lisan** oleh pemegang dan pengembang modul Registrasi (dilaporkan Rizki 3 Oktober 2026) — **lunas lewat `IGD-DEC-181`**: pemilik IGD memutuskan konfirmasi tertulis tidak disyaratkan untuk tim internal; DoD butir 8 terpenuhi. Sisa syarat `MVP-7`: C1, C3, C4, C5, C7. `IGD-OQ-110` terjawab untuk dev 3 Oktober 2026: **0 baris**. Uji tahap 1 dijalankan ulang ([laporan](testing/2026-10-03-laporan-uji-tahap-1-mvp-7.md) §6): **C5 lunas** (DoD butir 2 terpenuhi); **C4 gagal 1 dari 10** — layar loket IGD memanggil rute kiosk `POST /patient-encounters` sehingga petugas loket sungguhan ditolak `403` (rute petugas `/admin` bekerja); **diperbaiki sebagai pengerjaan ulang `FE-IGD-036` (`IGD-DEC-182`, catatan API §8.1) dan lulus uji ulang dengan akun loket — C4 lunas**. Agent penguji mengubah izin jabatan Petugas Pendaftaran dan Admin Rekam Medis sendiri lewat SuperAdmin (menyimpang dari aturan A1). Sisa syarat `MVP-7`: C1, C3, C7. Daftar lengkap sisa pekerjaan: [evidence/2026-10-03-sisa-pekerjaan-igd.md](evidence/2026-10-03-sisa-pekerjaan-igd.md) | Agent / Rizki |
> | 2 | `MVP-8`: perbaiki tata letak `FE-IGD-041` (kolom PENUTUPAN pada 1440 piksel); bawa temuan observasi `Escalated` (`BE-IGD-061` S6) ke `grill-me` | Agent / pemilik |
> | 3 | Push `RizkiV2` (`ahead 2`) dan `rizkiG` (`ahead 3`) bila siap; dokumen blueprint 3 Oktober belum di-commit | Rizki |
> | paralel | Eskalasi `BE-IGD-039` dan pemetaan 18 unit | Security/Privacy owner, Master Data |
> | terbuka | Tombol *Sekarang* pada pemilih jam waktu tiba — dipertahankan atau dihapus (laporan `FE-IGD-036` bagian 10.6) | Rizki |
>
> **Housekeeping keamanan:** `check-test-data.mjs` dan JSON bukti uji sore memuat kredensial dalam teks biasa (folder
> di-gitignore); tiga berkas `DataProtectionKeys/key-*.xml` masih ter-track di repository backend.
>
> **Diperbarui 3 Oktober 2026 — tahap A penutupan `MVP-7` (review status; docs dan satu pengerjaan ulang frontend).**
> Pemilik menjalankan uji ulang dan kueri invarian; agent memeriksa bukti mentahnya
> ([laporan uji ulang](testing/2026-10-03-laporan-uji-ulang-036-040-be-051.md) bagian 6). Hasil: `FE-IGD-040` ✅;
> `036-U7` dan `036-U8` terbukti; `BE-IGD-051` kriteria 8 terpenuhi di dev (0 baris); `IGD-OQ-108` dijawab
> `IGD-DEC-180` (kontrak berlaku). Ditemukan dan dipulihkan: kemunduran loket commit `4520abe64`. Pengerjaan ulang
> `FE-IGD-036` oleh agent: pesan galat dialog Mulai Triage digulir otomatis ke pandangan; tanggal bawaan waktu tiba
> memakai tanggal terdaftar, bukan jam browser UTC. `eslint` 0 error, unit test IGD 91/91, `npm run build` lulus
> (465/465 halaman, 0 warning).
>
> | Urutan | Tindakan | Pemilik |
> | ---: | --- | --- |
> | 1 | Commit `verification-step.jsx` (pemulihan) dan `emergency-triage-start-dialog.jsx` (pengerjaan ulang) di `RizkiV2` sebelum push | Rizki |
> | 2 | `npm run start` (hasil build 3 Oktober 10.14), lalu uji `036-U7`, `036-U1`, `036-U9` — bukan `next dev` | Rizki / agen penguji |
> | 3 | Bila lulus: `FE-IGD-036` ✅ → `MVP-7` 13/13; jalankan `verify-module-readiness` untuk `MVP-7` | Agent |
> | 4 | `MVP-8`: perbaiki tata letak `FE-IGD-041`; bawa temuan observasi `Escalated` ke `grill-me` | Agent / pemilik |
> | paralel | Eskalasi `BE-IGD-039` dan pemetaan 18 unit (membuka `MVP-6`, runtime `MVP-4`/`5`, `BE-IGD-035`, `041`, `061`) | Security/Privacy owner, Master Data |
>
> **Housekeeping:** tiga berkas `DataProtectionKeys/key-*.xml` masih ter-track di repository backend; branch
> backend `rizkiG` `ahead 3`.
>
> **Diperbarui 22 September 2026 (penutup) — desain disetujui; `plan-module-delivery` FINAL selesai (docs saja, nol source, nol commit).**
> Pemilik menyetujui bagian encounter-first kelima kontrak (`IGD-DEC-157`: API `0.11.0` §8, validation `0.8.0` §10, state `0.5.0` §8,
> integration `0.4.0` §5, permission/audit `0.5.0` §7) beserta koreksi B1 (nama `EmergencyEpisodeRule` di integration §5.2), B2 (pesan
> validation §10.4 aturan 4 — **teks usulan agent**, pemilik menjawab dengan placeholder), B4 (Swagger hanya di Development); B3 diterima
> sebagai keterbatasan (`IGD-DEC-158`); `IGD-OQ-104`…`107` → `IGD-DEC-159`…`162`. Revisi blueprint **`7`**; kontrak terkunci hash
> (manifest bagian 2). Kartu diselaraskan: backend R3.13 `BE-IGD-051`…`059` (**baru:** `057` NoShow, `058` waktu tiba + kunci `PUT`,
> `059` jalur umum Registrasi dibatasi), frontend R3.12 `FE-IGD-035`…`040` (**baru:** `038` alasan override ke pintu encounter,
> `039` aksi pergi sebelum ditriage, `040` panel waktu tiba). Kandidat "tautkan identitas U1" **dibatalkan** (`IGD-DEC-151`).
> `IGD-OQ-093` ditulis benar: `superseded` sebagian, realisasi `BE-IGD-053`. Pertanyaan baru `IGD-OQ-108` (tidak menahan).
>
> | Urutan | Task | Keadaan |
> | ---: | --- | --- |
> | 1 | `BE-IGD-051` — kunjungan berakhir menutup encounter | ✅ **22 September 2026 — atas penilaian pemilik** (build dari artefak; S1–S9 lulus menurut pemilik, tanpa lampiran; kueri invarian dikecualikan sampai sesudah rilis) ([laporan](task/report/backend/BE-IGD-051.md)) |
> | 2 | `BE-IGD-052` rekonsiliasi, `BE-IGD-054` daftar terpadu, `BE-IGD-055` Mulai Triage | **Siap** sejak `051` ✅ — butuh go-ahead; boleh paralel (migration tetap satu per satu, dibuat Rizki) |
> | 3 | `BE-IGD-053` penjaga pintu encounter, `BE-IGD-057` NoShow, `BE-IGD-058` waktu tiba | Menunggu `055` (dan `052` untuk `053`) |
> | 4 | `BE-IGD-059` jalur umum Registrasi | Menunggu `051`, `057` |
> | layar | `FE-IGD-035`, `038`, `040` → `039`, `036` | Menunggu backend pasangannya; `FE-IGD-038` **dirilis bersama** `BE-IGD-053` |
> | ⛔ | `BE-IGD-056`, `FE-IGD-037` | `IGD-OQ-102`, `IGD-OQ-103` |
>
> **Urutan rilis per lingkungan** (backend roadmap R3.13.5): `051` → `052` + jalankan rekonsiliasi K1 → `053` bersama `FE-IGD-038` →
> `057`, `059` → `FE-IGD-036`. Progres register: **BE 27/43 = 63%** (sesudah `BE-IGD-051` ✅), **FE 18/30 = 60%** (penyebut naik karena enam task baru, bukan karena
> ada yang turun). **Housekeeping keamanan:** commit backend lokal `69953e98` (belum di-push) memuat
> `DataProtectionKeys/key-6f6691ec-15c3-48d9-b04e-f0b89124a4eb.xml` — kunci kriptografi yang dilarang di-commit; lihat laporan sesi.
>
> **Diperbarui 22 September 2026 (akhir) — desain encounter-first selesai sebagai `draft` (`design-business-module`).**
> Gate slice: `PARTIALLY_READY` ([evidence](evidence/02-requirement-completeness-gate.md)); bentuk `SINGLE` (`IGD-DEC-155`); `erd/`
> dipertahankan + `flowcharts/` baru (`IGD-DEC-156`). Kontrak baru (semua `draft`, Rencana): API `0.11.0` (**bukan aditif murni**),
> validation `0.8.0`, state `0.5.0`, integration `0.4.0`, permission/audit `0.5.0`. Arsitektur §13 backend/frontend, kamus data §6,
> PRD §8 (`EPIC IGD-11`: `FR-IGD-069`…`085`, gelombang `MVP-7`; `EPIC IGD-12` kelayakan dokter `OPEN DECISION`), uji
> `AT-IGD-166`…`185`. Ditahan: `S7` (`IGD-OQ-102`/`103`). Tinjauan desain: `IGD-OQ-104`…`107` (tidak memblokir).
> **Langkah berikutnya:** pemilik menyetujui desain + kontrak, lalu `plan-module-delivery` final menyelaraskan kartu R3.13/R3.12.
>
> **Diperbarui 22 September 2026 (larut) — pertanyaan penutup impact scan dijawab: `IGD-DEC-151`…`154` (`approved`).**
> Pasien tanpa identitas memakai rekam pengganti lewat alur pasien baru yang ada (U1 `IGD-DEC-140` dan `IGD-DEC-149`
> `superseded`); koreksi waktu tiba tidak boleh sesudah peristiwa klinis pertama; `PATCH …/status` Registrasi menolak
> encounter Emergency, `…/cancel` hanya sebelum kunjungan lahir; `PatientId`/`EncounterId` kunjungan terkunci pada `PUT`.
> **Nol conflict terbuka** pada klaster encounter-first. Berikutnya `design-business-module`; kartu R3.13/R3.12 masih menunggu
> `plan-module-delivery` final. Yang tersisa hanya data (kueri D, E1–E3, A/B) dan pemilik yang belum dipetakan.
>
> **Diperbarui 22 September 2026 (malam) — impact scan `trace-existing-capabilities` selesai (capability map suplemen 3.2).**
> Temuan yang menahan desain: **`IGD-CONF-06`** — layar pendaftaran selalu menuntut pasien dan selalu membuat encounter,
> jadi jalur U1 `IGD-DEC-140` tidak punya penghasil di frontend. **Koreksi fakta:** waktu tiba **bukan** dasar SLA triage
> (`ResponseDueAt` dari mulai triage) — isi `IGD-DEC-147` tetap, alasannya dikoreksi. **Fakta baru:** pintu encounter sudah
> antre lewat kunci penomoran global (`PatientEncounterNumberService`). Empat pertanyaan penutup `IGD-TRQ-08`…`11` untuk
> pass `grill-me` singkat sebelum `design-business-module`.
>
> **Diperbarui 22 September 2026 (sore) — amendment pass `grill-me` kasus tepi encounter-first.**
> Pemilik menjawab butir A–I: `IGD-DEC-142`…`150` (`approved`), koreksi `IGD-DEC-139` (tanda kelima `NoShowAt`),
> `IGD-OQ-094`/`095`/`096`/`097`/`100`/`101` `superseded`, `IGD-OQ-093` `superseded` sebagian (realisasi terbuka di
> `BE-IGD-053`). **Kartu R3.13/R3.12 di bawah BELUM diselaraskan** — atas keputusan pemilik, urutannya:
> `trace-existing-capabilities` (impact scan) → `design-business-module` → `plan-module-delivery` final → implementasi.
> Kolom "Yang dibutuhkan dari pemilik" pada tabel di bawah karena itu **sebagian usang**: yang tersisa hanya angka
> kueri D, E1–E3, dan A/B.
>
> **Diperbarui 22 September 2026 — desain encounter-first ditulis ke repo (docs saja, nol source).**
> Tiga keputusan disetujui **prinsip**: `IGD-DEC-139` (encounter-first + rumus episode terbuka),
> `IGD-DEC-140` (pasien tanpa identitas: U1 transisi `approved`, U2 `candidate`), `IGD-DEC-141` (kelayakan
> dokter jaga + override). `IGD-OQ-093` **`superseded` sebagian** (bentuk B1; realisasi `BE-IGD-053`). `FE-IGD-027` **tetap ✅**.
> Task baru: backend R3.13 `BE-IGD-051`…`056`, frontend R3.12 `FE-IGD-035`…`037`
> ([evidence](evidence/2026-09-22-desain-encounter-first.md)).
>
> | Urutan | Task | Keadaan | Yang dibutuhkan dari pemilik |
> | ---: | --- | --- | --- |
> | 1 | `BE-IGD-051` — kunjungan berakhir menutup encounter | **Siap** (prasyarat ✅) | Go-ahead + jawaban TK-1 (hapus lunak) dan TK-2 (encounter `Outpatient` lama) |
> | 1 | `BE-IGD-054` — daftar Menunggu Triage terpadu | **Siap** (prasyarat ✅) | Go-ahead + terima/tunda risiko baris K3 lama tampil sampai `BE-IGD-052` |
> | 2 | `BE-IGD-052` — rekonsiliasi encounter historis | ⛔ | Konfirmasi K1–K4, **angka** kueri D, `IGD-OQ-095` |
> | 3 | `BE-IGD-053` — penjaga di pintu encounter | ⛔ | `IGD-OQ-096`, `IGD-OQ-101` (sesudah `051` dan `052`) |
> | — | `BE-IGD-055` — Mulai Triage | ⛔ | W1 (`IGD-OQ-094`), `IGD-OQ-100`, status awal kunjungan |
> | — | `BE-IGD-056` — dokter layak + override | ⛔ | **Angka** kueri E1–E3 |
> | sesudah backend | `FE-IGD-035`, `FE-IGD-036`, `FE-IGD-037` | Menunggu backend | `FE-IGD-036` **wajib** menunggu `BE-IGD-053` — memutus loket dari `POST emergency-visits` lebih dulu membuka pendaftaran ganda |
>
> **Masih terbuka dari gelombang sebelumnya:** angka kueri audit A/B `BE-IGD-050` acceptance 8. Teks
> `IGD-DEC-138` kolom status masih menulis "menunggu konfirmasi pemilik atas laporan rencana" padahal
> mekanismenya sudah diputuskan 21 September 2026 malam — housekeeping, tidak disentuh pass ini.
>
> **Diperbarui 21 September 2026 (sore) — `EPIC IGD-04` tuntas; backlog kecil diperiksa.** Butir 4 di bawah
> (`BE-IGD-044` → `BE-IGD-045` → `FE-IGD-027`) **selesai**. Sesuai urutan pemilik, backlog kecil (butir 7)
> dikerjakan sesudahnya, dengan hasil:
>
> | Task | Hasil | Yang dibutuhkan berikutnya |
> | --- | --- | --- |
> | `FE-IGD-014` kriteria 2 | ✅ **Dinilai ulang 21 September 2026 (malam) — atas penilaian pemilik, sesudah `FE-IGD-034` ✅; celah `IGD-OQ-093` `open` dinyatakan** ([laporan](task/report/frontend/FE-IGD-014.md) bagian 9). ✅ ini **tidak** menyatakan tidak ada encounter yatim dalam segala keadaan, dan encounter yatim yang sudah ada belum diaudit. *Riwayat 🟡:* **Sudah di source** (kotak peringatan + tombol *Buka Kunjungan IGD*, memakai endpoint yang sudah ada). `npm run build` **lulus**; **uji layar (pemilik, 21 September 2026 malam): kotak, nomor, dan status tampil benar**, hasil klik tombol belum dikonfirmasi ([laporan](task/report/frontend/FE-IGD-014.md) bagian 6.1). Uji itu membuktikan **encounter yatim terjadi** (peringatan *Encounter sudah terbentuk*). **Belum boleh ✅ end-to-end** (`IGD-DEC-138`) | Pemilik: konfirmasi hasil klik *Buka Kunjungan IGD*. Encounter yatim ditangani `BE-IGD-050` → `FE-IGD-034`; sisa celah `IGD-OQ-093` |
> | `FE-IGD-017` kriteria 1 | ✅ **22 September 2026 — atas penilaian pemilik** (`npm run build` dinyatakan lulus; keluaran tidak dilampirkan). *Riwayat 🟡 21 September 2026 (larut malam):* kolom pelaku/penyetuju membaca `recordedByName`/`approvedByName` (kontrak `0.9.0`, `BE-IGD-049`); eslint 0 error, 5 unit test baru lulus, suite 1415/1424 (9 gagal = Rawat Inap, sudah ada sebelumnya). **Uji layar pemilik LULUS (21 September 2026, larut malam): kolom *Pelaku* menampilkan `SuperAdmin` pada ketiga kejadian `DEP-260921064559-9B2DB7`, menggantikan GUID** ([laporan](task/report/frontend/FE-IGD-017.md) 6.1) — sekaligus membuktikan `BE-IGD-049` berjalan pada jalur `GET` daftar | ~~Pemilik: nyatakan `npm run build` lulus~~ — **dinyatakan 22 September 2026 → `FE-IGD-017` ✅**. ~~Uji API `BE-IGD-049` S2–S5~~ — **dilaporkan lulus 22 September 2026 → `BE-IGD-049` ✅** |
>
> **R3.12 / R3.11 (21 September 2026) — `BE-IGD-049` sudah diimplementasikan (malam, build lulus); `BE-IGD-050` ✅ sudah diimplementasikan dan diuji pemilik (malam, tiga berkas source, kontrak `0.10.0`; S1–S7 `PASS` menurut pemilik); `FE-IGD-034` ✅ sudah dikerjakan sesudahnya dan dinyatakan ✅ atas penilaian pemilik (malam, lima berkas frontend, commit `c941012ac`).** Task baru: `BE-IGD-049`
> (nama pelaku pada event kepergian), `BE-IGD-050` (pra-cek episode ganda sebelum encounter dibuat — korektif),
> `FE-IGD-034` (layar memanggil pra-cek). Rantai: `BE-IGD-033/034 ✅ → BE-IGD-049 → FE-IGD-017` dan
> `BE-IGD-023/025 ✅ → BE-IGD-050 → FE-IGD-034 ← FE-IGD-014`. Keputusan: `IGD-DEC-137`, `IGD-DEC-138`
> (`approved`), `IGD-OQ-093` (`open`). **Diputuskan pemilik 21 September 2026 (malam):** mekanisme `BE-IGD-050` = lapis A
> (pra-cek) dengan `IGD-OQ-093` sebagai backend gap eksplisit; hak akses `EmergencyVisit : Create`; `FE-IGD-034` `fail-open`
> bila pra-cek gagal. **Go-ahead implementasi `BE-IGD-050` diberikan pemilik dan dikerjakan 21 September 2026 (malam)** —
> laporan [BE-IGD-050](task/report/backend/BE-IGD-050.md). **Pemilik membangun dan menjalankan uji API S1–S7 — semuanya `PASS`
> (pernyataan pemilik); `BE-IGD-050` ✅ atas penilaian pemilik.** Sisa milik pemilik: kueri audit A/B (acceptance 8, dikecualikan
> dari ✅). **`FE-IGD-034` ✅ atas penilaian pemilik (21 September 2026 malam)** — build dan uji layar dilaporkan lulus (pernyataan pemilik; keluaran dan rincian skenario tidak dilampirkan; artefak `.next` 16:03 dan commit `c941012ac` diperiksa agent). **`FE-IGD-014` dinilai ulang → ✅ dengan celah `IGD-OQ-093` dinyatakan.** `IGD-OQ-093` dicatat sebagai **backend gap eksplisit** pada laporan (pendaftaran serentak dan
> klien tanpa pra-cek tidak tertutup).
>
> Sisa yang **tidak** menunggu pihak luar: laporan susulan (`BE-IGD-017`, `BE-IGD-043`, `FE-IGD-025`, `FE-IGD-026`)
> dan uji `Down` migration `20260826090500` (membuka `BE-IGD-026` dan `BE-IGD-031`; migration itu berada di
> belakang sekitar 75 migration modul lain, jadi butuh rancangan uji tersendiri). Sisanya menunggu pihak luar:
> `BE-IGD-039` beserta pemetaan 18 unit, `BE-IGD-042`.

> **Urutan delivery ditetapkan Product/Domain Owner 16 September 2026.** Butir 1 dan 2 wajib lulus
> lebih dulu; sesudahnya pekerjaan besar berikutnya adalah `EPIC IGD-04`. Butir lain **tidak boleh**
> didahulukan, dan agent **tidak memulai task lain secara otomatis**.

> **Sisipan 16 September 2026 (keempat).** Pemilik menugaskan gelombang **R3.9 — tata letak
> riwayat pada ruang kerja pemeriksaan**: `FE-IGD-031` lalu `FE-IGD-032`
> ([evidence](evidence/2026-09-16-tata-letak-riwayat-pemeriksaan.md), `IGD-DEC-133` dan
> `IGD-DEC-134`). Letaknya **sesudah butir 1** dan **sebelum butir 4** (`EPIC IGD-04`). Keduanya
> nol backend, nol kontrak, dan tidak menunggu siapa pun selain `FE-IGD-028` ✅ yang sudah
> selesai. Butir 1 tetap didahulukan karena tinggal satu klik.
>
> **Diperbarui 16 September 2026 (keempat):** implementasi kedua task **sudah selesai** dan
> berstatus 🟡. Yang menahan keduanya sekarang hanya dua hal, dan keduanya milik Rizki:
> menjalankan `npm run build` di `QuilvianSystemFrontendDev`, lalu membuka layar Assesmen IGD
> untuk mencoba segmen Formulir/Riwayat pada tab Assesmen Awal, dan — untuk `FE-IGD-032` —
> menyelesaikan satu periode observasi beserta isian Kesimpulan serta mencatat satu putaran
> pemantauan bertanda vital. Dua delta pada `FE-IGD-032` menunggu penilaian pemilik; lihat
> [laporan](task/report/frontend/FE-IGD-032.md) bagian 8.

Seluruh task di bawah **sudah punya kartu roadmap**. Urutan yang tidak menunggu pihak lain:

1. **`FE-IGD-029`** 🟡 — satu-satunya sisa gelombang R3.8, dan tinggal **satu klik**. Kriteria 1
   sudah terbukti lewat layar 16 September 2026: pasien yang baru didaftarkan langsung berstatus
   "Menunggu Triage". Kriteria 2 belum — tekan **Isi Triage** pada pasien baru itu lalu simpan
   sampai berhasil; itu membuktikan penolakan `409` yang memicu gelombang ini benar-benar hilang.
   Pasangannya **`FE-IGD-030`** ✅ sudah terbukti penuh.
2. **`BE-IGD-046`** → **`FE-IGD-028`** — pemantauan observasi bertanda vital (`IGD-DEC-122`…`126`,
   kontrak `0.6.0`). Keputusan dan kontraknya sudah terkunci 16 September 2026; implementasinya
   sudah ditulis dan tinggal uji lewat layar.
3. ~~**`BE-IGD-041`**~~ 🟡 — **dikerjakan 16 September 2026.** Pesan penutupan kini menyebut
   pesanan yang menahannya, dan kriteria 2 `BE-IGD-035` terpenuhi pada source. Tersisa
   `dotnet build` dan uji API tiga skenario ([laporan](task/report/backend/BE-IGD-041.md)).
4. **`BE-IGD-044`** → **`BE-IGD-045`** → **`FE-IGD-027`** — riwayat penugasan dokter
   (`EPIC IGD-04`). **Pekerjaan besar berikutnya menurut keputusan owner 16 September 2026**,
   dimulai setelah butir 1 dan 2 lulus. Pada `BE-IGD-044` agent menyiapkan model, konfigurasi EF,
   kebutuhan service, dan kebutuhan migration; **agent tidak menjalankan migration** — Rizki
   menjalankannya sendiri setelah review. Kartu ketiganya sudah ditinjau ulang 16 September 2026
   terhadap `IGD-DEC-116`/`117` dan kontrak terbaru; hasilnya di bagian *Tinjauan kesiapan
   `EPIC IGD-04`* di bawah.
5. **Laporan susulan:** `BE-IGD-017`, `BE-IGD-043` (`f76ebaab`), `FE-IGD-025` (`bd1d94a8a`,
   termasuk temuan privasi `IGD-EV-117`), `FE-IGD-026` (`c8613d88c`).
6. **Uji langkah mundur migration** `20260826090500` di basis data terpisah — membuka
   `BE-IGD-026` dan `BE-IGD-031`. Butuh basis data terpisah milik Rizki.
7. **Backlog kecil, sengaja tidak didahulukan** (keputusan owner 16 September 2026):
   `FE-IGD-017` (pelaku kepergian tampil sebagai UUID) dan `FE-IGD-014` kriteria 2 — keduanya
   `IGD-EV-123`. Dikerjakan setelah `EPIC IGD-04`, bukan sebelum.

**Diperbarui 17 September 2026.** `npm run build` frontend sudah dijalankan dan **lulus bersih**
([evidence](evidence/2026-09-17-verifikasi-build-dan-perkakas.md)), jadi penahan build pada
`FE-IGD-023`, `FE-IGD-024`, `FE-IGD-031`, dan `FE-IGD-032` **lunas**. Keempatnya kini menyisakan
**uji lewat layar** saja — ditambah penilaian pemilik atas dua delta `FE-IGD-032` bagian 8.

Yang masih menunggu **build backend milik Rizki**, lalu uji API: `BE-IGD-040` dan `BE-IGD-041`.
Perintahnya `dotnet build -p:RunAnalyzers=false` di dalam `NewQuilvianSystemBackend`.

Yang **menunggu owner**: `BE-IGD-042` — jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`
dari kueri yang dijalankan Rizki sendiri.

Gap yang **tidak** diberi ID atas instruksi owner: layar resusitasi IGD (`IGD-EV-111`) dan layar
baca/aksi `order-items` (`IGD-EV-109`).

**Ditegaskan ulang 16 September 2026** sesudah `BE-IGD-041` membuat penolakan penutupan menyebut
pesanan yang menahannya: gap aksi `order-items` **tetap tidak dibuatkan task implementasi**. Ia
dicatat sebagai terblokir dengan dependency `BE-IGD-039`. Layar baca-saja **juga tidak**
diprioritaskan, karena menampilkan daftar pesanan tanpa aksi belum memberi petugas jalan
penyelesaian — hanya memindahkan kebuntuan dari pesan galat ke layar.

## Tinjauan kesiapan `EPIC IGD-04` — 16 September 2026

Dijalankan atas perintah owner sebelum coding dimulai. Ketiga kartu `BE-IGD-044`, `BE-IGD-045`,
dan `FE-IGD-027` ditinjau terhadap `IGD-DEC-116`, `IGD-DEC-117`, dan kontrak `0.5.0`.
**Nol requirement didesain ulang.**

### Ketujuh pertanyaan audit owner terjawab kontrak yang sudah terkunci

| # | Pertanyaan | Dijawab oleh | Keadaan |
| ---: | --- | --- | :-: |
| 1 | Siapa dokter penanggung jawab saat ini | `GET /active` tanpa `at` (API §3); baris dengan `EffectiveTo IS NULL`, dijaga unique bersyarat | ✅ |
| 2 | Sejak kapan | Kolom `EffectiveFrom` (kamus data §4) | ✅ |
| 3 | Siapa dokter sebelumnya | `GET /` riwayat urut waktu (`BE-IGD-045` acceptance 8); baris lama tidak pernah ditimpa | ✅ |
| 4 | Kapan pengalihan terjadi | `EffectiveTo` baris lama ditutup bersamaan dengan `EffectiveFrom` baris baru dalam satu transaksi (acceptance 3) | ✅ |
| 5 | Siapa yang mengalihkan | `AssignedByUserId` pada baris baru, diisi sistem dari pengguna aktif | ✅ |
| 6 | Alasan pengalihan | `AssignmentReason` (500), wajib saat pengalihan (acceptance 4) | ✅ |
| 7 | Dokter pada waktu tertentu | `GET /active?at={datetime}` (`IGD-DEC-117`, API §3.1, acceptance 9) | ✅ |

### Empat hal yang perlu diputuskan — **SELURUHNYA DIPUTUSKAN OWNER 16 September 2026**

| No | Temuan | Keputusan |
| ---: | --- | --- |
| 1 | **`BE-IGD-045` tidak menjanjikan nama.** `FE-IGD-027` acceptance 6 menuntut dokter dan penugas tampil sebagai **nama**, bukan ID (`IGD-EV-123`), tetapi acceptance `BE-IGD-045` tidak menyebut proyeksi nama sama sekali. Bila backend hanya mengirim `DoctorId` dan `AssignedByUserId`, frontend tidak dapat memenuhi kriterianya tanpa panggilan tambahan | **`IGD-DEC-129` — DISETUJUI.** Proyeksi aditif `doctorName` dan `assignedByName` pada `GET /` dan `GET /active`; `doctorId` dan `assignedByUserId` tetap dikirim. Satu kueri berproyeksi, nol `N+1`, nol kolom baru. API naik `0.7.0` bagian 3.2; acceptance `BE-IGD-045` bertambah butir 12 |
| 2 | **`IsActive` dan `EffectiveTo` sama-sama menyatakan "aktif".** Kamus data §4 memuat keduanya, sedangkan unique bersyarat hanya menjaga `EffectiveTo IS NULL` | **`IGD-DEC-130` — DIPUTUSKAN.** `IsActive` **dihapus dari rancangan sebelum model dan migration dibuat**. Penugasan berjalan = `EffectiveTo IS NULL`; pada waktu `T` = `EffectiveFrom <= T AND (EffectiveTo IS NULL OR T < EffectiveTo)`. Kamus data §4 sudah diselaraskan; acceptance `BE-IGD-044` butir 2 dan 3 serta `BE-IGD-045` butir 13 mengikuti |
| 3 | **`BE-IGD-045` menuntut baris baru di `Program.cs`.** Service dan controller `EmergencyDoctorAssignment` sepenuhnya baru, jadi pendaftaran DI tidak terhindarkan. Kartunya sudah mensyaratkan agent berhenti dan meminta persetujuan owner | **`IGD-DEC-131` — DISETUJUI TERBATAS.** Hanya untuk pendaftaran DI service Emergency Doctor Assignment yang memang baru; dilarang cleanup atau refactor DI lain; baris pendaftarannya wajib dicantumkan pada laporan task |
| 4 | **Kontrak menyebut nama tabel yang sudah diganti.** API §3 masih menulis *"memperbarui `TrxPatientEncounter.DoctorId`"*, padahal tabel itu sudah menjadi `RegPatientEncounter` sejak `58c61a5b` (10 September 2026) | **`IGD-DEC-132` — DISETUJUI.** Kontrak memakai `RegPatientEncounter`. Sudah diterapkan pada API §3 dan integration contract. ERD dan kamus data §5.3 **masih** memakai nama lama dan berada di luar lingkup keputusan ini |

### Yang sudah siap dan tidak perlu dipertanyakan lagi

Pola reuse `BE-IGD-044` terbukti ada: `InPatientManagement/Models/InpDoctorAssignment.cs` beserta
`Repositories/Configurations/HealthServices/InPatientManagement/InpDoctorAssignmentConfiguration.cs`.
Hak akses `EmergencyDoctorAssignment` sudah terdaftar pada permission matrix baris 41 dengan
`Read`, `Create`, dan `Update`. Nama tabel, kolom, index, dan unique bersyarat sudah terkunci
`IGD-DEC-116` dan kamus data §4.

### Gerbang yang tetap terbuka

~~`IGD-DEC-082` masih `draft` menunggu Clinical Governance.~~ **Ditutup 17 September 2026** —
`approved` oleh Product/Domain Owner, jadi butir 10 Definition of Done `BE-IGD-045` tidak lagi
tertahan. Peran Clinical Governance tetap `OPEN` dan wajib meninjau ulang bila kelak ditunjuk.

Yang **masih** terbuka: `BE-IGD-044` acceptance 5 menuntut langkah mundur migration diuji di
**basis data terpisah** — milik Rizki, dan menjadi syarat ✅ task itu.

## Backlog lintas modul — `ClinicalManagement`

Ditemukan pada uji layar `FE-IGD-028` 16 September 2026, dan **atas keputusan owner tidak masuk
lingkup `EPIC IGD-04`**. Keduanya milik formulir tanda vital bersama, bukan modul IGD.

| Temuan | Isi | Bukti |
| --- | --- | --- |
| Jenis oksigen bertentangan dengan laju alir | Baris tersimpan dengan penanda memakai oksigen bernilai benar, jenis "tidak menggunakan", dan laju `50 L/menit`. Riwayat menampilkan *"Oksigen: Tidak menggunakan · 50 L/menit"* — tampilannya benar, isiannya yang longgar | Laporan `FE-IGD-028` butir (4) |
| Nilai tanda vital ekstrem diterima tanpa peringatan | Nadi `250 x/menit` dan frekuensi napas `80 x/menit` tersimpan. Penanda *"Tanda Vital Kritis"* menyala, tetapi nol batas kewajaran yang menahan salah ketik | Laporan `FE-IGD-028` butir (5) |

## Optional deterministic delivery progress

| Lapisan | Rumus | Hasil |
| --- | --- | --- |
| Backend | task ✅ / seluruh task roadmap | **59 / 64 = 92%** (26 Oktober 2026 petang, `BE-IGD-043` ✅ lewat laporan susulan; atas 67 baris register: 59 / 67 = 88%). *Sebelumnya tertulis* **58 / 64 = 91%** (26 Oktober 2026, `BE-IGD-070` ✅ sesudah penilaian Putaran D3; atas 67 baris register: 58 / 67 = 87%). *Sebelumnya tertulis* **57 / 64 = 89%** (21 Oktober 2026, `BE-IGD-068` ✅; atas 67 baris register: 57 / 67 = 85%). *Sebelumnya tertulis* **56 / 64 = 88%** (20 Oktober 2026 malam, `BE-IGD-067` ✅; atas 67 baris register: 56 / 67 = 84%). *Sebelumnya tertulis* **55 / 64 = 86%** (20 Oktober 2026 sesudah D2R, `BE-IGD-065` ✅; atas 67 baris register: 55 / 67 = 82%). *Sebelumnya tertulis* **54 / 64 = 84%** (20 Oktober 2026 akhir, `BE-IGD-066` ✅; atas 67 baris register: 54 / 67 = 81%). *Sebelumnya tertulis* **53 / 64 = 83%** (20 Oktober 2026 penutup, sesudah `BE-IGD-017` dibatalkan `IGD-DEC-333`; atas 67 baris register: 53 / 67 = 79%). *Sebelumnya tertulis* **53 / 65 = 82%** (20 Oktober 2026, `BE-IGD-035` ✅; tanpa dua task yang dibatalkan `IGD-DEC-304`: 53 / 63 = 84%). *Sebelumnya tertulis* **50 / 65 = 77%** (18 Oktober 2026 sore, `BE-IGD-072` ✅; tanpa dua task yang dibatalkan: 50 / 63 = 79%). *Sebelumnya tertulis* **49 / 65 = 75%** (18 Oktober 2026 sore, `BE-IGD-083` ✅; tanpa dua task yang dibatalkan: 49 / 63 = 78%). *Sebelumnya tertulis* **48 / 65 = 74%** (18 Oktober 2026: `BE-IGD-074` ✅, `BE-IGD-083` masuk register; tanpa dua task yang dibatalkan `IGD-DEC-304`: 48 / 63 = 76%; sebelum `BE-IGD-083`: 48 / 64 = 75%). *Sebelumnya tertulis* **47 / 64 = 73%** (17 Oktober 2026, sesudah koreksi `IGD-DEC-319`; tanpa dua task yang dibatalkan `IGD-DEC-304`: 47 / 62 = 76%). *Sebelumnya tertulis* **26 / 40 = 65%** (22 September 2026: penyebut naik karena `BE-IGD-051`…`056` direncanakan — bukan karena ada yang mundur; **dan dikoreksi**: register memuat 34 baris sebelum penambahan, bukan 35, jadi angka lama seharusnya 26 / 34 = 76%). *Sebelumnya tertulis* **26 / 35 = 74%** (`BE-IGD-049` ✅ 22 September 2026 atas penilaian pemilik sesudah uji API S2–S5; sebelumnya 25 / 35 = 71% dan 24 / 35 = 69%). *Catatan lama:* **24 / 35 = 69%** (penyebut naik dari 33 karena `BE-IGD-049` dan `BE-IGD-050` direncanakan 21 September 2026 sore — bukan karena ada yang mundur; sebelumnya **24 / 33 = 73%**: `BE-IGD-044` ✅ 21 September 2026 sesudah uji `Down` asli lulus; sebelumnya 23 / 33 = 70% sesudah `BE-IGD-048` ✅, dan 22 / 33 = 67% sesudah `BE-IGD-045` ✅ 17 September 2026). Dihitung dari *Register status task* `backend-roadmap.md`. Tidak dikecualikan: `BE-IGD-039` dan `BE-IGD-042` (⛔) tetap dihitung di penyebut. *Sebelumnya tertulis 18 / 29 = 62% — usang sejak `BE-IGD-040` dan `BE-IGD-046` ✅ dan `BE-IGD-046` masuk register* |
| Frontend | task ✅ / seluruh task roadmap | **49 / 59 = 83%** (26 Oktober 2026 petang, `FE-IGD-013`, `025`, `026` ✅; tanpa `FE-IGD-062` yang dibatalkan: 49 / 58 = 84%). *Sebelumnya tertulis* **46 / 59 = 78%** (26 Oktober 2026, `FE-IGD-071` 🟡 masuk register atas keputusan pemilik; tanpa `FE-IGD-062` yang dibatalkan: 46 / 58 = 79%). *Sebelumnya tertulis* **46 / 58 = 79%** (26 Oktober 2026, `FE-IGD-054` dan `FE-IGD-055` ✅ sesudah penilaian Putaran D3; tanpa `FE-IGD-062` yang dibatalkan: 46 / 57 = 81%. Keduanya sebelumnya bertanda 🟢 dan memang tidak dihitung selesai). *Sebelumnya tertulis* **44 / 58 = 76%** (21 Oktober 2026 malam, `FE-IGD-070` 🟡 masuk register; tanpa `FE-IGD-062` yang dibatalkan: 44 / 57 = 77%). *Sebelumnya tertulis* **44 / 57 = 77%** (21 Oktober 2026, `FE-IGD-045`, `046`, `048`, `049` ✅; tanpa `FE-IGD-062` yang dibatalkan: 44 / 56 = 79%). *Sebelumnya tertulis* **40 / 57 = 70%** (21 Oktober 2026, `FE-IGD-047` ✅; tanpa `FE-IGD-062` yang dibatalkan: 40 / 56 = 71%). *Sebelumnya tertulis* **39 / 57 = 68%** (20 Oktober 2026; tanpa `FE-IGD-062` yang dibatalkan: 39 / 56 = 70%. `FE-IGD-054`, `055`, `057` bertanda 🟢 di luar kosakata status dan tidak dihitung selesai). *Sebelumnya tertulis* **36 / 57 = 63%** (18 Oktober 2026, sesudah `FE-IGD-061` ✅; tanpa `FE-IGD-062`: 36 / 56 = 64%). *Sebelumnya tertulis* **35 / 57 = 61%** (17 Oktober 2026 malam, sesudah `FE-IGD-060` ✅; tanpa `FE-IGD-062`: 35 / 56 = 63%). *Sebelumnya tertulis* **34 / 57 = 60%** (17 Oktober 2026; tanpa `FE-IGD-062` yang dibatalkan: 34 / 56 = 61%; `FE-IGD-054` dan `055` bertanda 🟢 di luar kosakata status dan tidak dihitung selesai). *Sebelumnya tertulis* **18 / 27 = 67%** (22 September 2026: penyebut naik karena `FE-IGD-035`…`037` direncanakan; **dan dikoreksi**: register memuat 24 baris sebelum penambahan, bukan 25, jadi angka lama seharusnya 18 / 24 = 75%). *Sebelumnya tertulis* **18 / 25 = 72%** (`FE-IGD-017` ✅ 22 September 2026 atas penilaian pemilik; sebelumnya 17 / 25 = 68% sesudah `FE-IGD-034` ✅ dan `FE-IGD-014` ✅, dan 15 / 25 = 60%). *Catatan lama:* **15 / 25 = 60%** (penyebut naik dari 24 karena `FE-IGD-034` direncanakan 21 September 2026 sore; sebelumnya **15 / 24 = 63%**: `FE-IGD-027` ✅ kembali 21 September 2026 sesudah `npm run build` pemilik lulus; sempat 14 / 24 = 58% pada hari yang sama saat dinilai ulang). `FE-IGD-014` 🟡 dan `FE-IGD-017` 🟡 tidak dihitung (dihitung ulang 17 September 2026 sesudah uji layar pemilik). ✅: `FE-IGD-015`, `016`, `018`, `019`, `020`, `021`, `023`, `024`, `027`, `028`, **`029`**, `030`, **`031`**, **`032`**, **`033`**. *Sebelumnya tertulis 6 / 17 = 35% — usang sejak `FE-IGD-023`, `024`, `028`, `030` ✅ dan `FE-IGD-029`…`032` masuk register* |
| Gabungan | task ✅ backend + frontend / seluruh task kedua roadmap | **108 / 122 = 89%** (26 Oktober 2026 petang, langkah 2 dan 3; atas 126 baris register: 108 / 126 = 86%). *Sebelumnya* **104 / 122 = 85%** (26 Oktober 2026, sesudah `FE-IGD-071` masuk register; atas 126 baris register: 104 / 126 = 83%). *Sebelumnya* **104 / 121 = 86%** (26 Oktober 2026, penilaian lima putaran: `BE-IGD-070`, `FE-IGD-054`, `FE-IGD-055` ✅; atas 125 baris register: 104 / 125 = 83%). *Sebelumnya* **101 / 121 = 83%** (21 Oktober 2026 malam, penambahan `FE-IGD-070` 🟡; atas 125 baris register: 101 / 125 = 81%). *Sebelumnya* **101 / 120 = 84%** (21 Oktober 2026 sesudah D2R-3; atas 124 baris register: 101 / 124 = 81%). *Sebelumnya* **96 / 120 = 80%** (21 Oktober 2026; atas 124 baris register: 96 / 124 = 77%). *Sebelumnya* **95 / 120 = 79%** (20 Oktober 2026 malam; atas 124 baris register: 95 / 124 = 77%). *Sebelumnya* **94 / 120 = 78%** (20 Oktober 2026 sesudah D2R; atas 124 baris register: 94 / 124 = 76%). *Sebelumnya* **93 / 120 = 78%** (20 Oktober 2026 akhir; atas 124 baris register: 93 / 124 = 75%). *Sebelumnya* **92 / 120 = 77%** (20 Oktober 2026 penutup; atas 124 baris register: 92 / 124 = 74%). *Sebelumnya* **92 / 124 = 74%** (20 Oktober 2026); tanpa tiga task yang dibatalkan: 92 / 121 = 76%. *Sebelumnya* **86 / 122 = 70%** (18 Oktober 2026 sore, sesudah uji `Down()`); tanpa tiga task yang dibatalkan: 86 / 119 = 72%. *Sebelumnya* **85 / 122 = 70%** (18 Oktober 2026 sore); tanpa tiga task yang dibatalkan: 85 / 119 = 71%. *Sebelumnya* **84 / 122 = 69%** (18 Oktober 2026, sesudah Putaran B6 dan `BE-IGD-083` masuk register; sebelum kartu baru 84 / 121 = 69%); tanpa tiga task yang dibatalkan `IGD-DEC-304`: 84 / 119 = 71%. *Sebelumnya* **82 / 121 = 68%** (17 Oktober 2026 malam); tanpa tiga task yang dibatalkan `IGD-DEC-304`: 82 / 118 = 69%. *Sebelumnya* 81 / 121 = 67%. Lima task terblokir menunggu keputusan (`BE-IGD-042`, `071`, `FE-IGD-053`, `057`, `058`) tetap dihitung karena masih dalam lingkup |

Persentase turun dari 78%/45% bukan karena ada yang mundur, melainkan karena penyebutnya
bertambah task baru. Tiga area R3.5 (penunjang medis, pemakaian alat, billing IGD) **tidak**
masuk penyebut karena belum punya task. Persentase ini bukan ukuran kesiapan produksi.

## Status contract

`DRAFT` has identity but incomplete intake. `DISCOVERY` is collecting decisions/evidence. `READY` means planned phases may start. `PARTIAL` means at least one phase is ready while another is blocked or unknown. `BLOCKED` means no material phase can safely proceed. `IN_PROGRESS` has authorized active work. `VERIFYING` awaits readiness evidence. `DONE` requires appropriate verification evidence. `SUPERSEDED` records the successor blueprint.

Phase statuses are `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, and `SUPERSEDED`. A phase becomes `DONE` only when its acceptance/readiness evidence is recorded; file existence is insufficient.
