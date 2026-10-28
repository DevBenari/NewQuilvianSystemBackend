# Laporan Perubahan Frontend — `FE-MCU-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-015` |
| Judul | Label *Dicakup paket MCU* pada detail pesanan Lab/Rad |
| Slice | `MVP-2` (pasangan `BE-MCU-020`) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-015`* |
| Trace | `FR-MCU-032`; `MCU-DEC-022`; `03-frontend-architecture.md` §6 (titik sentuh modul lain) |
| Contract version | `MCU-API-v1` *Perubahan internal* (Lab, Rad — ruas baca `isCoveredByMcuPackage`, `coveringMcuOrderNumber` pada `LabOrderDetailResponse` dan `RadOrderDetailResponse`) — `approved` 2026-10-21 |
| Wewenang UI | Roadmap: *"Reuse halaman detail pesanan Lab/Rad yang ada; letak label `DEV_DISCRETION` di dalam halaman itu"*. Komponen `StatusBadge` yang ada |
| Dependency | `BE-MCU-020` ✅ |
| Klasifikasi | `LOW`–`MEDIUM` — dua layar modul lain disentuh secara aditif, satu aturan murni baru, tanpa perilaku baru, tanpa database |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: aturan bersama, dua view (Lab, Rad), uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `1b7ff563` (branch `yoga`) |
| Tanggal | 2026-10-28 |
| Status | Selesai — kriteria terbukti di uji unit dan E2E API-tiruan. Nama ruas dicocokkan dengan respons backend lokal yang sungguhan |

---

## 1. Keadaan yang ditemukan di awal

- Backend (`BE-MCU-020`) mengisi `IsCoveredByMcuPackage` dan `CoveringMcuOrderNumber` **hanya** pada respons detail.
  - Lab: `GET lab-orders/{id}`, proyeksi `GetDetailAsync`.
  - Rad: `GET rad-orders/{id}`, `MapDetail` + nomor order MCU.
  - Respons daftar pesanan tidak memuat kedua ruas.
- Halaman detail pesanan:
  - **Lab** — `…/laboratory-management/lab-orders/[slug]` (`lab-order-detail-view.jsx`), panel *Informasi Pesanan*
    dengan lencana status di kepala panel.
  - **Rad** — `…/radiology-management/rad-patients/[radOrderId]` (`rad-patient-detail-view.jsx`). Satu layar memuat
    **satu booking**: pesanan yang dibuka (respons detail) ditambah pesanan saudara di kunjungan dan tanggal yang sama
    (respons daftar). Bagian *Informasi Order* menumpuk nomor setiap pesanan.
- Tidak ada spec E2E untuk kedua halaman detail.

---

## 2. Perilaku dari sisi pengguna

- **Lab**: pesanan yang dicakup paket MCU menampilkan lencana *Dicakup paket MCU · MCU-RSMMC-000123* di samping lencana
  status pada kepala panel *Informasi Pesanan*.
- **Rad**: lencana yang sama tampil di bawah nomor pesanan yang dibuka pada *Informasi Order › No. Order*. Pesanan
  saudara tidak berlabel karena data daftarnya tidak membawa ruas cakupan (lihat §7).
- **Pesanan biasa**, baik ruas `false` maupun ruas tidak ada, tidak berlabel di kedua halaman.
- Label hanya berupa informasi. Tidak ada perubahan aksi, tarif, atau alur di layar Lab/Rad.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/medical-checkup-management/mcu-coverage-rules.js` (baru) | `describeMcuCoverage(order)` → `{ label, orderNumber, text }` atau `null`. Hanya `true` boolean yang dihitung bertanda; camelCase dan PascalCase dibaca (pola `readValue` layar Lab/Rad) |
| `src/components/view/health-services/laboratory-management/lab-orders/detail/lab-order-detail-view.jsx` | Lencana cakupan di kepala panel *Informasi Pesanan*; kedua lencana dibungkus `d-flex flex-wrap gap-2` |
| `src/components/view/health-services/radiology-management/rad-patients/rad-patient-detail-view.jsx` | Lencana cakupan pada baris nomor pesanan yang dibuka (`statusLine` yang sudah ada) |
| `tests/unit/mcu-coverage-rules.test.mjs` (baru) | 2 uji |
| `tests/e2e/mcu-coverage-label.spec.mjs` (baru) | 2 skenario API-tiruan (Lab, Rad) |

