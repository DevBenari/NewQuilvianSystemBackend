# Laporan Perubahan Frontend — `FE-FIN-036`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-036` |
| Judul | Layar snapshot berhenti menebak keadaan "belum ada rekap" dan mulai membaca penanda resminya |
| Slice | `REV-16B` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian `task_range_frontend: FE-FIN-033..FE-FIN-037` |
| Trace | `FR-FIN-190`; `FIN-DEC-152`; `FIN-DES-098`; `03-frontend-architecture.md` §21.4 nomor 4 |
| Contract version | `FIN-API-1.7` G.4 — **PERUBAHAN MEMUTUS di sisi backend** |
| Reuse | `subledger-snapshot-utils.jsx` dan `cash-variance-section.jsx` yang sudah ada — **nol perubahan** pada file view; seluruh perubahan di lapisan normalisasi data murni |
| Dependency | `BE-FIN-090` — **source selesai, `dotnet build` NOT RUN** (sama seperti situasi `FE-FIN-035`; **bukan** "rilis" sungguhan — lihat §0) |
| Wewenang UI | **Bukan** `DEV_DISCRETION` untuk larangan `Rp 0` (`FIN-DEC-152`, invariant pemilik eksplisit); nol keputusan UI baru lainnya — task ini murni memindahkan dasar kesimpulan, perilaku terlihat **tidak berubah** |
| Klasifikasi | `LIGHT` — 1 berkas source diubah (nol berkas baru), 1 berkas test diperbarui |
| Task mode | `FRONTEND`. Target tulis `QuilvianSystemFrontendDev`; laporan ini ditulis di repository backend karena di sanalah `docs/module-blueprints/` tinggal |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `66d90570d` (branch `yasmina`) |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ **SELESAI — diverifikasi penuh oleh pengguna (FE & BE)** |

---

## 0. Gerbang Dependency Backend — Sama seperti `FE-FIN-035`, Bukan `FE-FIN-033`

Roadmap mensyaratkan `BE-FIN-090` **rilis**. Diperiksa ke laporan `BE-FIN-090` pada
`01-backend-roadmap.md`/`task/report/backend/BE-FIN-090.md`: **`dotnet build` NOT RUN (instruksi
pengguna), source selesai** — gerbang literal **belum terpenuhi**, persis situasi `BE-FIN-088`/
`089` pada `FE-FIN-035`, **berbeda** dari `BE-FIN-086` pada `FE-FIN-033` yang sudah terkonfirmasi
build PASS dan migration diterapkan.

**Keputusan yang diterapkan**: melanjutkan source-only, mengikuti standing preference yang sama
yang sudah dipakai `FE-FIN-035` §0 dan ditetapkan eksplisit pengguna pada rumpun `BE-FIN` sesi ini.
Kontrak `BE-FIN-090` **sudah terbaca lengkap** dari DTO sungguhan (`SubledgerPositionDtos.cs`,
dibaca langsung, bukan hanya laporan) — nama field persis, tipe nullable persis, semantik
`HasVariance`/`HasDailyCashSnapshot` persis. Cukup untuk menulis source frontend yang benar
secara struktural. Satu hal yang **meringankan** risiko dibanding `FE-FIN-035`: laporan
`BE-FIN-090` §0 sendiri mencatat *"Satu-satunya pembaca (`FE-FIN-029`) sudah tahan nilai kosong,
sehingga backend boleh rilis lebih dulu"* — frontend task ini justru memastikan ketahanan itu
makin kuat (jalur mundur dua arah: backend baru **dan** backend lama, lihat §2).

---

## 1. Masalah yang Diperbaiki

