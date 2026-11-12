# Alur Proses Bisnis Presensi dan Kehadiran Staf Rumah Sakit 24/7 (Slice `S-B1`)

Dokumen ini memodelkan alur proses bisnis operasional presensi staf rumah sakit untuk **Sub-modul 07 Attendance Management (Slice `S-B1`)**, menyerap keputusan interview `HRD-DEC-028`, `HRD-DEC-029`, `HRD-DEC-104` s.d. `HRD-DEC-109`.

---

## 1. Alur 1: Penyerapan Log Mentah Biometrik & Pencocokan Shift Malam Lintas Hari (`HRD-DEC-104`)

Diagram ini menggambarkan bagaimana log tap mesin biometrik diserap secara *immutable* dan dipasangkan ke penugasan shift malam yang melintasi tengah malam.

```mermaid
flowchart TD
    subgraph Staf Medis / Perawat
        A1[Pegawai Tap Masuk di Mesin Fingerprint / Wajah] --> A2[Berdinas Shift Malam 21.00 s.d. 07.00]
        A2 --> A3[Pegawai Tap Pulang Keesokan Pagi]
    end

    subgraph Mesin & Layanan Ingestion
        A1 -.-> B1[Log Mentah Diterima Server]
        A3 -.-> B2[Log Mentah Pulang Diterima Server]
        B1 --> B3[(HrdAttendanceRawLog: Immutable)]
        B2 --> B3
    end

    subgraph Mesin Schedule Resolver
        B3 --> C1[Pencarian Jadwal Roster Aktif S-B4]
        C1 --> C2{Apakah Shift Malam Lintas Hari?}
        C2 -- Ya --> C3[Kaitkan Log Masuk & Pulang ke Tanggal Shift Dimulai Hari H]
        C2 -- Tidak --> C4[Kaitkan ke Jadwal Shift Standar Hari H]
        C3 --> C5[(HrdAttendanceDaily: ShiftDate = Hari H)]
        C4 --> C5
    end
```

### Tabel Langkah Alur 1

| Langkah | Aktor Pelaku | Masukan | Keluaran | Tindakan Bila Gagal |
| :---: | --- | --- | --- | --- |
| 1 | Staf Medis | Jari / Wajah pada sensor mesin | Log mentah biometrik | Coba ulang sensor biometrik atau gunakan tap cadangan |
| 2 | Layanan Ingestion | Payload log biometrik | `HrdAttendanceRawLog` tersimpan | Antrean lokal mesin menyimpan offline hingga jaringan pulih |
| 3 | Schedule Resolver | Log mentah & penugasan shift roster | `HrdAttendanceDaily` terisi | Log ditandai pending bila roster belum terbit |

---

## 2. Alur 2: Evaluasi Toleransi Keterlambatan Grace Period & Penanganan Lupa Tap (`HRD-DEC-105`, `HRD-DEC-106`)

Diagram ini memodelkan penentuan status terlambat berdasarkan toleransi 10 menit dan penanganan catatan absensi 1 tap.

```mermaid
flowchart TD
    subgraph Evaluasi Log Harian
        D1[Log Presensi Masuk Diproses] --> D2{Apakah Jumlah Tap Lengkap?}
        D2 -- Tidak 1 Tap Saja --> D3[Set Status IncompletePunch]
        D3 --> D4[Kirim Notifikasi Pengingat ke ESS Pegawai & Atasan]
        D2 -- Ya 2 Tap Lengkap --> D5[Hitung Selisih Jam Datang vs Jam Mulai Shift]
        D5 --> D6{Selisih Jam Datang <= 10 Menit?}
        D6 -- Ya Di Bawah Toleransi --> D7[Status Present: 0 Menit Terlambat]
        D6 -- Tidak Lewat Toleransi --> D8[Status Late: Hitung Penuh Menit Terlambat dari Jam Shift]
    end

    subgraph Cut-Off Periode
        D4 --> E1{Apakah Dikoreksi Sebelum Tutup Periode?}
        E1 -- Ya --> E2[Koreksi Disetujui: Status Menjadi Present]
        E1 -- Tidak --> E3[Periode Ditutup: Konversi Otomatis Menjadi Absent Mangkir]
    end
```

### Tabel Langkah Alur 2

| Langkah | Aktor Pelaku | Masukan | Keluaran | Tindakan Bila Gagal |
| :---: | --- | --- | --- | --- |
| 1 | Calculation Engine | Waktu tap masuk & jadwal shift | Evaluasi keterlambatan | Gunakan toleransi default 10 menit bila konfigurasi kosong |
| 2 | Calculation Engine | Jumlah tap pada hari tersebut | Status `Present`/`Late`/`IncompletePunch` | Memicu notifikasi pengingat ke ESS staf |
| 3 | Guard Penutupan | Sisa incomplete punch saat cut-off | Status terkonversi menjadi `Absent` | Staf wajib mengajukan sanggahan via HR Manager |

