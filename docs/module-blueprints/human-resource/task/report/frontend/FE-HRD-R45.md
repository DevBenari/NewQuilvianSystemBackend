# Laporan Perubahan Frontend — `FE-HRD-R45`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R45` |
| Judul | Verifikasi layar operasional |
| Slice | `S-A7-V2` (Persetujuan Terpadu), `S-D5` (Perjalanan Dinas), `S-D4` (Layanan SDM), `S-B3` (Lembur); Gelombang 4 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R45`; task asal `FE-HRD-290`, `305`, `185`, `129` |
| Trace | Kontrak bagian 30/31, v13, v17; laporan `FE-HRD-R31`, `R33`, `R34`, `R35` |
| Contract version | Kontrak slice + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Uji browser P0/P1, termasuk satu penulisan yang dapat dipulihkan (disetujui pemilik 18 Nov 2026) |
| Dependency | `FE-HRD-R31`, `FE-HRD-R33` 🟡, `FE-HRD-R34` 🟡, `FE-HRD-R35` 🟡 |
| Klasifikasi | `STANDARD` — verifikasi 26 rute + uji tulis delegasi; 0 berkas source diubah |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | Laporan ini, laporan uji live, tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f095e2e0a` → `f7a1f0917` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `604cac6a` → `fcdd9f4d` (branch `MHamzah`) |
| Tanggal | 18 November 2026 |
| Status | ⛔ Terblokir — blocker `BE: ApprovalDelegationV2Service tidak mengisi cakupan delegasi (CK_TrxApprovalDelegation_WorkflowScope)`. Penerbitan surat kuasa Plh selalu HTTP 500, sehingga AC "keputusan tetap tersimpan setelah dimuat ulang" tidak dapat dibuktikan. Seluruh 26 layar termuat, dan simulasi kegagalan server lulus. |

---

## 1. Keadaan yang ditemukan di awal

- 26 layar sudah memakai API nyata (`R21`–`R23`, `R31`, `R33`–`R35`), tetapi keputusan lintas peran belum pernah dibuktikan di browser.
- Pemilik mengizinkan **satu** penulisan yang dapat dipulihkan, yaitu delegasi Plh: terbitkan → muat ulang → cabut.

## 2. Proses bisnis dari sisi pengguna

1. Penyetuju (Mira Safitri) yang akan berhalangan membuka **Pendelegasian Wewenang (Plh)** lalu menekan **+ Tunjuk Plh**.
2. Ia memilih pengganti (Amanda Fitriani), masa berlaku 18–20 Nov 2026, cakupan "Semua jenis permohonan", dan menulis alasan minimal 10 karakter.
3. **Terbitkan Surat Kuasa** → server harus menyimpan, lalu setelah muat ulang surat kuasa tetap tampil, dan **Cabut** memindahkannya ke riwayat.
4. **Yang terjadi:** server menjawab HTTP 500 dengan pesan teknis EF berbahasa Inggris. Tidak ada data tersimpan, sehingga langkah muat ulang dan cabut tidak dapat dijalankan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

26 rute R45, `CreateDelegationModal.jsx`, `unified-approval-service.js`, `approval-constants.js`, `WorkflowDelegationsController.cs`, `ApprovalDelegationV2Service.cs` (sekitar baris 67–84), migration `20260730050557_initializeTrxWorkflowApproverAssignments` (constraint `CK_TrxApprovalDelegation_WorkflowScope`), dan log backend (`PostgresException 23514`).

### 3.2 Berkas yang berubah

Tidak ada source frontend yang berubah. Frontend mengirim payload yang benar (`delegatedRequestTypes: "ALL"`); kesalahan ada di backend.

### 3.3 Kepatuhan arsitektur frontend

