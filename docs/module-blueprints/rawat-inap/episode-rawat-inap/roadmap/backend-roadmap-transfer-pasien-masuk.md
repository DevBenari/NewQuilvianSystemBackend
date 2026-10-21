# Roadmap Backend Transfer Pasien Masuk (IGD/Rajal ke Ranap) & Penugasan PPJA

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` (COMPOSITE; kepemilikan data pada [module map 10.1](../../02-module-map.md)) |
| planning_id / revision / tanggal | `TRF-DEL-20261016-01` / 1 / 2026-10-16 Asia/Jakarta |
| status / approval | Desain disetujui pengguna 16 Oktober 2026 ("iya lakukan /plan-module-delivery"). Kontrak target `0.14.0`. Setiap task dieksekusi hanya lewat `build-module-backend` dengan task ID-nya |
| blueprint / kontrak | [Backend 16](../02-backend-architecture.md), [kamus data 23](../data/data-dictionary.md), [Kontrak Transfer](../contracts/admission-transfer-patient-handover-contract.md), [PRD 28](../04-prd-to-mvp.md), [Flowchart 19](../flowcharts/19-alur-transfer-pasien-masuk-igd-rajal-ke-ranap.md) |
| input revisions | Decision log 51; gate 1.13; module map 7; manifest anak 13 draft; kontrak target `0.14.0` |
| source SHA backend / frontend | `ca09cae909597442385ce4513be9c803549be10e` / `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` |
| hash lock | [transfer-pasien-masuk-delivery-lock.json](transfer-pasien-masuk-delivery-lock.json) |
| scope / ID | 3 task `BE-RWI-241` s.d. `243`. Ruang ID dipindai global 2026-10-16: terpakai sampai `BE-RWI-240`; berikutnya `BE-RWI-244` |
| roadmap pasangan | [Frontend](frontend-roadmap-transfer-pasien-masuk.md) · [Traceability](requirement-traceability-transfer-pasien-masuk.md) |

---

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
BE-RWI-241

BE-RWI-242

BE-RWI-243
```

Garis selalu prasyarat → task. Ketiga task backend ini independen secara modul kode (`InpAdmissionTransferService`, `CliTransferHandoverService`, dan `InpatientEpisodeDetailService`) dan dapat dikerjakan secara paralel pada Gelombang 1.

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
| ---: | --- | --- | --- |
| 1 | — | `BE-RWI-241` | Otomasi trigger serah terima klinis transfer pada admisi |
| 1 | — | `BE-RWI-242` | Ekstensi endpoint accept handover dengan PPJA & rekonsiliasi |
| 1 | — | `BE-RWI-243` | Ringkasan kelengkapan dokumen PPRI pada detail episode |

---

## Register Task

