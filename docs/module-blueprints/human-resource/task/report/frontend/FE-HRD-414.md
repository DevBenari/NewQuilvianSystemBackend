# Laporan Perubahan Frontend — `FE-HRD-414`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-414` |
| Judul | Layar HR Tukar Dinas |
| Slice | `A52-B` / `MVP-78` · `EPIC HRD-135` |
| Roadmap | `roadmap/frontend-roadmap.md`, kartu `FE-HRD-414` (amandemen `A52-B`) |
| Trace | `FR-PK-038`; `HRD-DEC-253`; `VR-PK-35`; `api-contract.md` 37.7; desain `03` 36.3 (`F-SC`: "menampilkan kedua pegawai dan kedua shift") |
| Contract version | `v31` — approved 24 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Pola `FE-HRD-413`; susunan kolom dan kartu `DEV_DISCRETION`. Rute baru `/hr/scheduling/shift-swaps` sesuai desain 36.2/36.3 |
| Dependency | `FE-HRD-413` ✅; `FE-HRD-410` ✅ (fungsi service); `BE-HRD-465` ✅ (cakupan unit dan `VR-PK-35`) |
| Klasifikasi | `MEDIUM` — rute, view, dan utilitas baru; konstanta diperluas; satu komponen kecil diekstrak dari view `FE-HRD-413`, dan utilitas `FE-HRD-413` diperumum tanpa mengubah perilakunya; uji unit dan spec E2E baru |
| Task mode | `FRONTEND` (instruksi pengguna 27 November 2026: "lanjutkan", sesudah `FE-HRD-413`) |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/scheduling/shift-swaps`, `src/components/view/hr/scheduling`, `src/utils/hr/scheduling`, `src/lib/constants/hr/scheduling`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + perubahan `FE-HRD-410` s.d. `413` yang belum di-commit + perubahan modul lain milik sesi lain (tidak disentuh) |
| Commit backend yang dijadikan rujukan | `cce1dbf5` (branch `yoga`) + perubahan `BE-HRD-467`/`468` yang belum di-commit |
| Tanggal | 27 November 2026 |
| Status | ✅ Selesai — acceptance criteria 1–4 (`FE-HRD-413`) untuk tukar dinas dan kriteria kedua pegawai/shift terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar HR Tukar Dinas | Service `getShiftSwap*`/`applyShiftSwapRequest` sudah ada (`FE-HRD-410`); belum ada rute, view, atau konstanta | Tukar dinas yang sudah disetujui tidak bisa diterapkan dari sisi HR lewat layar |
| Daftar tanpa filter jenis | `ShiftSwapController.GetPaged`: `requestStatus`, `startDate`, `endDate`, `search`, `sortDirection`, `pageNumber`, `pageSize` | Layar memakai filter status dan tanggal saja |
| Filter tanggal atas dua shift | `ShiftSwapService`: tukar dinas cocok bila tanggal shift pemohon **atau** rekan berada dalam rentang | Label filter "Tanggal shift dari/sampai" dan keterangan di bawah judul filter |
| Syarat Terapkan | `ApplyAsync`: `RequestStatus == Approved` **dan** `IsAcceptedByTarget == true`, lalu status menjadi `Applied`; selain itu `409` "Tukar shift hanya dapat diterapkan setelah target menerima dan workflow manager selesai." | Tombol Terapkan hanya untuk `Approved` yang sudah diterima rekan dan belum diterapkan |
| 10 nilai status | `Draft`, `PendingTarget`, `TargetAccepted`, `TargetRejected`, `PendingApproval`, `NeedRevision`, `Approved`, `Applied`, `Rejected`, `Cancelled` (metadata) | Label bahasa Indonesia untuk status rekan |
| Ringkasan | `waitingTarget` = `PendingTarget`; `waitingApproval` = `TargetAccepted` + `PendingApproval`; `rejected` = `Rejected` + `TargetRejected` | Dua kartu gabungan tidak dapat dipakai sebagai filter satu status |
| Utilitas dan panel `FE-HRD-413` | Kartu ringkasan, pemilah galat Terapkan, dan panel riwayat workflow tertulis khusus untuk Perubahan Shift | Diperumum seperlunya supaya dipakai ulang (kartu meminta "komponen dan utilitas `FE-HRD-413`") |
| Data devYoga | Baca-saja lewat backend lokal: ringkasan 0 di semua status, daftar 0 data | Uji layar data asli hanya membuktikan metadata, label, filter, dan keadaan kosong |

---

## 2. Proses bisnis dari sisi pengguna

Contoh: Rina (Pagi 16 Februari 2027) mengajukan tukar dinas dengan Tono (Malam 18 Februari 2027). Tono menerima, lalu dr. Bima menyetujui lewat workflow.

1. Kepala Unit membuka **Tukar Dinas** (`/hr/scheduling/shift-swaps`). Kartu ringkasan: Total, **Menunggu Tanggapan Rekan**, Menunggu Persetujuan, Perlu Revisi, **Disetujui — belum diterapkan**, Diterapkan, Ditolak, Dibatalkan.
2. Tabel menampilkan kedua pihak: kolom **Pemohon · Shift** ("Rina / 16 Feb 2027 · Pagi") dan **Rekan Tujuan · Shift** ("Tono / 18 Feb 2027 · Malam"), lalu **Tanggapan Rekan** ("Diterima", "Ditolak", atau "Belum menanggapi") dan status.
3. **Detail** menampilkan kedua pihak beserta shift, tanggapan dan catatan rekan, waktu diajukan/ditanggapi/disetujui, alasan, dan riwayat workflow.
4. **Terapkan** (hanya pada pengajuan yang disetujui dan sudah diterima rekan) membuka konfirmasi berisi kedua pihak dan catatan opsional: "Kedua shift langsung ditukar di roster, dan status pengajuan menjadi Diterapkan." Sesudah berhasil muncul "Tukar dinas SSR-… diterapkan ke roster.", lalu daftar dan ringkasan dimuat ulang.

Jalur lain sama dengan `FE-HRD-413`: `403` (salah satu unit di luar cakupan kelola) → pesan server; `409` → "Pengajuan ini sudah berubah sejak layar dibuka. Muat ulang lalu periksa kembali." + pesan server + **Muat Ulang**; tanpa `ShiftSwapRequest : Update` → tanpa Terapkan; tanpa `ShiftSwapRequest : Read` → pesan tidak berwenang (hak Perubahan Shift tidak membuka layar ini); galat → **Coba Lagi**; kosong → "Belum ada pengajuan tukar dinas."

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-414`; kontrak 37.7, `VR-PK-35`; desain 36.3; laporan `FE-HRD-413`; backend `ShiftSwapController`, `ShiftSwapService` (`GetFilterMetadata`, ringkasan, `BuildPagedAsync`, `ApplyAsync`), `ShiftSwapDtos`, `SchedulingRequestValueConstants.ShiftSwapStatus`; frontend `roster-scheduling-service.js`, berkas `FE-HRD-413`, `summary-grid.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/scheduling/shift-swaps/page.jsx` (baru) | Rute dan metadata halaman |
| `src/components/view/hr/scheduling/shift-swap-admin-view.jsx` (baru) | Layar Tukar Dinas dengan pola `FE-HRD-413`: kartu ringkasan, filter cari/status/tanggal shift, tabel kedua pihak, modal detail, modal Terapkan, pesan `403`/`409`, toast |
| `src/utils/hr/scheduling/shift-swap-admin-utils.js` (baru) | Pilihan status dari metadata, query (tanpa `requestType`), `canApplyShiftSwap` (`Approved` + `isAcceptedByTarget === true` + belum diterapkan), `describeSwapSide`, `describeTargetResponse`, definisi kartu, `classifyShiftSwapApplyError` |
| `src/components/view/hr/scheduling/schedule-request-workflow-history.jsx` (baru) | Panel Riwayat Workflow, diekstrak dari view `FE-HRD-413` dan dipakai kedua layar |
| `src/components/view/hr/scheduling/schedule-change-admin-view.jsx` | Memakai panel bersama; markup dan perilaku sama |
| `src/utils/hr/scheduling/schedule-change-admin-utils.js` | `buildRequestSummaryItems(cards, …)` umum (`buildScheduleChangeSummaryItems` tetap); `classifyApplyError(error, fallback)` menerima pesan cadangan (bawaan tetap "Gagal menerapkan perubahan shift.") |
| `src/lib/constants/hr/scheduling/scheduling-constants.js` | `SCHEDULING_ROUTES.shiftSwaps`; `SHIFT_SWAP_STATUS` (10 label + nada badge) |
| `tests/unit/hr-shift-swap-admin-utils.test.mjs` (baru) | 6 uji (AC1 ×2, AC2, AC3 termasuk cadangan pemilah umum, kedua pihak/tanggapan, kartu) |
| `tests/e2e/hr-shift-swap-admin.spec.mjs` (baru) | 6 uji Playwright API tiruan |

