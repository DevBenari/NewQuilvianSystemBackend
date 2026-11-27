# Laporan Perubahan Frontend — `FE-LAB-57`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-57` |
| Judul | Daftar Pasien Lab: tanggal rencana, *Terlewat*, dan aksi Jadwal Ulang |
| Slice | `S20-1` penjadwalan ulang pemeriksaan — gelombang `MVP-16d` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) *Gelombang `MVP-16`* — `FE-LAB-57` |
| Trace | `FR-21.8`, `FR-21.12`, `FR-21.13`; `LAB-DEC-258`, `260`, `267`, `273`, `277`; `03-frontend-architecture.md` amandemen 2026-11-25; `02-backend-architecture.md` 33.10 no. 2 |
| Contract version | `LAB-API-v1` **`r50`** 45.2, 45.5; `LAB-PERM-v1` **revision 18** 20.2 — **`approved` 2026-11-25** (Yoga Aji Pratama) |
| Wewenang UI | `DEV_DISCRETION`: warna, ikon, label tombol, susunan dialog, letak kolom *Tanggal Rencana*, bentuk penanda *Terlewat*, nama berkas komponen. **Bukan** `DEV_DISCRETION` (dipatuhi): aksi baris lewat dialog tanggal + alasan; bawaan *Tanggal Rencana* + `includeOverdue` **hanya** di Daftar Pasien Lab; tombol tersembunyi tanpa izin dan **nonaktif beralasan** bila `canReschedule = false`; `version` selalu dikirim |
| Dependency | Kontrak `r50` (approved). Backend `BE-LAB-101`..`103` ✅ selesai pada kode (working tree), **belum** di API dev bersama |
| Klasifikasi | `HEAVY` — repository 0, berkas diperiksa 2 (±30), berkas diubah 2 (11), logika 2 (bawaan per mode, cabang state, dialog + penanganan `409`/`422`), kontrak API 2 (endpoint dan ruas baru), database 0, keamanan 1 (izin baru), UI 2 (tiga daftar + dua menu) = **11** |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — Lab: service, slice, hook, aturan, konstanta, view, dialog, uji unit dan e2e; laporan, roadmap, dan traceability modul di repository backend. Tanpa commit |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`); roadmap disusun pada `004524f4c` — nol berkas Lab berubah di antaranya |
| Commit backend yang dijadikan rujukan | `cce1dbf5` + working tree `BE-LAB-101`..`103` |
| Tanggal | 2026-11-26 |
| Status | ✅ **Selesai pada kode** — lint bersih, uji unit 37/37 baru + uji Lab terkait lulus, **e2e 32/32** (9 baru + 23 regresi daftar pantau) terhadap API tiruan `r50`. Verifikasi peramban dengan akun Analis terhadap backend nyata **belum** — menunggu langkah rilis `MVP-16e` |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti |
| --- | --- |
| Daftar Pasien Lab dan Hasil dan Riwayat adalah satu view (`LabMonitoringView`, prop `mode`) dan **berbagi satu cabang state Redux per disiplin** — penyaring yang disusun di satu menu ikut terbawa ke menu lain | `lab-monitoring-slice.jsx` `initialState` per `disciplineKey`; hook tidak menerima `mode` |
| Bawaan penyaring tunggal `dateCategory: "OrderDate"`, periode kosong | `lab-monitoring-constants.jsx` `defaults` |
| Penyaring tanggal menolak tanggal sesudah hari ini (`AC-98`) | `validateDateRange`, `maxDateValue = todayDateValue()` |
| Kategori Periode tiga pilihan; `includeOverdue` tidak dikenal | `dateCategoryOptions`, `buildMonitoringParams` |
| Tidak ada kolom tanggal rencana, penanda *Terlewat*, aksi Jadwal Ulang, maupun fungsi service-nya | `lab-monitoring-table-columns.jsx`, `lab-order.service.js` |
| **Baris daftar pantau tidak membawa `version`** (`r50` 45.5), padahal `version` wajib dikirim (33.10 no. 2) | Kontrak `LabMonitoringItemResponse` |

Celah utama: bila bawaan *Tanggal Rencana* dipasang di cabang bersama, Hasil dan Riwayat ikut berubah bawaannya begitu
petugas berpindah menu — persis jebakan (a) roadmap.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** Analis Laboratorium (pemegang `LabOrder : Reschedule` lewat seeder `BE-LAB-102`) di Daftar Pasien Lab
ketiga disiplin.

1. Gilang membuka **Daftar Pasien Lab Patologi Klinik**. Layar langsung menampilkan pesanan bertanggal rencana **hari
   ini** beserta pesanan **terlewat** — penyaring terisi *Tanggal Rencana*, 26 Nov – 26 Nov, *Termasuk Terlewat*.
2. Kolom **Tanggal Rencana** menampilkan `24-11-2026` dan lencana **Terlewat 2 hari** untuk Profil Lipid yang belum
   diambil sampelnya.
3. Ia membuka menu aksi baris Gula Darah Puasa → **Jadwal Ulang**. Dialog terbuka dan memuat rincian pesanan (versi dan
   kelayakan terkini).
4. Dialog menampilkan pasien, nomor pesanan, dan *Tanggal rencana saat ini: 26-11-2026*. Ia memilih **27 Nov 2026**
   (tanggal sebelum hari ini tidak dapat dipilih) dan menulis alasan *pasien belum puasa*. **Simpan Jadwal** baru aktif
   sesudah keduanya terisi.
5. Simpan → `PUT …/reschedule` dengan `{ plannedDate, reason, version }`. Dialog tertutup, muncul *Pesanan
   LAB-RSMMC-000031 dijadwalkan ulang ke 27-11-2026.*, dan daftar dimuat ulang — pesanan itu kini ada di daftar 27 Nov.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Pesanan yang sampelnya sudah diambil | Butir **Jadwal Ulang** tampil **nonaktif** dengan keterangan *Sampel sudah diambil; batalkan wadah atau ambil ulang lebih dulu.* — alasan dari backend apa adanya |
| Petugas tanpa `LabOrder : Reschedule` (mis. dokter pemegang `Read`) | Butir tidak tampil |
| Petugas lain menyimpan lebih dulu (`409` versi) atau pesanan menjadi tidak layak (`409`) | Pesan server tampil di dialog apa adanya; dialog tetap terbuka; daftar dan rincian pesanan dimuat ulang — percobaan berikutnya membawa versi terbaru |
| Tanggal ditolak server (`422`) | Pesan tampil di bawah isian tanggal |
| Simpan sedang berjalan | Tombol nonaktif berpemutar — satu klik, satu permintaan |
| Rincian pesanan gagal dimuat | Pesan galat di dialog; Simpan nonaktif |
| Menu **Hasil dan Riwayat** | Tetap berbawaan *Tanggal Order* tanpa periode; *Tanggal Rencana* tersedia sebagai pilihan, tanpa *Terlewat* menyala |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; `rules/frontend/` (`frontend-architecture`, `REPORT_TEMPLATE`); roadmap
*Gelombang `MVP-16`*; `03-frontend-architecture.md` amandemen 2026-11-25; `LAB-API-v1` 45.2/45.5/45.6;
`lab-monitoring-view.jsx`, `lab-monitoring-table-columns.jsx`, `use-lab-monitoring.jsx`, `lab-monitoring-slice.jsx`,
`lab-monitoring-constants.jsx`, `lab-monitoring-rules.js`, `lab-cancellation-rules.js`, `lab-order.service.js`;
`ConfirmModal`, `FilterDatePicker`, `RowActionMenu`, `StatusBadge`, `usePermission`, `permission-slice`;
`asset-loan-action-modals.jsx` (preseden pemilih tanggal di dalam dialog); uji unit dan e2e daftar pantau yang ada.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/laboratory-management/lab-order.service.js` | `rescheduleLabOrder(id, payload)` (`PUT …/reschedule`), `getLabOrderReschedules(id)` (`GET …/reschedules`, dipakai `FE-LAB-58`) |
| `src/lib/constants/health-services/laboratory-management/lab-monitoring-constants.jsx` | Pilihan `PlannedDate` (*Tanggal Rencana*); `includeOverdue` (bawaan `false`) dan `includeOverdueOptions`; `LAB_MONITORING_MODES`; `resolveLabMonitoringBranchKey` (cabang `worklist` tetap bernama kunci disiplin, cabang `results` = `<disiplin>Results`); `buildLabMonitoringDefaults(mode, today)` |
| `src/lib/hooks/health-services/laboratory-management/lab-monitoring-rules.js` | Teruskan `dateCategory=PlannedDate` dan `includeOverdue` (hanya bersama `PlannedDate`); `validateDateRange` mengizinkan tanggal ke depan untuk *Tanggal Rencana*; `resolveFilterMaxDate`; `shouldRefreshWorklistDefaults` (isi bawaan hari ini; isi ulang bila bawaan dari hari lain dan belum diubah) |
| `src/lib/hooks/health-services/laboratory-management/lab-reschedule-rules.js` | **Baru** — aturan murni dialog: tanggal (`VAL-157`, `158`), alasan (`VAL-159`), muatan ber-`version`, penempatan pesan `409`/`422`, aksi baris dari `canReschedule`/`rescheduleBlockReason`, penanda *Terlewat*, format `dd-MM-yyyy` tanpa geser zona. **Tidak menghitung kelayakan** |
| `src/lib/state/slice/health-services/laboratory-management/lab-monitoring-slice.jsx` | **Dua cabang per disiplin** (`worklist`, `results`) dengan `mode` dan `defaultsDay`; reset memakai bawaan menunya; thunk `submitLabOrderReschedule` + `clearLabOrderRescheduleError` pola `submitLabOrderCancellation` |
| `src/lib/hooks/health-services/laboratory-management/use-lab-monitoring.jsx` | Menerima `mode`; seluruh kunci state memakai cabang; mengisi bawaan hari ini sebelum permintaan pertama; batas pemilih tanggal per kategori; alur Jadwal Ulang (izin, muat detail untuk `version`, kirim, sukses/`409`/`422`) |
| `src/components/view/health-services/laboratory-management/lab-orders/lab-reschedule-dialog.jsx` | **Baru** — dialog bersama (daftar dan detail `FE-LAB-58`): ringkasan, tanggal saat ini, pemilih tanggal (`min` = hari ini), alasan wajib ≤ 1000, peringatan tidak layak, galat per isian |
| `src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view.jsx` | Meneruskan `mode` ke hook; pilihan *Terlewat* tampil hanya saat *Tanggal Rencana*; memasang dialog dan aksi baris |
| `src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-table-columns.jsx` | Kolom **Tanggal Rencana** + lencana *Terlewat n hari*; butir **Jadwal Ulang** (tersembunyi tanpa izin, nonaktif beralasan) |
| `tests/unit/lab-reschedule-rules.test.mjs` | **Baru** — 37 uji |
| `tests/unit/lab-monitoring-rules.test.mjs` | Uji `r23` jumlah pilihan menjadi empat (`r23/r50`) — perubahan prasyarat yang disengaja |
| `tests/e2e/lab-reschedule-worklist.spec.mjs` | **Baru** — 9 skenario API tiruan |
| `tests/e2e/lab-monitoring-date-guard.spec.mjs`, `tests/e2e/lab-monitoring-r18-filters.spec.mjs` | Uji yang mengunci bawaan *Tanggal Order* (AC-98, AC-99, `r18` bawaan, `r23` pilihan) dipindah ke **Hasil dan Riwayat** — komponen dan aturan yang sama, bawaannya tetap *Tanggal Order*; `r23` kini memeriksa pilihan keempat |

