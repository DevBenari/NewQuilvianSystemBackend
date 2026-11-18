# LGS-FE-ADH-01 — Navigasi mandiri Logistik dan layar stok yang tersambung ke API nyata

- TASK ID: `LGS-FE-ADH-01` (task ad-hoc, belum ada roadmap; diizinkan instruksi tertulis pemrakarsa 2026-11-17)
- TASK TYPE: Perubahan navigasi dan integrasi layar existing
- COMPLEXITY: Menengah
- MODEL: Claude Opus 5.5
- TASK MODE: FRONTEND (dinyatakan eksplisit oleh pemrakarsa)
- WRITE TARGET: `QuilvianSystemFrontendDev` branch `Ikbalv2`, HEAD `1e9adc790`
- FILES CHANGED:
  - `src/utils/menu-sidebar/menu-items.jsx` — label bagian `Logistik` (`label-logistics`) baru; blok Logistik dipindah dari bagian Pelayanan Kesehatan ke bagian itu; lima `pathname` baru. Label dan urutan 78 baris label di dalam blok identik dengan sebelum perubahan (dibuktikan dengan diff)
  - `src/lib/services/health-services/pharmacy-management/drug-stock.service.js` — `getLowStock`
  - `src/lib/hooks/health-services/pharmacy-management/use-drug-stock.jsx` — `useLowStock`, `useDrugBatches`; `useStockBalances(initialStatus)`
  - `src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-view.jsx` — tab `Hampir Habis` dan `Daftar Batch`, prop `initialStatus`, kolom, saringan, dan pesan kosong per tab
  - `src/app/health-services/pharmacy-management/drug-stock/low-stock/page.jsx` (baru)
  - `src/app/health-services/pharmacy-management/drug-stock/batches/page.jsx` (baru)
  - `src/app/health-services/pharmacy-management/drug-stock/damaged/page.jsx` (baru)
  - `tests/unit/logistics-module-navigation.test.mjs` (baru)
  - Sejak 13 November, belum di-commit: `summary/page.jsx`, `expiring/page.jsx`
- IMPLEMENTATION:

  | Menu | Route | Sumber data |
  | --- | --- | --- |
  | Dashboard Logistik › Barang Hampir Habis | `/drug-stock/low-stock` | `GET /drug-stock/low-stock` (`LGS-BE-ADH-01`) |
  | Dashboard Logistik › Permintaan Unit | `/stock-requests` | `GET /stock-requests` |
  | Stok & Kartu Persediaan › Batch/Lot | `/drug-stock/batches` | `GET /drug-stock/batches` |
  | Stok & Kartu Persediaan › Riwayat Barang Masuk/Keluar | `/drug-stock/card` | `GET /drug-stock/mutations` |
  | Pengendalian Persediaan › Pemantauan Barang › Barang Rusak | `/drug-stock/damaged` | `GET /drug-stock/balances?status=4` |
  | Penyimpanan & Penempatan Barang › Penelusuran Lokasi Barang (lanjutan 2026-11-17) | `/drug-stock/locations` | `GET /drug-stock/balances?drugId=…`; tidak memanggil backend sebelum obat dipilih |

  Lanjutan 2026-11-17: tab Hampir Habis kini juga menampilkan Reserved dan Available. Berkas
  tambahan: `src/app/health-services/pharmacy-management/drug-stock/locations/page.jsx`.

  Lanjutan kedua 2026-11-17:
  - `use-drug-stock.jsx` — `useStockList` menerima `enabled`; enam hook layar stok hanya memanggil
    backend saat tabnya tampil (sebelumnya lima daftar dimuat sekaligus saat halaman dibuka).
  - `drug-stock-view.jsx` — setiap hook diberi `tab === "<kunci>"`.
  - `stock-request-detail-view.jsx` — kolom **Kekurangan** = diminta − diserahkan, hanya setelah
    penyerahan tercatat; memakai field `requestedQuantity`/`fulfilledQuantity` yang sudah ada dan
    tidak mengubah aturan "penyerahan sebagian menutup permintaan".

  **Audit statis halaman tertaut (18 route unik):** seluruh halaman Farmasi/Logistik memiliki
  loading, galat dengan tombol coba lagi, keadaan kosong, dan gerbang 403. Halaman Master Data
  (obat, kategori, konversi, lokasi, kebijakan stok) dan Finance (penerimaan barang, retur ke
  penyedia) memiliki loading, pesan galat, dan gerbang 403 tanpa tombol coba lagi khusus;
  halaman retur ke penyedia tidak memiliki teks kosong sendiri. Halaman tersebut milik modul lain
  dan tidak diubah. Seluruh endpoint yang dipakai dilindungi `[Authorize]` dan
  `AccessPermission(<resource>, "Read")` di backend; dua tautan Finance juga memakai
  `requiredPermission` yang sama di menu.

  Semua layar memakai `DataTable`, `DataFilter`, paginasi backend, serta keadaan loading, kosong, galat, dan coba lagi yang sudah ada pada `DrugStockView`. Tidak ada data dummy.
- API CONTRACT IMPACT: memakai satu endpoint baru (`LGS-BE-ADH-01`) dan endpoint existing tanpa perubahan
- SECURITY IMPACT: hak akses ditegakkan backend (`DrugStock:Read`, `StockRequest:Read`); tidak ada properti izin baru pada menu, mengikuti item Farmasi yang ditautkan
- VISUAL REFERENCE: NOT REQUIRED — memakai komponen dan gaya layar Stok yang sudah ada
- VALIDATION:
  - `npx eslint` delapan file berubah | PASS, 0 error | static | satu warning `react-hooks/set-state-in-effect` pada `useStockList` baris 55 — EXISTING WARNING, pola sebelum perubahan
  - `node --test` 48 file unit test terkait (570 test) | 23 gagal, sama persis dengan baseline integration | static | 0 kegagalan baru
  - `tests/unit/logistics-module-navigation.test.mjs` | PASS 5/5 | static | menguji bagian navigasi mandiri, 11 grup, keberadaan halaman setiap tautan, pemakaian endpoint nyata, dan prop route
  - `npm run build`, `npm run dev` | NOT RUN | dilarang oleh instruksi pemrakarsa
- KNOWN ISSUES: (1) Beberapa halaman kini dirujuk dari menu Farmasi dan menu Logistik sekaligus; penanda menu aktif memilih salah satunya. (2) Judul layar Stok masih "Modul Farmasi / Stok Farmasi" karena layarnya milik Farmasi.
- MANUAL TEST: NOT FEASIBLE — dev server dilarang pada sesi ini
- INCIDENTAL CHANGES: NONE
- GIT STATUS: file di atas `M`/untracked di `Ikbalv2`; `test-results/.last-run.json` berubah dari sebelumnya dan bukan bagian task; belum di-commit
- NEXT RECOMMENDED STEP: jalankan aplikasi dan periksa kelima layar di browser, termasuk saringan, reset, paginasi, dan tampilan baris `Satuan kebijakan berbeda`
