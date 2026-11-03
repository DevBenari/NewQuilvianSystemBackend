# Laporan Perubahan Frontend — `FE-OPT-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-001` |
| Judul | Aturan bersama teruji unit: input desimal bertanda minus, pratinjau transposisi, cermin aturan rentang/kelipatan |
| Slice | `MVP-0`/`MVP-1` — aturan bersama tanpa layar |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r1, `approved` 2026-11-03) — Ringkasan Task dan Rincian `FE-OPT-001` |
| Trace | `OPT-DEC-045`, `OPT-FE-003`; `OPT-VAL-004`, `005`, `010`..`016`, `018`; `OPT-AC-028` |
| Contract version | `OPT-VAL-v1` — `approved` sisi Optik (Yoga Aji Pratama, 2026-11-03) |
| Wewenang UI | Tidak ada; task tanpa layar, menu, atau komponen |
| Dependency | Tidak ada (roadmap: mandiri) |
| Klasifikasi | `LIGHT` — satu utilitas murni + satu berkas uji, tanpa state, HTTP, atau UI |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: `src/utils/health-services/optical-management/`, `tests/unit/`. Laporan ini di repository backend sesuai `REPORT_TEMPLATE` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `b6dd6c304` (branch `YogaV2`, upstream `origin/YogaV2`) |
| Commit backend yang dijadikan rujukan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi; belum di-commit (commit wewenang pemilik) |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Belum ada utilitas Optik apa pun | Folder `src/utils/health-services/optical-management/` belum ada |
| Pola desimal bersama sudah ada dan dipakai MCU | `src/lib/hooks/health-services/laboratory-management/lab-decimal-input-rules.js` (`normalizeDecimalInput`, `parseDecimal`, `DECIMAL_FIELD_PROPS`), dibungkus `src/utils/health-services/medical-checkup-management/mcu-format-rules.js` |
| Normalizer Lab membuang minus tipografis `−` (U+2212) | Regex `[^\d.,-]` menghapus karakter selain digit, pemisah, dan `-`. Akibatnya resep yang disalin dari dokumen bisa berubah dari `−1,50` menjadi `1,50`, sehingga tandanya terbalik |

---

## 2. Proses bisnis dari sisi pengguna

Task ini belum punya layar. Aturannya nanti dipakai layar resep (`FE-OPT-012`) sebagai berikut:

1. RO/Sp.M mengetik ukuran, misalnya OS SPH `−1,75`, CYL `−0,50`, AXIS `180`. Tanda minus tetap ada walau disalin dari dokumen, dan pemisah koma diterima.
2. Layar langsung menandai isian yang pasti ditolak server dengan kalimat yang sama dengan backend. **Contoh:** SPH `−1,30` → "SPH harus kelipatan 0,25 antara −20,00 dan +20,00."; CYL `−0,75` tanpa AXIS → "AXIS wajib diisi bila ada silinder.".
3. Isian yang hanya melewati rentang ditandai terpisah. **Contoh:** SPH `−22,00` → untuk RO muncul "SPH di luar rentang −20,00..+20,00; hanya Sp.M yang dapat mengonfirmasi."; untuk Sp.M muncul isian alasan, dan tanpa alasan muncul "Tuliskan alasan konfirmasi ukuran di luar rentang.".
4. Saat menyalin resep luar bernotasi plus, layar menampilkan pratinjau transposisi. **Contoh:** `+1,00 / +0,50 × 90` → `+1,50 / −0,50 × 180`, dengan teks asli `+1,00 / +0,50 × 90` disimpan sebagai catatan.

Jalur tidak normal: isian bukan angka (`abc`, `-1,5,0`) ditolak "Nilai harus berupa angka, contoh -1,50."; pratinjau transposisi tidak ditampilkan bila silinder plus tanpa AXIS.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md` (bagian `src/utils`); `rules/frontend/test-policy.md`; `src/lib/hooks/health-services/laboratory-management/lab-decimal-input-rules.js`; `src/utils/health-services/medical-checkup-management/mcu-format-rules.js`; `src/utils/shared/input-normalizer-utils.jsx`; `tests/unit/mcu-examination-screen-rules.test.mjs`; `tests/helpers/register.mjs`; kontrak `contracts/validation-matrix.md` (`OPT-VAL-v1`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/optical-management/optical-prescription-rules.js` | **Baru.** Batas isian kacamata (`OPT-DEC-045`), kalimat galat `OPT-VAL-v1`, normalizer desimal bertanda minus (termasuk `−`), parse/format ukur, cek kelipatan 0,25, validasi satu mata kacamata (`errors` + `outOfRange`), validasi PD, validasi BC/DIA terhadap rentang pengaturan, keputusan lewat rentang (RO/Sp.M), pratinjau transposisi, dan teks notasi asli |
| `tests/unit/optical-prescription-rules.test.mjs` | **Baru.** 22 uji `node:test` |

