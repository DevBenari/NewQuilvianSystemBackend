# Laporan Perubahan Backend — `BE-HRD-029`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-029` |
| Judul | Penelusuran Unit Induk pada Matriks Persetujuan untuk Langkah Direktur |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-7` (Fondasi, Formasi, dan Batch Formasi Awal) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-7 |
| Traceability | `HRD-DEC-072`, `HRD-DEC-064`, `FR-HRD-116`, `AT-HRD-D1-20`, `INT-HR-18`, `02-backend-architecture.md` §16.4.9 |
| Dependency | `BE-HRD-005` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | `WorkflowService.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & seluruh kriteria bisnis terpenuhi |

---

## 1. Masalah yang Diselesaikan

Sebelum perubahan ini, pencocokan aturan matriks persetujuan (`MstApprovalMatrix`) pada mesin workflow terpadu (`WorkflowService.cs`) bersifat kaku dan hanya mencocokkan `OrganizationUnitId` secara persis sama (`x.OrganizationUnitId == context.OrganizationUnitId`). Hal ini menimbulkan kendala fatal bagi alur hierarki persetujuan rumah sakit:
1. Ketika Kepala Ruang ICU mengajukan usulan penambahan formasi perawat, sistem tidak dapat menemukan penyetuju langkah Direktur Pelayanan Medis, karena matriks persetujuan direktur umumnya dipasang pada level Direktorat Pelayanan Medis, bukan pada level unit teknis ICU. Akibatnya pengajuan gagal atau memerlukan duplikasi konfigurasi matriks ke ratusan unit rawat.
2. Jika posisi Direktur pembina sedang lowong/kosong, mesin workflow sebelumnya langsung gagal tanpa mekanisme eskalasi cadangan (*fallback*) ke Direktur Utama.
3. Pergantian direktur sering kali membutuhkan perubahan manual pada konfigurasi matriks persetujuan jika approver dipetakan ke identitas personal pegawai (*SpecificUser*).

Melalui task `BE-HRD-029`, aturan `HRD-DEC-072` dan `HRD-DEC-064` resmi diterapkan pada `WorkflowService.cs` tanpa mengubah skema tabel database dan tanpa mengganggu kompatibilitas alur persetujuan modul lainnya (`T1` s.d. `T8`).

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

* **Contoh Skenario Rumah Sakit**:
  - Kepala Ruang ICU (unit teknis pelayanan) mengajukan usulan formasi tambahan 2 Perawat Pelaksana di luar formasi reguler.
  - Alur persetujuan membutuhkan persetujuan Direktur yang membawahi unit tersebut (Langkah Direktur pada `T9` dan `T12`).
  - **Penelusuran Hierarki Otomatis**: Sistem secara cerdas menelusuri rantai organisasi ke atas:
    $$\text{Unit ICU} \longrightarrow \text{Departemen Keperawatan} \longrightarrow \text{Direktorat Pelayanan Medis}$$
  - Sistem menemukan baris matriks persetujuan yang terdaftar pada Direktorat Pelayanan Medis dengan peran jabatan (*Position*) "Direktur Pelayanan Medis".
  - **Dinamika Pemegang Jabatan**: Penyetuju otomatis ditetapkan kepada pegawai yang memiliki penempatan aktif (`WfpOrganizationAssignment`) pada jabatan Direktur Pelayanan Medis per tanggal pengajuan. Jika terjadi rotasi/pergantian direktur melalui SK Kepegawaian (`HrdPersonnelActionOrder`), penyetuju pada sistem otomatis berganti tanpa perlu menyunting satu pun baris konfigurasi matriks.
  - **Penanganan Jabatan Lowong & Fallback**: Jika kursi jabatan Direktur Pelayanan Medis sedang kosong, sistem secara otomatis mengevaluasi baris cadangan (`IsFallback = true`) yang menugaskan Direktur Utama. Jika kedua jabatan tersebut kosong, pengajuan ditolak secara aman (*fail closed*) dengan status `400 Bad Request` (`UNRESOLVED_STEP_FAILS_CLOSED`) dan langkah persetujuan tidak pernah dilewati secara diam-diam.

---

## 3. Rincian Perubahan Teknis (`WorkflowService.cs`)

### 3.1 Fungsi Penelusuran Rantai Unit Induk (`GetOrganizationUnitAncestorChainAsync`)
Menambahkan metode asinkron yang menelusuri rantai `ParentOrganizationUnitId` pada `MstOrganizationUnit` dari unit pemohon hingga level puncak direktorat/rumah sakit:
* Menghasilkan koleksi berurutan: `[Unit Pemohon (jarak 0), Unit Induk (jarak 1), Unit Kakek (jarak 2), ...]`.
* Dilengkapi pencegahan *circular reference* menggunakan set memori `visited`.

### 3.2 Pemeringkatan dan Seleksi Matriks Persetujuan Berpasangan (`SelectApprovalMatrixPair`)
Memperbarui logika seleksi matriks persetujuan agar mengenali rantai unit induk:
* **Evaluasi Kecocokan Unit**: Menggunakan fungsi pembantu `MatchesOrganizationUnit` yang memastikan baris matriks cocok jika unitnya berada dalam rantai hierarki pemohon.
* **Prioritas Jarak Terdekat (Kriteria 2)**: Menggunakan fungsi `GetUnitDistance` sehingga baris matriks pada unit yang lebih dekat menang mutlak atas baris unit induknya (jarak 0 menang atas jarak 1, jarak 1 menang atas jarak 2, dan jarak hierarki menang atas matriks umum/tanpa unit).
* **Pemisahan Pasangan Matriks Primer & Fallback**: Mengembalikan tuple `(MstApprovalMatrix? Primary, MstApprovalMatrix? Fallback)` dalam satu putaran evaluasi yang efisien.

