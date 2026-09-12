# FE-BKC-030 — Panel Edit Billing

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-030` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `1.1`) |
| Task type | Frontend, satu panel baru (`FE-MPY-04`) dipasang ke kerangka `FE-BKC-028` — **panel terakhir**, seluruh slot toolbar `FE-MPY-01` kini terisi |
| Task mode | `FRONTEND` (backend read-only — endpoint dan aturan validasi `BE-BKC-049` dibaca langsung dari source, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `FE-BKC-028` (kerangka halaman), `BE-BKC-049` ✅ (`PUT /{id}/drug-billing-disposition`, dikonfirmasi ada persis sesuai kontrak beserta seluruh aturan validasinya dibaca dari `BillingPayerEditService.UpdateDrugBillingDispositionAsync`) |
| Status task | **Source selesai.** Panel terpasang pada slot terakhir (`activeMode === INVOICE_EDIT_MODES.DRUG_BILLING`) di `edit-tagihan-view.jsx`. **Sesuai instruksi baku pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — hanya `node --check` pada berkas logika non-JSX (lihat § DoD). Belum di-commit |

## Ringkasan untuk pembaca umum

Panel terakhir pada halaman Edit Tagihan ini menentukan obat mana yang benar-benar dibawa pulang
pasien saat pulang, memakai tiga pilihan: **Ditebus** (semua obat pada resep diambil), **Tebus
Sebagian** (kasir mencentang satu per satu obat mana yang diambil), atau **Tidak Ditebus** (tidak
ada satu pun yang diambil). Panel membuka dengan keadaan yang SUDAH sesuai data tersimpan
sekarang — bukan kosong — supaya kasir langsung melihat status sebenarnya sebelum mengubah apa
pun.

Jumlah tiap obat selalu ditampilkan sebagai teks biasa, tidak pernah bisa diketik ulang — layar
ini murni soal "diambil atau tidak", bukan tempat mengubah resep dokter.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-078`–`080`; `MPY-DEC-009`, `MPY-DES-011`. `frontend-roadmap.md` § `FE-BKC-030`.
`03-frontend-architecture.md` § "Skema fitur — `FE-MPY-04` Edit Billing". Acceptance frontend
`#64`, `#65`.

**Temuan yang wajib dilaporkan:**

1. **Mode awal panel DIAMBIL dari disposisi tersimpan, bukan selalu "Ditebus" kosong.** Diperiksa
   `BillingPayerEditService.UpdateDrugBillingDispositionAsync` (baris 1128–1155): `Disposition`
   per item obat bernilai `"INCLUDED"`/`"EXCLUDED"`, dan `GetEditContextAsync` sudah mengembalikan
   nilai TERKINI tiap item lewat `DrugBillingDisposition`. Panel menyimpulkan mode awal:
   seluruhnya `INCLUDED` → Ditebus; seluruhnya `EXCLUDED` → Tidak Ditebus; campuran → Tebus
   Sebagian dengan kotak yang sesuai sudah tercentang. Kalau panel dibuka selalu kosong/Ditebus
   tanpa memandang data nyata, kasir yang langsung menekan Simpan tanpa menyentuh apa pun bisa
   diam-diam menimpa disposisi manual yang sudah ada sebelumnya.
2. **Baris tabel disumberkan dari `editContext.items` (`BE-BKC-FIX-009`) disilangkan dengan
   `editContext.drugBillingDisposition`**, sama seperti pola `FE-BKC-029`. Baris yang TIDAK
   ditemukan di `drugBillingDisposition` (backend hanya mengisinya untuk item berkategori
   `IsPharmacy`/`SourceDomain=PHARMACY`) dirender sebagai **baris non-obat, hanya-baca tanpa
   kotak centang** — persis sesuai Wilayah "Baris non-obat" pada wireframe.
3. **Baris `VOIDED` TIDAK ditampilkan** di panel ini — berbeda dari `FE-BKC-029` yang eksplisit
   menampilkannya (terkunci). Wireframe `FE-MPY-04` tidak meminta baris dibatalkan tetap terlihat,
   dan backend sendiri (`activeItems` di `UpdateDrugBillingDispositionAsync`, baris 1056)
   mengecualikan `VOIDED` dari domain edit ini sepenuhnya — menampilkannya hanya akan
   membingungkan tanpa kegunaan.
