# Backend Roadmap — Platform Authorization & Access Control

`blueprint_id`: `SEC-BP-001` · Backend task prefix: `BE-SEC` · Frontend task prefix: `FE-SEC` (usulan)
· Revisi roadmap: `2` · Diperbarui 2 September 2026

---

## Kontrak kanonik yang berlaku

**Identitas otorisasi sebuah endpoint adalah pasangan `(resource, action)` pada
`[AccessPermission]`.** Seeder mendaftarkan pasangan itu apa adanya, sehingga kunci yang dicari saat
request masuk selalu ada di registry.

Kontrak ini ditetapkan `BE-SEC-001` dan **tidak berubah** oleh satu pun task di bawah. Business
Permission adalah lapisan **di atas** technical permission, bukan penggantinya.

Resolver transisional yang disetujui:

```
EffectiveTechnicalPermissions(user)
  = LegacyTechnicalPermissions(user)
    UNION
    TechnicalPermissions( EffectiveBusinessPermissions(user) )
```

Sumber Business Permission dapat dinyalakan dan dimatikan selama migrasi. `SysAccessPolicy` tidak
dihapus pada fase awal. Tidak boleh ada *silent privilege loss* maupun *privilege broadening*.

---

## Klasifikasi induk

`BE-SEC-002` diklasifikasikan **`EPIC`** menurut `rules/backend/TASK_CLASSIFICATION.md`: perancangan
ulang yang menyentuh seluruh arsitektur otorisasi dan berjalan multi-fase. Aturan eksekusi
menyatakan task `EPIC` tidak pernah dikerjakan langsung — `STOP → DECOMPOSE → klasifikasikan ulang
setiap fase`.

Dekomposisinya menghasilkan 11 task. Setiap task dapat direview sendiri, dites sendiri, di-rollback
sendiri, dan tidak menuntut migrasi *big-bang*.

---

## Task selesai

| Task ID | Outcome | Trace | Acceptance criteria/verifikasi | Status |
|---|---|---|---|---|
| `BE-SEC-001` | Integritas otorisasi existing pulih dan menjadi baseline yang sah | `SEC-REQ-001..012`, keputusan owner `D1`–`D4`, `N1`–`N3` | Source mismatch `89 → 0`; registry usang `59 → 0`; drift registry `0/0`; `SysAccessPolicy` tidak pernah dibuat seeder; 856 test otomatis lulus; 10 smoke test lulus dengan akun non-SuperAdmin | `COMPLETED` |
| `BE-SEC-002` | Arsitektur Business Permission dan Access Profile ditetapkan; seluruh keputusan owner ditutup; epic didekomposisi menjadi 11 task | `SEC-REQ-013..023`, keputusan owner `D-ARCH-1`–`D-ARCH-9` | Tiga dokumen evidence; 19 Business Permission terklasifikasi `A`/`B`/`C`/`D`; matriks pemecahan 7 → 28 identitas; audit collision prefix `Sec` nihil; audit semantik OR ASP.NET; impact query read-only pada database development; legacy parity matrix per Departemen × Posisi | **`CLOSED / READY FOR COMMIT`** — **bukan** task implementasi |

Bukti `BE-SEC-001`: [`../task/report/backend/BE-SEC-001.md`](../task/report/backend/BE-SEC-001.md)

Bukti `BE-SEC-002`:
[`../evidence/01-be-sec-002-audit-architecture.md`](../evidence/01-be-sec-002-audit-architecture.md),
[`../evidence/02-be-sec-002-decision-closure.md`](../evidence/02-be-sec-002-decision-closure.md),
[`../evidence/03-be-sec-003-pre-implementation-impact.md`](../evidence/03-be-sec-003-pre-implementation-impact.md)

Bukti planning `BE-SEC-003`:
[`../evidence/04-be-sec-003-implementation-plan.md`](../evidence/04-be-sec-003-implementation-plan.md)

---

## Task implementasi berikutnya

### `BE-SEC-003` — Technical Permission Granularity Hardening (pilot Dokter Rawat Jalan)

