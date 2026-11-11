# Laporan Perubahan Backend — `BE-HRD-316`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-316` |
| Judul | Eksekusi Transaksional Idempoten Domain Hilir (`IdempotentDownstreamExecutionService` - `WF-B8`, `INT-HR-93`, `INT-HR-94`) |
| Slice | `S-A7-V2` / Verifikasi |
| Roadmap | `roadmap/backend-roadmap.md` bagian 22, kartu `BE-HRD-316` |
| Trace | `WF-B8` (`02-backend-architecture.md` §33.3), `INT-HR-93` dan `INT-HR-94` (`contracts/integration-contract.md` §21.1–§21.2), `AT-HRD-WF-18`, `AT-HRD-WF-19` |
| Contract version | Kontrak API `v24` — tidak ada endpoint atau payload yang berubah |
| Dependency | `BE-HRD-309` ✅, `BE-HRD-312` ✅ |
| Klasifikasi | `MEDIUM` — satu repository (0), berkas diperiksa > 20 (2), berkas diubah 2 (0), logika bisnis sedang (1), memakai kontrak yang ada (1), perilaku persistence yang ada (1), terkait keamanan tetapi bukan inti (1), satu workflow (1) = 7 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — dua service di `Areas/Corporate/HumanResource/WorkflowManagement/Services/` dan dokumen blueprint modul ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `edfd843bac1c2860da7ae9a5ddcb68a75cf42666` (branch `MHamzah`) |
| Tanggal | 10 November 2026 |
| Status | 🟡 **SEBAGIAN** — `INT-HR-93` dan idempotensi selesai di kode; `INT-HR-94` belum ada; uji runtime belum dijalankan |

**Dikerjakan ulang.** Laporan sebelumnya (9 November 2026) menyatakan task ini selesai. Audit roadmap
10 November 2026 menemukan `IdempotentDownstreamExecutionService` hanya berisi cabang `if` dengan
komentar dan `await Task.CompletedTask`, sehingga tidak ada satu pun transaksi sumber yang
diperbarui. Laporan ini menggantikan isi laporan lama.

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkflowManagement` |
| Registry | `Corporate/HumanResource — WorkflowManagement / Workflow`, prefix `Wfl`, `SHARED PLATFORM CAPABILITY`, `ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` — service yang sudah ada, entity legacy `Trx*` tidak disentuh dan tidak di-rename |
| QBE yang berlaku | `QBE-SVC-001` (orkestrasi tetap di service), `QBE-TXN-001` (konsistensi keputusan dan transaksi sumber), `QBE-LOG-001` (aktor dicatat pada sinkronisasi hilir) |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*` — tidak ada entity, configuration, atau nomor bisnis baru |

---

## 1. Masalah yang diperbaiki

Kotak masuk persetujuan V2 (`WorkflowApprovalActionsController`) menyimpan keputusan atasan ke
tabel workflow, tetapi tidak meneruskannya ke modul asal permohonan.

**Contoh di rumah sakit.** Ners Rina mengajukan cuti tahunan 3 hari. Kepala Ruangan Melati
menyetujuinya lewat kotak masuk V2. Status workflow menjadi `Approved`, tetapi:

- permohonan cuti Ners Rina tetap berstatus **Menunggu Persetujuan**;
- saldo cutinya tidak terpotong;
- jadwal dinasnya tidak terkunci sebagai cuti.

Bila Kepala Ruangan menolak, permohonan cutinya juga tetap **Menunggu Persetujuan** selamanya.

Ada cacat kedua yang langsung menyangkut syarat "eksekusi ulang tidak boleh menggandakan
transaksi". Service tidak memeriksa apakah tahapan masih aktif. Tahapan terakhir yang sudah
disetujui bisa disetujui lagi — misalnya karena tombol ditekan dua kali — dan permohonan yang
sudah ditolak bisa "disetujui" sesudahnya. Penjaga idempotensi lama hanya
`FinalizedAt < sekarang − 10 detik`, sehingga dua klik dalam 10 detik lolos semua.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Keputusan atasan pada kotak masuk V2 langsung berlaku pada permohonan asalnya |
| Pelaku | Approver tahapan (atau Plh aktif); sistem meneruskan ke modul asal |
| Pemicu | Approver menekan **Setujui** atau **Tolak** pada `POST api/v1/hrd/workflow/approvals/{stepInstanceId}/approve` atau `/reject`, termasuk lewat persetujuan massal |
| Prasyarat | Tahapan masih aktif (`IsCurrentStep = true` dan status `Pending`, `Available`, atau `InProgress`) dan workflow belum berstatus akhir |

