# Laporan Perubahan Frontend — `FE-RAD-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-12` |
| Judul | Koreksi berversi dan riwayat versi |
| Epic | `EPIC RAD-03` |
| Requirement | `FR-RAD-020`, `FR-RAD-021` (penomoran blueprint) |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-API-001` endpoint `POST /{id}/amendments` dan `GET /{id}/versions`, berjalan sejak `BE-RAD-10`; `RAD-STATE-001` bagian 4 |
| Acceptance criteria | `AC-18` |
| Test yang diminta roadmap | `UAT-06` versi 1 masih dapat dibuka dan isinya tidak berubah; `UAT-07` mengubah versi rilis ditolak |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 5 butir 8 — alasan koreksi wajib ditampilkan bersama versi bacaan |
| Dependency | `FE-RAD-11` **selesai**; `BE-RAD-10` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini dan baris status roadmap |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-14 |
| Status | **Selesai.** 14 unit test baru lulus; 904 unit test repository lulus, 0 gagal |

---

## 1. Ketentuan mengikat: alasan koreksi tampil bersama versinya

`RAD-ARCH-FE-001` bagian 5 butir 8. Dipenuhi pada komponen riwayat versi, dengan tiga sikap
yang disengaja:

**Alasan diberi ruang sendiri, bukan diperlakukan sebagai catatan kaki.** Ia dirender sebagai
blok bergaris tepi peringatan tepat di bawah nomor versinya — bukan di balik tautan, bukan di
layar lain, dan bukan sebagai teks kecil abu-abu. Pembaca riwayat yang melihat dua bacaan
berbeda atas satu pemeriksaan akan bertanya "mana yang keliru, dan mengapa", dan itu pertanyaan
pertama ketika sebuah keputusan klinis ditinjau ulang.

**Koreksi tanpa alasan dinyatakan sebagai kejanggalan.** Bila sebuah versi berpenanda
`IsAmendment` ternyata tidak membawa `AmendmentReason`, layar menampilkan peringatan alih-alih
ruang kosong. Backend mewajibkan alasan, sehingga keadaan itu seharusnya mustahil — dan justru
karena itu ia perlu terlihat bila terjadi.

**Alasan diminta lebih dulu daripada isinya pada layar tulis koreksi**, mengikuti urutan
`CreateAmendmentAsync`. Urutan itu bukan selera: tanpa alasan, riwayat yang dihasilkan tidak
dapat dijelaskan, dan riwayat yang tidak dapat dijelaskan sama tidak bergunanya dengan riwayat
yang hilang.

---

## 2. `UAT-06` — versi lama tetap berlaku selama koreksi disusun

Ini bagian yang paling mudah salah dibaca, dan akibatnya klinis.

`CreateAmendmentAsync` **sengaja tidak menaikkan** `CurrentVersionNumber` ketika draf koreksi
ditulis:

```csharp
// CurrentVersionNumber SENGAJA tidak dinaikkan di sini. Versi rilis sebelumnya
// tetap yang berlaku sampai koreksinya dirilis.
report.ReportStatus = RadReportStatus.AmendmentDrafted;
```

Akibatnya, selama koreksi disusun, **versi dengan nomor tertinggi bukan versi yang berlaku**.
Layar yang menandai "versi terbaru" sebagai yang berlaku akan menampilkan draf koreksi yang
belum diperiksa siapa pun kepada dokter jaga pukul sepuluh malam.

`isVersionInForce` karena itu membandingkan terhadap `currentVersionNumber` — **bukan** terhadap
nomor tertinggi, dan bukan terhadap `workingVersionNumber`. Versi yang berlaku diberi lencana
"Berlaku bagi pembaca" beserta kalimat yang menyebutkan artinya; versi lain diberi keterangan
keadaannya masing-masing, termasuk bahwa draf koreksi **belum boleh dipakai mengambil keputusan
klinis**. `S1` menguncinya.

`AC-18` dipenuhi pada `S2` dan `S3`: isi versi 1 tetap utuh setelah koreksi ditulis, dan versi
koreksi menunjuk pendahulunya lewat `PreviousVersionId` — sementara versi pertama tidak menunjuk
siapa pun.

---

## 3. Isi setiap versi disembunyikan lebih dulu

`GET /{id}/versions` mengembalikan **isi lengkap setiap versi** sekaligus — `Findings`,
`Impression`, dan `Recommendation` untuk seluruh riwayat. Balasan itu adalah kumpulan isi bacaan
terpadat di modul ini.

Menampilkan semuanya terbuka berarti seluruh riwayat klinis seorang pasien terpampang begitu
layar dibuka, termasuk pada layar bersama di ruang pemeriksaan. Isinya karena itu dibuka satu per
satu atas permintaan, dan alasannya dinyatakan di layar.

**Batas yang jujur:** penyembunyian ini murni tampilan. Datanya sudah ada di memori halaman
karena memang dikirim sekaligus; yang berkurang adalah apa yang **terbaca sekilas**, bukan apa
yang terkirim. Itu yang dapat dikerjakan layar, dan menyebutnya lebih dari itu akan menyesatkan.

Berkas riwayat versi **ditambahkan ke penjaga penyimpanan** `rad-report-storage-guard.test.mjs`,
sehingga ketentuan mengikat `FE-RAD-10` bagian 5 butir 6 berlaku atasnya secara terperiksa —
delapan berkas bacaan kini dijaga, bukan tujuh.

---

## 4. Dua penolakan koreksi yang sengaja dibedakan

`CreateAmendmentAsync` memisahkan keduanya karena menuntut tindakan yang berbeda:

| Keadaan | Kode | Yang harus dilakukan petugas |
| --- | --- | --- |
| Sudah pernah dirilis, tetapi versi kerja bukan `Released` | `RAD_AMENDMENT_ALREADY_IN_PROGRESS` | Selesaikan atau rilis koreksi yang sedang berjalan |
| Belum pernah dirilis sama sekali | `RAD_REPORT_NEVER_RELEASED` | Tidak ada yang perlu dikoreksi; sunting drafnya saja |

`describeAmendBlock` menyalin pemisahan itu apa adanya, memakai `firstReleasedAt` sebagai
pembeda — sama seperti backend. Meleburnya menjadi satu pesan membuat petugas menunggu sesuatu
yang tidak akan pernah datang. `S10` menguncinya.

---

## 5. Satu jebakan yang ditemukan saat merangkai

Rancangan pertama memakai ulang `authorRole` milik isian draf pertama untuk koreksi. Itu keliru,
dan keliru secara diam-diam: pemilih peran penulis hanya dirender ketika `isNewDraft` bernilai
benar, sedangkan koreksi selalu dikerjakan atas bacaan yang **sudah dirilis** — sehingga
`authorRole` selalu bernilai kosong dan setiap koreksi akan ditolak `400
RAD_AUTHOR_ROLE_REQUIRED` bagi penulis yang bukan radiolog.

`CreateAmendmentAsync` memanggil `ResolveAuthorRoleAsync` yang sama dengan draf pertama, jadi
koreksi memang perlu menyebut peran penulisnya sendiri — dan yang menulis koreksi belum tentu
orang yang menulis versi sebelumnya. `authorRole` karena itu dipindahkan menjadi ruas milik
isian koreksi, dengan pemilihnya sendiri.

---

## 6. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/components/view/…/rad-reports/rad-report-version-history.jsx` | Baru — riwayat versi; **ketentuan mengikat bagian 5 butir 8 ada di sini** |
| `tests/unit/rad-report-amendment-rules.test.mjs` | Baru — 14 test; `UAT-06`, `UAT-07`, `AC-18` |
| `src/lib/hooks/health-services/radiology-management/rad-report-rules.js` | Diubah — `canAmend`, `describeAmendBlock`, `validateAmendmentContent`, `buildAmendmentPayload`, `amendmentFormFromReport`, `emptyAmendmentForm`, `isVersionInForce`, `getVersionInForceNumber`, `isAmendmentVersion`, `getAmendmentReason`, `getVersionNumber`, `getPreviousVersionId`, `assertVersionOrderDescending` |
| `src/lib/hooks/health-services/radiology-management/use-rad-report-draft.jsx` | Diubah — isian koreksi, `tulisKoreksi`, `bukaKoreksi`, `tutupKoreksi`; pembersihan unmount mencakup isian koreksi |
| `src/components/view/…/rad-reports/rad-report-draft-view.jsx` | Diubah — bagian koreksi; daftar versi sederhana **digantikan** komponen riwayat |
| `src/lib/constants/…/rad-report-constants.jsx` | Diubah — `RAD_REPORT_VERSION_COPY`, salinan teks koreksi pada `RAD_REPORT_ACTION_COPY` |
| `src/style/…/rad-reports/rad-report.module.css` | Diubah — 5 kelas riwayat versi, seluruhnya design token |
| `tests/unit/rad-report-storage-guard.test.mjs` | Diubah — berkas riwayat versi masuk daftar yang dijaga |

