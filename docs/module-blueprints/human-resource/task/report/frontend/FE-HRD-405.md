# Laporan Perubahan Frontend — `FE-HRD-405`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-405` |
| Nomor lama | `FE-HRD-397` — dinomori ulang saat merge `QuilvianIntegrationBackend` 23 November 2026; nomor lama kini milik amandemen `S-A7-V2` (lihat `blueprint-manifest.md` revisi `69`) |
| Judul | Layar Tes/Wawancara dua jalur |
| Slice | `S-PKB` / `MVP-75` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 31 |
| Trace | `FR-PKB-08`, `FR-PKB-09`, `FR-PKB-11`; `HRD-DEC-241`; API 36.4, 36.6; State 34.3 |
| Contract version | `v30`, disetujui 23 November 2026 |
| Wewang UI | Filter dipilih untuk memisahkan dua jalur; route detail existing dipertahankan |
| Dependency | `BE-HRD-455` ✅ (memuat `BE-HRD-454`) |
| Klasifikasi | `HEAVY` — route/view, dua jalur, detail guard, 403, browser test |
| Task mode | `CROSS-REPO` |
| Target tulis | Source frontend; laporan, roadmap, traceability HRD |
| Model | Codex GPT-5 |
| Commit frontend saat dikerjakan | `573b7429c967a3229fc651437a902c33eac3708f` |
| Commit backend yang dijadikan rujukan | `b837892ad499289e9dd7dcaf869d9090c50c9939` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Route detail HR dan klinis sudah ada, tetapi tidak ada worklist `/hr/recruitment/evaluations`. Workspace membuka data baris langsung tanpa mengambil detail terbaru, sehingga pesan 403 khusus backend tidak pernah terlihat pada tindakan membuka penilaian.

---

## 2. Proses bisnis dari sisi pengguna

Recruiter/asesor memilih Wawancara HR atau Uji Klinis/Teknis, mencari kandidat, dan dapat membatasi daftar ke Belum Dinilai. Backend otomatis membatasi daftar ke penilai yang login kecuali pemegang `ViewAll`. Tombol Buka mengambil detail terbaru; bila detail ditugaskan kepada orang lain, layar menampilkan pesan khusus. Veto klinis ditampilkan sebagai "Gagal Asesmen".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `CandidateEvaluationController.cs`, DTO, dan service backend
- `CandidateEvaluationWorkspace.jsx`
- route/detail HR dan klinis existing
- `recruitment-service.js`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/recruitment/evaluations/page.jsx` | Route worklist baru. |
| `evaluation-worklist-view.jsx` | Filter jalur/pending/cari, tabel, pagination, veto, open detail, dan state akses. |
| `CandidateEvaluationWorkspace.jsx` | Mengambil detail terbaru sebelum membuka form dan menampilkan galat 403 server. |
| `recruitment-constants.js` | Route daftar evaluasi. |
| `human-resource-package-b.spec.mjs` | Skenario headed dua jalur dan 403. |

### 3.3 Kepatuhan arsitektur frontend

Alur `page → view/workspace → RecruitmentService → InstanceAxios` dipertahankan. Route detail existing tidak diduplikasi. Semua komponen visual memakai base component/tokens existing.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/recruitment/candidates-directory-view.jsx`

| Elemen layar | Sumber | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header/filter | `Hero`, `DataFilter`, `FilterSelect` | `COMPOSE` | Dua jalur dan pending-only. |
| Daftar | `DataTable`, `RegionPagination`, badges | `COMPOSE` | Worklist HR baku. |
| Tindakan | `BaseButton`, route detail existing | `REUSE` | Tidak menyalin workspace penilaian. |
| Galat | `InformationAlert` | `REUSE` | Menampilkan pesan server khusus. |

`UI GATE: PASS — 4 REUSE, 2 COMPOSE, 0 NEW.`

```text
$ node C:/Users/Admin/.codex/rules/frontend/tools/check-css-usage.mjs <2 berkas JSX>
OK — 2 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hr/recruitment/evaluations` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-397-tes-wawancara.png` | Jalur klinis, veto, dan pesan 403 terlihat. Screenshot dibuat ulang 23 November 2026 saat regresi seluruh spec Paket B (15/15, laporan `FE-HRD-408`); salinan lama di `test-results/` terhapus oleh run Playwright berikutnya. |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat evaluasi..." |
| Kosong | "Belum ada uji yang ditugaskan kepada Anda." |
| Gagal | "Data gagal dimuat." atau pesan detail server. |
| Tanpa hak akses | "Anda belum punya hak untuk melihat evaluasi kandidat." |
| Detail milik penilai lain | "Evaluasi ini ditugaskan kepada penilai lain." |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Recruitment Management / Candidate Evaluation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations?track=...&pendingOnly=...` | Worklist dua jalur | `CandidateEvaluation : Read`; penuh dengan `ViewAll` |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations/{id}` | Membuka detail terbaru | `CandidateEvaluation : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit code `0` | `PASS` | ESLint repository |
| `npm run build` | Exit code `0`; 892 halaman statis | `PASS` | Production build |
| Pemeriksa CSS | 2 berkas, 0 temuan | `PASS` | Bagian 3.4 |
| Playwright Chromium headed | 1/1 lulus | `PASS` | `track=HR`, `track=CLINICAL`, veto, pesan 403, screenshot |

Uji manual: `PASS` melalui Chromium headed terotomasi.

**Tidak dijalankan:** DB dev nyata; respons role/endpoint dimock sesuai kontrak.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pemegang `ViewAll` dapat melihat evaluasi lintas penilai | Terpenuhi | Hak dikirim dalam browser test; daftar memuat penilai lain |
| Asesor tanpa uji mendapat pesan kosong yang dikunci | Terpenuhi | `emptyTitle` sesuai kontrak |
| Detail milik penilai lain menampilkan pesan 403 khusus | Terpenuhi | Playwright dan screenshot |
| Pemisah dua jalur berfungsi | Terpenuhi | Request `track=HR` dan `track=CLINICAL` teramati |
| Lint, build, screenshot, report, roadmap, traceability | Terpenuhi | Bagian 6 dan dokumen terkait |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | DTO list tidak memuat judul lowongan/candidateId; tombol Buka mengambil detail dahulu untuk memperoleh candidateId secara sah. |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `BE-HRD-455` ✅ |
| Perubahan sampingan | Workspace detail kini memvalidasi detail terbaru sebelum form dibuka. |
| Interupsi | `NONE` |
| Status Git | Perubahan Paket B belum di-commit. |
| Langkah berikutnya | Kerjakan `FE-HRD-406`. |
