# Administrasi Kepegawaian & Dokumen Peristiwa Kepegawaian

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Jenis | Flowchart proses bisnis |
| Slice | `S-A1` Workforce Core & Employee 360 |
| Status | `IMPLEMENTED` — Selesai & Terverifikasi (PLAN-REPAIR-HRD-003) |
| Kesiapan | `APPROVED / IMPLEMENTED` |
| Referensi Keputusan | `HRD-DEC-046` s.d. `HRD-DEC-057`, `HRD-DEC-091` s.d. `HRD-DEC-094`, `HRD-DEC-012`, `HRD-DEC-031`, `HRD-DEC-033` |

---

## 1. Apa yang dijawab berkas ini

Berkas ini memodelkan tiga alur operasional utama pada sub-modul Workforce Core & Administrasi Kepegawaian:
1. **Alur Dokumen Peristiwa Kepegawaian (SK Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara)**: bagaimana peristiwa kepegawaian dicatat sebagai satu SK utuh, diverifikasi terhadap periode terbuka, disetujui tanpa swa-setuju, dijadwalkan, dieksekusi pada tanggal berlaku, serta aturan pembatalannya.
2. **Alur Permohonan Perubahan Data Karyawan (Layanan Mandiri & HR Atas Nama)**: bagaimana perubahan data pribadi diproses secara mandiri oleh pegawai melalui rute `/self-services/human-resource/employee/profile-changes` maupun oleh HR atas nama pegawai (`HRD-DEC-052`). Memuat mekanisme *Live Diff Engine* otomatis (multi-kategori `Multiple`), kewajiban berkas bukti resmi untuk data sensitif (NIK, Nama, Tempat/Tgl Lahir, Status Nikah), aksi tunggal *Simpan dan Ajukan*, perlindungan mutlak master data, penegakan ketat *Anti-Self-Approval* (Four-Eyes Principle), persetujuan melalui Workflow Engine resmi, penerapan otomatis (*Apply*), dan pemantauan di *Riwayat Pengajuan Saya* (`HRD-DEC-091` s.d. `HRD-DEC-094`).
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

Merujuk pada `HRD-DEC-052`, `HRD-DEC-055`, `HRD-DEC-091`, `HRD-DEC-092`, `HRD-DEC-093`, `HRD-DEC-094`, `PLAN-REPAIR-HRD-003`, dan `AC-WFC-07`.

```mermaid
flowchart TD
    subgraph pemohon_ess[Layanan Mandiri Pegawai - ESS /profile-changes]
        A([Pegawai Login]) --> B[Buka Menu 'Kelola Data Saya']
        B --> C[Sistem Muat Snapshot Profil Awal via GET /current-profile]
        C --> D[Tampilkan Form Profil: Field Penempatan Dikunci Read-only]
        D --> E[Pegawai Edit Isian Data Pribadi / Kontak / Alamat / Kontak Darurat]
        E --> F[Live Diff Engine: Hitung Delta Nilai Lama vs Nilai Baru Secara Real-time]
        F --> G{Ada Field Berubah?}
        G -- Tidak --> G1[/Tombol Simpan Dinonaktifkan/]
        G -- Ya --> H{Ada Field Sensitif Berubah?<br/>NIK, Nama, Tempat/Tgl Lahir, Status Nikah}
        H -- Ya Sensitif --> I[Wajib Unggah Dokumen Bukti Resmi KTP/KK/Surat Nikah]
        H -- Tidak Sensitif --> J[Lampiran Bukti Opsional]
        I --> K[Aksi Tunggal: Simpan dan Ajukan Perubahan Data]
        J --> K
        K --> L[Sistem Eksekusi Transaksional:<br/>1. POST /profile-changes Draft Kategori Multiple<br/>2. POST /evidence Unggah Bukti Multipart<br/>3. POST /submit Ajukan ke Workflow Engine]
        L --> M[(Permohonan Submitted - Menunggu Verifikasi)]
        M --> M1[Pegawai Pantau di Tab 'Riwayat Pengajuan Saya']
        M1 --> M2{Batalkan Permohonan?}
        M2 -- Ya Draft/Submitted --> M3[POST /cancel Permohonan Dibatalkan]
        M3 --> M4([Selesai - Dibatalkan Pemohon])
    end

    subgraph pemohon_hr[Pengajuan Kuasa HR - On-Behalf /create-on-behalf]
        OB_A([HR Admin Buka Form Kuasa]) --> OB_B[Pilih Pegawai Target & Isi Alasan Kuasa Wajib]
        OB_B --> OB_C[Pilih Field & Masukkan Nilai Baru + Unggah Dokumen Kuasa]
        OB_C --> OB_D[POST /on-behalf Multipart Form Data]
        OB_D --> M
    end

    subgraph master_protection[Perlindungan Master Data Resmi]
        M -.-> PROT[Master Data MstEmployee & MstWorkforceProfile TERLINDUNGI<br/>Data Resmi Rumah Sakit TETAP MEMEGANG NILAI LAMA]
    end

    subgraph verifikator_hrd[Verifikasi & Persetujuan HRD]
        M --> V1[HRD Buka Daftar Permohonan /employee-profile-changes]
        V1 --> V2[Klik 'Lihat Rincian' -> Buka Modal Komparasi Nilai]
        V2 --> V3{Anti-Self-Approval Check:<br/>Verifikator == Pemohon?}
        V3 -- Ya Swa-Verifikasi --> V3A[/DITOLAK SISTEM 400/403<br/>Tombol Verifikasi & Approval Dinonaktifkan/]
        V3A --> V3B[Wajib Diverifikasi oleh Petugas HR Lain]
        V3B --> V1
        V3 -- Tidak Beda Orang --> V4[Klik 'Mulai Verifikasi HRD' -> Status UnderVerification]
        V4 --> V5[Periksa Berkas Bukti & Kesesuaian Nilai]
        V5 --> V6{Keputusan Verifikasi}
        V6 -- Bukti Tidak Sesuai --> V7[(NeedRevision - Perlu Revisi)]
        V7 --> D
        V6 -- Ditolak Resmi --> V8[(Rejected - Ditolak)]
        V8 --> V9([Selesai - Master Data Tetap Nilai Lama])
        V6 -- Terverifikasi Sah --> V10[Teruskan ke Approval Workflow Engine Resmi]
        V10 --> V11[(Approved - Disetujui)]
    end

    subgraph eksekusi_apply[Eksekusi Otomatis Penerapan Data Master]
        V11 --> APP1[Integration Service Eksekusi ApplyAsync]
        APP1 --> APP2{Validasi Old Value Cocok?}
        APP2 -- Terjadi Konflik Perubahan Lain --> APP3[/Deteksi Konflik 409 - Butuh Penyesuaian/]
        APP2 -- Valid Cocok --> APP4[Update Transaksional ke MstEmployee & MstWorkforceProfile]
        APP4 --> APP5[(Status Permohonan Menjadi Applied)]
        APP5 --> APP6[Karyawan Melihat Data Profil Resmi Terkini pada ESS]
        APP6 --> APP7([Selesai - Perubahan Sukses Diterapkan])
    end
```

