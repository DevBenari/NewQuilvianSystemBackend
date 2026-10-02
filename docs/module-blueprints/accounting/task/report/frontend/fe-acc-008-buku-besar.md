# `FE-ACC-008` — Buku besar

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-008` |
| Blueprint | `ACC-BP-001` revisi 9, `roadmap/frontend-roadmap.md` gelombang `MVP-2` |
| Task type | Frontend, layar daftar baca-saja |
| Task mode | `FRONTEND` |
| Kontrak | **`ACC-API-0.5`** grup General Ledger, endpoint `/movements` |
| Wewenang UI | `DEV_DISCRETION` — susunan kolom, CSS Module, ikon (`03-frontend-architecture.md` bagian 7) |
| Repository target tulis | `QuilvianSystemFrontendDev` |
| Branch | `RizkiV2` @ `1ebb4c419` |
| Baseline backend dibaca | `rizkiG` @ `26666ee` (read-only) |
| Status | **`IMPLEMENTED` — menunggu verifikasi manual owner di peramban** |
| Tanggal | 7 September 2026 |

## Ringkasan untuk pembaca umum

Layar ini menjawab satu pertanyaan: *"akun ini pernah bergerak apa saja, dan sesudah tiap
gerakan saldonya jadi berapa?"*

Dua hal yang membentuk seluruh keputusan teknisnya:

**Saldo berjalan bukan milik layar.** Angkanya datang utuh dari backend dan tidak dihitung ulang
di sini. Itu bukan kerapian — backend memasukkan saldo seluruh mutasi **sebelum** rentang tanggal
yang diminta, sedangkan penjumlahan di layar akan mulai dari nol. Begitu petugas menyaring
"September saja", kolom hasil hitungan sendiri akan salah pada setiap barisnya.

**Layar menahan permintaannya sendiri.** `AccountId` di backend bertipe `Guid` yang tidak
nullable, jadi memanggil `/movements` sebelum akun dipilih menghasilkan `404 Akun tidak
ditemukan` — pesan yang benar bagi backend tetapi menyesatkan bagi petugas yang sebenarnya hanya
belum memilih apa pun.

## 1. Validasi masukan sebelum implementasi

| Yang diperiksa | Hasil |
|---|---|
| Revision blueprint | `9`, `approved` |
| Hash artefak canonical | **17/17 cocok** |
| Kontrak | `ACC-API-0.5` — cocok dengan source `26666ee` |
| Dependency `FE-ACC-001` | `IMPLEMENTED` |
| Dependency `BE-ACC-012` | `DONE`; `GeneralLedgerController` memuat `/movements`, `/trial-balance`, `/account-balance/{accountId}` |

**Kedua baseline bergeser dari yang tercatat, sehingga impact scan dijalankan lebih dahulu.**

| Baseline | Tercatat | Sekarang | Temuan |
|---|---|---|---|
| Backend | `822d48a` | `26666ee` | **Nol berkas `GeneralLedger/` berubah.** Selisihnya hanya `JournalManagement` — `BE-ACC-015` dan penjaga pembalikan `ACC-TD-020`. Permukaan yang disandari task ini identik dengan baseline tercatat |
| Frontend | `418aebb05` | `1ebb4c419` | Dua anchor berubah, **keduanya tidak breaking**. `use-accounting-legal-entity.jsx` ditulis ulang dari `useState` ke `useSyncExternalStore` — bentuk kembaliannya tetap delapan kunci yang sama. `accounting-chart-of-account-slice.jsx` menerima komposisi reducer `ACC-TD-021`; `getChartOfAccountOptions` dan `selectChartOfAccountOptions` tetap diekspor. `DataTable`, `DataFilter`, `filter-date-picker`, `Formatters`, dan `store.jsx` nol perubahan |

## 2. Gerbang pemakaian ulang komponen

Nol komponen base baru. Nol perubahan `globals.css`. Nol perubahan backend. Nol perubahan factory.

| Kebutuhan | Dipakai ulang | Putusan |
|---|---|---|
| Tabel berhalaman | `DataTable` + `RegionPagination` | **REUSE** |
| Bilah penyaring | `DataFilter` | **REUSE** |
| Rentang tanggal | `filter-date-picker.jsx` | **REUSE** |
| Pemilih akun dan ukuran halaman | `filter-select.jsx` | **REUSE** |
| Pemilih badan hukum | `AccountingLegalEntitySelect` dari `FE-ACC-001`, yang membungkus `resource-filter-select.jsx` | **REUSE** |
| Pilihan akun | `getChartOfAccountOptions` dari slice `FE-ACC-002` | **REUSE**, nol jalur pengambilan data baru |
| Keadaan badan hukum | `use-accounting-legal-entity.jsx` | **REUSE** |
| Penjaga hak akses | `access-denied-gate.jsx` | **REUSE** |
| Pembantu format | `chart-of-account-utils.jsx`, `formatCurrencyIDR` | **REUSE** |
| Pola layar | `journal-view.jsx` | **REUSE** |

## 3. Keputusan teknis yang perlu diketahui pembaca berikutnya

### 3.1 Slice ditulis manual, dan alasannya lebih kuat daripada `FE-ACC-004`

Grup General Ledger **tidak punya mutasi sama sekali**: tidak ada `POST`, `PUT`, `DELETE`, dan
tidak ada `GET /{id}` maupun `/options`. Yang ada hanya pembacaan berhalaman.

`createMasterDataResourceSlice` menurunkan sebelas thunk. Sepuluh di antaranya akan menunjuk
endpoint yang tidak pernah ada. Itu bukan pemakaian ulang, melainkan menyembunyikan bentuk
sebenarnya dari pembaca berikutnya. Slice ini karena itu ditulis manual mengikuti preseden
`accounting-period-slice.jsx`.

**Akibatnya `ACC-TD-021` tidak berlaku di sini.** Cacat itu menyangkut thunk mandiri yang
menumpang slice factory; slice ini bukan slice factory, dan ia tidak punya `actionLoading` sama
sekali karena tidak ada tindakan yang perlu dijaga dari kiriman ganda.

### 3.2 Delta terhadap daftar reuse roadmap — `resource-filter-select` untuk akun

Roadmap menyebut `resource-filter-select.jsx` sebagai reuse. Untuk **badan hukum** itu memang
yang dipakai, lewat `AccountingLegalEntitySelect`.

Untuk **akun**, tidak. `resource-filter-select.jsx` bekerja di atas `useSelectResource`, dan
resource yang terdaftar di `src/lib/hooks/select/hr/hr-select-resources.js` hanya
`legalEntities` dan `costCenters` — **`chartOfAccounts` tidak ada di sana**. Memakainya berarti
mendaftarkan resource baru di registry milik HR demi kebutuhan Accounting.

Yang dipakai sebagai gantinya: `FilterSelect` bersifat `searchable`, diisi
`getChartOfAccountOptions` dari slice `FE-ACC-002` — persis pola yang sudah dipakai
`journal-view.jsx` untuk jenis jurnal dan `use-journal-editor.jsx` untuk akun. Nol jalur
pengambilan data baru, nol berkas modul lain tersentuh.

### 3.3 Nol ditampilkan sebagai tanda hubung

Tiap baris jurnal hanya mengisi salah satu dari debit atau kredit. Kolom yang penuh `Rp 0` hanya
menambah bising dan menyamarkan sisi mana yang sebenarnya bergerak. Saldo berjalan **tetap**
ditampilkan walau nol — di sana nol adalah informasi, bukan ketiadaan.

### 3.4 Angka lama dibersihkan saat akun diganti

`clearGeneralLedgerMovements` dipanggil begitu badan hukum atau akun dikosongkan. Tanpa itu,
mutasi akun sebelumnya menetap di layar sementara penyaringnya sudah berubah — pembaca akan
mengira angka itu milik akun yang sekarang.

## 4. Berkas yang berubah

### Ditambahkan

| Berkas | Isi |
|---|---|
| `src/lib/constants/corporate/accounting/general-ledger/general-ledger-constants.jsx` | Config, tujuh kolom, empat sebab kosong, keterangan "hanya jurnal disahkan" |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | Slice manual, satu thunk `getLedgerMovements` |
| `src/lib/hooks/corporate/accounting/general-ledger/use-general-ledger.jsx` | Penyaring, penahan permintaan, pilihan akun, paginasi |
| `src/components/view/corporate/accounting/general-ledger/general-ledger-view.jsx` | Layar daftar |
| `src/style/corporate/accounting/general-ledger-view.module.css` | Gaya, token `--base-*` |
| `src/app/corporate/accounting/general-ledger/page.jsx` | Rute tipis |
| `src/app/corporate/accounting/general-ledger/general-ledger-client.jsx` | Pembungkus client |
| `tests/unit/accounting-general-ledger.test.mjs` | **7 uji**, mengunci ketiga acceptance |

### Diubah

| Berkas | Perubahan |
|---|---|
| `src/lib/state/store.jsx` | `accountingGeneralLedger` didaftarkan — 2 baris |
| `src/utils/menu-sidebar/menu-items.jsx` | Menu **Buku Besar** di kelompok Akuntansi — 6 baris, ikon `RiBookletLine` yang sudah di-import |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Kartu *Buku Besar* ditandai tersedia beserta tautannya. **Ditambah satu koreksi bawaan `FE-ACC-007`**: kartu *Rincian Jurnal* masih tertulis "Belum tersedia" padahal layarnya sudah berdiri — kini `available: true` tanpa `href`, karena ia dibuka dari baris daftar jurnal, bukan dari kartu |

**Nol berkas backend berubah. Nol sentuhan `globals.css`. Nol komponen base baru. Nol perubahan
`master-data-resource-slice-factory.jsx`. Nol registry modul lain disentuh.**

## 5. Acceptance

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Saldo berjalan tampil per baris | **TERBUKTI — uji otomatis** | Kolom `runningBalance` membaca `["runningBalance","RunningBalance"]` apa adanya. Uji menegaskan nol akumulasi di view maupun hook, dan nol pengurangan debit-kredit |
| (2) | Rentang tanggal terbalik ditolak dan pesannya terbaca | **TERBUKTI di kode** | Backend menjawab `400 "Tanggal akhir tidak boleh mendahului tanggal mulai."` (`AccGeneralLedgerService.PeriksaRentang`). Slice meneruskan `action.payload.message` apa adanya ke `movementsError`, dan view menampilkannya pada `errorAlert`. Uji menegaskan aturan itu **tidak** disalin ke frontend |
| (3) | Angka rupiah diformat konsisten | **TERBUKTI — uji otomatis** | `formatCurrencyIDR` dipakai untuk debit, kredit, dan saldo berjalan. Uji menolak `toLocaleString` maupun `Intl.NumberFormat` di layar |

Ketiga acceptance dapat dipenuhi. Tidak ada yang perlu diakali di frontend.

## 6. Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `npm run lint:errors` | **PASS**, exit 0 |
| Unit test seluruh suite | **PASS** — `# tests 452`, `# pass 452`, `# fail 0` (445 sebelumnya + 7 baru) |
| `npm run build` + `postbuild` | **PASS**, `Compiled successfully in 42s` |
| Rute terbentuk | `○ /corporate/accounting/general-ledger` muncul di keluaran build |
| ESLint atas berkas yang berubah, termasuk warning | **0 error, 0 warning** |
| Nol Axios instance baru, nol `InstanceAxios` di view, nol `style={{ }}` | **0 / 0 / 0** |

