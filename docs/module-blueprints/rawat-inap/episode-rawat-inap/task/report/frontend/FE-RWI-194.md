# Laporan Task FE-RWI-194: Daftar Pesanan Ruang Bedah Pasien

| Metadata | Nilai |
|---|---|
| **Task ID** | `FE-RWI-194` |
| **Layar Blueprint** | `FE-INP-26` (Daftar Kasus Operasi & Kartu Detail Episode `FE-INP-04`) |
| **Status** | Selesai (Completed) |
| **Tanggal Selesai** | 5 Oktober 2026 |
| **Pelaksana** | Muhammad Hamzah (Agentic Pair Programming) |
| **Requirement Terkait** | `FR-RWF-044`, `FR-RWF-086`, `RWI-DEC-204`, `AC-RWF-041`, `UAT-RWF-24` |
| **Dependency** | `BE-RWI-174` |

---

## 1. Ringkasan Pekerjaan
Menambahkan daftar pesanan ruang bedah pasien tepat di bawah formulir pemesanan pada Ruang Kerja Keperawatan dan menambahkan kartu "Operasi & Jadwal Bedah" pada Detail Episode (`FE-INP-04`). Menampilkan 8 label status kasus operasi (Diminta, Terjadwal, Siap, Berjalan, Selesai, Ditunda, Dibatalkan, Ditolak), informasi penolakan (alasan, penolak, waktu penolakan), status pra-operasi, status serah terima, tombol Pesan Ulang, tombol Pra-Operasi, tombol Pasca Operasi, dan tombol Muat Ulang.

---

## 2. Berkas yang Dibuat dan Diubah

1. **`src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/patient-surgery-cases-table.jsx`**
   - Menampilkan tabel pesanan ruang bedah pasien ini via `inpatientSurgeryBookingService.getCasesByEncounter(encounterId)`.
   - Delapan label status operasi lengkap dengan skema warna dan badge.
   - Penolakan kasus diberi label merah tegas beserta alasan kutipan, nama penolak, dan waktu penolakan (`UAT-RWF-24`).
   - Tombol "Pesan ulang" untuk kasus ditolak yang otomatis mengisi kembali order ke formulir di atasnya.
   - Tombol "Pra-operasi" dan "Pasca operasi" sebagai pintu masuk ke `FE-RWI-195` dan `FE-RWI-196`.
   - Keterangan pengingat: *"Label 'Diminta' tidak menjamin ruang. Konfirmasi dan penetapan ruang dilakukan oleh petugas kamar operasi."*
   - Keadaan kosong: *"Belum ada pesanan ruang bedah."*
   - Keadaan gagal: *"Daftar operasi tidak dapat dimuat. Coba lagi."*
   - Dilindungi hak akses `OperatingRoomCase : Read`.
2. **`src/components/features/health-services/inpatient-management/surgery-case-summary-card.jsx`**
   - Kartu ringkas operasi pasien pada layar Detail Episode `FE-INP-04`.
   - Menampilkan daftar ringkas kasus beserta status dan tautan langsung ke Ruang Kerja Keperawatan tab Pemesanan Ruangan Bedah.
3. **`src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx`**
   - Mengintegrasikan `SurgeryCaseSummaryCard` tepat di bawah kartu keuangan.

---

## 3. Bukti Verifikasi

### AUTOMATED TEST
```bash
cmd.exe /c node --test tests/unit/surgery-booking.test.mjs
```
Hasil:
- ✔ FE-RWI-194 Acceptance Criteria: Tabel kasus dan kartu operasi memenuhi delapan label status dan aksi baris
- ℹ tests 4, pass 4, fail 0 (PASS)

### MANUAL TEST
- Seluruh 8 label status teruji tampil dengan warna dan teks yang tepat.
- Kasus berstatus Ditolak memunculkan kotak rincian alasan merah serta tombol "Pesan ulang".
- Kartu Operasi & Jadwal Bedah pada detail episode dapat dibuka-tutup (accordion) dan memiliki tombol pintasan ke tab pemesanan keperawatan.
