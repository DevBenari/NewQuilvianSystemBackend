# Laporan Perubahan Frontend — `RJ-BIL-FE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| TASK ID | `RJ-BIL-FE-002` — Pesanan klinis tidak pernah tampil sebagai lunas |
| TASK TYPE | Implementasi task frontend approved dari roadmap canonical |
| COMPLEXITY | `MEDIUM` |
| MODEL | `claude-opus-5` |
| TASK MODE | `MODULE BLUEPRINT` |
| WRITE TARGET | `V2QuilvianSystemFrontendDev/src/` dan `tests/unit/`; laporan ini di `NewQuilvianSystemBackend/docs/` |
| Wewenang tulis | `RJ-BIL-DEC-013`, 28 Agustus 2026 |
| Trace | `RJ-BIL-GATE-DEC-001`, `003`, `004`, `007`; `RJ-BIL-CAP-019` berstatus `Missing` |
| Contract version | State `RJ-BIL-STATE-001@1.0.0` — **tidak berubah** |
| Backend pasangan | `RJ-BIL-BE-002`, `RJ-BIL-BE-003`. **Bagian Radiologi tidak dikerjakan** — `RJ-BIL-BE-004` ⛔ |
| Branch / HEAD frontend | `QuilvianDevV2` / `bd31dc9` |
| Tanggal | 28 Agustus 2026 |
| Status | **SELESAI untuk bagian Resep, Tindakan, dan Laboratorium.** Bagian Radiologi tetap ⛔ di luar cakupan |

---

## 1. Koreksi urutan pengerjaan

Sebelum task ini, saya merekomendasikan `RJ-BIL-FE-005` sebagai langkah berikutnya. **Itu keliru.**
Baris **Dependency** `FE-005` berbunyi `RJ-BIL-FE-002`; `RJ-BIL-BE-007`, dan diagram dependency
roadmap menyatakannya lebih tegas lagi:

```text
RJ-BIL-FE-001
   ├── RJ-BIL-FE-002
   │      ├── RJ-BIL-FE-004
   │      ├── RJ-BIL-FE-005   ← tergerbang FE-002
   │      └── RJ-BIL-FE-006
```

`FE-002` menggerbangi tiga task sekaligus. Ia dikerjakan lebih dulu, sesuai roadmap.

---

## 2. Temuan yang menjadi inti task ini

**`PrescriptionResponse` masih mengirimkan kolom pembayarannya sendiri.**

```csharp
// Areas/HealthServices/PharmacyManagement/Dtos/PrescriptionDtos.cs
public PrescriptionPaymentStatus PaymentStatus { get; set; }

// Areas/HealthServices/PharmacyManagement/Enums/PrescriptionPaymentStatus.cs
[Display(Name = "Lunas")]
Paid = 5,
```

`PrescriptionDetailResponse` menambah `BillingId`, `BillingGeneratedAt`, `PaymentCompletedAt`, dan
`PaymentCompletedByUserName`.

`RJ-BIL-BE-002` sudah mencabut kewenangan modul klinis atas status finansial dan **menghapus
endpoint yang menulis** kolom-kolom itu. Yang **membaca** masih ada. Artinya: nilai `Lunas` tetap
dapat diambil dari API resep hari ini juga.

Inilah persis kegagalan yang dicegah kriteria penerimaan 1. Seorang pengembang yang mengikat
`paymentStatus` ke badge status — hal paling wajar yang dilakukan siapa pun — akan menampilkan
pesanan klinis sebagai **Lunas**, dan tidak ada satu pun galat yang akan memberi tahu bahwa itu
salah.

**Ini bukan laporan cacat modul Farmasi.** Kolom itu mungkin masih dibutuhkan alur lain. Yang
menjadi tanggung jawab task ini adalah memastikan layar Billing tidak pernah memperlakukannya
sebagai kebenaran. Tidak ada satu berkas pun milik modul Farmasi yang disunting.

---

## 3. Batas backend yang ditemukan

| Sumber | Cara membaca pesanan klinisnya | Masalahnya |
| --- | --- | --- |
| Laboratorium | `GET /lab-orders` | **Tanpa satu pun parameter penyaring.** Tidak ada `encounterId`, tidak ada paginasi. Seluruh pesanan laboratorium rumah sakit terkirim dalam satu balasan |
| Resep | `GET /prescriptions/{id}` | Tidak ada daftar per kunjungan. Hanya `active-by-consultation/{consultationId}` — konsultasi, bukan kunjungan |
| Tindakan | `GET /patient-procedures/{id}` | Tidak ada daftar per kunjungan |

