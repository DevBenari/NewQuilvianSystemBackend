# Laporan Task FE-RWI-193: Pemesanan Ruangan Bedah Bangsal

| Metadata | Nilai |
|---|---|
| **Task ID** | `FE-RWI-193` |
| **Layar Blueprint** | `FE-INP-25` (Pemesanan Ruangan Bedah, menu ke-7 `FE-KEP-07`) |
| **Status** | Selesai (Completed) |
| **Tanggal Selesai** | 5 Oktober 2026 |
| **Pelaksana** | Muhammad Hamzah (Agentic Pair Programming) |
| **Requirement Terkait** | `FR-RWF-040` s.d. `043`, `RWI-DEC-175`, `176`, `218`, `219`, `220` butir 5, `AC-RWF-040`, `048`, `UAT-RWF-32`, `42`, `43` |
| **Dependency** | `BE-RWI-175` |

---

## 1. Ringkasan Pekerjaan
Menggantikan tampilan *placeholder* menu ketujuh pada Ruang Kerja Keperawatan ("Pemesanan Ruangan Bedah") dengan formulir aktif yang memiliki dua tab: **Bedah Operasi** (`Surgery`) dan **Bedah Obgyn** (`Obstetric`). Formulir ini memungkinkan perawat atau dokter memesan ruang bedah yang merujuk satu order tindakan operasi aktif milik kunjungan episode dan menampilkan estimasi tarif tindakan.

---

## 2. Berkas yang Dibuat dan Diubah

1. **`src/lib/services/health-services/inpatient-management/inpatient-surgery-booking.service.js`**
   - Implementasi fungsi `bookSurgeryRoom` memanggil endpoint `POST /v1/health-services/inpatient-management/episodes/{episodeId}/surgery-bookings`.
   - Mengirimkan header `Idempotency-Key` (UUIDv4) untuk mencegah pesanan ganda.
2. **`src/utils/health-services/inpatient-management/inpatient-surgery-booking-utils.js`**
   - Definisi 8 status kasus operasi IBS, meta badge warna, pemformatan mata uang Rupiah, dan tanggal/waktu Indonesia.
3. **`src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-form.jsx`**
   - Dua tab: Bedah Operasi dan Bedah Obgyn (jenis kasus terkunci Obstetri).
   - Pengambilan order tindakan operasi aktif via `getPatientProcedures({ encounterId, procedureStatus: "Scheduled" })`.
   - Dokter operator terbaca read-only dari order tindakan ("dari order, tidak dapat diubah").
   - Menampilkan perkiraan tarif tindakan dari `UnitPrice` dan `CoverageStatus` berlabel "perkiraan — tagihan final di kasir" dengan catatan biaya anestesi, sewa kamar operasi, dan bahan yang menyusul.
   - Fallback tarif belum tersedia jika harga null/nol namun tombol pesan tetap aktif.
   - Peringatan banner saat episode `DischargePending` ("Pemesanan hanya untuk pasien yang sedang dirawat").
   - Keterangan info saat belum ada order aktif ("Tindakan operasi belum dipesan dokter. Minta dokter memesan tindakan lebih dulu.") dan tombol dinonaktifkan.
   - Pemeriksaan hak akses `OperatingRoomCase : Create`.
4. **`src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-section.jsx`**
   - Komponen induk section yang menghubungkan form pemesanan dengan tabel kasus pasien.
5. **`src/components/view/health-services/inpatient-management/nursing-workspace/components/nursing-workspace-sections.jsx`**
   - Menghubungkan section `surgery-booking` menggantikan `NursingUnavailableSection`.
6. **`tests/unit/surgery-booking.test.mjs`**
   - Unit test otomatis pengujian kelengkapan berkas, service, form, dan acceptance criteria.

---

## 3. Bukti Verifikasi

### AUTOMATED TEST
```bash
cmd.exe /c node --test tests/unit/surgery-booking.test.mjs
```
Hasil:
- ✔ FE-RWI-193 / FE-RWI-194: Seluruh berkas komponen dan service pemesanan ruang bedah tersedia
- ✔ FE-RWI-193: Service memuat endpoint bookSurgeryRoom dengan header Idempotency-Key dan unwrap response
- ✔ FE-RWI-193 Acceptance Criteria: Form memenuhi UAT-RWF-32, UAT-RWF-42, UAT-RWF-43, dan 2 Tab
- ℹ tests 4, pass 4, fail 0 (PASS)

### MANUAL TEST
- Tab Bedah Operasi dan Bedah Obgyn dapat beralih dengan mulus.
- Ketika order aktif belum ada, muncul notifikasi bahwa dokter belum memesan dan tombol dinonaktifkan (`UAT-RWF-32`).
- Pasien dalam status DischargePending memicu banner peringatan pasien sedang dalam proses pemulangan (`UAT-RWF-42`).
- Perkiraan tarif dan komponen biaya IBS tampil informatif (`UAT-RWF-43`).
