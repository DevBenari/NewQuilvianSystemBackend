# ISSUE-038 — Admisi bayi: "Lanjut ke Pemesanan" harus diklik dua kali, boks rawat gabung sering "belum tersedia", dan layar meminta muat ulang

```yaml
issue_id: ISSUE-EPS-038
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Admisi Bayi Baru Lahir, Langkah 4 Penempatan dan Pemesanan (route /health-services/inpatient-management/admissions/newborn?step=bed-selection)"
sumber_laporan: "Laporan pengguna 23-10-2026: 3 keluhan + 2 permintaan (analisis session storage/API, audit kode admisi bayi), 1 lampiran (screenshot halaman dan tab Network, jam layar 22-10-2026 15.48)"
tanggal_issue: "2026-10-23"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "d79e3ed13d5ec73af63a3d22cd386aa702477dc1 (MHamzah)"
source_sha_frontend: "2b16ec441982e8917be151b6c58bc5673cf67d11 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-038-klik-ganda-penempatan-dan-boks-rawat-gabung.md
```

---

## 1. Ringkasan

Laporan ini menyangkut **Langkah 4 "Penempatan dan Pemesanan"** pada admisi bayi baru lahir. Ada
tiga keluhan, dan ketiganya **bug**, bukan perilaku yang sengaja dirancang.

1. **Klik ganda.** Klik pertama "Lanjut ke Pemesanan" sebenarnya sudah membuat kunjungan dan
   episode `Draft` bayi di server. Begitu nomor episode ditulis ke alamat halaman, layar admisi
   menganggap dirinya sedang "melanjutkan admisi lama", menyembunyikan seluruh langkah selama
   episode dibaca ulang, lalu memasang ulang langkah 4 dari nol. Penanda "sudah masuk ke
   pemesanan" ikut hilang, sehingga layar kembali ke denah. Klik kedua berhasil karena episodenya
   sudah ada.
2. **Boks rawat gabung "belum tersedia".** Layar mencari boks bayi hanya pada **satu lantai denah**
   — lantai terakhir yang dibuka petugas di peramban itu — padahal kamar ibu bisa berada di lantai
   lain. Bila lantainya berbeda, boks tidak pernah ditemukan. Data boks lintas lantai sebenarnya
   sudah diambil dari server, hanya tidak dipakai. Selain itu, kamar tanpa boks (di luar VIP
   301–305) tampil dengan kalimat yang sama, sehingga petugas tidak tahu sebabnya.
3. **Perintah muat ulang.** Tombol "Pesan Boks Bayi & Lanjut" (rawat gabung) memesan boks memakai
   salinan pengendali lama yang dibuat sebelum episode ada. Server **berhasil** memesan, tetapi
   layar menganggapnya gagal dan menampilkan "Sesi berubah. Muat ulang data menggunakan sesi saat
   ini." Setelah dimuat ulang, boks itu sudah dipegang episode pertama, sehingga kembali tampil
   "belum tersedia" — keluhan 2 dan 3 saling memperparah.

Yang paling berbahaya adalah keluhan 3 beserta temuan tambahan T2: memuat ulang halaman pada
rawat gabung dapat membuat **kunjungan dan episode `Draft` kedua** untuk bayi yang sama tanpa
disadari petugas. Audit juga menemukan enam temuan tambahan, termasuk penyimpanan sesi
(`sessionStorage`) yang membawa pilihan ibu dari admisi bayi sebelumnya.

---

## 2. Laporan asli

| No. | Keluhan atau permintaan pelapor (diringkas) | Lampiran |
| :---: | --- | :---: |
| 1 | Saat admisi bayi, setelah memilih boks lalu klik **"Lanjut ke Pemesanan"**, layar tidak lanjut ke langkah berikut, malah meminta memilih lagi. Klik kedua baru lanjut ke "Penempatan dan Pemesanan Bayi". Selalu terjadi. | L-1 |
| 2 | Bila memilih **rawat gabung dengan ibu**, boks sering tidak tersedia. | — |
| 3 | Sering ada bug yang meminta **muat ulang**. | — |
| 4 | *Permintaan:* analisis masalah, cek session storage, API, dan lainnya agar masalah tidak terulang. | — |
| 5 | *Permintaan:* audit kode admisi bayi agar terhindar dari bug lain. | — |

**Lampiran L-1** — screenshot halaman `.../admissions/newborn?step=bed-selection&patientId=…&episodeId=…`
beserta tab Network peramban. Yang terlihat:

- Alamat halaman **sudah memuat `episodeId`**, tetapi layar masih menampilkan denah "Pilih Tempat
  Tidur" dengan `NICU-1` tetap terpilih dan tombol "Lanjut ke Pemesanan" aktif.
- Permintaan `floor-plan?floorCode=2` dan `floor-plan?floorCode=2&purpose=…` masing-masing muncul
  beberapa kali, sebagian berstatus **(canceled)**; `floor-plan?floorCode=3` juga terambil.
