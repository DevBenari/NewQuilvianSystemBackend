# `ACC-DEP-007` — Laporan untuk lead/platform: propagasi registry governance

| Field | Isi |
|---|---|
| Dependency | `ACC-DEP-007` |
| Klasifikasi | **PLATFORM / ENGINEERING GOVERNANCE defect** |
| Pemilik | Lead |
| Sifat laporan | **Read-only.** Tidak ada berkas governance, checker, atau workflow yang diubah |
| Ditemukan oleh | Owner Accounting, saat `BE-ACC-001` dan `BE-ACC-002` |
| Bukan | Masalah Accounting. Accounting hanya modul pertama yang tersandung |
| Backend source SHA | `ca6b7e0` (branch `rizkiG`) |
| Pembanding | `origin/QuilvianIntegrationBackend` |
| Tanggal | 2 September 2026 |

> **Batas laporan ini.** Empat hal sengaja **tidak** dilakukan, sesuai instruksi owner:
> tidak membuat governance canonical kedua; tidak menyalin registry dari suite skill ke backend;
> tidak mengubah checker; tidak menghapus berkas governance. Yang ada di sini murni temuan dan
> usulan.

---

## 1. Ringkasan satu paragraf

Gerbang CI QBE mati total. Checker mencari dokumen governance di
`agents/rules/engineering/`, sebuah folder yang tidak ada di branch mana pun. Perbaikannya
sebenarnya **sudah pernah dibuat dan sudah masuk** ke branch integration lewat PR #63 pada
31 Agustus 2026 — tetapi separuhnya **terhapus lagi** oleh sebuah merge tiga hari kemudian.
Berkas governance-nya selamat; perubahan pada checker-nya tidak. Sejak itu setiap PR ke
`QuilvianIntegrationBackend` berjalan tanpa penjagaan QBE sama sekali.

---

## 2. Koreksi penting atas catatan sebelumnya

Catatan `ACC-DEP-007` pada `05-prerequisite-readiness.md` sebelum hari ini menyebut akar
masalahnya adalah commit `4db8909` yang menghapus governance tanpa menyesuaikan checker.

**Itu benar untuk 28 Agustus, tetapi sudah tidak berlaku lagi.** Ada satu peristiwa sesudahnya
yang belum tercatat, dan peristiwa itu mengubah usulan perbaikannya secara mendasar. Bagian 3
menuliskan urutan lengkapnya.

---

## 3. Urutan peristiwa

| # | Tanggal | Commit | Pelaku | Yang terjadi |
|---:|---|---|---|---|
| 1 | — | `f5fdbaf` | — | Governance dipindahkan **ke** `agents/rules/engineering/`. Checker ikut diarahkan ke sana. Konsisten |
| 2 | 28 Agu 2026 | `4db8909` | MHamzah1 | Seluruh `agents/rules/` dihapus, termasuk `engineering/`. **Checker tidak disesuaikan.** Gerbang mati |
| 3 | 31 Agu 2026 | `c9692d0` | andryzainhome | **Perbaikan lengkap.** Governance dipulihkan ke `docs/engineering/`, **dan** checker diarahkan ke `docs/engineering/`. Masuk lewat PR #63 (`c19f801`) |
| 4 | 1 Sep 2026 | `3d14cac` | Muhammad Hamzah | Merge integration ke branch `MHamzah`. **Perubahan checker dibatalkan**; berkas governance tetap |
| 5 | 1 Sep 2026 | `fe88b1d` | — | PR #68 menggabungkan `MHamzah` kembali ke integration, **membawa serta pembatalan itu** |

Akar masalah hari ini adalah **langkah 4**, bukan langkah 2.

---

## 4. Bukti langkah 4 — pembatalan itu tidak mungkin terjadi karena kecelakaan git

Ini bagian yang paling perlu diperiksa lead, jadi ditulis lengkap.

Git menyelesaikan penggabungan sebuah berkas dengan membandingkan tiga versi: versi **dasar**
(titik pisah kedua branch), versi **kita** (branch tujuan), dan versi **mereka** (branch sumber).

Untuk `tooling/qbe/Invoke-QbeConformanceCheck.ps1` pada merge `3d14cac`:

| Peran | Commit | Isi berkas (hash blob) | Keterangan |
|---|---|---|---|
| Dasar | `3c4c06f` | `161ea88` | versi lama, menunjuk `agents/rules/engineering/` |
| Kita (`MHamzah`) | `734de81` | `161ea88` | **sama persis dengan dasar — tidak pernah disentuh** |
| Mereka (integration) | `ee18aac` | `3a9df9b` | versi hasil perbaikan, menunjuk `docs/engineering/` |
| **Hasil merge** | `3d14cac` | **`161ea88`** | **kembali ke versi lama** |

