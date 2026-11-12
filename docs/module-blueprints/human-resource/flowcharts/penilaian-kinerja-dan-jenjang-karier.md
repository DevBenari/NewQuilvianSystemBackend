# Alur Proses Bisnis Sub-Modul 17: Performance Management & Clinical Career (`S-C3`)

Dokumen ini memodelkan diagram alur proses bisnis (*business flowcharts*) berbasis standar tata kelola kinerja rumah sakit, PermenPAN-RB 6/2022, Permenkes 40/2017 (Jenjang Karir Profesional Perawat Klinis), dan regulasi KARS untuk **Sub-Modul 17: Performance Management & Clinical Career (Slice `S-C3`)**.

---

## 17.1 Siklus Evaluasi Kinerja & Cascading KPI Tahunan Rumah Sakit

Diagram ini menggambarkan alur pembukaan siklus evaluasi kerja tahunan/semesteran, cascading target sasaran strategis rumah sakit dan instalasi ke sasaran kerja individu, serta pencatatan check-in berkala.

```mermaid
flowchart TD
    subgraph Direksi_HRD["Direksi & Tim HR Performance"]
        A1["Buka Periode Siklus Kinerja (Tahunan/Semesteran)"] --> A2["Tetapkan Sasaran Strategis RS & Indikator Mutu Instalasi"]
        A2 --> A3["Publikasi Template Sasaran & Bobot Standar 60:40"]
    end

    subgraph Kepala_Ruangan["Kepala Ruangan / Supervisor Unit"]
        A3 --> B1["Cascading Target Unit ke Staf Ruangan"]
        B1 --> B2["Review & Persetujuan Target KPI Individu Pegawai"]
        B2 --> B3["Sesi Check-in Berkala & Monitoring Evidence Capaian"]
    end

    subgraph Pegawai_Staf["Pegawai / Tenaga Medis & Non-Medis"]
        B1 -.-> C1["Susun Draf Sasaran Kerja Individu di ESS"]
        C1 --> B2
        B3 -.-> C2["Catat Bukti Capaian Output & Logbook Berkala"]
    end

    B3 --> D1["Periode Evaluasi Kinerja Aktif & Siap Asesmen"]
```

### Tabel Langkah 17.1: Penetapan Sasaran Kerja & Cascading KPI
| No | Langkah Kegiatan | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Buka Periode Siklus Kinerja | HR Performance | Kalender kerja RS, parameter tahun evaluasi | Siklus berstatus `Active` | Jika periode tahun sebelumnya belum ditutup, sistem menolak pembukaan siklus baru (`VR-PRF-01`). |
| 2 | Cascading Target Unit ke Staf | Kepala Ruangan | Sasaran mutu unit, uraian tugas jabatan | Draf target KPI per pegawai | Jika target tidak selaras dengan KPI instalasi, sistem menandai peringatan inkonsistensi. |
| 3 | Penyusunan Sasaran Individu | Pegawai | Template target jabatan di ESS | Draf sasaran kerja individu | Pegawai wajib mengentri minimal 3 dan maksimal 8 butir target KPI spesifik (`VR-PRF-02`). |
| 4 | Persetujuan Target KPI | Kepala Ruangan | Draf sasaran kerja staf | Sasaran kerja status `Locked` | Atasan dapat mengembalikan draf jika target terlalu rendah atau tidak terukur. |
| 5 | Check-in Berkala & Evidence | Pegawai & Karu | Logbook harian, laporan bulanan unit | Catatan pembinaan berkala | Check-in dicatat minimal 1 kali per semester guna mencegah kejutan penilaian di akhir tahun. |

---

## 17.2 Penilaian Berjenjang & Sidang Pleno Kalibrasi Manajemen

Diagram ini memodelkan alur evaluasi akhir komposit 60% KPI + 40% Perilaku Core Values BerAKHLAK, penilaian berjenjang (Self-assessment -> Karu -> Kabid), dan sidang pleno kalibrasi manajemen dengan pedoman distribusi fleksibel.

