# Laporan Perubahan Frontend — `FE-RWI-038`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-038` |
| Judul | Daftar Pantau menunjukkan tindak lanjut yang nyata |
| Slice | `F12 — Repair layar existing` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), task `FE-RWI-038` |
| Trace | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-09`; skema §14 dan §24.1; `FR-RI-135` s.d. `FR-RI-138`; `FR-RI-161` |
| Contract version | API `0.4.0`; empat endpoint monitoring berstatus tersedia. Source backend pada commit `44fd6ccba` menjadi bukti runtime as-is |
| Wewenang UI | Label empat daftar dan tujuan tindak lanjut mengikuti skema §14 yang disetujui pemilik; visual tab dan penekanan aksi `DEV_DISCRETION` |
| Dependency | Endpoint monitoring existing ✅ tersedia. `RWI-UI-GAP-007` masih terbuka dan membatasi pembuktian dengan data runtime |
| Klasifikasi | `MEDIUM` — empat berkas existing diubah dan dua berkas source baru ditambahkan; tanpa perubahan backend, database, dependency, atau komponen bersama |
| Task mode | `FRONTEND` — source backend strict read-only; wewenang lintas repository hanya laporan, roadmap, dan traceability modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; folder laporan, roadmap, dan traceability Rawat Inap pada `NewQuilvianSystemBackend` |
| Model | OpenAI Codex (GPT-5) |
| Commit frontend saat dikerjakan | `3133fb765cf997490638eebf177272a0833dabec` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `44fd6ccbaa9b403df1bff2f16729eedf7a0ea32a` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Ketujuh acceptance criteria terpenuhi dari source dan validasi statis. `npm run lint` lulus dengan `0 errors` serta 571 warning existing; `npm run build` berhasil. Test `.mjs` tidak dijalankan sesuai arahan pengguna. Bukti runtime penuh tetap menunggu `RWI-UI-GAP-007` |

---

## 1. Keadaan yang ditemukan di awal

Daftar Pantau sudah memiliki empat endpoint baca dan tautan pada baris, tetapi keadaan runtime
kosong membuat seluruh jalur kerja itu tidak terlihat. Empat pilihan daftar tampil seperti deret
tombol tanpa jumlah. Definisi kolom berada di dalam view, aksi baris memakai kelas Bootstrap
mentah, dan keadaan kosong tidak mempunyai jalan menuju Daftar Kerja Episode.

Hook hanya membaca endpoint tab aktif dan menyimpan satu jawaban pada resource Redux
`monitoring`. Akibatnya jumlah tab lain tidak diketahui. Error tab aktif juga masih berdampingan
dengan tabel, sehingga kegagalan dapat disalahartikan sebagai daftar bernilai nol.

Approval skema diberikan langsung oleh pemilik pada 1 September 2026. Kontrak backend tidak
memerlukan perubahan: keempat operasi merupakan `GET` dengan permission yang sama,
`InpatientMonitoring : Read`.

---

## 2. Proses bisnis dari sisi pengguna

Tujuan layar ini adalah membantu kepala ruangan atau supervisor menemukan pekerjaan yang perlu
ditindaklanjuti tanpa menjadikan daftar pantau sebagai gerbang baru.

Prasyaratnya adalah pengguna mempunyai `InpatientMonitoring : Read`, unit layanan tersedia, dan
endpoint monitoring dapat dibaca.

1. Pengguna membuka **Rawat Inap → Daftar Pantau**.
2. Empat tab langsung menunjukkan nama dan jumlah: Penutupan Tertunda, Menembus Gerbang
   Keuangan, Tanpa Perawat Penanggung Jawab, dan Penempatan Tidak Sesuai Isolasi.
3. Pengguna memilih tab dan, bila perlu, menyaring unit layanan. Tab aktif tidak berubah ketika
   filter diubah, halaman dibaca ulang, atau tombol **Coba Lagi** ditekan.
4. Pada Penutupan Tertunda, pengguna dapat membuka Detail Episode atau langsung menuju
   Penutupan Episode.
5. Pada Penempatan Tidak Sesuai Isolasi, pengguna dapat membuka detail atau langsung menuju
   bagian Perpindahan Tempat Tidur pada Detail Episode.
6. Dua daftar lain menyediakan Detail Episode sebagai titik tindak lanjut.
7. Bila daftar kosong, pengguna melihat bahwa tidak ada tindak lanjut pada daftar itu dan dapat
   membuka **Daftar Kerja Episode** untuk memeriksa pekerjaan lain.

Contoh: tab Penutupan Tertunda menunjukkan `3`. Pengguna menyaring Unit Mawar dan count berubah
menjadi `1`. Satu baris episode samaran `RI-2026-000001` dapat dibuka melalui **Penutupan Episode**.
Halaman monitoring sendiri tetap tidak mengirim `POST`, `PUT`, `PATCH`, atau `DELETE`.

Jalur tidak normal: count endpoint yang gagal menampilkan kata `gagal` hanya pada tab tersebut;
count tab lain tetap tampil. Gagal membaca isi tab menyediakan **Coba Lagi**, menyembunyikan tabel
tab itu, dan tidak menghapus pilihan tab maupun filter. `401`/`403` tetap ditangani
`AccessDeniedGate`.

Perubahan status: `NOT APPLICABLE` — layar hanya membaca dan menavigasikan pengguna. Perubahan
episode tetap dilakukan oleh layar Penutupan atau Perpindahan yang memang memilikinya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: `05-skema-tampilan.md` §14/§24.1, `roadmap/frontend-roadmap.md`,
  `roadmap/requirement-traceability.md`, dan laporan `FE-RWI-016`.
- Backend read-only: `InpatientMonitoringController.cs`, `InpatientMonitoringDtos.cs`,
  `InpCensusQueryService.cs`, serta `contracts/api-contract.md` pada commit `44fd6ccba`.
- Frontend: route, view, hook, service, constants, normalizer monitoring, detail episode,
  `DataFilter`, `DataTable`, `BaseButton`, `StatusBadge`, `InformationAlert`, dan pola tabel
  Administrator pada commit `3133fb765` ditambah working tree task ini.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-monitoring-view.jsx` | Memakai shell modern, tab `DataFilter` dengan count, retry, empty CTA, serta pemisahan error dari state kosong |
