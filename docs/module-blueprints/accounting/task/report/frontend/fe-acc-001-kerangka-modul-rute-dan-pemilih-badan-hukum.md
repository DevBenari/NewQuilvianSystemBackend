# `FE-ACC-001` — Kerangka modul, rute, dan pemilih badan hukum

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-001` |
| Blueprint | `ACC-BP-001` revisi 10, `roadmap/frontend-roadmap.md` gelombang `MVP-1` |
| Task type | Frontend, kerangka modul (belum memanggil endpoint bisnis Accounting) |
| Task mode | `FRONTEND` |
| Kontrak | `ACC-API-0.3` — tidak dipakai pada task ini; satu-satunya panggilan data adalah daftar badan hukum milik modul HR |
| Wewenang UI | `ACC-FE-001` `closed`, `ACC-FE-003` `closed`, `ACC-FE-004`..`008` `DEV_DISCRETION` |
| Repository target tulis | `QuilvianSystemFrontendDev` |
| Branch | `RizkiV2` @ `1a86d9322` |
| Status | **`IMPLEMENTED` — menunggu verifikasi manual owner di peramban.** Alasannya di bagian 6 |
| Tanggal | 4 September 2026; **direvisi 4 September 2026** — lihat bagian 11 |

## Ringkasan untuk pembaca umum

Modul Akuntansi kini punya **pintu masuknya sendiri**. Sebelum ini, seluruh pekerjaan Accounting
hanya ada di backend — bisa dipanggil lewat Swagger, tetapi tidak ada layar yang bisa dibuka staf
akuntansi.

Yang dikerjakan task ini:

1. **Menu "Akuntansi" muncul di sidebar**, di kelompok *Perusahaan*, bersebelahan dengan Sumber
   Daya Manusia. Menekannya membuka halaman `/corporate/accounting`.
2. **Halaman itu menampilkan pemilih Badan Hukum.** Pembukuan dipisah per badan hukum
   (`ACC-DEC-037`), sehingga setiap layar akuntansi berikutnya perlu tahu badan hukum mana yang
   sedang dikerjakan. Pilihan itu **ikut terbawa** saat pengguna berpindah layar, sehingga tidak
   perlu memilih ulang berkali-kali.
3. **Halaman itu juga menjelaskan isi modul** — delapan layar rilis pertama, mana yang sudah
   tersedia dan mana yang belum. Layar yang belum dibangun sengaja **tidak** diberi tautan supaya
   tidak ada yang menekan tombol lalu mendarat di halaman 404.
4. **Pengguna tanpa hak akses melihat pesan "Akses Ditolak"**, bukan halaman kosong yang
   membingungkan.

Yang **belum** dikerjakan task ini: seluruh layar bisnisnya. Daftar akun, jurnal, buku besar, dan
neraca saldo menyusul di `FE-ACC-002` sampai `FE-ACC-009`.

## 1. Wewenang UI yang dipakai

| Decision | Pilihan owner | Cara dipakai di task ini |
|---|---|---|
| `ACC-FE-001` | `src/app/corporate/accounting/` (pilihan **B**) | Segmen `corporate/` dibuat baru dan dipakai **konsisten di lima lapisan**: rute, view, konstanta, hook, dan style. Keputusan sempat ditetapkan pilihan A pada hari yang sama, lalu **diubah owner menjadi B** sebelum task ditutup; berkasnya dipindahkan dan seluruh import ditulis ulang |
| `ACC-FE-003` | Halaman tersendiri, `base-detail-view.jsx` | **Belum terpakai** di task ini; ia mengikat `FE-ACC-007`. Dicatat agar tidak hilang |
| `ACC-FE-004` | `DEV_DISCRETION` | Belum ada tabel di layar ini |
| `ACC-FE-005` | `DEV_DISCRETION` | CSS Module diletakkan di `src/style/corporate/accounting/`, mengikuti segmen `corporate/` yang sama dengan lapisan lain |
| `ACC-FE-006` | `DEV_DISCRETION` | Ikon dari `react-icons/ri`. Blueprint menyebut `fa6` sebagai kecenderungan umum, tetapi `menu-items.jsx` memakai `ri` **secara seragam** — mengikuti berkas yang disunting lebih penting daripada kecenderungan umum |
| `ACC-FE-008` | `DEV_DISCRETION` | `sessionStorage` satu kunci. Rinciannya di bagian 3 |

## 2. Gerbang pemakaian ulang komponen

Aturan repository melarang membuat arsitektur tandingan. Sebelum menulis apa pun, kebutuhan
task ini dipetakan ke yang sudah ada:

| Kebutuhan | Dipakai ulang | Bukti pemeriksaan | Putusan |
|---|---|---|---|
| Daftar badan hukum | Resource select **`legalEntities`** yang sudah terdaftar | `src/lib/hooks/select/hr/hr-select-resources.js:285` — endpoint, `valueKey`, `labelKeys` sudah ditetapkan di sana | **REUSE.** Nol thunk baru, nol slice baru, nol Axios instance baru |
| Kotak pilihan | `ResourceFilterSelect` → `FilterSelect` | `resource-filter-select.jsx` memang adapter resmi antara `useSelectResource` dan `FilterSelect` | **REUSE** |
| Penolakan hak akses | `AccessDeniedGate` | `access-denied-gate.jsx` — menerima `error`, menampilkan `AccessDeniedAlert` bila pesannya berarti akses ditolak | **REUSE** |
| Kepala halaman | `Hero` | Dipakai seluruh view `hr` dan `health-services` | **REUSE** |
| Penanda status | `StatusBadge` | Dipakai view master data `hr` | **REUSE** |
| Kerangka halaman | `base-data-components.module.css` (`dataPage`, `dataShell`, `errorAlert`) | Kelasnya sudah ada dan dipakai lintas modul | **REUSE** |
| Kartu area modul | — | Tidak ada komponen kartu daftar-tautan yang cocok; `SummaryCards` untuk angka ringkas, bukan untuk ini | **CSS Module baru**, memakai token `--base-*` yang sudah ada. Nol komponen base baru |

**Nol Redux slice baru, dan `store.jsx` tidak disentuh.** `useSelectResource` tidak memakai Redux
sama sekali — diverifikasi: nol `useSelector`/`useDispatch` di `use-select-resource.jsx` — sehingga
task ini tidak perlu mendaftarkan reducer ke-143.

## 3. Cara pilihan badan hukum bertahan

`ACC-FE-008` membebaskan caranya, dengan satu syarat: **ia bukan pengamanan.**

Yang dipakai: satu kunci `sessionStorage`, `quilvian_accounting_legal_entity`.

| Hal | Keterangan |
|---|---|
| Kenapa bukan Redux | Menambah reducer ke-143 hanya untuk menyimpan satu teks lebih berat daripada manfaatnya, dan Redux hilang saat halaman dimuat ulang |
| Kenapa `sessionStorage`, bukan `localStorage` | Pilihan badan hukum melekat pada sesi kerja, bukan pada peramban selamanya. Pengguna berikutnya di komputer yang sama tidak mewarisi pilihan orang sebelumnya |
| Dibaca kapan | Sesudah render pertama lewat `useEffect`, **bukan** saat inisialisasi state — supaya hasil render server dan klien tidak berbeda |
| Bila peramban menolak penyimpanan | Dibungkus `try`/`catch`. Modul tetap berjalan, pilihannya saja yang tidak bertahan |
| **Bukan pengamanan** | Backend tetap menolak badan hukum yang bukan hak pengguna, dan `AccountingLegalEntityGuard` tetap berlaku. Nilai ini hanya kenyamanan agar petugas tidak memilih ulang di tiap layar |

## 4. Berkas yang berubah

### Ditambahkan

| Berkas | Baris | Isi |
|---|---:|---|
| `src/app/corporate/accounting/page.jsx` | 7 | Rute tipis, `metadata.title = "Akuntansi"` |
| `src/app/corporate/accounting/accounting-client.jsx` | 7 | Pembungkus `"use client"` |
| `src/components/view/corporate/accounting/accounting-home-view.jsx` | 94 | Isi halaman: gerbang akses, hero, pemilih badan hukum, daftar delapan layar |
| `src/components/view/corporate/accounting/shared/accounting-legal-entity-select.jsx` | 52 | Pemilih badan hukum, dipisah karena seluruh layar berikutnya memakainya |
| `src/lib/hooks/corporate/accounting/use-accounting-legal-entity.jsx` | 98 | Memuat pilihan lewat resource `legalEntities`, menyimpan pilihannya |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | 101 | Teks hero, kunci penyimpanan, dan daftar delapan layar |
| `src/style/corporate/accounting/accounting-home-view.module.css` | 123 | Gaya halaman, memakai token `--base-*` yang sudah ada |

> **Catatan perpindahan.** Berkas-berkas ini semula ditulis di `src/app/accounting/` dan
> pasangannya tanpa segmen `corporate/`, mengikuti `ACC-FE-001` pilihan A. Owner mengubah
> keputusannya menjadi pilihan B pada hari yang sama, sebelum task ditutup. Seluruh berkas
> dipindahkan, kelima folder `corporate/` dibuat baru, dan seluruh import ditulis ulang;
> `grep` atas jalur lama mengembalikan **nol** rujukan tersisa.

### Diubah

| Berkas | Perubahan |
|---|---|
| `src/utils/menu-sidebar/menu-items.jsx` | **+8 baris.** Satu entri menu `Akuntansi` di akhir kelompok *Perusahaan*, ditambah satu ikon `RiBookletLine` pada daftar import |

Menu sidebar ternyata **statis**, bukan diturunkan dari backend — sumbernya
`src/utils/menu-sidebar/menu-items.jsx` yang disaring `filter-menu-items-by-role.jsx`. Karena itu
pendaftaran menu memang bagian dari task ini, bukan efek samping backend. Penyaring perannya
sendiri tidak disentuh: ia hanya menangani satu kunci lama `ManajemenKesehatan`, dan visibilitas
Accounting ditegakkan `AccessDeniedGate` di tingkat halaman.

## 5. Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `npm run lint:errors` | **PASS**, exit 0 |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | **PASS**, exit 0, nol temuan |
| `npm run build` | **PASS**, exit 0. `postbuild` `prepare-standalone.mjs` juga berhasil |
| Unit test — 434 test | **PASS**, `# pass 434`, `# fail 0` |

