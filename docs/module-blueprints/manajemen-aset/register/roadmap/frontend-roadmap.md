# Register Aset — Roadmap Frontend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` — task belum diberi wewenang implementasi |
| Blueprint / contract | `AST-BP-001` rev 1, `register`, `AST-REG-1.0.0` `approved` |
| Repository | `QuilvianSystemFrontendDev` (`RizkiV2`, `e49e8a81`) |
| Task | `FE-AST-001` s.d. `FE-AST-010` (10 task) |
| Handoff | `build-module-frontend`, satu task per pemanggilan, dengan wewenang `FRONTEND MODE` eksplisit. Rupa (tata letak, warna, ikon, tab/modal) `DEV_DISCRETION` dalam design token dan base component Quilvian |

**Legenda tanda status:** ✅ selesai dan terbukti · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

**Verifikasi baku frontend (`V-FE`)** — `rules/frontend/test-policy.md`: `npm run lint`; `npm run build`
(dijalankan pemilik secara bawaan, atau agent bila diperintahkan); verifikasi manual kontrol interaktif
pada skema layar `03-frontend-architecture.md` termasuk keadaan memuat/kosong/gagal; butir menu terjangkau;
tombol tanpa hak tersembunyi. Unit test opsional untuk logika murni (`tests/unit/*.test.mjs`).

**DoD baku frontend (`DoD-FE`)**: acceptance criteria terpetakan ke source; `V-FE` dicatat apa adanya
(`AUTOMATED TEST`/`MANUAL TEST`); laporan `task/report/frontend/<TASK-ID>.md`; roadmap dan traceability
diperbarui.

## Grafik Urutan Dependency

Ringkasan antar-slice (node = slice):

```text
REG-FE-S1 ─> REG-FE-S2
```

### Slice `REG-FE-S1` — Fondasi dan pencatatan

```text
                                    BE-AST-007 [BE] ✅ ─┐
                                                        │
BE-AST-006 [BE] ✅ ─> FE-AST-001 🟡 ─> FE-AST-002 ✅ ─┬─┴─> FE-AST-003 ✅ ─> FE-AST-004 ✅
                                                      │
                                                      │  BE-AST-012 [BE] ─┐
                                                      │                   │
                                                      └───────────────────┴─> FE-AST-009
```

### Slice `REG-FE-S2` — Fitur register

`FE-AST-004` muncul lagi sebagai sambungan dari grafik slice `REG-FE-S1`.

```text
               BE-AST-008 [BE] ✅ ─┐
                                  │
FE-AST-004 ✅┬─────────────────────┴─> FE-AST-005 ✅
            │
            │  BE-AST-009 [BE] ✅ ─┐
            │                     │
            ├─────────────────────┴─> FE-AST-006 ✅ ────────┐
            │                                               │
            │  BE-AST-010 [BE] ✅ ─┐                        │
            │                     │                         │
            ├─────────────────────┴─> FE-AST-007 ✅ ──────┐ │
            │                                             │ │
            │    BE-AST-011 [BE] ─┐                       │ │
            │                     │                       │ │
            └─────────────────────┴─> FE-AST-008          │ │
                                                          │ │
                                     BE-AST-013 [BE] ─────┴─┴─> FE-AST-010
```

`[BE]` = task backend pada `backend-roadmap.md` sub-modul ini, cermin baca-saja.

