# Alur Proses Bisnis — Pemrosesan & Serah Terima Penggajian (Payroll Processing & Finance Handoff)

Dokumen ini memetakan alur kerja operasional siklus penggajian bulanan staf rumah sakit pada **Sub-Modul 10 (Payroll Management — Slice `S-B5`)**. Alur proses dirancang untuk menjamin akurasi mutlak perhitungan finansial, pemisahan wewenang bertingkat (*3-Tier Segregation of Duties*), deteksi proaktif variansi gaji anomali, kepatuhan perpajakan PMK 168/2023 dan BPJS, perlindungan batas wewenang serah terima ke Departemen Keuangan (*Finance Handoff* sesuai `HRD-DEC-009` & `HRD-DEC-126`), serta kerahasiaan slip gaji digital mandiri pegawai (*ESS Payslip Security* sesuai `HRD-DEC-127`).

---

## 1. Alur Pokok Siklus Penggajian Bulanan Rumah Sakit (Happy Path)

Diagram ini menggambarkan alur kerja standar penggajian dari inisiasi periode hingga penyerahan berkas final ke Keuangan dan penerbitan slip gaji.

```mermaid
flowchart TD
  subgraph HR_OFFICER["Petugas Penggajian (Payroll Officer)"]
    A["Mulai Siklus Penggajian Bulanan"] --> B["Verifikasi Kunci Prasyarat Waktu Kerja<br/>(Presensi, Cuti, Lembur tgl 20)"]
    B --> C["Inisiasi Periode & Tarik Masukan Snapshot"]
    C --> D["Eksekusi Mesin Perhitungan Gaji Otomatis"]
    D --> E["Evaluasi Pemeriksaan Variansi Gaji"]
    E --> F["Pengisian Justifikasi Catatan Variansi"]
    F --> G["Kirim Paket Gaji untuk Tinjauan Manajer"]
  end

  subgraph HR_MANAGER["Manajer SDM (HR Manager)"]
    G --> H["Pemeriksaan Administratif & Uji Petik Sampling"]
    H --> I["Persetujuan Tinjauan Manajerial SDM"]
  end

  subgraph DIRECTORS["Direktur SDM & Keuangan"]
    I --> J["Audit Eksekutif & Otorisasi Penguncian Final"]
    J --> K["Pembentukan Paket Serah Terima Terkunci"]
  end

  subgraph FINANCE_DEPT["Departemen Keuangan (Finance / Treasury)"]
    K --> L["Penerimaan Berkas Serah Terima Gaji"]
    L --> M["Proses Pembayaran Transfer Bank & Kas Negara"]
    M --> N["Penerbitan Sinyal Konfirmasi Pembayaran Selesai"]
  end

  subgraph EMPLOYEE_PORTAL["Portal Mandiri Pegawai (ESS)"]
    N --> O["Pembukaan Hak Akses Slip Gaji Digital"]
    O --> P["Pegawai Membuka Rincian Slip Gaji via PIN"]
  end
```

### Tabel Langkah: Alur Pokok Penggajian Bulanan

