# Laporan Perubahan Frontend — `PAY-FE-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-11` |
| Judul | Payroll V2 fase 2 frontend: relokasi 10 entitas data induk Payroll dari `hr/master-data` ke `payroll/master-data` |
| Slice | Payroll mandiri — penataan folder (tanpa perubahan perilaku) |
| Roadmap | Tidak ada roadmap Payroll terdaftar; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | `PAYROLL_V2_01` bagian 7 ("Frontend fase 2 ... perubahan ini kosmetik"); permintaan eksplisit pemilik 11 November 2026 (jawaban "Ya, sekaligus 10 entitas" dan "Catat keputusan + kerjakan frontend; backend jadi rencana") |
| Contract version | **Tidak ada perubahan kontrak.** Rute `/payroll/**`, endpoint, dan nama izin tidak disentuh |
| Wewenang UI | Tidak ada. Hanya lokasi berkas dan jalur impor yang berubah; markup, teks, dan gaya tidak disentuh |
| Dependency | Tidak ada. Backend tidak disentuh |
| Klasifikasi | `HEAVY` menurut jumlah berkas (90 dipindah, 45 diubah), tetapi `LOW RISK` menurut isi: perubahan hanya jalur impor |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; di repository backend hanya laporan ini dan baris register |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `03a57481f` (perubahan task ini belum di-commit) |
| Tanggal | 11 November 2026 |
| Status | 🟡 **Selesai secara statis; menunggu pemilik menjalankan `npm run build` dan membuka 10 layar.** Isi 90 berkas dibuktikan identik dengan `HEAD` selain jalur impor; resolver impor, suite unit, dan ESLint sama dengan baseline sebelum pemindahan. `npm run build`/`dev` tidak dijalankan |

---

## 1. Keadaan yang ditemukan di awal

Rute dan menu Payroll sudah berada di `/payroll/master-data/<entitas>` (fase 1), tetapi 90 berkas implementasinya masih di folder HRD: 10 entitas × 9 berkas (view daftar, view detail, view form, tiga hook, konstanta, util, slice Redux). Akibatnya pencarian kode Payroll harus masuk folder HR, dan folder `hr/master-data` memuat entitas yang bukan milik HR.

## 2. Proses bisnis dari sisi pengguna

