# Proses — Sesi Terapi dan Handoff Billing (FEAT-016..018, FEAT-032)

```mermaid
flowchart TD
    subgraph terapis[Terapis]
        A([Pasien hadir atau dikunjungi di bangsal]) --> B{Pasien datang?}
        B -- Tidak --> B1[Catat tidak terlaksana beserta alasan]
        B1 --> Z1[Tanda tangan catatan]
        Z1 --> Z2([Sesi NotDone, tidak tertagih])
        B -- Ya --> C[Pilih kunjungan hari ini]
        C --> D[Cek identitas, program aktif, kondisi, kontraindikasi]
    end
    subgraph sistem[Sistem]
        D --> E{Kunjungan terbuka dan program aktif?}
        E -- Tidak --> E1[/Ditolak, kunjungan atau program tidak sah/]
        E1 --> C
    end
    subgraph terapis2[Terapis]
        E -- Ya --> F{Aman dimulai?}
        F -- Tidak --> B1
        F -- Ya --> G[(Sesi InProgress)]
        G --> H[Beri terapi, catat tindakan aktual dan respons]
        H --> I{Kondisi memburuk atau perlu deviasi?}
        I -- Ya, hentikan --> J[Catat alasan, tindakan segera, pihak dihubungi]
        J --> K[(Sesi StoppedEarly)]
        J --> J2[Program ditandai perlu telaah]
        I -- Tidak --> L[(Sesi Completed)]
        K --> M[Tanda tangan]
        L --> M
    end
    subgraph sistem2[Sistem]
        M --> N[Bentuk tindakan bertarif per intervensi]
        N --> O{Terkirim ke Billing?}
        O -- Ya --> P([Handoff Succeeded])
        O -- Tidak pasti / gagal --> Q[(Handoff Pending / Failed)]
    end
    subgraph billing[Petugas billing]
        Q --> R[Periksa galat, perbaiki tarif bila perlu, ulang]
        R --> O
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Catat tidak terlaksana | Terapis | Alasan (tidak hadir, batal, ditunda, menolak, kondisi klinis, sumber daya) | Sesi `NotDone` final | — |
| Cek keselamatan | Terapis | Identitas, program, kondisi | Boleh mulai / tidak | Kontraindikasi dicatat. Sesi tidak dimulai, atau dicatat tidak terlaksana. |
| Mulai | Sistem | Kunjungan + program | `InProgress` | Kunjungan sudah ditutup atau pasien RWI sudah pulang: daftarkan kunjungan hari ini |
| Henti dini | Terapis | Alasan, tindakan segera | `StoppedEarly`, program perlu telaah | — |
| Tanda tangan | Terapis | Respons terisi | Catatan final, tindakan bertarif | Respons kosong: lengkapi |
| Handoff | Sistem / billing | Tindakan bertarif | `Succeeded` | `Failed`: billing memeriksa tarif lalu menekan Ulang. Tidak tertagih dua kali. |