Karena versi "kita" **identik** dengan versi "dasar", git tidak melihat konflik sama sekali. Satu
sisi berubah, sisi lain tidak — git otomatis mengambil sisi yang berubah. Hasil yang benar
seharusnya `3a9df9b`.

Hasilnya `161ea88`. Itu berarti versi lama **dipilih secara sengaja**: lewat opsi seperti
`-X ours`, lewat pengembalian berkas secara manual, atau lewat penyuntingan sesudah merge.

**Kesimpulannya:** ini bukan konflik yang salah diselesaikan. Ini penimpaan aktif atas sebuah
perbaikan yang sudah masuk.

---

## 5. Keadaan sekarang — apa yang ada di mana

| Lokasi | `BACKEND_ENGINEERING_CONTRACT.md` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | `QBE_EXCEPTIONS.json` |
|---|:---:|:---:|:---:|
| `backend:agents/rules/engineering/` — **yang dibaca checker** | tidak ada | tidak ada | tidak ada |
| `backend:docs/engineering/` @ `origin/QuilvianIntegrationBackend` | **ada** | **ada** | **ada** |
| `backend:docs/engineering/` @ `rizkiG` | tidak ada | tidak ada | tidak ada |
| `QuilvianEngineeringSkills:agents/rules/backend/engineering/` | **ada** | **ada** | — |
| Plugin cache terpasang `.claude/rules/backend/engineering/` | tidak ada | tidak ada | — |

Sisa folder `agents/rules/` **masih ada dan masih terlacak git** di backend — tujuh berkas
(`API_RULES.md`, `CROSS_REPO_RULES.md`, `DATABASE_RULES.md`, `REPORT_TEMPLATE.md`,
`REVIEW_RULES.md`, `TASK_CLASSIFICATION.md`, `TASK_RULES.md`), tanpa subfolder `engineering/`.
Ini yang membuat orang mengira path checker sudah benar padahal tidak.

---

## 6. Bukti kegagalan yang dapat diulang

Dijalankan 2 September 2026 pada branch `rizkiG`:

```bash
powershell -NoProfile -ExecutionPolicy Bypass \
  -File tooling/qbe/Invoke-QbeConformanceCheck.ps1 \
  -BaseRef aa837d7 -HeadRef HEAD -Mode Strict
```

Keluarannya:

```text
TOOL ERROR: Canonical governance missing: agents/rules/engineering/BACKEND_ENGINEERING_CONTRACT.md
Final result: TOOL ERROR
EXITCODE=2
```

Checker berhenti pada baris 31–33, yaitu **sebelum** satu berkas source pun diperiksa.

Baris yang menyusun path tersebut:

| Baris | Isi |
|---:|---|
| 28 | `'agents/rules/engineering/BACKEND_ENGINEERING_CONTRACT.md',` |
| 29 | `'agents/rules/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md'` |
| 36 | `$contract = Get-Content -Raw -LiteralPath (Join-Path $root 'agents/rules/engineering/BACKEND_ENGINEERING_CONTRACT.md')` |
| 85 | `return Join-Path $root 'agents/rules/engineering/QBE_EXCEPTIONS.json'` |
| 162 | `$registryPath = Join-Path $root 'agents/rules/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md'` |

---

## 7. Akibatnya pada CI — gerbangnya tidak sekadar longgar, melainkan mati

Workflow `.github/workflows/qbe-conformance.yml` menjalankan checker lalu meneruskan kode
keluarnya:

```yaml
& ./tooling/qbe/Invoke-QbeConformanceCheck.ps1 ... 
exit $LASTEXITCODE
```

Kode keluar `2` membuat langkah itu gagal, sehingga job `QBE Strict GitRange` gagal pada
**setiap** PR ke `QuilvianIntegrationBackend`, apa pun isi PR-nya.

Ini menimbulkan bahaya yang khas: sebuah gerbang yang selalu merah akan berhenti dipercaya.
Begitu orang terbiasa menggabungkan PR sambil mengabaikannya, gerbang itu efektifnya tidak ada —
dan pelanggaran QBE yang sebenarnya ikut lolos tanpa ada yang menyadari.

