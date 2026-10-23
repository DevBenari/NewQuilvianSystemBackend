# Laporan Pengujian Ulang Modul IGD — Skenario P1-06R

## Metadata Pengujian

| Parameter | Keterangan |
| --- | --- |
| **Tanggal Pengujian** | 19 Oktober 2026 |
| **Ruang Lingkup** | Uji Ulang Pembatalan Reservasi Pra-Kedatangan IGD Lewat Antarmuka Layar (`P1-06R`) |
| **Dasar Hukum & Keputusan** | `FE-IGD-067`, `BE-IGD-080`, `IGD-DEC-314`, `IGD-DEC-315`, `IGD-DEC-329` (Perbaikan Destrukturisasi Parameter `ConfirmModal`), `IGD-FACT-149` |
| **Pelaksana** | AI Agent Antigravity (Skill `live-browser-testing`) |
| **Engine Peramban** | Chromium Headed Mode (Visual / `headless: false`, slowMo: 400ms, Google Chrome) |
| **Lingkungan Frontend** | Next.js App Router, Port 3000 (`http://localhost:3000`), Mode Produksi Standalone |
| **Lingkungan Backend** | ASP.NET Core Kestrel (`https://localhost:7184`), Port 7184 / 5107 |
| **Akun Pelaksana Uji** | Petugas Pendaftaran & Admisi IGD: `rendy.saputra@rsmmc.local` (Peran Loket / Admisi) |
| **Status Akhir Pengujian** | ✅ **PASS (100% LULUS)** — Cacat UI Destrukturisasi Terbukti Tuntas |

---

## 1. Ringkasan Eksekutif

Pada pengujian Putaran P1 tanggal 18 Oktober 2026 malam, skenario pembatalan reservasi pra-kedatangan lewat antarmuka layar (`P1-06`) dinyatakan **FAIL pada UI** karena penekanan tombol *Ya, Batalkan Reservasi* pada modal selalu menghasilkan galat **HTTP 400 Bad Request** (*"Alasan pembatalan wajib diisi."*). Hal tersebut terjadi akibat destrukturisasi salah pada `EmergencyPreArrivalCancelModal.jsx` (`handleConfirm = ({ reason }) => ...` membaca `reason` dari objek `SyntheticEvent` yang memanggilnya).

Setelah perbaikan kode satu baris diterapkan pada commit `6b44f8742` (`handleConfirm = (_event, reason) => { onConfirm?.(reason); }`), pengujian ulang **P1-06R** ini dijalankan secara visual (*headed Chromium*) menggunakan akun resmi Petugas Admisi IGD (`rendy.saputra@rsmmc.local`).

### Temuan Utama Pengujian Ulang P1-06R:
1. **Pembuatan Data Reservasi Uji Mandiri Berhasil:**
   Petugas admisi membuat satu reservasi pra-kedatangan baru tanpa mengaitkan master pasien (`Keluarga Pasien Uji P1-06R`, kontak `081299887766`, keluhan *"Pasien mengeluh pusing berputar dan mual"*). Sistem menerbitkan nomor reservasi resmi **`RES-EMG-261019024704-022FD4`** (ID: `f30c9549-7fcc-4357-9bd5-50953e733fd3`) dengan status awal **`PendingArrival`** (`1`).
2. **Pembatalan Lewat Antarmuka Layar Berhasil Sempurna (HTTP 200 OK):**
   Petugas menekan tombol berikon silang merah *Batalkan Reservasi* pada baris reservasi di tabel, mengisi alasan pembatalan *"Pasien membatalkan kedatangan karena kondisi membaik"*, lalu menekan tombol konfirmasi *Ya, Batalkan Reservasi*. Respon jaringan mencatat **HTTP 200 OK** dari endpoint `POST /api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}/cancel`.
