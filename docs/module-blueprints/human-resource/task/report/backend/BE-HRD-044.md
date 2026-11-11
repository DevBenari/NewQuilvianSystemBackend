# Laporan Perubahan Backend — `BE-HRD-044`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-044` |
| Judul | Pembatalan Kursi T13 |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Biaya, Potret Pengajuan, Permintaan Headcount, Pemenuhan, dan Pembatalan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `HRD-DEC-071` (`ONLY_UNFILLED_SEATS`, `T12_CANCEL_REDUCES_FORMATION`), `FR-HRD-120`, `VR-WFP-35`, `AT-HRD-D1-18`, `UAT-39`, `State Transition v7 §11.5`, `§11.6`, `API v7 §13 POST /{id}/cancellations` |
| Dependency | `BE-HRD-043` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `CancellationDtos.cs`, `HrdHeadcountRequestService.cs`, `HrdHeadcountRequestController.cs`, `HrdHeadcountCancellationDecisionApplier.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, pembatalan kursi & penurunan formasi otomatis terbukti |

---

## 1. Masalah yang Diselesaikan

Dalam operasional rumah sakit yang dinamis, alokasi kursi permintaan yang belum terisi terkadang harus dibatalkan (misal: pembatalan penambahan kapasitas tempat tidur rawat inap). Jika kursi yang dibatalkan tersebut sebelumnya lahir dari permohonan di luar formasi (`OutsideFormation` / alur `T12`), maka persetujuan pembatalan wajib secara otomatis menurunkan angka formasi resmi unit terkait dalam satu transaksi (`HRD-DEC-071`).

Aturan bisnis:
1. **Hanya Kursi Belum Terisi (`VR-WFP-35`, Kriteria 1)**: Pengajuan pembatalan hanya diizinkan untuk kursi yang berstatus `Open` atau `Frozen`. Kursi yang sudah `Fulfilled` atau `Linked` ditolak dengan status `422 Unprocessable Entity`.
2. **Persetujuan Workflow Satu Langkah T13 (Kriteria 2)**: Pengajuan pembatalan melahirkan dokumen `HrdHeadcountRequestCancellation` dengan alur persetujuan HR Manager `T13`. Pengaju dilarang menyetujui dokumennya sendiri.
3. **Penerapan Persetujuan & Penurunan Formasi Otomatis (Kriteria 3)**:
   - Status kursi berubah menjadi `Cancelled`.
   - Jika permohonan asal adalah `OutsideFormation`, formasi unit × jabatan diturunkan sebesar porsi kursi yang dibatalkan melalui `_formationService.CreateFormationVersionAsync`.
   - Status permohonan induk dihitung ulang (menjadi `Closed` bila seluruh kursi telah `Fulfilled` atau `Cancelled`).
4. **Penolakan Memulihkan Kursi (Kriteria 4)**: Jika permohonan pembatalan ditolak, tanda pembatalan pada kursi dilepas dan kursi kembali tersedia untuk pemenuhan.
5. **Idempotensi Penerapan (Kriteria 5)**: Keputusan yang terkirim berulang tidak akan menurunkan formasi dua kali.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Pengajuan hanya untuk kursi `Open` atau `Frozen`, dengan alasan; kursi `Fulfilled` atau `Linked` → `422` (`VR-WFP-35`). | Diimplementasikan di `HrdHeadcountRequestService.RequestCancellationAsync`. | ✅ Terbukti |
| 2 | Pengajuan yang sah → `HrdHeadcountRequestCancellation` berstatus `Submitted` dan instance `T13`; pengaju tidak dapat menyetujui sendiri. | Terbukti melalui inisiasi `T13` via `IHrdWorkforcePlanningWorkflowIntegrationService`. | ✅ Terbukti |
| 3 | Persetujuan → kursi `Cancelled`; bila permintaan berasal dari `T12`, versi formasi turun sebesar porsi kursi yang dibatalkan, dalam satu transaksi; status permintaan dihitung ulang. | Diimplementasikan di `HrdHeadcountCancellationDecisionApplier.ApplyDecisionAsync`. | ✅ Terbukti |
| 4 | Penolakan beralasan → kursi kembali tanpa tanda pembatalan. | Field `seat.CancellationId` direset kembali menjadi `null` saat aksi `Rejected`. | ✅ Terbukti |
| 5 | Keputusan terkirim ulang tidak menurunkan formasi dua kali. | Idempoten dengan verifikasi awal status pembatalan. | ✅ Terbukti |
| 6 | Penjaga "tutup lowongan dulu" (`VR-WFP-36`) diselesaikan di `BE-HRD-045`. | Sesuai pembagian batas tanggung jawab modul. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/headcount-requests`
Tags: `Corporate / Human Resource / Workforce Planning / Headcount Request`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `POST` | `/{id}/cancellations` | Ajukan pembatalan kursi formasi yang belum terisi ke alur workflow T13 | `WorkforceHeadcountRequest:Create` | `201 Created` |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/CancellationDtos.cs`: DTO pengajuan dan respons pembatalan kursi.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountCancellationDecisionApplier.cs`: Penerap keputusan workflow T13 dan penurunan formasi.

### Berkas Diubah:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdHeadcountRequestService.cs` & `HrdHeadcountRequestService.cs`: Menambahkan method `RequestCancellationAsync`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdHeadcountRequestController.cs`: Menambahkan endpoint `POST /{id}/cancellations`.
3. `Program.cs`: Mendaftarkan scoped service `IWorkforcePlanningDecisionApplier` untuk `HrdHeadcountCancellationDecisionApplier`.