4. **Isian jumlah dirender sebagai teks polos (`{row.quantity}` di dalam `<td>`), bukan `<input>`
   sama sekali** — bukan cuma `readOnly`. Ini memastikan Acceptance `#65` ("tombol tambah/kurang
   jumlah **tidak muncul sama sekali**") terpenuhi secara struktural, bukan sekadar dihindari
   secara kebetulan.
5. **Acceptance `#64` (tombol Edit Billing nonaktif pada RANAP) dan gerbang "tanpa item obat
   layak" TIDAK diimplementasikan ulang di panel ini** — keduanya SUDAH digerbang di level
   kerangka (`FE-BKC-028`) lewat `capabilities.canEditDrugBilling`/`drugBillingBlockReason` yang
   dikirim `edit-context` (dikonfirmasi langsung: `CanEditDrugBilling = isOpen && !hasPaid &&
   !isRanap && eligibleDrugItemIds.Count > 0`, dengan `DrugBillingBlockReason` persis kalimat
   yang diminta wireframe untuk kedua kasus). Panel ini menerima `canEdit`/`blockReason` sebagai
   props dari kerangka yang sama, pola identik dua panel sebelumnya — tidak ada logika gerbang
   baru yang perlu ditulis.
6. **Kotak centang memakai `<input type="checkbox">` mentah**, bukan `BaseCheckboxCard` (satu-
   satunya komponen checkbox di `base-features/`). Diperiksa: `BaseCheckboxCard` adalah kartu
   berlabel/deskripsi/badge yang dirancang untuk toggle pengaturan berdiri sendiri, bukan sel
   tabel padat — memaksakannya ke dalam tiap baris akan menghasilkan tata letak yang rusak.
   Memakai elemen HTML asli bukan "membuat komponen baru" (tidak ada abstraksi baru yang
   diperkenalkan), sehingga tidak melanggar gerbang keputusan komponen.

## Base Component Decision Gate

`UI GATE: 0 elemen NEW, 0 elemen EXTEND, seluruh elemen REUSE (satu elemen HTML native)`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Tombol mode (Ditebus/Tebus Sebagian/Tidak Ditebus) | `REUSE` | `BaseButton`, pola identik toolbar `FE-BKC-028` dan tombol pilihan `FE-BKC-029` |
| Kotak centang per baris | `NATIVE HTML` (bukan komponen) | `<input type="checkbox">` langsung — `BaseCheckboxCard` tidak cocok untuk sel tabel padat (lihat § Keputusan butir 6); tidak ada komponen baru dibuat |
| Tabel baris biaya | `REUSE` | `baseStyles.dataTable`/`tableWrapper`, pola identik `FE-BKC-029` |
| Alert kosong/nonaktif | `REUSE` | `InformationAlert` |
| Isian alasan | `REUSE` | `BaseTextAreaField` |

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}/drug-billing-disposition` | Mengatur disposisi penebusan obat (`ALL_REDEEMED`/`PARTIAL_REDEEMED`/`NOT_REDEEMED`) dan menghitung ulang tagihan atomik | `BillingInvoice : Update` | Body: `Mode`, `IncludedInvoiceItemIds` (hanya diisi untuk `PARTIAL_REDEEMED`), `ExpectedRowVersion`, alasan; header `Idempotency-Key` | `InvoiceEditResultResponse` |

Tidak ada endpoint lain yang ditambahkan — panel ini murni memakai `edit-context` yang sudah
dimuat kerangka `FE-BKC-028`.

Kode status: `404` (tagihan tidak ditemukan), `400` (Idempotency-Key/alasan/`ExpectedRowVersion`
kosong/tidak valid, mode tidak dikenali, `IncludedInvoiceItemIds` kosong pada mode Tebus Sebagian
atau justru diisi pada mode Ditebus/Tidak Ditebus, baris ganda), `409` (tagihan sudah
difinalisasi/dibayar, atau versi data basi), `422` (kunjungan rawat inap, tagihan tanpa item obat
layak, baris bukan obat/tidak layak/sudah dibatalkan — pesan server ditampilkan apa adanya, sudah
ditangkap gerbang `capabilities` di kerangka sebelum panel ini sempat dibuka).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `.../billing-invoices/edit-tagihan/edit-billing-panel.jsx` | **Baru.** Panel `FE-MPY-04` — tombol tiga mode, tabel baris (obat bercentang, non-obat hanya-baca), jumlah sebagai teks polos, alasan, simpan/batal |
| `src/lib/hooks/.../billing-invoices/use-edit-billing-panel.js` | **Baru.** Hook panel — silang `items`×`drugBillingDisposition`, penyimpulan mode awal dari disposisi tersimpan, reset saat `editContext` berganti, `Idempotency-Key`/`CorrelationId` dibangkitkan sekali saat simpan |
| `src/lib/state/slice/.../billing-invoice-slice.jsx` | **Diubah.** Tambah thunk `updateDrugBillingDisposition` + 3 reducer case (pola identik dua thunk edit tagihan lain, memakai `actionLoading`/`actionError` bersama). Thunk/selector lama tidak diubah |
| `src/lib/hooks/.../billing-invoices/billing-invoice-constants.js` | **Diubah.** Tambah `DRUG_BILLING_MODES`, `DRUG_BILLING_MODE_OPTIONS`, `DRUG_BILLING_DISPOSITIONS`. Export lama tidak diubah |
| `.../billing-invoices/edit-tagihan/edit-tagihan-view.jsx` | **Diubah** (milik `FE-BKC-028`/`029`). Slot `activeMode === DRUG_BILLING` (placeholder terakhir) kini merender `EditBillingPanel`; komentar berkas diperbarui karena ketiga panel kini terisi |

Total: **2 berkas baru, 3 berkas diubah** (2 di antaranya milik task pasangan `FE-BKC-028`/`029`).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` / `test:unit` / `build` | **SKIPPED** — instruksi baku pengguna | Tidak dijalankan sesi ini |
| Sintaks berkas logika non-JSX valid | **LULUS** | `node --check` pada `use-edit-billing-panel.js` dan `billing-invoice-slice.jsx` (via salinan `.js` sementara) — keduanya `exit 0` |
| Sintaks berkas JSX baru | **BELUM DIVERIFIKASI ALAT** | Sama seperti dua task sebelumnya — diverifikasi manual lewat pembacaan ulang |
| Kontrol jumlah (tambah/kurang) tidak ada sama sekali (Acceptance `#65`) | **LULUS (tinjauan kode)** | `edit-billing-panel.jsx`: jumlah dirender `{row.quantity ?? "-"}` sebagai teks di dalam `<td>`, tidak ada elemen `<input>`/stepper/tombol +/- di seluruh berkas |
| Tombol Edit Billing nonaktif pada RANAP beserta keterangan (Acceptance `#64`) | **LULUS (tinjauan kode, level kerangka)** | `capabilities.CanEditDrugBilling`/`DrugBillingBlockReason` dari `BE-BKC-047` sudah menghitung ini; kerangka `FE-BKC-028` sudah meneruskannya ke toolbar sejak sebelum task ini — dikonfirmasi tidak ada logika baru yang perlu ditulis panel ini |
| Mode awal mencerminkan disposisi tersimpan | **LULUS (tinjauan kode)** | `deriveInitialState` di hook: seluruhnya `INCLUDED`→`ALL_REDEEMED`, seluruhnya `EXCLUDED`→`NOT_REDEEMED`, campuran→`PARTIAL_REDEEMED` dengan `checkedIds` terisi id yang `INCLUDED` |
| Kotak centang hanya muncul pada baris obat, hanya interaktif pada mode Tebus Sebagian | **LULUS (tinjauan kode)** | Baris `!row.isDrug` merender `"-"` tanpa `<input>` sama sekali; baris obat merender `<input>` dengan `disabled={!isPartialMode \|\| panel.isSaving}` |
| Nol perubahan/alasan kosong/Tebus Sebagian tanpa centang → simpan nonaktif | **LULUS (tinjauan kode)** | `canSave = !submitting && Boolean(reason.trim()) && (mode !== PARTIAL_REDEEMED \|\| checkedIds.size > 0)` — mencerminkan `BIL-VAL-062`/`083` persis |
| Verifikasi manual (klik-coba: ketiga mode, centang sebagian, simpan, cek `422` RANAP, cek panel diganti keterangan saat tanpa item obat layak) | **NOT FEASIBLE (sesi ini)** | Tidak ada environment ter-autentikasi pada sesi ini |

