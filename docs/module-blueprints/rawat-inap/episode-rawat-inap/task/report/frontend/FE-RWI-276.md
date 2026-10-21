# Laporan Perubahan Frontend — `FE-RWI-276`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-276` |
| Judul | Peta bangsal Lantai 2 |
| Slice | `PETA-S2` — lima lantai lain (`MVP-PETA-2`) |
| Roadmap | [`frontend-roadmap-peta-bangsal-denah.md`](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md), kartu `FE-RWI-276` |
| Trace | `FR-DNH-017`, `FR-DNH-018` untuk Lantai 2; `RWI-DEC-344`; `RWI-AC-498`, `RWI-AC-499`; `DNH-AT-498`, `499`, `918`; `RWI-FE-010` (bentuk lantai dan blok zona), `RWI-FE-011` (varian kartu ringkas) |
| Contract version | `0.18.0` — `approved_design_planned` lewat `RWI-DEC-352`. Hash lock dicocokkan ulang 21 Oktober 2026: 19 dari 19 masukan desain sama. API contract 14.2 tidak berubah; berkas denah backend tidak berubah sejak commit snapshot |
| Wewenang UI | Frontend architecture 21.3: bentuk lantai, koridor, penanda, dan blok zona adalah `DEV_DISCRETION` (`RWI-FE-010`); kartu ringkas, huruf, ukuran, dan jarak adalah `DEV_DISCRETION` (`RWI-FE-011`). Ditambah pilihan pemilik 21 Oktober 2026: sayap utara Lantai 2 ditampilkan sebagai blok bernama. Batasnya: kartu tidak boleh keluar dari poligon kamar; label ruang teknis, garis sumbu, dan angka ukuran tidak ditampilkan |
| Dependency | Gerbang `DEC-PETA-L3`, sama dengan `FE-RWI-277` dan `FE-RWI-278`: pengesahan Lantai 3 sudah dinyatakan pemilik pada sesi 21 Oktober 2026, tetapi pencatatannya sebagai keputusan baru di decision log **belum ada**. Task sempat ditunda atas instruksi pemilik ("lantai 2 di skip saja dulu"), lalu dibuka kembali pada hari yang sama ("oke sekarang buatkan lantai 2"). Tidak ada prasyarat backend |
| Klasifikasi | `HEAVY` — skor 9: dua repository 2, berkas diperiksa > 20 bernilai 2, berkas diubah 5 bernilai 1, logika geometri dan susunan kartu ringkas 1, memakai kontrak API yang ada 1, database 0, keamanan 0, empat pemakai layar 2 |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository sempit untuk laporan ini, roadmap, dan traceability |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan `tests/unit/**`; laporan ini di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `57e8be299` (branch `HamzahV2`, upstream `origin/HamzahV2`; sudah memuat `FE-RWI-277` dan `FE-RWI-278`) ditambah perubahan working tree task ini |
| Commit backend yang dijadikan rujukan | `be254d87eff2d0050c8724ec3aee727e105ca4cb` (branch `MHamzah`) |
| Tanggal | 21 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Seluruh acceptance criteria terpetakan ke source; skrip geometri 40/40, `npm run lint:errors`, dan `npm run build` lulus. Yang belum hanya pencatatan pengesahan Lantai 3 sebagai keputusan baru di decision log, yaitu syarat gerbang `DEC-PETA-L3`. Uji manual di aplikasi belum dijalankan; butir ini tidak menahan `✅` atas keputusan pemilik 1 September 2026 |

---

## 1. Keadaan yang ditemukan di awal

1. Lantai 2 masih digambar dari garis gambar kerja (CAD), lengkap dengan tekstur taman, sumbu 0–100
   dan W–Z, angka ukuran, kanopi bertangga di selatan, dan judul lembar "2nd Floor". Lantai 3, 4, 5, 8,
   dan 9 sudah berupa peta bangsal (`FE-RWI-272`, `FE-RWI-277`, `FE-RWI-278`).
