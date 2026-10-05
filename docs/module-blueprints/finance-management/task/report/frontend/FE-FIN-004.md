# Laporan Perubahan Frontend — `FE-FIN-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-004` |
| Judul | Penerimaan dan alokasi |
| Slice | `MVP-2` (`FIN-05`) — dikerjakan sebagai prasyarat `FE-FIN-013` atas instruksi eksplisit pemilik repository |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 4 (tabel task, baris `FE-FIN-004`) dan `roadmap/00-delivery-roadmap.md` bagian 4 |
| Trace | `FR-FIN-030`..`046`; `FIN-DEC-024`..`029`; `FIN-STATE-1.0` §4; `BKC-DEC-106`/`108`/`109` |
| Contract version | `FIN-API-1.0` — permukaan collection dikecualikan dari penguncian |
| Wewenang UI | `FIN-DEC-024`..`029` (rute flat `/finance/...`, penamaan menu, koreksi/penghapusan via modal — lihat bagian 3.3). Rekonsiliasi shift (`FIN-DEC-027`, halaman penuh terpisah) **kini dipakai** — lihat bagian 2 lanjutan |
| Dependency | `BE-FIN-016`..`018` ✅ selesai 23 September 2026 (UI brief closed) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); >8 berkas dibaca (roadmap, kontrak, DTO/service/controller backend, 6 berkas frontend rujukan pola) (skor 1); 10 berkas dibuat (skor 1); logika UI sedang — alokasi multi-baris, pencarian piutang, pembalikan alokasi (skor 1); kontrak API — endpoint sudah ada, dipakai apa adanya (skor 0); tanpa perubahan skema (skor 0); tanpa keputusan otorisasi baru (skor 0); UI/workflow — layar baru bercakupan sedang (skor 1). Total 4 → `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/receipts/**`, `src/components/view/finance/receivable/receipt/**`, `src/lib/hooks/finance/receivable/use-finance-receipt-*.jsx` (baru), `src/lib/constants/finance/receivable/finance-receipt-constants.jsx` (baru); `NewQuilvianSystemBackend` — laporan ini dan tautan bukti pada roadmap modul yang sama |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | Belum di-commit — branch `yasmina`, HEAD `a31da3c2` |
| Commit backend yang dijadikan rujukan | `50f29ccc` |
| Tanggal | 30 September 2026 (lanjutan setelah `BE-FIN-050` menutup gap backend register/rekonsiliasi shift) |
| Status | 🟡 **SEBAGIAN.** Seluruh lima cakupan asli task kini punya layar (Penerimaan, alokasi manual, koreksi, buku register, rekonsiliasi shift); `npm run lint:errors` PASS, `npm run build` PASS. Yang menahan `✅` murni verifikasi manual/runtime — lihat bagian 6 |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini, tidak ada satu pun layar frontend yang memanggil kontrak `FIN-API-1.0` untuk
`FinReceipt` (`api/v1/corporate/finance-management/receipts`). Dua artefak yang tampak relevan
ternyata bukan implementasi task ini:

- `finance-receivable-detail-view.jsx` (`/finance/receivable/[slug]`) — ini layar **koreksi dan
  penghapusan piutang** (`FinReceivableAdjustment`/`FinReceivableWriteOff`, state-transition-matrix
  §3), bukan alokasi penerimaan. Sudah ada dan berfungsi — **tidak disentuh** task ini.
- `finance-payment-ar-view.jsx` (`/finance/payment-ar`) — memanggil endpoint yang **tidak ada di
  kontrak manapun** (`POST /finance/receivable/payment`), langsung menandai piutang lunas tanpa
  konsep `FinReceipt`/alokasi/pembalikan. Artefak lama yang tidak sesuai kontrak — **tidak
  disentuh, tidak dijadikan dasar**.

**Temuan penting soal cakupan backend nyata** (`FinanceReceiptsController.cs`,
`FinanceReceiptService.cs` — dibaca langsung sebagai bukti otoritatif, bukan dokumentasi):

