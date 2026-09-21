# Laporan Perubahan Frontend — `FE-LAB-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-11` |
| Judul | Formulir Penerimaan Sampling/Specimen |
| Slice | `MVP-5a`, `EPIC-LAB-11` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6b |
| Trace | `FR-11.8`, `LAB-DEC-045`, `BR-40`, `BR-44`; `LAB-FE-009`, `LAB-FE-010`, `LAB-FE-011`; `LAB-DEC-039`, `LAB-DEC-040`, `LAB-DEC-050`; `VAL-51`..`VAL-58`, `AC-69` |
| Contract version | `LAB-API-v1` `r7`..`r9` — `approved`, terkunci. **Nol amandemen diminta task ini** |
| Wewenang UI | Tata letak `DEV_DISCRETION` (`LAB-FE-002`); empat butir wajib `10.4` ditegakkan, bukan ditafsir |
| Dependency | `BE-LAB-21` ✅, `BE-LAB-22` ✅, `BE-LAB-23` ✅, `BE-LAB-24` ✅ |
| Klasifikasi | `HEAVY` — layar terbesar modul ini; lima wilayah, dua komponen dipakai ulang, dan rangkaian lima panggilan API |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `686038858` |
| Commit backend yang dijadikan rujukan | `13665452` |
| Tanggal | 2026-09-17 |
| Status | **`SELESAI`** dengan **dua batas yang dilaporkan apa adanya** — lihat bagian 7 dan 8. Satu butir DoD **dicabut sebelum dikerjakan** karena bertentangan dengan keputusan yang sudah disetujui; lihat bagian 1.1 |

---

## 1. Keadaan yang ditemukan di awal

Jalur rujukan luar sudah punya layar **pendaftaran** (`FE-LAB-05`) dan layar **wadah per
pesanan** (`FE-LAB-07`), tetapi keduanya terpisah. `LAB-DEC-045` menuntut petugas menyelesaikan
satu pasien **dari identifikasi sampai penetapan kelayakan tanpa berpindah menu** — dan layar itu
belum ada.

### 1.1 Satu butir DoD dicabut sebelum satu baris pun ditulis

DoD task ini menuntut *"jumlah/Qty dapat diisi dan hasilnya tersimpan sebagai beberapa baris"*,
dan catatan di bawahnya memberi contoh *"mengisi jumlah 3 menghasilkan tiga baris"*.

**Butir itu bertentangan dengan tiga sumber otoritatif, dan ketiganya bertanggal 2026-09-14** —
hari yang sama task ini ditulis, sehingga besar kemungkinan ia tertulis tanpa memuat keputusan
yang baru turun:

| Sumber | Bunyi |
| --- | --- |
| `LAB-DEC-050`, `BR-45` | "Kolom Jumlah tidak dibuat"; `LAB-DEC-038` **dicabut** |
| `LAB-API-v1` `r9` | Ruas `Quantity` pada `POST /lab-examinations` **"tidak jadi dibuat"** |
| `03-frontend-architecture.md` §10.5 | Kolom Jumlah/Qty **tidak ada** — "Tidak di layar, tidak di permintaan API" |

**Sebabnya bukan selera.** `BE-LAB-23` menemukan `LabExamination` memiliki index unik
`(SpecimenId, ProcedureId)` di tingkat database, dipasang atas dasar `BR-20` dan `AC-35`. Dua
baris untuk jenis pemeriksaan yang sama pada satu wadah **tidak mungkin ada** — "jumlah 3 menjadi
tiga baris" akan ditolak database pada baris kedua.

Membangunnya sesuai DoD lama berarti membangun sesuatu yang **pasti gagal saat dipakai**. Butir
itu karena itu diganti pada roadmap menjadi kebalikannya — *"nol kolom Jumlah/Qty dirender dan nol
ruas `quantity` dikirim"* — dan `BR-44`, yang sebelumnya tidak masuk DoD sama sekali,
ditambahkan.

