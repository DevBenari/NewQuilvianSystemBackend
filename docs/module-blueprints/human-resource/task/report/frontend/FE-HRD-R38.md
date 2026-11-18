# Laporan Perubahan Frontend — `FE-HRD-R38`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R38` |
| Judul | Migrasi base component Roster |
| Slice | `S-B4` (Penjadwalan & Roster), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R38`; task asal `FE-HRD-077`–`085` |
| Trace | API Contract v10 bagian 16; HRD-DEC-102, VR-SCH-17, VR-SCH-80 |
| Contract version | API Contract v10 + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi base component; kisi roster tetap elemen khusus |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `HEAVY` — 10 berkas, 39 pelanggaran |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — seluruh layar dan modal bermigrasi, kisi dipertahankan, ID contoh dihapus; tiga gap backend dan uji browser belum dapat dijalankan |

---

## 1. Keadaan yang ditemukan di awal

- Daftar periode dan on-call membaca `totalCount/totalPages` (server: `totalData/totalPage`), sehingga paginasi tidak pernah muncul.
- Kisi roster memakai ID shift contoh (`1111…`, `2222…`), ID pola tetap `0000…0001` (apply-pattern selalu 404), mengirim ID sel `0000…` saat sel kosong diklik (404), dan memakai `window.confirm` untuk tutup periode.
- Formulir on-call mengirim UUID contoh untuk periode, jenis siaga, dan pegawai; nama diketik ke kolom ID.
- Modal call-in mengisi jam dengan `toISOString().slice(0,16)`, sehingga bergeser 7 jam (UTC).
- Drawer validasi memakai `shiftAssignmentId` sebagai kunci alasan; dua peringatan pada sel yang sama bertumpuk sehingga jumlah alasan kurang dan server menolak terbit.
- `scheduling.module.css` berisi 57 literal warna hex.

## 2. Proses bisnis dari sisi pengguna

1. Kepala unit membuat **periode roster** (nama, unit, rentang) dan langsung masuk ke kisi.
2. Di **kisi roster**, kepala unit memuat staf unit, menyalin periode lalu, atau menerapkan pola shift dari master. Sel berwarna menunjukkan shift pagi/siang, malam, atau siaga; titik merah menandai konflik blokir.
3. Klik sel berisi shift: pada draf membuka **Ubah Shift** (koreksi mundur maksimal 3 hari); pada roster terbit membuka **Ganti Petugas** yang langsung berlaku. Sel kosong tidak dapat diklik.
4. **Audit & Terbitkan** menjalankan validasi B/W; konflik blokir wajib diperbaiki, setiap peringatan W diberi alasan dispensasi, lalu roster diterbitkan.
5. Setelah terbit, HRD Payroll **menutup periode** (konfirmasi), sehingga kisi menjadi baca-saja.
6. **On-call**: admin menjadwalkan siaga (periode, petugas, jenis, waktu, target respons), lalu mengonfirmasi, mengaktifkan, mencatat panggilan cito (waktu panggilan dan tiba), dan menyelesaikan.
7. **Pengaturan** menampilkan ambang jeda istirahat, hari kerja dan malam berturut-turut, serta jendela koreksi mundur; pemegang hak Manage dapat menyimpan.
8. Jalur tidak normal: tanpa hak tampil peringatan; galat server tampil; tombol dinonaktifkan sesuai status.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`RosterPeriodController`, `ShiftReplacementController`, `OnCallAssignmentController`, `SchedulingSettingController`, `ShiftPatternController`, DTO `RosterSchedulingDtos.cs`, `SchedulingSettingDtos.cs`, layanan `RosterPeriodService`/`ShiftReplacementService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/scheduling/scheduling-constants.js` | BARU: rute, status roster/on-call, peran, alasan, nama aturan B/W, resource periode/pola/petugas |
| `src/utils/hr/scheduling/scheduling-display-utils.js` | Kolom tanggal, nada sel, label singkat, hitung hari mundur (tanpa warna literal) |
| `src/style/hr/scheduling/roster-grid.module.css` | BARU: kisi dan legenda berbasis token + `color-mix` |
| `roster-scheduling-view.jsx`, `roster-matrix-detail-view.jsx`, `on-call-scheduling-view.jsx`, `scheduling-settings-view.jsx` | Ditulis ulang; kisi dipertahankan dengan sel `<button>` yang terjangkau keyboard |
| `modals/CopyPreviousPeriodModal`, `ApplyShiftPatternModal`, `QuickShiftReplacementModal`, `RetroactiveShiftCorrectionModal`, `OnCallCallInModal` | Ditulis ulang di atas `ConfirmModal` |
| `modals/RosterValidationModal.jsx`, `modals/OnCallAssignmentFormModal.jsx` | BARU |
| `drawers/RosterValidationDrawer.jsx`, `style/hr/scheduling/scheduling.module.css` | Dihapus |
| `menu-items.jsx` | `requiredPermission` tiga menu penjadwalan |

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`; daftar periode membuka rute `/hr/scheduling/roster/[id]` yang sudah ada.

