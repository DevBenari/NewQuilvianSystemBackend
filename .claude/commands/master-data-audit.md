---
description: Audit master data backend terhadap kebutuhan frontend, lalu tulis register kekurangan di docs/hamzah/task/
argument-hint: <entitas | area | semua>
---

# Audit Master Data Backend

Cakupan yang diminta: **$ARGUMENTS**

Kalau bagian di atas kosong, tanyakan dulu cakupannya sebelum mulai — audit menyeluruh
60+ entitas sangat berbeda biayanya dari audit satu entitas.

## Yang harus dilakukan

Baca **`.claude/skills/master-data-audit/SKILL.md`** dan ikuti seluruh langkahnya. File itu
sumber kebenaran prosedurnya — jangan menyusun ulang alur sendiri.

Aturan yang mengikat, baca sebelum menilai apa pun:

| Dokumen | Isi |
|---|---|
| `.claude/rules/master-data-contract.md` | Kontrak backend — **ini yang dinilai** |
| `.claude/rules/git-read-only.md` | Batas pekerjaan Claude |
| `QuilvianFrontEnd/.claude/rules/rules-master-data.md` | Kontrak UI asli, acuan tertinggi |

## Dua batas yang paling sering dilanggar

1. **Skill ini tidak mengubah satu baris kode pun.** Yang dihasilkan hanya satu dokumen
   register di `docs/hamzah/task/<topik>.md`. Kalau di tengah audit tergoda memperbaiki
   sesuatu — jangan. Catat sebagai temuan, biarkan `/master-data-set` yang mengerjakan.

2. **Git di backend hanya boleh dibaca.** Jangan menjalankan `git add` atau `git commit`
   untuk register yang baru ditulis. Sajikan perintahnya supaya user yang menjalankan.

## Sebelum menyebut sesuatu "kekurangan backend"

Buktikan dulu bahwa frontend memang tidak bisa menyelesaikannya sendiri. Jebakan yang sudah
berkali-kali terjadi: select relasi kosong yang dikira backend belum punya `/options`,
padahal endpointnya ada dan frontend tidak pernah memanggilnya. Itu temuan **frontend**,
bukan backend.
