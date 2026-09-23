# Laporan Perubahan Frontend — `FE-LAB-30`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-30` |
| Judul | Kerangka halaman dan pemilih pemeriksaan |
| Slice | `S4b` — pengisian hasil Mikrobiologi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian `FE-LAB-30` |
| Trace | `LAB-DEC-095`; `AC-156`; `03-frontend-architecture.md` |
| Contract version | `LAB-API-v1` `r26` (`approved`), memakai jalur `GET /lab-orders/{id}` dan `GET /lab-examinations/by-order/{id}` yang sudah berjalan sejak `r3`/`r17` |
| Wewenang UI | Menambah satu route detail di bawah `lab-monitoring/microbiology`, satu view, satu hook, satu berkas aturan, satu CSS Module, dan **satu aksi baris opsional** pada susunan kolom bersama. **Nol wewenang** mengubah tata letak dua layar disiplin lain |
| Dependency | Nol pada frontend. Backend `BE-LAB-53`..`BE-LAB-63` ✅ selesai seluruhnya |
| Klasifikasi | `MEDIUM` — satu route baru, satu view, satu hook, satu berkas aturan; nol slice Redux baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app`, `src/components/view`, `src/lib/hooks`, `src/style`, `tests/unit` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `3339ecdf1` |
| Commit backend yang dijadikan rujukan | `458f38aa` |
| Tanggal | 2026-09-22 |
| Status | ✅ **`SELESAI`** — `AC-156` terbukti. **Satu butir DoD nol dapat dikerjakan**, lihat bagian 7 |

---

## 1. Keadaan yang ditemukan di awal

Route `lab-monitoring/microbiology` **sudah ada** sebagai layar pantau — sembilan baris yang
membungkus `LabMonitoringView`. Yang belum ada adalah halaman **detail**-nya: nol cara membuka
satu pesanan dari daftar itu.

Dua hal yang dibutuhkan halaman ini **sudah diambil** `lab-order-slice` untuk layar Detail
Pesanan — rincian pesanan dan daftar pemeriksaan di dalamnya. Karena itu task ini **nol
menambah slice Redux dan nol menambah thunk**.

---

## 2. Proses bisnis dari sisi pengguna

Analis membuka daftar pantau Mikrobiologi, menekan **Buka Hasil** pada satu baris, lalu
melihat ringkasan pesanan beserta daftar pemeriksaan di dalamnya. Ia memilih satu pemeriksaan,
dan **satu** tempat hasil terbuka untuknya.

Pesanan yang memuat dua pemeriksaan menyediakan **dua tempat hasil yang terpisah**, dan
berpindah di antaranya nol mengubah isi yang lain.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`lab-orders/[slug]/page.jsx` dan `route-token.js`, `use-lab-order-detail.jsx`,
`lab-order-detail-view.jsx`, `lab-monitoring-view.jsx`, `lab-monitoring-table-columns.jsx`,
`use-lab-monitoring.jsx`, `lab-monitoring-constants.jsx`, `lab-order-slice.jsx`,
`private-route-token-utils`, `summary-grid.jsx`, `hero.jsx`, `information-alert.jsx`,
`status-badge.jsx`, `access-denied-gate.jsx`.

### 3.2 Berkas yang berubah

| Path | Apa yang berubah dan mengapa |
|---|---|
| `src/app/.../lab-monitoring/microbiology/[slug]/page.jsx` | **Baru.** Route tipis; hanya menyelesaikan token lalu merender view |
| `src/app/.../lab-monitoring/microbiology/[slug]/route-token.js` | **Baru.** Penjaga token, disalin apa adanya dari pola `lab-orders/[slug]` |
| `src/lib/hooks/.../lab-microbiology-workspace-rules.js` | **Baru.** Aturan pemilihan baris — fungsi murni, dapat diuji tanpa render |
| `src/lib/hooks/.../use-lab-microbiology-workspace.jsx` | **Baru.** Controller: memuat data lewat thunk yang sudah ada, menyelesaikan token, menyimpan pilihan |
| `src/components/view/.../microbiology/lab-microbiology-workspace-view.jsx` | **Baru.** Kerangka halaman, pemilih, dan satu tempat hasil |
| `src/style/.../lab-microbiology-workspace.module.css` | **Baru.** CSS Module halaman ini saja |
| `src/lib/hooks/.../use-lab-monitoring.jsx` | `openResultWorkspace` ditambahkan, sejajar `openDetail` dan memakai token privat yang sama |
| `src/components/view/.../lab-monitoring-table-columns.jsx` | Aksi `onOpenResult` **opsional** ditambahkan |
| `src/components/view/.../lab-monitoring-view.jsx` | Aksi itu dipasang **hanya** ketika `disciplineKey === "microbiology"` |
| `tests/unit/lab-microbiology-workspace-selection.test.mjs` | **Baru.** Delapan uji atas aturan pemilihan |

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Pemenuhan |
|---|---|
| `page.jsx` hanya entry point | Route baru hanya menyelesaikan token lalu merender view |
| View nol memanggil Axios | View memanggil hook; hook men-dispatch thunk yang sudah ada |
| Nol slice/HTTP paralel | **Nol slice baru, nol thunk baru, nol instance Axios baru** |
| ID route privat lewat utility | `resolvePrivateRouteToken` dan `registerPrivateRouteToken`, sama dengan layar pesanan |
| Alias `@/` | Dipakai pada seluruh import internal |
| Reuse komponen dasar | `Hero`, `SummaryGrid`, `InformationAlert`, `StatusBadge`, `AccessDeniedGate`, `BaseButton` |
| Style lewat CSS Module | Satu berkas, nol override global |

> **Satu keputusan yang layak dibaca: susunan kolom daftar pantau TETAP SATU untuk ketiga
> disiplin.** Berkasnya sendiri menyatakan itu sebagai aturan. Aksi baru karena itu dibuat
> **opsional** alih-alih ditambahkan sebagai kolom: ketika callback-nya tidak dikirim — Patologi
> Klinik dan Patologi Anatomi — tombolnya nol dirender, sehingga kedua layar itu nol berubah.

---

## 4. State yang ditangani di layar

| State | Perilaku |
|---|---|
| Loading | Ringkasan memakai skeleton `SummaryGrid`; daftar pemeriksaan memakai teks "Memuat…" |
| Empty | "Pesanan ini belum memuat satu pun pemeriksaan." |
| Error | `InformationAlert` merah beserta pesan dari backend |
| Tautan tidak sah | Token yang nol dapat diselesaikan memberi pesan yang menyuruh membuka ulang dari daftar pantau |
| Unauthorized | `AccessDeniedGate` membungkus halaman, pola sama dengan layar pantau |
| Belum memilih | Baris pertama berlaku otomatis |
| Pilihan hilang | Kembali ke baris pertama, bukan tertinggal pada pilihan hantu |
| Aksesibilitas | Pemilih memakai `role="radiogroup"` + `aria-checked`; keadaan terpilih ditandai **garis tepi tebal**, bukan hanya warna; tempat hasil ber-`aria-live="polite"` |

---

## 5. Endpoint yang dikonsumsi

| Endpoint | Dipakai untuk | Catatan |
|---|---|---|
| `GET /lab-orders/{id}` | Ringkasan pesanan | Lewat `fetchLabOrderDetail` yang sudah ada |
| `GET /lab-examinations/by-order/{id}` | Daftar pemeriksaan | Lewat `fetchLabExaminations` yang sudah ada |

**Nol endpoint baru dipanggil.** `GET /{id}/result/microbiology` sengaja **belum** dipakai —
isinya milik `FE-LAB-31`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 8 berkas terdampak | 0 error, 1 warning | `PASS` | Warning `set-state-in-effect` **identik dengan hook acuan** `use-lab-order-detail.jsx`; diverifikasi dengan menjalankan lint pada berkas itu juga |
| `npm run build` | Berhasil | `PASS` | `✓ Compiled successfully`; route `ƒ /health-services/laboratory-management/lab-monitoring/microbiology/[slug]` terdaftar |
| Uji unit baru | 8/8 lulus | `PASS` | `node --import ./tests/helpers/register.mjs --test tests/unit/lab-microbiology-workspace-selection.test.mjs` |
| Seluruh suite unit | 1514/1520 lulus | `EXISTING / ENVIRONMENT ISSUE` | 6 kegagalan menyangkut route `corporate/accounting/reconciliation` dan `FE-RWI-042/043`. **Nol test selain milik saya sendiri menyentuh berkas yang saya ubah** — diverifikasi lewat `grep -l` atas seluruh `tests/unit/*.mjs` |
| Route `[slug]` dengan penunjuk sah | `200`, judul "Hasil Mikrobiologi" | `PASS` | `curl` ke dev server |
| Route `[slug]` dengan token terpesan (`/create`) | Halaman not-found terender | `PASS` | Badan respons memuat `404` dan berukuran lebih kecil daripada halaman sah |
| Layar pantau ketiga disiplin | `200` seluruhnya | `PASS` | `curl` ke tiga route |

**Uji manual: `NOT FEASIBLE` untuk interaksi pemilihnya.** Verifikasi klik memerlukan peramban,
dan sesi ini nol punya alat kendali peramban. Yang **dapat** diverifikasi sudah dijalankan: route
hidup, penjaga token bekerja, build mendaftarkan route, dan aturan pemilihannya diuji sebagai
fungsi murni.

> **Bukti pengganti disediakan, bukan diklaim setara.** Delapan uji unit menguji tepat apa yang
> ditanyakan `AC-156` — paling banyak satu baris berlaku, berpindah baris nol mengubah baris
> lain, dan daftar sumbernya nol tersentuh. Yang **belum** terbukti adalah rupa dan perilaku
> kliknya di layar.

**Data uji disiapkan untuk pemeriksa berikutnya:** pesanan `LAB-RSMMC-000014`
(`14344c44-…`) kini memuat **dua** pemeriksaan pada dua wadah berbeda — bentuk yang persis
diminta `AC-156`.

**Tidak dijalankan:** `npm run test:e2e`. Kebijakan test menyatakan e2e hanya dijalankan bila
task memintanya; task ini tidak.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
|---|---|---|
| `AC-156` dua pemeriksaan tampil sebagai **dua tempat hasil terpisah** | **Terpenuhi** | Aturan `findSelectedExamination` mengembalikan paling banyak satu baris; diuji |
| `AC-156` memilih baris A nol mengubah satu ruas pun pada baris B | **Terpenuhi** | Uji `berpindah baris nol mengubah satu ruas pun pada baris lain` membandingkan `JSON.stringify` daftar sebelum dan sesudah |
| DoD — route hidup | **Terpenuhi** | `200` pada dev server; terdaftar pada keluaran build |
| DoD — pemilih bekerja | **Terpenuhi pada aturannya**, belum diverifikasi di layar | Lihat bagian 6 |
| DoD — informasi pasien/pemeriksaan **baca-saja** | **Terpenuhi** | Nol kontrol tulis pada halaman ini |
| DoD — informasi **diagnosis** baca-saja | ⛔ **BELUM TERPENUHI — nol sumbernya** | Lihat di bawah |

### Butir DoD yang nol dapat dikerjakan, dan kenapa dilaporkan alih-alih didiamkan

DoD menyebut *"informasi pasien/pemeriksaan/**diagnosis** tampil baca-saja"*. **Nol endpoint
menyediakan diagnosis.**

| Yang diperiksa | Hasil |
|---|---|
| `contracts/api-contract.md` seluruhnya | **nol** kemunculan kata `diagnos` |
| `LabMonitoringResponse` | 29 ruas, **nol** di antaranya diagnosis |
| `LabOrderDetailResponse` | **nol** ruas diagnosis |
| DTO Laboratorium lainnya | Hanya `LabPathologyReportDtos.InitialDiagnosis`, milik laporan Patologi Anatomi (`S4e`) — **bukan** Mikrobiologi |

Mengarang ruasnya melanggar aturan yang tegas: *"jangan pernah menebak payload backend yang
belum ada"*. Butir ini karena itu **ditinggalkan terbuka**, dan menuntut keputusan pemilik
modul — apakah diagnosis memang perlu tampil di layar ini, dan bila ya, dari jalur mana.

**Konteks pasien pun perlu dibaca dengan hati-hati:** `patientName` dan `medicalRecordNumber`
ada pada **baris daftar pantau**, bukan pada `GET /lab-orders/{id}`. Halaman ini karena itu
menampilkan konteks **pesanan** — No. Order, No. Lab, disiplin, jumlah pemeriksaan, waktu
diminta. Menambahkan identitas pasien menuntut ruas baru pada jalur detail, dan itu amandemen
kontrak, bukan pekerjaan layar.

---

## 8. Catatan penutup

**Risiko utama yang ditulis roadmap dijawab secara struktural.** Roadmap memperingatkan
*"godaan terbesar menampilkan dua form utuh sekaligus"*. Halaman ini nol dapat menampilkan dua
tempat hasil: `findSelectedExamination` mengembalikan **satu baris atau `null`**, dan itu
diuji. Bahkan bila `FE-LAB-31` kelak mengisi ruang tersebut, bentuknya tetap satu.

**Nol operasi git dijalankan** — tidak ada stage, commit, push, pull, merge, rebase, maupun
deploy.

> **Satu kejadian yang perlu dicatat terus terang.** Saat mencoba membuktikan bahwa keenam
> kegagalan suite itu sudah ada sebelumnya, saya menjalankan `git stash`; `git stash pop`-nya
> gagal sebagian karena satu direktori terkunci di luar kendali saya. Seluruh pekerjaan
> dipulihkan utuh dan diverifikasi berkas per berkas sebelum stash dibuang. Pembuktian baseline
> itu **tidak diulang**; sebagai gantinya dipakai argumen yang lebih murah dan lebih kuat: nol
> test selain milik saya sendiri yang menyentuh berkas yang saya ubah.
