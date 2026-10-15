# Alur Kelayakan Pulang Lintas Kunjungan

Masukan `BKC-DEC-144`, `BKC-AC-094`, dan keputusan arsitektur `BKC-DES-063`–`BKC-DES-069`.
Status **draft**, 14 Oktober 2026.

Dokumen ini memetakan urutan kerja ketika pasien rawat inap akan dipulangkan, dan sistem memeriksa
bukan hanya tagihan rawat inapnya, tetapi juga tagihan kunjungan asal yang merujuk admisi itu.

## Mengapa Alur Ini Ada

Pasien sering masuk lewat Rawat Jalan atau IGD lebih dulu sebelum dirawat inap. Setiap kunjungan
punya tagihan sendiri. Sebelum alur ini ada, kelayakan pulang hanya memeriksa tagihan rawat inap,
sehingga pasien bisa resmi pulang walau tagihan IGD-nya masih bersisa — dan piutang itu baru
ditemukan jauh setelahnya.

Alur ini memeriksa **seluruh rangkaian kunjungan yang benar-benar terhubung**, dengan dua pengaman
penting:

1. Kunjungan lama yang **tidak** berkaitan dengan admisi ini tidak boleh menahan kepulangan.
2. Satu kewajiban tidak boleh dihitung dua kali, termasuk yang sudah dibayar, dialihkan menjadi
   kredit, atau ditanggung penjamin.

Bila hubungan antar kunjungan ternyata tidak jelas, sistem **tidak** menebak. Ia menyerahkannya ke
petugas Billing atau Kasir untuk diperiksa.

## Istilah

| Istilah | Artinya di dokumen ini |
| --- | --- |
| Rangkaian kunjungan | Kunjungan rawat inap beserta kunjungan asal yang merujuknya, berlapis sampai ujung |
| Kunjungan asal | Kunjungan yang menghasilkan rujukan ke kunjungan berikutnya, misalnya IGD yang merujuk ke rawat inap |
| Kewajiban terbuka | Sisa tagihan yang masih menjadi tanggungan pasien, sesudah pembayaran, kredit, keringanan, dan tanggungan penjamin diperhitungkan |
| Perlu rekonsiliasi | Keadaan ketika hubungan antar kunjungan tidak lengkap atau ambigu, sehingga sistem belum dapat menyimpulkan |

---

## 1. Alur Pokok — Pemeriksaan Kelayakan Berjalan Normal

```mermaid
flowchart TD
    subgraph RawatInap
        A1[Perawat meminta evaluasi kelayakan pulang]
        A2[Perawat melihat hasil pada layar bangsal]
        A3[Pasien diizinkan meninggalkan bangsal]
    end

    subgraph SistemBilling
        B1[Sistem mengumpulkan rangkaian kunjungan pasien]
        B2[Sistem memeriksa setiap kunjungan milik pasien yang sama]
        B3[Sistem menghitung kewajiban terbuka tiap tagihan]
        B4[Sistem menjumlahkan tanpa menghitung ganda]
        B5{Masih ada kewajiban terbuka?}
        B6[Sistem menerbitkan izin pulang finansial]
        B7[Sistem menahan izin dan merinci penghambatnya]
    end

    subgraph Kasir
        C1[Kasir melihat tagihan mana yang menahan kepulangan]
        C2[Kasir menagih sisa tagihan ke keluarga pasien]
    end

    A1 --> B1 --> B2 --> B3 --> B4 --> B5
    B5 -->|Tidak| B6 --> A2
    B5 -->|Masih ada| B7 --> A2
    B7 --> C1 --> C2
    C2 --> B1
    A2 -->|Sudah diizinkan| A3
```

### Tabel Langkah Alur Pokok

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal / bermasalah |
| --- | --- | --- | --- | --- |
| 1. Minta evaluasi | Perawat bangsal atau sistem | Identitas kunjungan rawat inap | Permintaan penilaian kelayakan | Perawat menghubungi kasir bila status tidak bergerak |
| 2. Kumpulkan rangkaian kunjungan | Sistem Billing | Kunjungan rawat inap | Daftar kunjungan yang terhubung | Bila rangkaian tidak dapat dibaca utuh, lanjut ke jalur pengecualian 1 |
| 3. Periksa kepemilikan | Sistem Billing | Daftar kunjungan | Kunjungan milik pasien yang sama saja | Kunjungan milik pasien lain dikeluarkan dan ditandai untuk diperiksa petugas |
| 4. Hitung kewajiban tiap tagihan | Sistem Billing | Tagihan tiap kunjungan | Sisa tagihan pasien per tagihan | Tagihan yang tidak ditemukan ditandai untuk diperiksa petugas |
| 5. Jumlahkan tanpa hitung ganda | Sistem Billing | Sisa tiap tagihan | Total kewajiban terbuka | Kewajiban yang sudah dibayar atau dialihkan tidak dihitung lagi karena sudah ternetralkan di tagihannya masing-masing |
| 6. Terbitkan atau tahan izin | Sistem Billing | Total kewajiban | Izin pulang finansial, atau penahanan beserta rincian penghambat | — |
| 7. Tagih sisa | Kasir | Rincian penghambat | Pembayaran dari keluarga pasien | Keluarga pasien dapat melunasi beberapa tagihan sekaligus lewat pelunasan gabungan |
| 8. Izinkan pasien pulang | Perawat bangsal | Izin pulang finansial | Pasien diserahterimakan | Pasien tidak boleh meninggalkan bangsal selama izin belum terbit |

