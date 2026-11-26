# Laporan Perubahan Frontend — `FE-HRD-407`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-407` |
| Nomor lama | `FE-HRD-399` — dinomori ulang saat merge `QuilvianIntegrationBackend` 23 November 2026; nomor lama kini milik amandemen `S-A7-V2` (lihat `blueprint-manifest.md` revisi `69`) |
| Judul | Layar Dashboard SDM dengan daftar nama |
| Slice | `S-PKB` / `MVP-76` · `EPIC HRD-125` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 31 |
| Trace | `FR-PKB-05` (serta tampilan `FR-PKB-01` s.d. `03`); `HRD-DEC-243`, `HRD-DEC-245`, `HRD-ASM-92`; API 36.1; State 34.1; `INT-HR-115`; `VR-PKB-03` |
| Contract version | `v30`, disetujui 23 November 2026 |
| Wewenang UI | Frontend 35.2 `FE-PKB-01`; bentuk panel daftar nama `DEV_DISCRETION` |
| Dependency | `BE-HRD-461` ✅ (memuat `BE-HRD-457` s.d. `BE-HRD-460`) |
| Klasifikasi | `HEAVY` — route, view, service, dan konstanta baru; dua endpoint; gerbang hak; paginasi |
| Task mode | `CROSS-REPO` (source frontend; laporan dan tanda status di repository backend) |
| Target tulis | Source frontend; laporan, roadmap, traceability HRD |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `6c05df212` (branch `HamzahV2`); hasil task di-commit pengguna sebagai `6c1d4e368` |
| Commit backend yang dijadikan rujukan | `a9c1af31` (branch `MHamzah`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Belum ada route `/hr/dashboard` maupun service Dashboard SDM. Backend sudah menyediakan `HrdWorkforceDashboardController` dengan `GET /summary` dan `GET /drilldown` (hak `HrdWorkforceDashboard : Read`). Nama kelompok sah dibaca dari `WorkforceDashboardPanels` dan `ContractRenewalBuckets`: panel profesi memakai GUID profesi atau `UNASSIGNED` sebagai `bucket`, `documentType` hanya sah untuk panel dokumen, dan `search` dibatasi 100 karakter.

---

## 2. Proses bisnis dari sisi pengguna

Rina (Manajer SDM) membuka Dashboard SDM. Layar memanggil `/summary` setiap kali dibuka dan menampilkan tanggal operasional, tiga kartu ringkasan pegawai, distribusi profesi, aktif/nonaktif, kontrak berakhir (enam kelompok berlabel sama dengan layar Kontrak dan Perpanjangan Kontrak), dan dokumen kedaluwarsa dengan pilihan jenis dokumen.

Rina memilih jenis **SIP/STR** lalu mengklik **Sudah Habis 2**. Di bawah panel muncul "Daftar Nama — Dokumen Kedaluwarsa · Sudah Habis" berisi dr. Dodi dan Sari dengan nomor lisensi tersamar ("SIP …0123") dan sisa hari ("Lewat 3 hari", "Hari ini"). Rina mengklik baris dr. Dodi dan berpindah ke Employee 360. Daftar dapat dicari menurut nama atau nomor pegawai dan ditutup dengan tombol Tutup.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Backend: `HrdWorkforceDashboardController.cs`, `HrdWorkforceDashboardDtos.cs`, `WorkforceDashboardPanels.cs`, `ContractRenewalBuckets.cs`, `HrdWorkforceDashboardService.cs` (validasi drill-down), `Responses/PagedResult.cs`
- Kontrak `v30` API 36.1 dan 36.6; frontend 35.1 dan 35.2
- Modul referensi: `learning-dashboard-view.jsx`, `relations-dashboard-view.jsx`, `screening-worklist-view.jsx`
- `summary-grid.jsx` (dukungan `onClick`, `active`, `ariaLabel` per kartu), `use-hr-remote-data.js`, `hr-panel.jsx`
- Label kelompok kontrak: `contract-renewals-list-view.jsx` dan `employment-contracts-list-view.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/dashboard/page.jsx` | Route baru. |
| `src/components/view/hr/workforce-core/workforce-dashboard-view.jsx` | View baru: gerbang hak, lima panel dengan kartu yang dapat diklik, filter jenis dokumen, daftar nama berpaginasi dengan pencarian, keadaan memuat/kosong/gagal. |
| `src/lib/services/hr/workforce-dashboard-service.js` | Service baru `getSummary` dan `getDrilldown` serta serialisasi query. |
| `src/lib/constants/hr/workforce-core/workforce-dashboard-constants.js` | Kode panel dan kelompok, label kartu, pilihan jenis dokumen, pemetaan kartu ke parameter drill-down. |
| `tests/unit/human-resource-package-b-dashboard.test.mjs` | 5 test unit pemetaan parameter dan alamat terhadap `[Route]` controller. |
| `tests/e2e/human-resource-package-b.spec.mjs` | 4 skenario Playwright `FE-HRD-407`. |

### 3.3 Kepatuhan arsitektur frontend

Data dimuat lewat `useHrRemoteData` seperti layar HR lain, tanpa Redux karena datanya tidak dibagi lintas halaman dan kontrak meminta pemuatan ulang setiap halaman dibuka (`HRD-ASM-92`). Pesan galat memakai teks kontrak, bukan pesan Axios. Tidak ada dependency, komponen dasar, atau CSS baru.

Parameter dibentuk satu fungsi murni (`buildWorkforceDashboardDrilldownParams`) agar kombinasi yang ditolak backend (`VR-PKB-02`, `VR-PKB-03`) tidak pernah dikirim: `documentType` hanya untuk panel dokumen, `professionId` hanya untuk panel profesi, `search` dipangkas ke 100 karakter, dan `pageSize` paling besar 100.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `learning-dashboard-view.jsx` dan `relations-dashboard-view.jsx` (Hero, SummaryGrid, HrPanel, DataTable).

| Elemen | Sumber | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header dan tombol Muat ulang | `Hero` + `BaseButton variant="secondary"` | `REUSE` | Pola dashboard HR. |
| Kartu angka yang dapat diklik | `SummaryGrid` dengan `onClick`, `active`, `ariaLabel` | `REUSE` | Fitur bawaan; kartu menjadi tombol yang dapat diakses keyboard. |
| Panel per bagian | `HrPanel` | `REUSE` | Sama dengan dashboard Learning. |
| Angka per kelompok di dalam panel | `HrPanel` + `SummaryGrid` | `COMPOSE` | Lihat pilihan A. |
| Filter jenis dokumen | `FilterSelect` di `actions` `HrPanel` | `REUSE` | Kontrol filter standar. |
| Daftar nama | `DataFilter` + `DataTable` + `RegionPagination` | `COMPOSE` | Lihat pilihan B. |
| Keadaan kosong/gagal/hak | `InformationAlert`, `emptyText` panel | `REUSE` | Pola layar Paket B lain. |

**Pilihan A — angka per kelompok di dalam panel:**

1. **`HrPanel` berisi `SummaryGrid` yang kartunya dapat diklik (rekomendasi, dipakai).** Rupa angka sama dengan kartu ringkasan di atasnya dan dengan layar Kontrak; tanpa CSS baru; risiko regresi nol karena tidak mengubah komponen dasar.
2. `HrPanel` berisi `DataTable` dua kolom (kelompok, jumlah) dengan klik baris. Konsisten dengan tabel lain, tetapi angka kehilangan penekanan visual dan butuh dua klik mata untuk dibaca.

**Pilihan B — bentuk daftar nama (`DEV_DISCRETION`):**

1. **Panel di halaman yang sama, di bawah panel dashboard, dengan gulir otomatis (rekomendasi, dipakai).** Tabel berpaginasi dan pencarian bekerja penuh, nyaman di ponsel, dan memakai pola daftar yang sudah ada.
2. `ConfirmModal` ukuran besar tanpa footer. Rupa dialog konfirmasi dipakai untuk daftar baca, dan tabel berpaginasi di dalam modal sempit di ponsel.
3. Halaman terpisah `/hr/dashboard/drilldown`. Menambah route dan kehilangan konteks angka yang diklik.

`UI GATE: PASS — 5 REUSE, 2 COMPOSE, 0 EXTEND, 0 NEW.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs src/components/view/hr/workforce-core/workforce-dashboard-view.jsx src/app/hr/dashboard/page.jsx
OK — 2 berkas diperiksa, 0 temuan.
```

Grep anti-regresi (`ui-consistency-checklist.md` bagian G) pada kedua berkas JSX:

```text
<button / btn-*            : 0
<table                     : 0
fw-* / fs-*                : 0
class Tailwind             : 0
fixed inset-0              : 0
<input|select|textarea>/UUID: 0
variant tidak sah          : 0
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hr/dashboard` dengan daftar nama | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-399-dashboard-sdm-drilldown.png` | Jenis SIP/STR; kartu Sudah Habis tersorot; dua nama dengan lisensi tersamar. |
| `/hr/dashboard` ringkasan gagal | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-399-dashboard-sdm-gagal.png` | Hanya pesan dan Coba lagi; tidak ada kartu atau panel. |
| `/hr/dashboard` serba kosong | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-399-dashboard-sdm-kosong.png` | Pesan kosong per panel dan daftar nama kosong. |

`SummaryGrid` menampilkan judul kartu "Pengingat H-60" sebagai "PENGINGAT H 60" (normalisasi judul bawaan komponen). Perilaku ini sama persis dengan layar Perpanjangan Kontrak yang memakai label dan komponen yang sama, sehingga dibiarkan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Tanpa `HrdWorkforceDashboard : Read` | "Anda belum punya hak untuk melihat Dashboard SDM."; `/summary` tidak dipanggil. |
| Memuat ringkasan | Kerangka kartu `SummaryGrid`. |
| Ringkasan gagal | "Data dashboard gagal dimuat." + Coba lagi; tidak ada angka atau panel (`AT-HRD-PKB-26`). |
| Profesi kosong | "Belum ada pegawai aktif." |
| Kontrak kosong (enam kelompok nol) | "Tidak ada kontrak yang berakhir dalam 60 hari." |
| Dokumen kosong untuk jenis terpilih | "Tidak ada dokumen yang kedaluwarsa dalam 90 hari." |
| Daftar nama memuat / kosong | "Memuat daftar nama..." / "Tidak ada pegawai pada kelompok ini." |
| Daftar nama gagal | "Daftar gagal dimuat." + Coba lagi. |
| Ganti jenis dokumen saat daftar dokumen terbuka | Daftar dimuat ulang dengan jenis baru agar sama dengan angkanya. |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Core / Workforce Dashboard

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/summary` | Lima panel | `HrdWorkforceDashboard : Read` |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/drilldown?panel=&bucket=&professionId=&documentType=&search=&pageNumber=&pageSize=` | Daftar nama | `HrdWorkforceDashboard : Read` |

Baris daftar nama membuka route existing `/hr/employee-360/{workforceProfileId}` (frontend 35.1).

---

## 6. Verifikasi

| Skenario/perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run build` | Exit code `0`; 894 halaman statis, termasuk `○ /hr/dashboard` | `PASS` | Production build |
| `npm run lint:errors` | Exit code `0` | `PASS` | ESLint seluruh repository |
| `npx eslint` berkas yang diubah | Exit code `0`; 0 error, 0 warning pada berkas FE-HRD-407 | `PASS` | View, route, service, konstanta, test |
| Pemeriksa CSS | 2 berkas, 0 temuan | `PASS` | Bagian 3.4 |
| Test unit Paket B | 7/7 lulus (5 baru + 2 relations) | `PASS` | `node --test` |
| Playwright Chromium headed | 4/4 lulus | `PASS` | Drill-down → Employee 360, ringkasan gagal + Coba lagi, tanpa hak, keadaan kosong + daftar gagal |
| Playwright seluruh spec Paket B | 15/15 lulus | `PASS` | Regresi `FE-HRD-401` s.d. `FE-HRD-408` |
| `npm run test:unit` (suite penuh) | 5.187 test; 73 gagal, semuanya juga gagal pada HEAD atau bergantung keberadaan source backend | `PASS` untuk cakupan task | Rincian di laporan `FE-HRD-408` bagian 6.1 |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/human-resource-package-b-dashboard.test.mjs — PASS (5/5)`

`AUTOMATED TEST: npx playwright test tests/e2e/human-resource-package-b.spec.mjs --grep "FE-HRD-399" --headed --workers=1 — PASS (4/4)`

`MANUAL TEST: PASS` lewat Chromium headed terotomasi; tiga screenshot diperiksa satu per satu. Cakupan filter yang diuji di browser: pilihan jenis dokumen, label terpilih ("SIP/STR"), efek terhadap angka kartu dan request (`documentType=SIP_STR`). Pencarian dan reset daftar nama serta perpindahan halaman tidak diuji di browser (data mock hanya satu halaman); parameter `search`, `pageNumber`, dan `pageSize` dibuktikan lewat test unit, dan paginasi memakai perilaku bawaan `DataTable` + `RegionPagination`.

**Tidak dijalankan:** DB dev dan akun Manajer SDM nyata. Response dimock mengikuti DTO backend; DB dev hanya berisi 34 pegawai tanpa kontrak, lisensi, atau SPK (catatan `BE-HRD-462`). Uji dengan akun nyata dan hak yang sudah diberikan ke peran adalah cakupan `FE-HRD-409` (gerbang G3 dan G4).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC1 — "Sudah habis 2" pada SIP/STR → dr. Dodi dan Sari dengan lisensi tersamar; klik baris membuka Employee 360 | Terpenuhi | Skenario headed 1: request `panel=DOCUMENT_EXPIRY&bucket=EXPIRED&documentType=SIP_STR`, URL berakhir `/hr/employee-360/{id}` |
| AC2 — `/summary` gagal → pesan + Coba lagi tanpa angka nol palsu | Terpenuhi | Skenario headed 2: tidak ada kartu "Pegawai Aktif" maupun panel; Coba lagi memanggil ulang |
| AC3 — Keadaan kosong per panel sesuai 35.2 | Terpenuhi | Skenario headed 4 dan screenshot kosong |
| AC4 — Bentuk panel daftar nama `DEV_DISCRETION` | Terpenuhi | Pilihan B nomor 1 (3.4) |
| Data dimuat ulang setiap halaman dibuka, tanpa cache lintas sesi | Terpenuhi | `useHrRemoteData` memuat saat mount; tidak ada penyimpanan |
| Test unit pemetaan panel/kelompok (direkomendasikan) | Terpenuhi | 5/5 |
| Lint, build, uji browser, laporan, roadmap, traceability | Terpenuhi | Bagian 6 dan dokumen terkait |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kartu yang bernilai nol tetap dapat diklik dan membuka daftar kosong; ini disengaja agar semua angka berperilaku sama. |
| Masalah yang diketahui | Tidak ada. |
| Dependency backend | `BE-HRD-461` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Di-commit pengguna sebagai `6c1d4e368` (23 November 2026, 15.17 WIB). |
| Langkah berikutnya | `FE-HRD-408` (butir menu), lalu `FE-HRD-409`. |