---

## 3. Alur 3: Koreksi Kehadiran Mandiri vs On-Behalf & Izin Pulang Cepat (`HRD-DEC-028`, `HRD-DEC-107`)

Diagram ini menggambarkan pengajuan koreksi presensi dan permohonan izin pulang cepat administratif.

```mermaid
flowchart TD
    subgraph Pengajuan
        F1[Pegawai Lupa Tap / Butuh Pulang Darurat] --> F2{Jenis Permohonan?}
        F2 -- Lupa Tap Presensi --> F3[Formulir Koreksi Presensi ESS]
        F2 -- Pulang Lebih Cepat --> F4[Formulir Izin Pulang Cepat Non-Deductible]
        F2 -- Terkendala Akses ESS --> F5[Petugas HR Buat Koreksi On-Behalf]
    end

    subgraph Persetujuan Atasan
        F3 --> G1[Antrean Persetujuan Kepala Ruangan]
        F4 --> G1
        F5 --> G1
        G1 --> G2{Kepala Ruangan Menyetujui?}
        G2 -- Ditolak --> G3[Status Rejected: Berikan Alasan Tertulis]
        G2 -- Disetujui --> G4[Status Approved]
    end

    subgraph Pembaruan Kehadiran
        G4 --> H1{Tipe Transaksi?}
        H1 -- Koreksi Presensi --> H2[HR Terapkan Koreksi: Jam Kerja Sah Dihitung Ulang]
        H1 -- Izin Pulang Cepat --> H3[Pengecualian Ditandai Waived: Saldo Cuti Tetap Utuh]
    end
```

### Tabel Langkah Alur 3

| Langkah | Aktor Pelaku | Masukan | Keluaran | Tindakan Bila Gagal |
| :---: | --- | --- | --- | --- |
| 1 | Pegawai / HR Admin | Formulir koreksi / izin pulang | Permohonan berstatus `Submitted` | Lengkapi alasan dan berkas lampiran |
| 2 | Kepala Ruangan | Tinjauan alasan darurat | Permohonan disetujui / ditolak | Diskusikan secara klinis bila ada keraguan jadwal |
| 3 | Attendance Engine | Status approval atasan | Jam kerja dihitung ulang / waived | Hubungi HR Officer bila hitung ulang gagal |

---

## 4. Alur 4: Penutupan Periode, Guard Koreksi & Buka Kembali Darurat (`HRD-DEC-108`)

Diagram ini memodelkan proses tutup buku absensi bulanan, penolakan bila ada transaksi pending, serta pembatalan serah terima payroll saat periode dibuka kembali.

```mermaid
flowchart TD
    subgraph Penutupan Periode Bulanan
        J1[HR Officer Tekan Tutup Periode Presensi] --> J2{Masih Ada Koreksi Pending?}
        J2 -- Ya Ditemukan Pending --> J3[Penutupan Ditolak HTTP 409: Tampilkan Daftar Pending]
        J2 -- Tidak Bersih --> J4[Konversi Sisa IncompletePunch Jadi Absent]
        J4 --> J5[Status Periode Menjadi Closed: Data Terkunci Read-Only]
        J5 --> J6[Eksekusi Serah Terima ke Payroll Executed]
    end

    subgraph Pembukaan Kembali Darurat
        K1[Ditemukan Kesalahan Darurat Pasca-Tutup] --> K2[HR Manager Tekan Reopen Periode]
        K2 --> K3[Input Alasan Audit Tertulis Wajib]
        K3 --> K4[Status Periode Menjadi Reopened: Kunci Dibuka]
        K4 --> K5{Serah Terima Payroll Sudah Executed?}
        K5 -- Ya --> K6[Eksekusi AttendancePayrollHandoff.Rollback Otomatis]
        K5 -- Tidak --> K7[Selesai Siap Koreksi]
        K6 --> K7
    end
```

### Tabel Langkah Alur 4

| Langkah | Aktor Pelaku | Masukan | Keluaran | Tindakan Bila Gagal |
| :---: | --- | --- | --- | --- |
| 1 | HR Attendance Officer | Tombol Close Period | Periode terkunci `Closed` | Selesaikan seluruh koreksi pending yang terdaftar |
| 2 | Guard Penutupan | Query koreksi pending | Lolos atau Ditolak HTTP 409 | Tampilkan pesan peringatan kepada operator |
| 3 | HR Manager | Tombol Reopen & Alasan audit | Periode `Reopened` & Rollback Payroll | Batalkan aksi bila alasan audit tidak memadai |
