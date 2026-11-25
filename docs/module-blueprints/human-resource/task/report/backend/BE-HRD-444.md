# Laporan Perubahan Backend — `BE-HRD-444`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-444` |
| Judul | Aksi Setujui/Tolak/Kembalikan Kotak Masuk Terpadu diteruskan ke engine; PIN dihapus |
| Slice | `S-A7-V2` amandemen / `MVP-72` gelombang 3 · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 29, kartu `BE-HRD-444` |
| Trace | `FR-WF-27`, `FR-WF-28`; `HRD-DEC-229`, `HRD-DEC-230` |
| Contract version | `v29` — API 35.2, 35.7; Validation `VR-WF-26` s.d. `VR-WF-28`, `VR-WF-30`; State 33.1 — approved 20 November 2026 |
| Dependency | `BE-HRD-441` ✅ (otorisasi Plh di engine); memakai himpunan penugasan `BE-HRD-443` ✅ |
| Klasifikasi | `STANDARD` — adapter service, satu method kueri, controller, DTO; tanpa perubahan skema |
| Task mode | `BACKEND` (instruksi pengguna 23 November 2026: "lanjutkan langkah berikutnya sesuai saran ini") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `8a68ab67` (branch `yoga`, working tree bersih sebelum task) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi; uji API lokal akun dr. Bima tertahan (sandi ditolak) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkflowManagement` (registry: `SHARED PLATFORM CAPABILITY`, prefix `Wfl`, `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — `WorkflowApprovalActionService` (ditulis ulang menjadi adapter), `WorkflowApprovalActionsController` (tiga action), `WorkflowV2Dtos` (tiga DTO aksi), `ApprovalInboxQueryService` (satu method baru), `BulkApprovalService` (penyesuaian tanda tangan method saja) |
| QBE yang berlaku | `QBE-SVC-001` (aturan siapa boleh bertindak dan validasi alasan berada di service; controller hanya meneruskan akun dan memetakan kode status), `QBE-API-001` (`ApiResponse`, kode status kontrak §35.7), `QBE-PERM-001` (atribut akses tidak berubah). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. `<summary>` XML lama pada ketiga action dihapus karena isinya usang (PIN, 15 karakter); tidak ada `<summary>` baru pada action |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh: Sari (perawat ICU) mengajukan SPPD. Engine `WorkflowService` membuat tugas persetujuan untuk akun dr. Bima. Tugas itu tampil di Kotak Masuk Terpadu dengan `stepInstanceId` (`BE-HRD-443`).

