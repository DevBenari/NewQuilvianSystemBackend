# 🔒 ATURAN KERAS — GIT DI BACKEND HANYA BOLEH DIBACA

**Berlaku di seluruh `QuilvianBackend/`. Tidak ada pengecualian, tidak ada "sekali ini saja".**

> **Claude menulis file dan menjalankan build. Seluruh perintah git yang MENGUBAH sesuatu
> — `add`, `commit`, `push`, `pull`, `fetch`, `checkout`, `branch`, `reset`, `restore`,
> `stash`, `merge`, `rebase` — dijalankan user sendiri.**

Claude hanya boleh **membaca** state git, tidak pernah menggerakkannya.

---

## RULE 1 — BATAS PEKERJAAN CLAUDE

| Tahap | Boleh Claude? |
|---|---|
| Menulis / mengubah / menghapus file `.cs`, `.json`, `.md` | ✅ Ya |
| `dotnet build`, `dotnet run`, `dotnet ef migrations add` | ✅ Ya |
| `git status`, `log`, `diff`, `show`, `blame`, `rev-parse`, `ls-files` | ✅ Ya — **baca saja** |
| **`git add`** | ❌ TIDAK |
| **`git commit`** | ❌ TIDAK |
| **`git push`** (bentuk apa pun, tujuan mana pun) | ❌ TIDAK |
| **`git pull`, `fetch`** | ❌ TIDAK |
| **`git checkout`, `switch`, `branch`, `reset`, `restore`, `stash`** | ❌ TIDAK |
| **`git merge`, `rebase`, `cherry-pick`, `revert`, `clean`, `rm`, `mv`, `tag`** | ❌ TIDAK |
| `git remote add/remove/set-url/rename` | ❌ TIDAK |
| `gh pr create/merge/close`, `gh release`, `gh api` non-GET | ❌ TIDAK |

Pemisahnya sederhana: **perintah git yang mengubah index, working tree, branch, atau remote
adalah milik user.** Yang tersisa untuk Claude adalah perintah yang hanya menampilkan
keadaan.

Ini **lebih keras** daripada larangan push saja. Alasannya: `git add` dan `git commit` juga
memindahkan pekerjaan ke tempat yang tidak bisa dibatalkan user tanpa usaha, dan `checkout`
/ `reset` / `restore` bisa membuang perubahan yang belum sempat dilihat user. Keputusan
kapan pekerjaan "dibekukan" ada di user.

---

## RULE 2 — CARA MENGAKHIRI PEKERJAAN

Setelah file selesai ditulis dan build lolos, **berhenti** dan serahkan dengan lengkap:

```markdown
## Siap di-commit

**File yang berubah**
- `Areas/.../XController.cs` — <apa yang berubah>
- `docs/hamzah/report/<topik>.md` — laporan (baru)

**Verifikasi**
- `dotnet build --no-incremental` → 0 Error, tidak ada warning baru

**Perintah untuk kamu jalankan**

```bash
git -C QuilvianBackend add <file1> <file2> docs/hamzah/
git -C QuilvianBackend commit -F - <<'EOF'
<judul singkat>

<badan pesan commit yang sudah jadi>
EOF
git -C QuilvianBackend push origin MHamzah
```
```

Tiga hal yang membuat serah terima ini berguna:

1. **Daftar file lengkap** — user tidak perlu menebak apa yang tersentuh.
2. **Pesan commit sudah jadi** — tinggal disalin, tidak perlu dikarang ulang.
3. **Hasil verifikasi** — user tahu commit ini sudah terbukti build-nya.

**Jangan** menawarkan untuk menjalankannya. **Jangan** bertanya "mau saya commit sekarang?".
Cukup sajikan perintahnya.

Kalau user memberi perintah eksplisit seperti "commit sekarang" atau "tolong di-push",
aturan ini **tetap berlaku** — hook akan menolak. Sampaikan bahwa git dipegang user, lalu
sajikan perintah siap pakainya.

---

## RULE 3 — MEMBACA STATE BOLEH, MENGGERAKKAN TIDAK

Claude tetap perlu tahu keadaan repo sebelum bekerja. Yang boleh:

