# Tiga Permintaan untuk Muhammad Hamzah — Modul Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-002` |
| `tanggal` | 2026-09-02 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium |
| `kepada` | **Muhammad Hamzah** — pemilik registry prefix dan pemilik repo marketplace `MHamzah1/QuilvianEngineeringSkillsClaude` |
| `induk` | Turunan `LAB-REQ-001` butir 9, 10, dan `LAB-OPEN-018` |
| `sifat` | Operasional. Bukan artefak desain |

Modul Laboratorium sudah selesai dirancang: 36 keputusan disetujui, lima kontrak dikunci, dan
roadmap 19 task backend serta 9 task frontend sudah terbit.

## Status jawaban — 2026-09-02

| No | Butir | Status |
|---:|---|---|
| 1 | Rules root runtime | ✅ **Disetujui** — pilihan **B**, marketplace diarahkan ke sumber canonical. **Belum dieksekusi**, lihat catatan di bawah |
| 2 | Lifecycle `PLANNED` → `ACTIVE` | ✅ **Disetujui dan sudah diterapkan** pada registry |
| 3 | Prefix data induk | ✅ **Disetujui** — memakai `Lab`; seluruh artefak sudah disesuaikan |

> **Yang masih perlu tindakan Anda: butir 1.** Registry dan penamaan sudah diterapkan, tetapi
> gerbang `AGENTS.md` masih menahan seluruh task backend selama rules root runtime belum memuat
> `GLOBAL_RULES.md` dan `rules/backend/engineering/`. Persetujuan sudah ada; yang belum adalah
> pengarahan marketplace itu sendiri, dan itu hanya dapat dilakukan pemilik repo marketplace.

Isi permintaan aslinya tetap ditulis lengkap di bawah sebagai dasar keputusan.

---

## 1. Rules root runtime tidak memuat dokumen tata kelola

**`LAB-OPEN-018` — menahan seluruh task backend, tanpa kecuali.**

`AGENTS.md` repository backend memerintahkan setiap task implementasi membaca
`rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan
`rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, lalu menutup dengan gerbang
yang tegas:

> *"Jika `AGENTS.md` ini atau akar `rules/` tidak dapat dibaca, berhenti dan laporkan
> `BLOCKED — canonical governance unavailable`… Jangan mengarang isi rules, menggantinya dengan
> sumber lain, atau memakai default agent sebagai pengganti."*

**Apa yang ditemukan, diperiksa 2026-09-02:**

| Yang diperiksa | Hasil |
|---|---|
| Rules root terpasang | 15 berkas. Sumber canonical `DevBenari/QuilvianEngineeringSkills` punya 29 |
| `GLOBAL_RULES.md` | **Tidak ada** — padahal `AGENTS.md` menyuruh membacanya paling awal |
| `rules/backend/engineering/` | **Tidak ada** — kedua dokumen tata kelola hilang |
| Rules frontend | 1 dari 11 berkas. Yang hilang termasuk `base-component-catalog.md`, `design-tokens.md`, `master-data-feature-standard.md`, `page-composition-patterns.md` |

**Kenapa `/plugin update` tidak menolong.** Marketplace `quilvian` menunjuk
`MHamzah1/QuilvianEngineeringSkillsClaude`, bukan sumber canonical. Repo itu hanya punya
**2 commit** dan satu branch, dan penelusuran seluruh riwayatnya menunjukkan kedua dokumen
tata kelola maupun `GLOBAL_RULES.md` **tidak pernah ada di commit mana pun**. Memperbarui
plugin hanya menarik isi yang sama.

**Yang diminta — pilih salah satu:**

