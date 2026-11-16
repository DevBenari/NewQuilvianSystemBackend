# Laporan Penyelesaian Task: BE-HRD-337 Pengunduran Diri: Status, Approval, Handoff, Persistence

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-337`
* **Nama Task**: Pengunduran Diri: Status, Approval, Handoff, Persistence
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 1: Pengunduran Diri)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Alur lengkap pengunduran diri pegawai (Resignation) dari pengajuan permohonan hingga handoff serah terima aset.
  - Validasi keberadaan profil pegawai aktif di database rumah sakit.
  - Pemeriksaan notice period (30 hari standar UU Ketenagakerjaan / regulasi internal RS) secara sistematis.
  - Alur persetujuan ter-audit (Pending -> Approved / Rejected).
  - Integrasi handoff ke `TrxEmployeeSeparation` dan pembentukan checklist inventaris serah terima tugas.
  - Persistensi data murni dari database: data tetap konsisten dan tersimpan utuh saat dilakukan pembacaan ulang (read-after-write/refresh).

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement` & `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `FEATURE REINFORCEMENT`
* **Wewenang**: Penyelarasan controller, service, dan DTO pengunduran diri pegawai.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Pengunduran diri perawat, dokter spesialis, atau staf apotek di rumah sakit membutuhkan tata kelola ketat:
1. **Notice Period 30 Hari**: Memberikan waktu bagi kepala ruang rawat atau komite medik untuk mengatur jadwal jaga pengganti (shift replacement) dan mencegah kekosongan tenaga medis yang membahayakan nyawa pasien rawat inap.
2. **Persetujuan Berjenjang**: Pengunduran diri harus disetujui Kepala Ruangan / Kepala Instalasi terlebih dahulu sebelum disahkan oleh HRD Manager.
3. **Pemberian Checklist Handoff**: Begitu disetujui, pegawai wajib menyerahkan tanggung jawab operasional dan aset inventaris (seperti stetoskop unit, kartu akses ruang farmasi, dan berkas rekam medis yang belum ditutup) melalui submenu Serah Terima.

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload / Query | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/employee-relations/resignations` | `[Tags("HR Resignation")]` | Ambil daftar pengajuan pengunduran diri terpaginasi | `Hrd.Separation.View` | `page`, `pageSize`, `status`, `search` | `ApiResponse<PagedResult<ResignationDto>>` |
| `GET` | `/api/corporate/human-resource/employee-relations/resignations/{id}` | `[Tags("HR Resignation")]` | Ambil rincian lengkap pengunduran diri | `Hrd.Separation.View` | - | `ApiResponse<ResignationDetailDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/resignations` | `[Tags("HR Resignation")]` | Ajukan permohonan pengunduran diri baru | `Hrd.Separation.Create` | `CreateResignationRequestDto` | `ApiResponse<ResignationDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/resignations/{id}/approve` | `[Tags("HR Resignation")]` | Setujui pengunduran diri pegawai | `Hrd.Separation.Approve` | `ApproveResignationRequestDto` | `ApiResponse<ResignationDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/resignations/{id}/reject` | `[Tags("HR Resignation")]` | Tolak pengunduran diri pegawai | `Hrd.Separation.Approve` | `RejectResignationRequestDto` | `ApiResponse<ResignationDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/resignations/{id}/handoff` | `[Tags("HR Resignation")]` | Teruskan ke serah terima tugas & kliring aset | `Hrd.Separation.Manage` | - | `ApiResponse<ResignationHandoffResultDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Persistence**: Data tersimpan langsung pada tabel basis data EF Core tanpa mock data runtime.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).