- `newborn?step=bed-selection&pa…` (navigasi halaman Next.js) terambil dua kali.
- Ada permintaan `episodes` (pembuatan episode), `care-dependency`, dan **`doctor-assignments`**.

Butir 2 dan 3 tidak punya lampiran sendiri; analisisnya berdasar source.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-038-01` | 1 | Langkah 4 dipasang ulang setelah episode dibuat, penanda pemesanan hilang | `BUG` | Frontend | High | SUDAH-VERIFIKASI di source; cocok dengan L-1 | `FIX-EPS-038-01`, `FIX-EPS-038-02` |
| `ISS-EPS-038-02` | 2 | Boks rawat gabung dicari hanya di satu lantai denah | `BUG` | Frontend | High | SUDAH-VERIFIKASI di source; lantai kamar ibu DUGAAN | `FIX-EPS-038-03` |
| `ISS-EPS-038-03` | 3 | Pemesanan boks berhasil di server tetapi layar menyatakan "Sesi berubah. Muat ulang" | `BUG`, `PERLU_KONFIRMASI` | Frontend | High | SUDAH-VERIFIKASI di source; teks yang dilihat pelapor perlu dikonfirmasi | `FIX-EPS-038-04` |
| `ISS-EPS-038-T1` | 4 | Draf sesi `newborn:active` dipakai bersama semua admisi bayi | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI di source | `FIX-EPS-038-05` |
| `ISS-EPS-038-T2` | 3, 4 | Episode rawat gabung tidak tercatat di alamat halaman; muat ulang membuat episode kedua | `BUG` | Frontend | High | SUDAH-VERIFIKASI di source; data ganda DUGAAN | `FIX-EPS-038-01` |
| `ISS-EPS-038-T3` | 2, 5 | Boks cadangan rawat gabung bisa berupa bed dewasa atau boks yang sedang dipakai | `RULE_VIOLATION` | Frontend | Medium | SUDAH-VERIFIKASI di source dan kontrak | `FIX-EPS-038-03` |
| `ISS-EPS-038-T4` | 1, 5 | Klik kedua dengan bed lain memakai episode yang unit layanannya dari bed pertama | `BUG` | Frontend + Backend | Low | Kelas SUDAH-VERIFIKASI diselaraskan server; unit layanan DUGAAN | `FIX-EPS-038-01` (menghilangkan pemicunya) |
| `ISS-EPS-038-T5` | 2, 5 | Kandidat ibu hanya halaman pertama; setelah muat ulang kamar ibu dianggap tertutup | `BUG` | Frontend | Low | DUGAAN | `FIX-EPS-038-06` |
| `ISS-EPS-038-T6` | 5 | Teks tetap "Kamar Ibu (VIP)", "Lantai 3", dan isi pengganti yang tampak seperti data | `BUG`, `DESIGN_CHANGE` | Frontend | Low | SUDAH-VERIFIKASI di source | `FIX-EPS-038-07`, `FIX-EPS-038-08` |

---

## 4. Rincian per temuan

### ISS-EPS-038-01 — Langkah 4 dipasang ulang setelah episode dibuat, penanda pemesanan hilang

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; cocok dengan lampiran L-1 (DARI-CAPTURE) |
| **Perbaikan** | `FIX-EPS-038-01`, `FIX-EPS-038-02` |

**Apa yang terjadi.** Petugas memilih tempat bayi di denah, misalnya `NICU-1`, lalu menekan
"Lanjut ke Pemesanan". Tombol berputar sebentar, lalu layar kembali menampilkan denah yang sama
seolah belum terjadi apa-apa. Padahal di belakang layar kunjungan dan episode `Draft` bayi sudah
dibuat. Petugas menekan tombol yang sama sekali lagi, dan barulah kartu "Pesan Tempat Tidur"
muncul.

**Kenapa terjadi.** Rantai sebabnya:

```text
Layar kembali ke denah setelah klik pertama
  ← NewbornPlacementStep dipasang ulang, state lokal inBookingView kembali false
    ← view admisi merender "null" untuk seluruh langkah selama resume.isResuming && !resume.isReady
      ← handleSelectionNext menulis episodeId ke URL, sehingga flow.resumeEpisodeId terisi
        dan hook pelanjutan (FE-RWI-032) membaca ulang episode itu dari server
        ← akar: dua perbaikan lama bertemu —
           FIX-EPS-033 menyimpan sub-tampilan "pemesanan" sebagai state lokal komponen langkah,
           FE-RWI-032 menyembunyikan seluruh langkah selama episode di URL sedang dibaca,
           tanpa membedakan episode lama dari episode yang baru saja dibuat layar ini sendiri.
