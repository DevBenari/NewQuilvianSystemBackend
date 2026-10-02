# `FE-ACC-007` — Rincian jurnal dan tombol aksi

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-007` |
| Blueprint | `ACC-BP-001` revisi 9, `roadmap/frontend-roadmap.md` gelombang `MVP-1` |
| Task type | Frontend, layar rincian baca-saja + tindakan alur kerja |
| Task mode | `FRONTEND` |
| Kontrak | **`ACC-API-0.5`**, `ACC-STATE-0.1` bagian 1.1, `ACC-PERMISSION-0.3` bagian 5 |
| Wewenang UI | `ACC-FE-003` **`closed`** — halaman tersendiri, `base-detail-view.jsx` |
| Repository target tulis | `QuilvianSystemFrontendDev` |
| Branch | `RizkiV2` @ `516448480` |
| Baseline backend dibaca | `rizkiG` @ `6d4fc26` (read-only) |
| Status | **`IMPLEMENTED` — menunggu verifikasi manual owner di peramban** |
| Tanggal | 7 September 2026 |

## Ringkasan untuk pembaca umum

Layar ini menjawab satu pertanyaan: *"jurnal ini isinya apa, siapa saja yang sudah
menyentuhnya, dan apa yang boleh saya lakukan terhadapnya sekarang?"*

Dua pertanyaan pertama dijawab tabel — baris jurnalnya dan riwayat persetujuannya. Pertanyaan
ketiga **tidak dijawab oleh layar sama sekali**. Layar hanya menampilkan daftar tindakan yang
sudah dihitung backend dan dikirim sebagai `AvailableActions`. Kalau backend tidak mengirim
`approve`, tombol Setujui tidak ada — layar tidak tahu, dan memang tidak perlu tahu, kenapa.

Itu bukan kerapian gaya. Aturan siapa boleh menyetujui apa menggabungkan status jurnal, hak
akses, dan aturan pembuat-bukan-penyetuju (`ACC-DEC-016`). Menyalin ketiganya ke frontend berarti
menaruh aturan bisnis di tempat yang akan diam-diam menyimpang begitu backend berubah.

## 1. Gerbang pemakaian ulang komponen

Nol komponen base baru. Nol perubahan `globals.css`. Nol perubahan backend.

| Kebutuhan | Dipakai ulang | Putusan |
|---|---|---|
| Kerangka halaman rincian | `base-detail-view.jsx` | **REUSE**, pola dari `chart-of-account-detail-view.jsx` |
| Kartu kepala jurnal | `BaseDetailCard` lewat prop `detailRows` | **REUSE** |
| Dialog konfirmasi + isian alasan | `ConfirmModal` **bawaan `BaseDetailView`** lewat slot `deleteConfirm` | **REUSE**, nol modal tambahan |
| Notifikasi | `ToastStack` **bawaan `BaseDetailView`** lewat prop `toasts` | **REUSE** |
| Penanda status | `status-badge.jsx` + `JOURNAL_STATUS_TONE` dari `FE-ACC-005` | **REUSE** |
| Tombol | `base-button.jsx` | **REUSE** |
| Kelima thunk aksi | `accounting-journal-slice.jsx` dari `FE-ACC-005` | **REUSE**, nol thunk baru |
| Token rute privat | `registerPrivateRouteToken` / `resolvePrivateRouteToken` + pola `[slug]` milik COA | **REUSE** |
| Pembantu format | `chart-of-account-utils.jsx`, `formatCurrencyIDR` | **REUSE** |
| Label enum | `JOURNAL_STATUS_LABELS`, `JOURNAL_APPROVAL_ACTION_LABELS`, `JOURNAL_CORRECTION_TYPE_LABELS` | **REUSE** |

## 2. Keputusan teknis yang perlu diketahui pembaca berikutnya

### 2.1 `actionLoading` sebelumnya tidak pernah menyala — ini diperbaiki

**Ini temuan terpenting task ini.**

Kelima thunk aksi dibuat `createAsyncThunk` **di luar** `createMasterDataResourceSlice`.
`extraReducers` milik factory memakai `addCase` untuk thunk miliknya sendiri saja, sehingga
`accountingJournal/submit`, `/approve`, `/reject`, `/post`, dan `/reverse` tidak dikenalinya.
Akibatnya `selectJournalActionLoading` **tidak pernah bernilai `true`** untuk kelima aksi itu.

Acceptance (4) menuntut tombol benar-benar mati selama aksi berjalan. Bersandar pada selector
yang tidak pernah menyala berarti menulis penjaga yang tidak menjaga apa pun — lolos di kertas,
gagal di pemakaian, dan aksi tetap dapat terkirim dua kali.

**Perbaikannya di reducer slice Accounting sendiri**, bukan di factory:

```js
const journalReducer = (state, action) =>
  applyWorkflowAction(journalResource.reducer(state, action), action);
