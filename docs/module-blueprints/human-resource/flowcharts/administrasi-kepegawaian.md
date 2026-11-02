# Administrasi Kepegawaian & Dokumen Peristiwa Kepegawaian

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Jenis | Flowchart proses bisnis |
| Slice | `S-A1` Workforce Core & Employee 360 |
| Status | `draft` |
| Kesiapan | `READY FOR DESIGN` |
| Referensi Keputusan | `HRD-DEC-046` s.d. `HRD-DEC-057`, `HRD-DEC-012`, `HRD-DEC-031`, `HRD-DEC-033` |

---

## 1. Apa yang dijawab berkas ini

Berkas ini memodelkan tiga alur operasional utama pada sub-modul Workforce Core & Administrasi Kepegawaian:
1. **Alur Dokumen Peristiwa Kepegawaian (SK Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara)**: bagaimana peristiwa kepegawaian dicatat sebagai satu SK utuh, diverifikasi terhadap periode terbuka, disetujui tanpa swa-setuju, dijadwalkan, dieksekusi pada tanggal berlaku, serta aturan pembatalannya.
2. **Alur Permohonan Perubahan Data Karyawan (Layanan Mandiri & HR Atas Nama)**: bagaimana perubahan data pribadi diproses dengan pemisahan tegas antara data non-sensitif dan data sensitif (rekening bank, NIK, tanggungan), kewajiban berkas bukti, dan pemisahan verifikator.
3. **Alur Batch Data Awal Penempatan Kepegawaian**: bagaimana 55 pegawai existing tanpa catatan jabatan/atasan/gaji dilengkapi datanya melalui satu batch tervalidasi yang disetujui sekali.

---

## 2. Diagram 1 — Dokumen Peristiwa Kepegawaian (SK Mutasi/Promosi/Rotasi/Demosi)

Merujuk pada `HRD-DEC-048`, `HRD-DEC-049`, `HRD-DEC-050`, `HRD-DEC-056`, dan `HRD-DEC-057`.

```mermaid
flowchart TD
    subgraph hr_admin[HR Admin / Pembuat Dokumen]
        A([Peristiwa Kepegawaian Muncul]) --> B[Buat Dokumen Peristiwa Baru]
        B --> C[Isi Unit, Jabatan, Atasan 'Dari -> Ke', Tgl Efektif, Alasan]
        C --> D[(Draft SK)]
        D --> E{Unggah Scan SK Resmi Bertandatangan?}
        E -- Belum Ada --> E1[/Tombol Ajukan Terkunci/]
        E1 --> D
        E -- Sudah Ada --> F[Periksa Tanggal Berlaku]
        F --> G{Tanggal Berlaku Surut?}
        G -- Ya --> H{Periode Kehadiran Masih Terbuka?}
        H -- Tertutup --> H1[/Ditolak Sistem 409 - Periode Tertutup/]
        H1 --> C
        H -- Terbuka --> I[Ajukan SK dengan Alasan Surut]
        G -- Tidak --> I
        I --> J[(Menunggu Persetujuan)]
    end

    subgraph hr_manager[HR Manager / Penyetuju T8]
        J --> K[Buka Dokumen & Periksa Scan SK]
        K --> L{Penyetuju == Pembuat SK?}
        L -- Ya Swa-setuju --> L1[/Ditolak Sistem 403 - Wajib Penyetuju Lain/]
        L1 --> L2[Alihkan ke Otoritas Lebih Tinggi]
        L2 --> K
        L -- Tidak --> M{Keputusan Penyetuju}
        M -- Tolak --> N[(Ditolak)]
        N --> N1([Selesai - Tidak Ada Perubahan])
        M -- Minta Perbaikan --> O[(Perlu Revisi)]
        O --> C
        M -- Setujui --> P{Tanggal Efektif Sudah Lewat?}
    end

    subgraph eksekusi[Sistem / Otomatisasi Tanggal Berlaku]
        P -- Ya Berlaku Surut --> Q[(Berlaku)]
        P -- Belum Masa Depan --> R[(Dijadwalkan)]
        R --> S{Ada Pengajuan Pembatalan?}
        S -- Ya Diajukan HR Admin --> T[HR Manager Menyetujui Batal]
        T --> U[(Dibatalkan)]
        U --> N1
        S -- Tidak --> V[Tanggal Efektif Tiba]
        V --> Q
        Q --> W[Tutup Penempatan Lama per H-1]
        W --> X[Buka Penempatan Baru per Tanggal Efektif]
        X --> Y[Catat 1 Baris Riwayat Kepegawaian Rujuk SK]
        Y --> Z[Perbarui Cerminan Master Data Pegawai]
        Z --> AA([Selesai - Penempatan Efektif Aktif])
    end
```

