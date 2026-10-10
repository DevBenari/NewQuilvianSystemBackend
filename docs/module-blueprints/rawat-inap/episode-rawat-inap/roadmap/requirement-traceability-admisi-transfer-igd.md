# Matriks Ketertelusuran Kebutuhan (Requirement Traceability Matrix) — Admisi Transfer Pasien IGD & Rawat Jalan

| Field | Nilai |
|---|---|
| Modul | `rawat-inap` / Sub-modul `episode-rawat-inap` |
| Blueprint ID | `RWI-BP-001` revision `11` |
| Kontrak Versi | **`0.12.0` `approved`** (10 Oktober 2026) |
| Status Roadmap | **`APPROVED`** — disetujui pengguna 10 Oktober 2026 sebagai rencana pengiriman resmi alur admisi transfer pasien IGD dan Poliklinik |
| Ditulis Oleh | `plan-module-delivery` |
| Dokumen Acuan | Analisis Bisnis (Alur 4 Tahap A-B-C-D), `backend-roadmap-admisi-transfer-igd.md`, `frontend-roadmap-admisi-transfer-igd.md`, API Contract `0.12.0` bagian 11.7 |
| Source Commit Baseline | Backend: `fdf85a07` (branch `MHamzah`); Frontend: `dd2cbf7c` (branch `HamzahV2`) |

---

## 1. Tabel Ketertelusuran Menyeluruh (Traceability Matrix)

Tabel berikut menghubungkan kebutuhan fungsional bisnis, keputusan bisnis, kriteria penerimaan, kontrak validasi, task backend, task frontend, rencana verifikasi bukti, dan status pelaksanaan.

| ID Kebutuhan | Keputusan Bisnis | Deskripsi Kebutuhan & Hasil Akhir | Kontrak Desain & Validasi | Task Backend | Task Frontend | Kriteria Penerimaan (AC) | Rencana Verifikasi & Bukti | Status |
|---|---|---|---|---|---|---|---|:---:|
| `FR-RI-187` | `RWI-DEC-274` | Petugas admisi ranap menerima daftar permintaan rawat inap dari IGD secara elektronik (Tahap A) | API Contract `0.12.0` (11.7.1), Permission Matrix `0.12.0` | [`BE-RWI-206`](../task/report/backend/BE-RWI-206.md) ✅ | [`FE-RWI-228`](../task/report/frontend/FE-RWI-228.md) ✅ | `RWI-AC-396` | Integrasi query API: seluruh pasien dengan disposisi IGD `Confirmed` muncul di antrean admisi | `COMPLETED_PROVEN` |
| `FR-RI-188` | `RWI-DEC-276` | Peringatan visual dan filter waktu tunggu pasien transfer jika melebihi ambang batas toleransi (> 60 menit) | API Contract `0.12.0` (11.7.1), `05-skema-tampilan.md` 3.3 | [`BE-RWI-206`](../task/report/backend/BE-RWI-206.md) ✅ | [`FE-RWI-228`](../task/report/frontend/FE-RWI-228.md) ✅ | `RWI-AC-397` | Perhitungan `WaitingMinutes` otomatis; baris berubah warna kuning/merah jika melebihi 60 menit | `COMPLETED_PROVEN` |
| `FR-RI-189` | `RWI-DEC-274` | Wali pasien diarahkan ke loket admisi ranap dan petugas memilih opsi kartu ke-4 "Transfer IGD / Poliklinik" (Tahap B) | `05-skema-tampilan.md` 3.0, Constants Flow `0.12.0` | — | [`FE-RWI-227`](../task/report/frontend/FE-RWI-227.md) ✅ | `RWI-AC-400` | UI component test: kartu ke-4 dirender dengan teks panduan dan mengarahkan ke antrean transfer | `COMPLETED_PROVEN` |
| `FR-RI-190` | `RWI-DEC-275` | Petugas admisi membuka data pasien transfer, data identitas, DPJP, dan diagnosa IGD terisi otomatis (*auto-populate*) tanpa ketik ulang (Tahap C) | API Contract `0.12.0` (11.7.2), State Transition `0.12.0` | [`BE-RWI-206`](../task/report/backend/BE-RWI-206.md) ✅ | [`FE-RWI-229`](../task/report/frontend/FE-RWI-229.md) ✅ | `RWI-AC-401`, `RWI-AC-403` | Mock flow test: klik "Proses Admisi" memuat data pasien, DPJP, dan diagnosa IGD tanpa input ulang | `COMPLETED_PROVEN` |
| `FR-RI-191` | `RWI-DEC-275` | Petugas dan wali menyelesaikan pemilihan penjamin, alokasi tempat tidur, dan tanda tangan digital persetujuan rawat inap (Tahap C lanjutan) | Validation Matrix `0.12.0`, Kanvas TTD `FE-RWI-225` | — | [`FE-RWI-229`](../task/report/frontend/FE-RWI-229.md) ✅ | `RWI-AC-404` | Stepper validation test: kamar ter-booking, skema bayar tersimpan, tanda tangan digital terekam | `COMPLETED_PROVEN` |
| `FR-RI-192` | `RWI-DEC-275` | Pendaftaran selesai (Tahap D): nomor episode ranap terbit, status disposisi IGD berubah menjadi `Executed`, dan berkas registrasi/gelang dicetak | API Contract `0.12.0` (11.7.3), `05-skema-tampilan.md` 3.4 | [`BE-RWI-207`](../task/report/backend/BE-RWI-207.md) ✅ | [`FE-RWI-230`](../task/report/frontend/FE-RWI-230.md) ✅ | `RWI-AC-398`, `RWI-AC-405` | E2E API & UI test: episode baru lahir, disposisi IGD `Executed`, pasien hilang dari antrean tunggu | `COMPLETED_PROVEN` |

---

## 2. Analisis Cakupan Uji & Coverage Gap

1. **Requirement-to-Test Coverage:**
   - Seluruh 6 kebutuhan fungsional (`FR-RI-187` s.d. `FR-RI-192`) dan kriteria penerimaan (`RWI-AC-396` s.d. `RWI-AC-405`) memiliki keterkaitan langsung dengan task teknis backend (`BE-RWI-206`, `BE-RWI-207`) dan frontend (`FE-RWI-227` s.d. `FE-RWI-230`).
   - **Coverage Gap:** `NOL` (0 gap). Tidak ada satu pun alur proses bisnis dari Tahap A hingga D yang luput dari rencana kerja.
2. **Ketergantungan Lintas Lapisan (Backend ke Frontend):**
   - Task `FE-RWI-228` (Tabel Antrean) dan `FE-RWI-229` (Pre-fill Stepper) bergantung secara kontraktual pada endpoint `BE-RWI-206`.
   - Task `FE-RWI-230` (Penyelesaian Admisi & Eksekusi Disposisi) bergantung pada endpoint transaksional `BE-RWI-207`.
   - Backend diselesaikan terlebih dahulu per vertical slice, memastikan tim frontend bekerja di atas kontrak API yang sudah tervalidasi dan siap pakai.
3. **Penyelarasan Siklus Hidup Pasien IGD ke Bangsal:**
   - Transisi status `EmgDisposition` dari `Confirmed` menjadi `Executed` pada saat admisi ranap diterbitkan memastikan sistem IGD selalu tersinkronisasi tanpa memerlukan entri data ganda dari staf perawat.
