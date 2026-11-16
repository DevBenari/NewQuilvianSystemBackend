# Laporan Perubahan Frontend — `FE-HRD-R33`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R33` |
| Judul | Persetujuan bagian 2: delegasi, matriks, overdue, portal ESS |
| Slice | `S-A7-V2` (Workflow Persetujuan), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R33`; task asal `FE-HRD-284`–`289` |
| Trace | VR-WF-09 (Plh), aturan revisi 48 jam; kartu asal `frontend-roadmap.md` |
| Contract version | Source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi base component dan penyambungan API nyata |
| Dependency | `FE-HRD-R21` (kotak masuk & bulk, tidak disentuh) — 🟡 |
| Klasifikasi | `HEAVY` — 6 layar, 1 modal, service, util baru, pencabutan slice mock |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — delegasi, matriks baca-saja, overdue, portal ESS, permohonan saya, dan revisi nyata; simpan matriks dan unggah lampiran revisi tertahan backend |

---

## 1. Keadaan yang ditemukan di awal

- Layar delegasi, matriks, portal ESS, permohonan saya, dan revisi membaca `unifiedApprovalSlice` yang berisi data contoh, serta `approvals-display-utils.js` yang juga berisi data contoh.
- Teks layar revisi menyebut "kembali ke tahap pertama", padahal service backend mengembalikan ke penyetuju yang meminta revisi dengan batas baru 48 jam.

## 2. Proses bisnis dari sisi pengguna

1. **Delegasi**: pejabat membuat penunjukan Plh (pegawai pengganti, rentang tanggal, alasan), melihat daftar delegasi, dan mencabutnya. Pegawai melihat peran Plh yang sedang ia emban.
2. **Matriks SLA**: admin melihat matriks langkah persetujuan per jenis permohonan dan departemen (baca-saja).
3. **Overdue**: admin melihat item yang melewati SLA, lalu melakukan eskalasi manual; laporan kepatuhan SLA ditampilkan.
4. **Portal ESS**: pegawai melihat saldo cuti pribadi dan tautan cepat ke formulir pengajuan.
5. **Permohonan saya**: pegawai memantau status semua permohonannya; permohonan yang dikembalikan membuka layar **revisi** untuk mengirim ulang dengan catatan.
6. Jalur tidak normal: daftar kosong menampilkan kalimat kosong; tanpa hak, seksi terkait tidak tampil; galat server tampil di `InformationAlert`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Controller workflow (`WorkflowDelegationsController`, `WorkflowSlaMatrix…`, `WorkflowInbox…`, `WorkflowApprovalActionsController`), `LeaveBalanceSelfServiceController`, service `WorkflowCommentAndAttachmentService`, `ConfigureMatrixAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/unified-approval-service.js` | Delegasi (params), peran Plh saya, matriks, overdue, eskalasi, laporan SLA, permohonan saya, resubmit |
| `src/lib/services/hr/employee-self-service.js` | `getLeaveSummary` |
| `src/lib/constants/hr/approvals/approval-constants.js` | Rute, permission, jenis permohonan, peran penyetuju, status delegasi, batas minimal |
| `src/utils/hr/approvals/workflow-display-utils.js` | BARU: `parseWorkflowContext`, `findLatestReturnAction`, `formatHours` |
| `unified-approval-delegation-view.jsx`, `modal/CreateDelegationModal.jsx`, `unified-approval-matrix-view.jsx`, `unified-ess-portal-view.jsx`, `unified-ess-my-requests-view.jsx`, `unified-ess-revision-view.jsx` | Ditulis ulang dengan base component dan API nyata |
| `unified-approval-detail-view.jsx` | Memakai util bersama |
| `src/lib/state/store.jsx` | Slice `unifiedApproval` dicabut |
| `unifiedApprovalSlice.js`, `approvals.module.css`, `approvals-display-utils.js` | Dihapus (data contoh/yatim) |
| `menu-items.jsx` | `requiredPermission` matriks, overdue, permohonan saya |

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`; Redux mock dicabut dan diganti `useHrRemoteData`. Tidak ada pola baru.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `DataTable` | `REUSE` | — |
| Tab matriks/overdue | Rute terpisah + `DataFilter` tabs | `REUSE` | — |
| Dialog delegasi, eskalasi, resubmit | `ConfirmModal` + `BaseFormControl` + `HrResourceField` | `COMPOSE` | — |
| Status | `HrStatusBadge` | `WRAP` | Peta status domain |

