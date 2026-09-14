# Laporan Perubahan Frontend — `FE-ACC-P2-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-002` |
| Judul | Aksi penutupan: ajukan, setujui, tolak |
| Slice | `P2-4` |
| Roadmap | `docs/module-blueprints/accounting/roadmap/frontend-roadmap-phase2.md` — kartu `FE-ACC-P2-002` |
| Trace | `ACC-DEC-052`, `ACC-DEC-055`, `ACC-DEC-016`; `FR-P2-026`, `FR-P2-027`, `FR-P2-028` |
| Contract version | `ACC-API-0.9` — `approved`, Rizki 10 September 2026 |
| Wewenang UI | Tiga tombol pada layar `FE-ACC-P2-001` dan satu modal alasan penolakan. **Tidak ada layar sendiri** |
| Dependency | `BE-ACC-P2-006` — selesai; `FE-ACC-P2-001` — selesai, dikerjakan sebagai satu paket dengan task ini |
| Klasifikasi | `LIGHT` — tiga tombol, tiga thunk, satu modal, tanpa layar dan tanpa komponen base baru |
| Task mode | `CROSS-REPO` — source di frontend, laporan di backend |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `47cf3c6a0` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `3e2fb76` (branch `rizkiG`) |
| Tanggal | 10 September 2026 |
| Status | **🟡 Tinggal satu butir DoD: `npm run build`, dijalankan owner — penilaian ulang 11 September 2026.** Keempat acceptance terpetakan ke source; tampilan tombol mati bagi pengguna kedua (`UAT-P2-15`..`18`) dikecualikan dari DoD development atas keputusan owner dan diserahkan ke tim UAT, `READY FOR UAT`. Layarnya ikut disentuh `FE-ACC-P2-001` acceptance (5), jadi build ulang diperlukan. *Riwayat: Alur penuh **terbukti terhadap backend sungguhan**; satu cacat ditemukan dan diperbaiki. Acceptance (2) terbukti sebagian — lihat bagian 6 dan 7.* |

---

## 1. Keadaan yang ditemukan di awal

Task ini **tidak dapat dikerjakan lebih dulu**, dan itu bukan pilihan gaya. Cakupannya berbunyi
*"Tiga tombol pada layar `FE-ACC-P2-001`"* — tombolnya menumpang layar daftar periksa dan tidak
punya tempat lain untuk diletakkan. Acceptance (1)-nya pun berbunyi "tombol Ajukan mati selama
masih ada penghalang", dan daftar penghalang itu milik `FE-ACC-P2-001`.

Karena itu keduanya dikerjakan sebagai **satu paket** dalam satu sesi, `001` lebih dahulu. Kartu
roadmap `FE-ACC-P2-002` sekarang sudah mencantumkan `FE-ACC-P2-001` pada kolom Dependency-nya,
supaya urutan ini tidak salah dibaca lagi.

Ketiga endpoint-nya sudah berdiri di backend, terbukti `AccountingPeriodController` pada `rizkiG`
dan pemeriksaan runtime pada bagian 6.

---

## 2. Proses bisnis dari sisi pengguna

Penutupan periode ditempuh **dua orang yang berbeda**. Itu inti `ACC-DEC-016` — prinsip empat
mata.

1. **Manajer Akuntansi** membuka Daftar Periksa Penutupan, memastikan tidak ada penghalang aktif,
   lalu menekan **Ajukan Penutupan**. Kotak konfirmasi menyebut nama periodenya. Bila sebagian
   pemeriksaan belum berjalan, kalimat peringatannya ikut muncul di kotak yang sama — bukan hanya
   di layar belakangnya.
2. Periode berpindah ke status **Menunggu Persetujuan**, dan barisnya di layar Periode Akuntansi
   menampilkan lencana kuning bertuliskan itu.
3. **Pimpinan keuangan** membuka layar yang sama. Baginya yang menyala adalah **Setujui
   Penutupan** dan **Tolak**.
4. Menekan Tolak membuka modal yang **mewajibkan alasan tertulis**. Tombol kirimnya tetap mati
   selama alasannya kosong. Alasan itu masuk ke Riwayat Penutupan dan tidak pernah dihapus.
5. Sesudah aksi apa pun berhasil, layar **memuat ulang dari backend** — daftar periksa dan
   riwayatnya sekaligus. Layar tidak pernah menebak status berikutnya sendiri.

### Kapan tombol mati, dan kenapa keterangannya wajib ada

Tombol yang tidak boleh ditekan **dimatikan, bukan disembunyikan**. Menyembunyikannya membuat
petugas bertanya-tanya di mana tombolnya; mematikannya beserta keterangan menjelaskan sendiri apa
yang kurang.

