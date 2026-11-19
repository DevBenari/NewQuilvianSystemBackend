# Laporan Perubahan Frontend — `FE-AST-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-006` |
| Judul | Penempatan dan berita acara penyerahan |
| Slice | `REG-FE-S2` — Fitur register |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-006` |
| Trace | `CAP-04.1`–`04.4`; `AST-DEC-044`, `AST-DEC-046`, `AST-DES-006`; `SCR-AST-REG-18..21`; `03-frontend-architecture.md` §3.1, §3.6; `02-module-map.md` §3.1 (grup "Penempatan & Tanggung Jawab Aset") |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** backend `BE-AST-009` (`f993e3ec`), termasuk delta yang tercatat di laporannya (`/filters/metadata`, `/summary`, `awaitingMyConfirmation`, `availableActions`) |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian |
| Dependency | `FE-AST-004` ✅, `BE-AST-009` ✅ |
| Klasifikasi | `HEAVY` — 6 route baru (4 butir menu, formulir serahkan, rincian), 1 aggregate ber-lifecycle dengan 4 aksi; 22 berkas baru, 5 berkas diubah; tanpa perubahan backend |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-17 ("lanjutkan FE-AST-006") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `b36232f6d` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `f993e3ec` (branch `rizkiG`) |
| Tanggal | 17 November 2026 |
| Status | ✅ SELESAI 17 November 2026 — keempat kriteria terpetakan ke source; uji layar Playwright 29 langkah lulus dengan empat aktor; ESLint 27 berkas `0 errors, 0 warnings`; build Rizki berhasil |

---

## 1. Keadaan yang ditemukan di awal

- `BE-AST-009` menyediakan 11 endpoint penyerahan dan penempatan, tetapi belum ada layar. Kepala Unit belum punya
  cara melihat atau mengonfirmasi penyerahan.
- Respons daftar dan rincian penyerahan sudah membawa `availableActions` (`confirm`, `reject`, `cancel`, `document`)
  dan `isAwaitingMyConfirmation`. Layar cukup mengikutinya; layar tidak menebak siapa penerima.
- Pilihan ruangan di master lokasi (`/locations/options`) sudah menerima `organizationUnitId`, tetapi resource
  pilihan `assetLocations` di frontend belum meneruskan saringan apa pun.
- Belum ada pemilih aset (`FE-AST-001` AC3 masih 🟡). Untuk formulir serahkan, pemilih dirangkai dari daftar aset
  (`GET /register/assets`) dengan saringan status dan penempatan; aset terkunci/hilang terlihat tetapi tidak dapat
  dipilih.
- Tidak ada perubahan backend yang dibutuhkan.

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Layar | Yang terjadi |
|---|---|---|---|
| 1 | Petugas Aset | Berita Acara Penyerahan | Menekan **+ Serahkan Aset** |
| 2 | Petugas Aset | Serahkan Aset | Memilih tujuan (**Penempatan Awal** / **Pindah Dalam Unit**), unit tujuan, ruangan (hanya ruangan milik unit itu), dan pegawai penanggung jawab; catatan opsional |
| 3 | Petugas Aset | Serahkan Aset | Mencentang aset di tabel bawah. Penempatan awal menampilkan aset `Terdaftar` yang belum ditempatkan; pindah dalam unit menampilkan aset yang sudah ditempatkan **di unit tujuan** beserta ruangan dan penanggung jawabnya sekarang. Pilihan tetap tersimpan saat mencari atau berpindah halaman, maksimal 200 |
| 4 | Petugas Aset | Serahkan Aset | Menekan **Serahkan n Aset** → nomor `AST-PNY-######` terbit → langsung ke rincian berstatus **Menunggu Konfirmasi** |
| 5 | Kepala Unit / penanggung jawab | Berita Acara Penyerahan | Kartu **Menunggu Konfirmasi Anda**, peringatan "n penyerahan menunggu konfirmasi Anda", tab **Menunggu Konfirmasi Anda (n)**, dan label "Menunggu konfirmasi Anda" di baris |
| 6a | Penerima | Rincian / menu baris | **Konfirmasi Terima** → modal menyebut jumlah aset, unit, ruangan, penanggung jawab → status **Dikonfirmasi**; penempatan aset berlaku |
| 6b | Penerima | Rincian / menu baris | **Tolak Terima** beralasan → **Ditolak**; penempatan tidak berubah |
| 6c | Penyerah | Rincian / menu baris | **Batalkan Penyerahan** beralasan sebelum dikonfirmasi → **Dibatalkan** |
| 7 | Siapa pun berhak baca | Rincian / menu baris | **Cetak Berita Acara** (hanya yang sudah dikonfirmasi) → PDF backend di tab baru |
| 8 | Siapa pun berhak baca | Penempatan Per Unit/Ruangan, Penanggung Jawab Aset, Riwayat Lokasi & Penggunaan | Melihat penempatan berjalan per unit → ruangan, tanggungan per pegawai, dan riwayat (berjalan/selesai, sumber); klik dua kali → Detail Aset |