### 3.3 Kepatuhan arsitektur frontend

- Pola dan komponen dasar sama dengan `FE-HRD-413` (`useHrRemoteData`, `useStrictPermissions`, `SummaryGrid`, `DataFilter`, `DataTable`, `ConfirmModal`, `HrInfoGrid`). Tidak ada service, slice, atau hook baru.
- Bagian yang benar-benar sama (panel riwayat workflow, kartu ringkasan, rentang tanggal, pemilah galat Terapkan) dipakai ulang, tidak disalin. View tetap terpisah per layar, karena kolom, syarat Terapkan, dan teksnya berbeda.
- Nilai status selalu dari metadata server; peta hanya memberi label.
- Alasan pengajuan hanya tampil di detail (privasi), sama dengan `FE-HRD-413`.

### 3.4 Delta terhadap kartu dan kontrak

| Hal | Kartu/desain | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Filter unit | Desain 36.3 F-SC (pola `FE-HRD-413`) | Tidak dipasang | Sama dengan `FE-HRD-413`: daftar backend tanpa parameter unit; server membatasi ke cakupan baca |
| Filter jenis | Pola `FE-HRD-413` | Tidak ada | Tukar dinas tidak punya jenis (`ShiftSwapFilterMetadataResponse` tanpa `requestTypeOptions`) |
| Syarat Terapkan | AC2 "status yang layak" | `Approved` **dan** rekan menerima (`isAcceptedByTarget === true`) | Mengikuti `ShiftSwapService.ApplyAsync`; tanpa syarat ini server menolak `409` |
| Kartu "Ditolak" dan "Menunggu Persetujuan" | — | Tidak dapat diklik | Masing-masing menggabungkan dua status |
| Perubahan pada berkas `FE-HRD-413` | Cakupan kartu: rute, view | Panel workflow diekstrak dan dua utilitas diperumum | Kartu meminta reuse komponen/utilitas `FE-HRD-413`. Perilaku `FE-HRD-413` tidak berubah (7 uji unit + 7 E2E tetap lulus) |
| Entri sidebar | — | Tidak ditambahkan | Lingkup `FE-HRD-324` |