Langkah ringkasan sesudahnya sudah mengantisipasi keadaan ini dan menulis pesan *"No structured
result was produced"*, tetapi pesan itu hanya menjelaskan, tidak memperbaiki.

---

## 8. Klasifikasi registry `docs/engineering/` di backend

Pertanyaan yang diminta owner dijawab: apakah salinan di backend itu **legacy copy**,
**generated copy**, **synced consumer**, atau **governance yang belum selesai migrasi**?

Untuk menjawabnya, isi kedua registry dibandingkan baris demi baris.

| Pembanding | Jumlah baris tabel |
|---|---:|
| `backend:docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` @ integration | 48 |
| `QuilvianEngineeringSkills:agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | 52 |

Selisihnya **tepat empat baris, dan seluruhnya tentang Accounting**:

1. baris kepemilikan `| Corporate | AccountingManagement / Accounting | … | Acc | ACTIVE |`
2. baris kepanjangan prefix `| Acc | Accounting |`
3. dua baris catatan lifecycle tertanggal 1 September 2026

**Tidak ada satu pun baris lain yang berbeda.** Tidak ada baris yang hanya ada di backend.

**Klasifikasinya: `synced consumer` yang tertinggal tepat satu pendaftaran.**

Ini bukan `legacy copy` — isinya sama persis sampai pendaftaran terakhir. Bukan `generated copy` —
tidak ada tooling yang menghasilkannya, dan tidak ada penanda "generated" di berkasnya. Bukan
pula migrasi yang belum selesai dalam arti isinya menyimpang; migrasinya justru sudah selesai di
sisi isi, dan yang belum selesai adalah **cara menyalurkannya**.

Persoalan sebenarnya: penyalinan itu dikerjakan manual, tidak ada yang memiliki tugasnya, dan
tidak ada pemeriksaan yang memberi tahu kalau ia tertinggal.

---

## 9. Matriks sumber kebenaran

Empat pemakai governance, empat path berbeda, dan hanya dua yang benar-benar ada.

| # | Pemakai | Path yang dibacanya | Ada? | Memuat `Acc`? |
|---:|---|---|:---:|:---:|
| 1 | Skill `build-module-backend` (agent) | `<suite>/rules/backend/engineering/` = `QuilvianEngineeringSkills/agents/rules/backend/engineering/` | **ada** | **ya, `ACTIVE`** |
| 2 | `AGENTS.md` baris 20 (model lama) | `backend:docs/engineering/` | hanya di integration | **tidak** |
| 3 | `AGENTS.md` baris 40 (model baru) | akar `rules/` terpasang, `rules/backend/engineering/` | **tidak** | — |
| 4 | Checker QBE + CI | `backend:agents/rules/engineering/` | **tidak** | — |

Baris 2 dan 3 berada di **satu berkas yang sama**. `AGENTS.md` backend memuat dua model
governance sekaligus dan tidak menyatakan mana yang menang:

| Baris | Isi |
|---:|---|
| 11 | urutan wewenang menyebut `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` |
| 17 | *"Lapisan operasionalnya tinggal di dalam repository ini pada folder `agents/rules/`"* |
| 20 | menunjuk `docs/engineering/…` |
| 40 | menunjuk `rules/backend/engineering/…` |
| 53 | *"Repository ini **tidak lagi** memiliki folder `agents/rules/`"* |
| 60 | pendaftaran wajib di `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

Baris 17 dan baris 53 saling meniadakan secara langsung.

---

## 10. Temuan tambahan — suite skill terpasang ikut tertinggal

Ditemukan saat memeriksa pemakai nomor 1. Dua versi plugin terpasang, dan keduanya tidak memuat
governance yang diwajibkan `AGENTS.md` baris 37–47:

| Versi terpasang | Isi `.claude/rules/` | `rules/GLOBAL_RULES.md` | `rules/backend/engineering/` |
|---|---|:---:|:---:|
| `0.1.0` | 7 berkas `backend/` + `rule-output/` + `frontend/` | tidak ada | **tidak ada** |
| `1.0.0` | hanya `rule-output/` | tidak ada | **tidak ada** |

Versi `1.0.0` yang lebih baru justru memuat **lebih sedikit** — seluruh `rules/backend/` hilang
dari sana.

Akibat praktisnya nyata dan sudah terjadi: agent yang hanya membaca plugin cache akan
menyimpulkan prefix `Acc` belum terdaftar, lalu memblokir task Accounting secara keliru. Persis
itu yang sempat terjadi pada sesi `BE-ACC-001` sebelum dikoreksi terhadap repo sumber
`QuilvianEngineeringSkills`.

