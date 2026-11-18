# Laporan Perubahan Frontend — `FE-HRD-R32`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R32` |
| Judul | OPPE bagian 2: FPPE dan rekredensial |
| Slice | `S-C1-OPPE` (OPPE/FPPE/Rekredensial), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R32`; task asal `FE-HRD-207`–`214` |
| Trace | Kartu asal pada `frontend-roadmap.md`; VR-OPP-09, VR-OPP-10, VR-OPP-12, VR-OPP-19 |
| Contract version | Source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi ke base component dan penyambungan API nyata; tanpa base component baru |
| Dependency | `FE-HRD-R14` (pola rujukan) — selesai 🟡 |
| Klasifikasi | `HEAVY` — 5 layar, 3 modal, service, konstanta, rute |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` (awal sesi), `af4da693e` (saat laporan; sebagian pekerjaan sudah di-commit pengguna) — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `7445a8b8` (awal sesi), `974f7208` (saat laporan) — branch `MHamzah` |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — alur FPPE dan rekredensial nyata; daftar penugasan/putusan tidak ada di backend dan uji browser belum dapat dijalankan |

---

## 1. Keadaan yang ditemukan di awal

- Lima layar FPPE/rekredensial menampilkan data contoh dan menghitung tier rekredensial di peramban dengan ambang 70/85, berbeda dari server (65/80).
- Modal inisiasi FPPE, pencatatan kasus, dan override tier memakai elemen HTML mentah dan `oppe.module.css`.
- Layar pemantauan proctoring dan pleno tidak punya cara memilih penugasan karena backend tidak menyediakan daftar penugasan.

## 2. Proses bisnis dari sisi pengguna