2. `rooms[]` Lantai 2 berisi enam poligon persegi: R. Bayi 1 dan 2 (Perinatologi, masing-masing 4
   boks), Ruang ICU (3 bed), ICU VIP 1 dan 2, dan ICU CK (masing-masing 1 bed). Tidak ada ruang
   cadangan Master Bed di lantai ini.
3. Berbeda dari Lantai 3–5, bagian di atas blok utama Lantai 2 bukan ruang servis biasa, tetapi unit
   klinis dan fasilitas: Taman, Kafetaria, R. Cath Lab, Nurse Station, R. Farmasi, dan R. OK
   (K. Bersih/K. Kotor). Cakupan roadmap memintanya tampil sebagai blok zona, sedangkan revisi
   pemilik pada Lantai 3–5 selalu menghilangkan bagian utara. Pemilik memilih **menampilkannya
   sebagai blok bernama** (bagian 2A).
4. Kode kamar yang dikirim server untuk Lantai 2 berupa kode pendek, misalnya `BAYI-2`, `ICU-CK`, dan
   `ICU-VIP-2`, bukan nomor tiga angka. Pada varian kartu ringkas, kode itu tidak muat bersama badge
   dalam satu baris. Contoh: kartu ICU-CK hanya selebar 41 satuan, sedangkan "ICU-CK" ditambah badge
   `1/1` butuh ± 54 satuan, sehingga isinya meluber ± 8,5 satuan di kedua sisi.
5. Kamar ICU VIP 2 (83 × 65,5 satuan, luas 5.436 satuan²) sudah masuk varian ringkas menurut aturan
   luas `FE-RWI-277`, padahal roadmap hanya menyebut BAYI-1, BAYI-2, dan ICU-CK.

Satuan pada laporan ini adalah satuan gambar sumber denah: 1 satuan = 1 piksel pada gambar
`denah-map-ruangan-rs-mmc/lantai-2.png` berukuran 1536 × 1024.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Petugas melihat Lantai 2 — lantai Perinatologi dan ICU di tengah unit tindakan — sebagai
peta bangsal yang bersih seperti lantai lain, dengan enam kamar rawat yang menonjol di antara blok unit
lain yang bernama.

**Pelaku.** Petugas Monitoring Bed, petugas admisi (Pilih Bed, termasuk admisi bayi), perawat yang
memindahkan pasien (detail episode dan Workspace Keperawatan), dan admin Master Bed.

**Pemicu.** Pengguna membuka tab **Lantai 2** di salah satu dari keempat layar denah.

**Prasyarat.** Data denah lantai itu dikirim server (satu panggilan per lantai, tidak berubah).

**Langkah utama.**

1. Pengguna membuka salah satu layar denah, lalu memilih tab **Lantai 2**.
2. Panggung denah menampilkan lempeng lantai berbentuk blok utama dengan sayap utara di atasnya.
3. Sayap utara berisi blok bernama Taman, Kafetaria, Cath Lab, Farmasi, OK bersih, dan OK kotor, serta
   penanda Nurse Station dan Toilet.
4. Blok utama menampilkan kartu R. Bayi 1 dan 2 di sisi barat dengan penanda R. NS Bayi di sebelahnya,
   blok Ruang tunggu, lalu kartu Ruang ICU, ICU VIP 1, ICU VIP 2, dan ICU CK di sisi timur.
5. Baris tengah berisi blok Hemodialisa, Persiapan endoskopi, R. Operasi, Endoskopi & diagnosa, dan
   CSSD, serta penanda Lift, Tangga, dan R. Dokter. Koridor utama berlabel "KORIDOR" memisahkannya dari
   baris selatan: blok OK 1, OK 2, OK 3, void, dan ruang kecil barat daya.
6. Klik, Enter, atau Spasi pada kartu membuka pop up kamar yang sama seperti sebelumnya. Zoom, geser,
   `Ctrl` + scroll, layar penuh, dan kompas tetap bekerja.

**Aturan bisnis yang terlihat di layar.**

