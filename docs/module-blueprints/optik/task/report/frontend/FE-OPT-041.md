# Laporan Perubahan Frontend — `FE-OPT-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-041` |
| Judul | Penyaring kebijakan diskon Optik |
| Slice | Amendment kesiapan — layar `FE-OPT-N07`, `N08` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r3) — Rincian `FE-OPT-041` |
| Trace | `FR-OPT-082`, `083`; `OPT-DEC-079`, `084`; `OPT-AC-037`; `UAT-28`, `29`; risiko `R4` |
| Contract version | `OPT-INT-v1.2` `INT-OPT-11`, `OPT-PERM-v1.2`, brief r3 §5.8 — `approved` sisi Optik, Yoga Aji Pratama 2026-11-06 |
| Wewenang UI | Tidak ada perubahan tata letak. Isi pilihan mengikuti brief r3 §5.8 |
| Approval task | Yoga Aji Pratama, 2026-11-06 ("sudah disetujui hamzah dan yasmina kerjakan"); `OPT-APR-014`, `OPT-OQ-020` Yasmina (`OPT-EVD-003`) |
| Dependency | `FE-OPT-020` ✅ |
| Klasifikasi | `LIGHT` — 4 berkas produksi kecil, 2 berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `811f22159` (branch `YogaV2`) + `FE-OPT-040` di working tree |
| Tanggal | 2026-11-06 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan). Kebijakan `OPT-DISC-10` belum dibuat di data, sehingga belum diuji dengan backend asli. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Modal Ajukan Diskon menampilkan semua kebijakan `PROMO_ITEM` aktif, termasuk milik unit lain. Petugas Optik bisa memilih promo 50% yang bukan untuknya, dan Billing langsung memberlakukannya.

Selain itu, bila pengguna tidak punya `DiscountPolicy : Read`, modal menulis "Belum ada kebijakan diskon aktif.", seolah kebijakannya memang tidak ada.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan:** petugas hanya melihat kebijakan Optik, sehingga diskon tidak melebihi 10% lewat layar ini.

1. Petugas membuka Ajukan Diskon.
2. Layar meminta kebijakan aktif dari Billing, lalu menyaring `PROMO_ITEM` berkode awalan `OPT-`.
3. Petugas memilih kebijakan dan baris tagihan, lalu mengisi alasan.
4. Billing langsung mengurangi tagihan.

**Contoh:** `OPT-DISC-10` tampil; memilihnya pada frame Rp 450.000 mengurangi tagihan Rp 45.000. `PROMO-50` dan diskon dokter tidak tampil. Permintaan 15% tidak punya pilihan.

| Keadaan | Hasil |
| --- | --- |
| Belum ada kebijakan Optik | *"Belum ada kebijakan diskon aktif."* |
| Tanpa `DiscountPolicy : Read` | Pesan server tampil di modal |
| Penolakan Billing saat menerapkan | Pesan server tampil di modal, seperti sebelumnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`use-apply-billing-discount.js`, `master-data-discount-policy-slice.jsx`, `DiscountPolicyService.cs`, modal aksi pesanan, hook detail pesanan, spec E2E pesanan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/optical-management/optical-order-constants.js` | `OPTICAL_DISCOUNT_POLICY_TYPE`, `OPTICAL_DISCOUNT_POLICY_CODE_PREFIX` |
| `src/utils/health-services/optical-management/optical-order-rules.js` | `toOpticalDiscountPolicies`; tidak peka huruf besar |
| `src/components/view/health-services/optical-management/orders/optical-order-action-modals.jsx` | Modal memakai penyaring itu |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-detail.jsx` | `discountError` juga membawa galat daftar kebijakan |
| `tests/unit/optical-order-rules.test.mjs`, `tests/e2e/optical-order-screen.spec.mjs` | 1 uji unit, 1 skenario E2E baru, dan skenario diskon lama diperketat |

Hook Billing `use-apply-billing-discount.js` tidak berubah, sehingga layar Billing tetap menampilkan seluruh kebijakannya.

### 3.3 Kepatuhan arsitektur frontend

| Aspek | Keadaan |
| --- | --- |
| Penyaring | Fungsi murni di berkas aturan Optik |
| Komponen | Tidak ada komponen baru |
| Thunk Billing | Dipakai apa adanya |

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku | Bukti |
| --- | --- | --- |
| Memuat | Indikator muat pilihan, seperti semula | Kode |
| Kosong | *"Belum ada kebijakan diskon aktif."* | E2E |
| `403` | Pesan server | E2E |
| Terpilih | Label nama kebijakan; kiriman membawa id kebijakan | E2E `UAT-28`, `29` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Billing Management / Master Data / Discount Policy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar kebijakan aktif untuk modal | `DiscountPolicy : Read` |

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/discounts` | Menerapkan kebijakan pada baris tagihan | `BillingDiscount : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Tanpa error | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil; 2 menit 7 detik | `PASS` | Keluaran build |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.481 uji: 4.405 lulus, 72 gagal, 4 dilewati. Daftar 72 yang gagal identik per nama dengan run `FE-OPT-040`, dan tidak ada yang milik Optik | `PASS` untuk task ini; 72 `UNRELATED EXISTING ISSUE` | Keluaran di scratchpad sesi |
| E2E seluruh layar Optik atas build produksi | 152 lulus, 0 gagal; 2,8 menit | `PASS` | Keluaran Playwright |

Filter/select di peramban: daftar opsi hanya memuat kebijakan Optik (`toHaveCount(1)`), label terpilih dan kiriman benar, dan keadaan kosong tampil. Reset, filter gabungan, dan paginasi `NOT APPLICABLE`: pilihan satu tingkat tanpa halaman di layar.

**Uji manual: `NOT FEASIBLE`.** Belum ada kebijakan `OPT-DISC-10` dan belum ada pesanan di devYoga (`B2`, `MstDiscountPolicy` 0).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-037`: hanya kebijakan berpenanda Optik | Terpenuhi | Uji unit; E2E `UAT-28` |
| (2) Tidak ada kebijakan Optik: keterangan tampil | Terpenuhi | E2E `FE-OPT-041` |
| (3) Tanpa `DiscountPolicy : Read`: pesan server tampil | Terpenuhi | E2E `FE-OPT-041` |
| (4) `UAT-28`, `29` lulus di E2E API tiruan | Terpenuhi | E2E |
| (5) Layar Billing tetap menampilkan seluruh kebijakannya | Terpenuhi | Hook dan slice Billing tidak berubah (diff) |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kode kebijakan wajib berawalan `OPT-`. Kebijakan Optik yang diberi kode lain tidak akan tampil di modal |
| Masalah yang diketahui | Hook memuat paling banyak 100 kebijakan aktif, lalu layar menyaring. Bila kebijakan aktif lebih dari 100, kebijakan Optik bisa terpotong. Jumlah sesungguhnya belum diperiksa, karena devYoga berisi 0 |
| Risiko tersisa | Penyaringan hanya di layar (risiko diterima `OPT-DEC-079`). Pemanggil langsung ke endpoint Billing tetap bisa memakai kebijakan lain |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` 4 berkas `src/` dan 2 berkas `tests/`, ditambah perubahan task lain yang belum di-commit |
| Langkah berikutnya | Billing membuat kebijakan `OPT-DISC-10`; `DiscountPolicy : Read` diberikan ke petugas penjualan dan Kepala Optik |