Aditif murni pada layar Lab/Rad (Lab +19/−7 baris, Rad +13/−1). Tidak ada panggilan API, state, komponen, atau CSS baru.

---

## 4. State yang ditangani

| State | Perilaku |
| --- | --- |
| Bertanda | Lencana dengan nomor order MCU; tanpa nomor (tidak terbaca) → *Dicakup paket MCU* saja |
| Tidak bertanda / ruas tidak ada | Tanpa lencana |
| Memuat, gagal, tanpa hak | Tidak berubah — mengikuti perilaku halaman Lab/Rad yang ada |
| Aksesibilitas | Lencana berupa teks yang terbaca pembaca layar |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-coverage-rules.test.mjs` | 2/2 lulus. Cakupan: bertanda + nomor (dipangkas), tanpa nomor, PascalCase; `false`, ruas tidak ada, `"true"` teks, dan `null` → tanpa label | `PASS` | `# pass 2` |
| E2E Lab | Bertanda → panel *Informasi Pesanan* memuat *"Dicakup paket MCU · MCU-RSMMC-000123"*. Muat ulang sebagai pesanan biasa → nol teks *Dicakup paket MCU*. Nol non-GET | `PASS` (tiruan) | Spec skenario 1, `lab-bertanda.png` |
| E2E Rad | Bertanda → label tepat satu, menempel pada `RAD-RSMMC-000045` (dibuka). Saudara `RAD-RSMMC-000046` dari daftar tanpa label. Pesanan biasa → nol label. Nol non-GET | `PASS` (tiruan) | Spec skenario 2, `rad-bertanda.png` |
| Backend lokal + devYoga (baca-saja) | Detail `LAB-RSMMC-000030` dan `RAD-RSMMC-000003` memuat `{"isCoveredByMcuPackage":false,"coveringMcuOrderNumber":null}`, sama persis dengan nama yang dibaca layar. Tidak ada pesanan bertanda di 30 pesanan Lab dan 3 pesanan Rad (belum ada pemeriksaan MCU yang dimulai) | `PASS` | Keluaran `uat-015.mjs` (scratchpad sesi `cbe51345`) |
| `npx eslint` (dua view, aturan, uji) | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.932 uji: 3.890 lulus, 42 gagal; daftar nama **identik** baseline. Uji Lab/Rad yang ada tidak terdampak | `EXISTING ISSUE` | Keluaran + `diff` |
| `npm run build` | Exit 0 | `PASS` | Keluaran build |

`MANUAL TEST`: kedua halaman dibuka di build standalone lewat Playwright, dan letak lencana diperiksa dari tangkapan
layar. Label pada pesanan bertanda **sungguhan** belum terlihat karena devYoga belum punya pesanan Lab/Rad dari
pemeriksaan MCU. Tampilan dengan data sungguhan ikut terbukti begitu `UAT-10` dijalankan dengan data sungguhan (batas
verifikasi `FE-MCU-011`).

`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU, identik baseline; uji MCU PASS)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pesanan bertanda menampilkan label | Terpenuhi (Lab dan Rad, E2E) | §5 |
| Pesanan biasa tidak | Terpenuhi (Lab dan Rad, E2E + unit; ruas `false` sungguhan di backend lokal) | §5 |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Batas kontrak (Rad) | Layar detail Rad menampilkan beberapa pesanan satu booking. Hanya pesanan yang dibuka yang membawa ruas cakupan, jadi pesanan saudara yang juga dicakup paket MCU **tidak** berlabel. Menandai semuanya butuh ruas yang sama di respons daftar pesanan Rad — penambahan kontrak di backend Radiologi, bila diinginkan |
| Tampilan | `StatusBadge` mengkapitalkan teks lewat CSS, sehingga di layar tampil *Dicakup Paket MCU*. Sama dengan lencana lain di kedua modul |
| Pemilik layar | Kedua view milik modul Lab dan Rad (roadmap: *Pemilik: Lab, Rad*). Perubahan aditif dan tanpa perilaku baru; pemilik modul sebaiknya diberi tahu |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE`. Output Playwright ke scratchpad; server dimatikan, port bebas |
| Status Git (frontend) | `M` dua view Lab/Rad; `??` aturan baru dan dua berkas uji |
| Langkah berikutnya | Seluruh task roadmap frontend MCU selesai. Tersisa penutupan batas verifikasi dengan data sungguhan (`UAT-10`..`19`) dan audit kesiapan modul (`verify-module-readiness`) |
