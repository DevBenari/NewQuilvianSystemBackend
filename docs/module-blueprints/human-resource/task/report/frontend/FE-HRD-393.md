# Laporan Perubahan Frontend — `FE-HRD-393`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-393` |
| Judul | Perbaikan aksi kotak masuk lama untuk penugasan Workflow V2 (SPPD) |
| Slice | `S-D5` amandemen / Verifikasi — perbaikan temuan `FE-HRD-340` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 28, kartu `FE-HRD-393` (didaftarkan 20 November 2026) |
| Trace | `FR-TRV-34`; `UAT-TRV-07` s.d. `UAT-TRV-09`; `HRD-DEC-018` |
| Contract version | API bagian 6 "Approval Inbox" (as-is, approved): `GET /{assignmentId}`, `POST /{assignmentId}/approve|reject|request-revision` |
| Wewenang | Keputusan pengguna 20 November 2026: "Perbaiki kotak masuk lama (Recommended)" |
| Wewenang UI | Pola modal yang ada (`ConfirmModal` + `BaseSelectField` + `BaseTextAreaField`); `DEV_DISCRETION` untuk label isian dan teks keterangan |
| Dependency | — (endpoint as-is) |
| Klasifikasi | `STANDARD` — satu view, satu util; uji unit + uji browser dengan data asli |
| Task mode | `FRONTEND` |
| Target tulis | Frontend `QuilvianSystemFrontendDev` (2 berkas diubah, 1 berkas uji baru); laporan dan roadmap di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `564365595` (branch `YogaV2`) + working tree `FE-HRD-338`/`339` dan task ini |
| Commit backend yang dijadikan rujukan | `815ce085` (branch `yoga`); uji browser memakai biner 19 November 15.53 |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi, dibuktikan di browser sebagai dr. Bima terhadap SPPD uji `UJI-340` |

---

## 1. Masalah yang ditemukan (`FE-HRD-340`)

Di `/hr/workflow/approval-inbox`, SPPD yang menunggu atasan memang tampil, tetapi:

| Gejala | Sebab |
| --- | --- |
| Setujui, Revisi, dan Tolak semuanya `404` | View mengirim `item.id`, sedangkan `ApprovalInboxItemResponse` tidak punya `id`; ruasnya `assignmentId`. Permintaan menjadi `approval-inbox/undefined/approve` |
| Kolom pemohon dan perihal "-" | View membaca `requesterDisplayName`, `title`, `referenceNumber`; DTO memakai `requestedByName`, `workflowName`, `externalReferenceNumber`/`requestNumber` |
| Kartu ringkasan 0 dan paging "0 dari 0 data" padahal ada baris | View membaca `pendingCount`, `totalCount`; DTO memakai `open`, `overdue`, `dueToday`, `approvedToday`, dan `PagedResult` memakai `totalData`, `totalPage` |
| Tolak tidak mungkin berhasil | Engine mewajibkan `RejectionReasonId` (`WorkflowRejectRequest`), sedangkan view hanya mengirim teks alasan |
| Revisi dengan catatan kosong ditolak server | `WorkflowRequestRevisionRequest.Comment` wajib, tetapi layar mengizinkan kosong |

---

## 2. Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/hr/approvals/workflow-display-utils.js` | Ditambah `inboxAssignmentId`, `inboxRowDisplay`, `inboxActionError`, `inboxSummaryCounts`, `inboxPaging` (ruas DTO didahulukan, ruas lama sebagai cadangan) |
| `src/components/view/hr/workflow/approval-inbox-view.jsx` | Aksi memakai `assignmentId`. Kolom, kartu ringkasan, paging, dan `rowKey` membaca ruas DTO. Tolak memuat `rejectionReasons` dari `GET approval-inbox/{assignmentId}` (hak `ApprovalInbox : Read`, tersaring per jenis permohonan dan tahap), memilih alasan lewat `BaseSelectField`, dan mengirim `rejectionReasonId` + `comment`. Catatan wajib untuk Revisi dan untuk alasan ber-`isCommentRequired`. Tombol konfirmasi nonaktif lewat `confirmProps` bila isian belum lengkap. Galat memakai `getHrApiErrorMessage` |
| `tests/unit/approval-inbox-legacy-actions.test.mjs` (baru) | 6 uji: id penugasan, kolom, validasi Revisi/Tolak, ringkasan dan paging, isi view (tanpa `item.id`, dengan `rejectionReasonId`) |