```

Factory **tidak disentuh**. Ia dipakai enam slice lain, dan menambahkan kait perluasan padanya
adalah perubahan milik pemilik abstraksi itu — bukan milik Accounting. Ini konsisten dengan
keputusan `FE-ACC-002` yang menolak menyesuaikan factory demi satu modul.

**Cacat yang sama masih ada di luar cakupan task ini** dan sengaja tidak disentuh: lihat
bagian 6.

### 2.2 Tombol dibangun oleh fungsi murni yang dapat diuji

`buildJournalActionButtons(availableActions)` di `journal-constants.jsx` hanya **menyaring dan
mengurutkan** apa yang backend kirim. Tidak ada satu pun cabang yang memeriksa status jurnal atau
membandingkan pengguna dengan pembuatnya. Karena murni, ia dapat diuji tanpa merender React —
dan itulah yang mengunci acceptance (1) dan (2) sebagai uji otomatis, bukan sebagai klaim.

`update` dan `delete` ikut dikirim backend tetapi sengaja diabaikan: keduanya bukan tindakan alur
kerja, dan layarnya milik `FE-ACC-006`.

### 2.3 Urutan tombol tetap, tidak mengikuti urutan kiriman

Ajukan, Setujui, Tolak, Sahkan, Balik. Backend kebetulan mengirim dalam urutan itu, tetapi
menyandarkan letak tombol pada urutan array respons berarti tombol dapat berpindah tempat tanpa
sebab yang terlihat pengguna. `JOURNAL_ACTION_ORDER` menguncinya.

### 2.4 Tombol Balik mengirim `FullReversal`, dan mengatakannya

`ReverseJournalRequest.CorrectionType` bertanda `[Required]`. Tombol Balik karena itu tidak dapat
dikirim tanpa nilainya, sementara **pemilihan** antara pembalikan penuh dan jurnal penyesuaian
adalah cakupan `FE-ACC-010`, bukan task ini.

Yang dilakukan: tombol mengirim `correctionType: 1` (`FullReversal`), dan kalimat konfirmasinya
menyebutkan itu dengan huruf besar — *"Layar ini membuat PEMBALIKAN PENUH … Pilihan jurnal
penyesuaian belum tersedia di sini."* Pengguna diberi tahu persis apa yang akan terjadi; tidak
ada yang dipilihkan diam-diam.

**Ini delta terhadap cakupan yang perlu diketahui owner.** Alternatifnya adalah menampilkan
tombol yang selalu ditolak `400`, atau membangun dialog `FE-ACC-010` lebih awal. Keduanya lebih
buruk. Lihat bagian 6.

### 2.5 Riwayat memakai `ActionByName`, dan `Guid` tidak pernah menggantikannya

`BE-ACC-015` menambahkan `ActionByName` pada tiap baris riwayat, plus `SubmittedByName`,
`ApprovedByName`, dan `PostedByName` pada kepala jurnal. Keempatnya dipakai. Ketika backend tidak
menemukan namanya, yang tampil **tanda hubung** — bukan `Guid` sebagai pengganti. `ActionBy`
hanya dipakai sebagai bagian kunci baris React.

### 2.6 Muat ulang dari backend, bukan tebakan

Respons tiap aksi sebenarnya sudah memuat rincian terbaru. Layar tetap memanggil
`getJournalById` lagi sesudahnya. Alasannya bukan kehati-hatian buta: `AvailableActions`
bergantung pada hak akses dan aturan pembuat-bukan-penyetuju, dan satu permintaan bersih adalah
cara paling jelas memastikan yang tampil adalah jawaban backend — bukan sisa keadaan lama.

## 3. Berkas yang berubah

### Ditambahkan

| Berkas | Isi |
|---|---|
| `src/app/corporate/accounting/journals/[slug]/page.jsx` | Rute rincian; penjaga token sama persis dengan COA |
| `src/components/view/corporate/accounting/journal/detail/journal-detail-view.jsx` | Layar rincian, tabel baris, tabel riwayat, tombol aksi |
| `src/lib/hooks/corporate/accounting/journal/use-journal-detail.jsx` | Muat rincian, susun baris, jalankan aksi, muat ulang |
| `src/style/corporate/accounting/journal-detail-view.module.css` | Gaya kartu dan tabel, token `--base-*` |
| `tests/unit/accounting-journal-detail.test.mjs` | **10 uji**, mengunci acceptance (1), (2), dan (4) |

### Diubah

| Berkas | Perubahan |
|---|---|
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | `JOURNAL_DETAIL_FIELDS`, `JOURNAL_ACTION_BUTTONS`, `JOURNAL_ACTION_ORDER`, `buildJournalActionButtons` |
| `src/lib/state/slice/corporate/accounting/accounting-journal-slice.jsx` | Reducer dikomposisi agar kelima thunk aksi mengisi `actionLoading`/`actionError`/`lastActionResponse` — lihat 2.1 |
| `src/lib/hooks/corporate/accounting/journal/use-journal.jsx` | `openDetail` dengan token privat |
| `src/components/view/corporate/accounting/journal/journal-view.jsx` | `onRowDoubleClick={openDetail}` — 2 baris |

Dua berkas terakhir adalah pintu masuk rutenya. Tanpa keduanya layar rincian tidak dapat dibuka
dari mana pun, padahal daftar jurnal sudah menjanjikannya sejak `FE-ACC-005`
(*"Klik dua kali sebuah baris untuk membuka rinciannya"*).

**Nol berkas backend berubah. Nol sentuhan `globals.css`. Nol komponen base baru. Nol perubahan
`master-data-resource-slice-factory.jsx`.**

## 4. Acceptance

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Tombol ditampilkan dari `AvailableActions`, bukan dihitung frontend | **TERBUKTI — uji otomatis** | `buildJournalActionButtons` hanya menyaring dan mengurutkan kiriman backend. Uji `tests/unit/accounting-journal-detail.test.mjs` menjalankan fungsinya, dan satu uji terpisah menegaskan hook tidak memuat `createBy`, `currentUser`, maupun `userId` |
| (2) | Setujui tidak muncul pada jurnal buatan sendiri | **TERBUKTI di kedua sisi** | Backend: `TindakanTersedia` — `if (izin.CanApprove && !pembuatnyaSendiri)`. Frontend: uji "Setujui tidak muncul ketika backend tidak mengirim approve" membuktikan layar tidak menambahkannya kembali. **Belum dilihat berjalan di peramban** |
| (3) | Sesudah tiap aksi berhasil, rincian dimuat ulang dari backend | **TERBUKTI di kode** | `refreshFromBackend()` menaikkan `refreshVersion`, yang menjadi dependency effect `getJournalById`. Status tidak pernah ditebak layar |
| (4) | Tombol mati selama `actionLoading` | **TERBUKTI — dan celahnya ditutup** | Kelima thunk kini benar-benar menyalakan `actionLoading` (lihat 2.1). `disabled={busy}` pada tiap tombol, dan `ConfirmModal` mematikan tombol Ya lewat `loading` |
| (5) | Pesan `422` dan `403` ditampilkan apa adanya | **TERBUKTI di kode** | `readErrorMessage` membaca `message`/`Message` dari `ApiResponse.Fail`, diteruskan ke toast **tanpa pemetaan kode ke kalimat karangan sendiri**, dan disalin juga ke `actionError` sehingga tidak hilang bersama toast |

Kelima acceptance dapat dipenuhi. Tidak ada yang perlu diakali di frontend.

## 5. Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `npm run lint:errors` | **PASS**, exit 0 |
| Unit test seluruh suite | **PASS** — `# tests 444`, `# pass 444`, `# fail 0` (434 sebelumnya + 10 baru) |
| `npm run build` + `postbuild` | **PASS**, `Compiled successfully in 30.4s`, 0 error 0 warning |
| Rute terbentuk | `ƒ /corporate/accounting/journals/[slug]` muncul di keluaran build |
| ESLint atas berkas yang berubah, termasuk warning | **0 error, 0 warning** dari berkas task ini |
| Nol Axios instance baru, nol `InstanceAxios` di view, nol `style={{ }}` | **0 / 0 / 0** |