```bash
git -C QuilvianBackend rev-parse --abbrev-ref HEAD   # branch aktif — harus MHamzah
git -C QuilvianBackend status --short                # ada file nyasar?
git -C QuilvianBackend diff                          # baca perubahan sendiri
git -C QuilvianBackend log --oneline -5              # riwayat terakhir
```

Yang **tidak** boleh, walaupun terasa tidak berbahaya:

- `git fetch` — mengubah remote-tracking ref. Kalau perlu tahu posisi remote, minta user
  menjalankannya, atau baca `git log --oneline origin/MHamzah..HEAD` dengan data yang ada.
- `git pull --rebase` — menulis ulang riwayat lokal.
- `git stash` — memindahkan perubahan user ke tempat lain.

Kalau riwayat lokal dan remote berbeda, **laporkan apa adanya** berdasarkan data yang bisa
dibaca, lalu serahkan keputusannya ke user.

---

## RULE 4 — VERIFIKASI TETAP WAJIB

Aturan ini **tidak** menghapus kewajiban memverifikasi. Yang berubah hanya siapa yang
menjalankan git. Sebelum menyerahkan pekerjaan, urutan berikut tetap wajib:

```
1. docs/hamzah/report/<topik>.md sudah ditulis        →  wajib
2. Dokumen register di docs/hamzah/task/ diperbarui   →  kalau pekerjaan berasal dari sana
3. git -C QuilvianBackend rev-parse --abbrev-ref HEAD →  harus MHamzah (baca saja)
4. git -C QuilvianBackend status --short              →  tidak ada bin/, obj/, .user,
                                                          appsettings lokal
5. git -C QuilvianBackend diff                        →  baca sendiri isinya
6. dotnet build QuilvianSystemBackend.csproj --no-incremental   →  harus 0 Error
7. Serahkan ke user sesuai RULE 2                     →  BERHENTI DI SINI
```

Kalau langkah 4 menemukan `bin/` atau `obj/` ikut muncul, **jangan** dibereskan dengan
`git rm` atau `restore` — laporkan ke user bahwa `.gitignore` bocor.

Catatan `dotnet build`: perintah polos **melewati kompilasi** kalau tidak ada file yang
berubah sejak build terakhir — selesai dalam hitungan detik dan melaporkan `0 Warning` yang
menyesatkan. Pakai `--no-incremental`. Baseline repo ini saat rebuild penuh:
**125 warning bawaan, 0 error** (per 2026-08-11).

---

## RULE 5 — JANGAN MENGAKALI

Semua bentuk di bawah dianggap pelanggaran yang sama, bukan celah:

- Menulis skrip / alias / file `.sh` yang isinya perintah git terlarang
- Menaruh perintah git di git hook (`.git/hooks/`), Makefile, atau task runner
- Memakai `gh` untuk mencapai efek yang sama
- Menjalankan git lewat tool lain (PowerShell, MCP, agent) untuk menghindari hook
- Menyarankan user menonaktifkan hook
- Mengedit file di dalam `.git/` secara langsung

Kalau merasa salah satu perintah itu benar-benar perlu — **sampaikan alasannya ke user**,
jangan cari jalan memutar.

---

## Penegak

`.claude/hooks/guard-git-read-only.mjs` (PreToolUse, matcher `Bash|PowerShell`).

Hook memakai **allowlist**: hanya verb git yang terbukti hanya-baca yang diloloskan, sisanya
ditolak. Jadi verb yang belum terpikir saat aturan ini ditulis pun ikut tertahan, bukan
lolos diam-diam.

Repo frontend **tidak terpengaruh** — hook hanya bereaksi pada konteks `QuilvianBackend/`.

Aturan ini **menggantikan** izin push mandiri di
`QuilvianFrontEnd/.claude/rules/backend-workflow.md` RULE 2, sekaligus mempersempit RULE 3
di sana (checklist pra-push): langkah verifikasinya tetap, tapi `git add`, `commit`, dan
`push` di akhir checklist itu bukan lagi pekerjaan Claude. Bagian lain file itu — backend
boleh diubah (RULE 1), laporan wajib (RULE 6), lapor ke user (RULE 7) — tetap berlaku penuh.
