# Repo Backend — Folder `.claude`, Git Hanya-Baca, dan Dua Skill Master Data

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

1. **Aturan git tidak sesuai lagi.** `backend-workflow.md` RULE 2 mengizinkan Claude
   melakukan push mandiri ke `MHamzah`, dan RULE 3 menjadikan `add` + `commit` + `push`
   bagian dari alur kerja Claude. Pemilik branch memutuskan memegang sendiri seluruh
   operasi git — bukan hanya push. Pencabutannya harus hidup di repo backend, bukan
   menumpang di repo frontend.
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
| `.claude/settings.json` | **Baru** — wiring hook + daftar `deny` untuk seluruh verb git yang menulis |
| `.claude/hooks/guard-git-read-only.mjs` | **Baru** — PreToolUse hook, allowlist verb git hanya-baca |
| `.claude/rules/git-read-only.md` | **Baru** — aturan keras: git di backend hanya boleh dibaca |
| `.claude/rules/master-data-contract.md` | **Baru** — kontrak master data dari sisi backend |
| `.claude/skills/master-data-audit/SKILL.md` | **Baru** — skill audit kekurangan |
| `.claude/skills/master-data-set/SKILL.md` | **Baru** — skill pengerjaan |
| `.claude/commands/master-data-audit.md` | **Baru** — slash command, wrapper tipis ke skill |
| `.claude/commands/master-data-set.md` | **Baru** — slash command, wrapper tipis ke skill |
| `docs/hamzah/report/claude-agent-setup-backend.md` | **Baru** — halaman ini |

Di luar repo ini (tidak ikut commit backend): `QuilvianFinal/.claude/settings.json` dan
`QuilvianFinal/CLAUDE.md` disesuaikan supaya hook backend ikut aktif saat sesi dibuka dari
folder root.

> **Catatan riwayat.** Versi pertama aturan ini hanya melarang `git push`, dengan nama file
> `rules/no-auto-push.md` + `hooks/guard-no-auto-push.mjs` (commit `61a4d98`). Cakupannya
> ternyata kurang: `add`, `commit`, `pull`, `fetch`, `checkout`, `branch`, `reset`,
> `restore`, dan `stash` juga harus dipegang user. Kedua file itu diganti nama sesuai
> cakupan barunya, jadi riwayat akan memperlihatkan penambahan lalu penggantian.

## Kontrak aturan

Pemisahnya: **perintah git yang mengubah index, working tree, branch, atau remote adalah
milik user.** Yang tersisa untuk Claude hanya perintah yang menampilkan keadaan.

| Kategori | Perilaku |
|---|---|
| `status`, `log`, `diff`, `show`, `blame`, `rev-parse`, `rev-list`, `ls-files`, `check-ignore` | **Diizinkan** — hanya membaca |
| `remote -v` / `show`, `config --get` / `--list`, `reflog show` | Diizinkan — bentuk bacanya saja |
| `add`, `commit`, `push`, `pull`, `fetch`, `checkout`, `switch`, `branch`, `reset`, `restore`, `stash`, `merge`, `rebase`, `cherry-pick`, `clean`, `rm`, `mv`, `tag` | **Ditolak hook** |
| `remote add/remove/set-url/rename`, `config <key> <value>` | Ditolak |
| `gh pr create/merge/close`, `gh release`, `gh api` non-GET | Ditolak — mengubah state remote tanpa lewat git |
| Seluruh perintah git di **repo frontend** | **Tidak terpengaruh** |
| `dotnet build` / `run` / `ef`, tulis file, `rg`, `ls` | Tidak diperiksa sama sekali |

Hook memakai **allowlist**, bukan denylist: hanya verb yang terbukti hanya-baca yang
diloloskan. Verb yang belum terpikir saat aturan ini ditulis ikut tertahan, bukan lolos
diam-diam.

Konteks backend dideteksi dari tiga arah: opsi `-C <path>`, `cd` yang mendahului perintah
dalam satu baris shell, dan `cwd` sesi. Perintah `gh` yang dijalankan dari dalam folder
backend tanpa menyebut nama repo juga tertangkap.

Hook ini berdampingan dengan `QuilvianFrontEnd/.claude/hooks/guard-backend-push.mjs` yang
masih mengizinkan `push origin MHamzah`. Keduanya dijalankan berurutan dan yang menolak
lebih dulu yang menang, sehingga hasil akhirnya: tidak ada operasi git tulis dari Claude
sama sekali.

## Cara Claude mengakhiri pekerjaan