| Langkah | Yang terjadi |
| --- | --- |
| dr. Bima menekan Setujui (`POST /approvals/{stepInstanceId}/approve`, `note` opsional) | Adapter mencari penugasan terbuka pada tahap itu yang boleh dijalankan dr. Bima, dengan aturan yang sama dengan antrean dan `myAction`. Adapter lalu memanggil `WorkflowService.ApproveAsync`. Engine menutup tahap, instance menjadi `Completed`, dan penangan SPPD menjadikan SPPD `Approved` beserta presensi Dinas Luar. Sari diberi tahu |
| dr. Bima menekan Tolak tanpa alasan | `400` "Pilih alasan penolakan yang berlaku untuk permohonan ini." Tidak ada yang berubah |
| dr. Bima memilih alasan yang mewajibkan catatan, tanpa catatan | `400` "Alasan ini mewajibkan catatan." |
| dr. Bima memilih alasan dan mengisi catatan | `WorkflowService.RejectAsync`; SPPD `Rejected`. Alasan tercatat di jejak aksi (`ActionReasonId`) |
| dr. Bima menekan Kembalikan tanpa catatan / dengan catatan | `400` "Isi catatan revisi untuk pemohon." / `RequestRevisionAsync`, SPPD `NeedRevision` |
| Wakil Karu (Plh aktif dr. Bima) menekan Setujui | Diteruskan ke engine atas penugasan dr. Bima; jejak mencatat pelaku Wakil Karu, `IsDelegated`, dan pejabat definitif |
| Orang lain menekan Setujui | `403` "Anda bukan penyetuju tahap ini." Sari sendiri: `403` "Pejabat tidak boleh menyetujui permohonan sendiri." |
| Tombol ditekan dua kali | Kali kedua `409`; tidak ada keputusan ganda |
| Klien lama masih mengirim `pinSignature` | Ruas itu diabaikan; aksi tetap `200`. `DigitalSignatureHash` tidak pernah diisi |

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowApprovalActionService.cs` | Ditulis ulang menjadi adapter. Tanda tangan baru: `ApproveAsync`/`RejectAsync`/`ReturnForRevisionAsync(stepInstanceId, request, currentUserId, ct)` → `WorkflowServiceResult<WorkflowActionV2ResponseDto>`. Penugasan dipilih lewat `IApprovalInboxQueryService.FindActionableAssignmentAsync`. Bila tidak ada, jawaban dibedakan: `404` tahap tidak ada; `403` pemohon; `409` penugasan pengguna sudah diproses atau permohonan tidak lagi menunggu; `403` selain itu. Tolak: alasan dinilai dari daftar `WorkflowRejectionReasonOptions` (sama dengan `myAction.rejectionReasons`), catatan wajib bila `IsCommentRequired`. Kembalikan: catatan wajib, diteruskan sebagai `comment`. Kode status dan pesan engine diteruskan; penolakan otorisasi engine memakai pesan kontrak §35.7. Pemberitahuan WF-B7 (pemohon; penyetuju tahap berikutnya) dikirim sesudah engine berhasil; kegagalannya hanya dicatat. **Dihapus**: ubah status langsung, riwayat status/komentar buatan sendiri, `ComputeSignatureHash`, pemanggilan `IdempotentDownstreamExecutionService`, validasi 15/10 karakter |
| `…/Services/ApprovalInboxQueryService.cs` | Method baru `FindActionableAssignmentAsync(stepInstanceId, userId)` dan kelas `WorkflowActionableAssignment`. `LoadOpenRowsAsync` menerima saringan tahap opsional. Aturan pemilihan tidak berubah |
| `…/Controllers/WorkflowApprovalActionsController.cs` | Tiga action meneruskan akun lewat `User.GetUserId()` dan memetakan hasil ke kode status (`ToActionResult`). `ProducesResponseType` 400/403/404/409. Setujui Massal meneruskan akun ke `BulkApprovalService` |
| `…/DTOs/WorkflowV2Dtos.cs` | `ApproveActionRequestDto`: `note` (maks 1000), `pinSignature` dihapus. `RejectActionRequestDto`: `rejectionReasonId`, `comment` (maks 1000); `rejectReason` dan `pinSignature` dihapus. `ReturnForRevisionRequestDto`: `revisionNotes` (maks 1000), tanpa panjang minimum |
| `…/Services/BulkApprovalService.cs` | Tanda tangan `ExecuteBulkApprovalAsync(request, currentEmployeeId, currentUserId)`; butir memanggil adapter baru tanpa PIN butir. Kelayakan `VR-WF-32` dan PIN batch **tidak diubah** (lingkup `BE-HRD-445`) |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkflowApprovalActionAdapterTests.cs` (baru), `AcceptanceTestIndex.md` | 12 uji (10 fakta + 1 teori 3 kasus) lewat controller dengan engine nyata dan penangan SPPD; satu baris indeks |

### Delta terhadap kartu roadmap

