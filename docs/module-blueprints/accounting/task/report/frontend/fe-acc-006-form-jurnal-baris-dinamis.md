# `FE-ACC-006` — Form jurnal dengan baris dinamis

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-006` |
| Blueprint | `ACC-BP-001` revisi 9, `roadmap/frontend-roadmap.md` gelombang `MVP-1` |
| Task type | Frontend, layar form |
| Task mode | `FRONTEND` |
| Kontrak | **`ACC-API-0.4`**, `ACC-VALIDATION-0.3` bagian 3 |
| Wewenang UI | `ACC-FE-004`..`008` `DEV_DISCRETION` |
| Repository target tulis | `QuilvianSystemFrontendDev` |
| Branch | `RizkiV2` @ `1a86d9322` |
| Status | **`IMPLEMENTED` — menunggu verifikasi manual owner di peramban** |
| Tanggal | 4 September 2026 |

## Ringkasan untuk pembaca umum

Petugas kini dapat **menyusun jurnal lewat layar**. Kepala jurnal diisi di atas, lalu barisnya
ditambah satu per satu di bawah: akun, unit biaya, keterangan, debit, kredit. Total debit, total
kredit, dan **selisihnya** dihitung ulang setiap kali angka berubah.

Dua hal yang penting dipahami:

1. **Draft boleh disimpan walau belum seimbang.** Itu memang dikehendaki — draft adalah pekerjaan
   yang belum selesai.
2. **Draft yang belum seimbang tidak boleh diajukan.** Tombol *Simpan dan Ajukan* mati sampai
   selisihnya nol.

Roadmap menandai task ini **risiko tertinggi pada frontend**, karena ia satu-satunya layar yang
tidak mengikuti pola form biasa.

## 1. Kenapa tidak memakai `BaseEditorView`

Seluruh form Accounting sebelumnya (`FE-ACC-002`, `FE-ACC-003`) memakai `BaseEditorView`. Layar ini
**tidak bisa**: `BaseEditorView` menyusun form dari daftar field yang jumlahnya tetap, sedangkan
jurnal punya jumlah baris yang berubah-ubah dan total yang dihitung silang antar baris.

Roadmap sudah mengantisipasinya dan menetapkan bahan yang dipakai: `react-hook-form` dengan
`FormProvider` dan `Controller`, `summary-grid.jsx`, `base-form-control.jsx`, dan
`resource-filter-select.jsx`. Keempatnya dipakai apa adanya — **nol komponen base baru**.

## 2. Gerbang pemakaian ulang komponen

| Kebutuhan | Dipakai ulang | Putusan |
|---|---|---|
| State form + baris dinamis | `react-hook-form` `useForm` + `useFieldArray` | **REUSE**, sudah dependency project |
| Isian teks, tanggal, area teks, kotak pilih | `base-form-control.jsx` | **REUSE** |
| Ringkasan total | `SummaryGrid` | **REUSE** |
| Unit biaya | Resource select **`costCenters`** yang sudah terdaftar di `hr-select-resources.js:616` | **REUSE**, nol jalur pengambilan data baru |
| Pilihan akun | `getChartOfAccountOptions` dari slice `FE-ACC-002` | **REUSE** |
| Pilihan jenis jurnal | `getJournalTypeOptions` dari slice `FE-ACC-003` | **REUSE** |
| Slice jurnal | `accounting-journal-slice.jsx` dari `FE-ACC-005` | **REUSE** |
| Token rute privat | `resolvePrivateRouteToken` + pola `[slug]/update` milik COA | **REUSE** |
| Uang rupiah | `formatCurrencyIDR` | **REUSE** |

## 3. Keputusan teknis yang perlu diketahui pembaca berikutnya

### Uang dibandingkan sebagai bilangan bulat sen

`totalDebit === totalCredit` pada bilangan pecahan biner **tidak dapat dipercaya**: `0.1 + 0.2`
bukan `0.3`. Karena itu seluruh nominal dikalikan 100 dan dibulatkan sebelum dijumlahkan dan
dibandingkan. Tombol *Ajukan* bergantung pada perbandingan ini, sehingga ia tidak boleh meleset
karena pembulatan.

### Kewajiban unit biaya datang dari backend, bukan dihafal frontend

Roadmap menegaskannya, dan itu diikuti: kolom unit biaya menjadi wajib mengikuti
**`RequiresCostCenter` pada `ChartOfAccountOptionResponse`**, bukan aturan `AccountType == Expense`
yang disalin ke frontend. Bila kelak aturannya berubah di backend, layar ini ikut berubah tanpa
disentuh.

### Pilihan akun tidak disaring ulang di frontend

`GET /chart-of-accounts/options` sudah membatasi ke `IsActive && IsPostable` — diperiksa langsung
pada source `ChartOfAccountService.GetOptionsAsync` baris 209–212. Menyaring lagi di frontend
berarti menyalin aturan bisnis ke tempat yang salah, dan akan berbeda diam-diam ketika backend
berubah.

### Satu baris = satu kotak pilih unit biaya

`useSelectResource` adalah hook, sehingga tidak boleh dipanggil di dalam `map`. Karena itu setiap
baris menjadi komponennya sendiri (`JournalLineRow`), dan hook-nya dipanggil di sana.

## 4. Berkas yang berubah

### Ditambahkan

| Berkas | Isi |
|---|---|
| `src/lib/hooks/corporate/accounting/journal/use-journal-editor.jsx` | State form, baris, total, simpan, ajukan |
| `src/components/view/corporate/accounting/journal/form/journal-form-view.jsx` | Layar form + `JournalLineRow` |
| `src/style/corporate/accounting/journal-form-view.module.css` | Gaya, token `--base-*` |
| `src/app/corporate/accounting/journals/create/page.jsx` | Rute tambah |
| `src/app/corporate/accounting/journals/[slug]/update/page.jsx` | Rute ubah, memakai penjaga token yang sama dengan COA |

### Diubah

| Berkas | Perubahan |
|---|---|
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | `createEmptyJournalLine`, `JOURNAL_HEADER_FIELDS` |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Kartu *Form Jurnal* ditandai tersedia |

## 5. Acceptance

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Tombol Ajukan mati selama selisih belum nol, selisih tampil berwarna peringatan | **TERBUKTI di kode** | `disabled={busy \|\| !totals.isBalanced}`; selisih dirender pada `.imbalanceWarning` yang memakai token `--base-danger-*`. Perilakunya di peramban belum dilihat |
| (2) | Pilihan akun hanya akun yang menerima transaksi, dari `/options`, tidak disaring ulang | **TERBUKTI** | Backend menyaring `IsActive && IsPostable`; frontend nol filter. Diverifikasi pada source, bukan diasumsikan |
| (3) | Memilih akun beban memunculkan kewajiban unit biaya | **TERBUKTI di kode** | `requiresCostCenter` dibaca dari respons `/options`, dipakai sebagai `rules.validate` pada `Controller` unit biaya |
| (4) | Baris muat tanpa gulir mendatar | **TERBUKTI** — diperbaiki, lihat bagian 10.1 | Semula `overflow-x: auto` dipakai untuk layar sempit, dan justru itu yang memotong dropdown. Gulir mendatar kini **dicabut sama sekali**: desktop memakai `table-layout: fixed`, layar di bawah 992px menumpuk tiap baris menjadi kartu. Nol gulir mendatar di semua lebar |
| (5) | Menutup layar di tengah pengisian tidak menghilangkan draft yang sudah tersimpan | **TERBUKTI di kode** | Draft yang sudah disimpan adalah jurnal berstatus Draft di backend; layar ubah memuatnya kembali lewat `getJournalById`. **Isian yang belum pernah disimpan memang hilang** — lihat bagian 8 |

## 6. Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `npx eslint --quiet` atas seluruh berkas accounting | **PASS**, exit 0 |
| Unit test 434 | **PASS**, `# pass 434`, `# fail 0` |
| `npm run build` + `postbuild` | **PASS**, `Compiled successfully` |
| Rute terbentuk | `/corporate/accounting/journals/create` dan `/journals/[slug]/update` |
| Nol Axios instance baru, nol `InstanceAxios` di view, nol `style={{ }}` | **0 / 0 / 0** |

