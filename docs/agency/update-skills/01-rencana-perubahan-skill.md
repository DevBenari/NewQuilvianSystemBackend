# Rencana Perubahan Skill Suite

|  |  |
| --- | --- |
| Tanggal | 2026-08-13 |
| Status | **Diimplementasikan** pada 2026-08-13. |
| Dokumen induk | [README.md](README.md) |

## 1. Masalah yang sedang diselesaikan

### 1.1 Apa itu adapter

Lima skill dipakai bersama oleh backend dan frontend: `grill-me`,
`trace-existing-capabilities`, `design-business-module`, `plan-module-delivery`, dan
`verify-module-readiness`.

Prosedur aslinya hanya ditulis satu kali, di backend. Agar skill tetap terlihat ketika sesi
dibuka dari frontend, frontend menyimpan file tipis bernama adapter. Isinya kira-kira begini:

```markdown
1. Temukan sibling `NewQuilvianSystemBackend/.claude/skills/grill-me/SKILL.md`.
2. Verifikasi SHA-256 file tersebut: `b18637d6...`.
3. Baca file canonical seluruhnya, lalu ikuti prosedurnya.
```

SHA-256 adalah sidik jari file. Kalau isi file berubah satu huruf saja, sidik jarinya berubah
total. Ini disengaja: adapter jadi tahu kalau prosedur di backend sudah berubah tetapi
frontend belum diberi tahu, lalu berhenti dengan status `STALE_SHARED_SKILL`.

### 1.2 Kenapa ini jadi beban

Mekanisme itu benar secara logika, tetapi mahal dipelihara. Setiap perubahan pada satu
prosedur di backend memaksa perhitungan ulang lima sidik jari.

Contoh nyata dari pengerjaan suite ini sendiri: dalam satu hari kerja, sidik jari harus
dihitung ulang **tiga kali**, masing-masing untuk lima sampai delapan file — total dua puluh
lebih pembaruan. Tidak satu pun dari pembaruan itu mengubah cara kerja skill. Semuanya hanya
pekerjaan administratif agar penjaga tidak salah menyalakan alarm.

Risikonya bukan hanya lelah. Kalau seorang developer mengubah prosedur di backend dan lupa
memperbarui sidik jari, seluruh workflow dari sisi frontend berhenti — padahal tidak ada yang
rusak.

### 1.3 Kenapa adapter dulu dibutuhkan, dan kenapa sekarang tidak lagi

Adapter lahir dari batasan lama: skill hanya ditemukan dari folder tempat sesi dibuka. Kalau
sesi dibuka dari frontend, skill milik backend tidak terlihat.

Dokumentasi Claude Code menyatakan hal berikut tentang folder tambahan:

> `--add-dir` … skills are an exception: `.claude/skills/` within an added directory is
> loaded automatically.

Artinya, folder yang ditambahkan lewat `--add-dir` ikut menyumbangkan skill-nya ke sesi yang
sedang berjalan. Satu perintah menyelesaikan persoalan yang dulu memerlukan lima file adapter:

```bash
cd NewQuilvianSystemBackend
claude --add-dir ../QuilvianSystemFrontendDev
```

Perlu dicatat, membuka sesi dari folder induk `Quilvian` saja **tidak** cukup. Dokumentasi
yang sama menyatakan skill di subfolder tidak dimuat saat sesi dimulai; ia baru muncul setelah
Claude menyentuh file di subfolder itu, dan sebelum itu tidak tampil di daftar. Jadi
`--add-dir` adalah caranya, bukan membuka sesi dari induk.

## 2. Prinsip penempatan yang diusulkan

Selama ini kolom "lokasi pemanggilan" diisi berdasarkan **dari mana skill boleh dipanggil**,
dan jawabannya menjadi "dari mana saja". Dari situlah adapter lahir.

Usulannya memakai satu aturan yang berbeda:

> **Lokasi skill ditentukan oleh tempat skill menulis hasilnya, bukan tempat ia membaca.**

Membaca boleh lintas repository. Menulis tidak pernah lintas repository.

Aturan ini mengisi tabel dengan sendirinya:

