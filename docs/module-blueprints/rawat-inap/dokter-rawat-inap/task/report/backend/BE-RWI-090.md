# Laporan Perubahan Backend — `BE-RWI-090`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-090` |
| Judul | Tindakan baru pada episode tertutup ditolak |
| Slice | Gelombang 1 — `DOK-V2-0` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-090` |
| Trace | `FR-DOK-081`; `RLN3-CAP-35`; `VAL-DOK-58` |
| Contract version | `0.6.0` |
| Dependency | Tidak ada dependency task untuk `BE-RWI-090` |
| Klasifikasi | `MEDIUM` — satu endpoint transaksi, penjaga status episode, dan regresi jalur non-rawat-inap |
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
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001` |
| Penempatan aturan | Controller memanggil resolver konteks episode yang sudah ada sebelum validasi dan penyimpanan tindakan |
| Database | `NOT APPLICABLE` — nol schema, entity, configuration, migration, atau eksekusi database |

## 1. Masalah yang diperbaiki

Jalur pembuatan tindakan dari catatan dokter sebelumnya memanggil penjaga dengan
`forNewDocument: false`. Nilai itu membuat episode tertutup diperlakukan seperti koreksi dokumen
lama, padahal action tersebut selalu membuat baris tindakan baru yang dapat ikut diproses ke
billing.

Masalahnya lebih lebar ketika request tidak mengirim `InpEpisodeId`: backend belum menilai status
episode dari kunjungannya. Sekarang episode dicari dari `EncounterId`, sehingga penanda episode
yang dihilangkan dari payload tidak dapat dipakai untuk melewati penjaga.

## 2. Proses bisnis

1. Dokter mengirim permintaan pembuatan tindakan.
2. Setelah pemeriksaan idempotency, backend menemukan episode rawat inap dari `EncounterId`.
3. Bila episode `Closed` atau `Cancelled`, backend berhenti sebelum validasi tarif, coverage, atau
   penyimpanan, lalu mengembalikan `422` beserta alasan yang dapat dipahami pengguna.
4. Bila episode masih dapat menerima dokumen baru, proses lama dilanjutkan: kewenangan dokter,
   validasi tindakan, tarif, coverage, dan penyimpanan.
5. Bila kunjungan tidak memiliki episode rawat inap, resolver menjawab `NoInpatientEpisode` dan
   controller meneruskan jalur poliklinik/IGD seperti sebelumnya.

Contoh: episode Budi ditutup pukul 13.00. Dokter mengirim tindakan pukul 14.00 tanpa
`InpEpisodeId`, tetapi menggunakan `EncounterId` milik episode tersebut. Backend tetap menemukan
episode tertutup dan menjawab `422`; tidak ada tindakan maupun calon tagihan baru yang lahir.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `PatientProcedureController.cs`
- `InpatientClinicalContextService.cs`
- `api-contract.md` bagian Patient Procedure
- `validation-matrix.md` aturan `VAL-DOK-58`

### 3.2 Berkas implementasi

| Berkas | Perubahan |
| --- | --- |
| `Controllers/PatientProcedureController.cs` | Memeriksa episode berdasarkan encounter sebelum create; mengembalikan `422` dengan alasan; mengubah panggilan penulis tindakan menjadi `forNewDocument: true`; metadata Swagger mencantumkan `422` |
| `Services/InpatientClinicalContextService.cs` | Resolver existing membedakan kunjungan tanpa rawat inap dari episode yang tidak dapat menerima dokumen baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route dan payload tetap; `POST /` kini mengembalikan `422` untuk episode yang sudah tidak dapat menerima tindakan baru |
| Database | `NOT APPLICABLE` — penolakan terjadi sebelum penyimpanan; tidak ada migration |
| Keamanan/Auth | Metadata `PatientProcedure : Create` tidak berubah; perubahan adalah validasi state episode |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat tindakan dari catatan dokter hanya ketika episode masih dapat menerima dokumen baru | `PatientProcedure : Create` | `CreatePatientProcedureRequest` | `ApiResponse<PatientProcedureCreateResponse>` |

Kode `422` menjelaskan bahwa perawatan sudah ditutup dan tindakan baru tidak dapat dicatat. Kode
`403` tetap digunakan bila dokter tidak memiliki penugasan yang sah. Kiriman ulang idempoten yang
sudah pernah tersimpan mengembalikan tindakan lama dan tidak membuat tindakan baru.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker `Strict`, rentang `36db5e6d..a0a710db` | 17 berkas dievaluasi; nol finding | `PASS` | `Final result: PASS` |
| QBE checker `Strict`, working tree akhir | 4 berkas controller dievaluasi; nol finding | `PASS` | `Final result: PASS` |
| Episode `Closed` | Resolver menghasilkan `EpisodeClosed`; controller mengembalikan `422` sebelum save | `PASS` | Review `CreateProcedure` |
| Pesan penolakan | Menyebut perawatan sudah ditutup dan tindakan baru tidak dapat dicatat | `PASS` | Payload `ApiResponse<object>.Fail` |
| Request tanpa `InpEpisodeId` | Episode tetap diturunkan dari `EncounterId` | `PASS` | Panggilan `ResolveAsync(request.EncounterId, ...)` |
| Episode aktif | Guard tidak menghasilkan `EpisodeClosed`; alur create lama dilanjutkan | `PASS` | Review percabangan controller |
| Kunjungan poliklinik/IGD | `NoInpatientEpisode` tidak ditolak oleh cabang `EpisodeClosed` | `PASS` | Review outcome resolver |
| Episode `Cancelled` | Tetap ditolak sebagai dokumen baru sesuai `VAL-DOK-58` | `PASS` | Resolver memperlakukan episode non-open sebagai tertutup bagi dokumen baru |
| `git diff --check` sebelum dokumentasi | Tidak ada whitespace error | `PASS` | Keluaran kosong |
| `dotnet build` | Tidak dijalankan atas instruksi eksplisit pengguna | `NOT RUN` | Pengguna akan menjalankan build mandiri |
| Uji runtime API | Tidak dijalankan karena aplikasi tidak dibangun/dijalankan | `NOT RUN` | Verifikasi statis saja |

Uji manual: `NOT FEASIBLE` pada sesi ini karena membutuhkan runtime dan episode pada beberapa
status.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — tindakan baru pada episode `Closed` ditolak `422` | Terpenuhi secara statis | Closed guard berjalan sebelum validasi dan save |
| AC-2 — pesan penolakan menyebut alasannya | Terpenuhi secara statis | Pesan menyebut perawatan sudah ditutup dan tindakan baru tidak dapat dicatat |
| AC-3 — episode selain `Closed` tidak berubah perilakunya | Terpenuhi untuk jalur yang sebelumnya sah | Episode aktif dan kunjungan non-rawat-inap meneruskan alur lama; `Cancelled` tetap tidak sah menurut `VAL-DOK-58` |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini dan pembaruan register |
| `dotnet build` | Belum diverifikasi | `NOT RUN` atas instruksi pemilik; bukan klaim build berhasil |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bukti kompilasi dan perilaku runtime menunggu build mandiri pemilik |
| Masalah yang diketahui | Tidak ada masalah source baru yang ditemukan melalui review statis |
| Risiko tersisa | Integrasi runtime dengan tarif, coverage, dan billing belum di-smoke-test pada sesi ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git saat laporan ditulis | Source implementation berada pada commit `a0a710db`; working tree memuat metadata response `422`, laporan, dan pembaruan register task ini |
| Langkah berikutnya | Pemilik menjalankan build dan smoke test create pada episode aktif, `Closed`, serta kunjungan poliklinik |