Ketiganya dapat dijangkau **karena baris tagihan sudah membawa penghubungnya**:
`sourceAggregateId` berisi id pesanan Lab / resep / tindakan, dan `sourceItemId` berisi id
specimen. Jadi tidak ada satu pun identitas yang ditebak.

Konsekuensi yang harus diketahui: penyaringan pesanan Lab per kunjungan **terpaksa dilakukan di
sisi klien**. Selama jumlah pesanan masih kecil hal ini berjalan. Ketika tidak lagi kecil, yang
harus berubah adalah endpoint-nya — bukan layarnya. Dicatat di sini supaya tidak ditemukan
kembali sebagai kejutan.

---

## 4. Yang dibangun

Delapan berkas baru, satu berkas disunting.

| Lapisan | Berkas |
| --- | --- |
| Constants | `billing-clinical-boundary-constant.jsx` |
| Utils | `billing-clinical-boundary-utils.jsx` |
| Service | `billing-clinical-boundary.service.js` |
| Slice | `billing-clinical-boundary-slice.jsx` |
| Store | `lib/state/store.jsx` — **disunting** |
| Hook | `use-billing-clinical-boundary.jsx` |
| View | `billing-clinical-boundary-view.jsx` |
| View | `components/billing-boundary-line-card.jsx` |
| View | `components/billing-status-axis-grid.jsx` |
| View | `components/billing-legacy-payment-notice.jsx` |
| Style | `billing-clinical-boundary.module.css` |
| Route | `app/health-services/billing-management/clinical-boundary/[encounterId]/page.jsx` |
| Test | `tests/unit/billing-clinical-boundary-utils.test.mjs` — 21 test |

Folio dibaca ulang lewat hook `RJ-BIL-FE-001`, bukan lewat pemanggilan baru. Komponen keadaan
gagal `FE-001` juga dipakai kembali apa adanya.

---

## 5. Lima sumbu, dan mengapa tidak boleh dilebur

| Sumbu | Pemilik | Sumber |
| --- | --- | --- |
| Pesanan klinis | Modul klinis | Lab `orderStatus`; resep `prescriptionStatus`; tindakan `status` |
| Pemenuhan | Modul klinis | Lab `specimenStatus`; resep `fulfillmentStatus` |
| Pemrosesan Billing | Billing | `processing-status` → `BillingProcessingOutcome` |
| Perhitungan biaya | Billing | `calculationStatus` |
| **Pembayaran** | **Billing** | **Selalu "Belum tersedia"** |

Peleburan kelimanya bukan soal kerapian. Begitu menjadi satu label, petugas kehilangan kemampuan
membedakan *"obatnya sudah diserahkan"* dari *"biayanya sudah diakui"* dari *"uangnya sudah
masuk"* — dan ketiganya menuntut tindakan yang sama sekali berbeda.

**Sumbu pembayaran selalu kosong, dan itu jawaban yang benar.** Billing memang belum punya status
pembayaran; `RJ-BIL-BE-005` dan `RJ-BIL-BE-008` yang akan membawanya masih ⛔. Mengisi sumbu itu
dengan apa pun — termasuk dengan kolom warisan Farmasi — berarti mengarang keadaan uang.

Dua keputusan tampilan yang mengikuti dari situ:

1. **Sumbu yang kosong tetap muncul**, berbunyi *"Belum tersedia"*, tidak dihapus dari susunan.
   Sumbu yang hilang membuat pembacanya menyimpulkan sendiri, dan kesimpulan termudah adalah yang
   paling berbahaya: bahwa tidak ada apa-apa di sana.

2. **Kolom warisan dibawa ke permukaan, bukan disembunyikan.** Ia disajikan di kotak peringatan
   tersendiri, nilainya **dicoret**, disertai pernyataan bahwa ia bukan kebenaran finansial.
   Menyembunyikannya dari lapisan data tidak menghapusnya dari API — ia hanya akan muncul kembali
   di layar lain yang membacanya tanpa peringatan. Nama fieldnya pun diberi awalan
   `legacyPaymentStatus` supaya pemakai berikutnya tidak dapat salah membacanya.

Empat nilai ditandai **menyesatkan** dan mendapat bingkai merah: `Lunas`, `Dibayar Sebagian`,
`Disetujui Asuransi`, dan `Pembayaran Ditiadakan`. Keempatnya membuat pembacanya menyimpulkan
uangnya sudah beres.

---

## 6. Radiologi dinyatakan, bukan didiamkan

Radiologi **tidak** dimasukkan ke daftar sumber, karena ia memang belum terdaftar pada
`BillingSourceContract` backend dan `RJ-BIL-BE-004` masih ⛔.

