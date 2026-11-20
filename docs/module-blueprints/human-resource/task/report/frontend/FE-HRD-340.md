# Laporan Verifikasi Frontend — `FE-HRD-340`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-340` |
| Judul | Verifikasi browser amandemen `S-D5` v28 |
| Slice | `S-D5` amandemen / Verifikasi · `EPIC HRD-123` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 28, kartu `FE-HRD-340` |
| Trace | `FR-TRV-34`; `AT-HRD-TRV-39`; `UAT-TRV-06` s.d. `UAT-TRV-12` |
| Contract version | `v28` (approved 19 November 2026) + API bagian 6 "Approval Inbox" |
| Dependency | `FE-HRD-338` ✅, `FE-HRD-339` ✅, `BE-HRD-380` ✅, `FE-HRD-393` ✅ (ditambahkan saat verifikasi) |
| Klasifikasi | `VERIFICATION` — tanpa perubahan source pada task ini. Perbaikan yang dibutuhkan dikerjakan sebagai `FE-HRD-393` atas keputusan pengguna |
| Task mode | `FRONTEND` (instruksi pengguna 20 November 2026: "lanjut"; keputusan "Perbaiki kotak masuk lama") |
| Lingkungan | `next dev` 3000 → backend lokal 7184 (biner 19 November 15.53, memuat `BE-HRD-374` s.d. `380`) → devYoga |
| Akun | Pemohon Dewi Anggraeni; atasan dan pengganti Finance dr. Bima. Sandi dari pengguna, tidak disimpan. Hak sementara dikembalikan identik di setiap jalan (Dewi 11, Bima 45) |
| Model | Claude Opus 5.5 |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi |

---

## 1. Jalannya verifikasi

| Jalan | Hasil | Tindak lanjut |
| --- | --- | --- |
| 1–3 | Skrip tersandung pada perilaku layar yang benar (Ajukan dari layar sukses tanpa modal; tanggal bentrok saat membuat ulang). Data uji `0013`/`0014`/`0015` terbentuk | Skrip dibuat idempoten |
| 4 sebelum perbaikan | Setujui, Revisi, dan Tolak di kotak masuk lama semuanya `404` | Pengguna memilih perbaikan → `FE-HRD-393` |
| 4 sesudah perbaikan | Langkah 1–5 lulus (keputusan atasan, revisi, ajukan ulang, `409` ditolak) | Bukti `fe-340-run4.out` |
| 5 (lanjut dari langkah 5) | Batal, uang muka, kotak masuk sesudah keputusan, settlement, dan jejak audit lulus | Bukti `fe-340.out` |
| Tambahan | Plafon 80% di modal uang muka; ringkasan dan paging kotak masuk | `fe-340b.mjs`, `fe-393.mjs` |

---

## 2. Hasil per skenario

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| **UAT-TRV-06** Buat lalu Ajukan | `0014`: klik ganda "Simpan Draf" → 1 `POST`, layar "SPPD Disimpan sebagai Draf"; klik ganda "Ajukan ke Atasan" di layar sukses → 1 `POST /submit` (200) "SPPD Diajukan". `0013` diajukan dari baris daftar, `0015` dari Detail (klik ganda di modal → 1 `POST`). Ketiganya "Menunggu Persetujuan" tanpa tombol Ajukan/Ubah, dan muncul di kotak masuk dr. Bima sebagai "Perjalanan Dinas" | `PASS` |
| **UAT-TRV-07** Atasan menyetujui | `0013` disetujui dari kotak masuk: `200`; status Disetujui, anggaran disetujui Rp2.000.000, presensi Hadir Dinas Luar 2 hari (3–4 Maret 2027). Uang muka: modal menampilkan "Anggaran Disetujui Rp 2.000.000" dan "Maksimal Uang Muka Rp 1.600.000"; Rp1.700.000 memunculkan "Uang muka maksimal 80% anggaran disetujui: Rp 1.600.000." dan tombol nonaktif tanpa `POST`; Rp1.000.000 (klik ganda) → 1 `POST`, `201`, baris `ADV/RSQ/2026/11/0002` Diajukan | `PASS` |
| **UAT-TRV-08** Kembalikan, ubah hotel, Ajukan ulang | `0014` dikembalikan (catatan wajib); pemohon melihat "Dikembalikan untuk Revisi" + peringatan di formulir; penginapan diubah ke Rp550.000 (`PUT` 200); Ajukan ulang `200`, workflow instance sama, kembali "Menunggu Persetujuan" | `PASS` |
| **UAT-TRV-09** SPPD ditolak diajukan | `0015` ditolak dari kotak masuk dengan alasan `UJI_376_TOLAK`; status Ditolak tanpa tombol Ajukan. Dipaksa lewat API (daftar dicegat agar tampil Draf): `409` "SPPD yang ditolak tidak dapat diajukan ulang. Buat SPPD baru." tampil di modal | `PASS` |
| **UAT-TRV-10** Uang muka untuk SPPD menunggu | `0014` (menunggu) dipaksa terpilih: `409` "SPPD belum disetujui. Ajukan SPPD dan tunggu persetujuan sebelum mengajukan uang muka." tampil | `PASS` |
| **UAT-TRV-11** Pemohon membatalkan | `0014` (menunggu sesudah ajukan ulang): konfirmasi memuat "akan ditarik"; `200`; SPPD Dibatalkan, workflow `Withdrawn`; tidak lagi ada di kotak masuk dr. Bima | `PASS` |
| **UAT-TRV-12** Jejak audit sampai settlement ditutup | `STL/RSQ/2026/11/0001` (SPPD `0010`) diverifikasi Finance (`200`) dan ditutup lewat "Catat Pembayaran" dengan bukti `UJI-340-KAS-01` (`200`); settlement dan SPPD `Completed`. `GET hrd/travel/audit-logs?travelRequestId=…`: 9 event bernama matriks, urut waktu terbaru dahulu: `REQUEST_CREATED`, `REQUEST_SUBMITTED`, `REQUEST_APPROVED`, `ROSTER_LOCK_APPLIED`, `ATTENDANCE_SYNCED` ×2, `SETTLEMENT_SUBMITTED`, `SETTLEMENT_VERIFIED`, `SETTLEMENT_CLOSED` | `PASS` (endpoint; layar audit tidak direncanakan) |
| **AT-HRD-TRV-39** Label, tombol, `409`, kirim ganda | Label Indonesia: Draf, Menunggu Persetujuan, Dikembalikan untuk Revisi, Disetujui, Ditolak, Dibatalkan; Ajukan hanya pada Draf/Revisi; `409` tampil apa adanya (`VR-TRV-28`, `VR-TRV-30`); klik ganda tidak pernah menghasilkan permintaan kedua | `PASS` |
| Ponsel 390 px | Daftar SPPD dan kotak masuk terbaca | `PASS` |

