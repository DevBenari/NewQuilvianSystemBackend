# Laporan Perubahan Frontend — `FE-HRD-413`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-413` |
| Judul | Layar HR Perubahan Shift |
| Slice | `A52-B` / `MVP-78` · `EPIC HRD-135` |
| Roadmap | `roadmap/frontend-roadmap.md`, kartu `FE-HRD-413` (amandemen `A52-B`) |
| Trace | `FR-PK-038`; `HRD-DEC-253`; `VR-PK-35`; `api-contract.md` 37.7; desain `03` 36.3 (`F-SC`) |
| Contract version | `v31` — approved 24 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Susunan kolom dan kartu `DEV_DISCRETION` (kartu). Rute baru `/hr/scheduling/schedule-changes` sesuai desain 36.2/36.3. Komponen dasar: `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `DataTable`, `ConfirmModal`, `HrInfoGrid`, `HrPanel`, `StatusBadge`, `InformationAlert`, `ToastStack` |
| Dependency | `FE-HRD-410` ✅ (fungsi service); `BE-HRD-465` ✅ (cakupan unit dan `VR-PK-35`) |
| Klasifikasi | `MEDIUM` — satu rute dan view baru, satu utilitas baru, konstanta diperluas; uji unit dan spec E2E baru; tanpa service, slice, atau entri sidebar baru |
| Task mode | `FRONTEND` (instruksi pengguna 27 November 2026: "kerjakan FE-HRD-413") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/scheduling/schedule-changes`, `src/components/view/hr/scheduling`, `src/utils/hr/scheduling`, `src/lib/constants/hr/scheduling`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + perubahan `FE-HRD-410` s.d. `412` yang belum di-commit + perubahan Lab milik sesi lain (tidak disentuh) |
| Commit backend yang dijadikan rujukan | `cce1dbf5` (branch `yoga`) + perubahan `BE-HRD-467`/`468` yang belum di-commit |
| Tanggal | 27 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar HR untuk Perubahan Shift | Pencarian `schedule-change` di `src/components`, `src/app`, `src/lib/constants` kosong; service sudah ada dari `FE-HRD-410` | Pengajuan yang sudah disetujui tidak bisa diterapkan dari sisi HR lewat layar |
| Metadata server hanya berisi nilai, tanpa label | `ScheduleChangeFilterMetadataResponse`: `requestStatusOptions`, `requestTypeOptions` (string), `sortDirections`, `pageSizeOptions` | Label bahasa Indonesia dipetakan di frontend; daftar nilai tetap dari server |
| "Disetujui — belum diterapkan" | `ScheduleChangeService.ApplyAsync`: hanya `Approved` yang dapat diterapkan; sesudah diterapkan status menjadi `Applied` dan `IsAppliedToRoster = true` | Filter "Disetujui — belum diterapkan" = `requestStatus=Approved` |
| Filter unit tidak ada di backend | `ScheduleChangeController.GetPaged`: `workforceProfileId`, `requestStatus`, `requestType`, `startDate`, `endDate` (atas `EffectiveStartDate`), `search`, `sortDirection`, `pageNumber`, `pageSize` — tanpa unit | Filter unit di kartu/desain tidak dapat dipasang tanpa menebak parameter (lihat delta) |
| `pageSize` dibatasi | `PageSizes = {10, 25, 50, 100}`; nilai lain diganti 25 | Layar memakai 25 |
| Ringkasan | `ScheduleChangeSummaryResponse`: `waitingApproval` = `Submitted` + `UnderReview` | Kartu gabungan itu tidak dapat dipakai sebagai filter satu status |
| `409` dan `403` Terapkan | `409` "Hanya pengajuan berstatus Approved yang dapat diterapkan." / "Current shift assignment atau requested shift tidak lagi tersedia."; `403` "Anda tidak berwenang mengubah roster unit ini." (`VR-PK-35`) | Layar memilah `409` sebagai "sudah berubah, muat ulang" |
| Data devYoga | Baca-saja lewat backend lokal: ringkasan 0 di semua status, daftar 0 data | Uji layar data asli hanya dapat membuktikan metadata, filter, dan keadaan kosong |