### Tabel Langkah Alur SK Kepegawaian

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal / ditolak |
| --- | --- | --- | --- | --- |
| Buat Dokumen SK | HR Admin | Pegawai, kategori peristiwa, unit/jabatan/atasan tujuan, tanggal efektif, nomor SK, alasan | Dokumen berstatus `Draft` | Isian wajib kosong -> validasi form 400 |
| Unggah Scan SK | HR Admin | Berkas PDF scan SK asli bertandatangan pejabat | Lampiran SK tersimpan | Gagal simpan berkas -> tombol Ajukan tidak aktif (`HRD-DEC-056`) |
| Ajukan SK | HR Admin | Dokumen `Draft` lengkap scan SK | Status `Menunggu persetujuan` | Ditolak jika tanggal efektif jatuh pada periode kehadiran tertutup (`HRD-DEC-050`) |
| Verifikasi & Approval | HR Manager / CorporateHr | Pratinjau SK dan berkas scan | Status `Dijadwalkan` atau `Berlaku` (jika retroaktif) | Ditolak sistem 403 bila penyetuju sama dengan pembuat (`HRD-DEC-031`) |
| Pembatalan SK | HR Admin & HR Manager | Alasan pembatalan resmi | Dokumen berstatus `Dibatalkan` | Ditolak bila SK sudah berstatus `Berlaku` (`HRD-DEC-057`) |
| Eksekusi Efektif | Sistem | SK berstatus `Dijadwalkan` yang mencapai tanggal efektif | Status `Berlaku`, penempatan lama ditutup H-1, penempatan baru dibuka, riwayat dicatat | Notifikasi log audit sistem |

---

## 3. Diagram 2 — Permohonan Perubahan Data Karyawan & Pengajuan HR Atas Nama

Merujuk pada `HRD-DEC-052`, `HRD-DEC-055`, dan `AC-WFC-07`.

```mermaid
flowchart TD
    subgraph pemohon[Pegawai / HR Atas Nama]
        A([Kebutuhan Ubah Data Pribadi]) --> B{Siapa yang Mengajukan?}
        B -- Mandiri Pegawai --> C[Form Layanan Mandiri Pegawai]
        B -- HR Atas Nama Pegawai --> D[Form HR Atas Nama Pegawai]
        D --> D1[Isi Identitas Pegawai & Alasan Atas Nama Wajib]
        C --> E[Pilih Field yang Diubah]
        D1 --> E
        E --> F{Field Termasuk Data Sensitif?<br/>Rekening, NIK, NPWP, Tanggungan}
        F -- Ya Data Sensitif --> G[Wajib Lampirkan Berkas Bukti Resmi]
        G --> H[(Draft Permohonan)]
        F -- Tidak Non-Sensitif --> H
        H --> I[Ajukan Permohonan]
        I --> J[(Menunggu Verifikasi)]
    end

    subgraph verifikator[Petugas HR Lain / Verifikator]
        J --> K[Buka Detail Permohonan]
        K --> L{Verifikator == Pembuat?}
        L -- Ya --> L1[/Ditolak Sistem 403 - Verifikator Wajib Berbeda/]
        L1 --> L2[Alihkan ke Petugas HR Lain]
        L2 --> K
        L -- Tidak --> M[Periksa Berkas Bukti & Data]
        M --> N{Keputusan Verifikasi}
        N -- Bukti Tidak Sesuai --> O[(Perlu Revisi)]
        O --> E
        N -- Ditolak --> P[(Ditolak)]
        P --> P1([Selesai Tanpa Perubahan Data])
        N -- Sesuai & Valid --> Q[(Disetujui)]
        Q --> R[Terapkan Perubahan Data]
        R --> S[(Diterapkan)]
        S --> T[Perbarui Berkas Profil Pegawai]
        T --> U([Selesai - Nilai Baru Aktif])
    end
```

### Tabel Langkah Alur Perubahan Data Karyawan

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal / ditolak |
| --- | --- | --- | --- | --- |
| Pengajuan Mandiri | Pegawai | Field yang diubah, alasan, berkas bukti jika sensitif | Permohonan `Submitted` | Bukti kosong untuk field sensitif -> tolak simpan |
| Pengajuan Atas Nama | HR Admin | Pegawai target, alasan atas nama, bukti dokumen | Permohonan `Submitted` (bertanda `IsOnBehalf`) | Alasan atas nama kosong -> tolak 400 |
| Pemeriksaan Bukti | HR Verifikator | Dokumen bukti vs nilai yang diminta | Evaluasi verifikasi | Verifikator sama dengan pembuat -> tolak 403 (`HRD-DEC-052`) |
| Penerapan Data | HR Verifikator | Permohonan `Approved` | Status `Applied`, berkas profil terupdate | Data lama tetap berlaku sampai tombol Terapkan berhasil |

---

## 4. Diagram 3 — Batch Data Awal Penempatan Kepegawaian (Baseline Migration)