Contoh dari uji: superadmin menyerahkan `AST-MBL-000008` dan `009` ke Instalasi Rawat Inap / Ruang Uji U9KOL A dengan
penanggung jawab Dimas Kurniawan (`AST-PNY-000010`). Dimas melihat penanda "menunggu konfirmasi Anda", menekan
Konfirmasi Terima, dan rincian berubah menjadi "Dikonfirmasi Oleh Dimas Kurniawan, Perawat IGD · 17 Nov 2026, 15.38".
Kedua kursi lalu muncul di Penempatan Per Unit/Ruangan dan di Riwayat sebagai "Berjalan · Penyerahan".

**Jalur tidak normal**

| Kondisi | Perilaku |
|---|---|
| Belum memilih aset | **Serahkan 0 Aset** nonaktif; bila dikirim lewat Enter, isian kosong ditandai merah dan muncul "Pilih 1 sampai 200 aset pada tabel di bawah." |
| Pindah dalam unit sebelum unit dipilih | Tabel kosong dengan kalimat "Pilih unit tujuan untuk menampilkan aset." |
| Ganti tujuan, atau ganti unit pada pindah dalam unit | Pilihan aset dikosongkan (kelayakannya berubah); ruangan dikosongkan saat unit berganti |
| Aset sedang dalam proses lain / hilang | Baris tampil dengan keterangan, misalnya "Sedang dalam proses penyerahan.", dan centangnya nonaktif |
| Backend menolak (`020`..`024`, `026`) | Pesan backend tampil di atas tabel; daftar aset dimuat ulang. `023` juga ditandai pada isian Ruangan |
| Tolak/batal tanpa alasan | Tombol konfirmasi modal nonaktif sampai alasan diisi |
| Aksi atas penyerahan yang sudah diputuskan orang lain (`409 AST-PLC-STATUS`) | Toast berisi pesan backend; daftar/rincian dimuat ulang |
| Bukan penerima yang dituju | Konfirmasi Terima/Tolak Terima tidak ditampilkan (mengikuti `availableActions` dan hak `Confirm`) |
| Tanpa `AssetPlacement : Read` | Grup menu tidak tampil; membuka URL → Akses Ditolak |
| Tanpa `AssetPlacement : Create` | Tombol **+ Serahkan Aset** tersembunyi; membuka `/create` → Akses Ditolak |
| Tab baru diblokir browser | Berita acara diunduh sebagai `berita-acara-AST-PNY-xxxxxx.pdf` dengan pesan penjelasan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md` dan referensi skill (gerbang keputusan base
component, pola komposisi halaman, checklist UI); layar `FE-AST-003`..`005` (hook daftar, kolom centang, rincian,
rute bertoken, pembuka PDF); base `data-filter` (tab, `aria-pressed`), `data-table` (`pagination`), `summary-grid`,
`row-action-menu`, `confirm-modal` (`requireReason`), `base-detail-view`, `base-grouped-editor-*` (`dependsOn`,
`filters`, `afterForm`), `access-denied-gate`; `use-select-resource` (`filterKeys`); resource `employees` dan
`organizationUnits` di registry pilihan HR; kontrak 2.9–2.10, §3.1, §3.6, `02-module-map` §3.1; source `BE-AST-009`
(`AssetPlacementService`, `.Query`, DTO, kedua controller, pesan galat).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/placement/asset-placement-constants.jsx` | Baru — endpoint, route, tab, kartu ringkasan, isian formulir, varian daftar penempatan, label sumber |
| `src/lib/services/corporate/asset-management/asset-placement-service.js` | Baru — 9 panggilan (daftar, ringkasan, rincian, serahkan, konfirmasi, tolak, batal, berita acara blob, penempatan, riwayat) |
| `src/utils/corporate/asset-management/placement/asset-placement-utils.js` | Baru — parameter daftar/penempatan/pemilih, validasi + payload formulir, keterangan aset tak dapat dipilih, format pelaku |
| `src/lib/hooks/corporate/asset-management/use-asset-pdf-tab.jsx` | Baru — buka tab saat klik → arahkan ke PDF → cadangan unduh. Dipakai label QR dan berita acara |
| `src/lib/hooks/corporate/asset-management/placement/use-asset-handover-route.jsx` | Baru — rute bertoken rincian, buat, daftar |
| `src/lib/hooks/corporate/asset-management/placement/use-asset-handover-actions.jsx` | Baru — konfirmasi, tolak, batal, cetak berita acara; satu sumber untuk daftar dan rincian |
| `src/lib/hooks/corporate/asset-management/placement/use-asset-handover-list.jsx` | Baru — `SCR-AST-REG-20` daftar + ringkasan + tab |
| `src/lib/hooks/corporate/asset-management/placement/use-asset-handover-detail.jsx` | Baru — rincian + muat ulang sesudah aksi |
| `src/lib/hooks/corporate/asset-management/placement/use-asset-handover-editor.jsx` | Baru — formulir serahkan + pemilih aset lintas halaman |
| `src/lib/hooks/corporate/asset-management/placement/use-asset-placement-list.jsx` | Baru — `SCR-AST-REG-18`, `19`, `21` (satu hook, tiga varian) |
| `src/components/view/corporate/asset-management/placement/asset-placement-table-columns.jsx` | Baru — kolom penyerahan, butir, pemilih, dan tiga varian penempatan |
| `src/components/view/corporate/asset-management/placement/asset-handover-action-modals.jsx` | Baru — modal konfirmasi dan modal beralasan |
| `src/components/view/corporate/asset-management/placement/asset-handover-list-view.jsx` | Baru |
| `src/components/view/corporate/asset-management/placement/asset-handover-detail-view.jsx` | Baru |
| `src/components/view/corporate/asset-management/placement/asset-handover-form-view.jsx` | Baru |
| `src/components/view/corporate/asset-management/placement/asset-placement-list-view.jsx` | Baru |
| `src/app/corporate/asset-management/placement/{units,custodians,history,handovers,handovers/create,handovers/[slug]}/page.jsx` | Baru — 6 route tipis |
| `src/lib/hooks/corporate/asset-management/register/use-asset-label-printer.jsx` | Diubah — memakai `use-asset-pdf-tab`; API dan pesannya tetap |
| `src/components/view/corporate/asset-management/register/asset-record-table-columns.jsx` | Diubah — 5 definisi kolom diekspor untuk dipakai ulang (isi tidak berubah) |
| `src/components/view/corporate/asset-management/master-data/shared/asset-master-resource-filter.jsx` | Diubah — prop `filters` diteruskan ke `useSelectResource` (ruangan per unit) |
| `src/lib/hooks/select/corporate/asset-management-select-resources.js` | Diubah — `assetLocations.filterKeys = ["organizationUnitId"]` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — grup "Penempatan & Tanggung Jawab Aset" + 4 butir (`AssetPlacement : Read`) |