| Skill | Membaca | Menulis | Lokasi |
| --- | --- | --- | --- |
| `/grill-me` | jawaban pengguna | `backend/docs/module-blueprints/` | Backend |
| `/trace-existing-capabilities` | **dua repository** | `backend/docs/module-blueprints/` | Backend |
| `/design-business-module` | blueprint | `backend/docs/module-blueprints/` | Backend |
| `/plan-module-delivery` | blueprint | `backend/docs/module-blueprints/roadmap/` | Backend |
| `/build-module-backend` | blueprint + backend | **backend source** | Backend |
| `/build-module-frontend` | blueprint + frontend | **frontend source** | Frontend |
| `/verify-module-readiness` | **dua repository** | `backend/docs/module-blueprints/testing/` | Backend |

Enam skill menulis ke backend, satu menulis ke frontend.

Dua skill membaca dua repository sekaligus, yaitu `/trace-existing-capabilities` dan
`/verify-module-readiness`. Justru keduanya harus tinggal di backend, karena hasil auditnya
adalah **satu** dokumen gabungan. Kalau skill itu tinggal di frontend, ia tetap harus menulis
ke backend — tidak ada yang didapat, hanya jarak tulis yang bertambah dan peluang muncul dua
salinan peta yang berbeda.

### 2.1 Kenapa `/build-module-frontend` satu-satunya yang tinggal di frontend

Karena ia satu-satunya yang menulis ke `QuilvianSystemFrontendDev/src/`. Ia tetap membaca
blueprint, contract version, dan matriks kewenangan UI dari backend — dan itu tidak masalah,
karena membaca lintas repository memang diizinkan.

Yang dilarang adalah kebalikannya: skill frontend menulis ke backend.

## 3. Keadaan sekarang

Total 40 file di dalam dua folder `.claude`, dengan 8 sidik jari aktif.

### Backend — `NewQuilvianSystemBackend/.claude/`

| Bagian | Jumlah | Keterangan |
| --- | ---: | --- |
| `skills/` | 12 file | 6 `SKILL.md` + 6 file reference |
| `commands/` | 7 file | `qv-grill`, `qv-trace`, `qv-design`, `qv-plan`, `qv-build-be`, `qv-verify`, `qv-lanjut` |
| `rules/rule-output/` | 4 file | Aturan output dokumentasi dan contohnya |
| `PANDUAN-PENGGUNAAN-SKILLS.md` | 1 file | Panduan canonical |

### Frontend — `QuilvianSystemFrontendDev/.claude/`

| Bagian | Jumlah | Keterangan |
| --- | ---: | --- |
| `skills/` | 7 file | 1 skill asli + **5 adapter** + 1 file reference |
| `commands/` | 7 file | 5 di antaranya memanggil adapter |
| `rules/rule-output/` | 1 file | **Adapter** aturan dokumentasi |
| `PANDUAN-PENGGUNAAN-SKILLS.md` | 1 file | Panduan operasional frontend |

Delapan sidik jari aktif seluruhnya berada di frontend: lima untuk adapter skill, tiga untuk
file aturan dokumentasi.

## 4. Keadaan target

### Backend — tidak berubah strukturnya

Enam skill, tujuh command, empat file aturan, satu panduan. Yang berubah hanya isi panduan.

### Frontend — menyusut

| Bagian | Sebelum | Sesudah | Perubahan |
| --- | ---: | ---: | --- |
| `skills/` | 7 file | 2 file | 5 adapter dihapus |
| `commands/` | 7 file | 7 file | 5 diubah menjadi penunjuk |
| `rules/rule-output/` | 1 file | 0 file | Adapter dihapus |
| Panduan | 1 file | 1 file | Diperbarui |
| **Sidik jari aktif** | **8** | **0** | Mekanisme dihapus seluruhnya |

Nol sidik jari bukan berarti pengaman hilang. Pengaman itu dulu melindungi dari salinan yang
tertinggal versi. Setelah tidak ada salinan sama sekali, tidak ada yang bisa tertinggal.
Satu-satunya kegagalan yang tersisa adalah path yang salah — dan itu gagal dengan keras dan
langsung terlihat, bukan diam-diam.

## 5. Daftar perubahan per file

### 5.1 Dihapus — 6 file

