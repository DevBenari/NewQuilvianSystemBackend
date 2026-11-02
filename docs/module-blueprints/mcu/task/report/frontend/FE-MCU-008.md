# Laporan Perubahan Frontend — `FE-MCU-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-008` |
| Judul | Booking, jadwal ulang, batal |
| Slice | `MVP-1` (pasangan `BE-MCU-011` ✅) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-008`* |
| Trace | `FR-MCU-010`..`014`; `UAT-03`, `UAT-04`; brief layar [`05-screen-brief.md`](../../../05-screen-brief.md) bagian 1 no. 3, 5.2, 5.5 |
| Contract version | `MCU-API-v1` grup `Mcu Order` (`POST /bookings`, `PATCH /{id}/reschedule`, `PATCH /{id}/cancel`), `Mcu Package` (`GET /packages/options`), `Mcu Daily Schedule` (`GET`) — `approved` 2026-10-21; `VAL-MCU-11`, `15`..`18`, `23`, `46` dari `McuBookingService` |
| Wewenang UI | Brief layar `FE-MCU-000` ✅; sumber opsi berizin `admin/options`: keputusan pemilik 2026-10-27 (`FE-MCU-005`) |
| Dependency | `FE-MCU-007` ✅, `FE-MCU-006` ✅, `BE-MCU-011` ✅ |
| Klasifikasi | `LARGE` — satu repository, ±9 berkas baru + 6 diubah, satu halaman formulir bertahap + dua modal dipakai di dua layar, aturan versi/privasi, kontrak yang sudah ada, tanpa database |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: route, view, hook, utilitas, konstanta booking dan aksi order, sambungan ke daftar dan detail, uji unit + E2E. Backend: laporan ini, tanda roadmap/traceability. devYoga: data UAT (izin user, lalu dibersihkan) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — `UAT-03` dan `UAT-04` terbukti di E2E API-tiruan **dan** terhadap backend lokal |

---

## 1. Keadaan yang ditemukan di awal

- Daftar dan detail order (`FE-MCU-007` ✅) sudah memetakan aksi *Jadwal Ulang*/*Batal* menurut `FE-MCU-001`, tetapi
  belum ada pemasangnya. Lapisan data `FE-MCU-006` ✅ sudah menjaga `version` dan memuat ulang detail saat `409`
  basi.
- Backend `McuBookingService`:
  - pasien terdaftar **atau** identitas sementara wajib (`VAL-MCU-16`), NIK 16 digit angka (`VAL-MCU-17`);
  - tanggal tidak boleh lewat (`VAL-MCU-15`) dan tanggal tutup ditolak (`VAL-MCU-10`);
  - kuota penuh `409` `VAL-MCU-11`, order ganda `409` `VAL-MCU-18`, kerja sama perusahaan `VAL-MCU-21`;
  - alasan batal wajib (`VAL-MCU-23`), alasan jadwal ulang opsional (≤ 400);
  - versi basi `409` `VAL-MCU-46`.
- `GET /patients/options` dijaga policy `KioskRead` (seperti dokter dan perusahaan di `FE-MCU-005`). Sesuai keputusan
  pemilik, pilihan pasien memakai `GET /patients/admin/options` (`Patient : Read`).
- devYoga: paket `UJI-MCU-BASIC` (`FE-MCU-003`) aktif; tanggal 28–29 Oktober belum diatur; belum ada order MCU.

---

## 2. Proses bisnis dari sisi pengguna

1. **Daftar Pasien MCU** menampilkan tombol **+ Booking MCU** bagi pemegang `McuOrder : Create`.
2. **Booking MCU** — halaman tersendiri dengan empat bagian:
   1. **Peserta**: *Pasien terdaftar* (cari No. RM/NIK/nama; deskripsi opsi hanya No. RM) **atau** *Peserta baru*
      (nama, NIK, tanggal lahir, HP). NIK yang ditempel dengan pemisah (`3201-2301-0190-0001`) dibersihkan menjadi
      16 digit.
   2. **Penjamin**: *Perorangan* atau *Perusahaan* (pilih perusahaan).
   3. **Paket**: dari `packages/options?companyGuarantorId&date`. Labelnya menampilkan harga yang berlaku, misalnya
      *UJI Paket MCU Basic — Rp 350.000 (Harga umum)*. Persyaratan tampil begitu paket dipilih. Paket yang tidak
      tersedia lagi untuk penjamin/tanggal baru dianggap belum dipilih.
   4. **Tanggal**: bawaan hari ini; *Sisa kuota: n dari m* dari jadwal harian. Tanggal tutup ditolak layar (*MCU tutup
      pada tanggal ini. Pilih tanggal lain.*).

   Isian wajib diperiksa layar dengan kalimat server. **Kuota penuh, order ganda, dan kerja sama perusahaan tidak
   diperiksa layar**: server yang menolak, dan kalimatnya tampil apa adanya di atas tombol, dengan isian tetap.
   Sesudah simpan, layar kembali ke Daftar Pasien MCU pada **tanggal booking** dengan pesan *"Booking MCU-RSMMC-…
   tersimpan."*
3. **Jadwal Ulang** (modal, dari menu ⋮ daftar dan header detail; `McuOrder : Update`):
   - isian tanggal baru (tidak lewat, tidak sama dengan tanggal sekarang, tidak tutup) dengan sisa kuotanya;
   - alasan opsional.
4. **Batal** (modal, order *Terjadwal*; `McuOrder : Cancel`): alasan wajib. Order menjadi *Dibatalkan* dan kuota
   dikembalikan.
5. **Versi** — `version` dibaca dari order terbaru di store saat dikirim. Bila petugas lain lebih dulu mengubah
   order (`409` *diubah petugas lain*), detail dimuat ulang, pesan server tampil di modal, dan kiriman ulang memakai
   versi baru.
6. Sesudah aksi berhasil: daftar dan ringkasan dimuat ulang (order pindah tanggal/status); detail diganti jawaban
   server dan riwayatnya dimuat ulang.

*Check-in* dan *Batal Penuh* tetap *Fitur belum tersedia* (`FE-MCU-009`, `011`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah (frontend)

| Berkas | Perubahan |
| --- | --- |
| `src/app/…/mcu-patients/booking/page.jsx` (baru) | Route *Booking MCU* |
| `src/app/…/mcu-patients/page.jsx`, `mcu-patients-client.jsx` | Membaca `?date` dan `?booked` dari `searchParams` (komponen server, divalidasi pola) |
| `src/components/view/…/mcu-patients/booking/mcu-booking-view.jsx` (baru) | Halaman booking empat bagian |
| `src/components/view/…/mcu-patients/mcu-order-action-modals.jsx` (baru) | Modal Jadwal Ulang dan Batal |
| `src/components/view/…/mcu-patients/mcu-patient-list-view.jsx` | Tombol *+ Booking MCU*, pesan sukses, modal, toast |
| `src/components/view/…/mcu-patients/detail/mcu-order-detail-view.jsx` | Modal dan toast |
| `src/lib/hooks/…/mcu-patients/use-mcu-booking.jsx`, `use-mcu-order-actions.jsx` (baru) | Logika booking dan aksi order |
| `src/lib/hooks/…/mcu-patients/use-mcu-patient-list.jsx`, `use-mcu-order-detail.jsx` | Pemasang aksi `reschedule`/`cancel`, muat ulang sesudah aksi |
| `src/utils/…/mcu-patients/mcu-booking-utils.js` (baru) | Form, validasi (kalimat server), payload, label paket, sisa kuota, versi terbaru |
| `src/lib/constants/…/mcu-patients/mcu-booking-constants.js` (baru) | Mode peserta/penjamin, batas, resource pasien `admin/options`, salinan teks |
| `tests/unit/mcu-booking-rules.test.mjs` (baru) | 8 uji |
| `tests/e2e/mcu-booking-screen.spec.mjs` (baru) | 6 skenario API-tiruan |
| `tests/e2e/mcu-patient-list-screen.spec.mjs` | Harapan menu ⋮ diperbarui: *Jadwal Ulang*/*Batal* kini aktif |

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice → service → InstanceAxios`. Booking dan aksi memakai thunk `FE-MCU-006`; opsi paket
  memakai thunk `fetchMcuPackageOptions` (`FE-MCU-002`). Sisa kuota memakai service jadwal secara lokal, seperti
  pola modalitas di editor paket, agar slice halaman Jadwal & Kuota tidak tertimpa.
