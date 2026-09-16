# Laporan Perubahan Backend — `BE-RWI-089`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-089` |
| Judul | Verifikasi CPPT hanya oleh DPJP aktif |
| Slice | Gelombang 1 — `DOK-V2-0` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-089` |
| Trace | `FR-DOK-082`; `INV-DOK-16`; `VAL-DOK-44` |
| Contract version | `0.6.0` |
| Dependency | Tidak ada dependency task untuk perilaku episode berjalan; pengecualian episode `Closed` dilanjutkan oleh `BE-RWI-095` |
| Klasifikasi | `HEAVY` — keputusan kewenangan klinis untuk verifikasi rekam medis |
| Task mode | `BACKEND` |
| Target tulis | `HealthServices/ClinicalManagement` dan dokumentasi delivery sub-modul dokter rawat inap |
| Model | GPT-5 |
| Commit backend saat diverifikasi | `a0a710db24490b253ebd411d3ee550cd545200fb`, branch `MHamzah` |
| Tanggal | 2026-09-16 |
| Status | ✅ **SELESAI 16 September 2026** berdasarkan validasi source; `dotnet build` tidak dijalankan sesuai instruksi eksplisit pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / ClinicalManagement` |
| Registry/prefix | `ClinicalManagement` terdaftar `ACTIVE`; tidak ada entity atau prefix baru |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-LOG-001` |
| Penempatan aturan | `CpptVerificationService` mengorkestrasi keputusan; pencarian assignment DPJP berada pada `InpatientClinicalContextService` |
| Database | `NOT APPLICABLE` — hanya membaca assignment dan memperbarui field verifikasi yang sudah ada |

## 1. Masalah yang diperbaiki

Penugasan dokter memiliki beberapa peran: DPJP, konsulen, dan dokter jaga. Ketiganya boleh membaca
dan menulis dalam lingkup penugasannya, tetapi pernyataan bahwa CPPT profesi lain sudah dibaca dan
disetujui hanya boleh dibuat oleh DPJP yang bertanggung jawab saat verifikasi dilakukan.

Contoh: dr. Sari adalah konsulen aktif dan dr. Ahmad adalah DPJP aktif. Keduanya dapat membaca
catatan perawat. Saat menekan Verifikasi, dr. Sari menerima `403`, sedangkan dr. Ahmad dapat
melanjutkan. Bila dr. Ahmad sudah digantikan dr. Rina, hak verifikasinya ikut berakhir.

## 2. Proses bisnis

1. Endpoint mengambil pengguna login dan menyelesaikan identitas dokternya dari data akun.
2. Service menemukan episode rawat inap milik catatan.
3. Untuk episode berjalan, service mencari assignment dokter yang memenuhi seluruh syarat:
   `AssignmentRole = Dpjp`, aktif, belum dihapus, sudah mulai, dan belum berakhir pada waktu
   verifikasi.
4. Bila salah satu syarat tidak terpenuhi, service mengembalikan `403` dengan pesan bahwa hanya
   DPJP yang sedang bertugas dapat memverifikasi.
5. Bila pelaku juga penulis catatan, verifikasi tetap ditolak `403`.
6. Jika sah, service hanya mengubah status, waktu, verifikator, dan audit update. Identitas serta
   isi penulis asli tidak berubah.

Episode `Closed` mengikuti pengecualian sempit `BE-RWI-095`: hanya DPJP terakhir dan hanya catatan
prapenutupan. Pengecualian itu tidak melonggarkan aturan episode berjalan pada task ini.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `CpptVerificationService.cs`
- `InpatientClinicalContextService.cs`
- `PatientIntegratedProgressNoteController.cs`
- `permission-audit-matrix.md`, `validation-matrix.md`, dan `api-contract.md` versi `0.6.0`

### 3.2 Berkas implementasi

| Berkas | Perubahan |
| --- | --- |
| `Services/InpatientClinicalContextService.cs` | `IsDpjpAssignedAsync` menyaring assignment berdasarkan role `Dpjp`, status aktif, dan rentang waktu |
| `Services/CpptVerificationService.cs` | `VerifyAsync` memakai penilaian kewenangan terpusat; konsulen, dokter jaga, DPJP lama, dan akun tanpa dokter ditolak `403` |
| `Controllers/PatientIntegratedProgressNoteController.cs` | Endpoint verifikasi meneruskan ID pengguna dan ID dokter hasil resolusi server ke service |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route dan payload tidak berubah; semantik `403` dipertegas untuk bukan DPJP aktif |
| Database | `NOT APPLICABLE` — nol schema/migration; hanya kolom verifikasi existing yang diperbarui |
| Keamanan/Auth | Metadata `PatientIntegratedProgressNote : Verify` tetap; pemeriksaan hubungan DPJP–episode diperketat di service |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/verify` | DPJP aktif menyatakan sudah membaca catatan profesi lain | `PatientIntegratedProgressNote : Verify` | — | `ApiResponse<PatientIntegratedProgressNoteResponse>` |