```

1. Tombol "Lanjut ke Pemesanan" memanggil `prepareBooking`, lalu `onNext(result.episodeId)` —
   `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx:195-200`.
2. `prepareBooking` membuat kunjungan dan episode `Draft` lewat `doctor.submit`, karena belum ada
   episode — `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx:1022-1031`
   dan `use-inpatient-admission-doctor.jsx:691-744`.
3. `onNext` pada langkah bayi adalah `handleSelectionNext`, yang menyalakan penanda lokal lalu
   menulis `episodeId` ke URL —
   `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx:370`
   dan `:456-459`:

   ```jsx
   const [inBookingView, setInBookingView] = useState(false);
   …
   const handleSelectionNext = (id) => {
     setInBookingView(true);
     flow.writeFlowUrl({ nextEntryMode: flow.entryMode, nextStepSlug: "bed-selection", nextEpisodeId: id, replace: true });
   };
   ```

4. `episodeId` di URL dibaca sebagai `flow.resumeEpisodeId`, lalu diberikan ke hook pelanjutan —
   `inpatient-admission-view.jsx:562-568`. Hook itu menyatakan `isResuming = Boolean(episodeId)`
   dan `isReady` baru benar setelah episode selesai dibaca —
   `use-inpatient-admission-resume.jsx:123` dan `:173`.
5. Selama itu, view tidak merender langkah apa pun — `inpatient-admission-view.jsx:1174`:

   ```jsx
   {resume.isResuming && !resume.isReady ? null : !flow.entryMode ? ( … ) : ( … <AdmissionStep … /> )}
   ```

   `NewbornPlacementStep` dilepas dari layar, dan state `inBookingView` ikut hilang.
6. Setelah episode terbaca, langkah 4 dipasang lagi dengan `inBookingView = false`. Penentu tampilan
   `isBookingReady = bed.hasActiveReservation || (inBookingView && bed.selectedBed)` bernilai salah,
   sehingga denah tampil lagi — `inpatient-newborn-admission-steps.jsx:452-454` dan `:534-539`.
   Bed terpilih tetap ada karena pengendali bed dipasang di level view, sehingga `NICU-1` masih
   tersorot seperti pada L-1.
7. Klik kedua berhasil karena `prepareBooking` langsung mengembalikan episode yang sudah ada
   (`use-inpatient-admission-bed.jsx:1028`), dan URL yang ditulis sama persis sehingga pelanjutan
   tidak dimulai ulang.

Bukti dari lampiran (DARI-CAPTURE): URL pada L-1 sudah memuat `episodeId` ketika denah masih
tampil, navigasi `newborn?step=bed-selection…` terambil dua kali (dua kali `router.replace`), dan
ada permintaan `doctor-assignments`. Satu-satunya pemanggil `GET {episodeId}/doctor-assignments`
pada alur admisi adalah efek adopsi episode pelanjutan pada
`use-inpatient-admission-doctor.jsx:191-245` (baris `:219`) — tanda bahwa jalur pelanjutan memang
berjalan di tengah langkah 4.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. Kontrak
`contracts/newborn-admission-repair-022.md` bagian 2 menetapkan langkah 4 `bed-selection` sebagai
satu langkah yang menghasilkan "Encounter, episode Draft, dan reservasi bed". Tidak ada desain
yang meminta petugas menekan tombol yang sama dua kali. Masalah ini adalah regresi dari
`FIX-EPS-033`, yang tes unitnya hanya mencocokkan teks source sehingga tidak mendeteksi
perilaku pasang-ulang (lihat bagian 8).

**Dampak nyata.** Petugas bangsal bayi menekan "Lanjut ke Pemesanan" untuk By. Ny. Sari pada
`NICU-1`. Layar kembali ke denah. Petugas mengira penyimpanan gagal lalu (a) menekan lagi — alur
lanjut, atau (b) memilih `PERINA-2` karena mengira `NICU-1` bermasalah — episode tetap memakai
unit layanan dari klik pertama, hanya kelasnya yang diselaraskan server (lihat T4), atau (c) meninggalkan layar — episode `Draft`
yatim tertinggal di daftar kerja.

**Rekomendasi.** Pertahankan langkah 4 tetap terpasang ketika episode di URL adalah episode yang
baru saja dibuat layar ini, dan pindahkan penanda sub-tampilan pemesanan ke pengendali bed yang
tidak ikut dilepas. Rincian di plan repair (`FIX-EPS-038-01`, `FIX-EPS-038-02`).

---

### ISS-EPS-038-02 — Boks rawat gabung dicari hanya di satu lantai denah

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; lantai kamar ibu pada kasus pelapor DUGAAN |
| **Perbaikan** | `FIX-EPS-038-03` |

**Apa yang terjadi.** Petugas memilih "Rawat Gabung dengan Ibu". Ibu sedang dirawat di kamar VIP
lantai 3 yang punya boks bayi kosong. Langkah 4 kadang menampilkan boks itu dengan benar, kadang
menampilkan "Boks Bayi Belum Tersedia di Kamar Ibu" dengan tombol pesan mati, dan menyarankan
pindah ke Perinatologi.

**Kenapa terjadi.**

```text
"Boks Bayi Belum Tersedia di Kamar Ibu"
  ← availableBabyBed null, isBedAvailable false (inpatient-newborn-admission-steps.jsx:388-408, :234, :305-321)
    ← boks dicari pada bed.board.availableBeds dan bed.floorPlan.data.rooms
      ← keduanya berasal dari SATU lantai denah (use-inpatient-admission-bed.jsx:320-359)
        ← lantai yang dimuat adalah lantai terakhir yang disimpan peramban di localStorage
           "quilvian.inpatient.floor-plan.last-floor", atau lantai pilihan server bila kosong
           (use-floor-plan.js:22, :31-38, :235-237, :297-299; InpFloorPlanQueryService.cs:146-150)
          ← akar: tampilan rawat gabung (FIX-EPS-037) memakai ulang sumber data denah, padahal
             denahnya tidak lagi ditampilkan sehingga petugas tidak bisa berpindah lantai
