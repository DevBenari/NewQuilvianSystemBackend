# Flowchart Alur Proses Bisnis — Administrasi Lembur Pegawai Rumah Sakit (Slice `S-B3`)

Dokumen ini memodelkan diagram alur proses bisnis (*business process workflow*) untuk **Sub-modul 09. Overtime Management (Slice `S-B3`)**, menyerap keputusan interview `HRD-DEC-116` s.d. `HRD-DEC-121` serta asumsi `HRD-ASM-23` s.d. `HRD-ASM-26`.

---

## 1. Alur Pengajuan Surat Perintah Kerja Lembur (SPKL) Pra-Dinas Terencana

Diagram ini memvisualisasikan pengajuan lembur terencana minimal H-1 oleh staf atau Kepala Ruangan, pengecekan kuota maksimal ketenagakerjaan PP 35/2021 (maksimal 4 jam/hari dan 18 jam/minggu), persetujuan Kepala Ruangan, serta verifikasi administratif kuota oleh HR Officer sebelum dinas lembur dikerjakan (`HRD-DEC-116`, `HRD-DEC-117`, `HRD-DEC-120`).

```mermaid
flowchart TD
    subgraph STAF ["Staf / Pegawai Pelaksana"]
        A1["Input Pengajuan SPKL Terencana (Minimal H-1)"] --> A2["Pilih Jenis Kompensasi: Finansial vs CTO"]
        A2 --> A3["Tentukan Jam Mulai-Selesai & Uraian Tugas"]
        A3 --> A4["Kirim Pengajuan SPKL"]
    end

    subgraph SISTEM ["Engine Validasi & Kuota Sistem"]
        A4 --> A5{"Cek Kuota PP 35/2021:<br/>Harian <= 4 Jam & Mingguan <= 18 Jam?"}
        A5 -->|"Melanggar Kuota"| A6["Tolak Otomatis (Hard-Block Kuota)"]
        A6 --> A1
        A5 -->|"Sesuai Kuota"| A7{"Cek Anti-Self-Approval:<br/>Pemohon != Penyetuju?"}
        A7 -->|"Diri Sendiri"| A8["Alihkan Approval ke Atasan Hierarkis"]
        A7 -->|"Staf Bawahan"| A9["Teruskan ke Kepala Ruangan"]
        A8 --> A10["Masuk Kotak Masuk Atasan Hierarkis"]
        A9 --> A11["Masuk Kotak Masuk Kepala Ruangan"]
    end

    subgraph ATASAN ["Kepala Ruangan / Pejabat Penyetuju"]
        A11 --> A12{"Keputusan Persetujuan Operasional"}
        A10 --> A12
        A12 -->|"Tolak"| A13["SPKL Ditolak dengan Alasan Tertulis"]
        A12 -->|"Setujui"| A14["SPKL Disetujui Tahap 1 (Operasional)"]
    end

    subgraph HRD ["HR Officer / Administrator Lembur"]
        A14 --> A15{"Verifikasi Administratif & Anggaran Unit"}
        A15 -->|"Setujui"| A16["SPKL Resmi Terbit (Approved Ready to Work)"]
        A15 -->|"Tolak"| A17["Ditolak HRD (Anggaran / Over-Quota)"]
    end
```

### Tabel Rincian Langkah SPKL Pra-Dinas

| No | Tahap / Peristiwa | Pelaku | Masukan | Keluaran | Penanganan Pengecualian / Gagal |
| :---: | --- | --- | --- | --- | --- |
| 1 | Input Form SPKL | Staf / Ka. Ruangan | Tanggal dinas, jam mulai-selesai, uraian tugas, opsi kompensasi | Draf pengajuan SPKL | Validasi `VR-OT-01`: Jam mulai lembur harus di masa depan (> waktu submit) |
| 2 | Guard Kuota PP 35/2021 | Sistem | Akumulasi lembur harian & mingguan berjalan | Status kelayakan kuota regulasi | Ditolak `VR-OT-04` bila akumulasi harian > 4 jam atau mingguan > 18 jam |
| 3 | Evaluasi Self-Approval | Sistem | User ID pemohon & User ID penilai | Rute approver sah | `VR-OT-03`: Jika pemohon adalah Kepala Ruangan, rute otomatis dialihkan ke Kepala Instalasi / Wadir |
| 4 | Persetujuan Operasional | Kepala Ruangan | Draf SPKL staf | Persetujuan Tahap 1 (`WaitingHrVerification`) | Jika ditolak, notifikasi alasan dikirim ke portal ESS pemohon |
| 5 | Verifikasi Administratif | HR Officer | Dokumen SPKL & ketersediaan alokasi anggaran unit | Dokumen SPKL Sah (`ApprovedForWork`) | Ditolak bila melanggar plafon jam kerja rumah sakit |