| No | Langkah Kerja | Pelaku (Aktor) | Masukan (Input) | Keluaran (Output) | Penanganan Bila Gagal / Terkendala |
| :---: | :--- | :--- | :--- | :--- | :--- |
| **1** | Verifikasi Kunci Prasyarat | Petugas Payroll | Parameter periode bulan berjalan | Konfirmasi status penutupan Sub-07, 08, 09 | Jika masih ada SPKL lembur atau cuti berstatus *Pending*, proses kalkulasi diblokir (`HRD-DEC-122`); petugas menghubungi kepala unit terkait |
| **2** | Inisiasi & Snapshot Masukan | Petugas Payroll | Daftar master pegawai aktif & penugasan gaji | Snapshot masukan kehadiran, lembur, dan cuti | Jika data master pegawai tidak valid (misal PTKP belum terdaftar), sistem menandai baris pegawai untuk dilengkapi |
| **3** | Eksekusi Mesin Perhitungan | Sistem Payroll | Snapshot masukan & formula tarif | Draf rincian pendapatan, potongan pajak, BPJS & gaji bersih | Jika terjadi kegagalan kalkulasi formula, proses di-rollback ke status Draf dan mencatat log teknis |
| **4** | Tinjauan Variansi Anomali | Petugas Payroll | Draf gaji vs riwayat bulan sebelumnya | Daftar pegawai bertanda *VarianceAlert* ($\ge 20\%$) | Petugas wajib menelusuri sumber lonjakan/penurunan gaji dan mengisi catatan justifikasi resmi |
| **5** | Pengajuan Tinjauan Manajer | Petugas Payroll | Draf gaji lengkap berjustifikasi | Status paket berubah menjadi Menunggu Tinjauan HR | Ditolak oleh sistem jika masih ada baris variansi anomali tanpa catatan penjelasan |
| **6** | Persetujuan Manajerial SDM | Manajer SDM | Draf gaji, catatan variansi, laporan rekap | Persetujuan administratif manajer | Manajer dapat mengembalikan paket ke Petugas Payroll dengan catatan perbaikan (*Request Revision*) |
| **7** | Otorisasi Final Direksi | Direktur SDM / Keuangan | Ringkasan eksekutif beban gaji rumah sakit | Otorisasi penguncian permanen (*Final Lock*) | Dilarang merangkap persetujuan (*anti-self-approval*); jika Direktur berhalangan, dialihkan ke pejabat wewenang pengganti |
| **8** | Pembentukan Serah Terima | Sistem Payroll | Draf gaji final terkunci | Paket berkas serah terima ber-checksum SHA-256 | Berkas tersimpan immutable dan notifikasi otomatis dikirimkan ke Departemen Keuangan |
| **9** | Transfer Bank & Konfirmasi | Finance / Kasir | Paket serah terima gaji HR | Bukti transfer bank & sinyal konfirmasi selesai | Jika ada kendala likuiditas kas, Finance menjalankan protokol penolakan batch resmi (`HRD-DEC-126`) |
| **10** | Pembukaan Slip Gaji ESS | Portal Pegawai ESS | Sinyal konfirmasi bayar / tgl gajian | Tampilan slip gaji digital berproteksi PIN | Slip gaji tetap terkunci hingga status pembayaran terkonfirmasi atau jadwal rilis tercapai |

---

## 2. Alur Pemeriksaan & Justifikasi Variansi Anomali Gaji

Diagram ini menggambarkan alur pengendalian kualitas otomatis untuk memitigasi risiko kesalahan bayar (*overpayment/underpayment*) sebelum diajukan ke atasan.

```mermaid
flowchart TD
  subgraph VARIANCE_DETECTION["Deteksi Variansi Sistem"]
    A["Mulai Evaluasi Variansi Gaji"] --> B["Bandingkan Net Pay Tiap Pegawai vs Bulan Lalu"]
    B --> C{"Deviasi Gaji Bersih<br/>>= 20% atau >= Rp 2.000.000?"}
    C -- Tidak --> D["Tandai Status Variansi Normal"]
    C -- Ya --> E["Tandai Status Peringatan: VarianceAlert"]
    E --> F["Tampilkan Indikator Warna pada Lembar Kerja"]
  end

  subgraph OFFICER_INVESTIGATION["Investigasi Petugas Payroll"]
    F --> G["Petugas Memeriksa Rincian Komponen Pegawai"]
    G --> H{"Penyebab Lonjakan / Penurunan?"}
    H -- "Lembur Cito Medis" --> I["Input Catatan: Bukti Penugasan Darurat Pasien"]
    H -- "Potongan Pinjaman RS" --> J["Input Catatan: Rujukan Nomor Akad Pinjaman Sub-11"]
    H -- "Koreksi Kehadiran Lampau" --> K["Input Catatan: Rujukan SK Rekonsiliasi Retrospektif"]
    H -- "Kesalahan Input Data" --> L["Koreksi Nilai pada Dokumen Sumber & Hitung Ulang"]
    I --> M["Simpan Catatan Justifikasi Resmi"]
    J --> M
    K --> M
    L --> B
  end

  subgraph VALIDATION_GATE["Gerbang Pengajuan Sistem"]
    M --> N{"Seluruh Baris VarianceAlert<br/>Sudah Memiliki Justifikasi?"}
    N -- Belum --> O["Kunci Tombol Pengajuan (Validasi Ditolak)"]
    N -- Sudah --> P["Buka Tombol Pengajuan ke Manajer SDM"]
  end
```

### Tabel Langkah: Pemeriksaan & Justifikasi Variansi