### 3.4 Gerbang tampilan

**Modul referensi visual:** layar R14.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `ResourceFilterSelect`, `DataTable` | `REUSE` | — |
| Kisi roster bulanan | `roster-grid.module.css` | `NEW` (CSS Module) | Elemen unik menurut AC R38; token saja |
| Panel kisi | `HrPanel` | `REUSE` | — |
| Hasil audit & terbit | `ConfirmModal` + `SummaryGrid` + `HrPanel` | `COMPOSE` | Pengganti drawer (DEC-FER-01 belum diputuskan) |
| Dialog lain | `ConfirmModal` + `BaseFormControl` + `HrResourceField` | `COMPOSE` | — |

`UI GATE: PASS — 3 elemen REUSE, 2 COMPOSE, 1 NEW (kisi, disetujui AC kartu R38)`

```text
$ node .../check-css-usage.mjs <10 berkas JSX scheduling>
OK — 1 berkas diperiksa, 0 temuan. (×10)
$ grep Tailwind <berkas R38>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/ringkasan; Hero "Memuat periode roster..." |
| Kosong | "Belum ada staf pada periode ini. Gunakan Muat Staf Unit atau Salin Periode Lalu." / "Belum ada periode roster." |
| Gagal | Pesan server di `InformationAlert` |
| Tanpa hak akses | "Anda belum punya hak untuk melihat …"; sel tidak dapat diklik tanpa `ShiftReplacement : Update` |

## 5. Endpoint yang dikonsumsi

#### HR Scheduling - Roster Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/roster-periods` | Daftar | `RosterPeriod : Read` |
| `GET` | `.../roster-periods/summary` | Ringkasan | `RosterPeriod : Read` |
| `GET` | `.../roster-periods/{id}` | Kisi | `RosterPeriod : Read` |
| `POST` | `.../roster-periods` | Buat periode | `RosterPeriod : Create` |
| `POST` | `.../{id}/populate-staff`, `.../copy-previous`, `.../apply-pattern` | Isi kisi | `RosterAssignment : Create` |
| `POST` | `.../{id}/validate`, `.../publish`, `.../close` | Audit, terbit, tutup | `RosterPeriod : Update` |

#### HR Scheduling - Shift Assignment & Replacement

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/human-resource/scheduling-management/shift-assignments/{id}/replace` | Ganti petugas | `ShiftReplacement : Update` |
| `POST` | `.../shift-assignments/{id}/retroactive-correct` | Ubah/koreksi mundur shift | `ShiftReplacement : Update` |

#### HR Scheduling - On-Call Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/on-call-assignments` | Daftar | `OnCallAssignment : Read` |
| `POST` | `.../on-call-assignments` | Jadwalkan | `OnCallAssignment : Create` |
| `POST` | `.../{id}/confirm`, `.../activate`, `.../complete`, `.../call-in` | Tahapan siaga | `OnCallAssignment : Update` |

#### HR Scheduling - Settings

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/settings` | Baca pengaturan | `HrdSchedulingSetting : Read` |
| `PUT` | `.../settings` | Simpan (dengan `rowVersion`) | `HrdSchedulingSetting : Manage` |

Pendukung: `master-data/shift-patterns/options`, `master-data/shifts/options`, `master-data/on-call-types/options`, `departments/options`, `workforce-core/position-assignments`.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 10 JSX + konstanta + util + rute + menu | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 10 JSX | 0 temuan | `PASS` | 3.4 |
| `npm run build` BUILD-2 | 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| Kisi roster tetap dipertahankan sebagai elemen unik | Terpenuhi | `roster-grid.module.css`, sel `<button>` |
| Tidak ada ID contoh | Terpenuhi | Shift, pola, periode, jenis siaga, dan petugas dipilih dari master |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Koreksi mundur di `ShiftReplacementService` di-hardcode 3 hari, mengabaikan `RetroactiveCorrectionWindowDays`; layar pengaturan menampilkan catatan ini |
| Masalah yang diketahui | `ShiftAssignmentItemResponse` tanpa `ShiftCode` (label sel memakai 2 huruf nama shift); `OnCallAssignmentResponse` tanpa `ExpectedResponseMinutes` (pratinjau SLA memakai 30 menit, penilaian akhir oleh server); belum ada UI menambah shift ke sel kosong |
| Dependency backend | Jendela koreksi dari pengaturan, `ShiftCode`, `ExpectedResponseMinutes` |
| Perubahan sampingan | Drawer dan CSS 57 hex dihapus |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Tiga perbaikan backend; uji browser kisi roster |
