# Flowchart — Reservasi Pasien Prioritas (Amendment RP)

| Field | Nilai |
|---|---|
| Revisi blueprint | `33` (`approved` 2026-10-16) |
| Keputusan | `RJ-PRI-DEC-008` s.d. `029` ([00-interview-decisions.md](../00-interview-decisions.md), *Amendment RP*) |
| Status pada node | Sama persis dengan [state-transition-matrix.md](../contracts/state-transition-matrix.md) *Amendment RP* |

Flowchart ini menggambarkan langkah petugas dan pasien. Kalimat pesan penolakan ada di
[validation-matrix.md](../contracts/validation-matrix.md) *Amendment RP*, dan tidak disalin di sini.

## 1. Membuat reservasi dan mengirim link

```mermaid
flowchart TD
    subgraph pasien1[Pasien]
        A([Pasien menghubungi RS lewat WA atau telepon])
    end
    subgraph admisi1[Admisi Rawat Jalan]
        A --> B[Cari pasien dengan No. RM, No. Telp, atau NIK]
        B --> C{Member aktif hari ini?}
        C -- Tidak --> C1[/Ditolak, bukan member aktif/]
        C1 --> C2([Daftarkan lewat Pendaftaran Pasien dengan poli dan dokter])
        C -- Ya --> D[Pilih tanggal kunjungan]
        D --> E{Tanggal dalam jendela reservasi?}
        E -- Tidak --> E1[/Ditolak, tanggal di luar jendela/]
        E1 --> D
        E -- Ya --> F{Sudah ada reservasi terbuka atau kunjungan aktif di tanggal itu?}
        F -- Ya --> F1[/Ditolak, buka reservasi atau kunjungan yang ada/]
        F1 --> F2([Lanjutkan dengan data yang sudah ada])
        F -- Tidak --> G[(Menunggu Kedatangan)]
        G --> H[Tekan Kirim via WhatsApp lalu kirim pesan]
    end
    subgraph pasien2[Pasien]
        H --> I([Pasien menerima nomor reservasi dan link])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Cari pasien | Admisi | No. RM, No. Telp, atau NIK | Pasien terpilih beserta label member | Pasien belum terdaftar: daftarkan pasien baru dulu (`CRM-DEC-003`) |
| Periksa kelayakan | Sistem | Pasien, hari ini (WIB) | Layak atau tidak | Bukan member Aktif: admisi mendaftarkan kunjungan biasa dengan poli dan dokter |
| Pilih tanggal | Admisi | Tanggal kunjungan | Tanggal sah | Di luar jendela: admisi memilih tanggal lain |
| Simpan | Admisi | Pasien, tanggal | Reservasi Menunggu Kedatangan, nomor, link | Ada reservasi terbuka atau kunjungan aktif: admisi membuka yang sudah ada |
| Kirim | Admisi | Teks pesan, No. HP pasien | Pesan WA terkirim dari akun RS | WA tidak terbuka: admisi menyalin link manual |

## 2. Kedatangan, skrining, dan pelengkapan

```mermaid
flowchart TD
    subgraph pasien[Pasien]
        A([Pasien tiba di RS]) --> B{Bisa membuka link?}
        B -- Ya --> C{Hari ini hari kunjungan?}
        C -- Belum --> C1[/Belum bisa konfirmasi/]
        C1 --> C2([Datang pada tanggal kunjungan])
        C -- Ya --> D[Tekan Saya sudah tiba]
        B -- Tidak --> B1[Lapor ke meja admisi]
    end
    subgraph admisi[Admisi]
        B1 --> B2[Tandai sudah tiba]
    end
    D --> E[(Sudah Tiba)]
    B2 --> E
    subgraph skrining[Petugas skrining dan admisi]
        E --> F[Layar daftar berbunyi, petugas menuju ruang tunggu prioritas]
        F --> G{Pasien ditemukan?}
        G -- Tidak --> G1[Kembalikan ke Belum Tiba dengan alasan]
        G1 --> G2[(Menunggu Kedatangan)]
        G -- Ya --> H[Isi checklist skrining awal]
        H --> I{Ada jawaban Ya?}
        I -- Ya --> I1[Arahkan pasien sesuai SOP PPI]
        I1 --> J
        I -- Tidak --> J[Admisi membuka Lengkapi di Pendaftaran Pasien]
        J --> K{Jadwal dan kuota dokter sah?}
        K -- Tidak --> K1[/Ditolak, pilih dokter atau jadwal lain/]
        K1 --> J
        K -- Ya --> L[(Menjadi Kunjungan)]
    end
    subgraph klinis[Perawat dan dokter]
        L --> M[Nomor prioritas terbit, tidak tampil di display]
        M --> N([Perawat lalu dokter menghampiri pasien di ruang tunggu prioritas])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Konfirmasi lewat link | Pasien | Link dari WA | Reservasi Sudah Tiba | Belum hari H: pasien datang pada tanggalnya. Link tidak berlaku: pasien melapor ke admisi |
| Tandai tiba | Admisi | Reservasi Menunggu Kedatangan | Sudah Tiba, sumber "petugas" | Bukan hari kunjungan: admisi memeriksa tanggal reservasi |
| Menghampiri | Petugas skrining, admisi | Kabar di layar daftar | Pasien ditemukan | Tidak ditemukan: kembalikan ke Belum Tiba dengan alasan, dan link aktif lagi |
| Checklist skrining | Petugas skrining | Batuk, demam, risiko jatuh | Checklist tersimpan | Isian kurang: lengkapi ketiganya. Ada "Ya": ikuti SOP PPI |
| Lengkapi | Admisi | Poli, dokter, jadwal, penjamin, consent | Kunjungan dan nomor antrean | Jadwal atau kuota tidak sah: pilih dokter atau jadwal lain. Reservasi tetap Sudah Tiba |
| Menghampiri pasien | Perawat, dokter | Penanda lokasi di worklist | Pasien dilayani | Pasien pindah tempat: petugas menghubungi lewat admisi |

