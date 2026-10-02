# Laporan Perubahan Frontend — `FE-LAB-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-12` |
| Judul | Daftar dan detail penerimaan |
| Slice | `MVP-5a`, `EPIC-LAB-11` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6b |
| Trace | `FR-11.5`, `FR-11.8`, `LAB-DEC-042`, `LAB-DEC-045`, `LAB-DEC-074`; `AC-67`, `AC-75` |
| Contract version | `LAB-API-v1` **`r16`** dan **`r17`** — `approved` 2026-09-17 |
| Wewenang UI | Tata letak `DEV_DISCRETION`; aturan tanggal Laboratorium berlaku module-wide |
| Dependency | `FE-LAB-11` ✅, `BE-LAB-37` ✅, `BE-LAB-38` ✅ |
| Klasifikasi | `MEDIUM` — dua layar baca, satu thunk, satu berkas aturan murni |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `686038858` |
| Commit backend yang dijadikan rujukan | `13665452` |
| Tanggal | 2026-09-17 |
| Status | **`SELESAI`** — seluruh butir DoD terpenuhi. **`MVP-5a` tuntas** |

---

## 1. Keadaan yang ditemukan di awal

Task ini **sempat ditandai ⛔ `TERTAHAN` pada hari yang sama**, pada pemeriksaan
pra-implementasi, sebelum satu baris pun ditulis: **nol endpoint mengembalikan daftar wadah
lintas pesanan.** Yang ada hanya `GET /summary` (angka rekap, nol baris) dan
`GET /by-order/{id}` (satu pesanan).

Penahannya terangkat pada hari yang sama juga: `r17` disetujui pemilik modul lalu dilaksanakan
`BE-LAB-38`, dan `r16`/`BE-LAB-37` menyediakan `orderNumber` yang ikut dibutuhkan.

**Setengah pekerjaannya sudah ada sejak `BE-LAB-22`** — `GetSummaryAsync` menyaring tepat pada
`PhysicallyReceivedAt ?? CreateDateTime`, dan `AC-67` terpenuhi di sana. Yang hilang **barisnya,
bukan aturannya**.

---

## 2. Proses bisnis dari sisi pengguna

**Dua pembaca, satu layar.** Petugas menelusuri penerimaan yang sudah dicatat; kepala instalasi
membaca laporannya menurut waktu kedatangan sebenarnya.

1. Membuka menu **Laporan Penerimaan**.
2. Layar menampilkan setiap wadah beserta **No. Order**, pasien, jenis specimen, dan **tiga kolom
   waktu**: kapan ia **tiba**, kapan ia **dicatat**, dan **selisih** keduanya.
3. Rentang tanggal disaring menurut **waktu kedatangan** — wadah yang tiba Senin 21.10 dan baru
   dicatat Selasa 08.05 muncul pada hari **Senin**.
4. Klik dua kali membuka detail: satu wadah beserta **setiap pemeriksaan sebagai barisnya
   sendiri**.

### 2.1 Jalur tidak normal

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Wadah tanpa waktu tiba tercatat | Kolom Tiba memakai waktu pencatatan, **dan ditandai** *"Diturunkan dari waktu pencatatan"* |
| Selisihnya tidak dapat dihitung | Kolom Selisih menampilkan `-`, bukan angka karangan |
| Rentang terbalik | Kalimat sebabnya muncul, dan **permintaan ditahan** |
| Tautan detail tidak sah | Diminta membuka ulang dari daftar; jalur baca tidak pernah dipanggil |
| Wadah tidak ditemukan | Kalimatnya disebut, bukan layar kosong |

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/.../lab-specimen.service.js` | **+1** `getLabSpecimenReceptions` |
| `src/lib/state/slice/.../lab-specimen-slice.jsx` | **+1 thunk**, **+1 potongan state** `receptions` beserta reducernya |
| `src/lib/hooks/.../lab-reception-report-rules.js` | **Baru.** Aturan murni: waktu kedatangan berlaku, selisih, parameter, penyaring pemeriksaan per wadah |
| `src/lib/hooks/.../use-lab-reception-report.jsx` | **Baru.** Controller daftar |
| `src/lib/hooks/.../use-lab-reception-detail.jsx` | **Baru.** Controller detail |
| `src/components/view/.../lab-reception-reports/lab-reception-report-view.jsx` | **Baru.** Layar daftar |
| `.../lab-reception-detail-view.jsx` | **Baru.** Layar detail |
| `src/app/.../lab-reception-reports/` | **Baru.** Route daftar, `[slug]` detail, penjaga token |
| `src/style/.../lab-reception-reports/...css` | **Baru.** Hanya token |
| `src/utils/menu-sidebar/menu-items.jsx` | **+1 entri** — `Laporan Penerimaan` |
| `tests/unit/lab-reception-report-rules.test.mjs` | **Baru, 10 uji** |
| `tests/e2e/lab-reception-report-screen.spec.mjs` | **Baru, 4 pemeriksaan layar** |

### 3.1 Tiga keputusan yang pantas dibaca ulang

**1. Penyaringan dikerjakan backend, bukan di browser.** Layar mengirim rentangnya lalu
menampilkan apa yang kembali. Menyaring ulang di sini adalah cara paling cepat membuat angka pada
layar berbeda dari angka pada rekap — dua jawaban untuk satu pertanyaan.

**2. Ekspresi waktu kedatangan ditulis sama persis dengan backend.**
`physicallyReceivedAt ?? createDateTime`. Menuliskannya berbeda membuat layar dan daftar bercerita
lain **tanpa satu pun galat muncul** — dan uji unit `S3` menguncinya.

**3. Detail memuat datanya sendiri**, lewat `GET /lab-specimens/by-order/{labOrderId}` — bukan
dari daftar yang kebetulan masih ada di memori. Itulah yang membuat tautan langsung dan muat ulang
berfungsi; pelajaran `LAB-API-v1` `r6`, yang lahir karena `FE-LAB-03` memuat barisnya dari halaman
daftar lalu diam-diam gagal di luar itu. Thunk yang dipakai **sudah ada**:
`fetchLabSpecimenWorkspace` membawa wadah dan pemeriksaannya sekaligus.

### 3.2 Aturan tanggal dipakai ulang, bukan disalin

`validateDateRange` dan `todayDateValue` diimpor dari `lab-monitoring-rules` — aturan yang
`LAB-DEC-074` nyatakan **berlaku module-wide**. Ini pemakaian ulang ketiga berkas itu, dan
alasannya sama setiap kali: dua definisi "hari ini" pada satu modul adalah cara aturan bergeser
tanpa ada yang menyadarinya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Mengambil daftar penerimaan..."*; detail menampilkan alertnya sendiri |
| Kosong | *"Belum ada penerimaan pada rentang ini."* beserta ajakan memperlebar rentang |
| Gagal | Pesan backend apa adanya; rentang tidak sah memunculkan **sebabnya** |
| Tanpa hak akses | Dibungkus `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/.../lab-specimens` | Daftar penerimaan lintas pesanan (`r17`) | `LabSpecimen : Read` |
| `GET` | `/v1/.../lab-specimens/by-order/{labOrderId}` | Detail satu penerimaan beserta pemeriksaannya | `LabSpecimen : Read` |

---

## 6. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint --quiet` pada berkas yang disentuh | Nol keluaran — **`PASS`** |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **1043/1043 lulus** — **`PASS`** |
| `npm run build` | Lulus; kedua route terdaftar — **`PASS`** |
| `npx playwright test tests/e2e/lab-reception-report-screen.spec.mjs` | **4 passed** — **`PASS`** |
| **`AC-76` regresi** — seluruh spec Laboratorium lain | **18 passed** — **`PASS`** |