### 3.3 Kepatuhan arsitektur frontend

- Alur URL → `app` → `view` → `hook` → `service` → `InstanceAxios`. Tidak ada slice atau perubahan store; data layar
  ini tidak dibagi lintas halaman.
- Aksi penyerahan ditulis sekali (`use-asset-handover-actions`) dan dipakai daftar maupun rincian. Tiga layar
  penempatan memakai satu hook dan satu view bervarian. Pembuka PDF ditarik dari `use-asset-label-printer` menjadi
  hook bersama, bukan disalin.
- Nol baris komentar, nol inline style, tidak ada CSS baru, tidak ada base component yang diubah.

`UI GATE: 15 elemen — REUSE 12, EXTEND 1, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Kepala layar | `Hero` / `BaseDetailView` | dipakai `FE-AST-003`..`005` | REUSE | — |
| Kartu "Menunggu Konfirmasi Anda" dan jumlah per status | `SummaryGrid` | dipakai `FE-AST-003` | REUSE | — |
| Peringatan menunggu konfirmasi, pesan hasil | `InformationAlert` | `information-alert.jsx` | REUSE | — |
| Tab status + "Menunggu Konfirmasi Anda (n)" | `DataFilter` `tabs` | dipakai `FE-AST-003` | REUSE | — |
| Saringan tujuan, jumlah baris | `FilterSelect` | dipakai `FE-AST-004` | REUSE | — |
| Saringan tanggal | `FilterDatePicker` (`min`/`max`) | dipakai `FE-AST-005` | REUSE | — |
| Saringan unit/kelompok/pegawai | `AssetMasterResourceFilter` | dipakai `FE-AST-004` | REUSE | — |
| Saringan ruangan per unit | `AssetMasterResourceFilter` + prop `filters` | komponen view modul (bukan base); `useSelectResource` sudah menerima `filters` | EXTEND | A (dipilih): teruskan `filters` — 1 baris, bawaan `undefined` tidak mengubah pemakaian lama. B: komponen saringan kedua khusus ruangan — duplikasi. Bukan base dan tidak mengubah perilaku bawaan, jadi tidak menunggu keputusan |
| Tabel + paginasi | `DataTable`, `Pagination` | dipakai `FE-AST-003`..`005` | REUSE | — |
| Status penyerahan + penanda di baris | `StatusBadge` | label di `asset-status-labels.js` (`handoverStatus`) | REUSE | — |
| Aksi baris | `RowActionMenu` | dipakai `FE-AST-003` | REUSE | — |
| Konfirmasi / tolak / batal beralasan | `ConfirmModal` (`requireReason`) | dipakai `FE-AST-003` | REUSE | — |
| Formulir tujuan | `BaseGroupedEditorView` (`dependsOn`, `filters`, `afterForm`) | dipakai `FE-AST-003` | REUSE | — |
| Kolom centang pemilih aset | `DataTable` + definisi kolom | keputusan `FE-AST-003` opsi A | COMPOSE | A — sama dengan `FE-AST-003`/`005` |
| Pemilih aset di bawah formulir | `BaseGroupedEditorView` `afterForm` + `DataFilter` + `DataTable` | slot `afterForm` | COMPOSE | A — tombol Serahkan di akhir alur (bawah tabel); B: tombol di formulir atas — pengguna harus menggulir balik. Dipilih A |

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
|---|---|
| Memuat | `DataTable` memuat; kartu ringkasan memuat; tab dan saringan nonaktif selama memuat |
| Kosong | Kalimat per layar, misalnya "Belum ada penyerahan pada saringan ini.", "Belum ada aset yang ditempatkan pada saringan ini." |
| Gagal muat | `InformationAlert` "…gagal dimuat."; 403 → `AccessDeniedGate` |
| Rincian di luar cakupan | 403 → Akses Ditolak dengan penjelasan siapa yang boleh membuka |
| Tautan rusak | "Tautan penyerahan tidak dapat dibuka. Buka ulang dari daftar berita acara penyerahan." |
| Kirim ganda | Tombol Serahkan/modal ber-`loading`; tombol aksi rincian nonaktif selama memuat ulang |
| Data basi (`409`) | Toast pesan backend + muat ulang |
| Ringkasan gagal | Kartu menampilkan "-"; daftar tetap jalan |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Asset Management / Register / Asset Handover (`…/register/handovers`)

| Layar | Endpoint |
|---|---|
| Berita Acara Penyerahan | `GET /?status=&awaitingMyConfirmation=&purpose=&targetOrganizationUnitId=&from=&to=&search=&sortDirection=desc`; `GET /summary` |
| Rincian | `GET /{id}` |
| Serahkan Aset | `POST /` (`purpose` angka, `targetOrganizationUnitId`, `targetLocationId`, `targetCustodianEmployeeId`, `assetIds`, `notes`) |
| Daftar, Rincian | `POST /{id}/confirm`; `POST /{id}/reject` dan `/cancel` (`{ "reason" }`); `GET /{id}/document` (`blob`) |

#### Corporate / Asset Management / Register / Asset Placement (`…/register/placements`)

| Layar | Endpoint |
|---|---|
| Penempatan Per Unit/Ruangan | `GET /?organizationUnitId=&locationId=&search=` |
| Penanggung Jawab Aset | `GET /?custodianEmployeeId=&organizationUnitId=&search=` |
| Riwayat Lokasi & Penggunaan | `GET /history?organizationUnitId=&locationId=&custodianEmployeeId=&from=&to=&sortDirection=desc` |

Pendukung: `GET /register/assets?recordStatus=Terdaftar&placementState=…&organizationUnitId=` (pemilih),
`GET /master-data/locations/options?organizationUnitId=`, opsi `organizationUnits` dan `employees` HR.

`GET /handovers/filters/metadata` tidak dipakai: pilihan status/tujuan sudah ada di `asset-status-labels.js` dengan
nilai yang sama.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 27 berkas (sebelum dan sesudah perbaikan tampilan) | `0 errors, 0 warnings` | `PASS` | Keluaran perintah |
| Grep komentar dan inline style pada berkas baru/diubah | Kosong | `PASS` | — |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-17 15:31:36; manifest memuat 6 route `placement` | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-006-20261117/uji-penyerahan.mjs`) | **29 langkah sesuai harapan**; 0 galat halaman; 0 respons 5xx (82 permintaan) | `PASS` | `hasil-uji.jsonl`, 12 tangkapan layar |
| Pemeriksaan tab (`periksa-tab.mjs`) | Hanya "Semua" `aria-pressed=true`; sorotan "Dikonfirmasi" di tangkapan layar 05 adalah hover kursor sisa klik "Masuk" (x 901–1162, y 580) | `PASS` | `05b-tab-tanpa-hover.png` |