3. **Pembaruan Status dan Tampilan Antarmuka Real-Time:**
   - Status reservasi berubah menjadi **`Cancelled`** (`3`).
   - Lencana status pada tabel langsung berubah warna menjadi merah bertuliskan **Dibatalkan**.
   - Alasan pembatalan tampil pada sub-informasi kolom status: *"Alasan: Pasien membatalkan kedata..."*.
   - Kolom aksi tidak lagi menyediakan tombol *Konfirmasi Tiba* maupun *Batalkan*, melainkan terkunci dengan teks non-aktif *"Dibatalkan"*.
   - Notifikasi toast sukses tampil di layar: *"Reservasi pra-kedatangan IGD berhasil dibatalkan."*.

---

## 2. Alur Proses Bisnis Pengujian Pembatalan Reservasi

Berikut alur kerja operasional pembatalan panggilan gawat darurat pra-kedatangan di rumah sakit:

```
[Keluarga Pasien Menghubungi Hotline IGD]
                  │
                  ├─ Pasien awalnya memesan reservasi kedatangan mandiri/ambulans
                  └─ Reservasi tercatat di sistem: RES-EMG-261019024704-022FD4
                  │
                  ▼
[Keluarga Pasien Menelepon Kembali untuk Membatalkan]
                  │
                  ├─ Kondisi pasien membaik / memilih faskes terdekat lain
                  └─ Petugas Admisi IGD membuka tabel Reservasi Pra-Kedatangan
                  │
                  ▼
[Petugas Menekan Tombol "Batalkan Reservasi"]
                  │
                  ├─ Sistem menampilkan ConfirmModal verifikasi pembatalan
                  ├─ Petugas WAJIB mengetik alasan pembatalan medis/operasional
                  └─ Input: "Pasien membatalkan kedatangan karena kondisi membaik"
                  │
                  ▼
[Petugas Menekan "Ya, Batalkan Reservasi"]
                  │
                  ├─ Frontend mengirim POST /api/v1/.../cancel dengan payload { cancellationReason }
                  ├─ Backend memvalidasi hak akses dan status reservasi
                  ├─ Database menyimpan waktu pembatalan, user ID petugas, dan alasan pembatalan
                  └─ Status reservasi berganti menjadi Status 3 (Cancelled)
                  │
                  ▼
[Tampilan Antarmuka Terperbarui Otomatis]
                  │
                  ├─ Muncul Toast Sukses: "Reservasi pra-kedatangan berhasil dibatalkan"
                  ├─ Lencana tabel menjadi "Dibatalkan"
                  └─ Aksi ditutup permanen (arsip audit trail)
```

### Contoh Skenario Rumah Sakit:
> Petugas admisi IGD Sdr. Rendy Saputra menerima panggilan darurat dari keluarga pasien yang mengabarkan pasien mengalami pusing hebat. Sdr. Rendy mencatat reservasi pra-kedatangan agar tim perawat triage bersiap. Dua puluh menit kemudian, pihak keluarga menelepon kembali bahwa pasien sudah merasa jauh lebih baik setelah istirahat dan memutuskan untuk tidak jadi berangkat ke IGD rumah sakit. Sdr. Rendy membuka daftar reservasi, menekan tombol *Batalkan Reservasi*, mencatat alasan pembatalan, dan mengonfirmasi pembatalan. Status reservasi langsung terarsip sebagai *Dibatalkan* tanpa meninggalkan data sampah atau kunjungan menggantung.

---

## 3. Spesifikasi Endpoint API Terkait (Bergaya Swagger)

Spesifikasi endpoint backend yang dieksekusi dan dibuktikan pada uji layar P1-06R:

`[Tags("Health Services / Emergency Installation Management / Emergency Pre-Arrival Reservation")]`

| Method | Path | Deskripsi | Auth / Role | Request Body | Response Sukses |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations` | Membuat pencatatan reservasi pra-kedatangan IGD baru | Bearer Token / Petugas Admisi IGD | `CreateEmergencyPreArrivalReservationRequest`<br>• `callerName`: `"Keluarga Pasien Uji P1-06R"`<br>• `callerPhoneNumber`: `"081299887766"`<br>• `chiefComplaint`: `"Pasien mengeluh pusing..."` | `201 Created`<br>`{ success: true, statusCode: 201, data: { id, reservationNumber, status: 1 } }` |
| `POST` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}/cancel` | Membatalkan reservasi pra-kedatangan dengan alasan wajib | Bearer Token / Petugas Admisi IGD | `CancelEmergencyPreArrivalReservationRequest`<br>• `cancellationReason`: `"Pasien membatalkan kedatangan karena kondisi membaik"` | `200 OK`<br>`{ success: true, statusCode: 200, data: { id, status: 3, cancellationReason, cancelledAt, cancelledByUserId } }` |

