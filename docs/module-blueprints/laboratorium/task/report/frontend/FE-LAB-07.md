# Laporan Perubahan Frontend — `FE-LAB-07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-07` |
| Judul | Layar wadah dan pemeriksaan |
| Slice | `S2` — pemisahan wadah fisik dan pemeriksaan terpesan (`roadmap/frontend-roadmap.md` bagian 5, gelombang `MVP-2`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 5 |
| Trace | `FR-07.2`, `FR-02.1` .. `FR-02.3`, `FR-02.5`; `LAB-FE-009`, `LAB-FE-010`; `VAL-05`, `VAL-07`, `VAL-13`; `AC-35`, `AC-36`, `AC-38` |
| Contract version | `LAB-API-v1` r3 grup Lab Specimen dan Lab Examination — `approved`, dikunci 2026-09-02. `LAB-STATE-v1` r2 bagian 3 |
| Wewenang UI | `LAB-FE-009` dan `LAB-FE-010` adalah **invariant keselamatan**: bentuk visualnya `DEV_DISCRETION`, **keberadaannya tidak** (`roadmap/frontend-roadmap.md` bagian 2.3) |
| Dependency | `FE-LAB-06` **selesai**; endpoint dari `BE-LAB-12` dan `BE-LAB-16` **selesai**, keduanya diverifikasi langsung pada source backend |
| Klasifikasi | `HEAVY` — skor 11: repository 1, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan 1, UI/workflow 2 |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — lapis modul `laboratory-management`, store Redux, satu berkas uji, **dan tiga berkas `billing-management`** untuk perbaikan build yang diminta eksplisit (bagian 5.1). `NewQuilvianSystemBackend` — **hanya** laporan ini beserta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | Mulai `72f050b50`; **berpindah ke `c71c02a07` di tengah pengerjaan** — lihat bagian 1.2 |
| Commit backend yang dijadikan rujukan | `8e48841`, branch `yoga` |
| Tanggal | 2026-09-07 |
| Status | **Selesai.** Keempat butir DoD terpenuhi; lint, uji, dan build seluruhnya `PASS`. Verifikasi manual masih tertunda menunggu backend dan data — bagian 5 |

---

## 1. Keadaan yang ditemukan

### 1.1 Dependency backend benar-benar ada

Diperiksa langsung pada source, bukan pada dokumen:

| Yang dicari | Hasil |
| --- | --- |
| `LabSpecimenController` — `plan`, `collect`, `receive`, `accept`, `reject`, `request-recollection`, `hold`, `resume` | **ada**, kedelapan-delapannya |
| `GET /lab-specimens/rejection-reasons` | **ada** |
| `LabExaminationController` — `by-order`, `by-specimen` | **ada** |
| `LabSpecimenStatus` beserta kedelapan nilainya | **ada** |

### 1.2 Repository berpindah commit di tengah pengerjaan

Task dimulai pada `72f050b50`. Di tengah pengerjaan, pemilik repository melakukan merge dari
`origin/QuilvianIntegrationFrontend`, sehingga `HEAD` berpindah ke `c71c02a07`.

**Dua akibatnya, dan keduanya perlu dicatat apa adanya.**

1. **Pekerjaan `FE-LAB-05` ikut tercommit** pada `a3a56ee4b` dan selamat melewati merge —
   diperiksa ulang, seluruh perubahannya masih utuh.
2. **Merge itu membawa masuk tiga berkas `billing-management` yang tidak dapat diparse**, dan
   itu yang kemudian memblokir gerbang build. Rinciannya pada bagian 5.

---

## 2. Proses bisnis

### 2.1 Kenapa layar ini ada

> Satu tabung darah ungu dapat dipakai untuk hemoglobin, leukosit, dan trombosit sekaligus.
> Sebelum `LAB-DEC-024`, satu wadah sama dengan satu pemeriksaan — pasien menerima tiga barcode
> untuk satu kali tusukan jarum.

Sekarang satu wadah menopang beberapa pemeriksaan. Konsekuensinya berbahaya dan menjadi alasan
seluruh bentuk layar ini: **setiap keputusan atas wadah berlaku untuk seluruh isinya.** Petugas
yang menolak wadah tanpa tahu isinya menggugurkan pekerjaan yang tidak ia maksud, dan penolakan
tidak dapat dibatalkan.

### 2.2 Alur normal

| Langkah | Status wadah | Yang dilakukan petugas |
| ---: | --- | --- |
| 1 | — | Merencanakan wadah, memilih pemeriksaan apa saja yang dikerjakan darinya |
| 2 | `Planned` | Mencatat pengambilan dari pasien |
| 3 | `Collected` | Mencatat wadah tiba di laboratorium |
| 4 | `Received` | Memutuskan: **menyatakan layak** atau **menolak** |
| 5a | `Accepted` | Seluruh pemeriksaan menjadi layak tagih sekaligus; faktanya diserahkan ke Billing |
| 5b | `Rejected` | Seluruh pemeriksaan gugur sekaligus; **tidak ada** tagihan terbentuk |
| 6 | `Rejected` | Meminta ambil ulang; wadah pengganti dibuat dengan barcode baru |

Wadah dapat ditahan (`OnHold`) dari status mana pun yang belum terminal, dan dilanjutkan kembali
ke status sebelumnya.

### 2.3 Dua invariant keselamatan, dan di mana persisnya ditegakkan

**`LAB-FE-009` — seluruh isi wadah terlihat sebelum tombol tolak.**

Ditegakkan di **tiga** tempat, dan sengaja berlapis:

| Lapis | Bentuknya |
| --- | --- |
| Data | `resolveSpecimenActions` **tidak membuat** aksi tolak selama isi wadah belum termuat. Tombolnya bukan disembunyikan — ia memang tidak ada |
| Kartu wadah | Isi wadah tampil sebagai daftar **terbuka**, bukan di balik akordeon, tab, atau tombol "lihat isi" |
| Dialog penolakan | Daftar yang sama ditampilkan **lagi**, tepat sebelum tombol konfirmasi |

Ketika isinya belum termuat, aksi menyatakan layak **tetap ada** — ia tidak menggugurkan apa
pun — dan keterangan mengapa tombol tolak tidak muncul ditampilkan apa adanya.

**`LAB-FE-010` — peringatan sebelum konfirmasi.**

Dialog penolakan membawa kalimat ini sebagai pesan utamanya, bukan sebagai catatan kaki:

> *"Menolak wadah ini menggugurkan SELURUH pemeriksaan di bawah sekaligus. Tidak ada satu pun
> yang dapat dipertahankan, dan penolakan tidak dapat dibatalkan."*

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Alasan penolakan belum dipilih | Ditahan sebelum dikirim |
| Alasan menuntut catatan, catatan kosong | Ditahan; ruas catatan ditandai wajib begitu alasannya dipilih |
| Sebab ambil ulang selain kesalahan internal, keterangan kosong | Ditahan; aturannya sama dengan backend |
| Wadah berpindah status di tangan petugas lain (`409`) | Pesan backend ditampilkan apa adanya, dan layar **disegarkan sendiri** supaya yang terlihat sama dengan yang tersimpan |
| Wadah sudah `Accepted` atau `Cancelled` | Tidak ada satu pun tombol aksi |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `roadmap/frontend-roadmap.md` bagian 2.3 dan 5 | Invariant keselamatan, cakupan, dan DoD |
| `contracts/state-transition-matrix.md` bagian 3 | Aksi yang sah per status, beserta jalur yang **dilarang** |
| `Areas/.../LabSpecimenController.cs` beserta DTO-nya | Kontrak sebenarnya: route, verb, hak akses, bentuk permintaan |
| `Areas/.../LaboratoryEnums.cs` | Kedelapan status wadah dan tiga sebab ambil ulang |
| `LabSpecimenService.GetRejectionReasonsAsync` | Bentuk katalog alasan beserta penanda `requiresNote` |
| `use-lab-order-detail.jsx`, `lab-order-detail-view.jsx` | Pola hook, view, dan `ConfirmModal` terdekat |
| `lab-order-urgency-rules.js` beserta ujinya | Pola aturan murni yang dapat diuji tanpa merender |
| `base-component-catalog.md` | Memastikan `ConfirmModal` sudah menyediakan yang dibutuhkan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../constants/.../lab-specimen-constants.jsx` | **Baru.** Label status, warna lencana, sebab ambil ulang, route, dan seluruh salinan teks termasuk kalimat peringatan `LAB-FE-010` |
| `.../hooks/.../lab-specimen-rules.js` | **Baru.** Aturan murni: aksi per status, pengelompokan isi wadah, dan validasi ketiga formulir |
| `.../services/.../lab-specimen.service.js` | **Baru.** Sembilan pemanggilan endpoint; **nol** yang menerima penunjuk pemeriksaan |
| `.../state/slice/.../lab-specimen-slice.jsx` | **Baru.** Wadah dan isinya dimuat **satu thunk bersamaan** |
| `.../hooks/.../use-lab-specimen-workspace.jsx` | **Baru.** Controller layar beserta seluruh dialog konfirmasi |
| `.../view/.../lab-specimens/lab-specimen-workspace-view.jsx` | **Baru.** Kartu wadah, daftar isi, dan tujuh dialog |
| `src/app/.../lab-orders/[slug]/specimens/page.jsx` | **Baru.** Route bersarang, memakai ulang penjaga token milik detail pesanan |
| `src/style/.../lab-specimens/lab-specimen.module.css` | **Baru.** Daftar isi wadah sebagai daftar terbuka |
| `src/lib/state/store.jsx` | Satu potongan Redux didaftarkan |
| `.../hooks/.../use-lab-order-detail.jsx` | Satu fungsi `openSpecimens` |
| `.../view/.../lab-orders/detail/lab-order-detail-view.jsx` | Satu tombol menuju layar wadah |
| `.../hooks/.../use-lab-patient-search.jsx` | Perapian dependency memo — warning milik `FE-LAB-05` |
| `tests/unit/lab-specimen-rules.test.mjs` | **Baru.** Sembilan belas uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tidak ada perubahan.** Layar mengonsumsi `LAB-API-v1` r3 apa adanya |
| Database | **Tidak ada dampak sama sekali** |
| Keamanan/Auth | Setiap tindakan memakai hak akses miliknya sendiri — `LabSpecimen : Plan`, `: Collect`, `: Receive`, `: Accept`, `: Hold`. Layar tidak menebak kewenangan; backend yang menolak, dan penolakannya ditampilkan apa adanya |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Invariant ditegakkan sebagai data, bukan sebagai susunan JSX** | `resolveSpecimenActions` yang memutuskan ada tidaknya aksi tolak, dan ia diuji terpisah. Menaruh penjagaan itu di dalam JSX berarti ia dapat tergeser diam-diam oleh perapian tampilan berikutnya, tanpa satu pun uji ikut gagal |
| 2 | **Wadah dan isinya dimuat satu thunk bersamaan** | Bukan pilihan gaya. Memuat isi belakangan berarti ada saat ketika kartu wadah sudah tampil beserta tombolnya, tetapi isinya belum — persis keadaan yang dilarang `LAB-FE-009` |
| 3 | **Pembatalan satu pemeriksaan sengaja tidak dibuat** | Backend punya `POST /lab-examinations/{id}/cancel`, tetapi membatalkan satu pemeriksaan adalah keputusan klinis yang berbeda dari menolak wadah. Mencampurnya melanggar `VAL-13`, dan `BE-LAB-16` sendiri memperingatkannya. Ada uji yang menjaga ketiadaan aksi berlingkup pemeriksaan pada katalog aksi |
| 4 | **Isi wadah tampil sebagai daftar terbuka, bukan tabel yang dapat digulung** | Apa pun yang dapat ditutup membuka celah untuk memutuskan tanpa melihat |
| 5 | **Route bersarang di bawah pesanan** | Wadah selalu milik satu pesanan; `GET /lab-specimens/by-order/{labOrderId}` bahkan tidak punya bentuk lain. Menu tingkat atas akan menuntut penyaring pesanan yang tidak pernah boleh kosong |
| 6 | **Bentrok `409` menyegarkan layar sendiri** | Wadah yang baru berpindah status di tangan petugas lain membuat tombol yang terlihat sudah tidak sah. Menyegarkan otomatis membuat yang terlihat kembali sama dengan yang tersimpan; perintahnya sendiri **tidak** diulang — itu keputusan petugas |
| 7 | **Aksi ambil ulang muncul pada wadah `Rejected`** | Meskipun `Rejected` adalah status terminal, ia satu-satunya yang punya kelanjutan sah, dan matriks transisi memang mengizinkannya |

---

## 4. Dokumentasi endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Dipakai layar | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/rejection-reasons` | Pilihan alasan penolakan | `LabSpecimen : Read` |
| `GET` | `/by-order/{labOrderId}` | Daftar wadah pesanan | `LabSpecimen : Read` |
| `POST` | `/by-order/{labOrderId}` | Merencanakan wadah | `LabSpecimen : Plan` |
| `POST` | `/{id}/collect` | Mencatat pengambilan | `LabSpecimen : Collect` |
| `POST` | `/{id}/receive` | Mencatat tiba di lab | `LabSpecimen : Receive` |
| `POST` | `/{id}/accept` | Menyatakan layak | `LabSpecimen : Accept` |
| `POST` | `/{id}/reject` | Menolak wadah | `LabSpecimen : Accept` |
| `POST` | `/{id}/request-recollection` | Meminta ambil ulang | `LabSpecimen : Accept` |
| `POST` | `/{id}/hold`, `/{id}/resume` | Menahan dan melanjutkan | `LabSpecimen : Hold` |

#### Health Services / Laboratory Management / Lab Examination

| Method | Path | Dipakai layar | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/by-order/{labOrderId}` | Isi setiap wadah | `LabExamination : Read` |

---

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tests/unit/lab-specimen-rules.test.mjs` | `pass 19, fail 0` | `PASS` |
| Seluruh uji unit (`--test tests/unit/`) | `pass 503, fail 0` | `PASS` |
| ESLint atas seluruh modul `laboratory-management`, `store.jsx`, dan berkas uji baru | **0 error**, 1 warning milik `FE-LAB-06` yang sudah ada sebelumnya | `PASS` |
| `npm run lint:errors` (seluruh repository) | Bersih, tanpa keluaran — **setelah perbaikan bagian 5.1** | `PASS` |
| `npm run build` | `Compiled successfully`; route `/lab-orders/[slug]/specimens` terbentuk — **setelah perbaikan bagian 5.1** | `PASS` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`
(perintah `npm run test:unit` tetap tidak dapat dipakai apa adanya pada Node `v20.20.2`;
alasannya sama dengan yang dicatat `FE-LAB-05.md`, dan `package.json` tidak disentuh).

### 5.1 Perbaikan build `billing-management` — atas instruksi eksplisit pemilik repository

Merge `c71c02a07` membawa masuk tiga berkas `billing-management` berisi **deklarasi kembar**
hasil auto-merge. Tidak ada satu pun marker konflik yang tertinggal, sehingga kerusakannya lolos
tanpa terlihat sampai build dijalankan.

Perbaikannya **semula tidak dikerjakan** — `AGENTS.md` melarang memperbaiki masalah existing
yang tidak berkaitan sebagai efek samping, dan memilih deklarasi mana yang dipertahankan adalah
keputusan pemilik modul Billing. Kerusakannya dilaporkan, lalu pemilik repository **secara
eksplisit meminta build diperbaiki**. Di bawah wewenang itulah perbaikan berikut dikerjakan.

| Berkas | Yang kembar | Yang dipertahankan, dan kenapa |
| --- | --- | --- |
| `.../billing-invoices/billing-invoice-constants.js` | `DOKUMEN_KASIR_PLACEHOLDER_TABS` | **Deklarasi pertama.** Isinya identik, tetapi `DOKUMEN_KASIR_RECOGNIZED_TABS` membacanya saat modul dimuat — mempertahankan yang kedua akan melempar `ReferenceError` karena pembacaannya jatuh di zona mati temporal |
| `.../menu-pembayaran/dokumen-kasir-view.jsx` | `formatMoney` | **Deklarasi pertama.** Kedua blok identik baris per baris |
| `.../slice/.../billing-invoice-slice.jsx` | `getInsuranceInvoiceDocument` | **Deklarasi pertama.** Keduanya memanggil endpoint yang sama dengan logika yang sama; hanya kalimat galat cadangannya berbeda |
| `.../slice/.../billing-invoice-slice.jsx` | Tiga `addCase` untuk thunk yang sama | **Blok pertama.** Ia **lebih lengkap**: jalur `rejected`-nya ikut mengosongkan `insuranceInvoiceDocument`, sehingga dokumen basi tidak tertinggal di layar setelah kegagalan. Blok kedua tidak melakukannya |
| `.../slice/.../billing-invoice-slice.jsx` | Tiga selector `selectInsuranceInvoiceDocument*` | **Deklarasi pertama.** Ketiganya identik baris per baris |

**Satu temuan yang lebih berat daripada gagal build.** `addCase` kembar untuk action type yang
sama membuat Redux Toolkit melempar saat *store* dibentuk — *"addCase cannot be called with two
reducers for the same action type"*. Artinya, seandainya build berhasil pun, aplikasinya tetap
tidak dapat dijalankan sama sekali. Kerusakan ini tidak terbatas pada layar Billing.

**Bentuk perubahannya: 70 baris dihapus, nol baris ditambahkan.** Tidak ada satu pun logika yang
ditulis ulang, diganti, atau dipindahkan — yang dilakukan hanya membuang salinan kedua.

### Verifikasi manual

`MANUAL TEST: NOT FEASIBLE`

Penahan build sudah dicabut pada bagian 5.1, tetapi satu alasan konkret tersisa: layar ini
menuntut data yang belum ada — satu pesanan laboratorium beserta wadah pada beberapa status
berbeda — dan backend yang dihentikan pada sesi sebelumnya belum dijalankan kembali.

**Yang wajib diperiksa manual begitu backend berjalan dan datanya tersedia:**

| No | Yang diperiksa | Yang diharapkan |
| ---: | --- | --- |
| 1 | Membuka layar wadah pada pesanan berisi wadah dengan dua pemeriksaan | Kedua pemeriksaan **terlihat langsung** pada kartu, tanpa perlu membuka apa pun |
| 2 | Menekan **Tolak Wadah** | Dialog memuat **kedua** pemeriksaan **dan** kalimat peringatan, sebelum tombol konfirmasi |
| 3 | Memilih alasan penolakan yang menuntut catatan | Ruas catatan menjadi wajib; konfirmasi ditahan selama kosong |
| 4 | Mencari jalur menolak satu pemeriksaan saja | **Tidak ada** — tidak pada baris pemeriksaan, tidak di mana pun |
| 5 | Menekan **Nyatakan Layak** | Dialog memuat daftar pemeriksaan yang akan menjadi layak tagih |
| 6 | Wadah `Rejected`, menekan **Minta Ambil Ulang** dengan sebab selain kesalahan internal | Keterangan wajib diisi |
| 7 | Merencanakan wadah tanpa memilih pemeriksaan | Ditahan `VAL-05` |
| 8 | Dua petugas memutuskan wadah yang sama bersamaan | Yang kalah menerima pesan `409` dan layarnya tersegarkan sendiri |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-35` — satu wadah menopang beberapa pemeriksaan, masing-masing dengan salinan tarifnya | Terpenuhi di sisi layar | Perencanaan mengirim daftar `examinations`; kartu wadah menampilkan seluruh isinya |
| `AC-36` — menolak wadah menggugurkan seluruh isinya | Terpenuhi di sisi layar | Tidak ada jalur penolakan sebagian; peringatannya dinyatakan eksplisit sebelum konfirmasi |
| `AC-38` — wadah pengganti menunjuk wadah asalnya | Terpenuhi di sisi layar | Alur ambil ulang tersedia pada wadah `Rejected`; penunjuk asal-usul dibuat backend |

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Seluruh pemeriksaan terlihat sebelum tombol tolak | Terpenuhi | Ditegakkan tiga lapis; dua uji menjaga lapis datanya |
| Peringatan muncul sebelum konfirmasi | Terpenuhi | Pesan utama dialog penolakan, bukan catatan kaki |
| Tidak ada jalur penolakan per pemeriksaan | Terpenuhi | Uji memeriksa katalog aksi tidak memuat satu pun aksi berlingkup pemeriksaan, dan tidak ada status selain `Received` yang menawarkan penolakan |
| Alur ambil ulang meminta sebab | Terpenuhi | Sebab wajib; keterangan wajib untuk sebab selain kesalahan internal — dijaga tiga uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Modul `laboratory-management` **0 error**. Satu warning tersisa pada `use-lab-order-detail.jsx` — `set-state-in-effect` milik `FE-LAB-06`, sudah ada sebelum task ini dan tidak diperbaiki sebagai efek samping |
| Masalah yang diketahui | Kerusakan build `billing-management` **sudah diperbaiki** pada sesi yang sama atas instruksi eksplisit pemilik repository — bagian 5.1. Perbaikannya murni membuang salinan kedua; **pemilik modul Billing tetap perlu meninjaunya**, terutama pilihan mempertahankan blok `addCase` yang ikut mengosongkan dokumen basi |
| Risiko tersisa | **Pertama, verifikasi manual belum dijalankan** — delapan skenario pada bagian 5, dan dua invariant keselamatan baru terbukti pada tingkat aturan dan uji, belum pada layar sungguhan. **Kedua**, `use-lab-order-detail.jsx` milik `FE-LAB-06` ikut disentuh secara aditif. **Ketiga**, perbaikan `billing-management` menyentuh modul di luar Laboratorium; kebenarannya dibuktikan lint dan build, **bukan** oleh uji perilaku Billing — modul itu tidak punya uji otomatis |
| Perubahan sampingan | `NONE`. Perbaikan `billing-management` **bukan** perubahan sampingan: ia diminta eksplisit oleh pemilik repository dan dicatat tersendiri pada bagian 5.1 |
| Interupsi | Satu, di luar kendali task: `HEAD` frontend berpindah dari `72f050b50` ke `c71c02a07` di tengah pengerjaan karena merge pemilik repository. Diperiksa ulang — pekerjaan `FE-LAB-05` yang sudah tercommit tetap utuh, dan pekerjaan `FE-LAB-07` yang sedang berjalan tidak tertimpa |
| Status Git | Tujuh berkas `M` — empat milik Laboratorium, tiga `billing-management` — dan sembilan entri `??`. **Tidak ada** `git add`, `commit`, maupun `push` |
| Langkah berikutnya | 1. Menjalankan backend, lalu menjalankan delapan skenario verifikasi manual. 2. Pemilik modul Billing meninjau perbaikan bagian 5.1. 3. `FE-LAB-08` dan `FE-LAB-09` — keduanya berantai dan pasangan backendnya sudah selesai |