### 3.3 Logika Eksekusi Fallback Otomatis pada `StartInstanceAsync`
Pada loop pembuatan instans langkah persetujuan (`workflowSteps`):
* Pertama, sistem mencoba mengevaluasi approver berdasarkan `PrimaryMatrix`.
* Jika approver primer tidak ditemukan (`resolvedApprovers.Count == 0`) — misalnya karena jabatan direktur sedang lowong — dan tersedia `FallbackMatrix`, sistem secara otomatis melakukan resolusi approver terhadap `FallbackMatrix` (Direktur Utama).
* Jika approver fallback ditemukan, instans langkah persetujuan diperbarui (`stepInstance.ApprovalMatrixId = fallbackMatrix.Id`) dan penugasan approver dialihkan ke Direktur Utama.
* Jika approver primer dan fallback sama-sama kosong, transaksi dibatalkan (*rollback*) dan mengembalikan `400 Bad Request`.

### 3.4 Dukungan Penelusuran Unit Induk pada Definisi Workflow (`ResolveWorkflowDefinitionAsync`)
Memperbarui query kandidat definisi workflow agar dapat mencocokkan workflow yang didefinisikan pada level unit induk, serta memprioritaskan unit yang paling dekat dengan pemohon pada fungsi `GetDefinitionScopeRank`.

### 3.5 Pemetaan Approver Berbasis Posisi Unit (`ResolveApproversByMatrixAsync`)
Pada penanganan `ApproverSource.Position`, menyelaraskan pemanggilan `ResolveByOrganizationAssignmentAsync` dengan meneruskan `matrix.ApproverOrganizationUnitId` (bukan hardcoded `null`), sehingga konsisten dengan logika resolusi posisi di tingkat step.

---

## 4. Evaluasi Kriteria Penerimaan (Acceptance Criteria)

| No | Kriteria Penerimaan | Status | Bukti Implementasi pada Kode |
| :---: | :--- | :---: | :--- |
| 1 | Baris matriks untuk sebuah direktorat berlaku bagi seluruh unit di bawahnya lewat penelusuran unit induk | **TERPENUHI** | `GetOrganizationUnitAncestorChainAsync` menghasilkan rantai unit, `MatchesOrganizationUnit` mencocokkan unit ancestor. |
| 2 | Baris untuk unit yang lebih dekat menang atas baris unit induknya | **TERPENUHI** | `OrderBy(x => GetUnitDistance(x.OrganizationUnitId, unitAncestorChain)).ThenByDescending(x => x.Priority)`. |
| 3 | Penyetuju adalah pemegang penempatan jabatan direktur aktif per tanggal pengajuan | **TERPENUHI** | `ResolveByOrganizationAssignmentAsync` memfilter `WfpOrganizationAssignments` berdasarkan tanggal aktif (`EffectiveStartDate <= now && EffectiveEndDate >= now`). |
| 4 | Jabatan direktur kosong naik ke pemegang jabatan Direktur Utama via `IsFallback` | **TERPENUHI** | Jika `resolvedApprovers.Count == 0`, sistem mencoba `fallbackMatrix` secara transparan. |
| 5 | Keduanya kosong → pembuatan instance ditolak `400`; langkah tidak dilewati | **TERPENUHI** | Rollback transaksi dan return `400 BadRequest` (`UNRESOLVED_STEP_FAILS_CLOSED`). |
| 6 | Rantai T1 s.d. T8 dan workflow lain berperilaku sama seperti sebelumnya | **TERPENUHI** | Metode overload `SelectApprovalMatrix` backward-compatible tetap dipertahankan; tanpa matriks ancestor, perilaku identik dengan baseline. |

---

## 5. Hasil Verifikasi dan Kepatuhan

1. **Review Diff Kode**:
   - Seluruh perubahan terisolasi 100% pada satu berkas: `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs`.
   - Tidak ada modifikasi skema database (tanpa migration baru, tanpa tabel/kolom baru).
2. **Kompilasi Solusi (`dotnet build`)**:
   - Status: **BERHASIL (`Build succeeded`)**
   - Error: **0 Error**
   - Warning: 414 Warning (seluruhnya pre-existing XML comment pada modul lain).

---

## 6. Status dan Langkah Selanjutnya

* **Status Task**: Selesai (`PASS`) dan siap digunakan oleh seluruh alur persetujuan dokumen perencanaan tenaga kerja `S-D1` (`T9` s.d. `T13`).
* **Task Siap Dikerjakan Berikutnya**:
  - `BE-HRD-030` — Lima jenis rujukan workflow dan layanan integrasi persetujuan `S-D1` (`HrdWorkforcePlanningWorkflowIntegrationService`).
  - `BE-HRD-031` — Pengaturan perencanaan per rumah sakit (`HrdWorkforcePlanningSettingService`).
  - `BE-HRD-032` — Buku formasi berversi: satu-satunya penulis formasi dan API baca (`HrdPositionFormationService`).