| File | Alasan |
| --- | --- |
| `frontend/.claude/skills/grill-me/SKILL.md` | Adapter; digantikan `--add-dir` |
| `frontend/.claude/skills/trace-existing-capabilities/SKILL.md` | Sama |
| `frontend/.claude/skills/design-business-module/SKILL.md` | Sama |
| `frontend/.claude/skills/plan-module-delivery/SKILL.md` | Sama |
| `frontend/.claude/skills/verify-module-readiness/SKILL.md` | Sama |
| `frontend/.claude/rules/rule-output/aturan-output-dokumentasi.md` | Adapter; canonical dibaca langsung |

### 5.2 Diubah

| File | Perubahan |
| --- | --- |
| `frontend/.claude/skills/build-module-frontend/SKILL.md` | Rujukan aturan dokumentasi diarahkan langsung ke canonical backend, bagian verifikasi sidik jari dihapus |
| `frontend/.claude/commands/qv-grill.md` | Menjadi penunjuk: jelaskan bahwa skill ini dijalankan dari sesi backend, sertakan perintah `--add-dir` |
| `frontend/.claude/commands/qv-trace.md` | Sama |
| `frontend/.claude/commands/qv-design.md` | Sama |
| `frontend/.claude/commands/qv-plan.md` | Sama |
| `frontend/.claude/commands/qv-verify.md` | Sama |
| `frontend/.claude/PANDUAN-PENGGUNAAN-SKILLS.md` | Tabel skill, cara memulai sesi, bagian pemeliharaan sidik jari |
| `backend/.claude/PANDUAN-PENGGUNAAN-SKILLS.md` | Bagian 2, 3, dan 12 — rincian di bawah |

### 5.3 Rincian perubahan panduan backend

**Bagian 2 — cara menemukan dan menjalankan skill.** Kalimat berikut dicabut:

> Jangan membuka sesi dari parent `Quilvian` lalu menganggap skill dalam dua child repo
> otomatis ditemukan.

Peringatan itu benar, tetapi tidak memberi jalan keluar. Diganti dengan instruksi
`--add-dir` yang memang menyelesaikan masalahnya.

**Bagian 3 — tabel daftar skill.** Kolom lokasi berubah dari "Backend atau adapter frontend"
menjadi satu nilai:

| Sebelum | Sesudah |
| --- | --- |
| `/grill-me` — Backend atau adapter frontend | `/grill-me` — Backend |
| `/trace-existing-capabilities` — Backend atau adapter frontend | `/trace-existing-capabilities` — Backend |
| `/design-business-module` — Backend atau adapter frontend | `/design-business-module` — Backend |
| `/plan-module-delivery` — Backend atau adapter frontend | `/plan-module-delivery` — Backend |
| `/verify-module-readiness` — Backend atau adapter frontend, "Tidak secara default" | `/verify-module-readiness` — Backend, **Tidak** |

Perhatikan perubahan terakhir. Kolom "mengubah source" untuk skill audit sebelumnya berbunyi
"Tidak secara default". Kata *default* mengundang orang menyuruh auditor sekalian memperbaiki
temuannya. Begitu auditor boleh memperbaiki, ia sedang menilai pekerjaannya sendiri.
Diusulkan menjadi **Tidak** tanpa pengecualian; perbaikan dikembalikan ke
`/build-module-backend` atau `/build-module-frontend`.

**Bagian 12 — pemeliharaan suite.** Dua butir tentang perhitungan ulang sidik jari dihapus
karena tidak ada lagi sidik jari yang dipelihara.

## 6. Yang tidak berubah

Supaya jelas batas usulan ini:

- Prosedur di dalam setiap `SKILL.md` tidak diubah. Gate approval, batas kewenangan, dan
  urutan kerja tetap sama.
- Lima aturan output dokumentasi tetap berlaku penuh.
- Blok effort dan model pada setiap skill tetap.
- Blueprint canonical tetap di `NewQuilvianSystemBackend/docs/module-blueprints/`.
- Source aplikasi backend maupun frontend tidak disentuh.
- Folder `.agents` untuk Codex tidak disentuh. Bila suite Codex ingin ikut disamakan, itu
  pekerjaan terpisah dengan sidik jarinya sendiri.