- **Varian kartu ringkas yang menyesuaikan isi.** Kartu ringkas pertama-tama memakai susunan lama:
  nomor dan badge sebaris, titik bed di bawahnya. Bila tidak muat, kartu membandingkan susunan sebaris
  dan susunan bertumpuk (nomor di atas, lalu titik bed dan badge sebaris), memilih yang paling sedikit
  perlu diperkecil, lalu memperkecil isi secara seragam sampai muat. Ukuran aslinya terlihat saat peta
  diperbesar.
  **Contoh:** kartu ICU-CK berukuran 41 × 42,5 satuan. Susunan sebaris butuh lebar 67,8 satuan (skala
  0,546), sedangkan susunan bertumpuk hanya butuh 38,3 satuan (skala 0,967), sehingga dipakai susunan
  bertumpuk pada 97 % ukuran asli. Bila kamar penuh dan berlabel HD, isinya bertambah satu baris
  sehingga tampil pada 81 % ukuran asli.
- **Skala per kamar pada keadaan terberat** (badge penuh, label HD, dan ikon pesanan):

| Kamar | Ukuran kartu | Tanpa penanda | Dengan penanda terberat |
| --- | --- | --- | --- |
| BAYI-1 | 58 × 40,5 | 0,947 | 0,796 |
| BAYI-2 | 53,5 × 40,5 | 0,868 | 0,767 |
| ICU VIP 2 | 77 × 59,5 | 1 (bertumpuk) | 1 (bertumpuk) |
| ICU CK | 41 × 42,5 | 0,967 | 0,811 |
| 520, 521, 522 (Lantai 5) | — | 1 (susunan lama) | 1 (susunan lama) |

**Perubahan status.** Tidak ada. Rupa status kartu tetap mengikuti `RWI-DEC-346` dan `RWI-DEC-350`.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Kamar ada di denah tetapi tidak dikirim server | Bentuk putus-putus tanpa nomor, tidak dapat diklik (urutan 7, tidak berubah) |
| Denah gagal dimuat | Tidak berubah: pesan galat dan tombol Coba Lagi dari layar pemakai |
| Mode Master | Tidak ada ruang cadangan di Lantai 2, jadi seluruh kartu memakai aturan Master yang sama dengan lantai lain |

**Hasil akhir.** Keenam lantai kini tampil sebagai peta bangsal. Data, kunci denah, dan aksi tidak
berubah. Jalur gambar lama masih disimpan sampai `FE-RWI-279`.

### 2A. Pilihan pemilik 21 Oktober 2026 — sayap utara Lantai 2

**Pertanyaan yang diajukan.** "Lantai 2: bagian di atas blok utama (Taman, Kafetaria, R. Cath Lab,
Nurse Station, R. Farmasi, R. OK K. Bersih/K. Kotor) mau dihilangkan seperti Lantai 3–5, atau
ditampilkan sebagai blok bernama?"

**Jawaban pemilik.** "Tampilkan sebagai blok bernama" — sesuai cakupan roadmap `FE-RWI-276`.

| Bagian lantai | Hasil |
| --- | --- |
| Baris fungsional sayap utara (y 262–449) | Ditampilkan: Taman, Kafetaria, blok utilitas (R. Panel Listrik, tanpa label teknis), Cath Lab, Nurse Station (penanda), Toilet (penanda), Farmasi, OK bersih, OK kotor |
| Lajur paling utara (tangga darurat, inti lift dan tangga servis, jalur drop-off melingkar) | Tidak dimuat, sesuai pratinjau pilihan yang disetujui |
| Kanopi bertangga di selatan void | Tidak dimuat; lempeng berhenti di dinding selatan blok utama (y 857) |

**Contoh dampak di layar.** Lempeng Lantai 2 berbentuk dua bertingkat: blok utama selebar 1.289
satuan, dan di atasnya sayap utara selebar 997 satuan. Kompas berada di ruang kosong di atas ICU VIP,
tidak menutupi kartu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance dan blueprint: sama dengan laporan [`FE-RWI-277`](FE-RWI-277.md) bagian 3.1; ditambah
  planning 5.9 baris Lantai 2, gambar `denah-map-ruangan-rs-mmc/lantai-2.png`, dan
  `UI-Sudah-Diterapkan/lantai-2.png`.
