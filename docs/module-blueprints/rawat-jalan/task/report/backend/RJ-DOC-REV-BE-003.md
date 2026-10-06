# Laporan Perubahan Backend — `RJ-DOC-REV-BE-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-003` |
| Judul | Perbaiki simpan SOAP |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 2f |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.2` |
| Trace | `RJ-DOC-DEC-007` |
| Contract version | Tidak berubah |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — investigasi runtime; nol berkas aplikasi diubah |
| Task mode | `BACKEND` |
| Target tulis | Dokumen blueprint rawat-jalan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001` |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 Sebagian — simpan SOAP terbukti berhasil end-to-end pada `HEAD`; kegagalan UAT `28/9` **tidak dapat direproduksi**, penyebabnya belum terbukti |

---

## 1. Masalah yang dilaporkan

UAT `28/9/2026`: *"Proses simpan SOAP masih belum bisa"*. Akibatnya CPPT juga tidak dapat diuji.

---

## 2. Proses bisnis yang diuji ulang

Alur yang sama dengan yang dijalankan layar SOAP (`use-doctor-soap.js`):

1. Buka tab SOAP → `GET /doctor-consultations/active-by-queue/{queueId}`.
2. Bila belum ada konsultasi → `POST /doctor-consultations` (konsultasi baru, status `InProgress`).
3. Cari ICD-10 → `GET /patient-diagnoses/master-options?search=…`.
4. Tambah diagnosis → `POST /patient-diagnoses`.
5. Muat rekomendasi → `POST /diagnosis-recommendations/resolve`.
6. Dokter mengetik → autosave `PATCH /doctor-consultations/{id}/soap` setiap 1 detik.
7. Muat ulang → nilai tersimpan terbaca kembali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DoctorConsultationController.cs` (`UpdateSoap`, `EnsureSoleAuthorAsync`), `InpatientClinicalContextService.ResolveForAuthorEditAsync`,
`ClinicalDocumentIntegrityService`, frontend `use-doctor-soap.js`, `doctor-soap-utils.js` (`buildSoapPatchPayload`),
`doctor-consultation.service.js`, `patient-diagnosis.service.js`, `diagnosis-recommendation.service.js`,
tabel hak akses `SysControllerAccess`/`SysActionAccess`/`SysAccessPolicy`, riwayat Git.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source backend |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Tidak ada perubahan schema. Lihat 5.2 untuk data uji |
| Keamanan/Auth | `NOT APPLICABLE`. Hak akses `DoctorConsultation : WriteSoap` sudah diberikan ke `3` posisi di `QuilvianNewDevSukma`, sama dengan `Create`/`Update` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint yang berubah.

---

## 5. Verifikasi

| Skenario | Hasil sebenarnya | Klasifikasi |
| --- | --- | :---: |
| R1 `PATCH /soap` dengan payload persis `buildSoapPatchPayload` pada dua konsultasi `InProgress` | `200` "SOAP konsultasi dokter berhasil disimpan." | `PASS` |
| R2 `PATCH /soap` pada konsultasi `Completed` | `400` "SOAP pada konsultasi yang sudah completed tidak dapat diubah." — penolakan yang benar | `PASS` |
| R3 Antrean `G002` tanpa konsultasi: `active-by-queue` lalu `POST /doctor-consultations` | `404` lalu `200`, konsultasi `CON-20261001-00001` | `PASS` |
| R4 `master-options?search=hypertension` → `POST /patient-diagnoses` (`I10`) → `POST /diagnosis-recommendations/resolve` | `200` / `200` / `200` | `PASS` |
| R5 `PATCH /soap` pada konsultasi baru lalu `GET /{id}` | `200`; `subjective` terbaca `TEST-REVBE003 S2` | `PASS` |
| `dotnet build`, `has-pending-model-changes`, QBE | — | `NOT RUN` — tidak ada perubahan source |

Uji dijalankan agent lewat HTTP sungguhan pada aplikasi scratchpad `localhost:5217` terhadap
`QuilvianNewDevSukma`, sesi `superadmin`.

**Tidak dijalankan:** uji sebagai akun dokter (kredensial akun dokter tidak tersedia).

### 5.1 Dugaan penyebab — belum terbukti

| Dugaan | Bukti | Keadaan |
| --- | --- | --- |
| Skema DB tertinggal dari kode | Commit `06678c61` (`30/9`) menambah kolom `TrxDoctorConsultation.SourceVitalSignId` lewat migration `20260930110000`. Lingkungan yang menjalankan kode itu tanpa migration-nya gagal pada **setiap** baca/tulis konsultasi. `QuilvianNewDevSukma` sendiri tertinggal `4` migration sampai `1/10` | Kuat untuk lingkungan pasca `30/9`; **tidak** menjelaskan UAT `28/9` |
| Hak akses `WriteSoap` tidak diberikan ke posisi dokter | Autosave memakai butir hak akses tersendiri (`WriteSoap`), terpisah dari `Update`. Di DB Sukma sudah diberikan | Perlu dicek di lingkungan UAT |
| Konsultasi dibuat dokter lain / rawat inap (`AuthorMismatch` `403`) | Penjaga penulis hanya aktif untuk kunjungan rawat inap | Tidak berlaku untuk rawat jalan |

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| `CON-20260930-00002`, `CON-20260826-00001` | SOAP tertimpa nilai uji `TEST-REVBE003 S` / `O` / `A` / `P` (data dev sebelumnya hilang pada empat field itu) |
| `CON-20261001-00001` | Konsultasi baru untuk antrean `G002` (`2026-07-15`), `subjective = TEST-REVBE003 S2`, satu diagnosis `I10` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `PATCH /doctor-consultations/{id}/soap` dari alur dokter rawat jalan berbalas `200` dan tersimpan | Terpenuhi | R1, R3–R5 |
| Penyebab kegagalan dibuktikan runtime | **Belum terpenuhi** — tidak dapat direproduksi pada `HEAD` dengan DB termigrasi | 5.1 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Lingkungan yang dipakai UAT wajib menerapkan seluruh migration `HEAD` (`dotnet ef database update`) sebelum diuji ulang |
| Masalah yang diketahui | Penyebab UAT `28/9` belum terbukti |
| Risiko tersisa | Bila posisi dokter di lingkungan UAT tidak memegang `DoctorConsultation : WriteSoap`, autosave tetap gagal `403` meski kode benar |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Hanya laporan ini dan dokumen roadmap/traceability |
| Langkah berikutnya | `RJ-DOC-REV-FE-003` menguji simpan SOAP dari browser; pemilik mengecek migration dan hak akses `WriteSoap` di lingkungan UAT |
