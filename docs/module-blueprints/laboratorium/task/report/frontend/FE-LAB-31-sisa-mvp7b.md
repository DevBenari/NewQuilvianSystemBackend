# Laporan Perubahan Frontend — `FE-LAB-31` sisa cakupan `MVP-7b`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-31` — **sisa cakupan `MVP-7b`**, melengkapi [`FE-LAB-31.md`](FE-LAB-31.md) yang menutup `AC-157`..`AC-170` |
| Judul | Dua bentuk antibiogram, satuan MIC, alasan penimpaan, dan penanda isolat tidak diuji |
| Slice | `S4b` — pengisian hasil Mikrobiologi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-7b` |
| Trace | `AC-178`, `AC-187`, `AC-188`, `AC-190`; `LAB-DEC-115`, `LAB-DEC-123`, `LAB-DEC-124`, `LAB-DEC-126`; `VAL-112`, `VAL-113` |
| Contract version | `LAB-API-v1` **`r27`** bagian 22.2 — `approved` 2026-09-21 |
| Wewenang UI | Menyunting formulir hasil Mikrobiologi yang sudah ada. **Nol layar lain disentuh**, nol komponen base diubah |
| Dependency | `BE-LAB-53` ✅, `BE-LAB-61` ✅ — keduanya selesai 2026-09-21 |
| Klasifikasi | `MEDIUM` — 4 acceptance criteria, 4 berkas |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `6ea61bcad` (branch `YogaV2`) |
| Tanggal | 2026-09-23 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — keempat AC terbangun, aturannya terbukti lewat 7 uji unit baru, lint 0 error, build hijau. **Belum diklik di peramban** — sesi ini nol punya alat kendali peramban |

---

## 1. Masalah yang diperbaiki

Sinkronisasi bukti kode 2026-09-23 menemukan cakupan tambahan `MVP-7b` **belum tuntas**, dan
laporan `FE-LAB-31`/`32`/`33` memang nol pernah mengklaimnya. Dua di antaranya bukan sekadar AC
yang belum dikerjakan, melainkan **jalan buntu pada layar yang sudah dipakai**.

| Keadaan sebelumnya | Akibat nyata bagi analis |
| --- | --- |
| Kolom `Kadar` ada, **pemilih satuannya nol ada** | Analis yang mengisi kadar memperoleh penolakan *"Pilih satuan untuk nilai kadar."* dan **nol punya tempat memilihnya**. Barisnya nol dapat disimpan sama sekali |
| `resultOverrideReason` dikirim, **nol kotak isian merendernya** | Begitu analis mengubah `Hasil` dari hitungan sistem, backend menuntut alasan yang **nol dapat diketik dari mana pun** |
| Tabel antibiogram **satu bentuk**, enam kolom serentak | Analis mengisi kolom yang nol bermakna bagi metode yang sedang dipakai |
| `isSusceptibilityTested` dikirim, **nol punya kontrol** | Kuman yang ditemukan tetapi tidak diuji nol dapat dibedakan dari pekerjaan yang belum selesai |

**Ketiga yang pertama berbagi satu bentuk:** ruasnya sudah sampai ke payload dan aturannya sudah
ditulis serta lulus uji — **permukaannya yang tidak ada**. Uji unit atas fungsi murni nol dapat
menangkapnya, sebab yang diuji adalah fungsi, bukan apakah ada jalan mengisi masukannya.

---

## 2. Proses bisnis

**Pelaku.** Analis Mikrobiologi yang mengisi hasil biakan.

**Alur berurutan.** Analis membuka halaman hasil dari daftar pantau → memilih pemeriksaan →
mengisi status temuan, kualifikasi, jenis biakan, dan **metode uji** → menambah isolat →
mengisi antibiogram per isolat → menyimpan Draft atau Final.

**Metode uji menentukan bentuk pengukurannya**, dan inilah yang berubah:

| Metode | Yang diukur | Kolom yang tampil |
| --- | --- | --- |
| **Difusi cakram** | Lebar zona hambat dalam milimeter | `UG`, rentang `R-S`, `Zona (mm)` |
| **Dilusi** | Kadar hambat minimum beserta satuannya | `Kadar`, `Satuan` |

Contoh berangka. Pada dilusi, analis mengisi `1,25` lalu memilih `mg/L`. Tanpa satuan, `1,25`
dapat dibaca sebagai `mg/L` maupun `ug/mL` — **berbeda seribu kali**, dan itu sebabnya `VAL-112`
menolaknya. Pada difusi, analis mengisi zona `13`; bila breakpoint organisme-antibiotik itu
terisi, server menghitung interpretasinya sendiri.

**Jalur tidak normalnya.** Ketika analis tidak setuju dengan hitungan server dan memilih nilai
lain, layar menampilkan hitungan aslinya berdampingan **dan meminta alasan**. Alasan itu yang
membuat keputusan terapi dapat ditelusuri kemudian.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/master-data-feature-standard.md`; kontrak `r27` bagian 22.2;
`lab-microbiology-result-form.jsx`, `-panel.jsx`, `-rules.js`,
`use-lab-microbiology-result-editor.jsx`; `base-form-control.jsx`;
`lab-microbiology-specimen-section.jsx` sebagai rujukan pola pemilih satuan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `lib/hooks/.../lab-microbiology-result-rules.js` | **+1 fungsi murni** `isResultOverriding`, **+1 aturan** penolakan penimpaan tanpa alasan |
| `components/view/.../lab-microbiology-result-form.jsx` | Dua bentuk tabel; pemilih satuan; kotak alasan penimpaan; penanda isolat tidak diuji; keterangan metode belum dipilih |
| `components/view/.../lab-microbiology-result-panel.jsx` | Meneruskan pilihan satuan ke formulir |
| `tests/unit/lab-microbiology-result-rules.test.mjs` | **+7 uji** |

