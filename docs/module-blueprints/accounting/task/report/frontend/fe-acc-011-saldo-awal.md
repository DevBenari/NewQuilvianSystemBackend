# Laporan Perubahan Frontend — `FE-ACC-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-011` |
| Judul | Saldo awal di layar |
| Slice | `MVP-3` — Koreksi dan saldo awal |
| Roadmap | [`../../../roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), bagian `MVP-3`, kartu `FE-ACC-011` |
| Trace | `ACC-DEC-018`, `ACC-DEC-033`; `FR-ACC-060`, `FR-ACC-061`; `EPIC ACC-08`; `UAT-16` |
| Contract version | `ACC-API-0.5` grup Journal — `approved`. **Nol endpoint baru dipanggil**; yang dipakai sudah dikonsumsi `FE-ACC-005` dan `FE-ACC-006` |
| Wewenang UI | Penyesuaian kecil pada layar jurnal yang sudah ada. **Tidak ada layar baru** — batas ini ditulis roadmap sendiri pada kolom `Reuse` |
| Dependency | `FE-ACC-006` `IMPLEMENTED`, `BE-ACC-014` `DONE` (**nol baris kode backend berubah**). Keduanya lunas |
| Klasifikasi | `LIGHT` — 7 berkas disunting, 201 baris bertambah, nol layar baru, nol slice baru, nol komponen base baru, nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; berkas laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` di repository backend |
| Model | `claude-opus-5` |
| Commit frontend saat dikerjakan | `bcccb67bc` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `f989453` (branch `rizkiG`), read-only |
| Tanggal | 7 September 2026 |
| Status | **`IMPLEMENTED`** — ketiga acceptance ada source-nya dan terkunci 8 unit test baru; `lint:errors` PASS, 472 unit test PASS, `build` PASS 0 warning. Skenario `UAT-16` di peramban **menunggu owner** — sesi ini tidak memiliki kredensial |

---

## Ringkasan untuk pembaca umum

Saldo awal adalah angka pembuka: posisi tiap akun pada saat rumah sakit mulai memakai sistem ini.
Tanpa angka itu, buku besar hanya memuat transaksi yang terjadi sesudahnya, dan neraca saldo tidak
pernah cocok dengan pembukuan lama.

Yang perlu diketahui lebih dulu: **saldo awal tidak punya layar sendiri, dan memang tidak
seharusnya punya.** Ia adalah jurnal biasa yang jenisnya `Saldo Awal` — sama seperti Jurnal Umum
atau Jurnal Penyesuaian. Petugas menyusunnya lewat form jurnal yang sudah ada, mengajukannya,
menyetujuinya, lalu mengesahkannya lewat jalur yang sama persis.

Karena itu pekerjaan task ini kecil, dan disengaja kecil. Hanya dua hal yang ditambahkan:

1. **Sebuah keterangan pada form jurnal.** Begitu petugas memilih jenis `Saldo Awal`, muncul kotak
   keterangan berwarna kuning yang mengingatkan satu hal penting: pengesahan saldo awal baru boleh
   dilakukan setelah pimpinan keuangan memberi persetujuannya — dan persetujuan itu **terjadi di
   luar sistem**. Sistem tidak merekamnya dan tidak akan menanyakannya di layar mana pun.
2. **Sebuah penanda pada daftar jurnal.** Baris jurnal saldo awal kini membawa label kecil
   **Saldo Pembuka** di kolom Jenis, supaya ia langsung terlihat di antara puluhan jurnal lain.

Tidak ada tombol baru, tidak ada langkah persetujuan tambahan, dan tidak ada status baru.

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Jalurnya memang sudah ada seluruhnya

`BE-ACC-014` selesai **tanpa satu baris kode backend pun berubah**. Ia bukan pembangunan fitur,
melainkan verifikasi bahwa jalur jurnal yang sudah berdiri sejak `BE-ACC-010` dan `BE-ACC-011`
memang dapat menerima saldo awal apa adanya. Pemeriksaan ulang di source `f989453` membenarkannya:

| Yang diperiksa | Bukti | Kesimpulan |
| --- | --- | --- |
| Jenis jurnal `SA` ada di master | `AccountingMasterDataSeeder.cs:96` — `new JournalTypeDefinition("SA", "Saldo Awal", "SA", true, true)` | Ada sejak seeder 3 September 2026, aktif, dan bertanda `IsSystemType` |
| `SA` ikut terkirim ke form | `JournalTypeController` `[HttpGet("options")]`, hanya jenis aktif | Terkirim; frontend tidak menyaringnya lagi |
| Periode terbuka menerima `SA` | `AccAccountingPeriodService.cs:371` — `AccountingPeriodStatus.Open => new List<string> { "JU", "JP", "JB", "SA" }` | Diterima |
| Endpoint jurnal membedakan `SA` | Tidak ada cabang `SA` mana pun di `JournalController` maupun `AccJournalService` | **Tidak dibedakan** — alurnya sama persis |

Artinya acceptance (1) sebenarnya sudah terpenuhi sebelum task ini dimulai. Yang belum ada hanyalah
penjelasan dan penandanya.

### 1.2 Celah yang sebenarnya

| Celah | Akibat bagi petugas |
| --- | --- |
| Form jurnal tidak menyebut apa pun tentang saldo awal | `ACC-DEC-033` mensyaratkan persetujuan pimpinan keuangan sebelum pengesahan, tetapi **tidak ada satu pun kalimat di layar** yang memberi tahu petugas hal itu. Ia akan menekan Sahkan tanpa tahu ada langkah yang seharusnya sudah selesai di luar sistem |
| Daftar jurnal memperlakukan `SA` seperti jenis lain | Nama "Saldo Awal" memang muncul di kolom Jenis, tetapi sebagai teks biasa di antara "Jurnal Umum" dan "Jurnal Penyesuaian". Pada daftar berisi puluhan baris, jurnal pembuka yang seharusnya paling menonjol justru paling mudah terlewat |

### 1.3 Satu hambatan kontrak yang ditemukan saat pemetaan

Penanda pada daftar tidak dapat dibaca langsung dari baris daftar:

| DTO backend | Membawa `JournalTypeId` | Membawa `JournalTypeCode` |
| --- | :---: | :---: |
| `JournalListResponse` (baris daftar) | **ya** | **tidak** |
| `JournalDetailResponse` (rincian) | ya | ya |
| `JournalTypeOptionResponse` (daftar pilihan) | ya (sebagai `Id`) | **ya** |

Menambah `JournalTypeCode` ke `JournalListResponse` adalah perubahan backend, dan task ini bermode
`FRONTEND`. Jalan keluarnya tidak memerlukannya: daftar pilihan jenis jurnal **sudah** diambil
layar daftar untuk penyaring "Semua jenis", dan daftar itu membawa kodenya. Pemetaan id-ke-kode
karena itu dirangkai dari dua data yang sudah ada di tangan — nol permintaan tambahan, nol
perubahan backend. Rinciannya di bagian 3.3.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya** petugas akuntansi (menyusun) dan Accounting Manager (menyetujui dan mengesahkan).
**Kapan dibuka:** sekali di awal pemakaian sistem per badan hukum, dan sesekali ketika saldo
pembuka periode perlu dicatat.

### 2.1 Alur normal, berurutan

1. Petugas membuka **Akuntansi → Daftar Jurnal**, memilih badan hukum, lalu menekan
   **+ Tambah Jurnal**.
2. Pada kotak **Jenis Jurnal**, petugas memilih **Saldo Awal**.
3. **Saat itu juga** — tanpa menyimpan, tanpa memuat ulang — muncul kotak keterangan kuning tepat
   di bawah baris isian kepala jurnal, di dalam kartu yang sama. Isinya:

   > **Saldo awal — persetujuan pimpinan keuangan dilakukan di luar sistem.**
   > Alur jurnalnya sama dengan jenis lain: simpan draft, ajukan, setujui, lalu sahkan. Bedanya
   > satu, dan letaknya di luar layar ini — pengesahan saldo awal oleh Accounting Manager baru
   > boleh dilakukan setelah persetujuan pimpinan keuangan diperoleh. Sistem tidak merekam dan
   > tidak akan meminta persetujuan itu di layar mana pun, jadi pastikan persetujuannya sudah
   > dikantongi sebelum menekan Sahkan pada rincian jurnal.

4. Petugas mengisi tanggal akuntansi, keterangan, dan baris debit/kredit **persis seperti jurnal
   lain**. Aturan seimbang, kewajiban unit biaya, dan penomoran tidak berubah sedikit pun.
5. **Simpan Draft** atau **Simpan dan Ajukan** — keduanya memanggil endpoint yang sama dengan
   jurnal jenis apa pun.
6. Di **Daftar Jurnal**, baris jurnal tadi tampil dengan kolom Jenis berbunyi `Saldo Awal` diikuti
   label pil **Saldo Pembuka**.
7. Accounting Manager membuka rinciannya, menyetujui, lalu — setelah memastikan persetujuan
   pimpinan keuangan sudah ada di luar sistem — menekan **Sahkan**.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Petugas memilih jenis selain `Saldo Awal` | Keterangan kuning **tidak muncul**. Ia juga langsung hilang bila jenisnya diganti dari `SA` ke jenis lain — penanda mengikuti kotak pilih, bukan beku di nilai awal |
| Daftar pilihan jenis jurnal gagal dimuat | Kotak Jenis Jurnal kosong. Keterangan tidak muncul karena kodenya memang tidak diketahui — layar diam, bukan menebak |
| Jenis `SA` dihapus atau dinonaktifkan admin | `SA` hilang dari kotak pilih, dan penanda pada daftar ikut hilang. Tidak ada galat: penandanya memang diturunkan dari daftar pilihan yang berlaku saat itu |
| Badan hukum belum dipilih pada layar daftar | Perilaku lama tidak berubah — "Badan hukum belum dipilih." |
| Belum ada satu pun jurnal `SA` | Daftar tampil apa adanya tanpa penanda. Bukan keadaan galat |
| Pengguna tanpa hak `Journal : Read` | `AccessDeniedGate` menahan seluruh layar seperti sebelumnya. Task ini tidak menyentuhnya |

### 2.3 Yang **tidak** dibangun, dan mengapa

`ACC-DEC-033` berbunyi: *"Saldo awal disahkan Accounting Manager dengan persetujuan pimpinan
keuangan."* Kalimat itu mudah dibaca sebagai perintah membangun persetujuan berlapis dua.

**Persetujuan pimpinan keuangan berada di luar sistem.** Karena itu tidak dibangun:

- alur persetujuan kedua atau langkah tambahan pada `AvailableActions`;
- field penyetuju, nama pemberi persetujuan, atau nomor surat persetujuan;
- status jurnal baru di luar kelima status yang sudah ada;
- cabang alur kerja yang membedakan `SA` dari jurnal lain.

Alasannya bukan sekadar menghemat pekerjaan: membangun salah satunya berarti sistem **mengaku
merekam** sesuatu yang sebenarnya tidak pernah sampai kepadanya. Field penyetuju yang diisi petugas
sendiri bukan bukti persetujuan; ia hanya membuat kolom kosong terlihat seperti kendali.

Yang dibangun karena itu hanya **satu kalimat**. Larangan ini dikunci uji otomatis — lihat bagian
6.3.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Frontend** (`QuilvianSystemFrontendDev`, `bcccb67bc`):

- `src/components/view/corporate/accounting/journal/journal-view.jsx` — daftar jurnal (`FE-ACC-005`)
- `src/components/view/corporate/accounting/journal/form/journal-form-view.jsx` — form (`FE-ACC-006`)
- `src/lib/hooks/corporate/accounting/journal/use-journal.jsx`, `use-journal-editor.jsx`
- `src/lib/constants/corporate/accounting/journal/journal-constants.jsx`
- `src/components/features/base-features/status-badge.jsx`, `information-alert.jsx`
- `src/style/components/features/base-features/information-alert.module.css`
- `src/style/corporate/accounting/journal-view.module.css`, `journal-form-view.module.css`
- `tests/unit/accounting-journal-detail.test.mjs` — uji regresi tata letak yang mengikat task ini

**Backend** (`NewQuilvianSystemBackend`, `f989453`, **read-only**):

- `Areas/Corporate/AccountingManagement/JournalManagement/DTOs/JournalDtos.cs`
- `Areas/Corporate/AccountingManagement/JournalManagement/Controllers/` — route, `[Tags]`, hak akses
- `Areas/Corporate/AccountingManagement/MasterData/JournalType/DTOs/JournalTypeDtos.cs`
- `Areas/Corporate/AccountingManagement/MasterData/JournalType/Controllers/`
- `Areas/Corporate/AccountingManagement/MasterData/Seeders/AccountingMasterDataSeeder.cs`
- `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccAccountingPeriodService.cs`

**Dokumen:** roadmap frontend, `requirement-traceability.md`, `blueprint-manifest.md`, laporan
`fe-acc-005`, `006`, `008`, `009`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | **+73 baris.** Blok saldo awal: `OPENING_BALANCE_JOURNAL_TYPE_CODE`, `isOpeningBalanceTypeCode`, `collectOpeningBalanceTypeIds`, `OPENING_BALANCE_BADGE`, `OPENING_BALANCE_NOTICE`. Kolom Jenis pada `listColumns` mendapat `format: "journalType"` dan lebarnya 160 → 190 supaya penanda muat |
| `src/lib/hooks/corporate/accounting/journal/use-journal.jsx` | **+14 baris.** Menurunkan `openingBalanceTypeIds` dari daftar pilihan jenis yang **sudah** diambil untuk penyaring, lalu mengembalikannya ke view |
| `src/lib/hooks/corporate/accounting/journal/use-journal-editor.jsx` | **+30 baris.** `journalTypeOptions` kini ikut membawa `code`; `useWatch` atas `journalTypeId` menghasilkan `isOpeningBalance`. Payload **tidak disentuh** |
| `src/components/view/corporate/accounting/journal/journal-view.jsx` | **+32 baris.** Cabang render `format === "journalType"`: nama jenis berdampingan dengan `StatusBadge` **Saldo Pembuka** bila jenisnya `SA` |
| `src/components/view/corporate/accounting/journal/form/journal-form-view.jsx` | **+32 baris.** `InformationAlert` bernada `warning` di dalam kartu kepala jurnal, muncul hanya saat `isOpeningBalance` |
| `src/style/corporate/accounting/journal-view.module.css` | **+11 baris.** `.typeCell` — nama jenis dan penanda berdampingan, membungkus ke bawah pada kolom sempit |
| `src/style/corporate/accounting/journal-form-view.module.css` | **+12 baris.** `.openingBalanceNotice` — menolkan `margin-bottom` bawaan `InformationAlert` karena kartu sudah mengatur jarak lewat `gap`. Hanya margin; warna dan tipografi tetap milik base component |
| `tests/unit/accounting-opening-balance.test.mjs` | **Berkas baru, 8 uji.** Termasuk penjaga jebakan `ACC-DEC-033` dan penjaga tata letak footer |

**Nol** layar baru, **nol** slice Redux baru, **nol** komponen base baru, **nol** Axios instance
baru, **nol** perubahan backend, dan `globals.css` **tidak disentuh**.

### 3.3 Gerbang pemakaian ulang komponen

`UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Keterangan pembantu saat jenis `SA` dipilih | `InformationAlert` | `src/components/features/base-features/information-alert.jsx`; sudah dipakai puluhan layar Administrator; varian `warning` sudah ada di `information-alert.module.css` | `REUSE` | Dipakai apa adanya lewat props `variant`, `title`, `message`, `className` |
| Penanda saldo pembuka pada daftar | `StatusBadge` | `src/components/features/base-features/status-badge.jsx`; sudah dipakai kolom Status pada layar yang sama; nada `info` termasuk lima nada bawaan | `REUSE` | Dipakai apa adanya lewat props `status`, `label`, `pill` |

Nol elemen berstatus `EXTEND`, `WRAP`, atau `NEW`, sehingga **tidak ada keputusan yang perlu
diminta ke pengguna** dan tidak ada pekerjaan yang tertahan menunggunya.

### 3.4 Keputusan teknis yang perlu diketahui pembaca berikutnya

**Penanda memakai KODE `SA`, bukan nama "Saldo Awal".**

Nama jenis jurnal boleh diubah admin lewat `BE-ACC-008`; kodenya tidak. `SA` bertanda
`IsSystemType`, dan tanda itu mengunci kode beserta awalan nomornya. Mencocokkan nama akan patah
tanpa suara begitu seseorang mengganti "Saldo Awal" menjadi "Saldo Pembuka" atau menambahkan
spasi. Uji `jenis saldo awal dikenali dari kode SA, bukan dari namanya` menahan hal itu.

**Id jenis `SA` dirangkai, tidak dihardcode.**

`JournalListResponse` membawa `JournalTypeId` tetapi tidak membawa kodenya. Yang membawa kode
adalah `JournalTypeOptionResponse`, dan daftar itu **sudah** diambil kedua layar — daftar untuk
penyaring jenis, form untuk kotak pilihnya. `collectOpeningBalanceTypeIds` menjembatani keduanya:

```text
daftar pilihan  ->  { value: <Guid>, code: "SA" }  ->  Set{ <Guid> }
baris daftar    ->  JournalTypeId                  ->  Set.has(id) ? tampilkan penanda
```

Guid-nya berbeda per pemasangan database, sehingga menuliskannya di kode akan salah pada
lingkungan mana pun selain satu. Cara ini benar di semua lingkungan tanpa permintaan tambahan.

**Keterangan diletakkan di dalam kartu — dan itu bukan pilihan gaya.**

Footer aplikasi ber-`position: fixed`, dan ruang amannya hanya dipasang `Footer.jsx` pada elemen
ber-`overflow-y` scroll setinggi >= 240px. Paragraf keterangan yang menggantung sebagai anak
terakhir halaman karena itu **tertutup footer dan tidak pernah terbaca** — cacat yang benar-benar
terjadi pada layar neraca saldo `FE-ACC-009` dan ditemukan owner. Keterangan saldo awal karena itu
ditempatkan di dalam kartu kepala jurnal, tepat di bawah kotak pilih yang memicunya. Uji
`keterangan saldo awal berada di dalam kartu, bukan menggantung di ujung halaman` mengunci
posisinya, melengkapi uji sejenis yang sudah ada di `accounting-journal-detail.test.mjs`.

**Nada `warning`, bukan `danger` maupun `info`.**

`danger` berarti ada yang salah — tidak ada yang salah di sini. `info` terlalu mudah dilewati mata
untuk kalimat yang menentukan boleh-tidaknya sebuah tombol ditekan. `warning` adalah salah satu
dari lima varian yang memang sudah dimiliki `InformationAlert`; nol varian baru ditambahkan.

### 3.5 Kepatuhan arsitektur frontend

| Ketentuan | Penerapan |
| --- | --- |
| Alur dependensi | `constants -> hook -> view`. Konstanta tidak mengimpor hook, hook tidak mengimpor view |
| Penempatan folder | Seluruh berkas berada di jalur modul Accounting yang sudah ada; nol folder baru |
| Aturan bisnis tidak dipindah ke frontend | Frontend tidak menghitung kelayakan apa pun. Ia hanya mencocokkan kode jenis untuk keperluan tampilan |
| Pemakaian ulang | `InformationAlert`, `StatusBadge`, `DataTable`, `BaseSelectField`, `Hero` — seluruhnya base component yang sudah ada |
| Redux | Nol slice baru, nol thunk baru, nol perubahan `store.jsx` |
| Token | Seluruh nilai visual memakai variabel `--base-*` / `--app-*`. Nol hex, nol `rgb()`, nol `!important`, nol inline style |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Perilaku lama dipertahankan: daftar menampilkan "Mengambil daftar jurnal...", form menonaktifkan isian selama `busy`. Penanda dan keterangan tidak muncul selama daftar pilihan jenis belum tiba — layar tidak menebak |
| Kosong | Daftar memakai tiga sebab kosong `FE-ACC-005` apa adanya. Daftar tanpa jurnal `SA` bukan keadaan kosong; ia hanya daftar tanpa penanda |
| Gagal | `errorMessage` lama tetap tampil di `errorAlert`. Bila daftar pilihan jenis gagal, kotak Jenis kosong dan keterangan tidak muncul — tidak ada penanda palsu |
| Tanpa hak akses | `AccessDeniedGate` membungkus kedua layar seperti sebelumnya. Task ini tidak mengubah perilaku otorisasi mana pun |

---

## 5. Endpoint yang dikonsumsi

Task ini **tidak menambah satu pun panggilan API**. Ketiganya sudah dikonsumsi `FE-ACC-005` dan
`FE-ACC-006`; yang berubah hanya satu field yang sebelumnya diabaikan frontend kini ikut dibaca.

#### Corporate / Accounting / Journal Management / Journal

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/journals` | Daftar jurnal. `JournalTypeId` tiap baris dicocokkan untuk menentukan penanda **Saldo Pembuka** | `Journal : Read` |
| `POST` | `/api/v1/corporate/accounting/journals` | Menyimpan jurnal saldo awal. **Payload identik** dengan jurnal jenis lain | `Journal : Create` |

#### Corporate / Accounting / Master Data / Journal Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/master-data/journal-types/options?onlyActive=true` | Mengisi kotak Jenis Jurnal dan penyaring jenis. **`JournalTypeCode` kini ikut dibaca** untuk mengenali `SA` | `JournalType : Read` |

---

## 6. Verifikasi

### 6.1 Perintah yang benar-benar dijalankan

| Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error | `PASS` | Keluaran kosong, exit 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | `# tests 472`, `# pass 472`, `# fail 0` | `PASS` | 464 sebelumnya + **8 baru** |
| `node --import ./tests/helpers/register.mjs --test tests/unit/accounting-opening-balance.test.mjs` | `# tests 8`, `# pass 8`, `# fail 0` | `PASS` | Berkas uji baru berjalan sendiri |
| `npm run build` + `postbuild` | `Compiled successfully in 39.0s`, **0 warning, 0 error** | `PASS` | Log build; `[prepare-standalone] Standalone runtime siap dijalankan.` |
| Rute terbentuk | `/corporate/accounting/journals`, `/corporate/accounting/journals/create`, `/corporate/accounting/journals/[slug]`, `.../[slug]/update` | `PASS` | Keluaran build |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`

`npm run test:unit` **gagal apa adanya pada Node 20** — glob `--test` baru tersedia pada Node 21,
sedangkan lingkungan ini `v20.20.2`. Yang dijalankan menguji berkas yang sama persis. Cacat tooling
yang sudah tercatat pada laporan `FE-ACC-007` bagian 5; **bukan** kegagalan task ini.

### 6.2 Grep anti-regresi konsistensi UI

Dijalankan hanya pada **baris yang ditambahkan** di berkas yang berubah:

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Warna literal (`#hex`, `rgb()`, `rgba()`) pada CSS baru | **kosong** |
| 2 | `font-size` / `font-weight` / `line-height` pada CSS baru | **kosong** |
| 3 | `<button>` mentah atau class `btn-*` pada JSX baru | **kosong** |
| 4 | `<table>` mentah pada JSX baru | **kosong** |
| 5 | Utility typography Bootstrap (`fw-*`, `fs-*`) | **kosong** |
| 6 | `!important` baru | **kosong** |
| 7 | `style={{ }}` inline | **kosong** |

Ketujuhnya kosong. `globals.css` tidak muncul pada `git status`.

### 6.3 Uji yang mengunci jebakan task

Roadmap menyebut satu risiko secara eksplisit: *"Jangan membangun alur persetujuan kedua di dalam
sistem — `ACC-DEC-033` menempatkannya di luar sistem."* Risiko itu tidak dijaga oleh niat baik,
melainkan oleh uji:

| Uji | Yang ditahannya |
| --- | --- |
| `saldo awal tidak menumbuhkan alur persetujuan kedua di dalam sistem` | Form dan hook editor tidak boleh memuat `approverId`, `approverName`, `financeApprov*`, `financeLeader`, `secondApprov*`, maupun status saldo awal tersendiri. `buildPayload` juga tidak boleh bercabang mengikuti jenis saldo awal — payload `SA` wajib identik dengan jurnal lain |
| `keterangan saldo awal berada di dalam kartu, bukan menggantung di ujung halaman` | Posisi `InformationAlert` wajib berada di antara `styles.headerCard` dan `styles.linesCard`, dan sebelum `styles.formActions` |
| `jenis saldo awal dikenali dari kode SA, bukan dari namanya` | Pencocokan nama akan menggagalkan uji |
| `daftar pilihan yang cacat tidak menghasilkan penanda palsu` | Baris tanpa kode, tanpa id, dan baris "Semua jenis" bervalue kosong tidak boleh ikut ditandai |

### 6.4 Verifikasi lewat HTTP — jalur terbukti

Backend (`:7184`) dan dev server frontend (`:3000`) sedang berjalan saat task dikerjakan.

| Yang diuji | Hasil | Arti |
| --- | --- | --- |
| `GET /api/v1/corporate/accounting/master-data/journal-types/options?onlyActive=true` | **`401`** | Endpoint **ada**, dijaga autentikasi |
| `GET /api/v1/corporate/accounting/journals` | `401` | Sama |
| `GET /api/v1/corporate/accounting/master-data/jalur-ngawur/options` — **kontrol pembanding** | **`404`** | Membuktikan `401` di atas berarti "ada", bukan "tidak diperiksa" |
| `GET localhost:3000/corporate/accounting/journals` | **`200`** | Rute daftar terbentuk dan dirender |
| `GET localhost:3000/corporate/accounting/journals/create` | **`200`** | Rute form terbentuk dan dirender |
| `GET localhost:3000/corporate/accounting/jalur-ngawur` — **kontrol pembanding** | **`404`** | Kedua `200` di atas bermakna |

Kontrol pembanding itu yang membuat `401` dan `200` berarti sesuatu: jalur yang salah ketik akan
menjawab `404` seperti kedua jalur ngawur tersebut.

### 6.5 Uji manual — `NOT FEASIBLE` untuk isi layar

**`MANUAL TEST: NOT FEASIBLE`** — sesi ini tidak memiliki kredensial pengguna, sedangkan seluruh
endpoint berada di balik `[AccessPermission("Journal", ...)]` dan
`[AccessPermission("JournalType", "Read")]`. Kotak keterangan kuning, label **Saldo Pembuka**, dan
perilaku muncul-hilang saat jenis diganti karena itu **belum dilihat berjalan di peramban**.

Yang sudah terbukti tanpa peramban: jalur endpoint benar, rute terbentuk, ketiga acceptance ada
source-nya, dan delapan uji mengunci perilakunya.

### 6.6 Skrip uji untuk owner — `UAT-16`

Prasyarat sudah terpenuhi: PT Metropolitan Medical Centre, periode `2026-09` terbuka, akun
`1002 Kas Besar` dan `4001 Pendapatan Rawat Jalan` tersedia, dan jenis `SA` sudah terisi seeder.

| # | Langkah | Hasil yang diharapkan |
| --- | --- | --- |
| 1 | **Akuntansi → Daftar Jurnal**, pilih PT Metropolitan Medical Centre | Daftar tampil; `JB/2026/09/00001` terlihat |
| 2 | Tekan **+ Tambah Jurnal** | Form terbuka, kotak keterangan kuning **belum** ada |
| 3 | Buka kotak **Jenis Jurnal** | Berisi Jurnal Umum, Jurnal Penyesuaian, Jurnal Pembalik, dan **Saldo Awal** — **acceptance (1)** |
| 4 | Pilih **Saldo Awal** | **Acceptance (2).** Kotak kuning muncul **seketika** di dalam kartu Kepala Jurnal, di bawah baris isian. Baca isinya: menyebut persetujuan pimpinan keuangan **di luar sistem**, dan menyebut Sahkan |
| 5 | Gulir ke bawah sampai tombol **Simpan Draft** | Kotak keterangan **tidak** tertutup footer — ia berada di dalam kartu, jauh di atas tombol |
| 6 | Ganti jenis ke **Jurnal Umum** | Kotak kuning **hilang** |
| 7 | Kembalikan ke **Saldo Awal** | Kotak kuning muncul lagi |
| 8 | Isi tanggal `2026-09-01`, keterangan "Saldo awal per 1 September 2026", baris 1 `1002 Kas Besar` debit `10.000.000`, baris 2 `4001 Pendapatan Rawat Jalan` kredit `10.000.000` | Selisih **Rp 0**, penanda hijau "sudah seimbang", tombol **Simpan dan Ajukan** hidup |
| 9 | Tekan **Simpan dan Ajukan** | Toast "Diajukan", layar kembali ke daftar. Nomor jurnal berawalan **`SA/`** — **acceptance (1)**, alurnya sama dengan jurnal lain |
| 10 | Perhatikan baris baru pada daftar, kolom **Jenis** | **Acceptance (3).** Tertulis `Saldo Awal` diikuti label pil **Saldo Pembuka**. Kolom **Status** di kanan tetap menampilkan `Menunggu Persetujuan` — keduanya tidak tertukar |
| 11 | Bandingkan dengan baris `JB/2026/09/00001` | Baris jurnal pembalik **tidak** membawa label Saldo Pembuka |
| 12 | Penyaring jenis → pilih **Saldo Awal** | Hanya jurnal `SA` tersisa, seluruhnya berlabel |
| 13 | Buka rincian jurnal `SA`, setujui, lalu **Sahkan** | Alurnya sama persis dengan jurnal lain. **Tidak ada** langkah persetujuan tambahan, **tidak ada** isian penyetuju — sesuai `ACC-DEC-033` |
| 14 | Buka **Buku Besar** akun `1002 Kas Besar` | Mutasi saldo awal muncul; saldo berjalan bertambah `10.000.000` |
| 15 | Buka **Neraca Saldo** periode September 2026 | Angka saldo awal ikut terhitung |

**Tidak dijalankan:** `npm run test:e2e` dan `npm run test:uat` — tidak diminta task, dan `AGENTS.md`
melarang menjalankannya tanpa permintaan eksplisit.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Jenis Saldo Awal dapat dipilih dan alurnya sama dengan jurnal lain | **Terpenuhi — terbukti di source** | `SA` aktif di seeder dan dikirim `/journal-types/options`; `mapOptionList` di `use-journal-editor.jsx` tidak menyaring apa pun; `buildPayload` tidak punya cabang `SA`, dan uji `saldo awal tidak menumbuhkan alur persetujuan kedua` menahannya tetap begitu. **Nomor `SA/` di layar menunggu langkah 9 owner** |
| (2) Keterangan pembantu menjelaskan persetujuan pimpinan keuangan dilakukan di luar sistem sebelum pengesahan | **Terpenuhi — terkunci uji** | `OPENING_BALANCE_NOTICE` dirender `InformationAlert` di `journal-form-view.jsx`; uji memeriksa teksnya memuat "di luar sistem", "pimpinan keuangan", dan "Sahkan". Posisinya di dalam kartu dikunci uji tersendiri. **Rupa di peramban menunggu langkah 4 owner** |
| (3) Jurnal `SA` mudah dikenali pada daftar | **Terpenuhi — terkunci uji** | Cabang `format === "journalType"` di `journal-view.jsx` merender `StatusBadge` **Saldo Pembuka**; `openingBalanceTypeIds` diturunkan dari daftar pilihan. Tiga uji menahan pengenalannya. **Rupa di peramban menunggu langkah 10 owner** |

**Definition of Done** roadmap: *"Layar berfungsi, laporan task tersedia."*

| Butir | Keadaan |
| --- | --- |
| Laporan task tersedia | **Terpenuhi** — berkas ini |
| Layar berfungsi | **Terpenuhi secara struktural**: lint PASS, 472 unit test PASS, build PASS 0 warning, keempat rute terbentuk, kedua rute menjawab `200`. **Belum terbukti secara visual** — `UAT-16` di peramban menunggu owner karena sesi ini tidak berkredensial |

Tidak ada butir yang didiamkan. Tidak ada acceptance yang diakali di frontend.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari lint maupun build. Git memunculkan `LF will be replaced by CRLF` pada empat berkas — peringatan `core.autocrlf` yang sudah ada, bukan akibat task ini |
| Masalah yang diketahui | `JournalListResponse` tidak membawa `JournalTypeCode`, sehingga penanda daftar bergantung pada daftar pilihan jenis jurnal. Konsekuensinya sempit dan sudah ditangani: bila daftar pilihan gagal dimuat, penanda tidak muncul — dan tidak muncul palsu — sedangkan isi daftar tetap tampil normal. Menambahkan field itu ke DTO adalah perubahan backend dan berada di luar mode task ini |
| Dependency backend | `NONE` — `BE-ACC-014` `DONE`, dan task ini tidak menuntut perubahan backend apa pun |
| Perubahan sampingan | `NONE`. Tujuh berkas yang disunting seluruhnya milik modul Accounting dan seluruhnya dalam cakupan task |
| Interupsi | Sesi sempat dihentikan pengguna pada tahap pembacaan governance, sebelum satu berkas pun diubah. Dilanjutkan dari titik yang sama; nol pekerjaan ganda, nol perubahan yang perlu dipulihkan |
| Divergensi aturan yang dilaporkan | `rules/rule-output/status-task-roadmap.md` menetapkan empat tanda `✅`/`🟡`/`⛔`/tanpa tanda, sedangkan roadmap Accounting sejak awal memakai kosakata kata — `READY`, `IMPLEMENTED`, `BLOCKED` — dan aturan yang sama melarang mencampur dua gaya dalam satu roadmap. Baris status `FE-ACC-011` karena itu ditulis mengikuti kosakata yang sudah dipakai sepuluh task lainnya. Penyeragaman kosakata seluruh roadmap adalah pekerjaan tersendiri milik pemilik modul |
| Status Git | `M` pada 7 berkas source frontend, `??` pada `tests/unit/accounting-opening-balance.test.mjs`. **Nol stage, nol commit, nol push** — sesuai permintaan pemilik pekerjaan. Rincian pada bagian 3.2 |
| Langkah berikutnya | Jalankan `UAT-16` bagian 6.6 di peramban. Sesudah itu `FE-ACC-010` menjadi satu-satunya task frontend yang tersisa, dan modul berdiri di 10 dari 11 |