## 7. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Sesi ini tidak memiliki kredensial. Selain itu, **`UAT-02` dan `UAT-04` belum dapat dijalankan
siapa pun**: `BLK-ACC-02` masih terbuka, sehingga daftar akun kosong dan periode belum
dibangkitkan. Tanpa akun, form ini tidak punya pilihan untuk diisi.

### Skrip uji untuk owner

Prasyarat: **daftar akun sudah terisi** dan **periode sudah dibangkitkan**.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | **Akuntansi → Jurnal → + Tambah Jurnal** | Form terbuka dengan dua baris kosong |
| 2 | Buka pilihan **Akun** pada baris 1 | Hanya akun paling bawah yang muncul — akun induk **tidak boleh** ada |
| 3 | Isi debit `1.000.000` pada baris 1, kredit `900.000` pada baris 2 | Selisih `Rp 100.000` tampil merah; **Simpan dan Ajukan mati** |
| 4 | Tekan **Simpan Draft** | Berhasil. Ini inti acceptance (1) — belum seimbang tetap boleh disimpan |
| 5 | Ubah kredit baris 2 menjadi `1.000.000` | Pesan berubah hijau; **Simpan dan Ajukan hidup** |
| 6 | Pilih akun **beban** pada salah satu baris | Kolom unit biaya berubah menjadi *Wajib dipilih*; menyimpan tanpa mengisinya ditolak |
| 7 | **Acceptance (5).** Tekan Batal, lalu buka lagi draft tadi dari daftar | Isian kembali seperti saat disimpan |
| 8 | Tekan **+ Tambah Baris** tiga kali, lalu **Hapus** | Baris bertambah dan berkurang; tombol Hapus mati saat tersisa dua baris |
| 9 | **Acceptance (4).** Lebarkan jendela penuh | Tujuh kolom muat tanpa gulir mendatar |
| 10 | Tekan **Simpan dan Ajukan** | Jurnal berpindah ke status *Menunggu Persetujuan*, layar kembali ke daftar |

