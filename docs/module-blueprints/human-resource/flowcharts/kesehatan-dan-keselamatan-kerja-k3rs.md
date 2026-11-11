# Alur Proses Bisnis: Kesehatan & Keselamatan Kerja Rumah Sakit (K3RS & MCU)

Dokumen ini mendefinisikan alur proses bisnis ujung-ke-ujung untuk **Sub-Modul 19: Occupational Health & Staff Safety (K3RS & Medical Check-Up) — Slice `S-C6`**, mencakup 4 pilar utama operasional perlindungan nakes di rumah sakit.

---

## 1. Alur Utama Medical Check-Up (MCU) Berkala, Kelaikan Kerja & Pembatasan Tugas Medis

```mermaid
flowchart TD
    A([Jadwal MCU Tahunan / Panggilan Staf]) --> B[Staf Registrasi di Poliklinik K3RS]
    B --> C[Pengambilan Sampel Darah/Urin & Rontgen Toraks]
    C --> D[Hasil Lab & Radiologi Terunggah Terenkripsi AES-256]
    D --> E[Dokter K3RS Bersertifikat Hiperkes Melakukan Telaah Klinis]
    E --> F{Keputusan Kelaikan Kerja<br/>Fitness to Work}

    F -->|Sehat Penuh| G[Status 'Fit'<br/>Sertifikat Kelaikan Diterbitkan]
    F -->|Perlu Pembatasan Tugas| H[Status 'Fit with Restriction'<br/>Terbitkan Surat Pembatasan Tugas]
    F -->|Penyakit Menular / Cedera Akut| I[Status 'Temporarily Unfit'<br/>Surat Istirahat Sakit / Rujukan Terapi]
    F -->|Cacat Permanen / Tidak Mampu| J[Status 'Permanently Unfit'<br/>Eskalasi ke Komite Medis & Sub-18 PHK]

    G --> K([Klaim Kelaikan Selesai - Akses Kerja Normal])

    H --> L[Sinyal INT-HR-89 Dikirim ke Sub-06 Roster]
    L --> M[Hard-Stop Aktif: Kepala Ruangan Dilarang Jadwalkan Shift Terlarang]
    M --> N[Staf Bertugas Sesuai Batasan Medis Maksimal 6 Bulan]
    N --> O[Asesmen Kembali Bekerja / Return-to-Work RTW]
    O --> P{Hasil Evaluasi RTW}
    P -->|Pulih Sempurna| Q[Cabut Pembatasan Tugas<br/>Gembok Roster Sub-06 Dilepas]
    P -->|Belum Pulih| R[Perpanjang Restriksi / Evaluasi Dokter Spesialis]
    Q --> K
    R --> M

    I --> S[Staf Menjalani Perawatan Medis]
    S --> O
    J --> T([Proses Pensiun Dini Medis / Alih Tugas])
```

---

## 2. Alur Tanggap Darurat Tertusuk Jarum & Pajanan Darah Pasien (Needle-Stick Emergency)

