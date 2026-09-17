# Proses: Pesanan Tindakan, Pesanan Penunjang, dan Verifikasi Instruksi

| Field | Nilai |
| --- | --- |
| Sub-modul | `dokter-rawat-inap` — kontrak pesanan; pesanan perawat dibuat dari ruang kerja `keperawatan` |
| Revision | `0.3` — berkas baru, blueprint revision `7` |
| Status | `draft` — belum disetujui manusia |
| Isi | Seluruh percabangan beserta jalur pengecualiannya |
| Kemampuan | `CAP-024`, `CAP-015-LAB`, `CAP-015-RAD` |
| Keputusan | `RWI-DEC-114`, `RWI-DEC-139`, `RWI-DEC-143` |
| Kontrak | Status sebaris dengan `contracts/state-transition-matrix.md` bagian 8.3 |

---

## 1. Membuat pesanan

```mermaid
flowchart TD
    subgraph perawat[Perawat]
        A([Menerima instruksi telepon dokter]) --> B[Membacakan ulang instruksi]
        B --> C[Membuat pesanan tindakan atau penunjang]
        C --> D{Dokter pemberi instruksi dipilih?}
        D -- Tidak --> D1[/Ditolak: pilih dokter/]
        D1 --> C
        D -- Ya --> E{Dokter itu bertugas atas pasien?}
        E -- Tidak --> E1[/Ditolak: dokter tidak bertugas/]
        E1 --> C
        E -- Ya --> F[(Pesanan Ordered, verifikasi Pending)]
    end
    subgraph dokter[Dokter]
        G([Dokter memesan sendiri]) --> H{Dokter bertugas saat ini?}
        H -- Tidak --> H1[/Ditolak: tidak bertugas/]
        H -- Ya --> I[(Pesanan Ordered, tanpa verifikasi)]
    end
    F --> Z([Pesanan langsung diteruskan ke layanan tujuan])
    I --> Z
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Membacakan ulang | Perawat | Instruksi dokter | Instruksi terkonfirmasi lisan | — |
| Membuat pesanan perawat | Perawat di unit pasien | Tindakan atau pemeriksaan, dokter pemberi instruksi | Pesanan `Ordered`, verifikasi `Pending`, penginput dari akun login | Dokter tidak bertugas → perawat menghubungi dokter yang bertugas |
| Membuat pesanan dokter | Dokter bertugas | Tindakan atau pemeriksaan | Pesanan `Ordered` | Tidak bertugas → meminta penugasan |
| Pesanan laboratorium dan radiologi perawat | Perawat | Sama | Sama | **Gerbang implementasi**: persetujuan pemilik modul Laboratorium dan Radiologi belum tercatat |

## 2. Mengubah, membatalkan, melaksanakan, dan memverifikasi

```mermaid
flowchart TD
    subgraph pelaku[Pengguna yang membuka pesanan]
        A([Membuka pesanan tertunda]) --> B{Aksi yang dipilih}
        B -- Ubah --> C{Pengguna penginput?}
        C -- Tidak --> C1[/Ditolak: hanya penginput/]
        C -- Ya --> C2[(Pesanan Ordered diperbarui)]
        B -- Batalkan --> D{Penginput atau DPJP aktif?}
        D -- Tidak --> D1[/Ditolak: tidak berwenang membatalkan/]
        D -- Ya --> D2{Alasan diisi dan belum tertagih?}
        D2 -- Tidak --> D3[/Ditolak: alasan kosong atau sudah tertagih/]
        D2 -- Ya --> D4[(Pesanan Cancelled)]
        B -- Laksanakan --> E[(Pesanan Completed, pelaksana jadi penulis)]
        B -- Verifikasi --> F{Pengguna dokter pemberi instruksi?}
        F -- Tidak --> F1[/Ditolak: hanya pemberi instruksi/]
        F -- Ya --> F2[(Verifikasi Verified)]
    end
    subgraph sistem[Sistem saat episode ditutup]
        G([Episode ditutup]) --> H{Pesanan tertunda sudah tertagih?}
        H -- Tidak --> I[(Pesanan Cancelled otomatis)]
        H -- Ya --> J[Dibiarkan dan masuk daftar pantau]
    end
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Mengubah | Penginput | Pesanan belum dilaksanakan | Isi pesanan berubah | Bukan penginput → meminta penginput mengubah, atau DPJP membatalkan lalu memesan ulang |
| Membatalkan | Penginput atau DPJP aktif | Alasan | Pesanan `Cancelled` | Sudah tertagih → ditangani bersama Billing |
| Melaksanakan | Dokter atau perawat berwenang | Pesanan `Ordered` | Pesanan `Completed`; catatan pelaksanaan final atas nama pelaksana | Episode ditutup → tidak dapat dilaksanakan |
| Memverifikasi | Dokter pemberi instruksi | Pesanan perawat `Pending` | Verifikasi `Verified`; penginput dan isi tidak berubah | Bukan pemberi instruksi → pesanan tetap menunggu pemberi instruksi |
| Penutupan episode | Sistem | Pesanan tertunda | Pesanan belum tertagih `Cancelled` beralasan "episode ditutup sebelum dilaksanakan" | Pesanan tertagih dibiarkan dan tampil pada daftar pantau episode |

**Contoh.** 23.00 dr. Yoga menelepon Ns. Siti; Siti membuat pesanan cek GDS dengan pemberi instruksi dr. Yoga. Pesanan
langsung ke laboratorium. 07.30 esoknya dr. Yoga membuka Perlu Review dan memverifikasi. Bila pagi itu dr. Rina yang
membuka pesanan yang sama, tombol verifikasi tidak ada baginya.
