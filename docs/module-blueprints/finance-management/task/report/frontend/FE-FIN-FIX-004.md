# Laporan Perubahan Frontend — `FE-FIN-FIX-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-FIX-004` |
| Judul | Angka penerimaan GR bersumber BLM pada detail PO tampil dalam satuan dasar beserta satuannya, bukan angka satuan-PO tanpa satuan |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap. Mengikuti pola `FIX` yang dipakai `FE-FIN-FIX-001`, `002`, dan `003` |
| Roadmap | Tidak ada baris roadmap resmi. Permintaan langsung pemilik, 10 November 2026 ("perbaiki saja"), menjawab pengamatan agen pada laporan layar `UJI-BLM-W07` |
| Trace | `FIN-DES-139`, `FIN-DES-140` (kolom lama `ReceivedQuantity` pada baris BLM hanya angka tampilan satuan PO, dibulatkan 2 desimal); `contracts/api-contract.md` W.1 dan W.2; `FE-FIN-062` (panel kemajuan satuan dasar) |
| Contract version | **Tidak ada perubahan kontrak.** Field yang dipakai sudah dikirim backend sejak `BE-FIN-147`: `baseUomCode`, `baseQuantity`, `conditionCode` pada baris GR |
| Dependency | Tidak ada tambahan. Backend tidak diubah |
| Wewenang UI | Hanya isi teks pada sel dan subjudul yang sudah ada. Nol komponen baru, nol kolom baru, nol CSS baru |
| Klasifikasi | `LIGHT` — 2 view disentuh pada isi teks, 1 konstanta, 1 utilitas, 1 test ditambah |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `yasmina`); di repository backend hanya laporan ini |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ef8be6584` (perubahan task ini belum di-commit) |
| Tanggal | 10 November 2026 |
| Status | ✅ **Selesai dan terverifikasi di layar oleh pemilik (11 November 2026).** 13 unit test baru PASS, ESLint PASS, render panel diperiksa. Pemilik menjalankan aplikasi di server lokal dan menyatakan hasilnya cocok dengan tabel di §6 (pernyataan pemilik di server lokal, 11 November 2026, tanpa tangkapan layar). `npm run dev`/`build` oleh agen tidak dijalankan |

---

## 1. Masalah yang diperbaiki

Pada halaman detail PO bersumber BLM, dua tempat menampilkan angka penerimaan GR dari kolom lama `receivedQuantity`. Untuk dokumen BLM kolom itu hanya angka tampilan dalam satuan PO yang dibulatkan dua desimal (`FIN-DES-140`), dan tampil **tanpa satuan**, berdampingan dengan panel kemajuan yang sudah benar dalam satuan dasar.

| Tempat | Sebelum (data uji `UJI-BLM-W05`: 6 PCS dari PO 1 BOX isi 12) | Sebab |
| --- | --- | --- |
| Subjudul baris "Penerimaan Barang" pada panel jejak dokumen | `Diterima 10 November 2026 • 1 baris • total qty 0.5` | Ringkasan jejak hanya membawa `totalReceivedQuantity` (kolom lama). Ringkasan itu tidak membawa angka satuan dasar maupun sumber dokumen |
| Sel "Qty Diterima" pada rincian GR (Lihat Rincian) | `0.5` | Sel memakai `receivedQuantity` mentah, padahal respons rincian GR sudah membawa `baseQuantity` dan `baseUomCode` |

Dengan angka `0.5` di sebelah "6 PCS" petugas bisa mengira ada dua penerimaan berbeda atau salah catat.

---

## 2. Proses bisnis dari sisi pengguna

Petugas membuka detail PO dari BLM lalu melihat riwayat penerimaan.

- **Baris GR di panel jejak** kini hanya menyebut tanggal dan jumlah baris. Jumlah barang yang diterima dibaca dari tabel "Kemajuan Penerimaan per Baris" di bawahnya, yang memakai satuan dasar.
- **Rincian GR** (tombol "Lihat Rincian") menampilkan jumlah fisik yang diterima per baris beserta satuannya, misalnya `4 PCS`. Baris yang belum sah diberi keterangan kecil di bawah angka: "Menunggu periksa" atau "Rusak". Baris yang baik tidak diberi keterangan.
- **PO lama dan GR lama tidak berubah sama sekali**: angka lama tetap tampil seperti sebelumnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`purchase-order-detail-view.jsx` (rincian GR, baris 286–370), `purchasing-document-trail-panel.jsx` (grup Penerimaan Barang), `use-purchase-order-detail.jsx` (rincian GR disimpan mentah dari respons, jadi field BLM sampai ke tampilan), `FinanceGoodsReceiptDtos.cs` dan `FinancePurchasingDocumentTrailDtos.cs` (bentuk respons), `goods-receipt-constants.jsx`, `goods-receipt-view.jsx` dan `goods-receipt-table-columns.jsx` (tidak menampilkan kuantitas, jadi tidak perlu disentuh).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/purchasing/goods-receipt-constants.jsx` | Ditambah `GOODS_RECEIPT_CONDITION_CODES` (GOOD, DAMAGED, PENDING_INSPECTION) dan `GOODS_RECEIPT_CONDITION_LABELS` ("Rusak", "Menunggu periksa"), kata yang sama dengan kolom panel kemajuan |
| `src/utils/finance/purchasing/purchasing-trail-utils.jsx` | Ditambah dua fungsi murni: `buildGoodsReceiptItemQuantityDisplay(grItem)` dan `buildTrailGoodsReceiptSubtitle({...})`. Memakai `formatBaseQuantity` yang sudah ada dari `FE-FIN-062`. Tidak ada penjumlahan, pembagian, atau konversi satuan di klien |
| `src/components/view/finance/purchasing/shared/purchasing-document-trail-panel.jsx` | Subjudul baris GR kini dibentuk oleh `buildTrailGoodsReceiptSubtitle`, dengan `isBlmProgress` yang sudah ada di komponen |
| `src/components/view/finance/purchasing/purchase-order/detail/purchase-order-detail-view.jsx` | Sel "Qty Diterima" memakai `buildGoodsReceiptItemQuantityDisplay`; keterangan kondisi memakai kelas `small text-muted` yang sudah dipakai berkas yang sama |
| `tests/unit/finance-purchasing-gr-base-quantity-display.test.mjs` | **Baru** — 13 unit test |

