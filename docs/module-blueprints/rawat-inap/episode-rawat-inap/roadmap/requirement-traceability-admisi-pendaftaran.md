# Matriks Ketertelusuran Kebutuhan (Requirement Traceability Matrix) — Amandemen Admisi Pendaftaran

| Field | Nilai |
|---|---|
| Modul | `rawat-inap` / Sub-modul `episode-rawat-inap` |
| Blueprint ID | `RWI-BP-001` revision `10` |
| Kontrak Versi | **`0.11.0` `approved`** (10 Oktober 2026) |
| Status Roadmap | **`APPROVED`** — disetujui pengguna 10 Oktober 2026 atas instruksi "setujui dan lakukan /plan-module-delivery" |
| Ditulis Oleh | `plan-module-delivery` |
| Dokumen Acuan | `00-interview-decisions.md` rev `40`, `04-prd-to-mvp.md` rev `0.11.0`, `03-frontend-architecture.md` rev `0.11`, `05-skema-tampilan.md` rev `0.7`, `contracts/validation-matrix.md` rev `0.11.0` |
| Source Commit Baseline | Backend: `fdf85a07` (branch `MHamzah`); Frontend: `dd2cbf7c` (branch `HamzahV2`) |

---

## 1. Tabel Ketertelusuran Menyeluruh (Traceability Matrix)

Tabel berikut menghubungkan kebutuhan fungsional bisnis, keputusan wawancara, kriteria penerimaan, kontrak validasi, task backend, task frontend, bukti verifikasi, dan status implementasi.

