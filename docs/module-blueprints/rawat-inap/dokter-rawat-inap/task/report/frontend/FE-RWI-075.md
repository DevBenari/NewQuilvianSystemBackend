# Laporan Perubahan Frontend — `FE-RWI-075`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-075` |
| Judul | Tab Visit (`FE-DOK-05`) disambungkan ke kerangka `FE-DOK-09` |
| Slice | Gelombang 2 — `DOK-MVP-FE-V2` |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-075` |
| Trace | `FE-DOK-05`; `03-frontend-architecture.md` §3.5, §10.2; `RWI-DEC-084`, `RWI-DEC-085`; `RWI-AC-150` s.d. `RWI-AC-156` |
| Contract version | `0.6.0` — tidak ada perubahan kontrak; endpoint visite dari `FE-RWI-047` dipertahankan apa adanya |
| Wewenang UI | `skema-tampilan-dokter-rawat-inap.md` §10; roadmap v2 kartu `FE-RWI-075` |
| Dependency | `FE-RWI-067` ✅ selesai; `FE-RWI-047` ✅ selesai (implementasi asli Tab Visit) |
| Klasifikasi | `LOW` — pemindahan tab yang sudah ada ke kerangka baru; satu perubahan import pada `doctor-inpatient-view.jsx`, satu unit test regresi baru. Tidak ada base component baru, tidak ada kontrak baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability-v2.md` sub-modul yang sama |
| Tanggal | 17 September 2026 |
| Status | ✅ `SELESAI`. Ketiga acceptance criteria terpetakan ke source yang ada dan seluruh validasi dijalankan |

---

## 1. Keadaan yang ditemukan di awal

Tab **Visit** sudah sepenuhnya diimplementasikan pada task `FE-RWI-047` (8 September 2026) dan telah
berfungsi pada kerangka `physician-workspace-view.jsx` (shell yang digunakan rute episode). Pada
`doctor-inpatient-view.jsx` (shell daftar pasien), tab visit juga sudah terhubung ke `PhysicianVisitTab`
dari task `FE-RWI-067`, tetapi tab tindakan dan resume masih memakai komponen lama atau placeholder.

Yang dikerjakan `FE-RWI-075` adalah **memastikan** tab visit tetap bekerja sepenuhnya di dalam
kerangka `FE-DOK-09` setelah pemindahan tab tindakan (`FE-RWI-073`) dan resume (`FE-RWI-074`),
dan menyelesaikan pembersihan placeholder resume yang tertinggal di `doctor-inpatient-view.jsx`.

---

## 2. Proses bisnis dari sisi pengguna

**Tidak berubah dari `FE-RWI-047`.** Dokter:

1. Membuka tab **Visit** pada ruang kerja dokter rawat inap.
2. Melihat **Riwayat Visite** berupa lini masa kejadian, terurut kronologis.
3. Menekan **Catat Visite** untuk mencatat kunjungan baru. Modal menampilkan isian Waktu Visite,
   Peran, Catatan, dan Tautkan Dokumen. Peringatan visite berdekatan ditampilkan bila ada, tetapi
   tidak pernah memblokir tombol (`RWI-DEC-085`).
4. Menekan **Batalkan** pada baris kejadian membuka modal pembatalan beralasan wajib. Kejadian yang
   dibatalkan tetap berdiri di riwayat, diredupkan, beserta alasan dan audit pembatal.
