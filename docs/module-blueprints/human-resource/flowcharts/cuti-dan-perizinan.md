# Flowchart Alur Proses Bisnis — Cuti dan Perizinan Staf Rumah Sakit (Slice `S-B2`)

Dokumen ini memodelkan diagram alur proses bisnis (*business process workflow*) untuk **Sub-modul 08. Leave & Permission Management (Slice `S-B2`)**, menyerap keputusan interview `HRD-DEC-110` s.d. `HRD-DEC-115` serta asumsi `HRD-ASM-19` s.d. `HRD-ASM-22`.

---

## 1. Siklus Saldo Cuti Tahunan, Carry-Forward Max 6 Hari & Kedaluwarsa 30 Juni

Diagram ini memvisualisasikan bagaimana hak cuti tahunan diberikan (12 hari kerja), pembatasan pemindahan sisa saldo akhir tahun maksimal 6 hari kerja, dan eksekusi hangus saldo bawaan secara otomatis tepat pada 30 Juni (`HRD-DEC-110`).

```mermaid
flowchart TD
    subgraph HRD_SISTEM ["Sistem & Scheduler HRD"]
        A1["1 Januari: Alokasi Hak Cuti Baru (+12 Hari)"] --> A2["Pembukuan Transaksi Entitlement di Ledger"]
        A2 --> A3["Saldo Berjalan Aktif Digunakan Pegawai"]
        A3 --> A4{"31 Desember:<br/>Evaluasi Sisa Saldo Akhir Tahun"}
        A4 -->|"Sisa Saldo > 6 Hari"| A5["Pindahkan 6 Hari ke Carry-Forward<br/>Sisa di atas 6 Hari Hangus (Forfeited)"]
        A4 -->|"Sisa Saldo <= 6 Hari"| A6["Pindahkan Seluruh Sisa ke Carry-Forward"]
        A5 --> A7["Saldo Bawaan Berlaku s.d. 30 Juni"]
        A6 --> A7
        A7 --> A8{"30 Juni 23:59:59 WIB:<br/>Apakah Sisa Bawaan Masih Ada?"}
        A8 -->|"Ya, Masih Ada Sisa"| A9["Eksekusi Hangus Otomatis (Mid-Year Forfeiture)"]
        A8 -->|"Tidak / Sudah Habis"| A10["Saldo Bawaan Bersih (Nol)"]
        A9 --> A11["Saldo Pegawai Murni Sisa Hak Tahun Berjalan"]
        A10 --> A11
    end
```

### Tabel Rincian Langkah Tata Kelola Saldo Cuti

| No | Tahap / Peristiwa | Pelaku | Masukan | Keluaran | Penanganan Pengecualian / Gagal |
| :---: | --- | --- | --- | --- | --- |
| 1 | Hak Baru Awal Tahun | Scheduler | Data pegawai aktif | +12 hari kerja tercatat di `TrxLeaveBalanceLedger` | Jika pegawai baru masuk tengah tahun, hak dihitung prorata bulan berjalan |
| 2 | Cut-Off Akhir Tahun (31 Des) | Scheduler | Sisa saldo 31 Des | Transaksi `CarryForward` (max 6 hari) & `Forfeiture` (>6 hari) | Notifikasi rincian saldo dikirimkan ke portal ESS pegawai |
| 3 | Eksekusi Tengah Tahun (30 Jun) | Scheduler | Sisa kuota carry-forward | Transaksi `CarryForwardForfeiture` pemotongan sisa kuota bawaan | Pengingat dini dikirim H-30 dan H-7 sebelum kedaluwarsa 30 Juni |

---

## 2. Pengajuan Cuti ESS, Validasi Kuota Layanan (*Safe Staffing Coverage Warning*), dan Keputusan Atasan

Diagram ini memvisualisasikan pengajuan cuti mandiri pegawai, validasi saldo, evaluasi ketersediaan staf shift unit 24/7 di modul penjadwalan 06, serta opsi atasan menyetujui dengan mitigasi perawat pengganti (`HRD-DEC-111`).

