# `FE-ACC-005` — Daftar jurnal

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-005` |
| Blueprint | `ACC-BP-001` revisi 9, `roadmap/frontend-roadmap.md` gelombang `MVP-1` |
| Task type | Frontend, layar daftar |
| Task mode | `FRONTEND` |
| Kontrak | **`ACC-API-0.4`** — diratifikasi terhadap source `822d48a` pada sesi yang sama, sebelum satu baris kode ditulis |
| Wewenang UI | `ACC-FE-004`..`008` `DEV_DISCRETION` |
| Repository target tulis | `QuilvianSystemFrontendDev` |
| Branch | `RizkiV2` @ `1a86d9322` |
| Status | **`IMPLEMENTED` — menunggu verifikasi manual owner di peramban** |
| Tanggal | 4 September 2026 |

## Ringkasan untuk pembaca umum

Petugas akuntansi kini punya **layar pencarian jurnal**. Sebelum ini, jurnal hanya dapat dilihat
lewat Swagger. Layar ini menampilkan jurnal satu badan hukum, dapat disaring menurut rentang
tanggal, jenis jurnal, status, dan kata pencarian, dengan nomor halaman di bawahnya.

Layar ini **membaca saja**. Menyusun jurnal ada di `FE-ACC-006`, dan tombol setujui/sahkan ada di
`FE-ACC-007`.

## 1. Gerbang kontrak dijalankan lebih dahulu

Task ini **tidak boleh** dimulai di atas `ACC-API-0.3`. Gap `ACC-GAP-004` berstatus **Tinggi** dan
berbunyi: *"Frontend yang menyusun klien dari kontrak akan salah."* Karena itu ratifikasi
dikerjakan lebih dahulu, dan baru sesudah `ACC-API` naik ke `0.4` kode ditulis.

Yang berpengaruh langsung ke task ini:

| Selisih | Akibat bila diabaikan |
|---|---|
| `JournalListDto` → **`JournalListResponse`** | Nama class di kontrak tidak pernah cocok dengan payload |
| `JournalListResponse` memuat `JournalTypeId` | Penyaring jenis tidak punya kunci untuk mencocokkan baris |
| `JournalPagedQuery` memuat `SortBy`, `SortDirection` | Pengurutan tidak akan pernah terkirim |

## 2. Gerbang pemakaian ulang komponen

| Kebutuhan | Dipakai ulang | Putusan |
|---|---|---|
| Daftar berhalaman | `DataTable` + `RegionPagination` | **REUSE** |
| Panel penyaring | `DataFilter` | **REUSE** |
| Rentang tanggal | `FilterDatePicker` | **REUSE** |
| Kotak pilih | `FilterSelect` | **REUSE** |
| Penanda status | `StatusBadge` — kelima nada memakai status bawaannya, nol status baru | **REUSE** |
| Pemilih badan hukum | `AccountingLegalEntitySelect` dari `FE-ACC-001` | **REUSE** |
| Pilihan jenis jurnal | `getJournalTypeOptions` dari slice `FE-ACC-003` | **REUSE**, nol jalur pengambilan data baru |
| Uang rupiah | `formatCurrencyIDR` pada `src/utils/Formatters.jsx` | **REUSE** |
| Helper umum | `safeString`, `getByKeys`, `unwrapApiData`, `formatDateOnly` | **REUSE** |
| Slice | `createMasterDataResourceSlice` | **REUSE** — lihat delta di bagian 3 |

**Nol Axios instance baru, nol komponen base baru, nol `style={{ }}`, `globals.css` nol berkas
berubah.** Diverifikasi dengan pencarian, bukan dengan ingatan.

## 3. Delta terhadap kalimat roadmap

Roadmap menulis slice ini *"ditulis manual mengikuti bentuk state hasil factory"*. Yang dipakai
adalah **factory-nya langsung**, bukan salinan bentuknya.

Alasannya: jalur daftar, rincian, tambah, dan ubah pada endpoint jurnal berbentuk sama persis
dengan yang sudah ditangani `createMasterDataResourceSlice`, dan `FE-ACC-002` sudah membuktikannya
pada endpoint Accounting. Menyalin ulang bentuk state secara manual berarti menulis arsitektur
tandingan untuk hasil yang identik — persis yang dilarang aturan repository.

Yang memang **tidak** dimiliki factory adalah lima tindakan alur kerja jurnal — ajukan, setujui,
tolak, sahkan, balik. Kelimanya ditulis sebagai thunk tersendiri lewat satu pabrik kecil, mengikuti
pola `deactivateChartOfAccount` yang sudah ada. Delta ini dicatat, bukan didiamkan.

## 4. Berkas yang berubah

### Ditambahkan

| Berkas | Isi |
|---|---|
| `src/lib/state/slice/corporate/accounting/accounting-journal-slice.jsx` | Factory + 5 thunk alur kerja |
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | Enum, label, kolom, penyaring, sebab kosong |
| `src/lib/hooks/corporate/accounting/journal/use-journal.jsx` | Hook daftar |
| `src/components/view/corporate/accounting/journal/journal-view.jsx` | Layar daftar |
| `src/style/corporate/accounting/journal-view.module.css` | Gaya, token `--base-*` |
| `src/app/corporate/accounting/journals/page.jsx` + `journal-client.jsx` | Rute tipis |

### Diubah

| Berkas | Perubahan |
|---|---|
| `src/lib/state/store.jsx` | +2 baris — reducer `accountingJournal` |
| `src/utils/menu-sidebar/menu-items.jsx` | +6 baris — entri **Jurnal**, di luar submenu *Master Data* karena jurnal transaksi, bukan master |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Kartu *Daftar Jurnal* ditandai tersedia beserta tautannya |

## 5. Acceptance

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Penyaring bekerja, pagination bentuk `{pageNumber, pageSize, totalData, totalPage, items}` | **TERBUKTI SEBAGIAN** | Bentuk itu dibaca `unwrapApiData` + `getByKeys` dengan pasangan camelCase/PascalCase. Perilakunya di peramban belum dilihat |
| (2) | Status jurnal berlabel Bahasa Indonesia | **TERBUKTI** | Kelima label disalin dari atribut `[Display]` pada `JournalStatus.cs` — Draft, Menunggu Persetujuan, Disetujui, Disahkan, Ditolak. Bukan karangan sendiri |
| (3) | Flag pemuatan terpisah per operasi | **TERBUKTI** | Hook mengembalikan `listLoading` dan `actionLoading` dari selector factory yang memang terpisah, bukan satu penanda halaman |
| (4) | Daftar kosong menjelaskan sebabnya | **TERBUKTI** | `JOURNAL_EMPTY_REASONS` memuat tiga sebab berbeda — badan hukum belum dipilih, tersaring habis, atau memang belum ada — dan hook memilihnya menurut keadaan |

## 6. Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `npx eslint --quiet` atas seluruh berkas accounting yang disentuh | **PASS**, exit 0 |
| Unit test 434 | **PASS**, `# pass 434`, `# fail 0` |
| `npm run build` + `postbuild` | **PASS**, `Compiled successfully` |
| Rute terbentuk | `/corporate/accounting/journals` muncul di daftar rute build |
| Nol Axios instance baru | `grep` pada slice dan hook: **0** |
| View memanggil `InstanceAxios` langsung | **0** |

