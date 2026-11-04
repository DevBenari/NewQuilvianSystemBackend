# Penjadwalan Kerja, Roster, dan On-Call

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Jenis | Flowchart proses bisnis |
| Slice | `S-B4` — Penjadwalan, Roster, dan On-Call |
| Status | `draft` — Sesuai keputusan `HRD-DEC-097` s.d. `HRD-DEC-103` |
| Tanggal | 2026-11-04 |

---

## 1. Alur 1: Penyusunan & Penerbitan Roster Bulanan Unit (`HRD-DEC-097`, `HRD-DEC-098`, `HRD-DEC-099`, `HRD-DEC-100`)

```mermaid
flowchart TD
    subgraph Kepala_Unit["Kepala Unit / Kepala Ruangan"]
        A1["Mulai: Buka Roster Unit"] --> A2["Buat Draft Roster Bulan Baru"]
        A2 --> A3{"Pilih Metode Pengisian"}
        A3 -- "Salin Periode Lalu" --> A4["Pilih Periode Sumber (Bulan Lalu)"]
        A3 -- "Terapkan Pola Shift" --> A5["Pilih Pola Shift (misal P-S-M-L-L)"]
        A3 -- "Manual Grid" --> A6["Isi Sel Shift Per Staf & Tanggal"]
        A4 --> A7["Matriks Shift Terisi"]
        A5 --> A7
        A6 --> A7
        A7 --> A8["Klik: Periksa Jadwal"]
        A9{"Hasil Evaluasi"} -- "Ada Masalah Memblokir (B1-B7)" --> A10["Perbaiki Jadwal di Kisi (Wajib Lolos)"]
        A10 --> A8
        A9 -- "Ada Peringatan (W1-W7)" --> A11["Pilih Alasan Wajib per Peringatan di Drawer"]
        A11 --> A12["Tombol Terbitkan Aktif"]
        A9 -- "Bersih (Tanpa Masalah)" --> A12
        A12 --> A13["Klik: Terbitkan Roster"]
    end

    subgraph Sistem_Backend["Sistem Backend & Mesin Validasi"]
        A2 --> S1["Auto-populate Staf Eligible dari WorkforceCore (Hanya Pegawai Siap Kerja)"]
        S1 --> A3
        A8 --> S2["Evaluasi Aturan B1-B7 & W1-W7"]
        S2 --> A9
        A13 --> S3["Validasi Ulang Otomatis Sebelum Terbit"]
        S3 -- "Lolos" --> S4["Ubah Status Periode ke 'Published'"]
        S4 --> S5["Kirim Notifikasi ke Pegawai & Kepala Bidang (FYI)"]
    end

    subgraph Konsumen_Lain["Modul Lain (Kehadiran & Modul Klinis)"]
        S4 --> K1["Modul Kehadiran (07): Membaca Jadwal Terbit untuk Referensi Absen"]
        S4 --> K2["Nurse View: Menampilkan Perawat Jaga Ruangan"]
    end
```

### Tabel Langkah Proses 1: Penyusunan & Penerbitan Roster

| No | Pelaku | Tindakan / Langkah | Masukan | Keluaran | Bila Gagal / Terjadi Masalah |
| :---: | --- | --- | --- | --- | --- |
| 1 | Kepala Unit | Membuat draft periode roster unit | Unit kerja dan bulan target | Draft roster berstatus `Draft`, staf terisi otomatis | Ditolak bila unit bukan unit kerja bergiliran (`HRD-DEC-097`) |
| 2 | Kepala Unit | Memilih metode pengisian cepat | Pilihan salin periode atau terapkan pola | Ratusan sel shift terisi otomatis dalam hitungan detik | Kepala unit dapat mengedit sel individual bila ada pengecualian |
| 3 | Kepala Unit | Menekan **Periksa Jadwal** | Matriks shift draft | Daftar error memblokir (`B1`–`B7`) dan peringatan (`W1`–`W7`) | Error memblokir (merah) wajib diperbaiki di kisi tabel |
| 4 | Kepala Unit | Memilih alasan peringatan | Dropdown alasan di drawer | Seluruh peringatan memiliki alasan terkonfirmasi | Tombol Terbitkan tetap terkunci bila ada alasan belum dipilih |
| 5 | Kepala Unit | Menekan **Terbitkan Roster** | Konfirmasi penerbitan | Status periode berubah menjadi `Published` | Sistem validasi ulang otomatis; ditolak bila ada bentrok baru |
| 6 | Sistem | Mengirimkan notifikasi | Event `RosterPublishedEvent` | Notifikasi in-app ke staf dinas dan Kepala Bidang | Notifikasi di-retry otomatis via queue |

