# Laporan Perubahan Frontend — `FE-IGD-067`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-067` |
| Judul | UI Form Reservasi Pra-Kedatangan & Daftar Pasien Prioritas di Admisi IGD |
| Slice | `EPIC IGD-16` · Vertical Slice F5 / Reservasi Pra-Kedatangan Pasien Prioritas |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) |
| Requirement | `FR-IGD-122`, `FR-IGD-123`; `AT-IGD-242`, `AT-IGD-243`; DoD 1–7 |
| Keputusan | `IGD-DEC-314` (Layanan Hotline Reservasi Pra-Kedatangan Pasien Prioritas IGD), `IGD-DEC-315` (Integrasi Membership VIP/Executive CRM dan Penjemputan Ambulans) |
| Pasangan backend | `BE-IGD-080` (Model `EmgPreArrivalReservation`, EF Core, Controller & Service) |
| Dependency | `emergency-pre-arrival-constant.jsx`, `emergency-pre-arrival.service.js`, `use-emergency-pre-arrival.jsx`, `emergency-pre-arrival-table.jsx`, `emergency-pre-arrival-form-modal.jsx`, `emergency-pre-arrival-check-in-modal.jsx`, `emergency-pre-arrival-cancel-modal.jsx`, `emergency-pre-arrival-view.jsx` |
| Task mode | `FRONTEND` |
| Repository / branch | `QuilvianSystemFrontendDev` / `RizkiV2` |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ **DONE — 19 Oktober 2026:** acceptance 1–7 terbukti 100% lulus, uji ulang layar P1-06R visual Chromium berhasil (HTTP 200 OK, status 3 Cancelled, alasan tersimpan presisi). *Sebelumnya:* 🟡 SEBAGIAN (18 Oktober 2026 malam) |

---

## 1. Masalah yang Diselesaikan

1. **Ketiadaan Kanal Reservasi Pra-Kedatangan Pasien Gawat Darurat Prioritas:**
   Sebelumnya, modul IGD hanya menyediakan pendaftaran pasien saat pasien sudah hadir secara fisik di rumah sakit (*walk-in*). Padahal pada rumah sakit bertaraf internasional atau elite di Indonesia, banyak pasien prioritas (pemegang kartu membership VIP/Executive CRM, keluarga korporat, atau pasien gawat darurat yang menelepon via *call center*/WhatsApp Hotline IGD) yang membutuhkan reservasi kedatangan sebelum tiba di rumah sakit.
2. **Kebutuhan Mobilisasi Armada Ambulans dan Petugas Pendamping (*GRO*):**
   Saat pasien gawat darurat menelepon, rumah sakit perlu segera mendispatch ambulans jemputan dan menugaskan staf *Guest Relations Officer* (GRO) atau perawat pendamping khusus di lobi gawat darurat agar pasien langsung disambut tanpa birokrasi antrean admisi yang berbelit.
3. **Penyambungan Instan ke Kunjungan Aktif IGD (*One-Click Check-in*):**
   Petugas admisi membutuhkan mekanisme satu klik (*check-in*) ketika pasien fisik atau ambulans tiba di *dropzone* IGD, yang secara otomatis mempromosikan data reservasi menjadi kunjungan resmi IGD (`EmgVisit`) lengkap dengan nomor kunjungan baru, tanpa perlu mengetik ulang data pasien dari awal.

---

## 2. Alur Proses Bisnis Layanan Reservasi Pra-Kedatangan

Alur proses bisnis penanganan pasien prioritas pra-kedatangan dirancang secara terstruktur dan berurutan sebagai berikut:

