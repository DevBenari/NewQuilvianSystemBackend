# Laporan Perubahan Frontend — `FE-BD-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-004` |
| Judul | Petugas mengalokasikan kantong dan membatalkan alokasi |
| Slice | Slice 3 — Kantong darah, pemberian, dan penyelesaiannya |
| Roadmap | [roadmap/frontend-roadmap.md](../../../roadmap/frontend-roadmap.md), kartu `FE-BD-004` |
| Trace | `DEC-BD-003`, `DEC-BD-007`, `DEC-BD-014`, `DEC-BD-023`, `DEC-BD-029`, `AC-BD-033/043/044/045/046/060/068/070`, `VAL-BD-018c`, `03-frontend-architecture.md` §1 (`FE-BD-04`, `FE-BD-05`), §2 (peta menu), kewajiban layar `FE-BD-002` |
| Keputusan pemilik | `G1`–`G5`, `Sukmagp` 24 September 2026 — lihat bagian 1.1 |
| Contract version | api-contract `v4` — Blood Unit, baris `allocate` dan `cancel-allocation`. Disetujui. Perilaku runtime dibaca dari source backend `BE-BD-006` ✅ |
| Wewenang UI | Layar `FE-BD-04` (daftar kantong + saringan `PendingReview`) dan `FE-BD-05` sebatas alokasi dan pembatalan alokasi. Rupa layar `DEV_DISCRETION`. **Tidak** mencakup penetapan/perpindahan lokasi dan saringan `inactiveLocation` (`FE-BD-012`), bukti kecocokan dan pemberian (`FE-BD-005`), penyelesaian `PendingReview` (`FE-BD-007`), maupun koreksi (`FE-BD-008`) |
| Dependency | `BE-BD-006` ✅ — kedua endpoint terbukti ada di `BbkBloodUnitController.cs` |
| Klasifikasi | `MEDIUM` — satu layar daftar, satu layar detail, dua aksi tulis, nol arsitektur baru, mengikuti pola `FE-BD-010` |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` (source) + berkas laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `a5f4be551` (branch `sukmagpV2`) — dasar kerja; source task ini **belum di-commit** |
| Commit backend yang dijadikan rujukan | `d93be8fa` (branch `sukmagp`) |
| Tanggal | 24 September 2026 |
| Status | ✅ **Selesai** — source lengkap, `lint:errors`/`test:unit`/`build` `PASS`, dan **16 dari 16 pemeriksaan runtime di browser sungguhan PASS** (validasi ulang 24 September 2026, bagian 6.2) |

---

## 1. Keadaan yang ditemukan di awal

Frontend **tidak punya satu baris pun** source kantong darah. Pencarian `grep -rln "blood-units\|BloodUnit" src`
memulangkan **0 hasil**, dan sidebar Bank Darah hanya memuat Order Darah, Permintaan PMI, Tindakan
Bank Darah, dan Setup. Padahal backend sudah lengkap: `GET /blood-units`, `GET /{id}`,
`POST /{id}/allocate`, dan `POST /{id}/cancel-allocation` ada sejak `BE-BD-006` ✅.

Layar daftar kantong (`FE-BD-04`) dan kerangka layar kerja kantong (`FE-BD-05`) semula milik
`FE-BD-012`, yang tertahan menunggu `BE-BD-020`. Karena itu task ini tidak dapat berjalan tanpa
keputusan cakupan.

Temuan audit backend yang membentuk keputusan pemilik:

| Temuan | Bukti |
| --- | --- |
| Kantong **tidak menyimpan golongan darah** | `BloodUnitListDto` dan `BloodUnitDetailDto` tanpa field golongan darah |
| Alokasi **tidak memeriksa** kecocokan komponen maupun golongan darah | `AllocateAsync` langkah 1–13; risiko tercatat di laporan `BE-BD-006` §7 |
| Alokasi **tidak membatasi jumlah** kantong per baris kebutuhan | `CheckOrderLineAsync` hanya memeriksa status order dan kunjungan; `OutstandingQuantity` hanya menghitung kantong yang sudah diberikan |
| Enum dikirim sebagai **angka** | Tidak ada `JsonStringEnumConverter`; `PendingReview` = `5` |

### 1.1 Keputusan pemilik, 24 September 2026

| Kode | Keputusan |
| --- | --- |
| `G1` | Task ini membangun **layar kantong minimal**: daftar kantong, saringan `PendingReview`, detail kantong, alokasi, dan pembatalan alokasi. Penetapan lokasi, perpindahan lokasi, dan saringan `inactiveLocation` tetap milik `FE-BD-012` |
| `G2` | Tambah butir menu **Bank Darah → Kantong Darah**, dijaga `BloodUnit : Read` |
| `G3` | **Tidak ada** validasi komponen atau golongan darah di frontend. Backend adalah sumber kebenaran |
| `G4` | Tidak ada task backend baru untuk penjaga jumlah alokasi. Frontend tidak menghitung aturan bisnis; hasil backend ditampilkan apa adanya; celahnya dicatat sebagai risiko |
| `G5` | Acceptance: alokasi `PASS`, pembatalan `PASS`, gerbang hak akses, alasan berkategori `AllocationCancellation`, status ditentukan backend, penanganan konflik versi, saringan `PendingReview`, **tidak ada** penyelesaian `PendingReview` |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas Bank Darah yang memegang `BloodUnit : Read`, dan untuk aksi tulis
`BloodUnit : Allocate`.

### 2.1 Menemukan kantong

1. Petugas membuka **Bank Darah → Kantong Darah**.
2. Di atas tabel tampil lima angka dari backend: Total Kantong, Tersedia, Dialokasikan, Menunggu
   Keputusan, dan Berlebih.
3. Tabel memuat nomor kantong PMI, komponen, asal permintaan PMI, **lokasi penyimpanan saat ini**
   beserta penanda **Lokasi nonaktif**, serta status kantong beserta penanda **Berlebih**.
4. Petugas mencari lewat kotak pencarian, menyaring status, atau menekan tombol
   **Menunggu Keputusan** untuk membuka daftar kerja #2 — kantong yang menunggu keputusan.
5. Klik dua kali pada baris membuka detail kantong.

### 2.2 Mengalokasikan kantong

1. Pada detail kantong **Tersedia**, petugas menekan **Alokasikan**.
2. Dialog menampilkan 100 order darah terbaru, masing-masing dengan status ordernya, misalnya
   "ORD-2026-000021 — Budi Santoso · Aktif".
3. Sesudah order dipilih, layar memuat ordernya dan menampilkan pasien serta status order. Pemilih
   **Baris Kebutuhan** lalu terisi seluruh baris order itu, misalnya
   "Baris 2 — Trombosit (diminta 4 · diberikan 1)". Angka diminta dan diberikan dibaca dari backend.
4. Petugas memilih satu baris lalu menekan **Alokasikan**. Layar mengirim **tepat dua isian**:
   baris kebutuhan dan token versi kantong.
5. Berhasil → status kantong menjadi **Dialokasikan**, bagian **Alokasi Aktif** menampilkan order,
   baris, pasien tujuan, dan komponen baris, lalu tombol berganti menjadi **Batalkan Alokasi**.

**Contoh yang sengaja tidak dicegah layar (`G3`/`G4`).** Kantong PRC dapat dialokasikan ke baris
Trombosit, dan baris yang meminta 1 kantong dapat menerima kantong kedua. Layar menawarkan semua
baris apa adanya. Bila backend kelak menolak, pesannya tampil apa adanya.

### 2.3 Membatalkan alokasi

1. Pada kantong **Dialokasikan**, petugas menekan **Batalkan Alokasi**.
2. Dialog memuat alasan berkategori **`AllocationCancellation`** saja. Tombol **Ya, Batalkan Alokasi**
   tertahan sampai satu alasan dipilih. Tidak ada kolom teks bebas.
3. Layar mengirim **tepat dua isian**: kode alasan dan token versi.
4. **Ke mana kantong kembali ditentukan backend**, bukan dipilih petugas:
   - order asal masih berjalan → kantong **Tersedia**, dan pesan "…Kantong kembali tersedia." tampil;
   - order asal sudah berakhir → kantong **Menunggu keputusan**, pesan backend tampil sebagai
     peringatan, dan kantong masuk daftar kerja #2.

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Petugas lain lebih dulu mengubah kantong (`409`) | Dialog ditutup, pesan backend tampil apa adanya — misalnya "Kantong ini baru saja dialokasikan petugas lain. Muat ulang dan pilih kantong lain." — dan detail kantong dimuat ulang otomatis |
| Backend menolak (`422`) — lokasi nonaktif, belum disimpan, menunggu keputusan, order tidak aktif, kunjungan berakhir | Pesan backend tampil apa adanya di dalam dialog, dan detail dimuat ulang di belakang |
| Kantong menunggu keputusan | Penanda "Kantong menunggu keputusan" tampil. Tombol Alokasikan tidak ada. **Tidak ada** tombol alihkan, kembalikan ke PMI, atau tidak layak — itu milik `FE-BD-007` |
| Belum ada alasan aktif untuk pembatalan | Pesan backend tampil, dan tombol konfirmasi tetap tertahan |
| Tanpa hak akses | Tombol yang tidak berhak **disembunyikan**, bukan ditampilkan lalu ditolak `403` — lihat bagian 4 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Governance:** `AGENTS.md` frontend, `rules/frontend/frontend-architecture.md`,
`base-component-decision-gate.md`, `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`,
`rules/rule-output/status-task-roadmap.md`.

**Backend (read-only):** `BbkBloodUnitController.cs`, `BbkBloodUnitService.cs` (`AllocateAsync`,
`CancelAllocationAsync`, `CheckOrderLineAsync`, `IsAllocationOriginActiveAsync`,
`AvailableActionsFor`, `BuildFilterMetadata`), `BloodUnitDtos.cs`, `BloodUnitAllocationDtos.cs`,
`BloodOrderDtos.cs`, `BbkBloodOrderController.cs`, `BloodBankReasonController.cs`,
`Enums/BbkBloodUnitStatus.cs`, `Responses/ApiResponse.cs`, laporan `BE-BD-006`,
`contracts/api-contract.md`, `03-frontend-architecture.md`.

**Frontend (referensi):** seluruh source `FE-BD-010` (route, view, hook, service, constants,
utils, CSS module, spec e2e), `use-blood-order-detail.jsx` (pola pembatalan beralasan),
`use-blood-order-list.jsx` (pola metadata + ringkasan), `blood-order.service.js`,
`blood-order-utils.js`, base component `filter-select`, `summary-grid`, `status-badge`,
`confirm-modal`, `data-table`, `access-denied-gate`, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/blood-bank-management/blood-unit-constants.jsx` | **Baru.** Route, endpoint, cerminan enum status sebagai angka, preset `PendingReview`, opsi status cadangan, pemetaan badge, dua nama aksi yang dibaca layar, kategori alasan `AllocationCancellation`, token route, salinan teks |
| `src/lib/services/health-services/blood-bank-management/blood-unit.service.js` | **Baru.** `InstanceAxios` untuk daftar, ringkasan, metadata, detail, alokasi, dan pembatalan. ID divalidasi UUID sebelum masuk URL; isi alokasi dan pembatalan masing-masing tepat dua isian |
| `src/utils/health-services/blood-bank-management/blood-unit-utils.js` | **Baru.** Fungsi murni: pengenal status `PendingReview`, opsi status dari metadata, opsi order, opsi baris kebutuhan, ringkasan order, opsi alasan, dan hasil pembatalan yang dibaca dari status backend |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-list.jsx` | **Baru.** Daftar, ringkasan, metadata, penyaring, preset Menunggu Keputusan, paginasi |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-detail.jsx` | **Baru.** Detail, gerbang hak akses, dialog alokasi berantai order → baris, dialog pembatalan, penanganan `409`/`422` |
| `src/components/view/health-services/blood-bank-management/blood-units/blood-unit-list-view.jsx` | **Baru.** Komposisi layar daftar |
| `src/components/view/health-services/blood-bank-management/blood-units/blood-unit-table-columns.jsx` | **Baru.** Tujuh kolom tabel |
| `src/components/view/health-services/blood-bank-management/blood-units/detail/blood-unit-detail-view.jsx` | **Baru.** Komposisi layar detail, Alokasi Aktif, Riwayat Alokasi, Riwayat Status, dan dua dialog |
| `src/app/health-services/blood-bank-management/blood-units/page.jsx` | **Baru.** Route tipis daftar |
| `src/app/health-services/blood-bank-management/blood-units/[slug]/page.jsx` | **Baru.** Route tipis detail |
| `src/app/health-services/blood-bank-management/blood-units/[slug]/route-token.js` | **Baru.** Penyelesaian token route privat |
| `src/style/health-services/blood-bank-management/blood-units/blood-unit.module.css` | **Baru.** Hanya dua kelas isi dialog |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Satu butir menu "Kantong Darah" di antara Permintaan PMI dan Tindakan Bank Darah, sesuai urutan `03-frontend-architecture.md` §2, dijaga `BloodUnit : Read` |
| `tests/unit/blood-unit-allocation.test.mjs` | **Baru.** Delapan test yang menjaga `G3`, `G4`, dan status yang ditentukan backend |
| `tests/e2e/blood-unit-allocation-screen.spec.mjs` | **Baru.** Empat belas pemeriksaan runtime di browser sungguhan. Alat pembuktian acceptance, bukan fitur |

