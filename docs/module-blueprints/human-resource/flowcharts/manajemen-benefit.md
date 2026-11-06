# Alur Proses Bisnis — Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit (Benefit Management)

Dokumen ini memetakan seluruh alur proses bisnis operasional untuk **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**. Alur proses dirancang untuk menjamin kepatuhan paket manfaat berjenjang 3-lapisan (Regulasi BPJS wajib, Asuransi Swasta Tambahan per grade, dan Subsidi Berobat Internal RS sesuai `HRD-DEC-134`), penegakan batas tanggungan keluarga inti sah (maksimal 1 pasangan + 3 anak hingga usia 21/25 tahun kuliah via verifikasi KK/Akta sesuai `HRD-DEC-135`), integrasi real-time loket registrasi dan kasir SIMRS untuk layanan berobat internal nakes di RS sendiri (`INT-HR-61` sesuai `HRD-DEC-136`), penegakan aturan Koordinasi Manfaat (*Coordination of Benefits* - COB) BPJS Kesehatan sebagai *Primary Payer* ber-SEP dan asuransi swasta sebagai *Secondary Payer* (`HRD-DEC-137`), perlindungan keselamatan kerja nakes sejak H+0 Onboarding dan aktivasi bertahap pasca-lulus masa percobaan 3 bulan (`HRD-DEC-138`), serta penonaktifan hak benefit otomatis pada tanggal efektif pemutusan hubungan kerja melalui gerbang Exit Clearance Sub-Modul 20 (`HRD-DEC-139`).

---

## 1. Alur Pendaftaran, Validasi Kelayakan & Verifikasi Tanggungan Keluarga di Portal ESS

Diagram ini memetakan alur pendaftaran data pasangan sah dan anak oleh pegawai rumah sakit melalui portal Employee Self-Service (ESS), pengecekan batasan kuota otomatis, verifikasi lampiran dokumen kependudukan oleh HR Benefit Officer, hingga status kepesertaan tanggungan aktif.

```mermaid
flowchart TD
  subgraph EMP["Pegawai Rumah Sakit (ESS Portal)"]
    A["Mulai: Buka Menu ESS Manfaat & Tanggungan"] --> B["Pilih Tambah Anggota Keluarga"]
    B --> C["Input Data Tanggungan: NIK, Nama, Hubungan, Tanggal Lahir"]
    C --> D["Unggah Berkas Wajib: Kartu Keluarga (KK), Akta Nikah / Akta Kelahiran"]
    D --> E{"Apakah Anak Berusia 21 - 25 Tahun?"}
    E -- "Ya (Anak Kuliah)" --> F["Unggah Surat Keterangan Mahasiswa Aktif dari Dekanat"]
    E -- "Tidak (Usia <= 21 / Pasangan)" --> G["Lanjut ke Review Data"]
    F --> G
    G --> H["Submit Pendaftaran Tanggungan"]
  end

  subgraph SYS["Sistem HRD Quilvian"]
    H --> I{"Validasi Kuota Keluarga & Batas Umur"}
    I -- "Pasangan > 1 ATAU Anak > 3" --> J["Tolak Pengajuan: Kuota Tanggungan Maksimal Terpenuhi"]
    I -- "Anak Usia > 25 Tahun ATAU Surat Kuliah Kosong" --> K["Tolak Pengajuan: Melebihi Batas Usia Tanggungan Sah"]
    I -- "Data Valid & Kuota Tersedia" --> L["Simpan Draft Tanggungan & Teruskan Notifikasi Tugas Verifikasi ke HR"]
  end

  subgraph HR_BO["Petugas Kompensasi & Benefit (HR Benefit Officer)"]
    L --> M["Buka Antrean Verifikasi Tanggungan"]
    M --> N{"Pemeriksaan Keabsahan Dokumen Kependudukan (KK & Akta)"}
    N -- "Dokumen Buram / NIK Tidak Cocok" --> O["Kembalikan ke Pegawai dengan Catatan Perbaikan"]
    N -- "Sah & Terverifikasi" --> P["Setujui Pendaftaran Tanggungan"]
  end

  subgraph FIN["Aktualisasi Hak Manfaat"]
    P --> Q["Aktifkan Kepesertaan Tanggungan pada Paket Benefit Pegawai"]
    Q --> R["Sinkronisasi Data Kepesertaan ke Portal BPJS Kesehatan / Asuransi Swasta"]
    R --> S["Selesai: Tanggungan Berhak Menerima Fasilitas Kesehatan"]
  end
```