### 1.2 Satu temuan yang lebih besar daripada task ini

Ruas wadah yang ditambahkan `r7` — `specimenTypeId`, `specimenTypeOtherNote`, `volumeAmount`,
`volumeUnitId`, `physicallyReceivedAt` — **nol punya penulis di frontend**. Penelusuran kelimanya
pada seluruh `src` menghasilkan nol kemunculan di luar berkas yang dibuat task ini.

Akibatnya bukan sekadar ruas yang menganggur. `VAL-51` menolak **tanpa syarat** setiap wadah baru
yang tidak membawa `SpecimenTypeId`:

```
if (specimenTypeId == Guid.Empty)
    throw new LabSpecimenValidationException("Pilih jenis specimen terlebih dahulu.");
```

Sejak migration `BE-LAB-21` diterapkan pada 2026-09-15, **layar wadah lama (`FE-LAB-07`) tidak
lagi dapat merencanakan wadah sama sekali** — setiap percobaan dijawab `422`. Traceability sudah
meramalkannya pada revisi 18; task ini membuktikannya dari source.

**Perbaikan layar lama tidak dikerjakan di sini**, karena cakupan task ini menyatakan layar lama
tidak diubah. Ia dilaporkan sebagai temuan pada bagian 8.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya petugas penerimaan laboratorium.** Pasien rujukan luar datang membawa surat
rujukan, dan sampelnya dibawa atau diambil di tempat.

1. **Wilayah A — Identifikasi Pasien.** Petugas mencari pasien berdasarkan NIK, No. RM, atau nama,
   lalu memilih unit layanan.
2. **Wilayah B — Data Pemeriksaan.** Instansi perujuk dan dokter perujuk dipilih **dari daftar**,
   nomor rujukan diketik. Metode pembayaran tampil **baca-saja**. Menekan `Daftarkan Pasien`
   membentuk kunjungan, dan nomornya langsung tampil.
3. **Wilayah D — Daftar Pemeriksaan.** Petugas memilih pemeriksaan dari katalog; harga satuan,
   subtotal, dan grand total tampil seiring pilihan.
4. **Wilayah C — Wadah/Specimen.** Jenis specimen dipilih dari daftar. Bila yang dipilih adalah
   `Lainnya`, satu kotak keterangan **muncul** dan wajib diisi. Volume dan satuannya boleh
   dikosongkan. Waktu penerimaan fisik diisi bila sampel tiba pada waktu yang berbeda dari waktu
   pencatatan. Menekan `Catat Wadah` membentuk pesanan **lalu** wadahnya.
5. **Wilayah E — Penetapan Kelayakan.** Tombol `Layak` dan `Tidak Layak` baru dapat ditekan
   sesudah wadah tercatat dan sekurang-kurangnya satu pemeriksaan dipilih.

