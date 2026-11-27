# Laporan Perubahan Frontend — `FE-HRD-314`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-314` |
| Judul | Jadwal Saya tersambung ke server |
| Slice | `A52` / `MVP-68` / `EPIC HRD-113` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-314` |
| Trace | `FR-PK-016`; `HRD-DEC-209`; `HRD-ASM-80` (`draft`, diterapkan server sesuai kontrak `v26`); `VR-PK-29`; `03-frontend-architecture.md` 31.2 baris "Jadwal Saya (ubah)" |
| Contract version | `v26` — `api-contract.md` bagian 32 "Self Services / Human Resource / My Schedule", `approved` 12 November 2026. Tanpa delta kontrak |
| Wewenang UI | Layar yang ada `/self-services/human-resource/schedule` diubah (kartu: "Service ESS dan view jadwal"). Tab **Jadwal Saya** dan **Roster Unit**, pemilih bulan: `DEV_DISCRETION`; base component wajib. Tidak ada rute atau menu baru (31.1: layar tetap di menu layanan mandiri) |
| Dependency | `FE-HRD-306` ✅ (`getMySchedule`, `getMyUnitRoster`), `BE-HRD-348` ✅ |
| Klasifikasi | `MEDIUM` — satu view ditulis ulang, satu service dirapikan, satu utilitas, satu modul gaya, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 24 November 2026: "lanjutkan FE-HRD-314") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/components/view/self-services/human-resource/schedule`, `src/lib/services/hr/employee-self-service.js`, `src/utils/hr/self-service`, `src/style/hr/self-service`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `004524f4c` (branch `YogaV2`) + working tree `FE-HRD-313` |
| Commit backend yang dijadikan rujukan | `4d2c0162` (branch `yoga`) |
| Tanggal | 24 November 2026 |
| Status | ✅ Selesai — acceptance criteria terpenuhi di browser dengan API tiruan; `UAT-PK-12`/`-13` dengan data nyata devYoga belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Layar Jadwal memanggil fungsi yang selalu gagal | `schedule-employee-view.jsx:53` memanggil `getMyScheduleAssignments`, yang melempar "Daftar jadwal kerja milik sendiri belum tersedia di server." | Pegawai selalu melihat galat; tidak ada jadwal yang tampil |
| Kolom tabel mengarang nilai | Jam bawaan "07:00 - 15:00" bila server kosong; status penugasan memakai `customLabel` yang tidak dikenal `StatusBadge` | Jam palsu tampil seolah jadwal sungguhan |
| Klien server sudah ada | `getMySchedule`, `getMyUnitRoster` dari `FE-HRD-306` (diuji `hr-a52-api-client.test.mjs`) | Task ini cukup menyambungkan view, tanpa service baru |
| Waktu dari server berbentuk UTC | `MyScheduleSelfService` menyimpan dan mengirim `StartAt`/`EndAt` UTC; tanggal hari dihitung WIB | Jam harus ditampilkan dalam WIB, bukan zona peramban |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: pegawai (`MySchedule : Read`). Identitas pegawai diambil server dari sesi; layar tidak pernah mengirim nomor pegawai.

**Contoh UAT-PK-12** (`HRD-DEC-209`). Rina, perawat ICU, membuka **Jadwal Saya & Tukar Shift**.

1. Bulan berjalan terpilih, misalnya "November 2026 (bulan ini)". Pilihan bulan hanya lima: tiga bulan ke belakang sampai bulan depan (`HRD-ASM-80`), sehingga bulan di luar batas tidak dapat diminta dari layar.
2. Tab **Jadwal Saya** menampilkan ringkasan (Shift Terbit, Berubah Sesudah Terbit, Siaga, Cuti/Sakit/Dinas) dan satu baris per hari dari server:
   - Minggu 15: **Bertugas**, "Pagi · 07.00–14.00", "ICU · ROST-…".
   - Rabu 18: "Malam · 21.00–07.00" dengan tanda **"Berubah — sebelumnya Siang 14.00–21.00"**.
   - Jumat 20: "Pagi · 07.00–14.00 tidak lagi milik Anda (Ganti Petugas)" untuk shift yang dialihkan ke petugas lain.
   - Sabtu 21: siaga "Siaga ICU · 17.00–05.00".
   - Selasa 24: **Cuti**, keterangan "Cuti Tahunan" (jenis cuti hanya untuk diri sendiri).
   - Rabu 25: **Hari Libur**, keterangan nama hari libur.
   - Pegawai jam kantor melihat jadwal tetapnya, misalnya "Jadwal Kantor · 08.00–16.00", dengan keterangan "Jadwal kerja tetap".
