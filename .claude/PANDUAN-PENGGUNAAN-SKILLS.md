# Panduan Penggunaan AI Skills Pengembangan Modul Quilvian

| Field | Nilai |
| --- | --- |
| Status | Canonical usage guide |
| Tanggal | 2026-08-13 |
| Cakupan | Backend dan frontend, dari discovery sampai readiness |
| Registry canonical | `NewQuilvianSystemBackend/docs/system-registry/` |
| Blueprint canonical | `NewQuilvianSystemBackend/docs/module-blueprints/<module>/` |
| Skill canonical | 7 backend + 1 frontend |
| Adapter frontend | Tidak ada, dihapus pada 2026-08-13 |

## 1. Tujuan

Gunakan panduan ini untuk menjalankan delapan skill sebagai satu workflow pengembangan modul,
baik untuk IGD, pengkajian rawat inap, maupun modul Quilvian lain. Workflow dimulai dari
pemindaian seluruh sistem, dilanjutkan wawancara kebutuhan, membaca kemampuan project yang
sudah ada secara mendalam, membuat blueprint dan ERD,
menyusun roadmap, mengimplementasikan task backend/frontend, lalu memverifikasi kesiapan
end-to-end.

Skill tidak menggantikan approval manusia. Product/domain owner, API owner, security owner,
dan frontend authority tetap memutuskan area sesuai kewenangannya.

## 2. Cara Claude Code menemukan dan menjalankan skill

Buka satu sesi dari backend sambil menambahkan frontend:

```bash
cd NewQuilvianSystemBackend
claude --add-dir ../QuilvianSystemFrontendDev
```

Sesi ini memuat **kedelapan** skill sekaligus: tujuh dari `.claude/skills/` backend, dan
`build-module-frontend` dari frontend. `--add-dir` memuat `.claude/skills/` milik direktori
yang ditambahkan secara otomatis, sekaligus memberi akses baca dan tulis ke source-nya.

Ketik `/` untuk melihat daftar skill yang terdeteksi atau panggil secara eksplisit dengan
`/nama-skill`.

Contoh:

```text
/grill-me Mulai perancangan modul pengkajian rawat inap dari awal.
```