---

## 2. Alur 2: Penggantian Petugas Darurat & Koreksi Mundur (`HRD-DEC-101`, `HRD-DEC-102`)

```mermaid
flowchart TD
    subgraph Kepala_Unit["Kepala Unit"]
        B1["Kondisi Darurat: Staf Sakit / Halangan Hari H"] --> B2["Buka Roster 'Published'"]
        B2 --> B3["Pilih Kotak Shift Staf Asal"]
        B3 --> B4["Klik: Ganti Petugas"]
        B4 --> B5["Pilih Staf Pengganti & Kategori Alasan"]
        B5 --> B6["Klik: Konfirmasi Penggantian"]
        
        C1["Koreksi Tanggal Mundur (Shift Lalu)"] --> C2{"Cek Selisih Hari"}
        C2 -- "<= 3 Hari Kalender" --> C3["Pilih Alasan Koreksi Mundur & Simpan"]
        C2 -- "> 3 Hari Kalender" --> C4["Form Terkunci: Wajib Lewat HRD"]
    end

    subgraph Sistem_Backend["Sistem Backend"]
        B6 --> SB1["Validasi Staf Pengganti (B1-B7)"]
        SB1 -- "Lolos" --> SB2["Simpan TrxShiftReplacement (Applied)"]
        SB2 --> SB3["Update Shift: IsManualOverride = true (Status Tetap Published)"]
        SB3 --> SB4["Kirim Notifikasi Instan (Pegawai Asal, Pengganti, Kepala Bidang)"]
        
        C3 --> SC1["Simpan Perubahan Shift"]
        SC1 --> SC2["Picu Event Rekalkulasi Absensi di Modul Kehadiran (07)"]
    end

    subgraph Pegawai_Pengganti["Pegawai Pengganti"]
        SB4 --> P1["Menerima Jadwal Tugas Pengganti"]
        P1 --> P2["Datang ke RS & Absen Tap-in Tanpa Kendala"]
    end
```

### Tabel Langkah Proses 2: Penggantian Staf & Koreksi Mundur

| No | Pelaku | Tindakan | Masukan | Keluaran | Bila Gagal |
| :---: | --- | --- | --- | --- | --- |
| 1 | Kepala Unit | Membuka modal Ganti Petugas pada shift terbit | Klik sel shift staf yang berhalangan | Form penunjukan staf pengganti | Ditolak bila periode sudah `Closed` |
| 2 | Kepala Unit | Memilih staf pengganti dan alasan | Nama staf pengganti & kategori alasan | Form terisi | Ditolak bila pengganti sedang cuti atau bentrok (`B1`–`B7`) |
| 3 | Sistem | Menerapkan penggantian langsung | Data mutasi penggantian | Status shift terbarui seketika (`IsManualOverride = true`), status roster tetap `Published` | Tidak perlu menunggu persetujuan Kepala Bidang (`HRD-DEC-101`) |
| 4 | Kepala Unit | Melakukan koreksi mundur jadwal | Tanggal dinas <= 3 hari lalu & alasan | Jadwal historis terkoreksi | Ditolak bila tanggal shift > 3 hari ke belakang (`HRD-DEC-102`) |
| 5 | Sistem | Menjalankan rekalkulasi absensi | Event koreksi mundur | Modul Kehadiran (07) menghitung ulang status hadir/mangkir | Rekalkulasi berjalan otomatis di latar belakang |

---

## 3. Alur 3: Tata Kelola Siaga (On-Call), Pemanggilan Cito & Integrasi Lab/Payroll (`HRD-DEC-103`, `LAB-DEC-111`)

