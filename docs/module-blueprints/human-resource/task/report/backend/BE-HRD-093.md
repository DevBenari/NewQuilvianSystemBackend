# Laporan Perubahan Backend — `BE-HRD-093`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-093` |
| Judul | Penerbitan Roster Mandiri Kepala Unit dengan Recheck Otomatis & Notifikasi Tembusan |
| Slice | S-B4 (SchedulingManagement) |
| Milestone | MVP-18 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-098, 02-backend-architecture.md Bagian 19.4.2, HRD-TF-052 (a), API Contract v10 /roster-periods/{id}/publish, AT-HRD-SCH-12, AT-HRD-SCH-13 |
| Dependency | `BE-HRD-089` ✅, `BE-HRD-090` ✅, `BE-HRD-092` ✅ |
| Klasifikasi | `CORE ROSTER PUBLICATION & AUTONOMOUS GOVERNANCE` |
| Task Mode | `BACKEND` |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Roslyn Build 0 Error |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Berdasarkan keputusan bisnis `HRD-DEC-098`, rumah sakit memberlakukan prinsip **Penerbitan Mandiri Kepala Unit (Autonomous Unit Head Publishing)**:
- Kepala Ruangan / Kepala Unit memiliki wewenang penuh untuk menerbitkan jadwal dinas unitnya secara mandiri tanpa menunggu persetujuan birokratis berjenjang dari Kepala Bidang Keperawatan atau HRD.
- Namun, wewenang mandiri ini diimbangi dengan **Gerbang Keselamatan Otomatis (Recheck Gate)**: Tombol Terbitkan (`Publish`) otomatis menjalankan uji ulang seluruh aturan pemblokiran `B1`–`B7` dan peringatan `W1`–`W5` tepat sebelum status diubah.
- Jika masih ada konflik memblokir (`BlockingConflicts > 0`), penerbitan ditolak mutlak (HTTP 422 Unprocessable Entity) — menutup celah *bypass* publikasi (`HRD-TF-052` a).
- Jika ada peringatan kerja (`WarningConflicts > 0`), publikasi hanya diizinkan bila seluruh peringatan disertai alasan konfirmasi resmi (`WarningReasons`).
- Setelah terbit resmi: status dimutasi menjadi `Published`, snapshot jejak publikasi dicatat di `TrxRosterPublication`, dan notifikasi tembusan dikirimkan ke Kepala Bidang serta seluruh staf yang bertugas.

Task `BE-HRD-093` mengimplementasikan logika penerbitan mandiri berpagar keselamatan ini pada backend.

---

## 2. Alur Proses Penerbitan Mandiri Berpagar Keselamatan

```mermaid
flowchart TD
    A["Kepala Unit Klik Tombol 'Terbitkan Roster'"] --> B["POST /roster-periods/{id}/publish"]
    B --> C["Jalankan Recheck Otomatis (RosterValidationService.ValidateRosterPeriodAsync)"]
    
    C --> D{"Ada Pelanggaran Memblokir (B1-B7)?"}
    D -->|Ya (BlockingConflicts > 0)| E["Tolak Publikasi (HTTP 422): Konflik Keselamatan Wajib Diperbaiki"]
    
    D -->|Tidak Ada Konflik Memblokir| F{"Ada Peringatan W1-W5?"}
    F -->|Ya| G{"Apakah Disertai Alasan Konfirmasi Sah untuk Semua Peringatan?"}
    G -->|Tidak Lengkap| H["Tolak Publikasi (HTTP 422): Alasan Konfirmasi Peringatan Kurang"]
    G -->|Lengkap| I["Lolos Validasi Peringatan"]
    F -->|Tidak Ada| I
    
    I --> J["Ubah Status Periode & Seluruh Shift Menjadi 'Published'"]
    J --> K["Set Flag IsPublished = True, IsRecheckPassed = True"]
    K --> L["Catat Jejak Audit Publikasi ke Tabel TrxRosterPublication"]
    L --> M["Kirim Tembusan Notifikasi ke Kepala Bidang & Staf Unit"]
    M --> N["Kembalikan Response 200 OK: Roster Resmi Terbit"]
```

---

## 3. Komponen Perubahan yang Diterapkan

1. **Metode Service (`RosterPeriodService.PublishRosterPeriodAsync`)**:
   - Menjalankan recheck otomatis dengan mengeksekusi `_rosterValidationService.ValidateRosterPeriodAsync`.
   - Menolak keras publikasi jika `valResult.BlockingConflicts > 0`.
   - Memvalidasi kecukupan alasan konfirmasi `req.WarningReasons` jika `valResult.WarningConflicts > 0`.
   - Mengubah status penugasan staf dan shift menjadi `Published`.
   - Menghasilkan entitas riwayat publikasi `TrxRosterPublication` lengkap dengan nomor publikasi unik (`PUB-...`), versi, snapshot audiens, dan alasan konfirmasi JSONB.
2. **DTO Pendukung (`RosterSchedulingDtos.cs`)**:
   - `PublishRosterPeriodRequest` (menerima array `WarningReasonItemRequest`).
   - `PublishRosterPeriodResponse` (mengembalikan timestamp, nama penerbit, dan pesan sukses).
3. **Endpoint Controller (`RosterPeriodController.cs`)**:
   - `POST /api/corporate/human-resource/scheduling/roster-periods/{id}/publish` dengan otorisasi `[AccessPermission("RosterPeriod", "Update")]`.

---

## 4. Spesifikasi Endpoint API Bergaya Swagger

| Method | Endpoint Path | Deskripsi | Auth / Role | Status |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/corporate/human-resource/scheduling/roster-periods/{id}/publish` | Menerbitkan periode roster secara mandiri dengan validasi ulang otomatis keselamatan kerja | Unit Head / Update | 200 OK / 422 Unprocessable |

---

## 5. Bukti Verifikasi dan Pengujian

- **Kompilasi Roslyn**: Lulus `dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj /t:Compile` dengan **0 Error(s)**.
- **Kasus Uji Penerimaan (`AT-HRD-SCH-12` & `AT-HRD-SCH-13`)**: Terbukti menolak penerbitan bila terdapat konflik memblokir `B1`–`B7` dan berhasil menerbitkan roster saat jadwal bersih atau disertai alasan peringatan lengkap.
