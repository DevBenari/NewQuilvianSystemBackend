# Laporan Perubahan Frontend — `FE-LAB-37`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-37` |
| Judul | Daftar Kerja membuka halaman order; dialog isi hasil dicabut |
| Slice | Gelombang `MVP-8c` — `EPIC-LAB-14` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-8`, bagian `FE-LAB-37` |
| Trace | `FR-14.11`; `LAB-DEC-149`, `LAB-FE-017`; `LAB-FE-006` (urutan cito); `03-frontend-architecture.md` amandemen 2026-09-24 *Daftar Kerja — diperbarui* |
| Contract version | — tidak memanggil endpoint baru (kontrak `MVP-8` `r33` `approved` 2026-09-24 tetap berlaku) |
| Wewenang UI | Diputuskan: dialog isi hasil dicabut; aksi baris membuka halaman order, dapat ditemukan tanpa petunjuk tersembunyi; urutan cito `LAB-FE-006`. `DEV_DISCRETION` yang dipakai: label tombol **Buka Hasil** di kolom *Aksi* yang sudah ada; aksi yang sama juga dipasang pada baris **Mikrobiologi** dan **Patologi Anatomi** (ke halaman hasil disiplinnya, pemetaan yang sama dengan daftar pantau) — lihat 3.3 |
| Dependency | `FE-LAB-36` ✅ (halaman pengganti berdiri dan terbuka dari daftar pantau sejak 2026-10-01) |
| Klasifikasi | `LIGHT` — 5 berkas source diubah (view, kolom, hook, aturan, CSS), 1 berkas uji; nol endpoint, nol berkas baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e613321c5` (branch `YogaV2`), di atas `FE-LAB-35`/`FE-LAB-36` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `55b032b0` + `BE-LAB-87` (belum ter-commit), `https://localhost:7184` di atas `QuilvianNewDevYoga` |
| Tanggal | 2026-10-01 |
| Status | ✅ **`SELESAI`** — nol dialog isi hasil di Daftar Kerja; setiap baris berdisiplin membuka halaman hasil ordernya; urutan cito tetap. Uji unit 45/45 (berkas terdampak), lint nol peringatan, build hijau, layar 9/9 dengan akun analis asli, nol tulis |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Daftar Kerja punya dialog `ConfirmModal` isi hasil Patologi Klinik — tempat mengisi hasil **kedua** di samping Halaman Hasil per order (`FE-LAB-36`) | `lab-worklist-view.jsx` (dulu baris 252-370) |
| Tombol *Isi Hasil*/*Perbaiki Hasil* tampil di **setiap** baris, termasuk Mikrobiologi dan Patologi Anatomi, dan semuanya membuka dialog Patologi Klinik | `lab-worklist-table-columns.jsx` kolom *Aksi* |
| Isian awal dialog (`buildResultFormState`) memotong `examinedAt` UTC lalu `buildResultPayload` membacanya sebagai waktu lokal — setiap simpan ulang menggeser waktu pemeriksaan **mundur tujuh jam**; satu uji unit justru mengunci pemotongan itu | `lab-worklist-rules.js`; `lab-worklist-result-entry.test.mjs` |
| Baris Daftar Kerja membawa `labOrderId` dan `discipline` — cukup untuk membuka halaman order tanpa panggilan tambahan | `LabWorklistItemResponse` |

---

## 2. Proses bisnis dari sisi pengguna

**Siapa:** analis laboratorium yang bekerja dari Daftar Kerja.

1. Analis membuka **Daftar Kerja Laboratorium**. Pemeriksaan cito tetap di urutan teratas.
2. Pada baris *Hemoglobin* (Patologi Klinik), ia menekan **Buka Hasil**.
3. Halaman **Hasil Patologi Klinik — LAB-RSMMC-000001** terbuka: seluruh pemeriksaan order itu dalam satu
   tabel, berikut informasi pasien. Di sanalah hasil diisi, disimpan, dan dinyatakan selesai.
4. Pada baris *Pewarnaan BTA Sputum* (Mikrobiologi), **Buka Hasil** membuka halaman hasil Mikrobiologi order
   tersebut; pada baris Patologi Anatomi, laporan PA order tersebut.

**Tidak ada lagi** dialog isi hasil — satu tempat untuk satu hasil.

**Jalur tidak normal:** baris tanpa disiplin atau tanpa `labOrderId` → tombol tidak dirender (bukan tautan
buntu). Kolom *Hasil* (*Hasil terisi*/*Belum diisi*) tetap menyatakan apa yang tercatat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-37`; `03-frontend-architecture.md` *Daftar Kerja — diperbarui*; `acceptance-test-matrix.md` `AC-235` | Cakupan dan kriteria |
| `lab-worklist-view.jsx`, `lab-worklist-table-columns.jsx`, `use-lab-worklist.jsx`, `lab-worklist-rules.js` | Dialog, tombol, dan state yang dicabut |
| `use-lab-monitoring.jsx` (`openResultWorkspace`, `openClinicalPathologyResult`, `openPathologyReport`) | Pola token privat dan route per disiplin |
| Backend `LabWorklistDtos.cs` | Ruas `labOrderId`/`discipline` baris |
| `tests/unit/lab-worklist-result-entry.test.mjs`, `lab-worklist-rules.test.mjs` | Uji yang menyentuh isian dialog |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../lab-worklists/lab-worklist-view.jsx` | Dialog `ConfirmModal` isi hasil dan seluruh destrukturisasi state hasil **dicabut** (376 → 249 baris); kolom menerima `openResultPage` |
| `src/components/view/.../lab-worklists/lab-worklist-table-columns.jsx` | Kolom *Aksi*: tombol **Buka Hasil** ber-`aria-label` *"Buka hasil {pemeriksaan}"*, dirender hanya bila baris punya halaman hasil |
| `src/lib/hooks/.../use-lab-worklist.jsx` | State dan fungsi dialog dicabut (`resultTarget` … `submitResult`, panggilan `getLabExaminationResultForm`/`setLabExaminationResult`); **`openResultPage`** — token privat order (sama dengan daftar pantau), lalu route menurut disiplin |
| `src/lib/hooks/.../lab-worklist-rules.js` | `EMPTY_RESULT_FORM` dan `buildResultFormState` (pembawa cacat tujuh jam) **dicabut**; **`resolveResultPageRoute`** dan **`hasResultPage`**. `validateResultEntry`/`buildResultPayload` tetap — dipakai Halaman Hasil PK |
| `src/lib/hooks/.../lab-clinical-pathology-result-rules.js` | `resolveDisciplineResultLink` memakai `resolveResultPageRoute` — route per disiplin kini satu sumber |
| `src/style/.../lab-worklists/lab-worklist.module.css` | Kelas khusus dialog (`resultForm`, `resultProcedure`, `resultField`, `resultHint`) dicabut |
| `tests/unit/lab-worklist-result-entry.test.mjs` | Dua uji isian awal dialog dicabut bersama fungsinya; dua uji baru: route per disiplin, dan jaminan nol dialog/nol tombol *Isi Hasil*/nol panggilan isi hasil di Daftar Kerja |