Bukti: tangkapan layar `340-*.png` dan `393-*.png`, keluaran `fe-340-run2.out`, `fe-340-run4.out`, `fe-340.out`, JSON jejak audit `340-audit-SPPD-RSQ-2026-11-0010.json` di scratchpad sesi.

---

## 3. Validasi

| Perintah | Hasil |
| --- | --- |
| Uji unit `approval-inbox-legacy-actions`, `business-travel-v28-gates`, `business-travel-v28-status` | 20/20 lulus |
| `npm run lint:errors` | Exit 0 |
| `npm run build` | 890/890 halaman |

---

## 4. Data uji yang tersisa di devYoga

| Data | Keadaan |
| --- | --- |
| `SPPD/RSQ/2026/11/0013` (UJI-340-A, Bandung 3–4 Maret 2027) | Disetujui; presensi Dinas Luar dan gembok roster Dewi 3–4 Maret 2027; uang muka `ADV/RSQ/2026/11/0002` Rp1.000.000 berstatus Diajukan |
| `SPPD/RSQ/2026/11/0014` (UJI-340-B) | Dibatalkan sesudah revisi dan ajukan ulang (workflow `Withdrawn`) |
| `SPPD/RSQ/2026/11/0015` (UJI-340-C) | Ditolak (`UJI_376_TOLAK`) |
| `SPPD/RSQ/2026/11/0010` + `STL/RSQ/2026/11/0001` | Settlement diverifikasi dan ditutup; SPPD Selesai |
| `SPPD/RSQ/2026/11/0009` | Tetap menunggu dr. Bima |

---

## 5. Temuan

| Temuan | Status |
| --- | --- |
| Aksi kotak masuk lama `404`, kolom/ringkasan/paging kosong, Tolak tanpa id alasan | Diperbaiki di `FE-HRD-393` |
| Kotak Masuk Terpadu V2 (menu) tidak menampilkan SPPD: kueri backend `Pending` + `ApproverEmployeeId` vs Workflow V2 `Available` + `AssignedApproverUserId` | Terbuka — perlu task backend atau keputusan menu; atasan memakai `/hr/workflow/approval-inbox` |
| Biner backend uji belum memuat merge `QuilvianIntegrationBackend` (`815ce085`) | Jalur yang diuji tidak berubah oleh merge; sebaiknya diulang singkat sesudah build terbaru |
| Alasan penolakan untuk `HRD_TRV_REQUEST` di devYoga hanya data uji `UJI_376_TOLAK` | Master alasan produksi perlu diisi admin workflow |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Acceptance criteria | 1. `AT-HRD-TRV-39` lulus · 2. `UAT-TRV-06` s.d. `11` lulus di layar, `UAT-TRV-12` lewat endpoint jejak audit · 3. lint dan build lulus |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
