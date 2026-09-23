# Laporan Perubahan Frontend — `FE-BD-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-010` |
| Judul | Daftar dan pencatatan tindakan Bank Darah |
| Slice | Slice 2 — Order, permintaan, dan tindakan |
| Roadmap | [roadmap/frontend-roadmap.md](../../../roadmap/frontend-roadmap.md), kartu `FE-BD-010` |
| Trace | `DEC-BD-021`, `DEC-BD-034`, `DEC-BD-048`, `DEC-BD-049`, `DEC-BD-016`, `BD-UI-GAP-001` Opsi A |
| Contract version | api-contract `v4` — Blood Bank Procedure, termasuk delta 17 September 2026 (`complete` membawa `BillingHandoff`). Disetujui |
| Wewenang UI | Layar `FE-BD-07` (daftar + kerja). Rupa penampilan hasil penyerahan biaya `DEV_DISCRETION`. Tidak mencakup layar lain |
| Dependency | `BE-BD-012` ✅, `BE-BD-013` ✅ — keduanya terbukti di source backend |
| Klasifikasi | `MEDIUM` — satu layar daftar, satu layar detail, dua aksi tulis, nol arsitektur baru, mengikuti pola `FE-BD-003` |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` (source) + berkas laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `1bdaf6bd6` (branch `sukmagpV2`) |
| Commit backend yang dijadikan rujukan | `3eb37c6c` (branch `sukmagp`) |
| Tanggal | 23 September 2026 |
| Status | ✅ **Selesai** — source lengkap, `lint`/`test:unit`/`build` PASS, dan **16 dari 16 pemeriksaan runtime di browser sungguhan PASS** |

---

## 1. Keadaan yang ditemukan di awal

Frontend **tidak punya satu baris pun** source tindakan Bank Darah. Pencarian
`grep -rn "BloodBankProcedure" src/` memulangkan **0 hasil**, dan sidebar belum memuat butir
menu "Tindakan Bank Darah". Temuan ini sama dengan catatan laporan `BE-BD-013` bagian 3.

Di sisi backend keadaannya sebaliknya — seluruh endpoint yang dibutuhkan sudah ada dan
terbukti di source, bukan sekadar tertulis di dokumen:

- `BbkBloodBankProcedureController.cs` memuat `GetAll`, `GetById`, `Create`, `Complete`, dan
  `ResendCostFact`;
- `BloodBankProcedureBillingHandoffDto` beserta pemetaan `MapHandoff` pada `Complete` sudah
  ada — inilah hasil `BE-BD-013`.

**Dua pembatasan kontrak yang menentukan bentuk layar ini**, dan keduanya ditemukan dari
source, bukan diasumsikan:

1. Endpoint tindakan **tidak menyediakan** `GET /filters/metadata` maupun `GET /summary`,
   berbeda dari order darah dan permintaan PMI. Karena itu layar ini tidak memiliki kartu
   ringkasan, dan pilihan penyaringnya ditulis lokal dari enum kontrak.
2. `BillingHandoff` **selalu kosong pada `GET`**. Hasil penyerahan biaya hanya ada pada
   jawaban aksi `complete`. Membuat kolom atau halaman status Billing yang tetap karena itu
   mustahil, dan memang dilarang.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas Bank Darah, sesuai tabel aksi per peran pada
`03-frontend-architecture.md` bagian 4.

**Kapan layar ini dibuka.** Ketika satu order darah sudah ditangani dan tindakan yang
menyertainya — misalnya uji silang serasi — perlu dicatat supaya biayanya sampai ke Billing.

**Langkah normal, berurutan:**

1. Petugas membuka menu **Bank Darah → Tindakan Bank Darah**. Daftar tindakan tampil,
   terbaru lebih dulu, memuat nomor tindakan, kode order, nama pasien, nama tindakan, tarif,
   dokter BDRS, dan status.
2. Petugas dapat mempersempit daftar lewat kotak pencarian (nomor tindakan, kode order, atau
   nama pasien), penyaring status (**Dicatat** / **Selesai**), dan pengatur jumlah baris.
   Tombol atur ulang mengembalikan semuanya ke bawaan.
3. Untuk mencatat tindakan baru, petugas menekan **+ Catat Tindakan**. Sebuah kotak dialog
   meminta **tiga** isian saja: order darah, tindakan bertarif, dan dokter BDRS penanggung
   jawab.
4. Setelah disimpan, layar langsung berpindah ke halaman detail tindakan yang baru dibuat.
5. Di halaman detail, selama status masih **Dicatat**, tersedia tombol **Selesaikan
   Tindakan**. Petugas menekannya, lalu mengonfirmasi.
6. Sesudah penyelesaian tersimpan, layar menyatakan **tindakan selesai** dan — terpisah dari
   itu — menampilkan hasil penyerahan fakta biaya ke Billing apa adanya.

**Kenapa hanya tiga isian.** Unit dan kelas pasien diambil backend dari kunjungan order
(`DEC-BD-048`); tarif beserta nominalnya dipilih backend dari data induk (`DEC-BD-049`); dan
petugas pencatat diambil dari akun yang sedang login. Layar tidak pernah mengirimkan harga,
unit, maupun kelas — tidak ada satu field pun yang memungkinkannya.

**Jalur tidak normal:**

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Tarif belum diatur untuk kelas pasien dan unit kunjungan (`VAL-BD-084`) | Pencatatan ditolak, dan kalimat dari backend ditampilkan apa adanya: "Tarif tindakan ini belum diatur untuk kelas pasien dan unit kunjungan ini. Hubungi bagian data induk tarif." Nomor tindakan tidak terbit |
| Order tidak sah (`VAL-BD-026`) | Ditolak `400`, pesannya ditampilkan apa adanya |
| Kunjungan order belum punya kelas pasien | Ditolak, pesannya ditampilkan apa adanya |
| Tindakan sudah **Selesai** | Tombol Selesaikan tidak muncul sama sekali, karena `AvailableActions` dari backend sudah kosong |
| **Billing menolak fakta biaya** | Tindakan **tetap** dinyatakan selesai, dan di bawahnya muncul penanda merah berisi pesan backend. Layar tidak pernah mengatakan tagihan terkirim |

**Contoh konkret jalur Billing yang gagal.** Folio kunjungan pasien sudah ditutup. Backend
tetap memulangkan `200` dengan `BillingHandoff.Kind = RejectedByBilling` dan pesan
"penyerahan fakta biaya ke Billing memerlukan tinjauan". Yang dilihat petugas: pemberitahuan
**"Tindakan selesai"** berwarna peringatan, dan sebuah kotak merah berjudul **"Fakta biaya
ditolak Billing"** berisi kalimat backend itu. Yang **tidak pernah** dilihat petugas:
kalimat "tindakan selesai dan tagihan terkirim".

Alasannya penting dan bukan soal kosmetik: `200` pada `complete` berarti *tindakan tersimpan
selesai*, **bukan** *tagihan terkirim*. Petugas yang mengira tagihan sudah terkirim tidak
akan menindaklanjuti, dan biaya tindakan itu hilang diam-diam dari penagihan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola:** `AGENTS.md` frontend, `rules/GLOBAL_RULES.md`, `rules/frontend/frontend-architecture.md`,
`base-component-decision-gate.md`, `page-composition-patterns.md`, `ui-consistency-checklist.md`,
`test-policy.md`, `REPORT_TEMPLATE.md`.

**Blueprint:** `roadmap/frontend-roadmap.md` (kartu `FE-BD-010`), `03-frontend-architecture.md`
(bagian 1, 2, 4, 5), `contracts/api-contract.md`, `contracts/validation-matrix.md` bagian 5,
`requirement-traceability.md`.

**Backend (read-only):** `BbkBloodBankProcedureController.cs`, `BloodBankProcedureDtos.cs`,
`BbkProcedureStatus.cs`, `BbkBloodBankProcedureService.cs`, `ProcedureController.cs`,
`DoctorController.cs`.

**Frontend sebagai rujukan pola:** seluruh berkas `provider-requests` (`FE-BD-003`),
`blood-order-constants.jsx`, `blood-order-utils.js`, `use-permission.jsx`,
`use-select-resource.jsx`, `hr-select-resources.js`, `health-service-select-resources.js`,
`base-form-control.jsx`, `information-alert.jsx`, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/blood-bank-management/blood-bank-procedure-constants.jsx` | **Baru.** Endpoint, penyaring bawaan, opsi status dan jumlah baris lokal, pemetaan warna badge, daftar `Kind` yang sah dibaca berhasil, pemetaan tampilan per `Kind`, salinan teks, token route, dan konfigurasi pemilih dokter BDRS |
| `src/utils/health-services/blood-bank-management/blood-bank-procedure-utils.js` | **Baru.** Fungsi murni: `formatCurrencyIdr`, `isBillingHandoffAccepted`, `presentBillingHandoff`, `buildCompleteToast` |
| `src/lib/services/health-services/blood-bank-management/blood-bank-procedure.service.js` | **Baru.** Pembungkus `InstanceAxios` untuk lima panggilan; setiap ID divalidasi UUID sebelum masuk URL, kata kunci pencarian disanitasi, parameter kosong dibuang |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-bank-procedure-list.jsx` | **Baru.** Pengendali daftar: penyaring, paginasi, gerbang kewenangan, dan kotak dialog pencatatan berisi tiga pemilih |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-bank-procedure-detail.jsx` | **Baru.** Pengendali detail: memuat detail, aksi penyelesaian, dan penyimpanan hasil penyerahan biaya yang hanya hidup selama sesi layar |
| `src/components/view/health-services/blood-bank-management/blood-bank-procedures/blood-bank-procedure-list-view.jsx` | **Baru.** Komposisi layar daftar beserta kotak dialog pencatatan |
| `src/components/view/health-services/blood-bank-management/blood-bank-procedures/blood-bank-procedure-table-columns.jsx` | **Baru.** Sembilan definisi kolom tabel |
| `src/components/view/health-services/blood-bank-management/blood-bank-procedures/detail/blood-bank-procedure-detail-view.jsx` | **Baru.** Komposisi layar detail, penanda hasil penyerahan biaya, riwayat status, dan konfirmasi penyelesaian |
| `src/app/health-services/blood-bank-management/blood-bank-procedures/page.jsx` | **Baru.** Route tipis daftar |
| `src/app/health-services/blood-bank-management/blood-bank-procedures/[slug]/page.jsx` | **Baru.** Route tipis detail |
| `src/app/health-services/blood-bank-management/blood-bank-procedures/[slug]/route-token.js` | **Baru.** Penyelesaian token route privat |
| `src/style/health-services/blood-bank-management/blood-bank-procedures/blood-bank-procedure.module.css` | **Baru.** Hanya style kotak dialog yang benar-benar khusus modul ini |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Satu butir menu "Tindakan Bank Darah" ditambahkan di bawah "Permintaan PMI", dijaga `BloodBankProcedure : Read` |
| `tests/unit/blood-bank-procedure-billing-handoff.test.mjs` | **Baru.** Delapan test yang menjaga kejujuran penyerahan biaya |
| `tests/e2e/blood-bank-procedure-screen.spec.mjs` | **Baru.** Enam belas pemeriksaan runtime di browser sungguhan — pencatatan, penyelesaian pada enam keadaan `BillingHandoff`, kewenangan, dan aturan bisnis. Bukan fitur baru, melainkan alat pembuktian acceptance |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi mengikuti `rules/frontend/frontend-architecture.md` tanpa pembalikan:
`src/app` → `components/view` → `lib/hooks` → `lib/services` → `InstanceAxios` → backend.