---

## 4. Rincian Langkah dan Hasil Pengujian Ulang (P1-06R)

Pengujian dilakukan mandiri oleh Agent menggunakan skrip Playwright headed Chromium `test-p1-06r-cancel-reservation.mjs`:

| Tahap | Aktor | Aksi & Input | Hasil yang Diharapkan | Hasil Pengamatan Visual & Jaringan | Status |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **1** | Petugas Loket | Login ke aplikasi di `http://localhost:3000/login` menggunakan email `rendy.saputra@rsmmc.local` dan kata sandi dari variabel lingkungan `QUILVIAN_LOKET_PASSWORD` (tidak ditulis di berkas ini, `IGD-DEC-215`) dengan izin geolokasi RS. | Login berhasil, redirect ke beranda, cookie otentikasi terpasang. | Berhasil login tanpa hambatan 401. URL berganti ke `http://localhost:3000/`. Bukti: `01_login_filled.png`. | **PASS** |
| **2** | Petugas Loket | Navigasi ke menu Reservasi Pra-Kedatangan IGD (`/health-services/emergency-installation-management/pre-arrival-reservations`). | Halaman terbuka, Hero banner tampil, metrik ringkasan dan tabel termuat. | Halaman termuat sempurna dalam 2.5 detik. Bukti: `02_daftar_reservasi_awal.png`. | **PASS** |
| **3** | Petugas Loket | Tekan tombol *Catat Reservasi Baru*, isi form pemesan `Keluarga Pasien Uji P1-06R`, nomor telepon `081299887766`, keluhan pusing mual, lalu tekan *Simpan Reservasi*. | Dialog modal tertutup, POST API 201 Created, reservasi baru muncul di tabel dengan status Menunggu Kedatangan. | Respon `POST` mengembalikan status `201 Created`. Terbit nomor reservasi `RES-EMG-261019024704-022FD4`. Modal tertutup dan baris muncul di tabel. Bukti: `03_form_reservasi_terisi.png`, `04_reservasi_muncul_di_tabel.png`. | **PASS** |
| **4** | Petugas Loket | Cari baris `RES-EMG-261019024704-022FD4`, tekan tombol aksi bahaya *Batalkan Reservasi* (ikon `RiCloseCircleLine`). | Modal konfirmasi `ConfirmModal` terbuka dengan judul "Batalkan Reservasi Pra-Kedatangan?". | Modal terbuka menampilkan rincian nomor reservasi, textarea alasan pembatalan dengan tanda bintang merah, tombol *Kembali*, dan tombol *Ya, Batalkan Reservasi*. Bukti: `05_modal_konfirmasi_batal_terbuka.png`. | **PASS** |
| **5** | Petugas Loket | Ketik alasan pada textarea: *"Pasien membatalkan kedatangan karena kondisi membaik"*, lalu klik tombol *Ya, Batalkan Reservasi*. | Modal tertutup, POST cancel berhasil HTTP 200 OK, toast sukses muncul, tidak ada galat 400. | Callback `handleConfirm(_event, reason)` meneruskan teks alasan secara utuh. Payload JSON terkirim dengan benar. Respon `POST /cancel` mengembalikan **HTTP 200 OK** (`status: 3`). Bukti: `06_alasan_batal_terisi.png`, `07_setelah_konfirmasi_batal.png`. | **PASS** |
| **6** | Petugas Loket | Periksa baris reservasi pada tabel setelah pembatalan selesai. | Status reservasi menjadi `Dibatalkan`, alasan tampil, tombol aksi check-in dan batal dinonaktifkan. | Lencana status berwarna merah `Dibatalkan`. Teks alasan tampil: `Alasan: Pasien membatalkan kedata...`. Kolom aksi menampilkan teks `Dibatalkan`. Bukti: `08_tabel_status_dibatalkan_final.png`. | **PASS** |