| `src/components/view/health-services/inpatient-management/inpatient-monitoring-table-columns.jsx` | Menetapkan kolom dan tindak lanjut spesifik untuk keempat daftar dengan `BaseButton`, `StatusBadge`, dan class sel semantik |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-monitoring.jsx` | Membaca count keempat endpoint secara independen, mempertahankan tab/filter, dan meneruskan abort signal pada request |
| `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` | Menambahkan helper route menuju bagian Perpindahan Tempat Tidur |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Memberi anchor stabil pada bagian Perpindahan agar tautan monitoring langsung menuju pemilik tindakan |
| `src/style/health-services/inpatient-management/inpatient-monitoring.module.css` | Menata count tab, kelompok aksi, dan stack status hanya dengan design token existing |

### 3.3 Kepatuhan arsitektur frontend

Alur request tetap `view → hook → service → InstanceAxios`. View tidak memanggil HTTP, dan helper
route berada di constants. Normalizer allow-list existing tetap dipakai agar alasan klinis isolasi
di luar respons monitoring tidak ikut masuk tampilan.

| Kebutuhan UI | Kandidat base | Status | Implementasi |
| --- | --- | --- | --- |
| Shell dan header | `Hero` + shell Administrator | `REUSE` | Pola modern yang sama dengan Census dan Nurse Station Cluster |
| Tab count dan filter | tab `DataFilter` | `REUSE` | Label React existing membawa count per endpoint |
| Tabel dan pagination | `DataTable` + `RegionPagination` | `REUSE` | Kolom domain berada di file terpisah |
| Status | `StatusBadge` | `REUSE` | Menandai bed dan ketidakcocokan tanpa style status baru |
| Aksi tindak lanjut | `BaseButton as={Link}` | `REUSE` | Detail, Penutupan, Perpindahan, dan empty CTA |
| Pesan operasional/error | `InformationAlert` | `REUSE` | Nada netral dan state gagal terpisah |
| Empty state dengan jalan kerja | `DataTable` + action `DataFilter` + `BaseButton` | `COMPOSE` | CTA ke Daftar Kerja Episode tanpa mengubah base component |

`UI GATE: 7 elemen — REUSE 6, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0.`

Tidak ada komponen dasar baru, HTTP client baru, state manager baru, atau request tulis baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tab tetap terlihat; tabel tab aktif menampilkan teks loading spesifik daftar |
| Berisi | Count empat tab dan tabel tab aktif; setiap baris mempunyai tujuan tindak lanjut sesuai pemiliknya |
| Kosong | Copy spesifik daftar, kalimat “Tidak ada tindak lanjut pada daftar ini”, dan Buka Daftar Kerja Episode |
| Count satu tab gagal | Tab itu menampilkan `gagal`; count dan navigasi tab lain tetap tersedia |
| Isi tab gagal | Alert bahaya dan Coba Lagi; tabel tab gagal tidak dirender sebagai nol data |
| Tanpa hak akses | `AccessDeniedGate` mengganti halaman ketika server menjawab `401`/`403` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/pending-closures` | Count dan daftar episode yang penutupannya melewati ambang | `InpatientMonitoring : Read` | Query `InpatientMonitoringQuery` | `ApiResponse<PendingClosurePagedResult>` |
| `GET` | `/closures-without-financial-clearance` | Count dan daftar episode yang ditutup menembus gerbang keuangan | `InpatientMonitoring : Read` | Query `InpatientMonitoringQuery` | `ApiResponse<OverrideClosurePagedResult>` |
| `GET` | `/unassigned-nurse-episodes` | Count dan daftar episode aktif tanpa perawat penanggung jawab | `InpatientMonitoring : Read` | Query `InpatientMonitoringQuery` | `ApiResponse<UnassignedNursePagedResult>` |
| `GET` | `/isolation-mismatch` | Count dan daftar episode dengan penempatan tidak sesuai kebutuhan isolasi | `InpatientMonitoring : Read` | Query `IsolationMismatchQuery` | `ApiResponse<IsolationMismatchPagedResult>` |