`npm run test:unit` tetap tidak dapat dijalankan apa adanya pada Node 20 — glob `--test` baru ada
di Node 21. Yang dijalankan menguji berkas yang sama: `node --import ./tests/helpers/register.mjs
--test tests/unit/`. Cacat tooling yang sudah ada, dicatat pada laporan `FE-ACC-007` bagian 5.

## 7. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Sebabnya sama seperti `FE-ACC-007` dan tetap berlaku: backend (`:5107`) dan dev server (`:3000`)
tidak berjalan, tidak ada kredensial, dan endpoint berada di balik
`[AccessPermission("GeneralLedger","Read")]`.

**Selain itu, layar ini belum dapat menampilkan satu baris pun oleh siapa pun.** Buku besar hanya
memuat jurnal **disahkan**, dan modul ini belum punya satu pun — `JB/2026/09/00001` masih
*Menunggu Persetujuan*. Verifikasinya karena itu **bergantung pada `UAT-01` diselesaikan lebih
dahulu**, yang sendiri menunggu akun penyetuju kedua (`ACC-DEC-016`).

### Skrip uji untuk owner

Prasyarat: **minimal satu jurnal sudah berstatus Disahkan**.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | **Akuntansi → Buku Besar** | Layar terbuka. Tabel kosong dengan pesan *"Akun belum dipilih."*, bukan galat |
| 2 | Perhatikan kotak keterangan di atas penyaring | Tertulis bahwa laporan hanya memuat jurnal yang sudah disahkan |
| 3 | Pilih badan hukum, biarkan akun kosong | Tetap *"Akun belum dipilih."* — **dan tidak ada permintaan `404` yang terkirim** |
| 4 | Pilih akun yang dipakai jurnal disahkan | Mutasinya muncul beserta kolom Saldo Berjalan |
| 5 | **Acceptance (1).** Bandingkan saldo baris terakhir dengan saldo akun di layar COA | Harus sama |
| 6 | **Acceptance (2).** Isi Tanggal Awal `31 Desember 2026`, Tanggal Akhir `1 Januari 2026` | Pesan backend tampil apa adanya: *"Tanggal akhir tidak boleh mendahului tanggal mulai."* |
| 7 | **Acceptance (1) yang sebenarnya.** Persempit rentang sehingga sebagian mutasi terpotong | Saldo berjalan **tidak** mulai dari nol — ia sudah memuat saldo sebelum rentang. Inilah yang membuktikan angkanya dari backend |
| 8 | **Acceptance (3).** Perhatikan kolom Debit, Kredit, Saldo | Format rupiah konsisten; sisi yang tidak terisi bertanda hubung, bukan `Rp 0` |
| 9 | Ganti akun | Tabel berganti, angka akun sebelumnya tidak menetap |
| 10 | Ubah ukuran halaman ke 10, lalu pindah halaman | Paginasi bekerja dan penyaring tidak hilang |