```mermaid
flowchart TD
    subgraph PEGAWAI ["Pegawai (ESS)"]
        B1["Pilih Jenis Cuti & Rentang Tanggal"] --> B2{"Opsi Cuti Setengah Hari?"}
        B2 -->|"Ya"| B3["Pilih Sesi Pagi / Siang (0.5 Hari)"]
        B2 -->|"Tidak"| B4["Hitung Hari Kerja Efektif"]
        B3 --> B5["Cek Kecukupan Saldo di Ledger"]
        B4 --> B5
        B5 -->|"Saldo Kurang"| B6["Tolak Pengajuan (Saldo Tidak Cukup)"]
        B5 -->|"Saldo Cukup"| B7["Submit Permohonan Cuti"]
    end

    subgraph ENGINE_VALIDASI ["Engine Validasi Ketersediaan Staf"]
        B7 --> B8["Evaluasi Jadwal Roster Unit 24/7 (S-B4)"]
        B8 --> B9{"Jumlah Staf Dinas<br/>Di Bawah Kuota Aman?"}
        B9 -->|"Ya (Kritis)"| B10["Tandai Safe Staffing Coverage Warning"]
        B9 -->|"Tidak (Aman)"| B11["Tandai Kuota Dinas Aman"]
    end

    subgraph ATASAN ["Kepala Ruangan / Atasan Langsung"]
        B10 --> B12["Inbox Approval: Tampil Banner Warning Kuota"]
        B11 --> B13["Inbox Approval: Tampil Kuota Aman"]
        B12 --> B14{"Keputusan Atasan"}
        B13 --> B14
        B14 -->|"Setujui Biasa"| B15["Status: Approved"]
        B14 -->|"Setujui dg Mitigasi"| B16["Input Nama Perawat Pengganti -> Approved with Mitigation"]
        B14 -->|"Minta Revisi"| B17["Kembalikan ke Pegawai (NeedRevision)"]
        B14 -->|"Tolak"| B18["Status: Rejected (Alasan Tertulis)"]
    end

    subgraph INTEGRASI ["Integrasi Jadwal & Presensi"]
        B15 --> B19["Kunci Pengurangan Saldo di Ledger"]
        B16 --> B19
        B19 --> B20["Update Status Shift Roster 06 & Presensi 07 (OnLeave)"]
    end
```

### Tabel Rincian Langkah Pengajuan Cuti & Evaluasi Kuota

| No | Langkah | Pelaku | Masukan | Keluaran | Jalur Alternatif / Mitigasi |
| :---: | --- | --- | --- | --- | --- |
| 1 | Input Pengajuan | Pegawai | Tanggal, jenis cuti, kontak darurat | Estimasi durasi hari kerja & pemotongan saldo | Pegawai dapat membatalkan form sebelum submit |
| 2 | Evaluasi Kuota Shift | Sistem | Roster terbit unit target | Rasio staf bertugas vs batas minimal aman | Jika engine jadwal offline, warning fallback manual diaktifkan |
| 3 | Tinjauan & Mitigasi | Kepala Unit | Form cuti + Banner Warning | Keputusan (Approve, Mitigasi, Revisi, Tolak) | Wajib mencatat nama staf pengganti bila menyetujui permohonan kritis |
| 4 | Pembaruan Kalender | Sistem | ID permohonan approved | Rekaman absensi `OnLeave` & debit ledger | Menghindari pencatatan mangkir pada modul presensi harian |

---

## 3. Alur Cuti Sakit Mandiri vs Wajib Surat Dokter Ber-SIP (Privasi Medis Staf K3RS)

Diagram ini memvisualisasikan pembedaan cuti sakit 1 hari (surat pernyataan mandiri) vs $\ge 2$ hari (wajib surat dokter ber-SIP), serta pemisahan hak akses antara atasan langsung dan dokter K3RS (`HRD-DEC-112`).

```mermaid
flowchart TD
    subgraph PEGAWAI_SAKIT ["Pegawai Sakit (ESS)"]
        C1["Pilih Pengajuan Cuti Sakit"] --> C2{"Berapa Hari Durasi Sakit?"}
        C2 -->|"1 Hari Kerja"| C3["Unggah Surat Pernyataan Mandiri"]
        C2 -->|">= 2 Hari Kerja"| C4["Wajib Unggah Surat Keterangan Sakit Dokter Ber-SIP"]
        C3 --> C5["Submit Permohonan Sakit"]
        C4 --> C5
    end

    subgraph PROTEKSI_DATA ["Lapisan Keamanan & Privasi Data"]
        C5 --> C6["Simpan Berkas di Storage Terenkripsi"]
        C6 --> C7["Masking Diagnosa Penyakit (Sembunyikan ICD-10)"]
    end

    subgraph PERSETUJUAN_ATASAN ["Atasan Langsung"]
        C7 --> C8["Buka Rincian Pengajuan Bawahan"]
        C8 --> C9["Melihat Durasi Istirahat, Faskes, & Nama Dokter"]
        C9 --> C10["Diagnosa Medis Ditampilkan Gembok Privasi (Masked)"]
        C10 --> C11["Setujui Izin Sakit Staf (Approval Administrasi)"]
    end

    subgraph VERIFIKASI_K3RS ["Tim Medis K3RS / HR Medis"]
        C6 --> C12["Akses Khusus Berkas Medis Asli"]
        C12 --> C13["Verifikasi Nomor SIP & Diagnosis Klinis"]
        C13 --> C14["Pencatatan Rekam Kesehatan Kerja K3RS RS"]
    end
```