### `npm run test:unit` gagal karena glob, bukan karena perubahan ini

`UNRELATED EXISTING ISSUE`. Skrip-nya berbunyi
`node --import ./tests/helpers/register.mjs --test "tests/unit/**/*.test.mjs"`. Node **v20.20.2**
belum meng-ekspansi pola `**` di dalam `--test` — kemampuan itu baru ada di Node 22 — sehingga
skrip ini bergantung pada shell untuk meng-ekspansinya. PowerShell tidak melakukannya, dan Node
menerima polanya apa adanya lalu melapor
`Could not find 'tests\unit\**\*.test.mjs'`.

Dijalankan lewat shell yang meng-ekspansi glob, perintah yang sama **lulus seluruhnya**:

```
# tests 434
# pass 434
# fail 0
# duration_ms 842.7
```

Tidak diperbaiki, sesuai aturan cakupan perubahan. Dicatat di sini supaya tidak terus-menerus
terlihat seperti kegagalan baru.

### Bukti bahwa rutenya benar-benar terbentuk

| Pemeriksaan | Hasil |
|---|---|
| `app-paths-manifest.json` | memuat `"/corporate/accounting/page"` |
| Berkas hasil build | `.next/server/app/corporate/accounting.html`, `.rsc`, `.meta`, `.segments` terbentuk |
| `<title>` hasil render | `Akuntansi` |
| Perbandingan dengan rute mapan | `corporate/accounting.html` dan `hr/master-data/allowance-type.html` sama-sama menampilkan tahap `Memverifikasi sesi pengguna...` milik route guard — artinya rute baru masuk ke alur autentikasi yang sama persis, bukan jalur khusus |