**Nol berkas backend tersentuh. Nol dependency ditambahkan.** Layanan order dan alasan
(`getBloodOrders`, `getBloodOrderDetail`, `getBloodBankReasonOptions`) dipakai ulang dari
`blood-order.service.js` tanpa perubahan.

### 3.3 Kepatuhan arsitektur frontend

Alurnya mengikuti `frontend-architecture.md`:
`src/app` (route tipis) → `components/view` → `lib/hooks` → `lib/services` → `InstanceAxios`.
Transformasi berada di `utils`, konfigurasi statis di `constants`. View tidak memanggil Axios.
Tidak ada Redux slice baru, mengikuti keputusan `FE-BD-002`, `FE-BD-003`, dan `FE-BD-010`: datanya
milik satu layar, bukan lintas halaman.

**Gerbang keputusan base component.**

`UI GATE: 12 elemen — REUSE 10, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | dipakai `blood-bank-procedure-list-view` | REUSE | — |
| Kartu ringkasan per status | `SummaryGrid` | dipakai `blood-order-list-view` | REUSE | Angka dari `GET /summary` |
| Pencarian, saringan status, jumlah baris, tombol preset | `DataFilter` + `FilterSelect` + `BaseButton` | dipakai `blood-bank-procedure-list-view` | REUSE | Preset lewat prop `actions` |
| Tabel kantong dan tabel riwayat | `DataTable` | dipakai seluruh layar Bank Darah | REUSE | `sortLatestFirst={false}` pada riwayat |
| Status, berlebih, lokasi nonaktif | `StatusBadge` | varian `active`/`inactive`/`warning`/`info`/`pending` | REUSE | `danger` tidak ada di CSS badge, maka tidak dipakai |
| Detail kantong | `BaseDetailView` | dipakai `blood-bank-procedure-detail-view` | REUSE | — |
| Alokasi aktif, riwayat alokasi, riwayat status | `BaseDetailSection` | idem | REUSE | — |
| Pesan galat, hasil, penanda menunggu keputusan | `InformationAlert` | idem | REUSE | — |
| Notifikasi hasil aksi | `ToastStack` lewat `BaseDetailView` | idem | REUSE | — |
| Tanpa hak akses | `AccessDeniedGate` | idem | REUSE | — |
| Dialog alokasi (order → baris) | `ConfirmModal` + `FilterSelect` + `InformationAlert` | pola dialog pencatatan `FE-BD-010` | COMPOSE | Merujuk keputusan yang sama pada `FE-BD-010` |
| Dialog pembatalan beralasan | `ConfirmModal` + `FilterSelect` | pola pembatalan order `FE-BD-002` | COMPOSE | Merujuk keputusan yang sama pada `FE-BD-002` |

Kedua baris `COMPOSE` merangkai base component di lapisan view tanpa mengubah satu pun base
component. Pilihan yang sama sudah dipakai pada modul yang sama (`FE-BD-002`, `FE-BD-010`), sehingga
cukup dirujuk. Alternatifnya — komponen dialog khusus kantong — akan menduplikasi `ConfirmModal`,
dan tidak ada alasan untuk itu.

**Satu temuan base component, dipertahankan tanpa mengubah base.** `FilterSelect` hanya merender
`label` opsi; `description` dipakai untuk pencarian saja (`filter-select.jsx` baris 100 dan 554).
Karena itu status order dan angka diminta/diberikan dimasukkan ke `label`. Prop opt-in
`renderOption` sengaja tidak dipakai supaya tampilan opsi tetap seragam dengan layar lain.

**Grep anti-regresi** (checklist G, pada berkas yang diubah):

| No | Pemeriksaan | Hasil |
| ---: | --- | --- |
| 1 | Warna literal di CSS baru | 1 temuan dipertahankan — `#6c757d` hanya fallback di dalam `var(--bs-secondary-color, …)`, persis modul referensi `FE-BD-010` |
| 2 | Typography di CSS baru | 3 temuan dipertahankan — hanya `.modalContent p` dan `.formField > span`, yaitu teks isi dialog domain, bukan komponen shared. Nilainya token dengan fallback |
| 3 | Tombol non-base | Bersih |
| 4 | `<table>` mentah | Bersih |
| 5 | Utility typography Bootstrap | Bersih |
| 6 | `!important` | Bersih |
| — | Inline style | Bersih |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka kartu ringkasan, "Memuat kantong darah..." pada tabel, "Memuat..." pada tombol Muat ulang, "Menyiapkan..." pada tombol Alokasikan/Batalkan Alokasi, "Memuat baris kebutuhan..." pada pemilih baris |
| Kosong | "Belum ada kantong" — "Belum ada kantong darah yang sesuai dengan filter ini."; "Tidak sedang dialokasikan"; "Belum pernah dialokasikan"; "Tidak ada order darah yang dapat dipilih saat ini."; "Order ini tidak memiliki baris kebutuhan yang dapat dipilih." |
| Gagal | Pesan backend apa adanya, dengan cadangan "Daftar kantong darah gagal dimuat." / "Detail kantong darah gagal dimuat."; pemulihan lewat Muat ulang atau Atur ulang filter |
| Tanpa hak akses | Halaman `403` → `AccessDeniedGate` "Ups! Akses Ditolak". Tombol tanpa hak akses **disembunyikan**: Alokasikan menuntut `BloodUnit : Allocate` **dan** `BloodOrder : Read`; Batalkan Alokasi menuntut `BloodUnit : Allocate` **dan** `BloodBankReason : Read`. Butir menu tersembunyi tanpa `BloodUnit : Read` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Blood Bank Management / Blood Unit

