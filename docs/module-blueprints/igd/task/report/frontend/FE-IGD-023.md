# Laporan Perubahan Frontend — `FE-IGD-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-023` |
| Judul | Tab Penunjang Medis membaca pesanan milik pasien |
| Slice | `IGD-S05` · `EPIC IGD-07` (keterbatasan penunjang dinyatakan di layar) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.6 |
| Trace | `FR-IGD-046`; `IGD-DEC-105`, `IGD-DEC-111`; bukti `IGD-EV-112`, `IGD-EV-115` |
| Contract version | API Laboratorium apa adanya, milik `LaboratoryManagement` — `GET .../lab-orders` dengan `LabOrderPagedQuery`, dibaca dari source backend `rizkiG` `7b0c2ece`. Nol kontrak IGD berubah |
| Wewenang UI | `DEV_DISCRETION` terbatas pada bentuk navigasi halaman (kriteria 4). Tata letak tab dan workspace IGD V2 **tidak** diubah |
| Dependency | `IGD-DEC-111` ✅. Pemesanan radiologi ke `POST rad-orders` **sengaja tidak disambungkan** (`IGD-DEC-111` butir d) |
| Klasifikasi | `LIGHT` — 4 berkas frontend, satu bagian state, satu tab; tanpa route, komponen bersama, atau CSS baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`): slice, constant, dan view pengkajian IGD. Backend: hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `43adae648` (branch `RizkiV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `7b0c2ece` (branch `rizkiG`) |
| Tanggal | 15 September 2026 |
| Status | **Implementation complete.** Lint dan unit test dijalankan dan lulus. **Runtime not verified**: layar belum dibuka di peramban, build belum dijalankan. **Bukan** UAT |

---

## 1. Keadaan yang ditemukan di awal

**Pola lama.** Thunk `fetchLabOrders` pada `emergency-assessment-slice.jsx` memanggil
`GET /v1/health-services/laboratory-management/lab-orders` **tanpa parameter apa pun**. Balasannya
dibaca dengan `unwrapItems`, lalu disaring di browser dengan
`item.encounterId === encounterId`. Komentar di atasnya menyatakan endpoint itu belum menerima
parameter.

**Root cause — kenapa pola itu tidak lagi tepat.** Sejak 4 September 2026 (`259d53ce`,
`a517cdbd`, tim Laboratorium) endpoint tersebut **berhalaman** dengan bawaan 25 baris dan sudah
menerima penyaring `encounterId`. Akibatnya:

1. Permintaan tanpa parameter hanya menerima **25 pesanan terbaru milik semua pasien**.
2. Penyaringan di browser hanya bekerja pada 25 baris itu. Pesanan pasien yang dibuat lebih awal
   tidak pernah sampai ke browser, sehingga tab menyatakan "belum ada pesanan" padahal pesanannya
   ada.
3. Pesanan milik pasien lain tetap terkirim ke browser, walau tidak ditampilkan.

*Contoh:* dalam satu hari tercatat 40 pesanan laboratorium. Pesanan milik Tn. A adalah pesanan
ke-12 dan ke-30 (urutan terbaru dulu). Pola lama hanya menerima pesanan 1–25, sehingga pesanan ke-30
milik Tn. A tidak pernah terlihat.

**Teks radiologi usang.** Keterangan "Yang Belum Tersedia" pada tab menyatakan *"modul Radiologi
belum ada"*, dan komentar `fetchLabProcedureOptions` menyatakan *"`RadiologyManagement` belum ada
berkasnya sama sekali"*. Keduanya keliru sejak modul Radiologi ada (31 Agustus 2026) dan
bertentangan dengan `IGD-DEC-111`.

**Ringkasan cepat.** Kartu "Ringkasan Cepat" menghitung `items.length`. Bila daftar berhalaman,
angka itu hanya jumlah satu halaman.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat atau dokter IGD yang membuka layar pengkajian satu pasien.

