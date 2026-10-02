# `FE-ACC-009` — Neraca saldo

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-009` |
| Blueprint | `ACC-BP-001` revisi 9, `roadmap/frontend-roadmap.md` gelombang `MVP-2` |
| Task type | Frontend, layar laporan baca-saja |
| Task mode | `FRONTEND` |
| Kontrak | **`ACC-API-0.5`** grup General Ledger, endpoint `/trial-balance` |
| Wewenang UI | `DEV_DISCRETION` |
| Repository target tulis | `QuilvianSystemFrontendDev` |
| Branch | `RizkiV2` @ `1b7a0eff5` |
| Baseline backend dibaca | `rizkiG` @ `f989453` (read-only) |
| Status | **`IMPLEMENTED`** — acceptance (1) dan (3) **terbukti di peramban** 7 Sep 2026. **Dua cacat ditemukan owner dan diperbaiki**: paginasi hantu (7b) dan keterangan tertutup footer (7c). Acceptance (2) menunggu uji ganti badan hukum |
| Tanggal | 7 September 2026 |

## Ringkasan untuk pembaca umum

Neraca saldo menjawab satu pertanyaan: *"pada periode ini, tiap akun berdiri di angka berapa,
dan apakah keseluruhannya masih seimbang?"*

Satu keputusan membentuk seluruh layar ini: **penanda seimbang bukan milik layar.** `IsBalanced`
datang dari backend dan tidak pernah dihitung ulang di sini. Menjumlahkan sendiri lalu
membandingkan kedua total terlihat setara — tetapi yang berhak menyatakan sebuah neraca seimbang
adalah pihak yang menjumlahkannya. Kalau frontend menghitung sendiri lalu hasilnya berbeda dari
backend, yang muncul bukan sekadar tampilan keliru, melainkan dua sumber kebenaran yang
bertengkar tentang angka keuangan.

Dan penandanya sengaja tidak netral. Pada data sehat, neraca saldo **selalu** seimbang: tiap
jurnal yang disahkan wajib seimbang, dan jumlah dari himpunan yang seluruhnya seimbang pasti
seimbang. Karena itu `IsBalanced = false` berarti **kerusakan data**, bukan laporan yang belum
rapi — dan layar menampilkannya sebagai peringatan merah, bukan label abu-abu.

## 1. Validasi masukan sebelum implementasi

| Yang diperiksa | Hasil |
|---|---|
| Revision blueprint | `9`, `approved` |
| Hash artefak canonical | **17/17 cocok** |
| Kontrak | `ACC-API-0.5` — cocok dengan source `f989453` |
| Dependency `FE-ACC-008` | `IMPLEMENTED`, sudah di-commit, terverifikasi di peramban |
| Dependency `BE-ACC-012` | `DONE`; `/trial-balance` berdiri di `GeneralLedgerController` |

**Kedua baseline bergeser dari yang tercatat, sehingga impact scan dijalankan lebih dahulu.**

| Baseline | Tercatat | Sekarang | Temuan |
|---|---|---|---|
| Backend | `822d48a` | `f989453` | **Nol berkas `GeneralLedger/` berubah.** Hanya 2 berkas `.cs`, keduanya `JournalManagement` — `BE-ACC-015` dan penjaga pembalikan `ACC-TD-020` |
| Frontend | `418aebb05` | `1b7a0eff5` | Keenam anchor `FE-ACC-009` — `DataTable`, `SummaryGrid`, `access-denied-gate`, `accounting-period-slice`, `AccountingLegalEntitySelect`, `Formatters` — **nol berubah** |

## 2. Gerbang pemakaian ulang komponen

Nol komponen base baru. Nol slice baru. Nol perubahan backend, `globals.css`, maupun factory.

| Kebutuhan | Dipakai ulang | Putusan |
|---|---|---|
| Slice | `accounting-general-ledger-slice.jsx` dari `FE-ACC-008` — **ditambah satu thunk** | **REUSE**, nol slice baru |
| Pilihan periode | `getAccountingPeriodList` dari slice `FE-ACC-004` | **REUSE**, nol jalur pengambilan data baru |
| Pemilih badan hukum | `AccountingLegalEntitySelect` dari `FE-ACC-001` | **REUSE** |
| Ringkasan total | `SummaryGrid` | **REUSE** |
| Penanda seimbang | `status-badge.jsx` | **REUSE**, memakai nada yang sudah ada |
| Tabel | `DataTable` **tanpa** paginasi | **REUSE** |
| Bilah penyaring | `DataFilter` + `filter-select.jsx` | **REUSE** |
| Pembantu format | `chart-of-account-utils.jsx`, `formatCurrencyIDR` | **REUSE** |
| Pola layar | `general-ledger-view.jsx` dari `FE-ACC-008` | **REUSE** |

## 3. Keputusan teknis yang perlu diketahui pembaca berikutnya

### 3.1 Berbagi slice, memisah konstanta

Thunk `getTrialBalance` masuk ke slice `FE-ACC-008` — keduanya endpoint milik satu grup backend,
dan memecahnya menjadi dua slice akan menciptakan arsitektur tandingan untuk satu controller.

Konstantanya justru **dipisah** menjadi `trial-balance-constants.jsx`. Kolom, sebab-kosong, dan
kalimat penjelasnya berbeda seluruhnya dari buku besar; menumpuknya dalam satu berkas hanya
membuat pembaca berikutnya menebak mana milik layar yang mana.

### 3.2 Nol paginasi — dan ini beda dari `FE-ACC-008`

`/movements` mengembalikan `PagedResult`. `/trial-balance` mengembalikan `Rows` **utuh**.
Menyalin mentah pola tabel `FE-ACC-008` akan memasang paginasi yang menampilkan halaman 1 dari 1
selamanya dan menyesatkan pembaca. Uji `tabel neraca saldo mematikan paginasi secara
eksplisit` menahannya — dan versi pertamanya ternyata terlalu lemah; lihat bagian 7b.

### 3.3 Berpindah badan hukum melepas periode — disetel saat render

Periode adalah milik satu badan hukum: `2026-09` pada badan hukum A dan pada badan hukum B adalah
**baris yang berbeda**. Membiarkan kode periode lama tetap terpilih saat badan hukumnya berganti
membuat backend mencari periode itu pada badan hukum baru, dan menjawab `404` bila tidak ada.

Penyetelannya dilakukan **saat render**, bukan di dalam `useEffect`:

```js
if (legalEntityId !== legalEntityTerakhir) {
  setLegalEntityTerakhir(legalEntityId);
  setPeriodCode("");
}
```

Ini pola yang dianjurkan React untuk menyesuaikan state ketika masukan berubah. Dua alasannya:
ia berjalan sebelum anak-anaknya dirender sehingga **tidak ada satu frame pun** yang sempat
menampilkan periode milik badan hukum lama, dan ia menghindari peringatan
`react-hooks/set-state-in-effect` yang sudah ditemui pada `FE-ACC-007`.

Pembersihan neraca lama tetap sebuah effect, karena state Redux tidak dapat dibuang saat render —
tetapi effect itu **hanya mengirim action**, tidak menyetel state lokal.

### 3.4 Dua keterangan batas, di dua tempat berbeda

Keduanya sengaja tidak digabung:

- **"hanya jurnal yang sudah disahkan"** diletakkan **di atas**, bersama pemilih badan hukum. Ia
  mengubah cara seluruh angka di bawahnya dibaca, jadi ia harus terbaca sebelum angkanya.
- **"hanya akun yang punya saldo pembuka atau mutasi"** diletakkan **di bawah tabel**, dan hanya
  muncul ketika tabelnya berisi. Ia menjawab pertanyaan yang baru timbul sesudah pembaca melihat
  isinya: *"kenapa akun saya tidak ada di sini?"*

### 3.5 Empat sebab kosong, bukan satu

`noLegalEntity`, `noPeriod`, `noPeriodAvailable`, dan `empty`. Yang ketiga penting: badan hukum
yang periodenya belum pernah dibangkitkan menghasilkan dropdown kosong, dan tanpa penjelasan
petugas akan mengira layarnya rusak. Kalimatnya menunjuk ke layar Periode Akuntansi.

## 4. Berkas yang berubah

### Ditambahkan

| Berkas | Isi |
|---|---|
| `src/lib/constants/corporate/accounting/general-ledger/trial-balance-constants.jsx` | Config, 6 kolom, 4 sebab kosong, 3 keadaan penanda seimbang |
| `src/lib/hooks/corporate/accounting/general-ledger/use-trial-balance.jsx` | Pilihan periode, penahan permintaan, reset saat badan hukum berganti |
| `src/components/view/corporate/accounting/general-ledger/trial-balance-view.jsx` | Layar laporan |
| `src/style/corporate/accounting/trial-balance-view.module.css` | Gaya, token `--base-*` |
| `src/app/corporate/accounting/trial-balance/page.jsx` | Rute tipis |
| `src/app/corporate/accounting/trial-balance/trial-balance-client.jsx` | Pembungkus client |
| `tests/unit/accounting-trial-balance.test.mjs` | **11 uji**, mengunci ketiga acceptance |

### Diubah

| Berkas | Perubahan |
|---|---|
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | Thunk `getTrialBalance`, state `trialBalance`, action `clearTrialBalance`, 4 selector |
| `src/utils/menu-sidebar/menu-items.jsx` | Menu **Neraca Saldo** — 6 baris, ikon `RiFileList3Line` yang sudah di-import |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Kartu *Neraca Saldo* ditandai tersedia beserta tautannya |

**Nol berkas backend berubah. Nol `globals.css`. Nol komponen base baru. Nol slice baru. Nol
perubahan `master-data-resource-slice-factory.jsx`. Nol pendaftaran resource di
`hr-select-resources.js`.**

## 5. Acceptance

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Total debit dan total kredit tampil beserta penanda seimbang | **TERBUKTI — uji otomatis** | `SummaryGrid` memuat ketiganya; `StatusBadge` memakai `IsBalanced` dari backend. Uji menolak perbandingan kedua total di hook maupun view |
| (2) | Berpindah badan hukum mengubah angka dan tidak mencampurnya | **TERBUKTI — uji otomatis** | Kode periode dilepas saat render begitu badan hukum berubah, dan `clearTrialBalance` membuang angka lama. Uji memeriksa ketiga mekanismenya |
| (3) | Layar menyebutkan laporan hanya memuat jurnal yang sudah disahkan | **TERBUKTI — uji otomatis** | `postedOnlyNotice` dirender di atas penyaring, `scopeNotice` di bawah tabel |

Ketiga acceptance dapat dipenuhi. Tidak ada yang diakali di frontend.

## 6. Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `npm run lint:errors` | **PASS**, exit 0 |
| Unit test seluruh suite | **PASS** — `# tests 463`, `# pass 463`, `# fail 0` (452 sebelumnya + 11 baru) |
| `npm run build` + `postbuild` | **PASS**, `Compiled successfully in 33.8s` |
| Rute terbentuk | `○ /corporate/accounting/trial-balance` di keluaran build |
| ESLint atas berkas yang berubah, termasuk warning | **0 error, 0 warning** |
| Nol Axios instance baru, nol `InstanceAxios` di view, nol `style={{ }}` | **0 / 0 / 0** |