Teks `SKILL.md` yang terpasang juga tertinggal: versi `0.1.0` masih menunjuk `.codex` dan
`docs/engineering/` repository target, sedangkan versi di repo sumber sudah menunjuk
`rules/backend/engineering/` milik suite.

---

## 11. Usulan penyelesaian

Disusun sebagai usulan untuk lead. Tidak satu pun dikerjakan dari sisi Accounting.

### Yang mendesak — memulihkan gerbang CI

**Terapkan ulang bagian checker dari `c9692d0` yang hilang.** Perbaikannya sudah pernah ditulis,
sudah pernah ditinjau, dan sudah pernah masuk lewat PR #63. Yang perlu dilakukan hanya
mengembalikan lima baris yang tertimpa merge `3d14cac`:

| Baris | Dari | Menjadi |
|---:|---|---|
| 28, 29, 36, 85, 162 | `agents/rules/engineering/…` | `docs/engineering/…` |

Sesudah itu checker menemukan governance yang memang sudah ada di integration, dan gerbangnya
hidup kembali.

**Ini pekerjaan lead, bukan Accounting.** Tooling QBE milik lead, dan menambalnya dari sisi modul
yang sedang menunggu gerbang itu adalah persis konflik kepentingan yang harus dihindari.

### Yang menyusul — menutup celah yang membuatnya bisa terulang

| # | Usulan | Alasan |
|---:|---|---|
| 1 | Tambahkan pemeriksaan CI yang gagal bila registry backend tertinggal dari registry suite | Cacat ini lolos karena tidak ada yang mengawasi selisihnya |
| 2 | Bereskan `AGENTS.md` menjadi satu model governance saja | Baris 17 dan 53 saling meniadakan; siapa pun boleh memilih yang menguntungkannya |
| 3 | Putuskan nasib sisa folder `agents/rules/` — dicabut atau diakui | Selama ia ada sementara `AGENTS.md` menyatakan tidak ada, kekeliruan yang sama akan terulang |
| 4 | Terbitkan ulang plugin suite skill agar memuat `rules/GLOBAL_RULES.md` dan `rules/backend/engineering/` | Agent tidak dapat mematuhi aturan yang tidak ikut terkirim |
| 5 | Bedakan kegagalan tooling dari kegagalan kepatuhan pada ringkasan CI | `TOOL ERROR` sekarang terlihat sama merahnya dengan `VIOLATION`, sehingga keduanya sama-sama diabaikan |

### Keputusan yang masih harus diambil lead

Satu hal belum dapat diputuskan dari sisi Accounting: **mana yang menjadi rumah canonical**.

| Pilihan | Konsekuensi |
|---|---|
| A. `backend:docs/engineering/` | Checker dan CI bekerja langsung, karena keduanya berjalan di dalam repo backend. Suite skill harus menyalin **dari** sana |
| B. `QuilvianEngineeringSkills` | Sejalan dengan pemusatan yang sudah diputuskan, tetapi CI backend tidak dapat membaca repo lain — tetap perlu salinan tersinkron di backend |

Kedua pilihan tetap menyisakan satu salinan di backend, karena CI hanya melihat repo yang
di-checkout. Yang membedakan hanyalah **arah penyalinannya** dan **siapa yang bertanggung jawab**.

Selama keputusan ini belum diambil, cacat yang sama akan lahir kembali setiap kali ada
pendaftaran prefix baru.

---

## 12. Yang tidak terhalang oleh `ACC-DEP-007`

Perlu ditegaskan supaya tidak ada yang menghentikan pekerjaan lebih dari seharusnya.

| Hal | Terhalang? | Alasan |
|---|:---:|---|
| Penulisan kode backend lokal | **Tidak** | Preflight skill `build-module-backend` bersifat **membaca dokumen**, bukan menjalankan checker |
| `BE-ACC-001` sampai `BE-ACC-005` | **Tidak** | Registry canonical suite terbaca, dan `Acc` di sana sudah `ACTIVE` |
| Verifikasi kepatuhan QBE | Sebagian | Dapat dilakukan manual terhadap registry suite, tetapi tidak otomatis |
| **Merge ke `QuilvianIntegrationBackend`** | **Ya** | Job CI gagal pada setiap PR |

Jadi `ACC-DEP-007` adalah **gerbang merge**, bukan gerbang eksekusi task. Menyatakan task
Accounting `BLOCKED` karenanya adalah kekeliruan.