**Tidak ada route baru** — koreksi dan riwayat tumbuh di layar bacaan yang sudah ada, karena
yang menulis koreksi perlu membaca versi berlakunya lebih dulu. Tidak ada potongan Redux baru,
tidak ada base component baru, tidak ada entri menu baru, dan tidak ada pemanggilan Axios baru:
`createRadReportAmendment` dan `getRadReportVersions` sudah ada sejak `FE-RAD-01`.

Riwayat versi dibaca dari `report.versions` yang **sudah terbawa** `GET /by-study/{id}`.
`GET /{id}/versions` karena itu tidak dipanggil terpisah — memanggilnya hanya akan menambah satu
permintaan untuk data yang sudah ada.

---

## 7. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-report-amendment-rules.test.mjs` | **14 lulus, 0 gagal** |
| `node --test tests/unit/rad-report-storage-guard.test.mjs` | **5 lulus** — kini menjaga delapan berkas bacaan |
| `node --test tests/unit/` (seluruh repository) | **904 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 3 warning** — seluruhnya `react-hooks/set-state-in-effect` yang sudah ada sebelumnya. **Tidak ada warning baru** |
| `npm run build` | **Compiled successfully in 34.3s.** Tidak ada route baru, sesuai rancangan |

Cakupan test: `UAT-06` pada `S1` dan `S2`; `AC-18` pada `S2` dan `S3`; ketentuan mengikat pada
`S4` sampai `S7`; `UAT-07` pada `S8`; dua penolakan yang dibedakan pada `S10`; urutan riwayat
pada `S13`.