- Komponen yang sudah ada: `Hero`, `BaseSelectField`/`BaseTextField`/`BaseDateField`/`BaseTextAreaField`,
  `ConfirmModal`, `InformationAlert`, `ToastStack`, dan kartu `editorStyles.editorCard`. Radio memakai kelas
  Bootstrap `form-check`. Tidak ada komponen, CSS Module, atau dependency baru.
- Tanggal dan pesan sesudah booking dikirim lewat query halaman dan dibaca `searchParams` di komponen server — tanpa
  `useSearchParams` di klien.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Opsi pasien/perusahaan/paket dengan penanda muat; tombol simpan dan modal terkunci selama kiriman |
| Validasi | Isian wajib di layar dengan kalimat server; NIK/HP dibersihkan saat diketik |
| Ditolak server | Booking: di atas tombol simpan; modal: di dalam modal. Apa adanya, isian tetap |
| Data basi | `409` `VAL-MCU-46` → detail dimuat ulang, pesan tampil, kirim ulang memakai versi baru |
| Tanpa hak akses | Tanpa `Create` → tanpa tombol booking dan halaman booking akses ditolak; aksi menurut izin (`FE-MCU-001`) |
| Privasi | Opsi pasien hanya menampilkan nama + No. RM, tanpa NIK utuh; NIK di daftar tetap tersamar |

