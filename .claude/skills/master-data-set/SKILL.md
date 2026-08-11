---
name: master-data-set
description: Kerjakan kebutuhan master data di backend sesuai kontrak baku — tambah filter tanggal, perbaiki proyeksi /options, tambah CreateByName/UpdateByName, atau buat controller + DTO master baru. Pakai saat user minta mengerjakan GAP/tugas dari register docs/hamzah/task/, atau menyebut entitas master data yang perlu dilengkapi di backend. Menulis laporan, memperbarui register, dan build — lalu menyerahkan perintah git ke user.
---

# Set Master Data — Kerjakan Kebutuhan Backend

Kerjakan satu tugas master data di backend sampai terverifikasi build, lalu serahkan
perintah git-nya ke user.

Bedanya dengan skill `master-data-audit`: skill itu **menemukan** kekurangan, skill ini
**menutupnya**.

> 🔒 **Jangan menjalankan perintah git yang mengubah apa pun** — `add`, `commit`, `push`,
> `pull`, `fetch`, `checkout`, `reset`, `restore`, `stash` semuanya milik user
> (`.claude/rules/git-read-only.md`). Claude berhenti setelah file tertulis dan build lolos.

---

## Masukan

Salah satu dari:

- **ID tugas dari register** — `T1`, `GAP-3`, "tugas T2 dan T3"
- **Satu entitas + kebutuhan** — "tambah filter tanggal di `leave-policy`"
- **Satu area** — "filter tanggal area Organization"

Kalau masukannya menyebut ID tugas, **baca register-nya dulu** di `docs/hamzah/task/`.
Di situ sudah ada Definition of Done, pola acuan, dan daftar objek terdampak — jangan
menyusun ulang dari nol.

---

## Aturan yang mengikat

| Dokumen | Isi |
|---|---|
| `.claude/rules/master-data-contract.md` | Kontrak yang harus dipenuhi — **baca sebelum menulis kode** |
| `.claude/rules/git-read-only.md` | Batas pekerjaan + checklist verifikasi |
| `docs/hamzah/task/<topik>.md` | Register yang memicu tugas ini |

> **Wajib mengikuti pola yang sudah ada di repo. DILARANG membuat pola sendiri.**
> Setiap kebutuhan di kontrak sudah punya implementasi acuan. Menulis versi baru membuat
> satu API punya dua semantik untuk hal yang sama.

---

## Langkah

### 1. Baca pola acuan lebih dulu

Sebelum menyentuh file target, buka implementasi acuannya dan **salin semantiknya**:

| Kebutuhan | Pola acuan |
|---|---|
| Filter rentang tanggal | `Workflow/Controllers/WorkflowMasterDataSupport.cs` → `ApplyDateFilter<T>()`, atau `PayrollAndBenefit/Controllers/AllowanceTypeController.cs` |
| Proyeksi `/options` | Proyeksi **list** di controller yang sama — nama field diambil dari sana |
| `CreateByName` / `UpdateByName` | `Areas/Administrator/MasterData/DTOs/BankDtos.cs` + `GetActorNameMapAsync` di controller-nya |
| Controller master baru | `Organization/Controllers/JobFamilyController.cs` (sederhana) atau `LegalEntityController.cs` (dengan relasi) |

### 2. Periksa pemakai kontrak sebelum mengubah

Kalau tugasnya menyentuh field/endpoint yang mungkin dipakai modul lain:

```bash
rg -n "<namaField>|<namaEndpoint>" --glob "*.cs"
rg -n "<namaField>" ../QuilvianFrontEnd/src/
```

Jangan mengubah kontrak yang sudah dipakai modul lain tanpa mengecek pemakainya dulu.

### 3. Kerjakan

Ikuti Definition of Done di `.claude/rules/master-data-contract.md` bagian 7, atau DoD
spesifik di register kalau ada.

Aturan yang paling sering dilanggar:

- **Seluruh penambahan bersifat aditif.** Parameter opsional, field tambahan, endpoint baru.
  `GET` tanpa parameter harus menghasilkan output yang **sama persis** seperti sebelumnya.
- **`GET /summary` tidak ikut difilter tanggal.**
- **Nama field disalin dari proyeksi list**, bukan dikarang. Satu entitas tidak boleh punya
  dua bentuk data.
- **`SortOrder` pada `PUT` jangan ditimpa tanpa syarat** — itu mereset urutan ke 0 setiap
  update, karena frontend tidak merender field-nya.
- **Kode auto-generate** lewat `GenerateCodeAsync`, bukan diinput user.

Kalau di tengah pekerjaan ternyata butuh **migration** atau menyentuh `appsettings*` —
**berhenti dan tanya user dulu**. Keduanya berdampak di luar repo.

### 4. Verifikasi build

```bash
dotnet build QuilvianSystemBackend.csproj --no-incremental
```

`--no-incremental` **bukan opsional**. Build polos melewati kompilasi kalau tidak ada file
yang berubah sejak build terakhir — selesai dalam hitungan detik dan melaporkan
`0 Warning` yang menyesatkan.