Base path: `/v1/health-services/blood-bank-management/blood-units`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar kantong; `unitStatus=5` = daftar kerja #2 | `BloodUnit : Read` |
| `GET` | `/summary` | Lima kartu ringkasan | `BloodUnit : Read` |
| `GET` | `/filters/metadata` | Opsi saringan status; cadangan lokal bila gagal | `BloodUnit : Read` |
| `GET` | `/{id}` | Detail, `AvailableActions`, `Version`, `CurrentAllocation`, `Allocations`, `Transitions` | `BloodUnit : Read` |
| `POST` | `/{id}/allocate` | Isi `{ bloodOrderLineId, version }` | `BloodUnit : Allocate` |
| `POST` | `/{id}/cancel-allocation` | Isi `{ reasonCode, version }` | `BloodUnit : Allocate` |

#### Health Services / Blood Bank Management / Blood Order

Base path: `/v1/health-services/blood-bank-management/blood-orders`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | 100 order terbaru sebagai pilihan tujuan, **tanpa** saringan status | `BloodOrder : Read` |
| `GET` | `/{id}` | `Lines[]` dan `Fulfillment.Lines[]` order terpilih | `BloodOrder : Read` |

#### Health Services / Master Data / Blood Bank Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/blood-bank-reasons/options?category=AllocationCancellation` | Pilihan alasan pembatalan | `BloodBankReason : Read` |

