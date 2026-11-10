# Rekrutmen dan Seleksi Tenaga Kerja Rumah Sakit

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Jenis | Flowchart proses bisnis |
| Slice | `S-D2` Rekrutmen dan Hiring |
| Status | `draft` — mengikuti keputusan `HRD-DEC-073` s.d. `HRD-DEC-080` |
| Kesiapan | `READY FOR DESIGN` |
| Keputusan | `HRD-DEC-073`, `HRD-DEC-074`, `HRD-DEC-075`, `HRD-DEC-076`, `HRD-DEC-077`, `HRD-DEC-078`, `HRD-DEC-079`, `HRD-DEC-080` |

---

## 1. Apa yang digambar

Alur proses bisnis pengadaan tenaga kerja rumah sakit dari penarikan kursi formasi yang disetujui, penayangan lowongan,
pendaftaran pelamar dan penapisan STR klinis, evaluasi wawancara dan uji klinis dengan hak veto mutlak, penawaran kerja
bersyarat, MCU pra-kerja dengan masking rahasia medis, persetujuan penawaran bertingkat, pengelolaan talent pool cadangan,
hingga serah terima kandidat ke tahap onboarding tanpa membuat identitas ganda.

---

## 2. Diagram 1 — Dari Requisition Formasi hingga Penapisan STR Klinis

```mermaid
flowchart TD
    subgraph wfp[Sub-modul 03 Workforce Planning]
        A([Headcount Request Disetujui]) --> B[Antrean Kursi Luar Recruiter]
    end

    subgraph rec[HR Recruiter]
        B --> C[Tarik Kursi, Buat Draf Requisition]
        C --> D{Jumlah kursi <= sisa kuota?}
        D -- Tidak --> D1[/Ditolak sistem, kursi melebihi plafon/]
        D1 --> C
        D -- Ya --> E[Ajukan Requisition]
        E --> F[Disetujui HR Manager]
        F --> G[Buat & Tayangkan Lowongan Kerja]
    end

    subgraph pelamar[Pelamar / Kandidat]
        G --> H[Daftar & Unggah Berkas: KTP, Ijazah, STR]
        H --> I{Apakah NIK sudah terdaftar?}
        I -- Ya --> J[Tautkan ke Profil Lama / Rehire Flow]
        I -- Tidak --> K[Buat Entitas Pelamar Baru]
    end

    subgraph screening[Sistem & Recruiter Screening]
        J --> L{Apakah Posisi Klinis?}
        K --> L
        L -- Ya --> M{STR Aktif & Belum Kadaluwarsa?}
        M -- Tidak --> M1[Gugur Administrasi / Screening Rejected]
        M -- Ya --> N[Lolos Penapisan Administrasi]
        L -- Tidak --> N
        N --> O([Siap Dijadwalkan Wawancara & Uji Klinis])
    end
```

---

## 3. Diagram 2 — Evaluasi Wawancara, Uji Klinis (Hak Veto), MCU, dan Onboarding

```mermaid
flowchart TD
    subgraph eval[Evaluasi Dua Jalur Independen]
        A([Kandidat Lolos Screening]) --> B[Sesi 1: Wawancara HR]
        A --> C[Sesi 2: Uji Klinis Kepala Ruang / Asesor]
        B --> D[Formulir HR: Skor Perilaku & Rekomendasi]
        C --> E[Formulir Unit: Skor Klinis & Rekomendasi]
        E --> F{Rekomendasi Asesor Unit?}
        F -- NOT_RECOMMENDED --> F1[Gugur Mutlak / Hak Veto Klinis]
        F -- RECOMMENDED --> G{Rekomendasi HR?}
        G -- NOT_RECOMMENDED --> F1
        G -- RECOMMENDED --> H{Kuota Kursi Masih Tersedia?}
        H -- Kuota Habis --> H1[Masuk Talent Pool Cadangan Aktif 6 Bulan]
        H -- Kuota Ada --> I[Kandidat Terpilih untuk Penawaran]
    end

    subgraph offer[Penawaran & Persetujuan Bertingkat]
        I --> J[Susun Draf Penawaran Kerja Bersyarat]
        J --> K{Gaji > Batas Maksimum Grade?}
        K -- Tidak --> L[Persetujuan Jalur Standar: Kepala Unit & HR Manager]
        K -- Ya --> M[Persetujuan Jalur Eskalasi: Direktur Keuangan & Dirut]
        L --> N[Kirim Surat Penawaran Bersyarat ke Kandidat]
        M --> N
        N --> O{Kandidat Menyetujui?}
        O -- Menolak --> P[Kursi Kembali Terbuka; Panggil Talent Pool]
        O -- Menyetujui --> Q[Jadwalkan MCU Pra-Kerja]
    end

    subgraph mcu[MCU Pra-Kerja & Onboarding]
        Q --> R[Pemeriksaan Kesehatan Kerja oleh Tim K3RS]
        R --> S{Kesimpulan Kelaikan Fisik Kerja?}
        S -- PERMANENTLY_UNFIT --> T[Penawaran Batal Demi Hukum / Rescinded]
        T --> P
        S -- TEMPORARILY_UNFIT --> U[Ditangguhkan untuk Pengobatan]
        S -- FIT_WITH_RESTRICTIONS --> V[Konfirmasi Penyesuaian Beban Kerja]
        S -- FIT --> W[Penawaran Definitif / Final Accepted]
        V --> W
        W --> X[Serah Terima ke Sub-modul 05 Onboarding]
        X --> Y([Draf Profil Pegawai: Pending Onboarding])
    end
```

