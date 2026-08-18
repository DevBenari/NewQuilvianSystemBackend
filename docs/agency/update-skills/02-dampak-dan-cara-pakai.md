# Dampak, Cara Pakai, dan Checklist Eksekusi

|  |  |
| --- | --- |
| Tanggal | 2026-08-13 |
| Status | **Diimplementasikan** pada 2026-08-13. |
| Dokumen induk | [README.md](README.md) |

## 1. Cara kerja setelah perubahan

### 1.1 Membuka sesi

Untuk hampir seluruh pekerjaan, buka satu sesi dari backend sambil menambahkan frontend:

```bash
cd NewQuilvianSystemBackend
claude --add-dir ../QuilvianSystemFrontendDev
```

Setelah itu ketik `/` dan seluruh tujuh skill akan terlihat, tanpa nama yang bertabrakan.
Tabrakan nama tidak terjadi karena setelah adapter dihapus, tidak ada lagi dua file dengan
nama skill yang sama.

Kalau seorang developer frontend hanya ingin mengerjakan satu task UI, sesi frontend juga
tetap bisa dipakai:

```bash
cd QuilvianSystemFrontendDev
claude --add-dir ../NewQuilvianSystemBackend
```

Dari sesi ini hanya `/build-module-frontend` yang tersedia sebagai skill frontend, dan itu
memang satu-satunya yang dibutuhkan. Backend ditambahkan supaya blueprint dan kontrak dapat
dibaca.

### 1.2 Urutan kerja

Urutannya tidak berubah dari yang sudah berlaku:

```text
Sesi backend
  /grill-me            wawancara bisnis, kunci batas scope
  /trace-existing-capabilities   audit dua repository, cari yang bisa dipakai ulang
  /grill-me            closure pass, tutup conflict dan unknown
  /design-business-module        arsitektur, ERD, kontrak target
  [approval owner]
  /plan-module-delivery          roadmap backend dan frontend
  ───────── serah terima: roadmap siap ─────────
  /build-module-backend          satu task backend
  /build-module-frontend         satu task frontend
  /verify-module-readiness       audit kesiapan
```

### 1.3 Kapan frontend boleh mulai

Ini sering disalahpahami, jadi ditegaskan di sini.

Frontend **tidak** perlu menunggu seluruh backend selesai. Frontend boleh mulai begitu
**contract version yang dipakainya sudah berstatus approved dan hash-nya terkunci**.

Contoh: modul IGD punya tiga slice. Slice 1 adalah retriage. Begitu kontrak retriage v2
disetujui, `FE-IGD-001` boleh dikerjakan walaupun slice 2 dan 3 belum disentuh sama sekali.

Yang dilarang adalah frontend menebak bentuk data dari endpoint yang belum disetujui.
Menunggu seluruh backend tuntas justru merugikan: masalah integrasi baru ketahuan di akhir,
saat paling mahal diperbaiki.

## 2. Dampak bagi tim

### 2.1 Yang berubah bagi developer frontend

| Sebelum | Sesudah |
| --- | --- |
| Mengetik `/grill-me` dari sesi frontend menjalankan adapter | Mengetik `/qv-grill` memberi arahan untuk membuka sesi backend |
| Workflow bisa berhenti dengan `STALE_SHARED_SKILL` walaupun tidak ada yang rusak | Status itu tidak ada lagi |
| Enam skill terlihat dari sesi frontend, lima di antaranya hanya perantara | Satu skill terlihat, dan memang hanya itu yang dibutuhkan |

Yang **tidak** berubah: cara mengerjakan task frontend, gate approval, kewajiban menangani
seluruh state UI, dan larangan mengubah backend dari sesi frontend.

### 2.2 Yang berubah bagi developer backend

Praktis tidak ada, kecuali satu hal yang melegakan: mengubah prosedur skill tidak lagi
memaksa perhitungan ulang sidik jari di frontend.

### 2.3 Yang hilang, dan apakah itu masalah

