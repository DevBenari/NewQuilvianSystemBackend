# Laporan Perubahan Frontend — `FE-LAB-43`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-43` |
| Judul | Label order pada daftar Pemeriksaan Mikrobiologi |
| Slice | Gelombang `MVP-10b` — `EPIC-LAB-16` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-10`, bagian `FE-LAB-43` |
| Trace | `FR-16.7` (bagian daftar); `LAB-DEC-135`, `LAB-DEC-008`, `LAB-CONFLICT-014` |
| Contract version | `LAB-API-v1` `r35` 30.5; arti `resultProgress` `r34` 29.5 — `approved` 2026-09-25 |
| Wewenang UI | Diputuskan: label **hanya** dari `resultProgress`, tidak dari `orderStatus`, tidak dihitung ulang. `DEV_DISCRETION` yang dipakai: **kolom tersendiri** *Keadaan Order* tepat sesudah *Status Pesanan*, berisi lencana bertulisan; warna lencana |
| Dependency | `BE-LAB-79` ✅; pemetaan label `LAB_RESULT_PROGRESS` dari `FE-LAB-39` ⚠ |
| Klasifikasi | `LIGHT` — 4 berkas diubah, 1 berkas uji baru; nol route, nol endpoint baru, nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | Dimulai di atas `e613321c5`; selama pengerjaan, `FE-LAB-35`..`41` di-commit `45efdd892` dan di-merge `b2e47f653` (branch `YogaV2`, upstream `origin/YogaV2`) oleh pemegang repository. Perubahan task ini berada di atas `b2e47f653`, belum ter-commit |
| Commit backend yang dijadikan rujukan | `c231ed92` (branch `yoga`, merge sesudah `64d709c1`); biner lokal mencerminkan seluruh source Laboratorium — nol berkas `LaboratoryManagement` lebih baru dari biner |
| Tanggal | 2026-10-02 |
| Status | ✅ **`SELESAI`** — kolom *Keadaan Order* hidup pada daftar Mikrobiologi terhadap backend asli; ketiga keadaan (*Dalam Pemeriksaan*, *Selesai*, kosong) teruji; uji unit 6 baru, lint nol peringatan, build hijau, layar 7/7, nol tulis. Keadaan *Selesai* dibuktikan dengan baris yang diubah di peramban — dev belum punya order Mikrobiologi yang seluruh pemeriksaannya dirilis (lihat 6) |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Daftar Pemeriksaan Mikrobiologi tidak menunjukkan apakah sebuah order masih berjalan atau sudah selesai dirilis | `lab-monitoring-table-columns.jsx` — hanya *Status Pesanan* (`orderStatus`) |
| Backend sudah mengirim `resultProgress` per baris pada `GET /lab-monitoring/microbiology` | `BE-LAB-79`; di dev: LAB-RSMMC-000014 `InProgress`, empat order lain kosong |
| Susunan kolom **dipakai bersama** ketiga disiplin; tambahan khusus disiplin memakai opsi yang bawaannya mati (`onOpenResult`) | `buildLabMonitoringColumns` |
| Pemetaan label order sudah ada di satu tempat: `LAB_RESULT_PROGRESS` | `lab-result-status-constants.jsx` (`FE-LAB-39`) |
| `orderStatus` `Completed` berlabel *Selesai* — kata yang sama dengan label order *Selesai*, arti berbeda | `LAB_ORDER_STATUS_LABEL`; `LAB-CONFLICT-014` |

---

## 2. Proses bisnis dari sisi pengguna

Petugas membuka **Laboratorium → Pemeriksaan Mikrobiologi**. Di samping kolom *Status Pesanan* kini ada kolom
**Keadaan Order**:

| Yang terbaca | Artinya | Contoh |
| --- | --- | --- |
| **Dalam Pemeriksaan** | Masih ada pemeriksaan tidak batal yang belum dirilis — termasuk hasil `Sementara` | Kultur urin sudah dirilis, kultur darah masih *Sementara* (`AC-199`) |
| **Selesai** | Seluruh pemeriksaan tidak batal sudah **dirilis** — bukan sekadar Final | — |
| `-` | Order tanpa pemeriksaan tidak batal | — |

*Status Pesanan* tidak berubah. Keduanya sengaja berdampingan dan **tidak** saling menggantikan: sebuah order dapat
berstatus *Selesai* (ditandai selesai secara manual) sementara Keadaan Order-nya masih *Dalam Pemeriksaan*.
Penyaring, pencarian, paginasi, dan aksi baris tidak berubah. Daftar Patologi Klinik dan Patologi Anatomi tidak berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-43`; `contracts/api-contract.md` 29.5, 30.5; `traceability.md` `FR-15.10`, `FR-16.7` | Cakupan dan arti label |
| `task/report/backend/BE-LAB-79.md` | Isi `resultProgress` Mikrobiologi |
| `lab-monitoring-view.jsx`, `lab-monitoring-table-columns.jsx`, `lab-monitoring-rules.js`, `lab-monitoring-slice.jsx`, `lab-result-status-constants.jsx`, `lab-order-constants.jsx` | Pola kolom bersama, normalisasi baris, label yang ada |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/.../lab-monitoring-rules.js` | `readOrderProgressBadge(row)` — `{ label, status }` dari `resultProgress` lewat `LAB_RESULT_PROGRESS`; kosong/tak dikenal → `null` |
| `src/lib/constants/.../lab-result-status-constants.jsx` | `LAB_RESULT_PROGRESS_BADGE` — varian lencana; kata tetap dari `LAB_RESULT_PROGRESS` |
| `src/components/view/.../lab-monitoring/lab-monitoring-table-columns.jsx` | Opsi `showResultProgress` (bawaan mati) → kolom *Keadaan Order* sesudah *Status Pesanan* |
| `src/components/view/.../lab-monitoring/lab-monitoring-view.jsx` | `showResultProgress` hanya untuk `microbiology` |
| `tests/unit/lab-monitoring-fe43-rules.test.mjs` (baru) | 6 uji — lihat 6 |

### 3.3 Kepatuhan arsitektur dan keputusan kecil

| Hal | Keputusan dan alasan |
| --- | --- |
| Satu pemetaan | Kata dari `LAB_RESULT_PROGRESS` yang sama dengan kartu *Keadaan Order* Halaman Hasil PK dan Mikrobiologi; uji unit mengunci kesamaannya dengan `readResultProgressLabel` |
| Kolom tersendiri | Menaruh label di sel *Status Pesanan* akan menjajarkan dua *Selesai* berbeda arti (`LAB-CONFLICT-014`) |
| Hanya Mikrobiologi | Roadmap hanya memberi wewenang daftar Mikrobiologi; daftar Patologi Klinik juga menerima `resultProgress` dari backend (`BE-LAB-76`), tetapi tidak satu task pun menugaskan labelnya di daftar (`FR-15.10` hanya halaman hasil). Opsinya siap — menyalakannya keputusan pemilik modul |
| Header | *Keadaan Order* — kata yang sama dengan kartu di halaman hasil |

`UI GATE: 2 elemen — REUSE 2` — `StatusBadge`, sel `mutedCell` yang sudah ada. Nol komponen baru, nol CSS baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat / kosong / gagal / tanpa hak akses | Tidak berubah — milik daftar Pemeriksaan (`FE-LAB-09`) |
| `resultProgress` kosong atau tak dikenal | `-` pada kolom, bukan label tebakan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Monitoring

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-monitoring/microbiology` | Tetap — kini membaca ruas `resultProgress` per baris | `LabMonitoring : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-monitoring-fe43-rules.test.mjs tests/unit/lab-monitoring-rules.test.mjs` | **49/49** (6 baru) | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2309: 2303 lolos, 6 gagal — **sama dengan baseline** (Hemodialisa ×4, Bank Darah M0, petty cash). Jumlah uji naik 24: 6 milik task ini, 18 dari merge `b2e47f653` | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint --max-warnings=0` lima berkas yang disentuh | Nol masalah | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `npm run build` (server BE/FE dimatikan, port dipastikan bebas) | Exit 0; route `lab-monitoring/microbiology` ada | `PASS` | Keluaran build |

**Uji unit baru (6):** `InProgress` → *Dalam Pemeriksaan*, `AllReleased` → *Selesai*; kosong, `null`, spasi, nilai tak
dikenal, dan huruf kecil → tanpa label; `orderStatus` `Completed` **tidak** menghasilkan *Selesai*; label tidak dihitung
ulang dari daftar pemeriksaan (`AC-199`: kultur urin dirilis + kultur darah `Sementara` → mengikuti `InProgress`
backend); kata sama dengan `readResultProgressLabel` halaman hasil; ruas PascalCase.

**Verifikasi manual** — `next dev` `localhost:3000` terhadap backend lokal, login superadmin lewat formulir, Playwright.
Nol tulis (setiap permintaan non-`GET` dicegat dan dicatat: nol).

| Skenario | Hasil sebenarnya |
| --- | --- |
| L1 Data asli | Kolom *Keadaan Order* ada di samping *Status Pesanan*; 5 order: tepat **1** *Dalam Pemeriksaan* (LAB-RSMMC-000014, `InProgress` dari backend), 4 lainnya `-` |
| L2 Baris diubah: `AllReleased` + `InProcess` | *Status Pesanan* **Sedang Dikerjakan**, *Keadaan Order* **Selesai** |
| L3 Baris diubah: `Completed` + `InProgress`; `Completed` + kosong | *Selesai* · **Dalam Pemeriksaan**; *Selesai* · `-` — `orderStatus` tidak pernah menjadi label order |
| L4 Pencarian | `search=ENC-RSMMC` terkirim; kolom bertahan |
| L5 Patologi Klinik dan Patologi Anatomi | Nol kolom *Keadaan Order* |
| L6 390 px | Tanpa gulir horizontal halaman |
| L7 | Nol tulis; nol galat runtime |

Layar: **7/7** `PASS`.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-monitoring-fe43-rules.test.mjs — PASS`