Kode `403` berarti pelaku bukan DPJP aktif, tidak tertaut ke dokter, atau mencoba memverifikasi
catatannya sendiri. Kode `409` berarti catatan sudah diverifikasi. Pembacaan CPPT tetap memakai
`PatientIntegratedProgressNote : Read` dan tidak dibatasi menjadi DPJP saja.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker `Strict`, rentang `36db5e6d..a0a710db` | 17 berkas dievaluasi; nol finding | `PASS` | `Final result: PASS` |
| QBE checker `Strict`, working tree akhir | 4 berkas controller dievaluasi; nol finding | `PASS` | `Final result: PASS` |
| Penyaring role assignment | Query mensyaratkan `AssignmentRole == Dpjp` | `PASS` | Review `IsDpjpAssignedAsync` |
| Penyaring waktu verifikasi | `StartDateTime <= now` dan `EndDateTime` belum lewat | `PASS` | Review query assignment |
| Konsulen/dokter jaga | Tidak memenuhi role DPJP dan mendapat hasil `403` | `PASS` | Jalur gagal `PenolakanVerifikasiBukanDpjpAktif` |
| DPJP lama | Assignment yang berakhir tidak cocok pada waktu verifikasi | `PASS` | Penyaring `EndDateTime > atUtc` |
| Akses baca | Endpoint baca tidak diberi pemeriksaan DPJP baru | `PASS` | Review controller dan diff |
| Penulis asli | `ProviderUserId` serta isi catatan tidak ditulis ulang saat verify | `PASS` | Review `VerifyAsync` |
| `dotnet build` | Tidak dijalankan atas instruksi eksplisit pengguna | `NOT RUN` | Pengguna akan menjalankan build mandiri |
| Uji runtime API | Tidak dijalankan karena aplikasi tidak dibangun/dijalankan | `NOT RUN` | Verifikasi statis saja |

Uji manual: `NOT FEASIBLE` pada sesi ini karena membutuhkan runtime serta assignment DPJP,
konsulen, dan dokter jaga yang berbeda.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — verifikasi hanya oleh DPJP aktif pada detik verifikasi | Terpenuhi secara statis | Role, status, dan rentang assignment diperiksa terhadap `nowUtc` |
| AC-2 — konsulen dan dokter jaga menerima `403` | Terpenuhi secara statis | Kedua role tidak cocok dengan `InpDoctorAssignmentRole.Dpjp` |
| AC-3 — dokter yang dahulu DPJP menerima `403` | Terpenuhi secara statis | Assignment yang masa berlakunya berakhir tidak cocok |
| AC-4 — membaca CPPT tetap terbuka bagi dokter berpenugasan | Terpenuhi secara statis | Pembatasan hanya dipanggil oleh action verifikasi; endpoint baca tidak berubah |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini dan pembaruan register |
| `dotnet build` | Belum diverifikasi | `NOT RUN` atas instruksi pemilik; bukan klaim build berhasil |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bukti kompilasi dan perilaku runtime menunggu build mandiri pemilik |
| Masalah yang diketahui | Tidak ada masalah source baru yang ditemukan melalui review statis |
| Risiko tersisa | Konfigurasi data assignment aktual baru terbukti saat smoke test |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git saat laporan ditulis | Source implementation berada pada commit `a0a710db`; working tree memuat laporan/register dan metadata response API terkait rangkaian task |
| Langkah berikutnya | Pemilik menjalankan build dan smoke test dengan akun DPJP, konsulen, dokter jaga, serta DPJP lama |
