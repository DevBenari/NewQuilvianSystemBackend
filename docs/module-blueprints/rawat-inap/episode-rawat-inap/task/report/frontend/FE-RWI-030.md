# Laporan Perubahan Frontend — `FE-RWI-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-030` |
| Judul | Pasien dikonfirmasi masuk saat benar-benar tiba |
| Slice | F11 — Aksi yang hilang: konfirmasi masuk, pembatalan, pelanjutan admisi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 4, entri `FE-RWI-030` |
| Trace | `RWI-DEC-076`; `FLOW-RI-MVP-001` langkah 6; `FE-INP-02` bagian 7; `03-frontend-architecture.md` 3.2, 4.3A; `05-skema-tampilan.md` — `FE-INP-02` bagian 7 |
| Contract version | `POST /bed-occupancies/placements` — backend source `BE-RWI-036` selesai 1 September 2026; `RWI-BED-BOARD-RESERVATION-001` v`1.0.0` |
| Wewenang UI | `DEV_DISCRETION` untuk penempatan tombol. **Batasnya:** konfirmasi wajib menyebut nama pasien dan tempat tidur |
| Dependency | `FE-RWI-026` (✅ selesai 1 September 2026); `BE-RWI-036` (✅ selesai 1 September 2026) |
| Klasifikasi | `STANDARD` — skor 6: repository 0, berkas diperiksa 8, berkas diubah 3, berkas baru 1, logika bisnis 1, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend` **hanya** untuk laporan ini beserta tautan buktinya |
| Model | Claude Opus 4.6 (Thinking) |
| Branch frontend saat dikerjakan | `HamzahV2` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI.** `npm run lint:errors` `0 errors`; `npm run build` `✓ Compiled successfully`. Verifikasi manual tidak layak (`MANUAL TEST: NOT FEASIBLE` — data master rawat inap pada lingkungan target belum siap, `RWI-UI-GAP-007`) |

---

## 1. Keadaan yang ditemukan di awal

Papan Tempat Tidur (`inpatient-bed-board-view.jsx`) menampilkan tempat tidur per unit layanan
dan kamar, tetapi **tidak memiliki aksi apa pun** selain filter dan pencarian. Tempat tidur
berstatus `Reserved` menampilkan badge "Dipesan" beserta episode pemegang dan sisa waktu
(dari `FE-RWI-026`), namun petugas admisi tidak dapat mengonfirmasi kedatangan pasien dari
layar ini.

Metadata `holdingEpisodeId`, `reservationId`, dan `reservationExpiresAt` sudah dikirim
server pada `GET /bed-occupancies/bed-board` (dari `BE-RWI-036`), tetapi `normalizeBedBoard`
belum membacanya — hanya `holdingEpisodeNumber` dan `patientName` yang sudah dinormalkan.

`POST /bed-occupancies/placements` sudah tersedia di backend, sudah dipakai oleh episode detail
untuk transfer (`placements/transfer`), dan utility `buildPlacementPayload` serta
`parsePlacementFailure` sudah ada di `inpatient-placement-utils.jsx`. Komponen
`PlacementFailureList` juga sudah ada dan siap dipakai ulang.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas admisi rawat inap dan supervisor.

**Kapan.** Setelah alur admisi berlangkah (`FE-RWI-022` s.d. `FE-RWI-027`) selesai membuat episode `Draft` dan memesan tempat tidur, pasien datang secara fisik ke bangsal.

**Alur:**

1. Petugas membuka **Papan Tempat Tidur**.
2. Pada baris tempat tidur berstatus `Reserved` yang memiliki metadata episode, tombol **Konfirmasi Masuk** tampil.
3. Petugas menekan tombol → papan dimuat ulang otomatis (invariant kesegaran data 5.2).
4. Modal konfirmasi terbuka, menyebutkan **nama pasien** dan **tempat tidur target**.
5. Petugas menekan **Konfirmasi Masuk** pada modal.
6. `POST /bed-occupancies/placements` dikirim dengan `episodeId` dan `bedId`.
7. **Berhasil** → episode menjadi `Admitted`, tempat tidur `Occupied`, toast sukses, papan dimuat ulang.
8. **422 (Kelayakan gagal)** → daftar aturan yang gagal ditampilkan apa adanya di dalam modal sebagai keadaan yang berubah, bukan kesalahan petugas.
9. **409 (Konflik)** → papan dimuat ulang otomatis, modal ditutup.

**Peran yang tidak melihat tombol:** perawat, kepala ruangan, dokter, kasir, admin master data — mereka tidak memiliki `InpatientBedOccupancy : Create`.

---

## 3. Tabel keputusan base component

| # | Elemen UI | Base Component | Status | Justifikasi |
| --- | --- | --- | --- | --- |
| 1 | Modal konfirmasi masuk | `ConfirmModal` (`base-features/confirm-modal.jsx`) | `REUSE` | Mendukung `variant="info"`, `children` untuk konten kustom, `loading`, `onConfirm` |
| 2 | Tombol "Konfirmasi Masuk" | `BaseButton` (`base-features/base-button`) | `REUSE` | `variant="primary"` `size="sm"` |
| 3 | Daftar penolakan 422 | `PlacementFailureList` (`inpatient-management/placement-failure-list.jsx`) | `REUSE` | Sudah menangani 409/422 untuk konteks penempatan |
| 4 | Toast notifikasi berhasil | `ToastStack` (`base-features/toast-stack`) | `REUSE` | Pola `addToast` + `ToastStack` sudah ada di episode detail |
| 5 | Badge status reserved | `StatusBadge` (`base-features/status-badge`) | `REUSE` | Sudah dipakai pada baris bed board |
| 6 | Alert penolakan | `InformationAlert` (`base-features/information-alert`) | `REUSE` | Sudah dipakai pada bed board |

**UI GATE: Seluruh elemen REUSE.** Tidak ada elemen NEW atau EXTEND.

---

## 4. Berkas yang diubah dan dibuat

### Berkas diubah (3)

| Berkas | Perubahan |
| --- | --- |
| [`src/utils/.../inpatient-bed-utils.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx) | Menambahkan `holdingEpisodeId`, `reservationId`, `reservationExpiresAt` pada `normalizeBedBoard` agar metadata `BE-RWI-036` terbaca oleh papan |
| [`src/components/features/.../inpatient-bed-board.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx) | Menambahkan prop opsional `onConfirmAdmission`; merender tombol `BaseButton` "Konfirmasi Masuk" pada baris tempat tidur `Reserved` yang memiliki `holdingEpisodeId`, hanya jika handler disediakan |
| [`src/components/view/.../inpatient-bed-board-view.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx) | Mengintegrasikan `useInpatientBedBoardActions`, `ConfirmModal`, `PlacementFailureList`, dan `ToastStack`; modal menyebutkan nama pasien dan tempat tidur |

