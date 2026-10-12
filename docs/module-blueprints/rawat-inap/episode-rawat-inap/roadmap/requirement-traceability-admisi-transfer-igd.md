# Matriks Ketertelusuran Kebutuhan (Requirement Traceability Matrix) — Admisi Transfer Pasien IGD & Rawat Jalan

| Field | Nilai |
|---|---|
| Modul | `rawat-inap` / Sub-modul `episode-rawat-inap` |
| Blueprint ID | `RWI-BP-001` revision `11` |
| Kontrak Versi | **`0.12.0` `approved`** (10 Oktober 2026) |
| Status Roadmap | **`APPROVED`** — disetujui pengguna 10 Oktober 2026 sebagai rencana pengiriman resmi alur admisi transfer pasien IGD dan Poliklinik |
| Ditulis Oleh | `plan-module-delivery` |
| Dokumen Acuan | Analisis Bisnis (Alur 4 Tahap A-B-C-D), `backend-roadmap-admisi-transfer-igd.md`, `frontend-roadmap-admisi-transfer-igd.md`, API Contract bagian 13.8 dan amandemen EPS-006 |
| Source Commit Baseline | Backend: `fdf85a07` (branch `MHamzah`); Frontend: `dd2cbf7c` (branch `HamzahV2`) |

---

## 1. Tabel Ketertelusuran Menyeluruh (Traceability Matrix)

Tabel berikut menghubungkan kebutuhan fungsional bisnis, keputusan bisnis, kriteria penerimaan, kontrak validasi, task backend, task frontend, rencana verifikasi bukti, dan status pelaksanaan.

| ID Kebutuhan | Keputusan Bisnis | Deskripsi Kebutuhan & Hasil Akhir | Kontrak Desain & Validasi | Task Backend | Task Frontend | Kriteria Penerimaan (AC) | Rencana Verifikasi & Bukti | Status |
|---|---|---|---|---|---|---|---|:---:|
| `FR-RI-187` | `EPS-006/K-02` | Petugas admisi ranap menerima daftar permintaan rawat inap dari IGD secara elektronik (Tahap A) | EPS-006 1.0 (admission-transfer-repair-006.md), Permission Matrix `0.12.0` | [`BE-RWI-206`](../task/report/backend/BE-RWI-206.md) 🟡 | [`FE-RWI-228`](../task/report/frontend/FE-RWI-228.md) 🟡 | `RWI-AC-396` | Integrasi query API: seluruh pasien dengan disposisi IGD `Confirmed` muncul di antrean admisi | `PARTIAL_RUNTIME_PENDING` |
| `FR-RI-188` | `EPS-006/WAIT-60` | Peringatan visual dan filter waktu tunggu pasien transfer jika melebihi ambang batas toleransi (> 60 menit) | EPS-006 1.0 (admission-transfer-repair-006.md), `05-skema-tampilan.md` 3.3 | [`BE-RWI-206`](../task/report/backend/BE-RWI-206.md) 🟡 | [`FE-RWI-228`](../task/report/frontend/FE-RWI-228.md) 🟡 | `RWI-AC-397` | Perhitungan `WaitingMinutes` otomatis; baris berubah warna kuning/merah jika melebihi 60 menit | `PARTIAL_RUNTIME_PENDING` |
| `FR-RI-189` | `EPS-006/K-02` | Wali pasien diarahkan ke loket admisi ranap dan petugas memilih opsi kartu ke-4 "Transfer IGD / Poliklinik" (Tahap B) | `05-skema-tampilan.md` 3.0, Constants Flow `0.12.0` | — | [`FE-RWI-227`](../task/report/frontend/FE-RWI-227.md) 🟡 | `RWI-AC-400` | UI component test: kartu ke-4 dirender dengan teks panduan dan mengarahkan ke antrean transfer | `PARTIAL_RUNTIME_PENDING` |
| `FR-RI-190` | `EPS-006/K-01/K-03` | Petugas admisi membuka data pasien transfer, data identitas, DPJP, dan diagnosa IGD terisi otomatis (*auto-populate*) tanpa ketik ulang (Tahap C) | EPS-006 1.0 (admission-transfer-repair-006.md), State Transition `0.12.0` | [`BE-RWI-206`](../task/report/backend/BE-RWI-206.md) 🟡 | [`FE-RWI-229`](../task/report/frontend/FE-RWI-229.md) 🟡 | `RWI-AC-401`, `RWI-AC-403` | Mock flow test: klik "Proses Admisi" memuat data pasien, DPJP, dan diagnosa IGD tanpa input ulang | `PARTIAL_RUNTIME_PENDING` |
| `FR-RI-191` | `EPS-006/K-01/K-03` | Petugas dan wali menyelesaikan pemilihan penjamin, alokasi tempat tidur, dan tanda tangan digital persetujuan rawat inap (Tahap C lanjutan) | Validation Matrix `0.12.0`, Kanvas TTD `FE-RWI-225` | — | [`FE-RWI-229`](../task/report/frontend/FE-RWI-229.md) 🟡 | `RWI-AC-404` | Stepper validation test: kamar ter-booking, skema bayar tersimpan, tanda tangan digital terekam | `PARTIAL_RUNTIME_PENDING` |
| `FR-RI-192` | `EPS-006/K-01/K-03` | Pendaftaran selesai (Tahap D): nomor episode ranap terbit, status disposisi IGD berubah menjadi `Executed`, dan berkas registrasi/gelang dicetak | EPS-006 1.0 (admission-transfer-repair-006.md), `05-skema-tampilan.md` 3.4 | [`BE-RWI-207`](../task/report/backend/BE-RWI-207.md) 🟡 | [`FE-RWI-230`](../task/report/frontend/FE-RWI-230.md) 🟡 | `RWI-AC-398`, `RWI-AC-405` | E2E API & UI test: episode baru lahir, disposisi IGD `Executed`, pasien hilang dari antrean tunggu | `PARTIAL_RUNTIME_PENDING` |

