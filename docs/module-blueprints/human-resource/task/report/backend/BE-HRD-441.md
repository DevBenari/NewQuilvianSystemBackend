# Laporan Perubahan Backend — `BE-HRD-441`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-441` |
| Judul | Engine: Plh aktif boleh memutuskan penugasan yang sudah ada |
| Slice | `S-A7-V2` amandemen / `MVP-72` gelombang 2 · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 29, kartu `BE-HRD-441` |
| Trace | `FR-WF-31`, `FR-WF-28`; `HRD-DEC-234`, `HRD-DEC-230`, `HRD-DEC-183` |
| Contract version | `v29` — Validation `VR-WF-30`, `VR-WF-25`, `VR-WF-02`; Integration `INT-HR-113`; Permission 33.2 — approved 20 November 2026 |
| Dependency | `BE-HRD-440` ✅ |
| Klasifikasi | `STANDARD` — perubahan otorisasi inti engine workflow; uji harness engine nyata |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026: "lanjut BE-HRD-441") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `815ce085` (branch `yoga`) + working tree `BE-HRD-440` dan task ini |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkflowManagement` (registry: `SHARED PLATFORM CAPABILITY`, prefix `Wfl`, `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` untuk `WorkflowService` (dua berkas parsial); `NEW CODE` untuk helper `WorkflowDelegationResolver` (kelas statis tanpa entity, tanpa repository generik) |
| QBE yang berlaku | `QBE-SVC-001` (logika tetap di service engine, controller tidak tersentuh), `QBE-API-001` (hasil dan kode status mengikuti `WorkflowServiceResult` yang ada). Tanpa entity, migration, atau `[AccessPermission]` baru |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`, `DATABASE_RULES.md` (suite Skill) |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowDelegationResolver.cs` (baru) | Helper bersama: `WhereActiveAt` (status `Active`/`Approved`, aktif, tidak dihapus/dibatalkan, dalam rentang `EffectiveStartAt`–`EffectiveEndAt`), `WhereCovers` (semua workflow, definisi, tahap), `OrderBySpecificity`, dan `FindActingDelegationAsync(pejabat, Plh, definisi, tahap)`. Dipakai engine sekarang dan adapter kotak terpadu (`BE-HRD-443`/`444`) nanti |
| `…/Services/WorkflowService.ActionsV2.cs` | `ValidateActionAssignment` → `ValidateActionAssignmentAsync` (Tolak, Revisi, Kembalikan). Baru: `ResolveActingAuthorityAsync`: pemegang penugasan **atau** Plh aktif dari pemegang penugasan, dan Plh bukan pemohon (`VR-WF-30`, `VR-WF-02`); penugasan tidak dipindahkan. Baru: `ApplyActingDelegation`: jejak aksi Plh mengisi `IsDelegated`, `ApprovalDelegationId`, `DelegatedFromUserId`/`WorkforceProfileId` = pejabat definitif, `ActionEmployeeId`, `OnBehalfOfEmployeeId` (`VR-WF-25`). Kelas `ActingAuthority`; `ActionAssignmentValidation` membawa `Delegation` |
| `…/Services/WorkflowService.cs` | `ApproveAsync` memakai `ResolveActingAuthorityAsync` dan `ApplyActingDelegation`. `ApplyDelegationAsync` (pengalihan saat penugasan dibuat) memakai predikat helper yang sama; perilakunya tidak berubah |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkflowDelegatedActionTests.cs` (baru), `AcceptanceTestIndex.md` | 11 uji (6 fakta + 5 kasus teori) dan satu baris indeks |

Engine tidak pernah mengisi `DigitalSignatureHash` (hanya engine V2 lama yang mengisinya). Butir "tanpa `DigitalSignatureHash`" (`HRD-DEC-230`) sudah terpenuhi as-is dan kini dikunci uji.

---