### Tabel Rincian Langkah 12.1

| No | Langkah Kerja | Pelaku | Masukan (Input) | Keluaran (Output) | Jalur Pengecualian / Mitigasi Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Input Data Tanggungan | Pegawai RS | NIK, Hubungan Keluarga, Tanggal Lahir | Data mentah formulir tanggungan | Validasi format NIK 16 digit angka; blokir submit jika tanggal lahir tidak logis. |
| 2 | Unggah Berkas Kependudukan | Pegawai RS | Scan KK, Akta Nikah, Akta Lahir, Surat Kuliah | Berkas lampiran terunggah | Validasi format PDF/JPG/PNG maks 5 MB; wajib ada surat dekanat jika anak berusia > 21 tahun. |
| 3 | Validasi Kuota Otomatis | Sistem HRD | Data keluarga terdaftar | Keputusan eligibilitas kuota | Tolak otomatis jika pasangan sah sudah terdaftar 1 orang atau anak sah sudah mencapai 3 orang. |
| 4 | Verifikasi Fisik Dokumen | HR Benefit Officer | Berkas pendaftaran & scan dokumen | Keputusan verifikasi (Approve/Reject) | Kembalikan permohonan ke ESS jika nomor KK berbeda atau NIK terindikasi salah ketik. |
| 5 | Aktivasi & Handoff BPJS | Sistem HRD | Data tanggungan disetujui | Record `Active` pada enrollment | Kirim data pendaftaran ke antrean sinkronisasi BPJS/Asuransi (`INT-HR-62`). |

---

## 2. Alur Berobat Internal Nakes di RS Sendiri & Billing SIMRS Real-Time (`INT-HR-61`)

Diagram ini memetakan integrasi antara modul HRD dengan sistem billing SIMRS rumah sakit ketika seorang tenaga medis atau keluarganya berobat di fasilitas rumah sakit sendiri, mencakup pemotongan buku besar plafon tahunan dan penanganan ekses tagihan.

```mermaid
flowchart TD
  subgraph ADM["Petugas Registrasi / Admission SIMRS"]
    A["Pasien (Nakes / Keluarga) Datang ke Loket Pendaftaran"] --> B["Input NIP Pegawai / NIK Pasien di SIMRS"]
    B --> C["Kirim Query Validasi Eligibilitas ke Modul HRD (INT-HR-61)"]
  end

  subgraph HR_SYS["Modul HRD Quilvian"]
    C --> D{"Pengecekan Status Kepegawaian & Plafon Berjalan"}
    D -- "Pegawai Non-Aktif / Tanggungan Tidak Terdaftar" --> E["Kirim Respons: Tidak Berhak Subsidi Internal (Tarif Pasien Umum)"]
    D -- "Eligible & Aktif" --> F["Hitung Sisa Saldo Plafon Tahunan & Hak Diskon Tarif Staf"]
    F --> G["Kirim Respons: Hak Kelas Rawat, % Diskon, & Limit Saldo Tersedia"]
  end

  subgraph MED["Pelayanan Medis Rumah Sakit"]
    G --> H["Pasien Menerima Layanan Medis (Dokter, Tindakan, Farmasi)"]
    H --> I["Unit Medis Input Rincian Biaya Tindakan ke SIMRS Billing"]
  end

  subgraph CAS["Kasir SIMRS Rumah Sakit"]
    I --> J["Kasir Menutup Billing Pasien & Menerapkan Diskon Tarif Staf"]
    J --> K{"Apakah Total Tagihan <= Sisa Saldo Plafon HRD?"}
    K -- "Ya (Cukup Ditanggung Plafon)" --> L["Kirim Permintaan Pemotongan Saldo Penuh ke HRD"]
    K -- "Tidak (Timbul Ekses Tagihan)" --> M["Pecah Tagihan: Porsi Subsidi Plafon & Porsi Ekses Biaya"]
    M --> N["Kirim Permintaan Pemotongan Maksimal Sisa Plafon ke HRD"]
    N --> O{"Metode Pembayaran Sisa Ekses Medis"}
    O -- "Bayar Langsung" --> P["Pasien Membayar Tunai / Debit di Kasir RS"]
    O -- "Potong Gaji" --> Q["Kirim Tagihan Ekses ke Sub-Modul 11 Payroll Deduction (INT-HR-63)"]
  end

  subgraph LEDGER["Buku Besar Plafon HRD"]
    L --> R["Catat Mutasi Debit pada HrdBenefitBalanceMovement"]
    N --> R
    R --> S["Perbarui Sisa Saldo Plafon Pegawai & Terbitkan Kuitansi Internal"]
  end
```

