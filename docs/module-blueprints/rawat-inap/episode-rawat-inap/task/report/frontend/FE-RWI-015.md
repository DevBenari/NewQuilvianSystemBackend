# FE-RWI-015 — Petugas dapat mencatat pasien sudah meninggalkan ruangan

- TASK ID: `FE-RWI-015`
- TASK TYPE: Penambahan satu aksi beserta aturan tampil per peran pada layar detail episode yang sudah ada
- COMPLEXITY: `MEDIUM`
- CLASSIFICATION SCORE: 8 — dua repository 2; 9–20 berkas diperiksa 1; 4–8 berkas diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga peran tanpa mengubahnya 1; alur berbatas pada satu layar 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca, kecuali laporan ini beserta tautan buktinya
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /discharges/{episodeId}/record-departure` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-015` beserta `BE-RWI-027`; `03-frontend-architecture.md` bagian 2, 3, 5.2, 5.3, 5.4, dan 9; `contracts/api-contract.md` bagian Inpatient Discharge beserta catatan bentuk jawaban `/record-departure`; `contracts/permission-audit-matrix.md` bagian 3; `00-interview-decisions.md` `RWI-DEC-055` dan `RWI-RULE-036`; [laporan `FE-RWI-014`](FE-RWI-014.md); `InpatientDischargeController.cs` bagian `RecordDeparture`; `InpatientClosureDtos.cs` (`RecordDepartureRequest`); `InpDischargeService.Closure.cs` bagian `RecordPatientDepartureAsync`; `InpatientActorClaims.cs`; `use-inpatient-episode-detail.jsx`; `inpatient-episode-detail-view.jsx`; `inpatient-episode-utils.jsx`; `inpatient-episode-constants.jsx`; `base-form-control.jsx`; `confirm-modal.jsx`; `tests/e2e/inpatient-episode-detail.spec.mjs`
- FILES CHANGED:
  - **Baru** `src/lib/constants/health-services/inpatient-management/inpatient-departure-constants.jsx`
  - **Baru** `src/utils/health-services/inpatient-management/inpatient-departure-utils.jsx`
  - **Baru** `tests/unit/inpatient-departure.test.mjs`
  - **Baru** `tests/e2e/inpatient-departure.spec.mjs`
  - **Diubah** `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` (keadaan dan aksi pencatatan kepergian)
  - **Diubah** `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (bagian Kepergian Pasien beserta dialog konfirmasinya)

## 1. Kenapa aksinya menempel pada detail episode, bukan pada layar penutupan

Penempatan tombol adalah `DEV_DISCRETION` — `03-frontend-architecture.md` bagian 9. Tetapi
pilihannya di sini **tidak** bebas sepenuhnya, dan alasannya ada pada hak akses.

`contracts/permission-audit-matrix.md` bagian 3 memberi perawat pelaksana `InpatientEpisode :
Read`, `InpatientBedOccupancy : Read/Transfer`, `InpatientDischarge : RecordDeparture`, dan
`InpatientCensus : Read` — **tanpa** `InpatientDischarge : Read`. Layar penutupan `FE-RWI-014`
membaca `closure-readiness` dan `clearance`, keduanya menuntut `InpatientDischarge : Read`.

Artinya: meletakkan aksi kepergian di layar penutupan akan **menutupnya dari perawat pelaksana** —
justru petugas yang paling sering mencatat kepergian pasien. Layar detail episode hanya memerlukan
`InpatientEpisode : Read` dan `InpatientBedOccupancy : Read`, keduanya dimiliki perawat.

Batas satu-satunya pada kebebasan bagian 9 tetap dipenuhi: konfirmasinya menyebut bahwa tindakan
tidak dapat dibatalkan.

## 2. Kriteria 4 melawan intuisi, dan layar mengatakannya apa adanya

Sesudah kepergian dicatat, pasien hilang dari census tetapi episodenya **masih hidup** dan masih
wajib ditutup. `RecordPatientDepartureAsync` sengaja **tidak** memanggil `ApplyStatusChangeAsync`:
kepergian fisik adalah fakta yang dicatat, bukan tahapan yang dilalui, dan `RWI-DEC-009` mengunci
lima nilai status.

Layar yang diam soal ini akan membuat petugas mengira episodenya sudah selesai — dan episode yang
dikira selesai tidak pernah ditutup siapa pun. Karena itu sesudah kepergian tercatat, layar
menyatakan ketiganya sekaligus: pasien tidak lagi muncul pada census, tempat tidurnya kembali
terbaca kosong, dan episodenya tetap berstatus Menunggu pulang, tetap muncul pada daftar pantau
penutupan tertunda, serta **masih wajib ditutup**. Tautan menuju layar penutupan dipasang tepat di
bawahnya.

**Batas bukti yang perlu dinyatakan:** bagian "tetap muncul pada daftar pantau penutupan tertunda"
baru dapat dibuktikan di layar setelah `FE-RWI-016` ada. Hari ini ia adalah kalimat yang benar
menurut `BE-RWI-027` kriteria 2 dan integration test-nya, bukan daftar yang dapat dibuka petugas
dari sini.

## 3. Waktu kepergian dikirim sebagai UTC, bukan sebagai waktu setempat tanpa zona

Isian `datetime-local` berbunyi waktu setempat **tanpa zona**. Mengirimkannya apa adanya akan
membuat `System.Text.Json` membacanya sebagai `DateTimeKind.Unspecified` lalu membandingkannya
dengan `DateTime.UtcNow`: kepergian pukul 09.00 WIB akan tercatat tujuh jam lebih lambat, dan
pemeriksaan "tidak boleh melewati waktu sekarang" akan menolaknya tanpa sebab yang terbaca petugas.

Karena itu muatan mengirim ISO 8601 berakhiran `Z`. Kolom yang dikosongkan **tidak dikirim sama
sekali** — bukan dikirim sebagai `null` — karena `DepartedAt` yang dikosongkan berarti sekarang,
dan itu ditentukan server.

Kedua aturan waktu diperiksa lebih dulu di layar dengan kalimat yang **disalin apa adanya** dari
service, supaya penolakan di layar dan penolakan server berbunyi sama persis. Server tetap
pemeriksa terakhirnya, karena jam browser petugas dapat saja meleset.

- IMPLEMENTATION: (1) Bagian Kepergian Pasien **tidak dirender** bagi pengguna berperan dokter — permission matrix bagian 3 menuliskan dokter dan DPJP tanpa `RecordDeparture`, "karena kepergian dicatat petugas ruangan". Supervisor yang kebetulan juga seorang dokter tetap melihatnya; yang ditutup adalah jalur DPJP, bukan orangnya. (2) `isVisible` dan `canRecord` sengaja dipisah: yang tidak boleh melihat aksinya sama sekali hanya dokter, sedangkan keadaan lain — episode belum diputuskan pulang, kepergian sudah dicatat, episode sudah selesai — justru wajib terbaca beserta kalimatnya. (3) Detail episode dimuat ulang **sebelum** dialog konfirmasi tampil — bagian 5.2 — karena petugas lain mungkin sudah mencatatnya beberapa detik lalu. (4) Konfirmasinya menyebut nama pasien, lokasi yang ditinggalkan, bahwa tindakan tidak dapat dibatalkan, dan bahwa tempat tidur langsung bebas. (5) Sesudah pencatatan berhasil, riwayat penempatan dan papan tempat tidur tujuan sama-sama dibaca ulang, sehingga lokasi terkini terbaca kosong tanpa menunggu muat ulang halaman. (6) Penjaga `departureInFlight` menahan klik kedua; penolakan 409 memuat ulang detail episode dan menampilkan pesan server apa adanya — bagian 5.4. (7) Waktu kepergian bersifat pilihan, dan kedua aturan waktunya diperiksa di layar sebelum permintaan terkirim
- API CONTRACT IMPACT: Tidak mengubah kontrak. Muatan memakai nama kolom `RecordDepartureRequest` apa adanya (`departedAt`, `note`), catatannya dipangkas ke 500 karakter sesuai `MaxLength`, dan kolom yang kosong tidak dikirim. Pelaku diturunkan server dari pengguna yang terautentikasi
- DATABASE IMPACT: Tidak ada
- SECURITY IMPACT: Tidak mengubah authorization. Layar menyembunyikan aksi yang pasti ditolak `InpatientDischarge : RecordDeparture` bagi peran dokter; server tetap satu-satunya penentu. **Satu risiko dicatat:** nama peran supervisor adalah asumsi `InpatientActorClaims` yang belum dikonfirmasi rumah sakit, dan frontend menyalinnya apa adanya
- VISUAL REFERENCE: NOT REQUIRED
- WEWENANG UI YANG DIPAKAI: "Penempatan tombol adalah `DEV_DISCRETION`. **Batasnya:** konfirmasinya wajib menyebut bahwa tindakan **tidak dapat dibatalkan**". Dipilih satu bagian pada layar detail episode — dengan alasan hak akses pada bagian 1 — memakai `InformationAlert`, `BaseTextField` bertipe `datetime-local`, `BaseTextAreaField`, `BaseButton`, `ConfirmModal`, dan `ToastStack` yang sudah ada. Bagiannya diletakkan sebelum Perpindahan Tempat Tidur, karena keduanya sama-sama menyangkut tempat tidur yang sedang ditempati. Tidak ada komponen baru, tidak ada hook baru, tidak ada pola HTTP baru, dan tidak ada arsitektur state baru

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Aksi tersedia bagi petugas admisi, perawat, kepala ruangan, dan supervisor; **tidak** bagi DPJP | **LULUS** | Empat e2e per peran membuktikan bagian kepergian tampil dan tombolnya aktif bagi keempat peran itu. e2e kelima membuktikan bagiannya **tidak dirender sama sekali** bagi DPJP — `toHaveCount(0)`, bukan dirender lalu dinonaktifkan. Test unit menutup keenam peran termasuk dokter yang bukan DPJP episode ini, dan membuktikan supervisor yang juga seorang dokter tetap melihat aksinya |
| 2. Konfirmasi menyebut bahwa tindakan tidak dapat dibatalkan | **LULUS** | e2e membuka dialog konfirmasi dan membuktikan isinya memuat nama pasien, kalimat "tidak dapat dibatalkan", dan "tempat tidur langsung bebas"; **nol** permintaan terkirim selagi dialog terbuka, dan tetap nol sesudah tombol Batal ditekan. e2e terpisah membuktikan detail episode dibaca ulang lebih dulu sebelum dialognya tampil — bagian 5.2. Test unit membaca sumber layar dan membuktikan kalimatnya ada di dalam dialognya sendiri, bukan hanya pada peringatan di halaman |
| 3. Setelah dicatat, tempat tidur terbaca kosong pada papan ketersediaan **tanpa** episode ditutup | **LULUS** | e2e mencatat kepergian lalu membuktikan tiga hal sekaligus: status episode **tetap** "Menunggu pulang", lokasi terkini berubah menjadi "Belum menempati tempat tidur" karena baris penempatannya sudah ditutup, dan papan ketersediaan sesudahnya membaca `bed-row-BD-001` sebagai "Dapat dipakai" tanpa nama pasien. Muatan permintaannya `{}` — waktu yang dikosongkan tidak dikirim sama sekali. Test unit membuktikan muatannya tidak memuat satu pun kolom status |
| 4. Pasien hilang dari census tetapi episodenya tetap muncul pada daftar pantau penutupan tertunda | **SEBAGIAN — keterbacaannya lulus; daftar pantaunya belum ada di frontend** | e2e membuktikan sesudah kepergian tercatat, layar menyatakan ketiganya apa adanya: pasien tidak lagi muncul pada census, episodenya tetap muncul pada daftar pantau penutupan tertunda, dan masih wajib ditutup — beserta tautan menuju layar penutupan. Test unit mengunci ketiga kalimat itu. **Yang belum dapat dibuktikan dari frontend:** daftar pantau penutupan tertunda itu sendiri adalah `FE-RWI-016`, yang belum dikerjakan. Perilaku server-nya dibuktikan integration test `BE-RWI-027` kriteria 2 dan 3 |
| 5. Mencatat pada episode yang belum diputuskan pulang ditolak, dan pesannya menjelaskan urutannya | **LULUS** | e2e membuka episode berstatus `Admitted`: tombolnya `toBeDisabled()`, keterangannya berbunyi "Kepergian hanya dapat dicatat setelah DPJP menyatakan pasien boleh pulang." — disalin apa adanya dari `RecordPatientDepartureAsync` — dialog konfirmasi tidak pernah tampil, dan **nol** permintaan terkirim. Test unit menutup `Draft` dan `Admitted`, ditambah `Closed` dan `Cancelled` yang berkalimat berbeda. e2e terpisah membuktikan penolakan 409 dari server terbaca apa adanya ketika petugas lain sudah mencatatnya lebih dulu |

- VALIDATION: e2e `tests/e2e/inpatient-departure.spec.mjs` | PASS, 12/12 | TASK | dijalankan pada browser sungguhan (Edge) terhadap build produksi; termasuk lima e2e per peran, dua e2e konfirmasi, e2e papan ketersediaan, e2e urutan, e2e 409, dan e2e aturan waktu
- VALIDATION: e2e regresi `inpatient-episode-detail.spec.mjs`, `inpatient-discharge.spec.mjs`, `inpatient-financial-clearance.spec.mjs`, `inpatient-census.spec.mjs` | PASS, 36/36 | TASK | layar detail episode adalah berkas bersama yang diubah task ini
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-departure.test.mjs` | PASS, 18/18 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 194, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` gagal `ERR_MODULE_NOT_FOUND` atas `src/utils/auth/base-login-utils.jsx`; berkas yang ada bernama `base-login-utils.js`. Tidak bersinggungan dengan diff ini
- VALIDATION: `npm run test:unit` | NOT RUN sebagai apa adanya | EXISTING / ENVIRONMENT ISSUE | script-nya memakai `--test tests/unit/` dan Node menolaknya dengan `ERR_UNSUPPORTED_DIR_IMPORT`. Dijalankan lewat bentuk glob
- MANUAL TEST: PASS — seluruh kontrol interaktif yang ditambahkan dijalankan di browser sungguhan (Edge) terhadap build produksi lewat e2e dengan peran berbeda per kasus: tombol catat kepergian beserta keadaan aktif dan nonaktifnya, isian waktu kepergian beserta penolakan waktu yang melewati sekarang, kotak catatan, dialog konfirmasi beserta tombol Batal-nya, dan pembacaan papan ketersediaan sesudah pencatatan. Isi setiap permintaan yang terkirim diperiksa, dan jumlah permintaan pada jalur yang seharusnya menolak diperiksa **nol**
- WARNINGS: **Kriteria 4 baru dapat ditutup penuh setelah `FE-RWI-016`** — daftar pantau penutupan tertunda belum ada di frontend, sehingga yang dibuktikan hari ini adalah keterbacaan keadaannya di layar, bukan daftar yang dapat dibuka petugas. **Pencatatan kepergian tidak dapat dibatalkan** dan tidak ada endpoint pembatalannya — `RWI-RULE-036`. Pasien yang ternyata belum jadi pulang menjalani admisi baru, dan layar menyatakan itu sebelum tindakan dijalankan. **Nama peran supervisor adalah asumsi** yang belum dikonfirmasi rumah sakit
- KNOWN ISSUES: Aturan "waktu kepergian tidak boleh melewati waktu sekarang" diperiksa memakai jam browser petugas. Bila jam itu meleset jauh, penolakan di layar dapat berbeda dari penolakan server — dan server tetap yang berlaku. Layar sengaja tidak menyalin jam server, karena tidak ada endpoint yang menyediakannya pada kontrak `0.4.0`
- DEPENDENCY BACKEND: `BE-RWI-027` ✅ **Selesai** — `POST .../record-departure` berstatus ✅ `Tersedia`, terbukti berjalan 26 Agustus 2026. `FE-RWI-014` ✅ **Selesai** pada sesi yang sama, dan tautan dari bagian kepergian menuju layar penutupan memakai route yang dibuat task itu
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan lewat `git checkout --`; direktori artefak Playwright dihapus. Tidak ada perubahan sampingan lain yang tersisa pada diff
- INTERRUPTIONS: NONE
- GIT STATUS: Dua berkas diubah dan empat berkas baru pada `QuilvianSystemFrontendDev`, bersama perubahan `FE-RWI-014` yang dikerjakan berurutan pada sesi yang sama. **Belum di-stage dan belum di-commit.** Tidak ada berkas backend yang disentuh selain laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md`
- NEXT RECOMMENDED STEP: Kerjakan `FE-RWI-016` — empat daftar pantau, termasuk penutupan tertunda — supaya kriteria 4 task ini dapat ditutup penuh dengan bukti di layar, bukan hanya kalimat yang benar menurut integration test backend
