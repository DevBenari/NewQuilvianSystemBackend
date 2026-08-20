# Panduan Penggunaan AI Skills Pengembangan Modul Quilvian

| Field | Nilai |
| --- | --- |
| Status | Canonical usage guide |
| Tanggal | 2026-08-13 |
| Cakupan | Backend dan frontend, dari discovery sampai readiness |
| Blueprint canonical | `NewQuilvianSystemBackend/docs/module-blueprints/<module>/` |
| Skill canonical | 6 backend + 1 frontend |
| Adapter frontend | 5 shared skills |

## 1. Tujuan

Gunakan panduan ini untuk menjalankan tujuh skill sebagai satu workflow pengembangan modul,
baik untuk IGD, pengkajian rawat inap, maupun modul Quilvian lain. Workflow dimulai dari
wawancara kebutuhan, membaca kemampuan project yang sudah ada, membuat blueprint dan ERD,
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

Sesi ini memuat **ketujuh** skill sekaligus: enam dari `.claude/skills/` backend, dan
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
| 1 | `/grill-me` | jawaban pengguna | `docs/module-blueprints/` | Backend | Tidak |
| 2 | `/trace-existing-capabilities` | dua repo | `docs/module-blueprints/` | Backend | Tidak |
| 3 | `/design-business-module` | blueprint | `docs/module-blueprints/` | Backend | Tidak |
| 4 | `/plan-module-delivery` | blueprint | `docs/module-blueprints/roadmap/` | Backend | Tidak |
| 5 | `/build-module-backend` | blueprint + backend | source backend | Backend | Ya |
| 6 | `/build-module-frontend` | blueprint + frontend | source frontend | Frontend | Ya |
| 7 | `/verify-module-readiness` | dua repo | `docs/module-blueprints/testing/` | Backend | Tidak |

Enam skill menulis ke backend, satu menulis ke frontend. Tidak ada adapter dan tidak ada
mekanisme sidik jari; seluruh skill tersedia dalam satu sesi melalui `--add-dir`.

Dua skill membaca dua repository sekaligus, yaitu `/trace-existing-capabilities` dan
`/verify-module-readiness`. Keduanya tetap tinggal di backend karena hasil auditnya adalah
satu dokumen gabungan yang tidak boleh punya dua salinan.

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

Semua keputusan dan bukti lintas repo disimpan satu kali di backend:

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

### Tahap 1 — Wawancara awal dengan `/grill-me`

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
docs/module-blueprints/<module>/roadmap/r…27263 tokens truncated…-- | --- |
| Minimum effort | `medium` |
| Model Claude minimum | Claude Sonnet 5 |
| Model Claude disarankan | Claude Opus 5 |
| Model GPT setara | GPT-5, reasoning `medium` |
| Alasan | Pemecahan vertical slice dan urutan dependency menuntut penalaran, tetapi seluruh bahannya sudah tersedia pada blueprint approved |

Jika sesi berjalan di bawah batas ini, beritahu pengguna sebelum mulai dan minta konfirmasi.

## Aturan output dokumentasi

Roadmap dan traceability wajib mengikuti
[aturan output dokumentasi](../../rules/rule-output/aturan-output-dokumentasi.md): Bahasa Indonesia,
bahasa yang mudah dipahami orang umum, penjelasan detail beserta contoh, bisnis proses yang
jelas, dan endpoint bergaya Swagger. Tulis outcome setiap task dengan kalimat yang dipahami
pemilik proses bisnis, bukan hanya nama layer teknis.

## Input wajib

Temukan blueprint canonical di `NewQuilvianSystemBackend/docs/module-blueprints/<module>/`, lalu baca:

1. manifest dan status approval;
2. decision log;
3. capability map beserta commit SHA kedua repository;
4. arsitektur backend dan frontend;
5. ERD per bounded context;
6. kontrak API/event target dan versinya.

