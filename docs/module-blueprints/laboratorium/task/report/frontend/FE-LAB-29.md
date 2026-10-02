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
| Status | ✅ **`SELESAI`** (naik 2026-10-02) — ketiga AC terbukti **di peramban** terhadap backend lokal dan PostgreSQL dev, 10/10 sesudah satu perbaikan hak akses; `Simpan` dijalankan sungguhan pada pesanan uji `LAB-RSMMC-000009` (`200`). **Penempatan di layar detail pesanan dikonfirmasi pemilik modul.** Lihat 8. *(Semula 2026-09-23: ⚠ — 9 uji unit, belum diklik di peramban, penempatan menunggu konfirmasi.)* |

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
| `AC-153` bagian **hanya muncul** bila disiplin pesanannya Patologi Anatomi | ✅ **Terbukti di peramban** (C1, C4) | Penjaga di `lab-order-detail-view.jsx`; 2 uji positif |
| `AC-154` seluruh ruas **opsional**, alur disiplin lain **nol berubah** — dibuktikan terbalik | ✅ **Terbukti di peramban** (C2, C4, C5) | 5 uji terbalik; formulir kosong menghasilkan payload sah; nol ruas bertanda wajib; pesanan Patologi Klinik dan Mikrobiologi nol bagian ini |
| `AC-155` `Masa Terakhir Haid` ditandai hanya bermakna bagi sitologi ginekologi | ✅ **Terlihat di peramban** (C3) | `description` ruas berbunyi *"Hanya bermakna bagi sitologi ginekologi. Kosongkan bila tidak relevan."* |
| DoD — bagian ini berjalan | ✅ | Simpan `200`, terbaca ulang, dan terbaca pula oleh laporan PA (C5–C7) |
| DoD — **seluruh uji pemesanan lama tetap lulus** | ✅ **Terpenuhi** | Nol kegagalan baru |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Satu keputusan penempatan perlu dikonfirmasi** | Arsitektur menulis *"pada layar pemesanan yang sudah ada"*. Bagian ini saya pasang pada **layar detail pesanan** (`lab-orders/[slug]`), **bukan** layar buat pesanan (`lab-orders/create`), dan alasannya teknis: endpoint-nya berkunci pada `labOrderId` yang **baru ada sesudah pesanan tersimpan**, sementara layar buat **memecah satu pilihan menjadi beberapa pesanan per disiplin** sehingga saat itu id-nya bisa belum ada atau lebih dari satu. Detail pesanan juga satu-satunya tempat disiplinnya sudah pasti. **Bila yang dimaksud justru layar buat**, pekerjaan tambahannya adalah mengirim `PUT` sesudah pesanan terbentuk dan memilih pesanan mana yang menerimanya ketika pemecahan terjadi — dan itu keputusan produk, bukan pilihan teknis |
| Peringatan | Nol peringatan lint baru |
| **Penempatan — dikonfirmasi 2026-10-02** | Pemilik modul menyetujui penempatan pada **layar detail pesanan** |
| Masalah yang diketahui | ~~Ketiga AC belum diklik di peramban.~~ **Tertutup 2026-10-02** (8). Uji menemukan bagian ini tetap dapat disunting oleh akun tanpa `LabOrder : Update` — diperbaiki (8.2) |
| Risiko tersisa | **Rendah.** Bagian ini aditif dan berpenjaga disiplin; pesanan disiplin lain nol menyentuh satu baris pun kode baru |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 9 berkas tersentuh, seluruhnya dalam cakupan. **Nol operasi Git dijalankan** |
| Langkah berikutnya | ~~Konfirmasi penempatan di atas; klik ketiga AC terhadap pesanan Patologi Anatomi yang sungguhan.~~ Keduanya selesai 2026-10-02. Tersisa: uji ulang dengan akun **dokter pemesan** sungguhan bila akun itu sudah ada (8.1 memakai superadmin sebagai pengganti) — bukan penahan |

---

## 8. Verifikasi susulan 2026-10-02 — di peramban, tulis pada pesanan uji

