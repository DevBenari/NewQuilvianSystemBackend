# Laporan Perubahan Frontend — `FE-BKC-FIX-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-008` (ad-hoc, di luar roadmap, otorisasi eksplisit pengguna lewat `AskUserQuestion` — task yang SAMA dengan `BE-BKC-FIX-003`, dipecah dua laporan sesuai konvensi repo) |
| Judul | Badge status per item dan split Subtotal/Pajak Mandiri-Asuransi jadi EKSAK, memakai hasil waterfall per komponen dari `BE-BKC-FIX-003` |
| Slice | Lanjutan investigasi laporan bug pengguna atas `FE-BKC-FIX-006`/`BE-BKC-FIX-002` |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — mengonsumsi field baru pada payload `GET .../calculation-preview` yang sudah ada (lihat `BE-BKC-FIX-003.md`) |
| Wewenang UI | `REUSE` murni — `StatusBadge`/`BILLING_ITEM_COVERAGE_BADGE_CONFIG` tetap dipakai apa adanya, hanya menambah satu status baru (`belum_terverifikasi`) dan mengganti SUMBER data badge/subtotal, bukan komponennya |
| Dependency | `BE-BKC-FIX-003` (backend) — WAJIB di-rebuild dulu, field baru belum ada di response tanpa itu |
| Klasifikasi | `MEDIUM` — mengganti seluruh logika badge dan perhitungan ringkasan pembayaran di satu file, tapi murni REUSE base component |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `menu-pembayaran-view.jsx`, `billing-invoice-constants.js` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai, lint bersih. **Belum diverifikasi hidup** — menunggu `BE-BKC-FIX-003` di-rebuild dan frontend dev server restart |

---

## 1. Keadaan yang ditemukan di awal

Lihat `task/report/backend/BE-BKC-FIX-003.md` § 1 untuk analisis lengkap. Ringkas: badge status
per item (`FE-BKC-FIX-006`) dan split Subtotal/Pajak Mandiri-Asuransi (percobaan sebelumnya di sesi
yang sama) SAMA-SAMA hanya bisa memakai data tingkat KATEGORI/INVOICE (bukan tingkat item
sesungguhnya), sehingga keduanya salah begitu diuji dengan data nyata pengguna. `BE-BKC-FIX-003`
memperluas backend untuk melacak hasil waterfall PER KOMPONEN; task ini mengonsumsinya di frontend.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir di Menu Pembayaran.

**Sebelum**: badge "Penjamin"/"Tunai" per baris item bisa salah (semua "Penjamin" walau sebagian
tidak benar-benar tercover); "Subtotal Mandiri"/"Subtotal Asuransi" dan "Pajak Mandiri"/"Pajak
Asuransi" adalah PERKIRAAN (proporsional terhadap total invoice), bisa meleset jauh dari kenyataan
saat ada item yang statusnya masih "Penjamin Belum Terverifikasi".

**Sesudah**:
1. Badge per baris item sekarang punya TIGA kemungkinan: "Tunai" (tidak tercover sama sekali),
   "Penjamin" (ada rule yang cocok dan disetujui), "Menunggu Verifikasi" (item ini secara aturan
   layak diklaim tapi belum ada rule yang cocok/provider tidak eligible - BUKAN otomatis Tunai
   ataupun Penjamin).
2. "Subtotal Mandiri"/"Subtotal Asuransi"/"Pajak Mandiri"/"Pajak Asuransi" sekarang EKSAK -
   dijumlahkan langsung dari hasil per item/komponen backend, bukan perkiraan.

**Jalur tidak normal**: item yang tidak ditemukan di `breakdown.items` (jarang - race condition
kalkulasi belum sempat menghitung ulang) default ke badge "Tunai" (lebih aman daripada menebak
"Penjamin" tanpa dasar).

---

## 3. Perubahan yang dikerjakan

Lihat `task/report/backend/BE-BKC-FIX-003.md` § 2.4-2.5 untuk detail perubahan berkas
(`menu-pembayaran-view.jsx`, `billing-invoice-constants.js`) - tidak diulang di sini supaya tidak
ada dua sumber kebenaran yang bisa berbeda seiring waktu.

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Badge status "Menunggu Verifikasi" | `REUSE` | `StatusBadge` + `BILLING_ITEM_COVERAGE_BADGE_CONFIG` sudah ada, hanya menambah satu entry status baru dengan `className` token yang SUDAH ADA (`region-status-pending`, dipakai ulang dari `CATALOG_CHARGE_COVERAGE_BADGE_CONFIG.needapproval`) - tidak ada nilai visual literal baru |

**`UI GATE`**: tidak ada - murni REUSE, tidak ada base component baru/extend.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint menu-pembayaran-view.jsx billing-invoice-constants.js` | Berhasil tanpa error | `PASS` |
| Uji hidup (badge 3 status, subtotal/pajak eksak menjumlah ke Total Tagihan) | **BELUM dilakukan** - menunggu `BE-BKC-FIX-003` di-rebuild backend dan frontend dev server restart | `MANUAL TEST: BLOCKED` |

**Tidak dijalankan:** component test (instruksi eksplisit pengguna sepanjang sesi ini — tanpa file
test).

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dependency backend | `BE-BKC-FIX-003` WAJIB di-rebuild lebih dulu - field `itemPrimaryAmount`/dst tidak ada di response tanpa itu, badge/subtotal akan tampil salah/kosong (fallback ke 0) sampai backend benar-benar mengembalikan data baru |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` |
| Status Git | Modified (task ini): `menu-pembayaran-view.jsx`, `billing-invoice-constants.js`. Belum staged/commit |
| Langkah berikutnya | Verifikasi hidup begitu `BE-BKC-FIX-003` selesai di-rebuild - lihat `BE-BKC-FIX-003.md` § 5 untuk skenario uji lengkap |