**Delta kontrak, bukan penahan.** Baris `GET /` pada `api-contract.md` menyebut query `status=`,
sedangkan source backend menerima `unitStatus=`. Layar mengikuti source. Kolom status kontrak
untuk endpoint Blood Unit masih bertuliskan "Rencana" walaupun sudah terimplementasi.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Keluaran kosong, kode keluar `0` |
| `npx eslint` pada berkas task | 0 error, 4 warning `react-hooks/set-state-in-effect` | `EXISTING WARNING` | Aturan dan pola yang sama menghasilkan 3 warning pada hook referensi `FE-BD-010` |
| `npm run test:unit` | 1576 test, **1569 lulus, 7 gagal**. Kedelapan test baru lulus (nomor 212–219) | `PASS` untuk cakupan task | Keluaran perintah |
| 7 test yang gagal | Nama, berkas, dan sebabnya sama persis dengan yang tercatat pada laporan `FE-BD-010` §6: 1568 → 1576 total dan 1561 → 1569 lulus, tepat +8 | `EXISTING / ENVIRONMENT ISSUE` | `accounting-reconciliation.test.mjs:357`, lima test `FE-RWI-042/043`, `menu-permission-filter.test.mjs:110` (`subMenu[0]` — galat `Cannot read properties of undefined (reading 'map')`, sama seperti sebelumnya) |
| `npm run build` | Selesai, kode keluar `0`, 370 halaman (sebelumnya 369), `postbuild` standalone berhasil | `PASS` | `/blood-units` dan `/blood-units/[slug]` terdaftar pada keluaran build dan `routes-manifest.json` |
| `git diff --check` | Tidak ada galat spasi | `PASS` | Keluaran kosong |
| Grep anti-regresi UI | 4 bersih, 2 temuan dipertahankan beserta alasan | `PASS` | Bagian 3.3 |
| `npx playwright test tests/e2e/blood-unit-allocation-screen.spec.mjs --workers=1` | **16 dari 16 lulus** dalam 42,5 detik di Chromium sungguhan (validasi ulang); jalan sebelumnya 14 dari 14 dalam 29,6 detik | `PASS` | Bagian 6.1 dan 6.2 |

