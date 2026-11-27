# Laporan Perubahan Frontend — `FE-KSK-PO-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-008` |
| Judul | Detail, Ubah Jadwal, Ubah Kontak, Batalkan |
| Slice | `EPIC KSK-PO-07` — layar petugas (`FR-KSK-PO-061`), gelombang 2, rilis `MVP-3` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-008` |
| Trace | `KSK-DEC-051`, `052`; `KSK-DSN-PO-016`; `KSK-PO-FE-005` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), grup *Online Registration* `GET {id}`, `GET {id}/referral-documents/{documentId}`, `PUT {id}/schedule`, `PUT {id}/contact`, `POST {id}/cancel`; validation matrix `KSK-VAL-PO-060` s.d. `072` |
| Wewenang UI | `KSK-PO-FE-005`: isi dan aksi sesuai `KSK-DEC-050` s.d. `052`. Tata letak dan bentuk modal `DEV_DISCRETION`: dialog `ConfirmModal`, sama dengan Detail Reservasi Prioritas |
| Dependency | `FE-KSK-PO-007` ✅, `BE-KSK-PO-010` ✅ |
| Klasifikasi | `MEDIUM` — perintah petugas yang mengubah kuota, dengan konkurensi `RowVersion` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: route detail, view, modal, hook, slice, service, konstanta, util, style; laporan dan baris status di blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `508c8beb8` (`sukmagpV2`) + perubahan `FE-KSK-PO-001` s.d. `007` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `93f0fa1b` (`sukmagp`) + perubahan `BE-KSK-PO-008`/`010` yang belum di-commit |
| Tanggal | 2026-11-20 |
| Status | ✅ Selesai — AC 19 dan AC 20 (tampilan) terbukti di browser dengan perubahan nyata ke backend uji |

## 1. Keadaan yang ditemukan di awal

Backend `BE-KSK-PO-010` sudah menyediakan detail beserta `can*`, `rowVersion`, berkas, dan riwayat (nilai lama → baru). Ketiga perintah petugas menjawab detail terbaru. Galat dikembalikan sebagai pesan baku, dan `409` dibedakan dari isi pesannya:
- data basi `072`;
- status akhir `060`;
- jadwal tidak tersedia `063`;
- kuota penuh `064`;
- tanggal bentrok `065`.

Pola layar rujukan, sesuai roadmap, adalah Detail Reservasi Prioritas (`BaseDetailView`, `ConfirmModal`).

**Selisih kontrak yang ditemukan.** Roadmap menyebut `GET patient-encounters/doctor-schedule-quotas` untuk pilihan Ubah Jadwal. Ada tiga hal yang membuat endpoint ini tidak cocok:
- Endpoint itu hanya mengembalikan angka kuota per `doctorScheduleId`, tanpa nama dokter maupun jam.
- Endpoint itu butuh hak `PatientEncounter : Read`.
- Backend `ChangeScheduleAsync` justru memvalidasi jadwal baru dengan `LoadOfferedSchedulesAsync`, sumber yang sama dengan endpoint opsi publik `options/doctor-schedules` (grup *Online Registration Public*, juga bagian `KSK-CONTRACT-v2`).

## 2. Proses bisnis dari sisi pengguna

1. Dari Daftar Pendaftaran Online, petugas mengklik dua kali sebuah baris. Halaman detail menampilkan:
   - nomor dan status;
   - pasien: nama, No. RM atau "Pasien baru", NIK, tanggal lahir, jenis kelamin;
   - kunjungan: tanggal, poli, dokter, jam, sesi;
   - jenis pasien dan penjamin (Tunai, kartu tersamar, atau kartu baru);
   - rujukan dan consent;
   - kontak dan status email;
   - check-in dan penutupan.

   Untuk Pasien Rujukan, ada tabel **Berkas Surat Rujukan** dengan tombol **Buka** yang membuka berkas di tab baru. Tabel **Riwayat** mencatat setiap kejadian beserta nilai lama → baru, petugas, alasan, dan status.
2. Tombol **Ubah Jadwal**, **Ubah Kontak**, dan **Batalkan** hanya tampil bila status masih Menunggu Check-in (`can*`) **dan** petugas punya hak `OnlineRegistration : Update`/`Cancel`.
3. **Ubah Jadwal:**
   - Dialog menampilkan jadwal sekarang, kalender tanggal baru (besok s.d. 30 hari ke depan), dan pilihan jadwal dokter **di poli yang sama**. Contoh opsi: "dr. Arif Lesmana · 15:00 - 23:55 · Malam · sisa kuota 8". Jadwal penuh bertanda "(penuh)" dan ditolak.
   - Contoh uji: pendaftaran Rab 25 Nov dengan dr. Dewi dipindah ke Rab 2 Des dengan dr. Arif. Kuota dr. Dewi 25 Nov naik dari 14 ke 15, kuota dr. Arif 2 Des turun dari 8 ke 7, status tetap Menunggu Check-in, dan kode pasien tetap berlaku.
4. **Ubah Kontak:** isian No. WhatsApp atau email. Isian tidak sah dan kontak yang sama ditolak. Email kode tidak dikirim ulang.
5. **Batalkan:** alasan wajib 5–500 karakter. Sesudah batal, status menjadi Dibatalkan, alasannya tampil, dan semua tombol aksi hilang.
6. Bila data sudah diubah petugas lain, simpan ditolak dengan "Data sudah berubah. Muat ulang lalu coba lagi.". Dialog ditutup dan detail dimuat ulang dengan data terbaru.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`priority-reservation-detail-view.jsx`, `priority-reservation-action-modals.jsx`, `priority-reservation-modal-buttons.js`, `use-priority-reservation-detail.jsx`, `priority-reservation-slice.jsx`, `priority-reservation.utils.js`, `base-detail-view.jsx`, `base-detail-card.jsx`, `base-form-control.jsx`, `use-procurement-attachments.jsx` (pola buka Blob). Di backend: `OnlineRegistrationController.cs`, `OnlineRegistrationService.Commands.cs`, `OnlineRegistrationDtos.cs`, `PatientEncounterController.cs` (`doctor-schedule-quotas`), `DoctorScheduleQuotaDtos.cs`, validation matrix `KSK-VAL-PO-060` s.d. `072`.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/app/health-services/registration-management/online-registrations/[id]/page.jsx` | Baru | Route detail, judul "Detail Pendaftaran Online" |
| `src/components/view/health-services/registration-management/online-registrations/detail/online-registration-detail-view.jsx` | Baru | `BaseDetailView`, tabel Berkas Surat Rujukan (Buka), tabel Riwayat, tombol aksi, `AccessDeniedGate`, `ToastStack` |
| `src/components/view/health-services/registration-management/online-registrations/detail/online-registration-action-modals.jsx` | Baru | Tiga `ConfirmModal`: Ubah Jadwal (`BaseDateField` + `BaseNativeSelectField`), Ubah Kontak (`BaseTextField`), Batalkan (alasan wajib) |
| `src/lib/hooks/health-services/registration-management/online-registrations/use-online-registration-detail.jsx` | Baru | Muat detail, hak + `can*`, pilihan jadwal publik, validasi layar, kirim perintah dengan `expectedRowVersion`, penanganan `409`, buka berkas |
| `src/lib/state/slice/health-services/registration-management/online-registration-slice.js` | Diubah | Detail + tiga perintah; detail terbaru dari jawaban perintah |
| `src/lib/services/health-services/registration-management/online-registration.service.js` | Diubah | `getOnlineRegistrationDetail`, `changeOnlineRegistrationSchedule`, `changeOnlineRegistrationContact`, `cancelOnlineRegistration`, `getOnlineRegistrationDocument` (Blob) |
| `src/lib/constants/health-services/registration-management/online-registration-constants.js` | Diubah | Aksi detail, batas alasan dan kontak, pesan `060` s.d. `072` |
| `src/utils/health-services/registration-management/online-registration/online-registration-staff.utils.js` | Diubah | Baris detail, kejadian riwayat lama → baru, opsi jadwal + sisa kuota/penuh, validasi alasan, pengenal `409` basi |
| `src/style/health-services/registration-management/online-registrations/online-registration.module.css` | Diubah | Permukaan detail datar, judul bagian, tumpukan modal, teks tombol primer (token saja, pola Reservasi Prioritas) |