### Tabel Rincian Langkah Cuti Sakit & Privasi

| No | Langkah | Pelaku | Masukan | Keluaran | Jalur Penolakan |
| :---: | --- | --- | --- | --- | --- |
| 1 | Pengajuan Sakit | Pegawai | Tanggal sakit & lampiran berkas | Tiket permohonan cuti sakit | Ditolak submit bila $\ge 2$ hari tanpa melampirkan berkas dokter |
| 2 | Enkripsi & Masking | Sistem | Berkas & metadata surat | Payload atasan termaskir tanpa diagnosa ICD-10 | Menjamin kepatuhan UU PDP dan privasi staf RS |
| 3 | Persetujuan Operasional | Atasan | Tiket sakit (durasi & faskes) | Persetujuan izin tidak dinas bawahan | Atasan dapat meminta konfirmasi ke HRD bila meragukan |
| 4 | Verifikasi K3RS | Dokter K3RS | Dokumen medis lengkap | Rekam kesehatan staf & verifikasi valid | Berkas palsu/tidak sah dapat ditolak oleh K3RS |

---

## 4. Alur Pemanggilan Kembali Pegawai Cuti Darurat (*Leave Recall*) & Pemulihan Saldo

Diagram ini memvisualisasikan prosedur resmi saat rumah sakit menghadapi situasi darurat (KLB, bencana alam, lonjakan pasien kritis ICU) sehingga pegawai yang sedang cuti dipanggil dinas kembali dengan persetujuan HR Manager dan pemulihan saldo otomatis (`HRD-DEC-113`).

```mermaid
flowchart TD
    subgraph SITUASI_DARURAT ["Situasi Darurat Rumah Sakit"]
        D1["Terjadi KLB / Bencana / Lonjakan Pasien Kritis"] --> D2["Kepala Ruangan Cek Staf yang Sedang Cuti Aktif"]
    end

    subgraph INISIASI_KEPALA_UNIT ["Kepala Ruangan / Unit"]
        D2 --> D3["Buka Layar Monitoring Cuti Aktif"]
        D3 --> D4["Inisiasi Permohonan Leave Recall"]
        D4 --> D5["Tentukan Tanggal Efektif Kembali Dinas"]
        D5 --> D6["Sistem Hitung Otomatis Sisa Hari Cuti Batal Dinikmati"]
        D6 --> D7["Kirim Usulan Recall ke HR Manager"]
    end

    subgraph OTORISASI_HRD ["HR Manager (Single Approval Gate)"]
        D7 --> D8["Tinjau Justifikasi & Urgensi Pemanggilan"]
        D8 --> D9{"Keputusan HR Manager"}
        D9 -->|"Tolak"| D10["Recall Ditolak (Pegawai Tetap Lanjut Cuti)"]
        D9 -->|"Setujui"| D11["Terbitkan SK Pemanggilan Dinas Darurat"]
    end

    subgraph EKSEKUSI_PEMULIHAN ["Eksekusi Pemulihan Sistem Otomatis"]
        D11 --> D12["Posting Kredit Saldo (RecallRestore) ke Ledger Staf"]
        D12 --> D13["Pulihkan Status Shift Pegawai di Roster 06 Menjadi Scheduled"]
        D13 --> D14["Kirim Notifikasi Tugas Darurat ke ESS & WA Pegawai"]
        D14 --> D15["Pegawai Hadir Bertugas Tanpa Penalti Presensi"]
    end
```

### Tabel Rincian Langkah Pemanggilan Darurat (Leave Recall)

| No | Langkah | Pelaku | Masukan | Keluaran | Jalur Pembatalan / Pengecualian |
| :---: | --- | --- | --- | --- | --- |
| 1 | Deteksi Kebutuhan | Kepala Unit | Kondisi darurat unit (BOR > 100%, KLB) | Daftar nama staf cuti yang kompeten | Memprioritaskan staf terdekat yang dapat dihubungi |
| 2 | Inisiasi Recall | Kepala Unit | ID cuti, tanggal efektif recall, alasan | Tiket recall berstatus `RequestedByHeadUnit` | Ditolak jika cuti staf sudah berakhir |
| 3 | Verifikasi HR Manager | HR Manager | Justifikasi operasional darurat | SK resmi atau penolakan tiket recall | HR Manager dapat menolak jika ada alternatif perawat cadangan |
| 4 | Pemulihan Saldo & Shift | Sistem | Tiket recall approved | Mutasi `RecallRestore` di ledger & shift aktif di Roster 06 | Pegawai menerima hak sisa cutinya kembali 100% utuh |