```

Ironisnya, data yang benar **sudah diambil** pada saat yang sama. Untuk rawat gabung, view mengisi
`newbornContext.roomId` dengan kamar ibu (`inpatient-admission-view.jsx:715`), sehingga hook papan
memanggil `GET /bed-occupancies/bed-board` dan `GET /bed-occupancies/available-beds` dengan
`roomId=<kamar ibu>&isForNewborn=true` beserta konteks bayi (`patientId`, `newbornCareMode`,
`motherEpisodeId`) — **tanpa batas lantai**
(`src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx:366-382` dan `:384-412`).
Server menilai kelayakan rawat gabung memakai kamar penempatan ibu yang aktif
(`NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpNewbornAdmissionRules.cs:70-81`
dan `:83-100`). Hasilnya tersimpan di `legacyBoard`, tetapi hanya dipakai untuk menandai bed pada
denah dan tidak pernah dibaca oleh tampilan rawat gabung. Setelah episode dibuat, papan itu bahkan
dimatikan (`use-inpatient-admission-bed.jsx:127`).

Ada juga sebab yang **sah**: sesuai `PLAN-REPAIR-031` (`FIX-EPS-031-03`), boks bayi hanya dipasang
di kamar `VIP 301` sampai `VIP 305` lantai 3. Ibu yang dirawat di kamar lain memang tidak punya boks.
Masalahnya, layar memakai kalimat yang sama ("belum tersedia atau sedang dipakai") untuk tiga
keadaan yang berbeda — kamar tanpa boks, boks sedang terpakai/dibersihkan, dan boks tidak
ditemukan karena salah lantai — sehingga petugas tidak tahu mana yang terjadi.

Kenapa "kadang-kadang": lantai terakhir di peramban berubah tergantung pekerjaan sebelumnya.
Petugas yang baru menempatkan bayi di NICU lantai 2 (seperti pada L-1) lalu mendaftarkan bayi
rawat gabung yang ibunya di lantai 3 akan mendapat denah lantai 2. Bila lantai terakhir kebetulan
lantai 3, boks ditemukan.

Penyebab kedua yang memperparah: setelah percobaan pertama gagal karena ISS-EPS-038-03, boks itu
sudah berstatus `Reserved` oleh episode bayi sendiri, sehingga juga tampil "belum tersedia".

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `PLAN-REPAIR-037` menjanjikan boks bayi kamar
ibu terpilih otomatis dari data kamar ibu, dan kontrak 022 invarian 8 menyatakan bed bayi yang
tersedia dapat dipilih. Desain tidak membatasi kamar ibu pada lantai tertentu.

**Dampak nyata.** Ny. Sari dirawat di VIP 301 (lantai 3) dengan boks `301-BOKS` kosong. Petugas
yang terakhir membuka denah lantai 2 melihat "Boks Bayi Belum Tersedia" lalu, mengikuti saran
layar, menekan "Ubah ke Perawatan Perinatologi". Bayi yang seharusnya rawat gabung menurut
keputusan klinis terdaftar terpisah dari ibunya.

**Rekomendasi.** Untuk rawat gabung, baca boks dari hasil `available-beds`/`bed-board` kamar ibu
yang sudah diambil (lintas lantai, kelayakan dinilai server), bukan dari denah satu lantai, dan
bedakan kalimat untuk tiga keadaan di atas. Rincian di `FIX-EPS-038-03`.

---

### ISS-EPS-038-03 — Pemesanan boks berhasil di server tetapi layar menyatakan "Sesi berubah. Muat ulang"

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` (utama), `PERLU_KONFIRMASI` (teks pesan yang dilihat pelapor) |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | Mekanisme SUDAH-VERIFIKASI di source; bahwa pesan inilah yang dilihat pelapor DUGAAN |
| **Perbaikan** | `FIX-EPS-038-04` |

**Apa yang terjadi.** Pada rawat gabung, petugas menekan "Pesan Boks Bayi & Lanjut". Muncul
kotak merah "Sesi berubah. Muat ulang data menggunakan sesi saat ini." dan layar tidak maju.
Petugas memuat ulang halaman. Padahal boks sudah dipesan oleh server.

**Kenapa terjadi.**