```mermaid
flowchart TD
    subgraph Staf["Pegawai (Self-Assessment)"]
        S1["Input Refleksi Diri & Unggah Bukti Portofolio"] --> S2["Serahkan Lembar Evaluasi Mandiri"]
    end

    subgraph Karu["Kepala Ruangan (Atasan Langsung)"]
        S2 --> K1["Nilai 60% KPI Hasil Kerja & 40% Core Values BerAKHLAK"]
        K1 --> K2["Beri Ulasan Catatan Pembinaan & Skor Rekomendasi"]
        K2 --> K3["Serahkan ke Atasan Kedua (Kabid)"]
    end

    subgraph Kabid["Kepala Bidang (Atasan Kedua)"]
        K3 --> M1["Review Konsistensi Penilaian Antar Ruangan"]
        M1 --> M2["Pengesahan Berkas Menuju Sidang Kalibrasi"]
    end

    subgraph Sidang_Kalibrasi["Komite Kalibrasi (Direksi, HR, Kabid)"]
        M2 --> C1["Kompilasi Kurva Sebaran Nilai RS (Guiding 10-15% A, 70-80% B, 5-10% C/D)"]
        C1 --> C2{"Apakah Terjadi Disparitas / Leniency Bias?"}
        C2 -- "Ya (Deviasi Ekstrem)" --> C3["Sidang Pleno & Penyesuaian Skor Berbasis Bukti (Override Notes)"]
        C2 -- "Tidak (Sebaran Wajar)" --> C4["Pengesahan Skor Final Komposit RS"]
        C3 --> C4
    end

    subgraph Output_Rilis["Sistem Kinerja HRD"]
        C4 --> R1["Kunci Lembar Evaluasi (FinalApproved) & Rilis ke Portal ESS Pegawai"]
    end
```

### Tabel Langkah 17.2: Penilaian Berjenjang & Sidang Kalibrasi
| No | Langkah Kegiatan | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Pengisian Self-Assessment | Pegawai | Lembar evaluasi di ESS, evidence capaian | Berkas status `SelfSubmitted` | Pegawai wajib mengisi refleksi diri sebelum tanggal batas waktu siklus mandiri berakhir. |
| 2 | Penilaian Atasan Langsung | Kepala Ruangan | Nilai self-assessment, log kehadiran, SOP | Lembar evaluasi status `ManagerAssessed` | Karu wajib memberi skor pada seluruh butir KPI dan perilaku Core Values (`VR-PRF-03`). |
| 3 | Review Atasan Kedua | Kepala Bidang | Rapor seluruh ruangan di bawah bidangnya | Berkas status `SecondLineReviewed` | Kabid dapat mengembalikan berkas ke Karu jika ditemukan ketidaksesuaian objektif. |
| 4 | Sidang Pleno Kalibrasi | Komite Kalibrasi | Matriks sebaran nilai seluruh instalasi | Risalah kalibrasi & skor disahkan | Penyesuaian skor wajib mencantumkan alasan tertulis resmi (*override notes*, `VR-PRF-05`). |
| 5 | Publikasi Skor Akhir | HR Performance | Berkas terkalibrasi | Skor rilis ke ESS pegawai | Pengumuman nilai membuka jendela masa sanggah banding selama 7 hari kalender (`VR-PRF-12`). |

---

## 17.3 Alur Program Peningkatan Kinerja (*Performance Improvement Plan* - PIP) 60 Hari

Diagram ini memodelkan pembinaan terstruktur bagi staf yang memperoleh skor kinerja berpredikat "Kurang" (skor < 65) selama 60 hari kalender, pendampingan mentor, evaluasi mingguan, dan 3 keputusan akhir.

```mermaid
flowchart TD
    subgraph Trigger_PIP["Pemicu Sistem"]
        P1["Skor Akhir Kinerja Terkalibrasi < 65 (Predikat Kurang)"] --> P2["Penerbitan Otomatis Kontrak PIP 60 Hari Kalender"]
    end

    subgraph Pelaksanaan_PIP["Staf, Mentor & Kepala Ruangan"]
        P2 --> M1["Penetapan Rencana Aksi Perbaikan & Target Spesifik"]
        M1 --> M2["Penandatanganan Komitmen PIP oleh Pegawai & Atasan"]
        M2 --> M3["Pelaksanaan Kerja & Mentoring Terstruktur"]
        M3 --> M4["Pengisian Logbook Evaluasi Mingguan (Minggu 1 s.d. 8)"]
    end

    subgraph Evaluasi_Akhir["Komite Kinerja & HRD"]
        M4 --> E1["Sidang Pleno Evaluasi Hari Ke-60"]
        E1 --> E2{"Hasil Capaian Rencana Aksi PIP?"}
        E2 -- "Lulus (Target Tercapai)" --> R1["Kinerja Kembali Normal (Status PIP Completed)"]
        E2 -- "Perbaikan Positif Parsial" --> R2["Perpanjangan Terbatas Maksimal 30 Hari (Status Extended)"]
        E2 -- "Gagal (Tidak Ada Perbaikan)" --> R3["Eskalasi ke HR Legal & Komite Etik (Status Failed)"]
        R2 --> E1
    end
```