## 8. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| **Isian yang belum pernah disimpan hilang saat layar ditutup** | **Sedang** | Acceptance (5) hanya menuntut draft **yang sudah tersimpan** tidak hilang, dan itu terpenuhi. Menyimpan otomatis ke penyimpanan peramban adalah keputusan tersendiri yang belum diminta — **butuh putusan owner** |
| `UAT-02` dan `UAT-04` belum dapat dijalankan | **Tinggi** | Bukan cacat task ini. `BLK-ACC-02`: daftar akun kosong dan periode belum dibangkitkan |
| Gulir mendatar di bawah 992px | Rendah | Disengaja. Tujuh kolom tidak muat di ponsel; memaksakannya membuat isian terlalu sempit untuk diketik |
| Nol test otomatis untuk layar ini | Rendah | Repository belum punya pola test komponen React; 434 test yang ada seluruhnya menguji helper murni. Menambah kerangkanya adalah keputusan tersendiri |
| Validasi sembilan syarat hanya ditegakkan backend | Rendah | Disengaja. Frontend hanya menjaga keseimbangan dan unit biaya; sisanya milik `AccJournalService.PeriksaSembilanSyaratAsync` |

## 9. Langkah berikutnya

`FE-ACC-007` — rincian jurnal dan tombol aksi. Kelima thunk alur kerja sudah tersedia di
`accounting-journal-slice.jsx`, dan `AvailableActions` sudah ada pada `JournalDetailResponse`.
Perhatikan `ACC-GAP-011`: nama penyetuju **tidak tersedia** dari endpoint ini.

---

## 10. Perbaikan sesudah temuan owner — 4 September 2026