## 6. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE` sebagian

Tiga dari empat acceptance menuntut sesi pengguna yang benar-benar masuk. Sesi ini tidak memiliki
kredensial, dan mengambil kredensial dari `.env` dilarang aturan keselamatan lingkungan. Karena
itu bagian yang menuntut peramban **tidak** saya klaim lulus.

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Rute dapat dibuka dan menampilkan tata letak | **TERBUKTI SEBAGIAN** | Rute terbentuk, ter-build, dan masuk alur route guard yang sama dengan rute mapan. Tampilan sesudah login belum dilihat |
| (2) | Pemilih badan hukum tampil dan pilihannya bertahan antar layar | **BELUM TERBUKTI** | Butuh peramban dan sesi login |
| (3) | Pengguna tanpa hak melihat `access-denied-gate` | **BELUM TERBUKTI** | Butuh akun tanpa hak akses Accounting |
| (4) | Nol `style={{ }}`, `globals.css` tidak tersentuh | **TERBUKTI** | Pencarian `style={{` pada seluruh berkas baru: **0**. `git diff --name-only -- src/app/globals.css`: **0 berkas** |

### Skrip uji manual untuk owner

Prasyarat: backend berjalan, dan pengguna dapat masuk.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | `npm run dev`, masuk sebagai pengguna berhak, lihat sidebar kelompok **Perusahaan** | Muncul menu **Akuntansi** di bawah Sumber Daya Manusia |
| 2 | Tekan menu itu | Halaman `/corporate/accounting` terbuka, hero berjudul **Akuntansi**, dan delapan kartu layar tampil dengan penanda *Belum tersedia* |
| 3 | Buka pemilih **Badan Hukum** | Daftar badan hukum termuat. Diharapkan memuat `PT Metropolitan Medical Centre` |
| 4 | Pilih `PT Metropolitan Medical Centre` | Nama itu tampil sebagai pilihan terpilih, dan pesan *"Pilih badan hukum lebih dahulu"* hilang |
| 5 | **Inti acceptance (2).** Pindah ke layar lain, lalu kembali ke `/corporate/accounting` | Pilihannya **masih** `PT Metropolitan Medical Centre` |
| 6 | Muat ulang halaman dengan F5 | Pilihannya **tetap** bertahan — inilah gunanya `sessionStorage` |
| 7 | Tutup seluruh tab lalu buka lagi | Pilihannya **kosong** kembali. Ini memang dikehendaki: pilihan melekat pada sesi kerja |
| 8 | **Acceptance (3).** Masuk sebagai pengguna **tanpa** hak akses Accounting, buka `/corporate/accounting` | Muncul kartu **"Ups! Akses Ditolak"**, bukan halaman kosong |
| 9 | Perkecil jendela sampai selebar ponsel | Kartu layar menjadi satu kolom, tidak ada yang terpotong |

