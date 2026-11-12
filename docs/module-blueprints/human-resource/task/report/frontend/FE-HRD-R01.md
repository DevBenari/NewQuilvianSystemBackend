# Laporan Perubahan Frontend — `FE-HRD-R01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R01` |
| Judul | Sinkronisasi status dan register roadmap |
| Slice | Semua slice HR |
| Roadmap | `roadmap/frontend-unfinished-work.md` bagian Tabel Task dan Kartu Task, Gelombang 1 |
| Trace | Kelompok G audit 11 November 2026; `rules/rule-output/status-task-roadmap.md` |
| Contract version | `NOT APPLICABLE` — dokumen saja |
| Wewenang UI | `NOT APPLICABLE` |
| Dependency | — (tanpa prasyarat) |
| Klasifikasi | `LIGHT` — dokumen saja |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Roadmap menandai 268 task selesai, padahal 137 di antaranya belum berfungsi. Status register `FE-HRD-058` s.d. `087` kosong, `FE-HRD-216` s.d. `305` tidak ada di register, register `FE-HRD-027` ✅ sedangkan kartu dan laporannya 🟡, dan kartu `S-D1` serta `S-B1` masih menulis "menunggu BE" padahal backend sudah ✅.

---

## 2. Proses bisnis dari sisi pengguna

Pemilik modul dan developer membaca roadmap untuk tahu layar mana yang sudah siap dipakai petugas rumah sakit. Sesudah task ini, task yang hanya menampilkan data contoh tidak lagi terbaca siap. Contoh: Kotak Masuk Persetujuan V2 (`FE-HRD-277`) kini 🟡 dengan alasan "Layar memakai data contoh di kode dan tidak memanggil API" dan rujukan ke task perbaikannya `FE-HRD-R21`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/frontend-roadmap.md`, `roadmap/frontend-unfinished-work.md`, laporan task di `task/report/frontend/`, dan hasil audit source.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `roadmap/frontend-roadmap.md` | 137 judul kartu dan baris status diturunkan ke 🟡 beserta alasan, jumlah kriteria, dan task perbaikannya; 59 baris register diturunkan; 57 tanda pada grafik pohon teks diganti 🟡; status register `FE-HRD-058` s.d. `087` diisi ✅ beserta tautan laporan; 90 baris `FE-HRD-216` s.d. `305` ditambahkan ke register; register `FE-HRD-027` disamakan dengan laporannya (🟡); kartu `S-D1` dan `S-B1` yang belum dikerjakan diberi rujukan task perbaikan; metadata `status_synced_at` diperbarui |

### 3.3 Kepatuhan arsitektur frontend

`NOT APPLICABLE` — tidak ada source yang diubah.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` — task hanya mengubah dokumen roadmap

`UI GATE: N/A — task hanya mengubah dokumen roadmap`

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — tidak ada layar.

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada panggilan API.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Hitung judul kartu ber-🟡 sesudah perubahan | 141 (137 hasil audit + 4 task `S-D1`/`S-B1` yang baru dibangun dan masih sebagian) | `PASS` | `grep -c "^#### 🟡\|^### 🟡" frontend-roadmap.md` |
| Periksa grafik pohon teks yang disentuh | Garis tegak tetap lurus; tanda 🟡 menggantikan ✅ atau ⏳ dengan lebar tampilan yang sama | `PASS` | Pemeriksaan visual blok grafik |
| Periksa baris register lain tidak ikut berubah | Percobaan pertama ikut mengubah 17 baris tabel slice `S-C4-ONB`; roadmap dipulihkan dari cadangan dan skrip dipersempit ke tabel register | `PASS` | Perbandingan dengan cadangan sebelum perubahan |

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:** tidak ada.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Jumlah task ✅ sama dengan hasil audit | Terpenuhi | 137 task Kelompok B, C, D kini 🟡 |
| Setiap task Kelompok B, C, D berstatus 🟡 beserta alasannya | Terpenuhi | Baris status tiap kartu menyebut alasan, jumlah kriteria, dan task perbaikan |
| DoD: laporan ada, status di roadmap dan dokumen rencana diperbarui | Terpenuhi | Laporan ini; `frontend-unfinished-work.md` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Grafik pohon teks lama yang node-nya tidak pernah bertanda (misalnya `S-B3`, `S-C1`, `S-D4`) dibiarkan tanpa tanda agar garisnya tidak rusak; status resminya tetap pada kartu dan register |
| Dependency backend | `NONE` |
| Perubahan sampingan | Percobaan pertama skrip ikut mengubah 17 baris tabel slice `S-C4-ONB`; roadmap dipulihkan dari cadangan sebelum dijalankan ulang |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | Jalankan Gelombang 2 mulai dari `FE-HRD-R14` (layar K3RS sebagai pola rujukan) |