---

## 2. Alur Pelaporan Klaim Lembur Darurat Medis Pasca-Dinas (*Post-Claim Cito*)

Diagram ini memvisualisasikan penanganan lembur darurat tindakan cito medis (operasi bedah darurat OK, resusitasi IGD, lonjakan kritis ICU, atau respon bencana). Pelayanan pasien diselesaikan terlebih dahulu tanpa hambatan administrasi, kemudian klaim dilaporkan maksimal H+1 kerja dengan nomor rekam medis wajib, evaluasi eskalasi Direktur Pelayanan Medik jika melebihi kuota regulasi, dan mitigasi kelelahan nakes (`HRD-DEC-116`, `HRD-DEC-117`).

```mermaid
flowchart TD
    subgraph KLINIS ["Tim Medis / Staf Pelaksana"]
        B1["Insiden Darurat Medis / Tindakan Cito Berlangsung"] --> B2["Penanganan Pasien Selesai Sempurna"]
        B2 --> B3["Input Klaim Lembur Pasca-Dinas (Maksimal H+1 Kerja)"]
        B3 --> B4["Wajib Input Nomor Rekam Medis / Nomor Insiden"]
        B4 --> B5["Pilih Kategori: Cito Bedah / IGD / Bencana"]
        B5 --> B6["Submit Klaim Darurat Cito"]
    end

    subgraph VALIDASI ["Engine Validasi Sistem"]
        B6 --> B7{"Cek Batas Waktu Submit:<br/>Apakah <= H+1 Hari Kerja?"}
        B7 -->|"Terlambat (> H+1)"| B8["Tolak Otomatis (Submission Deadline Passed)"]
        B7 -->|"Tepat Waktu"| B9{"Evaluasi Kuota Lembur PP 35/2021"}
        B9 -->|"Melebihi Kuota PP 35"| B10["Tandai Kebutuhan Eskalasi Direktur Medis"]
        B9 -->|"Dalam Kuota PP 35"| B11["Rute Standar Kepala Ruangan"]
        B10 --> B12["Kotak Masuk Direktur Pelayanan Medik"]
        B11 --> B13["Kotak Masuk Kepala Ruangan"]
    end

    subgraph DIREKTUR ["Kepala Ruangan & Direktur Medis"]
        B13 --> B14["Kepala Ruangan Verifikasi Tindakan Medis"]
        B14 --> B15["Klaim Cito Disetujui Ruangan"]
        B12 --> B16{"Direktur Medis Evaluasi Urgensi & Fatigue"}
        B16 -->|"Tolak"| B17["Ditolak Direktur (Bukan Keadaan Cito)"]
        B16 -->|"Setujui"| B18["Disetujui Khusus dengan Otorisasi Darurat"]
        B15 --> B19["Lanjut ke Tahap Verifikasi Kehadiran Biometrik"]
        B18 --> B19
    end
```

### Tabel Rincian Langkah Klaim Darurat Medis Cito

| No | Tahap / Peristiwa | Pelaku | Masukan | Keluaran | Penanganan Pengecualian / Gagal |
| :---: | --- | --- | --- | --- | --- |
| 1 | Tindakan Medis Darurat | Tim Nakes | Pasien kritis / operasi cito | Penyelamatan nyawa pasien tuntas | Tidak ada formulir yang menghambat tindakan medis darurat |
| 2 | Pengajuan Klaim H+1 | Nakes / Ka. Tim | No RM pasien, uraian tindakan, jam mulai-selesai fisik | Berkas klaim darurat cito | `VR-OT-02`: Ditolak jika nomor RM kosong atau diajukan > H+1 kerja |
| 3 | Evaluasi Batas Kuota | Sistem | Akumulasi jam kerja harian/mingguan staf | Klasifikasi jalur approval | Jika melebihi 4 jam/hari atau 18 jam/minggu, eskalasi ke Direktur Medis diwajibkan |
| 4 | Otorisasi Khusus Direktur | Direktur Pelayanan | Berkas cito & riwayat jam kerja nakes | Rekomendasi istirahat & persetujuan dispensasi | Menolak jika tindakan dinilai elektif dan dapat ditunda |
| 5 | Lanjut ke Verifikasi | Sistem | Klaim disetujui | Status klaim `PendingBiometricVerification` | Diselaraskan dengan data mesin presensi biometrik |