Pasangan prasyarat→task: **18** (7 pada `REG-FE-S1`, 11 pada `REG-FE-S2`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-AST-006` [BE] | `FE-AST-001` |
| 2 | `FE-AST-001` | `FE-AST-002` |
| 3 | `FE-AST-002` + `BE-AST-007` / `BE-AST-012` [BE] | `FE-AST-003`, `FE-AST-009` — boleh paralel |
| 4 | `FE-AST-003` | `FE-AST-004` |
| 5 | `FE-AST-004` + backend masing-masing | `FE-AST-005`, `FE-AST-006`, `FE-AST-007`, `FE-AST-008` — boleh paralel |
| 6 | `FE-AST-006`, `FE-AST-007`, `BE-AST-013` [BE] | `FE-AST-010` |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-AST-001` | Fondasi FE modul dan grup menu | `AST-FE-002`, `AST-OQ-015` | `02-module-map.md` §3.1 | `menu-items.jsx`, base-features | service klien, label status, pilih-aset, grup menu + butir Master | `BE-AST-006` | Kartu | `V-FE` | Letak grup menu `AST-OQ-015` | `DoD-FE` |
| `FE-AST-002` | Delapan layar master | `CAP-02.*`, `CAP-EXT-05` | API 2.1–2.7 | `data-table`, `base-editor-form` | `SCR-AST-REG-02..09` | `FE-AST-001` | Kartu | `V-FE` | — | `DoD-FE` |
| `FE-AST-003` | Pencatatan draft dan persetujuan nomor | `CAP-03.1..03.3`, `AST-DEC-043` | API 2.8 | `confirm-modal` | `SCR-AST-REG-10`, `12` + butir menu | `FE-AST-002`, `BE-AST-007` | Kartu | `V-FE`, `UAT-REG-01`, `02` | — | `DoD-FE` |
| `FE-AST-004` | Daftar inventaris, data perolehan, detail aset | `CAP-03.2`, `03.6`, `04.4` | API 2.8 | `base-detail-view` | `SCR-AST-REG-11`, `15`, `17` + butir menu | `FE-AST-003` | Kartu | `V-FE`, `UAT-REG-12` | — | `DoD-FE` |
| `FE-AST-005` | Cetak label QR | `CAP-03.4` | API 2.8 `/labels` | `react-to-print` / PDF backend | `SCR-AST-REG-13` | `FE-AST-004`, `BE-AST-008` | Kartu | `V-FE` | `AST-OQ-016` | `DoD-FE` |
| `FE-AST-006` | Penempatan dan berita acara penyerahan | `CAP-04.*` | API 2.9–2.10 | — | `SCR-AST-REG-18..21` | `FE-AST-004`, `BE-AST-009` | Kartu | `V-FE`, `UAT-REG-03`, `04` | — | `DoD-FE` |
| `FE-AST-007` | Kondisi dan status | `CAP-06.*` | API 2.11 | `UploadPhotoField.jsx` | `SCR-AST-REG-22..27` | `FE-AST-004`, `BE-AST-010` | Kartu | `V-FE`, `UAT-REG-05`, `06` | — | `DoD-FE` |
| `FE-AST-008` | Dokumen, pengamanan, pemanfaatan, menganggur | `CAP-03.5`, `08.*` | API 2.12–2.14 | — | `SCR-AST-REG-14`, `28..32` | `FE-AST-004`, `BE-AST-011` | Kartu | `V-FE`, `UAT-REG-07`, `08` | — | `DoD-FE` |
| `FE-AST-009` | Impor data awal | `CAP-EXT-01` | API 2.15 | — | `SCR-AST-REG-16` | `FE-AST-002`, `BE-AST-012` | Kartu | `V-FE`, `UAT-REG-09`, `10` | — | `DoD-FE` |
| `FE-AST-010` | Dashboard dan laporan register | `CAP-01.*`, `11.1`, `11.3`, `11.7` | API 2.16–2.17 | `summary-grid`, pustaka grafik | `SCR-AST-REG-01`, `33..35` | `FE-AST-006`, `FE-AST-007`, `BE-AST-013` | Kartu | `V-FE`, `UAT-REG-11` | — | `DoD-FE` |

## Kartu task

### 🟡 `FE-AST-001` — Fondasi frontend dan grup menu

| Field | Isi |
|---|---|
| **Status** | 🟡 **SEBAGIAN 16 November 2026.** Kriteria 1 (grup menu `subMenu` → `subItems` → 8 daun `pathname` ber-`requiredPermission`) dan 2 (`src/utils/corporate/asset-management/asset-status-labels.js`, 14 enum) terpetakan ke source dan teruji di layar (SuperAdmin dan LOKET). Kriteria 3 pilih-aset ⛔ **menunggu `BE-AST-007`** (`GET /register/assets` belum ada). `npx eslint --max-warnings=0` 39 berkas `0 errors, 0 warnings`; `npm run build` Rizki berhasil 16 November 2026 (`BUILD_ID` 14.20.20). Letak menu sementara di bawah Akuntansi sampai `AST-OQ-015` diputuskan. Laporan: [`FE-AST-001.md`](../task/report/frontend/FE-AST-001.md) |
| Cakupan | `src/services/corporate/asset-management/` (klien Axios per grup endpoint); `src/utils/asset-management/asset-status-labels.js`; `src/components/view/corporate/asset-management/asset-picker.jsx`; folder `src/app/corporate/asset-management/`; grup menu "Manajemen Aset" dan grup "Master Klasifikasi Aset" pada `menu-items.jsx` |
| Acceptance criteria | 1. Grup tingkat 0 memakai `subMenu`, grup tingkat 1 memakai `subItems`, daun `pathname` (`getChildrenFromItem`). 2. Label seluruh enum `register` dalam Bahasa Indonesia di satu berkas. 3. Pilih-aset menyaring aset terkunci/hilang/rusak berat sesuai parameter |

### ✅ `FE-AST-002` — Delapan layar master

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 16 November 2026.** Keempat acceptance criteria terpetakan ke source; uji layar Playwright 26 langkah lulus (7 daftar, cari, saringan flag/relasi, atur ulang, jumlah baris, rincian bertoken, galat `050`/`051`/`052` per isian, tambah→perbarui→nonaktifkan→hapus, dokumen wajib, pengaturan) dan akun tanpa hak → Akses Ditolak tanpa tombol. `npx eslint --max-warnings=0` `0 errors, 0 warnings`; `npm run build` Rizki berhasil (`BUILD_ID` 14.20.20; route `master-data/[feature]` dan `settings` ada di manifest). Tampilan kode terkunci belum teramati karena belum ada aset bernomor. Laporan: [`FE-AST-002.md`](../task/report/frontend/FE-AST-002.md) |
| Acceptance criteria | 1. `SCR-AST-REG-02..09` dengan bentuk daftar bersama dan formulir. 2. Pesan `AST-VAL-REG-050..056` tampil per isian. 3. Kode kelompok terkunci menampilkan alasan. 4. Tombol Tambah/Ubah tersembunyi tanpa hak `Create`/`Update` |

### ✅ `FE-AST-003` — Pencatatan draft dan persetujuan nomor

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 17 November 2026.** Layar Pencatatan Aset Baru (daftar "draft saya" + formulir buat/ubah, atribut medis bersyarat, Simpan Draft / Simpan & Ajukan) dan Nomor Inventaris (centang, Setujui Terpilih dengan hasil per aset, Setujui/Kembalikan per baris), grup menu "Registrasi & Inventaris". Kelima kriteria terpetakan ke source. `npx eslint --max-warnings=0` 16 berkas `0 errors, 0 warnings`; `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-17 08:58:57, 4 route `register`). Uji layar Playwright **24/24 langkah** dengan pengaju, penyetuju, dan LOKET; 0 galat halaman. Butuh tambalan daftar `BE-AST-007` (`createdByMe`, `SubmittedBy`, `RejectedAt`, `DecisionReason`), build Rizki berhasil. Belum di-commit. Catatan: kolom Aksi daftar draft perlu digeser pada lebar 1600 px. Laporan: [`FE-AST-003.md`](../task/report/frontend/FE-AST-003.md) |
| Acceptance criteria | 1. Skema `03-frontend-architecture.md` §3.2 dan §3.3. 2. Atribut medis tampil bila kelompok medis. 3. Persetujuan massal menampilkan hasil per aset. 4. Baris milik pengaju tidak dapat dicentang. 5. Butir menu "Pencatatan Aset Baru" dan "Nomor Inventaris" terdaftar |

### ✅ `FE-AST-004` — Daftar inventaris, data perolehan, detail aset

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 17 November 2026.** Layar Daftar Inventaris dan Data Perolehan (satu view, dua varian; tanpa rupiah), Detail Aset (Ringkasan, Nilai khusus `ViewAmount`, Linimasa berhalaman, 403 → "Aset ini bukan milik unit Anda."), route pindai `assets/nomor/[nomor]` (lewat pencarian sampai `BE-AST-008` menyediakan `/by-number`), "Lihat detail" di Pencatatan dan Nomor Inventaris, butir menu Data Perolehan + Daftar Inventaris. Keempat kriteria terpetakan ke source. `npx eslint --max-warnings=0` 22 berkas `0 errors, 0 warnings`; `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-17 09:32:22, 4 route baru). Uji layar Playwright **22/22 langkah** dengan superadmin, penyetuju berizin terbatas, dan LOKET; 0 galat halaman, 0 respons 5xx. Butuh tambalan daftar `BE-AST-007` (`ReadyForUseDate`, `SupplierName`, `GoodsReceiptNumber`, `DonationReference`), build Rizki berhasil. Laporan: [`FE-AST-004.md`](../task/report/frontend/FE-AST-004.md) |
| Acceptance criteria | 1. Skema §3.1 dan §3.4. 2. Bagian Nilai dirender hanya bila `/amounts` dapat dibaca. 3. 403 menampilkan "Aset ini bukan milik unit Anda." 4. Detail dapat dibuka dari baris tabel dan dari URL pindai |