Owner menguji layar ini dan menemukan **dua cacat**, keduanya milik implementasi awal task ini.
Diperbaiki pada baseline yang sama.

### 10.1 Dropdown tenggelam di dalam kartu `DIPERBAIKI`

**Gejala.** Membuka kotak pilih **Unit Biaya** pada baris jurnal membuat daftarnya terpotong tepi
kartu *Baris Jurnal*. Pilihan yang berada di bawah garis potong tidak dapat dilihat maupun ditekan.

**Sebab.** `.lineTableWrapper` diberi `overflow-x: auto` untuk mengantisipasi tabel lebar. Menurut
spesifikasi CSS, begitu satu sumbu bukan `visible`, **sumbu satunya ikut dihitung `auto`**.
Pembungkusnya karena itu menjadi wadah gulir dua arah, dan menu `FilterSelect` yang diposisikan
`absolute` terpotong olehnya. Bukan cacat `FilterSelect` — ia dipakai benar di 14 tempat lain.

**Perbaikan.** `overflow` dicabut sama sekali. Kebutuhan yang dulu dijawab gulir mendatar kini
dijawab tata letak: pada layar di bawah 992px, tiap baris berubah menjadi **kartu bertumpuk**,
sehingga tabel tidak pernah lagi perlu digulir mendatar. Cacat ini karena itu hilang di **semua**
lebar layar, bukan hanya di desktop.

Efek sampingnya positif: acceptance (4) — *baris muat tanpa gulir mendatar* — yang semula hanya
`TERBUKTI SEBAGIAN` kini terpenuhi pada seluruh lebar, bukan cuma desktop.

### 10.2 Label kolom menampilkan jalur field mentah `DIPERBAIKI`

**Gejala.** Di atas tiap isian baris tertulis `lines.1.accountId`, `lines.1.description`,
`lines.1.debitAmount`, `lines.1.creditAmount` — jalur internal `react-hook-form`, bukan kata yang
berarti bagi petugas.

**Sebab.** Implementasi awal mengirim `label: ""` dengan anggapan label akan disembunyikan.
`useFieldIdentity` pada `base-form-control.jsx:70` berbunyi
`sanitizeDisplayText(field.label, name)` — label kosong **jatuh balik ke nama field**, dan nama
field pada `useFieldArray` memang berbentuk `lines.<index>.<field>`.

**Perbaikan.** Kelima sel diberi label Bahasa Indonesia sungguhan — Akun, Unit Biaya, Keterangan,
Debit, Kredit. Di desktop label itu **disembunyikan lewat CSS, bukan dikosongkan**, karena kepala
tabel sudah menjelaskan kolomnya; ia tetap ada di DOM sehingga `htmlFor` dan pembaca layar tetap
bekerja. Di layar sempit label itu muncul, karena di sana justru kepala tabelnya yang disembunyikan.

### 10.3 Validasi ulang

| Perintah | Hasil |
|---|---|
| `npx eslint --quiet` | **PASS**, exit 0 |
| Unit test 434 | **PASS**, `# pass 434`, `# fail 0` |
| `npm run build` | **PASS**, `Compiled successfully` |
| `label: ""` tersisa | **0** |
| Aturan `overflow-x` tersisa | **0** — satu-satunya kemunculan tinggal di komentar penjelas |

### 10.4 Cakupan pemeriksaan

Pola yang sama dicari di seluruh style Accounting: **hanya layar ini** yang memiliki wadah gulir.
`chart-of-account-view.module.css` hanya memakai `overflow-wrap` untuk pemenggalan teks, yang tidak
memotong apa pun.

Pemeriksaan menyeluruh ke luar modul Accounting **belum dijalankan** — sebagian besar layar lain
menaruh `FilterSelect` di panel penyaring, bukan di dalam sel tabel, sehingga tidak berisiko sama.
Satu kandidat yang bentuknya mirip, `prescription-compound-editor.jsx`, belum diperiksa dan berada
di luar cakupan task ini.