**`MANUAL TEST: NOT FEASIBLE`**, dan rantai sebabnya panjang serta seluruhnya di luar frontend.
Koreksi hanya dapat ditulis atas bacaan berstatus `Released`. Agar sebuah bacaan dirilis,
ia harus disahkan lebih dulu; pengesahan menuntut `RadReport : ActAsRadiologist`, yang **tidak
dapat diberikan kepada peran mana pun**. Agar ada bacaan sama sekali, sebuah study harus mencapai
`QualityAccepted`; itu mustahil selama belum ada aturan keselamatan `Active` (`RAD-OPEN-011`).
**Tidak akan pernah ada satu pun bacaan `Released` di lingkungan mana pun saat ini**, sehingga
layar koreksi tidak dapat dicapai sama sekali. Seluruh jalur dikunci unit test terhadap perilaku
yang dibaca langsung dari `RadReportService` dan DTO-nya.

---

## 8. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Memanggil `GET /{id}/versions` terpisah | `report.versions` sudah membawanya. Satu permintaan tambahan untuk data yang sama tidak menambah apa pun |
| Perbandingan isi antar versi berdampingan | Tidak diminta, dan `DEV_DISCRETION` tidak berarti menambah yang tidak diminta. Isi tiap versi dapat dibuka sendiri-sendiri |
| Mengurutkan riwayat di sisi klien | Urutan milik server. Layar memeriksanya dan memperingatkan bila rusak — pola yang sama dengan urutan cito pada `FE-RAD-07` |
| Menampilkan seluruh isi versi terbuka | Lihat bagian 3 |
| Base component baru | Dilarang `AGENTS.md`. Riwayat versi disusun dari `BaseButton`, `StatusBadge`, dan `InformationAlert` yang sudah ada |

---

## 9. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **~~`RadReport : ActAsRadiologist` tidak dapat diberikan~~** | **KELIRU — dikoreksi 2026-09-14.** Penandanya sudah dapat diberikan sejak 2026-09-11; sisanya pekerjaan Administrator. Ditetapkan pada posisi **Dokter Radiologi** oleh `RAD-DEC-017` |
| **Tidak ada aturan keselamatan `Active`** | `RAD-OPEN-011` terbuka. Tidak ada bacaan yang lahir sama sekali |
| **`EnsurePendingReportAsync` tanpa pemanggil** | Temuan `FE-RAD-10`, masih terbuka |
| **Empat transisi study tanpa endpoint** | Temuan `FE-RAD-09`, masih terbuka |
| **Tiga enum tidak terbit pada metadata study** | Temuan `FE-RAD-09`, masih terbuka |
| **Study terkunci saat aturan keselamatan berubah** | Temuan `FE-RAD-08`, masih terbuka |
| **Kontrak `409` vs jalur API `422`** pada `rad-studies` | Temuan `FE-RAD-08`, masih terbuka |
| **`HoldAsync` menerima status terminal** | Temuan `FE-RAD-06`, masih terbuka |
| **Daftar kerja tanpa identitas pasien** | Temuan `FE-RAD-07`, masih terbuka |

---

## 10. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 2 berkas baru, 6 diubah. 14 test baru lulus; 904 test repository lulus; lint 0 error tanpa warning baru; build lulus. Ketentuan mengikat bagian 5 butir 8 dipenuhi dengan alasan koreksi yang diberi ruang sendiri, dan koreksi tanpa alasan dinyatakan sebagai kejanggalan. `UAT-06` dipenuhi dengan membaca `currentVersionNumber`, bukan nomor tertinggi — selama koreksi disusun keduanya berbeda. Satu jebakan ditemukan dan diperbaiki: `authorRole` koreksi perlu ruas sendiri, kalau tidak setiap koreksi ditolak `RAD_AUTHOR_ROLE_REQUIRED`. | `draft` |
