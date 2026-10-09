# Laporan Task Backend — `RJ-DOC-REV-BE-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-021` |
| Judul | Data uji `PMTEST` (`RJ-DOC-DEC-079`) |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Dependency | `RJ-DOC-REV-BE-017` ✅ |
| Task mode | `CROSS-REPO MODE` — data uji lewat API, tanpa kode (`RJ-DOC-DEC-083`) |
| Database | `QuilvianNewDevSukma` saja |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Yang dibuat (lewat endpoint master, backend uji 7185)

| Data | Isi | Endpoint |
| --- | --- | --- |
| Institusi perujuk | `PMTEST-KSS` *PMTEST Klinik Sehat Sentosa* (**mitra**), `PMTEST-PKM` *PMTEST Puskesmas Uji* (bukan mitra) | `POST /master-data/referral-institutions` |
| Dokter perujuk | *dr. PMTEST Rina Lestari* (KSS), *dr. PMTEST Budi Santoso* (PKM) | `POST /master-data/referral-doctors` |
| Jadwal dokter | 23 jadwal mingguan bernama `PMTEST …`, Poli Penyakit Dalam, Senin–Sabtu, 13.00–21.00 sesi *Sore*, berlaku 1 Okt – 31 Des 2026, untuk dr. Dewi Lestari, dr. Maya Permata Sari, dr. Rendy Pangalila, dr. Sinta Jojo | `POST /master-data/doctor-schedules/admin` |

Institusi dan dokter perujuk dibuat saat runtime `BE-018` dan diverifikasi ulang di sini. Skrip pembuatnya idempoten: jadwal `PMTEST` yang sudah ada tidak dibuat ulang.

## 2. Delta terhadap rencana

| Delta | Alasan |
| --- | --- |
| Memakai empat dokter internal yang sudah ada, bukan dokter `PMTEST` baru | Dokter internal adalah data induk Workforce/HR, di luar wewenang task. Penanda uji ada di nama jadwal (`PMTEST …`), sehingga mudah dibersihkan |
| Jadwal tanpa ruang (`roomId` kosong) | Ruang Poli Penyakit Dalam 1 sudah dipakai jadwal dr. Bagus 08.00–21.00, sehingga 5 jadwal bertabrakan dan ditolak backend (aturan existing). Diulang tanpa ruang |
| Jadwal Kamis dr. Rendy Pangalila tidak dibuat | Ditolak aturan existing: dokter sudah punya jadwal lain pada jam itu |

## 3. Verifikasi

| ID | Skenario | Hasil |
| --- | --- | --- |
| T1 | Query baca-saja: Poli Penyakit Dalam punya ≥ 4 dokter berjadwal aktif hari ini (Kamis) | PASS — 4 dokter |
| T2 | `options` institusi: `PMTEST-KSS` `isPartner = true`, `PMTEST-PKM` `false` | PASS |
| T3 | `options` dokter perujuk `PMTEST`: 2 | PASS |
| T4 | Ada poli aktif tanpa jadwal hari ini (Breast Clinic, Klinik Laktasi, Klinik Luka) untuk uji popup jadwal | PASS |

**4/4 PASS.**

## 4. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Poli Penyakit Dalam ≥ 4 dokter berjadwal aktif hari ini | Terpenuhi | T1 |
| 2. Institusi mitra muncul `isPartner = true` di `options` | Terpenuhi | T2 |
| 3. Daftar ID data uji dicatat untuk pembersihan | Terpenuhi | Bagian 5 |

## 5. Pembersihan

- Jadwal: `DELETE /master-data/doctor-schedules/admin/{id}` untuk setiap jadwal bernama `PMTEST …`. Untuk mencari ID-nya, query baca-saja `"ScheduleName" LIKE 'PMTEST%'`.
- Institusi/dokter perujuk `PMTEST-*` sudah dipakai kunjungan uji (dibatalkan), sehingga **dinonaktifkan**, bukan dihapus.
