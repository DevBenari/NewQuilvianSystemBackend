# Laporan Perubahan Frontend — `FE-RWI-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-042` |
| Judul | Operational Entry Point Dokter Rawat Inap |
| Slice | `DOK-MVP-FE` urutan 1 — menu Dokter → Rawat Inap membuka daftar pasien episode |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), bagian 3 task `FE-RWI-042`; gerbang §4.1 butir 1, 2, 18, 20 |
| Trace | `DOK-TRC-FE-01`; `03-frontend-architecture.md` §0, §2.1, §3.1.1; `02-module-map.md` §3.3; `IA-INP-01`, `IA-INP-05`; `RWI-RULE-026` |
| Contract version | `0.3.0`. Endpoint census dan episode dikonsumsi apa adanya dari source backend as-is; tidak ada endpoint baru yang diminta maupun dikarang |
| Wewenang UI | [`skema-tampilan-dokter-rawat-inap.md`](../../../skema-tampilan-dokter-rawat-inap.md) §1–5, §15, §17–19, §21–22, ditambah rules §1 roadmap revision 2 dan keputusan operasional pengguna 2026-09-07 (menu Dokter → Rawat Inap dipertahankan). Jarak, dekorasi, dan nama berkas mengikuti `DEV_DISCRETION` di atas design token existing |
| Dependency | `—` (task ini tidak memiliki dependency). `BE-RWI-044` dan seterusnya **tidak** diperlukan karena entry hanya membaca census dan episode yang sudah ada |
| Klasifikasi | `HEAVY` — skor 11: repository 2, berkas diperiksa >20 (2), berkas diubah >8 (2), logika bisnis sedang (1), memakai kontrak API existing (1), database 0, keamanan/auth berkaitan tetapi bukan intinya (1), UI/workflow banyak halaman (2) |
| Task mode | `CROSS-REPO` sempit — source hanya frontend; pada repository backend hanya laporan ini, roadmap, dan `requirement-traceability.md` sub-modul yang sama |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/` untuk laporan, roadmap, dan traceability |
| Model | Implementasi dikerjakan sesi Codex (GPT-5); validasi ulang penuh, review diff akhir, laporan, dan penandaan status dikerjakan Claude Opus 5 |
| Commit frontend saat dikerjakan | `eb505a9ea20d99505a69ff0e6ea428ec9a551dc5` pada branch `HamzahV2` — sama persis dengan `focused_review_source_sha.frontend` roadmap. Perubahan task ini **belum di-commit**; commit dilakukan pemilik pekerjaan sendiri |
| Commit backend yang dijadikan rujukan | `350361c7c489e8d3e861908fcc8410ea46bbdabf` pada branch `MHamzah` — sama persis dengan `focused_review_source_sha.backend` roadmap |
| Tanggal | 7 September 2026 |
| Status | ✅ **SELESAI 7 September 2026.** Keenam acceptance criteria terpetakan ke source yang benar-benar ada. `npm run lint:errors` lulus tanpa error; `npm run test:unit` lulus **442/442**; `npm run build` berhasil dan kedua route baru muncul pada manifest build; sembilan skenario peramban `tests/e2e/inpatient-physician-entry.spec.mjs` lulus di Edge; satu skenario interaktif tambahan untuk filter, pencarian, reset, dan paginasi lulus. Isi klinis ruang kerja (Patient Safety Context lengkap dan enam tab) **bukan** scope task ini — itu `FE-RWI-043` |

---

## 1. Keadaan yang ditemukan di awal

Layar `Dokter → Rawat Inap` sudah ter-commit sebelum task ini, dan **memakai kontrak yang salah** — persis peringatan bagian 0 roadmap:

1. `doctor-inpatient-view.jsx` berdiri di atas mesin antrean rawat jalan. Ia memuat papan antrean, kartu pasien ber-`queueCode`, aksi **Panggil**, **Lewati**, **Tidak Hadir**, timer, dan kunci panggilan. Akibatnya layar bisa menampilkan pasien **rawat jalan** dengan label "Rawat Inap", lalu mengirim aksi antrean kepada mereka.
2. Daftar pasiennya disaring **tanggal hari ini** warisan antrean, sehingga pasien yang sudah dirawat beberapa hari — misalnya hari rawat ke-3 — hilang dari layar dokter yang merawatnya.
3. Tidak ada route ruang kerja per episode. URL `…/doctor-inpatient` langsung menjadi "workspace" tanpa episode, sehingga konteks pasien tidak pernah dikunci.
4. `tests/unit/` dan `tests/e2e/` tidak memuat satu pun test untuk ruang kerja dokter rawat inap — sesuai catatan coverage gap `DOK-TRC-VER-01`.
5. Belum ada komponen dasar klinis yang menyatukan keadaan memuat, kosong, gagal, ditolak, dan baca-saja; setiap layar menyusun sendiri, dan kegagalan baca berisiko terbaca sebagai "tidak ada pasien".

Yang **sudah** tersedia dan karena itu dipakai ulang: endpoint census beserta penyaring `doctorId` pada backend (`Areas/HealthServices/InPatientManagement/DTOs/InpatientCensusDtos.cs:17`, disaring lewat penugasan dokter pada `InpCensusQueryService.cs:877`), hook `use-inpatient-census.jsx`, service `inpatient-census.service.js`, katalog `doctor-clinical-base`, base component `DataFilter`/`FilterSelect`/`BaseButton`/`RegionPagination`, dan butir menu **Dokter → Rawat Inap** pada `src/utils/menu-sidebar/menu-items.jsx`.

---

## 2. Proses bisnis dari sisi pengguna

Penggunanya adalah **dokter** yang merawat pasien rawat inap. Layar ini dibuka setiap kali dokter hendak melihat pasien yang sedang ia rawat, lalu masuk ke ruang kerja satu pasien.

### 2.1 Alur normal

1. Dokter membuka sidebar **Dokter**. Di dalamnya ada dua butir berdampingan: **Rawat Jalan** dan **Rawat Inap**. Butir Rawat Inap menuju `/health-services/inpatient-management/doctor-inpatient`.
2. Halaman terbuka dengan judul **"Dokter - Rawat Inap"** dan kalimat pengantar "Pilih episode pasien yang masih dirawat sebelum membuka ruang kerja dokter."
3. Sistem membaca identitas dokter dari sesi yang sudah masuk, lalu memanggil census **selalu dengan `doctorId` dokter itu**. Tidak ada filter tanggal hari ini, sehingga pasien hari rawat ke-3, ke-7, maupun ke-14 tetap muncul.
4. Ringkasan di kepala halaman menyebut jumlah pasien aktif dan posisi halaman, misalnya "12 pasien aktif" dan "Halaman 1 / 2".
5. Tabel menampilkan satu baris per **episode**: Pasien (beserta kelas perawatan), No. RM, Episode, Lokasi/Kamar/Bed, DPJP, Hari Rawat, dan Status. Tombol **Buka Workspace** ada di setiap baris.
6. Dokter dapat menyaring lebih dulu: pencarian bebas (nama pasien, nomor rekam medis, nomor episode, kamar, tempat tidur), unit layanan, kelas perawatan, dan jumlah baris per halaman. Setiap penyaringan mengembalikan pembacaan ke halaman 1 supaya tidak ada halaman kosong palsu.
7. Dokter menekan **Buka Workspace** pada satu baris. Halaman berpindah ke `/health-services/inpatient-management/episodes/<id-episode>/physician`, yaitu ruang kerja **satu pasien dan satu episode**.
8. Ruang kerja membaca episode yang dipilih, lalu menampilkan konteksnya: Pasien, No. RM, Episode, Unit Layanan, Kelas Perawatan, DPJP, dan Status; dilengkapi tombol **Buka Detail Episode** dan **Kembali ke Daftar Pasien**.

### 2.2 Dua jalur masuk alternatif

Selain lewat menu, ruang kerja episode yang sama dapat dicapai dari:

- **Census Rawat Inap** — kolom Aksi setiap baris kini memuat dua tombol: **Detail Episode** (perilaku lama, tidak diubah) dan **Workspace Dokter** (baru).
- **Detail Episode** — action bar episode memperoleh tombol **Workspace Dokter** di depan aksi yang sudah ada; aksi lama seperti penutupan episode tetap pada tempatnya.

Ketiga jalur menghasilkan URL yang sama persis untuk episode yang sama, sehingga tidak ada dua "ruang kerja" yang berbeda untuk satu pasien.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Sesi belum selesai dibaca | Daftar menampilkan keadaan memuat, dan **tidak** ada request census yang dikirim sebelum identitas dokter diketahui |
| Akun tanpa identitas dokter | "Konteks dokter tidak tersedia" — daftar tidak menampilkan data pasien sama sekali |
| Census dijawab 403 | Sama, diperlakukan sebagai penolakan akses, bukan daftar kosong |
| Census gagal (500, jaringan putus) | "Daftar pasien gagal dimuat" beserta tombol **Coba Lagi** — bukan "belum ada pasien" |
| Dokter memang belum punya pasien | "Belum ada pasien rawat inap yang ditugaskan kepada Anda." |
| Penyaring tidak menemukan siapa pun | "Tidak ada pasien yang cocok dengan penyaring ini." beserta tombol **Atur Ulang Penyaring** — dibedakan dari keadaan sebelumnya |
| ID episode pada URL tidak sah | "Alamat workspace tidak memuat ID episode yang valid." |
| Episode yang dijawab server berbeda dari yang dipilih | "Episode yang diterima tidak cocok dengan episode yang dipilih." — konteks tidak diisi respons milik episode lain |
| Episode sudah selesai atau dibatalkan | Konteks tetap dapat dibaca, dengan pemberitahuan **"Episode dibuka dalam mode baca saja"** |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap dan kontrak: `roadmap/frontend-roadmap.md` (revision 2), `roadmap/requirement-traceability.md`, `skema-tampilan-dokter-rawat-inap.md`.
- Governance: `AGENTS.md` frontend, `rules/frontend/frontend-architecture.md`, `base-component-catalog.md`, `base-component-decision-gate.md`, `design-tokens.md`, `page-composition-patterns.md`, `ui-consistency-checklist.md`, `test-policy.md`.
- Backend sebagai bukti perilaku as-is: `Areas/HealthServices/InPatientManagement/Controllers/InpatientCensusController.cs`, `Controllers/InpatientEpisodeController.cs`, `DTOs/InpatientCensusDtos.cs`, `Services/InpCensusQueryService.cs`.
- Frontend: `menu-items.jsx`, `use-inpatient-census.jsx`, `inpatient-census-utils.jsx`, `inpatient-census-view.jsx`, `inpatient-census-table-columns.jsx`, `inpatient-episode-detail-view.jsx`, `inpatient-management-slice.jsx`, `login-slice.jsx`, katalog `src/components/ui/doctor-clinical-base/`, `data-filter.jsx`, `filter-select.jsx`, `pagination.jsx`, `base-button`.

### 3.2 Berkas yang berubah

**Baru**

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/inpatient-management/episodes/[id]/physician/page.jsx` | Route ruang kerja per episode. Hanya entry point dan metadata; ID episode dibaca dari `params` lalu diteruskan ke view |
| `src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx` | Base component klinis generik. Urutannya sengaja tetap: ditolak → gagal → memuat → kosong → konten, sehingga data lama tidak sempat terlihat saat otorisasi berubah atau request baru berjalan |
| `src/components/ui/doctor-clinical-base/clinical-state-boundary.module.css` | Style base tersebut; seluruh nilai visual memakai design token |
| `src/components/view/.../doctor-inpatient/inpatient-physician-patient-filters.jsx` | Adapter domain di atas `DataFilter` + `FilterSelect`: pencarian, unit layanan, kelas perawatan, jumlah baris |
| `src/components/view/.../doctor-inpatient/inpatient-physician-patient-list.jsx` | Daftar episode: tabel desktop `ClinicalDataTable` dengan `rowKeyFn` berbasis episode, kartu bertumpuk untuk layar kecil, dan tombol **Buka Workspace** per baris |
| `src/components/view/.../doctor-inpatient/inpatient-physician-workspace-entry-view.jsx` | View ruang kerja per episode: konteks episode terkunci, penjagaan ID tidak sah dan respons tidak cocok, mode baca-saja untuk episode selesai/batal |
| `src/lib/constants/.../inpatient-physician-constants.jsx` | Route entry, pembangun route workspace per episode, konfigurasi daftar, dan dua kalimat keadaan kosong yang berbeda |
| `src/lib/hooks/.../use-inpatient-physician-patients.jsx` | Hook daftar pasien dokter: mengambil `doctorId` dari sesi, menahan request sebelum identitas diketahui, dan **tidak** menampilkan data yang belum tercakup dokter itu |
| `src/lib/hooks/.../use-inpatient-physician-workspace-entry.jsx` | Hook konteks episode: membatalkan request lama saat episode berganti, memisahkan penolakan akses dari kegagalan baca |
| `src/style/health-services/inpatient-management/doctor-inpatient-entry.module.css` | Style halaman entry dan ruang kerja, termasuk representasi kartu untuk layar kecil |
| `tests/unit/inpatient-physician-entry.test.mjs` | Delapan test source-level: parameter census, normalisasi pasien hari rawat ke-3, route workspace, urutan menu, nol dependency antrean, keadaan layar, tiga jalur masuk, dan penahanan request tanpa identitas dokter |
| `tests/e2e/inpatient-physician-entry.spec.mjs` | Sembilan skenario peramban dengan API tiruan: navigasi menu, dua entry alternatif, episode selesai, memuat, ditolak, gagal, serta tiga viewport |