| Hal | Kartu | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Penentu penugasan | Helper `BE-HRD-441` | Method baru `ApprovalInboxQueryService.FindActionableAssignmentAsync`, yang memakai aturan delegasi `BE-HRD-441` | Antrean, `myAction`, dan aksi memakai satu kueri, sehingga butir yang tampil di antrean selalu dapat dijalankan dengan penugasan yang sama. Otorisasi akhir tetap diperiksa engine (`VR-WF-30`) |
| `BulkApprovalService.cs` | Tidak termasuk cakupan | Disentuh dua baris | Tanda tangan adapter berubah. Tanpa penyesuaian proyek tidak terkompilasi. Isi massal tetap untuk `BE-HRD-445` |
| Validasi alasan | "Diperiksa engine" | Adapter memeriksa lebih dulu dengan daftar alasan yang sama dengan engine dan `myAction`; engine tetap memeriksa ulang | Pesan engine ("RejectionReasonId wajib diisi.", "Komentar wajib diisi…") berbeda dari pesan kontrak §35.7. Engine tidak diubah agar kotak masuk lama tetap sama |
| Pemberitahuan pemohon | Tidak disebut | Dipertahankan di adapter | Engine tidak mengirim pemberitahuan. Tanpa ini perilaku WF-B7 (`BE-HRD-314`) di kotak terpadu hilang |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `WorkflowApprovalActionAdapterTests` | 12/12 lulus pada putaran pertama | `PASS` (AC1–AC4) | Keluaran `dotnet test` |
| Kontrol negatif: 12 mutasi (alasan tidak divalidasi; catatan wajib diabaikan; catatan revisi tidak dicek; tahap tidak disaring; pemohon tidak dibedakan; penugasan sudah diproses tidak dibedakan; engine dipanggil dengan penugasan lain; controller selalu `400`; PIN kembali di DTO; penugasan engine lama ikut dijalankan; pemohon tidak diberi tahu; Plh tidak dipakai) | 12/12 tertangkap, tanpa galat kompilasi; source dipulihkan otomatis | `PASS` | `mut444.mjs` di scratchpad sesi |
| Regresi penuh proyek uji HR | 416/416 lulus (11 menit 38 detik), dengan env `QUILVIAN_BACKEND_ROOT`. Dijalankan dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085` | `PASS` | Keluaran `dotnet test` |
| Build proyek utama | 0 galat (`-p:RunAnalyzers=False`) | `PASS` | Keluaran `dotnet build` |
| QBE `Strict` atas 6 berkas | 0 violation, 0 review; probe `TrxZzzQbeProbe444` terdeteksi 3 violation (`QBE-NAM-001`, `QBE-CFG-001`, `QBE-MOD-002`) lalu dihapus | `PASS` | Keluaran checker |
| Uji API lokal akun dr. Bima (SPPD `0009`) | Backend lokal hidup (`/health` `200`), tetapi login `bima.kepala.lab@quilvian.local` ditolak `401` ("Password salah" di log). Tidak dicoba ulang agar akun tidak terkunci | `BLOCKED` | Log backend lokal |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Setujui SPPD dari endpoint kotak terpadu → instance `Completed`, SPPD `Approved` dengan presensi Dinas Luar (`AT-HRD-WF-24`) | Terpenuhi | `Setujui_SPPD_dari_kotak_terpadu_lewat_engine_menyelesaikan_SPPD_dan_presensi` |
| 2. Tolak tanpa alasan / alasan wajib catatan tanpa catatan / lengkap → `400`, `400`, `200` (`AT-HRD-WF-25`); Kembalikan tanpa/dengan catatan → `400`/`200`, SPPD `NeedRevision` (`AT-HRD-WF-26`) | Terpenuhi | `Tolak_wajib_alasan_berkode_dan_catatan_bila_dituntut`, `Tolak_dengan_alasan_tanpa_tuntutan_catatan_boleh_tanpa_catatan`, `Kembalikan_wajib_catatan_dan_SPPD_menjadi_NeedRevision` |
| 3. Bukan penyetuju → `403` tanpa perubahan (`AT-HRD-WF-27`) | Terpenuhi | `Bukan_penyetuju_atau_pemohon_ditolak_403_tanpa_perubahan` (3 kasus × 3 aksi), `Plh_dengan_delegasi_berakhir_ditolak_403`, `Penugasan_engine_lama_tidak_dijalankan_kotak_terpadu`; Plh aktif: `Plh_aktif_menyetujui_atas_nama_pejabat_lewat_kotak_terpadu` |
| 4. Tanpa PIN dan dengan PIN acak → `200`; `DigitalSignatureHash` kosong (`AT-HRD-WF-28`) | Terpenuhi | `Tanpa_PIN_dan_dengan_PIN_acak_disetujui_tanpa_tanda_tangan` |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: adapter lama mengubah status instance/tahap sendiri, mencari penugasan berstatus `Pending` + `ApproverEmployeeId`, dan memanggil eksekusi hilir terpisah | Keputusan dari kotak terpadu tidak menjalankan penangan modul engine. SPPD dan cuti engine tidak pernah berubah status dari layar ini | Diperbaiki (inti task) |
| As-is: tahap berikutnya dibuat adapter lama dengan SLA 48 jam tetap dan penyetuju "skip-level" | Menyimpang dari definisi workflow | Hilang; tahap berikutnya kini dibuka engine sesuai definisi |
| Engine mengizinkan pemegang penugasan yang juga pemohon (pemeriksaan pemohon hanya untuk Plh, `BE-HRD-441`) | Hanya terjadi bila seseorang menjadi penyetuju atas permohonannya sendiri; pembuatan penugasan engine tidak menghasilkan kasus itu pada data uji | Tidak diubah. Adapter mengikuti engine agar `myAction` dan aksi tidak saling berbeda |
| Klien yang masih mengirim `rejectReason` teks menerima `400` | Layar Kotak Masuk Terpadu perlu memakai `rejectionReasonId` dari `myAction.rejectionReasons` | Risiko kartu; frontend diperbarui di `FE-HRD-394`/`395` |
| `BulkApprovalService` masih meminta PIN batch dan menilai kelayakan cara lama | Setujui Massal belum mengikuti `VR-WF-32` | Lingkup `BE-HRD-445` |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Tidak ada eksekusi database. Uji API tidak mengubah data, karena login tertahan sebelum permintaan apa pun |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `WorkflowApprovalActionAdapterTests.cs` (cukup `git add`) |
| Dampak frontend | Kontrak request berubah: Tolak memakai `rejectionReasonId` + `comment`, PIN tidak dikirim. Layar dapat memakai `myAction` untuk aksi dan alasan |
| Uji API dr. Bima | Skrip siap (`api444.mjs`, hanya permintaan yang tidak mengubah data). Butuh sandi akun `bima.kepala.lab@quilvian.local` yang berlaku |
| Task berikutnya | `BE-HRD-445` (Setujui Massal lewat adapter ini, tanpa PIN, kelayakan `VR-WF-32`); `BE-HRD-446` (SLA/worker) dapat dikerjakan paralel |