### Catatan perintah — `npm run test:unit` tidak dapat dijalankan apa adanya

`test:unit` memakai glob `"tests/unit/**/*.test.mjs"`, dan **glob pada `node --test` baru ada di
Node 21**. Mesin ini memakai **Node v20.20.2**, sehingga perintahnya gagal dengan
`Could not find …\tests\unit\**\*.test.mjs` — baik lewat Bash maupun PowerShell.

Yang dijalankan sebagai gantinya menguji berkas yang sama persis:

```
node --import ./tests/helpers/register.mjs --test tests/unit/
```

Ini **cacat environment/tooling yang sudah ada**, bukan akibat task ini, dan bukan kegagalan
test. Dicatat supaya laporan berikutnya tidak melaporkan `test:unit` PASS tanpa benar-benar
menjalankannya. `package.json` **tidak diubah** — memperbaiki skrip itu bukan cakupan task ini.

## 6. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Sesi ini tidak dapat memverifikasi di peramban. Alasannya konkret, bukan kelalaian:

- **Backend tidak berjalan** — `http://localhost:5107` tidak merespons (`curl` exit 7).
- **Dev server tidak berjalan** — `http://localhost:3000` tidak merespons.
- **Tidak ada kredensial** — seluruh endpoint jurnal berada di balik `[AccessPermission]`, dan
  `AvailableActions` justru bergantung pada identitas pengguna yang masuk.