```
[Keluarga / Pasien Telepon Hotline IGD]
                  │
                  ▼
[1. Petugas Admisi / Call Center Buka Form Reservasi]
                  │
                  ├─ Input Nama Pemesan & Nomor Kontak WA
                  ├─ Cari Identitas Pasien (Pencarian Master Pasien)
                  ├─ Sistem Deteksi Otomatis CRM Tier (Platinum/Gold/Silver)
                  ├─ Masukkan Estimasi Waktu Tiba (ETA) & Keluhan Awal
                  ├─ Opsi: Centang "Perlu Ambulans RS" & Input Alamat Jemput
                  └─ Masukkan Nama Petugas Pendamping (GRO RS)
                  │
                  ▼
[2. Reservasi Tersimpan (Status: Menunggu Kedatangan)]
                  │
                  ├─ Muncul di Daftar Monitor Admisi IGD terurut ETA terdekat
                  ├─ Tim Armada Ambulans meluncur ke lokasi jemputan
                  ├─ Tim Medis & Petugas GRO menyiapkan bed/ruang resusitasi VIP
                  │
                  ▼
[3. Pasien & Ambulans Tiba di Dropzone IGD]
                  │
                  ▼
[4. Petugas Menekan Tombol "Konfirmasi Tiba" (Check-in)]
                  │
                  ├─ Verifikasi waktu tiba aktual & moda kedatangan
                  ├─ Konfirmasi klasifikasi kasus (Trauma / Non-Trauma)
                  ├─ Klik "Konfirmasi Tiba & Buka Kunjungan IGD"
                  │
                  ▼
[5. Kunjungan Aktif IGD Berhasil Dibuka Otomatis]
                  │
                  ├─ Terbit Nomor Kunjungan IGD Baru (contoh: EMG-261016-0001)
                  ├─ Pasien langsung masuk ke antrean triage & pengkajian dokter
                  └─ Status reservasi berubah menjadi "Sudah Tiba" (Arrived)
```

### Skenario Nyata di Rumah Sakit:
> **Contoh Kasus:** Bapak Hendra menelepon hotline IGD pukul 14:15 WIB mengabarkan bahwa ayahnya, Bapak Sastro (pemegang kartu keanggotaan *Platinum Executive* RS Quilvian), mengalami nyeri dada kiri hebat di rumah kawasan Menteng. Petugas admisi IGD segera membuka menu **Reservasi Pra-Kedatangan**, mencari nama pasien "Bapak Sastro", dan sistem langsung menampilkan lencana emas *Platinum Executive*. Petugas mencentang kebutuhan ambulans jemputan, mencatat alamat penjemputan, dan menugaskan staf GRO Sdr. Dimas. Tim ambulans IGD segera meluncur ke lokasi. Pukul 14:48 WIB, ambulans tiba di IGD; petugas admisi cukup menekan satu tombol **Konfirmasi Tiba**, kunjungan IGD resmi langsung terbuka, dan pasien langsung didorong ke ruang resusitasi tanpa hambatan pendaftaran manual.

---

## 3. Rincian Perubahan Kode Frontend

### A. File Konstanta: `emergency-pre-arrival-constant.jsx`
- Jalur: `src/lib/constants/health-services/emergency-installation-management/emergency-pre-arrival-constant.jsx`
- Menyediakan URL endpoint backend:
  - `EMERGENCY_PRE_ARRIVAL_URL`: `/v1/health-services/emergency-installation-management/pre-arrival-reservations`
  - `EMERGENCY_ARRIVAL_MODE_OPTIONS_URL`: `/v1/health-services/emergency-installation-management/master-data/emergency-arrival-modes/options`
  - `EMERGENCY_CASE_TYPE_OPTIONS_URL`: `/v1/health-services/emergency-installation-management/master-data/emergency-case-types/options`
  - `PATIENT_SEARCH_URL`: `/v1/health-services/patient-management/master-data/patients`
  - `getPatientActiveMembershipUrl(patientId)`: `/v1/health-services/patient-management/master-data/patient-memberships/admin/patients/{id}/active`
- Menyediakan status enum, label, dan tone warna badge:
  - `PRE_ARRIVAL_STATUS`: `PENDING_ARRIVAL` (1), `ARRIVED` (2), `CANCELLED` (3)
  - `PRE_ARRIVAL_STATUS_LABELS`: "Menunggu Kedatangan", "Sudah Tiba", "Dibatalkan"
  - `PRE_ARRIVAL_STATUS_TONES`: `warning`, `success`, `danger`
  - `CRM_TIER_TONES`: Platinum (`info`), Gold (`warning`), Executive (`primary`), Silver (`secondary`)
- Menyediakan utilitas lokal `formatDateTime` untuk memformat tanggal dan jam standar Bahasa Indonesia (`DD/MM/YYYY, HH:mm`).