### ✅ `FE-AST-005` — Cetak label QR

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 17 November 2026.** Layar Label Aset (`register/labels`: saringan unit, kelompok, tanggal disetujui dari/sampai; centang per baris/halaman, pilihan lintas halaman maks 200; **Cetak Label (n)** → PDF backend di tab baru, cadangan unduh bila diblokir), tombol Cetak Label di Detail Aset, layar pindai nomor beralih ke `GET /by-number` (403 unit lain tampil sebagai Akses Ditolak), butir menu "Label Aset". Kedua kriteria terpetakan ke source. `npx eslint --max-warnings=0` 12 berkas `0 errors, 0 warnings`; `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-17 11:04:19, route `register/labels`). Uji layar Playwright **22/22 langkah** dengan superadmin, penyetuju tanpa `ReadAllUnits`, dan LOKET; 0 galat halaman, 0 respons 5xx; cetak 26 aset lintas halaman → PDF 114.850 byte di tab `blob:`. Butuh tambalan daftar `BE-AST-007` (`approvedFrom`/`approvedTo`, `ApprovedAt`), build Rizki berhasil. Belum di-commit. Laporan: [`FE-AST-005.md`](../task/report/frontend/FE-AST-005.md) |
| Acceptance criteria | 1. Pilih aset → PDF backend terbuka di tab baru. 2. Butir menu "Label Aset" terdaftar |

### ✅ `FE-AST-006` — Penempatan dan berita acara penyerahan

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 17 November 2026.** Layar Berita Acara Penyerahan (kartu ringkasan, tab status + "Menunggu Konfirmasi Anda (n)", saringan tujuan/unit/tanggal, aksi baris sesuai `availableActions`), Serahkan Aset (tujuan, unit → ruangan milik unit → pegawai PJ; pemilih aset lintas halaman maks 200, aset terkunci/hilang tidak dapat dipilih), rincian (Konfirmasi Terima, Tolak Terima, Batalkan beralasan, Cetak Berita Acara), dan tiga layar penempatan (per unit/ruangan, penanggung jawab, riwayat); grup menu "Penempatan & Tanggung Jawab Aset" 4 butir. Keempat kriteria terpetakan ke source; tanpa perubahan backend. `npx eslint --max-warnings=0` 27 berkas `0 errors, 0 warnings`; `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-17 15:31:36, 6 route `placement`). Uji layar Playwright **29/29 langkah** dengan penyerah (superadmin), penerima (Dimas), penyetuju tanpa hak, dan LOKET; 0 galat halaman, 0 respons 5xx. Dua perbaikan tampilan sesudah uji (footer tabel butir, lebar kolom Riwayat/daftar) lulus lint, rupanya dilihat sesudah build berikutnya. Penanda untuk Kepala Unit tujuan memakai jalur data yang sama, tetapi DB dev belum punya penugasan manajerial aktif. Belum di-commit. Laporan: [`FE-AST-006.md`](../task/report/frontend/FE-AST-006.md) |
| Acceptance criteria | 1. `SCR-AST-REG-18..21`. 2. Kepala Unit melihat penanda "menunggu konfirmasi Anda". 3. Konfirmasi/tolak hanya bagi penerima yang dituju. 4. Cetak berita acara |

