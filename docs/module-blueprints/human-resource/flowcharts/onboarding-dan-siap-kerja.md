# Onboarding dan Siap Kerja

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Jenis | Flowchart proses |
| Slice | `S-C4-ONB` onboarding dan kontrak kerja (sub-modul 05) |
| Status | `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan" |
| Kesiapan | `READY FOR DESIGN` |
| Keputusan | `HRD-DEC-081`, `HRD-DEC-083`, `HRD-DEC-084`, `HRD-DEC-086`, `HRD-DEC-088`, `HRD-DEC-095` |

---

## 1. Apa yang digambar

Perjalanan pegawai baru dari serah terima rekrutmen sampai dinyatakan siap kerja, atau sampai batal
bergabung. Status pada node keadaan sama persis dengan `contracts/state-transition-matrix.md`
bagian 13.1 s.d. 13.3 dan 13.9.

## 2. Diagram — dari serah terima sampai kontrak ditandatangani

```mermaid
flowchart TD
    subgraph rekrutmen[HR Manager di Rekrutmen]
        A([Kandidat menerima tawaran dan lolos MCU]) --> B[Serahkan ke Onboarding]
    end
    subgraph sistem[Sistem]
        B --> C{Serah terima sudah punya kasus?}
        C -- Sudah --> C1[Tampilkan kasus yang sudah ada]
        C -- Belum --> D[Buat kasus, gabungkan template Umum, Klinis, dan Unit]
        D --> E[Susun draf kontrak dari tawaran]
        E --> F[Kirim permintaan akun]
        F --> G[(InProgress)]
    end
    subgraph hrd[Petugas HRD]
        G --> H{Pegawai datang pada tanggal mulai?}
        H -- Tidak --> H1[Batal Bergabung dengan alasan dan catatan]
        H1 --> H2[(Cancelled)]
        H -- Ya --> I[Catat hadir hari pertama]
        I --> J[Cetak draf kontrak, minta tanda tangan]
        J --> K{Scan bertanda tangan sudah diunggah?}
        K -- Belum --> K1[/Ditolak, scan wajib/]
        K1 --> J
        K -- Sudah --> L[Tandai sudah ditandatangani]
    end
    subgraph sistem2[Sistem]
        I --> M[Aktifkan akun untuk layanan mandiri]
        L --> N[(Active)]
        N --> O[Buat penempatan dan gaji awal dari tawaran]
        O --> P[Buat masa percobaan bila jenis kontrak mengizinkan]
    end
    H2 --> Q([Kursi kembali terbuka di Rekrutmen])
    P --> R([Lanjut ke checklist])
```

## 3. Diagram — checklist sampai siap kerja

```mermaid
flowchart TD
    subgraph hrd[Petugas HRD]
        A([Kasus berjalan]) --> B[Centang butir milik HRD]
        B --> B1{Butir wajib bukti?}
        B1 -- Ya, belum ada bukti --> B2[/Ditolak, unggah bukti/]
        B2 --> B
    end
    subgraph unit[Kepala Unit]
        A --> C[Centang butir milik unit]
        C --> D[Pilih atasan langsung]
        D --> E{Tenaga klinis?}
        E -- Ya --> F[Pilih preceptor]
        E -- Tidak --> G[Butir unit selesai]
        F --> G
    end
    subgraph sistem[Sistem]
        A --> H[Isi butir otomatis: kontrak, kelaikan, sertifikat wajib]
    end
    subgraph hrd2[Petugas HRD]
        B1 -- Tidak, atau bukti ada --> I{Seluruh butir wajib selesai?}
        G --> I
        H --> I
        I -- Belum --> I1[/Ditolak, butir wajib belum selesai/]
        I1 --> A
        I -- Sudah --> J{Yang memilih atasan langsung orang yang sama?}
        J -- Ya --> J1[/Ditolak, minta petugas HRD lain/]
        J -- Tidak --> K[Nyatakan Siap Kerja]
        K --> L[(Completed)]
    end
    subgraph sistem2[Sistem]
        L --> M[Status kepegawaian menjadi Aktif dan boleh dijadwalkan]
        M --> N[Buat relasi atasan, mulai pendampingan, minta akses kerja]
    end
    N --> O([Pegawai masuk jadwal dinas])