```text
Pesan "Sesi berubah. Muat ulang data menggunakan sesi saat ini."
  ← useBedOperation.run membandingkan intent lama dengan current.current setelah POST selesai
    (use-bed-operation.js:13, :17-33, pembandingan di :24, pesan di :26)
    ← intent disimpan per "scope"; scope pemesanan = `reservation:${episodeId}`
       (use-inpatient-admission-bed.jsx:360; bed-operation.service.js:4-13)
      ← saat tombol ditekan episodeId masih kosong → scope "reservation:";
         setelah prepareBooking membuat episode, render baru memakai scope "reservation:<id>"
        ← handleDirectReserveAndNext memanggil bed.reserve dari objek `bed` milik render LAMA
           setelah `await bed.prepareBooking(...)` (inpatient-newborn-admission-steps.jsx:414-447)
          ← akar: FIX-EPS-037-03 menggabungkan "buat episode" dan "pesan boks" dalam satu
             handler async, padahal identitas pengendali bed berubah di antara keduanya
```

Potongan kunci — `src/lib/hooks/health-services/inpatient-management/use-bed-operation.js:17-26`:

```js
const run = useCallback(async (body, request) => {
  if (!ownerKey) throw new Error("Sesi pengguna belum tersedia.");
  const promise = runBedOperation(intent, body, request);
  …
  const result = await promise;
  if (current.current !== intent)
    throw new Error("Sesi berubah. Muat ulang data menggunakan sesi saat ini.");
```

Karena perbandingan dilakukan **sesudah** permintaan `POST /bed-occupancies/reservations` selesai,
server sudah menyimpan pemesanan ketika layar melempar galat. Pada jalur ini pesan tersebut
muncul **setiap kali** episode baru dibuat oleh tombol yang sama — sesuai kata "selalu" pada
laporan.

Efek samping lain dari salinan lama yang sama (SUDAH-VERIFIKASI di source, akibat visualnya
DUGAAN):

- `reserve` lama memanggil `refreshBoard` lama, yaitu pemuat denah **tanpa** `purpose`. Pemuat itu
  membatalkan permintaan denah `purpose=Admission` yang baru berjalan
  (`use-floor-plan.js:253`) — cocok dengan baris **(canceled)** pada L-1 — dan menimpa data denah
  dengan data tanpa kelayakan.
- Baris `let targetEpisodeId = bed.episodeId` (`inpatient-newborn-admission-steps.jsx:424`) tidak
  pernah terisi karena pengendali bed tidak mengembalikan `episodeId`
  (`use-inpatient-admission-bed.jsx:1035-1097`). Jalurnya selamat hanya karena `prepareBooking`
  sendiri memeriksa episode.

Setelah pesan muncul, klik ulang tidak menolong: boks sudah `Reserved` oleh episode bayi sendiri,
tidak masuk daftar tersedia, dan pemesanan kedua ditolak server (`409`) — layar lalu menampilkan
"Bed baru saja dipilih petugas lain" dan "Boks Bayi Belum Tersedia" (ISS-EPS-038-02). Bila petugas
memuat ulang, alamat halaman belum memuat `episodeId` (T2), sehingga hubungan dengan episode
pertama hilang.

Tidak tertutup kemungkinan pelapor melihat pesan "muat ulang" lain, misalnya "Bed baru saja
dipilih petugas lain. Denah dimuat ulang." atau "Tempat yang dipesan belum dapat diverifikasi.
Muat ulang penempatan." (`use-inpatient-newborn-admission.js:235`). Karena itu teks persisnya
ditanyakan pada P-01.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `PLAN-REPAIR-037` `FIX-EPS-037-03` menjanjikan
tombol yang "secara atomik mengeksekusi `prepareBooking` dan `reserve(episodeId)`" lalu maju ke
Persetujuan Wali.

**Dampak nyata.** Petugas menekan "Pesan Boks Bayi & Lanjut" untuk By. Ny. Sari. Server menahan
`301-BOKS` selama 15 menit untuk episode `RI-A`. Layar meminta muat ulang. Setelah muat ulang, layar
membuat episode `RI-B` (T2) dan mendapati `301-BOKS` "belum tersedia" karena dipegang `RI-A`.
Petugas terpaksa menunggu 15 menit atau memindahkan bayi ke Perinatologi.

**Rekomendasi.** Pisahkan "buat episode" dan "pesan boks" sehingga pemesanan selalu memakai
pengendali yang sudah mengenal episode baru, dan setelah kegagalan yang tidak pasti baca ulang
papan untuk mengadopsi pemesanan milik episode ini sebelum menampilkan galat. Rincian di
`FIX-EPS-038-04`.

---

## 5. Tanya-jawab pelapor

> **T:** Kenapa "Lanjut ke Pemesanan" harus diklik dua kali?
>
> **J:** Karena klik pertama membuat episode lalu menulis nomornya ke alamat halaman, dan layar
> admisi salah mengira itu admisi lama yang sedang dilanjutkan. Seluruh langkah disembunyikan
> sebentar lalu langkah 4 dipasang ulang dari nol, sehingga tanda "sudah masuk pemesanan" hilang.
> Klik kedua tidak membuat episode baru, jadi tidak memicu pemasangan ulang. Rincian di
> ISS-EPS-038-01.