## 7. Delta terhadap kontrak

**Nol.** Task ini tidak memanggil satu pun endpoint `ACC-API-0.3`. Satu-satunya panggilan data
adalah `GET /v1/corporate/human-resource/master-data/legal-entities/options`, milik modul HR, lewat
resource select yang sudah terdaftar — bukan endpoint baru dan bukan pemakaian baru.

## 8. Catatan tindakan Git

Tiga tindakan Git dilakukan di repository frontend sebelum implementasi, dan dicatat apa adanya:

| Tindakan | Dasar |
|---|---|
| `git checkout -b rizkiG-accounting` lalu `git branch -D rizkiG-accounting` | **Inisiatif agent, bukan permintaan owner.** Branch dibuat atas dugaan keliru bahwa branch frontend owner bernama `rizkiG`. Owner mengoreksi bahwa branch-nya `RizkiV2`, dan branch itu langsung dihapus. Nol commit dibuat di atasnya, nol pekerjaan hilang |
| `git checkout RizkiV2` | Mengikuti koreksi owner |
| `git merge --ff-only QuilvianIntegrationFrontend` | **Diminta dan dikonfirmasi owner** pada sesi ini. `RizkiV2` berada 0 ahead / 78 behind, sehingga fast-forward murni |

Aturan `AGENTS.md` melarang tindakan Git tanpa permintaan eksplisit. Pembuatan dan penghapusan
branch di baris pertama **melanggar aturan itu**; keadaan akhirnya benar dan tidak ada yang
hilang, tetapi tindakannya sendiri tidak seharusnya diambil sendiri.