1. Petugas membuka pasien dari daftar pengkajian IGD, lalu memilih menu **Penunjang Medis**.
2. Layar menunggu data kunjungan termuat. Setelah `encounterId` diketahui, layar meminta halaman
   pertama pesanan laboratorium **milik encounter itu saja**.
3. Daftar menampilkan pesanan terbaru dulu, beserta status laboratorium, waktu pesanan, dan jumlah
   spesimen. Di bawah daftar tampil *"Menampilkan 1 sampai 25 dari 30 pesanan"*.
4. Bila pesanan lebih dari satu halaman, navigasi halaman muncul. Petugas memilih halaman 2 untuk
   melihat pesanan berikutnya.
5. Petugas dapat memesan pemeriksaan laboratorium baru. Setelah tersimpan, layar kembali ke halaman
   1 karena pesanan baru berada paling atas.
6. Bagian **Yang Belum Tersedia** menjelaskan bahwa hasil pemeriksaan belum dapat ditampilkan, dan
   bahwa pemesanan radiologi dari IGD belum disambungkan sehingga dicatat sebagai pesanan luar
   sistem pada serah terima pasien.

**Jalur tidak normal**

| Keadaan | Yang terjadi |
| --- | --- |
| `encounterId` belum ada | Permintaan **tidak dikirim**. Layar menyatakan encounter belum termuat |
| Encounter tidak punya pesanan | Keadaan kosong biasa, bukan galat |
| Gagal memuat | Pesan galat dan tombol *Coba lagi*. Daftar lama **dikosongkan** |
| Tanpa hak `LabOrder : Read` | Pemberitahuan tanpa hak akses |
| Pindah ke pasien lain | Daftar pasien sebelumnya dibuang; balasan terlambat milik pasien sebelumnya diabaikan |
| Halaman yang dibuka ternyata kosong karena jumlah pesanan berkurang | Layar pindah ke halaman terakhir yang berisi |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Backend (read-only): `LaboratoryManagement/Controllers/LabOrderController.cs`,
  `DTOs/LabOrderDtos.cs` (`LabOrderPagedQuery`, `LabOrderListResponse`),
  `Services/LabOrderService.cs` (`GetListAsync`, `CreateAsync`), `Responses/PagedResult.cs`.
- Frontend: `emergency-assessment-slice.jsx`, `emergency-assessment-diagnostic-support-tab.jsx`,
  `emergency-assessment-section.jsx`, `emergency-assessment-detail-view.jsx`,
  `use-emergency-assessment-detail.jsx`, `emergency-assessment-constant.jsx`,
  `features/pagination/pagination.jsx`, `features/base-features/data-table.jsx`,
  `style/.../emergency-assessment.module.css`, `tests/helpers/alias-resolver.mjs`.
- Dokumen: `MODULE-STATUS.md`, `00-interview-decisions.md` (`IGD-DEC-105`, `111`),
  `roadmap/frontend-roadmap.md` kartu `FE-IGD-023`, `roadmap/requirement-traceability.md`,
  `evidence/2026-09-15-pemeriksaan-status.md` (`IGD-EV-112`, `IGD-EV-115`).

### 3.2 Berkas yang berubah

Seluruhnya di repository frontend `QuilvianSystemFrontendDev`.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx` | Konstanta `EMERGENCY_LAB_ORDER_PAGE_SIZE = 25`, sama dengan bawaan backend |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` | `fetchLabOrders` mengirim `encounterId`, `pageNumber`, `pageSize`, `sortBy`, `sortDirection` dan membaca balasan dengan `unwrapPaged`. Penyaringan di browser dihapus. Tanpa `encounterId`, tidak ada request. State `labOrders` membawa `pageNumber`, `pageSize`, `totalData`, `totalPage`, `encounterId`, `requestId`. Reducer khusus menolak balasan yang bukan milik permintaan terakhir. Komentar radiologi pada `fetchLabProcedureOptions` diperbaiki |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-diagnostic-support-tab.jsx` | Bar ringkasan *"Menampilkan X sampai Y dari Z pesanan"* + `RegionPagination` bila lebih dari satu halaman. Muat ulang setelah pesanan dibuat kembali ke halaman 1. Pesan kosong dibedakan saat encounter belum termuat. Teks radiologi mengikuti `IGD-DEC-111` |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx` | Ringkasan Cepat memakai `totalData` bila bagian itu berhalaman (satu ekspresi) |

