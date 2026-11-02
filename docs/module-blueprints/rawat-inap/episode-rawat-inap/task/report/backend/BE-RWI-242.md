# Laporan Perubahan Backend — `BE-RWI-242`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-242` |
| Judul | Ekstensi Penerimaan Serah Terima Klinis dengan Validasi Wajib PPJA dan Pencegahan Akun Sama |
| Slice | Endpoint penerimaan serah terima klinis (`PATCH /transfer-handovers/{id}/accept`) mewajibkan penetapan PPJA, menolak akun penerima yang sama dengan pengirim, dan mencatat rekonsiliasi obat |
| Roadmap | [backend-roadmap-transfer-pasien-masuk.md](../../roadmap/backend-roadmap-transfer-pasien-masuk.md) — kartu `BE-RWI-242` |
| Trace | FR-TRF-003, FR-TRF-004, FR-TRF-005; TRF-AC-003, TRF-AC-004, TRF-AC-005; Kontrak 0.14.0 Transfer Handover 2.1; Kamus Data 23.1, 23.2 |
| Contract version | `0.14.0` |
| Dependency | Tidak ada (Gelombang 1) |
| Klasifikasi | `MEDIUM` — Penambahan field DTO request & response, penegakan 4 aturan validasi kesalahan (`CLI-TRH-001` s.d. `CLI-TRH-004`), pencatatan rekonsiliasi obat, dan otomasi penugasan perawat bangsal (`InpNurseAssignment`) |
| Task mode | `BACKEND` — Permintaan pengerjaan tuntas seluruh task oleh pengguna |
| Target tulis | `Areas/HealthServices/ClinicalManagement/DTOs/TransferHandoverDtos.cs`, `Areas/HealthServices/ClinicalManagement/Services/CliTransferHandoverService.cs`, `Areas/HealthServices/ClinicalManagement/Controllers/TransferHandoverController.cs` |
| Tanggal | 22 Oktober 2026 |
| Status | ✅ Selesai — Implementasi DTO, validasi bisnis terstandarisasi, penugasan perawat bangsal, dan pencatatan snapshot rekonsiliasi telah selesai |

---

## 1. Masalah yang Diselesaikan

Pada penerimaan pasien transfer (khususnya pasien transfer masuk dari IGD/Rajal maupun antar-ruangan), proses serah terima asuhan klinis menuntut akuntabilitas perawat penerima dan kepastian kelanjutan terapi medikasi:
1. **Ketiadaan Penugasan PPJA:** Pasien sering kali diterima di bangsal tanpa penunjukan Perawat Penanggung Jawab Asuhan (PPJA) yang jelas sejak awal pasien tiba di ruang rawat.
2. **Konflik Kepentingan & Validitas Serah Terima:** Tanpa pencegahan sistem, pengguna yang sama (perawat IGD) dapat mengirim sekaligus menerima dokumen serah terima dirinya sendiri, mencederai prinsip verifikasi ganda (double-check) keselamatan pasien.
3. **Pencatatan Rekonsiliasi Obat:** Belum ada struktur terstandar untuk memverifikasi obat-obatan yang dibawa dari unit asal apakah dilanjutkan, ditunda, diganti, atau dihentikan saat tiba di bangsal.

> **Skenario Sebelum:** Perawat dapat menerima serah terima transfer tanpa memilih siapa perawat penanggung jawab (PPJA), akun pengirim dan penerima dapat menggunakan akun yang sama, dan instruksi penyesuaian obat tidak tersimpan dalam audit serah terima.

> **Skenario Sesudah:** Saat perawat bangsal menerima serah terima klinis melalui `PATCH /transfer-handovers/{id}/accept`, sistem memvalidasi wajibnya penetapan PPJA (`CLI-TRH-001`), menolak jika akun penerima sama dengan pembuat dokumen serah terima (`CLI-TRH-002`), menolak jika status dokumen bukan `Sent` (`CLI-TRH-004`), otomatis memperbarui penugasan perawat di tabel `InpNurseAssignment`, dan mendokumentasikan rekonsiliasi obat ke dalam riwayat snapshot.

---

## 2. Alur Proses Bisnis & Aturan

| Unsur | Penjelasan |
| --- | --- |
| **Tujuan** | Menjamin serah terima klinis diterima oleh perawat bangsal yang sah, menetapkan PPJA resmi, dan merekonsiliasi terapi obat pasien |
| **Pelaku** | Perawat Bangsal Rawat Inap (Penerima Transfer) |
| **Pemicu** | Eksekusi `PATCH /api/v1/health-services/clinical-management/transfer-handovers/{id}/accept` |

### Langkah dan Aturan Bisnis:
1. **Validasi Status Dokumen (`CLI-TRH-004`):**
   - Dokumen serah terima harus berstatus `Sent`. Jika berstatus selain `Sent` (misalnya masih `Draft`/`PendingSend` atau sudah `Accepted`), sistem menolak dengan HTTP `409 Conflict`.
2. **Validasi Wajib PPJA (`CLI-TRH-001`):**
   - Request harus menyertakan `AssignedNurseId` yang valid (`Guid` tidak boleh kosong). Jika kosong, sistem menolak dengan HTTP `422 Unprocessable Entity` dan pesan: *"Penetapan Perawat Penanggung Jawab Asuhan (PPJA) wajib diisi saat menerima pasien transfer."*
3. **Pencegahan Akun Pengirim dan Penerima Sama (`CLI-TRH-002`):**
   - Akun pengguna yang melakukan accept (`currentUserId`) tidak boleh sama dengan pembuat dokumen (`CreatedByUserId` serah terima). Jika sama, sistem menolak dengan HTTP `400 Bad Request` dan pesan: *"Penerima serah terima klinis tidak boleh akun yang sama dengan perawat pengirim."*
