# Laporan Perubahan Frontend — `FE-MCU-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-009` |
| Judul | Check-in dan *walk-in* admisi |
| Slice | `MVP-1` (pasangan `BE-MCU-013` ✅, `BE-MCU-014` ✅) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-009`* |
| Trace | `FR-MCU-020`..`025`; `MCU-DEC-039`, `042`; `UAT-06`, `UAT-07`, `UAT-08`; brief layar [`05-screen-brief.md`](../../../05-screen-brief.md) bagian 5.3, 5.4 |
| Contract version | `MCU-API-v1` grup `Mcu Check In` (`GET patient-matches`, `GET check-in/preview`, `POST check-in`) dan `Mcu Order` (`POST walk-ins`) — `approved` 2026-10-21; `VAL-MCU-24`..`30`, `46` dari `McuCheckInService`; Patient Management `POST patients` (dipakai ulang lewat formulir PM) |
| Wewenang UI | Brief layar `FE-MCU-000` ✅. Keputusan pemilik 2026-10-27: (1) formulir pasien PM diperluas **aditif** dengan isian awal sekali pakai + alamat kembali; (2) opsi berizin `admin/options` |
| Dependency | `FE-MCU-007` ✅, `FE-MCU-006` ✅, `BE-MCU-013` ✅, `BE-MCU-014` ✅ |
| Klasifikasi | `LARGE` — dua repository-area (MCU + Patient Management), ±12 berkas baru + 9 diubah, dua halaman formulir, aturan keselamatan identifikasi pasien dan privasi, kontrak yang sudah ada, tanpa database |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: route, view, hook, utilitas, konstanta check-in dan *walk-in*; perluasan aditif Patient Management (utilitas isian awal, hook editor, view, halaman tambah); sambungan daftar/detail; uji unit + E2E. Backend: laporan ini, tanda roadmap/traceability. devYoga: data UAT (izin user) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — `UAT-06`, `UAT-07`, dan *walk-in* terbukti di E2E API-tiruan dan terhadap backend lokal; `UAT-08` di E2E API-tiruan |

---

## 1. Keadaan yang ditemukan di awal

- Daftar/detail order (`FE-MCU-007` ✅) memetakan aksi *Check-in*; lapisan data (`FE-MCU-006` ✅) menyediakan
  `patient-matches`, `check-in/preview`, `check-in` (ber-`version`, `409` basi memuat ulang pratinjau), dan
  `walk-ins`.
- Backend `McuCheckInService`:
  - pencocokan pasien: `Linked`, `FullMatch`, `NikMatchBirthDateDiffers`, `NoMatch`;
  - NIK sama tetapi tanggal lahir beda ditolak tanpa `matchConfirmation` (`VAL-MCU-25`); pasien yang didaftarkan
    sesudah booking dengan NIK + tanggal lahir sama otomatis *cocok penuh* (metode *Pasien baru didaftarkan*);
  - dokter wajib (`VAL-MCU-26`); persyaratan wajib ditandai (`VAL-MCU-27`); *Lanjut dengan catatan* wajib catatan
    (`VAL-MCU-28`); tanggal susulan sesudah hari ini dan buka;
  - tarif nonaktif (`VAL-MCU-29`), kartu penjamin perusahaan wajib (`VAL-MCU-30`), hanya pada tanggal pemeriksaan
    (`VAL-MCU-24`, juga `checkInBlockedReason` pratinjau).
- `CreateMcuWalkInRequest` menuntut `PatientId`, dan `packages/options` tidak membawa id persyaratan. Id itu ada di
  detail paket dan sama dengan `PackageComponentRequirementId`.
- Formulir pasien Patient Management (`PatientsFormView`) **tidak** menerima isian awal dan selalu berpindah ke
  detail pasien sesudah simpan. Formulir pasien IGD terikat alur `react-hook-form` pendaftaran IGD. Keputusan
  pemilik: perluasan aditif PM.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Check-in — `…/mcu-patients/[orderId]/check-in`

Dibuka dari menu ⋮ daftar atau aksi utama detail (*Terjadwal* pada hari pemeriksaan). URL bertoken, tanpa UUID.

1. **Cocokkan pasien**
   - *Tertaut sejak booking*: tidak ada yang dipilih.
   - *Cocok penuh*: NIK + tanggal lahir sama; pasien ditautkan otomatis (RM tampil).
   - *NIK sama, tanggal lahir beda*: peringatan memuat kedua tanggal (booking vs data pasien) dan **centang wajib**
     *"Saya sudah memeriksa KTP dan pasien ini orang yang sama"*. Tanpa centang, layar menolak tanpa mengirim. Ada
     juga tombol *Bukan orang yang sama — Buat Pasien Baru*.
   - *Tidak ada*: **Buat Pasien Baru** membuka formulir Tambah Pasien PM berisi nama, NIK, tanggal lahir, dan HP dari
     booking. NIK **tidak** lewat URL: hanya token acak, isinya di `sessionStorage`. Sesudah simpan (atau batal),
     layar kembali ke check-in; pencocokan dimuat ulang dan menjadi *cocok penuh*. Tersedia juga *Muat ulang
     pencocokan*.
2. **Dokter MCU** — bawaan dokter jaga tanggal itu (*dr. … (dokter jaga)*), boleh diganti (`doctors/admin/options`).
   Order perusahaan menampilkan pilihan **kartu penjamin** pasien (bawaan kartu utama).
3. **Persyaratan** — setiap persyaratan dipilih *Terpenuhi* / *Lanjut dengan catatan* (catatan wajib) / *Jadwal
   ulang komponen* (tanggal susulan sesudah hari ini). Tidak ada pilihan bawaan.
4. **Ringkasan harga** — paket, sumber harga (umum/perusahaan + masa berlaku), dan nominal dari pratinjau.

Tombol **Check-in** nonaktif selama permintaan berjalan, sehingga klik ulang tidak mengirim dua kali. Penolakan
server (konfirmasi KTP, kartu, tarif, versi basi) tampil di atas tombol apa adanya, dan isian tetap. Check-in yang
tidak boleh dilakukan (bukan hari pemeriksaan, sudah check-in, status lain) menampilkan alasan server dan
menonaktifkan tombol. Sesudah berhasil, layar berpindah ke detail order (*Hadir*).

### 2.2 Pendaftaran *Walk-in* — `…/mcu-patients/walk-in`

Tombol **+ Walk-in** di daftar (perlu `McuOrder : Create` + `CheckIn`). Halaman berisi:

1. Peserta: pasien terdaftar, atau **Daftarkan Pasien Baru** lewat formulir PM dan kembali ke halaman ini; No. HP
   opsional.
2. Penjamin: perorangan atau perusahaan.
3. Paket dan tanggal: tanggal terkunci **hari ini**, dengan sisa kuota.
4. Dokter MCU: bawaan dokter jaga hari ini.
5. Persyaratan: dari detail paket.
6. Ringkasan harga.

Tombol **Daftarkan & Check-in** menjadikan order langsung *Hadir*, lalu layar berpindah ke detailnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas MCU

| Berkas | Perubahan |
| --- | --- |
| `src/app/…/mcu-patients/[orderId]/check-in/page.jsx`, `walk-in/page.jsx` (baru) | Route check-in dan *walk-in* |
| `src/components/view/…/mcu-patients/check-in/mcu-check-in-view.jsx`, `mcu-requirement-outcomes.jsx` (baru) | Halaman check-in; penandaan persyaratan (dipakai bersama) |
| `src/components/view/…/mcu-patients/walk-in/mcu-walk-in-view.jsx` (baru) | Halaman *walk-in* |
| `src/lib/hooks/…/mcu-patients/use-mcu-check-in.jsx`, `use-mcu-walk-in.jsx` (baru) | Logika check-in dan *walk-in* |
| `src/utils/…/mcu-patients/mcu-check-in-utils.js` (baru) | Keadaan pencocokan, isian awal pasien, penandaan + validasi (kalimat server), payload check-in/*walk-in*, ringkasan harga, kartu penjamin, persyaratan dari detail paket |
| `src/lib/constants/…/mcu-patients/mcu-check-in-constants.js` (baru) | Hasil persyaratan, status pencocokan, batas, route Tambah Pasien PM |
| `use-mcu-patient-list.jsx`, `mcu-patient-list-view.jsx`, `use-mcu-order-detail.jsx` | Aksi *Check-in* (menu ⋮ dan header detail) dan tombol *+ Walk-in* |
| `tests/unit/mcu-check-in-rules.test.mjs` (baru) | 7 uji |
| `tests/e2e/mcu-check-in-screen.spec.mjs` (baru) | 7 skenario API-tiruan |
| `tests/e2e/mcu-patient-list-screen.spec.mjs`, `mcu-booking-screen.spec.mjs` | Harapan diperbarui: *Check-in* kini aktif |

### 3.2 Perluasan aditif Patient Management (keputusan pemilik 2026-10-27)

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/patient-management/master-data/patient/patient-prefill-utils.js` (baru) | `storePatientPrefill`/`takePatientPrefill`: isian awal **sekali pakai** di `sessionStorage` (kedaluwarsa 30 menit), hanya ruas `fullName`, `birthDate`, `identityType`, `identityNumber`, `phoneNumber`; alamat kembali hanya path internal (`/…`, menolak `//`, `\`, skema) |
| `src/lib/hooks/…/patients/use-master-data-patient-editor.jsx` | Parameter opsional `prefillToken`: mode tambah mengisi form dari isian awal; sesudah simpan/batal kembali ke alamat asal. **Tanpa token, perilaku tidak berubah** |
| `src/components/view/…/patients/add/patients-form-view.jsx` | Meneruskan `prefillToken` |
| `src/app/health-services/patient-management/master-data/patients/create/page.jsx` | Membaca `?prefill=<token>` (pola token divalidasi) dari `searchParams`; halaman kini dinamis |

### 3.3 Kepatuhan arsitektur frontend

- Alur `view → hook → slice (FE-MCU-006) → service → InstanceAxios`. Opsi paket memakai thunk `FE-MCU-002`. Detail
  paket (persyaratan *walk-in*) dan jadwal hari ini (dokter jaga, kuota) dibaca lewat service secara lokal.
- Pasien baru memakai **formulir PM yang ada**; tidak ada formulir pasien baru di MCU.
- Komponen yang sudah ada; radio/centang memakai kelas Bootstrap. Tidak ada CSS Module, komponen dasar, atau
  dependency baru.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | *Mengambil data check-in...*; persyaratan *walk-in* dimuat per paket; tombol dan isian terkunci selama kiriman |
| Gagal muat | Pesan server + *Coba lagi*; `403` → akses ditolak |
| Terblokir | Alasan server (`checkInBlockedReason`) tampil; tombol *Check-in* nonaktif |
| Validasi | Konfirmasi KTP, dokter, persyaratan (kalimat server), tanpa kiriman |
| Ditolak server | Di atas tombol, apa adanya; isian tetap |
| Data basi | `409` `VAL-MCU-46` → pratinjau dimuat ulang; kiriman ulang memakai versi baru |
| Kiriman ganda | Tombol nonaktif selama permintaan berjalan |
| Tanpa hak akses | Tanpa `McuOrder : CheckIn` → tanpa aksi *Check-in*, tanpa *+ Walk-in*, halaman check-in akses ditolak |
| Privasi | NIK tidak pernah di URL (isian awal lewat token); NIK utuh hanya pada layar berizin check-in |

---

## 5. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/orders/{id}/patient-matches` | Pencocokan pasien | `McuOrder : CheckIn` |
| `GET` | `…/orders/{id}/check-in/preview` | Persyaratan, harga, dokter jaga, kartu | `McuOrder : CheckIn` |
| `POST` | `…/orders/{id}/check-in` | Check-in (`version`) | `McuOrder : CheckIn` |
| `POST` | `…/orders/walk-ins` | *Walk-in* | `McuOrder : Create` + `CheckIn` |
| `GET` | `…/packages/{id}` | Persyaratan paket (*walk-in*) | `McuPackage : Read` |
| `GET` | `…/packages/options`, `…/daily-schedules` | Paket + harga; kuota + dokter jaga | `McuPackage : Read`, `McuSchedule : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/doctors/admin/options` | Dokter MCU | `Doctor : Read` |
| `GET` | `/v1/health-services/patient-management/master-data/patients/admin/options` | Pasien terdaftar (*walk-in*) | `Patient : Read` |
| `POST` | `/v1/health-services/patient-management/master-data/patients` | Pasien baru (formulir PM) | `Patient : Create` |

Base `…` = `/v1/health-services/medical-checkup-management`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-check-in-rules.test.mjs` | 7/7 lulus | `PASS` | `# pass 7` |
| Uji mutasi keselamatan identifikasi | Syarat konfirmasi dinonaktifkan → uji `UAT-07` **gagal**; dipulihkan identik (`cmp`) | `PASS` | Keluaran `node --test` |
| E2E API-tiruan `tests/e2e/mcu-check-in-screen.spec.mjs` | 7/7 lulus | `PASS` | `7 passed` |
| **UAT-06** (E2E) | *Tidak ada* → *Buat Pasien Baru* → `patients/create?prefill=<32 hex>` (NIK tidak di URL), formulir PM berisi nama/NIK/HP → kembali ke check-in → *cocok penuh* RM tampil; dokter jaga bawaan; persyaratan wajib ditolak layar; kiriman ganda: tombol nonaktif, klik ulang tidak mengirim; body persis; pindah ke detail | `PASS` | Skenario *UAT-06* |
| **UAT-07** (E2E) | Peringatan dua tanggal; tanpa centang ditolak layar tanpa kiriman; dengan centang `matchConfirmation: true`. Penolakan `VAL-MCU-25` server tampil apa adanya dengan isian tetap | `PASS` | Dua skenario *UAT-07* |
| **UAT-08** (E2E) | Order perusahaan tertaut: *Harga perusahaan (berlaku 01-10-2026 – 31-12-2026)* Rp 300.000; kartu utama terkirim; dokter dapat diganti | `PASS` | Skenario *UAT-08* |
| Terblokir dan basi | Alasan server + tombol nonaktif; `409` `VAL-MCU-46` → pratinjau dimuat ulang, kiriman ulang `version` 4 | `PASS` | Skenario *terblokir…* |
| *Walk-in* (E2E) | Pasien wajib; persyaratan dari `packages/{id}`; dokter jaga bawaan; kuota hari ini; body persis; pindah ke detail | `PASS` | Skenario *walk-in* |
| Izin | Tanpa `CheckIn`: tanpa aksi, tanpa *+ Walk-in*, halaman akses ditolak | `PASS` | Skenario *tanpa…* |
| Regresi seluruh spec MCU (7 berkas) | 46/46 lulus | `PASS` | `46 passed` |
| Regresi PM (`crm-membership-patient-form`) | Formulir tambah dan ubah master pasien lulus. Satu uji **pendaftaran pasien baru IGD** gagal: jalur IGD **tidak mengimpor** berkas yang diubah task ini (formulir IGD terpisah; alurnya berubah lewat merge upstream) | `UNRELATED EXISTING ISSUE` | `grep` impor nihil |
| **UAT tulis, backend lokal + devYoga** (izin user) | (0) Jadwal hari ini dibuka kuota 3, dokter jaga dr. Bima. (1) Booking `MCU-RSMMC-000002` *UAT FEMCU CHECKIN* (identitas sementara) dan `MCU-RSMMC-000003` *UAT FEMCU KONFIRMASI* (NIK pasien uji RM `00-00-00-17`, tanggal lahir beda) lewat API. (2) **UAT-06** lewat layar: *Tidak ada* → formulir PM berisi isian awal (URL tanpa NIK) → *Tempat Lahir* diisi → **pasien tersimpan RM `00-00-00-18`** → kembali → *cocok penuh* → dokter jaga bawaan → persyaratan *Terpenuhi* → check-in `200` → *Hadir*, kunjungan `ENC-RSMMC-00184`, harga Rp 350.000, metode *Pasien baru didaftarkan*. (3) **UAT-07**: peringatan *05-05-1985 · 14-10-2026*; tanpa centang ditolak layar, tidak ada kiriman; order uji dibatalkan lewat API. (4) **Walk-in** lewat layar: pasien RM `00-00-00-17`, kuota 2/3, dokter jaga bawaan → `MCU-RSMMC-000004` *Hadir*. GET non-200 nihil | `PASS` | Log skrip + tangkapan layar |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.897 uji: 3.855 lulus, 42 gagal — daftar nama **identik** dengan baseline `FE-MCU-006` (semua di luar MCU) | `EXISTING / ENVIRONMENT ISSUE` | `diff` kosong |
| `npm run lint:errors` | Exit 0. Hook editor PM membawa 2 peringatan `set-state-in-effect` yang **sudah ada** sebelum perubahan (jumlah sama) | `PASS` / `EXISTING WARNING` | Keluaran perintah |
| `npm run build` | Exit 0: *Compiled successfully*; route check-in, *walk-in*, dan Tambah Pasien (dinamis) terbentuk | `PASS` | Keluaran build |

`MANUAL TEST: PASS` — pencocokan empat keadaan, centang konfirmasi, *Buat Pasien Baru* → formulir PM → kembali,
pilihan dokter/kartu, radio persyaratan + catatan + tanggal susulan, tombol check-in (nonaktif saat kiriman/terblokir),
dan seluruh halaman *walk-in* dijalankan lewat peramban (Playwright) pada E2E dan UAT tulis.
`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU; 7/7 uji baru PASS)`.

### Data uji devYoga sesudah UAT (dibiarkan sesuai izin user)

| Data | Keadaan |
| --- | --- |
| Pasien baru **RM `00-00-00-18`** *UAT FEMCU CHECKIN* (NIK uji `9900000000000901`, lahir 03-02-1991, tempat lahir *UAT Jakarta*) | Dibuat lewat formulir PM |
| `MCU-RSMMC-000002` *UAT FEMCU CHECKIN* | *Hadir* — kunjungan `ENC-RSMMC-00184`, dr. Bima, Rp 350.000 |
| `MCU-RSMMC-000003` *UAT FEMCU KONFIRMASI* | *Dibatalkan* sebelum check-in |
| `MCU-RSMMC-000004` *UAT FECRM PASIEN IGD* (RM `00-00-00-17`, *walk-in*) | *Hadir* — dr. Bima, Rp 350.000 |
| Jadwal MCU 27-10-2026 | Buka, kuota 3 (terpakai 2), dokter jaga dr. Bima |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-06` — pasien baru dibuat, dokter, puasa terpenuhi → *Hadir* | Terpenuhi | §6 (E2E + sungguhan) |
| `UAT-07` — NIK cocok tanggal lahir beda tanpa konfirmasi → ditolak | Terpenuhi | §6 (E2E + sungguhan) |
| `UAT-08` — harga perusahaan Rp 300.000 | Terpenuhi di E2E | §6 |
| Tombol check-in nonaktif selama permintaan berjalan | Terpenuhi | §6 *UAT-06* |
| DoD: kriteria terbukti | Terpenuhi | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Tindakan pemilik | Jabatan admisi MCU perlu `McuOrder : CheckIn`, `Patient : Read` + `Patient : Create` (pasien baru), `Doctor : Read`, `McuPackage : Read` (persyaratan *walk-in*), dan `CompanyGuarantor : Read` |
| Keputusan lintas modul | Perluasan Patient Management bersifat aditif dan hanya aktif dengan `?prefill=`. Pemilik modul PM sebaiknya diberi tahu |
| Masalah yang diketahui | (1) `UAT-08` sungguhan belum dijalankan: butuh pasien dengan kartu penjamin perusahaan dan tarif perusahaan yang berbeda dari tarif umum di devYoga. (2) Formulir PM yang dibuka dari check-in memakai *Sumber Registrasi* bawaan PM (*Kiosk*); bila diinginkan, nilai *Admisi* dapat ditambahkan ke ruas isian awal. (3) *Walk-in* belum menawarkan pilihan kartu penjamin perusahaan; server memakai kartu yang berlaku dan menolak bila tidak ada (`VAL-MCU-30`) |
| Dependency backend | Tidak ada; `BE-MCU-013`, `014` ✅ |
| Perubahan sampingan | Perluasan PM (§3.2); harapan dua spec MCU sebelumnya diperbarui |
| Interupsi | `NONE` |
| Status Git (frontend) | ` M` 4 berkas PM (create page, form view, editor hook) + berkas MCU sebelumnya; `??` folder check-in/*walk-in*, utilitas isian awal PM, uji baru |
| Langkah berikutnya | `FE-MCU-010` (lapisan data pemeriksaan, resume, penyerahan); order `MCU-RSMMC-000002`/`000004` *Hadir* siap untuk uji *Mulai Pemeriksaan* |