| Field | Isi |
|---|---|
| **Status** | **`READY FOR IMPLEMENTATION`** — planning selesai dan tracked; seluruh keputusan owner tertutup; menunggu wewenang eksekusi task |
| **Klasifikasi** | `HEAVY` (skor 11) |
| **Planning evidence** | [`../evidence/04-be-sec-003-implementation-plan.md`](../evidence/04-be-sec-003-implementation-plan.md) — scope, split matrix, audit lifecycle seeder, deployment order, rollback, test plan, database impact, known limitations |
| **Impact evidence** | [`../evidence/03-be-sec-003-pre-implementation-impact.md`](../evidence/03-be-sec-003-pre-implementation-impact.md) — hasil query read-only database development |
| **Outcome** | Tujuh identitas technical permission yang terlalu kasar dipecah menjadi 28 identitas, tanpa satu pun Departemen × Posisi kehilangan atau memperoleh kemampuan |
| **Trace** | `SEC-REQ-013`, `SEC-REQ-014`, `SEC-REQ-021`; keputusan `D-ARCH-3`, `D-ARCH-6`, `D-ARCH-7`, `D-ARCH-8`, `D-ARCH-10`, `O-1` |
| **Scope** | Pemecahan pada 6 controller pilot; identitas `QueueVoice.PlayAudio` beserta otorisasi OR; penutupan identitas lama tanpa hard delete; perluasan `SysAccessPolicy` ke *exact historical capability set*; pembaruan test terkunci |
| **Di luar scope** | Business Permission, Access Profile, resolver, registry prefix, `MedicalCertificate`, penyempitan hak siapa pun, pemecahan platform-wide |
| **Dependency** | **`BE-SEC-002` `CLOSED`** — arsitektur, klasifikasi, dan keputusan owner berasal dari sana. Impact report `evidence/03` sudah ditinjau. Wewenang migrasi data `CONDITIONALLY APPROVED` untuk development |
| **Database** | Tidak ada perubahan skema, tidak ada EF migration. Migrasi **data**: `SysActionAccess` aktif 1.076 → 1.097 (28 baru, 7 ditutup); `SysAccessPolicy` fisik 498 → 537 (39 dibuat, nol dihapus); efektif 469 → 500, lalu 508 setelah langkah audio |
| **Acceptance criteria** | **1. Technical permission split selesai** — 28 identitas terdaftar dan terbaca layar Akses Role; 7 identitas lama tertutup tanpa hard delete; `PatientProcedure.Create` tetap aktif dan `PatientProcedure.Select` dibuat. **2. Legacy parity terverifikasi** — untuk setiap Departemen × Posisi, himpunan endpoint yang dapat dijangkau identik sebelum dan sesudah; jumlah pasangan tetap 11. **3. Tidak ada privilege broadening** — nol Departemen × Posisi memperoleh endpoint baru, di luar penyempitan audio yang diputuskan `O-1`. **4. Tidak ada silent privilege loss** — nol Departemen × Posisi kehilangan endpoint; 6 pengguna terdampak diverifikasi satu per satu. **5. Migrasi teruji** — mode laporan dijalankan dan ditinjau lebih dulu; hasil mode tulis sama dengan laporannya; perluasan idempoten. **6. Rollback teruji** — skrip balik dijalankan pada database uji dan mengembalikan `SysAccessPolicy` efektif ke 469 serta pasangan Departemen × Posisi ke 11. **7.** `ReconcileNeverCreatesAccessPolicy` tetap hijau. **8.** `CompatibilityFallbackMatchesApprovedLegacySetExactly` diperbarui secara sadar — jumlah tetap 69. **9.** Otorisasi audio terbukti **OR**, bukan AND. **10.** Tiga test SuperAdmin tetap hijau; seluruh test suite lulus; `has-pending-model-changes` bersih; smoke test akun non-SuperAdmin |
| **Larangan implementasi** | 1. `AllowAnonymous` pada endpoint audio. 2. Menjadikan `QueueDisplayRuntimeRead` sebagai permission user dokter/perawat. 3. Dua atribut otorisasi yang runtime-nya menghasilkan AND. 4. Menyentuh `MedicalCertificate`. 5. Memperbarui `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. 6. Menyempitkan hak siapa pun di luar langkah audio yang tercatat. 7. Menaruh perluasan policy di dalam `AccessMenuSeeder`. 8. Pre-seeding identitas baru sebelum deploy |
| **Risiko/DoD** | Risiko tertinggi di rangkaian ini. DoD: source + migrasi data + test + laporan tracked + bukti parity per Departemen × Posisi |
| **Rollback** | Mandiri, tanpa titik tanpa kembali. Balikkan source; seeder mendaftarkan ulang identitas lama; skrip balik mengaktifkan 8 policy lama dan menonaktifkan 39 policy baru; snapshot tersedia sebagai jaring pengaman |

---

## Task berikutnya, berurutan

| Task ID | Judul | Klasifikasi | Dependency | Perubahan database | Batas rollback | Status |
|---|---|---|---|---|---|---|
| `BE-SEC-004` | Registry prefix `Sec` dan skema katalog Business Permission | `HEAVY` | `BE-SEC-003` | Migration **aditif**: 3 tabel baru | `Down` menghapus 3 tabel kosong; baris registry dicabut | `NOT STARTED` |
| `BE-SEC-005` | Isi katalog Business Permission pilot | `MEDIUM` | `BE-SEC-004` | Data katalog, idempoten | Hapus baris katalog; tabel tetap | `NOT STARTED` |
| `BE-SEC-006` | Skema Access Profile dan penetapan organisasi | `HEAVY` | `BE-SEC-005` | Migration **aditif**: 4 tabel baru | `Down` menghapus 4 tabel | `NOT STARTED` |
| `BE-SEC-007` | Resolver Business Permission mode bayangan | `MEDIUM` | `BE-SEC-006` | Tidak ada — query baca | Hapus service dan registrasinya | `NOT STARTED` |
| `BE-SEC-008` | Aktivasi sumber izin kedua | `HEAVY` | `BE-SEC-007` + laporan bayangan ditinjau | Tidak ada | **Matikan sakelar** | `NOT STARTED` |
| `BE-SEC-009` | API admin Business Access | `HEAVY` | `BE-SEC-008` | Tidak ada skema baru | Cabut controller | `NOT STARTED` |
| `BE-SEC-010` | `GET /api/v1/access/me` | `MEDIUM` | `BE-SEC-008` | Tidak ada | Cabut endpoint | `NOT STARTED` |
| `BE-SEC-011` | Baseline Self Service otomatis | `TBD` | `BE-SEC-008` + keputusan HR | Belum diketahui | Belum dirancang | `NOT DECOMPOSED` |
| `FE-SEC-001` | State izin frontend dan perbaikan guard | `HEAVY` | `BE-SEC-010` + frontend authority | Tidak ada | Kembalikan berkas frontend | `NOT STARTED` |
| `FE-SEC-002` | Layar Manajemen Hak Akses business-oriented | `HEAVY` | `BE-SEC-009`, `FE-SEC-001` | Tidak ada | Cabut rute view baru | `NOT STARTED` |
| `FE-SEC-003` | Guard tab dan tombol Dokter Rawat Jalan | `MEDIUM` | `FE-SEC-001` | Tidak ada | Kembalikan berkas | `NOT STARTED` |

Scope, acceptance criteria, dan berkas yang diperkirakan berubah untuk setiap task ada pada
[`../evidence/02-be-sec-002-decision-closure.md`](../evidence/02-be-sec-002-decision-closure.md)
bagian L.

### Rantai dependency

```
BE-SEC-003  (hardening identitas)
   └── BE-SEC-004  (registry + skema katalog)
          └── BE-SEC-005  (isi katalog)
                 └── BE-SEC-006  (skema Access Profile)
                        └── BE-SEC-007  (resolver bayangan)
                               └── BE-SEC-008  (aktivasi bersakelar)  ← perilaku berubah di sini
                                      ├── BE-SEC-009  (API admin)
                                      │      └── FE-SEC-002
                                      ├── BE-SEC-010  (/api/access/me)
                                      │      └── FE-SEC-001
                                      │             ├── FE-SEC-002
                                      │             └── FE-SEC-003
                                      └── BE-SEC-011  (Self Service)