### Tabel Rincian Langkah 12.2

| No | Langkah Kerja | Pelaku | Masukan (Input) | Keluaran (Output) | Jalur Pengecualian / Mitigasi Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Query Eligibilitas Pasien | Admission SIMRS | NIP Staf atau NIK Pasien | Payload query ke API HRD | Bila koneksi API HRD *timeout*, gunakan fallback verifikasi manual Kartu Pegawai. |
| 2 | Cek Status & Plafon | Sistem HRD | NIP/NIK & Tanggal Kunjungan | Kuota plafon & diskon nakes | Blokir subsidi jika nakes sedang menjalani masa cuti di luar tanggungan atau skorsing disiplin berat. |
| 3 | Perhitungan Billing Medis | Kasir SIMRS | Rincian tindakan & obat | Rekap tagihan netto diskon | Menerapkan diskon tarif staf terpusat (contoh: 30% jasa medis, 20% farmasi sesuai `HRD-ASM-35`). |
| 4 | Pemotongan Saldo Plafon | Kasir & HRD | Nomor Registrasi & Nilai Klaim | Rekaman mutasi ledger plafon | Saldo tidak boleh negatif; pemotongan otomatis dipatok maksimal sebesar sisa saldo berjalan. |
| 5 | Penyelesaian Ekses Biaya | Kasir SIMRS | Tagihan ekses pasien | Struk lunas atau voucher potongan | Ekses dialihkan ke payroll deduction Sub-11 dengan penandatanganan surat kuasa pemotongan upah. |

---

## 3. Alur Koordinasi Manfaat (*Coordination of Benefits* - COB) BPJS Kesehatan vs Asuransi Swasta

Diagram ini memetakan alur klaim biaya perawatan kesehatan dengan mekanisme koordinasi penjaminan berganda, di mana BPJS Kesehatan wajib difungsikan sebagai Penjamin Primer (*Primary Payer*) dan asuransi komersial/fasilitas subsidi RS sebagai Penjamin Sekunder (*Secondary Payer*).