### 3.3 Kepatuhan arsitektur frontend

Alur `app → view → hook → state/service → InstanceAxios` dipertahankan: view tidak memanggil Axios; aturan murni di
`lib/hooks/.../*-rules.js`; konstanta dan pembentuk bawaan di `lib/constants`; slice tidak mengimpor dari folder hook
(pembentuk bawaan sengaja ditaruh di konstanta). Komponen dasar dipakai ulang: `ConfirmModal`, `FilterDatePicker`,
`FilterSelect`, `RowActionMenu`, `StatusBadge`, `InformationAlert`. **Pola baru:** cabang state per mode — dibutuhkan
`LAB-DEC-277`; cabang `worklist` tetap bernama kunci disiplin sehingga pemanggil lama tidak berubah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Daftar: pola tabel yang ada (*Mengambil daftar pantau...*); dialog: *Memuat rincian pesanan...*, Simpan nonaktif |
| Kosong | *Belum ada pesanan pada penyaring ini* (tidak berubah) |
| Gagal | Daftar: alert galat yang ada; dialog: pesan server (`409` apa adanya + muat ulang, `422` di bawah isian tanggal, lainnya alert); rincian gagal dimuat → alert, Simpan nonaktif |
| Tanpa hak akses | Daftar: `AccessDeniedGate` yang ada; butir Jadwal Ulang disembunyikan; `403` saat menyimpan tampil sebagai pesan server |
| Tidak layak | Butir nonaktif dengan alasan backend terlihat di menu; di dialog, peringatan berisi alasan yang sama |
| Simpan berjalan | Tombol berpemutar dan nonaktif |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}` | Versi, tanggal rencana, dan kelayakan terkini saat dialog dibuka | `LabOrder : Read` |
| `PUT` | `/v1/health-services/laboratory-management/lab-orders/{id}/reschedule` | Menyimpan jadwal ulang | `LabOrder : Reschedule` |

#### Health Services / Laboratory Management / Lab Monitoring

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-monitoring/{disiplin}` | Daftar dengan `dateCategory=PlannedDate`, `includeOverdue`, ruas baris `plannedDate`, `isOverdue`, `overdueDays`, `canReschedule`, alasan | `LabMonitoring : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas seluruh berkas yang diubah | 0 error, 0 peringatan pada berkas yang diubah (empat peringatan dependensi akibat penggantian kunci cabang sudah diperbaiki) | `PASS` | Keluaran eslint |
| `node --import ./tests/helpers/register.mjs --test` atas uji Lab terkait (aturan jadwal ulang, daftar pantau, FE-43, pembatalan, paritas v1, menu) | 121/122; satu gagal `FE-LAB-49 AC-282` membaca `menu-items.jsx` yang **tidak berubah** | `PASS` / `EXISTING / ENVIRONMENT ISSUE` | Keluaran uji |
| Seluruh `tests/unit/` | 5.581 uji, 152 gagal; berkas uji yang gagal dipetakan — nol yang membaca berkas task ini selain `lab-menu-br137` (kegagalan `menu-items.jsx` di atas) | `EXISTING / ENVIRONMENT ISSUE` | `fe57-unit-all.log` |
| `npm run build` (dua kali; server lokal dipastikan mati) | `Compiled successfully`, standalone siap | `PASS` | Keluaran build |
| E2E baru `lab-reschedule-worklist.spec.mjs` (server standalone 3710, API tiruan `r50`) | **9/9**: bawaan *Tanggal Rencana* hari ini + `includeOverdue` dan nol permintaan tanpa periode; kolom dan lencana *Terlewat 2 hari* (hanya satu); Hasil dan Riwayat tanpa `dateCategory`/periode; butir aktif, **nonaktif beralasan** (atribut dan teks alasan), tersembunyi tanpa izin; sukses (muatan `{plannedDate, reason ter-trim, version: 4}`, dialog tertutup, notifikasi, daftar dimuat ulang); Simpan nonaktif saat berjalan (satu `PUT`); `409` (pesan apa adanya, dialog terbuka, daftar + detail dimuat ulang, percobaan kedua membawa `version: 5`); `422` tanggal di bawah isian; kalender menolak kemarin | `PASS` | `pw-fe57.log` |
| Regresi e2e daftar pantau: `date-guard`, `r18-filters`, `gender-icon`, `label-print`, `mcu-coverage-label`, `result-entry`, `patient-search-paging` | Sebelum penyesuaian: 19 lulus, 4 gagal — tepat empat uji yang mengunci bawaan *Tanggal Order* pada Daftar Pasien Lab. Sesudah dipindah ke Hasil dan Riwayat: lulus | `PASS` | `pw-reg.log`, `pw-reg2.log` |
| E2E akhir pada build terakhir — 8 spec | **32/32** | `PASS` | `pw-final.log` |
| Pemeriksaan visual (tangkapan layar 1600 px) | Penyaring terisi bawaan; kolom *Tanggal Rencana*; menu dengan butir nonaktif beserta alasan; dialog rapi. Lencana *Terlewat 2 hari* turun baris karena aturan modul Lab yang sengaja mengizinkan lencana patah di tabel padat (`lab-module-layout.module.css`) — konsisten dengan lencana lain | `PASS` | `fe57-shots/*.png` |
| `test-results/` dan `playwright-report/` ter-track | Tidak berubah (`--output` ke scratchpad) | `PASS` | `git status` |

Uji manual: **`NOT FEASIBLE`** terhadap backend nyata — build frontend menunjuk API dev **bersama**, yang belum memuat
kode `BE-LAB-101`..`103`, dan devYoga belum menerapkan migration `BE-LAB-101` (langkah rilis `MVP-16e` no. 1–2). Setiap
kontrol interaktif yang diubah diperiksa di peramban sungguhan (Chromium Playwright) terhadap API tiruan berbentuk `r50`:
pilihan *Kategori Periode* dan *Terlewat*, pemilih tanggal penyaring dan dialog, isian alasan, menu aksi, Simpan/Batal,
serta penanganan `409`/`422`.

**Tidak dijalankan:** peramban dev 3000 dengan akun Analis (Gilang) terhadap backend nyata — menunggu langkah rilis.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-344`, `AC-346` / `T-S20-13` sisi layar — rencana hari ini + terlewat, `overdueDays` | Terpenuhi (API tiruan) | E2E 1 |
| `AC-356` — Hasil dan Riwayat tetap `OrderDate` | Terpenuhi | E2E 2 + uji unit cabang |
| `AC-353` / `T-S20-18` sisi layar — tombol tampil / nonaktif beralasan / tersembunyi | Terpenuhi (API tiruan) | E2E 3, 4 |
| Dialog berhasil memuat ulang daftar | Terpenuhi | E2E 5 |
| `409` menampilkan pesan server dan memuat ulang | Terpenuhi | E2E 7 |
| `422` di bawah isian | Terpenuhi (tanggal) | E2E 8 |
| Simpan nonaktif saat berjalan | Terpenuhi | E2E 6 |
| `version` selalu dikirim (33.10 no. 2) | Terpenuhi | E2E 5, 7 + uji unit |
| Uji unit aturan dan bawaan per mode | Terpenuhi | 37 uji |
| Peramban dev 3000 dengan akun Analis terhadap backend nyata | **Belum** | Menunggu `MVP-16e` no. 1–2 |
| DoD — laporan `FE-LAB-57.md` | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta terhadap rancangan | (1) **Dialog memuat detail pesanan saat dibuka**: baris daftar pantau tidak membawa `version` (`r50` 45.5) sedangkan `version` wajib dikirim — `GET /lab-orders/{id}` (sudah dipakai layar ini untuk Nota) memberi versi, tanggal, dan kelayakan terkini. Kontrak tidak diubah. (2) **Cabang state per mode** — tidak tertulis di rancangan, tetapi tanpa itu `LAB-DEC-277` tidak dapat dijamin. (3) Pesan `422` alasan (`VAL-159`) tampil sebagai alert di atas isian alasan, bukan di bawahnya: isian alasan milik `ConfirmModal` bersama. Praktis tidak tercapai karena layar sudah menahan alasan kosong. (4) `includeOverdue` dapat dimatikan lewat pilihan *Terlewat* — rancangan hanya menyebut bawaannya |
| Peringatan | Kolom *Tanggal Rencana* hanya terisi dari backend `r50` (`BE-LAB-103`). Terhadap API dev bersama yang belum memuatnya, kolom berisi `-` dan butir Jadwal Ulang nonaktif dengan keterangan *Kelayakan jadwal ulang belum tersedia* — tidak menebak |
| Masalah yang diketahui | `FE-LAB-49 AC-282` (menu Laporan Operasional) gagal sebelum task ini; di luar scope |
| Dependency backend | `BE-LAB-101`..`103` selesai pada kode; diperlukan langkah rilis `MVP-16e` no. 1–3 sebelum verifikasi peramban dengan akun asli |
| Perubahan sampingan | `NONE`. Spec visual sementara `zz-fe57-visual.tmp.spec.mjs` dibuat dan dihapus dalam sesi ini. Working tree juga memuat perubahan SDM (`src/lib/services/hr/*`, `src/utils/hr/hr-api-utils.js`, `tests/unit/hr-a52b-api-client.test.mjs`) milik **sesi lain** yang muncul selama pengerjaan — tidak disentuh; build kedua ikut memuatnya, tanpa pengaruh pada layar Lab |
| Interupsi | `NONE` |
| Status Git | ` M` `lab-monitoring-table-columns.jsx`, `lab-monitoring-view.jsx`, `lab-monitoring-constants.jsx`, `lab-monitoring-rules.js`, `use-lab-monitoring.jsx`, `lab-order.service.js`, `lab-monitoring-slice.jsx`, `tests/unit/lab-monitoring-rules.test.mjs`, `tests/e2e/lab-monitoring-date-guard.spec.mjs`, `tests/e2e/lab-monitoring-r18-filters.spec.mjs`; `??` `lab-reschedule-dialog.jsx`, `lab-reschedule-rules.js`, `tests/unit/lab-reschedule-rules.test.mjs`, `tests/e2e/lab-reschedule-worklist.spec.mjs`. Tanpa stage/commit |
| Langkah berikutnya | `FE-LAB-58` (detail pesanan: tanggal rencana, tombol, riwayat — memakai dialog ini) dan `FE-LAB-59` (kiosk). Sesudah langkah rilis: verifikasi peramban dengan akun Gilang |