```mermaid
flowchart TD
    A1([Insiden Tertusuk Jarum / Pajanan Darah Bekas Pasien]) --> B1[Staf Menekan Tombol Darurat Cito di Portal ESS Mobile]
    B1 --> C1[Sistem Menyiarkan Sirene Notifikasi Darurat < 5 Menit ke Tim K3RS & PPI]
    C1 --> D1[Staf Menuju IGD / Poliklinik K3RS untuk Triase Medis Cito < 1 Jam]
    D1 --> E1[Dokter Triase Memeriksa Luka & Menilai Risiko Pajanan Pasien Sumber]
    
    E1 --> F1{Tingkat Risiko Pajanan}
    F1 -->|Risiko Tinggi - Pasien HIV / HBV / HCV| G1[Fast-Track Resep Cito Profilaksis Pasca Pajanan PPP]
    F1 -->|Risiko Rendah - Jarum Steril / Bersih| H1[Pembersihan Luka & Edukasi Higiene]

    G1 --> I1[Sinyal INT-HR-91 ke Depo Farmasi Gawat Darurat Sub-12]
    I1 --> J1[Obat Starter Pack ARV / Vaksin & Ig Hepatitis B Diminumkan < 4 Jam]
    
    J1 --> K1[Pengambilan Darah Baseline Staf & Pasien Sumber]
    H1 --> K1

    K1 --> L1[Worker OHS-W1 Membentuk Protokol Surveilans Serial 1, 3, 6 Bulan]
    L1 --> M1[Evaluasi Darah Bulan ke-1]
    M1 --> N1[Evaluasi Darah Bulan ke-3]
    N1 --> O1[Evaluasi Darah Bulan ke-6]
    
    O1 --> P1{Hasil Serologi Bulan ke-6}
    P1 -->|Non-Reaktif / Negatif| Q1[Kasus Selesai: ClosedSeronegative<br/>Staf Dinyatakan Bebas Infeksi]
    P1 -->|Reaktif / Serokonversi Positif| R1[Eskalasi ke Pengobatan Penyakit Akibat Kerja PAK<br/>Perlindungan Medis Penuh Sub-12]
    Q1 --> S1([Penutupan Tiket Darurat])
    R1 --> S1
```

---

## 3. Alur Radar Imunisasi Nakes & Peringatan Dini Booster Otomatis

```mermaid
flowchart TD
    A2([Registrasi Vaksinasi Pegawai di K3RS]) --> B2[Catat Nomor Batch, Pabrikan & Expired Date Vaksin]
    B2 --> C2[Pemeriksaan Kadar Titer Antibodi Anti-HBs kuantitatif]
    C2 --> D2{Nilai Titer Anti-HBs}
    D2 -->|>= 10 mIU/mL| E2[Status 'Protective'<br/>Masa Proteksi Aman Tercatat di Radar STARKES]
    D2 -->|< 10 mIU/mL| F2[Status 'NonProtective'<br/>Sistem Terbitkan Tiket Rujukan Booster Otomatis]

    E2 --> G2[Worker OHS-W3 Memantau Masa Berlaku Proteksi]
    G2 --> H2{Simulasi H-30 Sebelum Expired}
    H2 -->|Ya| I2[Kirim Notifikasi Reminder Booster ke ESS Staf & Atasan]
    H2 -->|Tidak| G2

    F2 --> J2[Staf Dijadwalkan Vaksinasi Booster di Poliklinik K3RS]
    I2 --> J2
    J2 --> K2[Pemberian Dosis Booster Baru]
    K2 --> A2
```

---

## 4. Alur Pelaporan Kecelakaan Kerja (KAK) & Klaim BPJS Ketenagakerjaan JKK

```mermaid
flowchart TD
    A3([Insiden Kecelakaan Kerja / Fisik di Rumah Sakit]) --> B3[Laporan Insiden Dibuat Staf / Supervisor di Portal K3RS]
    B3 --> C3[Tim K3RS Melakukan Olah TKP & Investigasi Bersama Minimal 2 Personel]
    C3 --> D3[Penyusunan Analisis Akar Masalah RCA & Rekomendasi CAPA]
    
    D3 --> E3[Sistem Mengenerate Formulir Elektronik BPJS JKK Tahap I]
    E3 --> F3[Ekspor / Kirim via API BPJS Ketenagakerjaan dalam Batas SLA < 2x24 Jam]
    
    F3 --> G3[Staf Menjalani Tindakan Perawatan Medis & Pemulihan]
    G3 --> H3[Pencatatan Hari Kerja Hilang / LTI Sinkron ke Presensi Sub-07]
    
    H3 --> I3[Staf Dinyatakan Sembuh oleh Dokter Penanggung Jawab Pasien]
    I3 --> J3[Penyusunan Berkas Klaim JKK Tahap II: Resume Medis & Kwitansi Biaya RS]
    J3 --> K3[Pengajuan Dokumen Tahap II ke BPJS Ketenagakerjaan]
    K3 --> L3[Klaim JKK Cair & Santunan Biaya Diterima]
    L3 --> M3([Penutupan Berkas Kasus Kecelakaan Kerja])
```