`UI GATE: PASS — 6 elemen REUSE, 1 COMPOSE, 0 NEW`

```text
$ node .../check-css-usage.mjs <7 berkas JSX R33>
OK — 7 berkas diperiksa, 0 temuan.
$ grep Tailwind <berkas R33>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel dan ringkasan |
| Kosong | "Belum ada delegasi." / "Tidak ada item melewati SLA." / "Belum ada permohonan." |
| Gagal | Pesan server di `InformationAlert` |
| Tanpa hak akses | Seksi per hak tidak tampil; menu bergerbang permission |

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Workflow Delegations

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/workflow/delegations` | Daftar delegasi | `WorkflowDelegation : ViewDelegationList` |
| `POST` | `/v1/hrd/workflow/delegations` | Buat Plh | `WorkflowDelegation : CreateDelegation` |
| `PUT` | `/v1/hrd/workflow/delegations/{id}/revoke` | Cabut | `WorkflowDelegation : RevokeDelegation` |
| `GET` | `/v1/hrd/workflow/delegations/my-acting-roles` | Peran Plh saya | `WorkflowDelegation : ViewOwn` |

#### Corporate - HRD Workflow SLA & Matrix Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/workflow/matrix` | Matriks (baca-saja) | `WorkflowSlaMatrix : ViewMatrix` |
| `GET` | `/v1/hrd/workflow/sla/overdue` | Item overdue | `WorkflowSlaMatrix : ViewSlaOverdue` |
| `POST` | `/v1/hrd/workflow/sla/escalate/{id}` | Eskalasi manual | `WorkflowSlaMatrix : ManualEscalate` |
| `GET` | `/v1/hrd/workflow/sla/compliance-report` | Laporan kepatuhan | `WorkflowSlaMatrix : ExportSlaReport` |

#### Corporate - HRD Workflow Inbox & Instances

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/workflow/my-requests` | Permohonan saya | `WorkflowInboxInstance : ViewOwn` |
| `GET` | `/v1/hrd/workflow/instances/{id}` | Rincian | `WorkflowInboxInstance : ViewInstanceDetail` |

#### Corporate - HRD Workflow Approval Actions

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/hrd/workflow/instances/{id}/resubmit` | Kirim ulang revisi | `WorkflowApprovalAction : ResubmitRevision` |

#### Self Services / Human Resource / Leave Balance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/self-services/human-resource/leave/balances/summary` | Saldo cuti di portal ESS | `MyLeaveBalance : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 13 berkas R33 | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 7 JSX | 0 temuan | `PASS` | 3.4 |
| `npm run build` BUILD-1 / BUILD-2 | 821/821 dan 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| `FE-HRD-284` delegasi | Terpenuhi | Buat/cabut/daftar nyata |
| `FE-HRD-285` modal Plh setara (VR-WF-09) | Belum terpenuhi | Server tidak menegakkan VR-WF-09 dan tidak ada filter opsi; dropdown memuat semua pegawai |
| `FE-HRD-287` matriks menyimpan konfigurasi | Belum terpenuhi | `POST workflow/matrix` tidak menyimpan ke DB; layar baca-saja |
| `FE-HRD-288` portal ESS | Terpenuhi sebagian | Kuota lembur & plafon benefit pribadi tidak punya endpoint ringkasan |
| `FE-HRD-289` riwayat & revisi | Terpenuhi sebagian | Unggah ulang lampiran tidak dipasang (server tidak menulis isi berkas) |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `AverageResolutionHours` laporan SLA bernilai tetap 18,5 dan `CurrentApproverName` overdue selalu "Pejabat Otorisasi" — keduanya tidak ditampilkan |
| Masalah yang diketahui | `GET delegations` mengembalikan semua delegasi tanpa filter pemberi kuasa |
| Dependency backend | Simpan matriks, unggah lampiran workflow, VR-WF-09, ringkasan kuota ESS, unduh PDF bukti persetujuan |
| Perubahan sampingan | Slice mock dan dua berkas data contoh dihapus |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Perbaikan backend butir di atas; uji browser |
