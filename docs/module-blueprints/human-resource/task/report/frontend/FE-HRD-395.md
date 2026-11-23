# Laporan Perubahan Frontend — `FE-HRD-395`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-395` |
| Judul | Layar Sanksi: filter Status dan label status lengkap |
| Slice | `S-PKB` / `MVP-73` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 30 |
| Trace | `FR-PKB-18`; `HRD-DEC-230`; `HRD-DEC-234`; API 35.2; State 33.2 |
| Contract version | `v29`, disetujui 23 November 2026 |
| Wewenang UI | Menambah filter Status menggunakan base component existing; tidak mengubah alur penerbitan SP |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — view, konstanta, serialisasi service, unit test, dan browser test |
| Task mode | `CROSS-REPO` |
| Target tulis | Source frontend; laporan, roadmap, dan traceability HRD |
| Model | Codex GPT-5 |
| Commit frontend saat dikerjakan | `573b7429c967a3229fc651437a902c33eac3708f` |
| Commit backend yang dijadikan rujukan | `b837892ad499289e9dd7dcaf869d9090c50c9939` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Layar Surat Peringatan hanya dapat menyaring Tingkat. Peta `SANCTION_STATUS` hanya mengenal `Active`, dan service menyerahkan object parameter kepada serializer Axios yang berpotensi menulis array sebagai `statuses[]`, tidak sesuai kontrak backend.

---

## 2. Proses bisnis dari sisi pengguna

Petugas HR membuka daftar Surat Peringatan, memilih Status Berlaku, Kedaluwarsa, atau Dicabut lewat Banding, lalu layar meminta ulang data menggunakan kode status backend. Filter Tingkat tetap tersedia. Reset mengosongkan pencarian, Tingkat, dan Status. Bila backend mengirim kode status baru, kode asli tetap ditampilkan agar informasi tidak hilang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `relations-sanctions-view.jsx`
- `relations-constants.js`
- `employee-relation-service.js`
- `EmployeeDisciplinaryController.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hrd/relations/relations-sanctions-view.jsx` | Menambah state dan `FilterSelect` Status, meneruskan `status`, serta meresetnya bersama filter lain. |
| `src/lib/constants/hr/relations/relations-constants.js` | Menambah label `Expired`, `RevokedOnAppeal`, dan pilihan filter Status. |
| `src/lib/services/hr/employee-relation-service.js` | Menambah `status`/`statuses` serta serializer parameter berulang tanpa kurung siku. |
| `tests/unit/human-resource-package-b-relations.test.mjs` | Menguji label, fallback, dan serialisasi `statuses`. |
| `tests/e2e/human-resource-package-b.spec.mjs` | Menguji filter melalui Chromium headed dan menyimpan screenshot. |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `view → service → InstanceAxios`. Komponen existing `DataFilter`, `FilterSelect`, dan `HrStatusBadge` dipakai ulang. Tidak ada CSS inline baru, Tailwind, dependency, atau base component baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hrd/relations/relations-sanctions-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Wadah filter | `DataFilter` | `REUSE` | Filter baru berada pada komposisi yang sama. |
| Pilihan Status | `FilterSelect` | `REUSE` | Mendukung opsi terkontrol dan placeholder. |
| Label status tabel | `HrStatusBadge` | `REUSE` | Sudah menyediakan fallback kode asli. |
| Query daftar | service existing | `EXTEND` | Menambah parameter opt-in tanpa mengubah default. |

`UI GATE: PASS — 3 REUSE, 1 EXTEND opt-in, 0 COMPOSE, 0 NEW.`

```text
$ node C:/Users/Admin/.codex/rules/frontend/tools/check-css-usage.mjs src/components/view/hrd/relations/relations-sanctions-view.jsx
OK — 1 berkas diperiksa, 0 temuan.
$ rg -n "className=.*(?:bg-|text-|p-|m-|flex|grid)" src/components/view/hrd/relations/relations-sanctions-view.jsx
(tidak ada class Tailwind baru)
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hrd/relations/sanctions` | `QuilvianSystemFrontendDev/test-results/human-resource-package-b/FE-HRD-395-status-filter.png` | Filter Status menyatu dengan filter Tingkat existing. |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | DataFilter dan DataTable memakai state loading existing. |
| Kosong | DataTable menampilkan keadaan kosong existing. |
| Gagal | Alert "Gagal memuat daftar SP." tetap tersedia. |
| Tanpa hak akses | Daftar tidak diminta/tampil tanpa `EmployeeDisciplinary : ViewSanctions`. |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Employee Disciplinary & Just Culture

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/disciplinary/sanctions?status={status}&statuses={status}` | Daftar dan filter sanksi | `EmployeeDisciplinary : ViewSanctions` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit code `0` | `PASS` | ESLint seluruh repository |
| `npm run build` | Exit code `0`; 890 halaman statis | `PASS` | Next.js production build dan standalone preparation |
| Unit test Paket B relations | 2/2 lulus | `PASS` | Label/fallback dan parameter berulang |
| Pemeriksa CSS | 0 temuan | `PASS` | Keluaran bagian 3.4 |
| Playwright Chromium headed | 1/1 lulus | `PASS` | Request `status=Active` teramati dan screenshot tersimpan |

Uji manual: `PASS` melalui browser headed terotomasi.

**Tidak dijalankan:** integrasi ke DB dev nyata; backend telah diverifikasi pada `BE-HRD-446`, sedangkan skenario frontend memakai response jaringan deterministik.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Status Berlaku mengirim `status=Active` | Terpenuhi | Playwright menangkap request aktual |
| Tiga status berlabel Indonesia dan status asing tampil apa adanya | Terpenuhi | Unit test label/fallback |
| Filter Tingkat termasuk Teguran Lisan tetap tersedia | Terpenuhi | Peta/filter Tingkat tidak diubah; unit source memuat `VerbalWarning` |
| `statuses` berupa parameter berulang tanpa `[]` | Terpenuhi | Unit test `URLSearchParams.getAll("statuses")` |
| Lint, build, screenshot, laporan, roadmap, traceability | Terpenuhi | Bagian 6 dan dokumen terkait |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `BE-HRD-446` ✅ menyediakan filter multi-status untuk task berikutnya. |
| Perubahan sampingan | Serializer reusable dipakai `FE-HRD-398` tanpa duplikasi. |
| Interupsi | `NONE` |
| Status Git | Perubahan Paket B belum di-commit; artefak laporan Playwright lokal berubah setelah uji. |
| Langkah berikutnya | Kerjakan `FE-HRD-396`; `FE-HRD-398` kini tidak lagi menunggu sisi frontend. |