Aktor: superadmin uji (penyerah, **bukan** penerima), Dimas Kurniawan / akun PERAWAT (penanggung jawab yang dituju),
akun "Uji Penyetuju Aset" (tanpa izin `AssetPlacement`), LOKET.

| Langkah | Hasil |
|---|---|
| Daftar penyerahan (superadmin) | 200; 8 baris; kolom No, No. Penyerahan, Tujuan, Unit / Ruangan Tujuan, Penanggung Jawab, Jumlah, Diserahkan, Status, Aksi; tab Semua / Menunggu Konfirmasi Anda (0) / Menunggu Konfirmasi (0) / Dikonfirmasi (4) / Ditolak (2) / Dibatalkan (2); grup menu tampil; **+ Serahkan Aset** tampil |
| Tab Dikonfirmasi | `status=Dikonfirmasi`; 4 baris, semuanya Dikonfirmasi |
| Saringan tujuan | `purpose=PindahDalamUnit`; 2 baris, semuanya Pindah Dalam Unit |
| Atur ulang | Parameter kembali ke bawaan; 8 baris |
| Cari nomor | `search=AST-PNY-000006`; 1 baris |
| Aksi baris penyerahan terkonfirmasi | Menu hanya "Lihat detail", "Cetak berita acara"; **AC4** `GET /document` 200, 43.717 byte `%PDF`, tab `blob:`, pesan "Berita acara AST-PNY-000006 dibuka di tab baru." |
| Formulir serahkan | Pemilih memanggil `recordStatus=Terdaftar&placementState=BelumDitempatkan`; 25 baris; Ruangan nonaktif sebelum unit; **Serahkan 0 Aset** nonaktif |
| Ruangan milik unit | Sesudah unit Instalasi Rawat Inap: `locations/options?organizationUnitId=c797…`; opsi hanya U94W3 A/B dan U9KOL A/B (ruang C milik IGD tidak muncul) |
| Pindah dalam unit | `placementState=Ditempatkan&organizationUnitId=c797…`; 6 baris; kolom Ruangan Saat Ini dan Penanggung Jawab Saat Ini muncul |
| Serahkan 2 aset | `AST-MBL-000008` dan `009` dipilih lewat dua pencarian berbeda → **Serahkan 2 Aset** → `POST` 200 `AST-PNY-000010` → rincian |
| Rincian sebagai penyerah | Status Menunggu Konfirmasi, 2 butir; **Batalkan Penyerahan** tampil; Konfirmasi Terima dan Cetak Berita Acara **tidak** tampil |
| Serahkan 1 aset ×2 | `AST-PNY-000011` (`010`), `AST-PNY-000012` (`011`) |
| Batalkan | Tombol modal nonaktif tanpa alasan; dengan alasan → `POST /cancel` 200; status Dibatalkan; alasan tampil; tombol Batalkan hilang |
| **AC2** penanda (Dimas) | `GET /summary` `awaitingMyConfirmation=2`; kartu "Menunggu Konfirmasi Anda 2" (tangkapan layar 05); peringatan "2 penyerahan menunggu konfirmasi Anda"; tab "Menunggu Konfirmasi Anda (2)" |
| Tab Menunggu Konfirmasi Anda | `awaitingMyConfirmation=true`; 2 baris, semuanya berlabel "Menunggu konfirmasi Anda" |
| **AC3** tolak dari menu baris | Menu `AST-PNY-000011`: Lihat detail, Konfirmasi terima, Tolak terima, Batalkan penyerahan; tombol modal nonaktif tanpa alasan; `POST /reject` 200; toast "Penyerahan Ditolak"; tab tersisa 1 baris |
| Rincian sebagai penerima | Peringatan "Penyerahan ini menunggu konfirmasi Anda…"; Konfirmasi Terima dan Tolak Terima tampil |
| **AC3** konfirmasi | Modal "2 aset pada AST-PNY-000010 akan ditempatkan di Instalasi Rawat Inap / Ruang Uji U9KOL A dengan penanggung jawab Dimas Kurniawan…"; `POST /confirm` 200; status Dikonfirmasi; "Dikonfirmasi Oleh Dimas Kurniawan, Perawat IGD ·"; **Cetak Berita Acara** muncul |
| **AC4** cetak dari rincian | `GET /document` 200, 43.549 byte `%PDF`, tab `blob:` |
| **AC1** `SCR-AST-REG-18` | Kolom No, Unit, Ruangan, No. Inventaris, Nama Aset, Penanggung Jawab, Kondisi, Sejak, Aksi; saring unit → 8 baris semua Instalasi Rawat Inap termasuk `000008`; pilihan ruangan memanggil `locations/options?organizationUnitId=c797…` dan hanya berisi 4 ruang unit itu; saring Ruang U9KOL A → 4 baris semuanya U9KOL A |
| **AC1** `SCR-AST-REG-19` | Saring penanggung jawab Dimas → `custodianEmployeeId=e99d…`; 8 baris, semuanya Dimas; kolom Penanggung Jawab di depan |
| **AC1** `SCR-AST-REG-21` | Riwayat 10 baris; "Berlaku dari" hari ini → `from=2026-11-17`; memuat `000008`; 8 berjalan, 2 selesai |
| Klik dua kali baris riwayat | Detail Aset `AST-MBL-000008` terbuka |
| LOKET | Akses Ditolak; grup menu tidak tampil; tanpa tombol Serahkan |
| Penyetuju (tanpa `AssetPlacement`) | `/handovers/create` → Akses Ditolak; grup menu tidak tampil |
| Regresi cetak label (`FE-AST-005`, sesudah pemindahan ke `use-asset-pdf-tab`) | `POST /labels` 200; tab `blob:`; pesan "1 label dibuka di tab baru." |