**Langkah utama:**

1. Service memuat tahapan beserta workflow-nya.
2. Service menolak bila tahapan sudah tidak aktif atau workflow sudah berstatus akhir
   (`Approved`, `Rejected`, `Cancelled`, `Withdrawn`, `Completed`), dengan pesan
   **"Tahapan persetujuan belum aktif atau sudah diproses."** dan HTTP 400.
3. Service mencatat aksi, memperbarui tahapan dan workflow, lalu menyimpan semuanya dalam satu
   `SaveChangesAsync`.
4. Sesudah tersimpan, service memanggil `IdempotentDownstreamExecutionService`, yang meneruskan ke
   `WorkflowReferenceLifecycleService.HandleAsync` — pintu yang sama yang dipakai mesin workflow
   utama (`WorkflowService`).
5. Dispatcher memilih handler milik modul asal sesuai `ReferenceType`, lalu handler menurunkan
   ulang status transaksi sumber dari status workflow.

**Pemetaan status pada contoh cuti** (`LeaveRequestWorkflowLifecycleService.MapStatus`):

| Status workflow sesudah aksi | Status permohonan cuti |
| --- | --- |
| `InProgress` (tahapan antara disetujui) | Menunggu Persetujuan, dengan tahapan berjalan ikut diperbarui |
| `Approved` (tahapan terakhir disetujui) | Disetujui, lalu saldo diterapkan oleh `LeaveBalanceLifecycleService` |
| `Rejected` | Ditolak |

**Reference type yang punya handler hari ini:** perubahan profil pegawai, koreksi presensi,
penyesuaian cuti, permohonan cuti, pembatalan cuti, recall cuti, lembur, perubahan jadwal, tukar
shift, resign, dan perencanaan tenaga kerja.

**Jalur tidak normal:**

| Keadaan | Perilaku |
| --- | --- |
| Tombol **Setujui** ditekan dua kali | Klik kedua ditolak HTTP 400 "Tahapan persetujuan belum aktif atau sudah diproses."; tidak ada aksi, riwayat, atau eksekusi hilir ganda |
| Persetujuan massal berisi tiket yang sudah diproses | Tiket itu masuk daftar dilewati dengan alasan yang sama; tiket lain tetap diproses |
| Handler modul asal gagal (misalnya saldo cuti tidak cukup) | Keputusan workflow tetap sah; kegagalan dicatat log oleh `HandleAsync`, sama seperti mesin utama |
| Handler dipanggil ulang untuk workflow yang sama | Menghasilkan keadaan yang sama, karena handler menurunkan status dari workflow, bukan menambah transaksi |
| Reference type tanpa handler | Tidak ada efek (`IsHandled = false`) |
| **Kembalikan untuk revisi** | Sengaja **tidak** diteruskan ke modul asal — lihat bagian 7 |