| Kemampuan | Status nyata di source | Sumber |
| --- | --- | --- |
| `GET /receipts` (daftar berpaging) | **Ada dan berfungsi** — `GetPagedAsync` lengkap (filter Search/Status/SourceType/tanggal, multi-sort) | Terverifikasi langsung di `FinanceReceiptService.cs`. **Catatan:** komentar XML controller dan laporan `BE-FIN-018.md`/`api-contract.md` masih menyebut endpoint ini "belum ada" — dokumentasi tertinggal dari source, bukan sebaliknya |
| `GET /receipts/{id}` | Ada dan berfungsi (`GetByIdAsync`, termasuk `Allocations`) | — |
| `POST /receipts/{id}/allocations` | Ada dan berfungsi (`AllocateAsync`) | — |
| `POST /receipts/{id}/allocations/{allocationId}/reverse` | Ada dan berfungsi (`ReverseAllocationAsync`) | — |
| `GET /receipts/register` (buku penerimaan kasir) | Awalnya tidak ada; **ditutup `BE-FIN-050`** (✅, sesi lanjutan) | — |
| `GET /receipts/shift-reconciliation` | Awalnya tidak ada; **ditutup `BE-FIN-050`** (✅, sesi lanjutan) | — |
| `POST /receipts` (pencatatan manual non-kasir) | **Tidak ada** — hanya `CreateFromTenderIntakeAsync` (dipanggil internal dari Billing intake), tidak ada jalur manual petugas | — |
| `POST /receipts/{id}/reverse` (pembalikan penuh manual) | **Tidak ada** — `CreateReversalReceiptAsync` hanya dipanggil otomatis dari Billing intake | — |
| Penghapusan baris penerimaan | **Tidak pernah sah di level mana pun** (`state-transition-matrix.md` §4 baris "Hapus baris" — "Permintaan tidak disediakan API-nya") | Kontrak sendiri menegaskan ini bukan gap, tapi aturan permanen |

Atas dasar bukti ini, cakupan task pada pengerjaan awal dipersempit ke: **Penerimaan
(daftar+rincian) dan alokasi manual (susun + balik satu baris)**. Setelah `BE-FIN-050` menutup gap
register/rekonsiliasi shift, sesi lanjutan ini menambahkan kedua layarnya. "Pencatatan penerimaan
manual" dan "pembalikan penuh manual" tetap gap backend terbuka, dilaporkan apa adanya — bukan
dikarang di frontend.

### 1.1 Lanjutan (30 September 2026) — Buku Register dan Rekonsiliasi Shift

Setelah `BE-FIN-050` (✅, [laporan](../backend/BE-FIN-050.md)) menyediakan `GET /receipts/register`
dan `GET /receipts/shift-reconciliation`, dua layar tambahan dibangun untuk melengkapi cakupan
asli task ini:

- **Buku Register Penerimaan** (`/finance/receipts/register`) — daftar berpaging penerimaan per
  rentang tanggal/shift, menonjolkan `KwitansiNumber` dan `SourceTenderId` untuk telusur balik ke
  Billing (berbeda dari Daftar Penerimaan umum yang tidak menonjolkan kolom itu).
- **Rekonsiliasi Shift Kasir** (`/finance/receipts/shift-reconciliation`, halaman penuh terpisah
  sesuai `FIN-DEC-027`) — petugas mencari satu shift kasir lewat nomor shift (endpoint
  `CashierShift` milik Billing, dibaca ulang tidak diubah), lalu layar membandingkan `SystemCash`
  (Billing) dengan penerimaan tunai bersih Finance untuk shift itu, beserta selisihnya — seluruhnya
  dari response backend, nol perhitungan di klien.

