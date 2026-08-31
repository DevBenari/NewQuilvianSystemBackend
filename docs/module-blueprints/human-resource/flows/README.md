# Human Resource — Business Flows

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Backend baseline | `origin/QuilvianIntegrationBackend`, diverifikasi pada `16b8b71` |
| Frontend baseline | `AgentCodexFrontend`, `2a1cea784` |
| Status | `DRAFT` |

Folder ini memuat alur bisnis modul HR. Satu berkas untuk satu alur. Diagram memakai Mermaid dan
disimpan sebagai teks, bukan gambar.

---

## 1. Aturan provenance

**Setiap langkah dan aturan bisnis yang material wajib diberi penanda asal-usul.** Ini pagar
utama folder ini: pembaca harus dapat membedakan mana yang sudah terbukti ada, mana yang sudah
diputuskan manusia, dan mana yang masih kosong.

| Penanda | Artinya | Boleh dijadikan dasar implementasi? |
| --- | --- | --- |
| `[EXISTING]` | Terbukti dari source backend atau frontend pada baseline saat ini | Ya |
| `[DECISION]` | Berasal dari `HRD-DEC-xxx` yang berstatus `approved` | Ya |
| `[OPEN]` | Belum ada keputusan dari pihak yang berwenang | **Tidak** |
| `[BLOCKED]` | Tidak boleh dilanjutkan karena dependency yang disebut namanya | **Tidak** |

**Larangan yang mengikat:** `[OPEN]` tidak boleh diubah menjadi aturan final berdasarkan praktik
umum, dugaan, pengalaman di tempat lain, atau kebiasaan industri. Bila sebuah kebutuhan bisnis
memerlukan nilai kebijakan yang belum ada kewenangannya, buat Open Question baru pada decision
log — jangan menuliskan angkanya di sini.

Contoh yang benar:

> Pengajuan cuti diperiksa terhadap saldo yang berlaku. `[EXISTING]`
> Berapa hari hak cuti tahunan per jenis pegawai. `[OPEN]` — `HRD-Q-06`

Contoh yang salah:

> Pengajuan cuti diperiksa terhadap hak cuti 12 hari per tahun.

Angka 12 tidak berasal dari mana pun. Menuliskannya berarti mengarang kebijakan
ketenagakerjaan.

## 2. Bentuk setiap berkas flow

Setiap flow memuat tujuh belas bagian dengan urutan yang sama:

Purpose · Actors · Trigger · Preconditions · Happy Path · Alternative Flow · Exception Flow ·
Approval · State Transition · Data Created/Updated · Backend Capability · Frontend Capability ·
Integration Boundary · Audit Requirement · Blocking Decision · Acceptance Criteria · diagram
Mermaid.

## 3. Daftar flow

| # | Berkas | Keadaan |
| --- | --- | --- |
| 00 | [`00-module-context-flow.md`](./00-module-context-flow.md) | Ada |
| 01 | [`01-employee-administration.md`](./01-employee-administration.md) | Ada |
| 02 | [`02-attendance.md`](./02-attendance.md) | Ada |
| 03 | [`03-leave.md`](./03-leave.md) | Ada |
| 04 | [`04-overtime.md`](./04-overtime.md) | Ada |
| 05 | [`05-work-scheduling.md`](./05-work-scheduling.md) | Ada — ditulis `PHASE 2B` |
| 06 | [`06-shift-change-swap.md`](./06-shift-change-swap.md) | Ada — ditulis `PHASE 2B` |
| 07 | [`07-attendance-correction.md`](./07-attendance-correction.md) | Ada — ditulis `PHASE 2B` |
| 08 | [`08-early-leave-permission.md`](./08-early-leave-permission.md) | Ada — ditulis `PHASE 2B` |
| 09 | [`09-unified-approval.md`](./09-unified-approval.md) | Ada — ditulis `PHASE 2B` |
| 10 | [`10-payroll-processing-handoff.md`](./10-payroll-processing-handoff.md) | Ada — ditulis `PHASE 2C`. Status **`PARTIAL`**: didesain sampai batas HR/Finance `HRD-DEC-009`; sesudahnya tetap `[BLOCKED]` oleh `HRD-Q-10`/`HRD-Q-11` |
| 11 | [`11-lifecycle-offboarding.md`](./11-lifecycle-offboarding.md) | Ada — ditulis `PHASE 2C` |
| 12 | [`12-competency-training.md`](./12-competency-training.md) | Ada — ditulis `PHASE 2C` |
| 13 | [`13-performance-management.md`](./13-performance-management.md) | Ada — ditulis `PHASE 2C` |
| 14 | [`14-employee-relations-discipline.md`](./14-employee-relations-discipline.md) | Ada — ditulis `PHASE 2C` |

Cakupan `PHASE 2C` adalah flow 10–14 (dikoreksi `PHASE 2B.1`, lihat `00-interview-decisions.md`
bagian 23.10). Seluruh lima flow selesai ditulis 28 Agustus 2026 — lihat bagian 25 pada
`00-interview-decisions.md` untuk ringkasan temuan lengkap.

## 4. Yang sengaja tidak dibuat

Tidak ada flow untuk **kredensial, kewenangan klinis, SPK/RKK, OPPE, FPPE, maupun kesehatan
kerja staf**. Keenamnya `BLOCKED` menunggu `requirement-completeness-gate` dan
`hospital-domain-architect`. Menggambar alurnya sekarang berarti menetapkan batas kewenangan
praktik klinis dan aturan akses data kesehatan yang belum ada wewenangnya.
