# Laporan Perubahan Frontend — `FE-HRD-R08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R08` |
| Judul | Service Persetujuan Terpadu jujur dan tersambung |
| Slice | `S-A7-V2` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R08` |
| Trace | Task asal `FE-HRD-276`; Kelompok C dan E audit |
| Contract version | API Contract bagian 30 |
| Wewenang UI | Tidak ada wewenang tampilan baru; hanya penanganan galat dan pengosongan data contoh |
| Dependency | — |
| Klasifikasi | `LIGHT` — perubahan alamat service tanpa tampilan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

`unified-approval-service.js` memakai `axios` polos; `approveItem` menjawab "Persetujuan berhasil disahkan (mode terdistribusi)." saat server gagal, dan alamat aksinya `inbox/{id}/approve` tidak ada di backend.

---

## 2. Proses bisnis dari sisi pengguna

Belum ada layar yang memakai service ini; Kotak Masuk V2 disambungkan pada `FE-HRD-R21`. Sesudah itu, keputusan atasan benar-benar dikirim ke `workflow/approvals`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Service dan controller `WorkflowInboxAndInstancesController.cs`, `WorkflowApprovalActionsController.cs`, `WorkflowDelegationsController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/unified-approval-service.js` | Pindah ke `InstanceAxios`; detail ke `instances/{id}`; setujui, tolak, kembalikan, dan setujui massal ke `approvals/*`; cabut delegasi memakai `PUT delegations/{id}/revoke` |

### 3.3 Kepatuhan arsitektur frontend

Service tetap memakai `InstanceAxios` dan pola `unwrap` yang sudah ada. Alamat yang belum ada di backend melempar galat `createHrUnavailableError`, sehingga layar pemanggil menampilkan "belum tersedia" alih-alih galat 404.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` — task hanya mengubah service

`UI GATE: N/A — task hanya mengubah service`

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — tidak ada perubahan layar.

---

## 5. Endpoint yang dikonsumsi

Grup `[Tags]`: `Corporate - HRD Workflow Inbox & Instances`, `Corporate - HRD Workflow Approval Actions`, `Corporate - HRD Workflow Delegations`. Rincian ada pada Lampiran C `frontend-unfinished-work.md`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend | 10 dari 10 panggilan cocok | `PASS` | Skrip pembanding route backend |

Uji manual: `NOT APPLICABLE` — belum ada layar pemakai.

**Tidak dijalankan:** uji browser.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1–3 | Terpenuhi | Service memakai `InstanceAxios`, tanpa `catch`, alamat cocok |
| Tidak ada lagi jawaban "Persetujuan berhasil disahkan (mode terdistribusi)" saat server gagal | Terpenuhi | Blok `catch` dihapus |
| DoD | Terpenuhi | Lint, build, laporan, status roadmap |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R21` |
