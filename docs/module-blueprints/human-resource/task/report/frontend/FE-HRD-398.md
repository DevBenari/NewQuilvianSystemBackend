# Laporan Perubahan Frontend — `FE-HRD-398`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-398` |
| Judul | Layar Riwayat Penyelesaian |
| Slice | `S-PKB` / `MVP-73` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 30 |
| Trace | `FR-PKB-12`, `FR-PKB-13`; `HRD-DEC-234`, `HRD-ASM-94`; API 35.2; State 33.2; Permission 33.1 |
| Contract version | `v29`, disetujui 23 November 2026 |
| Wewang UI | Dua panel baca-saja dengan komposisi base component existing |
| Dependency | `FE-HRD-395` ✅; `BE-HRD-446` ✅ |
| Klasifikasi | `HEAVY` — route/view, dua endpoint, permission gate, pagination, browser test |
| Task mode | `CROSS-REPO` |
| Target tulis | Source frontend; laporan, roadmap, traceability HRD |
| Model | Codex GPT-5 |
| Commit frontend saat dikerjakan | `573b7429c967a3229fc651437a902c33eac3708f` |
| Commit backend yang dijadikan rujukan | `b837892ad499289e9dd7dcaf869d9090c50c9939` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Layar Sanksi dan Insiden tersedia terpisah, tetapi belum ada pintu baca-saja untuk kasus selesai. Serializer multi-status sudah diselesaikan `FE-HRD-395` dan backend `BE-HRD-446` menerima parameter `statuses` berulang.

---

## 2. Proses bisnis dari sisi pengguna

HR Relations membuka Riwayat Penyelesaian, mencari pegawai/nomor SP dan menyaring tingkat. Panel pertama memuat sanksi Kedaluwarsa atau Dicabut lewat Banding. Panel kedua memuat insiden yang selesai tanpa sanksi hanya bila akun memegang `ViewIncidents`. Tidak ada tombol ubah, terbitkan, atau tindakan lain.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- layar Sanksi dan Insiden existing
- `employee-relation-service.js`
- DTO/controller disiplin backend
- frontend architecture bagian 34.2

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/(dashboard)/hrd/relations/sanctions/history/page.jsx` | Route baru. |
| `relations-resolution-history-view.jsx` | Filter, dua panel/tabel, permission gate, pagination, state kosong/gagal. |
| `human-resource-package-b.spec.mjs` | Dua skenario headed dengan dan tanpa `ViewIncidents`. |

### 3.3 Kepatuhan arsitektur frontend

View memakai service existing dan komponen existing; tidak menambah dependency, CSS, atau aksi tulis.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `relations-sanctions-view.jsx` dan `relations-incidents-view.jsx`

| Elemen | Sumber | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header/filter | `Hero`, `DataFilter`, `FilterSelect` | `COMPOSE` | Filter bersama riwayat. |
| Panel/tabel | `HrPanel`, `DataTable`, `RegionPagination` | `COMPOSE` | Dua sumber baca-saja. |
| Status | `HrStatusBadge` dan peta existing | `REUSE` | Label kontrak sudah dikunci. |
| State | `InformationAlert` | `REUSE` | Kosong/gagal/akses. |

`UI GATE: PASS — 2 REUSE, 2 COMPOSE, 0 NEW.`

```text
$ node C:/Users/Admin/.codex/rules/frontend/tools/check-css-usage.mjs relations-resolution-history-view.jsx
OK — 1 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hrd/relations/sanctions/history` | `test-with-agy/hr-paket-b-1123/FE-HRD-398-riwayat-penyelesaian.png` | Dua panel tanpa kolom aksi. Screenshot dibuat ulang 23 November 2026 saat regresi seluruh spec Paket B (15/15, laporan `FE-HRD-400`); salinan lama di `test-results/` terhapus oleh run Playwright berikutnya. |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Pesan memuat per tabel. |
| Kosong seluruhnya | "Belum ada kasus yang selesai." |
| Gagal | "Data gagal dimuat." |
| Tanpa `ViewSanctions` | Peringatan akses; query sanksi tidak dikirim. |
| Tanpa `ViewIncidents` | Panel insiden tidak dirender dan query tidak dikirim. |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Employee Disciplinary & Just Culture

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/disciplinary/sanctions?statuses=Expired&statuses=RevokedOnAppeal` | Sanksi selesai | `EmployeeDisciplinary : ViewSanctions` |
| `GET` | `/v1/hrd/disciplinary/incidents?status=ResolvedNoSanction` | Insiden selesai tanpa sanksi | `EmployeeDisciplinary : ViewIncidents` |

---

## 6. Verifikasi

| Skenario/perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit code `0` | `PASS` | ESLint repository |
| `npm run build` | Exit code `0`; 893 halaman statis | `PASS` | Production build |
| Pemeriksa CSS | 1 berkas, 0 temuan | `PASS` | Bagian 3.4 |
| Playwright Chromium headed | 2/2 lulus | `PASS` | Data 2+1, no-action, repeated params, permission hide, screenshot |

Uji manual: `PASS` melalui Chromium headed terotomasi.

**Tidak dijalankan:** DB dev nyata; response dimock mengikuti DTO dan kontrak.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 2 sanksi dan 1 insiden tampil tanpa tombol aksi | Terpenuhi | Playwright headed; tidak ada header Aksi |
| Tanpa `ViewIncidents`, bagian insiden tidak dirender | Terpenuhi | Skenario headed kedua |
| Kosong menampilkan "Belum ada kasus yang selesai." | Terpenuhi | State gabungan view |
| `statuses` berulang | Terpenuhi | Request aktual diperiksa browser |
| Lint/build/screenshot/report/roadmap/traceability | Terpenuhi | Bagian 6 dan dokumen terkait |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | DTO insiden belum memuat nama/kode pegawai; UI memakai nama/kode bila tersedia dan fallback ID. |
| Masalah yang diketahui | Pencarian nama hanya dapat diteruskan ke endpoint Sanksi; endpoint Insiden hanya menyediakan `employeeId`. |
| Dependency backend | `BE-HRD-446` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan Paket B belum di-commit. |
| Langkah berikutnya | Kerjakan `FE-HRD-393` dan `FE-HRD-399`. |