---

## 5. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/medical-checkup-management/orders/bookings` | Booking | `McuOrder : Create` |
| `PATCH` | `…/orders/{id}/reschedule` | Jadwal ulang (`version`) | `McuOrder : Update` |
| `PATCH` | `…/orders/{id}/cancel` | Batal (`version`) | `McuOrder : Cancel` |
| `GET` | `…/packages/options?companyGuarantorId&date` | Paket + harga berlaku + persyaratan | `McuPackage : Read` |
| `GET` | `…/daily-schedules?from&to` | Sisa kuota tanggal | `McuSchedule : Read` |
| `GET` | `/v1/health-services/patient-management/master-data/patients/admin/options` | Pasien terdaftar | `Patient : Read` |
| `GET` | `/v1/administrator/master-data/company-guarantors/admin/options` | Perusahaan | `CompanyGuarantor : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-booking-rules.test.mjs` | 8/8 lulus | `PASS` | `# pass 8` |
| E2E API-tiruan `tests/e2e/mcu-booking-screen.spec.mjs` | 6/6 lulus | `PASS` | `6 passed` |
| Cacat yang tertangkap E2E | `maxLength` 16 pada NIK memotong NIK yang ditempel dengan pemisah sebelum dibersihkan (hasil 13 digit). Diperbaiki: ruang 32 karakter untuk teks mentah; batas 16 digit dijaga normalisasi | `PASS` | Keluaran Playwright sebelum/sesudah |
| **UAT-03** (E2E) | Isian kosong ditolak layar tanpa permintaan; peserta baru → body persis kontrak (tanpa `patientId`, perorangan); persyaratan dan *Sisa kuota: 4 dari 30* tampil; kembali ke daftar `?date=…&booked=…`, pesan *Booking … tersimpan.*, order baru di tabel | `PASS` | Skenario *UAT-03* |
| **UAT-04** (E2E) | Kuota 30/30 → `409` `VAL-MCU-11`, kalimat server persis di atas tombol, isian tetap, tetap di halaman booking | `PASS` | Skenario *UAT-04* |
| Pasien terdaftar + perusahaan | Opsi dari `patients/admin/options` dan `company-guarantors/admin/options`; paket terkunci sampai perusahaan dipilih; `packages/options?companyGuarantorId&date`; harga perusahaan *Rp 300.000*; tanggal tutup ditolak layar; body persis | `PASS` | Skenario *pasien terdaftar…* |
| Jadwal ulang dari menu ⋮ | Tanggal wajib; sisa kuota tanggal baru; `409` `VAL-MCU-46` → `GET` detail, pesan di modal; kirim ulang memakai versi 5 (bukan 2); toast; daftar dimuat ulang | `PASS` | Skenario *jadwal ulang…* |
| Batal dari header detail | Check-in nonaktif *Fitur belum tersedia*; alasan wajib; body `{ reason, version }`; detail menjadi *Dibatalkan* dengan alasan; Check-in hilang; riwayat dimuat ulang | `PASS` | Skenario *batal…* |
| Tanpa `McuOrder : Create` | Tanpa tombol booking; `/booking` → akses ditolak | `PASS` | Skenario *tanpa…* |
| Regresi seluruh spec MCU (paket, referensi, jadwal, daftar, booking) | 39/39 lulus | `PASS` | `39 passed` |
| **UAT tulis, backend lokal + devYoga** (izin user; tulis layar diizinkan per langkah) | (0) Jadwal 28–29 Okt dibuka kuota 1 lewat API. (1) **UAT-03**: booking *UAT FEMCU BUDI* (NIK uji ditempel berpemisah) paket `UJI-MCU-BASIC` 28-10 → `200`, order `MCU-RSMMC-000001` *Terjadwal*, daftar 28-10 dengan pesan, NIK `9900********0801`, kuota 1/1. (2) **UAT-04**: *UAT FEMCU SITI* 28-10 → `409` *"Kuota MCU tanggal 28-10-2026 sudah penuh (1/1). Minta persetujuan kepala unit untuk melampaui."* tampil apa adanya, isian tetap. (3) Jadwal ulang ke 29-10 dari menu ⋮ (`version` 0, sisa kuota 1/1 di modal) → kuota 28-10 0/1, 29-10 1/1. (4) Batal dari detail (`version` 1) → *Dibatalkan*; riwayat mencatat ketiga perpindahan. (5) Jadwal 28–29 ditutup kembali (0/0). GET non-200 nihil | `PASS` | Log skrip + tangkapan layar |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.890 uji: 3.848 lulus, 42 gagal — daftar nama **identik** dengan baseline `FE-MCU-006` (semua di luar MCU) | `EXISTING / ENVIRONMENT ISSUE` | `diff` kosong |
| `npm run lint:errors` | Exit 0 (berkas task tanpa peringatan sesudah dua peringatan React Compiler di hook booking diperbaiki) | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0: *Compiled successfully*, route `…/mcu-patients/booking` terbentuk | `PASS` | Keluaran build |