Tidak ada perubahan backend, migration, atau database.

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Penerapan |
| --- | --- |
| View tidak memanggil Axios | View → hook → slice/service; pilihan jadwal lewat `online-registration-public.service.js` |
| Hak dari layar Akses Role | Tombol mengikuti `usePermission("OnlineRegistration", "Update"/"Cancel")` dan `can*` backend; `403` → `AccessDeniedGate`; tanpa peran hardcode |
| Konkurensi | Setiap perintah mengirim `expectedRowVersion` dari detail; detail terbaru (dengan `rowVersion` baru) menggantikan state |
| Pembersihan | Request detail dan opsi jadwal dibatalkan saat unmount atau tanggal berganti; URL Blob dilepas 60 detik sesudah dibuka |
| Privasi | Kode check-in tidak ada di respons; berkas dibuka lewat endpoint terotorisasi, tidak lewat URL publik |

### 3.4 Gerbang keputusan base component

UI GATE: 11 elemen — REUSE 8, EXTEND 0, COMPOSE 3, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kerangka detail + baris data | `BaseDetailView` | Detail Reservasi Prioritas | REUSE | Baris kosong (mis. rujukan pada pasien bukan rujukan) tidak ditampilkan |
| Tombol aksi | `BaseButton` | sama | REUSE | — |
| Berkas rujukan | `DataTable` + `BaseButton` | sama | REUSE | Tombol Buka per berkas |
| Riwayat | `DataTable` + `StatusBadge` | sama | REUSE | Kejadian lama → baru |
| Batalkan | `ConfirmModal` `requireReason` | sama | REUSE | — |
| Notifikasi | `ToastStack` | sama | REUSE | — |
| Tanpa hak | `AccessDeniedGate` | sama | REUSE | — |
| Pesan dalam dialog | `InformationAlert` | sama | REUSE | — |
| Ubah Jadwal | `ConfirmModal` + `BaseDateField` + `BaseNativeSelectField` + `InformationAlert` | pola Ganti Tanggal Reservasi Prioritas | COMPOSE | Dialog |
| Ubah Kontak | `ConfirmModal` + `BaseTextField` | pola yang sama | COMPOSE | Dialog |
| Gaya detail/dialog | CSS feature | aturan `.flatDetailSurface`, `.modalStack`, `.primaryButtonText` Reservasi Prioritas | COMPOSE | Disalin ke CSS modul sendiri, token saja |