`NOT APPLICABLE`.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` untuk komponen baru. Screenshot:

| Langkah | Berkas |
| --- | --- |
| Daftar awal | `test-with-agy/screenshots/hrw4-1118/R45-tulis/01-daftar-awal.png` |
| Validasi alasan pendek | `.../R45-tulis/02-validasi-alasan-pendek.png` |
| Formulir lengkap | `.../R45-tulis/03-formulir-lengkap.png` |
| Penerbitan ditolak (500) | `.../R45-tulis/04-setelah-terbit.png` |
| 26 layar admin / non-admin / simulasi | `.../R45-admin/`, `.../R45-nonadmin/`, `.../R45-kegagalan-server/` |

`VISUAL CHECK: PASS`

## 4. State yang ditangani di layar

| State | Bukti |
| --- | --- |
| Validasi formulir Plh | Tombol nonaktif saat kosong; alasan kurang dari 10 karakter ditolak |
| Gagal simpan | Pesan server tampil di dalam dialog, dan dialog tetap terbuka sehingga isian tidak hilang. Isi pesannya teknis karena berasal dari backend (LBT-W4-OP-02) |
| Gagal muat `500`/`403` | 8/8 pesan ramah |
| Tanpa hak | 17 layar non-admin |

## 5. Endpoint yang dikonsumsi

| Method | Path | `[Tags]` | Hasil |
| --- | --- | --- | --- |
| POST | `/api/v1/hrd/workflow/delegations` | `Corporate - HRD Workflow Delegations` | **500**, constraint `CK_TrxApprovalDelegation_WorkflowScope` |
| PUT | `/api/v1/hrd/workflow/delegations/{id}/revoke` | sama | Tidak dijalankan |

Rincian lengkap ada di [laporan uji live](../../testing/test-by-agy/laporan-live-testing-gelombang4-operasional.md#4-endpoint-yang-diuji).

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Sapu admin 26 rute | 26/26 (1 `404` portal ESS superadmin tertangani) | `PASS` | `R45-admin/hasil.jsonl` |
| Sapu non-admin 26 rute | Tanpa crash; 17 pesan tanpa hak | `PASS` | `R45-nonadmin/hasil.jsonl` |
| Simulasi `500`/`403` (4 layar × 2) | 8/8 ramah | `PASS` | `R45-kegagalan-server/` |
| Validasi formulir Plh (3 langkah) | Sesuai aturan | `PASS` | `R45-tulis/02`, `03` |
| Terbitkan → muat ulang → cabut | 500 pada langkah terbitkan | `BLOCKED` | `R45-tulis/04`, log `be-run2.log` |
| Data uji tertinggal | `TrxApprovalDelegation` alasan "Uji Gelombang 4%" = 0 baris (SELECT baca-saja) | `PASS` | Bagian 8 laporan uji live |
| `npm run build` HEAD `f7a1f0917` | **FAIL** di berkas di luar cakupan (`getTodayDate`) | `FAIL (di luar cakupan)` | `scratchpad/build4.log` |

`MANUAL TEST: BLOCKED — POST delegasi 500 (backend).`
`AUTOMATED TEST: SKIPPED (opsional) — tidak ada perubahan source frontend.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Keputusan tetap tersimpan setelah halaman dimuat ulang | ⛔ Terblokir backend | LBT-W4-OP-01 |
| Keputusan lain (setujui V2, settlement, tiket, verifikasi lembur) | Belum diuji | Di luar izin penulisan |
| Laporan dan status roadmap | ✅ | Laporan ini |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Blocker | `ApprovalDelegationV2Service` membuat `TrxApprovalDelegation` dengan `DelegatedRequestTypes = "ALL"` tanpa `AppliesToAllWorkflows = true` (atau `WorkflowDefinitionId`/`ScopeDefinitionJson`), sehingga melanggar `CK_TrxApprovalDelegation_WorkflowScope` |
| Masalah yang diketahui | LBT-W4-OP-02: pesan EF bocor ke pengguna; LBT-W4-OP-03: menu tanpa `requiredPermission` terlihat oleh non-admin |
| Dependency backend | Perbaikan LBT-W4-OP-01 (task backend baru) |
| Perubahan sampingan | `NONE` |
| Data uji tertinggal | Tidak ada |
| Langkah berikutnya | Setelah backend diperbaiki, jalankan ulang `node test-with-agy/scripts/hrw4-1118-r45-delegation.mjs` dengan akun penyetuju; uji keputusan lain setelah izin data uji |