Baseline repo ini saat rebuild penuh: **125 warning bawaan, 0 error** (per 2026-08-11).
Yang perlu dipastikan: **0 Error**, dan **tidak ada warning baru dari file yang disentuh**.

```bash
# Pastikan tidak ada warning dari file yang diubah
dotnet build QuilvianSystemBackend.csproj --no-incremental > /tmp/build.log 2>&1
grep -E "<FileYangDisentuh>\.cs" /tmp/build.log
```

Kalau build gagal — **perbaiki, jangan lanjut commit**. Jangan melaporkan sukses sebagian.

### 5. Tulis laporan (WAJIB, sebelum commit)

`docs/hamzah/report/<nama-topik>.md`, kebab-case dan deskriptif — isi perubahannya, bukan
nomor tiket. Satu pekerjaan = satu halaman; kalau topiknya sama dengan halaman yang sudah
ada, **perbarui halaman itu**.

```markdown
# <Entitas/Modul> — <Ringkas perubahannya>

| | |
|---|---|
| Tanggal | YYYY-MM-DD |
| Branch | `MHamzah` |
| Pemicu | dari mana kebutuhan ini datang |
| Migration | ada / tidak ada |
| Breaking change | ya / tidak — jelaskan kalau ya |

## Kenapa diubah
Masalah nyatanya. Kenapa tidak bisa diselesaikan di frontend.

## Endpoint yang terpengaruh
Tabel: endpoint → apa yang berubah. Sebut juga endpoint yang sengaja TIDAK diubah.

## Kontrak parameter / field
Tabel: nama, tipe, perilaku. Termasuk perilaku saat parameter tidak dikirim.

## File yang disentuh
Tabel: path file → perubahannya.

## Dampak ke frontend
File frontend yang menyesuaikan. Tulis "tidak ada yang wajib" kalau memang begitu.

## Cara menguji
Contoh request konkret, termasuk kasus "perilaku lama harus tetap sama".

## Status verifikasi
Tabel hasil build dan pemeriksaan. Tulis apa adanya —
kalau belum dijalankan, tulis "belum dijalankan".
```

**Bagian Status verifikasi tidak boleh memuat klaim yang belum benar-benar dijalankan.**
Uji endpoint lewat Swagger biasanya belum dijalankan — tulis begitu, jangan dikarang.

### 6. Perbarui register

Kalau tugas ini berasal dari `docs/hamzah/task/<topik>.md`, dokumen itu **wajib ikut
diperbarui**, dan disebutkan di serah terima agar user men-stage-nya pada commit yang sama:

- Tandai GAP/tugasnya selesai di tabel **Ringkasan** dan tabel **Usulan pemecahan tugas**
- Sebutkan sha commit dan halaman laporannya
- Perbarui bagian **Pekerjaan yang sudah jalan** — bedakan tegas antara sudah **di-commit**
  dan sudah **di-push**
- Rapikan angka atau daftar yang jadi usang karena pekerjaan ini

Register yang tidak bergerak sementara laporan bertambah akan menyesatkan pembaca
berikutnya — pekerjaan yang sudah beres masih terbaca sebagai gap.

### 7. Periksa keadaan repo — baca saja

```bash
git -C QuilvianBackend rev-parse --abbrev-ref HEAD   # harus MHamzah
git -C QuilvianBackend status --short                # tidak ada bin/, obj/, .user
git -C QuilvianBackend diff                          # baca sendiri isinya
```

Kalau `status` memunculkan `bin/` atau `obj/`, **jangan** dibereskan dengan `git rm` atau
`restore` — laporkan ke user bahwa `.gitignore` bocor.

**Jangan jalankan `git add`, `commit`, atau `push`.** Hook akan menolaknya, dan itu memang
disengaja.

---

## Serah terima ke user

```markdown
🔧 **Backend diubah**
- File: `Areas/.../XController.cs`, `DTOs/.../XDtos.cs`
- Perubahan: ...
- Migration: ada / tidak
- Halaman laporan: `docs/hamzah/report/<topik>.md`
- Register diperbarui: `docs/hamzah/task/<topik>.md`

### Verifikasi
| Pemeriksaan | Hasil |
|---|---|
| `dotnet build --no-incremental` | 0 Error |
| Warning baru dari file yang disentuh | tidak ada |
| Uji endpoint lewat Swagger | belum dijalankan |

### Siap di-commit — jalankan sendiri

```bash
git -C QuilvianBackend add <file1> <file2> docs/hamzah/
git -C QuilvianBackend commit -F - <<'EOF'
<judul singkat>

<badan: apa yang berubah dan kenapa>

Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>
EOF
git -C QuilvianBackend push origin MHamzah
```
```

Sajikan **pesan commit yang sudah jadi**, bukan placeholder — user tinggal menyalin.

> ⚠️ Sarankan **heredoc** (`<<'EOF'`), bukan here-string PowerShell (`@'...'@`). Di Bash,
> sintaks PowerShell itu menyisipkan `@` nyasar ke baris pertama dan terakhir pesan commit.

Kalau ada yang gagal — build error, konflik, atau bagian scope yang tidak selesai —
**sebutkan apa adanya**. Jangan dilaporkan sebagai selesai.