### Tabel Langkah Alur Perubahan Data Karyawan

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal / ditolak |
| --- | --- | --- | --- | --- |
| Pemuatan Profil Mandiri | Pegawai | Akun terautentikasi (tanpa parameter URL bebas) | Snapshot profil awal via `GET /current-profile` | Akun belum terhubung ke Profil Workforce -> Tampilkan `InformationAlert` peringatan |
| Pengeditan & Live Diff | Pegawai | Isian data pribadi, kontak, alamat, kontak darurat | Perhitungan delta nilai lama vs baru secara real-time | Tidak ada perubahan -> tombol Simpan dinonaktifkan; penempatan kerja terkunci *read-only* (`HRD-DEC-091`) |
| Unggah Bukti Sensitif | Pegawai | Berkas PDF/JPG/PNG (maks. 5 MB) | Bukti pendukung tersimpan | Berkas kosong untuk field sensitif (NIK, Status Nikah, Nama, Lahir) -> tolak submit (`HRD-DEC-052`) |
| Simpan dan Ajukan Tunggal | Pegawai | Konfirmasi aksi tunggal | Permohonan terbit (`Draft` -> bukti -> `Submitted`) berkategori `Multiple` | Gagal jaringan/validasi -> tampilkan alert error tanpa mengubah master data |
| Pengajuan Atas Nama (Kuasa) | HR Admin | Pegawai target, alasan atas nama wajib, berkas kuasa | Permohonan `Submitted` bertanda `IsOnBehalf` via multipart | Alasan kosong -> tolak 400; header multipart ditangani otomatis bebas 415 (`FIX-HRD-003-06`) |
| Perlindungan Master Data | Sistem | Permohonan berstatus `Submitted` / `UnderVerification` | Master data `MstEmployee` / `WFP` tetap data lama | Master data tidak boleh termutasi sebelum `ApplyAsync` berhasil (`HRD-DEC-092`) |
| Pemeriksaan Anti-Self-Approval | Sistem | Identitas verifikator vs identitas pemohon | Validasi Four-Eyes | Verifikator sama dengan pemohon -> tolak backend 400 & tombol UI dinonaktifkan (`HRD-DEC-093`) |
| Mulai Verifikasi | HR Verifikator | Permohonan berstatus `Submitted` | Permohonan berstatus `UnderVerification` | Swa-verifikasi dicegah sistem |
| Keputusan Workflow | Approver HR | Evaluasi bukti & komparasi nilai | Keputusan `Approved`, `NeedRevision`, atau `Rejected` | Penolakan wajib menyertakan alasan tertulis |
| Penerapan Data (*Apply*) | Background Service / Applier | Permohonan `Approved` | Status `Applied`, master data resmi terbarui | Nilai lama berubah di master saat permohonan berjalan -> deteksi konflik penimpaan |

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
| Perubahan Data Mandiri (ESS) & Kuasa HR (On-Behalf) | `HRD-DEC-052`, `HRD-DEC-091`, `HRD-DEC-092`, `HRD-DEC-093`, `HRD-DEC-094` | `contracts/state-transition-matrix.md` §6.1, `api-contract.md`, `PLAN-REPAIR-HRD-003` | `AC-WFC-07`, `ESS-001` s.d. `ESS-005` |
| Batch Data Awal Penempatan Baseline | `HRD-DEC-054` | `contracts/state-transition-matrix.md` §6.4 | `AC-WFC-09` |