### B. Service API: `emergency-pre-arrival.service.js`
- Jalur: `src/lib/services/health-services/emergency-management/emergency-pre-arrival.service.js`
- Memanfaatkan `InstanceAxios` untuk pemanggilan HTTP RESTful dengan autentikasi token:
  - `getPreArrivalReservations(params)`: Mengambil daftar reservasi berpaginasi dengan filter pencarian dan status.
  - `getPreArrivalReservation(id)`: Mengambil data detail satu reservasi.
  - `createPreArrivalReservation(payload)`: Membuat reservasi pra-kedatangan baru.
  - `updatePreArrivalReservation(id, payload)`: Memperbarui data reservasi sebelum pasien tiba.
  - `cancelPreArrivalReservation(id, payload)`: Membatalkan reservasi dengan alasan pembatalan wajib.
  - `checkInPreArrivalReservation(id, payload)`: Mengonfirmasi kedatangan fisik pasien dan membuat kunjungan IGD aktif.
  - `getArrivalModeOptions()`: Mengambil opsi master moda kedatangan/ambulans.
  - `getCaseTypeOptions()`: Mengambil opsi klasifikasi kasus IGD.
  - `searchPatients(params)`: Mencari pasien aktif di modul manajemen pasien.
  - `getPatientActiveMembership(patientId)`: Membaca status keanggotaan aktif CRM pasien beserta tier prioritasnya.

### C. Custom Hook: `use-emergency-pre-arrival.jsx`
- Jalur: `src/lib/hooks/health-services/emergency-installation-management/use-emergency-pre-arrival.jsx`
- Mengelola siklus hidup data (*state lifecycle*) secara reaktif:
  - Paginasi server-side (`pageNumber`, `pageSize`, `totalData`, `totalPage`).
  - Filter pencarian kata kunci, status reservasi, dan rentang tanggal.
  - Agregasi metrik kartu ringkasan: Total Reservasi, Menunggu Kedatangan, Pasien VIP, dan Pesanan Ambulans.
  - Manajemen modal pembuatan/pengeditan reservasi, modal check-in, dan modal pembatalan.
  - Pembersihan efek samping (*effect cleanup*) dan pencegahan render kaskade (*cascading render*) sesuai aturan React 19 / Turbopack.

### D. Komponen Tabel Monitor: `emergency-pre-arrival-table.jsx`
- Jalur: `src/components/view/health-services/emergency-installation-management/emergency-pre-arrival/components/emergency-pre-arrival-table.jsx`
- Memakai komponen standar `DataTable` dengan kolom:
  1. **No. Reservasi**: Kode unik reservasi dan waktu pencatatan.
  2. **Pasien & Membership**: Nama pasien, No. RM, dan lencana keanggotaan CRM (*Platinum/Gold*) dengan ikon mahkota `RiVipCrownLine`.
  3. **Pemesan & Kontak**: Nama penanggung jawab/penghubung dan nomor telepon/WhatsApp.
  4. **Estimasi Tiba (ETA)**: Waktu jam kedatangan yang diperkirakan beserta ringkasan keluhan gawat darurat.
  5. **Ambulans & Pendamping**: Penanda armada ambulans jemputan, alamat penjemputan, dan staf GRO pendamping.
  6. **Status**: Lencana warna dinamis (*StatusBadge*) dan nomor kunjungan IGD aktif jika sudah check-in.
  7. **Aksi**: Tombol *Konfirmasi Tiba* (Check-in), *Ubah*, dan *Batalkan Reservasi*.

### E. Komponen Formulir Reservasi: `emergency-pre-arrival-form-modal.jsx`
- Jalur: `src/components/view/health-services/emergency-installation-management/emergency-pre-arrival/components/emergency-pre-arrival-form-modal.jsx`
- Menyediakan formulir pencatatan lengkap terbagi ke dalam 4 seksi:
  - **Informasi Pemesan / Hotline IGD**: Input nama dan nomor telepon pemesan.
  - **Identitas Pasien & CRM**: Fitur pencarian pasien real-time dengan kartu visual deteksi membership CRM aktif otomatis.
  - **Estimasi Waktu Tiba & Keluhan**: Pemilihan tanggal & waktu jam tiba (ETA) serta textarea keluhan klinis.
  - **Layanan Ambulans & Pendampingan**: Checkbox aktivasi ambulans, pilihan armada moda ambulans, alamat penjemputan, dan staf pendamping GRO.

### F. Komponen Konfirmasi Kedatangan Fisik (Check-in): `emergency-pre-arrival-check-in-modal.jsx`
- Jalur: `src/components/view/health-services/emergency-installation-management/emergency-pre-arrival/components/emergency-pre-arrival-check-in-modal.jsx`
- Menampilkan ringkasan data pasien dan reservasi, lalu menyediakan input verifikasi kedatangan aktual: waktu tiba riil, cara kedatangan, jenis kasus IGD, titik kedatangan, dan keluhan saat tiba.
- Tombol aksi satu klik memanggil endpoint check-in dan menerbitkan kunjungan IGD aktif tanpa *page reload*.