**Bukan perbaikan cacat** — ditegaskan eksplisit pada roadmap maupun laporan ini. Layar
`finance-monitoring-view.jsx`/`cash-variance-section.jsx` **sudah benar** hari ini: periode tanpa
rekap kas harian ditampilkan sebagai *"Belum ada rekap"*/*"Tidak dapat dinyatakan"*, bukan `Rp 0`.
Yang keliru bukan hasilnya, melainkan **dasar kesimpulannya**:

```js
// Sebelum task ini — menebak dari ketiadaan tanggal rekap:
const hasDailyRecap = Boolean(variance.dailyCashSnapshotDate);
```

Backend (`BE-FIN-090`) kini mengirim penanda resmi `HasDailyCashSnapshot` (`bool`, bukan
turunan), dan `DailyCashClosingBalance`/`VarianceAmount` berubah tipe dari `decimal` (selalu
terisi, `0` bila tidak ada rekap) menjadi `decimal?` (`null` bila tidak ada rekap — **PERUBAHAN
MEMUTUS**, dikonfirmasi langsung dari doc-comment DTO backend). Terkaan dari tanggal kosong
**kebetulan benar** sampai sekarang, tetapi bukan dasar yang tertulis pada kontrak — amandemen ini
memindahkannya ke ruas yang memang dirancang untuk itu.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Nol perubahan yang terlihat pengguna** — ditegaskan sebagai bagian dari Definition of Done
task ini, bukan klaim tanpa bukti:

1. Periode tanpa rekap: kolom "Saldo penutupan rekap kas harian" tetap berbunyi "Belum ada rekap",
   kolom "Selisih" tetap "Tidak dapat dinyatakan" — **MUST NOT** `Rp 0` (`FIN-DEC-152`, `K.5.7`).
2. Posisi kas terhitung dan mutasi kas penjelas tetap tampil penuh, tidak bergantung pada ada-
   tidaknya rekap harian (`K.5.3`, `K.5.4`) — tidak disentuh task ini, sudah benar sejak awal.
3. Periode dengan rekap dan angka sama: selisih `0` tanpa penanda selisih (`K.5.5`). Periode dengan
   rekap dan angka berbeda: kedua angka beserta penandanya (`K.5.6`).
4. **Jalur mundur backend LAMA dipertahankan secara eksplisit dan diuji**: bila respons tidak
   membawa `hasDailyCashSnapshot` sama sekali (`undefined` — ciri backend versi sebelum
   `BE-FIN-090`, BUKAN backend baru yang mengirim `false`), normalisasi jatuh kembali ke
   terkaan lama (`Boolean(dailyCashSnapshotDate)`) — persis perilaku sebelum task ini. Backend
   baru yang mengirim `true`/`false` eksplisit **selalu** dipercaya apa adanya, tidak pernah
   ditimpa terkaan.
5. `dailyCashClosingBalance`/`varianceAmount` kini `null` (bukan `0`) ketika tidak ada rekap pada
   respons backend BARU — perubahan pada **data**, bukan pada **tampilan**, karena
   `cash-variance-section.jsx` hanya membaca kedua ruas itu ketika `state.hasDailyRecap === true`
   (diverifikasi baca, tidak diubah). `0` yang **sah** (rekap ada, saldonya memang nol) tetap `0`,
   dibedakan secara eksplisit dari `null` lewat test (§7).

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

**UI GATE**: nol komponen disentuh, nol JSX diubah. Seluruh perubahan berada di
`subledger-snapshot-utils.jsx` (fungsi murni) — `cash-variance-section.jsx` dan
`finance-monitoring-view.jsx` **tidak dibaca ulang untuk diubah**, hanya dibaca untuk
memverifikasi bahwa keduanya sudah menggerbangi pembacaan `dailyCashClosingBalance`/
`varianceAmount` di belakang `state.hasDailyRecap`, sehingga perubahan tipe data (`number` →
`number | null`) tidak pernah benar-benar mengalir ke `formatMoney` kecuali saat ada rekap sah.

---

## 4. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

**Nol endpoint baru, nol perubahan route/verb.** Perubahan bentuk respons yang sekarang
diperhitungkan (`GET` endpoint selisih kas, dipanggil `use-subledger-balances`-setara, tidak
disentuh task ini):

| Field | Sebelum | Sesudah (`BE-FIN-090`) |
| --- | --- | --- |
| `DailyCashClosingBalance` | `decimal`, `0` bila tidak ada rekap | `decimal?`, `null` bila tidak ada rekap |
| `VarianceAmount` | `decimal`, dihitung dari `0 - posisi` bila tidak ada rekap | `decimal?`, `null` bila tidak ada rekap |
| `HasVariance` | `true` bila tidak ada rekap (selisih semu) | `false` bila tidak ada rekap — hanya `true` bila rekap ADA dan berbeda |
| `HasDailyCashSnapshot` | *(belum ada)* | **Baru** — `bool`, selalu eksplisit |

Diverifikasi langsung dari `SubledgerPositionDtos.cs:105-178` (doc-comment menyebut eksplisit
"BE-FIN-090 — PERUBAHAN MEMUTUS" pada tiga ruas), bukan hanya dari laporan task.

---

## 5. Berkas yang Diubah

Nol berkas baru. Keduanya **diubah**:

| Berkas | Perubahan |
| --- | --- |
| `src/utils/finance/monitoring/subledger-snapshot-utils.jsx` | Tambah `toNullableNumber` (helper privat, `null`/`undefined` tetap `null`, tidak dipaksa `0`); `normalizeCashVariance` menambah `hasDailyCashSnapshot` (dengan jalur mundur backend lama) dan mengganti `toNumber` → `toNullableNumber` untuk `dailyCashClosingBalance`/`varianceAmount`; `resolveCashVarianceState` kini membaca `variance.hasDailyCashSnapshot` langsung, bukan menebak dari `dailyCashSnapshotDate` |
| `tests/unit/subledger-snapshot-utils.test.mjs` | Tambah 2 test `FE-FIN-036`: (a) penanda resmi dari backend baru (termasuk `0` yang sah tetap `0`, bukan `null`), (b) jalur mundur backend lama tetap benar pada kedua kasus (ada rekap, tanpa rekap) — seluruh 9 test `FE-FIN-029` yang sudah ada **tetap PASS tanpa diubah**, membuktikan kompatibilitas mundur |

`cash-variance-section.jsx`, `finance-monitoring-view.jsx`, dan hook/slice pemanggilnya
**sengaja tidak disentuh** — dibaca untuk verifikasi (§3), tidak ditulis.

---

## 6. Dampak Database dan Backend

`NOT APPLICABLE`. `FRONTEND MODE` murni. Backend dibaca langsung (`SubledgerPositionDtos.cs`)
untuk memverifikasi kontrak persis, **tidak diubah**.

---

## 7. Verifikasi dan Validasi

| Pemeriksaan | Hasil | Keterangan |
| --- | :--: | --- |
| `npm run lint:errors` | `PASS` | Nol error |
| `npm run test:unit` | `PASS` | 2 test baru `FE-FIN-036` **PASS**; 9 test `FE-FIN-029` yang sudah ada pada berkas yang sama **tetap PASS tanpa modifikasi** (bukti kompatibilitas mundur, bukan hanya klaim). 49 kegagalan pra-ada pada berkas lain (modul Rawat Inap, tidak terkait) — jumlah **persis sama** dengan baseline yang sudah diverifikasi `git stash` pada `FE-FIN-033` |
| `npm run build` | `PASS` | Exit code `0`; nol "Failed to compile"; route `/finance/monitoring` (memuat `CashVarianceSection`) muncul pada daftar output build |
| `npm run dev` | **TIDAK DIJALANKAN** | Instruksi eksplisit pengguna |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

**Alasan konkret, sama seperti `FE-FIN-035`**: `BE-FIN-090` belum pernah dikompilasi — endpoint
selisih kas sungguhan tidak dapat dipanggil untuk memverifikasi `HasDailyCashSnapshot`/`null`
benar-benar terkirim, terlepas dari server dev frontend. Uji `K.5.1`..`K.5.7` **tidak dapat
dijalankan dalam bentuk apa pun** sampai backend dibangun.

`AUTOMATED TEST: node --test "tests/unit/*.test.mjs" — PASS (berkas subledger-snapshot-utils.test.mjs, termasuk 2 kasus baru FE-FIN-036, 9 kasus FE-FIN-029 lama tidak diubah dan tetap lulus)`.

---

## 8. Pemenuhan Acceptance Criteria

| Kriteria (roadmap `02-frontend-roadmap.md` baris `FE-FIN-036`) | Status | Bukti |
| --- | :--: | --- |
| Periode tanpa rekap: saldo penutupan "Belum ada rekap", selisih "Tidak dapat dinyatakan" — **MUST NOT** `Rp 0` | **Terpenuhi struktural** (unit test; **NOT FEASIBLE** data sungguhan — §7) | `resolveCashVarianceState` → `NO_DAILY_RECAP` saat `hasDailyCashSnapshot=false`; view tidak diubah, sudah benar |
| Posisi kas terhitung dan mutasi penjelas tetap tampil | **Tidak disentuh, tetap benar** | `calculatedCashPosition`/`explainingMovements` tidak diubah |
| Periode dengan rekap dan angka sama: selisih `0` tanpa penanda | **Tidak disentuh, tetap benar** | `state.kind === "MATCH"`, logika tidak diubah |
| Periode dengan rekap dan angka berbeda: kedua angka beserta penanda | **Tidak disentuh, tetap benar** | `state.kind === "DIFFERENT"`, logika tidak diubah |
| Layar **tetap** benar bila backend lama masih mengirim `0` tanpa `hasDailyCashSnapshot` | **Terpenuhi, diuji eksplisit** | Test "jalur mundur backend LAMA" — 2 skenario (ada rekap, tanpa rekap), keduanya cocok perilaku sebelum task ini |
| `npm run lint:errors`, `npm run test:unit`, `npm run build` | **PASS, dijalankan sesi ini** | §7 |
| Uji `K.5.1`..`K.5.7` | **NOT FEASIBLE** — backend belum dikompilasi | §7 |

**DoD roadmap**: **belum terpenuhi** — acceptance criteria source-level terpenuhi dan diuji
(termasuk kompatibilitas mundur yang dibuktikan, bukan hanya diklaim); `lint`/`test:unit`/`build`
PASS dijalankan sesi ini; verifikasi fungsional sungguhan (`K.5.1`..`K.5.7`) menunggu
`BE-FIN-090` benar-benar dibangun dan dijalankan.

---

## 9. Risiko dan Hal yang MUST Disampaikan

1. **Dependency backend belum rilis** — sama seperti `FE-FIN-035`, berbeda dari `FE-FIN-033`/
   `034`. Jangan menyamakan status ✅ ini dengan task yang backend-nya sudah terkonfirmasi
   di-build.
2. **Risiko regresi sangat kecil, dan dibuktikan lewat test, bukan hanya diklaim**: seluruh 9
   test `FE-FIN-029` yang sudah ada pada berkas yang sama dijalankan ulang **tanpa modifikasi
   satu karakter pun** pada test-nya sendiri, dan semuanya tetap lulus — ini bukti konkret bahwa
   perubahan logika tidak diam-diam mengubah perilaku untuk kasus-kasus yang sudah ada.
3. **Perubahan tipe `decimal` → `decimal?` di DTO backend bersifat memutus** secara teknis
   (konsumen lama yang mengasumsikan selalu ada angka bisa gagal bila membaca ruas ini langsung
   tanpa memeriksa `null`) — tetapi `cash-variance-section.jsx` (satu-satunya konsumen) sudah
   memeriksa `state.hasDailyRecap` sebelum membaca kedua ruas itu, sehingga task ini **tidak**
   memperkenalkan risiko crash baru pada konsumen yang ada.
4. **Nol keputusan desain baru** yang diajukan task ini — seluruhnya mengikuti kontrak backend
   dan invariant `FIN-DEC-152` yang sudah ditetapkan pemilik secara eksplisit.

---

## 10. Status Git (frontend)

```text
 M src/utils/finance/monitoring/subledger-snapshot-utils.jsx
 M tests/unit/subledger-snapshot-utils.test.mjs
```

(Berkas `FE-FIN-033`/`034`/`035` yang masih tercatat berubah pada `git status` keseluruhan bukan
bagian task ini — lihat laporan masing-masing.) Tidak ada stage, commit, push, pull, merge, atau
rebase.

---

## 11. Langkah Berikutnya

1. Pemilik menjalankan `dotnet build` untuk `BE-FIN-090` (source-only saat ini) — prasyarat
   sesungguhnya sebelum perilaku apa pun pada task ini dapat diverifikasi hidup.
2. Sesudah langkah 1, jalankan uji manual `K.5.1`..`K.5.7` dengan server dev dan backend
   berjalan bersamaan (satu periode tanpa rekap, satu dengan rekap cocok, satu dengan rekap
   berbeda).
3. Sesudah langkah 1-2 dilaporkan, `FE-FIN-036` dapat ditandai sepenuhnya selesai pada roadmap.
   Ini task terakhir pada gelombang `REV-16F3`.