**Nol `git add`, nol commit, nol push.** Seluruh perubahan berada di working tree.

`git status --short`:

```
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/corporate/
?? src/components/view/corporate/
?? src/lib/constants/corporate/
?? src/lib/hooks/corporate/
?? src/style/corporate/
```

## 9. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| Acceptance (2) dan (3) belum dilihat mata manusia | **Sedang** | Skrip bagian 6 dibuat supaya owner dapat menutupnya dalam beberapa menit |
| Modul belum dapat dipakai walau layarnya terbuka | Sedang | Bukan cacat task ini. `BLK-ACC-02` pada `testing/readiness-report.md`: daftar akun masih kosong dan periode belum dibangkitkan |
| Nol test otomatis untuk layar ini | Rendah | Repository frontend tidak memiliki pola test komponen React; 434 test yang ada seluruhnya menguji helper murni. Menambahkan kerangka test komponen adalah keputusan tersendiri, bukan bagian task ini |
| Daftar delapan layar ditulis manual di konstanta | Rendah | Bila `FE-ACC-002` dan seterusnya selesai, penanda `available` pada `accounting-constants.jsx` wajib ikut diperbarui, dan tautannya ditambahkan |

## 10. Langkah berikutnya

**`FE-ACC-002` — Daftar dan form daftar akun.** Dependency-nya sudah lunas: `FE-ACC-001` berdiri,
dan `BE-ACC-007` menyediakan delapan endpoint daftar akun. Roadmap mewajibkan memakai
`master-data-resource-slice-factory.jsx` untuk slice-nya — memakai factory itu keharusan, bukan
pilihan.

Sebelum itu, satu langkah owner yang lebih murah: jalankan skrip bagian 6 supaya `FE-ACC-001`
dapat naik dari `IMPLEMENTED` menjadi `DONE`.

---

## 11. Revisi 4 September 2026

Dikerjakan pada baseline yang sama, `RizkiV2` @ `1a86d9322`, dengan 17/17 hash artefak canonical
cocok dan kedua source SHA verification tepat pada yang tercatat (`1a86d9322` / `822d48a`).

Dua perubahan bertahan, satu dibatalkan owner. Ketiganya dicatat apa adanya.

### 11.1 Cacat — pemilih badan hukum tidak dapat dibuka `DIPERBAIKI`

Owner melaporkan kotak **Badan Hukum** tidak bisa diklik. Terbukti cacat kode, bukan salah pakai.

`accounting-home-view.jsx` mengirim `disabled={!hydrated || loading}`, dengan `loading` adalah
status pemuatan daftar opsi milik kotak itu sendiri. Digabung dengan `refetchOnOpen`, jadinya
lingkaran yang menutup dirinya sendiri:

| # | Peristiwa | Berkas |
|---|---|---|
| 1 | Klik → `setIsOpen(true)` lalu memanggil `onOpen()` | `filter-select.jsx:336` |
| 2 | `onOpen` adalah `select.refetch` → pemuatan ulang → `loading` `true` | `resource-filter-select.jsx:52`, `use-select-resource.jsx:900` |
| 3 | `loading` `true` → induk menghitung ulang → `disabled` `true` | `accounting-home-view.jsx` |
| 4 | `effectiveOpen = isOpen && !disabled` → `false`, menu tidak digambar | `filter-select.jsx:261` |
| 5 | Effect menutup paksa saat `disabled && isOpen` | `filter-select.jsx:297-302` |