---

## 2. Proses bisnis dari sisi pengguna

Contoh: Rina, perawat ICU, mengajukan perubahan Pagi → Malam untuk 16 Februari 2027 lewat layanan mandiri. Pengajuan disetujui dr. Bima lewat workflow.

1. Kepala Unit ICU membuka **Perubahan Shift** (`/hr/scheduling/schedule-changes`). Kartu ringkasan menampilkan Total Pengajuan, Menunggu Persetujuan, Perlu Revisi, **Disetujui — belum diterapkan**, Diterapkan, Ditolak, dan Dibatalkan, khusus pengajuan dalam cakupan unitnya.
2. Kepala Unit mengklik kartu **Disetujui — belum diterapkan** (atau memilih status yang sama di filter). Tabel menampilkan pengajuan Rina dengan tombol **Terapkan**.
3. **Detail** menampilkan pegawai, jenis, tanggal berlaku, semula/diminta, waktu diajukan/disetujui, alasan, dan riwayat workflow ("01 Feb 2027 · Submit → Submitted · Rina", "02 Feb 2027 · Approve → Approved · dr. Bima — Setuju").
4. **Terapkan** membuka konfirmasi berisi nomor pengajuan, pegawai, tanggal, "Pagi → Malam", catatan opsional, dan petunjuk "Jangan menuliskan nama atau No. RM pasien". Selama mengirim, tombol Terapkan dan Batal nonaktif. Sesudah berhasil muncul "Pengajuan SCR-… diterapkan ke roster.", lalu daftar dan ringkasan dimuat ulang dan status berubah menjadi **Diterapkan**.

Jalur lain:

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Unit di luar cakupan kelola (`403`) | Pesan server "Anda tidak berwenang mengubah roster unit ini." di dialog; dapat dicoba lagi atau dibatalkan |
| Pengajuan berubah sejak layar dibuka (`409`) | "Pengajuan ini sudah berubah sejak layar dibuka. Muat ulang lalu periksa kembali." disertai pesan server. Terapkan dikunci; tombol **Muat Ulang** menutup dialog dan memuat ulang daftar serta ringkasan |
| Tanpa `ScheduleChangeRequest : Update` | Tombol Terapkan tidak tampil |
| Tanpa `ScheduleChangeRequest : Read` | "Anda tidak berwenang membuka daftar Perubahan Shift."; tidak ada permintaan daftar |
| Daftar gagal dimuat | Pesan server + **Coba Lagi**; tabel tidak ditampilkan (galat bukan berarti kosong) |
| Tidak ada data | "Belum ada pengajuan perubahan shift." |
| Rentang tanggal terbalik | "Tanggal akhir tidak boleh sebelum tanggal awal."; permintaan tidak dikirim |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; kartu `FE-HRD-413`; kontrak 37.7 dan `VR-PK-35`; desain `03` 36.2–36.5; laporan `FE-HRD-410`; backend `ScheduleChangeController`, `ScheduleChangeService` (`GetFilterMetadata`, `BuildSummaryAsync`, `BuildPagedAsync`, `ApplyInScopeAsync`/`ApplyAsync`), `ScheduleChangeDtos`, `SchedulingRequestValueConstants`, `WorkflowEngineDtos` (`WorkflowInstanceDetailResponse`, `WorkflowStatusHistoryResponse`, `WorkflowApprovalActionResponse`); frontend `roster-scheduling-service.js`, `use-hr-remote-data.js`, `roster-revision-history-view.jsx` (pola layar), `summary-grid.jsx`, `data-filter.jsx`, `confirm-modal.jsx`, `menu-items.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/scheduling/schedule-changes/page.jsx` (baru) | Rute dan metadata halaman |
| `src/components/view/hr/scheduling/schedule-change-admin-view.jsx` (baru) | Layar: kartu ringkasan (klik = filter status), filter cari/status/jenis/berlaku dari–sampai, tabel berpaginasi (No. Pengajuan, Pegawai, Jenis, Berlaku, Semula → Diminta, Status, Aksi), modal detail + riwayat workflow, modal Terapkan dengan catatan opsional, pesan `403`/`409`, toast berhasil |
| `src/utils/hr/scheduling/schedule-change-admin-utils.js` (baru) | Pilihan filter dari metadata (tanpa duplikat; nilai tak dikenal tampil apa adanya), query daftar, validasi rentang, `canApplyScheduleChange` (`Approved` dan belum diterapkan), kartu ringkasan, ringkasan "semula → diminta", riwayat workflow (riwayat status urut nomor; cadangan tindakan persetujuan; `null` bila tanpa workflow), pemilah galat Terapkan |
| `src/lib/constants/hr/scheduling/scheduling-constants.js` | `SCHEDULING_ROUTES.scheduleChanges`; `SCHEDULE_CHANGE_STATUS` (label + nada badge) dan `SCHEDULE_CHANGE_TYPE_LABELS` |
| `tests/unit/hr-schedule-change-admin-utils.test.mjs` (baru) | 7 uji (AC1–AC3, ringkasan, label, riwayat workflow) |
| `tests/e2e/hr-schedule-change-admin.spec.mjs` (baru) | 7 uji Playwright API tiruan (AC1 ×2, AC2 ×2, AC3, AC4, detail) |