| No | Langkah Kerja | Pelaku (Aktor) | Masukan (Input) | Keluaran (Output) | Tindakan Bila Gagal / Terkendala |
| :---: | :--- | :--- | :--- | :--- | :--- |
| **1** | Deteksi Variansi Komparatif | Sistem Payroll | Nilai gaji bersih periode ini & bulan lalu | Persentase selisih & selisih nominal | Jika pegawai baru (belum ada riwayat bulan lalu), sistem menggunakan batas komparasi terhadap standar gaji penugasan |
| **2** | Pemberian Label Peringatan | Sistem Payroll | Nilai deviasi $\ge 20\%$ atau $\ge$ Rp 2 jt | Label `VarianceAlert` & penanda visual kuning/merah | Pegawai bersangkutan masuk dalam filter antrean prioritas investigasi |
| **3** | Pemeriksaan Akar Masalah | Petugas Payroll | Rincian lembar kerja komponen gaji | Identifikasi penyebab lonjakan/penurunan | Petugas mencocokkan riwayat lembur cito, mutasi cuti, atau pemotongan kasbon |
| **4** | Pengisian Catatan Formal | Petugas Payroll | Alasan operasional & nomor dokumen bukti | Catatan justifikasi tersimpan pada riwayat audit | Karakter catatan minimal 15 karakter dan dilarang mengisi teks tidak bermakna (*generic text*) |
| **5** | Validasi Gerbang Pengajuan | Sistem Payroll | Status justifikasi seluruh baris anomali | Status kelayakan kirim paket gaji | Jika masih ada baris variansi tanpa justifikasi, pengajuan diblokir dengan pesan error eksplisit |

---

## 3. Alur Serah Terima ke Keuangan & Penolakan/Rollback Terkendali

Diagram ini mengatur interaksi resmi antara sistem HR dan Departemen Keuangan (*Finance*) sesuai batas tanggung jawab `HRD-DEC-009` dan `HRD-DEC-126`.

```mermaid
flowchart TD
  subgraph HR_SUBMISSION["Sisi HRD — Otorisasi & Penyerahan"]
    A["Direksi Menyetujui Otorisasi Final"] --> B["Kunci Seluruh Transaksi Menjadi Immutable"]
    B --> C["Hasilkan Berkas Serah Terima Ber-Checksum SHA-256"]
    C --> D["Kirim Notifikasi Paket Siap ke Finance"]
  end

  subgraph FINANCE_VERIFICATION["Sisi Keuangan — Audit Kas & Perbankan"]
    D --> E["Finance Membuka Berkas Serah Terima Gaji"]
    E --> F["Audit Kesiapan Dana Kas & Validasi Rekening Bank"]
    F --> G{"Ditemukan Kendala Pembayaran?"}
    G -- Tidak --> H["Finance Eksekusi Pembayaran Transfer Bank"]
    H --> I["Kirim Sinyal Pembayaran Selesai (PaymentConfirmed)"]
  end

  subgraph FINANCE_REJECTION["Sisi Keuangan — Penolakan Resmi (Rollback)"]
    G -- Ya --> J["Finance Menginput Alasan Penolakan Tertulis"]
    J --> K["Kirim Sinyal Penolakan Resmi (RejectedByFinance)"]
  end

  subgraph HR_ROLLBACK["Sisi HRD — Penanganan Rollback Terkendali"]
    K --> L["Turunkan Status Run Menjadi NeedCorrection"]
    L --> M["Buka Kunci Data dengan Catatan Audit Pembatalan"]
    M --> N["Petugas Payroll Melakukan Perbaikan Data Terkait"]
    N --> O["Proses Ulang Alur Otorisasi dari Awal"]
  end
```

### Tabel Langkah: Serah Terima & Rollback Keuangan

| No | Langkah Kerja | Pelaku (Aktor) | Masukan (Input) | Keluaran (Output) | Tindakan Bila Gagal / Terkendala |
| :---: | :--- | :--- | :--- | :--- | :--- |
| **1** | Pembekuan Berkas Serah Terima | Sistem HR | Draf gaji final disetujui Direksi | Paket serah terima terenkripsi ber-checksum SHA-256 | Kegagalan pembuatan berkas memicu peringatan sistem dan data tetap terkunci di status Final |
| **2** | Audit Rekening & Likuiditas | Petugas Finance | Paket data transfer bank pegawai | Hasil verifikasi kliring & ketersediaan dana kas | Petugas Finance memvalidasi format nomor rekening tujuan bank mitra RS |
| **3** | Eksekusi Transfer Pembayaran | Departemen Finance | Berkas pembayaran gaji | Bukti transfer bank & pembayaran pajak kas negara | Menjadi tanggung jawab penuh Departemen Keuangan di luar sistem HR (`HRD-DEC-009`) |
| **4** | Konfirmasi Selesai Bayar | Departemen Finance | Nomor referensi transaksi perbankan | Status `PaymentConfirmed` pada sistem HR | Sistem HR membuka izin rilis slip gaji digital mandiri pegawai |
| **5** | Penolakan Batch Terkendali | Petugas Finance | Pilihan alasan & catatan tertulis kendala | Sinyal penolakan resmi `RejectedByFinance` | Wajib menyertakan alasan tertulis spesifik (misal: "Rekening 5 perawat tidak aktif" atau "Likuiditas dibagi 2 termin") |
| **6** | Pembukaan Kunci & Perbaikan | Petugas Payroll | Catatan penolakan dari Finance | Pembukaan status data & pencatatan jejak audit | Petugas memperbaiki nomor rekening atau mengalihkan pembayaran ke kasbon sementara |