`npm run test:unit` tetap gagal apa adanya pada Node 20 — glob `--test` baru ada di Node 21.
Yang dijalankan menguji berkas yang sama: `node --import ./tests/helpers/register.mjs --test
tests/unit/`. Cacat tooling yang sudah ada, tercatat pada laporan `FE-ACC-007` bagian 5.

## 7. Verifikasi lewat HTTP — **sebagian TERBUKTI**

Berbeda dari `FE-ACC-007` dan `FE-ACC-008`, **backend dan dev server sedang berjalan** saat task
ini dikerjakan, sehingga sebagian verifikasi dapat benar-benar dijalankan.

| Yang diuji | Hasil | Arti |
|---|---|---|
| `GET .../general-ledger/trial-balance?legalEntityId=…&periodCode=2026-09` | **`401`** | Endpoint **ada** dan menuntut autentikasi |
| `GET .../general-ledger/movements?legalEntityId=…` | `401` | Sama |
| `GET .../general-ledger/tidak-ada` — **kontrol pembanding** | **`404`** | Membuktikan `401` di atas memang berarti "ada", bukan "tidak diperiksa" |
| `GET localhost:3000/corporate/accounting/trial-balance` | **`200`** | Rute frontend terbentuk dan halamannya dirender |

Kontrol pembanding itu yang membuat kedua `401` bermakna: kalau jalur endpoint di slice salah
ketik, hasilnya akan `404` seperti jalur ngawur tersebut. **Jalur `/trial-balance` di
`accounting-general-ledger-slice.jsx` karena itu terbukti benar.**

