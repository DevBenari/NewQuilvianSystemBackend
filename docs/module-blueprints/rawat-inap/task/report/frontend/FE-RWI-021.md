# Laporan Perubahan Frontend — `FE-RWI-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-021` |
| Judul | Beranda Rawat Inap menjadi pintu masuk, bukan halaman penantian |
| Slice | `F8 — Keterjangkauan` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) revision `5`, kartu `FE-RWI-021` |
| Trace | `03-frontend-architecture.md` `FE-INP-19`, bagian 2B “Isi Beranda”, `IA-INP-01`; `05-skema-tampilan.md` bagian 5 dan 23 |
| Contract version | API `0.4.0`, Permission/Audit `0.4.0`, Validation `0.4.0` |
| Persetujuan | Pemilik menyetujui skema dan memerintahkan implementasi `FE-RWI-021` pada 2026-08-29. Instruksi “lanjutkan” juga diterima sebagai persetujuan memakai hasil parsial dependency `FE-RWI-020` untuk task ini |
| Wewenang UI | `RWI-FE-005`, `DEV_DISCRETION`; ketiga kelompok informasi wajib tersedia dan setiap angka wajib dapat diklik |
| Dependency | `FE-RWI-020` — kemampuan route/query `status=0` yang dibutuhkan task ini tersedia; gap expiry reservation milik kriteria lain tidak dipakai beranda |
| Klasifikasi | `HEAVY` — menambah view/hook/constants/utils/style/test, memperluas satu base component secara backward-compatible, serta memperbarui query-aware entry pada dua layar tujuan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` untuk source; `NewQuilvianSystemBackend` branch `MHamzah` hanya untuk laporan dan tautan bukti |
| Model | GPT-5 (Codex) |
| Commit frontend saat mulai | `e2e74e3f637783bb89f47f62cc16f3e074efe999` |
| Commit backend rujukan | `4db8909e5c77b06aadf2603bd1617ccdcca093db` |
| Tanggal | 2026-08-29 |
| Status | **Implementasi 5/5 kriteria tersedia; verifikasi DoD belum lengkap.** E2E sudah ditulis tetapi tidak dijalankan, dan build dihentikan atas arahan pengguna |

---

## 1. Keadaan awal dan hasil pengguna

Route `/health-services/inpatient-management` sebelumnya hanya menampilkan hero dan kalimat
“kemampuan operasional akan tersedia bertahap”. Petugas tidak melihat keadaan hari ini dan
tidak mendapat pintu masuk langsung ke pekerjaan Rawat Inap.

Sesudah perubahan, beranda menampilkan:

1. jumlah pasien dirawat per unit layanan dan kelas perawatan;
2. jumlah episode untuk kelima status, termasuk angka Draft yang menuju daftar kerja dengan
   query `status=0`;
3. jumlah baris pada empat daftar pantau, masing-masing menuju tab yang sesuai;
4. akses cepat ke enam layar operasional selain beranda.

Kegagalan satu sumber tidak menutup sumber lain. Data yang berhasil tetap ditampilkan ketika
sumber lain gagal atau sedang dicoba ulang. Respons `401/403` menyembunyikan kelompok yang
tidak berhak dibaca; halaman akses ditolak baru tampil apabila semua kemampuan baca ringkasan
ditolak.

---

## 2. UI gate sebelum implementasi

`UI GATE: 6 elemen — REUSE 3, EXTEND 1, COMPOSE 2, WRAP 0, NEW 0`.

| Elemen | Keputusan | Bukti/rekomendasi |
| --- | --- | --- |
| Hero dan aksi muat ulang | `REUSE` | `Hero` dan `BaseButton` sudah menyediakan pola heading/action yang sama |
| Loading ringkasan | `REUSE` | `SummaryGrid` sudah mempunyai skeleton dan empty state |
| Kartu angka yang dapat diklik | `EXTEND` | `SummaryGrid` diberi properti item opsional `href`; perilaku default item statis tidak berubah |
| Galat parsial dan coba lagi | `COMPOSE` | `InformationAlert`, `BaseButton`, dan `SummaryGrid` cukup; tidak perlu wrapper domain baru |
| Akses ditolak | `REUSE` | `AccessDeniedGate` tetap menjadi pola halaman baku |
| Akses cepat | `COMPOSE` | `BaseButton` dirender sebagai `Link`; tidak memakai raw button/link bergaya baru |

Alternatif yang tidak dipilih:

- tombol terpisah di bawah kartu angka: ditolak karena angka sendiri wajib dapat diklik;
- base component domain baru untuk retry: tidak ada pola visual baru yang membenarkannya;
- raw Bootstrap link/button: tidak konsisten dengan base component dan token Quilvian.

---

## 3. Perubahan source

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/inpatient-management/page.jsx` | Placeholder diganti route tipis menuju dashboard view |
| `src/components/view/health-services/inpatient-management/inpatient-dashboard-view.jsx` | **Baru.** Komposisi hero, tiga kelompok ringkasan, retry parsial, dan akses cepat |
| `src/components/view/health-services/inpatient-management/inpatient-dashboard-widgets.jsx` | **Baru.** Kartu overview dan panel chart domain yang merangkai `Card`, `Link`, `motion`, `SummaryGrid`, serta `react-apexcharts` mengikuti dashboard utama |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-dashboard.jsx` | **Baru.** Enam pembacaan independen, cancel signal, refresh per kelompok, permission-aware visibility, dan retensi data berhasil |
| `src/lib/constants/health-services/inpatient-management/inpatient-dashboard-constants.jsx` | **Baru.** Definisi empat daftar pantau dan enam akses cepat |
| `src/utils/health-services/inpatient-management/inpatient-dashboard-utils.jsx` | **Baru.** Normalisasi camelCase/PascalCase, penyusun kartu, dan pemeriksa empty state |
| `src/style/health-services/inpatient-management/inpatient-dashboard.module.css` | **Baru.** Layout dashboard menggunakan design token |
| `src/components/features/base-features/summary-grid.jsx` | Item opsional dapat menjadi `Link`; item lama tetap dirender sebagai kartu statis |
| `src/style/components/features/base-features/base-data-components.module.css` | State hover/focus kartu interaktif menggunakan token |
| `src/lib/constants/.../inpatient-census-constants.jsx` | Pembangun route census dengan penyaring unit/kelas dan guard Guid longgar sesuai project |
| `src/lib/hooks/.../use-inpatient-census.jsx` | Membaca `serviceUnitId`/`patientClassId` dari URL sebagai nilai awal penyaring |
| `src/lib/constants/.../inpatient-monitoring-constants.jsx` | Pembangun route daftar pantau dengan query `list` yang di-whitelist |
| `src/lib/hooks/.../use-inpatient-monitoring.jsx` | Membuka tab awal dari query `list` |
| `tests/unit/inpatient-dashboard.test.mjs` | **Baru.** Enam test fungsi murni dan route |
| `tests/e2e/inpatient-dashboard.spec.mjs` | **Baru.** Tiga skenario dashboard, klik Draft, dan galat parsial |

Alur dependensi tetap mengikuti arsitektur frontend:

```text
page.jsx
  -> inpatient-dashboard-view.jsx
  -> use-inpatient-dashboard.jsx
  -> inpatient census/episode/monitoring service
  -> inpatient-api.service.js
  -> InstanceAxios