`MANUAL TEST: PASS`

**Yang tidak teramati dengan data asli:** order Mikrobiologi yang seluruh pemeriksaannya dirilis (*Selesai*) dan
kultur urin dirilis + kultur darah `Sementara` — dev belum punya hasil Mikrobiologi yang dirilis (rilis menunggu
`LAB-COORD-016`). Keduanya diuji lewat baris yang diubah di peramban dan uji unit; isi `resultProgress` itu sendiri
sudah terbukti di backend (`BE-LAB-79`, harness).

**Tidak dijalankan:** `npm run test:e2e` — tidak diminta.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Bagian antarmuka `AC-199` bagi Mikrobiologi — label order benar | **Terpenuhi** | L1–L3; uji unit |
| Label hanya dari `resultProgress`, bukan `orderStatus`, tidak dihitung ulang | **Terpenuhi** | L3; uji unit |
| Unit test pemetaan termasuk nilai kosong | **Terpenuhi** | 6 |
| DoD — label tampil benar pada tiga keadaan | **Terpenuhi** — *Dalam Pemeriksaan* dengan data asli; *Selesai* dan kosong lewat baris yang diubah dan data asli | L1–L3 |
| DoD — laporan | **Terpenuhi** | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol |
| Masalah yang diketahui | **1.** Daftar Pemeriksaan sudah lebih lebar dari kartunya pada 1440 px sebelum task ini (sepuluh kolom; *Diminta*, *Konfirmasi*, dan *Aksi* di luar pandangan, tabel bergulir di dalam kartunya); kolom baru menambah 175 px. Halaman sendiri tidak bergulir menyamping. **2.** Daftar Patologi Klinik belum menampilkan label order walau backend mengirimnya — bukan cakupan task mana pun; opsinya siap |
| Dependency backend | Nol perubahan |
| Perubahan sampingan | `NONE` di repository. Lingkungan: proses `next dev`/backend tertinggal sesudah `TaskStop` dihentikan sebelum build; port dipastikan bebas |
| Interupsi | Repository frontend dan backend di-commit dan di-merge oleh pemegang repository di tengah pengerjaan (`45efdd892`/`b2e47f653`, `64d709c1`/`c231ed92`). Diperiksa: diff task ini tetap utuh di atas HEAD baru, branch dan upstream tidak berubah, verifikasi layar berjalan pada kode sesudah merge |
| Status Git | Frontend (`b2e47f653`): ` M` `lab-monitoring-table-columns.jsx`, `lab-monitoring-view.jsx`, `lab-result-status-constants.jsx`, `lab-monitoring-rules.js`; `??` `tests/unit/lab-monitoring-fe43-rules.test.mjs`. Backend (`c231ed92`): laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-42` — penyaring disiplin pada antrean validasi; task frontend terakhir yang belum dikerjakan |