### Yang belum terbukti — `MANUAL TEST: NOT FEASIBLE` untuk isi layar

Sesi ini tidak memiliki kredensial, dan seluruh endpoint berada di balik
`[AccessPermission("GeneralLedger","Read")]`. Bentuk respons `TrialBalanceResponse`, angka pada
tabel, dan penanda seimbang karena itu **belum dilihat berjalan**.

### Skrip uji untuk owner

Prasyarat sudah terpenuhi: `JB/2026/09/00001` sudah **Disahkan**, dan buku besar sudah terbukti
menampilkan mutasinya.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | **Akuntansi → Neraca Saldo** | Layar terbuka; tabel kosong dengan pesan *"Periode belum dipilih."* |
| 2 | Perhatikan kotak keterangan di atas penyaring | Tertulis laporan hanya memuat jurnal yang sudah disahkan |
| 3 | Buka dropdown periode | Berisi daftar periode milik PT Metropolitan Medical Centre, terbaru di atas, berbentuk *September 2026 (2026-09)* |
| 4 | Pilih **September 2026** | Ringkasan dan tabel muncul |
| 5 | **Acceptance (1).** Baca kartu ringkasan | Total Debit **Rp 1.000.000**, Total Kredit **Rp 1.000.000**, penanda **Seimbang** berwarna hijau |
| 6 | Baca tabel | Dua baris: `1002 Kas Besar` dan `4001 Pendapatan Rawat Jalan`, masing-masing dengan saldo pembuka, debit, kredit, saldo akhir |
| 7 | Perhatikan bawah tabel | Keterangan bahwa hanya akun bersaldo pembuka atau bermutasi yang ditampilkan |
| 8 | **Acceptance (2).** Ganti badan hukum ke yang lain | Pilihan periode **kosong kembali**, tabel dan ringkasan hilang — angka badan hukum lama tidak menetap |
| 9 | Kembali ke PT Metropolitan Medical Centre, pilih periode selain September 2026 | Tabel kosong dengan pesan *"Belum ada saldo pada periode ini."*, bukan galat |
| 10 | Tekan tombol reset pada bilah penyaring | Periode terlepas, ringkasan hilang |