Untuk bentuk Ubah Jadwal/Ubah Kontak (`DEV_DISCRETION`) ada dua pilihan:
- **A. `ConfirmModal`, sama dengan dialog Ganti Tanggal Reservasi Prioritas (rekomendasi, dijalankan).** Petugas mengenal polanya, dan tidak butuh komponen baru.
- **B. Drawer atau halaman tersendiri.** Ruang lebih luas, tetapi tidak ada drawer base, dan pola layarnya berbeda dari modul tetangga.

Untuk sumber pilihan jadwal ada dua pilihan:
- **A. Endpoint publik `options/doctor-schedules` (rekomendasi, dijalankan).** Aturan yang ditampilkan sama dengan validasi backend, data dokter, jam, dan sisa kuota lengkap, dan tidak butuh hak tambahan.
- **B. `doctor-schedule-quotas` sesuai roadmap.** Tetap butuh daftar jadwal dari endpoint petugas lain, plus hak `PatientEncounter : Read` dan hak jadwal dokter.

Selisih ini dicatat sebagai delta kontrak di bagian 8.

## 4. State yang ditangani di layar

| Keadaan | Penanganan |
| --- | --- |
| Memuat | `BaseDetailView` "Mengambil detail pendaftaran..."; tombol aksi nonaktif; dialog "Memuat jadwal dokter..."; tombol Buka "Membuka..." |
| `404` | "Pendaftaran tidak ditemukan." |
| `403` | `AccessDeniedGate` |
| Gagal muat | Pesan galat `BaseDetailView` |
| Tidak ada jadwal | "Tidak ada jadwal dokter pada poli dan tanggal ini." |
| Validasi layar | Tanggal di luar H+1 s.d. H+30 (`061`), jadwal belum dipilih, tanpa perubahan (`066`), jadwal penuh (`064`), kontak tidak sah atau sama (`067`), alasan < 5 karakter (`070`) |
| `409` jadwal (`063`, `064`) | Pesan di dialog; pilihan jadwal dimuat ulang; dialog tetap terbuka |
| `409` basi (`072`) atau status akhir (`060`) | Toast "Data berubah"; dialog ditutup; detail dimuat ulang |
| Berkas gagal dibuka | Toast "Berkas tidak dapat dibuka." |

