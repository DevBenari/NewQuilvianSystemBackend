# Laporan Perubahan Frontend — `FE-RWI-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-031` |
| Judul | Admisi yang keliru dapat dibatalkan |
| Slice | `F11 — Aksi yang hilang` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 5, `FE-RWI-031` |
| Trace | `FE-INP-17`; matriks peran bagian 3; `03-frontend-architecture.md` 3A.5; `RWI-DEC-010`, `RWI-RULE-004` |
| Skema tampilan | [`05-skema-tampilan.md`](../../../05-skema-tampilan.md) bagian 21, dipicu dari bagian 6 (`FE-INP-16`) dan bagian 9 (`FE-INP-04`) |
| Contract version | Tidak ada kontrak baru. `PATCH /episodes/{id}/cancel` sudah ada dan tidak berubah |
| Wewenang UI | Penempatan `DEV_DISCRETION`. Batasnya: konfirmasi wajib menyebut bahwa pemesanan dan penempatan ikut dilepas |
| Dependency | `FE-RWI-020` ✅ selesai |
| Klasifikasi | `LIGHT` — tiga berkas disunting, satu fungsi murni baru, tanpa route, hook, komponen, maupun endpoint baru |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; berkas laporan ini beserta roadmap dan `requirement-traceability.md` modul Rawat Inap |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `44dc07335` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `ce21f5b` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria terpenuhi. **Sebagian besar source-nya sudah ada sebelum task ini dijalankan** — dibawa commit `3e14079d6` — sehingga pekerjaan task ini adalah audit berbukti atas kelimanya ditambah menutup satu celah skema tampilan yang masih terbuka. Rinciannya pada bagian 1 dan 3 |

---

## 1. Keadaan yang ditemukan di awal

Register menandai task ini ⬜ belum dikerjakan, tetapi source-nya bercerita lain. Commit
`3e14079d6` — "add inpatient consent and admission cancellation functionalities" — sudah
membawa hampir seluruh kemampuan pembatalan:

| Yang sudah ada | Di mana |
| --- | --- |
| Aturan kewenangan menurut status episode | `resolveAdmissionCancellationAuthority` pada `inpatient-episode-utils.jsx` |
| Validasi alasan wajib | `validateAdmissionCancellationReason` pada berkas yang sama |
| Penyusun payload beserta pemotongan panjangnya | `buildAdmissionCancellationPayload` |
| Aksi pembatalan pada daftar kerja | `use-inpatient-episode-worklist.jsx` dan view-nya |
| Aksi pembatalan pada detail episode | `use-inpatient-episode-detail.jsx` dan view-nya |
| Dialog konfirmasi beralasan | `ConfirmModal` dengan `requireReason` pada kedua layar |

Selisih antara register dan source ini sudah dilaporkan lebih dulu pada
[FE-RWI-020](FE-RWI-020.md) bagian 9.13 dan tidak diubah diam-diam di sana.

Karena itu task ini **tidak** menulis ulang apa yang sudah ada. Yang dikerjakan dua hal:

1. **Mengaudit kelima acceptance criteria terhadap source dan terhadap kontrak backend**,
   khususnya risiko yang ditandai roadmap sendiri: "kewenangannya berbeda menurut status
   episode — sama mudahnya salah".
2. **Menutup satu celah skema tampilan yang masih terbuka**: bagian 21.2 meminta identitas
   tindakan menyebut tempat tidur yang dilepas, dan kedua dialog belum menyebutkannya.

---

## 2. Proses bisnis dari sisi pengguna

Penggunanya petugas admisi, kepala ruangan, dan supervisor. Layar dibuka ketika sebuah
admisi ternyata keliru — penjamin salah, DPJP salah, atau pasien batal dirawat.

### 2.1 Urutan yang dilakukan pengguna

1. Pengguna menemukan episodenya, dari **Daftar Kerja Episode** atau dari **Detail
   Episode**.
2. Tombol **Batalkan Admisi** hanya muncul bila pengguna memang berwenang atas status
   episode itu. Bila tidak berwenang, tombolnya tidak ada — bukan ada tetapi ditolak
   sesudah ditekan.
3. Dialog konfirmasi terbuka. Isinya menyebut pasien, nomor episode, statusnya, **tempat
   tidur yang akan dilepas bila terbaca**, serta kalimat bahwa pemesanan dan penempatan
   ikut dilepas.
4. Alasan pembatalan wajib diisi. Alasan yang hanya berisi spasi atau tanda baca ditolak
   sebelum permintaan dikirim.
5. Setelah berhasil, episode terbaca **Batal**, tempat tidurnya kembali tersedia, dan
   sebuah toast memberitahukannya.

### 2.2 Siapa boleh membatalkan apa