## 7b. Cacat yang ditemukan owner dan diperbaiki — 7 September 2026

Owner membuka layar ini dan **acceptance (1) serta (3) terbukti berjalan**: Neraca Saldo
September 2026, Total Debit dan Total Kredit masing-masing `Rp 1.000.000`, penanda **Seimbang**,
dua baris akun lengkap dengan saldo pembuka, mutasi, dan saldo akhir.

Pada tangkapan layar yang sama terlihat satu cacat.

**Gejala.** Di bawah tabel yang sedang menampilkan dua baris, tertulis
*"Menampilkan 0 sampai 0 dari 0 data"* beserta tombol **Sebelumnya** dan **Berikutnya**.

**Sebab.** `DataTable` menyalakan paginasi secara **bawaan** — `pagination = true` pada
`data-table.jsx:152`. Layar ini tidak pernah mengirim `totalData` maupun `totalPage` karena
memang tidak berhalaman, sehingga footer paginasi membaca nilai bawaan `0` dan menampilkannya.
Angkanya bukan salah hitung; ia menghitung sesuatu yang memang tidak pernah dikirim.

**Perbaikan.** Satu prop: `pagination={false}`.

**Kenapa uji tidak menangkapnya, dan itu bagian yang lebih penting.** Uji versi pertama berbunyi:

```js
assert.doesNotMatch(viewSource, /PaginationComponent|onPageChange|totalPage/);
```

Ia memeriksa bahwa paginasi **tidak dipasang**, dan itu memang benar — sehingga uji lolos
sementara layarnya tetap menampilkan baris yang salah. **Menguji ketiadaan yang salah bukan
pengganti menguji keberadaan yang benar.** Uji kini menuntut `pagination={false}` benar-benar
ada, bukan sekadar menuntut ketiadaan prop lain.

**`FE-ACC-008` tidak terdampak**: layar buku besar memang berhalaman dan mengirim
`PaginationComponent`, `totalData`, serta `totalPage` dengan benar. Diperiksa, bukan diasumsikan.

Validasi sesudah perbaikan: `lint:errors` PASS, **463 unit test PASS**, `build` PASS.

### 7c. Cacat kedua — keterangan cakupan tertutup footer `DIPERBAIKI`

**Gejala.** Kotak *"Hanya akun yang memiliki saldo pembuka atau mutasi..."* tidak pernah terlihat.
Elemennya **ada** dan dirender; ia tertutup footer aplikasi.

**Sebab.** `.iq-footer.app-footer` ber-`position: fixed`, tinggi 76px, 18px dari dasar layar.
Kompensasinya berupa `--app-footer-safe-space` yang dipasang `Footer.jsx` — tetapi hanya kepada
elemen yang ber-`overflow-y: auto|scroll`, punya isi melebihi tinggi tampak, **dan** setinggi
minimal 240px. Paragraf telanjang sebagai anak **terakhir** halaman tidak memenuhi syarat itu,
sehingga ia berada persis di bawah footer.