`AUTOMATED TEST: npm run test:unit — PASS` (8 test baru lulus; 7 kegagalan lain sudah ada
sebelumnya dan berada di luar cakupan)

### 6.1 Validasi runtime — 24 September 2026

**Uji manual: `PASS` — 14 dari 14 pemeriksaan.**

**Cara pembuktiannya.** Sama dengan `FE-BD-010`: hasil `next build` standalone dijalankan pada
`http://127.0.0.1:3710`, dan layar diuji di browser Chromium lewat Playwright. Jawaban API
dipasang lewat `page.route("**/v1/**")`. Yang diuji tetap **produk yang sebenarnya** — React, hook,
gerbang kewenangan, filter menu, dan komponen yang sama dengan yang dipakai pengguna.

Cara ini dipilih karena alokasi dan pembatalan **menulis** ke database bersama
`QuilvianNewDevSukma`, dan fixture `TEST-BD006-20260914094559` di sana belum punya rencana cleanup
yang disetujui (laporan `BE-BD-006` §9.7). Selain itu, `409` serentak dan pembatalan ke
`PendingReview` baru dapat dihadirkan secara pasti bila jawabannya dipasang. Hak akses diuji
dengan akun **non-SuperAdmin** (`isSuperAdmin: false`) dan daftar kewenangan yang dipersempit.