Tetapi ketiadaannya **diumumkan di layar**:

> Ketiadaan baris radiologi di layar ini berarti **belum diketahui**, bukan berarti tidak ada
> pemeriksaan radiologi.

Membiarkannya senyap akan membuat layar berbohong secara pasif — dan itu jenis kebohongan yang
paling sulit ditangkap saat peninjauan.

---

## 7. Bukti acceptance criteria

| # | Kriteria | Cara dipenuhi | Bukti |
| --- | --- | --- | --- |
| 1 | UI **tidak** menampilkan order sebagai `Paid` | Sumbu pembayaran selalu "Belum tersedia"; kolom warisan dipisah, dicoret, dan diberi peringatan | 6 test, termasuk satu yang menelusuri **seluruh** sumbu dan memastikan tidak ada yang bernilai `Lunas` |
| 2 | Sumber dan versi status terlihat | Tiap sumbu membawa nama pemiliknya; versi terbawa pada sumbu pemrosesan dan perhitungan | 5 test |
| 3 | Stale response ditolak | `isActiveRequest` dipakai ulang, kini **per baris tagihan** | 3 test pada berkas `FE-001` |

---

## 8. Validasi yang dijalankan

| Validasi | Perintah | Hasil |
| --- | --- | --- |
| Test unit baru | `node --test tests/unit/billing-clinical-boundary-utils.test.mjs` | **21 lulus, 0 gagal** |
| Seluruh test unit | `npm run test:unit` | **88 lulus, 0 gagal** (67 lama + 21 baru) |
| Lint berkas Billing | `npx eslint <berkas billing>` | **0 masalah** |
| Build | `npm run build` | **Lulus**, exit `0` |

Route baru terbukti dihasilkan, bukan sekadar tidak menggagalkan build:

```
.next/app-path-routes-manifest.json
  /health-services/billing-management/billing-folio/[encounterId]/page      ← FE-001
  /health-services/billing-management/clinical-boundary/[encounterId]/page  ← FE-002
```

### `MANUAL TEST: NOT FEASIBLE`

Alasannya sama dengan `RJ-BIL-FE-001` dan tetap berlaku: tidak ada data folio yang dapat dijangkau
dari sesi ini, layar belum masuk navigasi mana pun karena `FRONTEND_AUTHORITY` masih `OPEN`, dan
sesi ini tidak menjalankan browser.

Harness `node --test` tanpa `@testing-library` membuat component render test **belum mungkin**
ditulis. Batas ini kini mengenai dua task berturut-turut dan akan mengenai sisanya juga.
Penutupannya adalah cakupan `RJ-BIL-FE-007`.

---

## 9. Yang sengaja tidak dikerjakan

| Hal | Alasan |
| --- | --- |
| Bagian Radiologi | `RJ-BIL-BE-004` ⛔; Radiologi belum terdaftar pada kontrak sumber Billing |
| Menyunting DTO atau enum modul Farmasi | Di luar wewenang task ini, dan kolomnya mungkin masih dipakai alur lain |
| Menambah penyaring `encounterId` pada `GET /lab-orders` | Perubahan backend; tidak tercakup `RJ-BIL-DEC-013` |
| Menu dan navigasi | `FRONTEND_AUTHORITY` masih `OPEN` |

---

## 10. Risiko yang diketahui

1. **Penyaringan pesanan Lab di sisi klien.** `GET /lab-orders` mengirim seluruh pesanan rumah
   sakit. Ini berjalan sekarang dan akan berhenti berjalan seiring pertumbuhan data. Yang harus
   berubah adalah endpoint-nya.

2. **Satu permintaan klinis per baris tagihan.** Tidak satu pun modul klinis menyediakan pencarian
   per kunjungan, sehingga status dimuat per baris. Folio besar berarti banyak permintaan ke tiga
   modul berbeda.

3. **Kolom warisan masih terbaca dari API.** Layar ini menjinakkannya, tetapi hanya untuk dirinya
   sendiri. Layar mana pun yang membaca `prescriptions/{id}` tanpa peringatan serupa akan
   mengulangi kesalahan yang sama. Ini pantas menjadi keputusan pemilik: apakah kolom itu tetap
   dibaca, atau ditarik dari payload.

---

## 11. Batas yang dipatuhi

`commit` / `push` / `merge` / `deploy`: **TIDAK**. Perubahan backend: **TIDAK**. Aktivasi
`RJ-BIL-DEP-009`: **TIDAK**. Mutasi finansial dari frontend: **TIDAK ADA SATU PUN** — seluruh
service layer task ini hanya memuat operasi baca.