Setiap klik **membuka lalu langsung menutup** dropdown dalam satu frame. Deterministik.

**Menyimpang dari konvensi repository.** Seluruh 14 pemakaian `ResourceFilterSelect` lain diaudit:
nol yang mengikat `disabled` ke `loading` milik select-nya sendiri. Yang mengirim `disabled`
mengikatnya ke keadaan bisnis — `saving`, `locked`, `isSaved`, atau `handoverLoading`
(flag simpan, bukan flag muat opsi; `use-inpatient-episode-detail.jsx:88`).

Perbaikan: `disabled={!hydrated}` pada **tiga** layar yang memakai pola ini — beranda, COA, dan
periode akuntansi. Penjaga `hydrated` dipertahankan; hanya jeratnya yang dicabut. Alasannya
ditulis sebagai komentar di `accounting-legal-entity-select.jsx` supaya tidak terulang.

Cacat ini kemungkinan besar juga menahan verifikasi manual `FE-ACC-002` dan `FE-ACC-004`, karena
keduanya memakai pola `disabled` yang sama.

### 11.2 Penggantian nama layar menjadi **COA** `DITERAPKAN`

Keputusan owner: nama *Daftar Akun* diganti **COA**, karena kata "akun" akan tertukar dengan akun
pengguna. Nol perubahan rute, folder, nama berkas, atau identifier kode — **label tampilan saja**.

| Berkas | Yang diubah |
|---|---|
| `docs/module-blueprints/accounting/03-frontend-architecture.md` | Nama layar 1 pada bagian 4 + catatan keputusan; hash manifest digeser |
| `docs/.../task/report/frontend/fe-acc-002-...md` | Jalur navigasi pada skrip uji manual |
| `src/utils/menu-sidebar/menu-items.jsx` | Label menu sidebar |
| `src/app/corporate/accounting/chart-of-accounts/page.jsx` | `metadata.title` |
| `src/lib/constants/corporate/accounting/chart-of-account/chart-of-account-constants.jsx` | `heroTitle`, `listTitle`, `pluralLabel` |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Label kartu beranda + `heroDescription` |

Istilah **daftar akun** dalam prosa kontrak, ERD, PRD, dan laporan kesiapan — 50 kemunculan —
**sengaja tidak diubah**. Itu nama konsep akuntansinya, bukan nama layar; mengganti semuanya
akan menulis ulang kontrak yang sudah disetujui tanpa alasan. Yang diganti hanya label yang
benar-benar dilihat pengguna.

`label: "Akun"` (tunggal) **tidak** diubah — satu baris tetap sebuah akun, bukan sebuah COA. Kata
"Chart of Account" dieja penuh pada deskripsi hero layar COA dan pada kartu beranda, supaya
singkatannya tetap dapat dipahami staf baru.

### 11.3 Penataan ulang beranda `DIBATALKAN OWNER`

Owner sempat meminta tata letak beranda mengikuti rancangan baru — remah roti, bilah konteks satu
baris, tiga kelompok bernomor bernada warna, kartu berbadge dengan tombol panah, dan kartu
*Butuh Bantuan?*. Versi itu dibangun penuh dan lolos lint, 434 test, serta build.

**Owner kemudian membatalkannya pada sesi yang sama**, dan meminta tampilan beranda dikembalikan
ke bentuk semula. Pembatalan sudah dijalankan: `accounting-home-view.jsx`,
`accounting-home-view.module.css`, `accounting-legal-entity-select.jsx`, dan
`accounting-constants.jsx` kembali ke struktur awal. Diverifikasi dengan pencarian jejak —
`areaGroup`, `toneInfo`, `breadcrumb`, `helpCard`, `contextBar`, `ACCOUNTING_AREA_GROUPS`,
`ACCOUNTING_HELP_CARD`, `legalEntityControlInline`: **nol rujukan tersisa**.

Yang **tidak** ikut dikembalikan, karena bukan bagian dari tampilan yang dibatalkan: perbaikan
11.1 dan penggantian nama 11.2.