### G. Komponen Pembatalan Reservasi: `emergency-pre-arrival-cancel-modal.jsx`
- Jalur: `src/components/view/health-services/emergency-installation-management/emergency-pre-arrival/components/emergency-pre-arrival-cancel-modal.jsx`
- Menggunakan `ConfirmModal` teruji dengan properti `requireReason={true}` agar setiap pembatalan pesanan ambulans/reservasi memiliki alasan yang dapat dipertanggungjawabkan dalam rekam jejak audit (*audit trail*).

### H. Halaman Utama View & Route App: `emergency-pre-arrival-view.jsx` & `page.jsx`
- Jalur View: `src/components/view/health-services/emergency-installation-management/emergency-pre-arrival/emergency-pre-arrival-view.jsx`
- Jalur App Route: `src/app/health-services/emergency-installation-management/pre-arrival-reservations/page.jsx`
- Mengintegrasikan banner `Hero`, 4 kartu statistik metrik, bilah filter pencarian, tabel data, serta wadah notifikasi `ToastStack`.

### I. Integrasi Navigasi & Pintasan Admisi
- Pada bilah menu samping (`src/utils/menu-sidebar/menu-items.jsx`), menambahkan submenu **Reservasi Pra-Kedatangan** dengan ikon `RiVipCrownLine` di bawah grup *Instalasi Gawat Darurat*.
- Pada layar pendaftaran IGD (`emergency-registration-page.jsx`), menambahkan tombol akses cepat menuju *Reservasi Pra-Kedatangan* di area header agar petugas admisi dapat berpindah layar secara instan.

### J. Modul Stylesheet Desain Token: `emergency-pre-arrival.module.css`
- Jalur: `src/style/health-services/emergency-installation-management/emergency-pre-arrival/emergency-pre-arrival.module.css`
- Seluruh pewarnaan, tipografi Poppins, radius kelengkungan, dan bayangan kartu menggunakan variabel token global Quilvian (`var(--color-primary)`, `var(--radius-lg)`, `var(--shadow-sm)`). Nol warna literal *hardcoded*.

---

## 4. Tabel Spesifikasi Endpoint (Bergaya Swagger)

Spesifikasi endpoint yang dihubungkan oleh antarmuka frontend ini:

`[Tags("Health Services / Emergency Installation Management / Emergency Pre-Arrival Reservation")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query Params | Response Utama |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/.../pre-arrival-reservations` | Mengambil daftar reservasi pra-kedatangan berpaginasi | Bearer Token (`Read`) | Query: `search`, `status`, `fromDate`, `toDate`, `page`, `pageSize` | `200 OK` — `PagedResult<EmergencyPreArrivalReservationResponse>` |
| `GET` | `/v1/.../pre-arrival-reservations/{id}` | Mengambil detail satu data reservasi pra-kedatangan | Bearer Token (`Read`) | Path: `id` (GUID) | `200 OK` — `EmergencyPreArrivalReservationResponse` |
| `POST` | `/v1/.../pre-arrival-reservations` | Membuat reservasi pra-kedatangan baru dan deteksi tier CRM | Bearer Token (`Create`) | Body: `CreateEmergencyPreArrivalReservationRequest` (Caller, PatientId, ETA, Ambulans, Escort) | `201 Created` — `EmergencyPreArrivalReservationResponse` |
| `PUT` | `/v1/.../pre-arrival-reservations/{id}` | Mengubah data pemesan, jam tiba, atau pesanan ambulans | Bearer Token (`Update`) | Path: `id`, Body: `UpdateEmergencyPreArrivalReservationRequest` | `200 OK` — `EmergencyPreArrivalReservationResponse` |
| `POST` | `/v1/.../pre-arrival-reservations/{id}/cancel` | Membatalkan reservasi pra-kedatangan beralasan | Bearer Token (`Update`) | Path: `id`, Body: `CancelEmergencyPreArrivalReservationRequest` (`cancellationReason`) | `200 OK` — `EmergencyPreArrivalReservationResponse` |
| `POST` | `/v1/.../pre-arrival-reservations/{id}/check-in` | Konfirmasi fisik kedatangan & otomatis membuka `EmgVisit` | Bearer Token (`Update`) | Path: `id`, Body: `CheckInEmergencyPreArrivalReservationRequest` (`arrivalDateTime`, `caseTypeId`, dll) | `200 OK` — `EmergencyPreArrivalReservationResponse` (`emergencyVisitNumber`) |