### 3.3 Aturan tampilan

| Kasus | Tampilan |
| --- | --- |
| Baris GR yang membawa `baseUomCode` (GR BLM), kondisi `GOOD` | `{baseQuantity} {baseUomCode}`, mis. `4 PCS`, tanpa keterangan |
| Kondisi `PENDING_INSPECTION` atau `DAMAGED` | Jumlah fisik yang sama, ditambah keterangan "Menunggu periksa" atau "Rusak" |
| Kode kondisi yang belum dikenal (mis. `REJECTED`) | Kode ditampilkan apa adanya, tidak disembunyikan |
| `baseQuantity` kosong pada baris BLM | Tanda hubung `—`, bukan nol |
| Baris GR lama (tanpa `baseUomCode`) | `receivedQuantity` apa adanya, tanpa keterangan |
| Subjudul jejak, PO bersumber BLM | `Diterima {tanggal} • {n} baris` |
| Subjudul jejak, PO lama | Sama persis dengan sebelumnya: `Diterima {tanggal} • {n} baris • total qty {angka}` |

Format angka: lokal `id-ID`, maksimal 6 desimal, tanpa pembulatan tambahan di klien (sama dengan `FE-FIN-062`).

**Sengaja tidak diubah:**

| Hal | Alasan |
| --- | --- |
| Backend dan DTO ringkasan jejak | Menambah angka satuan dasar ke baris jejak adalah permintaan BE terpisah. Untuk saat ini fragmen "total qty" cukup dibuang pada PO BLM karena angka benar sudah ada di panel kemajuan |
| Tombol Catat, Koreksi, dan Batalkan pada PO BLM | Tetap tampil; menyembunyikannya adalah irisan berikutnya (kriteria 3 `FE-FIN-063`) |
| Halaman daftar dan detail GR | Tidak menampilkan kuantitas, jadi tidak terdampak |
| Satuan asli penerimaan (`sourceQuantity`, `sourceUomCode`) | Tidak ditampilkan. Mis. GR `UJI-GR-W09-1` (1 BOX isi 12) tampil `12 PCS`, sama dengan satuan yang dipakai panel kemajuan |