```mermaid
flowchart TD
  subgraph ADM_COB["Penerimaan & Admisi Pasien Rawat Inap"]
    A["Pasien Nakes Membutuhkan Tindakan Medis / Rawat Inap"] --> B["Petugas Admisi Terbitkan Surat Eligibilitas Peserta (SEP) BPJS Kesehatan"]
    B --> C{"Apakah Pasien Memilih Naik Kelas Rawat Inap?"}
    C -- "Tidak (Sesuai Hak BPJS)" --> D["Perawatan Dijamin Penuh oleh Tarif INA-CBGs BPJS"]
    C -- "Ya (Naik Kelas VIP Hak Asuransi RS)" --> E["Catat Klaim COB: Penjamin 1 = BPJS, Penjamin 2 = Asuransi Swasta/RS"]
  end

  subgraph DISCHARGE["Selesai Perawatan & Rekonsiliasi Billing"]
    D --> F["Pasien Pulang: Tidak Ada Selisih Biaya (Rp 0)"]
    E --> G["Pasien Selesai Perawatan & Unit Kasir Rekap Seluruh Biaya Riil"]
    G --> H["Klaimkan Tagihan Porsi Utama ke BPJS Kesehatan (Tarif Paket INA-CBGs)"]
    H --> I["Hitung Selisih Biaya (Cost-Sharing / Ekses Kenaikan Kelas)"]
  end

  subgraph CLAIM_SEC["Pengajuan Klaim Penjamin Sekunder (Secondary Payer)"]
    I --> J["Pegawai / Kasir Buka Menu Klaim COB di Modul HRD"]
    J --> K["Input Nomor SEP BPJS Kesehatan & Nilai Klaim Selisih"]
    K --> L["Unggah Dokumen: Salinan SEP, Billing Lengkap, Kwitansi Selisih Biaya, Resume Medis"]
    L --> M["Sistem Validasi Mandatory SEP: sep_number Wajib Terisi"]
  end

  subgraph VERIFY_SEC["Verifikasi Asuransi / HR Benefit"]
    M --> N{"Pemeriksaan Klaim Sekunder oleh Verifikator Asuransi"}
    N -- "Obat Non-Formularium Tanpa Indikasi Spesialis" --> O["Tolak Porsi Biaya Non-Medis / Kosmetik"]
    N -- "Kenaikan Kelas Sesuai Polis Asuransi Tambahan" --> P["Setujui Pembayaran Klaim Sekunder"]
    P --> Q["Asuransi Swasta / RS Membayarkan Selisih Biaya ke Rekening Billing"]
  end
```

### Tabel Rincian Langkah 12.3

| No | Langkah Kerja | Pelaku | Masukan (Input) | Keluaran (Output) | Jalur Pengecualian / Mitigasi Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Penerbitan SEP BPJS | Loket BPJS / Admission | Kartu BPJS & Rujukan Faskes | Nomor SEP BPJS Kesehatan | Wajib terbit pada saat admisi; tanpa nomor SEP, klaim COB sekunder tidak dapat diajukan. |
| 2 | Verifikasi Naik Kelas | Pasien & Admisi | Surat persetujuan naik kelas | Form penetapan selisih tarif | Petugas wajib menginformasikan konsekuensi selisih biaya naik kelas kamar kepada nakes/keluarga. |
| 3 | Perhitungan Porsi Klaim | Kasir RS | Rincian INA-CBGs & tagihan riil | Breakdown tagihan primer vs sekunder | Pisahkan biaya yang di-cover BPJS dari tagihan selisih kamar dan obat non-formularium. |
| 4 | Input Klaim COB Sekunder | Pegawai / HRD | Form klaim, lampiran SEP & kuitansi | Pengajuan klaim `TrxBenefitClaim` | Sistem otomatis memblokir submit jika kolom `sep_number` kosong (`VR-BEN-16`). |
| 5 | Settlement Penjamin 2 | Asuransi / Keuangan | Persetujuan klaim sekunder | Pelunasan tagihan ke RS | Sisa ekses di luar limit polis asuransi swasta menjadi tanggung jawab pribadi pegawai. |

---

## 4. Alur Masa Tunggu Aktivasi Bertahap & Penonaktifan via Exit Clearance

Diagram ini memetakan masa tunggu aktivasi paket benefit dari hari pertama masuk kerja (H+0) hingga lulus probation, serta alur penonaktifan otomatis dan penerbitan surat portabilitas BPJS Mandiri saat pegawai mengakhiri hubungan kerja.

