# Laporan Perubahan Frontend — `FE-FIN-FIX-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-FIX-003` |
| Judul | Kolom "Jenjang Persetujuan" pada daftar dan detail PO menampilkan "Eksternal (BLM)" untuk PO bersumber BLM, bukan teks mentah `EXTERNAL` |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap. Mengikuti pola `FIX` yang dipakai `FE-FIN-FIX-001` dan `002` |
| Roadmap | Tidak ada baris roadmap resmi. Permintaan langsung pemilik, 10 November 2026 ("bolehh perbaiki saja"), setelah pemilik melihat daftar PO uji di layar |
| Trace | `FIN-DES-130` (tier `EXTERNAL` untuk PO BLM); `CK_FinPurchaseOrder_ApprovalTier`; komentar `FIN-DEC-052` pada konstanta (label hanya untuk tampilan) |
| Contract version | **Tidak ada perubahan kontrak.** Nilai tier tetap berasal dari respons backend; konstanta hanya memberi nama yang dibaca manusia |
| Dependency | Tidak ada. Backend tidak diubah |
| Wewenang UI | Hanya salinan teks satu label. Nol perubahan tata letak, warna, tipografi, atau komponen |
| Klasifikasi | `LIGHT` — 1 berkas konstanta (+3 baris), 1 berkas test ditambah |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `yasmina`); di repository backend hanya laporan ini |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ef8be6584` (perubahan task ini belum di-commit) |
| Tanggal | 10 November 2026 |
| Status | ✅ **Selesai dan terverifikasi di layar oleh pemilik (10 November 2026).** 5 unit test baru PASS, ESLint PASS; label teramati pada daftar PO dan detail `UJI-BLM-W07`. `npm run dev`/`build` oleh agen tidak dijalankan |

---

## 1. Masalah yang diperbaiki

Backend mengizinkan tiga nilai `ApprovalTier` pada PO: `TIER_1`, `TIER_2`, dan (sejak Revisi 23) `EXTERNAL` untuk PO bersumber BLM, yang tidak melewati jenjang persetujuan lokal. Peta label di `purchase-order-constants.jsx` hanya memuat dua yang pertama. Daftar dan detail PO memakai pola `APPROVAL_TIER_LABELS[tier] || tier || "-"`, sehingga nilai tanpa label jatuh ke teks mentah.

**Bukti dari layar pemilik** (daftar PO uji, 10 November 2026): sembilan PO BLM menampilkan `EXTERNAL`, sedangkan dua PO lama menampilkan "Jenjang 1 — Supervisor Finance".

Contoh sebelum dan sesudah, pada kolom "Jenjang Persetujuan":

| PO | Sebelum | Sesudah |
| --- | --- | --- |
| `UJI-LAMA-A` (lama) | Jenjang 1 — Supervisor Finance | Jenjang 1 — Supervisor Finance (tidak berubah) |
| `UJI-BLM-W05` (BLM) | `EXTERNAL` | Eksternal (BLM) |

Penyebab ini terlewat pada `FE-FIN-062` dan `063` karena blueprint §27 hanya menyebut panel kemajuan item dan pesan 409; tidak ada task frontend yang memetakan tier baru.

---

## 2. Proses bisnis dari sisi pengguna

Petugas membuka daftar PO atau detail sebuah PO. Untuk PO dari BLM, kolom atau baris "Jenjang Persetujuan" kini terbaca "Eksternal (BLM)", yang menjelaskan bahwa PO itu tidak disetujui lewat jenjang lokal. PO lama tidak berubah. Bila backend kelak mengirim tier yang belum punya label, layar tetap menampilkan nilainya apa adanya (bukan kosong), dan nilai kosong tampil sebagai "-".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`purchase-order-constants.jsx`, `purchase-order-table-columns.jsx` (baris 57–62), `purchase-order-detail-view.jsx` (baris 175), `purchasing-invoice-constants.jsx` (peta label milik invoice), `due-date-report-view.jsx`, dan konfigurasi backend `FinPurchaseOrderConfiguration.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/purchasing/purchase-order-constants.jsx` | `APPROVAL_TIER_LABELS` ditambah `EXTERNAL: "Eksternal (BLM)"` beserta satu baris komentar alasan |
| `tests/unit/finance-purchasing-approval-tier-label.test.mjs` | **Baru** — 5 unit test |

Tidak ada JSX yang berubah: kolom daftar dan detail PO sudah memakai peta ini.

**Sengaja tidak diubah:**

| Hal | Alasan |
| --- | --- |
| `APPROVAL_TIER_LABELS` pada `purchasing-invoice-constants.jsx` | Peta milik invoice; tier `EXTERNAL` hanya berlaku untuk PO (`CK_FinPurchaseOrder_ExternalTier`) |
| `due-date-report-view.jsx` | Menampilkan tier mentah dalam huruf monospace pada laporan jatuh tempo; di luar permintaan, dan bisa jadi disengaja untuk laporan |
| Backend, kontrak, komponen | Di luar cakupan |

### 3.3 Kepatuhan arsitektur frontend

Konstanta tetap di `src/lib/constants/finance/purchasing/`, test di `tests/unit/`. Tidak ada pola baru dan tidak ada nilai visual literal.

**UI GATE:** `N/A` — hanya konstanta; tidak ada JSX atau CSS yang dibuat atau diubah.

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — tidak ada perubahan pada state memuat, kosong, gagal, atau tanpa hak akses. Satu-satunya perilaku yang disentuh: nilai tier tanpa label tetap jatuh ke nilai mentah, dan kosong menjadi "-" (keduanya dikunci test).

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada endpoint baru dan tidak ada perubahan pemanggilan API.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-approval-tier-label.test.mjs` | 5 test, 5 lulus | `PASS` | Keluaran perintah |
| `npx eslint --max-warnings 0` pada konstanta dan test | exit 0 | `PASS` | Keluaran perintah |
| Seluruh `tests/unit/` | 4651 test, 4609 lulus, 42 gagal; daftar nama yang gagal identik dengan sebelum perbaikan (modul lain) | `EXISTING / ENVIRONMENT ISSUE` (0 baru) | Keluaran perintah; `diff` daftar nama |
| Label tampil di layar (daftar dan detail PO) | Daftar PO: sembilan PO `UJI-BLM-*` menampilkan "Eksternal (BLM)", dua PO `UJI-LAMA-*` tetap "Jenjang 1 — Supervisor Finance". Detail `UJI-BLM-W07`: baris "Jenjang Persetujuan" terbaca "Eksternal (BLM)" | `PASS` (pengamatan pemilik) | Tangkapan layar pemilik, 10 November 2026, `localhost:3000/finance/purchasing/purchase-orders` |
| `npm run dev` / `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik |

Uji manual: `PASS` — diamati pemilik pada daftar PO dan detail PO (lihat baris di atas). Agen tidak memiliki browser, jadi pengamatan itu berasal dari tangkapan layar pemilik.

AUTOMATED TEST: `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/` — 5 test baru PASS; 42 kegagalan lama di modul lain tidak berubah.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| PO BLM tidak menampilkan teks mentah `EXTERNAL` pada daftar dan detail | Terpenuhi (source, unit test, dan pengamatan pemilik di layar) | Konstanta, test "PO BLM menampilkan 'Eksternal (BLM)'", tangkapan layar daftar dan detail |
| Label PO lama tidak berubah | Terpenuhi | Test "label PO lama tidak berubah" |
| Nilai tier tak dikenal tetap tampil apa adanya, kosong menjadi "-" | Terpenuhi | Test "nilai tak dikenal…" |
| Tidak ada perubahan di luar satu label | Terpenuhi | Diff: +3 baris pada konstanta, 1 berkas test baru |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Teks "Eksternal (BLM)" adalah pilihan agen. Bila pemilik ingin teks lain (mis. "Dari BLM"), cukup ganti satu string pada konstanta dan satu string pada test |
| Masalah yang diketahui | Laporan jatuh tempo (`due-date-report-view.jsx`) masih menampilkan tier mentah; tidak diubah karena di luar permintaan |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | FE: `M purchase-order-constants.jsx` dan `?? tests/unit/finance-purchasing-approval-tier-label.test.mjs` (task ini), ditambah perubahan `FE-FIN-062`/`063` yang belum di-commit. Tidak ada stage/commit |
| Langkah berikutnya | Tidak ada. Roadmap tidak ditandai karena tidak ada baris roadmap resmi (pola `FIX`) |