> **T:** Kenapa boks rawat gabung sering tidak tersedia?
>
> **J:** Ada dua sebab yang salah dan satu sebab yang sah. Yang salah: layar mencari boks hanya di
> lantai denah yang terakhir dibuka di peramban itu, bukan di lantai kamar ibu; dan setelah pesan
> "muat ulang" muncul, boks sebenarnya sudah dipesan oleh episode bayi itu sendiri sehingga
> terbaca tidak tersedia. Yang sah: boks hanya dipasang di kamar VIP 301–305 (`PLAN-REPAIR-031`),
> jadi ibu di kamar lain memang tidak punya boks — tetapi layar tidak menjelaskannya. Rincian di
> ISS-EPS-038-02 dan ISS-EPS-038-03.

> **T:** Kenapa sering muncul perintah muat ulang?
>
> **J:** Paling mungkin karena pesan "Sesi berubah. Muat ulang data menggunakan sesi saat ini."
> yang muncul setiap kali tombol "Pesan Boks Bayi & Lanjut" membuat episode baru. Pemesanannya
> sebenarnya berhasil; yang keliru adalah pemeriksaan di layar. Teks persis yang Anda lihat perlu
> dikonfirmasi (P-01).

> **T:** Apakah penyebabnya session storage atau API?
>
> **J:** Bukan. Ketiga keluhan berasal dari pengelolaan state di layar (frontend); API backend
> menjawab dengan benar dan aturannya sudah tepat. Session storage tidak menyebabkan klik ganda,
> tetapi audit menemukan dua masalah lain di sekitarnya: draf sesi `newborn:active` yang terbawa
> ke admisi bayi berikutnya (T1), dan nomor episode rawat gabung yang tidak pernah tercatat di
> alamat halaman sehingga hilang saat muat ulang (T2).

---

## 6. Temuan tambahan

### ISS-EPS-038-T1 — Draf sesi `newborn:active` dipakai bersama semua admisi bayi