| Yang hilang | Apakah masalah |
| --- | --- |
| Kemampuan menjalankan lima skill bersama langsung dari sesi frontend | Tidak. Kelima skill itu menulis ke backend; menjalankannya dari frontend tidak memberi keuntungan apa pun |
| Penjaga `STALE_SHARED_SKILL` | Tidak. Penjaga itu melindungi salinan agar tidak tertinggal versi. Tanpa salinan, tidak ada yang bisa tertinggal |
| Daftar skill lengkap di autocomplete sesi frontend | Sebagian. Ditutup dengan mengubah lima command menjadi penunjuk, sehingga pengguna yang salah sesi tetap diarahkan, bukan dibiarkan bingung |

## 3. Risiko dan mitigasi

| No | Risiko | Kemungkinan | Mitigasi |
| ---: | --- | --- | --- |
| 1 | Developer lupa memakai `--add-dir`, lalu skill lintas repo gagal membaca file | Sedang | Instruksi ditaruh di bagian paling awal kedua panduan; command penunjuk juga mencantumkan perintahnya |
| 2 | Ada dokumen atau kebiasaan lama yang masih menyuruh menjalankan skill dari frontend | Sedang | Kedua panduan diperbarui dalam perubahan yang sama. Folder `.agents` dihapus sesuai DEC-USK-004, sehingga tidak ada lagi salinan aturan lama yang bisa terbaca |
| 5 | Ada anggota tim yang masih memakai Codex dan alur kerjanya berhenti setelah `.agents` dihapus | Perlu dipastikan | Kedua folder ter-track penuh oleh git, sehingga dapat dipulihkan dari history. Bila dipulihkan, **MUST NOT** disamakan dengan `.claude` dengan cara menghapus adapternya — Codex tidak punya padanan `--add-dir` |
| 3 | Folder repository dipindah atau diganti nama, sehingga path sibling putus | Rendah | Kegagalan bersifat keras dan langsung terlihat; tidak ada kegagalan diam-diam |
| 4 | `.claude/` frontend diabaikan git, sehingga skill tidak sampai ke tim | **Tinggi** | **Sudah ditangani** oleh DEC-USK-003 |

### Rincian risiko nomor 4 dan penanganannya

Sebelum perubahan, `QuilvianSystemFrontendDev/.gitignore` memblokir seluruh `.claude/`,
terbukti 0 file ter-track sementara backend 20 file. Anggota tim yang melakukan clone tidak
mendapat skill `build-module-frontend` sama sekali.

Yang diterapkan:

```gitignore
# Claude Code
# Konfigurasi suite skill ikut ter-commit agar seluruh tim memakai versi yang sama.
# Berkas sesi dan pengaturan pribadi tetap diabaikan.
.claude/*
!.claude/skills/
!.claude/commands/
!.claude/PANDUAN-PENGGUNAAN-SKILLS.md
```

Perhatikan `.claude/*` dengan tanda bintang, bukan `.claude/`. Git tidak menelusuri isi
direktori yang sudah diabaikan, sehingga pola `.claude/` membuat seluruh pengecualian di
bawahnya tidak pernah dievaluasi. Hasil setelah perubahan: 10 file lolos dan siap ter-commit.

## 4. Keputusan yang diminta

### Keputusan 1 — Hapus adapter beserta mekanisme sidik jari

| Field | Isi |
| --- | --- |
| Owner | Pemilik suite skill |
| Pilihan A **(Direkomendasikan)** | Hapus lima adapter skill dan satu adapter aturan dokumentasi. Pemeliharaan turun drastis, tidak ada lagi kegagalan palsu |
| Pilihan B | Pertahankan adapter. Sesi frontend tetap bisa memanggil semua skill, dengan biaya perhitungan ulang sidik jari pada setiap perubahan prosedur |
| Pilihan C | Hapus adapter skill, pertahankan adapter aturan dokumentasi saja. Setengah jalan; menyisakan satu sidik jari tanpa manfaat yang sepadan |
| Konsekuensi bila A dipilih | Skill lintas repo hanya dapat dipanggil dari sesi yang memuat backend |

### Keputusan 2 — Tempat menjalankan `/build-module-frontend`

| Field | Isi |
| --- | --- |
| Owner | Pemilik suite skill |
| Pilihan A **(Direkomendasikan)** | Sesi backend dengan `--add-dir` frontend. Kontrak dan roadmap masih segar dalam satu alur kerja, serah terima antar task lebih mulus |
| Pilihan B | Sesi frontend terpisah dengan `--add-dir` backend. Batas antar repo lebih tegas secara kebiasaan kerja, tetapi konteks harus dibaca ulang setiap ganti sesi |
| Catatan | Keduanya berjalan setelah perubahan ini. Perbedaannya disiplin kerja, bukan kemampuan. Batas kewenangan tetap dijaga oleh isi `SKILL.md`, bukan oleh batas sesi |

