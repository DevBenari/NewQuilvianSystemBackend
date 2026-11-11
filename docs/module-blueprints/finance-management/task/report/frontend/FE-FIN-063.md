# Laporan Perubahan Frontend — `FE-FIN-063`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-063` |
| Judul | Bila petugas mencoba mencatat, mengubah, atau membatalkan GR untuk dokumen BLM, layar menampilkan pesan backend apa adanya |
| Slice | `REV-24` (Penampung Data PO/GR dari BLM) — paket rilis `MVP-19` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian REV-24 Frontend, tabel Task |
| Trace | `FR-FIN-283`; `FIN-DEC-265`; `FIN-DES-141`; `FIN-VAL-315`; `03-frontend-architecture.md` §27.1 baris kedua |
| Contract version | `FIN-API-1.13` §W.1 (respons 409) — approved 9 November 2026 (Revisi 23 putaran 2) |
| Wewenang UI | Tidak ada perubahan tampilan. Pesan baru hanya berasal dari backend; tombol **tidak** disembunyikan (slice penguncian berikutnya) |
| Dependency | `BE-FIN-146` 🟡 — jalur 409 terbukti pada data uji BLM (W.T13, 11 November 2026). Penahan 🟡 pada task backend itu hanya kriteria 2 (status PO BLM belum punya pemanggil) dan tidak memengaruhi task ini |
| Klasifikasi | `LIGHT` — verifikasi penanganan galat yang ada; tanpa perubahan source aplikasi |
| Task mode | `FRONTEND MODE` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `yasmina`) — hanya satu berkas test baru; di repository backend hanya laporan ini dan tanda status roadmap |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ef8be65849c2e95869801f69cfafaa530ca74542` (perubahan task ini belum di-commit) |
| Commit backend yang dijadikan rujukan | `98a9140ca81799d747821a4ee3695846b46f96e3` (perubahan `BE-FIN-142`..`147` masih di working tree backend) |
| Tanggal | 10 November 2026 |
| Status | ✅ **Selesai dan terverifikasi oleh pemilik (11 November 2026).** Verifikasi-saja: source aplikasi tidak perlu diubah, perilaku dikunci dengan 9 unit test (PASS). Pemilik mencoba Catat, Koreksi, dan Batalkan pada `UJI-BLM-W05` di server lokal dan menyatakan hasilnya cocok dengan panduan uji (pernyataan pemilik di server lokal, 11 November 2026, tanpa tangkapan layar) |

---

## 1. Keadaan yang Ditemukan di Awal

Roadmap menyebut task ini boleh berupa verifikasi saja bila penanganan galat sudah generik. Hasil penelusuran:

| Alur | Tempat di kode | Cara galat ditampilkan |
| --- | --- | --- |
| Catat GR baru (`POST …/goods-receipts`) | `use-purchase-order-detail.jsx`, `handleSubmitGoodsReceipt` | `setGoodsReceiptError(getApiErrorMessage(error, "Gagal mencatat Tanda Terima Barang."))` → kotak merah di dalam dialog (`alert alert-danger`, `purchase-order-detail-view.jsx:423`) |
| Koreksi GR (`PUT …/goods-receipts/{id}`) | sama, cabang `editingGoodsReceipt` | `getApiErrorMessage(error, "Gagal mengoreksi Tanda Terima Barang.")` → kotak merah yang sama |
| Batalkan GR (`POST …/goods-receipts/{id}/cancel`) | `handleCancelGoodsReceipt` | `addToast("error", getApiErrorMessage(error, "Tanda Terima Barang tidak dapat dibatalkan."))` → toast (`ToastStack` menampilkan `toast.message` apa adanya, `toast-stack.jsx:74-76`) |

`getApiErrorMessage` (`purchasing-display-utils.jsx`) mengembalikan `error.response.data.message`, lalu `error.message`, lalu teks cadangan. Interceptor respons Axios (`InstanceAxios.jsx:1278-1334`) hanya bertindak untuk 401/403; galat 409 diteruskan tanpa diubah (`return Promise.reject(error)`).

Backend (baca-saja): `PurchasingExternalDocumentException` dilempar di `FinanceGoodsReceiptService.cs` pada pencatatan (baris 103), pembatalan (209), dan koreksi (321) bila PO/GR bersumber BLM, lalu dipetakan controller menjadi 409 dengan `message` persis kontrak dan `errors.code = "PURCHASING_EXTERNAL_DOCUMENT"`. Pada pembatalan, pemeriksaan ini dilakukan sebelum RowVersion (komentar di source), sehingga tab yang basi tidak menutupi penyebab sebenarnya.

**Kesimpulan:** pesan backend sudah tampil apa adanya pada ketiga alur. Tidak ada celah yang perlu ditambal, jadi **tidak ada source aplikasi yang diubah**.

---

## 2. Proses Bisnis dari Sisi Pengguna

Pengguna: petugas Finance/Purchasing pada halaman detail PO.

1. Petugas membuka detail sebuah PO yang berasal dari BLM. Tombol "Catat Penerimaan Barang", "Koreksi", dan "Batalkan" tetap tampil seperti biasa (penyembunyian adalah slice berikutnya).
2. **Mencatat:** petugas menekan "Catat Penerimaan Barang", mengisi baris, lalu menyimpan. Dialog tetap terbuka dan menampilkan kotak merah: *"Penerimaan barang untuk PO dari BLM dicatat di BLM. Perubahan dan pembatalannya juga dilakukan dari BLM."* Tidak ada data yang tersimpan.
3. **Mengoreksi:** petugas menekan "Koreksi" pada sebuah GR BLM, mengubah isian, lalu menyimpan. Pesan yang sama muncul di kotak merah dialog.
4. **Membatalkan:** petugas menekan "Batalkan". Pesan yang sama muncul sebagai toast galat; GR tidak berubah.
5. Galat lain (bentrok versi, kuantitas melebihi sisa, jaringan, dan sebagainya) tampil seperti sebelumnya, dengan pesan masing-masing.

Jalur tidak normal: bila respons tidak memuat `message`, tampil pesan galat Axios; bila tidak ada respons sama sekali, tampil teks cadangan masing-masing aksi — keduanya perilaku lama, tidak diubah.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: `contracts/api-contract.md` §W.1; `03-frontend-architecture.md` §27.1; roadmap `02-frontend-roadmap.md` (kartu `FE-FIN-063`).
- Frontend: `use-purchase-order-detail.jsx`, `purchase-order-detail-view.jsx`, `purchasing-display-utils.jsx`, `InstanceAxios.jsx` (interceptor), `toast-stack.jsx`.
- Backend (baca-saja): `FinanceGoodsReceiptService.cs`, `PurchasingExceptions.cs`, `FinanceGoodsReceiptsController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/finance-purchasing-external-document-error.test.mjs` | **Baru** — 9 unit test pengunci: pesan 409 apa adanya untuk tiga aksi, galat lain tidak berubah, dan ketiga catch memakai `getApiErrorMessage` tanpa pemetaan khusus |

Tidak ada berkas di `src/` yang berubah.

### 3.3 Kepatuhan arsitektur frontend

Tidak ada pola baru. Test mengikuti pola `tests/unit/*.test.mjs` (`node:test` + `node:assert/strict`). Dua test membaca teks source (hook, view, `ToastStack`) untuk mengunci bahwa pesan tidak dipetakan ulang; pola pembacaan source ini sudah dipakai test lain di repo.

**UI GATE:** `N/A` — tidak ada JSX atau CSS yang dibuat atau diubah.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol dinonaktifkan selama proses (`actionLoading` / `goodsReceiptSubmitting`) — tidak diubah |
| Kosong | `NOT APPLICABLE` — tidak ada daftar baru |
| Gagal (409 dokumen BLM) | Pesan backend persis; di dialog (catat/koreksi) atau toast (batal) |
| Gagal (galat lain) | Pesan backend masing-masing atau teks cadangan — tidak diubah |
| Tanpa hak akses | 403 ditangani interceptor yang sudah ada (`notifyAccessDenied`) — tidak diubah |

---

## 5. Endpoint yang Dikonsumsi

Tidak ada endpoint baru. Ketiga endpoint di bawah sudah dipakai sebelumnya; yang berubah hanya kemungkinan respons 409.

#### Corporate / Finance Management / Purchasing / Goods Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/finance-management/purchasing/goods-receipts` | Mencatat GR dari detail PO | Tidak berubah |
| `PUT` | `/api/v1/corporate/finance-management/purchasing/goods-receipts/{id}` | Mengoreksi GR | Tidak berubah |
| `POST` | `/api/v1/corporate/finance-management/purchasing/goods-receipts/{id}/cancel` | Membatalkan GR | Tidak berubah |

Bentuk 409 yang benar-benar dihasilkan backend: `{"success":false,"statusCode":409,"message":"…","data":null,"errors":{"code":"PURCHASING_EXTERNAL_DOCUMENT"},…}`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran penanganan galat (tiga alur, interceptor, `ToastStack`, backend) | Pesan backend diteruskan apa adanya | `PASS` | Bagian 1 |
| `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-external-document-error.test.mjs` | 9 test, 9 lulus | `PASS` | Keluaran perintah |
| `npx eslint --max-warnings 0 tests/unit/finance-purchasing-external-document-error.test.mjs` | exit 0 | `PASS` | Keluaran perintah |
| Seluruh `tests/unit/` | 4646 test, 4604 lulus, 42 gagal; daftar nama yang gagal identik dengan sebelum task ini (modul lain, mis. billing, IGD, RWI, HMD) | `EXISTING / ENVIRONMENT ISSUE` (0 baru) | Keluaran perintah; `diff` daftar nama |
| `npx eslint --max-warnings 0` pada `use-purchase-order-detail.jsx` dan `purchase-order-detail-view.jsx` (tidak diubah task ini) | 1 peringatan `react-hooks/set-state-in-effect` di `use-purchase-order-detail.jsx:129` (`fetchDetail()` dalam `useEffect`); 0 galat | `EXISTING / ENVIRONMENT ISSUE` | Berkas tidak termodifikasi menurut `git status`; tidak diperbaiki karena di luar cakupan |
| `npm run dev` / `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik |
| 409 nyata dari backend pada PO BLM (W.T13) | Pemilik mencoba Catat Penerimaan Barang, Koreksi, dan Batalkan pada `UJI-BLM-W05` (data uji dari skrip pemilik); pesan backend tampil seperti yang dijanjikan panduan uji: "Penerimaan barang untuk PO dari BLM dicatat di BLM. Perubahan dan pembatalannya juga dilakukan dari BLM." Hasil pemeriksaan ulang skrip 2 sesudahnya tidak dilampirkan terpisah | `PASS` (pengamatan pemilik) | Pernyataan pemilik di server lokal, 11 November 2026, tanpa tangkapan layar |

Uji manual oleh agen: `NOT FEASIBLE` — tidak ada dev server/browser pada sesi agen. Uji manual oleh pemilik: `PASS` — kotak merah pada dialog dan toast diamati di browser pada server lokal (lihat baris di atas).

AUTOMATED TEST: `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/` — 9 test baru PASS; 42 kegagalan lama di modul lain tidak berubah.

**Tidak dijalankan:** `npm run build`, `npm run dev`, e2e, UAT — di luar instruksi.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Galat 409 `PURCHASING_EXTERNAL_DOCUMENT` menampilkan `message` backend | Terpenuhi (source, unit test) | Bagian 1; tiga test pertama; test "hook detail PO…" dan "tampilan meneruskan teks galat…" |
| (2) Tidak ada perubahan untuk galat lain | Terpenuhi | Tidak ada berkas `src/` yang berubah; empat test galat lain (bentrok versi, 422/400, tanpa respons, tanpa `message`) |
| (3) Tombol **tidak** disembunyikan di task ini | Terpenuhi | Tidak ada referensi `sourceSystem` di hook maupun view detail PO; kondisi tombol (`canRecordGoodsReceipt`, `canCorrectGoodsReceipt`, `gr.status !== "CANCELLED"`) tidak berubah |
| DoD: penelusuran penanganan galat dan ESLint | Terpenuhi | Bagian 1 dan 6 |
| DoD: laporan `task/report/frontend/FE-FIN-063.md` | Terpenuhi | Berkas ini |
| DoD: terbukti pada respons backend yang sebenarnya | Terpenuhi | Pengamatan pemilik di browser pada data uji BLM (W.T13), 11 November 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Contoh respons di `api-contract.md` §W.1 menaruh `code` di tingkat atas (`"code": "PURCHASING_EXTERNAL_DOCUMENT"`), sedangkan `ApiResponse` backend tidak punya properti `code` dan mengirimnya di `errors.code`. Layar ini hanya membaca `message`, jadi tidak terpengaruh. **Diputuskan pemilik 10 November 2026 (`FIN-DEC-292`)**: contoh kontrak dikoreksi menjadi `errors.code`, backend tidak diubah. Slice berikutnya yang ingin bercabang berdasarkan kode membaca `errors.code` |
| Masalah yang diketahui | Peringatan ESLint lama di `use-purchase-order-detail.jsx:129`, tidak diperbaiki (di luar cakupan). Dua test membaca teks source sehingga akan gagal bila slice berikutnya mengubah bentuk penanganan galat itu; kegagalan itu disengaja sebagai tanda untuk meninjau ulang |
| Dependency backend | `BE-FIN-146` 🟡 — 409 terbukti pada data uji (W.T13); yang menahan 🟡 hanya kriteria 2 pada task backend itu. Bila backend mengubah teks pesan, layar akan mengikuti otomatis |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | FE: `M purchasing-document-trail-panel.jsx`, `M purchasing-trail-constants.jsx`, `M purchasing-trail-utils.jsx` (dari `FE-FIN-062`); `?? tests/unit/finance-purchasing-blm-item-progress.test.mjs` (`FE-FIN-062`); `?? tests/unit/finance-purchasing-external-document-error.test.mjs` (task ini). Tidak ada stage/commit |
| Langkah berikutnya | Tidak ada untuk task ini. Slice berikutnya (di luar task ini): menyembunyikan tombol buat/ubah/setujui untuk dokumen BLM (`FIN-OQ-138`) |