---

## 4. Alur Akses Aman Slip Gaji Digital Mandiri (ESS) Ber-PIN

Diagram ini menggambarkan alur pegawai mengakses dokumen slip gaji pribadi dengan proteksi kerahasiaan berlapis (`HRD-DEC-127`).

```mermaid
flowchart TD
  subgraph TRIGGER_RELEASE["Gerbang Rilis Slip Gaji"]
    A["Jadwal Tanggal Gajian Resmi (tgl 25)<br/>atau Sinyal PaymentConfirmed"] --> B["Sistem Mengaktifkan Visibilitas Slip Gaji ESS"]
  end

  subgraph EMPLOYEE_ACCESS["Interaksi Pegawai di Portal ESS"]
    B --> C["Pegawai Membuka Menu Slip Gaji di Aplikasi ESS"]
    C --> D["Sistem Menampilkan Daftar Periode Penggajian"]
    D --> E["Pegawai Memilih Periode Tertentu"]
    E --> F["Sistem Memunculkan Dialog Masukkan PIN Keamanan"]
    F --> G["Pegawai Memasukkan 6-Digit PIN Rahasia Payroll"]
    G --> H{"Verifikasi PIN Berhasil?"}
  end

  subgraph SECURITY_PROTECTION["Penegakan Keamanan & Tampilan Dokumen"]
    H -- Salah --> I["Peringatan PIN Salah (Maksimal 3 Kali Percobaan)"]
    I --> J{"Percobaan >= 3 Kali?"}
    J -- Ya --> K["Kunci Akses Slip Gaji Sementara & Kirim Link Reset ke Email"]
    J -- Tidak --> F
    H -- Benar --> L["Buka Lembar Rincian Slip Gaji Digital di Layar"]
    L --> M["Opsi Unduh Berkas Dokumen PDF Resmi Ber-Watermark"]
  end
```

### Tabel Langkah: Akses Aman Slip Gaji Digital ESS

| No | Langkah Kerja | Pelaku (Aktor) | Masukan (Input) | Keluaran (Output) | Tindakan Bila Gagal / Terkendala |
| :---: | :--- | :--- | :--- | :--- | :--- |
| **1** | Evaluasi Gerbang Rilis | Sistem HR ESS | Tanggal sistem & status pembayaran run | Status ketersediaan dokumen (*Available/Locked*) | Jika Finance belum konfirmasi dan belum tanggal 25, tombol rincian slip gaji berstatus *Terkunci Sementara* |
| **2** | Pemilihan Periode Penggajian | Pegawai RS | Akun login aktif & daftar riwayat bulan | Permintaan buka dokumen slip gaji periode terpilih | Pegawai hanya dapat melihat data penggajian atas NIK miliknya sendiri |
| **3** | Permintaan Verifikasi PIN | Sistem HR ESS | Dialog modal sekunder | Input 6-digit angka PIN rahasia | Jika pegawai belum pernah membuat PIN, sistem mengarahkan ke formulir aktivasi PIN perdana |
| **4** | Validasi Autentikasi Sekunder | Sistem Keamanan ESS | Hash PIN tersimpan & PIN yang diinput | Status otorisasi pembacaan dokumen | Percobaan salah 3 kali berturut-turut memicu penguncian akun slip gaji selama 30 menit demi keamanan |
| **5** | Tampilan Rincian Gaji | Layar ESS Pegawai | Data terenkripsi komponen gaji | Tampilan visual pendapatan tetap, variabel, dan potongan | Layar dilengkapi tombol sembunyikan nominal (*toggle visibility*) untuk privasi di nurse station |
| **6** | Unduh Dokumen PDF Resmi | Pegawai RS | Tombol aksi cetak / unduh PDF | File PDF slip gaji ber-password & watermark resmi | PDF dilindungi kata sandi kombinasi tanggal lahir pegawai dan NIK |