```

## 4. Tabel langkah

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Serahkan ke Onboarding | HR Manager | Tawaran final diterima, kelaikan layak, jenis dan lama kontrak pada tawaran | Kasus `InProgress`, draf kontrak `Draft`, permintaan akun | Serah terima tidak berpindah; HR Manager melengkapi syarat yang ditampilkan lalu mengulang |
| Batal Bergabung | Petugas HRD | Pegawai belum tercatat hadir; alasan dan catatan | Kasus `Cancelled`, profil nonaktif, kursi terbuka lagi | Ditolak bila pegawai sudah hadir; keluarnya diproses sebagai pengunduran diri |
| Catat hadir hari pertama | Petugas HRD | Tanggal hari ini tidak sebelum tanggal mulai rencana | Butir hadir selesai; akun aktif untuk layanan mandiri | Ditolak bila sebelum tanggal mulai; bila akun gagal dibuat, HRD melengkapi email lalu menekan ulang di layar permintaan akses |
| Tandai kontrak sudah ditandatangani | Petugas HRD | Scan kontrak bertanda tangan | Kontrak `Active`; penempatan dan gaji awal; masa percobaan bila PKWTT | Ditolak tanpa scan; bila isi kontrak harus berubah, minta Recruiter merevisi tawaran |
| Centang butir HRD dan unit | Petugas HRD, Kepala Unit | Bukti untuk butir wajib bukti; orang terpilih untuk butir pemilihan | Butir `Completed` | Ditolak; petugas mengunggah bukti atau memilih orang lain yang sah |
| Nyatakan Siap Kerja | Petugas HRD | Seluruh butir wajib selesai; pemilih atasan langsung orang lain | Kasus `Completed`; status Aktif; relasi atasan; pendampingan berjalan; permintaan akses kerja | Ditolak; HRD menyelesaikan butir yang tersisa atau meminta rekan HRD menyatakan |

## 5. Contoh

Rina, perawat ICU, diserahkan ke onboarding pada 15 Januari 2027 dengan tawaran PKWT 12 bulan mulai
1 Februari 2027. Kasus terbentuk dengan 10 butir. Pada 1 Februari Dodi mencatat Rina hadir, akun Rina
aktif untuk layanan mandiri, dan kontrak ditandatangani lalu scan-nya diunggah. Sari memilih dirinya
sendiri sebagai atasan langsung dan Tono sebagai preceptor. Sertifikat BTCLS Rina terbaca otomatis.
Pada 4 Februari Dodi menekan Nyatakan Siap Kerja; Rina dapat dijadwalkan mulai 5 Februari dengan
penanda Didampingi.

## 6. Yang sengaja tidak ada di diagram ini

| Yang tidak digambar | Di mana ia digambar |
| --- | --- |
| Seleksi, tawaran, MCU | `rekrutmen-dan-seleksi.md` |
| Pendampingan sampai siap mandiri | `pendampingan-klinis.md` |
| Masa percobaan | `masa-percobaan.md` |
| Pemberian peran kerja oleh IT | Layar permintaan akses; bukan alur onboarding |

## 7. Traceability

| Bagian alur | Decision | Acceptance test |
| --- | --- | --- |
| Kasus otomatis dan batal bergabung | `HRD-DEC-083` | `AT-HRD-ONB-01` s.d. `AT-HRD-ONB-05` |
| Kontrak pertama | `HRD-DEC-084`, `HRD-DEC-095` | `AT-HRD-ONB-06` s.d. `AT-HRD-ONB-08` |
| Checklist | `HRD-DEC-086` | `AT-HRD-ONB-16` s.d. `AT-HRD-ONB-20` |
| Siap kerja dan akun | `HRD-DEC-088` | `AT-HRD-ONB-21` s.d. `AT-HRD-ONB-26` |