### 2.1 Jalur tidak normal

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Jenis specimen belum dipilih | *"Pilih jenis specimen terlebih dahulu."* — kalimat yang sama dengan `VAL-51` |
| Jenis `Lainnya` tanpa keterangan | Kotak keterangan muncul dan ditandai wajib |
| Volume diisi tanpa satuan | *"Pilih satuan volume."* — 5 mililiter dan 5 mikroliter berbeda seribu kali lipat |
| Waktu penerimaan melewati sekarang | Ditolak sebelum dikirim (`VAL-58`) |
| Kelayakan ditekan terlalu dini | Tombolnya mati, **dan sebabnya tertulis** |
| Perujuk belum terdaftar | Layar mengatakan jalan buntunya apa adanya — lihat 7.2 |
| Pemeriksaan lintas disiplin | Layar memberitahukan berapa pesanan terbentuk — lihat 7.1 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/.../laboratory-management/lab-reception-rules.js` | **Baru.** Aturan murni: penjaga wilayah C, pembentuk payload, gerbang kelayakan, peringatan `BR-44` |
| `src/lib/constants/.../laboratory-management/lab-specimen-reception-constants.jsx` | **Baru.** Salinan teks kelima wilayah, ruas wilayah C, dan kedua wilayah yang sengaja kosong |
| `src/lib/hooks/.../laboratory-management/use-lab-specimen-reception.jsx` | **Baru.** Controller yang merangkai lima panggilan |
| `src/components/view/.../lab-specimen-receptions/lab-specimen-reception-view.jsx` | **Baru.** Layar A–E |
| `src/app/.../lab-specimen-receptions/page.jsx` + client | **Baru.** Route |
| `src/style/.../lab-specimen-receptions/lab-specimen-reception.module.css` | **Baru.** Hanya token, nol nilai literal |
| `src/utils/menu-sidebar/menu-items.jsx` | **+1 entri** — `Penerimaan Sampling/Specimen` |
| `tests/unit/lab-reception-rules.test.mjs` | **Baru, 15 uji unit** |
| `tests/e2e/lab-specimen-reception-screen.spec.mjs` | **Baru, 6 pemeriksaan layar** |

**Nol berkas layar lama diubah**, sesuai cakupan.

### 3.2 Dua komponen dipakai ulang, dan cara memakainya menentukan

| Dipakai ulang | Cara |
| --- | --- |
| `useLabPatientRegistrationForm` | Dipanggil sebagai **hook**, dengan argumen `"externalReferral"`. Tanda tangannya **string posisional**, bukan objek beropsi — diperiksa dari sumbernya, bukan diduga dari bentuk hook lain di modul ini |
| `LabCatalogPicker` | Dipakai sebagai **komponen**, bukan hook. Ia memanggil `useLabCatalogPicker` di dalam dirinya sendiri; memanggilnya lagi dari controller akan melahirkan **instance kedua yang state-nya terpisah** — layar menampilkan satu pilihan sementara yang dikirim adalah pilihan yang lain. Jalur resminya `onSelectionChange` |

Pilihan katalog dikosongkan dengan menaikkan `resetToken` yang dipasang sebagai `key`, sehingga
pemilihnya lahir kembali bersih — tanpa menambah prop pada komponen yang juga dipakai layar lain.

### 3.3 Rangkaian lima panggilan, dan urutannya tidak dapat dibalik

```
pendaftaran → kunjungan → pesanan (per disiplin) → wadah → kelayakan
```

Pendaftaran hanya mengembalikan `encounterId`; wadah menempel pada **pesanan**; dan pesanan baru
dapat dibentuk sesudah pemeriksaannya dipilih. Pesanan dibentuk lewat
`POST /lab-orders/by-examinations` — bukan `POST /lab-orders` — karena jalur ini memang memesan
beberapa pemeriksaan sekaligus dan backend yang memecahnya per disiplin (`BR-47`).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol beraksi menampilkan keadaan memuatnya; daftar jenis specimen nonaktif selama diambil |
| Kosong | *"Belum ada pemeriksaan dipilih."*; *"Daftar jenis specimen belum diisi. Hubungi kepala instalasi."*; *"Daftar instansi perujuk masih kosong."* |
| Gagal | Pesan dari backend ditampilkan apa adanya pada alert dan toast merah |
| Tanpa hak akses | Dibungkus `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Patient Registration

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/.../lab-patient-registrations/external-referral` | Membentuk kunjungan dari wilayah A dan B | `LabPatientRegistration : Create` |

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/.../lab-orders/by-examinations` | Membentuk pesanan dari pemeriksaan terpilih | `LabOrder : Create` |

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/.../lab-specimens/by-order/{labOrderId}` | Mencatat wadah beserta bahannya | `LabSpecimen : Plan` |
| `POST` | `/v1/.../lab-specimens/{id}/accept` | Menyatakan layak | `LabSpecimen : Accept` |
| `POST` | `/v1/.../lab-specimens/{id}/reject` | Menyatakan tidak layak | `LabSpecimen : Accept` |

#### Health Services / Laboratory Management / Lab Specimen Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/.../lab-specimen-types/options` | Daftar pilihan jenis specimen, **hanya yang aktif** (`VAL-55`) | `LabSpecimenType : Read` |

