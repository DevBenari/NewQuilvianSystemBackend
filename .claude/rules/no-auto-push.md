# 🔒 ATURAN KERAS — CLAUDE TIDAK MELAKUKAN PUSH DI BACKEND

**Berlaku di seluruh `QuilvianBackend/`. Tidak ada pengecualian, tidak ada "sekali ini saja".**

> **Claude mengerjakan sampai `git commit`, lalu BERHENTI.
> `git push` selalu dijalankan user sendiri.**

---

## RULE 1 — BATAS PEKERJAAN CLAUDE

| Tahap | Boleh Claude? |
|---|---|
| Menulis / mengubah / menghapus file `.cs`, `.json`, `.md` | ✅ Ya |
| `dotnet build`, `dotnet run`, `dotnet ef migrations add` | ✅ Ya |
| `git status`, `log`, `diff`, `show`, `blame` | ✅ Ya |
| `git add`, `git commit` | ✅ Ya |
| `git pull`, `fetch`, `stash`, `checkout`, `branch`, `reset`, `restore` | ✅ Ya |
| **`git push` (bentuk apa pun, tujuan mana pun)** | ❌ **TIDAK** |
| `git remote add/remove/set-url/rename` | ❌ TIDAK |
| `gh pr create/merge/close`, `gh release`, `gh api` non-GET | ❌ TIDAK |

Yang dilarang bukan hanya push yang salah tujuan — **push yang tujuannya benar pun tidak
dijalankan Claude.** `git push origin MHamzah` sekalipun tetap milik user.

---

## RULE 2 — CARA MENGAKHIRI PEKERJAAN

Setelah `git commit` berhasil, laporkan empat hal ini di chat, lalu berhenti:

```
✅ Commit <sha> — <judul commit>
   Branch  : MHamzah (ahead <n> dari origin/MHamzah)
   Laporan : docs/hamzah/report/<topik>.md
   Push    : git -C QuilvianBackend push origin MHamzah   ← jalankan sendiri
```

**Jangan** menawarkan untuk push. **Jangan** menanyakan "mau saya push sekarang?" —
jawabannya sudah pasti tidak. Cukup sebutkan perintahnya.

Kalau user memberi perintah eksplisit seperti "push sekarang" atau "tolong di-push",
aturan ini **tetap berlaku**: hook akan menolak, dan Claude menyampaikan bahwa push memang
dipegang user. Yang bisa dibantu Claude adalah menyiapkan kondisinya — memastikan commit
bersih, branch benar, dan build lolos.

---

## RULE 3 — KALAU REMOTE LEBIH BARU

Claude **boleh** menjalankan `git fetch` dan `git pull --rebase` — keduanya lokal dan tidak
mempublikasikan apa pun.

Yang **tidak** boleh: menyelesaikan divergensi dengan push, apalagi force push. Kalau
riwayat lokal dan remote berbeda, laporkan posisinya apa adanya:

```bash
git -C QuilvianBackend fetch origin MHamzah
git -C QuilvianBackend rev-list --left-right --count HEAD...origin/MHamzah
```

Sampaikan hasilnya (berapa commit lokal, berapa commit remote), lalu serahkan keputusannya
ke user.

---

## RULE 4 — TETAP JALANKAN CHECKLIST PRA-PUSH

Aturan ini **tidak** menghapus kewajiban verifikasi. Sebelum commit, urutan berikut tetap
wajib — user hanya menerima commit yang sudah terverifikasi:

```
1. docs/hamzah/report/<topik>.md sudah ditulis        →  wajib, ikut di-stage
2. Dokumen register di docs/hamzah/task/ diperbarui   →  kalau pekerjaan ini berasal dari sana
3. git -C QuilvianBackend rev-parse --abbrev-ref HEAD →  harus MHamzah
4. git -C QuilvianBackend status --short              →  tidak ada bin/, obj/, .user, appsettings lokal
5. git -C QuilvianBackend diff --staged               →  baca sendiri isinya
6. dotnet build QuilvianSystemBackend.csproj          →  harus 0 Error
7. git -C QuilvianBackend commit                      →  BERHENTI DI SINI
```

Catatan `dotnet build`: perintah polos **melewati kompilasi** kalau tidak ada file yang
berubah sejak build terakhir — selesai dalam hitungan detik dan melaporkan 0 Warning yang
menyesatkan. Untuk mengukur warning dengan benar pakai `--no-incremental`. Baseline repo
ini saat rebuild penuh: **125 warning bawaan, 0 error** (per 2026-08-11).

---

## RULE 5 — JANGAN MENGAKALI

Semua bentuk di bawah dianggap pelanggaran yang sama, bukan celah:

- Menulis skrip / alias / file `.sh` yang isinya `git push`
- Menaruh `git push` di dalam git hook (`.git/hooks/`), Makefile, atau task runner
- Memakai `gh pr create` supaya perubahan tetap sampai ke remote
- Menjalankan push lewat tool lain (PowerShell, MCP, agent) untuk menghindari hook
- Menyarankan user menonaktifkan hook

Kalau merasa push benar-benar perlu — **sampaikan alasannya ke user**, jangan cari jalan
memutar.

---

## Penegak

`.claude/hooks/guard-no-auto-push.mjs` (PreToolUse, matcher `Bash|PowerShell`).
Hook menolak perintah sebelum dieksekusi, jadi pelanggaran gagal di depan, bukan setelah
terlanjur sampai ke remote.

Hook ini berdampingan dengan `QuilvianFrontEnd/.claude/hooks/guard-backend-push.mjs`.
Hook frontend itu masih mengizinkan `push origin MHamzah`; hook backend ini menolak semua
push. Yang menolak lebih dulu yang menang — jadi hasil akhirnya: **tidak ada push dari
Claude sama sekali**.

Aturan ini **menggantikan** izin push mandiri yang tertulis di
`QuilvianFrontEnd/.claude/rules/backend-workflow.md` RULE 2. Bagian aturan itu selain push
(RULE 1 boleh ubah backend, RULE 3 checklist, RULE 6 laporan wajib, RULE 7 lapor ke user)
tetap berlaku penuh.