```mermaid
flowchart TD
  subgraph ONB["Hari Pertama Kerja (H+0 Onboarding - Sub-04 S-C4-ONB)"]
    A["Pegawai Baru Tanda Tangan Kontrak Awal"] --> B["Event Trigger: OnEmployeeOnboardingCompleted"]
    B --> C["Sistem Otomatis Daftarkan BPJS Ketenagakerjaan (Program JKK & JKM)"]
    C --> D["Perlindungan Risiko Kecelakaan Kerja Nakes Aktif Seketika"]
    D --> E["Paket Manfaat Kesehatan (BPJS Kes, Swasta, Plafon RS) Berstatus: PendingProbation"]
  end

  subgraph PROBATION["Masa Percobaan 3 Bulan Selesai"]
    E --> F["Pegawai Menjalani Evaluasi Kinerja Masa Percobaan 3 Bulan"]
    F --> G{"Hasil Evaluasi Masa Percobaan (Sub-05)"}
    G -- "Tidak Lulus / Kontrak Berakhir" --> H["Nonaktifkan BPJS Ketenagakerjaan & Pegawai Berhenti"]
    G -- "Lulus Masa Percobaan (ProbationPassed)" --> I["Penerbitan SK Pengangkatan Tetap / PKWT Lanjutan"]
    I --> J["Event Trigger: OnProbationPassed"]
    J --> K["Aktivasi Penuh: BPJS Kesehatan PPU, Asuransi Swasta Tambahan, & Subsidi Plafon RS"]
    K --> L["Buka Hak Pendaftaran Tanggungan Keluarga di Portal ESS"]
  end

  subgraph OFFBOARDING["Siklus Penutupan Kerja (Exit Clearance - Sub-20)"]
    M["Pegawai Mengajukan Resign / Habis Kontrak / PHK"] --> N["HRD Menetapkan Effective Termination Date"]
    N --> O["Pemeriksaan Kewajiban Medis di Checklist Exit Clearance Sub-20"]
    O --> P{"Apakah Ada Tagihan Medis Ekses yang Belum Lunas?"}
    P -- "Ada Tagihan Tertunggak" --> Q["Konsolidasikan Tagihan ke Pemotongan Pesangon Akhir (Sub-11 S-B6)"]
    P -- "Bebas Tunggakan" --> R["Beri Tanda Centang Hijau: Benefit Obligations Cleared"]
    Q --> R
    R --> S["Tepat pada Tanggal Efektif Keluar: Otomatis Terminate Seluruh Polis & Plafon RS"]
    S --> T["Terbitkan Surat Keterangan Penonaktifan BPJS PPU untuk Mutasi Mandiri"]
  end
```

### Tabel Rincian Langkah 12.4

| No | Langkah Kerja | Pelaku | Masukan (Input) | Keluaran (Output) | Jalur Pengecualian / Mitigasi Gagal |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | Pendaftaran Cepat BPJS TK | Sistem HRD | Data pegawai onboarding baru | Pendaftaran JKK & JKM aktif | Langsung aktif H+0 tanpa menunggu masa percobaan guna melindungi nakes dari risiko pajanan kerja. |
| 2 | Penahanan Benefit Kesehatan | Sistem HRD | Masa kerja < 3 bulan | Status paket `PendingProbation` | Fasilitas rawat jalan nakes selama 3 bulan pertama difasilitasi melalui poli internal K3RS. |
| 3 | Aktivasi Pasca-Probation | Sistem HRD | SK Kelulusan Percobaan | Paket benefit aktif penuh & saldo plafon | Otomatis generate saldo awal plafon tahunan prorata sesuai sisa bulan kalender berjalan. |
| 4 | Cek Tagihan Medis Ekses | Sistem HRD & SIMRS | NIP pegawai yang akan keluar | Status bebas tunggakan medis | Blokir approval Exit Clearance jika pegawai masih memiliki piutang rawat inap di kasir RS. |
| 5 | Terminasi & Portabilitas | Sistem HRD | Tanggal efektif keluar | Status polis `Terminated` & surat mutasi | Berikan PDF Surat Keterangan Berhenti Bekerja bagi BPJS Kesehatan agar nakes dapat bayar mandiri. |