## 3. Jalur penutupan dan pengecualian

```mermaid
flowchart TD
    A([Reservasi Menunggu Kedatangan atau Sudah Tiba]) --> B{Apa yang terjadi?}
    B -- Pasien minta batal, atau ganti tanggal pada hari kunjungan --> C[Admisi membatalkan dengan alasan]
    C --> C1[(Dibatalkan)]
    C1 --> C2{Ganti tanggal?}
    C2 -- Ya --> C3([Buat reservasi baru, kelayakan dinilai ulang])
    C2 -- Tidak --> C4([Selesai])
    B -- Pasien memakai kiosk atau loket biasa --> D[Kunjungan terbentuk lewat jalur biasa]
    D --> D1[(Didaftarkan lewat Jalur Lain)]
    B -- Hari kunjungan berakhir --> E[Sistem menutup pada malam hari]
    E --> E1[(Tidak Datang)]
    B -- Pesan WA hilang --> F[Admisi kirim ulang link baru]
    F --> F1([Link lama mati, pasien memakai link baru])
    B -- Membership dibatalkan CRM --> G[Reservasi bertanda bukan prioritas lagi]
    G --> G1([Saat dilengkapi, pasien mendapat nomor Reguler])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Batal | Admisi | Alasan 10–500 karakter | Dibatalkan, link mati | Alasan kurang: lengkapi alasan |
| Ganti tanggal pada hari kunjungan | Admisi | Tanggal baru | Reservasi lama Dibatalkan, reservasi baru | Pasien sudah bukan member Aktif: daftarkan kunjungan biasa. Sebelum hari kunjungan, pakai bagian 4 (revisi `34`) |
| Jalur lain | Sistem | Kunjungan RJ berpoliklinik pada tanggal yang sama | Didaftarkan lewat Jalur Lain | Penutupan gagal: kunjungan ikut batal, dan petugas mengulang |
| Penutupan malam | Sistem | Tanggal kunjungan sudah lewat | Tidak Datang | Server mati: putaran berikutnya mengejar |
| Kirim ulang | Admisi | Reservasi Menunggu Kedatangan | Link baru | Pasien sudah tiba: link tidak dibutuhkan |
| Bukan prioritas lagi | Sistem (dibaca) | Status membership snapshot | Tanda di daftar dan detail | Admisi menghubungi pasien |

## 4. Ganti tanggal sebelum hari kunjungan (revisi `34`, `approved` 2026-10-21)

Keputusan `RJ-PRI-DEC-034` s.d. `038`. Pada hari kunjungan, jalurnya tetap bagian 3 (batal lalu buat baru).

```mermaid
flowchart TD
    subgraph pasien1[Pasien]
        A([Pasien minta pindah tanggal lewat WA atau telepon])
    end
    subgraph admisi[Admisi Rawat Jalan]
        A --> B[Buka detail reservasi]
        B --> C{Masih Menunggu Kedatangan dan belum hari kunjungan?}
        C -- Tidak --> C1[/Ganti tanggal tidak tersedia/]
        C1 --> C2([Batalkan lalu buat reservasi baru, bagian 3])
        C -- Ya --> D[Pilih tanggal baru dan isi alasan]
        D --> E{Tanggal baru sah?}
        E -- Tidak --> E1[/Ditolak, pilih tanggal lain/]
        E1 --> D
        E -- Ya --> F{Masih member aktif hari ini?}
        F -- Tidak --> F1[/Ditolak, reservasi lama tetap berlaku/]
        F1 --> F2([Sampaikan ke pasien: tetap datang di tanggal lama, atau batalkan])
        F -- Ya --> G{Tanggal baru sudah ada reservasi atau kunjungan?}
        G -- Ya --> G1[/Ditolak, buka data yang ada/]
        G -- Tidak --> H[(Menunggu Kedatangan pada tanggal baru)]
        H --> I[Kirim link baru lewat WhatsApp]
    end
    subgraph pasien2[Pasien]
        I --> J([Pasien memakai link baru pada tanggal baru. Link lama tidak berlaku])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Buka detail | Admisi | Nomor reservasi atau nama pasien | Detail dengan tombol Ganti Tanggal | Tombol tidak tampil: sudah hari kunjungan atau status bukan Menunggu Kedatangan. Admisi memakai bagian 3 |
| Pilih tanggal dan alasan | Admisi | Tanggal baru, alasan 10–500 karakter | Permintaan ganti tanggal | Tanggal sama, lampau, atau terlalu jauh: pilih tanggal lain |
| Nilai ulang member | Sistem | Pasien, hari ini (WIB) | Member Aktif atau tidak | Bukan member Aktif: reservasi lama tetap berlaku. Pasien datang di tanggal lama, atau admisi membatalkan atas permintaan pasien |
| Periksa tanggal baru | Sistem | Pasien, tanggal baru | Tanggal kosong | Sudah ada reservasi atau kunjungan: admisi membuka data yang ada |
| Simpan | Sistem | — | Tanggal baru, snapshot baru, link baru, riwayat | Diubah petugas lain di saat yang sama: muat ulang lalu ulangi |
| Kirim | Admisi | Teks pesan versi ganti tanggal | Pesan WA terkirim | WA tidak terbuka: salin link manual |