Hentikan perencanaan yang bergantung pada keputusan atau kontrak berstatus `DRAFT`, `CONFLICT`, atau `OPEN`. Bila SHA sumber berubah sejak audit, lakukan impact scan dan catat hasilnya sebelum melanjutkan.

## Workflow

### 1. Bentuk vertical slice

Kelompokkan pekerjaan berdasarkan hasil bisnis yang bisa diverifikasi, bukan hanya berdasarkan layer teknis. Satu slice dapat memuat perubahan data, API, permission, UI, dan test yang diperlukan untuk satu hasil pengguna.

### 2. Pisahkan task backend dan frontend

Backend dan frontend boleh berjalan paralel hanya setelah kontrak target terkait berstatus `APPROVED` dan memiliki versi/hash yang dikunci. Jangan membuat frontend menebak payload yang belum disetujui.

### 3. Definisikan setiap task

Setiap task wajib memiliki:

- ID stabil (`BE-...` atau `FE-...`);
- outcome pengguna atau bisnis;
- requirement ID, decision ID, dan contract version yang dilayani;
- dependency dan urutan eksekusi;
- capability existing yang digunakan, diperluas, atau diperbaiki;
- lokasi perubahan yang diperkirakan;
- acceptance criteria yang dapat diuji;
- test dan bukti yang wajib dihasilkan;
- risiko, blocker, serta owner;
- Definition of Done.

Jangan mengunci bentuk menu, layout, atau gaya visual apabila tidak ada keputusan produk/UI yang disetujui. Tandai ruang tersebut sebagai `DEV_DISCRETION` dan tetap cantumkan invariant aksesibilitas, privasi, dan keamanan.

### 4. Susun dependency dan milestone

Urutkan fondasi kontrak, slice minimum yang dapat dipakai, integrasi lintas modul, hardening, lalu readiness. Hindari roadmap yang menyatakan semua model harus selesai sebelum semua API dan UI dimulai jika sebuah vertical slice dapat diuji lebih awal.

### 5. Tulis artefak canonical

Gunakan format di [roadmap-template.md](references/roadmap-template.md). Simpan satu kali di:

```text
NewQuilvianSystemBackend/docs/module-blueprints/<module>/roadmap/
  backend-roadmap.md
  frontend-roadmap.md
  requirement-traceability.md
```

Frontend hanya mereferensikan module ID, revision, task ID, dan contract version tersebut; jangan menyalin roadmap sebagai sumber kebenaran kedua.

## Guardrail

- Jangan menulis source code aplikasi dari skill ini.
- Jangan menganggap scaffold sama dengan fitur end-to-end.
- Jangan menyembunyikan dependency eksternal sebagai task implementasi biasa.
- Jangan menandai task selesai tanpa acceptance evidence.
- Jika requirement tidak terhubung ke task atau test, keluarkan sebagai coverage gap.

## Tawarkan skill berikutnya

Setelah roadmap tuntas dan disetujui, **selalu** tawarkan langkah berikutnya secara eksplisit,
lengkap dengan alasan singkatnya. Sebutkan task ID konkret yang paling siap dikerjakan, bukan
sekadar nama skill:

| Kondisi setelah roadmap | Skill yang ditawarkan |
| --- | --- |
| Ada task backend paling siap tanpa dependency terbuka | `/build-module-backend` untuk satu task ID tersebut |
| Kontrak terkait sudah `APPROVED` dan hash terkunci | `/build-module-frontend` untuk satu task frontend, boleh paralel |
| Satu vertical slice sudah punya bukti implementasi | `/verify-module-readiness` |
| Roadmap menemukan requirement tanpa task atau tanpa test | `/design-business-module` atau `/grill-me` sesuai jenis gap |

Satu pemanggilan skill build hanya untuk satu task. Tawarkan, jangan jalankan sendiri, dan
jangan menggabungkan beberapa task menjadi satu permintaan.

