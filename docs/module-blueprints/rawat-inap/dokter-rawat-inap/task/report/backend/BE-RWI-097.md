# Laporan Perubahan Backend — `BE-RWI-097`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-097` |
| Judul | Pesanan tindakan dengan pemberi instruksi (migration R7) |
| Slice | Gelombang 2 — `DOK-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-097` |
| Trace | `FR-DOK-100`, `FR-DOK-101`, `FR-DOK-102`; `RWI-DEC-133`; `RWI-DEC-139`; `RWI-DEC-143`; `RWI-DEC-152` |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-090`, `BE-RWI-079` [BE-INP] |
| Klasifikasi | `HIGH` — schema migration R7, relaksasi `ConsultationId`, endpoint pesanan rawat inap, aturan penginput/DPJP, dan wiring Langkah 5 penutupan episode |
| Task mode | `BACKEND` |
| Target tulis | `HealthServices/ClinicalManagement`, `HealthServices/InPatientManagement`, `Migrations`, `Repositories/Configurations`, dan dokumentasi blueprint |
| Model | Google Antigravity |
| Tanggal | 2026-09-16 |
| Status | ✅ **SELESAI 16 September 2026** berdasarkan validasi statis source, kontrak, dan skema migration; `dotnet build` tidak dijalankan sesuai instruksi eksplisit pengguna |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / ClinicalManagement`, `HealthServices / InPatientManagement` |
| Registry/prefix | `ClinicalManagement` dan `InPatientManagement` terdaftar `ACTIVE`; tabel `TrxPatientProcedure` diekstensi dengan 6 kolom baru |
| Keputusan Tata Kelola | `RWI-DEC-152` — Sukma GP (pemilik blueprint `rawat-jalan`) menyetujui relaksasi `ConsultationId` menjadi nullable pada 16 September 2026, dengan penjaga kewajiban konsultasi dipindahkan ke tingkat alur/jalur |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001` |
| Penempatan aturan | Domain logika penginput dan instruksi ditempatkan di `PatientProcedureOrderService`; controller mengelola request handling dan permission |
| Database | Migration `20260916003000_AddProcedureInstructionColumns.cs` (R7); guard rollback asimetris terpasang |

## 1. Masalah yang diperbaiki

Di rawat inap, pesanan tindakan umumnya dimasukkan oleh perawat atas instruksi dokter yang bertugas merawat pasien. Struktur sebelumnya mewajibkan `ConsultationId` (`NOT NULL` pada database dan DTO), yang hanya tersedia pada alur konsultasi poliklinik rawat jalan. Akibatnya, perawat rawat inap tidak memiliki jalur untuk memesan tindakan secara sah.

Melalui amandemen `R7` dan persetujuan `RWI-DEC-152`, kewajiban `ConsultationId` dilonggarkan menjadi nullable khusus untuk jalur pesanan rawat inap, serta ditambahkan 6 kolom baru untuk merekam identitas penginput, dokter pemberi instruksi, status verifikasi instruksi dokter, dan penanda pembatalan otomatis saat penutupan episode rawat inap.

## 2. Proses bisnis

1. **Pemesanan Tindakan Rawat Inap (`FR-DOK-100`)**:
   - Perawat atau dokter login memesan tindakan rawat inap melalui `POST /inpatient-orders` tanpa membawa `ConsultationId`.
   - Jika pembuat pesanan adalah perawat, dokter pemberi instruksi (`InstructingDoctorId`) wajib dipilih dan diverifikasi sedang memiliki penugasan aktif pada episode pasien. Status verifikasi menjadi `Pending`.
   - Jika pembuat pesanan adalah dokter yang bertugas, pesanan langsung berstatus `NotRequired` untuk verifikasi instruksi.
   - Kolom `OrderedByUserId` merekam identitas pengguna yang memasukkan pesanan dari akun loginnya (`RWI-DEC-139`).
2. **Penjagaan Jalur Poliklinik Rawat Jalan (`FR-DOK-101`)**:
   - Jalur poliklinik `POST /` tetap mewajibkan `ConsultationId`. Bila dikirim kosong atau tanpa konsultasi yang sah, permintaan tetap ditolak dengan galat `400` dan pesan yang sama seperti sebelumnya (`ConsultationId wajib diisi.`).
3. **Penyuntingan dan Pembatalan Pesanan (`FR-DOK-102` / `INV-DOK-17`)**:
   - Pesanan tindakan yang belum dilaksanakan hanya boleh diubah oleh penginput aslinya (`OrderedByUserId == actorUserId`). Dokter pemberi instruksi sekalipun ditolak `403` jika mencoba menyunting isian penginput.
   - Pembatalan pesanan (`PATCH /{id}/cancel`) hanya dapat dilakukan oleh penginput asli atau DPJP aktif pada episode rawat inap. Alasan pembatalan (`CancelReason`) wajib diisi. Tindakan yang sudah masuk tagihan (`IsBillingGenerated`) ditolak dari modul klinis.
4. **Pembatalan Otomatis Saat Penutupan Episode (Langkah 5 / `RWI-DEC-143`)**:
   - Saat penutupan episode rawat inap di `InpDischargeService.CloseEpisodeInternalAsync`, sistem secara atomik membatalkan seluruh pesanan tindakan yang belum terlaksana (`Planned` atau `Ordered`) dan belum tertagih, dengan mencatat `CancelledByEpisodeClosure = true` dan `CancelReason = "Episode ditutup sebelum tindakan dilaksanakan."`.

Contoh konkret skenario rumah sakit:
Perawat Siti memesan tindakan Fisioterapi Dada untuk pasien Budi atas instruksi lisan dr. Hendra (Sp.P) yang sedang visite. Siti memilih dr. Hendra sebagai dokter pemberi instruksi. Pesanan tersimpan dengan status instruksi `Pending`, penginput Siti, dan tanpa `ConsultationId`. Di sisi lain, dr. Ratna di poli rawat jalan mengirim tindakan tanpa `ConsultationId`; sistem langsung menolaknya dengan pesan `ConsultationId wajib diisi.` tanpa mengubah alur poliklinik sedikit pun.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan ditambahkan

| Berkas | Jenis | Ringkasan Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/PatientProcedureInstructionVerificationStatus.cs` | Baru | Enum status verifikasi: `NotRequired` (0), `Pending` (1), `Verified` (2) |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` | Ubah | `ConsultationId` menjadi nullable (`Guid?`); penambahan 6 kolom instruksi/penutupan dan navigasi terkait |
| `Repositories/Configurations/HealthServices/TrxPatientProcedureConfiguration.cs` | Ubah | Konfigurasi EF Core untuk `ConsultationId` nullable, property baru, foreign keys (`Restrict`), dan index instruksi/penginput |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs` | Ubah | Penambahan `CreateInpatientProcedureOrderRequest`, pembaharuan response DTOs dengan `ConsultationId` nullable dan kolom baru |
| `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs` | Baru | Service transaksi pesanan tindakan rawat inap, aturan penginput (`INV-DOK-17`), pembatalan DPJP/penginput (`FR-DOK-102`), dan pembatalan otomatis penutupan episode (`CancelPendingOrdersForClosureAsync`) |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | Ubah | Penambahan endpoint `POST /inpatient-orders`; injeksi `PatientProcedureOrderService`; penjagaan penginput pada `PUT /{id}`; penjagaan DPJP/penginput pada `PATCH /{id}/cancel`; penyesuaian helper konsultasi dan `CanEditProcedure` |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Ubah | Injeksi `PatientProcedureOrderService` via Dependency Injection |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Ubah | Pemasangan Langkah 5 penutupan episode rawat inap: memanggil `CancelPendingOrdersForClosureAsync`, memperbarui `SideEffects.CancelledProcedureOrderCount`, dan mencabut penanda `LangkahLimaBelumTerpasang` |
| `Program.cs` | Ubah | Registrasi DI `builder.Services.AddScoped<PatientProcedureOrderService>();` |
| `Migrations/20260916003000_AddProcedureInstructionColumns.cs` | Baru | Migration `R7` EF Core untuk 6 kolom baru, relaksasi `ConsultationId`, foreign keys, index, serta guard rollback asimetris |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Ubah | Model snapshot diperbarui untuk `TrxPatientProcedure` (properti, index, foreign keys, dan navigasi baru) |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `POST /inpatient-orders` untuk pesanan rawat inap; endpoint existing `PUT /{id}` dan `PATCH /{id}/cancel` menegakkan aturan otorisasi penginput dan DPJP aktif |
| Database | Schema migration `R7` pada tabel `TrxPatientProcedure`; `ConsultationId` dilonggarkan menjadi nullable; 6 kolom baru ditambahkan |
| Keamanan & Otorisasi | Hak akses `PatientProcedure : Create` untuk membuat pesanan; `PatientProcedure : Update` untuk ubah/batal; validasi penginput (`INV-DOK-17`) dan penugasan DPJP aktif (`FR-DOK-102`) diverifikasi di backend |