> **Inilah konsumen pertama `GET /options`.** Thunk-nya berdiri sejak `FE-LAB-10` tanpa pembaca,
> dan dicatat di laporan itu sebagai sesuatu yang sengaja menunggu layar ini. Ia kini terpakai.

---

## 6. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint --quiet` pada seluruh berkas yang disentuh | Nol keluaran — **`PASS`** |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **1029/1029 lulus** (1014 lama + 15 baru) — **`PASS`** |
| `npm run build` | Lulus; route `/health-services/laboratory-management/lab-specimen-receptions` terdaftar — **`PASS`** |
| `npx playwright test tests/e2e/lab-specimen-reception-screen.spec.mjs` | **6 passed** — **`PASS`** |
| **`AC-76` regresi** — seluruh uji layar lab lama | **12 passed**, nol berkas lama disentuh — **`PASS`** |

Uji manual: **`PASS`** lewat pemeriksaan layar berjawaban terpasang. `MANUAL TEST: NOT FEASIBLE`
untuk jalur ujung-ke-ujung dengan backend sungguhan — lihat 8.

### 6.1 Enam pemeriksaan layar, tiga di antaranya menguji KETIADAAN

| Pemeriksaan | Yang dibuktikan |
| --- | --- |
| `LAB-FE-010` wilayah | Kelima wilayah hadir, dan **C serta D hadir bersamaan** pada satu layar |
| `LAB-FE-010` peringatan | Peringatan penguncian terlihat **sebelum** tombol ditekan, tanpa satu pun aksi lebih dulu |
| `LAB-DEC-039` gerbang | Kedua tombol kelayakan **mati**, dan sebabnya tertulis — bukan diam |
| `LAB-FE-011` pembayaran | Kalimat *"Belum dapat ditentukan"* tertulis, dan **nol** `select` pembayaran ada di DOM |
| `LAB-DEC-050` Jumlah | **Nol** kotak isian `quantity`, `jumlah`, maupun berlabel `Jumlah` dirender |
| `AC-69` perujuk | Jalan buntunya disebut apa adanya, bukan disembunyikan |

**Ketiga pemeriksaan ketiadaan itu yang paling mudah lolos bila hanya tampilannya dilihat
sepintas.** Kotak Jumlah yang diam-diam kembali tidak akan menimbulkan galat apa pun sampai
seorang petugas mengisinya dan database menolak baris keduanya.

### 6.2 Satu kekeliruan uji yang pantas dicatat

Uji `VAL-58` versi pertama membandingkan waktu `datetime-local` — yang **lokal** — terhadap `now`
yang ditulis UTC. Di WIB selisihnya tujuh jam, sehingga waktu yang seharusnya "masa depan" justru
terbaca sebagai masa lalu. **Kekeliruan uji, bukan cacat produk**: `new Date` memang membaca
`datetime-local` sebagai waktu lokal, dan itu tafsir yang benar bagi jam dinding yang diketik
petugas. Diperbaiki dengan menyamakan basis waktunya; uji itu kini tidak lagi bergantung pada
zona waktu mesin yang menjalankannya — pada mesin ber-UTC, versi pertamanya akan lolos dan
menyembunyikan masalahnya.

### 6.3 Grep anti-regresi

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal, `!important`, tombol non-base, `<table>` mentah, Bootstrap utility | **Kosong** |
| Typography pada style baru | 4 temuan, seluruhnya memakai token dan menyasar class milik layar ini sendiri — nol menyasar komponen shared |

---

