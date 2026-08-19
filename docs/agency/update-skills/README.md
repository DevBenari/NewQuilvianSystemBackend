# Update Skill Suite — Dari Adapter Berhash ke Satu Lokasi per Skill

|  |  |
| --- | --- |
| Tanggal | 2026-08-13 |
| Status | **Diimplementasikan** pada 2026-08-13. Seluruh keputusan tertutup dan dieksekusi. |
| Dokumen canonical | `docs/agency/update-skills/` |
| Dokumen induk | [rekomendasi-ai-skills-pengembangan-modul.md](../rekomendasi-ai-skills-pengembangan-modul.md) |
| Pemicu | Adapter frontend berhash menimbulkan biaya pemeliharaan berulang, sementara Claude Code sudah menyediakan mekanisme yang menggantikannya |
| Model penempatan usulan | **6 skill backend + 1 skill frontend, tanpa adapter** |
| Cakupan perubahan | Konfigurasi AI agent dan dokumentasi; source aplikasi tidak diubah |
| Database/runtime migration | Tidak ada |
| Breaking change | Ya, terbatas pada cara memanggil skill dari sesi frontend |

## Ringkasan untuk pembaca yang terburu-buru

Saat ini frontend menyimpan **lima file adapter**. Adapter tidak memuat prosedur apa pun; ia
hanya berkata "prosedur aslinya ada di backend, dan ini sidik jari SHA-256-nya". Adapter
dibuat karena dulu skill tidak dapat ditemukan bila sesi dibuka dari frontend.

Masalahnya, setiap kali prosedur asli di backend diubah, kelima sidik jari itu harus dihitung
ulang. Kalau lupa, seluruh workflow frontend berhenti dengan status `STALE_SHARED_SKILL`.

Claude Code sudah menyediakan jalan keluar yang tidak memerlukan adapter sama sekali:

```bash
cd NewQuilvianSystemBackend
claude --add-dir ../QuilvianSystemFrontendDev
```

Satu perintah ini membuat seluruh skill dari kedua repository tersedia dalam satu sesi.
Dengan begitu, adapter dan seluruh mekanisme sidik jari dapat dihapus.

Usulannya: **satu skill hanya tinggal di satu tempat**, yaitu tempat ia menulis hasilnya.

## Isi folder ini

| Dokumen | Isi |
| --- | --- |
| [01-rencana-perubahan-skill.md](01-rencana-perubahan-skill.md) | Prinsip penempatan, keadaan sekarang, keadaan target, dan daftar perubahan per file |
| [02-dampak-dan-cara-pakai.md](02-dampak-dan-cara-pakai.md) | Dampak bagi tim, cara kerja setelah perubahan, risiko, dan checklist eksekusi |
| [03-revisi-design-business-module.md](03-revisi-design-business-module.md) | Revisi keluaran skill desain: class diagram, ERD dan kamus data, arsitektur folder backend, status model |

Dokumen 01 dan 02 membahas **tempat** skill tinggal. Dokumen 03 membahas **isi keluaran**
salah satu skill. Keduanya berdiri sendiri dan dapat disetujui terpisah.

## Yang sudah selesai dan yang belum

Bagian ini memisahkan pekerjaan yang sudah terpasang dari yang masih menunggu keputusan.

### Sudah selesai

| Pekerjaan | Keterangan |
| --- | --- |
| Penyalinan `.agents` menjadi `.claude` | Dua repository, seluruh skill dan reference |
| Penyesuaian gaya Claude Code | Pemanggilan `$skill` menjadi `/skill`, `agents/openai.yaml` dihapus |
| Slash command `qv-*` | 7 command di backend, 7 di frontend |
| Aturan output dokumentasi | `.claude/rules/rule-output/`, empat file, lima aturan wajib |
| Blok effort dan model | Terpasang di seluruh 12 `SKILL.md` |
| Penawaran skill berikutnya | Terpasang di seluruh 12 `SKILL.md` |
| Disiplin batas scope pada `/grill-me` | Daftar di dalam scope dan di luar scope wajib dikunci sebelum bertanya |
| Penyeragaman nama repository | `QuilvianBackend` menjadi `NewQuilvianSystemBackend`, `QuilvianFrontEnd` menjadi `QuilvianSystemFrontendDev` |
| Contoh keluaran per skill | Studi kasus IGD, tersimpan di `.claude/rules/rule-output/contoh-output-per-skill.md` |

