# ISSUE-010 — Pasien yang sedang dirawat atau masih punya kunjungan aktif tetap lolos dari langkah 1 admisi

```yaml
issue_id: ISSUE-EPS-010
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 1 Pasien Lama, Cari & Verifikasi Pasien Terpadu (FE-INP-03 bagian 3.5.1); dampaknya terlihat di langkah Pilih Bed (3.9) dan Daftar Pasien Rawat Inap (FE-INP-16)"
sumber_laporan: "Permintaan pemilik sistem 13-10-2026: 2 butir (butir kedua menyusul di pesan berikutnya), 3 screenshot"
tanggal_issue: "2026-10-13"
status: SELESAI  # source/build/dokumen; uji runtime menunggu backend dijalankan ulang
keparahan_tertinggi: High
source_sha_backend: "8fce8c966803d21d763faa31de4ff3c049d557cf (MHamzah)"
source_sha_frontend: "1b0ad5dd4b347fa3493554a3b6484514cf2c2a43 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-010-pasien-lama-sudah-dirawat-lolos.md
```

## 1. Ringkasan

Pada langkah 1 jalur **Pasien Lama**, petugas mencari pasien dengan No. RM lalu menekan
**Lanjut ke Jenis Kunjungan & Kategori**. Layar tidak memeriksa apa pun selain "sudah ada pasien
terpilih", sehingga pasien yang **sedang dirawat dan menempati tempat tidur** maupun pasien yang
**masih punya kunjungan rawat jalan aktif** tetap bisa maju sampai empat langkah berikutnya.
Penolakan baru datang di langkah Pilih Bed/Booking Bed. Butir paling berbahaya (**High**) adalah
yang pertama: pada pasien yang sedang dirawat, sistem sempat membuat **kunjungan rawat inap baru
dan episode `Draft` baru** sebelum pemesanan bed ditolak — sisa inilah yang muncul sebagai admisi
ganda "Sedang Disiapkan" di Daftar Pasien Rawat Inap. Aturannya sendiri sudah benar di server
(`RWI-DEC-054`, aturan kunjungan aktif pendaftaran); yang kurang adalah pemeriksaan di awal alur.
Permintaan pemilik menutup celah ini adalah perubahan desain langkah 1 (`DESIGN_CHANGE`), bukan
penyimpangan dari desain yang ada.

## 2. Laporan asli

| No. laporan | Keluhan pelapor (diringkas) | Lampiran |
| ---: | --- | --- |
| 1 | "Setelah cari No RM lalu klik lanjut, ternyata pasien sudah terdaftar di rawat inap dan di bed. Seharusnya dibatasi: cek dulu apakah sudah terdaftar di episode rawat inap; belum → boleh lanjut, sudah → tidak boleh lanjut, agar tidak terjadi duplikat. Kalau mau, dia melakukan transfer bed." | Screenshot 1: langkah 1 Pasien Lama, pasien terpilih (pseudonim **Tn. Umar**, RM `00-00-00-9x`), badge "Siap Melanjutkan", tombol Lanjut aktif. Screenshot 2: Daftar Pasien Rawat Inap — pasien yang sama berstatus **Sedang Dirawat** di "BED 001 — Kamar 02" |
| 2 | "Tambahan: ketika pasien masih memiliki kunjungan aktif yang belum tertutup, di pencarian pasien lama juga tidak boleh dilanjut." Contoh pesan: "Pasien masih memiliki kunjungan aktif bernomor ENC-RSMMC-00xxx tanggal 30 Jul 2026. Selesaikan atau batalkan kunjungan tersebut sebelum mendaftar ke poliklinik atau layanan lain." | Screenshot 3: langkah 5 dari 9 "Pilih Bed", alert merah berisi pesan di atas |