| Keadaan | Yang mati | Keterangan yang ditampilkan |
| --- | --- | --- |
| Masih ada penghalang aktif | Ajukan | "Masih ada N penghalang yang harus diselesaikan lebih dahulu." |
| Periode tidak berstatus Terbuka | Ajukan | "Periode ini tidak berstatus Terbuka, jadi penutupannya tidak dapat diajukan." |
| Tidak punya hak `Close` | Ajukan | "Anda tidak memiliki hak Tutup Periode." |
| Tidak punya hak `Approve` | Setujui, Tolak | "Anda tidak memiliki hak Setujui Penutupan." |
| Pembuka layar adalah yang mengajukan | Setujui, Tolak | "Penutupan tidak dapat disetujui oleh yang mengajukan." |
| Belum ada pengajuan yang menunggu | Setujui, Tolak | "Belum ada pengajuan penutupan yang menunggu persetujuan." |

Pengaju dibaca dari **riwayat** — baris `Submitted` terakhir — lalu dibandingkan dengan pengguna
yang sedang masuk. Sengaja tidak ditebak dari peran, karena satu orang dapat memegang kedua
peran sekaligus. Backend tetap menegakkan aturan yang sama dengan `403`; layar hanya mendahului
supaya petugas tahu **sebelum** menekan, bukan sesudah ditolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `NewQuilvianSystemBackend/Areas/.../AccountingPeriodController.cs` | Path dan hak akses ketiga endpoint |
| `NewQuilvianSystemBackend/Areas/.../DTOs/PeriodClosingDtos.cs` | Bentuk `SubmitPeriodClosingRequest`, `ApprovePeriodClosingRequest`, `RejectPeriodClosingRequest` |
| `NewQuilvianSystemBackend/Areas/.../Enums/PeriodClosingAction.cs` | Nilai `Submitted`, `Approved`, `Rejected` |
| `src/components/features/base-features/confirm-modal.jsx` | Apakah modal beralasan sudah ada, dan bagaimana alasannya sampai ke pemanggil |
| `src/lib/hooks/auth/use-permission.jsx` | Cara membaca hak akses efektif |
| `src/lib/state/slice/auth/login-slice.jsx` | Bidang identitas pengguna yang sedang masuk |
| `src/components/view/corporate/accounting/accounting-period/accounting-period-view.jsx` | Pola dialog beralasan pada Buka Kembali Periode |

### 3.2 Berkas yang berubah

Task ini **tidak menambah berkas apa pun**. Seluruhnya menumpang berkas yang dibuat
`FE-ACC-P2-001`.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/corporate/accounting/accounting-period-closing-slice.jsx` | Tiga thunk aksi: `submitPeriodClosing`, `approvePeriodClosing`, `rejectPeriodClosing`, beserta jalur `actionLoading` dan `actionError` bersama |
| `src/lib/hooks/corporate/accounting/accounting-period/use-period-closing.jsx` | Pembukaan dialog per mode, pengiriman, toast hasil, muat ulang sesudah berhasil, dan penyusunan alasan tombol mati |
| `src/components/view/corporate/accounting/accounting-period/closing/period-closing-view.jsx` | Tiga `BaseButton`, satu `ConfirmModal` beralasan, pembacaan hak akses, dan penentuan pengaju dari riwayat |
| `src/lib/constants/corporate/accounting/accounting-period/period-closing-constants.jsx` | `resolveApproveDisabledReason` dan `resolveSubmitDisabledReason` — urutan pemeriksaan alasan tombol mati, diangkat dari view supaya dapat diuji |
| `tests/unit/accounting-period-closing-payload.test.mjs` | Empat uji regresi urutan alasan, memakai riwayat asli 2019-01 |

### 3.3 Kepatuhan arsitektur frontend

`UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Tiga tombol aksi | `BaseButton` | `base-features/base-button.jsx`, varian `primary`/`success`/`danger` | `REUSE` |
| Modal alasan penolakan | `ConfirmModal` | `requireReason`, `reasonLabel`, `reasonPlaceholder`, `maxReasonLength` sudah ada; alasannya sampai lewat `onConfirm(event, cleanReason)`; pola yang sama dipakai 5 modul administrator | `REUSE` |