| ID Kebutuhan | Keputusan Bisnis | Deskripsi Kebutuhan & Hasil Akhir | Kontrak Desain & Validasi | Task Backend | Task Frontend | Kriteria Penerimaan (AC) | Rencana Verifikasi & Bukti | Status |
|---|---|---|---|---|---|---|---|:---:|
| `FR-RI-179` | `RWI-DEC-267` | No. HP Kontak Darurat dibatasi maksimal 13 karakter numerik pada form pendaftaran pasien baru | Validation Matrix `0.11.0` (`VAL-ADM-01`), `05-skema-tampilan.md` 3.2A | [`BE-RWI-204`](../task/report/backend/BE-RWI-204.md) ✅ | [`FE-RWI-222`](../task/report/frontend/FE-RWI-222.md) ✅ | `RWI-AC-388` | Backend validator & DTO membatasi 13 digit numerik; Frontend masking test | `COMPLETED_PROVEN` |
| `FR-RI-183` | `RWI-DEC-268` | Dropdown Unit Tujuan di langkah Dokter disembunyikan dari antarmuka; payload pendaftaran otomatis mengikat Service Unit rawat inap default | Validation Matrix `0.11.0` (`VAL-ADM-02`), `05-skema-tampilan.md` 3.2F | [`BE-RWI-204`](../task/report/backend/BE-RWI-204.md) ✅ | [`FE-RWI-222`](../task/report/frontend/FE-RWI-222.md) ✅ | `RWI-AC-389` | Backend auto-resolve default ranap; DOM inspection (dropdown unit disembunyikan) | `COMPLETED_PROVEN` |
| `FR-RI-180` | `RWI-DEC-269` | Langkah 1 Pasien Baru menyediakan opsi Umum & Rujukan; opsi rujukan membuka form teks ringkas (No, Tgl/Jam, Faskes, Dokter, Diagnosa) tanpa upload file fisik | `05-skema-tampilan.md` 3.2A, Validation Matrix `0.11.0` (`VAL-ADM-03`) | — *(menggunakan DTO intake)* | [`FE-RWI-223`](../task/report/frontend/FE-RWI-223.md) ✅ | `RWI-AC-390` | Interactive UI test (klik Umum lanjut; klik Rujukan membuka 5 isian teks; verifikasi ketiadaan tombol upload fisik) | `COMPLETED_PROVEN` |
| `FR-RI-181` | `RWI-DEC-270` | Pilihan kategori pasien pada Langkah 2 Pasien Baru disaring menjadi tepat 3 opsi: Pasien Umum, Bayi Baru Lahir, Pegawai RS | `05-skema-tampilan.md` 3.2B | — | [`FE-RWI-223`](../task/report/frontend/FE-RWI-223.md) ✅ | `RWI-AC-391` | Visual verification test (hanya 3 kartu kategori yang dirender; opsi redundant seperti Ibu/Anak/Korporat tidak muncul) | `COMPLETED_PROVEN` |
| `FR-RI-182` | `RWI-DEC-271` | Langkah 1 Pasien Lama menyatukan pencarian No RM/NIK dan kartu verifikasi identitas ke dalam 1 layar split terpadu (kolom kanan-kiri) | `05-skema-tampilan.md` 3.2E, `03-frontend-architecture.md` 2A & 3A | — | [`FE-RWI-224`](../task/report/frontend/FE-RWI-224.md) ✅ | `RWI-AC-392` | Responsive layout test desktop/tablet (pencarian langsung memunculkan kartu verifikasi di panel kiri; tombol Ganti Pasien berfungsi) | `COMPLETED_PROVEN` |
| `FR-RI-184` | `RWI-DEC-272` | Langkah Mandiri (*Dedicated Step*) Form Persetujuan Rawat Inap & Kanvas Tanda Tangan Digital interaktif sebelum cetak; penyimpanan dokumen ke episode | API Contract `0.11.0`, Validation Matrix `0.11.0` (`VAL-ADM-04`), `05-skema-tampilan.md` 3.2C & 3.2D | [`BE-RWI-205`](../task/report/backend/BE-RWI-205.md) ✅ | [`FE-RWI-225`](../task/report/frontend/FE-RWI-225.md) ✅ | `RWI-AC-393` | Integration test simpan persetujuan; Canvas signature interaction test (gambar TTD, tombol bersihkan, tombol kunci TTD, submit payload Base64) | `COMPLETED_PROVEN` |
| `FR-RI-185` | `RWI-DEC-273` | Pasien kategori Bayi Baru Lahir secara mutlak mengunci subjek penanda tangan kepada Orang Tua / Wali sah; penolakan opsi Pasien Sendiri | Validation Matrix `0.11.0` (`VAL-ADM-05`), Invariant `INV-ADM-01` | [`BE-RWI-205`](../task/report/backend/BE-RWI-205.md) ✅ | [`FE-RWI-225`](../task/report/frontend/FE-RWI-225.md) ✅ | `RWI-AC-394` | Security & legal rule test (radio Diri Sendiri disabled di frontend; backend menolak HTTP 422 jika penanda tangan = Self untuk bayi) | `COMPLETED_PROVEN` |
| `FR-RI-186` | `RWI-DEC-272` | Dokumen General Consent di Workspace PPRI berstatus murni *read-only / print-ready* menampilkan formulir dan citra TTD digital tanpa form isian aktif | `03-frontend-architecture.md` 2A & 14.4.3, `05-skema-tampilan.md` 3.2C, Flowchart Bagian 3 | [`BE-RWI-205`](../task/report/backend/BE-RWI-205.md) ✅ *(GET endpoint)* | [`FE-RWI-226`](../task/report/frontend/FE-RWI-226.md) ✅ | `RWI-AC-395` | Cross-module flow verification (buka menu General Consent PPRI setelah admisi selesai; lembar persetujuan ber-TTD siap cetak tanpa form input ganda) | `COMPLETED_PROVEN` |

---

## 2. Analisis Cakupan Uji & Coverage Gap

1. **Requirement-to-Test Coverage:**
   - Seluruh 8 kebutuhan fungsional (`FR-RI-179` s.d. `FR-RI-186`) dan 8 kriteria penerimaan (`RWI-AC-388` s.d. `RWI-AC-395`) memiliki rencana verifikasi berbasis bukti konkret.
   - **Coverage Gap:** `NOL` (0 gap). Tidak ada kebutuhan atau kriteria penerimaan yang tidak memiliki penanggung jawab task backend atau frontend.
2. **Ketergantungan Lintas Lapisan (Backend ke Frontend):**
   - Task `FE-RWI-222` bergantung pada kontrak dan validasi backend `BE-RWI-204`.
   - Task `FE-RWI-225` dan `FE-RWI-226` bergantung pada kesiapan endpoint penyimpanan dan pembacaan tanda tangan digital `BE-RWI-205`.
   - Pola pengiriman dirancang berurutan sehingga tim frontend tidak pernah menebak payload atau skema response backend.
3. **Penyelarasan Ruang Kerja PPRI:**
   - Workspace PPRI yang sebelumnya diselesaikan pada `FE-RWI-214` (General Consent cetak saja) kini disempurnakan oleh `FE-RWI-226` agar mengonsumsi data tanda tangan digital yang disimpan di loket pendaftaran admisi, sehingga siklus dokumen penerimaan pasien rawat inap menjadi terpadu dan tanpa duplikasi kerja.