Karena Claude tidak lagi bisa commit, setiap pekerjaan ditutup dengan serah terima berisi
tiga hal: **daftar file yang berubah**, **hasil verifikasi build**, dan **blok perintah
`git add` + `commit` + `push` dengan pesan commit yang sudah jadi** — user tinggal menyalin.

## Dua skill master data

| Skill | Masukan | Keluaran | Menyentuh kode? |
|---|---|---|---|
| `master-data-audit` | entitas / area / "semua" | Register kekurangan di `docs/hamzah/task/<topik>.md` | **Tidak** |
| `master-data-set` | ID tugas dari register, atau entitas + kebutuhan | Kode + laporan di `docs/hamzah/report/<topik>.md`, register diperbarui | Ya |

Keduanya berpasangan dengan skill frontend yang sudah ada
(`QuilvianFrontEnd/.claude/commands/master-data-audit.md` dan `master-data-set.md`), bukan
menggantikannya. Skill frontend memeriksa dan memperbaiki UI; skill backend memeriksa dan
menutup kontrak API-nya.

Masing-masing punya dua pintu masuk ke prosedur yang sama: **slash command**
(`.claude/commands/<nama>.md`, dipanggil user) dan **skill**
(`.claude/skills/<nama>/SKILL.md`, dipilih sendiri oleh Claude saat permintaannya cocok).
File `commands/` sengaja tipis — isinya hanya mengarahkan ke `SKILL.md` plus mengulang
batas yang paling sering dilanggar. Prosedur lengkapnya hidup di satu tempat saja supaya
keduanya tidak bisa saling menyimpang.

`master-data-set` diwajibkan memperbarui register dan menyebutkannya di serah terima —
supaya dokumen register tidak menyesatkan pembaca berikutnya dengan menampilkan gap yang
sebenarnya sudah selesai.

## Dampak ke frontend

Tidak ada perubahan kode frontend. Yang berubah hanya cara kerja agent:

- `QuilvianFrontEnd/.claude/rules/backend-workflow.md` RULE 2 (push mandiri) dan langkah
  `add`/`commit`/`push` pada RULE 3 tidak lagi berlaku. Bagian lain file itu — backend
  boleh diubah, langkah verifikasi, laporan wajib, lapor ke user — tetap berlaku penuh.
- `QuilvianFinal/CLAUDE.md` diperbarui, dan path laporan dikoreksi dari `docs/report/` ke
  `docs/hamzah/report/` sesuai keadaan repo.

File aturan di repo frontend itu sendiri **belum disunting** — repo terpisah, commit
terpisah. Pertentangan pernyataannya sudah ditandai di aturan backend dan root `CLAUDE.md`.

## Cara menguji

Hook diuji lewat skrip yang memberi payload PreToolUse langsung ke `node`, 33 kasus:

| Kelompok | Kasus | Harapan |
|---|---|---|
| Verb yang menulis | `add`, `commit`, `push`, `pull`, `fetch`, `stash`, `checkout`, `branch -d`, `reset --hard`, `restore`, `merge`, `rebase`, `cherry-pick`, `clean -fd`, `tag` | ditolak |
| Jalur alternatif | `cd QuilvianBackend && git commit`, `remote set-url`, `config user.name x`, `gh pr create` | ditolak |
| Verb yang membaca | `status`, `log`, `diff`, `show`, `rev-parse`, `rev-list`, `blame`, `check-ignore`, `remote -v`, `config --get` | lolos |
| Bukan git | `dotnet build`, `rg` | lolos |
| Repo frontend | `git commit`, `git push origin HamzahV2` dengan cwd frontend | lolos |

Catatan saat menguji: perintah tes tidak boleh memuat teks git terlarang secara literal —
hook akan menolak perintah tesnya sendiri. Rakit nama verb dari potongan string di dalam
file skrip, lalu jalankan `node <skrip>`.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| Uji hook (33 kasus izin & tolak) | **33/33 lulus** — dijalankan 2026-08-11 |
| `dotnet build` | **Tidak dijalankan** — tidak ada kode aplikasi yang disentuh, `.claude/` tidak ikut dikompilasi |
| `.claude/` ikut ter-track git | Ya — `.gitignore` backend tidak memuat entri `.claude` |
| Rujukan ke nama file lama | **Bersih** — disisir dengan `rg`, tidak ada yang tertinggal selain catatan riwayat di halaman ini |
| Skill dijalankan end-to-end | **Belum** — baru ditulis, belum dipakai pada pekerjaan nyata |