---

## 4. State yang ditangani di layar

Sama dengan `FE-HRD-413`: memuat (kerangka kartu, tabel, detail, workflow), kosong, galat + Coba Lagi (tabel disembunyikan), tanpa hak `Read`/`Update`, mengirim (Terapkan dan Batal nonaktif lewat `loading`), basi `409` (Terapkan dikunci + Muat Ulang), rentang tanggal terbalik (tanpa permintaan), catatan maks. 1000 karakter dengan petunjuk privasi.

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Scheduling Management / Shift Swap Administration

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/filters/metadata` | Pilihan status | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/summary` | Kartu ringkasan | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests` | Daftar: `requestStatus`, `startDate`, `endDate`, `search`, `sortDirection=desc`, `pageNumber`, `pageSize=25` | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/{id}` | Detail | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/{id}/workflow` | Riwayat workflow | `ShiftSwapRequest : Read` |
| `POST` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/{id}/apply` | Terapkan `{ notes }` (kedua unit dalam cakupan kelola, `VR-PK-35`) | `ShiftSwapRequest : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit `hr-shift-swap-admin-utils` + `hr-schedule-change-admin-utils` | 13/13 lulus (6 baru + 7 `FE-HRD-413`) | `PASS` | Keluaran perintah |
| Kontrol negatif unit: 15 mutasi (13 utilitas tukar dinas + 2 generalisasi `FE-HRD-413`) | 15/15 tertangkap. Pada putaran pertama "rekan longgar (nilai truthy)" lolos; uji ditambah kasus `isAcceptedByTarget: "true"` dan putaran ulang menangkapnya. Kedua berkas pulih (hash sama) | `PASS` | `mut414.cjs`, `mut414-hasil.log` |
| E2E API tiruan `hr-shift-swap-admin.spec.mjs` (`next dev` 3711) | 6/6 lulus | `PASS` | Keluaran Playwright |
| Kontrol negatif E2E: 11 mutasi view (Terapkan tanpa cek hak; untuk semua baris; kolom tujuan memakai pemohon; shift tidak tampil di tabel; daftar tidak dimuat ulang; Muat Ulang tidak memuat ulang; galat tetap menampilkan tabel; tanpa hak Read tetap memuat; hak Perubahan Shift membuka layar ini; detail tanpa sisi tujuan; alasan tampil di tabel) | 11/11 tertangkap; view pulih | `PASS` | `mut414e2e.cjs`, `mut414e2e-hasil.log` |
| Regresi E2E penjadwalan: tukar dinas (6) + perubahan shift (7) + kisi pengganti (4) + riwayat revisi (8) + roster terbit (10) | 35/35 lulus; ulangan akhir kedua layar 13/13 | `PASS` | `e2e414-akhir.log` |
| Data asli devYoga (backend lokal 7184, GET diteruskan, izin disuapkan, POST dicegat) | Metadata asli → 10 pilihan berlabel; ringkasan 0; daftar kosong → keadaan kosong; filter "Disetujui — belum diterapkan" → `requestStatus=Approved`; tidak ada tulis | `PASS` (terbatas: devYoga tanpa pengajuan) | `uat414.mjs`, `uat414-hasil.log`, `uat414-daftar.png` |
| Pemeriksaan visual | Tabel kedua pihak dan detail terbaca. Percobaan mengecilkan kartu (135 px) agar delapan kartu muat satu baris memotong label di tengah kata; dibatalkan, sehingga kartu kedelapan turun ke baris kedua (perilaku `SummaryGrid` bawaan) | `PASS` | `fe414-tabel.png`, `fe414-detail.png` |
| `npx eslint` atas sembilan berkas yang diubah/baru | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Tanpa galat | `PASS` | `lint414.log` |
| `npm run build` | "Compiled successfully in 93s"; rute `○ /hr/scheduling/shift-swaps` dan `○ /hr/scheduling/schedule-changes` | `PASS` | `build414.log` |
| Suite unit penuh | 5.586 lulus, 161 gagal dari 5.747. Dibanding run `FE-HRD-413` (152/5.617): 14 kegagalan baru, semuanya uji Payroll (PPh, pengesahan bersyarat, `PAY-*-FIX-034/038`) dari perubahan sesi lain di working tree; 5 kegagalan lama kini lulus. Tidak satu pun dari berkas task ini | `EXISTING / ENVIRONMENT ISSUE` | `unit414.log`, `f413.txt`/`f414.txt` |

**Uji manual:** lewat Playwright: daftar pilihan status (10), label terpilih, efek pada query dan hasil, filter gabungan (status + tanggal), rentang terbalik, reset, paginasi dan kembali ke halaman 1, kartu sebagai filter, kartu gabungan tidak dapat diklik; detail; Terapkan dari baris dan dari detail; klik ganda; `403`; `409` + Muat Ulang. Tangkapan layar diperiksa visual.

**Tidak dijalankan:** Terapkan sungguhan dan baris berisi dengan data asli, karena devYoga belum punya pengajuan tukar dinas dan membuatnya berarti menulis data. Aturan cakupan kedua unit (`403`) dibuktikan backend di `BE-HRD-465`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC1 `FE-HRD-413` untuk tukar dinas: filter mengubah query dan hasil; reset; paginasi | Terpenuhi | Uji unit query/pilihan; E2E AC1; data asli: query `requestStatus=Approved` |
| AC2: Terapkan hanya untuk status yang layak dan pemegang `ShiftSwapRequest : Update`; nonaktif saat mengirim; daftar dimuat ulang | Terpenuhi | Uji unit `canApplyShiftSwap`; E2E AC2 ×2 (termasuk baris Approved tanpa penerimaan rekan tanpa tombol) |
| AC3: `403` → pesan server; `409` → "sudah berubah, muat ulang" | Terpenuhi | Uji unit; E2E AC3 |
| AC4: loading, kosong, galat + coba lagi | Terpenuhi | E2E AC4; data asli: keadaan kosong |
| Kedua shift dan pegawai terbaca di tabel dan detail | Terpenuhi | E2E AC1 (tabel) dan Detail; mutasi "kolom tujuan memakai pemohon", "shift tidak tampil", "detail tanpa sisi tujuan" tertangkap |
| DoD: AC terbukti, laporan `FE-HRD-414.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Suite unit penuh kini 161 gagal karena uji Payroll dari sesi lain; baseline perlu diperbarui sesudah perubahan Payroll itu selesai |
| Masalah yang diketahui | Filter unit tidak tersedia di backend (sama dengan `FE-HRD-413`); belum ada entri sidebar sampai `FE-HRD-324`; delapan kartu ringkasan menempati dua baris pada lebar 1600 px |
| Dependency backend | Tidak ada yang tertunda untuk AC |
| Perubahan sampingan | `NONE`. Spec tangkapan layar sementara `tests/e2e/zz-tmp-414-shot.spec.mjs` dibuat lalu dihapus; `test-results/` tidak berubah |
| Server | `next dev` 3711 dan backend 7184 dinyalakan task ini dan dihentikan sebelum build |
| Status Git | Berkas task ini: `M src/lib/constants/hr/scheduling/scheduling-constants.js`, `M src/components/view/hr/scheduling/schedule-change-admin-view.jsx`, `M src/utils/hr/scheduling/schedule-change-admin-utils.js` (keduanya berkas baru `FE-HRD-413` yang belum di-commit), `?? src/app/hr/scheduling/shift-swaps/page.jsx`, `?? src/components/view/hr/scheduling/shift-swap-admin-view.jsx`, `?? src/components/view/hr/scheduling/schedule-request-workflow-history.jsx`, `?? src/utils/hr/scheduling/shift-swap-admin-utils.js`, `?? tests/unit/hr-shift-swap-admin-utils.test.mjs`, `?? tests/e2e/hr-shift-swap-admin.spec.mjs`. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `FE-HRD-415` (layar Penugasan Tambahan / penempatan sementara) |