3. Tab **Roster Unit** menampilkan matriks roster terbit unit (unit penempatan utama dan unit penempatan sementara): satu baris per rekan, satu kolom per tanggal, hanya kode shift. Baris Rina bertanda "Saya". Budi yang cuti pada tanggal 15 tampil **"Libur"**, sama dengan hari libur biasa; alasan tidak pernah dikirim server dan layar tidak menebaknya.
4. Bulan depan yang rosternya masih `Draft` tampil kosong: "Belum ada jadwal terbit untuk Desember 2026." dan "Belum ada roster terbit untuk unit Anda pada Desember 2026."

Tombol **+ Ajukan Tukar Shift** dan modalnya tidak diubah (di luar cakupan kartu).

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Akun belum terhubung ke profil pegawai (404) | Pesan server dan tombol **Coba Lagi** |
| Rentang ditolak server (400, `VR-PK-29`) | Pesan server dan **Coba Lagi**; secara normal tidak terjadi karena pilihan bulan sudah dibatasi |
| Gagal memuat roster unit | Pesan server dan **Coba Lagi** di tab Roster Unit |
| Tanpa hak `MySchedule : Read` | "Anda tidak berwenang membuka Jadwal Saya."; tidak ada permintaan jadwal |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-314`; `api-contract.md` §32 My Schedule; `00-interview-decisions.md` `HRD-DEC-209`, `HRD-ASM-80`; `04-prd-to-mvp.md` `UAT-PK-12`, `-13`; `03-frontend-architecture.md` 31.1–31.2; laporan `BE-HRD-348`; `MyScheduleSelfServiceDtos.cs`, `MyScheduleSelfService.cs` (zona waktu, label status hari); `schedule-employee-view.jsx`, `employee-self-service.js`, `hr-a52-api-client.test.mjs`; `use-hr-remote-data.js`; `roster-matrix-detail-view.jsx`, `roster-grid.module.css`, `employee-360-view.jsx` (pola tab); `information-alert.jsx`, `status-badge.jsx`, `data-table.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/self-services/human-resource/schedule/schedule-employee-view.jsx` | Ditulis ulang: pemilih bulan, tab Jadwal Saya (ringkasan + tabel per hari) dan Roster Unit (matriks per unit), hak `MySchedule : Read`, galat + Coba Lagi. Modal tukar shift dipertahankan; muat ulang sesudah pengajuan memakai `schedule.reload` |
| `src/lib/services/hr/employee-self-service.js` | Fungsi `getMyScheduleAssignments` (selalu melempar galat) dihapus; pemakainya hanya view ini |
| `src/utils/hr/self-service/my-schedule-utils.js` (baru) | Fungsi murni: bulan berjalan WIB, pilihan bulan `HRD-ASM-80`, rentang bulan, kolom tanggal, jam UTC → WIB, teks "Berubah — sebelumnya …", nada status hari, ringkasan, baris matriks roster unit ("Libur" untuk tanggal tanpa shift) |
| `src/style/hr/self-service/my-schedule.module.css` (baru) | Sel matriks baca-saja, sorotan baris "Saya", susunan teks kolom jadwal; seluruhnya token |
| `tests/unit/hr-my-schedule-utils.test.mjs` (baru) | 6 uji |
| `tests/e2e/hr-my-schedule.spec.mjs` (baru) | 4 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Data memakai `EmployeeSelfService` (Axios) lewat `useHrRemoteData`, sama seperti layar `A52` lain; tidak ada slice atau service baru. Hak memakai `useStrictPermissions`. Tab mengikuti pola `BaseButton role="tab"` + `tabList` dari Profil Pegawai 360. Matriks roster unit memakai `roster-grid.module.css` dari kisi roster HR, dengan sel `span` baca-saja. Seluruh kontrol memakai base component. Status hari memakai label dari server (`dayStatusLabel`); layar hanya memilih nada lencana.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Mengambil jadwal Anda..."; ringkasan memuat; tab Roster Unit "Mengambil roster unit..."; pemilih bulan nonaktif selama permintaan |
| Kosong | "Belum ada jadwal terbit untuk {bulan}." / "Belum ada roster terbit untuk unit Anda pada {bulan}." |
| Gagal | Pesan server dan **Coba Lagi** per tab |
| Tanpa hak akses | "Anda tidak berwenang membuka Jadwal Saya."; tanpa permintaan data |
| Privasi | Roster unit hanya nama dan kode shift; tidak ada alasan ketidakhadiran rekan |

---

## 5. Endpoint yang dikonsumsi

#### Self Services / Human Resource / My Schedule

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/self-services/human-resource/my-schedule` | Jadwal sendiri; query hanya `fromDate`, `toDate` (satu bulan) | `MySchedule : Read` |
| `GET` | `/v1/self-services/human-resource/my-schedule/unit-roster` | Roster terbit unit; query hanya `month`; diminta saat tab Roster Unit dibuka | `MySchedule : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-my-schedule.spec.mjs` (build produksi, port 3710, API tiruan, nol tulis) | 4/4 lulus | `PASS` | Keluaran Playwright |
| — `UAT-PK-12` Jadwal Saya | Jam WIB dari UTC (Pagi 07.00–14.00; Malam 21.00–07.00); "Berubah — sebelumnya Siang 14.00–21.00"; shift dialihkan; siaga 17.00–05.00; Cuti + "Cuti Tahunan"; Hari Libur + nama | `PASS` | Skenario 1 |
| — `UAT-PK-13` (bagian layar) | Query `my-schedule` tepat `{ fromDate, toDate }`; `unit-roster` tepat `{ month }` — tanpa identitas pegawai | `PASS` | Skenario 1 dan 2 |
| — `UAT-PK-12` Roster Unit | Roster unit baru diminta saat tab dibuka; Tono "P" tanggal 15, Budi "Libur" tanggal 15 dan "S" tanggal 16; baris "Saya"; teks tabel tanpa kata cuti/sakit/dinas/tugas belajar; pemilih bulan 5 pilihan; bulan depan (draft) → kedua tab kosong dengan permintaan rentang bulan depan | `PASS` | Skenario 2 |
| — 404 akun tanpa profil | Pesan server; **Coba Lagi** memuat ulang dan jadwal tampil | `PASS` | Skenario 3 |
| — Tanpa hak | Pesan tidak berwenang; nol permintaan `my-schedule` | `PASS` | Skenario 4 |
| Pemeriksaan visual (tangkapan layar kedua tab) | Dua perbaikan dari pemeriksaan ini: footer "0 dari 0 data" dihapus (`pagination={false}`, satu bulan ≤ 31 baris); "Tidak Ada Jadwal" tidak lagi merah (nada `info`) | `PASS` | Tangkapan di scratchpad sesi |
| `tests/unit/hr-my-schedule-utils.test.mjs` + regresi `hr-a52-api-client.test.mjs` | 6/6 + 12/12 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 6 mutasi (batas 4 bulan; bulan berjalan tanpa geser WIB; jam tampil UTC; jadwal sebelumnya tidak disebut; rekan tanpa shift tidak "Libur" dan memuat alasan; akhir bulan salah) | 6/6 tertangkap; source dipulihkan identik byte | `PASS` | `mut314.cjs` di scratchpad sesi |
| Seluruh uji unit | 5.274 lulus, 100 gagal dari 5.374 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik per nama dengan putaran `FE-HRD-313`; seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npx eslint` atas berkas task | Tanpa error maupun peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil | `PASS` | Keluaran perintah |

AUTOMATED TEST: unit — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — kontrol yang diubah (pemilih bulan, tab Jadwal Saya/Roster Unit, Coba Lagi) dijalankan di peramban Chromium lewat Playwright terhadap build produksi. Modal tukar shift tidak diubah dan tidak diuji ulang.

**Tidak dijalankan:** `UAT-PK-12`/`-13` dengan data nyata devYoga. Izin `MySchedule : Read` belum diberikan ke jabatan mana pun di devYoga (`BE-HRD-348` §3.3); saat uji backend izin diberikan sementara lalu dikembalikan. Perilaku server (roster `Draft` tidak dibaca, parameter pegawai diabaikan, alasan rekan tidak dikirim) dibuktikan `AT-HRD-PK-42`–`44` di laporan `BE-HRD-348`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tanda "Berubah" | Terpenuhi | Skenario 1; uji unit 4; mutasi M4 |
| Ketidakhadiran rekan tampil "Libur" | Terpenuhi | Skenario 2; uji unit 5; mutasi M5 |
| Rentang dibatasi | Terpenuhi — pilihan bulan hanya 3 bulan ke belakang sampai bulan depan menurut WIB; satu permintaan = satu bulan | Skenario 2; uji unit 1–2; mutasi M1, M2, M6 |
| Outcome: `getMyScheduleAssignments` diganti panggilan `my-schedule`; tab roster unit | Terpenuhi | Bagian 3.2; skenario 1–2 |
| DoD umum: base component, memuat/kosong/galat/coba lagi, tidak berwenang, uji unit dan browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-12`, `UAT-PK-13` | Bagian layar terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Skenario 1–2 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) `HRD-ASM-80` masih `draft` di decision log; layar mengikuti batas yang diterapkan server dan dikunci kontrak `v26`. Bila asumsi diubah, cukup ubah `MY_SCHEDULE_MONTHS_BACK`/`AHEAD`. (2) Tukar shift yang disetujui belum bertanda "Berubah" karena persetujuan tukar shift belum menulis riwayat revisi (delta `BE-HRD-348` §3.4) |
| Masalah yang diketahui | Izin `MySchedule : Read` belum diberikan ke jabatan mana pun — sampai diberikan lewat Role Access, pegawai melihat pesan tidak berwenang. Modal tukar shift lama (di luar cakupan) tidak memiliki pemilih rekan sehingga `targetWorkforceProfileId` selalu kosong; tidak diubah di task ini |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` — keluaran Playwright diarahkan ke scratchpad; spec sementara tangkapan layar sudah dihapus |
| Interupsi | `NONE` |
| Status Git | ` M src/components/view/self-services/human-resource/schedule/schedule-employee-view.jsx`, ` M src/lib/services/hr/employee-self-service.js`, `?? src/utils/hr/self-service/`, `?? src/style/hr/self-service/`, `?? tests/unit/hr-my-schedule-utils.test.mjs`, `?? tests/e2e/hr-my-schedule.spec.mjs`; ditambah berkas `FE-HRD-313` yang belum di-commit. Repository backend: laporan ini (baru), ` M roadmap/frontend-roadmap.md`, ` M roadmap/requirement-traceability.md`. Tidak ada commit |
| Perbaikan sesudah verifikasi | `FE-HRD-316` (24 November 2026, data asli): `shiftCode` server ternyata kode sistem master shift ("SFT-RSMMC-00003"), terpotong di sel Roster Unit. Label sel kini dua huruf awal nama shift (sama dengan kisi roster HR), kode sistem di tooltip — lihat laporan `FE-HRD-316` P2 |
| Langkah berikutnya | `FE-HRD-315` (tujuh layar laporan dengan ekspor Excel; butuh `BE-HRD-349`, `BE-HRD-350`) |