### 6.1 Empat pemeriksaan layar

| Pemeriksaan | Yang dibuktikan |
| --- | --- |
| `AC-67` | Wadah tiba Senin 21.10, dicatat Selasa 08.05 → baris menampilkan **"10 jam 55 menit setelah tiba"**. Petugas tidak menghitung sendiri |
| `LAB-DEC-042` | Baris tanpa waktu tiba tercatat **ditandai** *"Diturunkan dari waktu pencatatan"*, **dan** selisihnya tidak dihitung |
| `AC-99` module-wide | Rentang terbalik memunculkan sebabnya, **dan nol permintaan tambahan berangkat** |
| Penyaring status | Permintaan pertama **tanpa** penyaring; sesudah status dipilih, `specimenStatus=Accepted` benar-benar terkirim |

**Pemeriksaan kedua adalah yang paling mudah terlewat.** Tanpa penanda itu, waktu pencatatan akan
terbaca sebagai waktu kedatangan — persis kekeliruan yang `LAB-DEC-042` ada untuk mencegahnya, dan
persis kekeliruan yang **tidak menimbulkan galat apa pun**.

### 6.2 Satu kekeliruan uji unit dicatat

Uji selisih versi pertama mengharapkan `"10 jam setelah tiba"`, padahal 655 menit adalah
**10 jam 55 menit**. Ekspektasinya yang keliru, bukan kodenya: pada laporan keterlambatan,
pembulatan ke jam justru membuang ketelitian yang dicari. Uji diperbaiki, kode dipertahankan.

### 6.3 Grep anti-regresi

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal, `!important`, tombol non-base, `<table>` mentah, Bootstrap utility | **Kosong** |
| Typography pada style baru | Memakai token, menyasar class milik layar ini sendiri |

---

## 7. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Daftar memakai **waktu penerimaan nyata**, bukan waktu sistem | **Terpenuhi** | 6.1; backend membuktikannya dua arah pada `BE-LAB-38` |
| Selisih terhadap waktu sistem dapat dilihat kepala instalasi | **Terpenuhi** | Kolom Selisih; uji unit dan layar |
| Layar detail menampilkan setiap baris pemeriksaan **secara terpisah** | **Terpenuhi** | Uji unit S10; detail merender satu `li` per pemeriksaan |
| Tautan langsung dan muat ulang berfungsi | **Terpenuhi** | Detail memuat datanya sendiri lewat `by-order`; token route privat beserta `allowUuidFallback` |

| AC | Status |
| --- | --- |
| `AC-67` | **Terpenuhi pada sisi layar**, melengkapi sisi backend |
| `AC-75` | **Terpenuhi** — penerimaan yang dicatat dapat ditelusuri kembali |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning baru |
| Masalah yang diketahui | Pencarian bebas menjangkau barcode wadah dan nomor order; **nama pasien dan No. RM belum ikut dicari** — batas yang diwarisi dari `BE-LAB-38` dan dilaporkan di sana. Kedua ruas tetap **ditampilkan** |
| Dependency backend | `NONE` yang tersisa |
| Perubahan sampingan | `NONE`. `test-results/` dihapus sesudah pemeriksaan |
| Interupsi | `NONE` |
| Status Git | 41 entri; nol `git add`, `commit`, maupun `push` |
| Langkah berikutnya | **`MVP-5a` tuntas.** Yang tersisa pada modul ini bukan pekerjaan frontend: `LAB-REQ-007`, `LAB-REQ-008`, dan `LAB-REQ-009` menunggu jawaban pihak lain, dan `S17` tetap tertahan `LAB-SIGN-001` |
