# Laporan Perubahan Backend — `BE-RWI-088`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-088` |
| Judul | Penjaga penulis klinis: penulis tunggal dan penugasan pada waktu klinis |
| Slice | Gelombang 1 — `DOK-V2-0` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-088` |
| Trace | `FR-DOK-075`, `FR-DOK-076`; `INV-DOK-14`, `INV-DOK-15`; `VAL-DOK-41`, `VAL-DOK-42`, `VAL-DOK-58` |
| Contract version | `0.6.0` |
| Dependency | Tidak ada dependency task untuk `BE-RWI-088` |
| Klasifikasi | `HEAVY` — invarian penulis dan kewenangan klinis melintasi lima grup dokumentasi |
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
| Registry/prefix | `ClinicalManagement` terdaftar `ACTIVE`; task tidak membuat entity atau prefix baru |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-LOG-001` |
| Penempatan aturan | Keputusan penulis dan penugasan berada pada `InpatientClinicalContextService`; controller hanya meneruskan konteks dan jawaban HTTP |
| Database | `NOT APPLICABLE` — nol schema, entity, configuration, atau migration |

## 1. Masalah yang diperbaiki

Konsep klinis sebelumnya dapat diubah atau diselesaikan oleh pengguna selain penulisnya. Selain
itu, pemeriksaan penugasan pada waktu klinis saja belum cukup: dokter yang penugasannya sudah
berakhir masih dapat mengirim dokumen baru bertanggal ketika ia dahulu bertugas.

Contoh: dr. Yoga bertugas sampai pukul 07.00. Ia mengirim catatan baru pukul 08.10 dengan waktu
klinis 05.00. Waktu klinis memang masuk masa tugas, tetapi dokter tidak lagi bertugas saat
menyimpan. Sistem sekarang menolak dengan `403`. Bila kepala ruangan memberi penugasan singkat
pukul 08.30–09.30, pengiriman ulang pukul 08.40 dapat diterima.

## 2. Proses bisnis

1. Saat dokumen baru dikirim, backend mengambil identitas dokter dari akun login.
2. Backend memastikan penugasan berlaku pada waktu klinis dokumen.
3. Untuk dokumen baru, backend memeriksa lagi bahwa dokter masih bertugas pada saat penyimpanan.
4. Saat konsep diubah, diselesaikan, atau dibatalkan, backend membandingkan pengguna login dengan
   penulis yang tersimpan pada dokumen.
5. Bila berbeda, permintaan dihentikan dengan `403`, termasuk bila pelakunya DPJP aktif.
6. Kunjungan tanpa episode rawat inap tetap diteruskan ke jalur poliklinik/IGD lama, sehingga
   perilakunya tidak berubah.

Konsep milik penulis sendiri tidak diwajibkan mempunyai penugasan yang masih aktif ketika
diselesaikan. Pengecualian ini disengaja: konsep tersebut sudah dibuat ketika kewenangannya sah.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `InpatientClinicalContextService.cs`
- `DoctorConsultationController.cs`
- `PatientAssessmentController.cs`
- `PatientIntegratedProgressNoteController.cs`
- `PatientDiagnosisController.cs`
- `PatientProcedureController.cs`
- kontrak permission, validation, API, dan state transition `0.6.0`

### 3.2 Berkas implementasi

| Berkas | Perubahan |
| --- | --- |
| `Services/InpatientClinicalContextService.cs` | `ResolveForDoctorWriteAsync` memeriksa penugasan pada waktu klinis dan, untuk dokumen baru, pada saat simpan; `ResolveForAuthorEditAsync` membatasi mutasi pada penulis |
| `Controllers/DoctorConsultationController.cs` | Penjaga penulis pada ubah, simpan SOAP, selesai, dan batal; metadata Swagger mencantumkan `403` |
| `Controllers/PatientAssessmentController.cs` | Penjaga penulis pada ubah, selesai, dan batal; metadata Swagger mencantumkan `403` |
| `Controllers/PatientIntegratedProgressNoteController.cs` | Penjaga penulis pada ubah dan batal tanpa mengubah jalur verifikasi DPJP; metadata Swagger mencantumkan `403` |
| `Controllers/PatientProcedureController.cs` | Dokumen tindakan baru memakai `forNewDocument: true` dan waktu tindakan sebagai waktu klinis |

### 3.3 Jumlah titik panggil

Impact scan roadmap mencatat **9** titik resolver: **1** titik visite sudah meneruskan dokter
pelaku dengan benar, sedangkan **8** titik lain telah diarahkan ke penjaga bersama. Pada source
saat ini terdapat **6** panggilan langsung `ResolveForDoctorWriteAsync` dalam **5** grup dokumen;
perubahan aturan pada service berlaku seragam pada semuanya.