### Tabel Langkah 17.3: Tata Kelola Program Peningkatan Kinerja (PIP)
| No | Langkah Kegiatan | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Pemicu Otomatis PIP | Sistem HRD | Skor komposit akhir < 65.00 | Draf kontrak PIP terbit otomatis | Sistem membekukan hak pengajuan mutasi/promosi selama staf berstatus PIP aktif (`VR-PRF-08`). |
| 2 | Penandatanganan Kontrak PIP | Staf & Karu | Form sasaran perbaikan, penunjukan mentor | Kontrak PIP status `Active` | Staf menolak tanda tangan: atasan mencatat berita acara pembangkangan ke HR Legal. |
| 3 | Logbook Mentoring Mingguan | Mentor & Karu | Laporan observasi harian kinerja staf | 8 Catatan mingguan terverifikasi | Peringatan otomatis dikirimkan jika catatan mingguan tidak diisi dalam 7 hari (`VR-PRF-09`). |
| 4 | Sidang Putusan PIP | Komite Kinerja | Rekapitulasi 8 logbook mingguan | Putusan: Lulus, Perpanjang, atau Gagal | Perpanjangan hanya diizinkan tepat 1 kali maksimal 30 hari kalender (`VR-PRF-10`). |
| 5 | Tindak Lanjut Gagal PIP | HR Legal | Putusan sidang berstatus `Failed` | Rekomendasi mutasi, demosi, atau evaluasi kontrak | Penanganan dialihkan ke prosedur hubungan industrial Sub-Modul 19 sesuai UU Ketenagakerjaan. |

---

## 17.4 Evaluasi Kenaikan Jenjang Karier Klinis Nakes (*Clinical Ladder* PK I - PK V)

Diagram ini menggambarkan pemisahan skor kinerja umum dengan kelayakan kenaikan jenjang karier klinis perawat/nakes, verifikasi logbook tindakan klinis, kompetensi Sub-16, dan rekomendasi Komite Keperawatan.

```mermaid
flowchart TD
    subgraph Staf_Klinis["Tenaga Keperawatan / Bidan / Nakes"]
        J1["Pengajuan Kenaikan Jenjang Karier Klinis di ESS"] --> J2["Unggah Portofolio Logbook Tindakan & Bukti CPD"]
    end

    subgraph Gate_Sistem["Gate Validasi Kelayakan Sistem (INT-HR-83)"]
        J2 --> G1{"Cek Prasyarat Administratif Minimal"}
        G1 -- "Skor KPI < 75 atau Masa Kerja Kurang" --> F1["Tolak Pengajuan (Belum Memenuhi Syarat Minimal)"]
        G1 -- "Lulus Prasyarat" --> G2{"Cek Prasyarat Kompetensi Sub-16 & Bebas Sentinel Sub-15"}
        G2 -- "Sertifikat Expired / Ada Kasus Sentinel" --> F2["Tolak Pengajuan (Prasyarat Klinis Tidak Terpenuhi)"]
        G2 -- "Lulus Semua Prasyarat" --> P1["Berkas Diteruskan ke Komite Keperawatan"]
    end

    subgraph Komite_Keperawatan["Sub-Komite Kredensial / Asesor Klinis"]
        P1 --> K1["Audit Dokumen Portofolio Logbook Klinis"]
        K1 --> K2["Ujian Asesmen Kompetensi Jenjang Klinis (Portofolio/Wawancara)"]
        K2 --> K3["Sidang Pleno Penetapan Rekomendasi Jenjang (PK I s.d. PK V)"]
    end

    subgraph Otorisasi_Direksi["Direktur Utama / Direktur Medik & Keperawatan"]
        K3 --> D1["Penerbitan Surat Keputusan (SK) Kenaikan Jenjang Klinis"]
        D1 --> D2["Update Profil Level Nakes di Sub-02 & Penyesuaian RKK di Sub-14"]
    end
```

### Tabel Langkah 17.4: Evaluasi Kenaikan Jenjang Karier Klinis
| No | Langkah Kegiatan | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Pengajuan Kenaikan Jenjang | Nakes | Formulir ESS, logbook kasus mandiri | Draf permohonan jenjang klinis | Pengajuan terkunci jika masa kerja pada jenjang berjalan belum mencukupi standar regulasi. |
| 2 | Verifikasi Prasyarat Sistem | Service Gate | Skor kinerja tahunan, data Sub-15/16 | Status `EligibilityVerified` | Sistem menolak jika skor kinerja tahunan < 75 atau sertifikat kompetensi mati (`VR-PRF-14`). |
| 3 | Asesmen Portofolio Komite | Asesor Klinis | Logbook tindakan klinis, bukti kredensial | Skor rubrik kompetensi jenjang | Jika logbook tindakan mandiri kurang dari kuota minimal, nakes diminta melengkapi kasus. |
| 4 | Sidang Rekomendasi Komite | Komite Nakes | Berita acara asesmen klinis | Surat rekomendasi kenaikan jenjang | Komite berwenang merekomendasikan kenaikan, penundaan 6 bulan, atau retensi jenjang. |
| 5 | Penetapan SK & Sinkronisasi | Direktur Utama | Rekomendasi komite | SK Penetapan Jenjang Klinis Baru | Sistem otomatis menyinkronkan jenjang baru ke profil Sub-02 dan kewenangan RKK di Sub-14 (`INT-HR-83`). |

