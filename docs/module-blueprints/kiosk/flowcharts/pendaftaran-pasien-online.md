# Flowchart — Pendaftaran Pasien Online (Amendment PO)

| Field | Nilai |
| --- | --- |
| Revisi blueprint | `2` (`approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`)) |
| Keputusan | `KSK-DEC-029` s.d. `035`, `038`, `041` s.d. `044`, `047` |
| Status yang dipakai | Sama persis dengan `contracts/state-transition-matrix.md` *Amendment PO* |

Pelaku: pasien atau wali di HP/laptop tanpa login, dan sistem.

```mermaid
flowchart TD
    subgraph pasien[Pasien atau wali]
        A([Membuka halaman Pendaftaran Pasien Online]) --> B{Pernah berobat?}
        B -- Ya --> C[Isi NIK atau No. RM dan tanggal lahir]
        B -- Belum --> D[Isi NIK, nama, tanggal lahir, jenis kelamin]
        D --> E[Isi No. WhatsApp atau email]
        E --> F[Pilih jenis pasien]
        F --> G[Pilih tanggal, poli, dan jadwal dokter]
        G --> H{Jenis pasien Rujukan?}
        H -- Ya --> H1[Isi data rujukan dan unggah surat]
        H -- Tidak --> I
        H1 --> I[Pilih penjamin]
        I --> J{Pasien baru?}
        J -- Ya --> J1[Setujui persetujuan umum dan tanda tangan]
        J -- Tidak --> K
        J1 --> K[Periksa ringkasan, selesaikan captcha, kirim]
    end
    subgraph sistem[Sistem]
        C --> C1{Data pasien cocok?}
        C1 -- Tidak --> C2[/Ditolak, data tidak ditemukan/]
        C2 --> C
        C1 -- Ya --> C3[Tampilkan nama tersamar dan kartu milik pasien]
        C3 --> E
        K --> L{Captcha dan batas percobaan lolos?}
        L -- Tidak --> L1[/Ditolak, coba lagi nanti/]
        L1 --> K
        L -- Ya --> M{Tanggal besok s.d. 30 hari, jadwal tersedia, kuota ada?}
        M -- Tidak --> M1[/Ditolak, pilih jadwal atau tanggal lain/]
        M1 --> G
        M -- Ya --> N{Sudah ada pendaftaran atau kunjungan pada tanggal itu?}
        N -- Ya --> N1[/Ditolak, sudah terdaftar pada tanggal ini/]
        N1 --> G
        N -- Tidak --> O[(Menunggu Check-in)]
        O --> P{Email diisi?}
        P -- Ya --> P1[Kirim email berisi kode]
        P -- Tidak --> Q
        P1 --> Q[Tampilkan kode dan tombol WhatsApp]
    end
    Q --> R([Pasien menyimpan kode dan datang pada hari kunjungan])
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Isi data pasien lama | Pasien | NIK atau No. RM, tanggal lahir, captcha | Nama tersamar dan kartu milik pasien | Pesan umum; pasien memeriksa isian atau mendaftar di loket |
| Isi data pasien baru | Pasien | NIK, nama, tanggal lahir, jenis kelamin | — | Pesan per kolom |
| Isi kontak | Pasien | No. WA atau email | — | Kolom wajib diisi salah satu |
| Pilih jadwal | Pasien | Tanggal H+1 s.d. H+30, poli, jadwal | Jadwal dengan sisa kuota | Jadwal penuh tidak dapat dipilih; pasien memilih yang lain |
| Data rujukan | Pasien | Faskes perujuk (hanya faskes mitra pada tanggal kunjungan, `KSK-DEC-057`), surat rujukan maks. 5 MB | — | Berkas terlalu besar ditolak; pasien memilih berkas lain. Faskes perujuk tidak ada di daftar → pasien daftar di loket |
| Persetujuan umum | Pasien baru atau walinya | Klausul dan tanda tangan | — | Tombol lanjut tetap terkunci |
| Kirim | Pasien | Ringkasan dan captcha | Pendaftaran berstatus Menunggu Check-in, kode, email | Kuota habis atau sudah terdaftar: kembali ke langkah jadwal. Captcha atau batas percobaan: coba lagi nanti |
| Email | Sistem | Alamat email | Email berisi kode | Pendaftaran tetap tersimpan; kode tetap tampil; Lupa Kode tersedia di kiosk |
