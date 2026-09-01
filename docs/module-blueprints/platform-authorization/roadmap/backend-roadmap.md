# Backend Roadmap — Platform Authorization & Access Control

`blueprint_id`: `SEC-BP-001` · Task prefix: `BE-SEC`

Kontrak kanonik yang berlaku: **identitas otorisasi sebuah endpoint adalah pasangan
`(resource, action)` pada `[AccessPermission]`**. Seeder mendaftarkan pasangan itu apa adanya,
sehingga kunci yang dicari saat request masuk selalu ada di registry.

| Task ID | Outcome | Trace | Cakupan dan reuse | Dependency | Acceptance criteria/verifikasi | Risiko/DoD | Status |
|---|---|---|---|---|---|---|---|
| `BE-SEC-001` | Integritas otorisasi existing pulih dan menjadi baseline yang sah | `SEC-REQ-001..006`, keputusan owner `D1`–`D4`, `N1`–`N3` | Identitas permission kanonik; rekonsiliasi registry generik; validator startup; proyeksi otorisasi organisasi; pencabutan hak; klasifikasi dan rebind aman policy inert | Audit Phase 1 dan rencana Phase A0 disetujui; otorisasi migration dan database development terpisah | Source mismatch `89 → 0`; registry usang `59 → 0`; drift registry `0/0`; `SysAccessPolicy` tidak pernah dibuat seeder; 856 test otomatis lulus; 10 smoke test lulus dengan akun non-SuperAdmin; `dotnet ef migrations has-pending-model-changes` tanpa perubahan tertunda | DoD: source + migration + test + laporan tracked. Migration sudah diterapkan ke database development; lingkungan lain belum | `COMPLETED / READY FOR COMMIT` |

Bukti lengkap: [`../task/report/backend/BE-SEC-001.md`](../task/report/backend/BE-SEC-001.md)

---

## Task yang BELUM dikerjakan

Butir berikut sengaja **tidak** ditandai selesai. Seluruhnya fase berikutnya dan belum disentuh.

| Rencana | Status |
|---|---|
| Business Permission catalog (`SysBusinessFeature`, `SysPermission`) | `NOT STARTED` |
| Access Profile (`SysAccessProfile`, `SysAccessProfilePermission`, `SysOrganizationAccessProfile`) | `NOT STARTED` |
| Endpoint `/api/access/me` | `NOT STARTED` |
| Baseline entitlement Self Service otomatis | `NOT STARTED` |
| Otorisasi frontend (sidebar, route guard, guard tab dan tombol) | `NOT STARTED` |
| Perancangan ulang layar Manajemen Hak Akses menjadi business-oriented | `NOT STARTED` |

---

## Keputusan owner yang masih terbuka

| Butir | Isi |
|---|---|
| Pemberian kemampuan sensitif | Kasir, refund, write-off, penerimaan sampel laboratorium, dan penandatanganan discharge kini **dapat** diberikan admin, tetapi tetap ditolak sampai benar-benar diberikan. Departemen × Posisi mana yang berhak belum diputuskan |
| 17 policy inert tersisa | 5 `SEMANTIC_CHANGED` dan 12 `REMOVED_CAPABILITY`, sengaja dibiarkan fail closed |
| Dua baris proyeksi legacy-unresolved | Dipertahankan tanpa menebak sumbernya, menunggu peninjauan |
| Dua endpoint audio antrean | `QueueVoice.GetAudio` dan `QueueVoice.DownloadAudio` belum diklasifikasi `[AllowAnonymous]` versus policy |
| Penerapan ke lingkungan lain | Migration dan rekonsiliasi baru diterapkan ke development |