Untuk invarian penulis tunggal, terdapat **9 action mutasi** yang dijaga: 4 pada konsultasi dokter,
3 pada kajian pasien, dan 2 pada CPPT. Dengan demikian simpan otomatis atau pembatalan tidak dapat
dipakai untuk melewati penjaga endpoint ubah/selesai.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route dan payload tetap; jalur rawat inap dapat menjawab `403` ketika pelaku bukan penulis atau penugasan tidak sah |
| Database | `NOT APPLICABLE` — tidak ada perubahan bentuk data |
| Keamanan/Auth | Mengetatkan pemeriksaan hubungan pengguna–dokumen–episode; metadata hak akses tidak berubah |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Doctor Consultation

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah konsep milik penulis | `DoctorConsultation : Update` | `UpdateDoctorConsultationRequest` | `ApiResponse<object>` |
| `PATCH` | `/{id}/soap` | Menyimpan SOAP milik penulis | `DoctorConsultation : Update` | `UpdateDoctorConsultationSoapRequest` | `ApiResponse<object>` |
| `PATCH` | `/{id}/complete` | Menyelesaikan konsep milik penulis | `DoctorConsultation : Update` | `FinalizeDoctorConsultationRequest` | `ApiResponse<ConsultationFinalizationResponse>` |
| `PATCH` | `/{id}/cancel` | Membatalkan konsep milik penulis | `DoctorConsultation : Update` | `CancelDoctorConsultationRequest` | `ApiResponse<object>` |

#### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah kajian milik penulis | `PatientAssessment : Update` | `UpdatePatientAssessmentRequest` | `ApiResponse<object>` |
| `PATCH` | `/{id}/complete` | Menyelesaikan kajian milik penulis | `PatientAssessment : Update` | `CompletePatientAssessmentRequest` | `ApiResponse<PatientAssessmentCompleteResponse>` |
| `PATCH` | `/{id}/cancel` | Membatalkan kajian milik penulis | `PatientAssessment : Update` | `CancelPatientAssessmentRequest` | `ApiResponse<object>` |

#### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah CPPT milik penulis | `PatientIntegratedProgressNote : Update` | `UpdatePatientIntegratedProgressNoteRequest` | `ApiResponse<PatientIntegratedProgressNoteUpdateResponse>` |
| `PATCH` | `/{id}/cancel` | Membatalkan CPPT milik penulis | `PatientIntegratedProgressNote : Update` | `CancelPatientIntegratedProgressNoteRequest` | `ApiResponse<object>` |

Kode `403` berarti pengguna bukan penulis atau tidak memiliki penugasan yang sah. Kode `422`
tetap dipakai ketika episode tidak dapat menerima dokumen baru.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker `Strict`, rentang `36db5e6d..a0a710db` | 17 berkas dievaluasi; nol finding | `PASS` | `Final result: PASS` |
| QBE checker `Strict`, working tree akhir | 4 berkas controller dievaluasi; nol finding | `PASS` | `Final result: PASS` |
| Pencarian panggilan resolver dokter | 6 panggilan pada 5 grup dokumen | `PASS` | `rg ResolveForDoctorWriteAsync` |
| Pencarian penjaga penulis | 9 action mutasi memiliki penanda `BE-RWI-088 titik panggil` | `PASS` | `rg "BE-RWI-088 titik panggil"` |
| Pemeriksaan dua waktu penugasan | Waktu klinis diperiksa oleh `ResolveAsync`; waktu simpan diperiksa ulang untuk dokumen baru | `PASS` | Review source service |
| Pemeriksaan batas poliklinik | `NoInpatientEpisode` bukan penolakan pada penjaga penulis | `PASS` | `PerluDitolak` dan helper controller |
| `git diff --check` sebelum dokumentasi | Tidak ada whitespace error | `PASS` | Keluaran kosong |
| `dotnet build` | Tidak dijalankan atas instruksi eksplisit pengguna | `NOT RUN` | Pengguna akan menjalankan build mandiri |
| Uji runtime API | Tidak dijalankan karena aplikasi tidak dibangun/dijalankan | `NOT RUN` | Verifikasi statis saja |

Uji manual: `NOT FEASIBLE` pada sesi ini karena membutuhkan runtime dan akun dokter dengan
penugasan berbeda.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — hanya penulis menyunting dan menyelesaikan konsep; DPJP lain `403` | Terpenuhi secara statis | Perbandingan author user pada 9 action mutasi |
| AC-2 — dokumen baru membutuhkan penugasan pada waktu klinis dan saat simpan | Terpenuhi secara statis | Pemeriksaan dua waktu pada `ResolveForDoctorWriteAsync` |
| AC-3 — berlaku pada seluruh titik resolver rawat inap | Terpenuhi secara statis | 9 titik impact scan diaudit; 8 jalur yang dahulu tidak meneruskan actor memakai penjaga bersama, 1 visite sudah benar |
| AC-4 — jalur poliklinik tidak berubah | Terpenuhi secara statis | `NoInpatientEpisode` diteruskan ke perilaku lama |
| AC-5 — dokter berpenugasan normal tetap dapat menulis/menyelesaikan miliknya | Terpenuhi secara statis | Jalur sukses mengembalikan konteks bila assignment dan author cocok |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini dan pembaruan register |
| `dotnet build` | Belum diverifikasi | `NOT RUN` atas instruksi pemilik; bukan klaim build berhasil |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bukti kompilasi dan perilaku runtime menunggu build mandiri pemilik |
| Masalah yang diketahui | Tidak ada masalah source baru yang ditemukan melalui review statis |
| Risiko tersisa | Kesalahan tipe/kompilasi hanya dapat ditutup setelah build dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git saat laporan ditulis | Source implementation berada pada commit `a0a710db`; working tree memuat metadata response API, laporan, dan pembaruan register task ini |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` dan smoke test dengan dua dokter berbeda |