### 3.3 Kepatuhan arsitektur frontend

- Utilitas murni di `src/utils/health-services/optical-management/`, sesuai aturan "utility domain mengikuti domain": tanpa hook, JSX, request API, atau Redux.
- **Pakai ulang** `lab-decimal-input-rules.js` lewat pola pembungkus yang sama dengan `mcu-format-rules.js`. Tidak ada normalizer desimal kedua.
- Batas isian kacamata ditaruh sebagai `Object.freeze` di berkas aturan, bukan di `src/lib/constants`, karena merupakan aturan validasi yang terkunci keputusan dan dipakai bersama kalimat galatnya. Rentang BC/DIA tidak di-hardcode; nilainya datang dari pengaturan Optik.
- Uji mengikuti pola tetangga: `tests/unit/<fitur>.test.mjs`, impor alias `@/` lewat `register.mjs`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` — tanpa layar |
| Kosong | `NOT APPLICABLE` — isian kosong diperlakukan "tidak diresepkan" (`null`), tanpa galat |
| Gagal | `NOT APPLICABLE` — tanpa request |
| Tanpa hak akses | `NOT APPLICABLE` — keputusan lewat rentang menerima `canOverrideRange` dari pemanggil; izinnya dibaca layar dari `OptPrescription : OverrideRange` |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task tidak memanggil API.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-prescription-rules.test.mjs` | 22 lulus, 0 gagal | `PASS` | Keluaran perintah (22/22) |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` (suite penuh) | 4.207 uji: 4.152 lulus, 55 gagal | `EXISTING / ENVIRONMENT ISSUE` | 55 gagal = baseline 2026-11-02 (55); tidak ada uji Optik di antara baris `not ok` |
| `npx eslint` atas dua berkas baru | Tanpa error | `PASS` | Exit code 0 |
| `-1,50` tidak menjadi `-150` | Tetap `-1.5` | `PASS` | Uji "-1,50 tidak pernah berubah menjadi -150" |
| Transposisi contoh kontrak | `+1,50 / −0,50 × 180` | `PASS` | Uji "transposisi contoh kontrak" |

AUTOMATED TEST: `npm run test:unit` (via perintah setara di atas) — `PASS` untuk berkas baru; suite penuh sesuai baseline.

Uji manual: `NOT APPLICABLE` — task tanpa kontrol interaktif. Perilaku kontrol diuji saat aturan dipasang di layar resep (`FE-OPT-012`).

**Tidak dijalankan:** `npm run build`, karena tidak ada route, komponen, atau import dari kode aplikasi yang berubah; berkas baru belum dipakai modul mana pun. E2E tidak berlaku.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| −1,50 tetap −1,50 | Terpenuhi | Uji normalizer + parse, termasuk minus tipografis |
| −1,30 ditandai salah | Terpenuhi | Uji `OPT-VAL-010` |
| Transposisi sesuai contoh (`+1,00 / +0,50 × 90` → `+1,50 / −0,50 × 180`) | Terpenuhi | Uji transposisi |
| AXIS wajib bila CYL ≠ 0 | Terpenuhi | Uji `OPT-VAL-012` |
| Uji unit lulus (DoD) | Terpenuhi | 22/22 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Aturan layar hanya cermin; backend (`BE-OPT-012`) tetap penentu. Bila `OPT-VAL-v1` berubah, berkas ini wajib diselaraskan |
| Masalah yang diketahui | Normalizer Lab bersama tetap membuang `−` (U+2212) untuk modul lain. Tidak diubah karena di luar target tulis; layar Lab/MCU yang menerima salinan bertanda tipografis berisiko sama. Usulan: task terpisah untuk `lab-decimal-input-rules.js` |
| Dependency backend | `BE-OPT-012` belum ada; kalimat galat disalin dari kontrak `OPT-VAL-v1`, belum dari respons server nyata |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `?? src/utils/health-services/optical-management/`, `?? tests/unit/optical-prescription-rules.test.mjs` |
| Langkah berikutnya | `FE-OPT-002`/`003` (lapisan data, paralel dengan backend), atau `BE-OPT-002` setelah baris registry `Opt` tercatat |