**Tidak ada perubahan yang terlihat pengguna.** Menu, alamat halaman, tampilan, dan perilaku 10 layar data induk Payroll sama seperti sebelumnya. Yang berubah hanya letak berkas di dalam kode.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PAYROLL_V2_01` bagian 7, `rules/frontend/frontend-architecture.md` (peran `hooks`, `state`, `constants`, `utils`, `view`), `master-data-feature-standard.md` (jalur slice `src/lib/state/slice/<domain>/master-data/...`), `next.config.js` (redirect `/hr/master-data/<fitur>` ke `/payroll/master-data/<fitur>`, tidak diubah), `src/lib/state/store.jsx`, 40 halaman rute `src/app/payroll/master-data/**`.

### 3.2 Entitas yang dipindah (10)

`allowance-type`, `deduction-type`, `hazard-allowance-policy`, `on-call-allowance-policy`, `payroll-component`, `payroll-component-category`, `payroll-period`, `salary-grade`, `salary-structure`, `shift-allowance-policy`.

### 3.3 Pemetaan lokasi (segmen `hr` menjadi `payroll`)

| Lapisan | Dari | Ke |
| --- | --- | --- |
| View (daftar, detail, form) | `src/components/view/hr/master-data/<slug>/` | `src/components/view/payroll/master-data/<slug>/` |
| Hook (daftar, detail, editor) | `src/lib/hooks/hr/master-data/<slug>/` | `src/lib/hooks/payroll/master-data/<slug>/` |
| Konstanta | `src/lib/constants/hr/master-data/<slug>/` | `src/lib/constants/payroll/master-data/<slug>/` |
| Util | `src/utils/hr/master-data/<slug>/` | `src/utils/payroll/master-data/<slug>/` |
| Slice Redux | `src/lib/state/slice/hr/master-data/master-data-<slug>-slice.jsx` | `src/lib/state/slice/payroll/master-data/master-data-<slug>-slice.jsx` (folder baru) |

Dipindah dengan `mv` biasa (bukan `git mv`), jadi tidak ada yang ter-stage. Daftar lengkap 90 pasangan jalur lama dan baru ada di scratchpad sesi (`p2/moved-files.tsv`).

### 3.4 Berkas yang berubah selain yang dipindah (45)

| Kelompok | Jumlah | Perubahan |
| --- | ---: | --- |
| Halaman rute `src/app/payroll/master-data/<slug>/**` (halaman, klien, buat, detail, ubah) | 40 | Hanya baris `import` |
| `src/lib/state/store.jsx` | 1 | 10 baris diganti (impor dan ekspor ulang slice); tidak ada yang bertambah atau berkurang |
| `src/lib/hooks/hr/master-data/benefit-eligibility-rule/use-master-data-benefit-eligibility-rule-editor.jsx` | 1 | Hanya impor; **tidak dipindah** (milik HR, memakai salah satu entitas yang dipindah) |
| `tests/unit/payroll-master-allowance-type-form.test.mjs` | 1 | Jalur sumber yang dibaca |
| `tests/unit/payroll-master-hidden-currency-code.test.mjs` | 1 | Dua jalur bertemplat `${entity}` pada `import()` dinamis |
| `tests/unit/payroll-module-contract.test.mjs` | 1 | Satu jalur bertemplat `${entity}` pada pembacaan berkas konstanta |

Dua test dengan jalur bertemplat tidak tertangkap oleh penggantian otomatis pertama dan menimbulkan 16 kegagalan; keduanya diperbaiki dan hasil akhirnya sama dengan baseline (lihat §6). Asersi test tidak dilonggarkan.

**Sengaja tidak diubah:**

| Hal | Alasan |
| --- | --- |
| Rute `/payroll/**`, menu, `next.config.js` | Di luar cakupan. Asersi redirect `/hr/master-data/<fitur>` → `/payroll/master-data/<fitur>` pada `payroll-module-contract.test.mjs` dan asersi menu pada `hr-master-data-sidebar-grouping.test.mjs` (baris yang menyebut `/hr/master-data/salary-grade`) adalah rute, bukan jalur berkas |
| `src/style/hr/master-data/*` | CSS tidak disentuh |
| Konstanta endpoint dan `routeBase` | Tidak berubah |
| Entitas HR lain di `hr/master-data` | Tidak ikut dipindah |

### 3.5 Kepatuhan arsitektur frontend

Arah dependensi (view → hook → state/service → axios) tidak berubah dan tidak ada impor siklik baru. Slice tetap didaftarkan di `store.jsx` dengan kunci state yang sama. Tidak ada komponen, hook, atau slice baru.

**UI GATE: N/A — relokasi tanpa perubahan markup.** Tidak ada JSX atau CSS yang dibuat atau diubah selain baris `import`.

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — tidak ada perubahan pada state memuat, kosong, gagal, maupun tanpa hak akses.

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada perubahan pemanggilan API.

---

## 6. Verifikasi

Baseline diambil sebelum memindah, pada `HEAD` `03a57481f` dengan working tree bersih.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Isi 90 berkas yang dipindah dibandingkan dengan `HEAD`, setelah penggantian jalur dibalik | 90 dari 90 identik. Dari itu, 80 berkas hanya berganti baris impor dan 10 berkas tidak berubah sama sekali | `PASS` | Skrip pembanding di scratchpad (bukan di repo) |
| Diff 40 halaman rute | Tidak ada baris selain `import` yang berubah | `PASS` | `git diff -U0` |
| Resolver impor statis (skrip di scratchpad: menguraikan `from`, `import()`, `require()` pada 6849 berkas `src` dan `tests`, alias `@/`, relatif, ekstensi, `index`) | Sebelum 61 impor tak terselesaikan, sesudah 61, **himpunan identik**. Seluruhnya impor lama yang tak terkait. Catatan: laporan fase 1 menyebut 58 dengan resolver yang berbeda; resolver ini juga memindai `tests` | `PASS` | `diff` hasil sebelum dan sesudah |
| Grep sisa jalur lama untuk 10 slug di `src` dan `tests` | Tidak ada jalur berkas lama tersisa. Satu kemunculan `/hr/master-data/salary-grade` adalah rute pada asersi menu (sengaja dipertahankan) | `PASS` | Keluaran `grep` |
| `node --test` terarah: enam berkas `payroll-*` dan `hr-master-data-sidebar-grouping` | 64 test, 64 lulus | `PASS` | Keluaran perintah |
| Seluruh `tests/unit/` | Sebelum: 4728 test, 4681 lulus, 47 gagal. Sesudah: 4728 test, 4681 lulus, 47 gagal. **Daftar nama yang gagal identik** (`diff` kosong). Tidak ada yang terkait Payroll | `EXISTING / ENVIRONMENT ISSUE` (0 baru) | Keluaran perintah; `diff` daftar nama |
| `npx eslint` pada 124 berkas (90 dipindah + 34 pemakai) sebelum dan sesudah | Sebelum dan sesudah sama: 0 galat, 22 peringatan (21 `react-hooks/set-state-in-effect`, 1 `react-hooks/exhaustive-deps`). Pesan per berkas, aturan, dan baris identik | `PASS` (0 galat baru) | Keluaran ESLint format JSON |
| `npx eslint` pada dua test yang diubah jalurnya | exit 0 | `PASS` | Keluaran perintah |
| Git: stage | 0 berkas ter-stage | `PASS` | `git diff --cached` |
| `npm run build` / `npm run dev` | Tidak dijalankan (dilarang pemilik) | `NOT RUN` | — |

MANUAL TEST: `NOT FEASIBLE` oleh agen — tidak ada server atau peramban yang boleh dijalankan. Daftar yang harus dibuka pemilik ada di bawah.

AUTOMATED TEST: `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/` — 4728 test, hasil identik dengan baseline sebelum pemindahan (47 kegagalan lama, nama sama).

**Yang dibuka pemilik setelah `npm run build` (atau `npm run dev`).** Hentikan dulu server pengembangan yang sedang berjalan supaya cache lama tidak menampilkan jalur berkas yang sudah berpindah.

Untuk setiap slug di §3.2, buka dan periksa: daftar `/payroll/master-data/<slug>`, tambah `/payroll/master-data/<slug>/create`, detail dan ubah dengan mengeklik baris pada daftar (alamat detail memakai token privat, bukan ID mentah). Yang dilihat: halaman termuat tanpa galat, daftar terisi, form tersimpan, filter dan paginasi berfungsi. Sepuluh slug: `allowance-type`, `deduction-type`, `hazard-allowance-policy`, `on-call-allowance-policy`, `payroll-component`, `payroll-component-category`, `payroll-period`, `salary-grade`, `salary-structure`, `shift-allowance-policy`. Periksa juga satu layar HR yang memakai entitas yang dipindah (formulir aturan kelayakan benefit) dan buka `/hr/master-data/salary-grade` untuk memastikan redirect ke `/payroll/master-data/salary-grade` masih bekerja.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 90 berkas dari 10 entitas berada di `payroll/master-data` pada lima lapisan | Terpenuhi | §3.3; tidak ada folder lama tersisa |
| Isi berkas tidak berubah selain jalur impor | Terpenuhi | Perbandingan dengan `HEAD`, 90 dari 90 |
| Tidak ada impor baru yang tak terselesaikan | Terpenuhi | Resolver: himpunan identik |
| Hasil test dan ESLint tidak memburuk | Terpenuhi | §6 |
| Rute, menu, endpoint, CSS tidak disentuh | Terpenuhi | §3.4 |
| Tidak ada stage atau commit | Terpenuhi | `git diff --cached` kosong |
| Terbukti pada build dan layar | **Belum** | Menunggu pemilik menjalankan `npm run build` dan membuka 10 layar (§6) |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bukti statis tidak menangkap impor yang disusun dinamis dengan cara yang tidak lazim. Dua test bertemplat `${entity}` sempat terlewat dan ditemukan lewat suite unit; pencarian pola dinamis lain di `src` dan `tests` tidak menemukan yang tersisa. Kompilasi Next.js belum dibuktikan |
| Masalah yang diketahui | Folder `src/style/hr/master-data/` tetap bernama HR walaupun sebagian CSS-nya dipakai layar Payroll; tidak dipindah (di luar cakupan) |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | FE: 45 `M`, 90 `D` (dipindah), lima direktori baru `??` (lokasi baru di view, hooks, constants, utils, dan `src/lib/state/slice/payroll/`). Tidak ada stage atau commit |
| Langkah berikutnya | Pemilik: `npm run build` dan buka 10 layar; setelah itu laporan ini naik ke ✅. Fase 2 backend (registry, pemindahan entity, penggantian nama tabel, migration) adalah rencana terpisah, belum dikerjakan |
