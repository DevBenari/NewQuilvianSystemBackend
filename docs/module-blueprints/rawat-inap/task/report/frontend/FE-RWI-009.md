# FE-RWI-009 — Satu episode terbaca utuh beserta riwayatnya

- TASK ID: `FE-RWI-009`
- TASK TYPE: Implementasi layar detail frontend beserta aturan tampil tombol berpenjaga
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 10 — dua repository 2; lebih dari 20 berkas diperiksa 2; empat berkas source diubah 1; logika kompleks 2; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga kewenangan per pasien tanpa mengubahnya 1; satu layar berbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `GET /episodes/{id}`, `/status-history`, `/doctor-assignments`, `/nurse-assignments`, `PATCH /{id}/isolation-requirement`, dan `GET /placements/by-episode/{episodeId}` seluruhnya berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-009`; `03-frontend-architecture.md` bagian 2, 3, 5.2, 5.3, dan 6; `contracts/permission-audit-matrix.md` bagian 5.4 dan 6; `InpatientEpisodeController.cs`; `InpEpisodeService.Assignments.cs` baris 110–260 tempat `GUARD-INP-04` dibentuk; `InpatientActorClaims.cs`; `InpatientEpisodeDtos.cs` (`InpatientEpisodeDetailResponse`); `InpatientEpisodeReadDtos.cs` (`InpatientEpisodeCurrentLocationResponse`, `InpatientEpisodeActiveNurseResponse`); `InpatientEpisodeAssignmentDtos.cs`; `InpatientCorrectionDtos.cs` (`InpatientStatusHistoryResponse`); `InpatientBedOccupancyDtos.cs` (`BedPlacementResponse`); `InpEpisodeStatus.cs`; `login-slice.jsx` bagian `selectUserInfo` dan `getInitialUserInfo`; fondasi `FE-RWI-002`
- FILES CHANGED: `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` (baru); `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` (baru); `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` (baru); `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (baru); `src/app/health-services/inpatient-management/episodes/[id]/page.jsx` (baru); `tests/unit/inpatient-episode-detail.test.mjs` (baru); `tests/e2e/inpatient-episode-detail.spec.mjs` (baru)

## Kenapa tombol isolasi tidak boleh diturunkan dari hak akses

Mesin hak akses menjawab `SetIsolation` dengan "boleh" untuk petugas admisi **dan** untuk
dokter mana pun. Yang membedakan keduanya adalah status episode dan siapa DPJP aktifnya, dan
itu dijaga service lewat `GUARD-INP-04`. Layar yang hanya membaca hak akses akan menampilkan
tombol yang pasti ditolak server.

`resolveIsolationAuthority` karena itu menyalin cabang penjaga backend apa adanya:

| Keadaan | Layar | Keterangan yang ditampilkan |
| --- | :---: | --- |
| Petugas admisi, episode `Draft` | Aktif | — |
| Petugas admisi, episode sudah `Admitted` | Nonaktif | "Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP" |
| DPJP aktif, status apa pun sebelum selesai | Aktif | — |
| Dokter yang bukan DPJP aktif | Nonaktif | "Anda bukan DPJP episode ini" |
| Episode `Closed` atau `Cancelled` | Nonaktif | "Episode ini sudah selesai, sehingga tidak dapat diubah lagi" |

Nilai status dibaca dari kolom **angka** `episodeStatus` — disalin dari enum
`InpEpisodeStatus` (`Draft = 0`, `Admitted = 1`) — bukan dari `episodeStatusName`. Nama status
adalah kalimat yang boleh diperbaiki kapan saja; angkanya bagian dari kontrak.