**Nol berkas di luar folder Mikrobiologi disentuh.** Nol slice Redux baru, nol service baru, nol
komponen base diubah, nol CSS Module ditambah.

### 3.3 Empat keputusan pelaksanaan yang pantas dibaca

**1. Satuan MIC memakai registry pemilih yang sudah ada, bukan sumber kedua.** Kontrak menyebut
`concentrationUnitId` menunjuk `MstMeasurement`; panel sudah memuat `useSelectResource("measurements")`
untuk satuan volume specimen. Daftar yang sama diteruskan ke antibiogram — **satu permintaan,
dua pemakai**. Sumber kedua untuk daftar yang sama pasti bercabang.

**2. Kotak alasan hanya muncul ketika benar-benar menimpa.** `isResultOverriding` menuntut **dua**
syarat: server sudah menghitung sesuatu, **dan** analis memilih nilai yang berbeda. Baris yang
breakpoint-nya belum terisi — server nol menghitung apa pun — **bukan** penimpaan melainkan
satu-satunya sumber (`VAL-114`); menuntut alasan di sana berarti meminta analis
mempertanggungjawabkan sesuatu yang nol pernah dibantah siapa pun. Ruas yang selalu tampak juga
berhenti dibaca.

**3. Metode yang belum dipilih memakai bentuk difusi, dan itu dinyatakan.** Difusi cakram metode
yang paling lazim. Keterangannya ditulis supaya analis tahu bentuknya **dapat berubah** — bukan
dibiarkan tampak seperti pilihan yang memang kosong.

**4. Penanda ditulis terbalik di layar.** Ruasnya `isSusceptibilityTested` (berdefault benar),
tetapi kotak centangnya berbunyi **"Kepekaan tidak diuji"**. Analis menandai **pengecualian**, dan
kotak centang yang sudah tercentang bagi keadaan normal akan dicentang-lepas tanpa dibaca.

---

## 4. Dokumentasi endpoint

**NOT APPLICABLE.** Nol endpoint baru dipanggil. Ruas yang diisi keempat kontrol ini sudah ada
pada payload `PUT /lab-examinations/{id}/result/microbiology` sejak `FE-LAB-31` versi pertama.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | **0 error** | `PASS` | Keluaran perintah |
| Uji unit berkas ini | **29/29 lulus** — 22 lama, **7 baru** | `PASS` | Keluaran perintah |
| Seluruh suite unit | **1640 lulus, 7 gagal** | `PASS` | Lihat bawah |
| Baseline suite pada pohon **bersih** | **1633 lulus, 7 gagal** | `PASS` | Dijalankan lewat `git stash` |
| `npm run build` | **Hijau**, `postbuild` standalone selesai | `PASS` | Keluaran perintah |

