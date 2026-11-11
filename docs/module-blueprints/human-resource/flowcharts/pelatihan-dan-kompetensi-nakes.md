# Alur Proses Bisnis Sub-Modul 16: Learning, Training & Competency (`S-C2`)

Dokumen ini memodelkan diagram alur proses bisnis (*business flowcharts*) berbasis standar akreditasi KARS / STARKES 2024 (KPS 8 & KPS 8.1), Permenkes 13/2025, dan UU Kesehatan 17/2023 untuk **Sub-Modul 16: Learning, Training & Competency (Slice `S-C2`)**.

---

## 16.1 Siklus Pelatihan Wajib Akreditasi RS (2 Tahunan) & Radar Kedaluwarsa

Diagram ini memodelkan tata laksana 5 materi pelatihan wajib (BHD/BLS, PPI, Patient Safety/SKP, K3RS & Code Red, Komunikasi Efektif SBAR) untuk 100% pegawai rumah sakit dengan masa berlaku 2 tahun serta eskalasi status kerja saat kedaluwarsa.

```mermaid
flowchart TD
    subgraph Staf_Pegawai["Pegawai Rumah Sakit (Medis / Non-Medis)"]
        A1["Pegawai Menerima Notifikasi Radar (H-90 / H-60 / H-30)"] --> A2["Memilih Jadwal Sesi Refresher Diklat di ESS"]
        A2 --> A3["Mengikuti Sesi Kelas & Simulasi Praktikum"]
        A7["Menerima Peringatan Status Overdue (H-0)"] --> A8["Bertugas di Bawah Pendampingan Senior (Buddy System)"]
    end

    subgraph Unit_Diklat["Bagian Pendidikan & Pelatihan (Diklat RS)"]
        B1["Scheduler Latar Belakang Memindai Masa Berlaku 2 Tahun"] --> B2{"Sisa Masa Berlaku?"}
        B2 -- "H-90 s.d. H-30" --> A1
        B2 -- "H-0 (Kedaluwarsa)" --> B3["Tandai Profil Pegawai: Overdue Mandatory Training"]
        B3 --> A7
        B3 --> B4["Kirim Sinyal Peringatan ke Sub-06 Scheduling & Roster"]
        B3 --> B5["Eskalasi ke Kepala Ruangan & Subkomite Kredensial"]
        A3 --> B6["Trainer Merekam Presensi Kehadiran & Skor Post-Test"]
        B6 --> B7{"Lulus Ujian Teori & OSCE?"}
        B7 -- "Ya (Skor >= 75/80 & Lulus Praktik)" --> B8["Terbitkan Sertifikat Resmi (QR Code & Validitas 2 Tahun)"]
        B7 -- "Tidak" --> B9["Jadwalkan Sesi Remedial dalam 14 Hari"]
        B8 --> B10["Auto-Arsip Sertifikat ke e-Dossier Sub-02 & Reset Timer 2 Tahun"]
    end

    subgraph Unit_Pelayanan["Unit Pelayanan / Ruangan Medis"]
        B4 --> C1["Kepala Ruangan Memvalidasi Jadwal Roster Shift"]
        C1 --> C2["Terapkan Buddy System: Pasangkan dengan Nakes Bersertifikat Aktif"]
        C2 --> A8
    end
```

### Tabel Langkah Proses 16.1

| No | Aktivitas / Langkah | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Jalur Gagal |
| :-: | :--- | :--- | :--- | :--- | :--- |
| 1 | Pemindaian Radar Kedaluwarsa H-90 s.d. H-30 | Background Worker | Tanggal berlaku sertifikat pelatihan wajib | Notifikasi reservasi sesi refresher diklat di portal ESS | Jika email gagal kirim, notifikasi dikirim via WhatsApp Gateway dan peringatan login ESS. |
| 2 | Pendaftaran Sesi Penyegaran (Refresher) | Pegawai RS | Pilihan jadwal sesi kelas Diklat internal | Konfirmasi pendaftaran peserta (*Enrollment Confirmed*) | Jika kuota kelas penuh, sistem otomatis merekomendasikan sesi kelas gelombang berikutnya. |
| 3 | Pengamanan Operasional H-0 Kedaluwarsa | Sistem & Diklat | Status sertifikat mencapai batas H-0 | Flag *Overdue Mandatory Training* aktif; alert ke Sub-06 & Kredensial | Nakes dilarang dinas mandiri, wajib didampingi nakes senior (*buddy system*) hingga remedial tuntas. |
| 4 | Pelaksanaan Ujian & Simulasi Praktik | Trainer Diklat | Lembar ujian post-test dan rubrik OSCE | Skor post-test dan penilaian keterampilan klinis | Jika peserta gagal ujian teori/praktik, wajib mengikuti remedial maksimal 2 kali dalam 14 hari kerja. |
| 5 | Penerbitan Sertifikat & Reset Timer | Sistem Diklat | Hasil asesmen lulus (skor >= 75/80) | Sertifikat digital ber-QR Code aktif 2 tahun, terarsip ke Sub-02 | Jika gagal arsip ke e-Dossier, instruksi sinkronisasi disimpan ke outbox retry bus. |

