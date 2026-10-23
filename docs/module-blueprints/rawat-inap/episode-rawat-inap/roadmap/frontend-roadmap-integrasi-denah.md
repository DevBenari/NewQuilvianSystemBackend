# Roadmap Frontend Integrasi Denah ↔ Bed Management

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | RWI-BP-001 / `episode-rawat-inap` (COMPOSITE; peta menu pada [module map 10.2](../../02-module-map.md)) |
| planning_id / revision / tanggal | `DNH-DEL-20261016-01` / 1 / 2026-10-16 Asia/Jakarta |
| status / approval | Desain disetujui: manifest anak revision 12, kontrak `0.13.0` — Muhammad Hamzah, 2026-10-16 (`RWI-DEC-324`). Setiap task dieksekusi hanya lewat `build-module-frontend` dengan task ID-nya |
| **Aturan keras !!!** | **`RWI-DEC-325` — berlaku untuk seluruh task di roadmap ini.** (1) References skill `build-module-frontend` **tidak wajib diikuti**. (2) Bed Management **fleksibel**; prioritasnya tampilan yang **sempurna dan bagus**. (3) Tampilan floor plan yang sudah didesain rapi **tidak boleh diubah**. (4) **Tidak wajib memakai base component**, karena tampilan Bed Management akan menjadi contoh untuk tampilan lain. Rincian dan batasnya: [frontend 16.7](../03-frontend-architecture.md) |
| blueprint / kontrak | [Frontend 16](../03-frontend-architecture.md), [API 14](../contracts/api-contract.md), [hak akses 12](../contracts/permission-audit-matrix.md), [validasi 17](../contracts/validation-matrix.md), [flowchart 15–18](../flowcharts/15-denah-pilih-bed.md), [PRD 27](../04-prd-to-mvp.md) |
| input revisions | Decision log 51; gate 1.13 `DNH-RCG-20261015-01`; module map 7; impact scan `BM-IMP-20261015-01` rev 1 |
| source SHA frontend / backend | `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` / `ca09cae909597442385ce4513be9c803549be10e` |
| hash lock | [integrasi-denah-delivery-lock.json](integrasi-denah-delivery-lock.json) |
| scope / ID | 8 task `FE-RWI-246` s.d. `253`. Ruang ID dipindai global 2026-10-16: terpakai sampai `FE-RWI-245`; berikutnya `FE-RWI-254` |
| roadmap pasangan | [Backend](backend-roadmap-integrasi-denah.md) · [Traceability](requirement-traceability-integrasi-denah.md) |

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

```text
                      BE-RWI-239 ✅ [BE] ─┐         BE-RWI-240 ✅ [BE] ─┐
                                          │                             │
FE-RWI-246 ✅ ──────┬─> FE-RWI-247 ✅ ─┬─────┴─> FE-RWI-248 ✅ ─> FE-RWI-249 ✅ ─┴─> FE-RWI-253 ✅
                    │               │
BE-RWI-238 ✅ [BE] ─┘               │
                                    │
                                    └───────────────────┬─┬─> FE-RWI-250 ✅ ─┬─> FE-RWI-251 ✅
                                                        │ │               │
BE-RWI-235 ✅ [BE] ─────────────────────────────────────┘ │               │
                                                          │               │
BE-RWI-236 ✅ [BE] ───────────────────────────────────────┘               │
                                                                          │
BE-RWI-237 ✅ [BE] ───────────────────────────────────────────────────────┘

BE-RWI-234 ✅ [BE] ─> FE-RWI-252 ✅
```

`[BE]` = cermin baca-saja task pada [backend-roadmap-integrasi-denah.md](backend-roadmap-integrasi-denah.md); tandanya disalin dari roadmap itu. Garis selalu prasyarat → task. `FE-RWI-249` menunggu `FE-RWI-248` karena memakai mode Pilih pop up yang dibangun di sana; `BE-RWI-239` sudah selesai lebih dulu lewat prasyarat itu. Persetujuan desain `RWI-DEC-324` berlaku untuk seluruh task, sehingga tidak digambar sebagai gerbang.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `FE-RWI-246` ✅ — [laporan](../task/report/frontend/FE-RWI-246.md) |
| 1 | `BE-RWI-234` [BE] | `FE-RWI-252` ✅ — [laporan](../task/report/frontend/FE-RWI-252.md) |
| 2 | `FE-RWI-246`, `BE-RWI-238` [BE] | `FE-RWI-247` ✅ — [laporan](../task/report/frontend/FE-RWI-247.md) |
| 3 | `FE-RWI-247`, `BE-RWI-239` [BE] | `FE-RWI-248` ✅ — [laporan](../task/report/frontend/FE-RWI-248.md) |
| 3 | `FE-RWI-247`, `BE-RWI-235` [BE], `BE-RWI-236` [BE] | `FE-RWI-250` ✅ — [laporan](../task/report/frontend/FE-RWI-250.md) |
| 4 | `FE-RWI-248` | `FE-RWI-249` ✅ — [laporan](../task/report/frontend/FE-RWI-249.md) |
| 4 | `FE-RWI-250`, `BE-RWI-237` [BE] | `FE-RWI-251` ✅ — [laporan](../task/report/frontend/FE-RWI-251.md) |
| 5 | `FE-RWI-249`, `BE-RWI-240` [BE] | `FE-RWI-253` ✅ — [laporan](../task/report/frontend/FE-RWI-253.md) |