| | |
| --- | --- |
| **No. laporan** | 4 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-038-05` |
| **Kenapa dicantumkan** | Pelapor meminta session storage diperiksa |

**Apa yang terjadi.** Petugas memulai admisi bayi, memilih Ny. Sari di langkah 1, lalu pindah menu
lewat sidebar tanpa "Batal Pendaftaran". Esoknya (tab yang sama) petugas memulai admisi bayi untuk
ibu lain, dan langkah 1 sudah terisi Ny. Sari beserta rencana perawatannya.

**Kenapa terjadi.** Kunci draf sesi dibentuk dari `episodeId`, lalu `patientId`, dan bila keduanya
kosong memakai kata `active` — `use-inpatient-admission-flow.jsx:147-151`. Pada langkah 1–2 bayi
belum punya pasien, jadi semua admisi bayi memakai satu kunci
`quilvian:inpatient-admission:flow:newborn:active`. Efek rehidrasi
(`use-inpatient-admission-flow.jsx:186-220`) memulihkan `newbornDraft` dari kunci itu ketika
`patientType` masih kosong saat halaman dipasang. Kunci hanya dibersihkan oleh `resetEntryMode`
(`:240-261`, `:452-470`), bukan saat petugas keluar halaman. Selain itu, ketika `episodeId` masuk
ke URL, data ditulis ke kunci baru dan kunci `newborn:<patientId>` lama ditinggalkan.

**Dampak nyata.** Ibu yang salah dapat terpilih tanpa disadari. Langkah 2 menampilkan nama otomatis
"By. Ny. Sari" — petugas yang terburu-buru dapat menyimpan identitas bayi dengan hubungan ibu yang
salah.

**Rekomendasi.** Jangan pulihkan draf kunci `active`; hapus kunci lama ketika kunci berpindah.

### ISS-EPS-038-T2 — Episode rawat gabung tidak tercatat di alamat halaman; muat ulang membuat episode kedua

| | |
| --- | --- |
| **No. laporan** | 3, 4 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; terbentuknya episode ganda DUGAAN |
| **Perbaikan** | `FIX-EPS-038-01` |
| **Kenapa dicantumkan** | Akarnya bersambung dengan ISS-EPS-038-03 dan permintaan memeriksa penyimpanan |

**Kenapa terjadi.** Episode yang dibuat di langkah 4 hanya disimpan di state hook dokter
(`use-inpatient-admission-doctor.jsx:744`). Efek yang menulis `episodeId` ke URL sengaja melewati
langkah `bed-selection` (`inpatient-admission-view.jsx:677-688`). Pada rawat gabung, URL baru
ditulis bila pemesanan sukses (`inpatient-newborn-admission-steps.jsx:437-443`). Bila pemesanan
"gagal" (ISS-EPS-038-03) lalu halaman dimuat ulang, layar tidak tahu episode pertama dan
`doctor.submit` membuat kunjungan dan episode baru. Pemeriksaan kelayakan admisi hanya memberi
**peringatan** untuk `Draft` lain, bukan penolakan
(`InpEpisodeService.AdmissionEligibility.cs:116-122`).

**Cara membuktikan (DUGAAN).** Pelapor dapat menjalankan query baca berikut pada database
pengembangan:

```sql
SELECT "PatientId", COUNT(*) AS jumlah_draft, MIN("CreateDateTime") AS pertama, MAX("CreateDateTime") AS terakhir
FROM public."InpEpisode"
WHERE "EpisodeStatus" = 0 AND NOT "IsDelete" AND "DeliveryCaseId" IS NOT NULL
GROUP BY "PatientId"
HAVING COUNT(*) > 1;
```

Baris yang muncul adalah bayi dengan lebih dari satu episode `Draft`.

**Rekomendasi.** Tulis `episodeId` ke URL segera setelah episode dibuat pada langkah 4, untuk
kedua mode perawatan.

### ISS-EPS-038-T3 — Boks cadangan rawat gabung bisa berupa bed dewasa atau boks yang sedang dipakai

| | |
| --- | --- |
| **No. laporan** | 2, 5 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source dan kontrak |
| **Perbaikan** | `FIX-EPS-038-03` |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri butir 2 |

**Kenapa terjadi.** Pencarian boks punya dua cadangan yang longgar
(`inpatient-newborn-admission-steps.jsx:388-406`): bila tidak ada boks bayi, diambil **bed apa pun**
di kamar ibu; dari denah diambil **boks apa pun** walau statusnya bukan `Available`. Penyesuaian
kelayakan rawat gabung di layar juga tidak mensyaratkan `isForNewborn`
(`use-inpatient-admission-bed.jsx:211-249`). Server menolak bed dewasa untuk pasien bayi dengan
`NEWBORN_BED_REQUIRED` (`InpNewbornAdmissionRules.cs:88-89`), dan kontrak 022 invarian 8 hanya
mengizinkan bed bayi.

**Dampak nyata.** Kamar VIP 301 punya bed kedua untuk penunggu yang kosong. Layar menampilkan bed
itu sebagai "Boks Bayi Disediakan", lalu server menolak pemesanan.

### ISS-EPS-038-T4 — Klik kedua dengan bed lain memakai episode yang unit layanannya dari bed pertama

| | |
| --- | --- |
| **No. laporan** | 1, 5 |
| **Jenis** | `BUG` |
| **Area** | Frontend + Backend |
| **Keparahan** | Low |
| **Status bukti** | Kelas SUDAH-VERIFIKASI diselaraskan server; unit layanan DUGAAN |
| **Perbaikan** | `FIX-EPS-038-01` menghilangkan pemicunya; tidak ada perubahan tersendiri |
| **Kenapa dicantumkan** | Konsekuensi langsung ISS-EPS-038-01 |

**Kenapa terjadi.** Unit layanan dan kelas episode diambil dari bed pada klik pertama
(`use-inpatient-admission-doctor.jsx:636-642`, `:724-742`). Bila petugas mengganti bed setelah
layar kembali ke denah, `prepareBooking` mengembalikan episode yang sama tanpa memperbarui apa pun
(`use-inpatient-admission-bed.jsx:1028`). Kelas episode **diselaraskan server** saat pemesanan
("Draft mengikuti kamar yang benar-benar dipesan, termasuk setelah memilih ulang" —
`InpBedOccupancyService.cs:493-494`), tetapi unit layanan tidak disentuh di baris yang sama.
**Cara membuktikan:** bandingkan `InpEpisode.ServiceUnitId` dengan unit kamar bed pada
`InpBedReservation` aktif untuk episode yang sama setelah memilih ulang bed di unit lain. Karena
`FIX-EPS-038-01` menghapus pantulan balik ke denah, petugas tidak lagi terdorong mengganti bed,
sehingga temuan ini cukup dicatat dan diverifikasi ulang.

### ISS-EPS-038-T5 — Kandidat ibu hanya halaman pertama; setelah muat ulang kamar ibu dianggap tertutup

| | |
| --- | --- |
| **No. laporan** | 2, 5 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | DUGAAN |
| **Perbaikan** | `FIX-EPS-038-06` |
| **Kenapa dicantumkan** | Penyebab lain "kamar ibu tidak tersedia" pada rawat gabung |

**Kenapa terjadi.** `selectedCandidate` dicari dari daftar kandidat ibu yang dimuat (`pageSize: 50`,
halaman 1, urut terbaru) — `use-inpatient-newborn-admission.js:49-52`, `:77-112`; server
`InpEpisodeService.NewbornCandidates.cs:74-77`. Setelah muat ulang di langkah 4, bila ibu tidak
termasuk 50 kasus persalinan terbaru, `selectedCandidate` kosong dan rawat gabung dianggap "Kamar
Penempatan Ibu Closed" (`inpatient-newborn-admission-steps.jsx:375-383`). **Cara membuktikan:**
muat ulang langkah 4 untuk bayi dari kasus persalinan ke-51 atau lebih lama.

### ISS-EPS-038-T6 — Teks tetap dan isi pengganti yang tampak seperti data

| | |
| --- | --- |
| **No. laporan** | 5 |
| **Jenis** | `BUG` (isi pengganti), `DESIGN_CHANGE` (teks kartu) |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-038-07` (isi pengganti), `FIX-EPS-038-08` (teks kartu, menunggu K-03) |
| **Kenapa dicantumkan** | Ditemukan saat audit |