`ConfirmModal` **tidak perlu di-extend sama sekali** — kewajiban alasan beserta tombol kirim yang
mati selama alasannya kosong sudah menjadi perilaku bawaannya. Membuat modal penolakan tersendiri
akan menduplikasi perilaku yang sudah teruji di lima modul lain.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Ketiga tombol mati selama daftar periksa masih dimuat |
| Mengirim | Tombol di dalam modal menampilkan "Memproses...", dan ketiga tombol layar ikut mati |
| Berhasil | Toast hijau: "Penutupan Diajukan", "Penutupan Disetujui", atau "Penutupan Ditolak", lalu layar memuat ulang sendiri |
| Gagal | Toast merah "Tindakan Ditolak" beserta pesan sebenarnya dari backend, misalnya `409` karena masih ada penghalang |
| Kosong | `NOT APPLICABLE` — tombol tidak punya keadaan kosong |
| Tanpa hak akses | Tombolnya mati beserta keterangan, sebagaimana tabel pada bagian 2 |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - Accounting - Accounting Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/accounting/periods/{id}/submit-closing` | Manajer Akuntansi mengajukan penutupan | `AccountingPeriod : Close` |
| `POST` | `/v1/corporate/accounting/periods/{id}/approve-closing` | Pimpinan keuangan menyetujui | `AccountingPeriod : Approve` |
| `POST` | `/v1/corporate/accounting/periods/{id}/reject-closing` | Pimpinan keuangan menolak; alasan wajib | `AccountingPeriod : Approve` |

Hak aksesnya `AccountingPeriod`, **bukan** `Period`. Nama `Period` yang sempat tertulis pada
kontrak akan menghasilkan `403` permanen yang tidak dapat diperbaiki dari layar Akses Role,
karena hak yang dicari tidak pernah ada untuk diberikan. Diperbaiki `ACC-API-0.9`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada ketiga berkas | Tanpa keluaran, exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` | 0 error | `PASS` | 673 warning seluruhnya pre-existing di modul lain |
| `npm run build` | `✓ Compiled successfully in 32.4s` | `PASS` | Keluaran build |
| `node --test tests/unit/` | 615 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Ketiga endpoint aksi berdiri | Route grup menjawab `HTTP 401` | `PASS` | `curl` ke `https://localhost:7184/api/v1/corporate/accounting/periods/...` — 401 berarti route ada dan meminta autentikasi |
| `accounting-periods` **tidak** ada | `HTTP 404` | `PASS` | `curl` ke path yang sama dengan `accounting-periods` |
| `UAT-P2-15`, `UAT-P2-16`, `UAT-P2-17`, `UAT-P2-18` | Tidak dijalankan | `NOT FEASIBLE` | Butuh dua sesi login berbeda — satu Manajer Akuntansi, satu pimpinan keuangan — beserta periode nyata berstatus Menunggu Persetujuan. Kredensial tidak tersedia di lingkungan ini |

### Alur penuh diuji terhadap backend sungguhan — 10 September 2026

Dijalankan pada tahun buku **2019** yang sengaja dibangkitkan sebagai data buangan, supaya buku
2026 tidak tersentuh. Sesi `superadmin`, basis data `QuilvianNewDevRizki`.

| # | Langkah | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| 1 | Daftar periksa sebelum diajukan | `HTTP 200`, `canSubmitClosing=true`, `notYetAvailable=7` | `PASS` |
| 2 | `POST /submit-closing` | `HTTP 200`, "Penutupan periode Januari 2019 berhasil diajukan dan menunggu persetujuan." Status periode berubah `1` → **`4`** | `PASS` |
| 3 | `GET /closing-history` sesudah pengajuan | `HTTP 200`, **1 baris**: `seq=1 action=1` beserta catatan pengajunya | `PASS` |
| 4 | **Prinsip empat mata** — `POST /approve-closing` oleh pengaju sendiri | **`HTTP 403`** — "Penutupan tidak dapat disetujui oleh orang yang mengajukannya." | `PASS` |
| 5 | `POST /reject-closing` tanpa alasan | **`HTTP 400`** — "Alasan penolakan wajib diisi." | `PASS` |
| 6 | `POST /submit-closing` ulang saat sudah menunggu persetujuan | **`HTTP 409`** — "Periode Januari 2019 tidak dalam keadaan terbuka." | `PASS` |
| 7 | `POST /reject-closing` dengan alasan | `HTTP 200`, periode kembali ke status **`1`** (Terbuka) | `PASS` |
| 8 | `GET /closing-history` akhir | `HTTP 200`, **2 baris**: `action=1` lalu `action=3` | `PASS` |

**Langkah 4 menutup acceptance (2) di sisi aturan bisnis.** Prinsip empat mata `ACC-DEC-016`
benar-benar ditegakkan backend, bukan hanya tertulis. Yang masih tersisa adalah membuktikan bahwa
**layar** menampilkan tombolnya dalam keadaan mati bagi pengguna kedua — itu menuntut peramban.

### Satu cacat ditemukan dan diperbaiki oleh uji ini

Langkah 7 dan 8 memunculkan keadaan yang tidak terpikir saat menulis kode: **sesudah penolakan,
periode kembali `Terbuka` tetapi riwayatnya tetap menyimpan baris pengajuan** — memang begitu
seharusnya, karena riwayat adalah bukti audit yang tidak pernah dihapus.