---

## 3. Alur Verifikasi Silang Biometrik & Penanganan Discrepancy Flagging

Diagram ini memvisualisasikan bagaimana durasi lembur diverifikasi secara otomatis terhadap catatan tap fisik mesin presensi (Sub-modul 07 Attendance), penerapan aturan nilai terkecil (*min-duration rule*), serta alur investigasi manual HR Admin atas anomali lupa tap atau deviasi jam lembur (`HRD-DEC-119`, `HRD-ASM-23`).

```mermaid
flowchart TD
    subgraph ENGINE_VERIFIKASI ["Engine Verifikasi Kehadiran Biometrik"]
        C1["Ambil Data SPKL Disetujui (Durasi Klaim)"] --> C2["Tarik Log Check-In & Check-Out Mesin Presensi (Sub-modul 07)"]
        C2 --> C3{"Apakah Ada Bukti Tap Mesin Presensi?"}
        C3 -->|"Tidak Ada Tap (Lupa Tap)"| C4["Tandai Flagged Discrepancy: MissingBiometric"]
        C3 -->|"Ada Tap Mesin"| C5["Hitung Selisih Jam Tap Fisik Aktual"]
        C5 --> C6{"Deviasi Klaim vs Tap > 30 Menit?"}
        C6 -->|"Ya, Deviasi Signifikan"| C7["Tandai Flagged Discrepancy: DurationMismatch"]
        C6 -->|"Tidak, Deviasi Wajar"| C8["Terapkan Min-Duration Rule:<br/>Payable = Min(Claimed, Actual)"]
        C8 --> C9["Pembulatan 30 Menit ke Bawah (HRD-ASM-23)"]
        C9 --> C10["Status: AutoVerified (Siap Rekonsiliasi)"]
    end

    subgraph INVESTIGASI_HRD ["Investigasi Manual HR Admin"]
        C4 --> C11["Kotak Masuk Anomali Lembur HR Admin"]
        C7 --> C11
        C11 --> C12["HR Minta Logbook Dinas & Tanda Tangan Ka. Ruangan"]
        C12 --> C13{"Hasil Pemeriksaan Bukti Fisik"}
        C13 -->|"Bukti Sah Terkonfirmasi"| C14["Manual Adjustment Durasi Sah & Catat Alasan"]
        C13 -->|"Bukti Fiktif / Tidak Sah"| C15["Tolak Klaim Lembur (Rejected Discrepancy)"]
        C14 --> C10
    end
```

### Tabel Rincian Langkah Verifikasi Biometrik

| No | Tahap / Peristiwa | Pelaku | Masukan | Keluaran | Penanganan Pengecualian / Gagal |
| :---: | --- | --- | --- | --- | --- |
| 1 | Query Presensi Aktual | Service | ID Pegawai & Tanggal Lembur | Jam tap masuk & keluar dari Sub-modul 07 | Jika service presensi offline, masuk antrean retry background |
| 2 | Evaluasi Deviasi | Engine | Durasi SPKL vs durasi fisik mesin | Status kelayakan verifikasi | Deviasi > 30 menit otomatis ditandai *Flagged Discrepancy* |
| 3 | Perhitungan Min-Duration | Engine | `ClaimedMinutes` & `ActualPhysicalMinutes` | `VerifiedPayableMinutes` | Nilai terkecil diambil untuk mencegah pembengkakan biaya |
| 4 | Pembulatan 30 Menit | Engine | Menit terverifikasi | Kelipatan 30 menit ke bawah | Durasi < 30 menit otomatis bernilai 0 jam kompensasi (`HRD-ASM-23`) |
| 5 | Investigasi Discrepancy | HR Admin | Logbook dinas fisik unit & konfirmasi atasan | Keputusan penyesuaian manual / tolak | Klaim ditahan dari payroll hingga ada bukti logbook resmi |