### 3.3 Kepatuhan arsitektur frontend

- Data dimuat lewat `useHrRemoteData` (pembatalan permintaan lama saat filter berubah) dan fungsi service `FE-HRD-410`; tidak ada service, slice, atau hook baru (desain 36.4: "Redux: tidak ada slice baru wajib").
- Hak akses lewat `useStrictPermissions`; tombol disembunyikan bila tidak berhak, dan server tetap memeriksa cakupan.
- Pesan galat dari server (`getHrApiErrorMessage`/`toHrApiError`). Satu-satunya kalimat buatan layar adalah petunjuk `409` yang diminta kartu, dan pesan server tetap ditampilkan di bawahnya.
- Nilai status/jenis tidak ditulis tetap sebagai pilihan; peta label hanya menerjemahkan nilai dari metadata.
- Alasan pengajuan dapat memuat kondisi kesehatan pegawai, jadi alasan tidak tampil di tabel dan hanya tampil di detail (pola `FE-HRD-308`).

### 3.4 Delta terhadap kartu dan kontrak

| Hal | Kartu/desain | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Filter unit | "filter status … dan tanggal/unit dari metadata"; desain 36.3 F-SC "unit" | Tidak dipasang | `GET schedule-change-requests` tidak menerima parameter unit, dan metadata tidak memuat pilihan unit. Server sudah membatasi daftar ke cakupan baca pengguna. Menambah parameter butuh perubahan backend/kontrak (usulan untuk pass blueprint) |
| `SummaryCards` | Kartu menyebut `SummaryCards` | `SummaryGrid` | Komponen dasar yang ada di repository adalah `SummaryGrid` (dipakai `FE-HRD-R38`) |
| Kartu "Menunggu Persetujuan" | — | Tidak dapat diklik | Menggabungkan dua status (`Submitted` + `UnderReview`), sedangkan daftar hanya menerima satu `requestStatus` |
| Filter tanggal | "tanggal" | "Berlaku dari/sampai" atas tanggal mulai berlaku | Server menyaring `EffectiveStartDate`; keterangan di bawah judul filter menyebutnya |
| Entri sidebar | — | Tidak ditambahkan | Roadmap menempatkan grup menu di `FE-HRD-324` sesudah gelombang 3. Sebelum itu layar dibuka lewat alamat `/hr/scheduling/schedule-changes` |
| Status kontrak 37.7 | "Rencana (belum tersedia)" | Tidak disunting | Artefak terkunci hash; dicatat untuk pass blueprint |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka kartu ringkasan; tabel "Memuat Perubahan Shift..."; pilihan filter nonaktif selama metadata dimuat; detail "Memuat detail pengajuan..." dan "Memuat riwayat workflow..." |
| Kosong | "Belum ada pengajuan perubahan shift." + keterangan cakupan unit |
| Gagal | Pesan server + **Coba Lagi** (tabel tidak ditampilkan); galat ringkasan, metadata, detail, dan workflow masing-masing tampil di tempatnya |
| Tanpa hak akses | Tanpa `Read`: pesan tidak berwenang, tanpa permintaan; tanpa `Update`: tanpa tombol Terapkan |
| Mengirim | Terapkan dan Batal nonaktif (`ConfirmModal` `loading`); klik ganda hanya mengirim satu permintaan |
| Basi (`409`) | Petunjuk muat ulang + pesan server; Terapkan dikunci; **Muat Ulang** |
| Validasi | Rentang tanggal terbalik → peringatan, tanpa permintaan; catatan maksimal 1000 karakter |
| Privasi | Alasan hanya di detail; petunjuk pada catatan penerapan |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Scheduling Management / Schedule Change Administration

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/filters/metadata` | Pilihan status dan jenis | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/summary` | Kartu ringkasan (cakupan baca) | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests` | Daftar: `requestStatus`, `requestType`, `startDate`, `endDate`, `search`, `sortDirection=desc`, `pageNumber`, `pageSize=25` | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/{id}` | Detail | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/{id}/workflow` | Riwayat workflow | `ScheduleChangeRequest : Read` |
| `POST` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/{id}/apply` | Terapkan `{ notes }` (cakupan kelola, `VR-PK-35`) | `ScheduleChangeRequest : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hr-schedule-change-admin-utils.test.mjs` | 7/7 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif unit: 13 mutasi (Terapkan tanpa cek `isAppliedToRoster`; Terapkan untuk semua yang belum diterapkan; `409` tidak dibedakan; `403` dianggap basi; pilihan tanpa dedupe; label status mentah; query tanpa halaman; query tanpa status; `pageSize` di luar daftar server; rentang tidak diperiksa; kartu gabungan dapat diklik; riwayat tanpa urut; tanpa workflow dianggap kosong) | 13/13 tertangkap; source pulih (hash `d8d41480946c` sama) | `PASS` | `mut413.cjs`, `mut413-hasil.log` di scratchpad sesi |
| E2E API tiruan `hr-schedule-change-admin.spec.mjs` (`next dev` 3711) | 7/7 lulus | `PASS` | Keluaran Playwright |
| Kontrol negatif E2E: 9 mutasi view (Terapkan tanpa cek hak; Terapkan untuk semua baris; daftar tidak dimuat ulang sesudah Terapkan; tombol tetap aktif saat mengirim; Muat Ulang tidak memuat ulang; galat tetap menampilkan tabel; alasan tampil di tabel; filter tidak mengembalikan halaman 1; tanpa hak Read tetap memuat) | 8/9 tertangkap. "Tombol tetap aktif saat mengirim" lolos karena setara dengan kode asli: `ConfirmModal` sudah menonaktifkan konfirmasi saat `loading` (`isBusy`). `applying ||` yang berlebih lalu dihapus dari `confirmProps`; uji klik ganda tetap membuktikan satu permintaan | `PASS` | `mut413e2e.cjs`, `mut413e2e-hasil.log` |
| Regresi E2E penjadwalan: spec baru + `hr-roster-replacement-grid` (4) + `hr-roster-revision-history` (8) | 19/19 lulus (diulang sesudah perubahan tata letak terakhir) | `PASS` | `e2e413-akhir.log` |
| Data asli devYoga (backend lokal 7184, GET diteruskan, izin disuapkan seperti Kepala Unit, POST dicegat) | Metadata asli → 8 pilihan berlabel; ringkasan 0 semua; daftar kosong → keadaan kosong; filter "Disetujui — belum diterapkan" mengirim `requestStatus=Approved&sortDirection=desc&pageNumber=1&pageSize=25`; tidak ada tulis | `PASS` (terbatas: devYoga tanpa pengajuan) | `uat413.mjs`, `uat413-hasil.log`, `uat413-daftar.png` |
| Pemeriksaan visual (tangkapan layar) | Data asli: kartu, filter, tabel kosong. Data tiruan: tabel berisi, modal detail dengan riwayat workflow, modal Terapkan. Temuan yang diperbaiki: kotak cari semula berada di antara "Berlaku dari" dan "Berlaku sampai" → dipindah ke depan (`searchPlacement="start"`) | `PASS` | `uat413-daftar.png`, `fe413-tabel.png`, `fe413-detail.png`, `fe413-terapkan.png` |
| `npx eslint` atas enam berkas yang diubah/baru | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` (seluruh repository) | Tanpa galat | `PASS` | `lint413.log` |
| `npm run build` | "Compiled successfully in 100s"; rute `○ /hr/scheduling/schedule-changes` | `PASS` | `build413.log` |
| Suite unit penuh | 5.465 lulus, 152 gagal dari 5.617. Jumlah gagal sama dengan baseline; tidak satu pun dari berkas task ini | `EXISTING / ENVIRONMENT ISSUE` | `unit413.log` |

**Uji manual:** lewat Playwright. Filter: daftar pilihan (8 status dari metadata), label terpilih, efek pada query dan hasil, filter gabungan (status + jenis), reset ke bawaan, paginasi (halaman 2, lalu filter kembali ke halaman 1), kartu sebagai filter, rentang tanggal terbalik. Modal: detail, Terapkan dari baris dan dari detail, catatan, klik ganda, `403`, `409` + Muat Ulang. Tangkapan layar diperiksa visual.

**Tidak dijalankan:** Terapkan sungguhan dan tampilan baris berisi dengan data asli. devYoga belum punya pengajuan perubahan shift, dan membuatnya berarti menulis data. Aturan `403` cakupan dan `409` dibuktikan backend di `BE-HRD-465`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Filter mengubah query dan hasil; reset mengembalikan keadaan bawaan; paginasi benar | Terpenuhi | Uji unit query/pilihan/rentang; E2E AC1 ×2 (status, jenis, gabungan, reset, halaman 2, kartu, tanggal); data asli: query `requestStatus=Approved` |
| 2. Terapkan hanya untuk status yang layak dan pemegang `Update`; tombol nonaktif saat mengirim; daftar dimuat ulang sesudahnya | Terpenuhi | Uji unit `canApplyScheduleChange`; E2E AC2 (hanya baris Approved, satu POST walau klik ganda, tombol nonaktif, daftar dan ringkasan dimuat ulang, baris menjadi Diterapkan) dan AC2 izin |
| 3. `403` cakupan → pesan server; `409` → "sudah berubah, muat ulang" | Terpenuhi | Uji unit pemilah galat; E2E AC3 |
| 4. Loading, kosong, galat + coba lagi | Terpenuhi | E2E AC4 (galat → Coba Lagi → kosong); data asli: keadaan kosong |
| DoD: AC terbukti, laporan `FE-HRD-413.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Suite unit penuh tetap memuat 152 kegagalan yang sudah ada di modul lain |
| Masalah yang diketahui | (1) Filter unit tidak tersedia di backend (delta 3.4). (2) Layar belum punya entri sidebar sampai `FE-HRD-324`. (3) Label nilai workflow di riwayat (mis. `Submit`, `Approved`) tampil apa adanya dari server |
| Dependency backend | Tidak ada yang tertunda untuk AC. Usulan untuk pass blueprint: parameter unit pada daftar Perubahan Shift bila filter unit tetap diinginkan |
| Perubahan sampingan | `NONE`. Spec tangkapan layar sementara `tests/e2e/zz-tmp-413-shot.spec.mjs` dibuat lalu dihapus. Playwright dijalankan dengan `--output` ke scratchpad; `test-results/` tidak berubah |
| Server | `next dev` 3711 dan backend 7184 dinyalakan task ini dan dihentikan sebelum build |
| Status Git | Berkas task ini: `M src/lib/constants/hr/scheduling/scheduling-constants.js`, `?? src/app/hr/scheduling/schedule-changes/page.jsx`, `?? src/components/view/hr/scheduling/schedule-change-admin-view.jsx`, `?? src/utils/hr/scheduling/schedule-change-admin-utils.js`, `?? tests/unit/hr-schedule-change-admin-utils.test.mjs`, `?? tests/e2e/hr-schedule-change-admin.spec.mjs`. Perubahan `FE-HRD-410` s.d. `412` dan Lab tidak disentuh. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `FE-HRD-414` (layar HR Tukar Dinas — pola layar ini dapat dipakai ulang) |
