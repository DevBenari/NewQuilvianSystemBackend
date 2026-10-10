# Laporan Perubahan Backend — `BE-RWI-207`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-207` |
| Judul | Eksekusi Disposisi Transfer IGD & Pembukaan Episode Rawat Inap Terpadu |
| Slice | Slice Transfer IGD / RJ; Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/backend-roadmap-admisi-transfer-igd.md`](../../../roadmap/backend-roadmap-admisi-transfer-igd.md) — kartu `BE-RWI-207` |
| Trace | `FR-RI-188`, `FR-RI-192`; `RWI-DEC-275`; `RWI-AC-398`, `RWI-AC-399`, `RWI-AC-405`; API Contract `0.12.0` (11.7.3) |
| Contract version | `0.12.0` **`approved`** (10 Oktober 2026) |
| Dependency | `BE-RWI-206` |
| Klasifikasi | `HIGH` — Transaksi atomik pembukaan episode rawat inap, alokasi bed, dan transisi status disposisi IGD |
| Task mode | `BACKEND` |
| Target tulis | `InpatientAdmissionTransferDtos.cs`, `InpAdmissionTransferService.cs`, `InpatientAdmissionTransferController.cs` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-398` dan `RWI-AC-399` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Sesuai kebutuhan Analisis Bisnis (Tahap C dan D alur admisi transfer IGD):
1. **Penerbitan Episode Rawat Inap (`RWI-DEC-275`, `RWI-AC-398`):** Pembukaan pendaftaran dari pasien transfer IGD harus menerbitkan episode rawat inap resmi (`InpatientEpisode`) dengan pengikatan penjamin biaya, dokter DPJP, dan alokasi tempat tidur.
2. **Sinkronisasi Atomik Status IGD (`RWI-DEC-275`, `RWI-AC-399`):** Saat episode rawat inap diterbitkan, status `EmgDisposition` pada kunjungan IGD asal wajib diperbarui secara atomik menjadi `Executed` (`ExecutedAt = UtcNow`, `ConfirmedByUserId = currentUserId`). Hal ini memastikan sistem IGD mencatat bahwa pasien telah dialokasikan ke bangsal rawat inap dan siap untuk proses serah terima (*Transfer Handover*).
3. **Pencegahan Registrasi Ganda:** Mencegah satu disposisi IGD yang sama dieksekusi lebih dari satu kali (penolakan dengan pesan bisnis jika status sudah `Executed`).

---

## 2. Rincian Perubahan Source Code

### 2.1 `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs`
- Mendefinisikan model request `OpenAdmissionFromTransferRequest` yang memuat `TransferDispositionId`, `SourceType`, `PatientId`, `ServiceUnitId`, `PatientClassId`, `DoctorId`, `BedId`, `Notes`, `PaymentScheme`, dan `DepositAmount`.
- Mendefinisikan model response `OpenAdmissionFromTransferResponse`.

### 2.2 `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs`
- Mengimplementasikan method `ExecuteTransferAdmissionAsync`:
  - Memverifikasi keberadaan dan keabsahan disposisi IGD (status wajib `Confirmed`, `ExecutedAt` belum terisi, dan pasien cocok).
  - Memanggil `InpEpisodeService.OpenAdmissionAsync` untuk membuat episode rawat inap dan encounter terkait.
  - Memanggil `InpBedOccupancyService.ReserveBedAsync` untuk mengalokasikan dan mengunci tempat tidur yang dipilih.
  - Memperbarui status disposisi IGD menjadi `Executed` dengan mencatat waktu eksekusi (`ExecutedAt = UtcNow`) dan user pelaksana.
  - Menulis catatan audit log terstruktur `InpAdmissionTransfer.Executed`.

### 2.3 `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionTransferController.cs`
- Menambahkan endpoint `POST /api/v1/health-services/inpatient-management/admission-transfers/admit`:
  - Tag: `[Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]`.
  - Hak akses: `InpatientEpisode : Create`.
  - Mengembalikan `201 Created` beserta data episode dan alokasi tempat tidur saat berhasil.
  - Mengembalikan `409 Conflict` jika rujukan transfer sudah pernah dieksekusi sebelumnya.
  - Mengembalikan `422 UnprocessableEntity` jika ada pelanggaran aturan bisnis perbankan atau medis.

---

## 3. Bukti Verifikasi & Traceability
- Transaksi simpan episode rawat inap dan pengubahan status `EmgDisposition` menjadi `Executed` berjalan dalam satu unit kerja.
- Jika alokasi bed gagal, pembuatan episode dibatalkan secara bersih (*cancellation safety*).
- Disposisi IGD yang sudah `Executed` secara otomatis hilang dari daftar antrean pasien menunggu di admisi.
