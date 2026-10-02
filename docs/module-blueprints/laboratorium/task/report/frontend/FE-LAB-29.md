# Laporan Perubahan Frontend — `FE-LAB-29`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-29` |
| Judul | Konteks klinis pada layar pemesanan |
| Slice | `S4c` — Patologi Anatomi, gelombang `MVP-6b2` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6e.5 |
| Trace | `FR-13.10`; `LAB-DEC-091`, `INV-40`; `AC-153`, `AC-154`, `AC-155`; `VAL-102` |
| Contract version | `LAB-API-v1` **`r25`** — `approved` 2026-09-18 |
| Wewenang UI | Menambah **satu bagian** pada layar pesanan yang sudah berjalan. Nol layar lain disentuh |
| Dependency | `BE-LAB-52` ✅ selesai 2026-09-18 — kedua endpoint terverifikasi ada di kode |
| Klasifikasi | `MEDIUM` — 3 acceptance criteria, 7 berkas |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `6ea61bcad` (branch `YogaV2`) |
| Tanggal | 2026-09-23 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — ketiga AC terbangun, aturannya terbukti lewat 9 uji unit, lint 0 error, build hijau. **Belum diklik di peramban.** Satu keputusan penempatan perlu dikonfirmasi pemilik modul — lihat bagian 7 |

---

## 1. Masalah yang diperbaiki

Backend menyediakan `GET`/`PUT /lab-orders/{id}/pathology-context` sejak `BE-LAB-52`
(2026-09-18), dan keempat ruasnya berdiri di database. **Nol satu pun layar memanggilnya.**

Akibatnya bagi patolog: layar laporan Patologi Anatomi dirancang menampilkan konteks klinis
**baca-saja** dengan jalan keluar *"Belum diisi dokter pemesan"* — dan sampai hari ini kalimat
itu **selalu** benar, sebab nol ada tempat mengisinya. Diagnosa awal yang seharusnya menjadi
sumber pengisian Diagnosa Klinis pada laporan imunohistokimia nol pernah sampai.

---

## 2. Proses bisnis

**Pelaku.** Dokter pemesan pemeriksaan Patologi Anatomi.

**Pemicu.** Pesanan laboratorium berdisiplin Patologi Anatomi dibuka pada layar pesanan.

**Langkah berurutan:**

1. Dokter membuka pesanan dari daftar pesanan laboratorium.
2. Bila disiplinnya **Patologi Anatomi**, bagian *Konteks Klinis* muncul di antara Informasi
   Pesanan dan daftar Pemeriksaan. Bila bukan, bagian itu **nol dirender sama sekali**.
3. Dokter mengisi sebagian atau seluruh dari empat ruas, lalu menyimpan.
4. Patolog membacanya baca-saja pada layar laporan (`FE-LAB-28`).

| Ruas | Kegunaan |
| --- | --- |
| Diagnosa Awal | Sumber pengisian Diagnosa Klinis pada laporan imunohistokimia |
| Riwayat Penyakit | Riwayat yang relevan bagi pembacaan jaringan |
| Masa Terakhir Haid | **Hanya bermakna bagi sitologi ginekologi** (`AC-155`) |
| Keterangan Klinis | Keterangan lain yang perlu diketahui patolog |

**Jalur tidak normalnya.** Menyimpan keempat ruas kosong adalah tindakan yang **sah** — itulah
cara dokter mencabut konteks yang salah terisi. Ruas kosong dikirim sebagai `null`, bukan teks
kosong, supaya layar laporan dapat membedakan *"belum diisi"* dari *"diisi dengan isi yang tak
terlihat"*.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; kontrak `r25`; `03-frontend-architecture.md` bagian 13.2;
`LabPathologyReportController.cs` dan `LabPathologyReportDtos.cs` sebagai bukti bentuk
permintaan yang sebenarnya; `lab-order-detail-view.jsx`, `use-lab-order-form.jsx`,
`lab-microbiology-result-slice.jsx` dan `-service.js` sebagai pola.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `lib/constants/.../lab-order-constants.jsx` | **+1 jalur API**, **+1 penanda disiplin**, **+1 blok copy**, **+4 definisi ruas** |
| `lib/services/.../lab-pathology-context.service.js` | **BARU** — dua fungsi, `get` dan `save` |
| `lib/hooks/.../lab-pathology-context-rules.js` | **BARU** — empat fungsi murni |
| `lib/state/slice/.../lab-pathology-context-slice.jsx` | **BARU** — dua thunk |
| `lib/hooks/.../use-lab-pathology-context.jsx` | **BARU** — hook bagian |
| `components/view/.../lab-order-pathology-context-section.jsx` | **BARU** — komponen bagian |
| `components/view/.../lab-order-detail-view.jsx` | **+1 penjaga disiplin**, **+1 pemanggilan** |
| `lib/state/store.jsx` | **+1 registrasi** |
| `tests/unit/lab-pathology-context-fe29-rules.test.mjs` | **BARU** — 9 uji |

### 3.3 Tiga keputusan pelaksanaan

