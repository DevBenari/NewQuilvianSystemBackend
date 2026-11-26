# Proses — Status Vendor: Dibatasi dan Blokir

Status dokumen: `approved` (`PBJ-DEC-062`). Keputusan: `PBJ-DEC-021`, `047`, `048`, `054`, `060`; `PBJ-ASM-003`.

**Tujuan:** vendor bermasalah dapat dibatasi cepat atau diblokir dengan persetujuan, dan statusnya sama di seluruh sistem.
**Pemicu:** penilaian merekomendasikan tindakan, atau kejadian di lapangan (keterlambatan, NCR).
**Prasyarat:** vendor punya profil; untuk blokir, alur approval vendor sudah dikonfigurasi admin.

```mermaid
flowchart TD
    subgraph manajer[Manajer Pengadaan]
        A([Ada alasan menindak vendor]) --> B{Jenis tindakan}
        B -- Batasi --> C{Vendor sedang Diblokir?}
        C -- Ya --> C1[/Ditolak, vendor sudah Diblokir/]
        C -- Tidak --> D[Isi alasan dan tanggal berakhir opsional]
        D --> E[(Dibatasi)]
        E --> E1{Dicabut atau tanggal berakhir lewat?}
        E1 -- Ya --> E2[(Aktif)]
        B -- Blokir atau buka blokir --> F{Ada usulan lain yang belum selesai?}
        F -- Ya --> F1[/Ditolak, selesaikan usulan sebelumnya/]
        F -- Tidak --> G[Buat usulan dengan alasan dan bukti penilaian]
        G --> H[Ajukan]
        H --> H1{Alur approval vendor aktif?}
        H1 -- Tidak --> H2[/Ditolak, hubungi admin/]
        H1 -- Ya --> I[(PENDING_APPROVAL)]
        R[(RETURNED)] --> G
        I --> W{Tarik?}
        W -- Ya --> W1[(CANCELLED)]
    end
    subgraph penyetuju[Penyetuju]
        W -- Tidak --> J{Keputusan}
        J -- Kembalikan --> R
        J -- Tolak --> K[(REJECTED)]
        J -- Setujui --> L[(APPROVED)]
    end
    subgraph sistem[Sistem]
        L --> M[Status blokir di Master Data diperbarui]
        M --> N[RFQ dan penetapan baru menolak vendor diblokir]
        M --> O[Proses berjalan diberi peringatan, tidak dibatalkan]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Batasi | Manajer Pengadaan | Alasan, tanggal berakhir | Dibatasi | Vendor Diblokir → tidak perlu dibatasi |
| Cabut | Manajer Pengadaan | Alasan | Aktif | — |
| Buat usulan | Manajer Pengadaan | Jenis, alasan, bukti | `DRAFT` | Usulan lain masih berjalan → selesaikan dulu |
| Ajukan | Manajer Pengadaan | — | `PENDING_APPROVAL`, nomor `UBV` | Approval belum dikonfigurasi → admin |
| Putuskan | Penyetuju | — | `APPROVED` / `REJECTED` / `RETURNED` | Penyetuju = pengusul → ditolak mesin approval |
| Terapkan | Sistem | Keputusan | Status blokir Master Data dan riwayat vendor | Penulisan gagal → keputusan tetap sah, usulan menunggu sinkron ulang |

**Contoh:** PT Alkes Maju terlambat tiga kali; Manajer membatasinya sampai 30-06-2027. Setelah tiga NCR kritis, Manajer
mengajukan UBV-000003 dengan bukti penilaian Semester I 2027; Direktur menyetujui; PT Alkes Maju Diblokir. Kontrak
KTR-2027-0008 yang sedang berjalan diberi peringatan, tidak dibatalkan.

**Master Data:** centang Blacklist di layar Supplier tampil nonaktif; perubahan hanya lewat proses ini (`PBJ-DEC-060`).
