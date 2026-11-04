# Alur Utama Modul Human Resource

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Jenis | Alur pokok modul, ujung ke ujung |
| Status | `draft` |
| Slice tercakup | `S0-A`, `S0-B`, `S-A1` s.d. `S-A7`, `S-B1` s.d. `S-B5`, `S-C2` s.d. `S-C5`, `S-C4-ONB`, `S-E` |
| Perubahan terakhir | 4 November 2026 — titik awal pegawai bergabung kini lewat onboarding (`S-C4-ONB`, `approved` 4 November 2026) |

---

## 1. Apa yang digambar di sini

Satu diagram, **jalur normal saja**. Percabangan dan jalur pengecualian ada di berkas proses
masing-masing; diagram ini sengaja tidak memuatnya supaya bentuk besar modul tetap terbaca.

Diagram ini menjawab: **dari mana satu bulan kerja pegawai dimulai, dan di mana ia berakhir bagi
HR.**

## 2. Diagram

```mermaid
flowchart TD
    subgraph hr[HR Admin]
        A([Kandidat menerima tawaran]) --> B[Jalankan onboarding dan tandatangani kontrak]
        B --> C[Nyatakan siap kerja]
    end

    subgraph atasan[Manajer unit]
        C --> D[Susun roster periode berikutnya]
        D --> E[Terbitkan jadwal kerja]
        E --> F[(Published)]
    end

    subgraph pegawai[Pegawai]
        F --> G[Bekerja sesuai jadwal]
        G --> H[Catat kehadiran masuk dan pulang]
        H --> I[Ajukan cuti, lembur, atau koreksi bila perlu]
    end

    subgraph persetujuan[Penyetuju]
        I --> J[Buka kotak masuk persetujuan]
        J --> K[Putuskan setiap pengajuan]
        K --> L[(Approved)]
    end

    subgraph sistem[Sistem]
        H --> M[Olah kehadiran menjadi kehadiran harian]
        L --> M
        M --> N[(Present)]
    end

    subgraph payroll[Petugas payroll]
        N --> O[Tutup periode kehadiran]
        O --> P[(Closed)]
        P --> Q[Hitung payroll periode itu]
        Q --> R[(Approved)]
        R --> S[Jalankan serah terima]
    end

    S --> T([Tanggung jawab HR selesai])
```

**Batas yang digambar terakhir bukan hiasan.** `HRD-DEC-009` menghentikan tanggung jawab HR
setelah serah terima dijalankan. Apa yang terjadi setelah itu — pembayaran, jurnal, pajak, dan
pelaporan — milik Finance, dan bentuk serah terimanya masih terbuka (`HRD-Q-10`, `HRD-Q-11`).

## 3. Tabel langkah

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Jalankan onboarding dan tandatangani kontrak | HR Admin | Serah terima dari rekrutmen; tawaran memuat jenis dan lama kontrak; scan kontrak bertanda tangan | Kasus onboarding berjalan; kontrak `Active`; penempatan dan gaji awal berlaku sejak tanggal mulai kontrak | Kontrak tidak sah tanpa scan; HR mengunggahnya, atau meminta revisi tawaran bila isinya harus berubah. Rincian: `onboarding-dan-siap-kerja.md` |
| Nyatakan siap kerja | HR Admin | Seluruh butir wajib checklist selesai | Pegawai Aktif dan boleh dijadwalkan | Ditolak; HR menyelesaikan butir yang tersisa |
| Susun roster | Manajer unit | Kebutuhan tenaga per hari, daftar pegawai unit | Roster berstatus draf | Roster tidak dapat disusun; manajer melengkapi kebutuhan tenaga lebih dulu |
| Terbitkan jadwal kerja | Manajer unit | Roster yang lolos pemeriksaan bentrok | Jadwal berstatus `Published` | Penerbitan ditolak; manajer menyelesaikan bentrok yang menghalangi |
| Catat kehadiran | Pegawai | Jadwal yang berlaku pada tanggal itu | Rekaman kehadiran tersimpan | Pencatatan ditolak; pegawai menghubungi HR untuk jalur koreksi |
| Ajukan cuti, lembur, atau koreksi | Pegawai | Saldo, jadwal, atau data hari yang akan dikoreksi | Pengajuan berstatus `Submitted` | Pengajuan ditolak sistem; pegawai memperbaiki isian |
| Putuskan pengajuan | Penyetuju | Kotak masuk berisi pengajuan yang ditugaskan kepadanya | Pengajuan berstatus `Approved` atau `Rejected` | Keputusan tidak tersimpan; penyetuju mengulang dari kotak masuk |
| Olah kehadiran harian | Sistem | Rekaman kehadiran, jadwal, pengajuan yang sudah disetujui | Kehadiran harian berstatus hasil hitung | Pengolahan berstatus `Error`; HR menjalankan ulang untuk tanggal itu |
| Tutup periode kehadiran | Petugas payroll | Tidak ada pengecualian pemblokir yang masih terbuka | Periode berstatus `Closed` | Penutupan ditolak; petugas menyelesaikan penghalang yang ditampilkan lebih dulu |
| Hitung payroll | Petugas payroll | Periode kehadiran yang sudah `Closed` | Putaran payroll berstatus `Approved` | Perhitungan gagal; petugas memperbaiki masukan lalu menghitung ulang |
| Jalankan serah terima | Petugas payroll | Putaran payroll yang sudah disetujui | Serah terima terlaksana | Serah terima gagal; petugas mengulang. **Bentuk penanganan penolakan Finance masih terbuka** |