```

---

## Task terpisah di luar rantai

| Pekerjaan | Repository | Alasan terpisah |
|---|---|---|
| Perbaikan route tab Surat Dokter: `doctor-certificates` → `medical-certificates` | Frontend | Cacat kontrak yang sudah ada. **Dilarang** diselipkan ke task `BE-SEC` mana pun |
| Pemecahan `MedicalCertificate.Update` (7 endpoint: `issue`, `verify`, `approve`, `reject`, `revoke`, `cancel`, `PUT`) | Backend | Menunggu perbaikan route di atas |
| Tombol "Tidak Hadir" memakai `InstanceAxios`, bukan `fetch` mentah | Frontend | Kebersihan arsitektur |
| Otorisasi SignalR hub `/hubs/queues` | Backend | Belum diaudit apakah `[Authorize]` saja memadai |

---

## Keputusan owner yang sudah ditutup

| Butir | Keputusan | Tanggal |
|---|---|---|
| Runtime architecture | Approach A — lihat kontrak kanonik di atas | 2 September 2026 |
| Prefix entity | `Sec` = *Security*; registry diperbarui pada `BE-SEC-004`, bukan `BE-SEC-003` | 2 September 2026 |
| Access Profile | Bundel dapat dipakai ulang; override hanya `ADDITIVE GRANT`; tanpa DENY | 2 September 2026 |
| Procedure approve/execute/cancel | Tiga permission terpisah; `Approve` sensitif dan fail closed | 2 September 2026 |
| Consultation write vs complete | Capability berbeda; `Complete` adalah transisi workflow | 2 September 2026 |
| Queue audio — mekanisme | `QueueVoice.PlayAudio` **OR** `QueueDisplayRuntimeRead`. `AllowAnonymous` ditolak. `QueueDisplayRuntimeRead` **tidak boleh** menjadi permission user dokter/perawat. Implementasi wajib memakai satu mekanisme yang benar-benar menghasilkan OR | 2 September 2026 |
| Queue audio — penerima `QueueVoice.PlayAudio` (`O-1`) | Delapan Departemen × Posisi pemegang izin antrean: Medis × Dokter Umum, Dokter Spesialis, Dokter IGD; Keperawatan × Perawat Rawat Jalan, Perawat Rawat Inap, Perawat IGD, Kepala Keperawatan, Kepala Ruangan. Mencakup 17 pengguna aktif | 2 September 2026 |
| Medical Certificate | Tetap `BROKEN_DEPENDENCY`; task terpisah | 2 September 2026 |
| Impact query read-only pada database development | Disetujui; sudah dijalankan, hasil pada `evidence/03` | 2 September 2026 |
| Migrasi data development | **Conditionally approved** — boleh dijalankan dalam `BE-SEC-003` bila dry-run membuktikan enam syarat parity | 2 September 2026 |

---

## Keputusan owner yang masih terbuka

| Butir | Menahan task | Sifat |
|---|---|---|
| Daftar Departemen × Posisi yang berwenang **menyetujui** tindakan pasien | `BE-SEC-006` | Kewenangan klinis dan finansial. Default fail closed |
| Daftar Departemen × Posisi yang berwenang **melaksanakan** tindakan pasien | `BE-SEC-006` | Kewenangan klinis |
| **`P-1`** — Topologi deployment untuk lingkungan selain development | Penerapan `BE-SEC-003` **di luar development** | Bila instance lama masih melayani traffic saat instance baru menyala, seeder instance baru menutup identitas lama di database bersama dan instance lama seketika menolak 6 pengguna. Dibutuhkan konfirmasi pola **hentikan-dulu-baru-nyalakan**. Tidak ditemukan setting replica pada source; topologi produksi tidak dapat disimpulkan. Lihat `evidence/04` bagian E.3 dan I.1 |
| Apakah Keperawatan × Bidan termasuk actor pemanggil pasien | Tidak menahan | Bidan dikecualikan dari penerima `QueueVoice.PlayAudio` karena **tidak memegang satu pun izin antrean**. Bila pemilik sistem menganggapnya actor pemanggil, penambahannya satu baris pada langkah audio `BE-SEC-003` |
| Penetapan frontend authority dan prefix `FE-SEC` | `FE-SEC-001` | Kepemilikan modul |
| Siapa yang berwenang menandatangani surat dokter | Task terpisah | Ditunda sampai route diperbaiki |
| Definisi "pegawai aktif" untuk baseline Self Service | `BE-SEC-011` | Keputusan HR |
| 17 policy inert warisan `BE-SEC-001` (5 `SEMANTIC_CHANGED`, 12 `REMOVED_CAPABILITY`) | — | Sengaja fail closed |
| Dua baris proyeksi legacy-unresolved | — | Sengaja dipertahankan |
| Penerapan ke lingkungan selain development | — | Operasional |