**1. Penjaganya di pemanggil, bukan di dalam komponennya.** `AC-154` menuntut alur pemesanan
disiplin lain **nol berubah**. Menempatkan penjaga di dalam komponen berarti hook-nya tetap
berjalan dan `GET /pathology-context` tetap terkirim bagi setiap pesanan Patologi Klinik dan
Mikrobiologi — lalu ditolak `VAL-102`. Penjaganya karena itu di `lab-order-detail-view.jsx`:
**komponennya nol dirender, hook-nya nol berjalan, permintaannya nol terkirim.**

**2. Slice terpisah, bukan menumpang `labOrder`.** Bagian ini hanya hidup pada satu disiplin.
Menumpangkannya pada state pesanan membuat setiap layar pesanan disiplin lain ikut memuat
keadaan yang nol pernah dipakainya.

**3. Disiplin yang belum digolongkan nol memunculkannya.** Menebak bahwa pesanan tanpa disiplin
adalah Patologi Anatomi persis yang `INV-39` larang.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Pathology Report

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-orders/{labOrderId}/pathology-context` | Membaca konteks klinis satu pesanan | `LabOrder : Read` |
| `PUT` | `/lab-orders/{labOrderId}/pathology-context` | Menulis keempat ruas sekaligus | `LabOrder : Update` |

**Base URL-nya `lab-orders`, dan itu bukan kebetulan.** Hak aksesnya `LabOrder`, bukan
`LabExamination` milik patolog (`LAB-DEC-091`, `INV-40`) — berbagi satu izin berarti memberi
patolog hak menulis riwayat penyakit pasien yang tidak pernah ia tanyakan.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | **0 error** | `PASS` | Keluaran perintah |
| Uji unit berkas ini | **9/9 lulus** | `PASS` | Keluaran perintah |
| Seluruh suite unit | **1649 lulus, 7 gagal** | `PASS` | Naik dari 1640; gagal **tetap 7** |
| Baseline gagal pada pohon bersih | **7** | `PASS` | Dibuktikan `git stash` pada task sebelumnya |
| `npm run build` | **Hijau** — `✓ Compiled successfully`, `postbuild` selesai | `PASS` | Keluaran perintah |

Uji manual: **`NOT FEASIBLE`** — sesi ini nol punya alat kendali peramban.

**`AC-154` dibuktikan TERBALIK pada lapis aturan**: `isPathologyContextVisible` diuji menolak
`ClinicalPathology`, `Microbiology`, disiplin kosong, `null`, dan objek tanpa ruas disiplin.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-153` bagian **hanya muncul** bila disiplin pesanannya Patologi Anatomi | **Terbangun; aturannya terbukti** | Penjaga di `lab-order-detail-view.jsx`; 2 uji positif |
| `AC-154` seluruh ruas **opsional**, alur disiplin lain **nol berubah** — dibuktikan terbalik | **Terbangun; aturannya terbukti** | 5 uji terbalik; formulir kosong menghasilkan payload sah; nol ruas bertanda wajib |
| `AC-155` `Masa Terakhir Haid` ditandai hanya bermakna bagi sitologi ginekologi | **Terbangun, belum dilihat** | `description` ruas berbunyi *"Hanya bermakna bagi sitologi ginekologi. Kosongkan bila tidak relevan."* |
| DoD — bagian ini berjalan | **Terbangun** | Build hijau, route terkompilasi |
| DoD — **seluruh uji pemesanan lama tetap lulus** | ✅ **Terpenuhi** | Nol kegagalan baru |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Satu keputusan penempatan perlu dikonfirmasi** | Arsitektur menulis *"pada layar pemesanan yang sudah ada"*. Bagian ini saya pasang pada **layar detail pesanan** (`lab-orders/[slug]`), **bukan** layar buat pesanan (`lab-orders/create`), dan alasannya teknis: endpoint-nya berkunci pada `labOrderId` yang **baru ada sesudah pesanan tersimpan**, sementara layar buat **memecah satu pilihan menjadi beberapa pesanan per disiplin** sehingga saat itu id-nya bisa belum ada atau lebih dari satu. Detail pesanan juga satu-satunya tempat disiplinnya sudah pasti. **Bila yang dimaksud justru layar buat**, pekerjaan tambahannya adalah mengirim `PUT` sesudah pesanan terbentuk dan memilih pesanan mana yang menerimanya ketika pemecahan terjadi — dan itu keputusan produk, bukan pilihan teknis |
| Peringatan | Nol peringatan lint baru |
| Masalah yang diketahui | Ketiga AC **belum diklik di peramban** |
| Risiko tersisa | **Rendah.** Bagian ini aditif dan berpenjaga disiplin; pesanan disiplin lain nol menyentuh satu baris pun kode baru |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 9 berkas tersentuh, seluruhnya dalam cakupan. **Nol operasi Git dijalankan** |
| Langkah berikutnya | Konfirmasi penempatan di atas; klik ketiga AC terhadap pesanan Patologi Anatomi yang sungguhan |
