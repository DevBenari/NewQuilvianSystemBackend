# Laporan Perubahan Frontend — `FE-ACC-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-010` |
| Judul | Pembalikan dan penyesuaian di layar |
| Slice | `MVP-3` — Koreksi dan saldo awal |
| Roadmap | [`../../../roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), bagian `MVP-3`, kartu `FE-ACC-010` |
| Trace | `ACC-DEC-017`, `ACC-DEC-029`; `FR-ACC-040` sampai `FR-ACC-043`; `EPIC ACC-06`; `UAT-10`, `UAT-11`, `UAT-12` |
| Contract version | `ACC-API-0.5` endpoint `POST /journals/{id}/reverse`; `ACC-VALIDATION-0.3` bagian 5 — keduanya `approved`, dan keduanya diverifikasi ulang terhadap source `0614be7` |
| Wewenang UI | Dialog koreksi pada layar rincian jurnal yang sudah ada. Tidak ada halaman baru |
| Dependency | `FE-ACC-007` `IMPLEMENTED`, `BE-ACC-013` `DONE`. Keduanya lunas |
| Klasifikasi | `MEDIUM` — 6 berkas disunting, 3 berkas baru (665 baris), nol halaman baru, nol slice baru, nol thunk baru, nol komponen base baru, nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; berkas laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` di repository backend |
| Model | `claude-opus-5` |
| Commit frontend saat dikerjakan | `5174e0cf1` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `0614be7` (branch `rizkiG`), read-only |
| Tanggal | 7 September 2026 |
| Status | **`IMPLEMENTED`** — keempat acceptance ada source-nya dan terkunci 9 unit test baru; `lint:errors` PASS, **481 unit test PASS**, `build` PASS 0 warning. Skenario `UAT-10`, `UAT-11`, `UAT-12` di peramban **menunggu owner** — sesi ini tidak memiliki kredensial |

---

## Ringkasan untuk pembaca umum

Jurnal yang sudah **disahkan** tidak boleh diubah dan tidak boleh dihapus — itu inti pembukuan yang
dapat diaudit. Tetapi kesalahan tetap terjadi, dan sistem harus punya jalan memperbaikinya tanpa
menghapus jejak. Jalan itu adalah **jurnal koreksi**: jurnal baru yang menetralkan kesalahan,
sementara jurnal aslinya tetap berdiri apa adanya.

Ada dua cara, dan memilih yang salah berakibat nyata:

- **Pembalikan Penuh** — seluruh isi jurnal dibatalkan. Dipakai ketika jurnalnya salah seluruhnya
  atau memang seharusnya tidak pernah ada. Sistem menyusun sendiri baris pembaliknya.
- **Jurnal Penyesuaian** — hanya selisihnya yang dicatat. Dipakai ketika sebagian besar jurnalnya
  sudah benar dan yang meleset hanya nilai atau akun tertentu. Baris selisihnya disusun petugas.

Sebelum task ini, layar **tidak memberi pilihan sama sekali**. Tombol Balik selalu mengirim
pembalikan penuh, karena layar pemilihannya memang belum dibangun. Task ini menggantinya dengan
dialog yang menanyakan cara koreksinya lebih dahulu, lengkap dengan penjelasan kapan memakai yang
mana, isian alasan yang wajib, dan — bila yang dipilih penyesuaian — tabel baris selisih yang harus
seimbang sebelum tombol kirim menyala.

Sesudah berhasil, layar menampilkan kartu berisi nomor jurnal koreksi yang baru beserta tombol
untuk membukanya, dan menyebut dua hal yang paling mudah disalahpahami: jurnal koreksi itu **masih
menunggu persetujuan**, dan jurnal yang sedang dibuka **tetap berstatus Disahkan**.

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Ganjalan yang memang ditinggalkan `FE-ACC-007` untuk task ini

Roadmap menyebutnya eksplisit pada baris status `FE-ACC-010`: *"`FE-ACC-007` menampilkan tombol
Balik yang selalu mengirim `CorrectionType: FullReversal` karena field itu `[Required]` sementara
pemilihannya adalah cakupan task ini."*

Bentuknya di source, pada `journal-constants.jsx` sebelum task ini:

```js
reverse: Object.freeze({
  message:
    "Layar ini membuat PEMBALIKAN PENUH: ... Pilihan jurnal " +
    "penyesuaian belum tersedia di sini. Alasan wajib diisi.",
  body: Object.freeze({ correctionType: 1 }),
}),
```

Ganjalan itu jujur — ia mengatakan apa adanya pada kalimat konfirmasi, bukan memilihkan diam-diam.
Tetapi akibatnya tetap nyata: **jurnal yang hanya salah nominal pun dibalik seluruhnya**, lalu
petugas harus menyusun ulang jurnalnya dari nol.

### 1.2 Kontrak backend sudah menyediakan seluruhnya

Diperiksa pada `0614be7`. `ReverseJournalRequest` membawa empat field, dan keempatnya memang
dibutuhkan keempat acceptance:

| Field | Sifat | Dipakai untuk |
| --- | --- | --- |
| `CorrectionType` | `[Required]`, `1` `FullReversal` / `2` `Adjustment` | Acceptance (1) |
| `Reason` | `[Required]`, maks 500 | Acceptance (2) |
| `AccountingDate` | opsional, kosong berarti mengikuti tanggal jurnal asal | Tidak dipakai layar — lihat bagian 3.4 |
| `AdjustmentLines` | wajib dan harus seimbang bila `Adjustment`, diabaikan pada pembalikan penuh | Acceptance (3) |

Respons `reverse` mengembalikan `JournalDetailResponse` **milik jurnal koreksi yang baru**, lengkap
dengan `Id` dan `JournalNumber` — itulah yang memungkinkan acceptance (4). Diperiksa pada
`AccJournalService.ReverseAsync`:

```csharp
return await MuatDanPetakanAsync(koreksiJurnal.Id, ...);
```

Dan jurnal asal memang tidak disentuh statusnya: service hanya menulis baris riwayat
`JournalApprovalAction.Reversed` padanya. Tidak ada satu pun baris yang mengubah
`asal.JournalStatus`.

### 1.3 Impact scan — backend bergerak 98 commit, Accounting nol berkas

Saat task ini dikerjakan, backend berpindah dari `f989453` ke `0614be7`, yaitu merge dari
`QuilvianIntegrationBackend` (PR #99). Karena merge integration adalah kejadian yang pernah
menghilangkan blok modul lain, delta-nya diperiksa lebih dahulu:

| Yang diperiksa | Hasil |
| --- | --- |
| Jumlah commit `f989453..0614be7` | **98** |
| Berkas berubah di `Areas/Corporate/AccountingManagement/` | **NOL** |
| Berkas berubah di `JournalManagement/` | **NOL** — `ReverseJournalRequest` dan `ReverseAsync` identik |
| Berkas berubah di `Migrations/` dan `ModelSnapshot` | 63 — **seluruhnya milik modul lain**, di luar cakupan task frontend |

Kontrak yang menjadi dasar implementasi karena itu **tidak bergeser sedikit pun** oleh merge
tersebut. Frontend juga berpindah `bcccb67bc` → `5174e0cf1`: dua commit owner, yaitu perbaikan
neraca saldo dan commit `FE-ACC-011` yang dikerjakan sesi sebelumnya. Keduanya Accounting, nol
berkas asing.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya** pemegang hak `Journal : Reverse` — dalam praktik Accounting Manager.
**Kapan dibuka:** ketika jurnal yang sudah disahkan diketahui salah.

### 2.1 Alur normal — pembalikan penuh (`UAT-10`)

1. Buka **Akuntansi → Daftar Jurnal**, klik dua kali jurnal berstatus **Disahkan**.
2. Tombol **Balik** muncul. Ia muncul karena backend mengirim `reverse` pada `AvailableActions` —
   layar tidak menghitung sendiri siapa yang boleh membalik.
3. Tekan **Balik**. Dialog koreksi terbuka. Isinya, berurutan: kalimat pembuka, dua kartu pilihan
   cara koreksi, kotak peringatan, dan isian alasan.
4. Tombol **Ya, Buat Jurnal Koreksi** **mati**, dan kotak peringatan berbunyi *"Pilih cara koreksi
   lebih dahulu."*
5. Pilih kartu **Pembalikan Penuh**. Kartu tersorot; tabel baris tidak muncul — memang tidak
   diperlukan, karena barisnya diturunkan backend dari jurnal asal.
6. Isi **Alasan Koreksi**. Tombol kirim menyala begitu alasan terisi.
7. Tekan kirim. Dialog menutup, toast **Jurnal Koreksi Dibuat** muncul, dan rincian dimuat ulang
   dari backend.
8. Kartu hasil muncul di atas Baris Jurnal: nomor jurnal koreksi berawalan **`JB/`**, tombol
   **Buka Jurnal Koreksi**, dan keterangan bahwa jurnal koreksi masih menunggu persetujuan
   sementara jurnal ini tetap Disahkan.
9. Baris **Dibalik** bertambah pada Riwayat Persetujuan jurnal asal, beserta alasannya.

### 2.2 Alur normal — jurnal penyesuaian (`UAT-11`)

Langkah 1 sampai 4 sama. Sesudah memilih kartu **Jurnal Penyesuaian**:

5. Tabel **Baris Selisih** muncul, berisi dua baris kosong, beserta tombol **+ Tambah Baris**.
   Barisnya **baris jurnal yang sama persis** dengan yang dipakai form jurnal `FE-ACC-006` —
   kotak pilih akun yang dapat dicari, unit biaya, keterangan, serta isian debit dan kredit
   ber-format Rupiah.
6. Di bawah tabel ada ringkasan **Total Debit**, **Total Kredit**, dan **Selisih**.
7. Selama selisihnya belum nol, kotak peringatan berbunyi *"Baris selisih belum seimbang..."* dan
   tombol kirim tetap mati. Bila ada baris yang belum memilih akun, kalimatnya berbeda: *"Setiap
   baris selisih harus memilih akun."*
8. Begitu seimbang, kotak berubah hijau *"Baris selisih sudah seimbang."* dan tombol menyala.
9. Sesudah kirim, nomor jurnal koreksi berawalan **`JP/`**.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Jurnal belum disahkan | Tombol **Balik** tidak muncul sama sekali — backend tidak mengirim `reverse` pada `AvailableActions` |
| Jurnal sudah pernah dibalik | Backend menolak `409` *"Jurnal ini sudah pernah dibalik dengan jurnal {nomor}."* Pesannya ditampilkan apa adanya pada toast dan `errorMessage`, lengkap dengan nomor pembaliknya |
| Alasan dikosongkan | Tombol kirim mati. Dijaga `requireReason` bawaan `ConfirmModal`, dan ditegakkan ulang backend `400` |
| Penyesuaian dikirim timpang | Tidak dapat terkirim dari layar. Seandainya lolos, backend menolak `400` beserta angka selisihnya |
| Periode tujuan menolak jurnal pembalik | Backend menolak `422` beserta nama periodenya. Ditampilkan apa adanya |
| Daftar akun gagal dimuat | Kotak pilih akun kosong, sehingga syarat "setiap baris memilih akun" tidak terpenuhi dan tombol tetap mati — layar tidak mengirim baris tanpa akun |
| Dialog ditutup lalu dibuka lagi | Seluruh isian kembali bersih: cara koreksi, baris selisih, dan alasan. Tidak ada sisa pilihan dari percobaan sebelumnya |
| Tanpa hak akses | `AccessDeniedGate` menahan layar seperti sebelumnya. Task ini tidak menyentuh otorisasi |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Frontend** (`5174e0cf1`): `journal-detail-view.jsx`, `use-journal-detail.jsx`,
`journal-form-view.jsx`, `use-journal-editor.jsx`, `journal-constants.jsx`,
`accounting-journal-slice.jsx`, `accounting-chart-of-account-slice.jsx`,
`master-data-resource-slice-factory.jsx`, `confirm-modal.jsx`, `base-detail-view.jsx`,
`base-checkbox-card.jsx`, `summary-grid.jsx`, `information-alert.jsx`,
`inpatient-admission-payment-step.jsx` (pola kartu pilihan), `private-route-token-utils.jsx`,
`globals.css` (kontrak `data-flat-table`, **dibaca saja**), `journal-detail-view.module.css`,
`journal-form-view.module.css`, `tests/unit/accounting-journal-detail.test.mjs`.

**Backend** (`0614be7`, **read-only**): `JournalDtos.cs`, `JournalController.cs`,
`AccJournalService.cs` bagian `ReverseAsync`.

**Dokumen:** roadmap frontend, `contracts/api-contract.md`, `contracts/validation-matrix.md`
bagian 5, `requirement-traceability.md`, `blueprint-manifest.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/accounting/journal/use-journal-reversal.jsx` | **Baru, 235 baris.** Keadaan dialog: cara koreksi terpilih, baris selisih lewat `react-hook-form`, pilihan akun, total/selisih dalam sen, syarat kirim, dan penyusun payload. **Tidak memanggil API sama sekali** |
| `src/components/view/corporate/accounting/journal/detail/journal-reversal-dialog.jsx` | **Baru, 207 baris.** Rangkaian `ConfirmModal` + `BaseCheckboxCard` + `JournalLineRow` + `SummaryGrid` + `InformationAlert`. Nol komponen base baru |
| `tests/unit/accounting-journal-reversal.test.mjs` | **Baru, 223 baris, 9 uji.** Mengunci keempat acceptance beserta jebakan task |
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | **+127 baris.** `JOURNAL_CORRECTION_CHOICES` (kartu pilihan beserta penjelasannya), `JOURNAL_REVERSAL_DIALOG` (salinan teks), `JOURNAL_CORRECTION_FULL_REVERSAL`/`_ADJUSTMENT`. Deskriptor `reverse` diganti: `body: { correctionType: 1 }` **dicabut**, `usesOwnDialog: true` ditambahkan |
| `src/lib/hooks/corporate/accounting/journal/use-journal-detail.jsx` | **+105 baris.** Memanggil `useJournalReversal`, memisahkan dua slot dialog, menyusun payload koreksi dari dialog, dan menangkap jurnal koreksi baru dari respons `reverse` |
| `src/components/view/corporate/accounting/journal/detail/journal-detail-view.jsx` | **+65 baris.** Merender dialog koreksi dan kartu hasil beserta tombol pembukanya |
| `src/style/corporate/accounting/journal-detail-view.module.css` | **+70 baris.** Tata letak isi dialog dan kartu hasil. Seluruhnya token `--base-*`/`--app-*` |
| `src/components/view/corporate/accounting/journal/form/journal-form-view.jsx` | **+6 baris.** `JournalLineRow` dijadikan named export beserta alasannya. **Isi komponennya tidak berubah satu baris pun** |
| `tests/unit/accounting-journal-detail.test.mjs` | **+26 baris.** Uji yang mengunci ganjalan `FE-ACC-007` diganti — lihat bagian 3.5 |

### 3.3 Gerbang pemakaian ulang komponen

`UI GATE: 5 elemen — REUSE 4, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kerangka dialog koreksi | `ConfirmModal` | `confirm-modal.jsx` sudah mendukung `children`, `size`, `requireReason`, dan `confirmProps.disabled` | `REUSE` | Dipakai apa adanya; nol prop baru ditambahkan padanya |
| Pilihan cara koreksi beserta penjelasannya | `BaseCheckboxCard` | `base-checkbox-card.jsx`; pola radio persis `inpatient-admission-payment-step.jsx:457` — `role="radio"`, `aria-checked`, `checked` dari nilai terpilih | `REUSE` | Mengikuti pola yang sudah ada, bukan membuat kelompok pilihan baru |
| Baris selisih penyesuaian | `JournalLineRow` (`FE-ACC-006`) | `journal-form-view.jsx`; perlu named export — aditif | `REUSE` | Diimpor apa adanya supaya baris selisih berperilaku identik dengan baris form jurnal |
| Ringkasan total dan selisih | `SummaryGrid` | `summary-grid.jsx`, dipakai `journal-form-view.jsx:391` | `REUSE` | Dipakai apa adanya |
| Kartu hasil beserta tautan jurnal koreksi | `InformationAlert` + `BaseButton` + `StatusBadge` | ketiganya base component yang sudah ada | `COMPOSE` | Dirangkai di layer view, tanpa komponen baru |

**Keputusan yang perlu diketahui: kartu hasil koreksi (satu-satunya elemen bukan `REUSE`).**

- **A. Compose `InformationAlert` + `BaseButton` di dalam kartu `styles.card` yang sudah dipakai
  layar ini — dipilih.** Bentuknya sama dengan kartu Baris Jurnal dan Riwayat Persetujuan di
  bawahnya, sehingga layar tetap terbaca sebagai satu halaman. Nol komponen baru, nol perubahan
  base, dan letaknya sebagai kartu membuatnya aman dari footer `position: fixed`.
- **B. Toast saja, tanpa kartu.** Paling murah, tetapi toast menghilang sendiri — dan bersamanya
  hilang pula satu-satunya tempat nomor jurnal koreksi pernah muncul. Acceptance (4) meminta
  layar **menampilkan tautan**, bukan menampilkannya sekejap.
- **C. Komponen `ResultPanel` baru pada base-features.** Paling rapi bila pola ini terulang di
  modul lain, tetapi sampai hari ini ia hanya dibutuhkan satu layar. Membuat base component untuk
  satu pemakai adalah abstraksi yang belum berbukti, dan `AGENTS.md` melarangnya tanpa permintaan
  eksplisit.

Opsi A yang dikerjakan. Nol elemen berstatus `NEW`, dan nol `EXTEND` yang mengubah perilaku
default base component — sehingga tidak ada butir yang perlu menunggu keputusan pengguna.

### 3.4 Keputusan teknis yang perlu diketahui pembaca berikutnya

**Dialog koreksi TIDAK menumpang slot `deleteConfirm` milik `BaseDetailView`.**

Keempat tindakan lain — Ajukan, Setujui, Tolak, Sahkan — tetap memakai slot itu apa adanya. Balik
tidak bisa, dan alasannya ada di source: `base-detail-view.jsx` meneruskan `title`, `message`,
`variant`, `requireReason`, dan beberapa lagi ke `ConfirmModal`, tetapi **tidak meneruskan
`children`, `size`, maupun `confirmProps`** — padahal ketiganya justru yang dibutuhkan dialog ini.

Ada dua jalan, dan yang dipilih adalah yang tidak menyentuh milik modul lain:

| Jalan | Konsekuensi |
| --- | --- |
| Menambah ketiga prop itu ke `BaseDetailView` | Mengubah komponen base yang dipakai belasan modul. Risiko regresi ada di luar Accounting, dan pemilik abstraksinya bukan Accounting |
| **Merangkai `ConfirmModal` langsung di `journal-detail-view.jsx`** | Hasilnya sama, risikonya nol ke modul lain, dan `ConfirmModal` memang sudah mendukung ketiganya tanpa perubahan apa pun |

Penandanya satu field, `usesOwnDialog: true`, dan hook rincian memakainya untuk memastikan kedua
dialog **tidak pernah terbuka bersamaan** — tanpa itu keduanya akan bertumpuk.

**Keseimbangan dihitung dalam sen, bukan dalam pecahan desimal.**

`0.1 + 0.2 !== 0.3` pada bilangan pecahan biner. Membandingkan total debit dan kredit secara
langsung karena itu dapat menyatakan dua angka yang sebenarnya sama sebagai tidak seimbang, lalu
menahan kiriman yang sah. `toCents` mengubah keduanya menjadi bilangan bulat lebih dahulu — pola
yang sama dengan `use-journal-editor.jsx`. Uji `keseimbangan baris selisih dihitung dalam sen`
menahannya.

**Cara koreksi sengaja tidak punya nilai awal.**

Memilihkan "pembalikan penuh" di depan berarti mengulang cacat yang justru dicabut task ini:
petugas yang menekan kirim tanpa membaca akan mengirim cara koreksi yang tidak pernah ia pilih.
Tombol kirim mati sampai ia benar-benar memilih, dan kotak peringatan mengatakan apa yang kurang.

**`AccountingDate` tidak dipakai layar, dan itu disengaja.**

Field itu opsional; kosong berarti backend mengikuti tanggal jurnal asal. Menawarkan isian tanggal
di dialog berarti membuka kemungkinan jurnal koreksi jatuh ke periode yang berbeda dari jurnal
asalnya — keputusan bisnis yang tidak ada pada `ACC-DEC-017` maupun `ACC-DEC-029`, dan tidak
disebut cakupan task. Delta terhadap kontrak dicatat di sini, bukan diputuskan sendiri.

**Tautan jurnal koreksi dibaca dari respons `reverse`, bukan dari pemuatan ulang.**

Sesudah tiap tindakan, layar memuat ulang rincian dari backend — dan yang dimuat ulang adalah
jurnal **asal**. Jurnal asal tidak memuat nomor pembaliknya (`ReversalOfJournalNumber` ada pada
jurnal koreksi, menunjuk ke asalnya, bukan sebaliknya). Satu-satunya tempat nomor jurnal koreksi
tersedia adalah respons tindakannya sendiri, jadi di situlah ia diambil. Uji
`tautan jurnal koreksi diambil dari respons reverse, bukan pemuatan ulang` menahannya.

Tautannya memakai token privat, pola yang sama dengan pembukaan rincian dari daftar: nomor maupun
`Id` jurnal tidak pernah muncul di bilah alamat.

### 3.5 Uji lama yang ikut berubah, beserta alasannya

`tests/unit/accounting-journal-detail.test.mjs` memuat uji yang **mengunci ganjalan `FE-ACC-007`**:

```js
test("Balik mengirim CorrectionType FullReversal", () => {
  assert.deepEqual(JOURNAL_ACTION_BUTTONS.reverse.body, { correctionType: 1 });
  assert.match(JOURNAL_ACTION_BUTTONS.reverse.message, /PEMBALIKAN PENUH/);
});
```

Uji itu benar pada masanya, dan **wajib berubah** sekarang: ganjalan yang dikuncinya adalah persis
yang diperintahkan roadmap untuk dicabut. Menyimpannya berarti menahan task ini gagal atas
perilaku yang memang sudah tidak dikehendaki.

Penggantinya membalik arah penjagaan — ia menahan agar ganjalan itu **tidak kembali**:

```js
test("Balik tidak lagi memilihkan cara koreksi di belakang pengguna", () => {
  assert.equal(JOURNAL_ACTION_BUTTONS.reverse.body, undefined);
  assert.equal(JOURNAL_ACTION_BUTTONS.reverse.usesOwnDialog, true);
  assert.doesNotMatch(JOURNAL_ACTION_BUTTONS.reverse.message, /belum tersedia/i);
});
```

Sebelas uji lain pada berkas yang sama **tidak disentuh**, termasuk uji regresi tata letak footer
dan uji yang menahan hook rincian agar tidak menghitung kewenangan sendiri.

### 3.6 Kepatuhan arsitektur frontend

| Ketentuan | Penerapan |
| --- | --- |
| Alur dependensi | `constants → hook → view`. `use-journal-reversal.jsx` tidak mengimpor komponen; dialognya tidak mengimpor slice |
| Penempatan folder | Hook di `lib/hooks/corporate/accounting/journal/`, dialog di `components/view/corporate/accounting/journal/detail/` — bersebelahan dengan layar yang memakainya |
| Redux | **Nol slice baru, nol thunk baru.** `reverseJournal` yang sudah ada sejak `FE-ACC-007` menerima badan permintaan apa adanya (`{ id, ...body }`), sehingga `adjustmentLines` lewat tanpa satu baris pun berubah di slice |
| Axios | Nol instance baru, nol panggilan langsung dari view |
| Aturan bisnis tidak dipindah ke frontend | Kapan sebuah jurnal boleh dibalik tetap milik `AvailableActions`. Syarat kirim di layar hanya mendahului aturan yang tetap ditegakkan backend, dan uji menahan agar hook maupun dialog tidak mencabang atas status jurnal atau pembuatnya |
| Token | Seluruh nilai visual memakai variabel. Nol hex mentah, nol `!important`, nol inline style, `globals.css` tidak disentuh |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Selama koreksi berjalan, tombol **Balik** menampilkan pemuat, seluruh tombol tindakan mati, dan isi dialog — kartu pilihan, baris selisih, tombol tambah baris — ikut mati sehingga tidak dapat diubah di tengah pengiriman |
| Kosong | Tabel baris selisih lahir dengan dua baris kosong, bukan nol baris: penyesuaian paling sederhana pun butuh satu debit dan satu kredit. Baris tidak dapat dihapus sampai tersisa dua |
| Gagal | Pesan backend ditampilkan **apa adanya** pada toast sekaligus `errorMessage` — `409` sudah pernah dibalik beserta nomornya, `400` belum seimbang beserta selisihnya, `422` periode menolak beserta nama periodenya. Tidak ada kalimat karangan frontend |
| Tanpa hak akses | Tombol **Balik** tidak muncul karena backend tidak mengirim `reverse`. Bila hak dicabut di tengah jalan, backend tetap menolak saat tindakan dijalankan, dan pesannya ditampilkan |

---

## 5. Endpoint yang dikonsumsi

Task ini **tidak menambah satu pun endpoint baru**. Yang berubah hanya isi badan permintaan
`reverse`, yang sebelumnya selalu `{ correctionType: 1 }`.

#### Corporate / Accounting / Journal Management / Journal

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/accounting/journals/{id}/reverse` | Membuat jurnal pembalik atau jurnal penyesuaian. Badan: `correctionType`, `reason`, `adjustmentLines` | `Journal : Reverse` |
| `GET` | `/api/v1/corporate/accounting/journals/{id}` | Memuat ulang rincian jurnal asal sesudah koreksi berhasil | `Journal : Read` |

#### Corporate / Accounting / Master Data / Chart Of Account

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/master-data/chart-of-accounts/options?legalEntityId=...` | Mengisi kotak pilih akun pada baris selisih. **Hanya dipanggil ketika dialog benar-benar terbuka** | `ChartOfAccount : Read` |

---

## 6. Verifikasi

### 6.1 Perintah yang benar-benar dijalankan

| Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error | `PASS` | Keluaran kosong, exit 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | `# tests 481`, `# pass 481`, `# fail 0` | `PASS` | 472 sebelumnya + **9 baru** |
| `node --import ... --test tests/unit/accounting-journal-reversal.test.mjs` | `# tests 9`, `# pass 9`, `# fail 0` | `PASS` | Berkas uji baru berjalan sendiri |
| `npm run build` + `postbuild` | `Compiled successfully in 36.6s`, **0 warning, 0 error** | `PASS` | Log build, exit 0 |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`

`npm run test:unit` tetap gagal apa adanya pada Node 20 — glob `--test` baru ada di Node 21,
sedangkan lingkungan ini `v20.20.2`. Cacat tooling yang sudah tercatat sejak `FE-ACC-007`.

### 6.2 Grep anti-regresi konsistensi UI

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Warna literal pada CSS baru | Tiga kemunculan, **seluruhnya bentuk `var(--token, #fallback)`** — pola yang sudah dipakai seluruh CSS Accounting. Nol warna mentah |
| 2 | Typography yang menyasar komponen shared | **kosong** |
| 3 | `<button>` mentah atau class `btn-*` | **kosong** |
| 4 | `<table>` mentah | **satu**, dan sengaja dipertahankan — lihat di bawah |
| 5 | Utility typography Bootstrap | **kosong** |
| 6 | `!important` baru | **kosong** |
| 7 | `style={{ }}` inline | **kosong** |
| 8 | `globals.css` tersentuh | **tidak** |

**Butir 4 dipertahankan beserta alasannya, sesuai ketentuan checklist.** Tabel baris selisih adalah
`<table>` biasa tanpa `data-flat-table="true"`. Alasannya dua:

1. Ia memakai `formStyles.lineTable` yang **sama persis** dengan tabel baris pada form jurnal
   `FE-ACC-006`. Tabel itu juga tanpa `data-flat-table`, jadi menambahkannya hanya di sini akan
   membuat dua tabel yang berbagi satu berkas CSS tampil berbeda — layar yang sudah diverifikasi
   owner ikut terlihat tidak konsisten.
2. Kontrak `data-flat-table` pada `globals.css` menyeragamkan tipografi **tabel data**, dan
   komentarnya sendiri menyebut komponen interaktif mempertahankan tipografinya. Tabel ini bukan
   tabel data: tiap selnya berisi kotak pilih dan isian teks.

Ketiadaan `data-flat-table` pada tabel `FE-ACC-006` dan `FE-ACC-007` adalah utang teknis yang sudah
ada sebelum task ini. Ia dilaporkan, tidak diperbaiki di sini — memperbaikinya mengubah tampilan
dua layar yang tidak diminta task ini.

### 6.3 Uji yang mengunci keempat acceptance dan jebakan task

| Uji | Yang ditahannya |
| --- | --- |
| `kedua cara koreksi memakai nilai enum backend apa adanya` | Kartu pilihan tidak boleh punya daftar enum sendiri yang dapat menyimpang dari `JournalCorrectionType` |
| `tiap cara koreksi membawa penjelasan kapan ia dipakai` | Acceptance (1) — kartu tanpa penjelasan, atau berpenjelasan terlalu pendek, menggagalkan uji. Badge `JB`/`JP` ikut diperiksa |
| `salinan teks dialog menyebut alasan wajib dan syarat seimbang` | Acceptance (2), (3), dan (4) dari sisi kalimat yang benar-benar dibaca petugas |
| `hanya tindakan Balik yang punya dialog sendiri` | Menahan agar tindakan lain tidak ikut lepas dari `ConfirmModal` bawaan |
| `keseimbangan baris selisih dihitung dalam sen` | Menahan kembalinya perbandingan pecahan desimal |
| `dialog koreksi tidak menghitung kewenangan atau status sendiri` | **Jebakan utama** — hook dan dialog tidak boleh mencabang atas status jurnal atau pembuatnya |
| `tombol kirim dialog ditahan lewat confirmProps.disabled` | Acceptance (3), sekaligus menahan agar baris selisih tetap memakai `JournalLineRow` milik `FE-ACC-006`, bukan salinannya |
| `tautan jurnal koreksi diambil dari respons reverse` | Acceptance (4), termasuk larangan menaruh `Id` di bilah alamat |
| `payload koreksi disusun dialog, thunk reverse tidak berubah` | Menahan agar slice tetap bersih dari thunk baru |

### 6.4 Verifikasi lewat HTTP — jalur terbukti

Backend (`:7184`) berjalan saat task dikerjakan.

| Yang diuji | Hasil | Arti |
| --- | --- | --- |
| `POST /journals/{guid}/reverse` | **`401`** | Endpoint **ada**, dijaga autentikasi |
| `POST /journals/{guid}/jalur-ngawur` — **kontrol pembanding** | **`404`** | Membuktikan `401` di atas berarti "ada" |
| `GET /master-data/chart-of-accounts/options?legalEntityId=...` | **`401`** | Endpoint pilihan akun ada |
| `GET /master-data/chart-of-accounts/jalur-ngawur` — **kontrol** | **`404`** | Sama |

Jalur `reverse` pada slice karena itu **terbukti benar**, dan jalur pilihan akun yang menjadi
tumpuan baris selisih juga terbukti ada.

### 6.5 Uji manual — `NOT FEASIBLE` untuk isi layar

**`MANUAL TEST: NOT FEASIBLE`** — dua sebab, keduanya disebut apa adanya:

1. Sesi ini tidak memiliki kredensial pengguna, sedangkan seluruh endpoint berada di balik
   `[AccessPermission("Journal", ...)]`.
2. **Dev server frontend pada `:3000` berhenti di tengah pengerjaan.** Ia sempat menjawab `200`
   saat `FE-ACC-011`, lalu tidak lagi menerima koneksi sesudah `npm run build` dijalankan. `next
   build` dan `next dev` berbagi direktori `.next`, sehingga build produksi menimpa keadaan dev
   server yang sedang berjalan — itu penyebab yang paling mungkin. Prosesnya **tidak** dinyalakan
   ulang tanpa sepengetahuan pemilik pekerjaan.

Yang sudah terbukti tanpa peramban: jalur endpoint benar, build membentuk rutenya, keempat
acceptance ada source-nya, dan sembilan uji mengunci perilakunya.

### 6.6 Skrip uji untuk owner — `UAT-10`, `UAT-11`, `UAT-12`

Prasyarat: `JB/2026/09/00001` sudah **Disahkan** pada PT Metropolitan Medical Centre periode
`2026-09`, dengan akun `1002 Kas Besar` dan `4001 Pendapatan Rawat Jalan`.

Jalankan `npm run dev` lebih dahulu — dev server sedang tidak berjalan.

| # | Langkah | Hasil yang diharapkan |
| --- | --- | --- |
| 1 | Buka rincian sebuah jurnal berstatus **Disahkan** | Tombol **Balik** terlihat di baris tombol |
| 2 | Tekan **Balik** | Dialog terbuka, berukuran lebar. Ada dua kartu pilihan, kotak peringatan kuning *"Pilih cara koreksi lebih dahulu."*, dan isian **Alasan Koreksi** |
| 3 | Perhatikan tombol **Ya, Buat Jurnal Koreksi** | **Mati** |
| 4 | Baca kedua kartu | **Acceptance (1).** Kartu 1 *Pembalikan Penuh* berbadge **Jenis JB**, kartu 2 *Jurnal Penyesuaian* berbadge **Jenis JP**. Masing-masing punya kalimat kapan dipakai dan dua butir penjelasan |
| 5 | **`UAT-10`.** Pilih **Pembalikan Penuh** | Kartu tersorot. Tabel baris **tidak** muncul. Kotak peringatan berubah hijau |
| 6 | Biarkan alasan kosong | **Acceptance (2).** Tombol kirim tetap mati |
| 7 | Isi alasan, tekan kirim | Toast **Jurnal Koreksi Dibuat**. Dialog menutup |
| 8 | Perhatikan bagian atas isi halaman | **Acceptance (4).** Kartu hasil muncul: nomor jurnal berawalan **`JB/`**, tombol **Buka Jurnal Koreksi**, dan keterangan bahwa koreksinya menunggu persetujuan sementara jurnal ini tetap Disahkan |
| 9 | Periksa baris **Status** pada rincian | **Masih `Disahkan`** — tidak berubah |
| 10 | Periksa **Riwayat Persetujuan** | Bertambah baris **Dibalik** beserta alasan yang tadi diisi |
| 11 | Tekan **Buka Jurnal Koreksi** | Rincian jurnal `JB/...` terbuka, berstatus **Menunggu Persetujuan**, barisnya kebalikan jurnal asal. Bilah alamat **tidak** memuat nomor maupun id jurnal |
| 12 | Kembali ke jurnal asal, tekan **Balik** lagi | Backend menolak `409` *"Jurnal ini sudah pernah dibalik dengan jurnal JB/..."*. Pesannya tampil apa adanya |
| 13 | **`UAT-11`.** Buka jurnal **Disahkan lain**, tekan **Balik**, pilih **Jurnal Penyesuaian** | Tabel **Baris Selisih** muncul dengan dua baris kosong, tombol **+ Tambah Baris**, dan ringkasan Total Debit / Total Kredit / Selisih |
| 14 | Isi baris 1 akun `1002 Kas Besar` debit `50.000`, baris 2 kosongkan | **Acceptance (3).** Kotak kuning *"Setiap baris selisih harus memilih akun."*, tombol mati |
| 15 | Isi baris 2 akun `4001 Pendapatan Rawat Jalan` kredit `25.000` | Kotak kuning *"Baris selisih belum seimbang..."*, tombol tetap mati. Selisih **Rp 25.000** |
| 16 | Ubah kredit baris 2 menjadi `50.000` | Kotak hijau *"Baris selisih sudah seimbang."*, Selisih **Rp 0**, tombol menyala |
| 17 | Tekan **+ Tambah Baris** lalu **Hapus** pada baris ketiga | Baris bertambah dan berkurang. Tombol Hapus mati ketika tersisa dua baris |
| 18 | Isi alasan, kirim | Nomor jurnal koreksi berawalan **`JP/`**. Barisnya hanya baris selisih, bukan seluruh isi jurnal asal |
| 19 | **`UAT-12`.** Buka jurnal koreksi, setujui, lalu sahkan | Alurnya sama dengan jurnal lain. Sesudah disahkan, **Buku Besar** akun terkait menampilkan mutasi koreksinya |
| 20 | Tutup dialog di tengah pengisian, lalu buka lagi | Seluruh isian kembali bersih — cara koreksi, baris selisih, dan alasan |

**Tidak dijalankan:** `npm run test:e2e` dan `npm run test:uat` — tidak diminta task, dan
`AGENTS.md` melarang menjalankannya tanpa permintaan eksplisit.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kedua cara koreksi tersedia beserta penjelasan singkat kapan memakai yang mana | **Terpenuhi — terkunci uji** | `JOURNAL_CORRECTION_CHOICES` berisi dua kartu bernilai enum backend, masing-masing dengan `description` dan dua butir `bullets`, dirender `BaseCheckboxCard`. Dua uji memeriksa nilai dan kelengkapan penjelasannya. **Rupa di peramban menunggu langkah 4 owner** |
| (2) Alasan wajib diisi | **Terpenuhi — terbukti di source** | `requireReason` bawaan `ConfirmModal` mematikan tombol kirim selama alasan kosong; backend menegakkan ulang `400`. **Menunggu langkah 6 owner** |
| (3) Baris selisih pada penyesuaian harus seimbang sebelum dapat dikirim | **Terpenuhi — terkunci uji** | `canSubmit` menuntut setiap baris berakun dan `totals.isBalanced`, disalurkan lewat `confirmProps.disabled`. Perhitungannya dalam sen. Tiga uji menahannya. **Menunggu langkah 14–16 owner** |
| (4) Setelah berhasil, layar menampilkan tautan ke jurnal pembalik yang baru, dan jurnal asal tetap berstatus disahkan | **Terpenuhi — terkunci uji** | `reversalResult` diambil dari respons `reverse`, dirender sebagai kartu beserta tombol **Buka Jurnal Koreksi** bertoken privat. Jurnal asal tidak disentuh — `ReverseAsync` hanya menulis riwayat padanya. **Menunggu langkah 8–11 owner** |

**Definition of Done** roadmap: *"Layar berfungsi, laporan task tersedia."*

| Butir | Keadaan |
| --- | --- |
| Laporan task tersedia | **Terpenuhi** — berkas ini |
| Layar berfungsi | **Terpenuhi secara struktural**: lint PASS, 481 unit test PASS, build PASS 0 warning, endpoint `reverse` terbukti ada lewat `401` berkontrol `404`. **Belum terbukti secara visual** — `UAT-10`, `UAT-11`, dan `UAT-12` menunggu owner, dan dev server sedang tidak berjalan |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari lint maupun build |
| Masalah yang diketahui | (a) Tabel baris selisih tanpa `data-flat-table="true"` — dipertahankan beserta alasannya di bagian 6.2. (b) `AccountingDate` pada `ReverseJournalRequest` tidak ditawarkan di layar; delta terhadap kontrak dicatat di bagian 3.4, bukan diputuskan sendiri |
| Dependency backend | `NONE` — `BE-ACC-013` `DONE`, dan task ini tidak menuntut perubahan backend apa pun |
| Perubahan sampingan | `NONE` dari saya. **Satu berkas berubah di luar pekerjaan ini dan sengaja tidak disentuh**: `.dockerignore` (`.env.*` → `.env.local`) sudah berubah di working tree sebelum task ini dimulai dan tidak berkaitan dengan Accounting |
| Interupsi | Dev server `:3000` berhenti di tengah pengerjaan, kemungkinan besar karena `npm run build` berbagi direktori `.next` dengannya. Dilaporkan apa adanya di bagian 6.5; prosesnya tidak dinyalakan ulang tanpa sepengetahuan pemilik pekerjaan |
| Pergeseran baseline | Backend `f989453` → `0614be7` (merge integration PR #99, 98 commit) dan frontend `bcccb67bc` → `5174e0cf1` terjadi saat task berjalan. Impact scan dijalankan lebih dahulu: **nol berkas Accounting berubah di backend**, sehingga kontrak `reverse` yang menjadi dasar implementasi tidak bergeser. Rinciannya di bagian 1.3 |
| Status Git | `M` pada 6 berkas source frontend, `??` pada 3 berkas baru, ditambah `.dockerignore` yang bukan milik task ini. **Nol stage, nol commit, nol push** |
| Langkah berikutnya | Jalankan `npm run dev`, lalu `UAT-10`, `UAT-11`, dan `UAT-12` bagian 6.6. Sesudah itu **seluruh 11 task frontend Accounting selesai**, dan yang tersisa bagi modul ini adalah audit kesiapan ulang — `testing/readiness-report.md` masih bertanggal 4 September 2026 dengan verdict `NOT_READY` yang penyebabnya berada di sisi bukti test backend, bukan di frontend |
