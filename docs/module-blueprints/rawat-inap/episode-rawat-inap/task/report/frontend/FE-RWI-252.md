# Laporan Perubahan Frontend — `FE-RWI-252`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-252` |
| Judul | Panel High/Low Dependency |
| Slice | Integrasi Denah ↔ Bed Management — `FE-DNH-05` |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-013`, `014`; `RWI-DEC-302`, `303`, `325`; `RWI-AC-440`, `441`, `443` |
| Contract version | `0.13.0` approved |
| Dependency | `BE-RWI-234` ✅ |
| Klasifikasi | `MEDIUM` — panel reusable, optimistic concurrency, riwayat, tiga entry point, dan pencatatan nilai awal sesudah Draft terbentuk |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Commit frontend saat mulai rangkaian | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`) |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Outcome

Satu panel High/Low Dependency sekarang dipakai ulang pada ringkasan cepat Workspace Keperawatan,
detail episode, dan langkah Dokter admisi. Panel menampilkan nilai aktif beserta sumber, pencatat,
waktu, catatan, dan riwayat; pengguna berhak dapat memilih High/Low serta menyimpan catatan sampai
500 karakter.

Pembaruan mengirim `ExpectedSetAt` dari nilai terakhir sehingga tab basi menerima conflict 409 dan
harus memuat ulang. Pesan 403/409/422 dari server dipertahankan. Episode Closed atau Cancelled hanya
dapat dibaca. Nilai awal yang dipilih saat admisi tidak dikarang sebagai bagian create episode: setelah
episode Draft berhasil dibuat, frontend memanggil endpoint care-dependency tersendiri.

HD/LD tidak menjadi input kelayakan bed. Semua pemilihan bed tetap mengikuti `IsSelectable` dan
`Eligibility` dari endpoint floor plan server.

## 2. Perubahan source

| Area | Implementasi |
| --- | --- |
| Panel | `care-dependency-panel.jsx` merender nilai aktif, form High/Low, catatan, state error/stale/read-only, reload, dan riwayat |
| Hook | `use-care-dependency.js` mengunci permission, normalisasi current/history, GET history, PATCH nilai, `ExpectedSetAt`, dan pesan server |
| Normalizer | `inpatient-episode-utils.jsx` mempertahankan lima field care dependency tanpa mengubah null menjadi status episode |
| Admisi | Konstanta/form Dokter menyimpan pilihan awal opsional; hook mengirim PATCH hanya sesudah Draft berhasil dibuat dan mempertahankan keberhasilan episode bila PATCH gagal |
| Detail episode | Panel reusable ditempatkan pada detail episode dan meminta refresh sesudah perubahan |
| Keperawatan | Panel compact ditempatkan pada quick summary tanpa menggandakan logic mutasi |
| Style | CSS module baru khusus susunan panel, pilihan, current state, dan history |
| Test | `care-dependency-panel.test.mjs` mengunci normalisasi null, payload, urutan Low→High→Low, permission/error/read-only, tiga placement, dan independensi eligibility bed |

## 3. Kontrak, permission, dan concurrency

- Baca panel/riwayat memerlukan `InpatientEpisode : Read`; mutasi memerlukan
  `InpatientEpisode : SetCareDependency`.
- PATCH hanya memuat `level`, `note`, dan `expectedSetAt`; source, actor, dan timestamp selalu berasal
  dari backend.
- Level mengikuti kontrak `1 = High`, `2 = Low`; nilai null tetap “Belum dinilai” dan tidak
  dinormalisasi menjadi status episode `0`.
- Catatan opsional dibatasi 500 karakter di UI serta tetap tunduk pada validasi 422 server.
- `ExpectedSetAt` nullable untuk pencatatan pertama dan memakai timestamp current terakhir untuk
  perubahan berikutnya.
- Conflict 409 menandai panel stale, menampilkan pesan server, dan menyediakan “Muat Ulang” sebelum
  operator mencoba lagi.
- Closed/Cancelled hanya baca dari status canonical; race status tetap ditangani pesan 422 server.
- Riwayat tidak diubah atau digabung di frontend; urutan dan actor/source ditampilkan dari respons.

## 4. Keputusan reuse / create-new

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Service episode dan effective permissions | `REUSE_WITH_ADAPTER` | Endpoint ditambahkan melalui service episode existing; keputusan akses memakai hook permission yang sama dengan modul |
| Normalizer episode | `EXTEND` | Lima field care dependency ditambahkan ke model view canonical agar tiga consumer membaca bentuk yang sama |
| Alur Draft admisi | `EXTEND` | Lifecycle existing dipertahankan; PATCH dijalankan sesudah create Draft berhasil |
| Panel dan hook care dependency | `CREATE_NEW` | Belum ada komponen reusable untuk current value, optimistic concurrency, dan history |
| `BaseButton`, `InformationAlert`, `AccessDeniedGate`, spinner | `REUSE` | Komponen existing mencukupi aksi, error, akses, dan loading |
| Kelayakan bed | `REUSE_UNCHANGED` | Tidak ditambahkan filter HD/LD; selector tetap memakai hasil backend |

