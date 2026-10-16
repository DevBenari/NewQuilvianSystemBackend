# Laporan Perubahan Backend — `RJ-REG-REV-BE-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-BE-004` |
| Judul | Dokumen identitas privat |
| Slice | Batch 2 — `RJ-REG-REV-006` (REV-06), mendukung `RJ-REG-REV-005` |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `3` |
| Trace | `RJ-REG-DEC-008`, `RJ-REG-DEC-009`; pola `RJ-DOC-REV-BE-019` (surat rujukan privat) |
| Contract version | Delta aditif pada `PatientIdentityDocumentController` |
| Dependency | — |
| Klasifikasi | `MEDIUM` |
| Task mode | `CROSS-REPO MODE` (`RJ-REG-DEC-001`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a52b3c98` (`sukmagp`), belum di-commit |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

**Backend Governance Preflight:** Area `HealthServices`, Module `PatientManagement / MasterData`, prefix `Mst`
(entity `MstPatientIdentityDocument` yang sudah ada, `TOUCHED LEGACY`); service baru `NEW CODE`. QBE:
`QBE-SVC-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DEL-001`, `QBE-TXN-001`.

## 1. Masalah yang diperbaiki

Pasien lama yang belum punya foto KTP tidak dapat dilengkapi dari Pendaftaran RJ. Entity
`MstPatientIdentityDocument` hanya menerima `FilePath` berupa teks dari klien, tanpa unggah berkas, dan foto KTP
lama (`MstPatient.PhotoPath`) tersimpan di `/uploads` publik.

## 2. Proses bisnis

1. Petugas memilih pasien lama → frontend memuat dokumen aktif pasien.
2. **Tambah Identitas**: unggah JPG/PNG/PDF (`multipart`) dengan `PatientId`, `IdentityType`, `IdentityNumber`.
   Server memeriksa: pasien ada, jenis identitas dikenal, ukuran ≤ `FileStorage:MaxIdentityDocumentSizeMb`
   (bawaan 5 MB), **magic bytes** (bukan ekstensi/MIME klien).
3. Berkas ditulis ke `FileStorage:PrivateRootPath/identity-documents/<patientId>/<idDokumen>.<ext>`; nama berkas
   pengguna hanya disimpan sebagai `DocumentName` yang disanitasi. `FilePath` = `private:…` (bukan URL).
4. Pasien yang **sudah** punya dokumen berkas aktif → `409` + `existingDocumentId`. Mengganti wajib
   `replaceDocumentId` dan izin `PatientIdentityDocument : Update`; dokumen lama dinonaktifkan + dihapus lunak
   (baris dan berkas tetap ada). Kunci advisory per pasien mencegah dua unggahan bersamaan.
5. **Unduh** lewat `GET admin/{id}/content` berizin: `inline`, `Cache-Control: no-store`, `nosniff`; path dibatasi
   ke folder pasien pemilik dokumen.
6. Audit mencatat id dokumen, pasien, dokumen yang diganti, tipe, ukuran, aktor — tanpa isi berkas dan tanpa NIK.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/PatientManagement/MasterData/Services/PatientIdentityDocumentFileService.cs` | Baru — unggah, buka, root privat, deteksi magic bytes, sanitasi nama |
| `…/MasterData/Controllers/PatientIdentityDocumentController.cs` | `POST admin/upload`, `GET admin/{id}/content`; `HasPrivateFile` pada mapper |
| `…/MasterData/DTOs/PatientIdentityDocumentDtos.cs` | `HasPrivateFile`; request/response unggah |
| `Program.cs` | Registrasi service |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru; field `hasPrivateFile` aditif |
| Database | Tanpa migration (memakai kolom yang ada) |
| Keamanan/Auth | Berkas privat, unduh berizin, magic bytes, batas ukuran, tanpa nama pengguna di path, audit tanpa PII. Antivirus/malware scanning: **tidak ada kebijakan/infra di repository** — dicatat sebagai risiko |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Identity Document

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/patient-identity-documents/admin/upload` | Unggah berkas identitas pasien lama | `PatientIdentityDocument : Create` (+ `Update` bila mengganti) |
| `GET` | `/patient-identity-documents/admin/{id}/content` | Buka berkas privat | `PatientIdentityDocument : Read` |

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build / EF / QBE Strict | `0 Error` / no changes / `PASS` | `PASS` |
| I1 unggah JPG → `200`; I2 tertaut ke pasien, primer, privat, nama `ktp<script>.jpg` → `ktp_script_.jpg` | sesuai | `PASS` |
| I3 unduh: isi identik, `inline`, `no-store`, `image/jpeg` | sesuai | `PASS` |
| I4 `/uploads/…` dan path mentah → `404` | sesuai | `PASS` |
| I5 unggah kedua tanpa replace → `409` + id lama | sesuai | `PASS` |
| I6/I7/I7b ganti → `200`, lama keluar dari daftar hidup dan tak bisa dibuka | sesuai (run ke-2) | `PASS` |
| I8 `.jpg` isi teks → `400`; I9 > 5 MB → `400`; I10 pasien tidak ada `404`; I11 jenis tak dikenal `400`; I12 tanpa login `401`; I13 dokumen tidak ada `404`; I14 tidak membuat pasien | sesuai | `PASS` |
| Layar U4–U11 (`RJ-REG-REV-FE-004`) | 9/9 | `PASS` |

Runtime BE-004+BE-005: run 1 **31/37** (2 cacat nyata ditemukan: unique index identitas dan antrean, lihat
catatan), run 2 sesudah perbaikan **37/37 PASS**.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. JPG/PNG/PDF ≤ batas → `200`, dokumen primer tertaut | Terpenuhi | I1, I2 |
| 2. Ekstensi palsu / melebihi batas → `400` | Terpenuhi | I8, I9 |
| 3. Pasien tidak ada → `404` | Terpenuhi | I10 |
| 4. `409` tanpa replace; replace menonaktifkan lama | Terpenuhi | I5–I7b |
| 5. Unduh berizin `inline` + `no-store`; tidak terbuka di `/uploads` | Terpenuhi | I3, I4 |
| 6. Tanpa izin `403` | `401` diuji; `403` lewat kode (akun uji SuperAdmin) | I12 |
| 7. Tidak membuat pasien; audit tanpa isi | Terpenuhi | I14, kode |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Delta desain: dokumen yang diganti juga ditandai `IsDelete` karena unique index `(PatientId, IdentityType, IdentityNumber)` hanya untuk baris hidup |
| Masalah yang diketahui | `PhotoPath` publik lama (Kiosk/IGD) tetap publik — di luar scope (`RJ-REG-DEC-008`), direkomendasikan kampanye terpisah |
| Risiko tersisa | Tidak ada malware scanning; PUT legacy masih dapat menulis `FilePath` bebas (unduh dibatasi ke folder pasien) |
| Perubahan sampingan | Berkas uji di `Storage/private/identity-documents/` (ter-ignore `/Storage/private/`) |
| Status Git | Berkas di atas `M`/`??` |
| Langkah berikutnya | Tentukan kebijakan antivirus dan migrasi `PhotoPath` publik |