`git diff --stat`: 4 berkas, 189 baris ditambah, 34 dihapus.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi tetap `view → hook/slice → InstanceAxios`. Tab sudah men-dispatch thunk sebelum
  task ini; pola itu diikuti, tidak ditambah lapisan baru.
- `unwrapPaged` yang sudah ada di slice dipakai ulang. Tidak ada helper paging baru.
- Konstanta ukuran halaman ditaruh di constants domain, bukan angka lepas di slice.
- Penjaga balasan basi memakai `action.meta.requestId`, pola yang sudah dipakai
  `hr/workforce-profile/workforce-profile-all.jsx`.
- `signal` dari `createAsyncThunk` diteruskan ke Axios.
- Nol komponen bersama, nol CSS global, dan nol CSS module yang diubah. Bar halaman memakai class
  `region-pagination-bar`/`region-pagination-summary` milik `pagination.css`, yang sudah digayakan
  untuk `.pageStack` IGD.

**Gerbang base component** — `UI GATE: 5 elemen — REUSE 4, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Daftar pesanan | `.recordList`/`.recordItem` IGD | `emergency-assessment.module.css` | REUSE | Tidak diubah |
| Memuat/kosong/gagal/tanpa akses/coba lagi | `EmergencyAssessmentSection` | `emergency-assessment-section.jsx` | REUSE | Pesan kosong dibedakan saat encounter belum ada |
| Badge status | `Badge` + `LAB_ORDER_STATUS_*` | tab yang sama | REUSE | Tidak diubah |
| Keterangan radiologi | `EmergencyAssessmentSection available={false}` | tab yang sama | REUSE | Teks saja |
| Jumlah + navigasi halaman | `RegionPagination` + class bar `DataTable` | `features/pagination/pagination.jsx`, `data-table.jsx` baris bar halaman | COMPOSE | Opsi A |

Pilihan yang disajikan: **A** `RegionPagination` + ringkasan (rekomendasi, dipilih); **B** tombol
"muat berikutnya" yang menumpuk baris — berisiko baris ganda atau terlewat bila pesanan baru masuk
di antara dua muatan, dan butuh state tumpukan; **C** mengganti daftar menjadi `DataTable` —
redesign tab, dilarang task.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tiga baris (`sectionSkeleton`); navigasi halaman nonaktif selama memuat |
| Kosong | *"Belum ada pesanan laboratorium pada episode pelayanan ini."* |
| Encounter belum ada | *"Encounter pasien belum termuat, sehingga pesanan laboratorium belum dapat dibaca."* + *"Muat ulang halaman atau periksa pendaftaran pasien."* — tanpa request |
| Gagal | Pesan dari backend (atau *"Gagal mengambil pesanan laboratorium."*) + tombol *Coba lagi* yang memuat ulang halaman yang sama. Daftar lama dikosongkan |
| Tanpa hak akses | *"Anda tidak memiliki hak akses untuk melihat data ini."* |
| Radiologi | *"Hasil pemeriksaan belum dapat ditampilkan di sini: respons pesanan laboratorium belum memuat nilai hasil. Pemesanan radiologi dari IGD belum disambungkan, sehingga permintaan radiologi dicatat sebagai pesanan luar sistem pada serah terima pasien."* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders` (frontend memanggil
`/v1/health-services/laboratory-management/lab-orders` lewat `InstanceAxios`).

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pesanan satu encounter, satu halaman | `LabOrder : Read` |
| `POST` | `/` | Membuat pesanan (tidak diubah task ini) | `LabOrder : Create` |