## 7. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Wilayah C dan D terlihat berdampingan (`LAB-FE-010`) | **Terpenuhi** | 6.1 |
| Peringatan penguncian terlihat sebelum tombol ditekan | **Terpenuhi** | 6.1 |
| Metode pembayaran baca-saja tanpa kotak pilihan (`LAB-FE-011`) | **Terpenuhi** | 6.1 |
| **Nol kolom Jumlah/Qty, nol ruas `quantity`** (`LAB-DEC-050`) | **Terpenuhi** | 6.1; uji unit S8 |
| Pembatalan sesudah `Layak` menyebut tagihannya (`BR-44`) | **Terpenuhi sebagian** | Kalimatnya ada dan teruji unit (S14), tetapi **jalur pembatalan baris belum dirender** — lihat 8 |
| Penamaan menu membedakan jalur (`LAB-FE-009`) | **Terpenuhi** | Menu `Penerimaan Sampling/Specimen`, beserta komentar pembeda pada `menu-items.jsx` |
| `AC-76` — uji layar lama lulus tanpa disentuh | **Terpenuhi** | 12/12 lulus; nol berkas lama diubah |

| AC | Status | Catatan |
| --- | --- | --- |
| `AC-75` | **Terpenuhi pada layar** | Satu pasien diselesaikan A sampai E tanpa berpindah menu |
| `AC-77` | **Terpenuhi pada layar** | Penetapan kelayakan tersedia pada wilayah E beserta gerbangnya |
| `AC-69` | **Terpenuhi** | Perujuk dipilih dari daftar; nol kotak teks bebas |

### 7.1 Batas: wadah direncanakan pada pesanan pertama

Ketika pemeriksaan yang dipilih melintasi beberapa disiplin, backend memecahnya menjadi beberapa
pesanan — dan **satu wadah hanya menempel pada satu pesanan**. Layar mencatat wadah pada pesanan
**pertama** dan memberitahukan pemecahannya apa adanya.

Merencanakan satu wadah untuk seluruh pesanan akan keliru menggabungkan bahan yang berbeda;
membentuk beberapa wadah sekaligus adalah **keputusan yang belum diambil siapa pun**. Sisanya
dikerjakan lewat layar wadah per pesanan yang sudah ada.

### 7.2 Batas: dua wilayah sengaja kosong

| Wilayah | Keadaan |
| --- | --- |
| Metode pembayaran | Baca-saja, bertuliskan *"Belum dapat ditentukan"*. Tertahan `LAB-COORD-007` |
| Usulan instansi perujuk | Digambar sebagai keterangan, bukan formulir. Tertahan `LAB-COORD-006` |

Keduanya **digambar sebagai wilayah yang ada**, sehingga tata letaknya tidak perlu dirombak
ketika `LAB-REQ-005` dijawab.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning baru dari lint maupun build |
| **Temuan di luar cakupan, dan ia mendesak** | **Layar wadah lama (`FE-LAB-07`) tidak lagi dapat merencanakan wadah** — `VAL-51` menolak `422` setiap permintaan tanpa `specimenTypeId`, dan layar itu nol mengirimnya. Berlaku sejak migration `BE-LAB-21` diterapkan 2026-09-15. Perbaikannya kecil: ruas wilayah C task ini tinggal dipasang di sana. **Tidak dikerjakan di sini** karena cakupan menyatakan layar lama tidak diubah; perlu task tersendiri |
| Masalah yang diketahui | `BR-44` baru terpenuhi pada **kalimatnya**, belum pada jalur pembatalan baris pemeriksaan sesudah `Layak` — wilayah D memakai pemilih katalog yang tidak mengenal keadaan terkunci. Menambahkannya berarti mengubah komponen yang juga dipakai layar lain |
| Dependency backend | `NONE` yang tersisa. Keempat task backendnya selesai |
| Perubahan sampingan | `NONE`. Folder `test-results/` dari Playwright dihapus sesudah pemeriksaan |
| Interupsi | `NONE` |
| Status Git | 27 entri; nol `git add`, `commit`, maupun `push` dijalankan |
| Langkah berikutnya | **Satu task perbaikan `FE-LAB-07`** untuk temuan di atas — itu yang paling mendesak. Lalu `FE-LAB-12`, daftar dan detail penerimaan |