### Keputusan 3 — Pencabutan pengecualian `.gitignore` frontend

| Field | Isi |
| --- | --- |
| Owner | Pemilik repository frontend |
| Pilihan A **(Direkomendasikan)** | Tambahkan pengecualian agar `skills/`, `commands/`, dan panduan ikut ter-commit |
| Pilihan B | Biarkan diabaikan. Setiap developer memasang sendiri konfigurasinya, dengan risiko versi berbeda-beda antar orang |
| Konsekuensi bila B dipilih | Suite tidak dapat diandalkan sebagai standar tim |

## 5. Checklist eksekusi

Dijalankan hanya setelah keputusan 1 disetujui.

| No | Langkah | Selesai |
| ---: | --- | :---: |
| 1 | Hapus lima adapter `SKILL.md` di `frontend/.claude/skills/` | ☑ |
| 2 | Hapus `frontend/.claude/rules/rule-output/aturan-output-dokumentasi.md` | ☑ |
| 3 | Arahkan rujukan aturan dokumentasi pada `build-module-frontend` langsung ke canonical backend | ☑ |
| 4 | Ubah lima command frontend menjadi penunjuk beserta perintah `--add-dir` | ☑ |
| 5 | Perbarui tabel skill, cara memulai sesi, dan bagian pemeliharaan pada panduan backend | ☑ |
| 6 | Perbarui hal yang sama pada panduan frontend | ☑ |
| 7 | Ubah kolom "mengubah source" `/verify-module-readiness` menjadi **Tidak** tanpa pengecualian | ☑ |
| 8 | Pastikan tidak ada lagi string sidik jari tersisa di kedua folder `.claude` | ☑ |
| 9 | Terapkan DEC-USK-003 pada `.gitignore` frontend | ☑ |
| 10 | Hapus folder `.agents` dari backend (19 file) dan frontend (14 file) sesuai DEC-USK-004 | ☑ |
| 11 | Uji: buka sesi dengan `--add-dir`, pastikan tujuh skill terlihat dan dapat dipanggil | ☑ |
| 12 | Perbarui status dokumen ini menjadi **Diimplementasikan** beserta tanggal dan pelaksananya | ☑ |

## 6. Cara memverifikasi hasilnya

Setelah eksekusi, tiga pemeriksaan berikut harus lulus.

**Pemeriksaan 1 — tidak ada sidik jari tersisa.**

```bash
grep -rho '[0-9a-f]\{64\}' NewQuilvianSystemBackend/.claude QuilvianSystemFrontendDev/.claude | wc -l
```

Hasil yang benar: `0`.

**Pemeriksaan 2 — jumlah skill sesuai target.**

```bash
find NewQuilvianSystemBackend/.claude/skills -name SKILL.md | wc -l    # harus 6
find QuilvianSystemFrontendDev/.claude/skills -name SKILL.md | wc -l   # harus 1
```

**Pemeriksaan 3 — tujuh skill terlihat dalam satu sesi.**

Buka sesi dengan `--add-dir`, ketik `/`, lalu pastikan ketujuh nama muncul tanpa nama ganda.
Ini pemeriksaan manual dan tidak bisa digantikan perintah.

## 7. Decision log wawancara

### Fakta yang ditemukan dari source, bukan dari wawancara

| ID | Fakta | Bukti |
| --- | --- | --- |
| F-11 | Claude Code terpasang versi 2.1.220, di atas ambang 2.1.203 yang disyaratkan dokumentasi untuk pemuatan skill dari direktori bersarang | `claude --version` |
| F-12 | Frontend `.claude/` memiliki 0 file ter-track git, backend 20 file | `git ls-files .claude` pada kedua repository |
| F-13 | Folder `.agents` masih lengkap dan ter-track: 6 skill di kedua repo, 19 file ter-commit di backend | `git ls-files .agents` |
| F-14 | `.agents` belum menerima satu pun perubahan yang sudah masuk ke `.claude`, yaitu aturan output dokumentasi, blok effort dan model, penawaran skill berikutnya, dan revisi kontrak keluaran skill desain | Perbandingan isi kedua folder |