---

## 4. Alur Penutupan Periode, Kompensasi CTO, dan Serah Terima Payroll

Diagram ini memvisualisasikan penguncian berkas lembur pada tanggal cut-off bulanan, pemisahan distribusi kompensasi (kredit otomatis ke saldo cuti CTO Sub-modul 08 vs uang tunai payroll Sub-modul 10), serta mekanisme penanganan SPKL menggantung melalui penyesuaian retrospektif pada bulan berikutnya (`HRD-DEC-118`, `HRD-DEC-121`, `HRD-ASM-25`).

```mermaid
flowchart TD
    subgraph CUTOFF ["Penutupan Periode Lembur Bulanan (Tgl 20)"]
        D1["Tanggal Cut-Off Tiba (Pukul 23:59:59 WIB)"] --> D2["Sistem Kunci Status Periode: CutoffLocked"]
        D2 --> D3{"Evaluasi Status Seluruh SPKL Bulan Berjalan"}
        D3 -->|"Status: AutoVerified / ManuallyAdjusted"| D4["Masukkan ke Final Snapshot Siap Bayar"]
        D3 -->|"Status: Masih Pending / Investigasi"| D5["Beri Label: PendingNextReconciliation"]
        D5 --> D6["Tahan dari Payroll Bulan Berjalan"]
        D4 --> D7["Kunci Seluruh Berkas Menjadi Immutable"]
    end

    subgraph DISTRIBUSI ["Distribusi Kompensasi Sesuai Pilihan"]
        D7 --> D8{"Pilihan Kompensasi Staf"}
        D8 -->|"Compensatory Time Off (CTO)"| D9["Kirim Mutasi Kredit ke Sub-modul 08 (TrxLeaveBalanceLedger)"]
        D9 --> D10["Saldo CTO Terbit Berlaku 3 Bulan (90 Hari)"]
        D8 -->|"Uang Lembur Finansial"| D11["Hitung Nilai Pengali Lembur (1.5x / 2x)"]
        D11 --> D12["Serah Terima Paket Data ke Sub-modul 10 Payroll"]
    end

    subgraph RETROSPEKTIF ["Siklus Bulan Berikutnya (Retrospektif)"]
        D6 --> D13["SPKL Pending Disetujui / Diverifikasi di Kemudian Hari"]
        D13 --> D14["Tandai Flag: IsRetrospectiveAdjustment = true"]
        D14 --> D15["Otomatis Disertakan pada Batch Payroll Bulan Depan"]
    end
```

### Tabel Rincian Langkah Penutupan Periode & Payroll Handoff

| No | Tahap / Peristiwa | Pelaku | Masukan | Keluaran | Penanganan Pengecualian / Gagal |
| :---: | --- | --- | --- | --- | --- |
| 1 | Lock Periode Cut-off | Scheduler / HR | Tanggal cut-off bulanan (tgl 20) | Periode berstatus `CutoffLocked` | SPKL baru setelah cut-off dialokasikan ke periode bulan berikutnya |
| 2 | Penanganan SPKL Pending | Sistem | SPKL belum selesai verifikasi | Status `PendingNextReconciliation` | Gaji reguler staf tetap cair tepat waktu tanpa hambatan lembur pending |
| 3 | Eksekusi Kredit CTO | Service | SPKL terverifikasi opsi CTO | Mutasi kredit saldo di Sub-modul 08 (`INT-HR-49`) | Berlaku 90 hari kalender; hangus otomatis jika tidak dipakai (`HRD-ASM-25`) |
| 4 | Payroll Handoff | Service | SPKL terverifikasi opsi Finansial | Berkas data serah terima Sub-modul 10 (`INT-HR-52`) | Data dikunci mutlak *read-only* untuk mencegah perbedaan angka slip gaji |
| 5 | Rekonsiliasi Retrospektif | Sistem | SPKL pending periode lalu yang baru tuntas | Rapel lembur pada payroll bulan berjalan | Mencegah hilangnya hak staf yang telat diproses atasan (`HRD-DEC-121`) |
