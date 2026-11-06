# Alur Proses Bisnis Sub-Modul 13: HR Service Management & Request (Layanan Administrasi, Helpdesk SDM & Penerbitan Dokumen Kepegawaian RS)

Dokumen ini mendefinisikan seluruh diagram alur proses bisnis (*flowchart*) untuk **Sub-Modul 13 (Slice `S-D4`)** yang mengatur otomasi layanan helpdesk SDM, manajemen SLA berjenjang kebutuhan rumah sakit, penerbitan dokumen resmi ber-QR Code publik dan ber-PIN finansial, pemutakhiran data master kepegawaian otomatis, serta pengarsipan abadi *e-Dossier*.

---

## 13.1 Alur Pengajuan Tiket Helpdesk SDM, Penugasan Antrean & Monitoring SLA 3-Tier

Diagram ini menggambarkan siklus pengajuan tiket layanan SDM oleh staf medis/non-medis via ESS, validasi kelengkapan berkas, penentuan tier SLA (Cito 4 jam, Standar 24 jam, Khusus 72 jam), peringatan dini 75% durasi, dan eskalasi otomatis ke Manajer SDM jika *overdue* sesuai [`HRD-DEC-141`](../00-interview-decisions.md#L91-L103).

```mermaid
flowchart TD
    subgraph StafPemohon["Staf Medis / Non-Medis (Pemohon ESS)"]
        A1["Buka Menu ESS: Buat Tiket Layanan SDM"] --> A2["Pilih Kategori & Jenis Layanan"]
        A2 --> A3{"Apakah Kategori Cito / Darurat Medis?"}
        A3 -- "Ya" --> A4["Unggah Disposisi Cito Kepala Ruangan / Komite Medik"]
        A3 -- "Tidak" --> A5["Isi Rincian Permohonan & Unggah Berkas Persyaratan"]
        A4 --> A5
        A5 --> A6["Submit Tiket Permohonan (Status: Submitted)"]
        A14["Terima Notifikasi Tiket Selesai / Dokumen Terbit"]
    end

    subgraph MesinSlaHelpdesk["Mesin SLA & Background Service (SLA Engine)"]
        A6 --> B1{"Evaluasi Tingkat Prioritas Layanan"}
        B1 -- "Cito Klinis" --> B2["Set SLA Target: 4 Jam Kerja"]
        B1 -- "Standar Admin" --> B3["Set SLA Target: 24 Jam Kerja"]
        B1 -- "Khusus Pimpinan" --> B4["Set SLA Target: 72 Jam Kerja"]
        B2 & B3 & B4 --> B5["Mulai Timer SLA & Masukkan ke Antrean Layanan"]
        B5 --> B6{"Pengecekan Rutin Timer SLA (Tiap 15 Menit)"}
        B6 -- "Durasi >= 75%" --> B7["Kirim Alert Peringatan Kuning ke Supervisor HR"]
        B6 -- "Durasi >= 100% (Overdue)" --> B8["Picu Status Overdue & Auto-Eskalasi ke Manajer SDM"]
        B6 -- "Tiket Selesai Tepat Waktu" --> B9["Catat Waktu Penyelesaian & Skor SLA Hijau"]
    end

    subgraph PetugasHRDesk["Petugas HR Service Desk / Specialist"]
        B5 --> C1["Ambil Tiket dari Antrean (Assign to Me: InReview)"]
        C1 --> C2{"Apakah Berkas Pendukung Lengkap & Valid?"}
        C2 -- "Kurang / Tidak Sah" --> C3["Buka Utas Klarifikasi (Status: WaitingStaffResponse)"]
        C2 -- "Lengkap & Sah" --> C4["Proses Permohonan / Teruskan ke Alur Terkait"]
        C4 --> C5["Selesaikan Tiket (Status: Resolved / Closed)"]
        C5 --> B9
    end

    subgraph SupervisorManajer["Supervisor HR & Manajer SDM"]
        B7 -.-> D1["Pantau Antrean Kritis di Dashboard SLA"]
        B8 -.-> D2["Terima Notifikasi Eskalasi & Re-assign Tiket Prioritas"]
    end

    C3 -.-> A5
    C5 --> A14
```

### Tabel Langkah 13.1

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Pengajuan Tiket** | Staf Pemohon (ESS) | Kategori layanan, deskripsi kebutuhan, berkas lampiran pendukung | Tiket tersimpan status `Submitted` | Sistem memvalidasi kelengkapan berkas wajib; jika belum ada, tombol submit terkunci (`VR-HRS-01`). |
| **2. Inisiasi SLA** | Background SLA Engine | Jenis layanan & tingkat urgensi | `sla_target_hours`, `sla_deadline`, timer aktif | Menggunakan jam operasional resmi rumah sakit (mengecualikan hari libur untuk layanan non-cito). |
| **3. Monitoring & Alert** | Background SLA Engine | Status tiket & sisa waktu SLA | Notifikasi peringatan 75% & eskalasi 100% | Jika supervisor tidak merespons peringatan 75%, tiket otomatis naik tingkat ke dashboard Manajer SDM (`VR-HRS-02`). |
| **4. Klarifikasi Berkas** | Petugas HR Desk | Catatan klarifikasi pada tiket | Status `WaitingStaffResponse`, timer SLA ditangguhkan (*pause*) | Pemohon menerima notifikasi ESS & email; timer kembali berjalan saat staf mengunggah berkas perbaikan. |
| **5. Resolusi & Penutupan** | Petugas HR Desk | Hasil pemrosesan & tautan output | Status `Resolved` / `Closed` | Pemohon memberikan rating kepuasan layanan bintang 1-5 di portal ESS. |

---

## 13.2 Alur Penerbitan Surat Resmi Ber-QR Code Publik & Proteksi PIN 4-Digit Gaji

Diagram ini menggambarkan penerbitan dokumen resmi rumah sakit (Surat Keterangan Kerja, Visa, Salary Certificate KPR) menggunakan penomoran otomatis anti-lompat, penarikan gaji otomatis dari Sub-10, proteksi PIN finansial, dan verifikasi portal publik sesuai [`HRD-DEC-140`](../00-interview-decisions.md#L78-L90), [`HRD-DEC-142`](../00-interview-decisions.md#L104-L116), dan [`HRD-DEC-144`](../00-interview-decisions.md#L130-L142).

```mermaid
flowchart TD
    subgraph PemohonESS["Staf Pemohon (ESS)"]
        E1["Pilih Permohonan Surat Resmi (Misal: Salary Certificate / KPR)"] --> E2["Isi Tujuan Surat (Nama Bank, Keperluan, Periode)"]
        E2 --> E3["Submit Permohonan Dokumen"]
        E10["Terima Dokumen Digital PDF Ber-QR Code di Menu Dokumen Saya"]
        E11["Terima 4-Digit PIN Rahasia via Notifikasi Privat ESS"]
    end

    subgraph GeneratorDokumen["Mesin Penerbitan Dokumen (Document Engine)"]
        E3 --> F1{"Apakah Jenis Dokumen Finansial / Gaji?"}
        F1 -- "Ya" --> F2["Tarik Riwayat Gaji Final Sub-10 via INT-HR-56 (Dilarang Input Manual)"]
        F1 -- "Tidak" --> F3["Tarik Variabel Profil Sub-02 (Nama, NIP, Jabatan, Unit)"]
        F2 --> F3
        F3 --> F4["Generate Draf Dokumen Berbasis Template Token Dinamis"]
        F4 --> F5["Kirim Draf ke Antrean Persetujuan Pejabat Berwenang"]
        F8["Ambil Nomor Surat Seri Unik (Gapless Auto-Numbering Engine)"] --> F9["Tempel QR Code Verifikasi, Stempel Digital & Enkripsi PIN"]
        F9 --> F10["Simpan PDF Resmi ke Storage & Terbitkan Status Issued"]
        F10 --> E10 & E11
    end

    subgraph OtorisasiHR["Supervisor C&B / Manajer SDM"]
        F5 --> G1{"Review Akurasi Draf & Data Gaji"}
        G1 -- "Ditolak / Data Janggal" --> G2["Tolak Permohonan Disertai Alasan Resmi"]
        G1 -- "Disetujui" --> G3["Bubuhi Tanda Tangan Digital / Approval Token"]
        G3 --> F8
    end

    subgraph PortalVerifikasiPublik["Portal Verifikasi Eksternal (/verify/document/{hash})"]
        H1["Pihak Ketiga (Bank / Kedutaan) Scan QR Code Dokumen"] --> H2["Sistem Validasi Hash SHA-256 Dokumen"]
        H2 --> H3{"Apakah Status Dokumen Masih Aktif & Sah?"}
        H3 -- "Dicabut / Revoked" --> H4["Tampilkan Banner Merah: DOKUMEN INI TELAH DICABUT"]
        H3 -- "Kedaluwarsa / Expired" --> H5["Tampilkan Banner Oranye: MASA BERLAKU DOKUMEN TELAH HABIS"]
        H3 -- "Sah & Aktif" --> H6["Tampilkan Metadata Legal: No Surat, Nama Staf, Pejabat & Tanggal Terbit"]
        H6 --> H7{"Apakah Dokumen Mengandung Informasi Finansial?"}
        H7 -- "Ya" --> H8["Nominal Gaji Disamarkan (Rp ••••••••) - Form Input PIN Terbuka"]
        H7 -- "Tidak" --> H9["Tampilkan Isi Keterangan Lengkap Dokumen"]
        H8 --> H10["Verifikator Input 4-Digit PIN dari Pemilik Dokumen"]
        H10 --> H11{"Validasi PIN via BCrypt Hash"}
        H11 -- "PIN Salah" --> H12["Peringatan: PIN Salah, Nominal Tetap Disamarkan"]
        H11 -- "PIN Benar" --> H13["Buka Rincian Nominal Gaji Pokok & Tunjangan Sah"]
    end

    F10 -.-> H1
```

### Tabel Langkah 13.2

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Permohonan Surat** | Staf Pemohon (ESS) | Jenis dokumen, tujuan penggunaan, nama institusi penerima | Draf permohonan tersimpan | Sistem memvalidasi kelayakan penerbitan (misal: nakes minimal telah melewati masa orientasi 1 bulan). |
| **2. Tarik Data Otomatis** | Document Engine | ID Pegawai & ID Jenis Dokumen | Variabel token dinamis terisi (`{{NamaLengkap}}`, `{{GajiNetto}}`, dll.) | Jika data gaji Sub-10 belum *finalized*, sistem menolak pembuatan draf dengan pesan instruksi (`VR-HRS-05`). |
| **3. Otorisasi Finansial** | Supervisor C&B / Manajer SDM | Draf surat berparameter gaji | Persetujuan digital ber-timestamp | Pemohon dilarang menyetujui suratnya sendiri (*Separation of Duties*). |
| **4. Penomoran & Stempel QR** | Gapless Auto-Numbering Engine | Kode naskah dinas, bulan Romawi, tahun | Nomor surat sah (misal: `042/RSQ-SDM/XI/2026/0128`), file PDF ber-QR | Transaksi database atomik mencegah duplikasi nomor atau nomor lompat (*gapless sequence*). |
| **5. Verifikasi Publik & PIN** | Pihak Ketiga (Bank/Kedutaan) | Scan kamera QR code, 4-digit PIN rahasia | Tampilan validitas metadata & pembukaan nominal gaji | Salah input PIN dibatasi 3 kali per jam untuk menangkal brute force (`VR-HRS-12`). |

---

## 13.3 Alur Permohonan Pemutakhiran Data Profil Master Kepegawaian Otomatis

Diagram ini menggambarkan alur permohonan staf untuk memperbarui rekening bank payroll, perubahan status pernikahan, atau kontak darurat yang diverifikasi keabsahan berkasnya oleh tim SDM, lalu sistem secara otomatis memperbarui master data kepegawaian Sub-02 tanpa ketik ulang sesuai [`HRD-DEC-143`](../00-interview-decisions.md#L117-L129).

```mermaid
flowchart TD
    subgraph StafESS["Staf Nakes / Non-Medis (ESS)"]
        J1["Buka Menu: Pemutakhiran Data Profil"] --> J2["Pilih Komponen: Rekening Payroll / Status Nikah / Alamat"]
        J2 --> J3["Input Nilai Data Baru (No Rekening, Nama Bank, dll.)"]
        J3 --> J4["Wajib Unggah Berkas Bukti Sah (Scan Buku Tabungan / KTP / Buku Nikah)"]
        J4 --> J5["Submit Permohonan Pemutakhiran Data"]
        J11["Terima Notifikasi: Data Profil Kepegawaian Berhasil Diperbarui"]
    end

    subgraph PetugasVerifikasiHR["Petugas HR Service / Personalia"]
        J5 --> K1["Buka Antrean Tiket Pemutakhiran Data"]
        K1 --> K2["Bandingkan Data Baru dengan Dokumen Bukti Lampiran"]
        K2 --> K3{"Apakah Bukti Lampiran Jelas, Sah & Sesuai Rekening Pemohon?"}
        K3 -- "Tidak Sesuai / Buram" --> K4["Tolak Permohonan Disertai Catatan Revisi Berkas"]
        K3 -- "Sah & Terverifikasi" --> K5["Klik Setujui & Mutakhirkan Master Data"]
    end

    subgraph BackendSync["Backend System Integration (INT-HR-65)"]
        K5 --> L1["Buka Transaksi Database Terisolasi"]
        L1 --> L2["Simpan Snapshot Data Lama ke Log HrdProfileChangeHistory"]
        L2 --> L3["Eksekusi Pembaruan Data pada Sub-02 (MstWorkforceProfile / BankDetail)"]
        L3 --> L4{"Apakah Pembaruan Master Berhasil?"}
        L4 -- "Gagal / Kendala DB" --> L5["Rollback Transaksi & Catat Error Audit Log"]
        L4 -- "Sukses" --> L6["Commit Transaksi & Tandai Tiket Resolved"]
        L6 --> J11
    end

    K4 -.-> J1
```

### Tabel Langkah 13.3

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Input Perubahan Data** | Staf Pemohon (ESS) | Field yang ingin diubah (misal rekening payroll BCA ke Mandiri) | Form perubahan & lampiran buku tabungan | Nama pada buku tabungan wajib diverifikasi kecocokannya dengan nama pegawai pada KTP. |
| **2. Verifikasi Dokumen** | Petugas HR Service | Scan berkas & data pemohon | Catatan approval verifikasi | Petugas memeriksa keaslian cap/buku rekening untuk mencegah rekening pihak ketiga fiktif. |
| **3. Audit Snapshot** | Backend Integration | Data lama sebelum perubahan | Baris riwayat perubahan tercatat abadi | Memastikan data lampau dapat direkonstruksi bila terjadi sengketa transfer payroll bulanan. |
| **4. Pembaruan Otomatis** | `IWorkforceProfileSyncService` | ID Pegawai & Payload data baru | `MstWorkforceProfile` dan tabel bank detail ter-update | Eksekusi atomik; kegagalan menulis database langsung memicu rollback tanpa merusak status tiket. |

---

## 13.4 Alur Pengarsipan Digital Abadi E-Dossier & Protokol Pencabutan Dokumen Sah (Revokasi)

Diagram ini menggambarkan penyimpanan otomatis seluruh dokumen yang diterbitkan ke repositori berkas kepegawaian (*Employee E-Dossier* Sub-02) serta tata cara pencabutan keabsahan dokumen (*revocation*) bila ditemukan kesalahan administratif atau manipulasi data sesuai [`HRD-DEC-145`](../00-interview-decisions.md#L143-L155).

```mermaid
flowchart TD
    subgraph PenerbitanDokumen["Sistem Penerbitan Dokumen Resmi"]
        M1["Dokumen Resmi Berhasil Diterbitkan (Status: Issued)"] --> M2["Kirim Salinan Metadata & File PDF ke Repositori E-Dossier Sub-02"]
        M2 --> M3["Kaitkan Dokumen ke Kategori Folder E-Dossier (Surat Resmi Kepegawaian)"]
        M3 --> M4["Tersedia di Portal ESS Tab Dokumen Saya (Unduh Kapan Saja)"]
    end

    subgraph PermasalahanHukum["Pemicu Revokasi Dokumen (Insiden Administrasi / Hukum)"]
        N1["Laporan Temuan: Pemalsuan Data / Pembatalan Beasiswa / Kesalahan Jabatan"] --> N2["Pemeriksaan Khusus oleh Manajer SDM / Legal Rumah Sakit"]
        N2 --> N3{"Apakah Dokumen Perlu Dicabut Keabsahannya?"}
        N3 -- "Tidak" --> N4["Arsip Tetap Aktif Normal"]
        N3 -- "Ya" --> N5["Inisiasi Perintah Pencabutan Dokumen (Revocation)"]
    end

    subgraph EksekusiRevokasi["Mesin Pencabutan Keabsahan (Revocation Engine)"]
        N5 --> P1["Input Alasan Resmi Pencabutan & Nomor Berita Acara"]
        P1 --> P2["Ubah Status Dokumen Menjadi Revoked (Dilarang Hapus Berkas Fisik/DB)"]
        P2 --> P3["Catat Waktu Pencabutan & User Eksekutor ke Audit Trail"]
        P3 --> P4["Nonaktifkan Unduhan di Portal ESS & Pasang Cap Merah REVOKED"]
        P4 --> P5["Kirim Notifikasi Resmi ke Pegawai Pemilik Dokumen"]
    end

    subgraph PihakLuarScan["Pihak Ketiga Memindai QR Code Dokumen Lama"]
        Q1["Bank / Kedutaan Memindai Lembar Kertas Dokumen Fisik"] --> Q2["Akses Portal /verify/document/{hash}"]
        Q2 --> Q3["Sistem Mendeteksi Status: REVOKED"]
        Q3 --> Q4["Tampilkan Peringatan Merah Tegas: DOKUMEN TELAH DICABUT & TIDAK BERLAKU"]
        Q4 --> Q5["Tampilkan Tanggal Pencabutan & Nomor Berita Acara Pembatalan"]
    end

    P4 -.-> Q1
```

### Tabel Langkah 13.4

| Langkah | Pelaku | Masukan | Keluaran | Penanganan Bila Gagal / Hambatan |
| :--- | :--- | :--- | :--- | :--- |
| **1. Pengarsipan E-Dossier** | Background Archiver | Dokumen status `Issued` & path PDF | Rekam berkas digital pada `MstWorkforceProfileDocument` Sub-02 | Berkas PDF dienkripsi dan diberi watermark hak cipta resmi RS Quilvian (`VR-HRS-15`). |
| **2. Pengambilan Mandiri** | Staf Pegawai (ESS) | Login ESS & Menu Dokumen Saya | File PDF bertanda tangan digital | Menghilangkan antrean fisik nakes di loket HR untuk meminta salinan ulang surat. |
| **3. Perintah Revokasi** | Manajer SDM / Direksi | ID Dokumen terbit, alasan pembatalan, nomor BAP | Status dokumen berubah menjadi `Revoked` | Dokumen asli tidak dihapus dari harddisk/database guna mempertahankan rekam jejak forensik (`HRD-DEC-145`). |
| **4. Peringatan Publik** | Portal Verifikasi Publik | Hash URL QR Code dokumen tercabut | Tampilan peringatan merah penolakan keabsahan | Pihak bank/luar langsung mengetahui bahwa dokumen fisik tersebut sudah tidak berlaku lagi secara hukum. |