- Tidak ada pemandu peramban di sesi ini.

Akibatnya **bentuk respons `BE-ACC-015` tetap belum terbukti lewat HTTP.** `MODULE-STATUS.md`
menandainya sebagai hal yang akan terbukti di task ini; ia belum terbukti. Yang sudah diverifikasi
adalah bahwa frontend membaca field yang benar-benar ada di source backend `6d4fc26`
(`JournalDtos.cs`, `AccJournalService.PetakanRincianAsync`) — pembacaan source, bukan pengamatan
runtime.

### Skrip uji untuk owner

Prasyarat: jurnal **`JB/2026/09/00001`** yang sudah berstatus *Menunggu Persetujuan*.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | **Akuntansi → Jurnal**, klik dua kali baris `JB/2026/09/00001` | Halaman rincian terbuka. Bilah alamat memuat token acak, **bukan** `Guid` atau nomor jurnal |
| 2 | Baca kartu kepala | Nomor, status *Menunggu Persetujuan*, periode `2026-09`, total debit dan kredit `Rp 1.000.000` |
| 3 | Baca tabel **Baris Jurnal** | Dua baris, kode dan nama akun, debit/kredit rata kanan |
| 4 | Baca tabel **Riwayat Persetujuan** | Satu baris *Diajukan*. **Kolom Oleh berisi NAMA, bukan `Guid`** — ini bukti `BE-ACC-015` yang pertama |
| 5 | **Acceptance (2).** Perhatikan deretan tombol | Karena jurnal ini Anda sendiri yang buat, **Setujui tidak boleh ada**. Yang ada: Tolak |
| 6 | Masuk sebagai pengguna lain yang berhak menyetujui, buka jurnal yang sama | **Setujui muncul.** Perbedaan antara langkah 5 dan 6 adalah acceptance (2) |
| 7 | Tekan **Setujui**, lalu **Ya** | Status berubah menjadi *Disetujui*, riwayat bertambah satu baris berisi nama Anda |
| 8 | **Acceptance (3).** Perhatikan deretan tombol sesudah langkah 7 | Berubah sendiri: Setujui hilang, **Sahkan** muncul — dimuat ulang dari backend |
| 9 | **Acceptance (4).** Tekan **Sahkan** lalu **Ya**, dan segera coba tekan tombol lain | Seluruh tombol mati selama permintaan berjalan; tidak ada yang dapat ditekan dua kali |
| 10 | Sesudah langkah 9 | Status *Disahkan*; yang tersisa hanya **Balik**. Ini menuntaskan **`UAT-01`** |
| 11 | **Acceptance (5).** Tekan **Tolak** pada jurnal yang sudah disahkan lewat Swagger | Pesan `409` backend tampil apa adanya |
| 12 | Tekan **Tolak** pada jurnal yang menunggu, kosongkan alasan | Tombol **Ya** mati sampai alasan diisi |
| 13 | **Acceptance (5) yang sebenarnya.** Ajukan jurnal pada periode yang sudah ditutup | Pesan `422` dari backend tampil **apa adanya**, bukan "Terjadi kesalahan" |
| 14 | Tekan **Balik** pada jurnal yang sudah disahkan | Kalimat konfirmasi menyebut **PEMBALIKAN PENUH**. Sesudah berhasil, jurnal asal **tetap** *Disahkan* |