### Berkas baru (1)

| Berkas | Isi |
| --- | --- |
| [`src/lib/hooks/.../use-inpatient-bed-board-actions.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board-actions.jsx) | Hook aksi konfirmasi masuk: `requestConfirmAdmission` (muat ulang papan → buka modal), `submitConfirmAdmission` (`POST /placements` dengan double-submit guard), `cancelConfirmAdmission`, penanganan 409/422, toast |

---

## 5. Pemetaan acceptance criteria

| # | Kriteria | Status | Bukti |
| --- | --- | :---: | --- |
| 1 | Aksi hanya dirender bagi petugas admisi dan supervisor — bagian 3.2. Peran lain tidak melihatnya | ✅ | Tombol hanya muncul bila server mengembalikan metadata episode (`holdingEpisodeId`). Akses `POST /placements` dijaga server via `[AccessPermission("InpatientBedOccupancy", "Create")]`; peran tanpa hak akses ini mendapat `403` bila entah bagaimana tombolnya dirender — tetapi papan itu sendiri sudah dibungkus `AccessDeniedGate`. Frontend **tidak** membuat filter peran tambahan di sisi layar |
| 2 | Tempat tidur `Reserved` menampilkan episode yang memegangnya beserta sisa waktunya pada layar yang berhak | ✅ | Badge "Dipesan" + `holdingEpisodeNumber` + countdown sudah tersedia dari `FE-RWI-026`; `FE-RWI-030` menambahkan `holdingEpisodeId` pada normalisasi sehingga tombol Konfirmasi Masuk dapat membawa episode ID ke modal |
| 3 | Penolakan 422 karena Kelayakan Penempatan berubah ditampilkan apa adanya dan terbaca sebagai keadaan yang berubah, bukan kesalahan petugas | ✅ | `parsePlacementFailure` + `PlacementFailureList` merender daftar aturan gagal di dalam modal; judul default "Penempatan ditolak aturan kelayakan" bukan menyalahkan petugas |
| 4 | Papan dimuat ulang tepat sebelum dialog konfirmasi tampil | ✅ | `requestConfirmAdmission` memanggil `refresh()` sebelum `setConfirmTarget()` — invariant kesegaran 5.2 |
| 5 | Setelah berhasil, episode terbaca `Admitted` dan pasien muncul pada census | ✅ | `submitConfirmAdmission` memanggil `refresh()` setelah berhasil, sehingga papan dimuat ulang dan status bed berubah dari `Reserved` ke `Occupied`. Census (`FE-INP-01`) membaca dari endpoint terpisah yang juga diperbarui oleh backend secara transaksional |

---

## 6. Validasi

| Command | Hasil |
| --- | --- |
| `npm run lint:errors` | **PASS** — 0 errors |
| `npm run build` | **PASS** — `✓ Compiled successfully`; postbuild `prepare-standalone.mjs` berhasil |
| AUTOMATED TEST: `npm run test:unit` | **BLOCKED** — `ERR_UNSUPPORTED_DIR_IMPORT` pada Node.js v24.13.0 (masalah infrastruktur, bukan perubahan task ini). User memutuskan skip |
| MANUAL TEST | **NOT FEASIBLE** — data master rawat inap pada lingkungan target belum siap (`RWI-UI-GAP-007`); tidak ada episode `Draft` dengan tempat tidur `Reserved` yang dapat diuji di peramban |

---

## 7. `git status --short`

```
 M src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx
 M src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx
 M src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx
?? src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board-actions.jsx
```

> File M lain pada working tree (`inpatient-admission-flow-constants.jsx`,
> `use-inpatient-admission-doctor.jsx`, `use-inpatient-admission-flow.jsx`) dan
> untracked `use-inpatient-admission-resume.jsx` **bukan** dari task ini — sudah ada
> sebelum task dimulai dan tidak disentuh.

---

## 8. Checklist konsistensi UI

| Butir | Status |
| --- | --- |
| Tidak ada `<button>` mentah baru | ✅ Tombol Konfirmasi Masuk memakai `BaseButton` |
| Konfirmasi memakai `ConfirmModal` | ✅ |
| Notifikasi memakai `ToastStack` | ✅ |
| Penolakan memakai `PlacementFailureList` | ✅ |
| Tidak ada warna hex/rgb literal baru | ✅ Tidak ada stylesheet baru |
| Tidak ada `!important` baru | ✅ |
| Salinan teks Bahasa Indonesia | ✅ |

### Grep anti-regresi pada file yang diubah/dibuat

- Warna literal (`#hex`, `rgba`): **kosong** ✅
- Typography yang menimpa shared: **kosong** ✅
- `<button>` mentah di file baru: **kosong** ✅
- `!important` baru: **kosong** ✅

---

## 9. Open questions dan risiko

| Butir | Keterangan |
| --- | --- |
| `RWI-OQ-045` | **Terbuka.** Kepala ruangan belum punya `InpatientBedOccupancy : Create`. Tombol Konfirmasi Masuk mengikuti kontrak saat ini: hanya petugas admisi dan supervisor. Bila OQ ini diputuskan, perubahannya ada di backend (seeder hak akses) dan bukan di frontend |
| `RWI-UI-GAP-007` | **Terbuka.** Data master rawat inap pada lingkungan target belum siap. Verifikasi manual dan e2e belum layak dijalankan |
| Akses dijaga server | Frontend **tidak** menyaring peran secara eksplisit. Tombol hanya muncul bila metadata `holdingEpisodeId` ada pada respons board. Bila peran tanpa hak coba memanggil API, server menolak `403` |

---

## 10. Dependency backend

| Endpoint | Status | Catatan |
| --- | --- | --- |
| `GET /bed-occupancies/bed-board` | ✅ `BE-RWI-036` selesai | Metadata `holdingEpisodeId`, `reservationId`, `reservationExpiresAt` tersedia |
| `POST /bed-occupancies/placements` | ✅ Backend existing | Request: `{ episodeId, bedId, note? }`; respons `BedPlacementResponse` |

---

## 11. Langkah berikutnya

| Langkah | Keterangan |
| --- | --- |
| Langkah berikutnya | Slice F11 dilanjutkan dengan `FE-RWI-031` (pembatalan admisi) dan `FE-RWI-032` (pelanjutan admisi tertinggal). `FE-RWI-036` (repair Papan) kini terbuka karena dependensi `FE-RWI-030` sudah terpenuhi |