Merujuk pada `HRD-DEC-054` dan `AC-WFC-09`.

```mermaid
flowchart TD
    subgraph persiapan[HR Admin]
        A([55 Pegawai Aktif Belum Lengkap Relasi]) --> B[Unduh Template Batch Data Awal]
        B --> C[Sistem Prefill 55 Pegawai & Unit Existing]
        C --> D[Isi Jabatan, Atasan, Tgl Efektif SK Terakhir]
        D --> E[Unggah File Batch ke Sistem]
    end

    subgraph validasi[Sistem Validasi]
        E --> F[Validasi Format & Integritas Master]
        F --> G{Pemeriksaan Aturan Bisnis:<br/>1. Tepat 1 Unit Utama?<br/>2. Tepat 1 Jabatan Utama?<br/>3. Pegawai Pernah Masuk Batch?}
        G -- Ada Pelanggaran Baris --> H[Tampilkan Daftar Baris Gagal & Alasan]
        H --> D
        G -- Seluruh Baris Valid --> I[(Batch Siap Diajukan)]
        I --> J[HR Admin Ajukan Batch #1]
        J --> K[(Menunggu Persetujuan HR Manager)]
    end

    subgraph approval[HR Manager]
        K --> L[Tinjau Ringkasan & Baris Batch]
        L --> M{Penyetuju == Pengunggah?}
        M -- Ya --> M1[/Ditolak Sistem 403 - Pembuat != Penyetuju/]
        M1 --> K
        M -- Tidak --> N{Keputusan HR Manager}
        N -- Tolak --> O[(Batch Ditolak)]
        O --> D
        N -- Setujui Sekali --> P[(Batch Disetujui)]
        P --> Q[Sistem Terbitkan Seluruh Penempatan Awal]
        Q --> R[Tandai Pegawai 'Sudah Masuk Data Awal']
        R --> S[Rekam Nomor Batch & Audit Trail]
        S --> T([Selesai - Baseline Kepegawaian Terkunci])
    end
```

---

## 5. Batas yang Tidak Boleh Dilanggar

| Larangan | Dasar Keputusan | Sebab & Dampak |
| --- | --- | --- |
| Dilarang menyediakan tombol Edit atau Hapus inline di tabel penempatan lintas-pegawai | `HRD-DEC-047`, `HRD-DEC-055` | Penempatan adalah catatan historis resmi. Perubahan sembarangan merusak riwayat mutasi dan hak akses unit (`HRD-TF-021`) |
| Dilarang menyetujui SK atau perubahan penempatan oleh pembuatnya sendiri | `HRD-DEC-031`, `HRD-DEC-056` | Pelanggaran fatal pemisahan peran (`APPROVER_MUST_DIFFER_FROM_CREATOR`) |
| Dilarang mengajukan SK tanpa lampiran scan SK resmi bertandatangan | `HRD-DEC-056` | Bukti fisik legalitas mutasi/promosi rumah sakit wajib terarsip |
| Dilarang membatalkan SK yang sudah berstatus `Berlaku` | `HRD-DEC-057` | Mengaburkan riwayat kepegawaian. Jika batal setelah berlaku, wajib terbitkan SK pemindahan baru |
| Dilarang mengubah data sensitif (rekening, NIK, tanggungan) langsung di form master pegawai | `HRD-DEC-052` | Data sensitif berdampak langsung pada pajak, BPJS, dan payroll; wajib lewat verifikasi berbukti |
| Dilarang memasukkan pegawai yang sama ke dalam Batch Data Awal lebih dari satu kali | `HRD-DEC-054` | Data awal hanya untuk baseline. Perubahan selanjutnya wajib lewat SK atau alur resmi |

---

## 6. Traceability

| Alur Proses Bisnis | Decision ID | Kontrak Terkait | Acceptance Test ID |
| --- | --- | --- | --- |
| Dokumen SK Peristiwa Kepegawaian | `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057` | `contracts/state-transition-matrix.md` §6.0, `api-contract.md` | `AC-WFC-11`, `AC-WFC-12`, `AC-WFC-13` |
| Aturan 1 Penempatan Utama & Tumpang Tindih | `HRD-DEC-049` | `contracts/validation-matrix.md` §7.0 | `AC-WFC-04` |
| Batasan Berlaku Surut ke Periode Terbuka | `HRD-DEC-050` | `contracts/validation-matrix.md` §7.0 | `AC-WFC-05` |
| Perubahan Data Sensitif & Pengajuan Atas Nama | `HRD-DEC-052` | `contracts/state-transition-matrix.md` §6.1, `validation-matrix.md` | `AC-WFC-07` |
| Batch Data Awal Penempatan Baseline | `HRD-DEC-054` | `contracts/state-transition-matrix.md` §6.4 | `AC-WFC-09` |