## 8. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| **Layar belum dapat diuji sama sekali** | **Tinggi** | Bukan cacat task ini. `BLK-ACC-02`: nol jurnal disahkan. Semua acceptance terbukti di source, **nol terbukti berjalan** |
| Performa belum diukur | Sedang | `ACC-TD-018` — `/movements` menghitung dua agregat tambahan per permintaan dan akan melambat lebih dulu daripada endpoint lain. Index menunggu bukti dari data nyata |
| Pilihan akun hanya akun yang menerima transaksi | Rendah | Disengaja. `/options` menyaring `IsActive && IsPostable`, dan hanya akun semacam itu yang dapat memiliki mutasi. Akun induk memang tidak punya buku besar sendiri |
| Nol test render | Rendah | Repository belum punya pola test komponen React. Yang dapat diuji tanpa render sudah diuji |
| Akun nonaktif yang masih bersaldo tidak muncul di pilihan | Rendah | `/options` menyaring `IsActive`. Menelusuri riwayat akun yang sudah dinonaktifkan karena itu belum mungkin lewat layar ini — **perlu putusan owner** apakah itu kebutuhan nyata |

## 9. Langkah berikutnya

1. **`FE-ACC-009`** neraca saldo — kini satu-satunya yang menunggu `FE-ACC-008`. Endpoint
   `/trial-balance` sudah berdiri, dan slice ini tinggal ditambah satu thunk.
2. **Selesaikan `UAT-01`** supaya layar ini dan `FE-ACC-009` dapat benar-benar dilihat bekerja.
3. `FE-ACC-010` dan `FE-ACC-011` sama-sama `READY` dan dapat berjalan paralel.

---

**Git status frontend saat laporan ditulis** — belum di-stage, belum di-commit:

```
 M src/lib/constants/corporate/accounting/accounting-constants.jsx
 M src/lib/state/store.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/corporate/accounting/general-ledger/
?? src/components/view/corporate/accounting/general-ledger/
?? src/lib/constants/corporate/accounting/general-ledger/
?? src/lib/hooks/corporate/accounting/general-ledger/
?? src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx
?? src/style/corporate/accounting/general-ledger-view.module.css
?? tests/unit/accounting-general-ledger.test.mjs
```