### Dikerjakan pada eksekusi 2026-08-13

| Pekerjaan | Hasil |
| --- | --- |
| Menghapus 5 adapter skill frontend | Skill frontend dari 6 menjadi 1 |
| Menghapus adapter aturan dokumentasi frontend | `build-module-frontend` membaca canonical backend langsung |
| Mengubah 5 command frontend menjadi penunjuk | `/qv-grill`, `/qv-trace`, `/qv-design`, `/qv-plan`, `/qv-verify` |
| Memperbarui tabel daftar skill pada kedua panduan | Kolom lokasi menjadi satu nilai, ditambah kolom Membaca dan Menulis |
| Mengganti peringatan "jangan buka sesi dari parent" | Diganti instruksi `--add-dir` beserta syarat versi 2.1.203 |
| Menghapus aturan pemeliharaan sidik jari | Sidik jari aktif dari 8 menjadi **0** |
| Mencabut pengecualian `.gitignore` frontend | 10 file `.claude` kini lolos dan dapat ter-commit |
| Menghapus folder `.agents` | 19 file backend dan 14 file frontend, tercatat git sebagai deletion |
| Revisi keluaran `design-business-module` | Dokumen 03, dieksekusi lebih dulu |

### Belum dikerjakan

| Pekerjaan | Keterangan |
| --- | --- |
| Menulis ulang blueprint IGD ke revision 4 | Task lanjutan sesuai DEC-RSK-008; dikerjakan **oleh** `/design-business-module` yang sudah diperbarui, bukan ditulis tangan |
| Menguji tujuh skill dalam satu sesi | Perlu sesi baru dengan `--add-dir`; tidak dapat diuji dari sesi yang sedang berjalan |

## Keputusan yang diminta

| No | Keputusan | Owner | Dokumen | Status |
| ---: | --- | --- | --- | --- |
| 1 | Menyetujui penghapusan lima adapter frontend beserta mekanisme sidik jari | Pemilik suite skill | 02 | **Selesai** — DEC-USK-001 |
| 2 | Menentukan apakah `/build-module-frontend` dijalankan dari sesi backend yang sama atau sesi frontend terpisah | Pemilik suite skill | 02 | **Selesai** — DEC-USK-002 |
| 3 | Menyetujui pencabutan pengecualian `.claude/` pada `.gitignore` frontend | Pemilik repository frontend | 02 | **Selesai** — DEC-USK-003 |
| 7 | Menentukan nasib folder `.agents` (Codex) | Pemilik suite skill | 02 | **Selesai** — DEC-USK-004 |
| 4 | Menentukan penanganan tiga inkonsistensi struktur folder dan namespace backend | Pemilik arsitektur backend | 03 | **Selesai** — DEC-RSK-003 |
| 5 | Menentukan cakupan tujuh rekomendasi tambahan pada skill desain | Pemilik suite skill | 03 | **Selesai** — DEC-RSK-004 |
| 6 | Menentukan format diagram: Mermaid, tabel saja, atau gambar hasil ekspor | Pemilik suite skill | 03 | **Selesai** — DEC-RSK-005 |

Seluruh keputusan sudah tertutup lewat wawancara. Rinciannya ada pada bagian 7 dokumen 02
(DEC-USK-001 sampai 004) dan bagian 11 dokumen 03 (DEC-RSK-001 sampai 008 beserta CF-RSK-001).

Keputusan 7 muncul dari wawancara, tidak ada pada rancangan awal: dokumen 01 semula menyatakan
`.agents` tidak disentuh, tetapi setelah adapter dihapus dari `.claude`, memelihara dua suite
berarti mengerjakan setiap perubahan dua kali. Keputusannya `.agents` dihapus dari kedua
repository.