`approval-inbox-service.js` tidak diubah: service sudah memetakan `notes` ke `comment` dan meneruskan `rejectionReasonId`.

---

## 3. Verifikasi

### 3.1 Validasi otomatis

| Perintah | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/approval-inbox-legacy-actions.test.mjs tests/unit/business-travel-v28-gates.test.mjs tests/unit/business-travel-v28-status.test.mjs` | 20/20 lulus |
| Kontrol negatif: 3 mutasi (`assignmentId` dihapus dari pembantu id; syarat alasan Tolak dihapus; approve kembali ke `item.id`) | 3/3 tertangkap, source dipulihkan |
| `npm run lint:errors` | Exit 0 |
| `npm run build` | 890/890 halaman, termasuk `/hr/workflow/approval-inbox` |

### 3.2 Uji browser (dr. Bima, `next dev` 3000 → backend 7184 → devYoga)

Hak `ApprovalInbox : Read/Approve/Reject/RequestRevision` diberikan sementara dan dikembalikan identik (45 izin).

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Daftar | Baris `SPPD/RSQ/2026/11/0013`–`0015`: nomor SPPD, "Perjalanan Dinas", "Dewi Anggraeni", "Atasan langsung pemohon", "Persetujuan Perjalanan Dinas" | `PASS` (AC3) | `340-05-kotak-masuk-run4.png` |
| Setujui `0013` | `200` "Approval workflow berhasil diproses."; SPPD Disetujui, workflow `Completed`, presensi Dinas Luar 2 hari | `PASS` (AC1) | `340-05-modal-A-run4.png` |
| Revisi `0014` tanpa catatan | Tombol "Kirim Permintaan Revisi" nonaktif, keterangan "Isi catatan revisi untuk pemohon." | `PASS` (AC1) | `340-05-modal-B-run4.png` |
| Revisi `0014` dengan catatan | `200`; SPPD Dikembalikan untuk Revisi, workflow `RevisionRequested` | `PASS` (AC1) | Keluaran skrip |
| Tolak `0015` | Tanpa alasan: tombol nonaktif, "Pilih alasan penolakan."; pilihan alasan dari server: "UJI-376 Kegiatan tidak relevan dengan tugas"; alasan tanpa catatan: tetap nonaktif, "Alasan ini mewajibkan catatan."; dengan catatan: `200`, SPPD Ditolak | `PASS` (AC2) | `340-05-modal-C-run4.png` |
| Kartu ringkasan dan paging | "Perlu Tindakan 1", "Disetujui Hari Ini 1", "Menampilkan 1 sampai 1 dari 1 data" | `PASS` | `393-1-kotak-masuk.png` |
| Ponsel 390 px | Terbaca | `PASS` | `393-2-kotak-masuk-ponsel.png` |

Keadaan "server tidak punya alasan untuk jenis ini" diuji di tingkat unit (`inboxActionError`); devYoga sudah punya alasan untuk `HRD_TRV_REQUEST`.

---

## 4. Wewenang UI yang dipakai (`DEV_DISCRETION`)

| Pilihan | Alasan |
| --- | --- |
| Isian "Alasan Penolakan" (pilihan) + "Catatan Penolakan" | Engine menuntut id alasan; catatan bebas tetap dikirim sebagai `comment` |
| Keterangan validasi di bawah isian | Menjelaskan mengapa tombol konfirmasi nonaktif |

---

## 5. Risiko dan catatan

| Hal | Isi |
| --- | --- |
| Kotak Masuk Terpadu V2 (menu) | Tetap tidak menampilkan SPPD: `ApprovalInboxQueryService` menyaring `Pending` + `ApproverEmployeeId`, sedangkan Workflow V2 membuat `Available` + `AssignedApproverUserId`. Perlu task backend atau keputusan menu (temuan `FE-HRD-339`) |
| Workflow lain di kotak masuk lama | Perbaikan berlaku umum untuk semua jenis yang memakai `approval-inbox`; hanya SPPD yang diuji di browser |
| Delta kontrak | Endpoint master `rejection-reasons` tidak dipakai; alasan diambil dari detail penugasan (as-is, belum tertulis di API bagian 6) |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