| Pilihan | Tindakan | Akibat |
|---|---|---|
| **A** | Terbitkan `BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, dan `GLOBAL_RULES.md` ke `MHamzah1/QuilvianEngineeringSkillsClaude` | Menutup gerbang backend. Rules frontend tetap kurang |
| **B** | Arahkan marketplace `quilvian` ke sumber canonical `DevBenari/QuilvianEngineeringSkills` | Menutup gerbang backend **dan** 10 rules frontend sekaligus, serta menghapus kemungkinan dua sumber menyimpang di kemudian hari |

Usulan kami **pilihan B**, tetapi keputusannya milik Anda.

---

## 2. Lifecycle registry Laboratorium masih `PLANNED`

**`LAB-OPEN-019` — menahan setiap entity `Lab*` dan setiap migration.**

Baris registry yang berlaku:

| Area | Module/pemilik | Category | Prefix | Lifecycle |
|---|---|---|---|---|
| HealthServices | LaboratoryManagement / Laboratory | BUSINESS DOMAIN / MODULE | Lab | **`PLANNED`** |

Registry menyatakan sendiri bahwa persetujuan penamaan *"**tidak** memberi wewenang
implementasi, migration, pekerjaan database, deployment, maupun aktivasi modul berstatus
`PLANNED`."*

**Keadaan yang perlu dijelaskan.** `LabOrder`, `TrxLabSpecimen`, `TrxLabTransitionHistory`, dan
`MstLabRejectionReason` **sudah berjalan di produksi** beserta migration dan **30 pengujian**
— 18 pada `LaboratorySpecimenLifecycleTests.cs`, 12 pada `LaboratoryAuthorityTests.cs`.
Menurut registry, modul berstatus `PLANNED` belum berwenang atas semua itu.

**Dua kemungkinan, keduanya perlu ditutup:**

| Kemungkinan | Tindakan |
|---|---|
| Lifecycle-nya yang usang — pekerjaan itu memang sudah diberi wewenang | Naikkan ke `ACTIVE`, catat pada *Catatan perubahan lifecycle* |
| Pekerjaan itu berjalan tanpa wewenang registry | Perlu ditinjau lebih dulu sebelum pekerjaan baru ditambahkan |

**Preseden dari Anda sendiri.** Pada 2026-08-24 Anda menaikkan `InPatientManagement / Inp` dari
`PLANNED` ke `ACTIVE` atas dasar blueprint `RWI-BP-001` keputusan `RWI-DEC-068`, dengan catatan
bahwa eksekusi database di luar lokal dan deployment tetap wewenang terpisah. Bentuk yang sama
sudah cukup untuk Laboratorium.

> **✅ DISETUJUI DAN DITERAPKAN 2026-09-02.** Baris registry kini berbunyi
> `HealthServices | LaboratoryManagement / Laboratory | BUSINESS DOMAIN / MODULE | Lab | ACTIVE`,
> dan *Catatan perubahan lifecycle* bertambah satu baris atas nama Muhammad Hamzah dengan bentuk
> yang sama seperti preseden `Inp`: wewenangnya mencakup source dan pembuatan migration, sementara
> eksekusi database di luar dev pemilik dan deployment tetap wewenang terpisah.
>
> Diterapkan pada sumber canonical `QuilvianEngineeringSkills/agents/rules/backend/engineering/`
> dan pada salinan `NewQuilvianSystemBackend/docs/engineering/` yang dibaca checker. Mirror
> `Claude/.claude/rules/` menyusul lewat `tooling/sync-rules.ps1`.

---

## 3. Prefix data induk Laboratorium — cukup dijawab ya atau tidak

**`LAB-OPEN-021` — menahan `BE-LAB-02` dan penamaan resource pada `BE-LAB-04`.**

Modul Laboratorium membutuhkan dua tabel data induk baru: **batas nilai pemeriksaan** dan
**pilihan hasil terbatas**. `QBE-NAM-004` melarang menyimpulkan prefix sendiri, jadi blueprint
tidak memutuskan.

> **✅ DISETUJUI 2026-09-02 — pakai `Lab`.** Kedua tabel bernama `LabValueBound` dan
> `LabValueOption`. Prefix `Mst` tidak dipakai. Seluruh artefak blueprint sudah disesuaikan, dan
> keputusannya tercatat pada *Catatan perubahan lifecycle* registry.
>
> Usulan aslinya beserta buktinya tetap ditulis di bawah sebagai dasar keputusan.

**Alasannya bukan selera, melainkan perilaku checker yang sudah berjalan.**
`tooling/qbe/Invoke-QbeConformanceCheck.ps1` menentukan pemilik entity **dari letak foldernya**:

| Langkah checker | Hasil untuk `Areas/HealthServices/LaboratoryManagement/Models/` |
|---|---|
| Ambil segmen modul dari path | `laboratorymanagement` |
| Cari baris registry yang cocok | Baris `LaboratoryManagement / Laboratory`, prefix `Lab` |
| Baris `Master / Reference` ikut dipertimbangkan? | **Tidak** — aliasnya `master` dan `reference`, tidak cocok segmen path mana pun |
| Uji prefix | Entity wajib diawali `Lab` |

Artinya `MstLabValueBound` yang diletakkan di folder Laboratorium akan dilaporkan sebagai
pelanggaran `QBE-MOD-002`. Prefix `Mst` hanya sah bila tabelnya benar-benar pindah ke folder
Master Data dan kepemilikannya diserahkan ke Master Data — bertentangan dengan pernyataan
blueprint bahwa kedua tabel ini milik Laboratorium.

**Preseden `MstLabRejectionReason` justru memperkuat, bukan melemahkan.** Berkasnya ada di
`Areas/HealthServices/LaboratoryManagement/Models/MstLabRejectionReason.cs` — pola yang sama
persis, dan menurut aturan yang berlaku sekarang ia **tidak konform**. Ia lolos hanya karena
checker mengecualikan legacy yang tidak disentuh. Mengikutinya untuk tabel baru berarti
menyalin cacat yang sudah ada.

---

## 4. Ringkasan

| No | Yang diminta | Yang tertahan | Bentuk jawaban |
|---:|---|---|---|
| 1 | Rules root memuat dokumen tata kelola — pilihan A atau B | **Seluruh** task backend dan frontend | Pilih A atau B |
| 2 | Lifecycle `PLANNED` → `ACTIVE` | Seluruh entity `Lab*` dan seluruh migration | Naikkan, atau nyatakan perlu ditinjau |
| 3 | Prefix data induk — usulan `Lab` | `BE-LAB-02`, penamaan pada `BE-LAB-04` | Setuju atau tolak |

**Nomor 1 yang paling luas akibatnya.** Tanpa itu, menaikkan lifecycle pada nomor 2 tidak
membuat satu task pun bisa dijalankan — gerbang `AGENTS.md` tetap menghentikannya lebih dulu.

**Yang tidak diminta:** persetujuan atas rancangan Laboratorium (sudah disetujui pemilik
modulnya), izin menulis ke tabel modul lain (Laboratorium tidak akan menulis), dan perubahan
cara kerja modul lain.

---

## 5. Rujukan

| Dokumen | Isi |
|---|---|
| `approval-requests/2026-09-01-permintaan-koordinasi-lintas-modul.md` | Permintaan lengkap 11 butir untuk seluruh penerima |
| `roadmap/backend-roadmap.md` bagian 1 | Tabel gerbang beserta apa yang tertahan tiap penghambat |
| `roadmap/traceability.md` bagian 5 | Daftar penahan dan pencabutnya |
| `blueprint-manifest.md` | Revision 22, `active_blockers` |