FR-DNH-004 (hitungan boks rawat gabung pada badge) berstatus OPEN DECISION karena RWI-OQ-144, sehingga belum punya task dan tidak masuk gelombang mana pun. Task-nya dibuat sesudah pemilik menjawab. Sampai saat itu badge memakai `Counts` dari server apa adanya, dan boks tampil sebagai baris tersendiri di pop up.

## Register Task

| Task ID / status | Outcome | Requirement / decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-246` ✅ [laporan](../task/report/frontend/FE-RWI-246.md) | Denah menjadi komponen bersama tanpa perubahan tampilan | FR-DNH-001; RWI-DEC-299, 325 | 0.13.0 approved; lock DNH-DEL-20261016-01 | `DNH-CAP-34` | Pemindahan komponen dan geometri + peta kunci netral kelas | — | RWI-AC-436 (fondasi); DNH-AT-911 | V-FE + bukti tampilan sama | Tampilan peta adalah invariant pemilik | D-FE |
| `FE-RWI-247` ✅ [laporan](../task/report/frontend/FE-RWI-247.md) | Monitoring Bed memakai denah dan pop up kamar | FR-DNH-001, 002, 003, 014; RWI-DEC-287, 299, 303, 316, 320, 325 | 0.13.0 approved | `DNH-CAP-09`, `10`, `12`, `22`, `35`, `36` | Satu layar existing + pop up mode Lihat | `FE-RWI-246`, `BE-RWI-238` | RWI-AC-436, 442, 443, 453 | V-FE PASS; UAT-801, 802 NOT RUN | Privasi Housekeeping | D-FE |
| `FE-RWI-248` ✅ [laporan](../task/report/frontend/FE-RWI-248.md) | Langkah Pilih Bed admisi dan admisi bayi memakai denah | FR-DNH-005; RWI-DEC-299, 316, 325 | 0.13.0 approved | `DNH-CAP-13`, `18`, `37` | Dua langkah admisi existing + pop up mode Pilih | `FE-RWI-247`, `BE-RWI-239` | RWI-AC-436, 449, 453, 454 | V-FE PASS; UAT-803, 804 NOT RUN | Berkas admisi diubah pekerjaan paralel issue 022 | D-FE |
| `FE-RWI-249` ✅ [laporan](../task/report/frontend/FE-RWI-249.md) | Tujuan transfer dipilih lewat denah | FR-DNH-005, 007; RWI-DEC-312, 316, 325 | 0.13.0 approved | `DNH-CAP-16`, `18`, `37` | Tiga konsumen transfer existing; memakai mode Pilih dari `FE-RWI-248` | `FE-RWI-248` | RWI-AC-436, 450, 453 | V-FE PASS; UAT-804 NOT RUN | Kategori kelas khusus | D-FE |
| `FE-RWI-250` ✅ [laporan](../task/report/frontend/FE-RWI-250.md) | Master Bed menjadi editor denah | FR-DNH-008, 009, 010; RWI-DEC-304, 306, 313, 317, 318, 325 | 0.13.0 approved | `DNH-CAP-24`, `26`, `41` | Satu layar menu existing + pop up mode Master | `FE-RWI-247`, `BE-RWI-235`, `BE-RWI-236` | RWI-AC-436, 444, 451, 454, 455, 456, 457 | V-FE PASS; UAT-805, 806 NOT RUN | Seluruh tim MasterData | D-FE |
| `FE-RWI-251` ✅ [laporan](../task/report/frontend/FE-RWI-251.md) | Susunan denah diimpor dari layar Master Bed | FR-DNH-011, 012; RWI-DEC-306, 314, 321, 323, 325, 326 | 0.13.0 approved | `DNH-CAP-27` | Satu layar anak | `FE-RWI-250`, `BE-RWI-237` | RWI-AC-446, 451, 459, 461 | V-FE PASS; UAT-807, 808 NOT RUN | Menjalankan impor = D-DNH-3 | D-FE |
| `FE-RWI-252` ✅ [laporan](../task/report/frontend/FE-RWI-252.md) | Panel High/Low Dependency | FR-DNH-013, 014; RWI-DEC-302, 303, 325 | 0.13.0 approved | `DNH-CAP-19` | Satu komponen panel di tiga tempat | `BE-RWI-234` | RWI-AC-440, 441, 443 | V-FE PASS; UAT-809, 810 NOT RUN | Berkas langkah Dokter admisi diubah pekerjaan paralel issue 022 | D-FE |
| `FE-RWI-253` ✅ [laporan](../task/report/frontend/FE-RWI-253.md) | Sisa tampilan bed lama dan isian jenis kelamin dihapus | FR-DNH-003, 006; RWI-DEC-299, 315, 325 | 0.13.0 approved | `DNH-CAP-38`, `39`, `40` | Penghapusan berkas lama + isian/pesan jenis kelamin | `FE-RWI-249`, `BE-RWI-240` | RWI-AC-436, 439, 452 | V-FE + DNH-AT-912 PASS | Breaking; rilis bersama `BE-RWI-240` | D-FE |

## Batas Handoff dan Definition of Done

### Aturan keras !!! — `RWI-DEC-325`

> 1. References skill `build-module-frontend` — katalog dan gerbang keputusan base component, design
>    token, pola komposisi halaman, standar fitur master data, checklist konsistensi UI, profil proyek,
>    dan kebijakan test — **tidak wajib diikuti** untuk task di roadmap ini.
> 2. Bed Management **fleksibel**; prioritasnya tampilan yang **sempurna dan bagus**.
> 3. Tampilan floor plan yang sudah didesain rapi **tidak boleh diubah**. Yang boleh berubah hanya
>    permintaan pemilik: tanpa garis merah kamar penuh, outline hijau kamar yang bisa dipilih, badge,
>    dan daftar bed di pop up (RWI-DEC-303, 316).
> 4. **Tidak wajib memakai base component**, karena tampilan Bed Management akan menjadi contoh untuk
>    tampilan lain.

Tetap mengikat: privasi penghuni (RWI-DEC-287), tanpa jenis kelamin (RWI-DEC-315), Guid tidak ditampilkan maupun diketik (relasi lewat daftar pilihan berlabel nama + kode), tombol simpan tidak dinonaktifkan karena isian kosong, kontrak API `0.13.0`, stack Next.js App Router JavaScript/JSX dengan Redux dan Axios `InstanceAxios`, dan struktur route → view → hook → service. Komponen baru tinggal di folder feature denah; membuat base component baru di `base-features` tetap butuh permintaan eksplisit (`AGENTS.md` frontend).

### D-FE — bukti wajib setiap task frontend

Periksa `AGENTS.md`/`CLAUDE.md` frontend saat eksekusi, branch dan wewenang tulis, hash masukan pada lock, serta status task backend yang ditunggu. V-FE adalah: review diff/scope, `npm run lint:errors` (0 error), dan `npm run build`. Suite `npm run test:unit` existing dijalankan bila waktu memungkinkan dan dibandingkan dengan garis dasarnya; menulis test baru tidak wajib. Uji peramban, E2E, dan test `.mjs` **tidak menahan status selesai** atas keputusan pemilik 1 September 2026, tetapi langkah yang tidak dijalankan ditulis `NOT RUN` apa adanya.

Bukti tampilan denah tidak berubah: diff yang menunjukkan markup dan CSS denah dipindah tanpa perubahan selain import dan props, ditambah screenshot setiap lantai sebelum/sesudah bila lingkungan tersedia. Verifikasi dengan data nyata butuh langkah D-DNH-3 (impor di DB Hamzah) pada [roadmap backend](backend-roadmap-integrasi-denah.md#langkah-operasional-di-luar-task).

D-FE terpenuhi bila setiap acceptance criteria kartu terpetakan ke source yang benar-benar ada, status server menjadi sumber tampilan, UI tidak membocorkan data, laporan tracked tersedia di `task/report/frontend/<TASK-ID>.md`, dan kartu, register, grafik, gelombang, serta traceability diperbarui bersama laporannya.

## Kartu Task

### ✅ FE-RWI-246 — Denah menjadi komponen bersama tanpa perubahan tampilan

| Field | Isi |
| --- | --- |
| Status | ✅ **Selesai 16 Oktober 2026.** Source, test registry 2/2, `npm run lint:errors`, build produksi, dan bukti blob CSS/geometri identik PASS. Screenshot denah `NOT RUN` karena browser lokal berhenti di verifikasi sesi; hal ini tidak menahan selesai berdasarkan D-FE. |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Satu komponen denah siap dipakai Monitoring, Pilih Bed, transfer, dan Master Bed, dengan tampilan persis sama |
| Ukuran / batas | Pemindahan berkas dan antarmuka komponen; tanpa data API |
| Jejak | FR-DNH-001; RWI-DEC-299, 325; `DNH-MIS-06`; DNH-AT-911; NFR-801, 807 |
| Desain / kontrak | Frontend 16.2 (FE-DNH-01), 16.4, 16.5, 16.7 |
| Reuse / disposisi | `DNH-CAP-34` Reuse with adapter: `floor-plan-drawing`, `floor-plan-room`, `floor-plan-view`, `zoom-toolbar`, `floor-plan.module.css` dipindah apa adanya |
| Cakupan | Pindahkan komponen ke `src/components/features/health-services/inpatient-management/floor-plan/` dan geometri (`lantai-*.plan.js`, `floors.js`, `plan-primitives.js`) ke `src/lib/constants/health-services/inpatient-management/floor-plan/`. Tambah peta kunci denah netral kelas untuk seluruh 106 kamar dan ruang cadangan (`lantai-3/k-329`, `lantai-3/k-321b`, `lantai-2/icu-ck`, `lantai-3/cadangan-1`) beserta nomor kamar/kode ruang pendek. Komponen menerima data kamar per kunci, mode (Lihat/Pilih/Master), dan callback klik; bentuk tanpa data tampil seperti ruang tanpa label sekarang. Halaman dummy sementara memakai komponen yang dipindah agar tampilan dapat dibandingkan |
| Batas source | `src/components/features/health-services/inpatient-management/floor-plan/*`; `src/lib/constants/health-services/inpatient-management/floor-plan/*`; `src/app/health-services/inpatient-management/floor-plan/*` (sementara) |
| Dependency langsung | — |
| Pekerjaan independen | Tidak menunggu backend |
| Risiko / owner / aktivasi | Perubahan tampilan sekecil apa pun melanggar RWI-DEC-299; owner frontend |
| Verifikasi yang wajib | V-FE + bukti tampilan sama (DNH-AT-911); test unit peta kunci dianjurkan, tidak wajib |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-246.md` |
| Laporan | [FE-RWI-246.md](../task/report/frontend/FE-RWI-246.md) |

Acceptance positif:

- Setiap lantai (2, 3, 4, 5, 8, 9) tampil sama sebelum dan sesudah pemindahan; layar 390 px tidak bergeser horizontal.
- Seluruh 106 kamar dan ruang cadangan punya kunci denah netral kelas yang unik.

Acceptance negatif:

- Tidak ada kunci yang memuat nama kelas (mis. `svip-318`); tidak ada perubahan warna, garis, atau tata letak peta.

### ✅ FE-RWI-247 — Monitoring Bed memakai denah dan pop up kamar

| Field | Isi |
| --- | --- |
| Status | ✅ **Selesai 16 Oktober 2026.** Monitoring memakai satu request denah per lantai; badge/outline/popup, privacy Housekeeping, aksi cleaning/availability/riwayat, stale guard, retry, dan penghapusan render kartu/tabel terbukti pada source/test. Lint dan build PASS; UAT browser `NOT RUN` sesuai D-FE. |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Tab Monitoring Bed menampilkan denah dengan status kamar, badge, outline hijau, dan pop up berisi setiap bed beserta aksinya |
| Ukuran / batas | Satu tab existing + pop up mode Lihat |
| Jejak | FR-DNH-001, 002, 003, 014; RWI-DEC-287, 299, 303, 316, 320, 325; RWI-AC-436, 442, 443, 453; NFR-802 |
| Desain / kontrak | Frontend 16.2 (FE-BM-01, FE-DNH-02), 16.3, 16.5; API 14.2 Bed Management `GET /floor-plan` dan endpoint pembersihan existing; flowchart 00 |
| Reuse / disposisi | `DNH-CAP-09`, `22` Reuse (aksi pembersihan, `use-bed-management`); `DNH-CAP-10`, `36` baru; `DNH-CAP-35` label kelas resmi dari API |
| Cakupan | Hook `use-floor-plan.js` + fungsi `getFloorPlan` pada `bed-management.service.js`; Monitoring merender denah per lantai (tab lantai, lantai terakhir disimpan di browser dengan try/catch). Kamar: badge `kapasitas/terisi` dari `Counts`, outline hijau dari `IsSelectable`, kamar Penuh tanpa merah dan tetap dapat diklik. Pop up kamar mode Lihat memakai kartu pop up denah yang sekarang (judul, garis aksen kelas, baris rincian) ditambah daftar bed: label, titik status berwarna legenda, penghuni dan HD/LD sesuai respons, sisa waktu pesanan, aksi mulai/selesai/sahkan pembersihan, tutup/buka, riwayat. Tampilan kartu dan tabel Pemantauan dihapus dari `bed-management-view.jsx` (RWI-DEC-320) |
| Batas source | `src/components/view/health-services/inpatient-management/bed-management/*`; `src/components/features/health-services/inpatient-management/floor-plan/*`; `src/lib/hooks/health-services/inpatient-management/{use-floor-plan.js, use-bed-management.js}`; `src/lib/services/health-services/inpatient-management/bed-management.service.js` |
| Dependency langsung | `FE-RWI-246`, `BE-RWI-238` |
| Pekerjaan independen | Kerangka pop up dapat disiapkan dari kontrak 14.3 sebelum endpoint siap |
| Risiko / owner / aktivasi | Privasi: identitas hanya dari respons server, tidak diisi ulang dari cache. Data nyata butuh D-DNH-3 |
| Verifikasi yang wajib | V-FE + UAT-801, 802 |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-247.md` |
| Laporan | [FE-RWI-247.md](../task/report/frontend/FE-RWI-247.md) |

Acceptance positif:

- Kamar 3 bed Terisi/Dipesan/Tersedia tampil badge `3/2` dengan outline hijau; kamar Penuh `3/3` tanpa merah dan pop up penghuninya tetap terbuka.
- Pop up kamar 329 menampilkan "High Dependency (1 HD · 1 LD)" dan aksi pembersihan berjalan dari pop up lalu denah dimuat ulang.

Acceptance negatif:

- Akun Housekeeping tidak melihat nama, No. RM, maupun HD/LD.
- Gagal muat menampilkan pesan dan tombol muat ulang; respons lama tidak menimpa lantai yang baru dipilih.

### ✅ FE-RWI-248 — Langkah Pilih Bed admisi dan admisi bayi memakai denah

| Field | Isi |
| --- | --- |
| Status | ✅ **Selesai 17 Oktober 2026.** Mode Pilih admisi biasa/bayi, eligibility server, expected version terbaru, race 409, ambiguous recovery, test fokus 21/21, lint, dan build produksi PASS. UAT-803/804 `NOT RUN` karena server/sesi/fixture aktual tidak tersedia; hal ini tidak menahan selesai berdasarkan D-FE. |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Petugas admisi memilih bed lewat denah; hanya kamar dengan bed layak untuk pasien itu yang ber-outline hijau |
| Ukuran / batas | Dua langkah admisi existing + pop up mode Pilih |
| Jejak | FR-DNH-005; RWI-DEC-299, 316, 325; RWI-AC-436, 449, 453, 454 |
| Desain / kontrak | Frontend 16.2, 16.3; API 14.2 Bed Occupancy `GET /floor-plan` (`Admission`), `POST /reservations`; flowchart 15 |
| Reuse / disposisi | `DNH-CAP-13`, `18` Reuse with adapter (`use-inpatient-admission-bed`, versi bed, kunci idempotensi); `DNH-CAP-37` baru |
| Cakupan | `inpatient-admission-bed-step.jsx` dan `inpatient-newborn-admission-steps.jsx` mengganti papan kartu dengan denah mode Pilih (`purpose=Admission`, episode dari langkah sebelumnya). Bed layak menampilkan "Pilih bed ini"; bed tidak layak menampilkan alasannya; konfirmasi memesan bed dengan versi dari respons terbaru; langkah admisi menampilkan "Kelas 2 329 · bed 329-C"; 409 memuat ulang lantai; hasil belum pasti diperiksa sebelum mengulang |
| Batas source | `src/components/view/health-services/inpatient-management/{inpatient-admission-bed-step.jsx, inpatient-newborn-admission-steps.jsx}`; `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`; `src/lib/services/health-services/inpatient-management/bed-occupancy.service.js`; komponen denah |
| Dependency langsung | `FE-RWI-247`, `BE-RWI-239` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | Berkas admisi sedang diubah pekerjaan paralel PLAN-REPAIR-EPS-022; kerjakan di atas hasil yang sudah di-commit, jangan menimpanya |
| Verifikasi yang wajib | V-FE + UAT-803, 804 |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-248.md` |
| Laporan | [FE-RWI-248.md](../task/report/frontend/FE-RWI-248.md) |

Acceptance positif:

- Pasien tanpa isolasi memilih 329-C → bed Dipesan, langkah admisi menampilkan "Kelas 2 329 · bed 329-C".
- Bayi rawat gabung hanya dapat memilih boks di kamar ibunya; pasien isolasi hanya melihat 319 ber-outline.

Acceptance negatif:

- Dua petugas memilih bed yang sama → petugas kedua diminta memilih ulang sesudah denah dimuat ulang.
- Tidak ada pesan atau tampilan jenis kelamin.

### FE-RWI-249 — Tujuan transfer dipilih lewat denah

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Tab Bed Transfer, transfer di detail episode, dan panel transfer Workspace Keperawatan memilih tujuan lewat denah |
| Ukuran / batas | Tiga konsumen transfer existing; mode Pilih pop up dari `FE-RWI-248` dipakai ulang |
| Jejak | FR-DNH-005, 007; RWI-DEC-312, 316, 325; RWI-AC-436, 450, 453 |
| Desain / kontrak | Frontend 16.2 (FE-BM-02), 16.3; API 14.2 Bed Occupancy `GET /floor-plan` (`Transfer`), `GET /transfer-context`, `POST /placements/transfer`; flowchart 15 |
| Reuse / disposisi | `DNH-CAP-16`, `18`, `37`; `use-inpatient-bed-transfer` existing |
| Cakupan | `inpatient-bed-transfer.jsx` (tab Bed Transfer dan detail episode) dan `nursing-transfer-form-panel.jsx` memakai denah mode Pilih (`purpose=Transfer`). Asal tetap otomatis; pilihan kategori mengikuti `AllowedManualCategories` (kelas khusus: ketiganya); alasan wajib; konfirmasi memakai versi dari respons terbaru |
| Batas source | `src/components/features/health-services/inpatient-management/inpatient-bed-transfer.jsx`; `src/components/view/health-services/inpatient-management/nursing-workspace/sections/transfer/nursing-transfer-form-panel.jsx`; hook transfer; komponen denah |
| Dependency langsung | `FE-RWI-248` |
| Pekerjaan independen | Mode Pilih pop up dibangun pada FE-RWI-248; data denah tujuan transfer berasal dari BE-RWI-239 yang sudah selesai lewat prasyarat itu |
| Risiko / owner / aktivasi | Bukti kelas khusus nyata butuh D-DNH-1 |
| Verifikasi yang wajib | V-FE + UAT-804 |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-249.md` |
| Laporan | [FE-RWI-249.md](../task/report/frontend/FE-RWI-249.md) |

Acceptance positif:

- Transfer Kelas 3 → ICU menampilkan Up, Down, dan Same Grade; pilihan apa pun tersimpan.
- Asal Menunggu Pembersihan dan tujuan Terisi sesudah simpan; denah dimuat ulang.

Acceptance negatif:

- Kelas 2 → Kelas 1 dicatat Down Grade ditolak dengan pesan server; input petugas tetap utuh.

### FE-RWI-250 ✅ — Master Bed menjadi editor denah

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai — V-FE PASS; UAT-805/806 NOT RUN |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Admin mengelola kamar dan bed langsung dari denah |
| Ukuran / batas | Satu butir menu existing + pop up mode Master |
| Jejak | FR-DNH-008, 009, 010; RWI-DEC-304, 306, 313, 317, 318, 325; RWI-AC-436, 444, 451, 454, 455, 456, 457 |
| Desain / kontrak | Frontend 16.2 (FE-DNH-03), 16.3; API 14.2 Room (`GET /floor-plan-layout`, `POST /floor-plan-layout/rooms`, `PUT /{id}`) dan Bed (`POST /`, `PUT /{id}`, `PATCH /{id}/status`, `PATCH /{id}/availability`, `DELETE /{id}`); flowchart 16 |
| Reuse / disposisi | `DNH-CAP-41` Reuse with adapter (`use-bed-master-confirmation`, `bed-master-mutation.service`); `DNH-CAP-24`, `26` |
| Cakupan | `bed-client.jsx` merender view baru `master-data-bed-floor-plan-view.jsx`; daftar Master Bed lama diganti. Pop up mode Master: ubah kelas (alasan dan versi bed terdampak), ubah arah wing, tambah bed atau boks (label dari server), kurangi bed (hapus bila tanpa riwayat, nonaktif bila beriwayat), ubah label, tutup/buka. Ruang cadangan abu-abu dengan form aktivasi. Daftar "Kamar tanpa posisi di denah" menautkan halaman bed/kamar existing. Kelas dan unit dipilih dari daftar berlabel nama + kode. Service baru `floor-plan-layout.service.js` dan hook `use-floor-plan-layout.js` |
| Batas source | `src/app/health-services/master-data/bed/bed-client.jsx`; `src/components/view/health-services/master-data/bed/*`; `src/lib/hooks/health-services/master-data/bed/use-floor-plan-layout.js`; `src/lib/services/health-services/master-data/floor-plan-layout.service.js`; komponen denah |
| Dependency langsung | `FE-RWI-247`, `BE-RWI-235`, `BE-RWI-236` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | Halaman detail/tambah/ubah bed existing dipertahankan untuk bed di kamar tanpa kunci; seluruh tim MasterData |
| Verifikasi yang wajib | V-FE + UAT-805, 806 |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-250.md` |
| Laporan | [FE-RWI-250.md](../task/report/frontend/FE-RWI-250.md) |

Acceptance positif:

- Tambah bed di 329 berpenghuni → 329-D muncul, badge `4/2`, bed menunggu pengesahan.
- Ubah arah wing tersimpan; aktivasi ruang cadangan 324 Kelas 2 dengan 3 bed memunculkan "Kelas 2 324" di semua denah.

Acceptance negatif:

- Ubah kelas kamar berpenghuni menampilkan "Kelas 2 329 masih dihuni 2 pasien"; tombol tidak dinonaktifkan lebih dulu, server yang menolak.
- Hapus bed beriwayat menyarankan nonaktif; nomor ganda dan label ganda ditolak dengan pesan server.

### FE-RWI-251 — Susunan denah diimpor dari layar Master Bed

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai — V-FE PASS; UAT-807/808 NOT RUN |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Admin impor menyusun manifest dari denah, memetakan unit, memeriksa, lalu mengimpor |
| Ukuran / batas | Satu layar anak dari Master Bed |
| Jejak | FR-DNH-011, 012; RWI-DEC-306, 314, 321, 323, 325, 326; RWI-AC-446, 451, 459, 461 |
| Desain / kontrak | Frontend 16.2 (FE-DNH-04), 16.3; API 14.2 Room `POST /floor-plan-layout/import`, 14.3 `FloorPlanImportRequest`/`FloorPlanImportResult`; flowchart 17 |
| Reuse / disposisi | `DNH-CAP-27`; geometri dan peta kunci dari `FE-RWI-246` |
| Cakupan | `floor-plan-import-view.jsx` + hook `use-floor-plan-import.js`: tombol "Impor susunan denah" hanya untuk Room : ImportFloorPlan; tabel kamar dari data denah (lantai, kunci, nomor, nama untuk kamar tanpa nomor, kelas dicocokkan dengan master menurut nama resmi, jumlah bed dan boks, arah bawaan dihitung kisi 3 × 3 dari geometri sesuai RWI-DEC-314); pemetaan unit per kamar dan sekaligus per lantai; pilihan kamar existing (opsional); "Periksa" (dry run) menampilkan ringkasan dan alasan per kamar; "Impor"; teks akibat lingkungan pada kesiapan bed |
| Batas source | `src/components/view/health-services/master-data/bed/floor-plan-import-view.jsx`; `src/lib/hooks/health-services/master-data/bed/use-floor-plan-import.js`; `floor-plan-layout.service.js`; konstanta denah |
| Dependency langsung | `FE-RWI-250`, `BE-RWI-237` |
| Pekerjaan independen | Fungsi arah bawaan dan penyusun manifest dapat diuji unit tanpa backend |
| Risiko / owner / aktivasi | Menekan Impor di lingkungan nyata adalah langkah D-DNH-3 dengan wewenang terpisah |
| Verifikasi yang wajib | V-FE + UAT-807, 808 |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-251.md` |
| Laporan | [`FE-RWI-251.md`](../task/report/frontend/FE-RWI-251.md) — source/unit/lint/build PASS; 106 kamar, 168 bed, 8 boks; UAT aktual NOT RUN |

Acceptance positif:

- Arah bawaan: 330 Barat Laut, 319 Timur Laut, 301 Barat Daya, 309 Selatan, 318 Tenggara.
- Pemeriksaan menampilkan jumlah dibuat/diperbarui/tanpa perubahan/ditolak beserta alasan; impor ulang menampilkan 0 dibuat.

Acceptance negatif:

- Kamar tanpa unit ditandai ditolak dengan alasannya; tombol Impor tetap dapat ditekan untuk kamar yang lolos.

### FE-RWI-252 — Panel High/Low Dependency

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai — V-FE PASS; UAT-809/810 NOT RUN |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Perawat penanggung jawab dan DPJP mencatat HD/LD; petugas admisi mencatat nilai awal; riwayat dapat dibaca |
| Ukuran / batas | Satu komponen panel di tiga tempat |
| Jejak | FR-DNH-013, 014; RWI-DEC-302, 303, 325; RWI-AC-440, 441, 443 |
| Desain / kontrak | Frontend 16.2 (FE-DNH-05), 16.3; API 14.2 Inpatient Episode; flowchart 18 |
| Reuse / disposisi | `DNH-CAP-19` baru |
| Cakupan | `care-dependency-panel.jsx` + `use-care-dependency.js`: nilai saat ini, sumber, pencatat, waktu; pilihan High/Low + catatan; `ExpectedSetAt` dari data terakhir; riwayat. Ditempatkan di `nursing-quick-summary-panel.jsx`, `inpatient-episode-detail-view.jsx`, dan isian catatan awal di `inpatient-admission-doctor-step.jsx` (episode Draft). Pesan 403/409/422 dari server |
| Batas source | `src/components/features/health-services/inpatient-management/care-dependency-panel.jsx`; `src/lib/hooks/health-services/inpatient-management/use-care-dependency.js`; tiga berkas penempatan di atas |
| Dependency langsung | `BE-RWI-234` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | `inpatient-admission-doctor-step.jsx` sedang diubah pekerjaan paralel PLAN-REPAIR-EPS-022; aktivasi produksi `DNH-G04` |
| Verifikasi yang wajib | V-FE + UAT-809, 810 |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-252.md` |
| Laporan | [FE-RWI-252.md](../task/report/frontend/FE-RWI-252.md) |

Acceptance positif:

- Admisi Low → perawat High → DPJP Low: panel menampilkan nilai terbaru dan riwayat tiga baris.

Acceptance negatif:

- Perawat bukan penanggung jawab menerima pesan 403; tab kedua yang basi menerima 409 dan diminta muat ulang; episode Closed hanya baca.

### ✅ FE-RWI-253 — Sisa tampilan bed lama dan isian jenis kelamin dihapus

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai — V-FE dan DNH-AT-912 PASS |
| **Aturan keras !!!** | Berlaku `RWI-DEC-325`: references `build-module-frontend` dan base component tidak wajib; tampilan sempurna; **floor plan tidak boleh diubah** |
| Slice / outcome | Tidak ada lagi tampilan bed kedua, halaman dummy, maupun isian dan pesan jenis kelamin |
| Ukuran / batas | Penghapusan berkas lama + penyesuaian test |
| Jejak | FR-DNH-003, 006; RWI-DEC-299, 315, 325; RWI-AC-436, 439, 452; DNH-AT-912; `DNH-MIS-08`, `09` |
| Desain / kontrak | Frontend 16.4 (berkas dihapus); API 14.1 (field jenis kelamin hilang) |
| Reuse / disposisi | `DNH-CAP-38` Reuse (endpoint dan service `bed-board` untuk daftar pasien tetap); `DNH-CAP-39`, `40` Repair |
| Cakupan | Hapus `inpatient-bed-board.jsx` beserta CSS module-nya dan `use-inpatient-bed-board.jsx` bila tidak ada konsumen lain; hapus halaman dummy `floor-plan/` beserta `_data/*.occupancy.js` dan `room-classes.js`; hapus isian dan pesan jenis kelamin di `room-constants.jsx`, `room-utils.jsx`, `bed-constants.jsx`, `bed-utils.jsx`, `inpatient-placement-utils.jsx`, dan form master; sesuaikan atau hapus test yang mengunci perilaku lama. Dirilis bersama `BE-RWI-240` |
| Batas source | Berkas yang disebut di atas; `tests/unit/*` terkait |
| Dependency langsung | `FE-RWI-249`, `BE-RWI-240` |
| Pekerjaan independen | Inventaris konsumen dapat disiapkan lebih dulu. FE-RWI-249 adalah konsumen terakhir papan kartu; FE-RWI-248 sudah selesai lewat prasyarat itu |
| Risiko / owner / aktivasi | Breaking; daftar pasien rawat inap (`use-inpatient-episode-worklist.jsx`) tetap memanggil `bed-board` |
| Verifikasi yang wajib | V-FE + DNH-AT-912 |
| Definition of Done | D-FE; laporan `task/report/frontend/FE-RWI-253.md` |
| Laporan | [FE-RWI-253.md](../task/report/frontend/FE-RWI-253.md) |

Acceptance positif:

- Pencarian source tidak menemukan `IsForMale`, `IsForFemale`, atau pesan jenis kelamin bed; daftar pasien rawat inap tetap memuat lokasi bed.

Acceptance negatif:

- Tidak ada konsumen yang masih mengimpor komponen kartu bed yang dihapus; route dummy denah tidak dapat dibuka.