**Query `GET /` yang dikirim** — nama diambil dari `LabOrderPagedQuery`, bukan ditebak:

| Query | Nilai dari layar | Aturan backend |
| --- | --- | --- |
| `encounterId` | `visit.encounterId` | `Guid?`; kosong/`Guid.Empty` = tanpa penyaring — karena itu layar tidak pernah mengirim tanpa nilai |
| `pageNumber` | 1, atau halaman yang dipilih | Minimal 1 |
| `pageSize` | `25` | Bawaan 25, dibatasi 1–100 |
| `sortBy` | `createDateTime` | `createDateTime` atau `orderStatus`; nilai lain kembali ke bawaan |
| `sortDirection` | `desc` | Bawaan `desc` |

**Balasan** `ApiResponse<PagedResult<LabOrderListResponse>>`: `items`, `totalData`, `totalPage`,
`pageNumber`, `pageSize`.

**Contract mismatch:** `NONE`. Frontend sesudah perubahan cocok dengan source backend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --quiet` pada 4 berkas yang berubah | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run lint:errors` (`eslint . --quiet`, seluruh repository) | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run test:unit` | Gagal sebelum test berjalan: *"Could not find '...\tests\unit\**\*.test.mjs'"*. Node `v20.20.2` tidak mengekspansi pola glob pada `--test` | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; tidak berkaitan dengan perubahan |
| `node --import ./tests/helpers/register.mjs --test tests/unit` (perintah DoD roadmap) | **686 test, 686 lulus, 0 gagal** | `PASS` | Keluaran perintah. Tidak ada test existing yang menyentuh slice pengkajian IGD |
| Skrip cek ad-hoc di scratchpad sesi — **tidak** ditambahkan ke repository | 5/5 lulus: (1) tanpa `encounterId` → 0 request; (2) query persis `{encounterId, pageNumber:1, pageSize:25, sortBy:"createDateTime", sortDirection:"desc"}`, halaman 2 terbaca 5 dari 30; (3) balasan encounter lama yang tiba setelah reset tidak menimpa encounter baru; (4) pindah encounter membuang baris lama, galat mengosongkan daftar; (5) payload `data: null` tidak melempar | `PASS` | Keluaran skrip; `InstanceAxios.get` di-stub |
| Grep anti-regresi pada tab | Nol `<button`/`.btn`, nol `<table`, nol `fw-*`/`fs-*`. Nol stylesheet diubah | `PASS` | Keluaran perintah |
| Grep *"modul Radiologi belum ada"* di `src/` | Nol temuan | `PASS` | Keluaran perintah |
| `npm run build` | Tidak dijalankan agent; perintah diserahkan kepada Rizki sesuai DoD R3.6 | `NOT RUN` | — |
| Tab Penunjang dibuka di peramban untuk pasien yang punya pesanan | Tidak dijalankan | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` — agent tidak menjalankan dev server dan tidak memegang kredensial
petugas; data pesanan lab pasien IGD pada basis data pengembangan tidak diperiksa (agent dilarang
menjalankan kueri basis data).