Akibatnya `isSelfApproval` tetap bernilai benar pada periode yang sudah tidak punya pengajuan sama
sekali. Urutan pemeriksaan yang lama memeriksa keadaan itu lebih dahulu, sehingga layar akan
berbunyi *"Penutupan tidak dapat disetujui oleh yang mengajukan"* — mengirim petugas mencari
masalah yang tidak ada. Kalimat yang benar adalah *"Belum ada pengajuan penutupan yang menunggu
persetujuan."*

Perbaikannya: urutan dibalik sehingga `isPendingApproval` diperiksa lebih dahulu, dan logikanya
diangkat menjadi fungsi murni `resolveApproveDisabledReason` pada `period-closing-constants.jsx`
supaya dapat diuji sendiri. Empat uji regresi ditambahkan, salah satunya memakai **riwayat asli
periode `2019-01` sesudah ditolak** sebagai `tests/fixtures/period-closing-history-ditolak.json`.

Cacat ini tidak akan tertangkap lint, build, maupun uji berbasis contoh buatan sendiri. Ia hanya
muncul sesudah alur penuh dijalankan sampai penolakan.

Uji manual: `NOT FEASIBLE`. Alasannya konkret dan bukan kemalasan: acceptance (2) menuntut dua
identitas berbeda pada satu periode yang sama, dan peran `Accounting Director` harus sudah ada di
mekanisme hak akses.

**Tidak dijalankan:** `npm run test:e2e`, sebab alasan yang sama.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tombol Ajukan mati selama masih ada penghalang, dan alasannya terbaca di layar | Terpenuhi | `canSubmit` mengikuti `canSubmitClosing` dari backend; `submitBlockedReason` dirender sebagai teks di bawah kartu aksi dan sebagai `title` tombol |
| (2) Tombol Setujui dan Tolak mati bila pembuka layar adalah yang mengajukan, disertai keterangan | **Terbukti sebagian** | **Aturannya terbukti ditegakkan:** `POST /approve-closing` oleh pengaju sendiri menjawab `HTTP 403` pada uji 10 Sep 2026 (langkah 4). **Logika layarnya terbukti lewat uji:** `resolveApproveDisabledReason` diuji terhadap riwayat sungguhan, termasuk keadaan sesudah penolakan yang sebelumnya salah. **Yang belum:** tampilan tombol mati bagi pengguna kedua di peramban, karena peran `Accounting Director` belum terpasang |
| (3) Penolakan tanpa alasan tidak dapat dikirim | Terpenuhi | `requireReason` pada `ConfirmModal` mematikan tombol kirim selama alasan kosong; perilaku bawaan yang sudah dipakai 5 modul administrator |
| (4) Tombol yang bukan haknya dimatikan, bukan disembunyikan | Terpenuhi | `usePermission("AccountingPeriod", "Close")` dan `("AccountingPeriod", "Approve")` hanya mengisi `disabled` dan keterangannya; tidak ada percabangan yang menghapus tombol dari DOM |

**Definition of Done:** lint hijau, build hijau, dan laporan tracked ini ada — terpenuhi. Yang
**belum** terpenuhi: keempat UAT peramban, dan acceptance (2) yang belum terbukti sungguhan.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` yang berasal dari task ini |
| Masalah yang diketahui | Acceptance (2) terbukti sebagian: aturan `403` dan logika layar sudah terbukti, tampilan bagi pengguna kedua belum. Menunggu peran `Accounting Director` terpasang beserta dua akun uji |
| Data uji yang tertinggal | **12 periode tahun buku 2019** beserta **2 baris riwayat penutupan** pada `2019-01` (`action=1` lalu `action=3`). Riwayat penutupan memang **tidak dapat dihapus** — ia bukti audit — dan periode tidak punya endpoint hapus. `2019-01` sudah kembali berstatus `Terbuka`. Buku 2026 tidak tersentuh sama sekali |
| Dependency backend | `BE-ACC-P2-006` sudah selesai. Yang belum: peran `Accounting Director` di mekanisme hak akses, yang menahan pembuktian acceptance (2) |
| Perubahan sampingan | `NONE`. Seluruh perubahan menumpang berkas yang dibuat `FE-ACC-P2-001` |
| Interupsi | `NONE` |
| Status Git | Tidak ada berkas yang khusus milik task ini; perubahannya menyatu pada tiga berkas `FE-ACC-P2-001`. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Siapkan dua akun uji beserta peran `Accounting Director`, lalu jalankan `UAT-P2-15` sampai `UAT-P2-18` untuk menutup acceptance (2) |