### Keputusan

#### DEC-USK-001 — Penghapusan adapter frontend (menutup Keputusan 1)

| Field | Nilai |
| --- | --- |
| Status | `approved` |
| Keputusan | Lima adapter skill dan satu adapter aturan dokumentasi dihapus dari `QuilvianSystemFrontendDev/.claude/`. Seluruh delapan sidik jari ikut hilang |
| Alasan | Adapter tidak memuat prosedur apa pun. Versi 2.1.220 sudah mendukung pemuatan skill lewat `--add-dir`, sehingga alasan keberadaan adapter tidak berlaku lagi |
| Bukti biaya | Pembaruan `design-business-module` memaksa penghitungan ulang 2 dari 8 sidik jari, padahal tidak ada perilaku skill yang berubah |
| Konsekuensi | Skill lintas repo hanya dapat dipanggil dari sesi yang memuat backend |

#### DEC-USK-002 — Tempat menjalankan `/build-module-frontend` (menutup Keputusan 2)

| Field | Nilai |
| --- | --- |
| Status | `approved` |
| Keputusan | Dijalankan dari sesi backend dengan `--add-dir` ke frontend |
| Perintah baku | `cd NewQuilvianSystemBackend && claude --add-dir ../QuilvianSystemFrontendDev` |
| Alasan | Kontrak, roadmap, dan hasil task backend masih segar dalam satu alur kerja, sehingga serah terima antar task tidak perlu membaca ulang blueprint dari nol |
| Catatan | Batas kewenangan tetap dijaga isi `SKILL.md`, bukan oleh batas sesi |

#### DEC-USK-003 — Pencabutan pengecualian `.gitignore` frontend (menutup Keputusan 3)

| Field | Nilai |
| --- | --- |
| Status | `approved` |
| Keputusan | `.claude/skills/`, `.claude/commands/`, `.claude/rules/`, dan panduan dikecualikan dari `.gitignore` agar ikut ter-commit |
| Alasan | Terbukti 0 file ter-track, sehingga anggota tim yang melakukan clone tidak mendapat `build-module-frontend` sama sekali. Backend sudah ter-track 20 file, jadi kedua repo menjadi setara |
| Yang tetap diabaikan | Berkas sesi dan pengaturan pribadi di dalam `.claude/` |

#### DEC-USK-004 — Nasib folder `.agents` (Codex)

| Field | Nilai |
| --- | --- |
| Status | `approved` |
| Keputusan | Folder `.agents` dihapus dari kedua repository |
| Alasan | Memelihara dua suite berarti setiap perubahan aturan dikerjakan dua kali, ditambah lima sidik jari Codex yang harus dihitung ulang. `.agents` juga sudah tertinggal jauh: belum menerima aturan output dokumentasi, blok effort dan model, penawaran skill berikutnya, maupun revisi kontrak skill desain |
| Cakupan | 19 file di backend, 14 file di frontend |
| Pemeriksaan sebelum hapus | Kedua folder ter-track penuh oleh git — backend 19 dari 19, frontend 14 dari 14 — dan tidak ada perubahan yang belum ter-commit. Penghapusan dapat dikembalikan dari git history |
| Konsekuensi | Alur kerja Codex berhenti. Bila kemudian dibutuhkan lagi, pulihkan dari git history, bukan tulis ulang |
| Catatan penting | `.agents` **MUST NOT** dipulihkan lalu disamakan dengan `.claude` dengan cara menghapus adapternya. Codex tidak memiliki padanan `--add-dir`, sehingga adapter di sana masih dibenarkan secara teknis |

### Ringkasan status wawancara

| ID | Pokok | Status |
| --- | --- | --- |
| DEC-USK-001 | Adapter frontend dihapus, delapan sidik jari hilang | `approved` |
| DEC-USK-002 | `/build-module-frontend` dijalankan dari sesi backend dengan `--add-dir` | `approved` |
| DEC-USK-003 | Pengecualian `.gitignore` frontend dicabut | `approved` |
| DEC-USK-004 | Folder `.agents` dihapus dari kedua repository | `approved` |

Tidak ada pertanyaan terbuka yang memblokir eksekusi.