**Tidak dijalankan:** `npm run build` (diserahkan ke Rizki), `test:e2e`, `test:uat`, tangkapan
layar. Tidak ada test baru yang ditambahkan ke repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Request membawa `encounterId`, `pageNumber`, dan `pageSize`; penyaringan pesanan di browser dihapus | Terpenuhi | `fetchLabOrders` — `params: { encounterId, pageNumber, pageSize, sortBy, sortDirection }`; `items.filter(item?.encounterId …)` dihapus (grep nol temuan); cek ad-hoc 2 |
| 2. Tanpa `encounterId`, request tidak dikirim sama sekali | Terpenuhi | `if (!encounterId) return { items: [] … }` sebelum `InstanceAxios.get`; cek ad-hoc 1 |
| 3. Pesanan pasien tetap tampil walau banyak pesanan lain dibuat sesudahnya | Terpenuhi pada source | Penyaringan kini di backend (`LabOrderService.GetListAsync` `Where(EncounterId == …)` sebelum `Skip/Take`), sehingga pesanan pasien lain tidak menempati halaman. Belum dibuktikan di runtime |
| 4. Bila `totalData` lebih besar dari baris yang dimuat, layar menyebut jumlah seluruhnya dan menyediakan cara memuat sisanya memakai paging backend | Terpenuhi | Bar *"Menampilkan X sampai Y dari Z pesanan"* + `RegionPagination` bila `totalPage > 1`, memanggil `fetchLabOrders({ encounterId, pageNumber })`; Ringkasan Cepat memakai `totalData` |
| 5. *"modul Radiologi belum ada"* tidak muncul di layar maupun komentar; pengganti menyatakan belum disambungkan dan dicatat sebagai pesanan luar sistem pada serah terima | Terpenuhi | `unavailableMessage` tab dan komentar `fetchLabProcedureOptions` (`IGD-DEC-111`); grep `src/` nol temuan |
| 6. Nol tombol pemesanan radiologi, nol integrasi Radiologi, nol tombol alur di dalam laboratorium | Terpenuhi | Diff tidak menambah tombol selain navigasi halaman; nol URL `radiology-management`; nol panggilan `start-process`/`complete`/`hold`/`resume`/`cancel` |
| 7. Keterangan *"hasil pemeriksaan belum dapat ditampilkan"* tetap ada | Terpenuhi | Kalimat pertama `unavailableMessage`. `LabOrderListResponse` memuat `isResultFinal` dan `resultAvailabilityNote`, tetapi **tidak** memuat nilai hasil |
| 8. Nol perubahan backend | Terpenuhi | `git status --short` backend hanya menunjukkan berkas `docs/` |

**Definition of Done R3.6**

| Butir | Status |
| --- | --- |
| Acceptance criteria terpetakan ke source | Ya — 8/8 |
| `npm run lint:errors` dijalankan, hasil dicatat | Ya — exit 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit` dijalankan, hasil dicatat | Ya — 686/686 |
| Perintah `npm run build` diberikan kepada Rizki | Ya — lihat bagian 8 |
| Catatan uji layar ditulis apa adanya | Ya — `NOT FEASIBLE`, beserta alasannya |
| Laporan tracked | Ya — berkas ini |
| Roadmap dan traceability diperbarui | Ya |
| Nol komponen bersama dan CSS global diubah | Ya |
| Tanpa UAT PASS | Ya |

**Pembedaan status:** Requirement approved = Ya · Delivery planned = Ya · **Implementation
complete = Ya** · **Runtime verified = Belum**.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Git mencatat *"LF will be replaced by CRLF"* pada tiga berkas — perilaku `core.autocrlf` repository, bukan perubahan isi |
| Masalah yang diketahui | (1) `npm run test:unit` tidak berjalan di Node 20 karena pola glob — `EXISTING / ENVIRONMENT ISSUE`, tidak diperbaiki (di luar lingkup). (2) Belum ada test otomatis untuk slice pengkajian IGD; cek ad-hoc tidak disimpan. (3) Radiologi tetap belum disambungkan, ditahan `IGD-DEC-111` butir (d) sampai `ActAsRadiologist` dapat diberikan — pemilik Yoga Aji Pratama. **Tidak** membuat task ini terblokir |
| Dependency backend | `NONE` untuk kriteria task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git frontend | ` M .../emergency-assessment-diagnostic-support-tab.jsx`, ` M .../emergency-assessment-detail-view.jsx`, ` M .../emergency-assessment-constant.jsx`, ` M .../emergency-assessment-slice.jsx` |
| Langkah berikutnya | Rizki menjalankan `npm run build` dari `QuilvianSystemFrontendDev`, lalu membuka tab **Penunjang Medis** untuk pasien IGD yang punya lebih dari 25 pesanan pada encounter-nya (atau satu pesanan di antara banyak pesanan pasien lain) dan mencatat hasilnya sebagai bukti runtime |