```

Tidak ada Axios instance, Redux store, endpoint, atau komponen visual global baru.

---

## 4. Kontrak, permission, privasi, dan keadaan layar

| Area | Implementasi |
| --- | --- |
| Census | `GET /census/summary`; `TotalPatient`, `ByServiceUnit`, dan `ByPatientClass` dinormalisasi dari camelCase/PascalCase |
| Episode | `GET /episodes/summary`; lima status selalu tersedia dan memakai label fallback frontend yang sudah dikunci `FE-RWI-020` |
| Monitoring | Empat endpoint dipanggil dengan `pageNumber=1&pageSize=1`; angka diambil dari `TotalData` |
| Permission | `401/403` tidak ditampilkan sebagai angka nol. Kelompok unauthorized disembunyikan; akses halaman ditolak hanya bila seluruh kemampuan baca ditolak |
| Privasi | Beranda hanya menyimpan label kelompok/status dan jumlah; nama pasien, diagnosis, resume, atau keterangan isolasi tidak dibaca/dirender |
| Loading | Skeleton per kelompok; refresh mempertahankan data berhasil yang sudah ada |
| Empty | Angka nol tetap dapat dibuka dan pesan “Belum ada pekerjaan Rawat Inap pada penyaring ini” ditampilkan bila seluruh sumber berhasil dan bernilai nol |
| Error | Galat census/episode memiliki retry sendiri; setiap kegagalan daftar pantau ditampilkan terpisah tanpa menutup angka daftar lain |
| Unauthorized | Kelompok tanpa hak baca disembunyikan; `AccessDeniedGate` dipakai bila semua kelompok ditolak |
| Aksesibilitas | Section memakai heading/`aria-labelledby`; kartu interaktif adalah link native dengan `aria-label` dan focus ring token |

Endpoint yang kini dipakai beranda:

- `GET /v1/health-services/inpatient-management/census/summary`
- `GET /v1/health-services/inpatient-management/episodes/summary`
- `GET /v1/health-services/inpatient-management/monitoring/pending-closures`
- `GET /v1/health-services/inpatient-management/monitoring/closures-without-financial-clearance`
- `GET /v1/health-services/inpatient-management/monitoring/unassigned-nurse-episodes`
- `GET /v1/health-services/inpatient-management/monitoring/isolation-mismatch`

---

## 5. Bukti acceptance criteria

| # | Kriteria | Bukti source/test | Status implementasi |
| --- | --- | --- | :---: |
| 1 | Jumlah pasien per unit dan kelas terbaca | `buildInpatientCensusSummaryCards`; kartu membawa route census berpenyaring; unit test kriteria 1 | ✅ |
| 2 | Jumlah episode per status; Draft menuju worklist tersaring | Lima kartu dibentuk dari status fallback; Draft memakai `buildInpatientEpisodeWorklistRoute(0)`; hook worklist yang sudah ada mengubah `status=0` menjadi `episodeStatus=0`; unit test kriteria 2 | ✅ |
| 3 | Empat jumlah monitoring terbaca/dapat diklik | Definisi empat endpoint; `TotalData` dinormalisasi; route `?list=<key>` membuka tab awal; unit test kriteria 3 | ✅ |
| 4 | Layar tingkat dua dicapai maksimal tiga klik | Enam akses cepat: admisi, bed board, worklist, census, monitoring, dan bed drift. Layar per-episode dicapai melalui worklist/census lalu detail | ✅ |
| 5 | Kalimat penantian hilang | Placeholder di root route dihapus; e2e juga menyimpan assertion negatif | ✅ |

Implementasi memenuhi kelima kriteria. Status task belum dinaikkan menjadi selesai penuh karena
DoD meminta E2E **ada dan lulus**, sedangkan arahan pengguna menghentikan validasi sebelum E2E
dijalankan.

---

## 6. Hasil validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `git diff --check` | **PASS** |
| UI consistency grep pada tambahan JSX/CSS | **PASS** — tidak ada raw `<button>`, raw `<table>`, warna literal, `rgba()`, `!important`, atau utility typography Bootstrap pada source dashboard baru |
| Unit target `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-dashboard.test.mjs` | **PASS — 6/6** |
| `npm run lint:errors` | **PASS** pada seluruh repository |
| `npm run test:unit` | **FAIL SEBELUM TEST** — Node `v24.19.0` menolak directory import `tests/unit/` (`ERR_UNSUPPORTED_DIR_IMPORT`) |
| Seluruh file unit eksplisit selain defect auth baseline | **PASS — 263/263**. `auth-security.test.mjs` tidak dapat dimuat karena mengimpor `src/utils/auth/base-login-utils.jsx` yang tidak ada; tidak terkait task |
| `npm run build` | **DIBATALKAN** pada tahap “Creating an optimized production build” atas arahan pengguna; tidak ada hasil pass/fail |
| `tests/e2e/inpatient-dashboard.spec.mjs` | Spec tersedia, **tidak dijalankan** atas arahan pengguna |
| Manual test | **NOT FEASIBLE / tidak dilanjutkan** setelah pengguna meminta validasi dihentikan |

Dependency dipasang dari lockfile untuk lint/build. Instalasi melaporkan enam advisory dependency
repository (3 moderate, 3 high); tidak ada dependency atau lockfile yang sengaja diubah oleh task.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Blocker implementasi | `NONE` |
| Blocker sign-off DoD | E2E belum dijalankan; build tidak selesai atas arahan pengguna |
| Perubahan backend | Tidak ada source backend yang diubah |
| Perubahan database | Tidak ada |
| Perubahan kontrak | Tidak ada; task hanya mengonsumsi kontrak `0.4.0` |
| Keputusan UI material | Extension opsional `SummaryGrid` dipilih karena menjaga seluruh pemakai lama tetap statis dan memungkinkan angka dashboard menjadi link native |
| Catatan permission | Frontend tidak mengarang permission claim. Visibility diturunkan dari respons endpoint dan role supervisor yang sudah tersedia untuk bed drift; route tujuan tetap menjadi enforcement akhir |
| Catatan Git | Tidak ada `git add`, commit, atau push. `yarn.lock` sempat tersentuh oleh instalasi; hash konten normalized sama persis dengan `HEAD` (`0f9cc0ea7156566278156d603f894c28f3b7f3c8`) dan `git diff` kosong walaupun stat cache Windows masih menandainya `M` |
| Langkah sign-off | Jika validasi dibuka kembali, jalankan build sampai selesai dan Playwright `tests/e2e/inpatient-dashboard.spec.mjs`; baru naikkan status menjadi ✅ selesai |

---

## 8. Revisi visual mengikuti dashboard utama — 2026-08-29

Pemilik meminta tampilan tidak berhenti pada deretan angka putih. Referensi visual dikunci ke
route Dashboard utama `/`, khususnya `dashboard-home.jsx`, `InformasiCardCount`,
`DailyVisitChart`, dan `PasienBaruLamaChart`.

Perubahan visual yang diterapkan:

1. hero memakai pola status sistem dan tombol muat ulang seperti dashboard utama;
2. empat kartu overview berwarna menampilkan pasien dirawat, kebutuhan isolasi, total episode,
   dan jumlah pekerjaan yang perlu ditindaklanjuti;
3. empat panel chart menampilkan distribusi pasien per unit, komposisi kelas, komposisi status
   episode, dan pekerjaan per daftar pantau;
4. angka rinci tetap dirender melalui `SummaryGrid`, sehingga pengguna keyboard tetap dapat
   membuka tujuan walaupun chart bersifat visual;
5. batang atau segmen chart dapat diklik dengan mouse untuk membuka route milik data tersebut;
6. akses cepat tetap tersedia di bawah chart.

### UI gate revisi

`UI GATE: 7 elemen — REUSE 4, COMPOSE 3, EXTEND 0, WRAP 0, NEW 0`.

| Elemen | Keputusan | Dasar |
| --- | --- | --- |
| Hero dan refresh | `REUSE` | `Hero`, `BaseButton` |
| Status sinkronisasi | `COMPOSE` | Pola chip status dashboard utama, memakai token |
| Kartu overview | `COMPOSE` | `Card`, `Link`, ikon, `motion`; bentuk mengikuti `InformasiCardCount` tanpa memakai context dashboard umum |
| Panel chart | `COMPOSE` | `Card` dan dynamic `react-apexcharts`, mengikuti chart kunjungan dashboard utama |
| Angka rinci | `REUSE` | `SummaryGrid` |
| State halaman | `REUSE` | `InformationAlert`, skeleton, `AccessDeniedGate` |
| Akses cepat | `REUSE` | `BaseButton` sebagai `Link` |

### Validasi revisi

- `AUTOMATED TEST: eslint` pada `inpatient-dashboard-view.jsx` dan
  `inpatient-dashboard-widgets.jsx` — **PASS**.
- UI grep warna literal, raw button, raw table, Bootstrap typography utility, dan
  `!important` pada berkas revisi — **PASS**, tidak ada temuan.
- Typography feature memakai semantic design token; tidak ada selector yang menimpa
  typography `Hero`, `SummaryGrid`, atau `BaseButton`.
- `git diff --check` — **PASS**.
- `AUTOMATED TEST: SKIPPED (opsional)` — test `.mjs` tidak dijalankan kembali sesuai arahan
  pengguna.
- `MANUAL TEST: NOT FEASIBLE` — tidak ada sesi browser autentik yang dijalankan pada revisi
  ini; pemeriksaan dilakukan terhadap komposisi source dan referensi dashboard utama.

Sesuai instruksi pemilik, revisi visual ini tidak mengubah roadmap, skema tampilan, atau
`requirement-traceability.md`.