1. Komite Medik membuka **Pemicu FPPE**: daftar pemicu (sentinel, indikator OPPE di bawah ambang) dimuat dari server, dengan tombol Muat Ulang.
2. Dari satu pemicu, komite membuat **penugasan FPPE**: memilih dokter (daftar dokter, bukan UUID), proctor, jumlah kasus minimal dan durasi hari (dibatasi VR-OPP-09/10).
3. Setelah tersimpan, tautan membuka **Proctoring** untuk penugasan itu (`?engagementId=`). Proctor yang ditunjuk mencatat kasus lalu memverifikasinya; pengguna lain tidak melihat tombol verifikasi (VR-OPP-12).
4. Setelah kasus terpenuhi, **Pleno** menyimpulkan hasil (lanjut, perpanjang, cabut kewenangan) dan menyinkronkan kewenangan klinis.
5. Untuk rekredensial, komite membuka **Skor Komposit** dokter, dapat melakukan override tier dengan alasan, lalu menerbitkan putusan.
6. **Konsol Sinkron SIMRS** mengirim putusan ke SIMRS dan menampilkan log audit, termasuk kirim ulang dari log.
7. Jalur tidak normal: tanpa `engagementId` layar proctoring/pleno menampilkan petunjuk membuka dari pemicu; bila identitas pegawai sesi kosong, aksi pengesahan dikunci dengan pesan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Controller FPPE dan rekredensial backend (Tags di bagian 5), `oppe-constants.js`, `clinical-performance-service.js`, kartu `FE-HRD-R32`, pola `FE-HRD-R14`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/oppe/oppe-constants.js` | Permission, rute, resource pilihan dokter (`valueKey workforceProfileId`), peta status dan opsi FPPE/rekredensial/SIMRS |
| `src/lib/services/hr/clinical-performance-service.js` | Base path, `signal` pada GET, `syncFppePrivilege`, `getRecredScorecard`, `issueRecredVerdict` |
| `src/utils/hr/oppe/oppe-display-utils.js` | Ditulis ulang; kalkulasi tier klien dihapus |
| `src/components/view/**/fppe-triggers-dashboard-view.jsx`, `fppe-proctoring-dashboard-view.jsx`, `fppe-conclusions-plenary-view.jsx`, `recred-composite-scorecard-view.jsx`, `recred-simrs-sync-console-view.jsx` | Ditulis ulang dengan base component dan API nyata |
| `FppeInitiateModal`, `FppeCaseLogEntryModal`, `RecredentialingTierOverrideModal` | Ditulis ulang di atas `ConfirmModal` + `BaseFormControl`; reset lewat `key` |
| 4 berkas `page.jsx` proctoring/pleno (corporate dan admin) | Async, membaca `searchParams.engagementId` |
| `src/utils/menu-sidebar/menu-items.jsx` | `requiredPermission` lima menu FPPE/rekredensial |
| `src/style/hr/oppe/oppe.module.css` | Dihapus (yatim) |

### 3.3 Kepatuhan arsitektur frontend

Alur `page → view → service (InstanceAxios)`; data lewat `useHrRemoteData`/`useHrAction`. Komponen bersama `HrPanel`, `HrInfoGrid`, `HrStatusBadge`, `HrResourceField` (R14) dipakai ulang. Tidak ada pola baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** layar R14 (`components/view/hr/workforce-core/*`).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header | `Hero` | `REUSE` | — |
| Ringkasan | `SummaryGrid` | `REUSE` | — |
| Filter | `DataFilter` + `FilterSelect` + `ResourceFilterSelect` | `REUSE` | — |
| Tabel | `DataTable` | `REUSE` | — |
| Status | `StatusBadge` / `HrStatusBadge` | `REUSE` / `WRAP` | Peta status domain |
| Dialog | `ConfirmModal` + `BaseFormControl` | `COMPOSE` | — |
| Pilihan dokter/proctor | `HrResourceField` | `REUSE` | Menggantikan isian UUID |
| Bilah progres kasus | `exit-clearance.module.css` (`progressBarTrack/Fill`) | `REUSE` | Lebar dinamis via `style` |

`UI GATE: PASS — 9 elemen REUSE, 1 COMPOSE, 0 NEW`

```text
$ node .../check-css-usage.mjs <8 berkas JSX R32>
OK — 8 berkas diperiksa, 0 temuan.
$ grep Tailwind <berkas R32>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan di lingkungan ini; menyalakan backend pada basis data dev bersama akan menulis data sesi/audit, di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/ringkasan dari `DataTable`/`SummaryGrid` |
| Kosong | "Belum ada pemicu FPPE." / petunjuk membuka penugasan dari pemicu |
| Gagal | Pesan server lewat `InformationAlert` danger; tombol Muat Ulang |
| Tanpa hak akses | Peringatan "Anda belum punya hak…"; tombol aksi disembunyikan per permission |

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD FPPE Incident & Focused Evaluation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/hrd/clinical-performance/fppe/triggers/feed` | Daftar pemicu | `FppeFocusedEvaluation : ViewTrigger` |
| `POST` | `/v1/corporate/hrd/clinical-performance/fppe/engagements` | Buat penugasan | `FppeFocusedEvaluation : CreateEngagement` |
| `GET` | `/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}` | Rincian penugasan | `FppeFocusedEvaluation : Read` |
| `POST` | `/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/case-logs` | Catat kasus | `FppeFocusedEvaluation : AddCaseLog` |
| `PUT` | `/v1/corporate/hrd/clinical-performance/fppe/case-logs/{logId}/verify` | Verifikasi kasus | `FppeFocusedEvaluation : VerifyCaseLog` |
| `POST` | `/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/conclude` | Kesimpulan pleno | `FppeFocusedEvaluation : ApproveConclusion` |
| `POST` | `/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/sync-privilege` | Sinkron kewenangan | `FppeFocusedEvaluation : SyncPrivilege` |

#### Corporate - HRD Recredentialing Quality & Composite Scorecard

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}` | Skor komposit | `RecredentialingQuality : Read` |
| `POST` | `.../scorecards/{staffId}/override-tier` | Override tier | `RecredentialingQuality : Decide` |
| `POST` | `.../verdicts` | Terbitkan putusan | `RecredentialingQuality : Decide` |
| `POST` | `.../verdicts/{id}/dispatch-simrs` | Kirim ke SIMRS | `RecredentialingQuality : DispatchSimrs` |
| `GET` | `.../simrs-sync/audit-logs` | Log sinkron | `RecredentialingQuality : ViewSyncAudit` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 18 berkas R32 | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 8 JSX | 0 temuan | `PASS` | Bagian 3.4 |
| `npm run build` (BUILD-1, setelah R32–R35) | Compiled 113 s, 821/821 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| `npm run build` (BUILD-2, akhir Gelombang 3) | Compiled 62 s, 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser per peran | Tidak dijalankan | `NOT RUN` | Lihat 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` (base component, 0 temuan pemeriksa, tanpa data contoh) | Terpenuhi | 3.2, 3.4 |
| Alur FPPE pemicu → proctoring → pleno nyata | Terpenuhi sebagian | Proctoring/pleno dibuka via `?engagementId=` karena tidak ada daftar penugasan (diarahkan ke `FE-HRD-R53`) |
| Rekredensial skor → override → putusan → SIMRS | Terpenuhi sebagian | Tidak ada daftar putusan; dispatch dari panel sesi dan kirim ulang dari log |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Badge "berkedip" sentinel tidak dibuat (aksesibilitas WCAG 2.3.1); diganti badge danger + banner |
| Masalah yang diketahui | Backend tidak mencocokkan `ProctorWorkforceId` dengan proctor yang ditunjuk; frontend membatasi tombol |
| Dependency backend | Daftar penugasan FPPE, daftar putusan rekredensial, unggah nota pertimbangan override (VR-OPP-19), log akses ditolak SIMRS |
| Perubahan sampingan | `oppe.module.css` dihapus (yatim) |
| Interupsi | Konteks sesi dipadatkan sekali; pekerjaan dilanjutkan dari catatan |
| Status Git | 81 entri belum di-commit di repo frontend (62 diubah, 8 dihapus, 11 baru) untuk seluruh Gelombang 3; sebagian pekerjaan sudah di-commit pengguna |
| Langkah berikutnya | Backend: endpoint daftar penugasan/putusan (`FE-HRD-R53`); uji browser per peran |