4. **Validasi Alasan Penolakan Serah Terima (`CLI-TRH-003`):**
   - Pada alur penolakan serah terima (`reject`), alasan penolakan wajib diisi minimal 10 karakter.
5. **Otomasi Penugasan Perawat PPJA ke Rawat Inap (`InpNurseAssignment`):**
   - Jika serah terima terikat ke episode rawat inap (`InpEpisodeId` tidak null), sistem secara otomatis mengakhiri penugasan aktif sebelumnya untuk perawat bersangkutan dan membuat rekam penugasan baru di `InpNurseAssignment` dengan peran `PPJA` (urutan sekuens naik).
6. **Pencatatan Rekonsiliasi Obat & Catatan Serah Terima:**
   - Daftar item rekonsiliasi obat (`MedicationReconciliations` yang memuat nama obat, dosis, frekuensi, rute, status tindak lanjut, dan instruksi khusus) dan catatan penerimaan (`Notes`) disimpan ke dalam `SnapshotJson` dokumen serah terima dan diproyeksikan pada saat retrieval `GET /transfer-handovers/{id}`.

---

## 3. Rincian Perubahan Kode

### 3.1 Berkas yang Diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/DTOs/TransferHandoverDtos.cs` | • Menambahkan class `MedicationReconciliationItemDto`<br>• Menambahkan `AssignedNurseId`, `Notes`, dan `MedicationReconciliations` pada `AcceptTransferHandoverRequest`<br>• Menambahkan properti rekonsiliasi dan catatan pada `TransferHandoverSnapshot`<br>• Menambahkan `AssignedNurseId`, `AssignedNurseName`, `AssignedRole`, `Notes`, dan `MedicationReconciliations` pada `TransferHandoverResponse`<br>• Menjadikan `FromPlacementId` dan `ToPlacementId` nullable pada response DTO |
| `Areas/HealthServices/ClinicalManagement/Services/CliTransferHandoverService.cs` | • Menambahkan konstanta kode kesalahan `CLI-TRH-001`, `CLI-TRH-002`, `CLI-TRH-003`, dan `CLI-TRH-004`<br>• Pada `AcceptAsync`: mengimplementasikan validasi error terstandar, memperbarui status menjadi `Accepted`, mencatat log penugasan perawat di `InpNurseAssignment`, dan menyimpan riwayat rekonsiliasi obat ke JSON snapshot<br>• Pada `GetByIdAsync`: memproyeksikan data penugasan perawat aktif dari `InpNurseAssignment` serta parsing rekonsiliasi obat dari snapshot |
| `Areas/HealthServices/ClinicalManagement/Controllers/TransferHandoverController.cs` | • Memperbarui dokumentasi XML summary dan remarks endpoint `PATCH /transfer-handovers/{id}/accept` dengan detail kode status 200, 400, 409, dan 422 |

---

## 4. Dokumentasi Endpoint

#### Health Services / Clinical Management / Transfer Handover
Base URL: `api/v1/health-services/clinical-management/transfer-handovers`

| Method | Path | Deskripsi | Otorisasi | Request Body | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `PATCH` | `/{id}/accept` | Menerima serah terima transfer klinis, memvalidasi PPJA & akun pengirim/penerima, menetapkan PPJA ke bangsal, dan mencatat rekonsiliasi obat | `ClinicalTransferHandover : Update` | `AcceptTransferHandoverRequest` | `TransferHandoverResponse` |
| `GET` | `/{id}` | Mengambil detail dokumen serah terima transfer klinis beserta informasi PPJA terpilih dan daftar rekonsiliasi obat | `ClinicalTransferHandover : Read` | — | `TransferHandoverResponse` |

---

## 5. Verifikasi Kriteria Penerimaan (Acceptance Criteria)

| Kode AC | Kriteria Penerimaan | Bukti Implementasi pada Kode | Status |
| :--- | :--- | :--- | :---: |
| `TRF-AC-003` | Rekonsiliasi obat IGD/Poli tersimpan pada snapshot serah terima klinis saat diterima | `CliTransferHandoverService.cs`: item `MedicationReconciliations` diserialisasikan ke `SnapshotJson` dan diproyeksikan saat `GetByIdAsync` | ✅ Terbukti |
| `TRF-AC-004` | `PATCH /transfer-handovers/{id}/accept` menolak request tanpa `assignedNurseId` dengan `422 Unprocessable Entity` (`CLI-TRH-001`) | `CliTransferHandoverService.cs`: pengecekan `request.AssignedNurseId == Guid.Empty` melempar `InvalidOperationException` dengan format `CLI-TRH-001` (422) | ✅ Terbukti |
| `TRF-AC-005` | `PATCH /transfer-handovers/{id}/accept` menolak request jika akun login sama dengan pengirim (`CLI-TRH-002`) | `CliTransferHandoverService.cs`: pengecekan `currentUserId == handover.CreatedByUserId` melempar `InvalidOperationException` dengan format `CLI-TRH-002` (400) | ✅ Terbukti |
| Status Guard | Menolak penerimaan dokumen jika status bukan `Sent` | `CliTransferHandoverService.cs`: pengecekan status melempar `CLI-TRH-004` (409) | ✅ Terbukti |
| Penugasan PPJA | Otomatis mencatat penugasan PPJA ke rawat inap | `CliTransferHandoverService.cs`: penambahan entitas `InpNurseAssignment` baru ke dalam DbContext | ✅ Terbukti |