### Contoh Berangka

Pasien Ibu **Sari Mandasari** (nama samaran):

| Tingkat | Kunjungan | Tanggal | Sisa tagihan pasien |
| ---: | --- | --- | ---: |
| 0 | Rawat Inap | 9 Oktober 2026 | Rp 3.500.000 |
| 1 | IGD | 9 Oktober 2026 | Rp 0 — sudah lunas |
| 2 | Rawat Jalan | 2 Oktober 2026 | Rp 250.000 |

Total kewajiban terbuka = Rp 3.500.000 + Rp 0 + Rp 250.000 = **Rp 3.750.000**. Izin pulang
**ditahan**, dan kasir melihat dua tagihan yang menahannya.

Perhatikan tagihan IGD tetap ditampilkan walau nilainya nol, supaya kasir melihat seluruh rangkaian
dan dapat menjelaskannya ke keluarga pasien. Nilainya nol karena pembayarannya sudah diperhitungkan
pada tagihan itu sendiri — bukan karena diabaikan.

---

## 2. Jalur Pengecualian 1 — Hubungan Antar Kunjungan Tidak Jelas

Alur ini terjadi ketika rangkaian kunjungan tidak dapat ditelusuri utuh: rujukannya berputar,
rangkaiannya lebih panjang dari batas aman, atau ada kunjungan yang ternyata milik pasien lain.

```mermaid
flowchart TD
    subgraph SistemBilling
        A1[Sistem menelusuri rangkaian kunjungan]
        A2{Rangkaian terbaca utuh?}
        A3[Sistem tetap menghitung tagihan yang sudah ditemukan]
        A4[Sistem menandai keadaan perlu rekonsiliasi]
        A5[Sistem mencatat sebab ketidakjelasannya]
        A6[Sistem menahan kesimpulan izin pulang]
    end

    subgraph PetugasBilling
        B1[Petugas membuka daftar kewajiban yang ditemukan]
        B2[Petugas memeriksa hubungan antar kunjungan]
        B3{Hubungan sah?}
        B4[Petugas mengikutkan tagihan beserta alasan]
        B5[Petugas mengecualikan tagihan beserta alasan]
        B6[Sistem menghitung ulang lalu menetapkan izin]
    end

    subgraph RawatInap
        C1[Perawat melihat status perlu rekonsiliasi]
        C2[Perawat menahan kepulangan pasien]
    end

    A1 --> A2
    A2 -->|Tidak| A3 --> A4 --> A5 --> A6
    A6 --> C1 --> C2
    A6 --> B1 --> B2 --> B3
    B3 -->|Ya| B4 --> B6
    B3 -->|Tidak| B5 --> B6
```

### Tabel Langkah Rekonsiliasi

| Langkah | Pelaku | Masukan | Keluaran | Yang dilakukan petugas bila terjadi |
| --- | --- | --- | --- | --- |
| 1. Deteksi ketidakjelasan | Sistem Billing | Rangkaian kunjungan | Penandaan perlu rekonsiliasi beserta sebabnya | — |
| 2. Tetap hitung yang ditemukan | Sistem Billing | Tagihan yang berhasil dibaca | Angka kewajiban sementara | Sistem **tidak** menyimpulkan lunas, dan **tidak** membuat tagihan baru |
| 3. Tahan kesimpulan | Sistem Billing | Penandaan perlu rekonsiliasi | Izin pulang belum terbit | Perawat menahan kepulangan sampai status berubah |
| 4. Periksa hubungan | Petugas Billing atau Kasir | Daftar kewajiban yang ditemukan | Penilaian apakah tiap kunjungan memang terkait | Petugas memeriksa berkas pendaftaran dan rujukan pasien |
| 5. Putuskan beserta alasan | Petugas berwenang | Hasil pemeriksaan | Keputusan mengikutkan atau mengecualikan tiap tagihan | Alasan wajib diisi; keputusan tanpa alasan ditolak sistem |
| 6. Hitung ulang | Sistem Billing | Keputusan petugas | Izin pulang terbit atau tetap tertahan | Bila masih bersisa, kasir menagih sisanya seperti alur pokok |

### Sebab yang paling sering dijumpai

| Sebab | Penjelasan bagi petugas |
| --- | --- |
| Rujukan berputar | Dua kunjungan saling menunjuk sebagai asal, biasanya karena salah input saat pendaftaran |
| Rangkaian terlalu panjang | Penelusuran berhenti pada batas aman; masih ada kunjungan yang belum diperiksa |
| Kunjungan milik pasien lain | Kolom rujukan terisi keliru sehingga menunjuk kunjungan pasien yang berbeda |
| Tagihan kunjungan asal tidak ditemukan | Kunjungan tercatat tetapi tagihannya belum pernah dibuka |
| Lebih dari satu admisi aktif | Satu kunjungan asal tampak merujuk lebih dari satu admisi rawat inap yang masih berjalan, padahal aturannya hanya boleh satu |