`MANUAL TEST: PASS` (otomatis lewat Playwright di browser sungguhan, ditambah pemeriksaan visual tangkapan layar).
`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`.

**Perbaikan tampilan sesudah uji (belum terlihat di build 15:31):** dari pemeriksaan tangkapan layar,
(1) tabel butir di rincian menampilkan "Menampilkan 0 sampai 0 dari 0 data" walau ada 2 baris → kini
`pagination={false}` (jumlah sudah tampil di kepala bagian); (2) Riwayat (11 kolom) dan daftar penyerahan terpotong di
kanan pada lebar 1600 px → Riwayat menggabungkan Unit/Ruangan dan Status/Sumber, daftar penyerahan dirapatkan lebar
minimumnya. ESLint lulus; perubahan hanya presentasi dan tidak menyentuh perilaku yang diuji. Rupa akhirnya perlu
dilihat sekali sesudah build berikutnya.

**Data yang ditulis ke `QuilvianNewDevRizki`** (lewat layar, melanjutkan izin uji modul Aset): `AST-PNY-000010`
(Dikonfirmasi; `AST-MBL-000008`/`009` kini Ditempatkan di Instalasi Rawat Inap / Ruang Uji U9KOL A, penanggung jawab
Dimas Kurniawan), `AST-PNY-000011` (Ditolak, `010`), `AST-PNY-000012` (Dibatalkan, `011`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `SCR-AST-REG-18..21` | Terpenuhi | `asset-placement-list-view` (varian `units`, `custodians`, `history`), `asset-handover-list-view`, `-form-view`, `-detail-view`; 4 butir menu; uji AC1 dan alur serahkan |
| 2. Kepala Unit melihat penanda "menunggu konfirmasi Anda" | Terpenuhi | Kartu, peringatan, tab, dan label baris dari `/summary` dan `isAwaitingMyConfirmation`; uji AC2 dengan penanggung jawab yang dituju. Varian Kepala Unit tujuan memakai jalur data yang sama (`HeadOfUnitIds` backend), tetapi DB dev belum punya penugasan manajerial aktif — sama dengan catatan `AT-REG-37` di `BE-AST-009` |
| 3. Konfirmasi/tolak hanya bagi penerima yang dituju | Terpenuhi | `use-asset-handover-actions.can` = `availableActions` ∩ hak `Confirm`; superadmin (bukan penerima) tidak melihat tombolnya, Dimas melihat dan berhasil memakainya |
| 4. Cetak berita acara | Terpenuhi | `use-asset-pdf-tab` + `downloadHandoverDocument`; dari menu baris dan rincian, hanya untuk Dikonfirmasi |
| `DoD-FE` | Terpenuhi | Lint, build pemilik, uji layar, laporan, roadmap, traceability |

Rekap: **4 dari 4 kriteria terpenuhi** → `✅`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (1) Di DB dev, akun Perawat (Dimas) juga memegang `AssetPlacement : Create/Cancel` dan hak seluruh unit, sehingga ia ikut melihat **+ Serahkan Aset** dan **Batalkan penyerahan**. Itu konfigurasi peran dev, bukan cacat layar: tombol mengikuti `availableActions` backend (`CanCancel` = hak seluruh unit atau penyerahnya) dan hak peran. Matriks izin menetapkan `Create`/`Cancel` hanya untuk Petugas/Kepala Bagian Aset. (2) Pilihan pegawai penanggung jawab memakai opsi pegawai HR; peran yang tidak punya hak baca opsi itu tidak dapat mengisi isian tersebut — dicek saat peran Petugas Aset disusun. (3) `AT-REG-38` (unit sedang opname) menunggu `BE-AST-021`; pesan `026` sudah ditampilkan apa adanya |
| Risiko | Data uji baru di DB dev (bagian 6) |
| Perubahan sampingan | `use-asset-label-printer` kini menumpang `use-asset-pdf-tab`, perilakunya diuji ulang. Lima kolom register diekspor tanpa perubahan isi. `FE-AST-001` AC3 (pilih-aset) tidak ditandai ulang di sini; pemilih di formulir serahkan dapat menjadi dasar penilaiannya |
| Status Git | Frontend `RizkiV2`: 5 berkas diubah, 22 berkas baru, belum di-commit. Backend: hanya dokumen ini dan baris status |
| Langkah berikutnya | Lihat sekali rupa tabel rincian dan Riwayat sesudah build berikutnya; lalu `BE-AST-010` → `FE-AST-007` (kondisi dan status) |