## 4. Dokumentasi Endpoint

#### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request Body | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/inpatient-orders` | Membuat pesanan tindakan rawat inap oleh dokter atau perawat atas instruksi dokter bertugas | `PatientProcedure : Create` | `CreateInpatientProcedureOrderRequest` | `ApiResponse<PatientProcedureResponse>` (`201` baru / `200` idempoten) |
| `POST` | `/` | Jalur lama konsultasi dokter / poliklinik (wajib `ConsultationId`) | `PatientProcedure : Create` | `CreatePatientProcedureRequest` | `ApiResponse<PatientProcedureCreateResponse>` |
| `PUT` | `/{id}` | Mengubah pesanan tindakan yang belum dilaksanakan (hanya penginput asli) | `PatientProcedure : Update` | `UpdatePatientProcedureRequest` | `ApiResponse<PatientProcedureUpdateResponse>` |
| `PATCH` | `/{id}/cancel` | Membatalkan pesanan tindakan (hanya penginput asli atau DPJP aktif; wajib alasan) | `PatientProcedure : Update` | `CancelPatientProcedureRequest` | `ApiResponse<object>` |

Tabel Status Respon:
- `201 Created`: Pesanan tindakan rawat inap berhasil dibuat.
- `200 OK`: Pesanan berhasil diperbarui, dibatalkan, atau permintaan idempoten dengan kunci yang sama dikembalikan.
- `400 BadRequest`: Alasan pembatalan kosong, dokter instruksi tidak dipilih oleh perawat, atau konsultasi dokter kosong pada jalur poliklinik.
- `403 Forbidden`: Pengguna bukan penginput asli pesanan (saat update), atau bukan penginput maupun DPJP aktif (saat cancel), atau dokter instruksi tidak sedang bertugas pada episode tersebut.
- `404 NotFound`: Tindakan pasien atau episode rawat inap tidak ditemukan.
- `409 Conflict`: Tindakan sudah dibatalkan sebelumnya.
- `422 UnprocessableEntity`: Episode rawat inap sudah ditutup (`Closed` / `Cancelled`).