| Task ID / Status | Outcome | Requirement / Decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-241` | Otomasi pemicu pembuatan serah terima klinis saat admisi transfer disubmit | FR-TRF-001 | 0.14.0 draft; lock TRF-DEL-20261016-01 | `InpAdmissionTransferService`, `CliTransferHandoverService` | Service admisi transfer memanggil pembuatan dokumen serah terima `PendingSend` beserta snapshot TTV & obat | — | TRF-AC-001, TRF-AC-002 | V-BE + UAT-TRF-01 | InPatientManagement + ClinicalManagement; transaksi multi-service | D-BE |
| `BE-RWI-242` | Ekstensi penerimaan serah terima klinis dengan validasi wajib PPJA dan pencegahan akun sama | FR-TRF-003, FR-TRF-004, FR-TRF-005 | 0.14.0 draft | `CliTransferHandoverService`, `TransferHandoverController` | Update DTO `AcceptTransferHandoverRequest`, validasi PPJA wajib, penugasan perawat bangsal, rekonsiliasi obat | — | TRF-AC-003, TRF-AC-004, TRF-AC-005 | V-BE + UAT-TRF-02 | ClinicalManagement; integritas penugasan perawat bangsal | D-BE |
| `BE-RWI-243` | Endpoint detail episode menyajikan ringkasan kelengkapan berkas admisi PPRI | FR-TRF-007 | 0.14.0 draft | `InpatientEpisodeDetailService`, `InpAdmissionDocumentService` | Penambahan agregat `AdmissionDocumentsSummary` pada `InpEpisodeDetailDto` | — | TRF-AC-006, TRF-AC-007 | V-BE + UAT-TRF-03 | InPatientManagement; performa pembacaan census/detail | D-BE |

---

## Batas Handoff dan Definition of Done

### D-BE — Bukti Wajib Setiap Task Backend

QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend dan dokumen engineering canonical: owner/prefix `Inp` dan `Cli`, wewenang tulis per task, dan hash masukan pada lock.

V-BE adalah bundel bukti menurut `rules/backend/TEST_POLICY.md`: review diff/scope, `dotnet build` project aplikasi (`dotnet restore` bila project/dependency berubah), verifikasi kontrak API (method, path, permission, request/response, kode status), dan verifikasi proses bisnis untuk jalur normal dan tidak normal. Dokumen ini **tidak menuntut unit test otomatis**.

Laporan tracked disimpan di `task/report/backend/<TASK-ID>.md`.

---

## Kartu Task

### BE-RWI-241 — Otomasi Pemicu Pembuatan Serah Terima Klinis saat Admisi Transfer

| Field | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Outcome | Pendaftaran admisi transfer IGD/Rajal otomatis menerbitkan dokumen serah terima klinis berstatus `PendingSend` lengkap dengan snapshot TTV dan obat dari unit asal |
| Ukuran / Batas | Satu ekstensi method pada `InpAdmissionTransferService.ExecuteTransferAdmissionAsync` |
| Jejak | FR-TRF-001; TRF-AC-001, TRF-AC-002 |
| Desain / Kontrak | Backend 16.1; Kontrak Spesifikasi Transfer Bagian 2.1 |
| Reuse / Disposisi | `InpAdmissionTransferService` (Extend), `CliTransferHandoverService` (Reuse) |
| Cakupan | Pada `ExecuteTransferAdmissionAsync`, sesudah episode dibuat dan bed dialokasikan, panggil `CliTransferHandoverService.CreateForTransferAsync` dengan status `PendingSend`. Salin snapshot TTV terakhir, alat terpasang, dan daftar obat aktif dari kunjungan asal. Idempotensi: jika handover untuk episode sudah ada, jangan buat duplikat |
| Batas Source | `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs`; `Areas/HealthServices/ClinicalManagement/Services/CliTransferHandoverService.cs` |
| Dependency | — |
| Verifikasi yang Wajib | V-BE (QBE preflight saat eksekusi) + verifikasi proses bisnis |
| Definition of Done | D-BE; seluruh AC terpetakan ke source; laporan `task/report/backend/BE-RWI-241.md` |

**Acceptance Kriteria Positif**:
- `POST /admission-transfers/admit` berhasil menghasilkan episode dan 1 record `CliTransferHandover` baru dengan status `PendingSend`.
- Snapshot handover memuat data TTV, alat, dan riwayat obat kunjungan asal.
- Replay request dengan `Idempotency-Key` sama mengembalikan response yang sama tanpa menduplikasi handover.

**Acceptance Kriteria Negatif**:
- Jika pencatatan serah terima klinis gagal karena data unit asal tidak lengkap, admisi tetap berhasil dengan warning log (tidak menggagalkan penerbitan episode rawat inap).

---

### BE-RWI-242 — Ekstensi Penerimaan Serah Terima Klinis dengan Validasi Wajib PPJA dan Pencegahan Akun Sama

| Field | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Outcome | Endpoint penerimaan serah terima klinis (`PATCH /transfer-handovers/{id}/accept`) mewajibkan penetapan PPJA, menolak akun penerima yang sama dengan pengirim, dan mencatat rekonsiliasi obat |
| Ukuran / Batas | Satu pembaruan DTO + satu penambahan validasi dan penyimpanan penugasan perawat di `CliTransferHandoverService` |
| Jejak | FR-TRF-003, FR-TRF-004, FR-TRF-005; TRF-AC-003, TRF-AC-004, TRF-AC-005 |
| Desain / Kontrak | Backend 16.1; Kontrak Spesifikasi Transfer Bagian 2.1; Kamus Data 23.1, 23.2 |
| Reuse / Disposisi | `CliTransferHandoverService` (Extend), `TransferHandoverController` (Extend) |
| Cakupan | Tambah `AssignedNurseId` dan `MedicationReconciliations` pada `AcceptTransferHandoverRequest`. Validasi `CLI-TRH-001` (PPJA tidak boleh Guid.Empty) dan `CLI-TRH-002` (currentUserId != CreatedByUserId). Simpan penugasan PPJA ke data episode. Simpan rekonsiliasi obat ke riwayat |
| Batas Source | `Areas/HealthServices/ClinicalManagement/DTOs/TransferHandoverDtos.cs`; `Areas/HealthServices/ClinicalManagement/Services/CliTransferHandoverService.cs`; `Areas/HealthServices/ClinicalManagement/Controllers/TransferHandoverController.cs` |
| Dependency | — |
| Verifikasi yang Wajib | V-BE + verifikasi kontrak endpoint dan error handling |
| Definition of Done | D-BE; seluruh AC terpetakan ke source; laporan `task/report/backend/BE-RWI-242.md` |

**Acceptance Kriteria Positif**:
- Menerima serah terima dengan `assignedNurseId` valid dan akun berbeda mengubah status handover menjadi `Accepted` dan menyimpan penugasan perawat PPJA ke episode.
- Catatan rekonsiliasi obat tersimpan rapi pada snapshot serah terima.

**Acceptance Kriteria Negatif**:
- Menerima tanpa `assignedNurseId` ditolak `422 Unprocessable Entity` (`CLI-TRH-001: "Penetapan Perawat Penanggung Jawab Asuhan (PPJA) wajib diisi saat menerima pasien transfer."`).
- Menerima menggunakan akun login yang sama dengan perawat pengirim ditolak `400 Bad Request` (`CLI-TRH-002: "Penerima serah terima klinis tidak boleh akun yang sama dengan perawat pengirim."`).
- Menerima dokumen yang statusnya bukan `Sent` ditolak `409 Conflict`.

---

### BE-RWI-243 — Endpoint Detail Episode Menyajikan Ringkasan Kelengkapan Berkas Admisi PPRI

| Field | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Outcome | Kueri detail episode rawat inap menyajikan ringkasan kelengkapan berkas admisi Workspace PPRI secara non-blocking |
| Ukuran / Batas | Satu penambahan properti DTO dan agregat penghitungan di `InpatientEpisodeDetailService` |
| Jejak | FR-TRF-007; TRF-AC-006, TRF-AC-007 |
| Desain / Kontrak | Backend 16.1; Kontrak Spesifikasi Transfer Bagian 2.2; Kamus Data 23.3 |
| Reuse / Disposisi | `InpatientEpisodeDetailService` (Extend), `InpAdmissionDocumentService` (Reuse) |
| Cakupan | Tambah `AdmissionDocumentsSummary` pada `InpEpisodeDetailDto`. Hitung jumlah dokumen admisi wajib (General Consent, Serah Terima, Selisih Biaya, Deposit), jumlah `Completed`, dan daftar pending dokumen. Tidak memblokir kueri jika dokumen belum lengkap |
| Batas Source | `Areas/HealthServices/InPatientManagement/DTOs/InpEpisodeDtos.cs`; `Areas/HealthServices/InPatientManagement/Services/InpatientEpisodeDetailService.cs` |
| Dependency | — |
| Verifikasi yang Wajib | V-BE + verifikasi response payload |
| Definition of Done | D-BE; seluruh AC terpetakan ke source; laporan `task/report/backend/BE-RWI-243.md` |

**Acceptance Kriteria Positif**:
- `GET /episodes/{id}` mengembalikan objek `admissionDocumentsSummary` yang akurat mencerminkan status dokumen admisi saat ini.
- `isAllCompleted` bernilai `true` jika semua berkas sudah `Completed`.

**Acceptance Kriteria Negatif**:
- Jika ada berkas yang belum `Completed`, `isAllCompleted` bernilai `false` dan daftar `pendingDocumentNames` memuat nama berkas yang belum selesai, tanpa menghalangi pembacaan data episode.
