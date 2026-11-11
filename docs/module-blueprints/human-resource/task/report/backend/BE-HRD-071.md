# Laporan Perubahan Backend — `BE-HRD-071`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-071` |
| Judul | Pembentukan Masa Percobaan Otomatis dari Jenis Kontrak (PKWTT) |
| Slice | S-C4-ONB (LifecycleManagement) |
| Milestone | MVP-14 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-087 butir 5, VR-ONB-35, VR-ONB-36, AT-HRD-ONB-28 |
| Dependency | BE-HRD-063 ✅, BE-HRD-069 ✅ |
| Klasifikasi | `CRITICAL / CORE` |
| Task Mode | `BACKEND` |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Roslyn Build 0 Error |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Menghasilkan masa percobaan (HrdProbation) tepat 3 bulan secara otomatis saat kontrak kerja pertama berjenis Tetap (PKWTT) disahkan. Menegakkan aturan UU Ketenagakerjaan bahwa kontrak PKWT dilarang memiliki masa percobaan (VR-ONB-35). Menyinkronkan tanggal akhir masa percobaan ke riwayat kontrak dan master data pegawai.

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

```mermaid
flowchart TD
    A["Pengesahan Kontrak Kerja Pertama"] --> B{"Periksa Jenis Kontrak Pegawai"}
    B -->|PKWT (Kontrak Waktu Tertentu)| C["Dilarang Ada Masa Percobaan (VR-ONB-35) -> Selesai"]
    B -->|PKWTT (Pegawai Tetap)| D["HrdProbationService.CreateForInitialContractAsync"]
    D --> E["Hitung Tanggal Selesai = StartDate + Tepat 3 Bulan - 1 Hari (VR-ONB-36)"]
    E --> F["Hitung Cutoff Evaluasi = Akhir Percobaan - 7 Hari"]
    F --> G["Terbitkan Rekam HrdProbation (Status: Running)"]
    G --> H["Sinkronisasi ProbationEndDate ke WfpContractHistory & MstEmployee"]
```

---

## 3. Komponen Perubahan yang Diterapkan

- HrdProbationDtos.cs di LifecycleManagement/DTOs/
- IHrdProbationService.cs & HrdProbationService.cs di LifecycleManagement/Services/
- Query daftar masa percobaan aktif dan evaluasi penilaian bulanan.

---

## 4. Spesifikasi Endpoint API Bergaya Swagger

| Method | Path | Deskripsi | Auth | Request/Response |
| :--- | :--- | :--- | :--- | :--- |
| GET | /api/v1/corporate/human-resource/probations | Mengambil daftar masa percobaan pegawai | HR Staff | PagedResult<ProbationListItem> |
| GET | /api/v1/corporate/human-resource/probations/{id} | Mengambil detail masa percobaan & riwayat penilaian | HR Staff | ProbationDetailResponse |

---

## 5. Aturan Validasi dan Invarian Bisnis yang Ditegakkan

- VR-ONB-35: Masa percobaan hanya sah untuk perjanjian kerja waktu tidak tertentu (PKWTT).
- VR-ONB-36: Durasi masa percobaan PKWTT dibatasi maksimal dan tepat 3 bulan kalender.

---

## 6. Bukti Verifikasi dan Pengujian

- **Kompilasi Roslyn**: Lulus `dotnet build /t:Compile` dengan **0 Error(s)** pada `QuilvianSystemBackend.csproj`.
- **Integritas Skema**: Menghasilkan file migrasi EF Core `20261104160000_AddEmploymentLifecycleCoreAndReshapeTemplates.cs` dengan proteksi `EMPTY_TABLE_GUARD`.
- **Arsitektur Bersih**: Seluruh dependensi terdaftar pada `Program.cs` tanpa siklus ketergantungan.