> **Ketujuh kegagalan suite BUKAN milik perubahan ini, dan itu dibuktikan bukan diasumsikan.**
> Pohon bersih diuji lewat `git stash` dan menghasilkan **ketujuh kegagalan yang sama persis** —
> lima `FE-RWI-042`/`FE-RWI-043` milik rawat inap, satu `M0` bank darah, satu registry route.
> Perubahan ini menaikkan yang lulus **1633 → 1640** dengan jumlah gagal **tetap**.
>
> *(Catatan: laporan `FE-LAB-24` mencatat baseline **6** pada 2026-09-22. Ia sudah menjadi 7
> sebelum sesi ini dimulai, dari pekerjaan lain — bukan dari perubahan ini.)*

Uji manual: **`NOT FEASIBLE`** — sesi ini nol punya alat kendali peramban. Batas yang sama sudah
dicatat `FE-LAB-24`, `FE-LAB-31` versi pertama, dan `FE-LAB-34`.

**Yang dapat dibuktikan tanpa peramban sudah dibuktikan:** aturan murninya lulus uji, seluruh
route terkompilasi pada build, dan lint bersih. Yang **tidak** dapat dibuktikan tanpa itu disebut
apa adanya.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-178` menyimpan kadar tanpa satuan **ditolak beserta sebabnya**; pemilih satuannya tersedia | **Terbangun; aturannya terbukti** | Pemilih `Satuan` berdiri di sebelah `Kadar` pada bentuk dilusi; 1 uji dua arah |
| `AC-187` penimpaan **ditolak bila alasannya kosong**; hasil timpaan tersimpan beserta nilai hitungan aslinya | **Terbangun; aturannya terbukti** | 4 uji — ditolak, diterima, nilai sama bukan penimpaan, server nol menghitung bukan penimpaan |
| `AC-188` difusi menampilkan `UG`/rentang/zona; dilusi menampilkan MIC beserta satuannya | **Terbangun, belum dilihat** | `<thead>` dan sel dikondisikan `susceptibilityMethod` |
| `AC-190` isolat tanpa baris kepekaan tersimpan tanpa penolakan, **dan penandanya dapat disetel** | **Terbangun; aturannya terbukti** | 2 uji — payload membawa `false`, validasi meloloskan isolat tanpa baris |
| DoD — nol pengetikan bebas pada organisme dan antibiotik | ✅ **Tetap terpenuhi** | Keduanya tetap `BaseSelectField` |
| DoD — nol pilihan `NeedsAttention`/`Critical` | ✅ **Tetap terpenuhi** | Status temuan nol disentuh |
| DoD — seluruh uji Laboratorium tetap lulus | ✅ **Terpenuhi** | Nol kegagalan baru, dibuktikan terhadap pohon bersih |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Satu selisih kontrak dilaporkan, NOL diubah** | `r27` bagian 22.2 menurunkan `result` **dari wajib menjadi opsional** — server mengisinya bila breakpoint tersedia. Aturan frontend masih **mewajibkannya** (`lab-microbiology-result-rules.js`, mengutip `AC-165`/`VAL-83` milik `r24`). Akibatnya analis yang mengisi zona tanpa memilih interpretasi tertahan di layar, padahal server sanggup menghitungnya. **Nol disentuh** sebab berada di luar keempat AC yang diberi wewenang task ini, dan mengubahnya menyentuh `AC-165` yang masih berlaku. Perlu keputusan pemilik modul: `AC-165` dipertahankan, atau diselaraskan dengan `r27` |
| Peringatan | Nol peringatan lint baru |
| Masalah yang diketahui | Keempat kontrol **belum diklik di peramban** |
| Risiko tersisa | **Rendah pada aturannya, sedang pada tata letaknya.** Bentuk dilusi memindahkan dua kolom dan menambah satu pemilih di dalam sel tabel; lebarnya pada layar sempit belum dilihat |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 4 berkas `M`, seluruhnya dalam cakupan. **Nol operasi Git dijalankan** |
| Langkah berikutnya | Klik keempat kontrol terhadap backend berisi data; putuskan selisih `result` wajib/opsional di atas |