**Diubah**

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../doctor-inpatient/doctor-inpatient-view.jsx` | Ditulis ulang. Papan antrean, kartu ber-`queueCode`, aksi Panggil/Lewati/Tidak Hadir, timer, dan kunci panggilan dilepas; isinya kini header, ringkasan, penyaring, batas keadaan, dan daftar episode |
| `src/lib/hooks/.../use-inpatient-census.jsx` | Menerima `doctorId`, `enabled`, dan `resourceKey`. `doctorId` selalu ikut pada query; hasil census diberi penanda lokal `doctorIdScope` agar cache dokter sebelumnya tidak sempat terlihat |
| `src/utils/.../inpatient-census-utils.jsx` | `buildCensusQuery` meneruskan `doctorId` bila ada; tidak ada parameter tanggal yang ditambahkan |
| `src/lib/state/slice/.../inpatient-management-slice.jsx` | Kunci resource `physicianCensus` ditambahkan supaya daftar dokter tidak berbagi cache dengan census umum |
| `src/components/view/.../inpatient-census-table-columns.jsx` | Kolom Aksi memperoleh tombol **Workspace Dokter** sebagai tambahan; **Detail Episode** tetap ada dan tidak berubah. Dikendalikan opsi `includePhysicianWorkspace` yang bawaannya menyala |
| `src/components/view/.../inpatient-episode-detail-view.jsx` | Action bar episode memperoleh tombol **Workspace Dokter**; aksi existing tidak digeser maknanya |
| `src/style/.../inpatient-census.module.css` | `.actionStack` supaya dua tombol aksi tetap rapi dan membungkus pada layar sempit |
| `src/components/ui/doctor-clinical-base/index.js` | Mengekspor `ClinicalStateBoundary` |
| `src/app/.../doctor-inpatient/page.jsx` | Menambahkan `metadata.title` "Dokter - Rawat Inap"; route tetap hanya entry point |

**Dihapus**

| Berkas | Alasan |
| --- | --- |
| `src/components/view/.../doctor-inpatient/inpatient-screening-tab.jsx` (314 baris) | Tab skrining berbasis antrean; tidak lagi dipakai jalur mana pun setelah view ditulis ulang |
| `src/style/health-services/inpatient-management/doctor-inpatient.module.css` (681 baris) | Stylesheet papan antrean rawat inap; yatim setelah papan antrean dilepas |

Komponen antrean **Rawat Jalan** tidak disentuh sama sekali; yang dihapus hanya berkas yang benar-benar tidak lagi diimpor jalur rawat inap.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi mengikuti `rules/frontend/frontend-architecture.md`: route di `src/app` hanya entry point dan metadata → view di `src/components/view/...` → hook di `src/lib/hooks/...` → service Axios existing → Redux slice modul. Tidak ada arsitektur state, HTTP, atau abstraksi paralel yang ditambahkan.
- Definisi kolom tabel tetap di berkas komponen daftar domain, sejalan dengan pola `doctor-clinical-base` yang dipakai layar klinis lain; halaman census yang memang mengikuti pola list umum tetap memakai `<feature>-table-columns.jsx`.
- Tidak ada panggilan `fetch` mentah: seluruh request lewat service existing di atas `InstanceAxios`.
- Tidak ada nilai visual literal. Warna, tipografi, jarak, radius, dan bayangan seluruhnya token; grep anti-regresi dicatat pada bagian 6.

### 3.4 Gerbang keputusan base component

`UI GATE: 11 elemen — REUSE 6, EXTEND 1, COMPOSE 2, WRAP 1, NEW 1`

| Elemen | Keputusan | Alasan dan konsekuensi |
| --- | --- | --- |
| Header halaman | `REUSE` | `ClinicalPageHeader` apa adanya |
| Ringkasan jumlah pasien | `REUSE` | `ClinicalSummaryBar`, diisi jumlah pasien census — bukan hitungan antrean |
| Tabel episode | `REUSE` | `ClinicalDataTable` dengan `rowKeyFn` berbasis id episode; API base tidak diubah |
| Badge status episode | `REUSE` | `ClinicalStatusBadge` |
| Tombol aksi | `REUSE` | `BaseButton`; tidak ada `<button>` mentah maupun `.btn` Bootstrap |
| Paginasi | `REUSE` | `RegionPagination` lewat komponen pagination existing |
| Penyaring pasien | `COMPOSE` | Pilihan 1 **(rekomendasi, dipakai)** `DataFilter` + `FilterSelect` dalam adapter domain — konsisten dan tetap tipis. Pilihan 2 markup filter lokal — lebih cepat, tetapi menduplikasi pola dan berisiko menyimpang dari standar filter |
| Tampilan kartu untuk layar kecil | `COMPOSE` | Pilihan 1 **(rekomendasi, dipakai)** representasi kartu dari data dan aksi yang sama — informasi kritis terbaca tanpa mengubah API base table. Pilihan 2 mengubah API `ClinicalDataTable` — dampaknya melebar ke konsumen lain |
| Daftar pasien dan navigasi episode | `WRAP` | Pilihan 1 **(rekomendasi, dipakai)** komponen domain tipis — memisahkan view dan hook serta mudah diuji. Pilihan 2 menaruh semua di view — berkas lebih sedikit, tetapi tanggung jawab bercampur |
| Aksi alternatif dari Census dan Detail Episode | `EXTEND` | Pilihan 1 **(rekomendasi, dipakai)** menambah aksi opsional menuju workspace — perilaku lama utuh. Pilihan 2 mengganti aksi Detail Episode — lebih sederhana, tetapi merusak jalur yang sudah dipakai. Perluasan hanya lewat opsi `includePhysicianWorkspace`, bukan perubahan perilaku bawaan base component |
| Keadaan memuat/kosong/gagal/ditolak/baca-saja | `NEW` | Pilihan 1 **(rekomendasi, dipakai)** `ClinicalStateBoundary` generik — sudah dikunci kontrak task sebagai deliverable dan menjadi milik bersama slice klinis berikutnya. Pilihan 2 menyusun keadaan per halaman — tanpa base baru, tetapi berulang dan menyimpang dari deliverable `FE-RWI-042` |

**Catatan kejujuran atas status `NEW`.** Aturan gerbang meminta status `NEW` menunggu keputusan user. Tabel pilihan di atas sudah disajikan kepada pemilik pekerjaan pada sesi implementasi 7 September 2026, dan `ClinicalStateBoundary` memang tercantum eksplisit sebagai **New Base Components** pada kartu task `FE-RWI-042` di roadmap yang sudah `APPROVED`. Pekerjaan dilanjutkan atas dasar kontrak roadmap itu; pemilik pekerjaan tidak menyampaikan jawaban pilihan yang berbeda. Bila pemilik menghendaki pilihan 2, perubahannya terbatas pada satu berkas base dan penggunanya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kartu memuat dengan kerangka baris: "Mengambil pasien rawat inap Anda..." beserta keterangan bahwa daftar dibatasi identitas dokter pada sesi. Bukan layar kosong |
| Kosong — belum ada pasien | "Belum ada pasien rawat inap yang ditugaskan kepada Anda." beserta penjelasan bahwa pasien muncul setelah dokter tercatat sebagai DPJP aktif |
| Kosong — penyaring tidak cocok | "Tidak ada pasien yang cocok dengan penyaring ini." beserta tombol **Atur Ulang Penyaring** |
| Gagal | "Daftar pasien gagal dimuat" beserta pesan dari server dan tombol **Coba Lagi**. Tidak pernah diturunkan menjadi keadaan kosong |
| Tanpa hak akses | "Konteks dokter tidak tersedia" — akun tanpa identitas dokter yang sah atau tanpa izin membaca census; tidak ada satu pun baris pasien yang ditampilkan |
| Baca saja (ruang kerja) | "Episode dibuka dalam mode baca saja" untuk episode selesai atau dibatalkan; konteks tetap terbaca |
| Permintaan berganti | Saat episode berpindah, request lama dibatalkan dan respons yang tidak cocok ditolak dengan kalimat "Episode yang diterima tidak cocok dengan episode yang dipilih." |

Setiap keadaan membawa penanda `data-clinical-state` yang berbeda (`loading`, `empty`, `error`, `denied`, `ready`), sehingga perbedaannya dapat diverifikasi mesin, bukan hanya dilihat mata.

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Census

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/census/filters/metadata` | Mengisi pilihan unit layanan, kelas perawatan, dan jumlah baris pada penyaring | `InpatientCensus : Read` |
| `GET` | `/v1/health-services/inpatient-management/census` | Daftar pasien rawat inap dokter. **Selalu** membawa `doctorId` dari sesi, ditambah `search`, `serviceUnitId`, `patientClassId`, `pageNumber`, `pageSize`, `sortBy`, `sortDirection` | `InpatientCensus : Read` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/episodes/{id}` | Mengunci konteks satu episode saat ruang kerja dibuka | `InpatientEpisode : Read` |

Nama resource census diambil dari source backend, bukan dari dokumen: `InpatientCensusController.cs` memasang `[AccessPermission("InpatientCensus", "Read")]` pada ketiga endpoint-nya. Ini menutup butir `RESOLVED — selisih dokumentasi permission` pada §6 roadmap. `DoctorConsultation : Read` **belum** dipakai karena task ini belum membaca satu pun dokumen klinis; pemakaiannya jatuh pada `FE-RWI-043`.

---

## 6. Verifikasi

Seluruh angka di bawah berasal dari eksekusi nyata pada 7 September 2026.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Keluaran perintah kosong (eslint `--quiet`) |
| `npm run test:unit` | `tests 442`, `pass 442`, `fail 0` | `PASS` | Ringkasan runner Node, termasuk delapan test `FE-RWI-042` |
| `npm run build` | Kompilasi produksi berhasil, keluar dengan kode 0 | `PASS` | `.next/BUILD_ID` tertulis 7 September 2026 pukul 13:53 |
| Route baru masuk hasil build | `/health-services/inpatient-management/doctor-inpatient` dan `/health-services/inpatient-management/episodes/[id]/physician` | `PASS` | `.next/app-path-routes-manifest.json` |
| `tests/e2e/inpatient-physician-entry.spec.mjs` (Edge, server standalone port 3710, API tiruan) | **9 passed** — menu → census terfilter → episode yang sama tanpa request antrean; Census dan Detail Episode sebagai entry alternatif; episode selesai baca-saja; memuat; ditolak; gagal beserta retry; tiga viewport desktop/tablet/mobile | `PASS` | Keluaran reporter `list` Playwright |
| Skenario interaktif filter, pencarian, reset, dan paginasi (spec sementara, Edge, API tiruan) | **1 passed** — halaman 2 memuat pasien berikutnya dengan `doctorId` tetap terkirim; pencarian mengembalikan halaman ke 1 dan menyempitkan hasil; filter gabungan pencarian + unit layanan memunculkan keadaan "tidak cocok dengan penyaring", bukan "belum ada pasien"; label filter terpilih terbaca pada pemicunya; reset melepas seluruh penyaring dan mengembalikan daftar penuh; nol request antrean sepanjang skenario | `PASS` | Keluaran reporter `list` Playwright |
| Grep anti-regresi UI pada berkas yang diubah | Warna literal 0; tombol non-base 0; `<table>` mentah 0; utility typography Bootstrap 0; `!important` baru 0; blok `prefers-color-scheme` baru 0 | `PASS` | Perintah grep §G `ui-consistency-checklist.md` |
| Pemindaian dependency antrean pada jalur baru | Nol kecocokan untuk `queue`, `Panggil`, `Lewati`, `Tidak Hadir`, `no-show`, `skip` pada route, view, komponen, dan hook jalur rawat inap; import service hanya census dan episode | `PASS` | Grep atas 8 berkas jalur baru beserta hook turunannya |

**Uji manual:** `PASS` — dikerjakan lewat peramban Edge yang dikemudikan skrip Playwright terhadap hasil build produksi, bukan pengamatan kode. Kontrol interaktif yang diverifikasi: daftar opsi filter, label keadaan terpilih, efek terhadap request dan hasil, reset, filter gabungan, pencarian, paginasi, tombol **Coba Lagi**, tombol **Atur Ulang Penyaring**, tombol **Buka Workspace** dari tabel maupun kartu, dan dua tombol entry alternatif.

**Catatan cara menjalankan e2e.** Repository tidak memiliki `playwright.config.*`, dan binary browser bawaan tidak cocok versi. Karena itu dipakai config sementara di dalam repository dengan `channel: "msedge"`, dan config itu **dihapus setelah dijalankan** supaya tidak masuk diff. Spec interaktif filter juga bersifat sementara dan sudah dihapus; hasilnya dicatat di tabel atas sebagai bukti verifikasi, bukan sebagai berkas yang ditinggalkan. Server yang diuji adalah `.next/standalone/server.js` dengan API dipalsukan lewat `page.route`, sehingga tidak menyentuh backend maupun database tim.

**Tidak dijalankan:** `npm run lint` versi penuh (dengan warning) tidak dijalankan terpisah karena `lint:errors` sudah menutup gerbang error; `npm run test:e2e` polos tidak dipakai karena akan menyapu `tests/unit/*.test.mjs` dan gagal karena sebab yang tidak berhubungan dengan task ini.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Menu **Dokter → Rawat Inap tetap tersedia**, bersama Rawat Jalan | Terpenuhi | `src/utils/menu-sidebar/menu-items.jsx` kelompok **Dokter** memuat **Rawat Jalan** lalu **Rawat Inap**; test unit "menu Dokter mempertahankan Rawat Jalan lalu Rawat Inap"; skenario peramban mengklik butir menunya, bukan membuka URL langsung |
| 2. Workspace dicapai lewat menu → daftar pasien → episode, juga lewat Census dan detail episode | Terpenuhi | Tombol **Buka Workspace** pada tabel dan kartu; tombol **Workspace Dokter** pada kolom aksi census dan action bar detail episode; skenario "Census dan Detail Episode menjadi entry alternatif workspace yang sama" |
| 3. Daftar memakai census pasien rawat inap sesuai dokter; pasien rawat jalan tidak muncul | Terpenuhi | `doctorId` sesi selalu ikut pada query census; backend menyaring lewat penugasan dokter pada episode (`InpCensusQueryService.cs:877`) dan census memang hanya diturunkan dari penempatan rawat inap yang masih aktif; skenario peramban menolak census tanpa `doctorId` dan tetap lulus |
| 4. Nol request layanan antrean pada entry/workspace dan dependency turunannya | Terpenuhi | Pemindaian import dan dependency transitif nol; kedua skenario peramban memeriksa seluruh URL yang diminta halaman dan tidak menemukan satu pun request antrean |
| 5. Tidak ada Panggil/Lewati/Tidak Hadir, timer, lock, atau `queueCode` | Terpenuhi | View lama ditulis ulang; tab skrining dan stylesheet papan antrean rawat inap dihapus; grep nol pada jalur baru |
| 6. Satu baris episode membuka pasien/episode yang cocok; pasien hari rawat ke-3 tidak hilang karena filter tanggal | Terpenuhi | Route workspace dibangun dari `episodeId` baris; ruang kerja menolak respons episode yang tidak cocok; `buildCensusQuery` tidak pernah mengirim parameter tanggal — dibuktikan test unit yang memeriksa ketiadaan `date`/`today` dan normalisasi pasien hari rawat ke-3 |
| Visual: judul dan kolom jelas menyatakan rawat inap; episode dan lokasi terbaca sebelum memilih | Terpenuhi | Judul "Dokter - Rawat Inap"; kolom Episode dan Lokasi/Kamar/Bed ada di tabel maupun kartu |
| Visual: memuat, kosong, gagal, ditolak berbeda | Terpenuhi | Empat penanda `data-clinical-state` berbeda; dua kalimat kosong yang berbeda; skenario memuat, ditolak, dan gagal terpisah |
| Visual: enam tab workspace tidak dibuat sebagai menu sidebar tambahan | Terpenuhi | Kelompok menu Dokter tetap dua butir; tidak ada butir menu klinis baru |
| Visual: desktop, tablet, mobile memenuhi §1.4 | Terpenuhi | Tiga skenario viewport lulus; pada layar kecil daftar berubah menjadi kartu bertumpuk yang tetap memuat nama, No. RM, episode, lokasi, status, dan **Buka Workspace** |
| Gerbang §4.1 butir 1 — tanpa dependency antrean | Terpenuhi | Sama dengan kriteria 4 dan 5 |
| Gerbang §4.1 butir 2 — satu workspace satu pasien dan satu episode | Terpenuhi | Route per episode; penjagaan ID tidak sah dan respons tidak cocok; request lama dibatalkan saat episode berganti |
| Gerbang §4.1 butir 18 — kosong, gagal, ditolak berbeda dan tidak ada sukses palsu | Terpenuhi | Bagian 4 laporan ini beserta skenario gagal/ditolak |
| Gerbang §4.1 butir 20 — layout terpakai pada tiga viewport | Terpenuhi | Tiga skenario viewport |
| DoD: laporan menyebut route, sumber request, bukti tiga entry dan viewport | Terpenuhi | Bagian 2, 3, 5, dan 6 laporan ini |

**Yang belum dan sengaja tidak diklaim:** ruang kerja pada task ini baru mengunci **konteks episode**. Patient Safety Context lengkap (alergi, diagnosis kerja, hari rawat, penanda kewenangan) dan keenam tab klinis adalah scope `FE-RWI-043` beserta dependency `BE-RWI-044`, dan tidak dikerjakan di sini.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kunci kelompok menu masih bernama `healthServicesDoctorQueue` — warisan penamaan, bukan dependency antrean; mengganti kunci menu berdampak ke penyimpanan status menu pengguna sehingga tidak dilakukan tanpa permintaan |
| Masalah yang diketahui | Backend belum menyediakan endpoint khusus "pasien saya"; daftar memakai penyaring `doctorId` pada census apa adanya. Akibatnya akun tanpa `doctorId` pada sesi — misalnya admin — melihat keadaan "Konteks dokter tidak tersedia", dan itu memang perilaku yang dikehendaki rules §1 roadmap |
| Dependency backend | `NONE` untuk task ini. Endpoint census dan episode sudah tersedia pada SHA rujukan. Dependency `BE-RWI-044` dan seterusnya baru mengikat `FE-RWI-043` dan tab-tab klinis |
| Perubahan sampingan | Dua berkas warisan antrean rawat inap dihapus karena benar-benar yatim setelah view ditulis ulang: `inpatient-screening-tab.jsx` dan `doctor-inpatient.module.css`. Komponen antrean **Rawat Jalan** tidak disentuh. Artefak sementara — config Playwright, spec interaktif, dan folder `test-results/` — sudah dihapus sehingga tidak masuk diff |
| Interupsi | Sesi implementasi sebelumnya berhenti karena batas pemakaian. Pekerjaan dilanjutkan dari kondisi kerja yang terverifikasi lewat `git status`/`git diff`, lalu seluruh validasi **dijalankan ulang dari awal** sesudah patch cache-scope terakhir; tidak ada hasil lama yang dipakai ulang sebagai klaim |
| Status Git | Frontend `HamzahV2`, 23 berkas — 12 baru, 9 diubah, 2 dihapus — **belum di-commit** — commit dan push dilakukan pemilik pekerjaan sendiri. Backend `MHamzah`, hanya dokumen blueprint sub-modul ini yang disentuh |
| Langkah berikutnya | `FE-RWI-043` — Patient Safety Context dan enam tab klinis di atas route `/episodes/[id]/physician` yang sudah tersedia, setelah `BE-RWI-044` dipastikan siap |