## 7. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Sesi ini tidak memiliki kredensial, dan mengambil kredensial dari `.env` dilarang aturan
keselamatan lingkungan. Seluruh perilaku penyaring, paginasi, dan gabungan penyaring **belum
dilihat mata manusia**.

### Skrip uji untuk owner

Prasyarat: backend berjalan, badan hukum sudah dipilih, dan **minimal satu jurnal sudah ada**.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | Buka **Akuntansi → Jurnal** | Tabel tampil; bila badan hukum belum dipilih, pesan kosongnya berbunyi *"Badan hukum belum dipilih."* |
| 2 | Pilih badan hukum, lalu perhatikan tabel | Daftar termuat ulang otomatis |
| 3 | Isi **Tanggal Awal** dan **Tanggal Akhir** | Daftar menyempit, dan halaman kembali ke 1 |
| 4 | Pilih satu **jenis jurnal** | Penyaring bergabung dengan rentang tanggal, tidak saling menghapus |
| 5 | Pilih status **Disahkan** | Hanya jurnal disahkan yang tampil, penandanya hijau |
| 6 | Ketik nomor jurnal yang tidak ada | Pesan kosong berbunyi *"Tidak ada jurnal yang cocok dengan penyaring."* — bukan pesan "belum ada jurnal" |
| 7 | Tekan **Reset** | Seluruh penyaring kosong dan daftar penuh kembali |
| 8 | Ubah **25 baris** menjadi **10 baris**, lalu pindah halaman | Nomor halaman dan total ikut menyesuaikan |

## 8. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| Belum ada satu pun jurnal di database | **Sedang** | `BLK-ACC-02` masih terbuka: daftar akun dan periode belum terisi, sehingga jurnal belum dapat dibuat. Layar ini akan tampil kosong sampai itu beres |
| Pengurutan belum dipakai | Rendah | `SortBy`/`SortDirection` ada di kontrak dan didukung backend, tetapi layar ini belum menyediakan kendalinya |
| Baris tidak dapat diklik menuju rincian | Rendah | Disengaja — layar rincian adalah `FE-ACC-007` dan belum ada. Menautkannya sekarang menghasilkan 404 |

## 9. Langkah berikutnya

`FE-ACC-007` — rincian jurnal dan tombol aksi. Kelima thunk alur kerjanya **sudah tersedia** di
slice ini. Perhatikan `ACC-GAP-011`: `JournalApprovalResponse` tidak memuat nama penyetuju, hanya
`ActionBy` bertipe `Guid`.