| Status episode | Yang berwenang | Yang tidak |
| --- | --- | --- |
| **Sedang disiapkan** (`Draft`) | Petugas admisi, supervisor | Peran lain |
| **Sedang dirawat** (`Admitted`) | Kepala ruangan, supervisor | **Petugas admisi** |
| Rencana pulang, Selesai, Batal | Tidak ada | Semua |

Perbedaan pada baris kedua itulah yang ditandai roadmap sebagai risiko. Petugas admisi
boleh membatalkan admisi yang belum berjalan, tetapi tidak boleh membatalkan pasien yang
sudah menempati tempat tidur — keputusan itu milik kepala ruangan atau supervisor.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Alasan kosong atau hanya tanda baca | Ditolak di layar sebelum permintaan dikirim; dialog tetap terbuka dan alasan yang sudah diketik tidak hilang |
| Server menolak `403` | Pesan server tampil apa adanya di dalam dialog |
| Server menolak `422` | Sama — kalimat penolakan server tampil apa adanya |
| Server menjawab `409` | Keadaan admisi sudah berubah. Dialog ditutup, daftar/detail dimuat ulang, dan sebuah toast peringatan menjelaskan apa yang terjadi |
| Admisi sudah gugur sendiri | Termasuk `409` di atas. Server menjawab "Admisi ini sudah gugur sendiri karena ditinggalkan melewati batas waktu" dan kalimat itu yang tampil |
| Tempat tidur tidak terbaca | Baris "tempat tidur yang dilepas" **hilang**, bukan diisi tebakan. Kalimat umum tentang pelepasan tetap ada |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`;
`rules/frontend/base-component-decision-gate.md`;
`rules/frontend/ui-consistency-checklist.md`; `rules/frontend/REPORT_TEMPLATE.md`;
roadmap `FE-RWI-031`; `05-skema-tampilan.md` bagian 6, 9, dan 21;
`inpatient-episode-worklist-view.jsx`; `use-inpatient-episode-worklist.jsx`;
`inpatient-episode-detail-view.jsx`; `use-inpatient-episode-detail.jsx`;
`inpatient-episode-utils.jsx`; `inpatient-episode-constants.jsx`; serta source backend
read-only `InpatientEpisodeController.cs`, `InpEpisodeService.cs`, dan
`InpatientEpisodeDtos.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | Menambah `describeCancellationRelease` — menyebutkan tempat tidur yang benar-benar akan dilepas, atau teks kosong ketika tidak ada yang terbaca |
| `src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx` | Dialog pembatalan menyebut penempatan atau pemesanan yang dilepas. Pemesanan dibaca dari indeks papan yang sudah dipakai kolom Lokasi, sehingga tidak ada permintaan tambahan |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Dialog pembatalan menyebut penempatan yang dilepas, dibaca dari lokasi terkini yang sudah ditampilkan layar |

Tidak ada berkas lain yang disentuh. Aturan kewenangan, validasi alasan, penyusun payload,
kedua hook, dan kedua tombol **tidak diubah** karena pemeriksaan pada bagian 7 membuktikan
keduanya sudah benar.

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya tetap `view → hook → service → InstanceAxios`. Fungsi baru berada di
`utils` sebagai fungsi murni tanpa React hook, tanpa permintaan API, dan tanpa membaca
Redux. Tidak ada route, package, base component, Redux slice, hook, maupun abstraksi baru.

### 3.4 Gerbang keputusan base component

`UI GATE: PASS` — satu elemen, `REUSE`, tidak ada `NEW` maupun `EXTEND`.

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Baris keterangan tempat tidur yang dilepas di dalam dialog | `ConfirmModal` beserta `children`-nya | `base-features/confirm-modal.jsx`; kedua dialog sudah memakai `children` untuk identitas pasien dan kalimat dampak | `REUSE` |