### 11.4 Berkas yang berubah pada revisi ini

| Berkas | Sifat |
|---|---|
| `src/components/view/corporate/accounting/accounting-home-view.jsx` | Struktur awal + perbaikan 11.1 |
| `src/components/view/corporate/accounting/shared/accounting-legal-entity-select.jsx` | Struktur awal + komentar alasan 11.1 |
| `src/components/view/corporate/accounting/chart-of-account/chart-of-account-view.jsx` | Satu baris — 11.1 |
| `src/components/view/corporate/accounting/accounting-period/accounting-period-view.jsx` | Satu baris — 11.1 |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Struktur awal + label COA |
| `src/lib/constants/corporate/accounting/chart-of-account/chart-of-account-constants.jsx` | Label COA |
| `src/app/corporate/accounting/chart-of-accounts/page.jsx` | `metadata.title` COA |
| `src/utils/menu-sidebar/menu-items.jsx` | Label menu COA |

`store.jsx` **tidak disentuh** pada revisi ini; perubahan 9 baris di dalamnya berasal dari
`FE-ACC-002`..`004` dan sudah ada sebelum sesi ini. Nol berkas `components/features/**` diubah,
`globals.css` nol berkas berubah, nol `style={{ }}`.

### 11.5 Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `npx eslint --quiet` atas berkas accounting yang disentuh + `menu-items.jsx` | **PASS**, exit 0 |
| Unit test 434 | **PASS**, `# pass 434`, `# fail 0` |
| `npm run build` + `postbuild` | **PASS**, `Compiled successfully` |
| `<title>` layar COA hasil render | `COA` |
| Pencarian jejak rework | nol rujukan tersisa |

### 11.6 Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Sesi ini tidak memiliki kredensial, dan mengambil kredensial dari `.env` dilarang aturan
keselamatan lingkungan. Endpoint daftar badan hukum diperiksa secara struktural: ia **ada** dan
membalas `401` tanpa sesi — artinya rutenya benar dan menuntut login.

Perbaikan 11.1 **belum dilihat mata manusia.** Skrip uji bagian 6 tetap berlaku; langkah 3 dan 4
adalah pembuktian langsung atas perbaikan ini.

### 11.7 Catatan governance

`AGENTS.md` mewajibkan sejumlah berkas aturan sebelum menyentuh warna, komposisi halaman, dan
komponen. Pada plugin yang terpasang, **hanya `rules/frontend/frontend-architecture.md` yang ada**;
`rules/GLOBAL_RULES.md`, `rules/frontend/design-tokens.md`, `base-component-catalog.md`,
`base-component-decision-gate.md`, `page-composition-patterns.md`, `ui-consistency-checklist.md`,
`test-policy.md`, dan `rules/frontend/REPORT_TEMPLATE.md` **tidak ada**. Pekerjaan dijalankan
memakai berkas yang tersedia; kepatuhan pada berkas yang hilang tidak dapat diklaim. Perlu
perhatian pemilik plugin.

### 11.8 Risiko dan keputusan terbuka

| Hal | Berat | Keterangan |
|---|---|---|
| `roadmap/frontend-roadmap.md` belum diperbarui | Sedang | Berkas itu **ter-hash** di manifest (`df5f0b44...`). Menyuntingnya memutus hash, jadi ditahan sampai owner memutuskan |
| Nama **COA** sudah tercermin di artefak blueprint | — | **SELESAI 4 September 2026.** Nama layar pada `03-frontend-architecture.md` bagian 4 diganti `COA`, disertai catatan keputusan; hash manifest digeser `381b0acf` → `a8e3f7c0`. Jalur navigasi skrip uji `fe-acc-002` ikut disesuaikan. `04-prd-to-mvp.md` **tidak** menyebut *Daftar Akun* sebagai nama layar — pernyataan sebelumnya keliru dan dikoreksi di sini |
| `FE-ACC-001`..`004` masih `IMPLEMENTED` | Sedang | Hanya owner yang dapat menaikkannya lewat skrip bagian 6 |
