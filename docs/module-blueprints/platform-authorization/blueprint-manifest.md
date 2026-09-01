# Blueprint Manifest — Platform Authorization & Access Control

| Field | Nilai |
|---|---|
| `blueprint_id` | `SEC-BP-001` |
| `module_name` | Platform Authorization & Access Control |
| `module_slug` | `platform-authorization` |
| `module_prefix` | `NOT APPLICABLE` — lihat bagian *Prefix entity operasional* |
| `revision` | `1` |
| `status` | `approved` |
| Category | `SHARED PLATFORM CAPABILITY` |
| Backend task prefix | `BE-SEC` |
| Product/domain owner | Pemilik sistem |
| Security owner | Pemilik sistem |
| Frontend authority | Belum ditetapkan — frontend belum masuk cakupan |
| `approved_by` / `approved_at` | Pemilik sistem / 1 September 2026 |
| Backend SHA saat Phase A0 | `43ba35d934d1615edbeac9952e0f19e3cca353fd` |
| Compatibility | Tidak memutus endpoint existing; seluruh perubahan bersifat aditif atau pemulihan |

---

## 1. Mengapa blueprint ini ada

Hak akses Quilvian bukan milik satu modul bisnis. Ia dipakai bersama oleh Rawat Inap, IGD,
Billing, Farmasi, Laboratorium, Rekam Medis, Human Resource, dan seluruh modul lain. Sebelum
blueprint ini ada, pekerjaan yang menyentuh otorisasi tidak punya tempat tinggal: menaruhnya di
blueprint modul bisnis mana pun akan salah arsip, karena tidak satu pun modul memilikinya.

Blueprint ini menjadi rumah bagi kemampuan bersama tersebut, sejajar dengan pola
`SHARED PLATFORM CAPABILITY` yang sudah dipakai Workflow.

---

## 2. Cakupan

Yang **termasuk**:

| Cakupan | Isi |
|---|---|
| Integritas registry permission | Identitas permission kanonik, pendaftaran kemampuan ke layar Akses Role, rekonsiliasi baris usang |
| Proyeksi otorisasi organisasi | Penurunan penempatan organisasi otoritatif menjadi proyeksi yang dipakai pemeriksaan izin |
| Resolusi izin efektif | Gabungan izin dari seluruh penempatan yang sah milik satu akun |
| Infrastruktur berdampingan autentikasi | Filter, atribut, validator startup, dan pemeriksaan kelayakan penempatan |
| Fondasi Business Permission | Dasar yang dipakai fase berikutnya, tanpa mendahuluinya |

Yang **tidak termasuk**:

- Blueprint Human Resource, Billing, Operasi, Rekam Medis, dan modul bisnis lain. Blueprint ini
  tidak pernah memiliki entity operasional mereka.
- Layar Manajemen Hak Akses (Role Management UI) beserta perancangan ulangnya.
- Business Permission catalog, Access Profile, `/api/access/me`, baseline Self Service otomatis,
  dan otorisasi frontend. Seluruhnya fase berikutnya dan **belum** dikerjakan.

---

## 3. Prefix entity operasional

Phase A0 **tidak membuat satu pun entity operasional baru**. Yang ditambahkan hanya satu kolom
pada tabel platform yang sudah ada (`AspNetUserOrganization.SourceAssignmentId`) beserta dua
service dan satu validator yang tidak dipersistensi.

Karena itu blueprint ini **belum** mendaftarkan prefix pada
`rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Prosedur registry menyatakan
pendaftaran dibutuhkan ketika sebuah folder sub-domain akan memuat model persisted; syarat itu
belum terpenuhi.

Bila kelak fase Business Permission membuat entity persisted baru — misalnya
`SysBusinessFeature`, `SysPermission`, atau `SysAccessProfile` — pendaftaran prefix menjadi wajib
dan **harus diusulkan lebih dulu** kepada pemilik sistem sebelum modelnya dibuat.

---

## 4. Task backend

| Task ID | Judul | Status |
|---|---|---|
| `BE-SEC-001` | Authorization Integrity Foundation | `COMPLETED` — lihat `roadmap/backend-roadmap.md` |

---

## 5. Dokumen terkait

| Dokumen | Isi |
|---|---|
| `roadmap/backend-roadmap.md` | Daftar task backend beserta acceptance criteria dan statusnya |
| `roadmap/requirement-traceability.md` | Hubungan requirement dengan bukti implementasi |
| `task/report/backend/BE-SEC-001.md` | Laporan tracked Phase A0 |
