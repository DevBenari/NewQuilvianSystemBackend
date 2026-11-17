# Laporan Perubahan Frontend — `FE-AST-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-005` |
| Judul | Cetak label QR |
| Slice | `REG-FE-S2` — Fitur register |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-005` |
| Trace | `CAP-03.4`; `AST-DEC-010`, `AST-RULE-004`, `AST-OQ-016`; `SCR-AST-REG-13`, `17`; `03-frontend-architecture.md` §3.4, §3.7 |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** backend `BE-AST-008` (`4e71b13b`) ditambah tambalan daftar 2026-11-17 (bagian 1) |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian |
| Dependency | `FE-AST-004` ✅, `BE-AST-008` ✅ |
| Klasifikasi | `MEDIUM` — 1 layar baru, 1 tombol di layar detail, 1 layar pindai dialihkan; 4 berkas baru, 8 berkas diubah |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-17 ("commit dulu baru lanjut frontend"); tambalan backend sempit atas pilihan Rizki pada hari yang sama |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); backend hanya tambalan daftar (bagian 1); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `f46743f29` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `4e71b13b` (branch `rizkiG`) + tambalan belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ SELESAI 17 November 2026 — kedua kriteria terpetakan ke source, uji layar 22 langkah lulus dengan tiga aktor, ESLint `0 errors, 0 warnings`, build pemilik (frontend dan backend) berhasil |

---

## 1. Keadaan yang ditemukan di awal

- `BE-AST-008` menyediakan `POST /labels` (PDF) dan `GET /by-number/{nomor}`, tetapi belum ada layar untuk keduanya.
- §3.7 meminta saringan **tanggal disetujui** ("cetak label aset yang disetujui hari ini"). `GET /register/assets`
  tidak punya saringan itu, dan daftarnya tidak membawa tanggal disetujui. Pilihan diajukan ke Rizki; ia memilih
  **tambal backend dulu**. Tambalan: `ApprovedFrom`/`ApprovedTo` (tanggal WIB, diubah ke rentang UTC lewat
  `AppDateTimeHelper.OperationalDateToUtc`) pada `AssetPagedQuery`, `ApprovedAt` pada `AssetListItemResponse`,
  urutan `approvedAt`, dan entri metadata. +21 baris, QBE Strict `PASS` (2 berkas), nol komentar. Dicatat sebagai
  delta no. 12 di laporan `BE-AST-007`.
- Layar pindai nomor (`FE-AST-004`) masih mencari lewat daftar, sehingga aset unit lain tampil sebagai pesan
  gabungan "tidak ditemukan, atau bukan milik unit Anda". Laporan `FE-AST-004` menjadwalkan penggantiannya ke
  `/by-number` begitu `BE-AST-008` selesai.
- Laporan `FE-AST-004` juga mencatat tombol "Cetak label" di Detail Aset (§3.4) sebagai bagian task ini.

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Layar | Yang terjadi |
|---|---|---|---|
| 1 | Petugas Aset | Label Aset | Membuka menu Registrasi & Inventaris → **Label Aset**. Daftar memuat aset `Terdaftar`, terbaru disetujui di atas |
| 2 | Petugas Aset | Label Aset | Menyaring, misalnya kelompok Mebel dan "Disetujui dari/sampai" hari ini → 29 kursi yang baru disetujui |
| 3 | Petugas Aset | Label Aset | Mencentang per baris atau centang satu halaman. Pilihan tetap tersimpan saat pindah halaman atau mengganti saringan. Contoh uji: 25 kursi di halaman 1 + 1 kursi di halaman 2 → tombol **Cetak Label (26)** |
| 4 | Petugas Aset | Label Aset | Menekan **Cetak Label (26)** → tab baru terbuka berisi PDF dari backend; pesan "26 label dibuka di tab baru." |
| 5 | Petugas Aset | Label Aset | **Kosongkan Pilihan** untuk mulai pilihan baru |
| 6 | Petugas Aset | Detail Aset | Untuk satu aset `Terdaftar`: tombol **Cetak Label** di kepala halaman → PDF satu label di tab baru |
| 7 | Petugas lapangan | Pindai nomor | QR membuka `/register/assets/nomor/<nomor>` → `GET /by-number` → langsung ke Detail Aset |

**Jalur tidak normal**

| Kondisi | Perilaku |
|---|---|
| Memilih lebih dari 200 aset | Pilihan berhenti di 200; peringatan "Paling banyak 200 aset sekali cetak. Cetak dulu yang sudah dipilih, lalu lanjutkan." |
| Browser memblokir tab baru | PDF diunduh sebagai `label-aset-<waktu>.pdf`, dengan pesan penjelasan |
| Backend menolak (400/403/404/409) | Tab yang sudah dibuka ditutup lagi; pesan backend tampil, misalnya "Aset ini bukan milik unit Anda." |
| Nomor pindai tidak dikenal | "Nomor inventaris tidak ditemukan." |
| Nomor pindai milik unit lain | Layar Akses Ditolak dengan pesan "Aset ini bukan milik unit Anda." (403 dari `/by-number`) |
| Pengguna tanpa `ReadAllUnits` | Daftar Label Aset hanya berisi aset unitnya (dijaga backend) |

Tab baru dibuka **saat tombol ditekan**, sebelum PDF selesai dibuat, lalu diarahkan ke PDF. Dengan begitu browser
tidak menganggapnya popup tanpa klik, walaupun pembuatan 200 label butuh beberapa detik.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; referensi skill (gerbang keputusan base component,
checklist UI); layar `FE-AST-003`/`004` (hook daftar, kolom centang, detail, pindai, rute bertoken); base
`base-detail-view` (`renderDetailActions`, `afterHero`), `hero`, `data-filter` (`actions`), `filter-date-picker`
(`min`/`max`), `information-alert`, `base-button`, `pagination`; pola PDF blob di
`use-closed-billing-receivable-list.jsx` dan pembacaan galat blob di `opening-item-batch-service.jsx`; pola rentang
tanggal di `journal-view.jsx`; kontrak 2.8, §3.4, §3.7; source `BE-AST-008`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/asset-management/register/use-asset-label-printer.jsx` | Baru — buka tab saat klik → `POST /labels` → arahkan tab ke PDF; cadangan unduh; pesan hasil. Dipakai layar Label Aset dan Detail Aset |
| `src/lib/hooks/corporate/asset-management/register/use-asset-label-print.jsx` | Baru — varian daftar `label` + pilihan lintas halaman (maks 200) + cetak |
| `src/components/view/corporate/asset-management/register/asset-label-view.jsx` | Baru — `SCR-AST-REG-13` |
| `src/app/corporate/asset-management/register/labels/page.jsx` | Baru — route tipis |
| `src/components/view/corporate/asset-management/register/asset-record-table-columns.jsx` | Diubah — `buildAssetLabelColumns` (centang, No. Inventaris, Nama, Kelompok, Unit, Tgl Perolehan, Disetujui, Aksi) |
| `src/components/view/corporate/asset-management/register/asset-detail-view.jsx` | Diubah — kepala halaman lewat `renderDetailActions` (Kembali, Cetak Label, Ubah Draft); pesan cetak di `afterHero` |
| `src/lib/hooks/corporate/asset-management/register/use-asset-detail.jsx` | Diubah — `canPrintLabel` (hanya `Terdaftar` bernomor), `printLabel` |
| `src/lib/hooks/corporate/asset-management/register/use-asset-number-lookup.jsx` | Diubah — `fetchAssetByNumber` (`/by-number`); 404 → "Nomor inventaris tidak ditemukan." |
| `src/lib/services/corporate/asset-management/asset-record-service.js` | Diubah — `fetchAssetByNumber`, `printAssetLabels` (`responseType: "blob"`, galat blob dibaca sebagai JSON) |
| `src/utils/corporate/asset-management/register/asset-record-utils.js` | Diubah — kunci saringan `approvedFrom`/`approvedTo`, `withJsonErrorBody`, `saveAssetFile`; `findExactInventoryNumber` dihapus (tidak dipakai lagi) |
| `src/lib/constants/corporate/asset-management/register/asset-record-constants.jsx` | Diubah — varian daftar `label`, `ASSET_LABEL_MAX_SELECTION`, `ASSET_LABEL_FILE_PREFIX` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — butir "Label Aset" (`RiQrCodeLine`, `AssetRegister : Read`) sesudah Nomor Inventaris |

