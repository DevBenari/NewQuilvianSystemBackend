# Alur Proses Bisnis: Kotak Masuk Persetujuan Terpadu & Layanan Mandiri Karyawan (ESS & Unified Approval V2)

Dokumen ini mendefinisikan alur proses bisnis ujung-ke-ujung untuk **Sub-Modul 21: Employee Self-Service (ESS) & Unified Approval Inbox (V2 Hardening) — Slice `S-A7-V2`**.

---

## 1. Alur Utama Pengajuan Permohonan ESS, Penugasan Tiket & Eksekusi Hilir

```mermaid
flowchart TD
    A([Pegawai Membuka Portal ESS]) --> B[Pilih Jenis Permohonan: Cuti/Lembur/Shift/Klaim]
    B --> C[Pengisian Formulir Mandiri & Unggah Lampiran]
    C --> D[Kirim Permohonan Resmi - Status: Submitted]
    
    D --> E[Workflow Engine: Evaluasi Matriks Alur Persetujuan]
    E --> F{Apakah Pemohon Merupakan Approver Definitif?}
    
    F -->|Ya - Self Approval Terdeteksi| G[Anti Self-Approval HRD-DEC-183: Skip-Level ke Atasan di Atasnya]
    F -->|Tidak - Normal| H[Tetapkan Approver Definitif Sesuai Pohon Jabatan Sub-02]
    
    G --> I{Apakah Approver Aktif / Berhalangan?}
    H --> I
    
    I -->|Sedang Cuti & Punya Delegasi Plh| J[Arahkan Tiket ke Inbox Pelaksana Harian Plh]
    I -->|Hadir Bertugas Normal| K[Arahkan Tiket ke Inbox Pejabat Definitif]
    
    J --> L[Tiket Berstatus UnderReview di Unified Approval Inbox]
    K --> L
    
    L --> M{Keputusan Approver}
    M -->|Setuju - Approve| N[Verifikasi PIN / Digital Signature]
    M -->|Minta Perbaikan - Return| O[Status: ReturnedForRevision - Kirim Catatan ke Pemohon]
    M -->|Tolak - Reject| P[Wajib Isi Alasan Penolakan Minimal 15 Karakter]
    
    N --> Q{Apakah Masih Ada Tahapan Lanjutan Step Berikutnya?}
    Q -->|Ya| E
    Q -->|Tidak - Tahapan Terakhir Selesai| R[Status: Approved Final]
    
    R --> S[Sinyal INT-HR-93 ke Modul Hilir: Potong Cuti / Ubah Shift / Rekam Lembur]
    S --> T([Notifikasi Sukses ke Pemohon & Transaksi Selesai])
    
    O --> U[Staf Mengedit Draf di ESS & Resubmit dalam Maksimal 7 Hari]
    U --> L
    
    P --> V[Status: Rejected - Tiket Ditutup Permanen]
    V --> W([Notifikasi Penolakan ke Pemohon])
```

---

## 2. Alur Pendelegasian Wewenang Digital Berbatas Waktu (Plh / Acting Approver)

```mermaid
flowchart TD
    A1([Pejabat Struktural Hendak Mengambil Cuti / Dinas Luar]) --> B1[Buka Menu Pendelegasian Wewenang /hrd/approvals/delegation]
    B1 --> C1[Pilih Pejabat Pengganti Setara atau 1 Tingkat di Bawahnya]
    C1 --> D1[Tentukan Tanggal Mulai dan Tanggal Selesai Delegasi Wewenang]
    D1 --> E1[Pilih Kategori Permohonan yang Dialihkan: Seluruhnya atau Parsial]
    E1 --> F1[Simpan Surat Kuasa Digital Delegasi - Status: Scheduled / Active]
    
    F1 --> G1[Sistem Memantau Rentang Waktu Delegasi Harian]
    G1 --> H1{Tanggal Hari Ini Berada dalam Rentang Delegasi?}
    H1 -->|Ya| I1[Tiket Permohonan Staf Masuk ke Inbox Plh dengan Badge 'Delegated']
    H1 -->|Tidak / Tanggal Berakhir Terlewati| J1[Worker WF-W2 Mencabut Wewenang Plh Otomatis]
    
    I1 --> K1[Plh Mengeksekusi Keputusan Persetujuan]
    K1 --> L1[Audit Trail Mencatat Identitas Ganda: Eksekutor Plh Atas Nama Pejabat Definitif]
    L1 --> M1([Jejak Otorisasi Akuntabel Sesuai Standar Akreditasi])
    J1 --> N1([Wewenang Kembali Penuh ke Pejabat Definitif])
```

---

## 3. Alur Penegakan SLA, Peringatan Dini H-4 Jam & Auto-Eskalasi Tanpa Auto-Approve

```mermaid
flowchart TD
    A2([Tiket Permohonan Masuk ke Inbox Approver]) --> B2[Sistem Mengaktifkan Timer Hitung Mundur SLA]
    B2 --> C2[Worker WF-W4 Memindai Tiket yang Berjalan Mendekati Batas SLA]
    
    C2 --> D2{Sisa Waktu SLA <= 4 Jam?}
    D2 -->|Ya| E2[Kirim Push Notification Peringatan Merah Darurat ke Ponsel Approver]
    D2 -->|Tidak| C2
    
    E2 --> F2{Apakah Approver Merespon Sebelum SLA Habis?}
    F2 -->|Ya - Direspon| G2([Tiket Diproses Normal])
    F2 -->|Tidak - Waktu SLA Terlewati| H2[Worker WF-W1 Menandai IsOverdue = true]
    
    H2 --> I2{Kebijakan HRD-DEC-185: Larangan Auto-Approve}
    I2 -->|Hard-Stop| J2[Sistem Dilarang Menyetujui Otomatis Demi Keselamatan Pasien]
    J2 --> K2[Tiket Otomatis Dieskalasi ke Atasan Tingkat di Atasnya]
    K2 --> L2[Atasan Tingkat Atas Menerima Tiket Macet untuk Diambil Alih]
    L2 --> M2([Penyelesaian Tiket oleh Atasan Atas])
```

---

## 4. Alur Persetujuan Massal Bersyarat (Conditional Bulk Approval)

```mermaid
flowchart TD
    A3([Atasan Membuka Dasbor Kotak Masuk Persetujuan Terpadu]) --> B3[Pilih Tab Permohonan Rutin: Lembur / Cuti Tahunan]
    B3 --> C3[Atasan Menekan Tombol Centang Pilih Banyak]
    C3 --> D3[Engine Sistem Memindai Seluruh Tiket yang Dipilih]
    
    D3 --> E4{Apakah Ada Tiket dengan Tanda Peringatan / Bentrok Roster / Tukar Shift Kritis?}
    E4 -->|Ada Tiket Berisiko| F4[Sistem Otomatis Menolak Item Berisiko dari Batch]
    E4 -->|Seluruh Indikator Hijau Bersih| G4[Buka Modal Konfirmasi Persetujuan Massal]
    
    F4 --> H4[Tampilkan Notifikasi: Item Tertentu Wajib Ditinjau Lembar Detail Manual]
    H4 --> G4
    
    G4 --> I4[Tampilkan Ringkasan: Total Tiket Bersih yang Siap Disetujui]
    I4 --> J4[Approver Memasukkan 1 Kali PIN Otorisasi Keamanan]
    J4 --> K4[Worker WF-W5 Memproses Batch Transaksi Secara Berurutan]
    K4 --> L4[Seluruh Tiket Terpilih Berubah Status Approved & Memicu Domain Hilir]
    L4 --> M4([Laporan Ringkasan Eksekusi Sukses Ditampilkan])
```
