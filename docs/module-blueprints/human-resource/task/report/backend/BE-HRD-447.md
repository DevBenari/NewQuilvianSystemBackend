# Laporan Perubahan Backend — `BE-HRD-447`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-447` |
| Judul | Endpoint generik menolak permohonan modul; "Permohonan Saya" untuk permohonan engine |
| Slice | `S-A7-V2` amandemen / `MVP-72` gelombang 2 · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 29, kartu `BE-HRD-447` |
| Trace | `FR-WF-32`; `HRD-DEC-229`, `HRD-DEC-235` |
| Contract version | `v29` — API 35.1, 35.2, 35.6; Validation `VR-WF-29`; Integration `INT-HR-114` — approved 20 November 2026 |
| Dependency | `BE-HRD-440` ✅ (urutan gelombang) |
| Klasifikasi | `STANDARD` — service dan controller kotak terpadu; uji lewat controller |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026: "lanjut") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `815ce085` (branch `yoga`) + working tree `BE-HRD-440`–`442` dan task ini |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkflowManagement` (registry: `SHARED PLATFORM CAPABILITY`, prefix `Wfl`, `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — `WorkflowInstanceCreationService`, `WorkflowResubmitService`, dua controller, DTO V2; `NEW CODE` — `WorkflowInstanceOwnership` (penanda kepemilikan + exception `409`) |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service; controller hanya memetakan exception ke `409`), `QBE-API-001` (`ApiResponse` dan kode status pola yang ada), `QBE-PERM-001` (atribut akses tidak berubah). Tanpa `<summary>` XML baru pada action controller |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`, `API_RULES.md` (suite Skill) |
| Wewenang terpisah | Implementasi source ✅; migration ✖; eksekusi database ✖; deployment ✖ |

---

## 1. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowInstanceOwnership.cs` (baru) | Penanda permohonan milik modul: `WorkflowDefinitionId` terisi **dan** tidak ada penugasan berisi `ApproverEmployeeId` (instance engine V2 lama dibuat tanpa definisi dan selalu mengisi `ApproverEmployeeId`). `IsModuleManagedExpression`, `IsModuleManagedAsync`, tiga pesan kontrak, dan `WorkflowModuleManagedException` |
| `…/Services/WorkflowInstanceCreationService.cs` | `CreateInstanceAsync` → `WorkflowModuleManagedException` (penanda `GenericCreationEnabled = false`; implementasi lama dipertahankan sampai endpoint dihapus, desain 37.10). `CancelInstanceAsync` menolak permohonan modul. `GetMyRequestsAsync(userId, employeeId, …)` mencocokkan `RequestedByUserId` **atau** `RequestedByEmployeeId`, tanpa keduanya mengembalikan halaman kosong, dan mengisi `IsModuleManaged`. `GetInstanceDetailAsync` mengisi `IsModuleManaged` |
| `…/Services/WorkflowResubmitService.cs` | `ResubmitAsync` menolak permohonan modul sebelum aturan revisi lain |
| `…/Controllers/WorkflowInboxAndInstancesController.cs` | `POST /instances` dan `PUT /instances/{id}/cancel`: `WorkflowModuleManagedException` → `409` (+ `ProducesResponseType 409`). `GET /my-requests` meneruskan akun lewat `User.GetUserId()` (klaim `user_id` lalu `NameIdentifier`) dan pegawai lewat `ResolveEmployeeId` |
| `…/Controllers/WorkflowApprovalActionsController.cs` | `POST /instances/{id}/resubmit`: `409` (+ `ProducesResponseType 409`) |
| `…/DTOs/WorkflowV2Dtos.cs` | `WorkflowInstanceV2ResponseDto.IsModuleManaged` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkflowGenericEndpointGuardTests.cs` (baru), `AcceptanceTestIndex.md` | 6 uji lewat controller dan service; satu baris indeks |

---

## 2. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `WorkflowGenericEndpointGuardTests` | 6/6 lulus | `PASS` (AC1–AC2) | Keluaran `dotnet test` |
| Kontrol negatif: 7 mutasi (buat generik diizinkan; batal/kirim ulang tanpa cek modul; tanpa cocok akun; tanpa penjaga id kosong; penanda modul selalu `true`; controller tanpa `409`) | 7/7 tertangkap (G1 dan G2 diulang manual karena skrip gagal mengurai ringkasan); source dipulihkan | `PASS` | `mut447.js` |
| Regresi penuh proyek uji HR | 389/389 lulus (12 menit 4 detik; dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085`) | `PASS` | Keluaran `dotnet test` |
| QBE `Strict` atas 6 berkas | 0 violation, 0 review; probe `TrxZzzQbeProbe447` terdeteksi 4 violation lalu dihapus | `PASS` | Keluaran checker |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tiga endpoint generik → `409` untuk SPPD, data tidak berubah (`AT-HRD-WF-35`) | Terpenuhi (instance engine lama tetap dapat dibatalkan) | `Buat_instance_generik_ditolak_409`, `Batal_dan_kirim_ulang_generik_…`, `Instance_engine_lama_tetap_dapat_dibatalkan` |
| 2. Pemohon tanpa data pegawai melihat SPPD-nya di `my-requests` dengan `isModuleManaged = true` (`AT-HRD-WF-36`) | Terpenuhi | `Permohonan_saya_mencocokkan_akun_pemohon_…`, `Tanpa_akun_dan_pegawai_…`, `Detail_memuat_isModuleManaged` |

---

## 3. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: `GetMyRequestsAsync` tanpa id pegawai (klaim kosong) mengembalikan **seluruh** tiket workflow semua pegawai | Kebocoran data permohonan orang lain di "Permohonan Saya" | Diperbaiki dalam cakupan task: tanpa akun maupun pegawai, halaman kosong; dicocokkan per akun/pegawai |
| `GetCurrentUserId()` lokal controller hanya membaca `NameIdentifier`, sedangkan resolver HR bersama membaca `user_id` lalu `NameIdentifier` | Akun dengan klaim `user_id` saja tidak dikenali | `GET /my-requests` memakai `User.GetUserId()`; action lain tidak diubah (di luar cakupan) |

---

## 4. Catatan penutup

| Hal | Isi |
| --- | --- |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `WorkflowInstanceOwnership.cs`, `WorkflowGenericEndpointGuardTests.cs` (cukup `git add`) |
| Task berikutnya | `BE-HRD-443` (antrean/ringkasan/`myAction`), lalu `BE-HRD-444`; `BE-HRD-446` (SLA) dapat dikerjakan karena `BE-HRD-442` selesai |