Backend (tambalan yang disetujui; rincian di laporan `BE-AST-007` delta no. 12):
`Register/DTOs/AssetRecordDtos.cs`, `Register/Services/AssetRecordService.Query.cs`.

### 3.3 Kepatuhan arsitektur frontend

- Alur URL → `app` → `view` → `hook` → `service` → `InstanceAxios`. Tidak ada slice atau perubahan store.
- Layar Label Aset memakai ulang `useAssetInventoryList` lewat varian `label`; tidak ada hook daftar kedua.
- Nol baris komentar, nol inline style, tidak ada CSS baru, tidak ada komponen base baru atau base yang diubah.

`UI GATE: 10 elemen — REUSE 9, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Kepala layar | `Hero` | `hero.jsx`, dipakai `FE-AST-004` | REUSE | — |
| Saringan + cari + tombol aksi | `DataFilter` (`actions` array) | `data-filter.jsx` `normalizeActions` | REUSE | — |
| Saringan unit, kelompok | `AssetMasterResourceFilter` | dipakai `FE-AST-004` | REUSE | — |
| Saringan tanggal disetujui | `FilterDatePicker` (`min`, `max`) | `filter-date-picker.jsx`; pola `journal-view.jsx` | REUSE | — |
| Tabel + paginasi | `DataTable`, `Pagination` | `data-table.jsx` | REUSE | — |
| Cetak Label, Kosongkan Pilihan | `BaseButton` (`loading`, `loadingLabel`) | `base-button.jsx` | REUSE | — |
| Pesan hasil/galat | `InformationAlert` (`success`/`info`/`warning`/`danger`) | `information-alert.jsx` | REUSE | — |
| Aksi baris "Lihat detail" | `RowActionMenu` | dipakai `FE-AST-004` | REUSE | — |
| Tombol Cetak Label di Detail Aset | `BaseDetailView` `renderDetailActions` + `afterHero` | `base-detail-view.jsx`; dipakai `kiosk-device`, `nurse-view-bed-tablet` | REUSE | — |
| Kolom centang | `DataTable` + kolom `header`/`render` | `DataTable` tanpa prop pilih | COMPOSE | A — sama dengan keputusan `FE-AST-003` |

Keputusan kolom centang merujuk keputusan yang sama di `FE-AST-003` (opsi A: rangkai lewat definisi kolom; base
tidak berubah), sehingga tidak diajukan ulang.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
|---|---|
| Memuat | `DataTable` memuat; centang dan tombol cetak nonaktif selama memuat/mencetak |
| Kosong | "Belum ada aset terdaftar pada saringan ini." |
| Gagal muat | `InformationAlert` + `AccessDeniedGate` (403 → Akses Ditolak) |
| Belum ada pilihan | **Cetak Label (0)** dan **Kosongkan Pilihan** nonaktif |
| Mencetak | Tombol berlabel "Menyiapkan label..." |
| Batas 200 | Peringatan; pilihan tidak bertambah |
| Tab diblokir | Unduh otomatis + pesan |
| Galat backend | Tab ditutup; pesan backend |
| Rentang tanggal | "Disetujui dari" tidak dapat melewati "sampai" dan sebaliknya (`min`/`max`) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Asset Management / Register / Asset (`api/v1/corporate/asset-management/register/assets`)

| Layar | Endpoint |
|---|---|
| Label Aset | `GET /?recordStatus=Terdaftar&organizationUnitId=&groupId=&approvedFrom=&approvedTo=&search=&sortBy=approvedAt&sortDirection=desc` |
| Label Aset, Detail Aset | `POST /labels` (`{ "assetIds": [...] }`, `responseType: blob`) |
| Pindai nomor | `GET /by-number/{inventoryNumber}` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 12 berkas | `0 errors, 0 warnings` | `PASS` | Keluaran perintah |
| Grep anti-regresi checklist UI (inline style, `<button>`/`<table>`/`<select>` mentah, warna literal, `fw-`/`fs-` di view) dan grep komentar | Semua kosong | `PASS` | — |
| QBE Strict tambalan backend | 2 berkas, `VIOLATION: 0` | `PASS` | Keluaran perintah |
| `dotnet build` (tambalan backend) | Dijalankan Rizki dan berhasil; DLL 2026-11-17 11:01:36, sesudah suntingan 10:49:54 | `PASS` | `bin/Debug/net9.0`; permintaan `approvedFrom` teramati dan tersaring |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-17 11:04:19, tidak ada berkas aset yang lebih baru; manifest memuat `register/labels` | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-005-20261117/uji-label.mjs`) | **22 langkah sesuai harapan**; 0 galat halaman; 0 respons 5xx | `PASS` | `hasil-uji.jsonl`, 7 tangkapan layar |