---

## 3. Jalur Pengecualian 2 — Tagihan Susulan pada Kunjungan Lain

Alur ini terjadi ketika pasien sudah mendapat izin pulang, lalu tagihan susulan masuk — bukan pada
tagihan rawat inapnya, melainkan pada tagihan kunjungan asal.

```mermaid
flowchart TD
    subgraph PetugasLayanan
        A1[Petugas memasukkan biaya yang terlambat dicatat pada kunjungan asal]
    end

    subgraph SistemBilling
        B1[Sistem menerima biaya baru]
        B2[Sistem menilai ulang seluruh rangkaian kunjungan]
        B3{Izin pulang sudah terbit?}
        B4[Sistem mencabut izin pulang]
        B5[Sistem mencatat sebab pencabutan]
        B6[Sistem memancarkan sinyal penahanan ke bangsal]
        B7[Sistem menambahkan biaya ke kewajiban terbuka]
    end

    subgraph RawatInap
        C1[Layar bangsal berubah menjadi tertahan]
        C2[Perawat menahan kepulangan pasien]
        C3[Perawat mengarahkan keluarga ke loket kasir]
    end

    A1 --> B1 --> B2 --> B3
    B3 -->|Sudah| B4 --> B5 --> B6
    B3 -->|Belum| B7
    B6 --> C1 --> C2 --> C3
```

### Tabel Langkah Tagihan Susulan Lintas Kunjungan

| Langkah | Pelaku | Masukan | Keluaran | Yang dilakukan petugas bila terjadi |
| --- | --- | --- | --- | --- |
| 1. Masukkan biaya susulan | Petugas farmasi, laboratorium, atau ruangan | Layanan yang terlambat dicatat | Biaya baru pada tagihan kunjungan asal | Petugas wajib memberi catatan alasan keterlambatan |
| 2. Nilai ulang rangkaian | Sistem Billing | Biaya baru | Kewajiban terbuka dihitung ulang atas seluruh rangkaian | — |
| 3. Cabut izin pulang | Sistem Billing | Kewajiban bertambah | Izin pulang dicabut beserta sebabnya | Notifikasi dikirim ke bangsal seketika |
| 4. Tahan kepulangan | Perawat bangsal | Penanda tertahan | Pasien tertahan di bangsal | Perawat menjelaskan secara santun bahwa ada biaya susulan dari unit lain |
| 5. Lunasi selisih | Kasir dan keluarga pasien | Rincian biaya baru | Pembayaran tambahan | Boleh dibayar sendiri, boleh digabung dengan tagihan lain yang masih terbuka |

> Catatan penting: biaya susulan pada **kunjungan asal** kini dapat menahan kepulangan pasien rawat
> inap. Sebelum alur ini ada, hal itu tidak terjadi. Perubahan perilaku ini **wajib** disampaikan ke
> bangsal sebelum fitur diaktifkan, supaya perawat tidak menganggapnya kekeliruan sistem.

---

## 4. Jalur Pengecualian 3 — Pasien Tanpa Kunjungan Asal

Alur ini terjadi pada pasien yang langsung dirawat inap tanpa melalui Rawat Jalan maupun IGD.

```mermaid
flowchart TD
    subgraph SistemBilling
        A1[Sistem menelusuri rangkaian kunjungan]
        A2{Ada kunjungan asal?}
        A3[Sistem menilai tagihan rawat inap saja]
        A4[Sistem menerbitkan atau menahan izin seperti biasa]
    end

    subgraph RawatInap
        B1[Perawat melihat hasil pada layar bangsal]
    end

    A1 --> A2
    A2 -->|Tidak ada| A3 --> A4 --> B1
```

### Tabel Langkah Pasien Tanpa Kunjungan Asal

| Langkah | Pelaku | Masukan | Keluaran | Catatan |
| --- | --- | --- | --- | --- |
| 1. Telusuri rangkaian | Sistem Billing | Kunjungan rawat inap | Nol kunjungan asal ditemukan | Keadaan **normal**, bukan anomali, dan **tidak** menandai perlu rekonsiliasi |
| 2. Nilai tagihan rawat inap | Sistem Billing | Satu tagihan | Kewajiban terbuka | Hasilnya **identik** dengan perilaku sebelum alur ini ada |
| 3. Terbitkan atau tahan izin | Sistem Billing | Kewajiban terbuka | Izin pulang finansial | Tidak ada perbedaan yang dirasakan perawat maupun kasir |

Jalur ini digambarkan secara khusus karena inilah mayoritas pasien rawat inap. Penambahan penelusuran
rangkaian kunjungan **tidak boleh** mengubah apa pun bagi mereka — itu yang dijaga uji regresi.