Seluruh lampiran dirujuk butir di atas. Data pasien pada lampiran diganti pseudonim di dokumen ini.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-010-01` | 1 | Langkah 1 tidak memeriksa episode rawat inap yang sedang hadir | `DESIGN_CHANGE`, `RULE_VIOLATION` | FE + BE | High | SUDAH-VERIFIKASI di source; DARI-CAPTURE (screenshot 1–2) | `FIX-EPS-010-01`, `FIX-EPS-010-02` |
| `ISS-EPS-010-02` | 1 | Admisi pasien yang sedang dirawat meninggalkan kunjungan dan episode `Draft` sisa sebelum ditolak | `BUG` | BE + FE | High | SUDAH-VERIFIKASI di source | `FIX-EPS-010-01`, `FIX-EPS-010-02` |
| `ISS-EPS-010-03` | 2 | Kunjungan rawat jalan aktif baru ditolak di langkah Pilih Bed | `DESIGN_CHANGE`, `RULE_VIOLATION` | FE + BE | High | SUDAH-VERIFIKASI di source; DARI-CAPTURE (screenshot 3) | `FIX-EPS-010-01`, `FIX-EPS-010-02` |
| `ISS-EPS-010-T1` | — | Permintaan admisi Kamar Pulih yang masih menunggu juga baru ditolak di Pilih Bed | `RULE_VIOLATION` | FE + BE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-010-01`, `FIX-EPS-010-02` |
| `ISS-EPS-010-T2` | — | Admisi lain yang masih "Sedang Disiapkan" tidak diberitahukan, sehingga Draft kedua lahir diam-diam | `RULE_VIOLATION` | FE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-010-02` |
| `ISS-EPS-010-T3` | — | Badge "Siap Melanjutkan" statis, tampil juga untuk pasien yang tidak dapat diadmisi | `BUG` | FE | Low | SUDAH-VERIFIKASI; DARI-CAPTURE (screenshot 1) | `FIX-EPS-010-02` |

## 4. Rincian per temuan

### ISS-EPS-010-01 — Langkah 1 tidak memeriksa episode rawat inap yang sedang hadir

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` (utama), `RULE_VIOLATION` (aturan server tidak dicerminkan layar) |
| **Area** | Frontend + Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE pada screenshot 1–2 |
| **Perbaikan** | `FIX-EPS-010-01`, `FIX-EPS-010-02` |

**Apa yang terjadi.** Petugas admisi mengetik No. RM Tn. Umar, memilih kartunya, lalu menekan
**Lanjut ke Jenis Kunjungan & Kategori**. Layar langsung maju, padahal Tn. Umar sudah dirawat di
"BED 001 — Kamar 02" sejak 6 Oktober. Petugas mengisi jenis kunjungan, pembayaran, deposit, dan
DPJP, memilih bed, lalu baru di langkah Booking Bed muncul penolakan "Tn. Umar sudah dirawat pada
episode RI-… di Kamar 02 BED 001. Bila memang pindah kamar, pakai perpindahan, bukan admisi baru."

**Kenapa terjadi.**

```text
Tombol Lanjut maju tanpa pemeriksaan
  ← inpatient-admission-existing-patient-step.jsx:880-886 — tombol hanya mati bila belum ada pasien
     (disabled={!hasSelectedPatient}); onClick langsung onNext(currentSelectedPatientId)
    ← inpatient-admission-view.jsx:382-384 — onNext = flow.goNextWithPatient, hanya menulis URL
      ← tidak ada endpoint baca "bolehkah pasien ini diadmisi" sama sekali; aturan INV-INP-10
         hanya dipanggil saat bed dipesan (InpBedOccupancyService.cs:363-374) dan saat pasien
         ditempatkan (InpBedOccupancyService.cs:612-624)
        ← desain 02-backend-architecture.md §1.5 menaruh lapis 1 INV-INP-10 "sebelum menempatkan
           pasien"; skema 3.5.1 mengaktifkan tombol cukup bila "tepat 1 pasien berhasil diverifikasi"
```

`InpEpisodeService.Assignments.cs:59-107` sudah punya `FindPresentEpisodeAsync` — episode
`Admitted`, atau `DischargePending` yang kepergiannya belum dicatat — lengkap dengan kalimat
penolakan yang menyebut nomor episode dan lokasi (`InpPresentEpisodeInfo.RejectionMessage`,
`:1120-1124`). Bahan pemeriksaannya ada; yang belum ada hanya jalur baca yang dapat dipanggil layar.

**Apakah ini menyimpang dari desain?** Tidak, dan karena itu jenis utamanya `DESIGN_CHANGE`.
`05-skema-tampilan.md` 3.5.1 baris tombol "Lanjut ke Jenis Kunjungan": "Tepat 1 pasien berhasil
diverifikasi → Maju ke Langkah 2". Namun prinsip "validasi layar mencerminkan aturan server"
dilanggar: aturan `RWI-DEC-054` sudah pasti menolak pasien ini, dan layar baru memberitahu setelah
lima langkah — maka `RULE_VIOLATION` sebagai jenis kedua. Pemilik meminta tombol Lanjut menolak
pasien yang sudah terdaftar di episode rawat inap (laporan 13-10-2026).