**Perbaikan.** Keterangan dipindahkan ke dalam `contextCard`, berdampingan dengan keterangan
*"hanya jurnal yang sudah disahkan"*. Keduanya menjawab pertanyaan yang sama bentuknya — *"apa
yang TIDAK ada di angka ini"* — sehingga dibaca bersama justru lebih jelas.

**Alasan penempatan semula gugur oleh fakta.** Bagian 3.4 semula berargumen bahwa keterangan itu
diletakkan di bawah tabel karena baru relevan sesudah pembaca melihat isinya. Argumen itu benar
secara nalar tetapi tidak berlaku bila teksnya tidak pernah terbaca sama sekali.

**Sebaran diperiksa, bukan diperkirakan.** Kelima layar Accounting disisir:

| Layar | Anak terakhir | Keadaan |
|---|---|---|
| `trial-balance-view.jsx` | paragraf telanjang | **terdampak** — diperbaiki |
| `journal-view.jsx` | paragraf *"Memproses tindakan jurnal..."* | **terdampak** — dipindah ke atas tabel |
| `chart-of-account-view.jsx` | `ConfirmModal` + `ToastStack` | aman, keduanya lapisan melayang |
| `accounting-period-view.jsx` | `ConfirmModal` + `ToastStack` | aman |
| `general-ledger-view.jsx` | `DataTable` | aman, berupa kartu |

Ditahan uji regresi lintas layar pada `tests/unit/accounting-journal-detail.test.mjs`, yang
menolak `<p className={styles.…}>` mana pun sesudah `<DataTable>` di ketiga layar bertabel.

**Mekanisme footer-nya sendiri TIDAK disentuh.** `footer.css` dan `footer.jsx` milik pemilik tata
letak global, dan celah ini berlaku untuk layar mana pun di repository — bukan hanya Accounting.
Diteruskan sebagai catatan, bukan diperbaiki dari sini.

## 8. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| Acceptance (2) belum diuji | **Rendah** | Turun dari *Sedang*. Acceptance (1) dan (3) sudah terbukti di peramban; yang tersisa hanya mengganti badan hukum sekali dan memastikan periode terlepas serta angka lama hilang |
| Daftar periode dibatasi 100 baris | Rendah | Satu tahun buku = 12 periode, jadi cukup untuk delapan tahun. Bila suatu saat lebih, periode terlama akan terpotong — penyaring tahun buku dapat ditambahkan, tetapi itu di luar cakupan yang diminta |
| Performa belum diukur | Sedang | `ACC-TD-018`. `/trial-balance` menjalankan dua agregat berkelompok atas seluruh baris jurnal periode itu, dan akan melambat seiring volume |
| Neraca timpang belum pernah diuji | Rendah | Butuh data rusak yang sengaja dibuat. Jalur tampilannya terbukti lewat unit test, bukan lewat data |
| Nol test render | Rendah | Repository belum punya pola test komponen React |

## 9. Langkah berikutnya

1. **Owner menjalankan skrip bagian 7.** Langkah 5 dan 8 adalah inti kedua acceptance yang belum
   terlihat berjalan.
2. `MVP-2` selesai. Yang tersisa `FE-ACC-010` pembalikan dan `FE-ACC-011` saldo awal — keduanya
   `READY` dan dapat berjalan paralel.
3. `FE-ACC-010` memikul satu utang dari `FE-ACC-007`: tombol **Balik** kini selalu mengirim
   `CorrectionType: FullReversal`, karena field itu `[Required]` sementara pemilihannya adalah
   cakupan `FE-ACC-010`. Dialognya sudah mengatakan itu apa adanya kepada pengguna.

---

**Git status frontend saat laporan ditulis** — belum di-stage, belum di-commit:

```
 M src/lib/constants/corporate/accounting/accounting-constants.jsx
 M src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/corporate/accounting/trial-balance/
?? src/components/view/corporate/accounting/general-ledger/trial-balance-view.jsx
?? src/lib/constants/corporate/accounting/general-ledger/trial-balance-constants.jsx
?? src/lib/hooks/corporate/accounting/general-ledger/use-trial-balance.jsx
?? src/style/corporate/accounting/trial-balance-view.module.css
?? tests/unit/accounting-trial-balance.test.mjs
```