### 3.3 Kepatuhan arsitektur dan keputusan kecil

| Hal | Keputusan dan alasan |
| --- | --- |
| Aksi pada baris Mikrobiologi dan PA | Cakupan menyebut baris **Patologi Klinik**. Baris Mikrobiologi dan PA sebelumnya juga memakai tombol yang membuka dialog Patologi Klinik — yang tidak berlaku bagi mereka. Mencabut dialog tanpa memberi mereka halaman yang benar berarti meninggalkan baris tanpa aksi. Dipasang dengan pemetaan **yang sama** dengan aksi *Buka Hasil* daftar pantau — `DEV_DISCRETION`, nol halaman baru |
| Label | *Buka Hasil* — kata yang sama dengan daftar pantau, menyatakan tindakan membuka, bukan mengisi |
| Token route | `registerPrivateRouteToken` dengan scope order yang sama dengan detail pesanan dan daftar pantau |
| Urutan cito | Tidak disentuh — penegakan `LAB-FE-006` tetap di `sortWorklistRows`/`enforceCitoFirst` dan `sortLatestFirst={false}` |

`UI GATE: 1 elemen — REUSE 1` — `BaseButton` pada kolom *Aksi* yang sudah ada.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah (`DataTable` *loading*) |
| Kosong | Tidak berubah |
| Gagal | Tidak berubah; Daftar Kerja tidak lagi memanggil endpoint hasil, sehingga galat hasil kini milik halaman hasil |
| Tanpa hak akses | Tidak berubah; hak isi hasil kini ditegakkan halaman hasil (`FE-LAB-35`/`FE-LAB-36`) |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — nol endpoint baru. Daftar Kerja **berhenti** memanggil `GET /lab-examinations/{id}/result`
dan `PUT /lab-examinations/{id}/result`; keduanya kini hanya dipanggil Halaman Hasil per order.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-worklist-result-entry.test.mjs tests/unit/lab-worklist-rules.test.mjs tests/unit/lab-clinical-pathology-result-rules.test.mjs` | **45/45** | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2217 uji: 2211 lolos, 6 gagal — kegagalan lama di luar Laboratorium; seluruh uji Daftar Kerja lolos | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint --max-warnings=0` berkas yang disentuh | Nol masalah | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0 (79 detik); route `lab-worklists` dan `lab-worklists/cito-overdue` ada | `PASS` | Keluaran build |

