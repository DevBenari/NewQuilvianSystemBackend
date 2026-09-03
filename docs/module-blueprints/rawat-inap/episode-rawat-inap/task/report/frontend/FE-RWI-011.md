# FE-RWI-011 — DPJP dan perawat penanggung jawab dapat dialihkan

- TASK ID: `FE-RWI-011`
- TASK TYPE: Implementasi aksi frontend beserta aturan tampil per peran
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 9 — dua repository 2; lebih dari 20 berkas diperiksa 2; tiga berkas source diubah 0; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga peran tanpa mengubahnya 1; alur berbatas pada satu layar 1; naik satu tingkat karena dua faktor berada di tingkat berikutnya
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /episodes/{id}/doctor-assignments` dan `POST /episodes/{id}/nurse-assignments` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-011`; `03-frontend-architecture.md` bagian 3; `InpatientEpisodeController.cs` bagian `HandoverDoctor` dan `AssignNurse`; `InpEpisodeService.Assignments.cs` baris 260–560 tempat `RWI-RULE-016` dan `RWI-DEC-032` dibentuk; `InpatientActorClaims.cs` bagian `SupervisorOrWardHeadRoles`; `InpatientEpisodeAssignmentDtos.cs` (`HandoverDoctorRequest`, `AssignNurseRequest`, kedua response riwayat); `hr-select-resources.js` bagian `doctors` dan `employees`; `use-select-resource.jsx`; `login-slice.jsx` bagian peran
- FILES CHANGED: `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` (daftar peran dan batas kolom); `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` (bagian penugasan); `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` (aksi pengalihan dan penugasan); `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (bagian penanggung jawab dan riwayatnya); `tests/unit/inpatient-episode-detail.test.mjs`; `tests/e2e/inpatient-episode-detail.spec.mjs`

## Kriteria 4 sengaja dikerjakan sebagai keterangan, bukan peringatan

Ini yang paling mudah dikerjakan terbalik. Episode tanpa perawat penanggung jawab terlihat
seperti data yang belum lengkap, dan naluri pertama adalah memasang peringatan yang menahan
tindakan sampai perawatnya diisi.

`RWI-DEC-032` memilih **tidak menahan**, karena penugasan perawat sering menyusul beberapa
menit setelah pasien tiba, dan menahan pekerjaan sampai kolom itu terisi hanya memindahkan
antrean ke tempat lain. Yang muncul untuk episode tanpa perawat adalah daftar pantau kepala
ruangan, bukan penolakan.

Karena itu layar menampilkan satu keterangan bernada netral — "Episode ini tetap dapat dibuka
dan seluruh tindakan lain tetap tersedia" — dan **tidak satu pun** tombol dinonaktifkan
karenanya. Test unit memeriksanya dari arah yang benar: pada episode tanpa perawat,
`resolveTransferAuthority`, `resolveIsolationAuthority`, dan `resolveAssignmentAuthority`
menjawab persis sama seperti pada episode yang perawatnya sudah ada.

- IMPLEMENTATION: (1) Kedua aksi hanya dirender bagi kepala ruangan dan supervisor; peran lain tidak melihat bagiannya sama sekali. Nama perannya disalin apa adanya dari `InpatientActorClaims.SupervisorOrWardHeadRoles`, dan pencocokannya tidak peka huruf besar-kecil. (2) Pilihan dokter dan pegawai memakai `useSelectResource` yang sudah ada (`doctors` dan `employees`), bukan pemanggilan baru. (3) Alasan pengalihan DPJP diperiksa di layar sebelum permintaan dikirim, setara `HasMeaningfulReason` di service. (4) Penugasan perawat **tidak** meminta alasan, mengikuti `AssignNurseRequest` yang memang hanya bermuatan `employeeId`. (5) Riwayat DPJP dan perawat terbaca urut nomor urut beserta periodenya, dan yang masih berlaku ditandai. (6) Penjaga `handoverInFlight` dan `nurseInFlight` menahan klik kedua.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Payload memakai nama kolom `HandoverDoctorRequest` (`doctorId`, `handoverReason`) dan `AssignNurseRequest` (`employeeId`) apa adanya.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization. Menyembunyikan aksi yang pasti ditolak `RWI-RULE-016`; server tetap satu-satunya penentu.
- VISUAL REFERENCE: NOT REQUIRED.
- WEWENANG UI YANG DIPAKAI: "Bebas". Dipilih satu bagian dua kolom pada layar detail episode, berdampingan dengan riwayat DPJP dan riwayat perawat, supaya pengalihan dikerjakan sambil membaca siapa yang sedang berwenang.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Hanya kepala ruangan dan supervisor melihat kedua aksi | **LULUS** | e2e dengan peran Perawat: bagian penanggung jawab beserta kedua tombolnya `toHaveCount(0)` — tidak dirender, bukan dirender lalu dinonaktifkan. e2e dengan peran Kepala Ruangan: bagian itu tampil. Test unit menutup enam peran: kepala ruangan dan supervisor boleh; petugas admisi, perawat, DPJP, dan dokter lain tidak |
| 2. Pengalihan DPJP wajib beralasan; tanpa alasan ditolak dengan pesan jelas | **LULUS** | e2e memilih DPJP pengganti lalu menekan Alihkan tanpa mengisi alasan; pesan "Alasan pengalihan DPJP wajib diisi." muncul dan **nol** permintaan terkirim. Sesudah alasan diisi, tepat satu permintaan terkirim dengan `doctorId` dan `handoverReason` yang benar |
| 3. Riwayat DPJP dan perawat terbaca urut beserta periodenya | **LULUS** | Server tiruan mengirim riwayat terbalik; layar menampilkannya urut nomor urut. Test unit memeriksa urutan dan format periodenya: penugasan yang masih berlaku berakhir "— sekarang", yang sudah ditutup menampilkan kedua ujungnya |
| 4. Episode tanpa perawat tetap dapat dibuka dan seluruh tindakan lain tetap tersedia | **LULUS** | e2e membuka episode tanpa perawat: ringkasan berbunyi "Belum ditugaskan", keterangan netral tampil, tombol Alihkan DPJP dan Tugaskan Perawat aktif, pemilihan tempat tidur tujuan dan tombol pindah aktif, dan penugasan perawat berhasil terkirim dari layar yang sama. Test unit membuktikan ketiga penjaga kewenangan menjawab sama persis dengan episode yang perawatnya sudah ada |

- VALIDATION: e2e `tests/e2e/inpatient-episode-detail.spec.mjs` | PASS, 10/10 | TASK | termasuk e2e per peran dan e2e episode tanpa perawat
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-episode-detail.test.mjs` | PASS, 16/16 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 106, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs`, rusak sejak sebelum task ini
- MANUAL TEST: NOT FEASIBLE — memerlukan akun berperan kepala ruangan dan supervisor pada database tim, ditambah episode berjalan yang perawatnya belum ditugaskan. Tidak tersedia tanpa database itu. Seluruh kontrol — kedua isian pilihan, kotak alasan, dan kedua tombol — dijalankan di browser sungguhan (Edge) lewat e2e dengan peran yang berbeda per kasus
- WARNINGS: **Daftar nama peran adalah asumsi yang belum dikonfirmasi rumah sakit.** `InpatientActorClaims` menandainya demikian, dan frontend menyalinnya apa adanya. Bila nama peran kepala ruangan di rumah sakit berbeda dari keempat nilai itu, layar akan menyembunyikan kedua aksi dari orang yang sesungguhnya berwenang — dan server pun akan menolaknya 403. Perbaikannya harus dikerjakan di backend lebih dulu, lalu disalin ke `SUPERVISOR_OR_WARD_HEAD_ROLES`
- KNOWN ISSUES: Tidak ada cacat implementasi yang diketahui pada scope task
- DEPENDENCY BACKEND: `BE-RWI-017` dan `BE-RWI-018` — kedua endpoint berstatus ✅ `Tersedia` dan terbukti berjalan 26 Agustus 2026. `BE-RWI-018` masih 🟡 karena kriteria 3-nya baru terbukti di tingkat service; itu menyangkut pembuktian di sisi backend, bukan bentuk balasan yang dikonsumsi layar ini
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan
- INTERRUPTIONS: NONE
- GIT STATUS: Perubahan pada constants, utils, hook, dan view detail episode beserta test-nya. **Belum di-stage dan belum di-commit**
- NEXT RECOMMENDED STEP: Naikkan pertanyaan nama peran kepala ruangan kepada pemilik modul, karena satu daftar yang sama dipakai backend dan frontend dan keduanya akan salah bersamaan bila asumsinya meleset
