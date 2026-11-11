# Alur Proses Bisnis Sub-Modul 14: Credentialing, License & Clinical Privilege (Kredensial, Lisensi Praktik STR/SIP & Rincian Kewenangan Klinis RS)

Dokumen ini mendefinisikan seluruh diagram alur proses bisnis (*flowchart*) untuk **Sub-Modul 14 (Slice `S-C1`)** yang mengatur tata kelola kredensialing 3 komite mandiri (Medik, Keperawatan, Nakes Lain), verifikasi keabsahan dokumen primer (*Primary Source Verification* - PSV) STR seumur hidup dan SIP 5 tahunan, penilaian panel Mitra Bestari (*Peer Review*), pengesahan Surat Penugasan Klinis (SPK) dan Rincian Kewenangan Klinis (RKK) oleh Direktur Utama, serta penegakan gerbang wewenang klinis SIMRS/EMR (`INT-HR-69`) dengan mekanisme *Hard-Stop* bertingkat dan bypass darurat keselamatan pasien.

---

## 14.1 Alur Pengajuan Kredensial, Verifikasi PSV STR Seumur Hidup & Pemantauan Masa Berlaku SIP 5 Tahun

Diagram ini menggambarkan alur pendaftaran berkas kualifikasi klinis nakes, verifikasi sumber primer (PSV) ke Konsil Kedokteran Indonesia (KKI) / SatuSehat SDMK Kemenkes, serta radar otomatis pemantau SIP 5 tahunan berjenjang (H-90, H-60, H-30, H-7) hingga pemblokiran otomatis (*fail-closed*) saat jatuh tempo H-0 sesuai [`HRD-DEC-146`](../00-interview-decisions.md#L7165-L7185).

```mermaid
flowchart TD
    subgraph NakesPemohon["Tenaga Medis / Tenaga Kesehatan (Pemohon)"]
        A1["Buka Menu: Permohonan Kredensial & Lisensi"] --> A2["Pilih Jenis Profesi & Unggah Ijazah, STR, SIP, Sertifikat Pelatihan"]
        A2 --> A3["Input Nomor STR & Nomor Registrasi KKI/SatuSehat SDMK"]
        A3 --> A4["Submit Berkas Kredensial (Status: Submitted)"]
        A11["Terima Peringatan Radar SIP: H-90 / H-60 / H-30 / H-7"]
        A14["Terima Notifikasi Akun & Jadwal Dinonaktifkan (SIP Expired)"]
    end

    subgraph PetugasKredensialing["Sekretariat Kredensialing / KPS RS"]
        A4 --> B1["Ambil Berkas dari Antrean Verifikasi (Status: UnderVerification)"]
        B1 --> B2{"Lakukan Primary Source Verification (PSV) ke Portal KKI / Kemenkes"}
        B2 -- "Data Tidak Valid / Palsu" --> B3["Tolak Berkas (Status: Rejected) & Laporkan Dugaan Pemalsuan"]
        B2 -- "Data Sah & Terverifikasi" --> B4["Catat Bukti PSV (URL, Tanggal Cek, Stempel Digital)"]
        B4 --> B5["Validasi Masa Berlaku SIP (5 Tahun dari Tanggal Terbit Dinkes)"]
        B5 --> B6["Kredensial Legalitas Sah (Status: Verified) -> Teruskan ke Subkomite"]
    end

    subgraph MesinRadarSip["Mesin Radar Pemantau Lisensi & Background Worker"]
        B6 --> C1["Registrasi Tanggal Kedaluwarsa SIP ke Job Scheduler"]
        C1 --> C2{"Pengecekan Harian Jam 00:00 (Scheduler)"}
        C2 -- "Masa Berlaku H-90 / H-60 / H-30 / H-7" --> C3["Kirimkan Alert Peringatan Email, WhatsApp & Notifikasi ESS"]
        C2 -- "Jatuh Tempo H-0 (Pukul 00:00)" --> C4["Eksekusi Fail-Closed Action: Ubah Status SIP Expired"]
        C4 --> C5["Trigger Integrasi INT-HR-71: Blokir Roster Shift Sub-06"]
        C4 --> C6["Trigger Integrasi INT-HR-69: Nonaktifkan Akun Order SIMRS/EMR"]
    end

    subgraph ManajemenMedis["Manajer Pelayanan Medis & Kepala Unit"]
        C3 -.-> D1["Pantau Dashboard Kepatuhan Lisensi Rumah Sakit"]
        C4 -.-> D2["Terima Alert Darurat: Tenaga Medis Nonaktif Legalitas"]
    end

    B3 --> A4
    C3 --> A11
    C4 --> A14
```

### Tabel Langkah 14.1

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Pengajuan Berkas** | Nakes Pemohon | Ijazah, STR, SIP, sertifikat kompetensi terkini | Berkas tersimpan status `Submitted` | Sistem memvalidasi kelengkapan format PDF/JPG dan keharusan input nomor STR (`VR-CRD-01`). |
| **2. Pelaksanaan PSV** | Sekretariat Kredensialing | Nomor STR & portal SatuSehat SDMK / KKI | Status `Verified`, URL verifikasi & tangkapan layar bukti PSV | Bila portal KKI/Kemenkes sedang down, berkas berstatus `PendingPSV` dan diulang maksimal 3x24 jam (`VR-CRD-02`). |
| **3. Pendaftaran Radar SIP** | Background Job Scheduler | Tanggal akhir berlaku SIP nakes | Jadwal notifikasi H-90, H-60, H-30, H-7 terdaftar | Tanggal akhir SIP wajib divalidasi tidak boleh melebihi 5 tahun sejak tanggal terbit izin (`VR-CRD-03`). |
| **4. Peringatan Dini** | Background Engine | Status sisa hari SIP | Email & notifikasi multi-kanal ke nakes dan atasan | Jika nakes belum mengunggah perpanjangan SIP pada H-30, notifikasi eskalasi dikirimkan ke Direktur Medis. |
| **5. Penonaktifan Otomatis** | Fail-Closed Gateway | Event `OnSipExpired` pada H-0 | SIP `Expired`, akun SIMRS nonaktif, jadwal dinas terblokir | Dilakukan tepat pukul 00:00 WIB untuk mencegah risiko malpraktik faskes tanpa izin operasional (`INT-HR-69`, `INT-HR-71`). |

---

## 14.2 Alur Sidang Kredensial 3 Komite Profesi & Penilaian Panel Mitra Bestari (*Peer Review*)

Diagram ini menggambarkan tata kelola telaah klinis terpisah untuk Komite Medik, Komite Keperawatan, dan Komite Tenaga Kesehatan Lainnya, penilaian mandiri buku putih (*whitebook*), dan rekomendasi independen minimal 2 anggota panel Mitra Bestari (*Peer Review*) sesuai [`HRD-DEC-147`](../00-interview-decisions.md#L7186-L7207).

```mermaid
flowchart TD
    subgraph StafKlinis["Tenaga Klinis Terverifikasi"]
        E1["Akses Katalog Rincian Kewenangan Klinis (Whitebook)"] --> E2["Pilih Butir Tindakan Klinis yang Diminta (Mandiri / Supervisi)"]
        E2 --> E3["Lampirkan Logbook Kasus & Bukti Kompetensi Khusus"]
        E3 --> E4["Submit Permohonan RKK (Status: PrivilegeRequested)"]
    end

    subgraph SubkomiteKredensial["Subkomite Kredensial Komite Terkait"]
        E4 --> F1{"Identifikasi Komite Profesi Pemilik"}
        F1 -- "Dokter / Spesialis" --> F2["Komite Medik"]
        F1 -- "Perawat / Bidan" --> F3["Komite Keperawatan"]
        F1 -- "Nakes Lainnya" --> F4["Komite Nakes Lain"]
        F2 & F3 & F4 --> F5["Tunjuk Panel Mitra Bestari (Minimal 2 Peer Reviewers)"]
    end

    subgraph PanelMitraBestari["Panel Mitra Bestari (Peer Reviewers)"]
        F5 --> G1["Reviewer 1 & 2 Telaah Logbook, Sertifikat & Portofolio Klinis"]
        G1 --> G2{"Apakah Perlu Wawancara / Uji Praktik Klinis?"}
        G2 -- "Ya" --> G3["Jadwalkan & Laksanakan Asesmen / Uji Portofolio"]
        G2 -- "Tidak" --> G4["Kompilasi Skor Asesmen Tiap Butir Tindakan"]
        G3 --> G4
        G4 --> G5{"Rekomendasi Panel Reviewer"}
        G5 -- "Memenuhi Mandiri Penuh" --> G6["Rekomendasi: Mandiri (Level 4)"]
        G5 -- "Perlu Pendampingan" --> G7["Rekomendasi: Di Bawah Supervisi (Level 3)"]
        G5 -- "Belum Memenuhi Syarat" --> G8["Rekomendasi: Tidak Berwenang / Ditolak (Level 1-2)"]
        G6 & G7 & G8 --> G9["Submit Berita Acara Rekomendasi Mitra Bestari (Signed by Reviewers)"]
    end

    subgraph RapatPlenoKomite["Rapat Pleno Komite Profesi"]
        G9 --> H1["Sidang Pleno Komite Membahas Rekomendasi Peer Review"]
        H1 --> H2{"Keputusan Pleno Komite"}
        H2 -- "Perlu Klarifikasi / Tambahan Bukti" --> H3["Kembalikan ke Pemohon dengan Catatan Pembinaan"]
        H2 -- "Disetujui" --> H4["Terbitkan Surat Rekomendasi Ketua Komite ke Direktur Utama"]
    end

    H3 -.-> E3
```

### Tabel Langkah 14.2

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Pengajuan RKK** | Staf Klinis Pemohon | Checklist buku putih, logbook kasus minimal, bukti pelatihan | Berkas `PrivilegeRequested` | Butir tindakan invasif wajib melampirkan logbook minimal yang disyaratkan dalam whitebook (`VR-CRD-04`). |
| **2. Pembentukan Peer Review** | Subkomite Kredensial | Komite profesi & rumpun spesialisasi | Panel Mitra Bestari (minimal 2 reviewer sebidang) | Jika RS tidak memiliki dokter spesialis sebidang kedua, ditunjuk pakar kolegium dari luar RS (`HRD-ASM-44`). |
| **3. Asesmen Portofolio** | Panel Mitra Bestari | Berkas nakes, checklist tindakan, uji wawancara | Berita acara rekomendasi per butir tindakan | Jika terdapat perbedaan penilaian antara Reviewer 1 dan 2, diadakan konsensus dipimpin Ketua Subkomite. |
| **4. Rekomendasi Derajat** | Panel Mitra Bestari | Standar kompetensi tindakan klinis | Rekomendasi Level 4 (Mandiri), Level 3 (Supervisi), Level 1-2 (Ditolak) | Tindakan baru/canggih yang belum pernah dilakukan wajib direkomendasikan berstatus supervisi terlebih dahulu. |
| **5. Rapat Pleno Komite** | Pengurus Komite Profesi | Rekomendasi panel mitra bestari | Surat Rekomendasi Ketua Komite bertanda tangan elektronik | Jika ada penolakan hak klinis tertentu, wajib disertakan alasan klinis objektif dan saran remedial kompetensi. |

---

## 14.3 Alur Penerbitan dan Pengesahan SPK & RKK oleh Direktur Utama serta Klasifikasi Derajat Supervisi

Diagram ini menggambarkan alur penyerahan rekomendasi komite, penyusunan draf Surat Penugasan Klinis (SPK) dan Rincian Kewenangan Klinis (RKK), pengesahan tunggal Direktur Utama rumah sakit, serta pembagian kewenangan mandiri versus supervisi sesuai [`HRD-DEC-148`](../00-interview-decisions.md#L7208-L7228).

```mermaid
flowchart TD
    subgraph SekretariatKomite["Sekretariat Komite Medis / Keperawatan / Nakes"]
        I1["Terima Surat Rekomendasi Ketua Komite"] --> I2["Generate Draf SPK & Lampiran RKK dari Template Sistem"]
        I2 --> I3["Tetapkan Dokter Supervisor untuk Tindakan Level 3 (Supervisi)"]
        I3 --> I4["Kirimkan Berkas Draf ke Meja Direktur Pelayanan Medik"]
    end

    subgraph DirekturPelayanan["Direktur Pelayanan Medik / Keperawatan"]
        I4 --> J1["Review Kebutuhan Pelayanan RS & Rekomendasi Komite"]
        J1 --> J2{"Paraf Draf Persetujuan Penugasan Klinis?"}
        J2 -- "Minta Penyesuaian" --> J3["Kembalikan ke Komite dengan Catatan Manajerial"]
        J2 -- "Setuju" --> J4["Bubuhkan Paraf Digital & Teruskan ke Direktur Utama"]
    end

    subgraph DirekturUtama["Direktur Utama Rumah Sakit"]
        J4 --> K1["Akses Console Pengesahan SPK/RKK"]
        K1 --> K2{"Pengesahan Akhir Direktur Utama"}
        K2 -- "Tolak / Minta Revisi" --> K3["Kembalikan Berkas ke Direktur Pelayanan"]
        K2 -- "Sahkan & Tanda Tangan" --> K4["Tanda Tangan Elektronik (e-Sign) & Terbitkan Dokumen SPK-RKK Resmi"]
    end

    subgraph DistribusiSistem["Distribusi Sistem & E-Dossier"]
        K4 --> L1["Terbitkan Nomor Resmi SPK/RKK Gapless Auto-Number"]
        L1 --> L2["Generate File PDF Ber-QR Code Keabsahan Publik"]
        L2 --> L3["Arsipkan Otomatis ke E-Dossier Nakes di Sub-Modul 02 (INT-HR-72)"]
        L3 --> L4["Kirim Dokumen Digital ke Akun ESS Staf Medis (Menu: Kewenangan Klinis)"]
        L4 --> L5["Sinkronisasi Real-Time Hak Akses Tindakan ke SIMRS / EMR (INT-HR-69)"]
    end

    J3 -.-> I2
    K3 -.-> J1
```

### Tabel Langkah 14.3

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Penyusunan Draf SPK/RKK** | Sekretariat Komite | Rekomendasi komite & katalog tindakan | Draf SPK dan lampiran RKK sistem | Setiap tindakan berstatus Level 3 wajib ditetapkan nama dokter supervisor pendampingnya (`VR-CRD-05`). |
| **2. Paraf Direktur Pelayanan** | Direktur Pelayanan | Draf SPK/RKK sistem | Paraf persetujuan manajerial | Direktur Pelayanan memvalidasi kesesuaian penugasan dengan peta kebutuhan layanan rumah sakit. |
| **3. Pengesahan Dirut** | Direktur Utama RS | Draf SPK/RKK terparaf | SPK/RKK sah bertanda tangan digital Direktur Utama | Pengesahan SPK/RKK tidak dapat didelegasikan ke pihak lain sesuai *Hospital Bylaws* (`HRD-DEC-148`). |
| **4. Penerbitan Dokumen** | Document Auto-Number Engine | Data pengesahan Dirut | Berkas PDF resmi ber-QR Code & nomor registrasi SPK gapless | Sistem mengunci status dokumen menjadi `Active` dan mencatat masa berlaku efektif 3 tahun. |
| **5. Distribusi & Integrasi** | Background Integration Bus | File PDF SPK/RKK sah & dataset RKK | Tersimpan di e-Dossier Sub-02, rilis ke ESS nakes, sinkron ke SIMRS EMR | Jika sinkronisasi SIMRS gagal, worker mengulang otomatis via event bus dengan retry ber-backoff (`INT-HR-69`). |

---

## 14.4 Alur Penegakan Batas Kewenangan Klinis SIMRS/EMR Gate (`INT-HR-69`), Hard-Stop & Life-Saving Override

Diagram ini menggambarkan arsitektur penegakan wewenang klinis secara real-time saat dokter/nakes menjalankan order tindakan di SIMRS/EMR, resolusi final [`HRD-DEC-005`] & [`HRD-DEC-149`] (Hard-Stop bertingkat), penanganan order supervisi (*Pending Co-Signature*), dan jalur khusus penyelamatan nyawa (*Life-Saving Emergency Override*) ber-audit komite.

```mermaid
flowchart TD
    subgraph StafKlinisSIMRS["Dokter / Tenaga Medis di SIMRS / EMR"]
        M1["Buka Menu Pasien: Input Order Tindakan / Resep / Jadwal Operasi"] --> M2["Pilih Butir Prosedur Klinis (Contoh: Tindakan Laparoskopi / Kemoterapi)"]
        M2 --> M3["Klik Tombol 'Simpan & Eksekusi Order'"]
        M7["Order Gagal: Muncul Dialog Hard-Stop (Akses Ditolak)"]
        M9["Order Diterima Sementara: Menunggu Persetujuan Supervisor (Pending Co-Signature)"]
        M10["Order Berhasil Diproses Langsung (Mandiri Level 4)"]
        M12["Klik Tombol 'Darurat Nyawa (Life-Saving Override)' & Isi Alasan Klinis Cito"]
        M14["Order Darurat Berhasil Dieksekusi dengan Audit Flag Merah"]
    end

    subgraph SIMRSEmrGate["Gerbang Integrasi Penegakan RKK (INT-HR-69 Gate Service)"]
        M3 --> N1["Kirim Request Validasi Cepat: DoctorId, ProcedureCode, UnitId"]
        N1 --> N2{"Cek Status SIP & SPK/RKK di Redis Cache (Latensi < 50ms)"}
        N2 -- "SIP Expired / SPK Suspended" --> N3["Return Penolakan: Lisensi / SPK Kedaluwarsa"]
        N2 -- "SIP & SPK Aktif" --> N4{"Evaluasi Derajat Wewenang Butir Prosedur"}
        N4 -- "Level 4 (Mandiri)" --> N5["Return Status: ALLOW_INDEPENDENT"]
        N4 -- "Level 3 (Supervisi)" --> N6["Return Status: ALLOW_UNDER_SUPERVISION (Need Co-Sign)"]
        N4 -- "Level 1-2 (Tidak Berwenang)" --> N7["Return Status: DENIED_NOT_PRIVILEGED"]
        N3 & N7 --> M7
        N5 --> M10
        N6 --> M9
    end

    subgraph SupervisorDokter["Dokter Supervisor Senior (Pendamping)"]
        M9 --> O1["Terima Notifikasi Order Menunggu Co-Signature"]
        O1 --> O2{"Persetujuan Supervisi Klinis dalam 24 Jam?"}
        O2 -- "Tolak Order" --> O3["Order Dibatalkan & Pasien Diberitahu"]
        O2 -- "Setuju (Co-Signed)" --> O4["Order Aktif Penuh & Prosedur Dijalankan"]
    end

    subgraph EmergencyOverrideHub["Audit Penyelamatan Nyawa (KMKP & Komite Medik)"]
        M7 --> M11{"Apakah Kondisi Pasien Gawat Darurat Mengancam Jiwa (Cito)?"}
        M11 -- "Bukan Cito / Pasien Stabil" --> M13["Order Tetap Dibatalkan (Patuhi RKK)"]
        M11 -- "Ya, Cito Darurat Mengancam Nyawa" --> M12
        M12 --> P1["Bypass Hard-Stop SIMRS & Catat Log Audit Darurat di TrxClinicalPrivilegeEmergencyOverrideLog"]
        P1 --> P2["Terbitkan Alert Otomatis ke Ketua Komite Medik & Tim Mutu KMKP dalam 24 Jam"]
        P2 --> P3["Komite Medik & KMKP Sidangkan Laporan Klinis dalam 7 Hari Kerja"]
        P1 --> M14
    end
```

### Tabel Langkah 14.4

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Request Validasi RKK** | SIMRS/EMR Service | `DoctorId`, `ProcedureCode`, `UnitId` | Panggilan API `POST /api/v1/corporate/hrd/clinical-privileges/validate-gate` | Layanan merespons dari cache terdistribusi (Redis) dengan target latensi < 50ms (`HRD-ASM-45`). |
| **2. Pengecekan SIP & SPK** | Gate Service | Tanggal hari ini vs masa berlaku SIP & SPK | Status validasi lisensi aktif | Jika SIP kedaluwarsa atau SPK suspended, transaksi seketika ditolak dengan pesan jelas (`VR-CRD-06`). |
| **3. Evaluasi Derajat Wewenang** | Gate Service | Kode prosedur vs daftar tindakan sah RKK | Keputusan: `ALLOW_INDEPENDENT`, `ALLOW_UNDER_SUPERVISION`, atau `DENIED` | Resolusi final `HRD-DEC-005` & `HRD-DEC-149`: Hard-Stop tegas untuk tindakan tidak berwenang. |
| **4. Alur Co-Signature Supervisi** | Dokter Supervisor | Notifikasi order tindakan di bawah supervisi | Tanda tangan digital persetujuan supervisor | Batas waktu persetujuan maksimal 24 jam; jika melebihi batas waktu, order tindakan otomatis kedaluwarsa (`VR-CRD-07`). |
| **5. Life-Saving Override** | Staf Klinis Cito | Justifikasi klinis darurat penyelamatan jiwa | Tindakan darurat tereksekusi, log audit terbit | Penyalahgunaan tombol darurat untuk kasus elektif dikenakan sanksi disiplin etik Komite Medik (`VR-CRD-08`). |