### ✅ `FE-AST-007` — Kondisi dan status

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 18 November 2026.** Pemeriksaan Kondisi (kartu, tab "Menunggu Penetapan"/"Menunggu Persetujuan Kehilangan", Tetapkan/Tolak/Setujui Hilang/Ditemukan Kembali, berita acara kehilangan), formulir Laporkan Kondisi (foto lewat `UploadPhotoField.jsx` dengan kamera, aset terisi dari daftar), Aset Aktif/Rusak Ringan-Berat/Tidak Digunakan/Hilang memakai satu view Daftar Inventaris bervarian, Riwayat Perubahan Status; grup menu "Kondisi & Status Aset" 6 butir. Ketiga kriteria terpetakan ke source; tanpa perubahan backend. `SCR-AST-REG-27` memakai laporan kondisi yang ditetapkan atas keputusan Rizki (delta dari `/reports/history` milik `BE-AST-013`). `npx eslint --max-warnings=0` 35 berkas `0 errors, 0 warnings`; `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-18 09:29:34). Uji layar Playwright **27/27 langkah** dengan pelapor (Dimas), superadmin, penyetuju tanpa hak, dan LOKET; 0 galat halaman, 0 respons 5xx. Ikut dikerjakan atas permintaan Rizki: formulir tambah master Aset diseragamkan dengan master Akuntansi; label pilihan aset kini memuat nomor inventaris (diperiksa di layar, belum masuk `BUILD_ID`). Belum di-commit. Laporan: [`FE-AST-007.md`](../task/report/frontend/FE-AST-007.md) |
| Acceptance criteria | 1. `SCR-AST-REG-22..27`; empat daftar kondisi memakai satu komponen bersaringan tetap. 2. Foto memakai `UploadPhotoField.jsx`. 3. Setujui Hilang mewajibkan lampiran berita acara |

### `FE-AST-008` — Dokumen, pengamanan, pemanfaatan, menganggur

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. `SCR-AST-REG-14`, `28..32`. 2. Antrean persetujuan pemanfaatan menampilkan penyetuju yang diwajibkan. 3. Dokumen hampir habis dan aset menganggur memakai angka backend |

### `FE-AST-009` — Impor data awal

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Unduh templat, unggah, tabel galat per nomor baris Excel. 2. Dua tombol persetujuan sesuai hak. 3. Ringkasan total untuk Kepala Akuntansi |

### `FE-AST-010` — Dashboard dan laporan register

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Skema §3.5; kartu 1.4 dan 1.5 disisipkan kemudian oleh `FE-AST-014` dan `FE-AST-018`. 2. Kartu yang gagal dimuat tidak menjatuhkan kartu lain. 3. Laporan 11.1, 11.3, 11.7 dengan ekspor. 4. Butir menu Dashboard Aset dan tiga butir laporan terdaftar |