Kartu rawat gabung menulis "Kamar Ibu (VIP)" dan "Kamar privat ibu di Lantai 3"
(`inpatient-newborn-admission-constants.js:21`, `:23`). Teks ini **sesuai** konfigurasi saat ini
(`PLAN-REPAIR-031`: boks hanya di VIP 301–305 lantai 3), jadi mengubahnya adalah `DESIGN_CHANGE`;
risikonya, teks akan salah begitu master boks berubah. Yang `BUG` adalah isi pengganti: kelas ibu
diisi "VIP" bila kosong (`inpatient-newborn-admission-steps.jsx:411`), dan kartu konfirmasi
menampilkan "Kamar Penempatan Ibu", "Bed Ibu", "Kelas Perawatan Ibu" seolah-olah data (`:226-231`).
Ibu kelas 2 yang datanya belum terbaca tampil sebagai VIP.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Teks persis pesan yang meminta muat ulang, dan pada mode perawatan apa (Rawat Gabung, Perinatologi, NICU)? | Ada tiga pesan "muat ulang" pada alur ini | `FIX-EPS-038-04` tetap dikerjakan; bila teksnya lain, dibuka butir baru |
| P-02 | Ketika boks rawat gabung "tidak tersedia", apakah itu langsung saat langkah 4 dibuka, atau setelah pesan muat ulang? Kamar ibu di lantai berapa? | Membedakan sebab ISS-02 (lantai) dari akibat ISS-03 | Kedua sebab tetap ditutup `FIX-EPS-038-03`/`-04` |
| P-03 | Bila memungkinkan, jalankan query pada T2 dan kirim jumlah barisnya | Membuktikan episode `Draft` ganda | T2 tetap berstatus DUGAAN |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bila kamar ibu punya lebih dari satu boks kosong, siapa yang memilih? | Otomatis boks pertama / petugas memilih dari daftar | Otomatis bila tepat satu; tampilkan daftar pilihan bila lebih dari satu | Tidak — `FIX-EPS-038-03` memakai rekomendasi sampai diputuskan lain |
| K-02 | Episode `Draft` ganda dan yatim yang sudah terbentuk akibat bug ini — dibiarkan gugur sendiri atau dibatalkan petugas? | Gugur otomatis (`RWI-DEC-030`) / pembatalan manual dari daftar kerja / skrip data | Gugur otomatis ditambah pembatalan manual bila mengganggu; tanpa skrip data | Tidak |
| K-03 | Teks kartu "Rawat Gabung" di langkah 3 tetap menyebut "Kamar Ibu (VIP)" dan "Lantai 3"? | Tetap / teks umum "Kamar ibu yang memiliki boks bayi" | Teks umum, agar tidak salah ketika master boks bertambah | `FIX-EPS-038-08` |

---

## 8. Catatan pola

1. **Regresi beruntun pada langkah yang sama.** ISS-EPS-038-01 lahir dari `FIX-EPS-033` (state
   lokal `inBookingView`) yang bertemu gerbang pelanjutan `FE-RWI-032`. ISS-EPS-038-03 lahir dari
   `FIX-EPS-037-03` (dua operasi tulis dalam satu handler). Issue 016, 022, 028, 029, 030, 031,
   033, 036, dan 037 seluruhnya menyentuh langkah penempatan bayi.
2. **Tes unit membaca teks source, bukan perilaku.** `tests/unit/inpatient-newborn-admission.test.mjs:210-220`
   dan `:273-291` hanya mencocokkan pola teks seperti `setInBookingView(false)`. Tes semacam itu
   lulus 18/18 walau tombolnya harus diklik dua kali.
3. **Dua pola teknis yang perlu dihindari ke depan:**
   - menyimpan sub-tahap alur di state lokal komponen yang berada di bawah gerbang yang dapat
     melepasnya;
   - memanggil fungsi pengendali sesudah `await` yang mengubah identitas pengendali itu sendiri
     (episode baru → scope operasi baru).
4. **Usulan pencegahan.** Keputusan "tampilan mana yang aktif" dan "boks mana yang dipakai"
   dipindah ke fungsi murni di `src/utils` beserta tes perilaku; episode yang dibuat layar sendiri
   selalu langsung tercatat di URL; sub-tahap langkah dipegang satu pemilik di level view.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-23 | Issue dibuat dari laporan 3 keluhan + 2 permintaan, 1 lampiran; 3 temuan pelapor dan 6 temuan tambahan | `diagnose-module-issue` |
| 2026-10-23 | Pemilik menyetujui seluruh rencana beserta K-01, K-02, K-03 (rekomendasi). Status → `DALAM_PERBAIKAN`; perbaikan dikerjakan pada task `FIX-EPS-038` ([laporan](../../task/report/frontend/FIX-EPS-038.md)), menunggu uji layar S1–S10 | `build-module-frontend` |