Perintah: `npx playwright test tests/e2e/blood-unit-allocation-screen.spec.mjs --workers=1` —
**14 passed (29,6 detik)**.

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `R1` | Alokasi: pilih order lalu Baris 1, simpan. Isi permintaan **tepat** `bloodOrderLineId` dan `version`; `version` = `3` dari detail; baris dimuat dari `GET /blood-orders/{id}`. Pesan backend tampil, tombol berganti ke **Batalkan Alokasi** | `PASS` |
| `R1b` | Kantong PRC: baris **Trombosit** tetap ditawarkan (`G3`). Opsi "Baris 2 — Trombosit (diminta 4 · diberikan 1)" terbaca, angkanya dari `Fulfillment` backend | `PASS` |
| `R2` (`Available`) | Alasan dimuat dengan `category=AllocationCancellation`; tombol konfirmasi **tertahan** sampai alasan dipilih; isi **tepat** `reasonCode` dan `version` (`4`); backend memulangkan **Tersedia** → pesan "…Kantong kembali tersedia." dan tombol Alokasikan muncul | `PASS` |
| `R2` (`PendingReview`) | Pembatalan yang sama, backend memulangkan **Menunggu keputusan** → pesan backend tampil, penanda "Kantong menunggu keputusan" muncul, tombol Alokasikan **tidak** ada | `PASS` |
| `R4a` | Tanpa `BloodUnit : Allocate` → **Alokasikan** nol kemunculan | `PASS` |
| `R4b` | Tanpa `BloodUnit : Allocate` → **Batalkan Alokasi** nol kemunculan | `PASS` |
| `R4c` | Tanpa `BloodOrder : Read` → **Alokasikan** nol kemunculan | `PASS` |
| `R4d` | Tanpa `BloodBankReason : Read` → **Batalkan Alokasi** nol kemunculan | `PASS` |
| `R4e` | Izin penuh, kantong **Tersedia** → Alokasikan tampil, Batalkan Alokasi tidak (kelayakan status dari `AvailableActions`) | `PASS` |
| `R5a` | `409` → dialog tertutup, pesan backend tampil apa adanya, dan `GET` detail dipanggil ulang | `PASS` |
| `R5b` | `422` lokasi nonaktif → pesan backend tampil apa adanya di dalam dialog | `PASS` |
| `R6` | Tombol **Menunggu Keputusan** meminta `GET /blood-units?unitStatus=5`; baris kantong berlebih tampil dengan penanda **Berlebih** | `PASS` |
| `R6b` | Butir menu **Kantong Darah** terpasang menuju `/health-services/blood-bank-management/blood-units` | `PASS` |
| `R7` | Kantong `PendingReview` dengan `AvailableActions` penyelesaian **dan** hak akses `ResolveReallocate`/`ResolveReturn`/`ResolveNotUsable`/`Store`/`Issue`/`EmergencyIssue` → nol tombol alihkan, kembalikan, tidak layak, tetapkan/pindahkan lokasi, berikan, maupun darurat; nol panggilan `/reallocate`, `/return-to-provider`, `/mark-not-usable`, `/storage-location` | `PASS` |

Pemeriksaan "tombol tidak ada" (`R4a`–`R4d`, `R7`) baru dijalankan **sesudah** `auth/permissions`
dijawab dan jaringan tenang, supaya tidak lolos palsu sebelum kewenangan terbaca. Snapshot halaman
juga menunjukkan filter menu bekerja: dengan izin Bank Darah yang dipersempit, sidebar hanya
memuat Order Darah, Kantong Darah, dan Setup.

**Empat kegagalan pada jalan pertama, dan penyebabnya.** Jalan pertama menghasilkan 10 lulus dan
4 gagal (`R1`, `R1b`, `R5a`, `R5b`). Berbeda dari `FE-BD-010`, keduanya **cacat produk** dan
diperbaiki di source:

1. Teks bantuan di bawah pemilih baris ada di dalam `<label>`, sehingga ikut menjadi nama
   aksesibel tombol ("Baris Kebutuhan * Angka diminta dan diberikan dibaca dari backend.").
   Pembaca layar ikut membacakannya. Teks dipindah ke luar label.
2. Angka diminta/diberikan ditaruh di `description` opsi, padahal `FilterSelect` tidak
   merendernya — petugas tidak pernah melihatnya. Angka dipindah ke `label`.