Kedua layar dijangkau lewat dua tombol baru pada Hero halaman **Daftar Penerimaan** ("Buku
Register", "Rekonsiliasi Shift") — bukan lewat menu sidebar (tetap wewenang `FE-FIN-014`).

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Petugas AR (menyusun dan membalik alokasi).

**Langkah normal:**

1. Petugas AR membuka **Penerimaan** (`/finance/receipts`) — daftar penerimaan dari tender Billing
   maupun sumber lain, dapat disaring nomor, sumber, status, dan rentang tanggal diterima.
2. Klik dua kali satu baris membuka **Detail Penerimaan** (`/finance/receipts/{id}`) — menampilkan
   nominal, sisa belum teralokasi, dan daftar alokasi yang sudah ada.
3. Selama masih ada sisa belum teralokasi dan penerimaan belum dibalik, tombol **+ Alokasikan
   Penerimaan** membuka modal (`FIN-DEC-028`). Petugas menyusun satu atau beberapa baris: mencari
   piutang lewat nomor (opsional — kosong berarti pelunasan langsung ke tagihan tanpa piutang),
   mengisi nominal per baris. **Sistem tidak pernah mencocokkan otomatis** — petugas yang memilih
   sendiri (`FR-FIN-040`, `FIN-DES-013`). Setelah disimpan, detail dimuat ulang dan daftar alokasi
   bertambah.
4. Bila ada alokasi yang keliru, petugas menekan **Balik** pada baris tersebut → modal konfirmasi →
   `POST /{id}/allocations/{allocationId}/reverse` membuat baris pembalik baru (baris asli **tidak
   pernah dihapus**, `FIN-DES-012`); sisa piutang dan sisa penerimaan kembali ke keadaan sebelumnya.
   Baris yang sudah dibalik, dan baris pembalik itu sendiri, tidak lagi punya tombol Balik.

**Jalur tidak normal:**

- **Memuat:** spinner + teks "Mengambil data penerimaan...", "Memuat detail penerimaan...".
- **Kosong:** "Data penerimaan tidak ditemukan.", "Belum ada alokasi tercatat untuk penerimaan ini."
- **Gagal:** pesan error backend apa adanya (mis. "Alokasi melebihi sisa penerimaan. Sisa saat ini
  Rp X.") ditampilkan lewat alert/toast, bukan pesan generik yang dikarang frontend.
- **Tanpa hak akses:** `AccessDeniedGate` menangani `403` konsisten dengan seluruh layar Finance.
- **Penerimaan sudah dibalik (`REVERSED`):** tombol Alokasikan disembunyikan — backend menolak
  `422` ("uangnya sudah tidak ada"), tapi layar sudah mencegahnya duluan secara UX.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` (kartu task
  `FE-FIN-004`, bagian 5 "Yang MUST diputuskan UI brief", bagian 6 `DEV_DISCRETION`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (grup Receipt),
  `permission-audit-matrix.md` (resource `FinanceReceipt`), `state-transition-matrix.md` §4,
  `validation-matrix.md` (`FIN-VAL-033`, `118`, `121`, `128`, `137`)
  , `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-018.md`, `BE-FIN-040.md`
- `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs`,
  `Services/FinanceReceiptService.cs`, `Dtos/FinanceReceiptDtos.cs`,
  `Models/FinReceipt(Allocation|Deduction).cs` — dibaca sebagai bukti otoritatif runtime,
  **tidak diubah**
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs`,
  `Dtos/FinanceReceivableDtos.cs` (`ReceivableQuery.Search`) — dipakai widget pencarian piutang
  di modal alokasi, dibaca **tidak diubah**
- Frontend — pola rujukan terdekat: `invoice-exchange-view.jsx`/`use-invoice-exchange-list.jsx`
  (daftar berpaging hook-based), `invoice-exchange-detail-view.jsx`/`use-invoice-exchange-detail.jsx`
  (detail + aksi + `ConfirmModal`), `supplier-return-form-view.jsx` (tabel baris mentah di dalam
  form), `add-tender-modal.jsx` (pola form modal dengan `Modal` react-bootstrap langsung),
  `finance-receivable-detail-view.jsx` (dikonfirmasi **bukan** cakupan task ini, hanya dibaca untuk
  memastikan tidak tumpang tindih)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/receivable/finance-receipt-constants.jsx` | **Baru.** Endpoint base, status (`RECEIVED`/`ALLOCATED`/`RECONCILED`/`REVERSED`), source type, target type alokasi, label/tone |
| `src/lib/hooks/finance/receivable/use-finance-receipt-list.jsx` | **Baru.** Hook Daftar Penerimaan — `GET /` berpaging, filter search/status/sourceType/tanggal |
| `src/lib/hooks/finance/receivable/use-finance-receipt-detail.jsx` | **Baru.** Hook Detail — `GET /{id}`, turunan status "sudah dibalik"/"dapat dibalik" per baris alokasi, aksi `POST /{id}/allocations/{allocationId}/reverse` |
| `src/lib/hooks/finance/receivable/use-finance-receipt-allocation-editor.jsx` | **Baru.** Hook modal alokasi — susun banyak baris, cari piutang lewat nomor (`GET receivables?search=`), `POST /{id}/allocations` |
| `src/components/view/finance/receivable/receipt/finance-receipt-table-columns.jsx` | **Baru.** Kolom Daftar Penerimaan |
| `src/components/view/finance/receivable/receipt/finance-receipt-view.jsx` | **Baru.** Layar Daftar Penerimaan |
| `src/components/view/finance/receivable/receipt/allocate-receipt-modal.jsx` | **Baru.** Modal susun alokasi multi-baris |
| `src/components/view/finance/receivable/receipt/detail/finance-receipt-detail-view.jsx` | **Baru.** Layar Detail Penerimaan (rincian, daftar alokasi, aksi Alokasikan/Balik) |
| `src/app/finance/receipts/page.jsx` | **Baru.** Route Daftar Penerimaan |
| `src/app/finance/receipts/[slug]/page.jsx` | **Baru.** Route Detail Penerimaan |
| `src/lib/hooks/finance/receivable/use-finance-receipt-register.jsx` | **Baru (lanjutan).** Hook Buku Register — `GET /register` berpaging, filter tanggal/shift |
| `src/lib/hooks/finance/receivable/use-finance-receipt-shift-reconciliation.jsx` | **Baru (lanjutan).** Hook Rekonsiliasi Shift — cari shift (`GET` Billing `CashierShift`), lalu `GET /shift-reconciliation` untuk shift terpilih |
| `src/components/view/finance/receivable/receipt/register/finance-receipt-register-table-columns.jsx` | **Baru (lanjutan).** Kolom Buku Register (`KwitansiNumber`, `SourceTenderId`) |
| `src/components/view/finance/receivable/receipt/register/finance-receipt-register-view.jsx` | **Baru (lanjutan).** Layar Buku Register Penerimaan |
| `src/components/view/finance/receivable/receipt/shift-reconciliation/finance-receipt-shift-reconciliation-view.jsx` | **Baru (lanjutan).** Layar Rekonsiliasi Shift (halaman penuh, `FIN-DEC-027`) |
| `src/app/finance/receipts/register/page.jsx` | **Baru (lanjutan).** Route Buku Register |
| `src/app/finance/receipts/shift-reconciliation/page.jsx` | **Baru (lanjutan).** Route Rekonsiliasi Shift |
| `src/lib/constants/finance/receivable/finance-receipt-constants.jsx` | **Diperluas (lanjutan).** Tambah endpoint/rute register+rekonsiliasi shift, endpoint pencarian `CashierShift` |
| `src/components/view/finance/receivable/receipt/finance-receipt-view.jsx` | **Diperluas (lanjutan).** Tambah dua tombol navigasi Hero ("Buku Register", "Rekonsiliasi Shift") |

Menu sidebar **tidak disentuh** — "Penerimaan" bukan butir baru yang diminta task ini, dan
`FIN-DEC-025` menetapkan grup menu `corporateFinance` sudah ada tanpa penambahan butir baru untuk
kapabilitas ini; navigasi ke layar ini untuk sesi berjalan lewat tautan langsung.

### 3.3 Kepatuhan arsitektur frontend

Hook state lokal (`useState`/`useCallback`, tanpa Redux slice) mengikuti pola rumpun terbaru
`finance-management` (Purchasing `FE-FIN-008`..`011`, Batch AR `FE-FIN-012`), bukan pola Redux
slice `finance-receivable-slice.jsx` yang lebih lama — konsisten dengan seluruh layar baru
`finance-management` sejak `REV-4`. Modal form (`allocate-receipt-modal.jsx`) memakai `Modal`
react-bootstrap langsung, persis pola `add-tender-modal.jsx` — bukan pola baru.

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Layout halaman, Hero, breadcrumb, filter (search+tanggal+select), tabel daftar, pagination, toast, badge status, modal konfirmasi | `REUSE` | Identik `invoice-exchange-view.jsx`/`finance-receivable-view.jsx` |
| Modal susun alokasi (form multi-baris + pencarian piutang) | `COMPOSE` — `Modal` react-bootstrap + tabel bootstrap mentah, bukan komponen baru | Opsi 1 (**dipilih**): pola identik `add-tender-modal.jsx` (form modal) digabung idiom tabel baris `supplier-return-form-view.jsx` — nol komponen baru. Opsi 2 (ditolak): bikin base component "multi-line allocator" generik — biaya dan risiko regresi jauh lebih besar untuk kebutuhan satu layar, dan melanggar aturan "tanpa abstraksi luas baru" `AGENTS.md` |
| Widget cari piutang per baris (typeahead sederhana) | `COMPOSE` — input teks + daftar hasil lokal (debounce 350ms), bukan `ResourceFilterSelect` | Opsi 1 (**dipilih**): `ResourceFilterSelect`/`select-resource-registry.js` dirancang untuk dropdown master-data (ratusan baris tetap, di-cache), sedangkan piutang berjumlah besar dan berubah cepat — mendaftarkannya ke registry bersama menambah beban ke seluruh fitur lain yang memakai file itu. Opsi 2 (ditolak): daftarkan resource baru `receivables` ke `select-resource-registry.js` — menyentuh file bersama berisiko lebih tinggi untuk manfaat yang sama |
| Tabel Daftar Alokasi pada halaman detail | `COMPOSE` — tabel bootstrap mentah, bukan `DataTable` | Jumlah baris per penerimaan kecil (bukan daftar berpaging), konsisten dengan pola `purchasing-invoice-detail-view.jsx` (tabel item read-only di halaman detail) |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Spinner + teks "Mengambil data penerimaan...", "Memuat detail penerimaan..." |
| Kosong | "Data penerimaan tidak ditemukan.", "Belum ada alokasi tercatat untuk penerimaan ini." |
| Gagal | Pesan error backend apa adanya lewat alert/toast |
| Tanpa hak akses | `AccessDeniedGate` — pesan akses ditolak standar |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receipts` | Daftar penerimaan berpaging | `FinanceReceipt : Read` |
| `GET` | `/v1/corporate/finance-management/receipts/{id}` | Rincian penerimaan beserta alokasi | `FinanceReceipt : Read` |
| `POST` | `/v1/corporate/finance-management/receipts/{id}/allocations` | Menyusun alokasi manual (tanpa baris potongan — cakupan `FE-FIN-013`) | `FinanceReceipt : Allocate` |
| `POST` | `/v1/corporate/finance-management/receipts/{id}/allocations/{allocationId}/reverse` | Membalik satu baris alokasi | `FinanceReceipt : Allocate` |
| `GET` | `/v1/corporate/finance-management/receipts/register` | Buku register penerimaan per tanggal/shift (lanjutan, `BE-FIN-050`) | `FinanceReceipt : Read` |
| `GET` | `/v1/corporate/finance-management/receipts/shift-reconciliation` | Rekonsiliasi satu shift (lanjutan, `BE-FIN-050`) | `FinanceReceipt : Read` |

Endpoint tambahan yang **dibaca ulang, tidak diubah**, dipakai widget pencarian:
`GET /v1/corporate/finance-management/receivables?search=...` (pencarian piutang untuk modal
alokasi) dan `GET /v1/health-services/billing-management/cashier/shifts?search=...` (pencarian
shift kasir untuk layar rekonsiliasi — hak akses `CashierShift : Read` milik Billing; bila peran
Finance AR belum diberi hak akses ini, pencarian gagal `403`, dicatat sebagai risiko bagian 8).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error (pengerjaan awal dan lanjutan) | `PASS` | Keluaran `eslint . --quiet` selesai tanpa output error kedua kali |
| `npm run build` | Keempat route (`/finance/receipts`, `/finance/receipts/[slug]`, `/finance/receipts/register`, `/finance/receipts/shift-reconciliation`) berhasil di-compile | `PASS` | Keluaran build mencantumkan keempat route tanpa `Failed to compile` |
| Susun alokasi multi-baris, salah satu ke piutang, satu lagi kosong (`INVOICE_DIRECT`) | — | `NOT FEASIBLE` | Memerlukan backend berjalan dan data `FinReceipt` nyata untuk diuji |
| Balik satu alokasi, verifikasi baris asli terkunci dan baris pembalik tidak bisa dibalik lagi | — | `NOT FEASIBLE` | Sama seperti di atas |
| Filter daftar (status, sumber, rentang tanggal, pencarian nomor) | — | `NOT FEASIBLE` | Sama seperti di atas |

Uji manual: `NOT FEASIBLE` — tidak ada instance backend yang dikonfirmasi berjalan pada sesi ini
untuk diuji hidup; verifikasi yang dilakukan bersifat review kode terhadap
`FinanceReceiptService.cs`/`FinanceReceiptsController.cs` sebagai bukti kontrak nyata (bukan
tebakan), bukan eksekusi runtime.

**AUTOMATED TEST:** `NOT APPLICABLE — repository ini tidak memelihara Jest/test runner otomatis (rules/frontend/test-policy.md); pembuatan test baru bersifat opsional dan tidak diminta eksplisit pada task ini.`

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat` — tidak diminta task ini.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Alokasi dipilih petugas, bukan dicocokkan otomatis (satu-satunya acceptance criteria tertulis roadmap) | Terpenuhi | `allocate-receipt-modal.jsx`/`use-finance-receipt-allocation-editor.jsx` — petugas mencari dan memilih piutang sendiri per baris, nol logika pencocokan otomatis di klien maupun yang dipanggil dari backend (`AllocateAsync` juga eksplisit "TIDAK ADA pencocokan otomatis") |
| Cakupan "Penerimaan" (daftar+rincian) | Terpenuhi | `finance-receipt-view.jsx`, `finance-receipt-detail-view.jsx` |
| Cakupan "alokasi manual" | Terpenuhi | `allocate-receipt-modal.jsx` → `POST /{id}/allocations` |
| Cakupan "koreksi" (dibaca sebagai pembalikan alokasi sisi penerimaan) | Terpenuhi | Tombol Balik → `POST /{id}/allocations/{allocationId}/reverse` |
| Cakupan "penghapusan" | `NOT APPLICABLE` — state-transition-matrix.md §4 menegaskan penghapusan baris penerimaan **tidak pernah sah**, dan bukti terpisah menunjukkan "koreksi/penghapusan piutang" sudah tercakup layar `finance-receivable-detail-view.jsx` (`FinReceivableAdjustment`/`WriteOff`) yang sudah ada — di luar cakupan tulis task ini | Lihat bagian 1 dan 8 |
| Cakupan "rekonsiliasi shift" | Terpenuhi | `finance-receipt-shift-reconciliation-view.jsx` → `GET /shift-reconciliation` (`BE-FIN-050`) |
| Cakupan "buku register" | Terpenuhi | `finance-receipt-register-view.jsx` → `GET /register` (`BE-FIN-050`) |

Task ini **belum** dapat ditandai `✅` — satu-satunya butir tersisa adalah verifikasi manual/runtime
terhadap backend hidup, belum terpenuhi karena tidak ada instance backend yang dikonfirmasi
berjalan pada sesi ini.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini awalnya dipanggil sebagai prasyarat `FE-FIN-013` setelah ditemukan bahwa layar "alokasi penerimaan" yang seharusnya sudah ada (menurut cakupan `FE-FIN-013`) ternyata belum pernah dibangun sesuai kontrak — dikonfirmasi ke pemilik repository sebelum dikerjakan |
| Masalah yang diketahui | (1) `POST /receipts` (pencatatan manual non-kasir) dan `POST /receipts/{id}/reverse` (pembalikan penuh manual oleh Treasury) tetap nol method service — masih di luar cakupan yang bisa dibangun, tidak diminta pada sesi ini. (2) Dokumentasi backend (komentar XML controller, `BE-FIN-018.md`, `api-contract.md`) sempat menyebut `GET /receipts` (daftar) sebagai "belum ada", padahal `FinanceReceiptService.GetPagedAsync` sudah lengkap dan terhubung controller sejak sebelum task ini — delta dokumentasi-vs-source ini sudah dilaporkan ke pemilik backend. (3) Widget pencarian shift kasir pada layar Rekonsiliasi memakai endpoint Billing (`CashierShift : Read`) — bila peran Finance AR yang memakai layar ini belum diberi hak akses tersebut, pencarian akan gagal `403`; ini keputusan RBAC pemilik akses, bukan bug layar |
| Dependency backend | `BE-FIN-016`..`018` ✅ selesai 23 September 2026 — source dikonfirmasi ada dan sesuai lewat pembacaan langsung. `BE-FIN-050` ✅ menutup gap register/rekonsiliasi shift. Dua kemampuan (pencatatan manual, pembalikan manual) **tidak** memiliki service backend sama sekali — bukan status "belum diverifikasi", tapi "belum dibangun", dan tidak diminta pada sesi ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` (repository frontend) mencakup task ini: file baru `src/app/finance/receipts/**` (termasuk `register/`, `shift-reconciliation/`), `src/components/view/finance/receivable/receipt/**`, `src/lib/hooks/finance/receivable/use-finance-receipt-*.jsx`, `src/lib/constants/finance/receivable/finance-receipt-constants.jsx`. Perubahan milik task frontend lain (`FE-FIN-008`..`012`, rumpun Purchasing/Batch AR) juga terlihat pada `git status` — tidak disentuh, tidak dilaporkan di sini |
| Langkah berikutnya | (1) Jalankan backend dan uji kelima layar (Daftar, Detail, Alokasi, Buku Register, Rekonsiliasi Shift) hidup terhadap data `FinReceipt`/`BilCashierShift` nyata; (2) minta pemilik akses memastikan peran Finance AR punya `CashierShift : Read` untuk widget pencarian shift; (3) setelah runtime terbukti, perbarui laporan ini dan tanda status roadmap dari 🟡 ke ✅ |