- Source frontend: `lantai-2.plan.js` (`rooms[]`, label gambar), `floor-plan-room-map.js` (kode
  pendek Lantai 2), `floor-plan-bed-capacities.js`, `floor-plan-import-manifest.js` (nomor kamar yang
  diimpor = kode pendek), `floor-plan-room.jsx` (`CompactCardContent`, `CardIndicators`, `BedDots`),
  `floors.js`, dan uji denah yang ada.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/lantai-2.schematic.js` | **Baru.** Lempeng 8 titik (blok utama `x 91–1380`, `y 439–867`, ditambah sayap utara `x 150–1147`, `y 252–439`); `viewBox` `x 71, y 162, 1329 × 725`; 9 jalur sirkulasi termasuk koridor utama `y 717–755`; 34 blok zona (15 berlabel); 9 penanda |
| `src/lib/constants/health-services/inpatient-management/floor-plan/floors.js` | Lantai 2 mendapat properti `schematic`; komentar menyebut keenam lantai kini berskematik |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room.jsx` | `CompactCardContent`: bila isi tidak muat pada ukuran asli, isi diserahkan ke komponen baru `FittedCompactContent` yang memilih susunan sebaris atau bertumpuk, lalu memperkecil seragam lewat `transform`. Bila isi muat, perhitungan dan markup sama persis dengan sebelumnya |
| `tests/unit/floor-plan-schematic-floors.test.mjs` | Lantai 2 ditambahkan ke skrip geometri dengan uji bernama `FE-RWI-276: …`: 6 poligon, kartu di dalam poligon, tanpa tumpang tindih, di dalam lempeng dan `viewBox`, dan tanpa label teknis (termasuk "Panel"). Ada uji khusus sayap utara bernama, lajur paling utara dan kanopi yang tidak dimuat, serta penanda wajib. Uji lempeng/bingkai kini membandingkan kotak batas sehingga lempeng 8 titik ikut diperiksa. Uji varian ringkas kini memodelkan skala isi ringkas pada keadaan terberat (minimal 0,75; kamar 520–522 tetap 1) |
| `tests/unit/floor-plan-schematic-geometry.test.mjs` | Uji `FE-RWI-272` yang mengunci daftar lantai berskematik kini mengunci keenam lantai |

Hook, service, kontrak API, route, menu, permission, `package.json`, CSS, backend, dan database
tidak berubah. `rooms[]`, `floorPlanKey`, dan berkas CAD tidak disentuh.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi dan bentuk data mengikuti `lantai-3.schematic.js`. Aturan bingkai (lempeng 10
  satuan di luar dinding luar, `viewBox` 20/90 satuan) diperiksa satu uji untuk keenam lantai.
- Perubahan renderer berada di fungsi presentasi kartu ringkas di folder feature denah
  (`RWI-DEC-325` butir 4). Tidak ada state, request, atau CSS baru; skala memakai atribut SVG
  `transform`.

**Keputusan elemen UI (base component decision gate).** Base component tidak wajib untuk modul denah
(`RWI-DEC-325` butir 4), dan tidak ada base component SVG untuk kartu denah.

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Lapisan peta Lantai 2 (lempeng 8 titik, zona, koridor) | `FloorPlanSchematic` | `roundedOrthogonalPath` sudah mendukung poligon tegak lurus bersudut dalam | `REUSE` | Data skematik baru |
| Ikon penanda (tangga, lift, NS, toilet, ruang dokter) | `LANDMARK_ICONS` `lucide-react` | Kelima jenis sudah ada | `REUSE` | Tanpa ikon baru |
| Label blok zona (15 unit) | `Zone` di `FloorPlanSchematic` | Prop `label` sudah didukung; uji lebar label lulus | `REUSE` | — |
| Kartu Ruang ICU dan ICU VIP 1 | `FloorPlanRoom` varian kartu | Isi bertumpuk muat | `REUSE` | — |
| Kartu ringkas BAYI-1, BAYI-2, ICU VIP 2, ICU CK | `FloorPlanRoom` `CompactCardContent` | Kode pendek tidak muat sebaris | `EXTEND` | `FittedCompactContent` |

