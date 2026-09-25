# Farmasi — Arsitektur Frontend Routing Depo

Status: `approved` oleh product/domain owner pada 21 Agustus 2026.

Tidak ada layar, route, menu, atau Redux state baru untuk routing. Pemilihan Depo berjalan di backend. UI workflow Farmasi hanya perlu menampilkan pesan backend ketika konfigurasi tidak ditemukan atau ambigu, mempertahankan tombol coba lagi, dan tidak menawarkan pemilihan Depo manual sebagai fallback.

| Keadaan | Perilaku UI | Wewenang |
| --- | --- | --- |
| Routing berhasil | Lanjutkan workflow tanpa dialog pemilihan | Backend authoritative |
| Tidak ada kandidat | Tampilkan pesan konfigurasi dan hentikan proses | Backend authoritative |
| Kandidat ganda | Tampilkan pesan konfigurasi dan hentikan proses | Backend authoritative |
| Gangguan jaringan | Tampilkan retry; jangan menganggap routing berhasil | Pola project existing |

Teks visual, posisi alert, dan komponen pesan adalah `DEV_DISCRETION` selama memakai design system existing. Menampilkan seluruh kandidat lokasi kepada pengguna tidak diizinkan karena dapat membocorkan konfigurasi dan mendorong fallback manual.

---

# Slice Financial Clearance — kebutuhan frontend

Status **draft** · input `PHA-DEC-063`–`070` · 21 September 2026.

## Tidak ada layar baru, tetapi ada yang wajib terlihat

Slice ini **tidak menambah layar, route, maupun butir menu**. Yang berubah adalah apa yang
tampil pada layar kerja Farmasi yang sudah ada.

Alasannya: petugas tidak perlu tempat baru untuk memeriksa pembayaran — ia perlu tahu, **di
tempat ia sedang bekerja**, mengapa sebuah resep tidak bisa dilanjutkan.

## Peta butir menu

**Nol butir menu baru.** Seluruh layar yang terdampak sudah punya butir menunya masing-masing
pada blueprint Farmasi yang berjalan. Slice ini tidak menambah keterjangkauan baru.

## Skema fitur — tambahan pada layar kerja resep

```text
┌────────────────────────────────────────────────────────────────┐
│  Antrean Resep Farmasi                                         │
├────────────────────────────────────────────────────────────────┤
│  Pasien          │ No. Resep │ Keadaan       │ Tindakan        │
│  ────────────────┼───────────┼───────────────┼──────────────── │
│  Budi S.         │ RX-00871  │ Siap ditelaah │ [ Telaah ]      │
│  Siti A.         │ RX-00872  │ ⏸ Ditahan     │ [ Telaah ]✕     │  ← A
│                  │           │   tagihan     │                 │
│                  │           │   berubah     │                 │
│  Andi P.         │ RX-00873  │ ⧗ Menunggu    │ [ Telaah ]✕     │  ← B
│                  │           │   pembayaran  │                 │
└────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Kolom Keadaan | Keadaan pemenuhan resep, ditambah penanda tahan finansial beserta alasannya bila ada | Response layar kerja resep yang sudah ada, dengan field tambahan | `Prescription : Read` — tidak berubah | Antrean kosong: "Tidak ada resep menunggu." | Daftar gagal dimuat: pesan beserta tombol muat ulang |
| A — Resep ditahan | Penanda visual dan alasan singkat mengapa pekerjaan terhenti | Sama | Tombol lanjut **dinonaktifkan** | — | — |
| B — Menunggu pembayaran | Penanda bahwa kasir belum mengonfirmasi | Sama | Tombol lanjut **dinonaktifkan** | — | — |

### Mengapa tombolnya dinonaktifkan, bukan disembunyikan

Berbeda dari tombol yang dijaga hak akses — yang **MUST** disembunyikan bagi peran tak berwenang
— tombol di sini dinonaktifkan dan tetap terlihat. Sebabnya: petugas **memang berwenang**
menelaah resep; yang belum siap adalah resepnya. Menyembunyikan tombol akan membuat petugas
mengira ia kehilangan kewenangan, lalu menghubungi administrator untuk masalah yang sebenarnya
ada di kasir.

Alasan penahanan **MUST** terbaca tanpa petugas perlu mengklik apa pun. `PHA-DEC-069` mencatat
ini sebagai keharusan karena petugas bisa sedang memegang obatnya saat penahanan terjadi.

## Penanganan keadaan

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka daftar seperti yang sudah berjalan |
| Keadaan finansial belum diketahui | Ditampilkan apa adanya sebagai belum diketahui, **bukan** sebagai siap dikerjakan |
| Salinan tertinggal atau sinkronisasi bermasalah | Ditampilkan sebagai tidak dapat dipastikan; tombol lanjut dinonaktifkan; **tidak** ada tombol coba paksa |
| Data basi di layar | Daftar dimuat ulang setelah tindakan apa pun yang berhasil |
| Pengiriman ganda | Tombol lanjut dikunci sampai jawaban kembali |

## Yang sengaja tidak dibuat

| Yang wajar diharapkan | Alasan |
| --- | --- |
| Tombol menandai resep sudah dibayar | Kewenangan itu dihapus permanen dari Farmasi |
| Tombol memaksa lanjut saat sinkronisasi bermasalah | `PHA-DEC-067` — tidak ada override bagi peran mana pun |
| Tombol menyinkronkan ulang secara manual | Mengundang kebiasaan menekannya sampai hasilnya menyenangkan |
| Layar khusus keadaan finansial | Petugas butuh keterangan di tempat ia bekerja, bukan layar terpisah |

## Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Keberadaan penanda, keterbacaan alasan, tombol dinonaktifkan saat ditahan | Terkunci dokumen ini — ketiganya menyangkut keselamatan dan kejelasan |
| Bunyi pesan bagi pengguna | Mengikuti `contracts/validation-matrix.md`, bukan dikarang ulang di frontend |
| Ikon, warna penanda, posisi kolom, komponen | `DEV_DISCRETION` |

Trace `PHA-DEC-063`, `PHA-DEC-067`, `PHA-DEC-069`. Tests `PHA-AT-CLR-10`.