---

## 5. Bukti Validasi Data & Respon API

### Respon Jaringan Pembatalan Reservasi (P1-06R):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Reservasi pra-kedatangan IGD berhasil dibatalkan.",
  "data": {
    "id": "f30c9549-7fcc-4357-9bd5-50953e733fd3",
    "reservationNumber": "RES-EMG-261019024704-022FD4",
    "patientId": null,
    "patientName": null,
    "medicalRecordNumber": null,
    "patientMembershipId": null,
    "memberNumber": null,
    "membershipTierId": null,
    "tierCode": null,
    "tierName": null,
    "priorityLevel": 0,
    "callerName": "Keluarga Pasien Uji P1-06R",
    "callerPhoneNumber": "081299887766",
    "estimatedArrivalDateTime": "2026-10-18T20:17:00Z",
    "chiefComplaint": "Pasien mengeluh pusing berputar dan mual",
    "requiresAmbulance": false,
    "ambulanceArrivalModeId": null,
    "ambulanceArrivalModeName": null,
    "ambulancePickupAddress": null,
    "assignedEscortStaffName": null,
    "status": 3,
    "statusName": "Cancelled",
    "resultEmergencyVisitId": null,
    "emergencyVisitNumber": null,
    "arrivedAt": null,
    "cancelledAt": "2026-10-19T02:47:12.008695Z",
    "cancelledByUserId": "907590c9-dab7-44d1-9402-91bd2da20c3c",
    "cancellationReason": "Pasien membatalkan kedatangan karena kondisi membaik",
    "notes": null,
    "createDateTime": "2026-10-19T02:47:04.126222Z",
    "rowVersion": "76dd73d9-2d6f-40cc-92bc-3eacc5c06ad8"
  },
  "errors": null,
  "timestamp": "2026-10-19T09:47:12.0569061+07:00"
}
```

---

## 6. Daftar Berkas Bukti Visual Pengujian

Seluruh berkas bukti tangkapan layar tersimpan pada folder resmi pengujian frontend:  
`QuilvianSystemFrontendDev/test-with-agy/screenshots/uji-p1-06r/`

1. `01_login_filled.png`: Form login terisi akun petugas loket/admisi `rendy.saputra@rsmmc.local`.
2. `02_daftar_reservasi_awal.png`: Tampilan layar awal reservasi pra-kedatangan IGD beserta hero dan kartu metrik.
3. `03_form_reservasi_terisi.png`: Modal pembuatan reservasi baru dengan data pemesan dan keluhan awal terisi.
4. `04_reservasi_muncul_di_tabel.png`: Reservasi baru `RES-EMG-261019024704-022FD4` berhasil masuk tabel (Status: Menunggu Kedatangan).
5. `05_modal_konfirmasi_batal_terbuka.png`: Dialog konfirmasi pembatalan `ConfirmModal` terbuka dengan field alasan wajib.
6. `06_alasan_batal_terisi.png`: Alasan pembatalan telah diisi pada textarea sebelum tombol konfirmasi ditekan.
7. `07_setelah_konfirmasi_batal.png`: Notifikasi toast sukses dan pembaruan tabel seketika setelah tombol konfirmasi ditekan.
8. `08_tabel_status_dibatalkan_final.png`: Verifikasi tabel final dengan lencana merah `Dibatalkan` dan alasan tersimpan.
9. `p1-06r-summary.json`: Ringkasan log eksekusi otomasi Playwright.

---

## 7. Kesimpulan dan Rekomendasi Status Task

Berdasarkan hasil uji ulang layar visual (**P1-06R**):
- Cacat antarmuka pada modal pembatalan reservasi pra-kedatangan terbukti **100% terselesaikan**.
- Seluruh 7 Acceptance Criteria pada task **`FE-IGD-067`** kini telah terpenuhi secara utuh dan terverifikasi di peramban Chromium aktif.
- Status task **`FE-IGD-067`** direkomendasikan dinaikkan secara resmi dari 🟡 **SEBAGIAN** menjadi ✅ **DONE**.