## 2. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `WorkflowDelegatedActionTests` | 11/11 lulus | `PASS` (AC1–AC4) | Keluaran `dotnet test` |
| Kontrol negatif: 8 mutasi (tanpa pencarian delegasi; tanpa cek pemohon; jejak Plh tidak dicatat; tanpa batas akhir; tanpa cek status; cakupan selalu cocok; approve hanya pemegang; penerima delegasi tidak dicek) | 8/8 tertangkap; source dipulihkan | `PASS` | `mut441.js` |
| `TravelSubmitTests` + `TravelWorkflowLifecycleTests` (perilaku modul SPPD) | Hijau (bersama uji baru, 55 uji pada satu jalan sebelum perbaikan susunan uji) | `PASS` (AC4) | Keluaran `dotnet test` |
| Regresi penuh proyek uji HR | 371/371 lulus (10 menit 59 detik; dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085`, lihat `BE-HRD-440`) | `PASS` (AC4) | Keluaran `dotnet test` |
| QBE `Strict` atas 3 berkas | 0 violation, 0 review; probe `TrxZzzQbeProbe441` terdeteksi 4 violation lalu dihapus | `PASS` | Keluaran checker |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Plh aktif menyetujui penugasan yang dibuat sebelum delegasi → `200`, jejak berisi pelaku Plh dan pejabat definitif | Terpenuhi (juga Tolak dan Kembalikan) | `Plh_aktif_menyetujui_…`, `Plh_aktif_dapat_menolak_dan_mengembalikan` |
| 2. Delegasi berakhir/dicabut → Plh `403` | Terpenuhi (juga belum mulai, definisi lain, untuk orang lain, tanpa delegasi) | Teori `Plh_tanpa_delegasi_aktif_yang_mencakup_ditolak` (5 kasus), `Delegasi_untuk_orang_lain_tidak_memberi_hak` |
| 3. Plh yang adalah pemohon → `403` | Terpenuhi ("Pejabat tidak boleh menyetujui permohonan sendiri.") | `Plh_yang_juga_pemohon_ditolak` |
| 4. Kotak lama dan modul (SPPD) tetap sama | Terpenuhi | Uji SPPD hijau; `Pejabat_definitif_tetap_dapat_menyetujui_…`; `Delegasi_saat_penugasan_dibuat_tetap_mengalihkan_penugasan` |

---

## 3. Keputusan implementasi (dalam batas kontrak)

| Hal | Keputusan | Alasan |
| --- | --- | --- |
| Penugasan saat Plh bertindak | Tidak dipindahkan (`AssignedApproverUserId` tetap pejabat definitif) | Integration 26.3: tugas kembali hanya ke pejabat sesudah delegasi berakhir |
| Pemohon sebagai pemegang penugasan langsung | Tidak diubah (masih diatur `AllowSelfApproval` tahap saat penugasan dibuat) | Di luar cakupan task; cek pemohon baru berlaku untuk jalur Plh |
| Helper berbentuk kelas statis dengan `DbContext` | Tanpa registrasi DI baru dan tanpa repository generik | `DATABASE_RULES`: tidak memperkenalkan abstraksi baru |

---

## 4. Delta dan sisa

| Hal | Status |
| --- | --- |
| Antrean Plh dan blok `myAction` di kotak terpadu | `BE-HRD-443` (memakai helper ini) |
| `BuildAssignmentAvailableActions` (detail workflow kotak lama) belum menampilkan aksi untuk Plh | Tidak diubah: kotak lama dipensiunkan (`HRD-DEC-231`); di kotak terpadu digantikan `myAction` (`BE-HRD-443`) |
| `ActionEmployeeId` berisi `EmployeeId` konteks pengguna; bisa `null` bila akun tanpa data pegawai | Sesuai data as-is; `OnBehalfOfEmployeeId` dari `PrincipalEmployeeId` delegasi |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Regresi penuh HR | 371/371 lulus |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `WorkflowDelegationResolver.cs`, `WorkflowDelegatedActionTests.cs` (cukup `git add`) |
| Task berikutnya | `BE-HRD-442` (eskalasi di engine), `BE-HRD-447` (endpoint generik), lalu `BE-HRD-443`/`444` |
