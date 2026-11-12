# Proses — Penghasilan Variabel Manual (Insentif, Bonus, Honor Tugas Tambahan)

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1`, status `approved` (Yasmin, 2026-11-12) |
| `last_changed_in` | `PGJ-C1` |
| Traceability | `EPIC PGJ-04`; PRL-DEC-07, 31, 38, 43 |

Status pada diagram sama persis dengan `contracts/state-transition-matrix.md` bagian 1.

```mermaid
flowchart TD
    subgraph petugas[Petugas Payroll]
        A([Ada insentif, bonus, atau honor bulan ini]) --> B{Run periode ini masih menerima input?}
        B -- Tidak --> B1[/Ditolak, run sudah tidak menerima input/]
        B1 --> B2([Masukkan pada periode berikutnya])
        B -- Ya --> C{Satu baris atau banyak?}
        C -- Satu --> D[Isi satu input]
        C -- Banyak --> E[Impor berkas]
        E --> F{Semua baris benar?}
        F -- Tidak --> F1[/Impor dibatalkan beserta daftar baris salah/]
        F1 --> E
        F -- Ya --> G[(Draft)]
        D --> D1{Isian lengkap dan benar?}
        D1 -- Tidak --> D2[/Ditolak, isian salah/]
        D2 --> D
        D1 -- Ya --> G
        G --> H[Ajukan]
        H --> I[(Submitted)]
    end
    subgraph penyetuju[Petugas yang menyetujui, boleh orang yang sama]
        I --> J{Nominal dan alasan sesuai?}
        J -- Tidak --> K[Tolak dengan alasan]
        K --> K1[(Rejected)]
        K1 --> K2([Petugas membuat input baru bila perlu])
        J -- Ya --> L[Setujui]
        L --> M[(Approved)]
    end
    subgraph run[Pengesah run]
        M --> N{Run dihitung ulang setelah persetujuan?}
        N -- Belum --> N1[/Pengajuan run ditolak, hitung ulang dulu/]
        N1 --> N2[Hitung ulang run]
        N2 --> N
        N -- Sudah --> O[Ajukan dan sahkan run]
        O --> P[(Applied)]
        O -. run dibatalkan .-> Q[(Cancelled)]
    end
    P --> R([Penghasilan masuk slip bulan ini])
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Periksa run | Petugas Payroll | Run periode berjalan | Lanjut atau berhenti | Run sudah diajukan: input dimasukkan pada periode berikutnya |
| Isi satu input | Petugas Payroll | Pegawai di run, komponen penghasilan, jenis, nominal, alasan untuk honor | Input `Draft` | Petugas memperbaiki isian yang ditolak |
| Impor berkas | Petugas Payroll | Baris berisi nomor pegawai, kode komponen, jenis, nominal, catatan | Banyak input `Draft` | Seluruh impor dibatalkan; petugas memperbaiki baris yang disebut lalu mengimpor ulang |
| Ajukan | Petugas Payroll | Input `Draft` | Input `Submitted` | — |
| Setujui atau tolak | Petugas berhak menyetujui | Input `Submitted` | `Approved` atau `Rejected` | Ditolak wajib beralasan; input `Rejected` tidak bisa dihidupkan lagi |
| Batalkan | Petugas berhak membatalkan | Input `Draft`, `Submitted`, atau `Approved` | Input `Cancelled` | Alasan wajib |
| Hitung ulang run | Petugas Payroll | Ada input yang berubah setelah perhitungan terakhir | Run `Calculated` terbaru | — |
| Sahkan run | Pengesah run | Run `Submitted` | Input `Applied` | Run dikembalikan; input tetap `Approved` |

**Contoh jalur gagal yang paling sering:** petugas menyetujui insentif pukul 14.00, padahal run
terakhir dihitung pukul 13.00. Pengesah mencoba mengajukan run dan ditolak. Petugas menghitung
ulang run, lalu pengajuan diterima.