Claude Code juga dapat memilih skill secara implisit apabila permintaan cocok dengan `description`,
tetapi pemanggilan eksplisit lebih disarankan untuk workflow ini. Jika perubahan skill belum
terlihat, restart/reopen sesi Claude Code. Perilaku discovery dan pemanggilan ini mengikuti
[Claude Code Docs — Agent Skills](https://docs.claude.com/en/docs/claude-code/skills).

Membuka sesi dari folder induk `Quilvian` saja **tidak** cukup. Skill di subfolder tidak
dimuat saat sesi dimulai; ia baru muncul setelah Claude menyentuh file di subfolder itu, dan
sebelum itu tidak tampil di daftar. Gunakan `--add-dir` seperti di atas.

Perintah tersebut memerlukan Claude Code versi 2.1.203 atau lebih baru.

## 3. Daftar skill dan tanggung jawab

Lokasi setiap skill ditentukan oleh satu aturan:

> **Lokasi skill = tempat skill menulis hasilnya, bukan tempat ia membaca.**

Membaca boleh lintas repository. Menulis tidak pernah lintas repository.

| Urutan | Skill | Membaca | Menulis | Lokasi | Mengubah source aplikasi? |
| ---: | --- | --- | --- | --- | --- |
| 0 | `/scan-system-registry` | dua repo | `docs/system-registry/` | Backend | Tidak |
| 1 | `/grill-me` | registry + jawaban pengguna | `docs/module-blueprints/` | Backend | Tidak |
| 2 | `/trace-existing-capabilities` | dua repo | `docs/module-blueprints/` | Backend | Tidak |
| 3 | `/design-business-module` | blueprint | `docs/module-blueprints/` | Backend | Tidak |
| 4 | `/plan-module-delivery` | blueprint | `docs/module-blueprints/roadmap/` | Backend | Tidak |
| 5 | `/build-module-backend` | blueprint + backend | source backend | Backend | Ya |
| 6 | `/build-module-frontend` | blueprint + frontend | source frontend | Frontend | Ya |
| 7 | `/verify-module-readiness` | dua repo | `docs/module-blueprints/testing/` | Backend | Tidak |

Tujuh skill menulis ke backend, satu menulis ke frontend. Tidak ada adapter dan tidak ada
mekanisme sidik jari; seluruh skill tersedia dalam satu sesi melalui `--add-dir`.

Tiga skill membaca dua repository sekaligus, yaitu `/scan-system-registry`,
`/trace-existing-capabilities`, dan `/verify-module-readiness`. Ketiganya tetap tinggal di
backend karena hasil auditnya adalah satu dokumen gabungan yang tidak boleh punya dua salinan.

`/scan-system-registry` berada di urutan 0 karena ia berlaku untuk **seluruh sistem**, bukan
untuk satu modul. Ia dijalankan sekali, lalu hasilnya dipakai semua modul. Rinciannya ada pada
[rule-prascan](rules/rule-prascan/README.md).

`/verify-module-readiness` **tidak pernah** mengubah source, tanpa pengecualian. Perbaikan
temuan dikembalikan ke `/build-module-backend` atau `/build-module-frontend` agar audit tetap
netral.

## 4. Persiapan sebelum mulai

Pastikan:

1. `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` tersedia sebagai sibling dalam workspace yang
   sama;
2. nama modul dan tujuan awal dapat dijelaskan, walaupun belum lengkap;
3. owner bisnis/domain dan calon approver dapat diidentifikasi;
4. perubahan worktree existing sudah diketahui agar tidak tertimpa;
5. akses database/runtime eksternal dinyatakan tersedia atau tidak;
6. build, test, migration, credential, dan environment yang boleh digunakan sudah jelas.

Gunakan nama folder modul dalam kebab-case, misalnya:

- `igd`;
- `pengkajian-rawat-inap`;
- `manajemen-kamar-operasi`.

## 5. Artefak canonical

Ada dua kelompok artefak. Yang pertama berlaku untuk seluruh sistem dan hanya ada satu:

```text
NewQuilvianSystemBackend/docs/system-registry/
├── registry-manifest.md
├── 01-peta-area-dan-modul.md
├── 02-entity-terdaftar.md
├── 03-kepemilikan-data-bersama.md
├── 04-kavling-nama-dan-endpoint.md
├── 05-zona-konflik.md
└── 06-indeks-entity.md
```

Yang kedua berlaku per modul. Semua keputusan dan bukti lintas repo disimpan satu kali di
backend:

```text
NewQuilvianSystemBackend/docs/module-blueprints/<module>/
├── blueprint-manifest.md
├── 00-interview-decisions.md
├── 01-existing-capability-map.md
├── 02-backend-architecture.md
├── 03-frontend-architecture.md
├── erd/
├── contracts/
├── roadmap/
│   ├── backend-roadmap.md
│   ├── frontend-roadmap.md
│   └── requirement-traceability.md
└── testing/
    ├── acceptance-test-matrix.md
    └── readiness-report.md
```

Daftar file di atas bersifat pasti, bukan pilihan. File yang tidak relevan bagi sebuah modul
tetap dibuat, berisi satu baris alasan yang menyebut sebabnya, misalnya "Tidak berlaku untuk
modul ini karena IGD tidak memanggil sistem luar". Jangan menghapus file tanpa jejak, dan
jangan pula membuat dokumen kosong tanpa keterangan. Dengan begitu pembaca selalu dapat
membedakan file yang memang tidak diperlukan dari file yang terlupa ditulis.

Frontend mereferensikan blueprint ID, revision, decision ID, task ID, dan contract version;
frontend tidak menyimpan salinan aturan canonical.

## 6. Alur lengkap dari awal sampai tuntas

### Tahap 0 — Pindai sistem dengan `/scan-system-registry` (wajib)

Jalankan sekali sebelum modul apa pun dibahas:

```text
/qv-scan full
```

Skill memetakan seluruh area, modul, entity, tingkat kesiapan, kepemilikan data, kavling nama,
grup endpoint, dan zona konflik pada kedua repository.

Output minimum:

```text
docs/system-registry/registry-manifest.md
docs/system-registry/01-peta-area-dan-modul.md
docs/system-registry/02-entity-terdaftar.md
docs/system-registry/03-kepemilikan-data-bersama.md
docs/system-registry/04-kavling-nama-dan-endpoint.md
docs/system-registry/05-zona-konflik.md
docs/system-registry/06-indeks-entity.md
```

Registry hanya memuat **keadaan nyata**. Usulan entity baru, prioritas, dan urutan implementasi
tidak boleh masuk ke sini; itu keluaran wawancara dan desain.

Untuk pemakaian berikutnya cukup `/qv-scan refresh`, yang hanya menyisir berkas yang berubah
sejak commit terakhir yang tercatat pada manifest.

Tahap ini selesai bila manifest berstatus `SEGAR` terhadap `HEAD` kedua repository.

### Tahap 1 — Wawancara awal dengan `/grill-me`

`/grill-me` **berhenti sebelum pertanyaan pertama** bila registry belum ada atau SHA-nya sudah
berbeda dari `HEAD`. Bila registry segar, skill membuka sesi dengan Kartu Konteks Pra-Wawancara
berisi modul yang bersinggungan, entity yang sudah siap dipakai ulang, entity yang belum
lengkap, zona konflik terkait, dan daftar pertanyaan yang tidak akan diajukan karena sudah
terjawab registry. Aturan lengkapnya ada di
[rule-prascan](rules/rule-prascan/aturan-prascan-modul.md).

Jalankan dari backend:

```text
/grill-me

Mulai Scope Pass untuk modul <nama-modul>.
Tujuan awal: <hasil bisnis yang diharapkan>.
Wawancarai saya secara kritis satu pertanyaan per giliran.
Jangan menulis source code. Simpan keputusan pada blueprint canonical.
```

Skill akan menggali scope, aktor, ownership, invariant, state, exception, permission,
privacy, integration, failure behavior, acceptance criteria, dan kewenangan UI.

Output minimum:

```text
docs/module-blueprints/<module>/00-interview-decisions.md
```

Tahap ini selesai bila tujuan dan batas audit existing sudah cukup jelas. Pertanyaan yang
dapat dijawab dari source tidak perlu ditanyakan kepada user; bawa pertanyaan tersebut ke
tahap audit.

### Tahap 2 — Audit existing dengan `/trace-existing-capabilities`

```text
/trace-existing-capabilities

Audit kemampuan existing untuk blueprint <module> pada backend dan frontend.
Telusuri tabel/entity, relasi, migration, API, DI, permission, route, UI consumer,
integrasi upstream/downstream, dan test secara read-only.
Jangan build dan jangan memperbaiki kode.
```

Skill wajib mencari reuse sebelum mengusulkan tabel baru, terutama untuk patient, doctor,
employee, encounter/admission, insurance, procedure, prescription, room/bed, dan master
bersama.

Output minimum:

```text
docs/module-blueprints/<module>/01-existing-capability-map.md
```

Setiap capability diklasifikasikan sebagai `Ready to reuse`, `Reuse with adapter`, `Extend`,
`Repair`, `Missing`, `Conflict`, atau `Unknown`, disertai repo, path, line/symbol, dan commit
SHA.

Tahap ini selesai bila capability yang relevan memiliki status dan bukti, serta conflict atau
unknown telah menjadi closure question.

### Tahap 3 — Tutup keputusan dengan `/grill-me` Closure Pass

```text
/grill-me

Jalankan Closure Pass untuk blueprint <module>.
Baca capability map, lalu tanyakan hanya conflict, unknown, ownership, dan keputusan yang
memblokir desain. Perbarui decision log tanpa menghapus histori.
```

Product/domain owner harus memberi approval nyata terhadap keputusan kritis. Jangan mengubah
status menjadi approved hanya karena agent telah menulis dokumen.

Jika source berubah sejak audit, jalankan impact scan:

```text
/trace-existing-capabilities

Lakukan impact scan read-only untuk blueprint <module> karena source SHA berubah.
Perbarui hanya capability dan kontrak as-is yang terdampak.
```

Tahap ini selesai bila invariant, ownership, source of truth, exception utama, dan kewenangan
UI yang dibutuhkan desain tidak lagi berstatus open/conflict.

### Tahap 4 — Desain modul dengan `/design-business-module`

```text
/design-business-module

Susun blueprint target untuk <module> berdasarkan decision log approved dan capability map.
Hasilkan arsitektur backend, arsitektur frontend, ERD per bounded context, API/integration
contract, state transition, validation, permission/audit, dan acceptance-test strategy.
Jangan menulis kode, flowchart, atau use-case diagram.
```

Output yang relevan:

- `blueprint-manifest.md`;
- `02-backend-architecture.md`;
- `03-frontend-architecture.md`;
- `erd/*.md`;
- `contracts/*.md`;
- `testing/acceptance-test-matrix.md`.

Pisahkan as-is contract dari to-be contract. ERD menandai entity sebagai `Existing`,
`Extend`, `New`, atau `Adapter/View` beserta owner. Jangan membuat salinan patient/doctor atau
master lintas domain.

Tahap ini menghasilkan desain `draft`. Approval tetap dilakukan manusia.

### Tahap 5 — Approval blueprint dan kontrak

Sebelum planning, owner berwenang harus meninjau:

- business rule dan state transition;
- arsitektur dan ownership data;
- ERD serta migration/data impact;
- API/integration contract dan compatibility;
- permission, security, privacy, audit, retention;
- authority UI dan area `DEV_DISCRETION`;
- acceptance-test matrix.

Catat `approved_by`, `approved_at`, revision, contract version, input hash, artifact hash,
backend SHA, dan frontend SHA. Jika ada perubahan material setelah approval, buat revision
atau contract version baru; jangan menimpa histori.

### Tahap 6 — Roadmap dengan `/plan-module-delivery`

```text
/plan-module-delivery

Buat roadmap delivery blueprint <module> yang sudah approved.
Pisahkan backend dan frontend, gunakan vertical slice, dan hubungkan setiap task ke
requirement ID, decision ID, contract version, acceptance criteria, test, dependency,
risiko, owner, dan Definition of Done.
```

Output:

```text
docs/module-blueprints/<module>/roadmap/backend-roadmap.md
docs/module-blueprints/<module>/roadmap/frontend-roadmap.md
docs/module-blueprints/<module>/roadmap/requirement-traceability.md
```

Backend dan frontend boleh bekerja paralel hanya bila contract version terkait sudah
approved dan hash-nya dikunci. Task harus kecil dan dapat diuji; jangan memakai satu task
besar seperti “buat seluruh backend modul”.

### Tahap 7 — Implementasi satu task backend

Mulai sesi Claude Code dari `NewQuilvianSystemBackend`, lalu panggil task tertentu:

```text
/build-module-backend

Implementasikan hanya task <BE-TASK-ID> dari blueprint <module>.
Gunakan contract version <version> dan acceptance criteria pada roadmap.
Pertahankan perubahan user yang tidak terkait. Jalankan verifikasi yang diizinkan dan buat
laporan backend, tetapi jangan melakukan git add/commit/push.
```

Skill memeriksa gate approval, revision/hash, dependency, reuse, dan scope sebelum mengubah
source. Satu pemanggilan hanya mengerjakan satu task backend.

Task backend belum selesai bila hanya build sukses. Task selesai setelah acceptance criteria,
test/evidence, laporan perubahan, dan traceability diperbarui.

### Tahap 8 — Implementasi satu task frontend

Mulai sesi terpisah dari `QuilvianSystemFrontendDev`:

```text
/build-module-frontend

Implementasikan hanya task <FE-TASK-ID> dari blueprint <module>.
Gunakan contract version <version>, authority UI, dan acceptance criteria yang approved.
Gunakan komponen serta pola project existing. Verifikasi seluruh state yang relevan dan
jangan mengubah backend.
```

Frontend wajib menangani state yang relevan: loading, empty, error, retry, success, disabled,
unauthorized/forbidden, validation, stale data, dan duplicate submit. Jangan menampilkan UUID
atau identifier teknis sebagai label pengguna.

Jika controller saat ini berbeda dari target contract approved, hentikan bagian terkait dan
buat contract delta. Frontend tidak otomatis mengikuti perubahan backend yang belum
disetujui.

Ulangi tahap 7 dan 8 per task/vertical slice. Jangan meminta satu skill mengimplementasikan
seluruh roadmap sekaligus.

### Tahap 9 — Verifikasi readiness

Setelah satu slice terintegrasi atau seluruh roadmap dianggap selesai:

```text
/verify-module-readiness

Audit readiness blueprint <module> secara read-only.
Telusuri requirement -> decision -> design/ERD -> contract -> task -> code/config -> test.
Pisahkan scaffold progress dari end-to-end readiness dan berikan blocker serta verdict.
```

Output:

```text
docs/module-blueprints/<module>/testing/readiness-report.md
```

Verdict:

- `READY`: semua invariant kritis, runtime dependency, dan acceptance scenario terbukti;
- `READY_WITH_CONDITIONS`: fungsi utama terbukti, dengan risiko terbatas, owner, dan mitigasi;
- `NOT_READY`: masih ada blocker bisnis, security/privacy, data, contract, runtime, atau test.

## 7. Loop perbaikan setelah readiness

Gunakan jenis gap untuk menentukan skill berikutnya:

| Temuan | Kembali ke skill |
| --- | --- |
| Registry sistem sudah tidak segar, atau ada area/modul baru | `/scan-system-registry` mode `refresh` atau `full` |
| Keputusan/owner/invariant belum jelas | `/grill-me` Amendment/Closure Pass |
| Capability atau SHA stale | `/trace-existing-capabilities` impact scan |
| Arsitektur/ERD/contract salah atau kurang | `/design-business-module` revision baru |
| Task atau dependency belum tercakup | `/plan-module-delivery` |
| Gap implementasi backend | `/build-module-backend` untuk satu task |
| Gap implementasi frontend | `/build-module-frontend` untuk satu task |
| Perbaikan sudah selesai | `/verify-module-readiness` ulang |

Jangan memperbaiki source melalui `/verify-module-readiness`; skill tersebut menjaga audit
tetap netral.

## 8. Kapan modul dianggap tuntas

Modul hanya dianggap tuntas bila:

1. decision dan blueprint material approved oleh owner yang tepat;
2. capability map serta source SHA tidak stale;
3. ERD dan kontrak target memiliki version/hash yang berlaku;
4. setiap requirement memiliki task dan evidence pada traceability;
5. task backend/frontend terkait memenuhi Definition of Done;
6. migration, seed, DI, permission, configuration, dan integration dependency terbukti;
7. acceptance path, exception/failure path, contract, integration, dan E2E yang kritis lulus;
8. readiness verdict `READY`, atau `READY_WITH_CONDITIONS` yang secara eksplisit diterima
   owner beserta mitigasinya;
9. tidak ada blocker security, privacy, integritas data, atau penutupan workflow;
10. dokumentasi, laporan perubahan, dan bukti test dapat ditelusuri ke commit yang diaudit.

## 9. Melanjutkan pekerjaan pada sesi baru

Jangan memulai ulang dari nol. Gunakan prompt:

```text
/<skill-yang-sesuai>

Lanjutkan blueprint <module> revision <revision>.
Baca manifest, artefak upstream, roadmap, dan readiness report terakhir.
Validasi hash serta source SHA sebelum melanjutkan. Kerjakan hanya <scope/task-id>.
```

Jika revision/hash/SHA berbeda, hentikan area terdampak dan lakukan impact scan. Jangan
mengandalkan ringkasan chat lama sebagai sumber kebenaran; artefak canonical adalah sumber
handoff.

## 10. Contoh awal untuk modul berbeda

### IGD

```text
/grill-me Mulai Scope Pass modul IGD. Fokus pada registrasi darurat, triage/retriage,
observasi, tindakan, disposition, transfer, billing, dan penutupan encounter. Cari reuse
patient, doctor, encounter, room/bed, prescription, dan master existing.
```

### Pengkajian rawat inap

```text
/grill-me Mulai Scope Pass modul pengkajian rawat inap. Fokus pada ownership encounter dan
admission existing, assessment per profesi, correction/reopen, CPPT, permission, audit, dan
handoff klinis. Jangan membuat episode pasien baru sebelum capability audit.
```

## 11. Troubleshooting

### Skill tidak tampil

1. Pastikan current working directory berada di dalam repo yang benar.
2. Ketik `/` dan periksa daftar skill yang terdeteksi.
3. Pastikan folder berada di `<repo>/.claude/skills/<skill>/SKILL.md`.
4. Restart/reopen Claude Code bila perubahan belum terbaca.

### Skill berhenti karena approval

Itu adalah perilaku yang benar. Minta owner yang relevan memutuskan item spesifik, lalu catat
status dan bukti approval. Jangan meminta agent menganggap draft sebagai approved.

### Skill berhenti karena source/hash stale

Jalankan impact scan. Bila contract atau business rule berubah, buat revision/version baru dan
ulang approval bagian terdampak.

### Ada perubahan yang tidak terkait di worktree

Pertahankan perubahan tersebut. Batasi implementasi pada task ID dan file yang diperlukan.
Jika overlap tidak aman, hentikan task dan minta arahan developer.

## 12. Pemeliharaan suite

- Setiap skill hanya ada di satu tempat. Tidak ada salinan, tidak ada adapter, dan tidak ada
  sidik jari SHA-256 yang perlu dihitung ulang.
- Mengubah prosedur skill cukup dikerjakan sekali, di file skill itu sendiri.
- Jalankan `quick_validate.py` untuk skill yang berubah.
- Forward-test suite pada minimal satu modul IGD dan satu modul non-IGD sebelum promosi ke
  global/plugin.
- Jangan menaruh user guide ini di dalam folder package skill individual; package tetap fokus
  pada instruksi yang dibaca agent.

## 13. Slash command pintasan

Selain memanggil skill langsung dengan `/nama-skill`, tersedia slash command di
`.claude/commands/` yang sudah membawa prompt tahap beserta pengamannya. Prefix `qv-` dipakai
agar tidak bentrok dengan nama skill.

| Command | Skill yang dipanggil | Argumen |
| --- | --- | --- |
| `/qv-scan` | `scan-system-registry` | `[full\|refresh\|focus <area>]` |
| `/qv-grill` | `grill-me` | `<module> [scope\|closure\|amendment] [tujuan]` |
| `/qv-trace` | `trace-existing-capabilities` | `<module> [impact-scan]` |
| `/qv-design` | `design-business-module` | `<module> [fokus]` |
| `/qv-plan` | `plan-module-delivery` | `<module> [revision]` |
| `/qv-build-be` | `build-module-backend` | `<BE-TASK-ID> <module> [contract version]` |
| `/qv-verify` | `verify-module-readiness` | `<module> [slice/scope]` |
| `/qv-lanjut` | dipilih sesuai konteks | `<module> <revision> <scope/task-id>` |

Contoh:

```text
/qv-grill pengkajian-rawat-inap scope Standarkan pengkajian awal per profesi
/qv-build-be BE-042 igd v3
```

Command hanya pintasan prompt. Prosedur, gate approval, dan batas kewenangan tetap berasal
dari `SKILL.md`; jangan memindahkan aturan canonical ke dalam file command.

## 14. Aturan output dokumentasi

Seluruh dokumen yang dihasilkan skill tunduk pada aturan canonical di
[`.claude/rules/rule-output/`](rules/rule-output/README.md):

1. Bahasa harus Bahasa Indonesia.
2. Gunakan bahasa yang mudah dipahami orang umum.
3. Jelaskan secara detail beserta contoh untuk hal yang sulit dipahami.
4. Bisnis proses harus dijelaskan dengan jelas dan urut.
5. Sajikan endpoint bergaya Swagger: judul grup persis nilai `[Tags(...)]` pada controller,
   diikuti tabel API.

Contoh judul grup yang dimaksud, diambil dari controller yang sudah ada:

```text
Corporate / Human Resource / Master Data / Allowance Type
```

Aturan lengkap ada di [aturan-output-dokumentasi.md](rules/rule-output/aturan-output-dokumentasi.md)
dan contoh penerapannya di [contoh-dokumentasi-modul.md](rules/rule-output/contoh-dokumentasi-modul.md).

Aturan ini mengikat bentuk dan gaya dokumen. Aturan ini tidak menggantikan gate approval atau
batas kewenangan pada `SKILL.md`.

## 15. Kebutuhan effort dan model per skill

Setiap `SKILL.md` memuat tabel **Effort dan model minimum**. Ringkasannya:

| Skill | Minimum effort | Model Claude minimum | Model GPT setara |
| --- | --- | --- | --- |
| `scan-system-registry` | `high` untuk `full`, `medium` untuk `refresh` | Claude Sonnet 5 | GPT-5, reasoning `high` |
| `grill-me` | `medium` | Claude Sonnet 5 | GPT-5, reasoning `medium` |
| `trace-existing-capabilities` | `high` | Claude Sonnet 5 | GPT-5, reasoning `high` |
| `design-business-module` | `high` | Claude Opus 5 | GPT-5, reasoning `high` |
| `plan-module-delivery` | `medium` | Claude Sonnet 5 | GPT-5, reasoning `medium` |
| `build-module-backend` | `high` | Claude Sonnet 5 | GPT-5 / GPT-5-Codex, reasoning `high` |
| `build-module-frontend` | `medium` | Claude Sonnet 5 | GPT-5 / GPT-5-Codex, reasoning `medium` |
| `verify-module-readiness` | `high` | Claude Opus 5 | GPT-5, reasoning `high` |

Claude Opus 5 disarankan untuk seluruh skill. Dua skill tidak menyediakan turunan model yang
lebih kecil, yaitu `design-business-module` dan `verify-module-readiness`, karena kesalahannya
mahal dan sulit terdeteksi.

## 16. Penawaran skill berikutnya

Setiap skill wajib menawarkan langkah berikutnya setelah pekerjaannya tuntas, beserta alasan
singkatnya. Skill hanya menawarkan; perpindahan tetap menunggu persetujuan pengguna.

Alur normalnya:

```text
/scan-system-registry (sekali, wajib, dan wajib segar)
  -> /grill-me (Scope Pass)
  -> /trace-existing-capabilities
  -> /grill-me (Closure Pass)
  -> /design-business-module
  -> [approval owner]
  -> /plan-module-delivery
  -> /build-module-backend dan /build-module-frontend per task
  -> /verify-module-readiness
```

Jika masih ada blocker, skill menyebutkan blocker tersebut lebih dulu dan tidak menawarkan
langkah maju seolah pekerjaan sudah tuntas.
