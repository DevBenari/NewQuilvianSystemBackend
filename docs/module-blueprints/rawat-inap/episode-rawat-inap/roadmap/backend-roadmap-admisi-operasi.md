# Roadmap Backend Admisi Pasien Operasi & Kasus Bedah Obgyn

| Metadata | Nilai |
| :--- | :--- |
| **blueprint_id / owner** | `RWI-BP-001` / `episode-rawat-inap` |
| **planning_id / revision / tanggal** | `ADM-OPR-DEL-20261023-01` / 1 / 2026-10-23 Asia/Jakarta |
| **status / approval** | `APPROVED_DESIGN_PLANNED`; disetujui lewat `RWI-DEC-355` s.d. `RWI-DEC-358` dan `OPS-DEC-038` s.d. `OPS-DEC-042`. Eksekusi memakai `build-module-backend`, tepat satu task per pemanggilan |
| **kontrak terkunci** | `0.19.0`; [Dokumen Desain Terpadu](../../../operations/05-desain-amandemen-operasi-dan-admisi.md), [Dokumen Rujukan Admisi](../06-desain-amandemen-admisi-operasi-obgyn.md) |
| **scope / ID** | 3 task backend: `BE-RWI-182`, `BE-RWI-183`, dan `BE-RWI-184` |
| **roadmap pasangan** | [Frontend](frontend-roadmap-admisi-operasi.md) |

---

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
| :---: | :--- |
| ✅ | Selesai dan seluruh acceptance/DoD terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat task belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | `NOT_STARTED` |

---

## Grafik Urutan Dependency

```text
[BE-OPR] BE-OPR-013 ─> BE-RWI-182 ─> BE-RWI-183 ─┬─> BE-RWI-184
                                                 │
[BE-OPR] BE-OPR-014 ─────────────────────────────┘
```

> **Legenda:**  
> `[BE-OPR]` = Cermin baca-saja dari task upstream Modul Operasi (`operations/roadmap/backend-roadmap.md`).

---

## Tabel Gelombang Eksekusi

| Gelombang | Boleh mulai setelah | Task ID | Hasil Gelombang |
| ---: | :--- | :--- | :--- |
| 1 | Kontrak `BE-OPR-013` (Lookup IBS) siap | `BE-RWI-182` | Endpoint lookup jadwal IBS untuk admisi & perluasan payload pembukaan episode |
| 2 | `BE-RWI-182` selesai | `BE-RWI-183` | Penahanan bed bangsal selama operasi (`BedHoldStatus`), penugasan tri-dokter, dan skema paket SC |
| 3 | `BE-RWI-183` selesai & event `BE-OPR-014` siap | `BE-RWI-184` | Consumer event kelahiran bayi intra-op SC & pembuatan episode neonatus otomatis |

---

## Register Task Backend

| Task ID | Outcome | Trace | Kontrak | Cakupan & Reuse | Dependency | Acceptance Criteria & Verifikasi | DoD |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `BE-RWI-182` | Handshake jadwal operasi IBS pada Langkah 1 Admisi Ranap | `RWI-DEC-356`, `OPS-DEC-040` | `0.19.0` | `InpatientAdmissionController`, `InpatientEpisodeService`, DTO `OpenAdmissionRequest` | `[BE-OPR] BE-OPR-013` | 1. Endpoint `GET /inpatient-admissions/surgical-booking-lookup` membaca jadwal IBS secara read-only.<br/>2. Payload `OpenAdmissionRequest` menerima `SurgicalBookingNumber` dan `ClinicalSubCategory`.<br/>3. QBE conformance check lulus. | Endpoint + DTO + Conformance Check |
| `BE-RWI-183` | Penahanan tempat tidur (*Bed Hold*), tri-dokter DPJP, dan paket melahirkan | `RWI-DEC-355`, `RWI-DEC-357`, `RWI-DEC-358` | `0.19.0` | `InpEpisode.cs`, `InpBedOccupancyService`, `InpatientDoctorAssignment` | `BE-RWI-182` | 1. Pasien operasi menahan ranjang bangsal dengan status `Occupied / InSurgery`.<br/>2. Pasien kembali ke bangsal lewat transfer internal tanpa admisi ulang.<br/>3. Field `IsRoomingInBabyPlanned` dan `MaternityPackageId` tersimpan di database.<br/>4. QBE conformance check lulus. | Model update + Service logic + Conformance Check |
| `BE-RWI-184` | Consumer event kelahiran bayi & pembuatan episode neonatus otomatis | `OPS-DEC-042`, `DSN-OPR-RWI-20261023` | `0.19.0`, `OPR-INT-008` | `NewbornEpisodeService`, `MotherBabyLinkage` | `BE-RWI-183`, `[BE-OPR] BE-OPR-014` | 1. Consumer menerima event `operating-room.birth-event.recorded` secara idempotent.<br/>2. Auto-generate No. RM sementara bayi dan membuka Episode Bayi Baru Lahir.<br/>3. Tautan `MotherBabyLinkage` tersimpan di database. | Event consumer + Idempotency check + Conformance Check |

---

## Aturan Teknis Tambahan
1. **Pemisahan Wewenang**: Modul rawat inap **dilarang keras** memodifikasi status kamar bedah atau jadwal operasi secara langsung. Seluruh interaksi ke jadwal operasi bersifat read-only melalui API IBS.
2. **Kemandirian Transaksi**: Jika consumer kelahiran bayi (`BE-RWI-184`) mengalami antrean tertunda, hal tersebut tidak boleh membatalkan pencatatan rekam medis persalinan ibu di kamar bedah.