### 3.4 Kepatuhan arsitektur frontend

Konstanta di `src/lib/constants/finance/purchasing/`, fungsi murni di `src/utils/finance/purchasing/`, view hanya memanggil fungsi itu, test di `tests/unit/`. Tidak ada pola baru dan tidak ada nilai visual literal. Impor mengikuti arah dependensi yang berlaku (view → utils → constants); tidak ada impor siklik.

**UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Sel kuantitas pada tabel rincian GR | Markup `<td>` dan tabel `data-flat-table="true"` yang sudah ada di `purchase-order-detail-view.jsx` | Hanya isi sel yang berubah; keterangan kondisi memakai kelas `small text-muted` yang sudah dipakai berkas yang sama (baris "Tanggal: …") | REUSE | Tidak ada komponen baru |
| Subjudul baris Penerimaan Barang | `TrailRow` yang sudah ada | Hanya string `subtitle` yang berubah | REUSE | Tidak ada komponen baru |

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Rincian GR belum dimuat atau sedang dimuat | Tidak berubah (tombol "Lihat Rincian" memuat rincian sesuai kebutuhan) |
| Respons tanpa field BLM (GR lama atau GR buatan Quilvian) | Tampilan lama, angka `receivedQuantity` apa adanya |
| Akun tanpa hak membaca bagian Penerimaan Barang pada jejak | Tidak berubah (bagian dinyatakan terbatas) |
| Field BLM sebagian kosong | `baseQuantity` kosong menjadi `—`; kode kondisi kosong dianggap baik |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada endpoint baru dan tidak ada perubahan pemanggilan API. Field yang dibaca berasal dari `GET …/goods-receipts/{id}` dan `GET …/purchase-orders/{id}/document-trail` yang sudah ada.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-gr-base-quantity-display.test.mjs` bersama tiga berkas test BLM terkait (`finance-purchasing-blm-item-progress`, `…-approval-tier-label`, `…-external-document-error`) | 41 test, 41 lulus (13 baru + 28 yang sudah ada) | `PASS` | Keluaran perintah |
| `npx eslint --max-warnings 0` pada lima berkas yang diubah ditambah berkas test | exit 0 | `PASS` | Keluaran perintah |
| Seluruh `tests/unit/` | 4664 test, 4622 lulus, 42 gagal; daftar nama yang gagal **identik** dengan sebelum task ini (`diff` kosong). Sebelum task: 4651 test, 4609 lulus, 42 gagal | `EXISTING / ENVIRONMENT ISSUE` (0 baru) | Keluaran perintah; `diff` daftar nama |
| Render sisi server panel jejak (`react-dom/server`) dengan data PO BLM, PO lama dengan `itemProgress`, dan PO lama tanpa `itemProgress` | BLM: `UJI-GR-W05-1 · Diterima 10 November 2026 • 1 baris`. Lama: `… • 1 baris • total qty 60` pada kedua kasus | `PASS` (render harness di luar repository) | Keluaran skrip |
| Grep anti-regresi pada baris yang ditambah di dua view | Tidak ada `<button>`, `.btn`, `fw-*`, `fs-*`, warna literal, `!important`, `style={{}}`, atau `<table>` baru dari task ini | `PASS` | Keluaran `grep` pada `git diff` |
| Rincian GR di dalam halaman detail PO (sel "Qty Diterima") | Belum dirender utuh karena komponen bergantung pada hook dan Redux; logika sel diuji sebagai fungsi murni dan JSX-nya hanya memanggil fungsi itu | `NOT FEASIBLE` oleh agen | Alasan di samping |
| Pengamatan di layar oleh pemilik | Server lokal, 11 November 2026: hasil cocok dengan tabel di bawah | `PASS` (pengamatan pemilik) | Pernyataan pemilik di server lokal, 11 November 2026, tanpa tangkapan layar |
| `npm run dev` / `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik |

### Yang harus tampil di layar (data uji pemilik; dicocokkan pemilik pada 11 November 2026)

Buka detail PO, klik "Lihat Rincian" pada GR yang disebut.

| PO | Baris GR di panel jejak | Rincian GR, kolom "Qty Diterima" |
| --- | --- | --- |
| `UJI-BLM-W05` | `UJI-GR-W05-1` · `Diterima … • 1 baris` (tanpa "total qty") | `6 PCS` |
| `UJI-BLM-W07` | `UJI-GR-W07-1`, `UJI-GR-W07-2` tanpa "total qty" | W07-1: `6 PCS`. W07-2: dua baris, `4 PCS` tanpa keterangan dan `2 PCS` dengan keterangan kecil "Menunggu periksa" |
| `UJI-BLM-W08` | Sama seperti W07 | W08-2: `4 PCS` dan `2 PCS` dengan keterangan kecil "Rusak" |
| `UJI-BLM-W09` | `UJI-GR-W09-1` tanpa "total qty" | `12 PCS` (bukan `1 BOX` dan bukan `0.08`) |
| `UJI-BLM-W10` | `UJI-GR-W10-1` tanpa "total qty" | `6 KG`. Satuan dasar GR sengaja berbeda dari item PO; angkanya jujur menampilkan apa yang tercatat, sedangkan panel kemajuan menandai konversi belum pasti |
| `UJI-BLM-W11` | W11-1 dan W11-2 (dibatalkan) tanpa "total qty" | W11-1: `6 PCS` |
| `UJI-LAMA-B` | `… • N baris • total qty 60` **masih ada** (tidak berubah) | Angka lama, mis. `60` |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Rincian GR BLM menampilkan jumlah satuan dasar beserta satuannya | Terpenuhi (source, unit test, dan pengamatan pemilik di layar) | Test "baris GR BLM menampilkan jumlah satuan dasar…" |
| Kondisi bukan GOOD diberi keterangan "Menunggu periksa" atau "Rusak" | Terpenuhi (source, unit test, dan pengamatan pemilik di layar) | Test "PENDING_INSPECTION dan DAMAGED…" |
| Subjudul jejak PO BLM tidak memuat "total qty" | Terpenuhi pada source, unit test, dan render | Test subjudul PO BLM; keluaran render |
| GR lama dan PO lama tampil persis seperti sebelumnya | Terpenuhi pada source, unit test, dan render | Test "baris GR lama…", test "subjudul PO lama identik…", keluaran render |
| Tidak ada kolom, komponen, atau CSS baru | Terpenuhi | Diff dan grep anti-regresi |
| Tidak ada perhitungan atau konversi satuan di klien | Terpenuhi | Fungsi hanya memformat nilai dari backend |
| Pengamatan di layar oleh pemilik | Terpenuhi | Pernyataan pemilik, 11 November 2026 (tabel §6) |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kata "Menunggu periksa" dan "Rusak" disamakan dengan judul kolom panel kemajuan. Bila pemilik ingin kata lain, ganti satu konstanta |
| Masalah yang diketahui | Baris jejak PO BLM tidak lagi menyebut jumlah barang. Bila pemilik ingin angka satuan dasar muncul juga di baris itu, perlu permintaan BE terpisah (ringkasan jejak harus membawa total satuan dasar dan sumber dokumen). Tidak dikerjakan di task ini |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | FE: `M goods-receipt-constants.jsx`, `M purchasing-trail-utils.jsx`, `M purchasing-document-trail-panel.jsx`, `M purchase-order-detail-view.jsx`, `?? tests/unit/finance-purchasing-gr-base-quantity-display.test.mjs` (task ini), ditambah perubahan `FE-FIN-062`, `063`, dan `FIX-003` yang belum di-commit. Tidak ada stage/commit |
| Langkah berikutnya | Tidak ada. Roadmap tidak ditandai karena tidak ada baris roadmap resmi (pola `FIX`) |