**Hasil akhir:** permohonan asal mengikuti keputusan atasan pada saat yang sama, dan satu
keputusan hanya tercatat satu kali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/HumanResource/WorkflowManagement/Services/IdempotentDownstreamExecutionService.cs`
- `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowApprovalActionService.cs`
- `Areas/Corporate/HumanResource/WorkflowManagement/Services/BulkApprovalService.cs`
- `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowReferenceLifecycleService.cs`
- `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.ActionsV2.cs` (pola pembanding: penjaga tahapan aktif dan `HandleAsync` sesudah commit)
- `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowInstanceCreationService.cs`, `WorkflowResubmitService.cs` (status awal dan status sesudah resubmit)
- `Areas/Corporate/HumanResource/WorkflowManagement/Models/WorkflowValueConstants.cs`
- `Areas/Corporate/HumanResource/WorkflowManagement/Controllers/WorkflowApprovalActionsController.cs`
- `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveRequestWorkflowLifecycleService.cs`
- `Program.cs` (registrasi DI)
- `contracts/integration-contract.md` §21.1–§21.3, `02-backend-architecture.md` §33.3

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/IdempotentDownstreamExecutionService.cs` | Cabang kosong per reference type diganti penerusan ke `WorkflowReferenceLifecycleService.HandleAsync`. Signature menjadi `ExecuteDownstreamActionsAsync(Guid workflowInstanceId, CancellationToken)`. Aktor diambil sebagai user id dari token (`NameIdentifier`/`user_id`). Penjaga `FinalizedAt − 10 detik` dihapus |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowApprovalActionService.cs` | Penjaga `EnsureStepIsActionable` pada Approve, Reject, dan Return. Eksekusi hilir dipindah ke sesudah `SaveChangesAsync` pada Approve, dan ditambahkan pada Reject |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route, DTO, dan bentuk response tidak berubah. Perilaku baru: aksi atas tahapan yang sudah diproses dijawab HTTP 400 dengan pesan di atas |
| Database | `NOT APPLICABLE` — tidak ada perubahan schema, entity, maupun migration. Handler hilir menulis ke tabel modul asal seperti saat dipanggil mesin utama |
| Keamanan/Auth | Hak akses endpoint tidak berubah (`WorkflowApprovalAction : ApproveRequest`, `RejectRequest`, `ReturnForRevision`). Penjaga baru mencegah keputusan ganda dan pembalikan keputusan akhir |

---

## 4. Dokumentasi endpoint

Endpoint tidak berubah. Perilaku berikut berlaku sesudah task ini.

#### Corporate - HRD Workflow Approval Actions

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/hrd/workflow/approvals/{stepInstanceId}/approve` | Menyetujui tahapan aktif; pada tahapan terakhir, permohonan asal ikut disetujui | `WorkflowApprovalAction : ApproveRequest` |
| `POST` | `api/v1/hrd/workflow/approvals/{stepInstanceId}/reject` | Menolak tahapan aktif; permohonan asal ikut ditolak | `WorkflowApprovalAction : RejectRequest` |
| `POST` | `api/v1/hrd/workflow/approvals/{stepInstanceId}/return` | Mengembalikan tahapan aktif untuk revisi | `WorkflowApprovalAction : ReturnForRevision` |
| `POST` | `api/v1/hrd/workflow/approvals/bulk-approve` | Persetujuan massal; tiap tiket melewati jalur Approve yang sama | `WorkflowApprovalAction : BulkApprove` |

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Keputusan tersimpan dan diteruskan ke permohonan asal |
| `400` | Validasi gagal, termasuk "Tahapan persetujuan belum aktif atau sudah diproses." |
| `403` | Pengguna tidak punya hak akses aksi tersebut |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 578 warning — sama dengan sebelum perubahan | `PASS` | Keluaran build 10 November 2026 |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <2 berkas>` | 2 berkas, 0 violation, 0 review | `PASS` | Keluaran checker |
| Siklus dependency injection | `IIdempotentDownstreamExecutionService`, `IWorkflowApprovalActionService`, dan `IBulkApprovalService` hanya dipakai `WorkflowApprovalActionsController` dan service V2 itu sendiri; tidak ada handler hilir yang bergantung balik | `PASS` | `grep` konsumen di `Areas/` dan `Program.cs` |
| Kesesuaian status tahapan dengan pembuat instance | V2 membuat tahapan pertama `Pending` + `IsCurrentStep`; resubmit mengembalikan `Pending` + `IsCurrentStep`; mesin utama memakai `InProgress` — ketiganya diterima penjaga | `PASS` | `WorkflowInstanceCreationService.cs:117-118`, `WorkflowResubmitService.cs:73-74`, `WorkflowService.ActionsV2.cs:1185` |
| Uji alur cuti end-to-end dengan akun approver (`AT-HRD-WF-18`) | — | `NOT RUN` | Aplikasi tidak dijalankan dan database tidak disentuh |
| Pemicu delegasi Plh (`AT-HRD-WF-19`) | — | `NOT RUN` | Fiturnya belum ada (bagian 6) |

Uji manual: `REQUIRED` — jalankan `AT-HRD-WF-18` dengan akun approver non-SuperAdmin sebelum task
ini ditandai selesai.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Domain hilir terupdate seketika setelah persetujuan akhir | Terpenuhi di kode untuk 11 reference type yang punya handler; belum diuji runtime | `WorkflowApprovalActionService.ApproveAsync` → `IdempotentDownstreamExecutionService` → `WorkflowReferenceLifecycleService.HandleAsync` |
| Eksekusi ulang tidak menyebabkan duplikasi transaksi (idempoten) | Terpenuhi di kode | Penjaga `EnsureStepIsActionable`; handler hilir memakai pola sinkronisasi status |
| Outcome: pelepasan pencairan kasbon/klaim di Sub-11 | Belum terpenuhi — tidak berlaku pada mesin ini | Restitusi, klaim benefit, dan perjalanan dinas memakai endpoint persetujuannya sendiri dan tidak membuat workflow instance, sehingga tidak ada keputusan V2 yang perlu diteruskan |
| Outcome: `INT-HR-94` — pengingat menunjuk Plh saat cuti pejabat struktural disetujui | **Belum terpenuhi** | Tidak ada source. Belum ada penanda pejabat struktural pada data jabatan, dan mekanisme pengingatnya bergantung pada gateway notifikasi `BE-HRD-314` yang masih kosong |

**Ringkasan:** 2 dari 2 acceptance criteria terpenuhi di kode; 1 dari 2 butir outcome
(`INT-HR-94`) belum ada; uji runtime belum dijalankan. Karena itu statusnya `🟡`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | (1) **Return tidak diteruskan ke modul asal.** Service V2 menyimpan status `ReturnedForRevision`, yang tidak ada di `WorkflowValueConstants`. Handler modul memetakan status tak dikenal ke "Menunggu Persetujuan", sehingga meneruskannya justru membuat cuti salah status. Perbaikan kosakata status menjadi wilayah `BE-HRD-315`. (2) **Approver tidak dicocokkan dengan penugasan.** Bila pengguna bukan approver tahapan dan bukan Plh aktif, `ApproveAsync` tetap melanjutkan. Pemeriksaan ini tidak bisa ditambahkan sekarang karena `WorkflowApprovalActionsController` membaca klaim `"EmployeeId"` yang tidak pernah ada di token, sehingga identitas yang dibandingkan selalu user id. Menunggu keputusan identitas pegawai (temuan nomor 5 pada `evidence/03-hr-authorization-remediation.md`) |
| Risiko tersisa | Dua approver yang menekan tombol pada milidetik yang sama masih dapat lolos penjaga, karena tidak ada token konkurensi pada tahapan. Mesin utama memakai transaksi dan idempotency key; V2 belum |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` dua service di atas, `M` laporan ini, `M` roadmap, `M` `requirement-traceability.md`, `??` `evidence/03-hr-authorization-remediation.md` (dari pekerjaan sebelumnya) |
| Langkah berikutnya | `BE-HRD-314` (gateway notifikasi), lalu `INT-HR-94` di atasnya; uji `AT-HRD-WF-18` dengan akun approver nyata |

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (`INT-HR-94`)