**Task ini belum bisa ditandai selesai.** Source selesai dan ditinjau kode menyeluruh terhadap
validasi backend nyata, tetapi lint/test/build serta klik-coba ter-autentikasi belum dijalankan.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat `npm run lint`/`test:unit`/`build` maupun browser** pada
   sesi ini.
2. **Bergantung pada `FE-BKC-028`/`029` yang juga belum di-build/test** — tiga task kini menumpuk
   perubahan pada berkas yang sama (`edit-tagihan-view.jsx`, `billing-invoice-slice.jsx`,
   `billing-invoice-constants.js`) tanpa satu pun lolos build. Disarankan build/test ketiganya
   sekaligus, bukan satu-satu.
3. **Seluruh rumpun Edit Tagihan (`FE-BKC-028`–`030`) kini source-complete tapi nol
   diverifikasi hidup** — halaman punya tiga mode penuh berfungsi secara kode, tapi belum satu
   klik-coba pun terjadi terhadap tagihan nyata.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint:errors`/`test:unit`/`build` untuk seluruh rumpun
   `FE-BKC-028`–`030` sekaligus (satu halaman, tiga task saling bergantung pada berkas yang sama).
2. Klik-coba manual menyeluruh: ganti penanggung (Edit Asuransi), ubah penanggung per baris (Edit
   Status Tagihan), atur penebusan obat (Edit Billing) — pada kunjungan Tunai, Asuransi, Penjamin
   Perusahaan, dan Rawat Inap, masing-masing.
3. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-028`, `029`, `030`) dan
   `requirement-traceability.md`, tandai ketiganya `✅`, tautkan laporan masing-masing.
4. `FE-BKC-034` (Aksesibilitas, privasi, dan regresi lintas layar) adalah task berikutnya pada
   roadmap yang bergantung pada ketiga task ini — termasuk regresi wajib langkah pembayaran
   admisi Rawat Inap sesudah `BasePayerWorkspace` dipakai ulang `FE-BKC-028`.