**Dampak nyata.** Tn. Umar tidak pernah dapat diadmisi ulang (memang benar), tetapi petugas
menghabiskan 4–5 langkah dan baru tahu di akhir. Lebih buruk lagi, lihat `ISS-EPS-010-02`.

**Rekomendasi.** Endpoint baca kelayakan admisi di backend, dipanggil langkah 1 saat pasien dipilih
dan saat Lanjut ditekan. Pasien yang sedang hadir ditolak dengan nomor episode dan lokasinya,
beserta tombol **Pindah Tempat Tidur** (Detail Episode, bagian Perpindahan Tempat Tidur) dan
**Lihat Detail Episode**. Rincian di plan repair `FIX-EPS-010-01` dan `-02`.

### ISS-EPS-010-02 — Admisi pasien yang sedang dirawat meninggalkan kunjungan dan episode `Draft` sisa sebelum ditolak

| | |
| --- | --- |
| **No. laporan** | 1 ("agar tidak terjadi duplikat") |
| **Jenis** | `BUG` |
| **Area** | Backend (utama) + Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-010-01`, `FIX-EPS-010-02` |

**Apa yang terjadi.** Pada contoh Tn. Umar di atas, saat petugas menekan **Lanjut ke Pemesanan**
di langkah Pilih Bed, sistem sudah membuat kunjungan rawat inap baru dan episode baru berstatus
`Draft`. Pemesanan bed lalu ditolak, tetapi keduanya tidak ikut hilang. Di Daftar Pasien Rawat
Inap muncul baris kedua Tn. Umar "Sedang Disiapkan · Tanpa Pemesanan Aktif" di samping baris
"Sedang Dirawat" — inilah duplikat yang dikeluhkan.

**Kenapa terjadi.**

```text
Baris Draft kedua + kunjungan rawat inap kedua untuk pasien yang sedang dirawat
  ← use-inpatient-admission-bed.jsx:486-512 — prepareBooking memanggil admission.prepare(selectedBed)
    ← use-inpatient-admission-doctor.jsx:529-578 — submit: POST /patient-encounters/admin, lalu
       POST /episodes; keduanya berhasil
      ← InpEpisodeService.cs:138-409 — OpenAdmissionAsync tidak memeriksa INV-INP-10; yang diperiksa
         hanya Draft lain (sebagai peringatan, :285-305)
        ← POST /bed-occupancies/reservations baru menolak 409 PATIENT_ALREADY_ADMITTED
           (InpBedOccupancyService.cs:363-374) — sesudah kunjungan dan episode tersimpan
```

Jalur Transfer IGD/Poli tidak meninggalkan sisa, karena seluruhnya berjalan di satu transaksi yang
digulung balik saat pemesanan gagal (`InpAdmissionTransferService.cs:113-147`). Jalur Pasien Lama
dan Kamar Pulih memakai tiga permintaan HTTP terpisah, sehingga yang sudah tersimpan tetap tinggal.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `validation-matrix.md` bagian 3 hanya menjamin
"isian admisi tetap utuh" untuk penolakan yang **dapat dipulihkan** (bed direbut pasien lain).
Penolakan `INV-INP-10` tidak pernah dapat dipulihkan dari admisi yang sama, sehingga kunjungan dan
Draft yang tertinggal tidak punya guna apa pun dan bertentangan dengan tujuan `RWI-DEC-054`.

**Dampak nyata.** Draft sisa baru gugur setelah batas `DraftEpisodeExpiryHours`; selama itu ia
tampil di daftar kerja dengan tombol "Lanjutkan Admisi" yang pasti gagal lagi, dan kunjungan rawat
inap kedua ikut terhitung pada laporan kunjungan.

**Rekomendasi.** `OpenAdmissionAsync` menolak 409 `PATIENT_ALREADY_ADMITTED` sebelum menulis apa pun,
dan langkah Pilih Bed memeriksa ulang kelayakan **sebelum** membuat kunjungan. Rincian di
`FIX-EPS-010-01` dan `-02`.

### ISS-EPS-010-03 — Kunjungan rawat jalan aktif baru ditolak di langkah Pilih Bed

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` (utama), `RULE_VIOLATION` |
| **Area** | Frontend + Backend |
| **Keparahan** | High (server pasti menolak isian yang ditawarkan layar) |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE pada screenshot 3 |
| **Perbaikan** | `FIX-EPS-010-01`, `FIX-EPS-010-02` |