Rincian uji layar (superadmin uji; penyetuju = akun "Uji Penyetuju Aset" dengan `ReadAllUnits` dicabut sementara;
tanpa hak = LOKET):

| Langkah | Hasil |
|---|---|
| Layar Label Aset | 200; permintaan `recordStatus=Terdaftar&sortBy=approvedAt&sortDirection=desc`; 25 baris; kolom centang, No, No. Inventaris, Nama Aset, Kelompok, Unit, Tgl Perolehan, Disetujui, Aksi; butir menu tampil; **Cetak Label (0)** nonaktif |
| Disetujui dari hari ini | `approvedFrom=2026-11-17`; 25 baris, semuanya kursi uji yang disetujui hari ini (aset 16 Nov tersaring keluar) |
| Disetujui sampai hari ini | `approvedFrom=2026-11-17&approvedTo=2026-11-17` |
| Atur ulang | Parameter kembali ke bawaan |
| Kelompok Mebel | `groupId=…`; 25 baris, semuanya Mebel |
| Pilihan lintas halaman | Centang halaman 1 → 25; halaman 2 belum tercentang; tambah 1 → 26; teks "26 dari paling banyak 200 aset terpilih" |
| **AC1** cetak | `POST /labels` 26 id → 200 `application/pdf`, 114.850 byte, diawali `%PDF`; tab baru beralamat `blob:`; pesan "26 label dibuka di tab baru." |
| Kosongkan Pilihan | **Cetak Label (0)**, nonaktif |
| Detail Aset → Cetak Label | `AST-MBL-000011`: `POST /labels` 1 id → 200 PDF; tab baru `blob:`; pesan "1 label dibuka di tab baru." |
| Pindai nomor | `GET /by-number/AST-MBL-000025` 200 → Detail Aset nomor itu |
| Pindai nomor tak dikenal | `by-number/AST-XXX-999999` 404 → "Nomor inventaris tidak ditemukan." |
| Pindai unit lain | Penyetuju tanpa `ReadAllUnits`: 403 → Akses Ditolak, "Aset ini bukan milik unit Anda." |
| Label Aset tanpa `ReadAllUnits` | 200, kosong ("Belum ada aset terdaftar pada saringan ini.") |
| Pemulihan izin | Policy penyetuju dikembalikan ke Read/Approve/ViewAmount/ReadAllUnits (200) |
| **AC2** LOKET | Layar Akses Ditolak; butir "Label Aset" tidak tampil |