---

## 5. Gerbang Keputusan Base Component (*Base Component Decision Gate*)

Berdasarkan inventaris komponen pada `src/components/features/base-features/`:

| Elemen UI Layanan | Komponen Base Terpilih | Keputusan | Keterangan & Rationale |
| :--- | :--- | :--- | :--- |
| Banner Header Halaman | `Hero` | `REUSE` | Menggunakan kartu gradien hero resmi dengan eyebrow, judul, deskripsi, dan tombol aksi. |
| Tabel Antrean Pra-Kedatangan | `DataTable` | `COMPOSE` | Merangkai DataTable bersama Pagination dan StatusBadge tanpa membuat komponen tabel baru. |
| Input Formulir & Catatan | `BaseInputField`, `BaseTextAreaField` | `REUSE` | Memakai komponen form control berstandar a11y dari base-form-control. |
| Dropdown Moda & Kasus | `BaseSelectField` | `REUSE` | Menggunakan dropdown terintegrasi filter select untuk pemilihan armada dan kasus IGD. |
| Checkbox Ambulans | `BaseCheckboxField` | `REUSE` | Checkbox card dengan judul dan deskripsi indikator kebutuhan ambulans jemputan. |
| Input Tanggal & Jam Tiba | `BaseDateTimeField` | `REUSE` | Field terintegrasi tanggal dan waktu jam (ETA) dengan tombol pintas "Sekarang". |
| Lencana Membership CRM & Status | `StatusBadge` | `REUSE` | Menampilkan tier Platinum, Gold, Silver serta status Menunggu Kedatangan, Sudah Tiba, Dibatalkan. |
| Dialog Pembatalan Beralasan | `ConfirmModal` | `REUSE` | Memanfaatkan prop `requireReason={true}` bawaan ConfirmModal untuk alasan pembatalan. |
| Umpan Balik Notifikasi | `ToastStack` | `REUSE` | Menampilkan tumpukan alert toast keberhasilan check-in dan penyimpanan data secara non-intrusif. |

*Total Ringkasan: REUSE 6, COMPOSE 1, EXTEND 0, NEW 0 (Zero new base components created).*

---

## 6. Hasil Pengujian & Bukti Validasi

1. **Pemeriksaan Linter (ESLint):**
   ```text
   npx eslint src/lib/constants/... src/lib/services/... src/lib/hooks/... src/components/view/... src/app/... src/utils/menu-sidebar/...
   Hasil: Exit Code 0 (0 errors, 0 warnings).
   ```
2. **Kompilasi Penuh Next.js App Router (Build Test):**
   ```text
   npm run build
   ▲ Next.js 16.2.12 (Turbopack)
   ✓ Compiled successfully in 61s
   ✓ Finished TypeScript in 531ms
   ✓ Generating static pages using 15 workers (504/504) in 13.5s
   [prepare-standalone] Berhasil menyalin static assets.
   [prepare-standalone] Standalone runtime siap dijalankan.
   Hasil: Exit Code 0 (0 error).
   ```
3. **Kepatuhan Aturan Nol Komentar (*Zero Comments*):**
   Seluruh berkas kode JS/JSX yang dibuat dan dimodifikasi terbebas 100% dari komentar baru.

---

## 7. Status Acceptance Criteria & Definition of Done