**Apa yang terjadi.** Ny. Sari (pseudonim) masih punya kunjungan poliklinik 30 Juli 2026 yang belum
selesai. Petugas tetap bisa melewati langkah 1–4; di Pilih Bed muncul alert merah "Pasien masih
memiliki kunjungan aktif bernomor ENC-RSMMC-00xxx…". Tidak ada data yang tersimpan, tetapi seluruh
isian empat langkah terbuang.

**Kenapa terjadi.**

```text
Alert merah di Pilih Bed
  ← use-inpatient-admission-doctor.jsx:533-558 — POST /patient-encounters/admin ditolak 400
    ← PatientEncounterController.cs:1795-1803 (validasi) dan :808-826 (cek ulang berkunci) —
       aturan "satu pasien satu kunjungan aktif" pendaftaran, saklar
       HealthServices:Registration:BlockActiveEncounter = true (appsettings.json:173)
      ← kunjungan yang menghalangi: rawat jalan berklinik, bukan IGD, belum batal/selesai, status
         Draft s.d. Sedang Konsultasi — OutpatientEncounterRules.WhereBlocksRegistration (:43-48)
        ← langkah 1 admisi tidak membaca aturan ini; aturannya hanya ada di jalur tulis pendaftaran
           dan pesannya dibentuk method private controller (:1708-1720)
```

**Apakah ini menyimpang dari desain?** Tidak — skema 3.5.1 tidak meminta pemeriksaan ini
(`DESIGN_CHANGE`). Tetapi aturan server sudah pasti menolak (`RULE_VIOLATION`). Pemilik meminta
langkah 1 menolak pasien dengan kunjungan aktif yang belum tertutup.

**Dampak nyata.** Empat langkah isian terbuang setiap kali; petugas sering mengira masalahnya ada
pada bed yang dipilih.

**Rekomendasi.** Endpoint kelayakan yang sama ikut membaca aturan pendaftaran itu — memakai saklar,
aturan, dan kalimat yang **sama persis** dengan pendaftaran — sehingga langkah 1 menolak dengan
pesan yang sama. Kunjungan dari IGD/Poli yang memang diputuskan rawat inap tetap lewat jalur
**Transfer IGD/Poli**, yang tidak memakai pemeriksaan ini. Rincian di `FIX-EPS-010-01` dan `-02`.

## 5. Tanya-jawab pelapor

> **T:** "Contoh pasien ini sudah terdaftar, kok bisa dia lanjut padahal sudah terdaftar di episode
> rawat inap?"
>
> **J:** Karena langkah 1 hanya memeriksa "sudah ada pasien yang dipilih". Aturan "satu pasien satu
> episode yang hadir" sudah ada di server, tetapi selama ini baru diperiksa saat bed dipesan dan
> saat pasien ditempatkan — empat langkah sesudahnya. Perbaikan ini memindahkan pemeriksaannya ke
> tombol Lanjut di langkah 1, dan server juga menolak sejak admisi dibuka.

> **T (tersirat):** "Kalau mau, dia melakukan transfer bed" — di mana transfer bed-nya?
>
> **J:** Di **Detail Episode** pasien itu, bagian **Perpindahan Tempat Tidur** (rute
> `/health-services/inpatient-management/episodes/{id}#episode-transfer-section`, tautan yang sama
> dengan tombol "Pindahkan" di Daftar Pantau). Setelah perbaikan, penolakan di langkah 1 memuat
> tombol yang langsung membuka bagian itu.

## 6. Temuan tambahan

### ISS-EPS-010-T1 — Permintaan admisi Kamar Pulih yang masih menunggu juga baru ditolak di Pilih Bed

| | |
| --- | --- |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend + Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-010-01`, `FIX-EPS-010-02` |
| **Kenapa dicantumkan** | Akar masalahnya sama dengan `ISS-EPS-010-01`: aturan server yang pasti menolak, tetapi tidak dibaca langkah 1 |

Pasien yang punya permintaan admisi `Pending` dari Kamar Pulih (IBS) wajib diadmisi dari permintaan
itu (`VAL-RWF-87`). `InpAdmissionReferralService.CheckForAdmissionAsync` (`:198-229`) baru dipanggil
`OpenAdmissionAsync` (`InpEpisodeService.cs:172-183`) dari langkah Pilih Bed, dan layar menerjemahkan
kodenya di `use-inpatient-admission-doctor.jsx:632-639`. Bila petugas mencari pasien yang sama lewat
Pasien Lama, ia baru tahu di akhir. Perbaikan: endpoint kelayakan ikut memanggil pemeriksaan yang
sama, dan langkah 1 mengarahkan petugas ke jalur Kamar Pulih.

### ISS-EPS-010-T2 — Admisi lain yang masih "Sedang Disiapkan" tidak diberitahukan

| | |
| --- | --- |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-010-02` |
| **Kenapa dicantumkan** | Sumber duplikat kedua yang serupa dengan laporan 1 |