## 5. Endpoint yang dikonsumsi

| Endpoint | Pemakaian |
| --- | --- |
| `GET /api/v1/health-services/registration-management/online-registrations/{id}` | Detail, berkas, riwayat |
| `GET …/online-registrations/{id}/referral-documents/{documentId}` | Buka berkas (Blob) |
| `PUT …/online-registrations/{id}/schedule` `{ visitDate, doctorScheduleId, expectedRowVersion }` | Ubah Jadwal |
| `PUT …/online-registrations/{id}/contact` `{ contact, expectedRowVersion }` | Ubah Kontak |
| `POST …/online-registrations/{id}/cancel` `{ reason, expectedRowVersion }` | Batalkan |
| `GET …/online-registrations/public/options/doctor-schedules?clinicId=&visitDate=` | Pilihan jadwal Ubah Jadwal (delta, bagian 3.4) |

## 6. Verifikasi

Lingkungan uji: dev server frontend di port 3000 dan backend uji di `https://localhost:7185` (build Release dari scratchpad). Request browser ke 7184 dialihkan ke 7185. Keduanya saya nyalakan sendiri, lalu saya matikan sesudah uji. Backend user di 7184 tidak disentuh. Akun uji:
- `TEST-PO009 reader` (Read, Update, Cancel);
- `TEST-PO009 noaccess` (Read saja, diberikan uji BE-010);
- `TEST-PO007 noread` (tanpa hak).