---

## 16.2 Pelaksanaan Sesi Diklat, Presensi & Evaluasi Kompetensi Klinis (OSCE)

Diagram ini memodelkan pemisahan tegas antara pencatatan kehadiran fisik/virtual dengan asesmen kelulusan keterampilan klinis langsung (*Objective Structured Clinical Examination* - OSCE).

```mermaid
flowchart TD
    subgraph Peserta["Peserta Pelatihan (Dokter / Perawat / Nakes)"]
        P1["Hadir di Lokasi Praktikum / Kelas Diklat"] --> P2["Melakukan Tap Presensi QR / Biometrik Sesi"]
        P2 --> P3["Mengerjakan Pre-Test Teori di Portal Diklat"]
        P3 --> P4["Mengikuti Materi Pembelajaran & Demonstrasi Keterampilan"]
        P4 --> P5["Mengerjakan Post-Test Teori (Batas Waktu 60 Menit)"]
        P5 --> P6["Melakukan Ujian Praktik Simulasi Klinis di Station OSCE"]
        P6 --> P7{"Hasil Evaluasi?"}
        P7 -- "Lulus Kompeten" --> P8["Menerima Sertifikat Kompetensi Digital di ESS"]
        P7 -- "Belum Kompeten" --> P9["Menerima Feedback Gap Klinis & Jadwal Remedial"]
    end

    subgraph Instruktur["Instruktur / Assessor Klinis Tersertifikasi"]
        I1["Membuka Sesi Pelatihan & Mengaktifkan Kode QR Presensi"] --> P2
        I2["Memantau Pengerjaan Post-Test Peserta"] --> P5
        I3["Menilai Keterampilan di Station OSCE (Rasio Maks 1:5)"] --> I4["Menginput Lembar Penilaian Checklist Keterampilan Klinis"]
        I4 --> I5{"Evaluasi Syarat Kelulusan Ganda"}
        I5 -- "Presensi >= 80% & Post-Test >= 80 & OSCE Lulus" --> I6["Bubuhi Tanda Tangan Digital Kelulusan Assessor"]
        I5 -- "Salah Satu Syarat Tidak Terpenuhi" --> I7["Terbitkan Hasil Belum Lulus & Rekomendasi Bimbingan"]
        I6 --> P7
        I7 --> P7
    end

    subgraph Sistem_LMS["Engine Pelatihan & Evaluasi Kompetensi Quilvian"]
        P2 --> S1["Validasi Geofencing & Waktu Presensi Sesi"]
        P3 --> S2["Kalkulasi Nilai Pre-Test Otomatis"]
        P5 --> S3["Kalkulasi Skor Post-Test Otomatis"]
        I6 --> S4["Generate Berkas Sertifikat Digital Ber-QR Code"]
        S4 --> S5["Sinkronisasi Status Kompetensi ke Profil Pegawai Sub-02"]
        S4 --> P8
    end
```

### Tabel Langkah Proses 16.2

| No | Aktivitas / Langkah | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Jalur Gagal |
| :-: | :--- | :--- | :--- | :--- | :--- |
| 1 | Presensi Kehadiran Sesi Pelatihan | Peserta & Instruktur | Pemindaian QR Code sesi atau tap RFID biometrik | Catatan presensi valid di `TrxTrainingAttendance` | Jika kendala jaringan, instruktur dapat melakukan verifikasi presensi manual via portal admin. |
| 2 | Evaluasi Teori Post-Test | Peserta & Sistem | Jawaban post-test pilihan ganda / studi kasus | Skor post-test (skala 0-100) di `TrxTrainingAssessment` | Jika skor < 80 (klinis) atau < 75 (non-klinis), sistem membuka bank soal remedial 1 kali. |
| 3 | Ujian Simulasi Klinis OSCE | Assessor Klinis | Observasi simulasi tindakan (cth: RJP, pasang CVC) | Lembar rubrik penilaian checklist OSCE bertanda tangan | Assessor wajib tersertifikasi; rasio penguji terhadap peserta dibatasi maksimal 1:5 peserta. |
| 4 | Validasi Kelulusan Ganda | Sistem Engine Diklat | Data presensi + skor post-test + status kelulusan OSCE | Status kelulusan terverifikasi (`Passed` / `Failed`) | Jika presensi < 80% meskipun tes lulus, sertifikat ditahan hingga peserta menuntaskan jam pengganti. |
| 5 | Penerbitan Sertifikat & Integrasi Profil | Sistem Engine Diklat | Tanda tangan digital assessor & data peserta | Sertifikat digital PDF ber-QR Code publik | Data kompetensi aktif langsung memperbarui direktori keahlian pegawai di Sub-02. |