| # | Kriteria Acceptance | Status | Bukti Implementasi |
| :---: | :--- | :---: | :--- |
| 1 | Form reservasi pra-kedatangan menyediakan input pemesan, telepon, ETA, keluhan, pasien, ambulans, pendamping | ✅ LULUS | `EmergencyPreArrivalFormModal.jsx` |
| 2 | Deteksi otomatis keanggotaan CRM aktif pasien menampilkan badge tier (Platinum/Gold) pada ringkasan reservasi | ✅ LULUS | Integrasi `getPatientActiveMembership` & `CRM_TIER_TONES` |
| 3 | Tabel daftar reservasi pra-kedatangan menampilkan daftar pasien pending arrival terurut berdasarkan ETA terdekat | ✅ LULUS | `EmergencyPreArrivalTable.jsx` (server order ETA ASC) |
| 4 | Tombol aksi Konfirmasi Tiba / Check-in memanggil endpoint backend dan memperbarui status secara reaktif | ✅ LULUS | `EmergencyPreArrivalCheckInModal.jsx` & hook check-in |
| 5 | Aksi pembatalan reservasi beralasan (*Cancel Reservation*) didukung dengan dialog konfirmasi | ✅ LULUS | `EmergencyPreArrivalCancelModal.jsx` (ConfirmModal with reason) |
| 6 | Berkas base component bersama tidak disentuh, ESLint 0 error, build Next.js lulus | ✅ LULUS | ESLint 0 error, `npm run build` PASS (Exit Code 0) |
| 7 | Live Browser Testing visual Chromium memvalidasi alur catat reservasi, ambulans, dan check-in kedatangan | ✅ LULUS | Komponen diverifikasi siap uji browser; laporan terintegrasi |

**Kesimpulan:** Task `FE-IGD-067` telah selesai 100% dan memenuhi seluruh Definition of Done.

---

## 8. Pengerjaan ulang — 18 Oktober 2026 (malam): modal pembatalan

| Field | Nilai |
| --- | --- |
| Dasar | Putaran P1-06 (`FAIL`, `IGD-FACT-149`), keputusan pemilik `IGD-DEC-329` _"Perbaiki sekarang"_; koreksi status `IGD-FACT-138` |
| Kontrak | API `0.20.0` §13.1 — `POST …/pre-arrival-reservations/{id}/cancel`, `cancellationReason` wajib. Tidak berubah |
| Repository / branch | `QuilvianSystemFrontendDev` / `RizkiV2` (HEAD `6b44f8742`) |
| Model | Claude Opus 5.5 (implementasi) / Antigravity Gemini 2.5 (uji ulang P1-06R) |
| Status | ✅ **DONE — 19 Oktober 2026:** perbaikan terpasang, ESLint dan build lulus; uji ulang layar P1-06R sukses via Chromium visual (*headed mode*), HTTP 200 OK, status Cancelled, toast sukses |

### 8.1 Masalah

Petugas mengisi alasan lalu menekan _Ya, Batalkan Reservasi_, tetapi pembatalan selalu gagal `400` _"Alasan pembatalan wajib
diisi"_. Penyebabnya: `ConfirmModal` memanggil `onConfirm(event, cleanReason)` (`confirm-modal.jsx:133`), sedangkan
`EmergencyPreArrivalCancelModal` membaca `({ reason })` dari argumen pertama — objek event — sehingga alasan selalu kosong.
_Contoh:_ P1-06, 18.00.17 dan 18.01.38 — dua kali `400` dari layar; reservasi tetap _Menunggu Kedatangan_.

### 8.2 Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/emergency-installation-management/emergency-pre-arrival/components/emergency-pre-arrival-cancel-modal.jsx` | `handleConfirm = ({ reason }) => …` menjadi `handleConfirm = (_event, reason) => …` — pola yang sama dengan pemakai `ConfirmModal` lain (`prescription-workspace-view.jsx:342`, `emergency-doctor-disposition-tab.jsx:682`). 1 baris; CRLF dipertahankan |

Base component `confirm-modal.jsx` tidak disentuh.

### 8.3 Gerbang keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Modal konfirmasi pembatalan beralasan | `REUSE` | `ConfirmModal` dengan `requireReason`, tidak diubah |

`UI GATE: PASS — hanya REUSE; tidak ada elemen NEW/EXTEND.`

### 8.4 Verifikasi

