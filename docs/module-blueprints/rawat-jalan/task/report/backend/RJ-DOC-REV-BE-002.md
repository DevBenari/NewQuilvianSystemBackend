# Laporan Perubahan Backend — `RJ-DOC-REV-BE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-002` |
| Judul | Risiko jatuh tidak dipilih tidak tersimpan dengan skor |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 1a |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.2` |
| Trace | `RJ-DOC-DEC-007`; `BE-RWI-109`; `BE-RWI-056` |
| Contract version | Tidak berubah |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — investigasi; nol berkas aplikasi diubah |
| Task mode | `BACKEND` |
| Target tulis | Dokumen blueprint rawat-jalan saja (tidak ada perubahan source yang dibutuhkan) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai — perilaku backend sudah sesuai; tidak ada perubahan kode. Tampilan angka diperbaiki di `RJ-DOC-REV-FE-002` |

---

## 1. Masalah yang diperbaiki

Pada tab *Hasil Skrining* dokter, kolom **Risiko Jatuh** menampilkan angka `1` untuk pasien yang risiko
jatuhnya **tidak dipilih**. Pemilik membacanya sebagai "skor tersimpan lebih dari 0".

Penelusuran menunjukkan angka itu **bukan skor**:

| Field | Nilai tersimpan saat risiko jatuh tidak dipilih | Arti |
| --- | --- | --- |
| `HasFallRisk` | `false` | Tidak ada risiko |
| `FallRiskScore` | `null` | **Skor kosong** — sudah sesuai permintaan |
| `FallRiskStatus` | `1` (`NoRisk`) | Kategori "Tidak berisiko", disimpan sebagai angka enum |

`AssessmentHistoryTable.jsx` menampilkan `fallRiskNote`, dan bila kosong jatuh ke `fallRiskStatus`
mentah. Hasilnya angka `1` tampil seolah-olah skor.

---

## 2. Proses bisnis

1. Perawat atau dokter mengisi asesmen dan tidak mencentang Risiko Jatuh.
2. Backend menghitung `FallRiskScore = null` (`CalculateFallRiskScore` mengembalikan `null` bila tidak
   ada risiko) dan merapikan `HasAtaxia`, `HasPosturalInstability`, serta skor menjadi kosong
   (`PatientAssessmentController.cs` blok normalisasi `!entity.HasFallRisk`).
3. Kategori diisi `NoRisk` karena memang sudah dikaji dan tidak berisiko — berbeda dari `Unknown`
   ("belum dikaji"). Kategori ini dipertahankan; mengubahnya menjadi `0` akan menghapus beda
   "tidak berisiko" dan "belum dikaji" yang dipakai `BE-RWI-056`.
4. Layar wajib menampilkan **skor** (`0`/`-`) dan **label kategori**, bukan angka enum — dikerjakan
   `RJ-DOC-REV-FE-002`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientAssessmentController.cs` (`CalculateAssessmentValues` dua overload, `CalculateFallRiskScore`,
normalisasi entity), `TrxPatientAssessment.cs`, frontend `AssessmentHistoryTable.jsx`,
`doctor-queue.constants.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source backend |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | `NOT APPLICABLE` — tidak ada perubahan schema maupun data |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint yang berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Query baca `TrxPatientAssessment` di `QuilvianNewDevSukma` | `50` baris `HasFallRisk = false`: seluruhnya `FallRiskScore = null`, `FallRiskStatus = 1`. `16` baris `HasFallRisk = true`: skor `1`, status `3` | `PASS` | Nol baris "tidak berisiko" yang berskor > 0 |
| R1 — `GET /patient-assessments?patientId=…` pasien antrean `G001`, `G002` (`2026-07-15`) | `hasFallRisk = false` → `fallRiskScore = null`, `fallRiskStatus = 1` | `PASS` | Runtime aplikasi scratchpad `localhost:5217` |
| `dotnet build`, `has-pending-model-changes`, QBE | — | `NOT RUN` | Tidak ada perubahan source; hasil `RJ-DOC-REV-BE-001` berlaku untuk working tree yang sama |

Uji manual: `PASS`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Asesmen dengan `HasFallRisk = false` menyimpan `FallRiskScore` `null`/`0`, tidak lebih dari `0` | Terpenuhi (perilaku yang sudah ada) | Bagian 5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Angka `1` di layar adalah enum kategori, bukan skor |
| Masalah yang diketahui | Tampilan kolom Risiko Jatuh — diserahkan ke `RJ-DOC-REV-FE-002` |
| Risiko tersisa | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Hanya laporan ini dan dokumen roadmap/traceability yang bertambah |
| Langkah berikutnya | `RJ-DOC-REV-FE-002` menampilkan skor `0`/`-` dan label kategori |