**Masalah yang ditutup:** butir outcome `INT-HR-94` (pengingat menunjuk Plh saat cuti pejabat struktural disetujui) belum punya source karena data jabatan tidak punya penanda "pejabat struktural" dan gateway notifikasi masih kosong.

**Keputusan yang diterapkan:** "pejabat" diturunkan dari bukti yang ada — pemohon cuti yang tercatat sebagai manajer aktif dengan wewenang menyetujui permohonan minimal satu bawahan (`WfpManagerAssignment.CanApproveRequests`). Tidak ada penanda baru pada data jabatan.

**Proses:** persetujuan final V2 → `IdempotentDownstreamExecutionService` meneruskan ke modul cuti (sudah ada) → bila reference type-nya cuti, status `Approved`, pemohon adalah manajer aktif, dan belum ada `TrxApprovalDelegation` aktif yang menutup rentang cuti → pejabat menerima pemberitahuan prioritas tinggi "Cuti Anda disetujui — tunjuk Plh" dengan tautan ke layar pembuatan delegasi (satu kali per permohonan cuti). Kegagalan pengingat tidak membatalkan keputusan yang sudah sah.

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/IdempotentDownstreamExecutionService.cs` | Metode `PromptOfficerLeaveDelegationAsync` (`INT-HR-94`); konstruktor menerima `ApplicationDbContext`, `IHrdNotificationService`, `ILogger` |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Outcome `INT-HR-94` kini ada di kode; `AT-HRD-WF-18`/`AT-HRD-WF-19` tetap `NOT RUN`; tabel pemberitahuan menunggu migration. Status tetap 🟡. Temuan lama "approver tidak dicocokkan dengan penugasan" kini dapat ditindaklanjuti karena controller V2 sudah membaca klaim `employee_id` (kelompok G), tetapi pemeriksaannya belum ditambahkan pada task ini.


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.


## Pembaruan 11 November 2026 — Penuntasan Temuan

Penyetuju kini wajib approver yang ditugaskan pada tahapan atau Plh aktifnya; selain itu ditolak `403`.

`dotnet build` 0 error, 602 warning. QBE Strict `PASS`. Aplikasi dijalankan pada 11 November 2026: seluruh worker berjalan tanpa galat query dan lima dokumen Swagger berhasil dibuat. Uji API dengan akun sungguhan `NOT RUN`.
