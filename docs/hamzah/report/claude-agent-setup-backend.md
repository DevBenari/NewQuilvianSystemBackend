# Repo Backend — Folder `.claude`, Larangan Push Otomatis, dan Dua Skill Master Data

| | |
|---|---|
| Tanggal | 2026-08-11 |
| Branch | `MHamzah` |
| Pemicu | Permintaan pemilik branch: backend perlu aturan agent sendiri, dan dua skill untuk mencari + menutup kekurangan master data |
| Migration | **Tidak ada** |
| Breaking change | **Tidak** — tidak ada kode aplikasi yang disentuh |

## Kenapa diubah

Sebelum ini repo backend tidak punya konfigurasi agent sama sekali. Semua aturan datang dari
repo frontend (`QuilvianFrontEnd/.claude/`), termasuk aturan yang mengatur backend. Dua
akibatnya:

1. **Aturan push tidak sesuai lagi.** `backend-workflow.md` RULE 2 mengizinkan Claude
   melakukan push mandiri ke `MHamzah`. Pemilik branch memutuskan memegang sendiri titik
   publikasi ke remote, jadi izin itu perlu dicabut — dan pencabutannya harus hidup di repo
   backend, bukan menumpang di repo frontend.
2. **Pekerjaan master data tidak punya prosedur.** Audit kekurangan backend dan
   pengerjaannya selama ini dilakukan ad hoc. Pola yang sudah terbukti di repo (helper
   filter tanggal, proyeksi `/options`, `GetActorNameMapAsync`) tidak tercatat di mana pun
   sebagai rujukan wajib, sehingga berisiko ditulis ulang berbeda-beda tiap entitas.

Ini tidak bisa diselesaikan di frontend: aturan yang mengikat pekerjaan di repo backend
harus ikut ter-commit di repo backend supaya terbaca tim backend.

## Endpoint yang terpengaruh

**Tidak ada.** Perubahan ini murni konfigurasi agent dan dokumentasi. Tidak ada controller,
DTO, model, migration, atau `Program.cs` yang disentuh.

## File yang disentuh

| File | Perubahan |
|---|---|
| `.claude/CLAUDE.md` | **Baru** — entry point agent untuk repo backend |
| `.claude/settings.json` | **Baru** — wiring hook + permission, `git push` masuk daftar `deny` |
| `.claude/hooks/guard-no-auto-push.mjs` | **Baru** — PreToolUse hook, menolak semua push backend |
| `.claude/rules/no-auto-push.md` | **Baru** — aturan keras: kerja berhenti di `git commit` |
| `.claude/rules/master-data-contract.md` | **Baru** — kontrak master data dari sisi backend |
| `.claude/skills/master-data-audit/SKILL.md` | **Baru** — skill audit kekurangan |
| `.claude/skills/master-data-set/SKILL.md` | **Baru** — skill pengerjaan |
| `docs/hamzah/report/claude-agent-setup-backend.md` | **Baru** — halaman ini |

Di luar repo ini (tidak ikut commit backend): `QuilvianFinal/.claude/settings.json` dan
`QuilvianFinal/CLAUDE.md` disesuaikan supaya hook backend ikut aktif saat sesi dibuka dari
folder root.

## Kontrak aturan

| Aturan | Perilaku |
|---|---|
| `git push` di backend | **Ditolak hook**, bentuk apa pun, tujuan mana pun — termasuk `origin MHamzah` |
| `git remote set-url/add/remove/rename` di backend | Ditolak |
| `gh pr create/merge/close`, `gh release`, `gh api` non-GET | Ditolak — bisa mempublikasikan tanpa lewat `git push` |
| `git add`, `commit`, `pull`, `fetch`, `rebase`, `checkout`, `reset`, `stash` | Diizinkan, semuanya lokal |
| `dotnet build` / `run` / `ef` | Diizinkan |
| `git push` di **frontend** | **Tidak terpengaruh** — hook hanya bereaksi pada konteks repo backend |

Hook mendeteksi konteks backend dari tiga arah: opsi `-C <path>`, `cd` yang mendahului
perintah dalam satu baris shell, dan `cwd` sesi. Perintah `gh` yang dijalankan dari dalam
folder backend tanpa menyebut nama repo juga tertangkap.

Hook ini berdampingan dengan `QuilvianFrontEnd/.claude/hooks/guard-backend-push.mjs` yang
masih mengizinkan `push origin MHamzah`. Keduanya dijalankan berurutan dan yang menolak
lebih dulu yang menang, sehingga hasil akhirnya: tidak ada push dari Claude sama sekali.

## Dua skill master data

| Skill | Masukan | Keluaran | Menyentuh kode? |
|---|---|---|---|
| `master-data-audit` | entitas / area / "semua" | Register kekurangan di `docs/hamzah/task/<topik>.md` | **Tidak** |
| `master-data-set` | ID tugas dari register, atau entitas + kebutuhan | Kode + laporan di `docs/hamzah/report/<topik>.md`, register diperbarui, commit | Ya |

Keduanya berpasangan dengan skill frontend yang sudah ada
(`QuilvianFrontEnd/.claude/commands/master-data-audit.md` dan `master-data-set.md`), bukan
menggantikannya. Skill frontend memeriksa dan memperbaiki UI; skill backend memeriksa dan
menutup kontrak API-nya.

`master-data-set` diwajibkan memperbarui register pada commit yang sama — supaya dokumen
register tidak menyesatkan pembaca berikutnya dengan menampilkan gap yang sebenarnya sudah
selesai.

## Dampak ke frontend

Tidak ada perubahan kode frontend. Yang berubah hanya cara kerja agent:

- Aturan push mandiri di `QuilvianFrontEnd/.claude/rules/backend-workflow.md` RULE 2 tidak
  lagi berlaku. Bagian lain file itu tetap berlaku penuh.
- `QuilvianFinal/CLAUDE.md` diperbarui supaya tidak lagi menyatakan push boleh mandiri, dan
  path laporan dikoreksi dari `docs/report/` ke `docs/hamzah/report/` sesuai keadaan repo.

## Cara menguji

Hook diuji lewat skrip yang memberi payload PreToolUse langsung ke `node`:

| Kasus | Harapan |
|---|---|
| `git -C QuilvianBackend push origin MHamzah` | ditolak |
| `cd QuilvianBackend && git push origin MHamzah` | ditolak |
| `git -C QuilvianBackend push origin HEAD:MHamzah` | ditolak |
| `git push` polos dengan cwd di backend | ditolak |
| `git -C QuilvianBackend remote set-url origin ...` | ditolak |
| `gh pr create --fill` dengan cwd di backend | ditolak |
| `git -C QuilvianBackend commit -m x` | lolos |
| `git -C QuilvianBackend add .` | lolos |
| `git -C QuilvianBackend pull --rebase` | lolos |
| `dotnet build QuilvianSystemBackend.csproj` | lolos |
| `git push origin HamzahV2` dengan cwd di frontend | lolos |

Catatan saat menguji: perintah tes tidak boleh memuat teks push backend secara literal —
hook akan menolak perintah tesnya sendiri. Taruh payload di file skrip, lalu jalankan
`node <skrip>`.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| Uji hook (11 kasus izin & tolak) | **11/11 lulus** — dijalankan 2026-08-11 |
| `dotnet build` | **Tidak dijalankan** — tidak ada kode aplikasi yang disentuh, `.claude/` tidak ikut dikompilasi |
| `.claude/` ikut ter-track git | Ya — `.gitignore` backend tidak memuat entri `.claude` |
| Skill dijalankan end-to-end | **Belum** — baru ditulis, belum dipakai pada pekerjaan nyata |