Sesudah perbaikan itu, lint, build, dan unit test dijalankan ulang, lalu 14 dari 14 lulus.

**Prasyarat:** Chromium Playwright sudah ada di cache pengguna sejak `FE-BD-010`; nol perubahan
`package.json` dan lockfile.

### 6.2 Validasi ulang — 24 September 2026

Atas permintaan pemilik, build dan runtime dijalankan ulang tanpa perubahan source produk.
Spec ditambah dua pemeriksaan supaya setiap butir permintaan punya bukti eksplisit:

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `R6a` | Daftar kantong termuat — kartu ringkasan dari `GET /summary` dan dua baris dari `GET /blood-units` — lalu klik dua kali pada baris membuka **Detail Kantong Darah** lewat `GET /blood-units/{id}` | `PASS` |
| `R4f` | Tanpa `BloodUnit : Read`: backend menolak `403` "Anda tidak memiliki akses ke menu atau fitur ini." dan halaman menampilkan **Ups! Akses Ditolak**; pada halaman Order Darah yang masih berhak, butir **Kantong Darah** nol kemunculan sementara butir Order Darah tetap ada | `PASS` |

Hasil: `npm run build` `PASS` (370 halaman, kedua route kantong terdaftar);
`npx playwright test tests/e2e/blood-unit-allocation-screen.spec.mjs --workers=1` — **16 passed
(42,5 detik)**; `npm run lint:errors` `PASS` sesudah spec diubah.

**Dua percobaan R4f yang gagal lebih dulu — cacat spec, bukan produk.** Tanpa butir Kantong Darah,
route yang terbuka tidak ada di menu, sehingga grup Bank Darah tidak terbuka sendiri dan anaknya
tidak dirender; memeriksa ketiadaan link di sana bisa lolos palsu. Membuka grup lewat klik juga
gagal karena tombol grup sidebar tidak punya nama aksesibel. Spec akhirnya membuka halaman Order
Darah — grupnya terbuka otomatis — lalu membandingkan kedua butir pada sidebar yang sama.

**Pemetaan permintaan validasi pemilik:**

| Butir permintaan | Skenario yang membuktikan |
| --- | --- |
| R1 — daftar kantong termuat, saringan `PendingReview`, detail kantong | `R6a`, `R6` |
| R2 — alokasi, pilih order, pilih baris kebutuhan, status berubah dari backend | `R1`, `R1b` |
| R3 — pembatalan, alasan hanya `AllocationCancellation`, status `Available`/`PendingReview` dari jawaban backend | `R2` (`Available`), `R2` (`PendingReview`) |
| R4 — gerbang `BloodUnit : Read`, `BloodUnit : Allocate`, `BloodOrder : Read`, `BloodBankReason : Read` | `R4f`, `R4a`/`R4b`, `R4c`, `R4d`, dan `R4e` sebagai kontrol |
| R5 — konflik versi `409` dan pesan backend tampil | `R5a`, ditambah `R5b` untuk `422` |
| R6 — `PendingReview` tanpa aksi penyelesaian | `R7` |

**Tidak dijalankan:** `npm run test:uat` — tidak diminta task. Uji terhadap backend dev sungguhan
tidak dijalankan karena alokasi dan pembatalan menulis ke database bersama; kesembilan skenario
backend-nya sudah dilaporkan lulus pada `BE-BD-006` §9.3.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`G5`, 24 September 2026) | Status | Bukti |
| --- | --- | --- |
| Alokasi `PASS` | **Terpenuhi** | `R1` — `POST /{id}/allocate` dengan tepat dua isian; hasil Dialokasikan; baris dari `GET /blood-orders/{id}` |
| Pembatalan alokasi `PASS` | **Terpenuhi** | `R2` kedua arah — `POST /{id}/cancel-allocation` dengan tepat dua isian |
| Gerbang hak akses | **Terpenuhi** | `R4a`–`R4f` dengan akun non-SuperAdmin — keempat butir `BloodUnit : Read`, `BloodUnit : Allocate`, `BloodOrder : Read`, `BloodBankReason : Read`; butir menu (`R6b`, `R4f`) |
| Alasan berkategori `AllocationCancellation` | **Terpenuhi** | `R2` — query `category=AllocationCancellation`; konfirmasi tertahan tanpa alasan; nol teks bebas |
| Status ditentukan backend | **Terpenuhi** | `R2` — layar membaca `unitStatus` jawaban; `Tersedia` dan `Menunggu keputusan` sama-sama dari backend; 8 unit test |
| Penanganan konflik versi | **Terpenuhi** | `R1`/`R2` — `version` dari detail ikut dikirim; `R5a` — `409` menutup dialog, menampilkan pesan, dan memuat ulang |
| Saringan `PendingReview` | **Terpenuhi** | `R6` — `unitStatus=5` lewat tombol preset dan saringan status |
| Tidak ada penyelesaian `PendingReview` | **Terpenuhi** | `R7` — nol tombol, nol panggilan endpoint penyelesaian, walau hak akses dan kelayakan diberikan |
| **Roadmap** — daftar `PendingReview` wajib ada (`FE-BD-002`) | **Terpenuhi** | `R6` |
| **DoD** — worklist #2 tersedia | **Terpenuhi** | `R6` — sebagai **saringan** pada `FE-BD-04`, bukan daftar kerja tersendiri (`DEC-BD-023`) |
| **DoD** — butir menu Kantong Darah (`G2`) | **Terpenuhi** | `menu-items.jsx`; `R6b` |

