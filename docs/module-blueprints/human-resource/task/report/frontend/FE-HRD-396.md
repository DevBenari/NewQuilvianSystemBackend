# Laporan Perubahan Frontend — `FE-HRD-396`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-396` |
| Judul | Layar Seleksi Administrasi |
| Slice | `S-PKB` / `MVP-73` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 30 |
| Trace | `FR-PKB-06`, `FR-PKB-07`; `HRD-DEC-236`, `HRD-DEC-076`; API 35.5; State 33.4 |
| Contract version | `v29`, disetujui 23 November 2026 |
| Wewenang UI | Komposisi worklist, filter, dan modal memakai base component existing |
| Dependency | Endpoint existing; tidak ada task backend baru |
| Klasifikasi | `HEAVY` — route/view baru, ekstraksi form reusable, hak akses, filter, aksi, dan browser test |
| Task mode | `CROSS-REPO` |
| Target tulis | Source frontend; laporan, roadmap, dan traceability HRD |
| Model | Codex GPT-5 |
| Commit frontend saat dikerjakan | `573b7429c967a3229fc651437a902c33eac3708f` |
| Commit backend yang dijadikan rujukan | `b837892ad499289e9dd7dcaf869d9090c50c9939` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Service daftar dan evaluasi penapisan serta form pada detail kandidat sudah ada, tetapi belum tersedia worklist `/hr/recruitment/screenings`. Recruiter harus membuka kandidat satu per satu dan tidak memiliki default worklist Menunggu.

---

## 2. Proses bisnis dari sisi pengguna

Recruiter membuka Seleksi Administrasi dan langsung melihat lamaran Menunggu. Daftar dapat dicari serta disaring menurut lowongan, status, dan hasil STR. Pemegang hak `Evaluate` memilih Periksa, mengisi hasil verifikasi/catatan, lalu menyimpan satu kali; tombol terkunci saat request berjalan. Status STR kedaluwarsa/tiada dan status Gugur terlihat per lamaran. Pengguna tanpa hak evaluasi tetap dapat membaca daftar tanpa tombol Periksa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `CandidateScreeningController.cs` dan `CandidateScreeningDtos.cs`
- `candidate-detail-screening-view.jsx`
- `recruitment-service.js` dan `recruitment-constants.js`
- base component daftar/filter/form HR

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/recruitment/screenings/page.jsx` | Route baru Seleksi Administrasi. |
| `src/components/view/hr/recruitment/screening-worklist-view.jsx` | Worklist, empat filter, state akses/muat/kosong/gagal, tabel, pagination, dan aksi evaluasi. |
| `candidate-screening-evaluation-fields.jsx` | Form hasil STR/catatan reusable. |
| `candidate-detail-screening-view.jsx` | Memakai form reusable yang sama. |
| `recruitment-constants.js` | Route serta opsi status/hasil STR. |
| `tests/e2e/human-resource-package-b.spec.mjs` | Skenario browser headed Paket B. |

### 3.3 Kepatuhan arsitektur frontend

Alur `page → view → recruitment-service → InstanceAxios` dipertahankan. Form existing diekstrak menjadi satu komponen bersama, bukan disalin. Tidak ada dependency atau base component baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/recruitment/candidates-directory-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header | `Hero` | `REUSE` | Pola halaman HR. |
| Filter | `DataFilter`, `FilterSelect`, `ResourceFilterSelect` | `COMPOSE` | Mendukung empat parameter kontrak. |
| Daftar | `DataTable`, `RegionPagination`, `HrStatusBadge` | `COMPOSE` | Sama dengan worklist HR existing. |
| Evaluasi | `ConfirmModal`, base form control | `REUSE` | Diekstrak dari detail kandidat. |
| Pesan | `InformationAlert`, `BaseButton` | `COMPOSE` | Pesan/tindakan dikunci blueprint. |

`UI GATE: PASS — 6 REUSE, 3 COMPOSE, 0 NEW base component.`

```text
$ node C:/Users/Admin/.codex/rules/frontend/tools/check-css-usage.mjs <3 berkas JSX>
OK — 3 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/recruitment/screenings` | `test-with-agy/hr-paket-b-1123/FE-HRD-396-seleksi-administrasi.png` | Komposisi dan token setara halaman daftar Rekrutmen existing. Screenshot dibuat ulang 23 November 2026 saat regresi seluruh spec Paket B (15/15, laporan `FE-HRD-400`); salinan lama di `test-results/` terhapus oleh run Playwright berikutnya. |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat seleksi administrasi..." pada tabel. |
| Kosong default | "Tidak ada lamaran yang menunggu penapisan." dan `Lihat semua`. |
| Kosong hasil filter | "Belum ada data pada saringan ini." |
| Gagal | "Data gagal dimuat." dan `Coba lagi`. |
| Tanpa hak akses | "Anda belum punya hak untuk melihat seleksi administrasi."; request daftar tidak dikirim. |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Recruitment Management / Candidate Screening

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/recruitment/screenings` | Daftar/filter per lamaran | `CandidateScreening : Read` |
| `POST` | `/api/v1/corporate/human-resource/recruitment/screenings/evaluate` | Menilai kelengkapan/STR | `CandidateScreening : Evaluate` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit code `0` | `PASS` | ESLint seluruh repository |
| `npm run build` | Exit code `0`; 891 halaman statis | `PASS` | Route baru masuk production build |
| Pemeriksa CSS | 3 berkas, 0 temuan | `PASS` | Keluaran bagian 3.4 |
| Playwright Chromium headed | 1/1 lulus | `PASS` | Default Pending, Semua Status, tombol/modal, screenshot |

Uji manual: `PASS` melalui browser headed terotomasi.

**Tidak dijalankan:** DB dev nyata; response API dimock deterministik untuk menguji kontrak UI dan request.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Default menampilkan IGD Pending; Semua menampilkan IGD dan ICU | Terpenuhi | Playwright headed menangkap query `status=Pending`, lalu query tanpa status |
| Lamaran klinis tanpa STR menampilkan Gugur dan hasil STR | Terpenuhi | Pemetaan `Rejected → Gugur`; kolom hasil memakai `StrMissing`/`StrExpiredAutoReject` |
| Tanpa hak `Evaluate`, tombol tidak tampil | Terpenuhi | Render tombol dijaga `canEvaluate` |
| Pesan kosong/gagal dan tindakan sesuai kontrak | Terpenuhi | State bagian 4 |
| Pencegahan kirim ganda | Terpenuhi | `useHrAction.running` dan `confirmProps.disabled` |
| Lint, build, screenshot, laporan, roadmap, traceability | Terpenuhi | Bagian 6 dan dokumen terkait |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | DTO list menyediakan `screeningDate`, bukan tanggal lamaran; kolom diberi nama "Tanggal Penapisan" agar tidak menyesatkan. |
| Masalah yang diketahui | `NONE` |
| Dependency backend | Endpoint existing sesuai API 35.5. |
| Perubahan sampingan | Form evaluasi existing diekstrak untuk dipakai dua layar. |
| Interupsi | `NONE` |
| Status Git | Perubahan Paket B belum di-commit. |
| Langkah berikutnya | Kerjakan `FE-HRD-397`. |