## 5. Verifikasi

| Skenario atau Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| AC-1 — Enam kolom baru pada `TrxPatientProcedure` | Sesuai kamus data `0.6.0`: `OrderedByUserId`, `InstructingDoctorId`, `InstructionVerificationStatus`, `InstructionVerifiedAt`, `InstructionVerifiedByUserId`, `CancelledByEpisodeClosure` | `PASS` | Model, Entity Configuration, Snapshot, dan Migration R7 |
| AC-2 — `ConsultationId` nullable dengan penjaga per jalur | `ConsultationId` dibuat nullable pada DB/Model; jalur rawat jalan tetap memvalidasi `ConsultationId` wajib | `PASS` | `TrxPatientProcedure.cs` & `PatientProcedureController.ValidateCreateRequestAsync` |
| AC-3 — Perawat memesan tindakan atas instruksi dokter bertugas | Perawat wajib memilih dokter instruksi bertugas; dokter login langsung NotRequired; penginput diambil dari login | `PASS` | `PatientProcedureOrderService.CreateInpatientOrderAsync` |
| AC-4 — Pesanan poliklinik tanpa `ConsultationId` ditolak | `ValidateCreateRequestAsync` memeriksa `if (request.ConsultationId == Guid.Empty) return (false, "ConsultationId wajib diisi.");` | `PASS` | Jalur `POST /` controller rawat jalan |
| AC-5 — Aturan penyuntingan penginput dan pembatalan DPJP | Update dibatasi penginput (`INV-DOK-17`); cancel dibatasi penginput atau DPJP aktif dengan alasan wajib | `PASS` | `PatientProcedureController.UpdateProcedure` & `CancelProcedure` |
| AC-6 — Regresi poliklinik tidak berubah hasilnya | Jalur konsultasi poliklinik tetap menggunakan alur dan validasi semula | `PASS` | Review integritas endpoint `POST /` |
| Wiring Langkah 5 penutupan episode | `InpDischargeService.Closure.cs` memanggil `CancelPendingOrdersForClosureAsync` dan mengisi `CancelledProcedureOrderCount` | `PASS` | `InpDischargeService.Closure.cs` lines 1045-1120 |
| Pengaman rollback migration asimetris | Script Down menolak rollback jika terdapat baris dengan `ConsultationId IS NULL` | `PASS` | `20260916003000_AddProcedureInstructionColumns.cs` Down SQL block |
| `dotnet build` | Tidak dijalankan atas instruksi eksplisit pengguna | `NOT RUN` | Sesuai mandat pengerjaan tanpa `dotnet build` |
| Eksekusi migration ke database | Tidak dijalankan pada sesi ini | `NOT RUN` | File migration R7 telah disiapkan untuk diaplikasikan oleh pengguna |

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Catatan Bukti |
| --- | --- | --- |
| AC-1 s.d. AC-6 | Terpenuhi secara statis | Enam kolom baru, relaksasi `ConsultationId`, alur rawat inap, penjagaan poliklinik, hak akses penginput/DPJP, dan regresi poliklinik telah diimplementasikan |
| Persetujuan tata kelola rawat jalan | Terpenuhi | Merujuk pada keputusan `RWI-DEC-152` oleh Sukma GP (16 September 2026) |
| Laporan tracked, roadmap, dan traceability | Terpenuhi | Laporan ini dibuat; `backend-roadmap-v2.md` dan `requirement-traceability-v2.md` diperbarui |
| `dotnet build` | Belum diverifikasi runtime | `NOT RUN` sesuai instruksi khusus pengguna; kode divalidasi secara statis |

## 7. Catatan Penutup

1. **Integritas Penutupan Episode**: Dengan terpasangnya `PatientProcedureOrderService.CancelPendingOrdersForClosureAsync` pada `InpDischargeService.Closure.cs`, Langkah 5 penutupan episode rawat inap kini aktif dan terhubung secara penuh.
2. **Kesiapan Task Berikutnya**: Task `BE-RWI-098` (verifikasi instruksi oleh dokter) dan `BE-RWI-104` (instruksi pada pesanan Lab/Rad) kini dapat dilanjutkan karena fondasi `R7` dan service transaksi `PatientProcedureOrderService` telah selesai dibangun.