**Verifikasi manual** — `next dev` `localhost:3000` terhadap backend lokal, akun analis asli; seluruh non-GET
Laboratorium dicatat dan dibatalkan:

| Skenario | Hasil sebenarnya |
| --- | --- |
| Daftar Kerja termuat | 8 baris tampil = 8 dari API |
| Nol tombol *Isi Hasil*/*Perbaiki Hasil*; nol dialog | Sesuai |
| Tombol *Buka Hasil* | 8 tombol = 8 baris berdisiplin berhalaman (5 PK, 1 PA, 2 Mikrobiologi) |
| Nol `GET /{id}/result` dari Daftar Kerja | Sesuai |
| `LAB-FE-006` | Cito, Cito, lalu enam baris biasa |
| *Buka Hasil* baris Hemoglobin (PK) | Membuka `clinical-pathology/{token}`; halaman memanggil `GET /by-order/{labOrderId baris itu}/results` — **order yang benar**; nol dialog |
| *Buka Hasil* baris Pewarnaan BTA Sputum (Mikrobiologi) | Membuka `microbiology/{token}`; halaman memanggil `GET /lab-orders/{labOrderId baris itu}` — order yang benar |
| Nol permintaan tulis | Sesuai |

Uji manual: `PASS`.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-worklist-result-entry.test.mjs — PASS`

`MANUAL TEST: PASS`

**Tidak dijalankan:** klik *Buka Hasil* baris Patologi Anatomi di layar — route-nya dibuktikan uji unit dan
sama dengan aksi daftar pantau yang sudah berjalan sejak `FE-LAB-28`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-235` — buka Daftar Kerja: nol dialog isi hasil; aksi baris membuka halaman order | Terpenuhi | Layar + uji unit |
| Verifikasi — uji unit Daftar Kerja yang ada tetap lulus | Terpenuhi; dua uji isian awal dialog dicabut bersama fungsinya (3.2) | Keluaran perintah |
| DoD — nol `ConfirmModal` isi hasil di Daftar Kerja | Terpenuhi | Uji unit membaca source; layar |
| DoD — `AC-235` terbukti; laporan `FE-LAB-37.md` | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint; build tanpa error |
| Masalah yang diketahui | Pengamatan **sudah ada sebelum task ini** (tangkapan layar sebelum perubahan sama): lencana kolom *Hasil* (*Hasil terisi*) terjepit dan terpecah per huruf karena kolomnya menyempit di tabel lebar. Tidak diubah — di luar lingkup |
| Dependency backend | Nol |
| Perubahan sampingan | `NONE` di repository. **Lingkungan:** `TaskStop` pada `next dev` hanya menghentikan pembungkus `npx`, sehingga build sempat berjalan saat dev server masih hidup (melanggar aturan pemilik modul "matikan server sebelum build"); ketiga proses `node` tertinggal dihentikan sesudah ketahuan, build tetap sah. Prosedur diperbaiki: port diperiksa bebas sebelum build |
| Interupsi | `NONE` |
| Status Git | Frontend (`e613321c5`): ` M` `lab-worklist-view.jsx`, `lab-worklist-table-columns.jsx`, `use-lab-worklist.jsx`, `lab-worklist-rules.js`, `lab-worklist.module.css`, `lab-worklist-result-entry.test.mjs` — milik `FE-LAB-37`; `lab-clinical-pathology-result-rules.js` disentuh `FE-LAB-36` dan task ini; sisanya milik `FE-LAB-35`/`FE-LAB-36`. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-38` (`SIAP DIKERJAKAN`) atau `FE-LAB-39` (`SIAP DIKERJAKAN`, melanjutkan Halaman Hasil PK dengan validasi dan rilis) |