```mermaid
flowchart TD
    subgraph Penyusun_Siaga["Penyusun Siaga"]
        D1["Kepala Unit: Susun Siaga Staf Penunjang Cito"] --> D3["Jadwal Siaga Terbit (Scheduled)"]
        D2["Pelayanan Medis / KSM: Susun Siaga Dokter Spesialis"] --> D3
        D3 --> D4["Jam Siaga Aktif: Status Berubah ke 'Active'"]
    end

    subgraph Unit_Klinis["Unit Pemanggil (Lab / IGD / OK)"]
        E1["Ditemukan Pasien Darurat / Hasil Tes Lab Kritis"] --> E2["Buka Daftar Tenaga Siaga Aktif"]
        E2 --> E3["Sistem Lab Menemukan Dokter Siaga (Status: Active)"]
        E3 --> E4["Klik: Panggil Siaga (Mencatat Waktu Panggil)"]
        E5["Dokter / Staf Tiba di Rumah Sakit"] --> E6["Input Waktu Kedatangan Staf"]
    end

    subgraph Mesin_SLA_Payroll["Sistem Backend & Integrasi"]
        E6 --> M1["Hitung ResponseMinutes = ArrivalTime - CallInTime"]
        M1 --> M2{"Evaluasi Terhadap ExpectedResponseMinutes"}
        M2 -- "<= Target SLA" --> M3["Set IsSlaMet = true (Memenuhi SLA)"]
        M2 -- "> Target SLA" --> M4["Set IsSlaMet = false (Melebihi SLA)"]
        M3 --> M5["Simpan ke TrxOnCallAssignment (Status Tetap Active Selama Periode)"]
        M4 --> M5
        M5 --> M6["Handoff ke Modul 09 (Lembur) & 10 (Payroll) untuk Insentif Tugas Panggil Cito"]
        D4 --> M7["Handoff Jam Standby ke Modul 10 untuk Tunjangan Siaga"]
    end
```

### Tabel Langkah Proses 3: Siaga & Pemanggilan Cito

| No | Pelaku | Tindakan | Masukan | Keluaran | Bila Gagal |
| :---: | --- | --- | --- | --- | --- |
| 1 | Penyusun Siaga | Membuat jadwal siaga dokter/staf | Periode tanggal, jam mulai, jam selesai, tipe siaga | Penugasan siaga `Scheduled` | Jam mulai wajib sebelum jam selesai |
| 2 | Sistem | Mengaktifkan periode siaga | Jam sekarang mencapai jam mulai | Status siaga menjadi `Active` | Status terbaca oleh `LabConfirmingDoctorResolver` |
| 3 | Analis Lab / Perawat | Mencatat pemanggilan darurat cito | Klik tombol Panggil Siaga | Waktu telepon panggil tercatat (`CallInTime`) | Dokter menerima notifikasi panggilan cito |
| 4 | Analis Lab / Perawat | Mencatat kedatangan dokter/staf di RS | Input waktu tiba fisik staf | Waktu tiba tercatat (`ArrivalTime`) | Waktu kedatangan tidak boleh mendahului waktu panggil |
| 5 | Sistem | Mengevaluasi SLA respon menit | `ResponseMinutes` vs `ExpectedResponseMinutes` | Indikator kepatuhan SLA mutu rumah sakit tercatat | Status terekam untuk laporan akreditasi Kemenkes |
| 6 | Sistem | Menghubungkan ke kompensasi | Rekap standby & transaksi call-in | Data masuk ke modul Payroll (10) dan Lembur (09) | Hak jasa medis dibayarkan transparan |

---

## 4. Alur 4: Penutupan Periode Roster oleh Payroll (`HRD-DEC-102`)

```mermaid
flowchart TD
    subgraph Tim_Payroll["Tim Payroll HRD"]
        F1["Proses Cut-off Penggajian Bulanan"] --> F2["Buka Daftar Roster Unit Bulan Berjalan"]
        F2 --> F3["Klik: Tutup Periode (Close)"]
    end

    subgraph Sistem["Sistem Penjadwalan 06"]
        F3 --> SF1{"Cek Status Roster"}
        SF1 -- "Masih Draft" --> SF2["Tolak: Periode Belum Terbit (HTTP 422)"]
        SF1 -- "Published" --> SF3["Ubah Status ke 'Closed' & Catat ClosedAt, ClosedByUserId"]
        SF3 --> SF4["Kunci Mutlak Seluruh Jadwal (Read-Only)"]
    end

    subgraph Pasca_Tutup["Operasional Pasca-Tutup"]
        SF4 --> P1["Seluruh Percobaan Edit Ditolak (HTTP 403)"]
        P1 --> P2["Penyesuaian Selisih Diselesaikan via Rapel Payroll di Periode Berjalan"]
    end
```
