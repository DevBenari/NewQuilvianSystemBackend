# Alur Paket B — Dashboard SDM dan Worklist

| Field | Nilai |
| --- | --- |
| Dasar | Decision log bagian 57 (`HRD-DEC-237` s.d. `HRD-DEC-246`); backend 38; frontend 35; kontrak `v30`; `HRD-DEC-247` |
| Status | `approved` — pengguna, 23 November 2026 ("1. ya setujui 2. A"; `HRD-DEC-247`) |
| `last_changed_in` | `v30` — 22 November 2026 |

Dokumen ini menggambar urutan kerja petugas pada lima pintu masuk Paket B. Aturan internal penapisan, evaluasi, dan disiplin tetap pada flowchart `rekrutmen-dan-seleksi.md` dan `hubungan-karyawan-dan-disiplin.md`.

## 1. Manajer SDM memantau dan menindaklanjuti

```mermaid
flowchart TD
    subgraph MSDM[Manajer SDM]
        A[Buka Dashboard SDM] --> B{Data berhasil dimuat?}
        B -- Tidak --> B1[Lihat pesan gagal, tekan Coba lagi]
        B1 --> A
        B -- Ya --> C[Baca lima panel]
        C --> D{Ada angka yang perlu ditindaklanjuti?}
        D -- Tidak --> Z[Selesai]
        D -- Ya --> E[Klik angka]
        E --> F[Lihat daftar nama]
        F --> G[Buka profil pegawai]
        G --> H[Tindak lanjut di layar pemilik: perpanjang kontrak, minta perpanjangan SIP, jadwalkan pelatihan]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Buka dashboard | Manajer SDM | — | Lima panel per tanggal operasional | Pesan gagal + Coba lagi; tidak ada angka nol palsu |
| Klik angka | Manajer SDM | Panel dan kelompok | Daftar nama | Pesan "Daftar gagal dimuat." + Coba lagi |
| Tindak lanjut | Manajer SDM / petugas terkait | Profil pegawai | Pekerjaan di layar Kontrak, Kredensial, atau Diklat | Mengikuti alur layar pemilik |

## 2. Recruiter menapis lamaran

```mermaid
flowchart TD
    subgraph REC[Recruiter]
        A[Buka Seleksi Administrasi] --> B[Daftar lamaran Menunggu]
        B --> C{Ada lamaran?}
        C -- Tidak --> Z[Selesai]
        C -- Ya --> D[Periksa berkas lamaran]
        D --> E{Berkas lengkap dan sah?}
        E -- Ya --> F[Tandai Lolos]
        E -- Tidak --> G[Tandai Gugur]
        E -- Surat perpanjangan STR --> H[Verifikasi manual surat]
        H --> E
    end
    subgraph SYS[Sistem]
        S1[Pelamar klinis tanpa STR berlaku sudah tampil Gugur]
    end
    S1 -.-> B
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Buka daftar | Recruiter | Filter lowongan/status | Satu baris per lamaran | Pesan gagal + Coba lagi |
| Tandai Lolos/Gugur | Recruiter | Hasil pemeriksaan | Status penapisan berubah | Simpan gagal → status tidak berubah, petugas mengulang; tombol dikunci selama menyimpan |

## 3. Asesor mengisi uji klinis/teknis

```mermaid
flowchart TD
    subgraph AS[Kepala Unit / Asesor]
        A[Buka Tes/Wawancara] --> B[Pilih jalur Uji Klinis/Teknis]
        B --> C[Lihat uji yang ditugaskan]
        C --> D{Ada uji yang belum dinilai?}
        D -- Tidak --> Z[Selesai]
        D -- Ya --> E[Buka uji]
        E --> F[Isi nilai dan rekomendasi]
        F --> G{Penilai yang ditugaskan?}
        G -- Ya --> H[Nilai tersimpan]
        G -- Tidak --> I[Ditolak: ditugaskan kepada penilai lain]
        H --> J{Rekomendasi Tidak Direkomendasikan?}
        J -- Ya --> K[Kandidat Gagal Asesmen, terkunci dari penawaran]
        J -- Tidak --> Z
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Lihat uji | Asesor | Jalur | Hanya uji miliknya | Kosong → "Belum ada uji yang ditugaskan kepada Anda." |
| Kirim nilai | Asesor | Nilai, rekomendasi | Evaluasi tersimpan | Bukan penilainya → ditolak; minta HR menugaskan ulang |

## 4. HR Relations meninjau kasus disiplin

```mermaid
flowchart TD
    subgraph HRR[HR Relations]
        A[Buka Tindak Disiplin] --> B[Daftar sanksi seluruh pegawai]
        B --> C{Perlu melihat kasus yang sudah selesai?}
        C -- Ya --> D[Buka Riwayat Penyelesaian]
        D --> E[Sanksi kedaluwarsa atau dicabut, dan insiden ditutup tanpa sanksi]
        C -- Tidak --> F[Buka profil pegawai]
        F --> G[Bagian disiplin: sanksi resmi dan catatan lama]
    end
    subgraph AT[Atasan langsung]
        H[Buka profil bawahan] --> I[Bagian disiplin: hanya untuk HR]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Buka Tindak Disiplin | HR Relations | — | Daftar sanksi | Pesan gagal + Coba lagi |
| Riwayat Penyelesaian | HR Relations | — | Kasus selesai, baca-saja | Tanpa hak insiden → bagian insiden tidak tampil |
| Profil pegawai | HR Relations / atasan | — | HR melihat riwayat; atasan melihat pemberitahuan | — |