Tidak ada komponen baru, dan tidak ada prop baru pada `ConfirmModal`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol konfirmasi memakai `loading` milik `ConfirmModal`; dialog tidak dapat ditutup selama permintaan berjalan |
| Kosong | Tidak berlaku — dialog selalu menunjuk satu episode tertentu. Baris tempat tidur hilang sendiri ketika tidak ada yang terbaca |
| Gagal | Pesan server tampil apa adanya di dalam dialog lewat `InformationAlert`; alasan yang sudah diketik tidak hilang. `409` menutup dialog, memuat ulang, dan menampilkan toast peringatan |
| Tanpa hak akses | Tombol **Batalkan Admisi** tidak dirender sama sekali. Penolakan `403` dari server tetap ditampilkan apa adanya bila sampai terjadi |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/inpatient-management/episodes/{id}/cancel` | Membatalkan admisi beserta pemesanan dan penempatannya dalam satu tindakan | `InpatientEpisode : Update` |

Endpoint ini sudah ada sebelum task ini dan tidak berubah. Task ini tidak menambah
permintaan baru sama sekali: keterangan tempat tidur disusun dari data yang **sudah**
dibaca kedua layar.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa keluaran | `PASS` | Keluaran perintah |
| `npm run lint` | `571 problems (0 errors, 571 warnings)` — **sama persis** dengan garis dasar, dan **nol** warning pada ketiga berkas task ini | `PASS` | Keluaran perintah |
| `npm run build` | `✓ Compiled successfully in 28.1s` | `PASS` | Keluaran perintah |
| Enam grep anti-regresi UI | Nol nilai visual literal, nol `!important`, nol `<table>` mentah, nol tombol non-base dan nol utility typography **yang ditambahkan task ini** | `PASS` | Keluaran grep, lihat di bawah |
| Test `.mjs` | Tidak ditulis dan tidak dijalankan | `NOT REQUIRED` | Pemilik pekerjaan menyatakan pengujian `.mjs` tidak diperlukan |
| Uji manual di peramban | Tidak dijalankan | `NOT REQUIRED` | Pemilik pekerjaan menyatakan cukup bukti source |

Uji manual: `NOT REQUIRED`.

`AUTOMATED TEST: SKIPPED (opsional) — pemilik pekerjaan menyatakan pengujian .mjs tidak diperlukan.`

**Catatan grep yang jujur.** `inpatient-episode-detail-view.jsx` menghasilkan 5 hit pada
grep tombol non-base dan 11 hit pada grep utility typography. Keduanya **bukan** dari task
ini: menjalankan grep yang sama terhadap versi berkas di `HEAD` menghasilkan angka yang
persis sama, 5 dan 11. Diklasifikasikan `UNRELATED EXISTING ISSUE` dan sengaja tidak
diperbaiki — `AGENTS.md` melarang memperbaiki masalah existing yang tidak terkait sebagai
efek samping. Berkas `inpatient-episode-worklist-view.jsx` menghasilkan 1 hit grep
typography, dan hit itu adalah komentar lama yang justru melarang `fw-semibold` dipakai.

**Tidak dijalankan:** `npm run test`, `npm run test:unit`, `npm run test:e2e`, dan
`npm run test:uat`. Berkas `tests/e2e/inpatient-admission-cancellation.spec.mjs` sudah ada
di repository sejak commit `3e14079d6`; task ini tidak menyentuhnya dan tidak
menjalankannya.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pembatalan `Draft` tersedia bagi petugas admisi dan supervisor | **Terpenuhi** | `resolveAdmissionCancellationAuthority` mengembalikan `canCancel` untuk `episodeStatus === DRAFT` ketika `actor.isAdmissionOfficer` atau `actor.isSupervisor`; peran lain menerima `canCancel: false` beserta `INPATIENT_GUARD_MESSAGES.DRAFT_CANCELLATION_ROLE`. Berlaku pada kedua layar karena keduanya memanggil fungsi yang sama |
| 2. Pembatalan `Admitted` tersedia bagi kepala ruangan dan supervisor, **tidak** bagi petugas admisi | **Terpenuhi** | Cabang `ADMITTED` memakai `actor.isWardHead \|\| actor.isSupervisor`, dan `isAdmissionOfficer` **tidak** ikut diperiksa di sana. Kecocokannya dengan server dibuktikan pada bagian 7.1 |
| 3. Pembatalan wajib beralasan | **Terpenuhi** | `validateAdmissionCancellationReason` memakai `hasMeaningfulReason` yang menuntut minimal satu huruf atau angka, sehingga spasi dan tanda baca saja ditolak. Kedua `ConfirmModal` memakai `requireReason`, dan `buildAdmissionCancellationPayload` memotong alasan pada 500 karakter — sama dengan `[MaxLength(500)]` pada `CancelAdmissionRequest` |
| 4. Konfirmasi menyebut bahwa tempat tidur akan dilepas | **Terpenuhi** | Kedua dialog memuat kalimat "Pemesanan dan penempatan yang masih ada ikut dilepas. Tempat tidur akan kembali tersedia untuk pasien berikutnya." **Ditambah task ini:** baris yang menyebut tempat tidurnya secara spesifik lewat `describeCancellationRelease`, sesuai skema tampilan 21.2 |
| 5. Setelah dibatalkan, episode terbaca `Cancelled` dan tempat tidurnya terbaca bebas pada papan | **Terpenuhi** | Detail episode memanggil `applyEpisode(data)` dengan jawaban server lalu menaikkan `bedRefreshToken` dan `refresh()`; daftar kerja memanggil `refresh()` yang sekaligus membaca ulang papan tempat tidur — pembacaan papan itu milik `FE-RWI-020` dan ikut memperbarui penanda pemesanan. Backend melepas reservation dan placement dalam satu transaksi `CancelEpisodeInternalAsync` |

### 7.1 Pemeriksaan kewenangan terhadap kontrak backend

Roadmap menandai kewenangan bertingkat ini sebagai risiko utama task, jadi kecocokannya
diperiksa baris per baris terhadap `InpEpisodeService.CancelAdmissionAsync`.

| Status | Aturan backend | Aturan layar | Cocok? |
| --- | --- | --- | --- |
| `Draft` | Tidak ada pemeriksaan peran; cukup permission `InpatientEpisode : Update` | Petugas admisi atau supervisor | **Layar lebih ketat.** Aman — layar tidak pernah menawarkan tindakan yang akan ditolak server, dan batas yang lebih ketat itu memang yang diminta acceptance criteria 1 |
| `Admitted` | `actorIsSupervisorOrWardHead`, jika tidak → `403` | `isWardHead \|\| isSupervisor` | **Setara.** `WARD_HEAD_ROLES` = `["KepalaRuangan", "Kepala Ruangan"]` dan `SUPERVISOR_ROLES` = `["SuperAdmin", "Supervisor"]`; gabungannya sama persis dengan `SUPERVISOR_OR_WARD_HEAD_ROLES` yang dipakai backend |
| `DischargePending` | Ditolak sebagai aturan bisnis | Tombol tidak dirender | **Cocok** |
| `Closed` | Ditolak `409` | Tombol tidak dirender | **Cocok** |
| `Cancelled` | Ditolak `409` | Tombol tidak dirender | **Cocok** |

Satu akibat yang layak dicatat, bukan sebagai cacat melainkan sebagai keadaan yang memang
dipilih: **kepala ruangan tidak dapat membatalkan episode `Draft` dari layar**, walaupun
server akan menerimanya. Itu mengikuti acceptance criteria 1 apa adanya. Bila suatu saat
kepala ruangan dinilai perlu diberi wewenang itu, yang diubah adalah keputusan pada
roadmap lebih dulu, bukan diam-diam di layar.

### 7.2 Definition of Done

DoD roadmap: "Kelima kriteria lulus; e2e ada dan lulus."

- Kelima kriteria lulus — **terpenuhi**.
- E2E ada dan lulus — berkas `tests/e2e/inpatient-admission-cancellation.spec.mjs` **ada**
  sejak commit `3e14079d6`, tetapi **tidak dijalankan**. Butir itu **dikecualikan atas
  keputusan pengguna 1 September 2026**, sejalan dengan bagian "Keputusan penutupan
  verifikasi" pada roadmap.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari task ini. Jumlah warning lint tetap 571, sama persis dengan garis dasar |
| Masalah yang diketahui | Kepala ruangan tidak dapat membatalkan `Draft` dari layar walaupun server menerimanya — mengikuti acceptance criteria 1; lihat bagian 7.1. Pada detail episode, episode `Draft` tidak menampilkan baris tempat tidur yang dilepas karena detail episode tidak memuat kolom pemesanan; itu dibiarkan kosong, bukan dikarang |
| Dependency backend | `NONE`. Seluruh kontrak yang dibutuhkan sudah ada dan tidak berubah |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu arahan pengguna diterima sebelum pekerjaan dimulai: kerjakan tanpa pengujian `.mjs`. Diterapkan sepenuhnya — tidak ada berkas test yang ditulis maupun dijalankan |
| Status Git frontend | Tiga berkas berubah pada branch `HamzahV2` di atas commit `44dc07335`; tidak ada `git add`, commit, push, pull, merge, rebase, maupun deploy |
| Status Git backend | Hanya berkas laporan ini, `frontend-roadmap.md`, dan `requirement-traceability.md` modul Rawat Inap yang disentuh. Tidak ada source backend yang diubah |
| Langkah berikutnya | `FE-RWI-033` dapat berjalan; seluruh dependensinya pada `FE-RWI-020` s.d. `FE-RWI-032` kini tertutup |

### 8.1 Catatan untuk pembaca register

Status ⬜ pada register sebelum task ini **tidak akurat** terhadap source: kemampuan
pembatalan sudah ada sejak commit `3e14079d6`. Yang belum ada hanyalah satu baris keterangan
pada dialog dan — yang lebih penting — **bukti tertulis bahwa kelima kriteria memang
terpenuhi**. Laporan ini menyediakan bukti itu, sehingga status task dapat dinaikkan
berdasarkan pemeriksaan, bukan berdasarkan asumsi bahwa kode yang ada sudah benar.