---

## 17.5 Alur Pengajuan Banding / Sanggah Nilai Kinerja Pegawai (*Appraisal Dispute*)

Diagram ini memodelkan prosedur hak sanggah bagi pegawai yang tidak menyetujui hasil evaluasi kinerja pasca kalibrasi, batas waktu pengajuan 7 hari, mediasi HR Performance, dan putusan final Komite dalam 14 hari kalender.

```mermaid
flowchart TD
    subgraph Pegawai_Pemohon["Pegawai (Pemohon Banding)"]
        B1["Terima Skor Akhir Terkalibrasi di Portal ESS"] --> B2{"Apakah Merasa Nilai Tidak Adil?"}
        B2 -- "Tidak (Menerima)" --> OK["Tanda Tangan Pengakuan Nilai di ESS (Selesai)"]
        B2 -- "Ya (Keberatan)" --> B3{"Pengajuan <= 7 Hari Kalender?"}
        B3 -- "Melebihi 7 Hari" --> EXP["Banding Ditolak Sistem (Kadaluwarsa Waktu Sanggah)"]
        B3 -- "Memenuhi Waktu" --> B4["Input Formulir Sanggah & Wajib Unggah Dokumen Bukti Pembanding"]
    end

    subgraph Mediasi_HR["Tim HR Performance (Fasilitator)"]
        B4 --> H1["Verifikasi Kelayakan Berkas Sanggah dalam 3 Hari Kerja"]
        H1 --> H2["Mediasi Klarifikasi Tertutup antara Atasan Penilai & Pegawai"]
        H2 --> H3{"Apakah Tercapai Kesepakatan di Mediasi?"}
        H3 -- "Ya (Sepakat)" --> H4["Penyesuaian Skor Bersama & Penandatanganan Berita Acara"]
        H3 -- "Tidak (Buntu)" --> H5["Eskalasi ke Komite Kalibrasi / Direktur SDM"]
    end

    subgraph Putusan_Final["Komite Kalibrasi & Direktur SDM"]
        H5 --> K1["Sidang Majelis Banding Kinerja (Batas Waktu 14 Hari Kalender)"]
        K1 --> K2["Penerbitan Putusan Final Mengikat: Nilai Tetap / Nilai Disesuaikan / Ganti Penilai"]
        K2 --> K3["Eksekusi Pemutakhiran Skor Akhir di Database & Penutupan Sengketa"]
    end
```

### Tabel Langkah 17.5: Prosedur Banding Sanggah Nilai Kinerja
| No | Langkah Kegiatan | Pelaku | Masukan (Input) | Keluaran (Output) | Penanganan Kendala / Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Pengajuan Sanggah Banding | Pegawai | Form sanggah di ESS, berkas bukti capaian | Berkas sanggah status `DisputeSubmitted` | Pengajuan ditolak seketika oleh sistem jika diajukan > 7 hari kalender pasca rilis nilai (`VR-PRF-12`). |
| 2 | Verifikasi Dokumen Bukti | HR Performance | Bukti logbook / dokumen pembanding | Status `DisputeUnderReview` | Sanggahan ditolak jika pemohon tidak melampirkan berkas bukti pembanding konkret. |
| 3 | Mediasi Tertutup | HR, Karu, Staf | Berita acara klarifikasi | Notulen kesepakatan mediasi | Jika penilai mengakui kekhilafan data, skor langsung direvisi dengan persetujuan HRD. |
| 4 | Sidang Majelis Banding | Komite Kalibrasi | Berkas sengketa yang belum selesai | Putusan final banding terbit | Komite wajib memutus perkara maksimal dalam 14 hari kalender sejak eskalasi (`VR-PRF-13`). |
| 5 | Eksekusi Pemutakhiran | Sistem HRD | Putusan final Direktur SDM | Skor final terkoreksi & log audit permanen | Putusan final bersifat mengikat dan menutup kesempatan banding lanjutan pada siklus yang sama. |