---

## 11. Verifikasi owner di peramban — 4 September 2026

**Jurnal pertama modul ini berhasil disusun dan diajukan lewat layar.** Bukti pada database
sungguhan `QuilvianNewDevRizki`:

| Bukti | Nilai |
|---|---|
| Nomor jurnal | `JB/2026/09/00001` — penomoran `BE-ACC-010` terbukti pada data nyata |
| Keterangan | `testing` |
| Debit / Kredit | `1000000.00` / `1000000.00`, seimbang |
| `AccJournalLine` | **2 baris** |
| `AccJournalApproval` | **1 baris** — jejak audit terbentuk |
| Status | `Menunggu Persetujuan` |
| Diajukan | 2026-09-04 08:36:51 |
| Disetujui / Disahkan | **`None` / `None`** |

Acceptance (1) **TERBUKTI**: owner melihat tombol Ajukan mati saat selisih belum nol, lalu hidup
saat selisih `Rp 0`. Acceptance (4) **TERBUKTI**: tujuh kolom muat tanpa gulir mendatar.

Acceptance (3) dan (5) **belum teruji** — COA belum memuat akun beban, sehingga kewajiban unit
biaya tidak pernah terpicu; dan draft belum pernah ditutup lalu dibuka ulang.

**Setujui dan sahkan belum terjadi, dan itu BUKAN cacat**: layar aksinya adalah `FE-ACC-007`, yang
belum dibangun. Tidak ada tombol Setujui/Sahkan di layar mana pun saat ini.

### Empat cacat ditemukan owner, seluruhnya sudah diperbaiki

| # | Gejala | Sebab | Commit |
|---|---|---|---|
| 1 | Dropdown unit biaya terpotong tepi kartu | `overflow-x: auto` menjadikan pembungkus wadah gulir dua arah | `bf4fd0ed6` |
| 2 | Label berbunyi `lines.1.accountId` | `label: ""` jatuh balik ke nama field (`base-form-control.jsx:70`) | `bf4fd0ed6` |
| 3 | **Total debit/kredit beku di `Rp 0`** | `watch("lines")` mengembalikan referensi array yang sama; `useMemo` tidak pernah menghitung ulang. Diganti `useWatch` | `a57074f3d` |
| 4 | Tombol simpan tertutup footer aplikasi | Layar tidak memakai token `--app-footer-safe-space` | `a57074f3d` |

Cacat 3 yang paling berat: ia membuat tombol Ajukan **tidak akan pernah hidup** berapa pun angka
yang diketik, dan kewajiban unit biaya ikut beku. Tidak dilaporkan owner — ditemukan dari gambar
layar yang dikirimnya.

Ditambah dua penyempurnaan menyusul:

| Perubahan | Sebab | Commit |
|---|---|---|
| Isian nominal berformat `Rp 10.000` | Permintaan owner. Memakai `applyFieldNormalizer(..., currencyIdr)`, helper yang sudah ada — nilai tersimpan tetap `"10000"` supaya total tidak rusak | `63f660b0b` |
| Simpan yang ditolak validasi kini bersuara | `handleSubmit` dipanggil tanpa `onInvalid`, dan fokus otomatis RHF tidak bekerja karena `BaseTextField` tidak meneruskan `field.ref`. Owner melaporkannya sebagai "tombol simpan tidak ada aksi" | `418aebb05` |

### Catatan domain untuk owner

Jurnal pertama ini memakai jenis **Jurnal Pembalik** (`JB`), bukan Jurnal Umum. Backend
menerimanya — jadi kekhawatiran bahwa pembalik akan ditolak tanpa jurnal asal **tidak terbukti**.
Secara akuntansi ini janggal: jenis pembalik dimaksudkan untuk mengoreksi jurnal yang sudah
disahkan, bukan untuk pencatatan pertama. Bukan cacat kode, tetapi layak diputuskan owner apakah
backend perlu menolak `JB` yang tidak menunjuk jurnal asal.