`UI GATE: WAIVED BY RWI-DEC-325`. Panel mengikuti gaya halaman existing dan tidak mengubah geometri
floor plan. Kontrak, permission, lint, build, test, dan pelaporan tetap mengikat.

## 5. State dan kegagalan

| State | Perilaku |
| --- | --- |
| Belum dinilai | Badge “Belum dinilai”; pengguna berhak dapat memilih High/Low |
| Draft admisi belum punya ID | Pilihan disimpan pada state form, lalu dikirim sesudah episode Draft dibuat |
| Loading/history | Loading terpisah; history dibuka atas permintaan dan dibaca ulang sesudah save |
| 403 | Pesan server ditampilkan; akses tidak disimulasikan oleh frontend |
| 409 | Panel ditandai stale dan meminta muat ulang data terbaru |
| 422 | Pesan validasi/status server ditampilkan tanpa menghilangkan input |
| Closed/Cancelled | Current value dan history tetap dapat dibaca; kontrol mutasi tidak tersedia |
| Gagal PATCH nilai awal | Episode Draft tetap berhasil; warning care dependency ditampilkan agar dapat dicatat ulang |

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Test fokus panel + floor plan monitoring + detail episode | 28/28 lulus | `PASS` |
| Test baru `care-dependency-panel.test.mjs` | 6/6 lulus | `PASS` |
| ESLint fokus file task | Exit `0`; hanya tiga warning existing `set-state-in-effect` pada detail episode ketika dijalankan tanpa `--quiet` | `PASS` |
| `npm run lint:errors` | Exit `0`, tidak ada error ESLint | `PASS` |
| `npm run build` | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 499/499 static pages, standalone siap | `PASS` |
| Seluruh unit test | 2.964 test; 2.934 lulus, 30 gagal existing di luar scope; enam test baru lulus dan jumlah gagal tetap baseline | `EXISTING OUT-OF-SCOPE FAILURES` |
| `git diff --check` scope frontend | Exit `0` | `PASS` |
| HTTP/live browser | Server, sesi, akun, dan fixture role/episode aktual tidak tersedia | `NOT RUN` |
| UAT-809, UAT-810 | Memerlukan akun perawat penanggung jawab/DPJP, dua tab, dan episode aktual | `NOT RUN` |

UAT browser tidak menahan status selesai berdasarkan D-FE. Verifikasi source mengunci acceptance
positif Low admisi → High perawat → Low DPJP sebagai tiga entry history, serta acceptance negatif
403, 409/reload, dan Closed read-only.

## 7. Acceptance criteria dan DoD

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Satu panel reusable di tiga tempat | ✅ | Import/render pada quick summary, detail episode, dan langkah Dokter admisi |
| Current value, source, actor, time, note | ✅ | Normalizer + panel current card |
| Pilihan High/Low dan catatan maksimal 500 | ✅ | Radio options, `maxLength`, payload builder |
| `ExpectedSetAt` dari data terbaru | ✅ | Hook memakai current `careDependencySetAt`; null hanya untuk nilai pertama |
| Riwayat tidak ditimpa | ✅ | GET history terpisah; fixture Low→High→Low tetap tiga baris |
| Pesan 403/409/422 server | ✅ | Error normalizer dan branch stale/reload |
| Episode Closed hanya baca | ✅ | Guard canonical status dan source test |
| Nilai awal dicatat pada episode Draft | ✅ | Deferred form + PATCH sesudah create episode |
| HD/LD tidak memengaruhi kelayakan bed | ✅ | Tidak ada level pada selector/request bed; test source mengunci independensi |
| Permission Read/SetCareDependency | ✅ | Effective permission gate pada hook/panel |
| Laporan, roadmap, graph, register, traceability | ✅ | Laporan ini dan pembaruan dokumen delivery |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, deployment, ataupun mutasi data nyata.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite global memiliki 30 kegagalan modul lain; tidak ada kegagalan pada test task |
| Runtime | UAT-809/810 aktual `NOT RUN` karena akun dan fixture role/episode tidak tersedia |
| Perubahan sampingan | `NONE`; perubahan worktree lain dipertahankan |
| Langkah berikutnya | `FE-RWI-253`: hapus sisa tampilan bed lama dan isian/pesan jenis kelamin |