5. **Tidak ada tombol Sunting/Edit** — koreksi berbentuk batal lalu catat ulang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/doctor-inpatient-view.jsx` | Ubah | Mengganti `InpatientResumeTabPlaceholder` → `InpatientResumeTab`, `PrescriptionProcedureTab` → `InpatientProcedureTab` untuk tab `procedure` dan `resume`. Tab `visit` → `PhysicianVisitTab` sudah benar dari sebelumnya dan dipertahankan. Menghapus fungsi `InpatientResumeTabPlaceholder` yang tidak lagi diperlukan |
| `tests/unit/inpatient-physician-visit-regression.test.mjs` | Baru | 6 unit test regresi memvalidasi normalisasi visite, payload catat visite, peringatan berdekatan, status dibatalkan, anti-edit, dan koneksi tab ke kerangka `FE-DOK-09` |

### 3.2 Berkas yang TIDAK diubah (regresi nol)

| Berkas | Alasan tidak diubah |
| :--- | :--- |
| `physician-visit-tab.jsx` | Tab visit sudah lengkap dari `FE-RWI-047` |
| `physician-visit-timeline.jsx` | Komponen lini masa tidak berubah |
| `record-visit-modal.jsx` | Modal catat visite tidak berubah |
| `cancel-visit-modal.jsx` | Modal batal visite tidak berubah |
| `use-inpatient-physician-visit.jsx` | Hook pengelola state visite tidak berubah |
| `inpatient-physician-visit-utils.jsx` | Utilitas normalisasi, validasi, dan idempoten tidak berubah |
| `inpatient-physician-visit-constants.jsx` | Konstanta enum, label, dan teks UI tidak berubah |
| `physician-visit.service.js` | Service Axios endpoint visite tidak berubah — **AC-3 terpenuhi** |
| `physician-workspace-tabs.jsx` | Tab `visit: PhysicianVisitTab` sudah terhubung dari sebelumnya |

### 3.3 Tabel keputusan base component

| Elemen | Status | Alasan |
| :--- | :---: | :--- |
| Seluruh base component (ClinicalTimeline, ClinicalTimelineItem, ConfirmModal, dll.) | `REUSE` | Tidak ada base component baru; seluruhnya sudah dipakai dari `FE-RWI-047` |

**UI GATE**: Tidak ada elemen `NEW` atau `EXTEND` — gerbang keputusan terlewati tanpa eskalasi.

---

## 4. Peta acceptance criteria

| AC | Deskripsi | Bukti |
| :--- | :--- | :--- |
| AC-1 | Riwayat visit dan Catat Visit bekerja sama seperti sebelum pemindahan | `PhysicianVisitTab` dipetakan identik di `TAB_CONTENT` (`physician-workspace-tabs.jsx` baris 32) dan `TAB_COMPONENTS` (`doctor-inpatient-view.jsx` baris 38). Tidak ada satu pun baris source visit yang diubah. Unit test regresi `inpatient-physician-visit-regression.test.mjs` (6/6 PASS) memverifikasi normalisasi, payload, dan koneksi |
| AC-2 | Pembatalan visit yang salah tetap tersedia | `CancelVisitModal` tetap dirender oleh `PhysicianVisitTab`, menggunakan `ConfirmModal` dengan `requireReason`. `PhysicianVisitTimeline` tetap merender tombol **Batalkan** pada kejadian aktif. Unit test regresi memverifikasi kejadian dibatalkan tetap berdiri di riwayat beserta alasan |
| AC-3 | Tidak ada perubahan kontrak API pada tab ini | `physician-visit.service.js` **tidak diubah**. Endpoint `GET /physician-visits/episodes/{episodeId}`, `POST /physician-visits`, dan `PATCH /physician-visits/{id}/cancel` tetap dipertahankan apa adanya dari `FE-RWI-047`. Diff Git pada file service menunjukkan 0 baris berubah |

---

## 5. Hasil pengujian dan verifikasi

### A. Unit Testing (`tests/unit/inpatient-physician-visit-regression.test.mjs`)

```text
✔ AC-1: normalisasi daftar visite mempertahankan kejadian aktif dan urutan kronologis (6.475ms)
✔ AC-1: pembentukan payload catat visite dan kunci idempoten bekerja konsisten (1.031ms)
✔ AC-1: deteksi visite berdekatan memperingatkan tanpa memblokir tombol (0.295ms)
✔ AC-2: kejadian yang dibatalkan tetap berdiri di riwayat beserta alasan dan audit (0.198ms)
✔ AC-2 & AC-3: tab visit tidak memiliki antarmuka edit/sunting (hanya catat dan batalkan) (6.913ms)
✔ AC-1: kerangka FE-DOK-09 menghubungkan tab visit ke PhysicianVisitTab (1.331ms)
ℹ tests 6 | pass 6 | fail 0 | cancelled 0
```

### B. Lint Verification (`eslint`)

- **Hasil:** 0 error pada berkas yang terdampak `FE-RWI-075`.
- 1 warning pre-existing (`react-hooks/set-state-in-effect` pada `doctor-inpatient-view.jsx` baris 86) — milik `FE-RWI-067`, bukan regresi baru.

### C. Build Verification (`npm run build`)

- **Hasil:** Next.js 16.2.12 (Turbopack) production build sukses dengan exit code 0.

### D. Verifikasi Manual

- `MANUAL TEST: NOT FEASIBLE` — Aplikasi membutuhkan koneksi backend dan sesi dokter aktif. Regresi dibuktikan melalui fakta bahwa **nol baris source tab visit diubah** dan unit test regresi mencakup seluruh jalur utama.

---

## 6. Risiko dan isu terbuka

| # | Risiko / Isu | Dampak | Status |
| :--- | :--- | :--- | :--- |
| 1 | Dua isu dari `FE-RWI-047` tetap terbuka: hak Cancel konsulen dan mutasi event pada episode Closed | Dokter konsulen mungkin tidak dapat membatalkan visite yang dicatatnya; visite mungkin tetap bisa dicatat pada episode yang sudah ditutup | **Terbuka** — diteruskan dari `FE-RWI-047`, bukan regresi `FE-RWI-075` |

---

## 7. Dependency backend

Tidak ada dependency backend baru. Seluruh endpoint visite milik `BE-RWI-048` dan `BE-RWI-049` yang
sudah selesai sejak `FE-RWI-047`.