Query count memakai `pageNumber=1` dan `pageSize=1` karena backend belum menyediakan endpoint
summary; jumlah dibaca dari `TotalData`, bukan dari panjang satu baris yang dikembalikan.

Kode `200` berarti daftar berhasil dibaca, termasuk ketika `Items` kosong. Kode `401` berarti sesi
tidak sah. Kode `403` berarti pengguna tidak mempunyai `InpatientMonitoring : Read`. Kegagalan
jaringan/server menampilkan pesan gagal pada tab yang terdampak dan dapat dicoba ulang.

Tautan baris bernavigasi ke route existing Detail Episode, Penutupan Episode, dan anchor
Perpindahan Tempat Tidur. Navigasi itu tidak menambah request tulis pada halaman monitoring.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm.cmd run lint` | Exit `0`; 571 warning existing dan `0 errors` | `PASS` | Keluaran ESLint 1 September 2026 |
| ESLint terarah pada lima berkas JSX/JS task | Exit `0`; tidak ada error/warning task | `PASS` | `npm.cmd exec -- eslint ...` |
| `npm.cmd run build` | Next.js `16.2.12` berhasil compile dan menghasilkan 245 halaman statis | `PASS` | `✓ Compiled successfully in 31.8s`; postbuild standalone berhasil |
| Enam grep anti-regresi UI | Tidak ditemukan warna/typography literal, tombol Bootstrap mentah, tabel mentah, utility typography Bootstrap, atau `!important` pada source task | `PASS` | Pemeriksaan terarah enam berkas source |
| Pemeriksaan method pada hook | Tidak ditemukan `.post(`, `.put(`, `.patch(`, atau `.delete(` | `PASS` | `use-inpatient-monitoring.jsx` |
| `git diff --check` | Tidak ada whitespace error; hanya peringatan konversi LF ke CRLF dari Git Windows | `PASS` | Exit `0` |
| Empat tab/state/tujuan pada runtime | Tidak dijalankan karena data environment menunggu `RWI-UI-GAP-007` dan pengguna menetapkan lint/build sebagai validasi penyelesaian | `NOT FEASIBLE` | Gap dan arahan pengguna 1 September 2026 |

Uji manual: `NOT FEASIBLE` — episode yang membentuk empat daftar belum tersedia pada environment
target.

`AUTOMATED TEST: SKIPPED (opsional)` — pengguna meminta tidak menjalankan `testing.mjs`; task
divalidasi dengan full lint dan production build.

`MANUAL TEST: NOT FEASIBLE` — filter, retry, variasi error per endpoint, dan navigasi dengan data
nyata harus dibuktikan setelah `RWI-UI-GAP-007` ditutup.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Keempat tab menampilkan count dan mempertahankan tab aktif saat filter/retry | Terpenuhi | Count dibaca independen oleh `listCountState`; `updateFilter` dan `refresh` tidak mengubah `activeListKey` |
| Penutupan tertunda menawarkan Detail dan Penutupan Episode | Terpenuhi | Builder kolom pending closure merender dua `BaseButton` menuju route existing |
| Ketidakcocokan isolasi menawarkan jalan ke Detail/Perpindahan | Terpenuhi | Dua aksi tersedia; Perpindahan memakai route ber-anchor ke `episode-transfer-section` |
| Daftar lain menawarkan Detail Episode | Terpenuhi | Override closure dan unassigned nurse memakai `DetailAction` |
| Empty state menyatakan tidak ada tindak lanjut dan menyediakan jalan ke Daftar Kerja Episode | Terpenuhi | Copy empty ditambah kalimat eksplisit dan CTA `monitoring-empty-worklist` |
| Satu tab gagal tidak menutup tab lain yang berhasil | Terpenuhi | Empat count request mempunyai state `loading/error/count` terpisah; error isi hanya dipakai tab aktif |
| Tidak ada request tulis dari halaman daftar pantau | Terpenuhi | Hook hanya memanggil `inpatientMonitoringService.get`; pemeriksaan method tulis bersih |

Definition of Done source, lint, build, laporan, roadmap, dan traceability terpenuhi. Bukti runtime
penuh tetap menunggu data environment; tidak ada data pasien atau master tiruan yang ditanam.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Full lint tetap mempunyai 571 warning existing dan nol error. Query count menambah empat request `GET` ringan (`pageSize=1`) karena kontrak belum mempunyai endpoint summary |
| Masalah yang diketahui | Frontend tidak mempunyai katalog permission per tab. Backend saat ini memang menjaga keempat endpoint dengan permission yang sama, sehingga tidak ada perbedaan tab yang dapat disembunyikan secara sah |
| Dependency backend | Keempat endpoint tersedia. `RWI-UI-GAP-007` masih menahan data runtime untuk pembuktian nyata |
| Perubahan sampingan | `NONE` |
| Interupsi | Branch frontend maju secara eksternal dari `fe90a8bfe` ke `3133fb765` karena commit `FE-RWI-037` saat task berlangsung; perubahan `FE-RWI-038` tetap utuh dan lint/build dijalankan setelah advance tersebut. Agent tidak menjalankan pull/merge/commit |
| Status Git | Frontend: empat berkas modified dan dua berkas untracked milik `FE-RWI-038`. Backend: laporan, roadmap, dan traceability milik task ini modified/untracked |
| Langkah berikutnya | Sediakan data runtime untuk empat daftar lalu buktikan state per endpoint. Task roadmap berikutnya adalah `FE-RWI-039` dan memerlukan invocation skill terpisah |
