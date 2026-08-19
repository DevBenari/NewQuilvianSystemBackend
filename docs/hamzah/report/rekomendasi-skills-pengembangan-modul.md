# Revisi Pembagian Skill Suite Pengembangan Modul

| | |
|---|---|
| Tanggal | 2026-08-12 |
| Branch | `MHamzah` |
| Pemicu | Permintaan membagi rekomendasi skill antara project backend dan frontend |
| Cakupan | Dokumentasi saja |
| Database/runtime migration | Tidak ada |
| Breaking change | Tidak ada pada aplikasi runtime |

## Kenapa diubah

Versi sebelumnya merekomendasikan tujuh skill dalam satu struktur `.claude/skills/` tanpa
menentukan repository pemiliknya. Karena QuilvianBackend dan QuilvianFrontEnd adalah dua
Git repository terpisah, struktur tersebut berisiko menimbulkan dua salinan business rule,
ERD, API contract, roadmap, dan readiness verdict yang berbeda.

Dokumen diperbarui menggunakan model hybrid:

- lima shared skill mengelola interview, discovery, desain, roadmap, dan verification
  lintas kedua repository;
- `build-module-backend` hanya berada di backend;
- `build-module-frontend` hanya berada di frontend;
- untuk tahap awal, source package shared skill dititipkan pada
  `QuilvianBackend/agent-skills/` sebagai lokasi yang dapat di-track;
- `.claude/` yang sedang di-ignore hanya menjadi installation target lokal/generated;
- frontend menggunakan wrapper tipis dan tidak menyalin prosedur shared.

Secara konseptual pembagiannya **5 shared + 2 repo-local**. Secara fisik tahap awal menjadi
**6 paket di backend + 1 paket di frontend**.

`docs/agency/rekomendasi-skills-pengembangan-modul.md` ditetapkan sebagai dokumen
rekomendasi canonical. Jika kelak ditemukan salinan bernama sama pada folder lain, salinan
tersebut tidak diperlakukan sebagai sumber aturan kedua.

## Endpoint yang terpengaruh

Tidak ada. Tidak ada controller, DTO, service, entity, configuration, migration, dependency
injection, route frontend, atau API runtime yang diubah.

## Kontrak parameter / field

Tidak ada kontrak runtime yang berubah. Spesifikasi dokumentasi menambahkan metadata
artefak yang kelak wajib dipakai oleh skill suite:

- `blueprint_id` dan `revision`;
- status `draft`, `approved`, atau `superseded`;
- owner, `approved_by`, dan `approved_at`;
- commit SHA backend dan frontend;
- `contract_version`;
- `artifact_hashes`, `input_revisions`, dan `input_hashes`.

Metadata tersebut digunakan untuk mendeteksi contract drift serta mencegah implementation
skill menjalankan input yang stale atau belum disetujui.

## File yang disentuh

| File | Perubahan |
|---|---|
| `docs/agency/rekomendasi-skills-pengembangan-modul.md` | Draft existing yang belum tracked diperbarui — pembagian hybrid, lokasi canonical, wrapper, manifest, handoff, dan guardrail anti-drift |
| `docs/hamzah/report/rekomendasi-skills-pengembangan-modul.md` | Baru — laporan perubahan ini |

Tidak ada salinan `docs/skils-global/` pada worktree saat verifikasi. Tidak ada dokumen lain
atau source frontend yang diubah dalam pekerjaan ini.

## Dampak ke frontend

Tidak ada dampak runtime saat ini. Ketika suite benar-benar dibuat:

- source package `build-module-frontend` ditempatkan pada
  `QuilvianFrontEnd/agent-skills/build-module-frontend/`, lalu di-install/sync ke
  `.claude/skills/` bila runner membutuhkannya;
- shared skill tetap mempunyai satu sumber canonical;
- command frontend untuk shared skill hanya menjadi wrapper tipis;
- command `QuilvianFrontEnd/.claude/commands/grill-me.md` yang lama perlu dimigrasikan
  karena saat ini masih mencampur audit API dan interview serta membawa kebijakan lama;
- menu, route, layout, dan tampilan tetap mengikuti authority matrix, arahan atasan/product/
  UI lead, design system, dan diskresi developer yang sah.

Business rule, ERD, API/integration contract, roadmap, dan readiness verdict disimpan satu
kali di `QuilvianBackend/docs/module-blueprints/<module-name>/`. Frontend hanya
mereferensikan `blueprint_id`, revision, requirement/decision ID, dan contract version.

## Cara menguji

1. Pastikan tujuh nama skill tetap tersedia tepat satu kali pada daftar utama.
2. Pastikan lima skill diklasifikasikan shared dan dua skill repo-local.
3. Pastikan lokasi tahap awal menyatakan enam paket backend dan satu paket frontend.
4. Pastikan wrapper frontend tidak boleh menyalin prosedur canonical.
5. Pastikan blueprint manifest, contract version, approval, SHA, dan stale-input guard ada.
6. Pastikan promosi global mensyaratkan IGD, rawat inap, dan satu project non-Quilvian.
7. Pastikan flowchart dan use-case diagram tetap dikecualikan.
8. Pastikan code fence Markdown seimbang dan hierarki heading dapat dibaca.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| Struktur Markdown | **Lulus** — heading utama rekomendasi bernomor 1–14 lengkap dan unik; code fence seimbang |
| Konsistensi pembagian skill | **Lulus** — tepat 7 skill, terdiri dari 5 shared + 2 repo-local dan source package 6 backend + 1 frontend |
| Pemeriksaan source/install path | **Lulus** — `agent-skills/` tidak di-ignore; `.claude/` di-ignore dan hanya diposisikan sebagai installation target lokal/generated |
| Pemeriksaan file yang berubah | **Lulus** — draft agency yang belum tracked diperbarui dan laporan ini dibuat; tidak ada source aplikasi yang diubah |
| Review independen | **Lulus setelah revisi** — temuan lokasi versioning, canonical artifact, metadata, dan wording laporan telah ditutup |
| `dotnet build` | Tidak dijalankan — perubahan dokumentasi saja |
| Implementasi paket skill | Belum — di luar scope revisi dokumentasi |