- IMPLEMENTATION: (1) Hook membaca lima sumber sekaligus: detail episode, riwayat status, riwayat DPJP, riwayat perawat, dan riwayat penempatan. (2) **Lokasi terkini diturunkan dari riwayat penempatan**, yaitu baris yang belum ditutup dan bernomor urut terbesar — bukan dari kolom `currentLocation` pada episode. (3) Seluruh riwayat diurutkan menurut **nomor urut**, bukan waktu, sehingga dua baris pada detik yang sama tetap punya urutan pasti. (4) Kewenangan tombol isolasi dihitung `resolveIsolationAuthority` dari status episode dan identitas dokter pengguna (`userInfo.doctorId` dibanding `activeDoctor.doctorId`). (5) Keterangan kebutuhan isolasi hanya dirender pada layar detail ini. (6) Layar detail dijangkau dari baris census `FE-RWI-008`.
- API CONTRACT IMPACT: Tidak mengubah kontrak.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication. Membuat `GUARD-INP-04` terlihat di layar supaya tombol yang pasti ditolak tidak ditampilkan sebagai pilihan yang tersedia. Server tetap satu-satunya penentu.
- VISUAL REFERENCE: NOT REQUIRED.
- WEWENANG UI YANG DIPAKAI: "Penggabungan dengan layar lain diperbolehkan". Dipakai: `FE-INP-04` detail episode, `FE-INP-15` penetapan kebutuhan isolasi, `FE-INP-05` perpindahan (`FE-RWI-010`), dan bagian penanggung jawab (`FE-RWI-011`) digabung menjadi satu halaman `/episodes/[id]` dengan bagian bertajuk terpisah. Kalimat keterangan tombol pada bagian 3 arsitektur **tidak** termasuk `DEV_DISCRETION` dan disalin kata per kata.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Lokasi terkini dibaca dari riwayat penempatan, bukan dari kolom pada episode | **LULUS** | Server tiruan pada e2e sengaja mengisi kolom `currentLocation` episode dengan "Bed Salah / Kamar Salah" yang berbeda dari riwayat penempatannya. Layar menampilkan "Bed 1 — Melati 1 — Rawat Inap Melati" dari riwayat, dan "Bed Salah" **tidak muncul sama sekali** (`toHaveCount(0)`) |
| 2. Tombol ubah kebutuhan isolasi berperilaku mengikuti status episode | **LULUS** | e2e menjalankan keempat kombinasi di browser sungguhan. Admisi+`Draft`: sakelar aktif, tanpa keterangan. Admisi+`Admitted`: sakelar dan tombol simpan `disabled`, keterangan berbunyi persis "Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP" |
| 3. Bagi dokter yang bukan DPJP aktif, keterangannya "Anda bukan DPJP episode ini" | **LULUS** | e2e kombinasi keempat menampilkan kalimat itu persis, dan setiap kasus nonaktif juga diperiksa **tidak** memuat kalimat arah satunya — kedua keterangan tidak mungkin tertukar |
| 4. Keterangan kebutuhan isolasi hanya tampil bagi peran berhak, tidak pada daftar mana pun | **LULUS SEBAGIAN — lihat catatan** | Test unit membuktikan `isolationNote` dirender **hanya** pada layar detail, dan tidak ada pada `inpatient-census-view.jsx` maupun `inpatient-bed-board.jsx`. Bagian "peran berhak" dijaga server: `GET /episodes/{id}` menggerbangnya dengan `InpatientEpisode : Read`, dan tidak ada sinyal peran terpisah di frontend yang dapat memutuskannya tanpa menebak |
| 5. Riwayat status, DPJP, dan perawat terbaca urut | **LULUS** | Server tiruan mengirim riwayat **terbalik**; e2e membuktikan layar menampilkannya urut nomor urut (`1. Awal → Admisi sedang disiapkan`, lalu `2. … → Sedang dirawat`), termasuk riwayat penempatan (Bed 5 lalu Bed 1). Periode setiap penugasan ikut terbaca, dan yang masih berlaku ditandai "sekarang" |

- VALIDATION: e2e `tests/e2e/inpatient-episode-detail.spec.mjs` | PASS, 10/10 | TASK | termasuk keempat kombinasi kewenangan tombol isolasi dan pembacaan lokasi terkini dari riwayat penempatan
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-episode-detail.test.mjs` | PASS, 16/16 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `ƒ /health-services/inpatient-management/episodes/[id]` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 106, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` mengimpor `base-login-utils.jsx` sedangkan berkasnya `.js`; sudah tercatat rusak sejak `FE-RWI-006`
- MANUAL TEST: NOT FEASIBLE — keempat kombinasi kriteria 2 dan 3 memerlukan empat akun berbeda: petugas admisi, DPJP episode itu, dokter lain, dan pengguna dengan episode yang masih `Draft`. Tidak ada satu pun akun seperti itu yang tersedia tanpa database tim. Keempatnya dijalankan di browser sungguhan (Edge) lewat e2e dengan klaim `doctor_id` dan peran yang berbeda per kasus
- WARNINGS: Kriteria 4 bagian "peran berhak" bertumpu pada server. Bila suatu hari `GET /episodes/{id}` mengirim `isolationNote` kepada peran yang tidak berhak, layar akan menampilkannya — frontend tidak punya sinyal peran terpisah untuk kolom itu, dan menebaknya justru akan membuat layar dan server berselisih. Pemilik privasi modul ini **belum ditunjuk** (`contracts/permission-audit-matrix.md` bagian 6), sehingga aturan tertulis yang berlaku
- KNOWN ISSUES: Tidak ada cacat implementasi yang diketahui pada scope task
- DEPENDENCY BACKEND: `BE-RWI-009` dan `BE-RWI-014` — seluruh endpoint yang dipakai berstatus ✅ `Tersedia` dan terbukti berjalan 26 Agustus 2026. Keduanya masih 🟡 karena kriteria **403** belum terbukti dengan akun yang login tanpa butir hak aksesnya; yang sudah terbukti baru 401 tanpa token. Itu menyangkut pembuktian di sisi backend, bukan bentuk balasan yang dikonsumsi layar ini
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan
- INTERRUPTIONS: NONE
- GIT STATUS: Berkas baru pada constants, utils, hook, view, route dinamis, dan test. **Belum di-stage dan belum di-commit**
- NEXT RECOMMENDED STEP: Ketika `FE-RWI-012` sampai `FE-RWI-015` dikerjakan, tempelkan aksinya pada layar detail ini dan pakai ulang `getEpisodeActorContext` beserta pola `resolve*Authority`, supaya penjaga per pasien tetap satu tempat