## 4. Yang sengaja tidak ada di diagram ini

| Yang tidak digambar | Di mana ia digambar |
| --- | --- |
| Seluruh jalur penolakan dan perbaikan | Berkas proses masing-masing di folder ini |
| Pelatihan, penilaian kinerja, kedisiplinan, dan pengunduran diri | `kompetensi-dan-pelatihan.md`, `manajemen-kinerja.md`, `hubungan-karyawan-dan-disiplin.md`, `offboarding.md` |
| Batal bergabung, masa percobaan, pendampingan klinis, perpanjangan kontrak, dan data awal kontrak | `onboarding-dan-siap-kerja.md`, `masa-percobaan.md`, `pendampingan-klinis.md`, `siklus-kontrak.md`, `batch-data-awal-kontrak.md` (`S-C4-ONB`, `approved`) |
| Kredensial dan kewenangan klinis | **Tidak digambar di mana pun.** `S-C1` `BLOCKED`. Sub-modul 05 hanya menampilkan peringatannya |
| Kesehatan kerja staf | **Tidak digambar di mana pun.** `S-C6` `BLOCKED` |
| Perencanaan tenaga kerja: standar, formasi, rencana tahunan, permintaan headcount | `standar-kebutuhan-tenaga.md`, `formasi-dan-rencana-tahunan.md`, `permintaan-headcount.md`. Sengaja tidak masuk diagram pokok karena siklusnya tahunan dan per kebutuhan, bukan harian; titik sambungnya ke alur pokok adalah penempatan pegawai baru dan pembacaan standar oleh Penjadwalan |

## 5. Traceability

| Bagian alur | Decision | Slice | Acceptance test |
| --- | --- | --- | --- |
| Onboarding, kontrak pertama, dan siap kerja | `HRD-DEC-083`, `HRD-DEC-084`, `HRD-DEC-088`, `HRD-DEC-095` | `S-C4-ONB` | `AT-HRD-ONB-01`, `AT-HRD-ONB-07`, `AT-HRD-ONB-23` |
| Profil dan penempatan | `HRD-DEC-012` | `S-A1` | `AT-HRD-A1-01` |
| Roster dan penerbitan jadwal | `HRD-DEC-026` | `S-B4` | `AT-HRD-B4-01` |
| Pencatatan dan pengolahan kehadiran | `HRD-DEC-013` | `S-B1` | `AT-HRD-B1-01` |
| Kotak masuk persetujuan | `HRD-DEC-011`, `HRD-DEC-018` | `S-A7` | `AT-HRD-A7-01` |
| Penutupan periode | `HRD-DEC-022` | `S-B1` | `AT-HRD-B1-05` |
| Batas serah terima payroll | `HRD-DEC-009`, `HRD-Q-10`, `HRD-Q-11` | `S-B5` | `AT-HRD-B5-01` |
| Perencanaan tenaga kerja | `HRD-DEC-058` s.d. `HRD-DEC-072` | `S-D1` | `AT-HRD-D1-01` s.d. `AT-HRD-D1-21` |