Perubahan nyata dilakukan pada pendaftaran uji `FE-KSK-PO-003`: `0013` (jadwal), `0016` (kontak), dan `0015` (batal). Kredensial tidak dicetak.

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` pada berkas yang disentuh | PASS — 0 error, 0 warning (satu warning memoisasi diperbaiki) |
| `npm run lint:errors` | PASS — 0 error |
| `npm run build` | PASS — 855/855 halaman, `ƒ /health-services/registration-management/online-registrations/[id]` |
| Playwright 1366 px, run utama (`detail-check.cjs`) | 19 butir PASS. D2 gagal karena skrip mengharapkan URL `blob:` (di headless PDF diunduh); N2 tidak sempat jalan karena `networkidle` tidak tercapai di layar Akses Ditolak |
| Playwright D2/N2/N3 (`dn-check.cjs`) | **3/3 PASS** |
| Grep konsistensi UI | Bersih: tanpa hex/rgba, tanpa ukuran huruf, tanpa `<button>`/`<table>` mentah, tanpa inline style, tanpa `!important` |
| Grep privasi | 0 `console`/`localStorage`/`sessionStorage` |

Rincian uji browser:

| Butir | Hasil |
| --- | --- |
| D1 | Detail Rujukan `0014`: pasien, kunjungan, penjamin, faskes, nomor surat, diagnosa, consent, kontak |
| D2 | 2 baris berkas; Buka → `GET referral-documents` `200 application/pdf`, tab baru terbuka, tanpa pesan gagal |
| D3–D5 | Riwayat ≥ 1 kejadian; tiga tombol aksi tampil untuk akun ber-hak pada Menunggu Check-in; tidak ada teks berpola kode |
| S1 | Dialog Ubah Jadwal membuka tanggal dan jadwal sekarang; simpan tanpa perubahan → "Tidak ada perubahan jadwal." (`066`) |
| S2–S3 | Semua request pilihan jadwal memakai `clinicId` pendaftaran (poli sama); opsi memuat "sisa kuota N"; kalender: hari ini nonaktif (`061`) |
| S4 | Jadwal penuh (tiruan) tampil "(penuh)" dan ditolak dengan kalimat `064` |
| S5 | `409` kuota dari backend (tiruan) → dialog tetap terbuka dengan pesan, pilihan jadwal dimuat ulang |
| S6–S8 | **Nyata:** Rab 25 Nov dr. Dewi → Rab 2 Des dr. Arif; status tetap Menunggu Check-in; kuota `{dewi25: 14 → 15, arif02: 8 → 7}`; riwayat "25 November 2026 … → 2 Desember 2026 …" oleh TEST-PO009 reader (AC 19) |
| K1–K2 | "0812" ditolak (`067`); kontak sama (beda huruf besar/kecil) → "Tidak ada perubahan kontak."; **nyata:** kontak diganti `081311112222`, detail dan riwayat lama → baru diperbarui (AC 19) |
| R1 | **RowVersion basi nyata:** dialog terbuka, lalu petugas lain (API paralel) mengubah kontak; simpan → `409` "Data sudah berubah…", toast, dialog tertutup, detail menampilkan perubahan petugas lain (`072`) |
| B1–B2 | Alasan "abc" ditolak (`070`); **nyata:** batal dengan alasan → Dibatalkan, alasan tampil; tombol aksi hilang (AC 20) |
| D6 | 1366 px tanpa gulir mendatar; tanpa galat halaman |
| N1 | Akun Read saja: detail tampil, tiga tombol aksi tersembunyi |
| N2 | Akun tanpa hak: "Akses Ditolak", data tidak tampil |
| N3 | Id tidak dikenal: "Pendaftaran tidak ditemukan." |

```
MANUAL TEST: Playwright — 19 + 3 butir PASS (lihat catatan run); perubahan nyata ke backend uji: jadwal, kontak, RowVersion basi, batal
AUTOMATED TEST: SKIPPED (opsional) — tes otomatis baru tidak diwajibkan test-policy; bukti perilaku dari uji browser di atas
```

Catatan jalannya uji:
- Run pertama berhenti di S4. Penyebabnya: Escape pada kalender juga menutup dialog, dan hitungan baris berkas ikut menghitung tabel riwayat. Data belum berubah saat itu (dicek lewat API).
- Skrip dan tangkapan layar ada di scratchpad sesi (`fe-po008/detail-check.cjs`, `dn-check.cjs`, `run2.log`, `detail-cancelled-1366.png`) dan tidak di-commit.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
| --- | --- | --- |
| AC 19 (tampilan): ubah tanggal + dokter lain di poli yang sama; status tetap; kuota bertukar; riwayat lama → baru; kode tetap | S1–S8 (kode tidak disentuh perintah ini; terbukti di `BE-KSK-PO-010`) | ✅ |
| AC 19: jadwal baru penuh → ditolak, pendaftaran tidak berubah | S4, S5 | ✅ |
| AC 19: ubah kontak dengan riwayat, email tidak dikirim ulang | K1, K2 | ✅ |
| AC 20 (tampilan): batal dengan alasan → status 5; batal tanpa alasan / status akhir ditolak | B1, B2 (status akhir: tombol hilang; `409` `060`/`071` ditangani dengan muat ulang) | ✅ |
| Tambahan: bentuk dialog `DEV_DISCRETION` dicatat | 3.4 | ✅ |
| Tombol disembunyikan sesuai hak dan `can*`; `409` memuat ulang | D4, N1, N2, B2, R1 | ✅ |

## 8. Catatan penutup

- **Delta kontrak:** pilihan jadwal Ubah Jadwal memakai `GET …/public/options/doctor-schedules`, bukan `GET patient-encounters/doctor-schedule-quotas` (bagian 1 dan 3.4). Keduanya bagian `KSK-CONTRACT-v2`, tetapi kolom Kontrak kartu roadmap menyebut yang kedua. Endpoint publik dibatasi `OptionsPermitPerMinute` (60/menit per IP). Untuk pemakaian petugas jumlah ini cukup, tetapi bila banyak petugas berbagi satu IP keluar, batas itu perlu diperhatikan.
- `KSK-VAL-PO-062` (poli lain) tidak dapat terjadi dari layar, karena pilihan jadwal selalu dimuat dengan `clinicId` pendaftaran. `065` (tanggal bentrok) dan `063` ditangani sebagai pesan `409` di dialog. Aturannya terbukti di `BE-KSK-PO-010`.
- Data uji yang berubah:
  - `PO-20261120-0013` kini 2 Des dr. Arif;
  - `0016` kontak `uji.paralel@contoh.test` (perubahan paralel R1);
  - `0015` Dibatalkan.

  Semuanya ikut dibersihkan bersama fixture uji modul.
- Tidak ada commit, push, migration, atau eksekusi database langsung.

**Task berikutnya:** frontend layar petugas selesai (`MVP-3`). Sisa modul: `BE-KSK-PO-007` dan runtime `200` `BE-KSK-PO-006` pada 2026-11-21, lalu `FE-KSK-PO-005` dan `006`; runtime Tidak Datang `BE-KSK-PO-008` pada 2026-11-22.