---

## 16.3 Pengajuan Pelatihan Eksternal, Kontrol Anggaran Diklat & Ikatan Dinas

Diagram ini memodelkan tata laksana permohonan pelatihan/fellowship eksternal berbayar, verifikasi pagu anggaran Diklat, dan penerbitan adendum ikatan dinas retensi kerja di Sub-05.

```mermaid
flowchart TD
    subgraph Pemohon["Pegawai / Tenaga Medis Pemohon"]
        E1["Mengisi Formulir Pengajuan Diklat Eksternal di ESS"] --> E2["Melampirkan Brosur, Rincian Biaya & Jadwal Pelatihan"]
        E2 --> E3["Submit Permohonan Keikutsertaan Pelatihan"]
        E7["Menandatangani Perjanjian Ikatan Dinas Digital (e-Sign)"] --> E8["Berangkat Pelatihan & Melaksanakan Tugas Diklat"]
        E8 --> E9["Unggah Laporan Hasil Pelatihan & Sertifikat Kelulusan"]
    end

    subgraph Atasan_Direksi["Kepala Ruangan, KSM & Direksi"]
        E3 --> A1["Kepala Ruangan/KSM Memvalidasi Pengaturan Jadwal Dinas"]
        A1 -- "Disetujui" --> A2["Manajer Diklat Memvalidasi Relevansi & Kebutuhan TNA"]
        A1 -- "Ditolak (Kekurangan Staf)" --> A3["Permohonan Ditolak dengan Alasan Tertulis"]
        A2 --> A4{"Biaya > Rp 10 Juta atau Durasi > 1 Bulan?"}
        A4 -- "Ya" --> A5["Wajib Persetujuan Direktur SDM & Direktur Utama"]
        A4 -- "Tidak" --> A6["Persetujuan Manajer SDM / Kepala Bagian Diklat"]
        A5 -- "Disetujui" --> D2["Terbitkan Draf Perjanjian Ikatan Dinas"]
    end

    subgraph Pengendalian_Anggaran["Engine Anggaran Diklat & Kontrak Kerja"]
        A2 --> D1{"Pagu Anggaran Unit Tersedia?"}
        D1 -- "Tersedia" --> A4
        D1 -- "Melebihi Kuota Pagu" --> D3["Blokir: Anggaran Diklat Unit Telah Habis"]
        D2 --> E7
        E7 --> D4["Tambahkan Adendum Ikatan Dinas (1-2 Tahun) pada Kontrak Sub-05"]
        E9 --> D5["Verifikasi Sertifikat & Pencairan Biaya Restitusi Sub-11"]
    end
```

### Tabel Langkah Proses 16.3

| No | Aktivitas / Langkah | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Jalur Gagal |
| :-: | :--- | :--- | :--- | :--- | :--- |
| 1 | Pengajuan Pelatihan Eksternal via ESS | Pegawai Pemohon | Formulir permohonan, kurikulum diklat, dan RAB biaya | Berkas permohonan `TrxTrainingEnrollmentRequest` draf | Pengajuan wajib diajukan minimal H-14 sebelum tanggal mulai pelatihan eksternal. |
| 2 | Verifikasi Kesiapan Shift Ruangan | Kepala Ruangan / SMF | Data jadwal rotasi dinas dan ketersediaan nakes pengganti | Persetujuan izin dinas belajar atasan langsung | Jika ruangan kekurangan tenaga kritis, atasan dapat menolak atau meminta penggeseran gelombang. |
| 3 | Validasi Pagu Anggaran Diklat | Bagian Diklat & Keuangan | Estimasi biaya pendaftaran, transportasi, akomodasi | Pengecekan saldo anggaran di `TrxTrainingBudget` | Jika pagu habis, permohonan ditolak kecuali ada memo persetujuan penambahan anggaran dari Direksi. |
| 4 | Penerbitan Perjanjian Ikatan Dinas | Sistem & HR Legal | Biaya > Rp 10 Juta atau durasi meninggalkan dinas > 1 bulan | Surat Perjanjian Ikatan Dinas ber-eSign (retensi 1-2 tahun) | Pegawai wajib menandatangani perjanjian sebelum surat tugas resmi diterbitkan. |
| 5 | Pelaporan Hasil & Reimbursement | Pegawai & Bagian Diklat | Laporan resume pelatihan, kuitansi biaya, dan sertifikat | Verifikasi kelulusan & instruksi transfer pencairan | Jika pegawai tidak lulus atau tidak menyerahkan laporan, klaim biaya dibatalkan. |