Tiga putaran awal berhenti karena kesalahan skrip, bukan layar: (1) selector menu campuran yang tidak sah;
(2–3) tombol paginasi berupa `<a>`, bukan `<button>`. Diperiksa terpisah (`[aria-label]` di DOM; klik → permintaan
`pageNumber=2`), lalu skrip diperbaiki. Layar tidak diubah.

`MANUAL TEST: PASS` (otomatis lewat Playwright di browser sungguhan). `AUTOMATED TEST: SKIPPED (opsional) —
kebijakan tanpa unit test`.

**Data yang ditulis ke `QuilvianNewDevRizki`:** tidak ada data aset. Policy penyetuju diubah sementara lalu dipulihkan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pilih aset → PDF backend terbuka di tab baru | Terpenuhi | `use-asset-label-printer` + `asset-label-view`; uji AC1 (26 aset) dan Detail Aset (1 aset), tab `blob:` berisi PDF |
| 2. Butir menu "Label Aset" terdaftar | Terpenuhi | `menu-items.jsx`; tampil untuk pemegang `AssetRegister : Read`, tidak untuk LOKET |
| `DoD-FE` | Terpenuhi | Lint, build pemilik, uji layar, laporan, roadmap, traceability |

Rekap: **2 dari 2 kriteria terpenuhi** → `✅`. Ikut tertutup: catatan `FE-AST-004` tentang pindai nomor lewat
pencarian dan tombol "Cetak label" di Detail Aset.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (1) Ukuran stiker masih default `AST-OQ-016` (A4 3 × 7). (2) Pemindaian label tercetak dengan kamera belum diuji; layar pindai kamera milik `FE-AST-016`. (3) Saringan "Disetujui dari/sampai" memakai tanggal WIB dan hanya mencocokkan aset yang punya tanggal disetujui; bila impor data awal (`BE-AST-012`) tidak mengisi tanggal itu, aset impor hanya tampil saat saringan tanggal dikosongkan |
| Risiko | Tidak ada data uji baru |
| Perubahan sampingan | Logika pembuka rute tidak berubah; tombol kepala Detail Aset kini dirender lewat `renderDetailActions` dengan susunan yang sama (Kembali, lalu aksi). Dua berkas lain di working tree frontend (layar dokter IGD dan rawat inap) bukan milik task ini dan tidak di-commit |
| Status Git | Frontend `RizkiV2`: 8 berkas diubah, 4 berkas baru. Backend `rizkiG`: 2 berkas tambalan + dokumen |
| Langkah berikutnya | `BE-AST-009` (penyerahan dan penempatan) → `FE-AST-006` |