---

## 4. Diagram 3 — Talent Pool dan Promosi Cepat Saat Kandidat Batal

```mermaid
flowchart TD
    subgraph pool[Bank Kandidat Cadangan / Talent Pool]
        A([Pelamar Lolos Uji Klinis Tanpa Kuota]) --> B[Status: TALENT_POOL_QUALIFIED]
        B --> C[Urutkan Berdasarkan Peringkat Skor Klinis]
        C --> D{Apakah Masa Aktif > 6 Bulan?}
        D -- Ya --> E[Kedaluwarsa / Expired: Wajib Uji Ulang]
        D -- Tidak --> F[Tersedia untuk Promosi Cepat]
    end

    subgraph trigger[Pemicu Kekosongan Kursi]
        G([Kandidat Utama Menolak Offer / No-Show Onboarding]) --> H[Kursi Requisition Kembali Terbuka]
        H --> I[Recruiter Buka Menu Talent Pool]
        I --> J[Pilih Kandidat Cadangan Peringkat 1]
        J --> K[Klik: Promosikan dari Talent Pool]
        K --> L[Draf Surat Penawaran Baru Terbit Otomatis]
        L --> M([Lanjut ke Penawaran Bersyarat & MCU])
    end
```

---

## 5. Tabel Langkah Kerja Petugas

| No | Tahap | Pelaku | Masukan | Keluaran | Bila Gagal / Ditolak |
| --- | --- | --- | --- | --- | --- |
| 1 | Buka Requisition | HR Recruiter | Nomor Headcount Request yang disetujui | Requisition berstatus `APPROVED` | Ditolak sistem bila kuota kursi habis |
| 2 | Buat Lowongan | HR Recruiter | Data Requisition dan persyaratan profesi | Lowongan tayang (*Published*) | Ditolak bila tanggal penutupan lampau |
| 3 | Pendaftaran & Upload | Pelamar | NIK KTP, biodata, dokumen STR & ijazah | Berkas lamaran terdaftar | Validasi format NIK 16 digit |
| 4 | Penapisan Berkas | Sistem & Recruiter | Dokumen STR dan ijazah | Lolos atau Gugur Administrasi | Jika STR kadaluwarsa, otomatis auto-reject |
| 5 | Evaluasi Wawancara | HR Recruiter | Rubrik penilaian perilaku | Nilai HR dan rekomendasi | Pelamar gugur bila HR menolak |
| 6 | Uji Klinis Unit | Kepala Ruang / Dokter | SOP klinis & uji tindakan | Nilai klinis dan rekomendasi | Jika Unit menolak, berlaku hak veto mutlak |
| 7 | Buat Draf Offer | HR Recruiter | Data kandidat terpilih & grade gaji | Draf Surat Penawaran Bersyarat | Ditolak bila gaji di bawah minimum grade |
| 8 | Persetujuan Offer | HR Manager / Dirut | Nominal gaji dan batas grade | Penawaran berstatus disetujui | Dikembalikan untuk revisi angka gaji |
| 9 | Pemeriksaan MCU | Dokter K3RS | Kandidat yang menerima penawaran | Status kelaikan k3 (Fit / Unfit) | Jika Unfit permanen, penawaran otomatis batal |
| 10 | Serah Terima | HR Manager | Kandidat kelaikan Fit | Draf profil `Pending Onboarding` | Kursi requisition bertambah terpenuhi |
| 11 | Promosi Cadangan | HR Recruiter | Kursi batal & kandidat Talent Pool | Penawaran baru tanpa seleksi ulang | Kandidat kadaluwarsa (>6 bln) ditolak promosi |
