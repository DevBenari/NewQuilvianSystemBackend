# Alur Proses Bisnis Sub-Modul 15: OPPE, FPPE & Recredentialing (Evaluasi Praktik Profesional & Rekredensialing Staf Medis RS)

Dokumen ini mendefinisikan seluruh diagram alur proses bisnis (*flowchart*) untuk **Sub-Modul 15 (Slice `S-C1-OPPE`)** yang mengatur evaluasi praktik profesional berkelanjutan (*Ongoing Professional Practice Evaluation* - OPPE), evaluasi terfokus intensif (*Focused Professional Practice Evaluation* - FPPE), kerahasiaan telaah sejawat (*Peer Review Privilege*), agregasi rapor 3 tahunan, serta persidangan komite profesi sebelum perpanjangan SPK/RKK disahkan Direktur Utama RS.

---

## 15.1 Alur Pengumpulan Indikator Mutu Klinis OPPE Tahunan Terintegrasi SIMRS / KMKP

Diagram ini menggambarkan siklus pengumpulan data indikator kuantitatif dari SIMRS EMR & Komite Mutu (KMKP) yang dipadukan dengan penilaian kualitatif peer review Kelompok Staf Medis (KSM) dalam siklus tahunan/semesteran berbobot 60% kuantitatif + 40% kualitatif sesuai [`HRD-DEC-152`](../00-interview-decisions.md#L7290-L7310).

```mermaid
flowchart TD
    subgraph SistemPelayananSIMRS["SIMRS EMR & Komite Mutu (KMKP)"]
        A1["Pelayanan Pasien Berjalan di Rawat Jalan / Inap / OK"] --> A2["Catat Volume Prosedur & Tindakan Invasif"]
        A1 --> A3["Catat Kepatuhan Pengisian Resume Medis 1x24 Jam"]
        A1 --> A4["Catat Kepatuhan Clinical Pathway & Resep Formularium"]
        A1 --> A5["Catat Insiden Klinis (KTD / KNC / IDO / Komplikasi)"]
        A2 & A3 & A4 & A5 --> A6["Agregasi Data Bulanan SIMRS ke Engine INT-HR-73"]
    end

    subgraph MesinEvaluasiOppe["Mesin Agregasi OPPE & Scheduler Tahunan"]
        A6 --> B1["Hitung Skor Indikator Kuantitatif (Bobot 60%)"]
        B1 --> B2{"Tutup Periode Semester / Tahunan"}
        B2 --> B3["Generate Draf Lembar Penilaian OPPE Dokter"]
        B3 --> B4["Buka Antrean Evaluasi Kualitatif KSM"]
    end

    subgraph KepalaKsmDanKomite["Kepala KSM & Subkomite Mutu Profesi"]
        B4 --> C1["Akses Lembar Penilaian Kualitatif Dokter"]
        C1 --> C2["Input Skor 6 Pilar: Profesionalisme, Komunikasi SBAR, Kerjasama Tim (Bobot 40%)"]
        C2 --> C3["Beri Rekomendasi Tertulis / Catatan Pembinaan"]
        C3 --> C4["Finalisasi Evaluasi OPPE Tahunan (Signed Digital)"]
    end

    subgraph StafMedisESS["Staf Medis / Tenaga Kesehatan (ESS)"]
        C4 --> D1["Terima Notifikasi: Rapor OPPE Tahunan Terbit"]
        D1 --> D2["Buka Portal ESS: Lihat Nilai Rapor & Feedback KSM"]
        D2 --> D3{"Ada Keberatan / Sanggahan Nilai?"}
        D3 -- "Ya" --> D4["Ajukan Klarifikasi Tertulis ke Ketua KSM dalam 7 Hari"]
        D3 -- "Tidak" --> D5["Konfirmasi Penerimaan Rapor (Acknowledged)"]
    end

    D4 -.-> C1
```

### Tabel Langkah 15.1

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Agregasi Indikator Klinis** | Background Integration Bus (`INT-HR-73`) | Data episode SIMRS & log insiden KMKP | Dataset kuantitatif tersimpan di `TrxOppeStaffEvaluationDetail` | Jika koneksi SIMRS gagal, worker mengulang otomatis via event bus dengan retry ber-backoff (`HRD-ASM-47`). |
| **2. Penghitungan Skor Kuantitatif** | Engine Kalkulator OPPE | Nilai capaian vs target metrik katalog | Skor kuantitatif (maksimal 60 poin) | Metrik yang tidak memiliki denominator kasus pada periode bersangkutan dikecualikan dari pembagi (*normalized score*). |
| **3. Penilaian Kualitatif KSM** | Kepala KSM / Subkomite Mutu | Observasi perilaku, kepatuhan etika, komunikasi | Skor kualitatif (maksimal 40 poin) | Kepala KSM dilarang menilai berkas dirinya sendiri (*Anti-Self-Review* `VR-OPP-05`). |
| **4. Finalisasi Rapor OPPE** | Subkomite Mutu Profesi | Skor kuantitatif + kualitatif | Rapor final tersimpan status `Finalized`, skor 0–100 | Rapor yang telah difinalisasi dikunci abadi (*immutable*) dan tidak dapat diubah tanpa persetujuan Direktur Medis. |
| **5. Transparansi ESS & Klarifikasi** | Staf Medis Pemilik Akun | Rapor OPPE tahunan | Konfirmasi penerimaan (*Acknowledged*) | Dokter diberikan hak sanggah maksimal 7 hari kerja jika terdapat ketidaksesuaian data SIMRS (`VR-OPP-07`). |

---

## 15.2 Alur Pemicu Alarm Insiden & Pelaksanaan Evaluasi Terfokus (FPPE)

Diagram ini menggambarkan deteksi otomatis ambang batas pemicu kritis (*red flag triggers*: sentinel, komplikasi > 2SD, resume EMR > 48 jam > 20%), penerbitan alert rahasia, serta penetapan rencana audit terfokus intensif sesuai [`HRD-DEC-153`](../00-interview-decisions.md#L7311-L7330).

```mermaid
flowchart TD
    subgraph DetektorAmbangBatas["Mesin Audit Risiko Klinis (FppeTriggerDetectionJob)"]
        E1["Monitoring Mingguan Data Pelayanan & Laporan KMKP"] --> E2{"Evaluasi Kondisi Ambang Pemicu Kritis"}
        E2 -- "Terjadi 1 Kasus Insiden Sentinel" --> E3["Trigger Aktif: SENTINEL_EVENT"]
        E2 -- "Komplikasi Bedah > 2 SD Rata-Rata KSM" --> E4["Trigger Aktif: ABNORMAL_COMPLICATION"]
        E2 -- "Resume Medis > 48 Jam Kronis > 20%" --> E5["Trigger Aktif: CHRONIC_EMR_DELINQUENCY"]
        E2 -- "Rekomendasi Sidang Etik Profesi" --> E6["Trigger Aktif: ETHICAL_DISCIPLINARY"]
        E3 & E4 & E5 & E6 --> E7["Kirim Alert Rahasia ke Ketua Komite Medik & Dir Medis"]
    end

    subgraph SidangSubkomiteMutu["Komite Medik & Subkomite Mutu Profesi"]
        E7 --> F1["Akses Console Audit FPPE Triggered"]
        F1 --> F2{"Telaah Awal: Apakah Perlu Evaluasi Terfokus?"}
        F2 -- "Tidak, Faktor Eksternal Pasien Kompleks" --> F3["Tutup Alert dengan Catatan Klinis Justifikasi"]
        F2 -- "Ya, Indikasi Mutu / Keterampilan Klinis" --> F4["Terbitkan Surat Tugas FPPE: Tunjuk Supervisor / Proctor"]
        F4 --> F5["Tetapkan Durasi (Maks 6 Bulan) & Target Kuota Kasus"]
    end

    subgraph PelaksanaanFPPE["Supervisor / Proctor & Dokter Terfokus"]
        F5 --> G1["Dokter Menjalankan Tindakan Klinis Didampingi Supervisor"]
        G1 --> G2["Supervisor Mengisi Lembar Observasi Setiap Kasus (TrxFppeCaseLog)"]
        G2 --> G3{"Apakah Durasi Selesai / Target Kasus Terpenuhi?"}
        G3 -- "Belum Terpenuhi" --> G1
        G3 -- "Target Terpenuhi" --> G4["Kompilasi Skor Akhir & Kesimpulan Evaluasi FPPE"]
    end

    subgraph SidangPutusanFPPE["Sidang Pleno Komite & Direktur Utama"]
        G4 --> H1["Sidangkan Hasil Laporan FPPE"]
        H1 --> H2{"Kesimpulan Putusan FPPE"}
        H2 -- "Kompeten Penuh" --> H3["Pulihkan Kewenangan Mandiri (Level 4)"]
        H2 -- "Perlu Supervisi Lanjutan" --> H4["Perpanjang Masa Pendampingan (Maks 3 Bulan)"]
        H2 -- "Tidak Memenuhi Standar Mutu" --> H5["Batasi Kewenangan (Level 3) / Cabut Tindakan (Level 1-2)"]
        H3 & H4 & H5 --> H6["Sinkronkan Status RKK ke SIMRS EMR (INT-HR-69)"]
    end
```

### Tabel Langkah 15.2

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Deteksi Ambang Pemicu** | Background Job Scheduler | Data SIMRS, KMKP, dan log etik | Alert insiden klinis terkirim rahasia | Ambang deviasi komplikasi dihitung otomatis menggunakan formula statistik 2 standar deviasi (`VR-OPP-03`). |
| **2. Telaah Disposisi FPPE** | Ketua Komite Medik & Dir Medis | Laporan pemicu & kronologi klinis | Surat Keputusan Penugasan FPPE | Evaluasi awal wajib diselesaikan dalam tempo maksimal 7 hari kerja sejak alert terpicu (`HRD-DEC-153`). |
| **3. Pelaksanaan Supervisi** | Dokter Supervisor Penanggung Jawab | Pendampingan tindakan klinis nakes | Lembar observasi per kasus tersimpan di `TrxFppeCaseLog` | Supervisor wajib mengisi logbook pendampingan maksimal 1x24 jam pasca-tindakan bedah (`VR-OPP-09`). |
| **4. Sidang Evaluasi FPPE** | Pleno Komite Profesi | Rekapitulasi log kasus proctoring | Berita acara rekomendasi putusan FPPE | Jika dalam 6 bulan kuota kasus belum tercapai, komite wajib mengevaluasi kelanjutan proctoring (`HRD-ASM-49`). |
| **5. Penegakan Status SIMRS** | Clinical Gate Service (`INT-HR-69`) | Putusan sidang komite & pengesahan Dirut | Pembaruan status wewenang di SIMRS seketika | Jika wewenang dibatasi/dicabut, SIMRS seketika menerapkan *Hard-Stop* atau mewajibkan co-signature supervisor. |

---

## 15.3 Alur FPPE Adaptasi Staf Medis Baru & Proctoring Prosedur Klinis Baru

Diagram ini menggambarkan alur pembinaan klinis awal bagi dokter spesialis baru bergabung (3 bulan / 10 kasus) dan dokter yang mengajukan penambahan wewenang prosedur baru (6 bulan / 5 kasus) sesuai [`HRD-DEC-154`](../00-interview-decisions.md#L7331-L7350).

```mermaid
flowchart TD
    subgraph InisiasiKewenangan["Staf Medis Pemohon / Baru Bergabung"]
        I1["Dokter Baru Bergabung / Mengajukan Prosedur Baru di Sub-14"] --> I2{"Jenis Permohonan FPPE"}
        I2 -- "Dokter Spesialis Baru" --> I3["Tetapkan Target: 3 Bulan & Minimal 10 Kasus Pertama"]
        I2 -- "Prosedur Klinis Baru" --> I4["Tetapkan Target: 6 Bulan & Minimal 5 Kasus Terpantau"]
    end

    subgraph PenunjukanProctor["Subkomite Kredensial & KSM"]
        I3 & I4 --> J1["Tunjuk Dokter Proctor Senior Bersertifikat Kolegium"]
        J1 --> J2["Terbitkan Status SPK Level 3 (Supervisi / Proctoring)"]
        J2 --> J3["Sinkronkan Status Supervisi ke SIMRS (Wajib Co-Signature)"]
    end

    subgraph PelaksanaanProctoring["Proctoring & Pendampingan Kasus Riil"]
        J3 --> K1["Dokter Menjadwalkan Pasien / Prosedur di SIMRS"]
        K1 --> K2["Dokter Proctor Hadir Mendampingi Tindakan"]
        K2 --> K3["Proctor Input Penilaian: Keterampilan Teknik, Keputusan Kritis & Komplikasi"]
        K3 --> K4{"Apakah Jumlah Kasus Minimal Terpenuhi?"}
        K4 -- "Belum" --> K1
        K4 -- "Ya, Kuota Terpenuhi" --> K5["Proctor Submit Berita Acara Kelulusan Proctoring"]
    end

    subgraph PengesahanKemandirian["Ketua Komite Medik & Direktur Utama"]
        K5 --> L1["Verifikasi Rekam Jejak Bebas Komplikasi Fatal"]
        L1 --> L2{"Rekomendasi Kelulusan Mandiri"}
        L2 -- "Lulus Memuaskan" --> L3["Terbitkan Rekomendasi Peningkatan Wewenang ke Level 4 Mandiri"]
        L2 -- "Belum Mandiri" --> L4["Perpanjang Masa Supervisi dengan Remedial Portofolio"]
        L3 --> L5["Pengesahan SPK Baru Mandiri oleh Direktur Utama RS"]
        L5 --> L6["Buka Kunci Akses Order Mandiri Penuh di SIMRS (INT-HR-69)"]
    end
```

### Tabel Langkah 15.3

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Inisiasi Target Kasus** | Subkomite Kredensial | Jenis pengajuan (Dokter Baru / Prosedur Baru) | Ketetapan masa pemantauan & kuota kasus minimal | Dokter baru wajib menyelesaikan 10 kasus; prosedur baru wajib minimal 5 kasus mandiri terdampingi (`HRD-DEC-154`). |
| **2. Penunjukan Proctor** | Kepala KSM | KSM spesialisasi terkait | Nama dokter proctor senior ber-SPK Level 4 | Proctor wajib memiliki keahlian sebidang dan tidak memiliki konflik kepentingan pribadi (`VR-OPP-11`). |
| **3. Pendampingan & Input Log** | Dokter Proctor | Pelaksanaan tindakan operasi / prosedur klinis | Evaluasi per kasus di `TrxFppeCaseLog` | Bila terjadi komplikasi serius selama proctoring, tindakan langsung dihentikan dan dievaluasi komite mutu. |
| **4. Rekomendasi Mandiri** | Dokter Proctor & Ketua Komite | Rekapitulasi logbook kasus lengkap | Berita acara kelulusan proctoring | Tindakan yang direkomendasikan mandiri wajib bebas dari insiden kesalahan prosedur berat. |
| **5. Promosi Level 4 di SIMRS** | Direktur Utama RS & Gate Service | Berita acara kelulusan sah | SPK/RKK Level 4 Mandiri aktif di SIMRS | Sistem seketika membuka blokir order mandiri dan menonaktifkan kewajiban co-signature supervisor (`INT-HR-69`). |

---

## 15.4 Alur Sidang Rekredensialing 3 Tahunan & Sinkronisasi Putusan RKK ke SIMRS EMR

Diagram ini menggambarkan agregasi rapor OPPE 3 tahun berturut-turut (bobot 25%-35%-40%), penilaian kelayakan kesehatan (*Medical Fitness*), pengelompokan tier Hijau/Kuning/Merah, dan eksekusi 4 putusan akhir ke SIMRS EMR sesuai [`HRD-DEC-155`] & [`HRD-DEC-157`].

```mermaid
flowchart TD
    subgraph MesinRekredensialing["Mesin Siklus 3 Tahun (Background Worker H-60)"]
        M1["Deteksi Jatuh Tempo SPK 3 Tahun (Sub-14 H-60)"] --> M2["Tarik Rapor OPPE Tahun 1 (Bobot 25%), Th 2 (35%), Th 3 (40%)"]
        M2 --> M3["Tarik Status MCU Kesehatan Fisik & Jiwa K3RS (INT-HR-76)"]
        M3 --> M4["Hitung Skor Komposit Rekredensialing (0-100)"]
        M4 --> M5{"Evaluasi Kategori Kelayakan Rekredensialing"}
        M5 -- "Skor >= 80 & Bebas Sentinel" --> M6["Tier Hijau: Fast-Track Perpanjangan"]
        M5 -- "Skor 65 - 79 / Catatan EMR" --> M7["Tier Kuning: Perlu Wawancara Klarifikasi"]
        M5 -- "Skor < 65 / Gagal FPPE / Etik" --> M8["Tier Merah: Sidang Pleno Khusus Komite"]
    end

    subgraph SidangPlenoRekredensial["Komite Medik / Keperawatan & Direktur Utama"]
        M6 --> N1["Rapat Pleno Cepat: Setujui Perpanjangan Penuh"]
        M7 --> N2["Wawancara Klarifikasi & Catatan Pembinaan"]
        M8 --> N3["Sidang Pleno Mendalam: Uji Portofolio Ulang"]
        N2 & N3 --> N4{"Putusan Akhir Sidang Rekredensialing"}
        N4 -- "Perpanjang As-Is" --> N5["Putusan 1: Perpanjangan Penuh (Renewed As-Is)"]
        N4 -- "Lulus FPPE Prosedur Baru" --> N6["Putusan 2: Peningkatan Kewenangan (Upgraded Privilege)"]
        N4 -- "Kinerja Menurun / Komplikasi" --> N7["Putusan 3: Pembatasan Wewenang (Restricted to Supervised)"]
        N4 -- "Pelanggaran Berat / Malpraktik" --> N8["Putusan 4: Pencabutan Tindakan (Revoked / Terminated)"]
    end

    subgraph PengesahanDanSinkronisasi["Pengesahan Direktur Utama & Gerbang SIMRS"]
        N1 --> N5
        N5 & N6 & N7 & N8 --> O1["Pengesahan SPK & Lampiran RKK Baru oleh Direktur Utama RS"]
        O1 --> O2["Terbitkan Dokumen PDF Ber-QR Code & Arsip ke E-Dossier (INT-HR-72)"]
        O2 --> O3["Trigger Integrasi INT-HR-75: Sinkronkan Hak Akses ke SIMRS / EMR"]
        O3 --> O4{"Dampak Penegakan di SIMRS EMR (INT-HR-69)"}
        O4 -- "Putusan 1 & 2" --> O5["Hak Order Mandiri Level 4 Aktif Penuh"]
        O4 -- "Putusan 3" --> O6["Tindakan Ditahan di SIMRS: Wajib Co-Signature Supervisor 24 Jam"]
        O4 -- "Putusan 4" --> O7["Tindakan Diblokir Keras di SIMRS: Hard-Stop Mutlak"]
    end
```

### Tabel Langkah 15.4

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Agregasi Rapor 3 Tahun** | Background Engine Rekredensialing | Nilai OPPE Th 1, Th 2, Th 3 dan MCU K3RS | Skor komposit terbobot & penetapan tier Hijau/Kuning/Merah | Bobot progresif 25%-35%-40% mencerminkan kompetensi terkini (`HRD-DEC-155`). |
| **2. Verifikasi Kelayakan Medis** | K3RS Service (`INT-HR-76`) | Hasil Medical Check-Up fisik & jiwa berkala | Surat Keterangan *Medical Fitness* | Jika dokter belum MCU, berkas rekredensialing berstatus `PendingMCU` (`HRD-ASM-50`). |
| **3. Persidangan Rekredensial** | Pengurus Komite Profesi | Berkas komposit & portofolio 3 tahun | Risalah putusan sidang rekredensialing | Berkas tier Hijau diproses fast-track; tier Kuning/Merah wajib wawancara tatap muka. |
| **4. Pengesahan SPK Baru** | Direktur Utama RS | Usulan resmi Ketua Komite | Dokumen SPK/RKK sah masa berlaku 3 tahun | Dokumen resmi terbit ber-QR Code dan langsung terarsip abadi di e-Dossier Sub-02. |
| **5. Penyelarasan Otomatis SIMRS** | Clinical Gate Service (`INT-HR-75`, `INT-HR-69`) | Dataset RKK disahkan Direktur Utama | Hak akses wewenang SIMRS terbarui seketika | Invalidasi cache Redis gate < 50ms seketika; tidak memerlukan instruksi memo manual (`HRD-DEC-157`). |
