# Laporan Perubahan Frontend — `FE-BD-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-003` |
| Judul | Petugas mengelola permintaan PMI dan penerimaan |
| Layar | `FE-BD-03` — daftar kerja + detail, dengan tiga aksi: buat permintaan, catat penerimaan, batalkan |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) kartu `FE-BD-003` |
| Trace | `DEC-BD-061`, `DEC-BD-062`; `DEC-BD-020`; `VAL-BD-006`, `VAL-BD-007`, `VAL-BD-014`, `VAL-BD-016` |
| Contract version | `v4` — Provider Request. Nol amandemen `v5` untuk grup ini |
| Dependency | `BE-BD-004` ✅, `BE-BD-015` ✅, `DEC-BD-061` ✅, `DEC-BD-062` ✅ — nol penahan |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev` — `src/**` saja; nol perubahan source backend |
| Model | Claude Opus 5 |
| Branch frontend | `sukmagpV2` |
| Commit frontend saat sesi dimulai | `600136722` |
| Tanggal | 23 September 2026 |
| Status | ✅ **SELESAI — 23 September 2026. Empat dari lima acceptance terbukti runtime di layar sungguhan, `PASS` seluruhnya, nol skenario gagal.** `AC-BD-014` **tidak dapat dibuktikan dari UI** dan dicatat sebagai **backend-only validation** dengan bukti empiris, bukan dugaan (bagian 7). Lint `0 error`; build **`exit 0`**, `✓ Compiled successfully in 36.1s`, **368/368** halaman; `git diff --check` bersih. Validasi dijalankan langsung agent lewat Playwright/Chromium terhadap backend lokal di atas `QuilvianNewDevSukma` |

---

## 1. Keadaan yang ditemukan di awal

**Nol source permintaan PMI ada di frontend.** Pencarian `provider-request|providerRequest|ProviderRequest`
di seluruh `src/` memulangkan nol hasil, dan folder Bank Darah hanya berisi `blood-orders`. Butir menu
"Permintaan PMI" juga belum ada, walaupun `03-frontend-architecture.md` sudah menetapkannya sejak awal.

Backend-nya sudah lama siap: `BE-BD-004` ✅ sejak 11 September 2026, delapan endpoint berdiri, dan dua
kriteria yang menyentuh `PendingReview` sudah diteruskan ke `BE-BD-015` ✅.

Audit sebelum implementasi menemukan enam gap. Tiga di antaranya menuntut keputusan pemilik, dan
implementasi **ditahan** sampai ketiganya diputuskan — bukan ditebak.

---

## 2. Keputusan yang mengikat task ini

| ID | Isi | Akibatnya bagi layar |
| --- | --- | --- |
| `DEC-BD-061` | Pembatalan permintaan PMI memakai alasan terkendali **aktif tanpa penyaring kategori**. Kategori milik pembatalan order darah **tidak** dipakai | `GET /blood-bank-reasons/options` dipanggil **tanpa** `category`. Modal menulis konteks "Pembatalan Permintaan PMI" supaya petugas tahu alasan mana yang sedang dipilih |
| `DEC-BD-062` | Pemilih order memakai `GET /blood-orders` yang sudah ada, disaring ke status layak `Active` dan `PartiallyFulfilled`. **Nol endpoint baru, nol task backend baru** | Endpoint yang sama dipanggil **dua kali** — sekali per status — lalu hasilnya digabung, karena `orderStatus` hanya menerima satu nilai |
| Perluasan acceptance | `AC-BD-005`, `AC-BD-006`, `AC-BD-009`, `AC-BD-014`, `AC-BD-031` menjadi kriteria layar. `AC-BD-022`/`AC-BD-023` tetap **integrasi**, bukan kriteria layar | Layar wajib menampilkan efek `ClosedEncounter` dan `PendingReview` apa adanya, tanpa menurunkannya sendiri |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dibuat

| Berkas | Isi |
| --- | --- |
| `lib/constants/.../provider-request-constants.jsx` | Route, API base, lima status beserta badge, copy layar, `ORDER_STATUS_ELIGIBLE_FOR_PROVIDER_REQUEST` |
| `lib/services/.../provider-request.service.js` | Delapan endpoint, `getEligibleBloodOrders` (`DEC-BD-062`), `getProviderRequestCancelReasons` (`DEC-BD-061`) |
| `lib/hooks/.../use-provider-request-list.jsx` | Daftar, penyaring, reset, ringkasan, modal buat permintaan |
| `lib/hooks/.../use-provider-request-detail.jsx` | Detail, penerimaan kantong, pembatalan, gating per butir hak akses |
| `components/view/.../provider-requests/provider-request-list-view.jsx` | Layar daftar dan modal buat |
| `components/view/.../provider-requests/provider-request-table-columns.jsx` | Delapan kolom |
| `components/view/.../provider-requests/detail/provider-request-detail-view.jsx` | Detail, tiga seksi, dua modal |
| `app/.../provider-requests/page.jsx` · `[slug]/page.jsx` · `[slug]/route-token.js` | Route |
| `style/.../provider-requests/provider-request.module.css` | Hanya style domain-specific |

### 3.2 Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir "Permintaan PMI", digate `BloodProviderRequest : Read` |

`blood-order-utils.js` **tidak** di-rename atas instruksi pemilik, dan diimpor apa adanya. Namanya sudah
tidak mewakili isinya — catatan itu tetap terbuka pada bagian 8.

### 3.3 Gerbang keputusan base component

**Nol komponen `base-features/` baru.** Yang dipakai ulang: `hero`, `data-filter`, `data-table`,
`base-detail-view`, `base-detail-section`, `summary-grid`, `status-badge`, `information-alert`,
`confirm-modal`, `filter-select`, `base-form-control` (`BaseTextField`, `BaseSelectField`),
`access-denied-gate`, `toast-stack`, `use-select-resource`.

---

## 4. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/provider-requests` | Daftar kerja | `BloodProviderRequest : Read` |
| `GET` | `/provider-requests/summary` | Kartu ringkasan | `BloodProviderRequest : Read` |
| `GET` | `/provider-requests/filters/metadata` | Opsi status dan ukuran halaman | `BloodProviderRequest : Read` |
| `GET` | `/provider-requests/{id}` | Detail, komponen, riwayat penerimaan, riwayat status | `BloodProviderRequest : Read` |
| `POST` | `/provider-requests` | Buat permintaan — body hanya `{ bloodOrderId }` | `BloodProviderRequest : Create` |
| `POST` | `/provider-requests/{id}/receipts` | Catat kedatangan kantong | `BloodProviderRequest : Process` |
| `POST` | `/provider-requests/{id}/cancel` | Batalkan dengan alasan terkendali | `BloodProviderRequest : Update` |
| `GET` | `/blood-orders?orderStatus=0` dan `=1` | Pemilih order layak (`DEC-BD-062`) | `BloodOrder : Read` |
| `GET` | `/master-data/blood-components/options` | Komponen tiap kantong | `BloodComponent : Read` |
| `GET` | `/master-data/blood-bank-reasons/options` **tanpa** `category` | Alasan pembatalan (`DEC-BD-061`) | `BloodBankReason : Read` |

`GET /provider-requests/{id}/status-history` **tidak** dipanggil — riwayat sudah dibawa payload detail.

---

## 5. Tiga keputusan desain pelaksanaan

**Kelebihan diturunkan dari angka, bukan dari kalimat.** `VAL-BD-014` dipulangkan `200` dengan peringatan
di `message` dan **tanpa** slot `errors`. Layar menurunkan penandanya dari `totalExcessQuantity > 0`;
`message` hanya ditampilkan apa adanya pada toast. Ini aturan yang sama dengan yang ditegakkan
`FE-BD-002` terhadap `VAL-BD-001`.

**`VAL-BD-006` ditampilkan utuh tanpa ditafsirkan.** Penolakan permintaan ganda juga `422` **tanpa**
`errors.code` — berbeda dari `VAL-BD-001` pada order darah yang membawa kode terstruktur. Karena tidak
ada kode untuk dibaca, layar **tidak menafsirkan sama sekali**: kalimat backend ditampilkan apa adanya.

**Kelayakan aksi dibaca dari `availableActions` backend.** Tombol Catat Penerimaan dan Batalkan muncul
hanya bila backend menyebut `Receive` / `Cancel` **dan** butir hak aksesnya ada. Nilai `Receive`
diperiksa langsung ke `AvailableActionsOf` — dugaan awal `RecordReceipt` keliru dan dikoreksi sebelum
kode dijalankan.

**Nol perhitungan di layar.** Diminta, diterima, berlebih, dan sisa seluruhnya dari backend.

---

## 6. Verifikasi statis

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` atas seluruh scope Bank Darah | **Nol keluaran — `0 error`** | `PASS` |
| `npm run build` | **`✓ Compiled successfully in 36.1s`**, TypeScript `376ms`, **368/368** halaman, `exit 0` | `PASS` |
| Route baru terbangun | `○ /…/provider-requests` dan `ƒ /…/provider-requests/[slug]` | `PASS` |
| Jumlah halaman naik | 367 → **368** | `PASS` |
| `git diff --check` | Bersih | `PASS` |
| Nol penafsiran kalimat pesan | Nol `message.includes` pada scope permintaan PMI | `PASS` |
| Nol perhitungan ulang angka | Nol penjumlahan/pengurangan atas quantity; seluruhnya dari backend | `PASS` |

---

## 7. Validasi runtime — DIEKSEKUSI 23 September 2026

Dijalankan langsung agent lewat Playwright/Chromium terhadap aplikasi Next.js yang berjalan, menembak
backend lokal di atas **`QuilvianNewDevSukma`**. Seluruh aksi yang diuji — buat permintaan dan catat
penerimaan — dilakukan **lewat layar**, bukan lewat API.

### 7.1 Data uji yang disiapkan

Tiga order darah baru dibuat lewat API supaya jumlah kantongnya persis sesuai skenario, karena data lama
tidak memuat kombinasi 3-diminta dan 2-diminta yang dibutuhkan:

| Order | Komponen | Jumlah | Dipakai |
| --- | --- | ---: | --- |
| `ORD-00000091` | `TEST-BD006 Packed Red Cells` | 3 | `AC-BD-005`, `AC-BD-006` |
| `ORD-00000092` | `TEST-BD007 PRC Validity24` | 2 | `AC-BD-031` |
| `ORD-00000093` | `TEST-BD008 PRC Validity24` | 1 | `AC-BD-009` |

### 7.2 `AC-BD-005` — minta 3 PRC, terima 2

Permintaan `PMI-00000046` dibuat lewat modal di layar daftar, lalu dua kantong dicatat lewat modal
penerimaan di layar detail.

| Yang diperiksa | Hasil | Klasifikasi |
| --- | --- | :---: |
| Penerimaan tercatat | `POST /receipts` → `200` | `PASS` |
| Status | **`Diterima sebagian`** | `PASS` |
| Diterima | **2** | `PASS` |
| Sisa | **1** | `PASS` |
| Berlebih | **0** | `PASS` |
| Layar menampilkan status dari backend | "Diterima sebagian" terbaca di layar | `PASS` |

### 7.3 `AC-BD-006` — permintaan kedua untuk kebutuhan yang sama

| Yang diperiksa | Hasil | Klasifikasi |
| --- | --- | :---: |
| Status HTTP | **`422`** | `PASS` |
| Kalimat backend | "Sudah ada permintaan darah yang masih berjalan untuk kebutuhan ini. Tidak boleh dibuat permintaan baru." | `PASS` |
| Kalimat tampil di layar **apa adanya** | Terbaca utuh pada modal | `PASS` |
| Slot `errors` | **`null`** — backend tidak memberi kode terstruktur, sehingga layar memang tidak punya apa pun untuk diparsing | `PASS` |

### 7.4 `AC-BD-009` — permintaan dibuat, belum ada penerimaan

Permintaan `PMI-00000045` dibuat lewat layar dan **tidak** diberi penerimaan.

| Yang diperiksa | Hasil | Klasifikasi |
| --- | --- | :---: |
| Permintaan tercipta | `200` | `PASS` |
| Diterima | **0** | `PASS` |
| Sisa = jumlah diminta | **1 = 1** | `PASS` |
| Status di layar | **`Diminta`** | `PASS` |
| Layar tidak menyatakan sudah diterima | Bagian Riwayat Penerimaan kosong; nol kantong tercatat | `PASS` |

### 7.5 `AC-BD-031` — minta 2, datang 3

Permintaan `PMI-00000047` dibuat lewat layar, lalu **tiga** kantong dicatat pada satu kedatangan.

| Yang diperiksa | Hasil | Klasifikasi |
| --- | --- | :---: |
| Penerimaan tercatat | `200` | `PASS` |
| Status | **`Terpenuhi`** | `PASS` |
| Sisa | **0 — bukan −1** | `PASS` |
| Berlebih | **1** | `PASS` |
| Ketiga kantong tercatat | Diterima **3** | `PASS` |
| Peringatan kelebihan tampil | "Kiriman melebihi permintaan. 1 kantong tercatat berlebih…" — diturunkan dari `totalExcessQuantity`, **bukan** dari kalimat `message` | `PASS` |

### 7.6 `AC-BD-014` — BACKEND-ONLY VALIDATION, tidak dapat dibuktikan dari UI

**Kesimpulan: kriteria ini tidak dapat dijangkau dari layar mana pun, dan itu dibuktikan, bukan
diduga.**

`VAL-BD-007` pada jalur permintaan PMI berasal dari `EmptyQuantityMessage` di
`BbkProviderRequestService.CreateAsync`, yang menyala ketika **order darah asal tidak punya satu pun
baris kebutuhan**. Jumlah kantong permintaan PMI memang diturunkan dari baris order — layar tidak pernah
mengisinya.

Agar penjaga itu menyala, dibutuhkan order darah tanpa baris. Order seperti itu **tidak dapat dibuat**:

| Bukti | Hasil |
| --- | --- |
| `POST /blood-orders` dengan `lines: []` | **`400`** — `"The field Lines must be a string or array type with a minimum length of '1'."` Ditolak model validation `[MinLength(1)]` sebelum menyentuh service |
| Form order darah di layar (`FE-BD-002`) | Baris terakhir tidak dapat dihapus, dan `validate()` menolak `lines.length === 0` |
| Kalimat konstanta lawan matriks validasi | `"Jumlah kantong yang diminta wajib diisi."` — **identik kata per kata** dengan `VAL-BD-007` |

Karena order tanpa baris mustahil ada, `EmptyQuantityMessage` adalah **penjaga defensif** terhadap
keadaan yang jalur pembuatan order sendiri sudah tutup. Ia tetap benar dan tetap perlu ada — yang tidak
ada adalah jalan menuju ke sana dari layar.

**Nol workaround dibuat**, sesuai instruksi pemilik. Kriteria ini dicatat sebagai **milik backend**, dan
pembuktiannya menjadi wilayah `BE-BD-004` yang memang sudah menutupnya.

### 7.7 Batas bukti

1. **Aktor tunggal `superadmin`.** Keempat kriteria yang diuji tidak membedakan pelaku, jadi aktor kedua
   tidak menambah apa pun bagi mereka. **Yang belum teruji:** perilaku layar bagi pemegang sebagian
   butir saja — misalnya `Read` tanpa `Process` — sehingga penyembunyian tombol Catat Penerimaan dan
   Batalkan baru terbukti dari source, bukan dari layar.
2. **Aksi pembatalan belum diuji runtime.** `DEC-BD-061` terbukti dari source dan dari kontrak backend
   (`GetOptionsAsync` tanpa `category` memulangkan seluruh alasan aktif), tetapi modal pembatalan belum
   pernah dibuka pada aplikasi berjalan. Itu bukan bagian dari lima acceptance yang ditetapkan pemilik.
3. **`AC-BD-022`/`AC-BD-023` tidak diuji**, sesuai penetapan pemilik: keduanya integrasi, dan efek
   `ClosedEncounter` serta `PendingReview` diturunkan backend.
4. **Data uji tertinggal.** Tiga order darah (`ORD-00000091`..`93`) dan tiga permintaan PMI
   (`PMI-00000045`..`47`) tetap ada di `QuilvianNewDevSukma`. Tidak dihapus: kontrak tidak menyediakan
   endpoint hapus, dan menghapus lewat database melanggar jejak audit.
5. **Mode `dev`.** Build produksi diverifikasi terpisah (bagian 6).

---

## 8. Risiko tersisa

| # | Risiko | Keterangan |
| :---: | --- | --- |
| 1 | **Pemilih order tidak menampilkan seluruh order** | `DEC-BD-062` memanggil `GET /blood-orders` sekali per status layak dengan `pageSize` 50. Order layak yang lebih lama dari 50 terbaru per status **tidak muncul** di pemilih. Memadai untuk memilih, tetapi bukan daftar lengkap |
| 2 | **Pembatalan menampilkan seluruh alasan aktif** | Konsekuensi langsung `DEC-BD-061`: alasan milik konteks lain — koreksi pemberian, pengembalian — ikut tampil. Petugas dibantu label konteks, bukan oleh penyaringan. Backend tetap menerima kode apa pun yang aktif |
| 3 | **`VAL-BD-006` tanpa kode terstruktur** | Layar bergantung penuh pada kalimat backend. Bila kalimatnya diubah, layar tetap benar — tetapi tidak ada cara memberi perlakuan khusus pada penahanan ini, misalnya menyorot permintaan yang bentrok |
| 4 | **Gating tombol belum terbukti runtime** | Penyembunyian Catat Penerimaan dan Batalkan bagi pemegang sebagian butir baru terbukti dari source |
| 5 | **Ketergantungan hak akses belum tertulis di kontrak** | Layar menuntut `BloodBankReason : Read` dan `BloodComponent : Read` di luar butir `BloodProviderRequest`. Belum tercatat di `permission-audit-matrix.md` — gap yang sama jenisnya dengan yang tercatat pada `FE-BD-002` bagian 8.1 |
| 6 | **`blood-order-utils.js` dipakai lintas modul dengan nama yang menyesatkan** | Permintaan PMI mengimpor dari berkas bernama "blood-order". Rename ditahan atas instruksi pemilik; makin banyak modul memakainya, makin mahal diperbaiki |

---

## 9. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-005` — minta 3, terima 2 → `Diterima sebagian`, sisa 1 | ✅ **Terpenuhi** | Runtime bagian 7.2, seluruhnya `PASS` |
| `AC-BD-006` — permintaan kedua kebutuhan sama ditolak | ✅ **Terpenuhi** | Runtime bagian 7.3 — `422`, kalimat backend tampil utuh, nol parsing |
| `AC-BD-009` — sudah diminta, belum diterima | ✅ **Terpenuhi** | Runtime bagian 7.4 — diterima 0, status `Diminta`, riwayat penerimaan kosong |
| `AC-BD-031` — minta 2, datang 3 → `Terpenuhi`, sisa 0, berlebih 1 | ✅ **Terpenuhi** | Runtime bagian 7.5, termasuk peringatan yang diturunkan dari angka |
| `AC-BD-014` — permintaan tanpa jumlah kantong ditolak `VAL-BD-007` | ⚪ **BACKEND-ONLY** | Bagian 7.6 — terbukti **tidak terjangkau dari UI**; order tanpa baris ditolak `400` oleh model validation. Nol workaround dibuat |
| `AC-BD-022` / `AC-BD-023` | ⚪ **INTEGRASI** | Ditetapkan pemilik sebagai efek backend, bukan kriteria layar |

### Definition of Done

| Butir | Status |
| --- | --- |
| Acceptance layar terbukti | ✅ Empat dari empat yang dapat diuji dari layar; kelima dicatat backend-only dengan bukti |
| Sumber data terkunci pada endpoint kontrak | ✅ Nol endpoint karangan; nol endpoint baru dibuat |
| Dilarang membuat komponen dasar tandingan | ✅ Nol komponen `base-features/` baru |
| Lint bersih | ✅ `0 error` |
| Build berhasil | ✅ `exit 0`, 368/368 halaman |
| `git diff --check` bersih | ✅ |
| Laporan tracked `task/report/frontend/FE-BD-003.md` | ✅ Berkas ini |

**Definition of Done terpenuhi.** Batas bukti yang melekat tercatat pada bagian 7.7.

---

## 10. Status Git

| Hal | Isi |
| --- | --- |
| Branch frontend | `sukmagpV2` |
| Commit saat sesi dimulai | `600136722` |
| Berkas frontend | 9 baru, 1 diubah (`menu-items.jsx`) |
| Berkas backend | **Nol source disentuh.** Yang berubah hanya dokumen: laporan ini, `00-interview-decisions.md` (`DEC-BD-061`, `DEC-BD-062`), dan `roadmap/frontend-roadmap.md` |

---

## 11. Langkah berikutnya

1. Uji gating dengan aktor yang memegang sebagian butir saja, untuk menutup risiko 4.
2. Uji runtime modal pembatalan, untuk menutup batas bukti 7.7 butir 2.
3. Catat ketergantungan `BloodBankReason : Read` dan `BloodComponent : Read` pada
   `contracts/permission-audit-matrix.md` bagian 2.
4. Putuskan nasib `blood-order-utils.js` sebelum modul ketiga ikut memakainya.