Server memberi peringatan pada `warnings` bila pasien punya episode `Draft` lain
(`InpEpisodeService.cs:285-305`; `validation-matrix.md` bagian 3, "Peringatan admisi Draft ganda —
bukan penolakan"). `use-inpatient-admission-doctor.jsx` tidak membaca `warnings` sama sekali, dan
peringatan itu pun baru lahir di Pilih Bed, saat Draft kedua sudah dibuat. Keputusan bahwa Draft
ganda **bukan penolakan** tetap dihormati; yang diperbaiki adalah memberi tahu petugas di langkah 1
beserta tombol **Lanjutkan Admisi {nomor}** agar ia memilih melanjutkan yang lama.

### ISS-EPS-010-T3 — Badge "Siap Melanjutkan" statis

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI; DARI-CAPTURE (screenshot 1) |
| **Perbaikan** | `FIX-EPS-010-02` |
| **Kenapa dicantumkan** | Terlihat pada screenshot 1 tetapi tidak dikeluhkan; memberi kesan pasien boleh diadmisi |

`inpatient-admission-existing-patient-step.jsx:636` menulis `badgeText="Siap Melanjutkan"` untuk
setiap pasien terpilih. Setelah perbaikan badge mengikuti hasil pemeriksaan: "Memeriksa…",
"Siap Melanjutkan", "Sedang Dirawat", "Kunjungan Aktif", dan seterusnya.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| — | Pasien yang sedang dirawat dan pasien berkunjungan aktif tidak boleh lanjut dari langkah 1 | — | **Sudah diambil** pemilik pada laporan 13-10-2026 ("jika sudah terdaftar tidak boleh lanjut", "tidak bisa di lakukan dan tidak bisa di lanjut"); dicatat sebagai `RWI-DEC-297` | Tidak |
| K-01 | Apakah admisi lain yang masih `Draft` juga **ditolak**, bukan sekadar diperingatkan? | Tetap peringatan (keputusan berlaku) / ditolak | **Tetap peringatan** dengan tombol "Lanjutkan Admisi {nomor}" sebagai pilihan utama. Alasannya: keputusan "Draft ganda bukan penolakan" sudah tertulis di `validation-matrix.md` bagian 3 dan controller, dan Draft yang telantar gugur sendiri. Bila pemilik ingin ditolak, cukup satu baris di endpoint kelayakan dan `OpenAdmissionAsync` | Tidak — perbaikan memakai rekomendasi |

## 8. Catatan pola

- Ini kali ketiga dalam sepekan aturan server yang pasti menolak baru terlihat di akhir alur admisi
  (`ISSUE-EPS-009` alasan galat tertutup; `ISSUE-EPS-003` validasi pendaftaran). Polanya sama:
  aturan tinggal di jalur **tulis**, tidak ada jalur **baca** yang dapat dipanggil layar lebih awal.
  Usulan pencegahan: setiap aturan tolak pada admisi yang hanya bergantung pada identitas pasien
  disatukan pada satu endpoint kelayakan (`admission-eligibility`), sehingga aturan berikutnya cukup
  ditambahkan di sana.
- Pemindahan titik tulis ke langkah Pilih Bed (`FE-RWI-240`) memperpanjang jarak antara langkah 1 dan
  penolakan pertama dari dua langkah menjadi lima, sehingga masalah ini lebih terasa sejak 13-10-2026.
- Aturan kunjungan aktif dan kalimatnya kini dipakai dua modul. Perbaikan memindahkan saklar, aturan,
  dan kalimatnya ke `OutpatientEncounterRules` supaya tidak bercabang.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Issue dibuat dari laporan 1 butir; butir 2 ditambahkan pelapor sebelum analisis selesai. Pemilik meminta langsung diperbaiki, status `DALAM_PERBAIKAN` | `diagnose-module-issue` |
| 2026-10-13 | Seluruh perbaikan ✅: `BE-RWI-230`, `FE-RWI-244`, amandemen dokumen. Status `SELESAI` pada tingkat source; uji runtime menunggu pemilik | `build-module-backend`, `build-module-frontend` |