`UI GATE: 5 elemen — REUSE 4, EXTEND 1, COMPOSE 0, WRAP 0, NEW 0`

**Keputusan: kartu ringkas yang kode kamarnya tidak muat**

- **A. Pilih susunan sebaris atau bertumpuk, lalu perkecil isi seragam bila masih tidak muat —
  Rekomendasi, dijalankan.** Isi kartu selalu di dalam kartu, nomor, badge, titik bed, dan penanda
  tetap tampil, dan ukuran asli terlihat saat diperbesar. Kartu yang sudah muat (520, 521, 522) tidak
  berubah sama sekali, dibuktikan perbandingan markup di bagian 6.
- **B. Memendekkan kode yang ditampilkan, misalnya "CK" untuk ICU-CK.** Muat tanpa skala, tetapi
  nomor di kartu berbeda dari nomor di pop up, Master Bed, dan data server.
- **C. Menyembunyikan badge atau titik bed pada kartu kecil.** Paling sederhana, tetapi melanggar
  `DNH-AT-504` yang meminta badge kapasitas/terisi di setiap kartu.

Keputusan ini `DEV_DISCRETION` (`RWI-FE-011`) dan bukan perubahan perilaku base component, sehingga
tidak menunggu pemilik.

**Grep anti-regresi (`ui-consistency-checklist` bagian G).** Tidak ada stylesheet yang diubah. Pada
92 baris JSX yang ditambah dan data Lantai 2: tidak ada `<button>`, `<table>`, utilitas tipografi
Bootstrap, `style=`, warna literal, maupun `!important`.

**Tafsiran yang dicatat.**

1. ICU VIP 2 tetap memakai varian ringkas menurut aturan luas `FE-RWI-277` (5.436 < 6.000 satuan²),
   menambah daftar roadmap yang hanya menyebut BAYI-1, BAYI-2, dan ICU-CK. Pada kartu biasa, isi ICU
   VIP 2 meluber pada keadaan berpesanan. Pada varian ringkas bertumpuk, isinya tampil penuh pada
   ukuran asli, misalnya "ICU-VIP-2" / titik dan `1/0` / label HD.
2. Penyederhanaan zona mengikuti larangan "tidak menggambar detail ruang unit lain": R. Loker OK dan
   R. Dokter satu blok berpenanda R. Dokter; Endoskopi, Diagnosa, dan ruang di sebelahnya satu blok
   "Endoskopi & diagnosa"; ruang-ruang kecil barat daya dua blok polos; void satu blok polos tanpa
   label. Ejaan label memakai Bahasa Indonesia ("Persiapan endoskopi") walau gambar menulis
   "ENDOSCOPY" dan "Endosocpi".
3. "R. OK (K. BERSIH)" dan "R. OK (K. KOTOR)" ditulis "OK bersih" dan "OK kotor" supaya muat di
   bloknya tanpa menebak arti singkatan "K.".
4. Taman memakai rupa blok abu seperti blok lain, sesuai cakupan roadmap "(blok polos)" dan pilihan
   pemilik; usulan blok hijau-pucat di planning 5.9 tidak dipakai.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka berbentuk kartu dari `FE-RWI-274`, sama untuk semua lantai (tidak berubah) |
| Kosong | Lempeng, koridor, blok zona, dan penanda tetap tampil; kamar tanpa data API tampil putus-putus tanpa nomor |
| Gagal | Tidak berubah: "Denah gagal dimuat. Muat ulang." dengan tombol Coba Lagi/Muat Ulang Denah dari layar pemakai |
| Tanpa hak akses | Tidak berubah: Master Bed memakai `AccessDeniedGate`; layar lain mengikuti halaman induknya |

---

## 5. Endpoint yang dikonsumsi