- Route di `src/app` hanya entry point dan metadata; tidak ada markup, Axios, maupun style.
- View tidak memanggil `InstanceAxios` sama sekali; seluruh data datang dari hook.
- Endpoint tinggal di constants, bukan tersebar sebagai magic string di view.
- Nol Axios instance baru, nol arsitektur state baru, nol base component baru.
- Setiap request pembaca meneruskan `AbortController.signal` dan dibatalkan saat unmount.
- Modul ini memakai service, **bukan** Redux slice, mengikuti `FE-BD-002`/`FE-BD-003` di
  modul yang sama. Karena itu tidak ada reducer yang perlu didaftarkan ke `store.jsx`.

**Gerbang keputusan base component** dijalankan sebelum baris JSX pertama ditulis:

`UI GATE: 11 elemen — REUSE 10, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status |
| --- | --- | --- |
| Header, penyaring + pencarian, tabel, badge status, panel detail, bagian detail, pemberitahuan, konfirmasi, notifikasi, gerbang akses, riwayat | `Hero`, `DataFilter`/`FilterSelect`, `DataTable`, `StatusBadge`, `BaseDetailView`, `BaseDetailSection`, `InformationAlert`, `ConfirmModal`, `ToastStack`, `AccessDeniedGate` | `REUSE` |
| Kotak dialog pencatatan berisi tiga pemilih | `ConfirmModal` + `BaseSelectField` | `COMPOSE` — bentuk yang sama sudah disetujui pada kotak dialog pembuatan `FE-BD-003` di modul ini |

Tidak ada elemen berstatus `NEW`, sehingga tidak ada yang perlu menunggu keputusan pemilik.

**Dua penyimpangan yang disengaja, beserta alasannya:**

1. **Tanpa kartu ringkasan.** Checklist konsistensi UI menetapkan urutan halaman daftar
   memuat `SummaryGrid`. Endpoint tindakan tidak menyediakan `GET /summary`, dan menghitung
   angkanya sendiri di layar berarti memindahkan aturan bisnis ke frontend. Keputusan pemilik
   `G4`, 23 September 2026.
2. **Pemilih dokter tidak memakai resource `doctors` yang sudah terdaftar.** Resource itu
   menunjuk `GET /doctors/options`, yang pada backend dijaga
   `[Authorize(Policy = "KioskRead")]` — **bukan** butir hak akses `Doctor : Read`.
   Memakainya berarti melewati gerbang kewenangan yang ditetapkan pemilik pada keputusan
   `G3`. Modul ini memakai `GET /doctors/admin/options`, yang benar-benar dijaga
   `Doctor : Read`. Registry bersama sengaja **tidak** diubah supaya modul lain tidak
   terdampak.

**Hasil grep anti-regresi** pada berkas yang ditambahkan:

| Pemeriksaan | Hasil |
| --- | --- |
| Tombol non-base (`<button`, `.btn`) | Kosong — patuh |
| Tabel mentah (`<table`) | Kosong — patuh |
| Utility typography Bootstrap (`fw-*`, `fs-*`) | Kosong — patuh |
| `!important` baru | Kosong — patuh |
| Warna literal di stylesheet baru | **2 temuan, dipertahankan** — keduanya nilai cadangan di dalam `var(--bs-secondary-color, #6c757d)`, disalin persis dari `provider-request.module.css` milik `FE-BD-003`. Nilai cadangan di dalam `var()` tidak melewati sistem token, dan menghapusnya justru membuat berkas ini menyimpang dari modul tetangganya |
| Typography di stylesheet baru | **5 temuan, dipertahankan** — seluruhnya token `var(--app-text-sm)`, `var(--app-font-weight-semibold)`, dan satu `line-height` yang hanya menyasar class lokal `.modalContent` dan `.formField`. **Tidak ada** yang menyasar `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `BaseButton`, `StatusBadge`, `BaseFormControl`, maupun `Pagination` |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan kerangka baris dengan kalimat "Memuat tindakan Bank Darah...", bukan layar kosong. Tombol aksi menampilkan label prosesnya sendiri — "Menyiapkan...", "Memuat..." |
| Kosong | "Belum ada tindakan" beserta baris kedua "Belum ada tindakan Bank Darah yang sesuai dengan filter ini.", dan tombol atur ulang penyaring tetap tersedia |
| Gagal | Kotak merah berisi kalimat dari backend, misalnya "Daftar tindakan Bank Darah gagal dimuat." Pada halaman detail tersedia tombol **Muat ulang** sebagai pemulihan |
| Tanpa hak akses | Halaman dibungkus `AccessDeniedGate`, sehingga penolakan kewenangan tampil sebagai pemberitahuan yang terbaca, bukan halaman rusak. Tombol yang tidak berhak **disembunyikan**, bukan ditampilkan lalu ditolak `403` |
| Data basi | Penolakan `409` dan `422` pada penyelesaian memicu pengambilan ulang detail, sehingga layar tidak bertahan pada data lama |
| Pengiriman ganda | Aksi tulis dikunci `actionLockRef` dan tombolnya dinonaktifkan selama proses berjalan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Blood Bank Management / Blood Bank Procedure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-bank-procedures` | Daftar tindakan beserta penyaring `search`, `procedureStatus`, `pageNumber`, `pageSize` | `BloodBankProcedure : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-bank-procedures/{id}` | Detail tindakan, riwayat status, dan `AvailableActions` | `BloodBankProcedure : Read` |
| `POST` | `/v1/health-services/blood-bank-management/blood-bank-procedures` | Mencatat tindakan; badan permintaan tepat tiga isian | `BloodBankProcedure : Create` |
| `POST` | `/v1/health-services/blood-bank-management/blood-bank-procedures/{id}/complete` | Menyatakan tindakan selesai; jawabannya membawa `BillingHandoff` | `BloodBankProcedure : Update` |

`POST /{id}/resend-cost-fact` **sengaja tidak dipanggil** — `BD-UI-GAP-001` Opsi A.

#### Health Services / Master Data / Procedure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/procedures/options` | Pilihan tindakan bertarif pada kotak dialog pencatatan | `Procedure : Read` |

#### Corporate / Human Resource / Master Data / Doctor

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/doctors/admin/options` | Pilihan dokter BDRS penanggung jawab | `Doctor : Read` |

#### Health Services / Blood Bank Management / Blood Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-orders` | Pilihan order darah sebagai konteks tindakan | `BloodOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Keluaran perintah kosong |
| `npm run test:unit` | 1568 test, **1561 lulus, 7 gagal**. Kedelapan test baru milik task ini lulus (nomor 192–199) | `PASS` untuk cakupan task | Keluaran perintah |
| 7 test yang gagal | Seluruhnya **sudah gagal sebelum task ini** | `EXISTING / ENVIRONMENT ISSUE` | Lihat rincian di bawah |
| `npm run build` | Selesai, keluar dengan kode `0`, `postbuild` standalone berhasil | `PASS` | Keluaran perintah |
| Route terdaftar di hasil build | `blood-bank-procedures` dan `blood-bank-procedures/[slug]` muncul di `routes-manifest.json` | `PASS` | Keluaran `grep` |
| `git diff --check` | Tidak ada galat spasi | `PASS` | Keluaran perintah kosong |
| Grep anti-regresi konsistensi UI | 4 bersih, 2 temuan dipertahankan beserta alasan | `PASS` | Bagian 3.3 |
| Kebenaran path endpoint | Empat endpoint asli memulangkan `401`, sedangkan path kontrol yang sengaja dibuat ngawur memulangkan `404` | `PASS` | Probe `curl` tanpa kredensial |
| Acceptance (4) — larangan | `resend-cost-fact` nihil, auto-resend nihil, `BillingHandoff` hanya diisi dari jawaban `complete` | `PASS` | Pembuktian ketiadaan lewat `grep` |
| `npx playwright test tests/e2e/blood-bank-procedure-screen.spec.mjs` | **16 dari 16 lulus** dalam 24,7 detik, di browser Chromium sungguhan | `PASS` | Bagian 6.1 |

**Rincian 7 test yang gagal, beserta bukti bahwa semuanya bukan akibat task ini:**

| Nomor | Berkas | Sebab | Bukti pre-existing |
| --- | --- | --- | --- |
| 110 | `accounting-reconciliation.test.mjs` | Menuntut butir menu `/corporate/accounting/reconciliation` | Path itu **tidak ada** pada `menu-items.jsx` versi `HEAD` **maupun** versi kerja — `grep -c` memulangkan `0` pada keduanya |
| 810, 813, 814, 815, 829 | `inpatient-physician-entry.test.mjs`, `inpatient-physician-workspace.test.mjs` | Menuntut berkas dan konstanta rawat inap | Seluruh berkas yang dibacanya berada di `inpatient-management`. Task ini **tidak menyentuh satu pun** berkas rawat inap — lihat `git status --short` |
| 1301 | `menu-permission-filter.test.mjs` | Mengambil `subMenu[0].subItems`, mengira butir pertama Bank Darah adalah grup Setup | Literal menu versi `HEAD` diurai ulang dengan logika test yang sama: `subMenu` sudah berisi `['Order Darah', 'Permintaan PMI', 'Setup']` dan `subMenu[0].subItems` sudah `undefined` **sebelum** perubahan ini. Test rusak sejak `FE-BD-002` menambahkan "Order Darah" di urutan pertama |

Ketujuhnya **tidak diperbaiki** dalam task ini, sesuai aturan cakupan perubahan `AGENTS.md`:
masalah existing yang tidak terkait tidak boleh diperbaiki sebagai efek samping. Perbaikan
test 1301 layak dijadwalkan tersendiri karena anggapan `subMenu[0]`-nya akan terus rusak
setiap kali butir menu Bank Darah bertambah.

`AUTOMATED TEST: npm run test:unit — PASS` (8 test baru milik task ini lulus; 7 kegagalan
lain sudah ada sebelumnya dan berada di luar cakupan)

### 6.1 Validasi runtime — 23 September 2026

**Uji manual: `PASS` — 16 dari 16 pemeriksaan.**

**Cara pembuktiannya, dan kenapa cara ini sah.** Layar dijalankan sungguhan di browser
Chromium lewat Playwright, memakai hasil `next build` standalone pada
`http://127.0.0.1:3710`. Jawaban API dipasang lewat `page.route("**/v1/**")`, mengikuti pola
e2e yang sudah mapan di repository ini (`tests/e2e/lab-*.spec.mjs`,
`tests/e2e/inpatient-*.spec.mjs`). Yang diuji tetap **produk yang sebenarnya** — React, Redux,
hook, gerbang kewenangan, dan komponen yang sama persis dengan yang dipakai pengguna; yang
digantikan hanya jawaban jaringannya.

Cara ini justru **lebih kuat** daripada menguji ke backend dev untuk acceptance (2) dan (3),
karena keenam nilai `BillingHandoff.Kind` dapat dihadirkan satu per satu secara pasti.
Menunggu backend sungguhan memulangkan `OutcomeUnknown` atau `ReconciliationRequired` tidak
dapat dijadwalkan, sedangkan justru di situlah letak risiko yang dijaga acceptance. Cara ini
juga **nol menulis** pada lingkungan bersama: tidak ada tindakan yang benar-benar berpindah ke
`Completed`, dan tidak ada fakta biaya yang benar-benar masuk ke ledger Billing.

Perintah: `npx playwright test tests/e2e/blood-bank-procedure-screen.spec.mjs --workers=1`
— **16 passed (24,7 detik)**.

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `R1` | Mencatat tindakan: pilih order darah, tindakan bertarif, dan dokter BDRS, lalu simpan. Badan permintaan diperiksa berisi **tepat** `bloodOrderId`, `procedureRefId`, `bdrsDoctorId`, dan hasilnya berstatus **Dicatat** | `PASS` |
| `R1b` | Tidak satu pun kiriman memuat `tariff`, `tariffId`, `tariffAmount`, `price`, `amount`, `serviceUnitId`, `patientClassId`, maupun `performedByUserId` | `PASS` |
| `R2` (`Emitted`) | Status menjadi **Selesai**, penanda berjudul "Fakta biaya diserahkan ke Billing", pesan backend tampil apa adanya, toast bervarian sukses, dan **nol** toast peringatan | `PASS` |
| `R2` (`Replayed`) | Sama, dengan judul "Fakta biaya sudah tercatat di Billing" | `PASS` |
| `R3` (`RejectedByBilling`) | Tindakan **tetap Selesai**; penanda berjudul "Fakta biaya ditolak Billing" ber-`role="alert"`, memuat pesan backend apa adanya; **nol** judul keberhasilan; toast peringatan, **nol** toast sukses | `PASS` |
| `R3` (`OutcomeUnknown`) | Tetap Selesai; judul "Hasil penyerahan fakta biaya belum pasti"; nol klaim berhasil | `PASS` |
| `R3` (`ReconciliationRequired`) | Tetap Selesai; judul "Penyerahan fakta biaya memerlukan rekonsiliasi"; nol klaim berhasil | `PASS` |
| `R3d` | `BillingHandoff` kosong → layar **hanya** menyatakan tindakan selesai; kelima judul penanda Billing nol kemunculan | `PASS` |
| `R3e` | Sesudah penanda penolakan tampil, tombol **Muat ulang** ditekan; `GET` memulangkan `BillingHandoff` `null` dan penandanya **hilang** — tidak ada status Billing tetap yang dikarang dari `GET` | `PASS` |
| `R4a` | Tanpa `BloodBankProcedure : Create`, tombol **+ Catat Tindakan** nol kemunculan, sedangkan halamannya tetap terbaca | `PASS` |
| `R4b` | Tanpa `BloodBankProcedure : Update`, tombol **Selesaikan Tindakan** nol kemunculan, sedangkan detailnya tetap terbaca | `PASS` |
| `R4c` (3 kasus) | Tanpa `BloodOrder : Read`, tanpa `Procedure : Read`, atau tanpa `Doctor : Read`, tombol **+ Catat Tindakan** nol kemunculan — disembunyikan, bukan ditampilkan lalu ditolak `403` | `PASS` |
| `R5a` | `POST /{id}/complete` terkirim **tanpa badan permintaan** (`body` bernilai `null`) | `PASS` |
| `R5b` | Sesudah Billing menolak, ditunggu 3 detik: **nol** panggilan `resend-cost-fact`, **tepat satu** panggilan `complete`, dan **nol** tombol bertuliskan "kirim ulang" | `PASS` |

**Tiga kegagalan pada jalannya yang pertama, dan penyebabnya.** Jalan pertama menghasilkan
13 lulus dan 3 gagal. Ketiganya **cacat pada spec, bukan cacat produk**, dan dicatat di sini
apa adanya:

1. `R1` dan `R1b` mencari tombol pemilih memakai teks placeholder. Snapshot aksesibilitas
   halaman membuktikan nama tombolnya adalah **label field**-nya — `Order Darah *`,
   `Tindakan Bertarif`, `Dokter BDRS Penanggung Jawab`. Spec disesuaikan dengan keadaan
   sebenarnya; layar tidak diubah.
2. `R3e` menyuruh `GET` memulangkan tindakan yang sudah selesai sejak awal, sehingga tombol
   **Selesaikan Tindakan** memang tidak pernah ada — layar berperilaku benar. Spec diperbaiki
   supaya menirukan backend sungguhan: `GET` baru memulangkan `Completed` **sesudah**
   `complete` berhasil.

Sesudah ketiganya diperbaiki, 16 dari 16 lulus. Nol baris source produk diubah untuk
membuat pemeriksaan ini lulus.

**Prasyarat yang dipasang:** `npx playwright install chromium` — mengunduh Chromium ke cache
Playwright milik pengguna (`~/AppData/Local/ms-playwright`). Ini prasyarat script `test:e2e`
yang memang sudah ada di `package.json`; **nol** perubahan pada `package.json` maupun
lockfile.

**Tidak dijalankan:** `npm run test:uat` — tidak diminta task. Pengujian terhadap backend dev
sungguhan juga tidak dijalankan, karena `complete` bersifat terminal dan menyerahkan fakta
biaya sungguhan ke ledger Billing bersama; pembuktian acceptance tidak menuntutnya.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Daftar, pencatatan, dan penyelesaian tindakan dapat dijalankan dari layar `FE-BD-07` memakai endpoint kontrak | **Terpenuhi** | Runtime `R1` — daftar terbuka, tindakan tercatat lewat `POST /blood-bank-procedures`, dan `R2`/`R3` membuktikan penyelesaian berjalan lewat `POST /{id}/complete`. Path keempat endpoint terbukti benar lewat probe `401` versus kontrol `404` |
| (2) Sesudah `POST /{id}/complete`, layar membaca `BillingHandoff` dan menampilkan hasilnya apa adanya, termasuk pesan dari backend | **Terpenuhi** | Runtime `R2` dan `R3` pada **lima** nilai `Kind`: pesan backend tampil apa adanya pada setiap kasus, diperiksa dengan kalimat yang persis sama dengan yang dikirim server |
| (3) Layar tidak pernah menyatakan penyerahan berhasil bila `Kind` bukan `Emitted`/`Replayed`; bila `BillingHandoff` kosong, layar hanya menyatakan tindakan selesai | **Terpenuhi** | Runtime `R3` — pada `RejectedByBilling`, `OutcomeUnknown`, dan `ReconciliationRequired`, kedua judul keberhasilan nol kemunculan dan toast sukses nol kemunculan, sementara statusnya tetap **Selesai**. `R3d` membuktikan `BillingHandoff` kosong tidak memunculkan satu pun penanda Billing. Ditopang 8 test unit pada tingkat logika |
| (4) Layar tidak menyediakan `resend-cost-fact`, tidak mengarang status Billing tetap dari `GET`, dan tidak mengirim ulang otomatis | **Terpenuhi** | Runtime `R5b` — nol panggilan `resend-cost-fact`, tepat satu panggilan `complete` sesudah ditunggu 3 detik, dan nol tombol "kirim ulang". Runtime `R3e` — penanda Billing hilang sesudah muat ulang, karena `GET` memulangkan `null`. Ditopang pembuktian ketiadaan lewat `grep` |
| **DoD** — butir menu terdaftar (keputusan pemilik `G2`, 23 September 2026) | **Terpenuhi** | `menu-items.jsx` memuat "Tindakan Bank Darah" dengan `requiredPermission: { resource: "BloodBankProcedure", action: "Read" }`; butirnya terlihat pada snapshot halaman runtime |

**Tambahan di luar acceptance yang ikut terbukti runtime:** kewenangan. `R4a`, `R4b`, dan
ketiga kasus `R4c` membuktikan kelima butir hak akses benar-benar menahan tombolnya, dan
tombol yang tidak berhak **disembunyikan** — bukan ditampilkan lalu ditolak `403`. `R1b` dan
`R5a` membuktikan layar tidak pernah mengirim harga, tarif, unit, maupun kelas pasien.

**Butir yang belum terpenuhi:** tidak ada. Keempat acceptance criteria dan DoD-nya terbukti,
masing-masing dengan bukti yang dapat ditelusuri ke nomor skenario pada bagian 6.1.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru dari `lint` maupun `build` yang berasal dari perubahan ini |
| Masalah yang diketahui | 7 test unit yang gagal sudah ada sebelum task ini dan sengaja tidak diperbaiki. Test `menu-permission-filter.test.mjs` nomor 1301 akan terus rusak setiap kali butir menu Bank Darah bertambah, karena menganggap `subMenu[0]` adalah grup Setup; perbaikannya layak dijadwalkan tersendiri |
| Dependency backend | `NONE` yang menahan. `BE-BD-012` dan `BE-BD-013` keduanya ✅ dan terbukti di source. Catatan kontrak, bukan penahan: endpoint tindakan tidak punya `GET /summary` dan `GET /filters/metadata`, dan `BillingHandoff` selalu kosong pada `GET` |
| Perubahan sampingan | Satu-satunya berkas existing yang disentuh **dengan sengaja** adalah `menu-items.jsx`, dan hanya satu butir menu yang ditambahkan. **Dipulihkan:** menjalankan Playwright menimpa `test-results/.last-run.json` dan menghapus satu artefak milik test Rawat Inap, karena `test-results/` dilacak Git di repository ini. Keduanya dikembalikan lewat `git checkout -- test-results/`, sehingga nol jejak tersisa. Chromium Playwright terunduh ke cache pengguna sebagai prasyarat `test:e2e`; nol perubahan pada `package.json` dan lockfile |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | Task ini selesai. Yang layak dijadwalkan berikutnya adalah **`FE-BD-004`** (alokasi kantong, `BE-BD-006` ✅), karena `FE-BD-012` tertahan menunggu `BE-BD-020` — penyaring kantong di lokasi nonaktif. Di luar Bank Darah, `tests/unit/menu-permission-filter.test.mjs` nomor 1301 layak diperbaiki tersendiri: anggapan `subMenu[0]`-nya akan terus rusak setiap kali butir menu Bank Darah bertambah |

```text
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/blood-bank-management/blood-bank-procedures/
?? src/components/view/health-services/blood-bank-management/blood-bank-procedures/
?? src/lib/constants/health-services/blood-bank-management/blood-bank-procedure-constants.jsx
?? src/lib/hooks/health-services/blood-bank-management/use-blood-bank-procedure-detail.jsx
?? src/lib/hooks/health-services/blood-bank-management/use-blood-bank-procedure-list.jsx
?? src/lib/services/health-services/blood-bank-management/blood-bank-procedure.service.js
?? src/style/health-services/blood-bank-management/blood-bank-procedures/
?? src/utils/health-services/blood-bank-management/blood-bank-procedure-utils.js
?? tests/e2e/blood-bank-procedure-screen.spec.mjs
?? tests/unit/blood-bank-procedure-billing-handoff.test.mjs
```

Repository backend (laporan dan bukti roadmap):

```text
 M docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md
 M docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md
?? docs/module-blueprints/bank-darah/task/report/frontend/FE-BD-010.md
```