**Tambahan yang ikut terbukti:** kolom lokasi dan penanda **Lokasi nonaktif** pada tabel
(kewajiban layar `FE-BD-011` pada `FE-BD-04`) sudah ada karena datanya tersedia di daftar. Saringan
`Received` dan `inactiveLocation` (`FE-BD-010`) **tidak** dibangun — tetap milik `FE-BD-012`.

**Butir yang belum terpenuhi:** tidak ada.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 4 warning lint `set-state-in-effect` — pola sama dengan hook referensi `FE-BD-010`. Nol warning build baru dari perubahan ini |
| Risiko tercatat (`G4`) | **Jumlah alokasi per baris kebutuhan tidak dijaga backend.** Baris yang meminta 1 kantong dapat menerima beberapa alokasi aktif, karena `CheckOrderLineAsync` tidak menghitung alokasi aktif dan `OutstandingQuantity` hanya menghitung kantong yang sudah diberikan. Layar sengaja tidak menghitungnya. Keputusan pemilik 24 September 2026: tidak ada task backend baru |
| Risiko tercatat (`G3`) | **Tidak ada pemeriksaan komponen maupun golongan darah saat alokasi**, dan kantong tidak menyimpan golongan darah. Kecocokan pasien baru dijaga pada bukti kecocokan dan pemberian (`BE-BD-007`, layar `FE-BD-005`). Tetap terbuka sebagai risiko klinis pada laporan `BE-BD-006` §7 |
| Masalah yang diketahui | (1) Pemilih order menawarkan 100 order terbaru **tanpa** saringan status, sama seperti `FE-BD-010`; order yang tidak lagi menerima alokasi baru ditolak backend. Status order ikut tampil pada label supaya terbaca. (2) Di luar cakupan: pemilih order `FE-BD-010` menaruh status order di `description`, yang tidak dirender `FilterSelect`. (3) 7 test unit lama tetap gagal, termasuk `menu-permission-filter.test.mjs` yang rusak karena anggapan `subMenu[0]` |
| Dependency backend | `NONE` yang menahan. Delta kontrak: `status=` lawan `unitStatus=`; baris Blood Unit kontrak masih "Rencana" |
| Perubahan sampingan | **Dipulihkan:** menjalankan Playwright menimpa `test-results/.last-run.json` dan menghapus satu artefak Rawat Inap, karena `test-results/` dilacak Git. Keduanya dikembalikan lewat `git checkout -- test-results/`. Server standalone uji dihentikan sesudah validasi |
| Interupsi | `NONE` |
| Git | Source frontend **belum di-commit** dan **belum di-push** — tidak diminta pada task ini |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | Commit source frontend bila disetujui pemilik. Task frontend berikutnya menurut urutan: **`FE-BD-005`** (golongan darah, bukti kecocokan, pemberian, jalur darurat), yang kini dapat berangkat dari kantong Dialokasikan di layar ini. `FE-BD-012` tetap ⛔ menunggu `BE-BD-020` |

```text
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/blood-bank-management/blood-units/
?? src/components/view/health-services/blood-bank-management/blood-units/
?? src/lib/constants/health-services/blood-bank-management/blood-unit-constants.jsx
?? src/lib/hooks/health-services/blood-bank-management/use-blood-unit-detail.jsx
?? src/lib/hooks/health-services/blood-bank-management/use-blood-unit-list.jsx
?? src/lib/services/health-services/blood-bank-management/blood-unit.service.js
?? src/style/health-services/blood-bank-management/blood-units/
?? src/utils/health-services/blood-bank-management/blood-unit-utils.js
?? tests/e2e/blood-unit-allocation-screen.spec.mjs
?? tests/unit/blood-unit-allocation.test.mjs
```

Repository backend (laporan dan bukti roadmap):

```text
 M docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md
 M docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md
?? docs/module-blueprints/bank-darah/task/report/frontend/FE-BD-004.md
```