**Lingkungan.** Backend lokal (`dotnet run`, `Development`) terhadap PostgreSQL dev bersama; `next dev`
port 3000 dari working tree `YogaV2`; Chromium lewat Playwright; login **lewat formulir**.

**Akun.** Akun **dokter pemesan** belum ada di dev, sehingga jalur menyimpan dijalankan dengan
**superadmin** sebagai penggantinya (pemegang `LabOrder : Update`); jalur tanpa hak dijalankan dengan
akun asli **Vina (analis)**. **Wewenang tulis:** hanya `PUT /lab-orders/{id}/pathology-context` milik
pesanan uji **`LAB-RSMMC-000009`** diteruskan; selebihnya digagalkan.

### 8.1 Hasil

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| C1 | `AC-153` pesanan Patologi Anatomi: bagian *Konteks Klinis* tampil di layar **detail** pesanan | `PASS` | — |
| C2 | `AC-154` keempat ruas opsional — nol penanda wajib | `PASS` | `wajib=[]` |
| C3 | `AC-155` `Masa Terakhir Haid` berketerangan "Hanya bermakna bagi sitologi ginekologi" | `PASS` | — |
| C4 ×2 | `AC-153`/`AC-154` pesanan Patologi Klinik (`LAB-RSMMC-000001`) dan Mikrobiologi (`LAB-RSMMC-000014`): bagian **tidak muncul**, layar detail tetap termuat | `PASS` | Percobaan pertama `FAIL` karena pemeriksa uji mencari nomor pesanan pada judul; diperiksa ulang lewat judul *Detail Pesanan Laboratorium* + baris disiplin |
| C5 | Simpan → `PUT` **`200`**; ruas kosong terkirim `null` (opsional) | `PASS` | Badan `{initialDiagnosis, relevantHistory:"Uji layar FE-LAB-29 2026-10-02", lastMenstrualPeriod:null, clinicalNote:null}` |
| C6 | Muat ulang: nilai tersimpan terbaca dari backend | `PASS` | — |
| C7 | Laporan PA (`FE-LAB-28`) membaca konteks yang sama, baca-saja — rantai pemesan → patolog | `PASS` | "RIWAYAT PENYAKIT Uji layar FE-LAB-29 2026-10-02" |
| C8 | 390 px: tanpa gulir horizontal halaman | `PASS` | `375 ≤ 390` |
| C9 | Vina (tanpa `LabOrder : Update`): bagian tampil, isian **terkunci** | **`FAIL` → `PASS`** | Lihat 8.2 |
| C10 | Nol tulis di luar pesanan uji | `PASS` | Satu tulis diteruskan, milik `LAB-RSMMC-000009` |

### 8.2 Satu cacat yang ditemukan uji, dan perbaikannya

**Gejala.** Bagian ini menerima `canWrite` dari pemanggil dengan bawaan `true`, dan pemanggilnya tidak
pernah mengirimkannya. Akibatnya analis tanpa `LabOrder : Update` melihat isian yang dapat diketik dan
tombol `Simpan` — lalu berakhir `403` ketika menyimpan.

| Berkas frontend | Perubahan |
| --- | --- |
| `lib/hooks/health-services/laboratory-management/use-lab-pathology-context.jsx` | `usePermission("LabOrder", "Update")` → dikembalikan sebagai `canWrite` |
| `components/view/…/lab-orders/detail/lab-order-pathology-context-section.jsx` | Bawaan `canWrite` kini dari hook; pemanggil tetap boleh menimpanya |

Penegakan tetap di backend; perbaikan ini hanya membuat layar jujur tentang apa yang boleh dilakukan.
Selama daftar izin belum termuat, `allowed` bernilai benar — konvensi `usePermission` bersama.

**Jejak di database dev.** `LAB-RSMMC-000009`: `initialDiagnosis` tetap `x` (nilai sebelumnya),
`relevantHistory` = *"Uji layar FE-LAB-29 2026-10-02"*.

Validasi akhir sesi: uji unit 2303/2309 (6 gagal = baseline, nol Laboratorium), `lint:errors` 0 error,
`npm run build` hijau. Perbaikan 8.2 belum ter-commit. **Nol operasi Git dijalankan.**