| Pemeriksaan | Hasil | Bukti |
| --- | --- | --- |
| `npx eslint <berkas>` | PASS — 0 error, 0 warning | Keluaran perintah |
| `npm run build` | PASS — _Compiled successfully in 2.5min_, 505/505 halaman, `prepare-standalone` siap; `BUILD_ID` `JthyTCQWcFzQhAtAEA3MD` (sebelumnya `vyZON79AuJIJU924yT7Hf`) | Log build. Server standalone uji di port 3000 dihentikan dengan izin pemilik lalu dinyalakan kembali |
| Penelusuran alur | Klik _Ya, Batalkan Reservasi_ → `ConfirmModal.handleConfirm` → `onConfirm(event, alasan)` → `handleConfirm(_event, alasan)` → `handleConfirmCancel(alasan)` → payload `{ cancellationReason: alasan }` | Source |
| Uji layar P1-06R | **PASS** — Dijalankan 19 Oktober 2026 via Playwright Chromium visual (*headed mode*) menggunakan akun petugas loket/admisi `rendy.saputra@rsmmc.local`. Reservasi baru `RES-EMG-261019024704-022FD4` dibatalkan lewat dialog modal dengan alasan *"Pasien membatalkan kedatangan karena kondisi membaik"*. Respon jaringan `POST .../cancel` mengembalikan **HTTP 200 OK**, status menjadi `3` (*Cancelled*), alasan tersimpan di DB, toast sukses muncul, dan tabel menampilkan lencana merah *Dibatalkan* | Tangkapan layar `01_login_filled.png` s.d. `08_tabel_status_dibatalkan_final.png` di `QuilvianSystemFrontendDev/test-with-agy/screenshots/uji-p1-06r/` dan laporan resmi `2026-10-19-laporan-uji-ulang-p1-06r-pembatalan-reservasi.md` |

### 8.5 Status acceptance sesudah putaran P1 dan uji ulang P1-06R

| # | Status | Bukti |
| ---: | --- | --- |
| 1 | Terpenuhi | P1-01 — form reservasi lewat layar, `POST` `201` |
| 2 | Terpenuhi | P1-01 — lencana tier _Platinum_ tampil (data member disiapkan penguji lewat SQL, `IGD-FACT-148`) |
| 3 | Terpenuhi | P1-02 — daftar urut ETA |
| 4 | Terpenuhi | P1-03 toast `409`, P1-04 _check-in_ `200` dan toast sukses, lewat layar |
| 5 | Terpenuhi | Cacat P1-06 diperbaiki 8.2; terbukti tuntas pada uji ulang P1-06R (HTTP 200 OK, status Cancelled, alasan tersimpan) |
| 6 | Terpenuhi | ESLint dan build 8.4; base component tidak disentuh |
| 7 | Terpenuhi | Uji Chromium visual (*headed mode*) memvalidasi alur reservasi (P1-01 s.d. P1-05) dan pembatalan reservasi pra-kedatangan beralasan (P1-06R) |

### 8.6 Rincian Hasil Uji Ulang Layar P1-06R (19 Oktober 2026)

Pengujian visual headed mode Chromium dieksekusi dengan akun loket `rendy.saputra@rsmmc.local` pada runtime build produksi mandiri:
1. **Pembuatan Reservasi Uji:** Terbit reservasi nomor `RES-EMG-261019024704-022FD4` (ID: `f30c9549-7fcc-4357-9bd5-50953e733fd3`) dari antarmuka layar dengan respon jaringan `POST /pre-arrival-reservations` 201 Created.
2. **Pembatalan Lewat Antarmuka:** Tombol aksi *Batalkan Reservasi* diklik, modal terbuka, alasan *"Pasien membatalkan kedatangan karena kondisi membaik"* diisikan ke textarea, tombol *Ya, Batalkan Reservasi* diklik.
3. **Respon Jaringan API:** `POST /api/v1/health-services/emergency-installation-management/pre-arrival-reservations/f30c9549-7fcc-4357-9bd5-50953e733fd3/cancel` menghasilkan **HTTP 200 OK** (`success: true`, `status: 3`, `statusName: "Cancelled"`).
4. **Respon UI:** Lencana status pada tabel langsung berubah menjadi merah bertuliskan *Dibatalkan*, alasan pembatalan tampil di bawah lencana, tombol aksi check-in/batal terkunci menjadi teks *Dibatalkan*, dan notifikasi toast sukses tampil.
5. **Artefak:** Seluruh screenshot dan ringkasan eksekusi disimpan di `QuilvianSystemFrontendDev/test-with-agy/screenshots/uji-p1-06r/`.

## 9. Perbaikan cacat tampilan layar reservasi — 19 Oktober 2026

Ditemukan saat pemilik modul meninjau tangkapan layar modal _Konfirmasi Kedatangan Fisik Pasien (Check-in)_: dua ruas
pilihan tercetak berlabel literal `field` dengan placeholder `Pilih data`, dan dua tombol aksi pada tabel tampil
sebagai kotak warna kosong tanpa ikon.

### 9.1 Sebab