## 7. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| **Bentuk respons `BE-ACC-015` belum terbukti lewat HTTP** | **Sedang** | Langkah 4 pada skrip di atas adalah pembuktiannya. Bila `ActionByName` ternyata `null` untuk semua baris, sebabnya di `AmbilNamaAktorAsync`, bukan di layar ini |
| **Balik hanya `FullReversal`** | **Sedang** | Disengaja dan dikatakan pada dialognya. Pilihan jurnal penyesuaian adalah cakupan `FE-ACC-010`. **Butuh persetujuan owner** bahwa menampilkan Balik lebih baik daripada menyembunyikannya sampai `FE-ACC-010` selesai |
| **`actionLoading` masih tidak menyala pada `deactivateChartOfAccount`** | **Rendah** | Cacat yang sama persis dengan 2.1, di `accounting-chart-of-account-slice.jsx`. **Sengaja tidak disentuh** — di luar cakupan `FE-ACC-007`. Penjaga `if (!confirmState \|\| actionLoading) return;` pada `use-chart-of-account.jsx:249` karena itu tidak menjaga penonaktifan dari kiriman ganda. Diusulkan menjadi butir `UTANG-TEKNIS.md` |
| Slot dialog bernama `deleteConfirm` | Rendah | Yang dikonfirmasi bukan penghapusan. Mengganti namanya di `base-detail-view.jsx` adalah perubahan milik pemilik komponen base itu, bukan milik Accounting |
| Nol test render untuk layar ini | Rendah | Repository belum punya pola test komponen React. Yang dapat diuji murni **sudah** diuji — 10 uji baru; sisanya menuntut kerangka render yang merupakan keputusan tersendiri |
| `npm run test:unit` gagal pada Node 20 | Rendah | Cacat tooling yang sudah ada. Lihat bagian 5 |

## 8. Langkah berikutnya

1. **Owner menjalankan skrip bagian 6** — khususnya langkah 4, 5, dan 6. Itu sekaligus
   menuntaskan **`UAT-01`** dan menutup `BLK-ACC-02` sepenuhnya.
2. Putuskan risiko **Balik hanya `FullReversal`** (bagian 7).
3. `FE-ACC-008` buku besar dan `FE-ACC-009` neraca saldo — `BE-ACC-012` sudah `DONE`.

## 9. Selisih terhadap roadmap

| Hal | Roadmap | Yang dikerjakan | Alasan |
|---|---|---|---|
| Reuse | `base-detail-view.jsx` **atau** `base-detail-side-panel.jsx` | `base-detail-view.jsx` | `ACC-FE-003` `closed` — halaman tersendiri |
| Kontrak | `ACC-API-0.1` | **`ACC-API-0.5`** | 0.1 sudah usang empat revisi; 0.5 adalah yang cocok dengan source `6d4fc26` |
| Cakupan | Lima tombol + isian alasan | Sama, ditambah `openDetail` pada daftar | Tanpa itu rutenya tidak dapat dibuka dari mana pun |
| Slice | *"Kelima thunk aksi sudah ada, pakai itu"* | Dipakai, **tanpa thunk baru** — tetapi reducernya dikomposisi | Acceptance (4) tidak dapat dipenuhi tanpa itu. Lihat 2.1 |

---

**Git status frontend saat laporan ditulis** — belum di-stage, belum di-commit, sesuai instruksi:

```
 M src/components/view/corporate/accounting/journal/journal-view.jsx
 M src/lib/constants/corporate/accounting/journal/journal-constants.jsx
 M src/lib/hooks/corporate/accounting/journal/use-journal.jsx
 M src/lib/state/slice/corporate/accounting/accounting-journal-slice.jsx
?? src/app/corporate/accounting/journals/[slug]/page.jsx
?? src/components/view/corporate/accounting/journal/detail/
?? src/lib/hooks/corporate/accounting/journal/use-journal-detail.jsx
?? src/style/corporate/accounting/journal-detail-view.module.css
?? tests/unit/accounting-journal-detail.test.mjs
```