`MANUAL TEST: PASS` — radio peserta/penjamin, select pasien/perusahaan/paket (dengan pencarian), kalender tanggal
lahir dan pemeriksaan, sisa kuota, simpan (berhasil/ditolak), modal jadwal ulang dan batal (validasi, basi,
berhasil) dijalankan lewat peramban (Playwright) pada E2E dan UAT tulis.
`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU; 8/8 uji baru PASS)`.

### Data uji devYoga sesudah UAT

| Data | Keadaan akhir |
| --- | --- |
| Order `MCU-RSMMC-000001` (*UAT FEMCU BUDI*, identitas sementara, paket `UJI-MCU-BASIC`) | *Dibatalkan* (29-10-2026, alasan *UAT FE-MCU-008 dibersihkan*) |
| Jadwal MCU 28 dan 29 Oktober 2026 | Diatur lalu ditutup kembali (buka = tidak, kuota 0, terpakai 0) |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-03` — booking peserta baru → *Terjadwal*, belum punya No. RM | Terpenuhi | §6 (E2E + sungguhan) |
| `UAT-04` — kuota penuh → pesan server apa adanya | Terpenuhi | §6 (E2E + sungguhan) |
| DoD: kriteria terbukti | Terpenuhi | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Tindakan pemilik | Jabatan admisi MCU perlu `Patient : Read` (pilihan pasien terdaftar) dan `CompanyGuarantor : Read` (penjamin perusahaan), di samping izin `McuOrder` |
| Selisih terhadap brief | Kalender tidak dapat menonaktifkan tanggal tertentu, karena `FilterDatePicker` hanya punya batas `min`/`max`. Tanggal tutup tetap tidak dapat dipakai: layar menampilkan *MCU tutup pada tanggal ini.* dan menolak simpan dengan kalimat yang sama |
| Masalah yang diketahui | `VAL-MCU-18` (order ganda) dan `VAL-MCU-21` (kerja sama perusahaan) hanya terbukti lewat jalur pesan server yang sama dengan `VAL-MCU-11`, belum dengan data sungguhan |
| Dependency backend | Tidak ada; `BE-MCU-011` ✅ |
| Perubahan sampingan | `NONE` di luar sambungan daftar/detail. devYoga: lihat §6 *Data uji* |
| Interupsi | Skrip UAT berhenti sekali karena memotong jawaban server sebelum mem-parse JSON (cacat skrip). Dilanjutkan dari langkah 2 tanpa mengulang langkah yang sudah menulis |
| Status Git (frontend) | ` M` berkas `FE-MCU-006`/`007` sebelumnya + `menu-items.jsx`, `mcu-package-screen.spec.mjs`; `??` folder `mcu-patients` (route, view, hook, konstanta, utilitas), uji baru |
| Langkah berikutnya | `FE-MCU-009` (Check-in dan *walk-in* admisi) — siap |