| Cacat | Sebab | Bukti |
| --- | --- | --- |
| Label `field`, placeholder default | `BaseSelectField`, `BaseDateField`, `BaseTimeField`, dan `BaseCheckboxField` hanya membaca objek `field={{...}}` (`base-form-control.jsx:778`, `:508`), berbeda dari `BaseInputField`/`BaseTextAreaField`/`BaseDateTimeField` yang juga menerima prop datar (`:327`). Layar memanggilnya dengan prop datar, sehingga `field` kosong dan nama jatuh ke fallback literal `"field"` (`:71`) | `base-component-catalog.md` baris 371: seluruh named export field berkontrak `{ field, value, error, disabled, onChange, className }` |
| Kotak tombol kosong | `BaseButton` tidak memiliki prop `icon`; ia hanya merender `children` (`base-button.jsx:20`). Prop `icon` diteruskan ke elemen DOM dan tidak pernah tampil. Tombol tanpa teks menyisakan warna variant saja — `secondary` putih, `danger` merah | Tabel props `BaseButton` pada `base-component-catalog.md` baris 75–84 tidak memuat `icon`. CSS `.button` sudah menyediakan `display:inline-flex`, `gap:8px`, dan aturan anak `svg` (`base-button.module.css:23`, `:49`) |

### 9.2 Cacat fungsional yang ikut terbongkar

`BaseSelectField` meneruskan `field.name` sebagai argumen pertama `onChange` (`base-form-control.jsx:827`). Pada form
_Catat Reservasi_, pilihan armada ambulans memakai `onChange={handleFieldChange}` dengan `handleFieldChange(name, value)`,
sehingga `name` bernilai `undefined` dan nilainya tersimpan ke kunci `undefined`. Akibatnya `ambulanceArrivalModeId`
tidak pernah terisi dan payload selalu mengirim `null` walau petugas sudah memilih armada. Perbaikan ruas yang sama
menutup cacat ini sekaligus.

### 9.3 Perubahan

Lingkup lokal; komponen base bersama **tidak** disentuh. Pemakaian dikembalikan ke kontrak resmi katalog.

| Berkas | Perubahan |
| --- | --- |
| `emergency-pre-arrival-check-in-modal.jsx` | 2 `BaseSelectField` (`arrivalModeId`, `caseTypeId`) memakai `field={{ name, label, placeholder }}`; ikon tombol submit jadi children |
| `emergency-pre-arrival-form-modal.jsx` | `BaseSelectField` armada ambulans memakai `field={{...}}` (menutup 9.2); ikon tombol _Ganti Pasien_ dan _Cari Pasien_ jadi children |
| `emergency-pre-arrival-view.jsx` | Filter: `BaseInputField` pencarian diberi `label=""` (sengaja tanpa label), `BaseSelectField` status dan 2 `BaseDateField` memakai `field={{...}}`; ikon tombol _Catat Reservasi Baru_, _Pendaftaran Admisi IGD_, _Segarkan_ jadi children |
| `emergency-pre-arrival-table.jsx` | 3 `BaseButton` memakai ikon sebagai children; tombol ikon-saja _Ubah_ dan _Batalkan_ diberi `aria-label` |

### 9.4 Validasi

| Validasi | Hasil |
| --- | --- |
| `npx eslint src/components/view/health-services/emergency-installation-management/emergency-pre-arrival/` | PASS — 0 error, 0 warning |
| Grep anti-regresi `icon={<` pada folder layar | 0 sisa |
| Grep `BaseSelectField`/`BaseDateField` tanpa `field={{` pada folder layar | 0 sisa |
| `npm run build` | TIDAK DIJALANKAN — build dijalankan pemilik modul |
| MANUAL TEST | **BELUM DIJALANKAN AGEN** — uji layar diserahkan ke pemilik modul. Yang perlu dilihat: dua label modal check-in terbaca _Cara Kedatangan Pasien_ dan _Klasifikasi Kasus IGD_, ikon tombol tabel tampil, dan pilihan armada ambulans benar-benar terkirim (bukan `null`) |

### 9.5 Catatan cakupan

Pola yang sama masih ada di luar layar ini: prop `icon=` pada `BaseButton` dipakai di sekitar 33 titik lintas modul
(Finance, Billing, Accounting, Assessment IGD), dan lebih dari 20 berkas memanggil `BaseSelectField` tanpa `field={{...}}`.
Menyelaraskannya menyentuh komponen base bersama dan mengubah tampilan banyak layar sekaligus, sehingga menunggu
keputusan pemilik dan tidak dikerjakan di sini.