---

## 16.4 Pelacakan Akumulasi SKP Plataran Sehat Kemenkes & Prasyarat Kredensialing RKK

Diagram ini memodelkan pencatatan perolehan Satuan Kredit Profesi (SKP) Kemenkes terintegrasi platform Plataran Sehat dan validasi prasyarat kompetensi pada kredensialing Sub-14.

```mermaid
flowchart TD
    subgraph Tenaga_Medis["Dokter / Perawat / Tenaga Kesehatan"]
        M1["Menyelesaikan Kegiatan Pembelajaran / Pelayanan / Pengabdian"] --> M2["Memperoleh Bukti Sertifikat Digital Ber-SKP Kemenkes"]
        M2 --> M3["Mengentri Data SKP di Portal Portofolio ESS"]
        M3 --> M4["Memantau Visual Tracker Akumulasi SKP Menuju H-0 SIP"]
    end

    subgraph Verifikasi_Diklat["Bagian Diklat & Sekretariat Komite Profesi"]
        M3 --> V1["Petugas Diklat Memeriksa Nomor Akreditasi SK Kemenkes"]
        V1 --> V2["Validasi ID Sertifikat Digital ke Portal Plataran Sehat"]
        V2 --> V3{"Bukti Terverifikasi Valid?"}
        V3 -- "Ya" --> V4["Sahkan Poin SKP Resmi (Pembelajaran/Pelayanan/Pengabdian)"]
        V3 -- "Tidak (Data Fiktif/Kedaluwarsa)" --> V5["Tolak Pengajuan SKP dengan Catatan Klarifikasi"]
        V4 --> V6["Perbarui Akumulasi Nilai SKP pada Rekam Jejak Profesi Nakes"]
    end

    subgraph Integrasi_Submodul["Integrasi Kredensialing (Sub-14) & Kinerja OPPE (Sub-15)"]
        V4 --> G1["Gerbang Prasyarat RKK Sub-14 (Prerequisite Gate)"]
        G1 --> G2{"Sertifikat Kompetensi Khusus Aktif?"}
        G2 -- "Ya (Cth: Sertifikat Dialisis / Ventilator Aktif)" --> G3["Izinkan Checklist Kewenangan Klinis Mandiri di RKK"]
        G2 -- "Tidak / Kedaluwarsa" --> G4["Kunci Butir Tindakan RKK (Wajib Supervisi atau Ditolak)"]
        V4 --> G5["Kirimkan Poin Pemenuhan CPD ke Pilar Medical Knowledge OPPE Sub-15"]
    end
```

### Tabel Langkah Proses 16.4

| No | Aktivitas / Langkah | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Jalur Gagal |
| :-: | :--- | :--- | :--- | :--- | :--- |
| 1 | Entri Data SKP Kegiatan Profesi | Tenaga Medis | Nomor SK Kemenkes, tanggal kegiatan, perolehan SKP | Draf klaim perolehan SKP di portal tenaga medis | Data pendukung sertifikat digital wajib diunggah dalam format PDF/JPG resolusi jelas. |
| 2 | Verifikasi Sertifikat Plataran Sehat | Staf Diklat | ID verifikasi sertifikat portal Plataran Sehat Kemenkes | Status sertifikat terverifikasi sah (*Verified*) | Jika nomor akreditasi tidak terdaftar di Kemenkes, verifikator mengembalikan berkas ke nakes. |
| 3 | Pemutakhiran Tracker Akumulasi SKP | Sistem Diklat | Poin SKP terverifikasi berdasarkan 3 ranah resmi | Visual progress bar pemenuhan target SKP 5 tahun | Memberikan alert peringatan pada H-180 dan H-90 perpanjangan SIP jika akumulasi SKP masih kurang. |
| 4 | Validasi Prasyarat RKK Sub-14 | Service Gate `INT-HR-77`| Permohonan butir tindakan klinis spesifik di Sub-14 | Keputusan validasi prasyarat kompetensi (`Eligible` / `Locked`) | Jika sertifikat prasyarat tidak ditemukan atau expired, butir RKK otomatis terkunci pada opsi supervisi. |
| 5 | Sinkronisasi Mutu Klinis OPPE Sub-15 | Service Query `INT-HR-78`| Data kelulusan diklat dan jam pembelajaran nakes | Pemenuhan skor pilar *Medical Knowledge* OPPE | Berkontribusi langsung pada komponen kualitatif penilaian kinerja profesional dokter dan perawat. |