---

## 2. Analisis Cakupan Uji & Coverage Gap

1. **Requirement-to-Test Coverage:**
   - Seluruh 6 kebutuhan fungsional (`FR-RI-187` s.d. `FR-RI-192`) dan kriteria penerimaan (`RWI-AC-396` s.d. `RWI-AC-405`) memiliki keterkaitan langsung dengan task teknis backend (`BE-RWI-206`, `BE-RWI-207`) dan frontend (`FE-RWI-227` s.d. `FE-RWI-230`).
   - **Bukti runtime belum lengkap:** receipt/rollback/konkurensi pada PostgreSQL belum dijalankan; mock UI dan unit frontend tidak membuktikan commit database.
2. **Ketergantungan Lintas Lapisan (Backend ke Frontend):**
   - Task `FE-RWI-228` (Tabel Antrean) dan `FE-RWI-229` (Pre-fill Stepper) bergantung secara kontraktual pada endpoint `BE-RWI-206`.
   - Task `FE-RWI-230` (Penyelesaian Admisi & Eksekusi Disposisi) bergantung pada endpoint transaksional `BE-RWI-207`.
   - Backend diselesaikan terlebih dahulu per vertical slice, memastikan tim frontend bekerja di atas kontrak API yang sudah tervalidasi dan siap pakai.
3. **Penyelarasan Siklus Hidup Pasien IGD ke Bangsal:**
   - Transisi status `EmgDisposition` dari `Confirmed` menjadi `Executed` pada saat admisi ranap diterbitkan memastikan sistem IGD selalu tersinkronisasi tanpa memerlukan entri data ganda dari staf perawat.


## Status koreksi EPS-006 - 12 Oktober 2026

Status task historis di atas dikoreksi menjadi sebagian; klaim selesai lama tidak menjadi bukti runtime pasangan branch ini. Acuan route, payload dan lifecycle final: [amandemen EPS-006](../contracts/admission-transfer-repair-006.md). ID RWI-DEC-274/275/276 lama tidak dipakai untuk keputusan admisi karena bertabrakan dengan keputusan Bed Management. EPS-006/WAIT-60 hanya merujuk ambang existing 60 menit.

Backend MHamzah dan frontend HamzahV2 telah diperbaiki pada working tree. Backend build Repair006: 0 error; QBE strict: 0 violation. Uji frontend terarah: 28/28 lulus. Bukti browser dan gerbang rilis: [FIX-EPS-006-08](../task/report/frontend/FIX-EPS-006-08.md). Migration belum diterapkan; uji transaksi PostgreSQL belum dilakukan.

| Task perbaikan | Pemilik | Status | Laporan |
| --- | --- | --- | --- |
| FIX-EPS-006-01 | backend | ✅ | [Laporan](../task/report/backend/FIX-EPS-006-01.md) |
| FIX-EPS-006-02 | backend | 🟡 | [Laporan](../task/report/backend/FIX-EPS-006-02.md) |
| FIX-EPS-006-03 | backend | 🟡 | [Laporan](../task/report/backend/FIX-EPS-006-03.md) |
| FIX-EPS-006-04 | frontend | 🟡 | [Laporan](../task/report/frontend/FIX-EPS-006-04.md) |
| FIX-EPS-006-05 | frontend | 🟡 | [Laporan](../task/report/frontend/FIX-EPS-006-05.md) |
| FIX-EPS-006-06 | frontend | 🟡 | [Laporan](../task/report/frontend/FIX-EPS-006-06.md) |
| FIX-EPS-006-07 | backend | 🟡 | [Laporan](../task/report/backend/FIX-EPS-006-07.md) |
| FIX-EPS-006-08 | frontend | 🟡 | [Laporan](../task/report/frontend/FIX-EPS-006-08.md) |