Task ini tidak menambah atau mengubah panggilan API. Daftar endpoint, grup `[Tags(...)]`, dan hak
aksesnya sama persis dengan laporan [`FE-RWI-277`](FE-RWI-277.md) bagian 5:
`GET .../bed-management/floor-plan` (`InpatientBedManagement : Read`),
`GET .../bed-occupancies/floor-plan` (`InpatientBedOccupancy : Read`), dan
`GET .../master-data/rooms/floor-plan-layout` (`Room : Read`), satu panggilan per lantai.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/floor-plan-schematic-floors.test.mjs tests/unit/floor-plan-schematic-geometry.test.mjs` | 40 uji, 40 lulus: 5 uji baru Lantai 2, uji lintas lantai yang diperluas, dan 12 uji Lantai 3 | `PASS` | Keluaran `node:test` |
| `npm run test:unit` | 3383 uji: 3346 lulus, 37 gagal di 28 berkas uji di luar denah; tidak satu pun berkas yang gagal membaca berkas task ini. Jumlah kegagalan sama dengan sebelum task (37) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah dan pencarian silang berkas uji |
| `npm run lint:errors` | `eslint . --quiet` selesai dengan exit 0 tanpa satu pun galat | `PASS` | Keluaran perintah |
| `npx eslint . -f json` | 5456 berkas, `0 errors`, 1086 warning — sama dengan pengukuran sesudah `FE-RWI-277`/`278`; kelima berkas task ini 0 warning | `PASS` | Ringkasan keluaran JSON |
| `npm run build` | `✓ Compiled successfully in 109s` (Next.js 16.2.12, Turbopack; lebih lambat karena berjalan bersamaan dengan ESLint), 514 halaman statis, `prepare-standalone` berhasil, exit 0, tanpa warning build. Sebelum build dipastikan tidak ada proses `node .next/standalone/server.js` | `PASS` | Keluaran perintah |
| Perbandingan markup kartu `HEAD` (`57e8be299`) vs working tree, semua kamar × 6 keadaan data × 3 mode | Lantai 2: 108 render, yang berubah hanya `r-bayi-1`, `r-bayi-2`, `r-icu-vip-2`, `r-icu-ck`. Lantai 3, 4, 5, 8, 9: 1.800 render, **tidak ada kartu berubah**, termasuk kartu ringkas 520–522 | `PASS` | Skrip render server di luar repository |
| Tumpang-susun data skematik di atas `lantai-2.png` | Lempeng, koridor, blok zona, dan penanda jatuh di ruang yang benar; satu celah koridor di persimpangan R. NS Bayi–Persiapan Endoscopy ditemukan dan ditutup | Bukti pendukung | Screenshot Edge di luar repository |
| Pratinjau statis komponen asli (data samaran, enam keadaan per kamar) | Lantai 2 tampil sebagai peta bangsal tanpa unsur CAD; kartu ringkas BAYI-1, BAYI-2, ICU VIP 2, dan ICU CK muat di kartunya pada keadaan terberat (penuh HD, berpesanan) | Bukti pendukung, bukan uji manual | Screenshot Edge di luar repository, tidak dilampirkan |
| Uji di aplikasi: Lantai 2 di Monitoring, Pilih Bed (termasuk admisi bayi), transfer, dan Master | Belum dijalankan | `NOT RUN` | Butuh login dan data backend; dijalankan pengguna |

Uji manual: `NOT FEASIBLE` untuk agent, karena aplikasi butuh login dan data API backend. Tidak ada
kontrol interaktif yang berubah. Uji manual di peramban tidak menahan `✅` atas keputusan pemilik
1 September 2026, tetapi dianjurkan dijalankan pengguna saat meninjau.

`AUTOMATED TEST: node --test tests/unit/floor-plan-schematic-floors.test.mjs tests/unit/floor-plan-schematic-geometry.test.mjs — PASS (40/40)`

`AUTOMATED TEST: npm run test:unit — FAIL (37 dari 3383 gagal di 28 berkas uji di luar denah; uji denah lulus)`

**Tidak dijalankan:** uji di aplikasi dan jejak jaringan, karena butuh sesi login dan backend yang
berjalan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `DNH-AT-498` lulus untuk Lantai 2 — tanpa dinding, pintu, anak tangga, sumbu, angka ukuran, label teknis, tekstur taman, dan judul lembar | Terpenuhi di source; belum diuji di aplikasi | `floors.js` memberi `schematic`; uji "data peta Lantai 2 tidak memuat unsur gambar kerja"; pratinjau statis |
| `DNH-AT-499` lulus untuk Lantai 2 — 6 kartu di dalam poligon `rooms[]`, kelas dan nomor dari API | Terpenuhi di source; belum diuji di aplikasi | Uji "terdaftar berskematik dengan 6 poligon kamar"; kartu memakai `room.classLabel` dan `room.roomNumber` dari API |
| `DNH-AT-918` lulus untuk Lantai 2 — kartu di dalam poligon, tidak menimpa koridor dan penanda, teks muat, varian ringkas BAYI-1, BAYI-2, ICU-CK | Terpenuhi; ICU VIP 2 ikut ringkas (tafsiran 1) | `tests/unit/floor-plan-schematic-floors.test.mjs`; tabel skala bagian 2 |
| Keenam kamar dapat dibuka di keempat pemakai | Terpenuhi di source; belum diuji di aplikasi | Jalur klik, Enter, dan Spasi `CardRoom` tidak berubah; ketiga berkas pemakai meneruskan `state.floor.schematic` (uji `FE-RWI-272`) |
| Verifikasi wajib: bandingkan dengan `lantai-2.png` | Terpenuhi | Tumpang-susun di atas gambar sumber |

| Butir DoD (`D-FE`) | Status |
| --- | --- |
| Setiap acceptance criteria terpetakan ke source yang ada | Terpenuhi |
| `V-FE` 1 — baca ulang source dan `git diff` sebelum edit | Terpenuhi; berkas denah bersih saat mulai (perubahan task sebelumnya sudah di-commit sebagai `57e8be299`) |
| `V-FE` 2 — `lint:errors` | Terpenuhi: exit 0, `0 errors` |
| `V-FE` 3 — `test:unit` | Dijalankan: 3346/3383 lulus; 37 kegagalan bukan regresi task |
| `V-FE` 4 — `build` | Terpenuhi: `✓ Compiled successfully`, exit 0 |
| `V-FE` 5 — bukti visual | Pratinjau statis komponen asli dan tumpang-susun gambar sumber, di luar repository; uji di aplikasi belum dijalankan (tidak menahan `✅`, keputusan 1 September 2026) |
| Tanpa perubahan hook, service, kontrak API, route, menu, permission, package, backend, database | Terpenuhi |
| Laporan dan titik status roadmap serta traceability | Terpenuhi pada perubahan yang sama |
| Gerbang `DEC-PETA-L3` | **Belum tertutup penuh**: pengesahan dinyatakan pemilik 21 Oktober 2026, pencatatan sebagai keputusan baru di decision log belum ada. Inilah satu-satunya alasan status `🟡` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning lint pada berkas task ini |
| Masalah yang diketahui | Penyederhanaan `RWI-FE-010` dicatat di komentar kepala berkas data. Pada keadaan terberat, isi kartu BAYI-2 tampil pada 77 % ukuran asli; petugas dapat memperbesar peta |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Berkas pratinjau, skrip render, dan screenshot hanya ada di folder kerja agent di luar repository |
| Interupsi | `NONE` |
| Status Git | `M floor-plan-room.jsx`, `M floors.js`, `M tests/unit/floor-plan-schematic-floors.test.mjs`, `M tests/unit/floor-plan-schematic-geometry.test.mjs`, `?? schematic/lantai-2.schematic.js`. Tidak ada berkas lain yang berubah di working tree frontend |
| Langkah berikutnya | Catat pengesahan Lantai 3 dan arahan 21 Oktober 2026 (tanpa area servis Lantai 4–5; sayap utara Lantai 2 ditampilkan bernama) sebagai keputusan baru di decision log, lalu naikkan `FE-RWI-276` s.d. `FE-RWI-278` ke `✅` dan buka `FE-RWI-279` |
