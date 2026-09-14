# Laporan Perubahan Frontend — `FE-RAD-13`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-13` |
| Judul | Hasil bacaan di rekam medis |
| Epic | `EPIC RAD-04`, `EPIC RAD-07` |
| Requirement | `FR-RAD-030`, `FR-RAD-031`, `FR-RAD-032` (penomoran blueprint) |
| Decision | `RAD-DEC-006` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 5, gelombang `MVP-4` |
| Contract version | `RAD-INT-001` bagian 2; `RAD-API-001` endpoint `GET /rad-reports/by-encounter/{encounterId}` |
| Acceptance criteria | `AC-19`, `AC-20` |
| Test yang diminta roadmap | `UAT-08` gangguan menampilkan pesan bukan daftar kosong; `UAT-09` koreksi langsung terlihat |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 5 butir 2, 3, dan 4 |
| Dependency | `FE-RAD-12` **selesai**; `BE-RAD-11` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini dan baris status roadmap |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-14 |
| Status | **Selesai.** 16 unit test baru lulus; 920 unit test repository lulus, 0 gagal. **Task terakhir roadmap frontend Radiologi** |

---

## 1. Risiko tertinggi roadmap, dan apa yang dikerjakan untuknya

Roadmap menandai task ini risiko tertinggi di seluruh roadmap frontend, dengan satu kalimat
sebagai sebabnya: *daftar kosong terbaca "pasien tidak punya pemeriksaan"*.

`resolveEncounterState` membedakan **empat** keadaan, dan urutan pemeriksaannya dibuat eksplisit
karena di situlah letak bahayanya:

| Urutan | Keadaan | Artinya bagi dokter |
| ---: | --- | --- |
| 1 | `loading` | Belum ada yang dapat disimpulkan |
| 2 | `error` | **"Kami tidak tahu"** — diperiksa **sebelum** kekosongan |
| 3 | `error` (belum pernah berhasil dimuat) | Juga bukan kekosongan |
| 4 | `empty` | **"Kami tahu, dan jawabannya tidak ada"** |

Dua di antaranya sama-sama menghasilkan layar tanpa baris, tetapi **artinya berlawanan**.
Permintaan yang gagal tidak pernah jatuh menjadi "belum ada hasil" — sekalipun daftarnya
kebetulan kosong, dan sekalipun masih ada baris sisa dari pemuatan sebelumnya. `S1` sampai `S6`
menguncinya.

Pesan gangguannya tidak berhenti pada "coba lagi". Ia menyatakan apa yang **tidak** boleh
disimpulkan: *"Ini BUKAN berarti pasien tidak punya pemeriksaan radiologi — jangan mengambil
kesimpulan dari layar ini sampai hasilnya benar-benar termuat."* Kalimat itu yang sebenarnya
mencegah bahayanya; sisanya hanya sopan santun.

---

## 2. Jebakan yang paling berbahaya, dan mengapa ia halus

`GET /by-encounter` sudah menyaring bacaan yang belum dirilis di backend, memakai dua penyaring
yang saling menguatkan. Tetapi balasannya **tidak membawa isi bacaan sama sekali** — lihat
bagian 3 — sehingga isinya harus diambil lewat `GET /rad-reports/{id}`.

**Dan `GET /{id}` tidak menyaring apa pun.** Ia mengembalikan seluruh versi, **termasuk draf
koreksi yang sedang disusun beserta isinya**. Lebih halus lagi: `CreateAmendmentAsync` sengaja
**tidak** menaikkan `currentVersionNumber` selama koreksi belum dirilis, sehingga **versi
bernomor tertinggi bisa jadi draf yang belum diperiksa siapa pun**.

Layar yang menampilkan "versi terbaru", atau yang merender `versions[]`, akan menyodorkan draf
itu kepada dokter pengirim sebagai hasil — melanggar butir 2 dan butir 3 sekaligus, dan persis
menghasilkan keputusan pengobatan atas bacaan yang belum disahkan siapa pun.

**Yang dikerjakan:** `pickDisplayVersion` mengembalikan **hanya `currentVersion`**. Tidak pernah
nomor tertinggi, tidak pernah hasil penelusuran `versions[]`. `S7` dan `S8` menguncinya — `S8`
secara harfiah memeriksa bahwa teks draf tidak muncul di mana pun pada hasil yang ditampilkan.

**Lapis kedua:** `isSafeToDisplay` menolak versi yang statusnya bukan `Released`, sengaja
menduplikasi penyaring backend. Duplikasi pemeriksaan keselamatan ini disengaja, dengan alasan
yang sama seperti pada gerbang keselamatan `FE-RAD-08`: kedua kekeliruan tidak setara. Hasil sah
yang tertahan berakhir sebagai keluhan; draf yang lolos berakhir sebagai keputusan pengobatan.
`S9` menguncinya untuk ketiga status yang tidak aman.

---

## 3. Temuan: `by-encounter` tidak membawa isi bacaan

`MapList` tidak menyertakan `Findings`, `Impression`, maupun `Recommendation`. Yang dibawa hanya
ringkasan: nomor bacaan, nomor pemeriksaan, keadaan, nomor versi, penulis, pengesah, dan waktu
perilisan.

Artinya seorang dokter yang membuka rekam medis melihat **daftar hasil, bukan hasilnya**.
Kesimpulan bacaan — bagian yang benar-benar ia butuhkan — tidak ada di balasan itu.

**Yang dikerjakan.** Isi diambil per bacaan lewat `GET /{id}` ketika dokter benar-benar
membukanya, bukan seluruhnya di muka. Dua alasan yang berdiri sendiri: mengambil seluruhnya
berarti seluruh riwayat klinis pasien tersaji begitu tab dibuka, dan sebagian besar di antaranya
tidak akan dibaca.

**Yang tidak dikerjakan, dan mengapa.** Menggabungkan isi ke dalam balasan `by-encounter` akan
menyelesaikannya dalam satu permintaan — tetapi itu perubahan backend, dan ia juga berarti
setiap pembukaan rekam medis mengirim seluruh isi bacaan pasien ke browser tanpa ada yang
memintanya. **Perlu keputusan pemilik modul** apakah kenyamanan itu sepadan.

---

## 4. Dibaca langsung, tanpa salinan — `RAD-INT-001` bagian 2

Kontrak melarang Clinical Management menyimpan isi bacaan dalam bentuk apa pun, termasuk
"cache" versi lama. Tiga larangan itu dipatuhi berlapis:

| Larangan | Yang dikerjakan |
| --- | --- |
| Menyimpan isi bacaan di modul lain | Tidak ada tabel, tidak ada Redux, tidak ada penyimpanan browser |
| Menyimpan salinan versi lama sebagai cache | Permintaan dikirim **setiap kali layar dibuka**; tidak ada potongan Redux untuk hasil ini |
| Menampilkan daftar kosong seolah pasien tidak punya hasil | Lihat bagian 1 |

Skenario yang dicegah ditulis kontraknya sendiri, dan itulah `UAT-09`: dr. Andi membaca hasil
pukul 08.00, dr. Sinta merilis koreksi pukul 09.00, dr. Andi membuka lagi pukul 10.00. Karena
dibaca langsung, yang terbaca adalah versi koreksi — tanpa langkah penyalinan apa pun. `S11`
menguncinya, termasuk bahwa **alasan koreksi ikut terbaca** di layar pembaca, bukan hanya di
layar radiologi.

Berpindah pasien mengosongkan seluruh isi yang sedang terbuka: isi bacaan pasien sebelumnya
tidak boleh tertinggal di layar maupun di state. Tiga berkas baru ditambahkan ke penjaga
penyimpanan, sehingga **sebelas berkas bacaan** kini dijaga pada level source.

---

## 5. Tempat layar ini dipasang

Ditempatkan sebagai **tab tersendiri** pada workspace konsultasi dokter — `radiologyResult`,
berlabel "Hasil Radiologi".

**Bukan** di `supportingExam` yang sudah ada. Tab itu berbunyi *"Order lab, radiologi, dan
pemeriksaan penunjang lain"* — ia mengurus **pemesanan** penunjang, sedangkan task ini mengurus
**pembacaan hasilnya**. Keduanya dikerjakan pada saat yang berbeda oleh orang yang berbeda, dan
menumpangkan hasil bacaan pada slot pemesanan juga akan menyerobot tempat Laboratorium yang
belum mengisinya.

**Bukan** pula di `medical-record-management`. Ditelusuri lebih dulu: `medical-records/[slug]`
beserta `medical-record-timeline-list.jsx` dan `medical-record-document-panel.jsx` **tidak
memiliki `encounterId`** sama sekali, sedangkan endpoint ini beralamat pada kunjungan.
Memaksakan integrasi ke layar yang tidak punya konteks kunjungan hanya akan menghasilkan
penelusuran identitas yang menebak-nebak. **Penyajian pada rekam medis karena itu belum
dikerjakan, dan itu perlu diketahui** — lihat bagian 8.

`PatientClinicalDocumentSource.Radiology` **tidak disentuh**, sesuai catatan kontrak: slot itu
hanya untuk berkas unggahan dari luar, bukan untuk hasil bacaan yang lahir di modul ini.

---

## 6. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/hooks/health-services/radiology-management/rad-encounter-report-rules.js` | Baru — fungsi murni; pembeda empat keadaan, pemilih versi berlaku, penjaga versi aman |
| `src/lib/hooks/health-services/radiology-management/use-rad-encounter-reports.jsx` | Baru — controller; baca langsung, isi diambil saat dibuka |
| `src/components/view/…/rad-reports/rad-encounter-report-panel.jsx` | Baru — panel hasil bacaan |
| `tests/unit/rad-encounter-report-rules.test.mjs` | Baru — 16 test |
| `src/lib/constants/…/rad-report-constants.jsx` | Diubah — `RAD_ENCOUNTER_REPORT_COPY` |
| `src/lib/constants/…/doctor-queue/doctor-queue.constants.js` | Diubah — satu entri tab `radiologyResult` |
| `src/components/view/…/doctor-queues/doctor-queue-view.jsx` | Diubah — merender panel pada tab itu |
| `tests/unit/rad-report-storage-guard.test.mjs` | Diubah — tiga berkas baru masuk daftar yang dijaga |

Tidak ada route baru, tidak ada potongan Redux baru, tidak ada base component baru, dan tidak
ada pemanggilan Axios baru: `getRadReportsByEncounter` dan `getRadReportById` sudah ada sejak
`FE-RAD-01`.

---

## 7. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-encounter-report-rules.test.mjs` | **16 lulus, 0 gagal** |
| `node --test tests/unit/rad-report-storage-guard.test.mjs` | **5 lulus** — kini menjaga sebelas berkas bacaan |
| `node --test tests/unit/` (seluruh repository) | **920 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 3 warning** — seluruhnya `react-hooks/set-state-in-effect` yang sudah ada sebelumnya. **Tidak ada warning baru** |
| `npm run build` | **Compiled successfully in 35.4s.** Route `/health-services/registration-management/doctor-queues` terdaftar |

Cakupan test: `AC-20` dan `UAT-08` pada `S1` sampai `S6`; ketentuan mengikat butir 2 dan 3 pada
`S7` sampai `S10`; `UAT-09` dan `AC-19` pada `S11`; penjaga tambahan pada `S13`.

**`MANUAL TEST: NOT FEASIBLE`**, dan rantai sebabnya seluruhnya di luar frontend:
`GET /by-encounter` hanya mengembalikan bacaan yang **sudah dirilis** → perilisan menuntut
pengesahan → pengesahan menuntut `RadReport : ActAsRadiologist` yang **tidak dapat diberikan
kepada peran mana pun** → dan bacaan itu sendiri baru lahir setelah study mencapai
`QualityAccepted`, yang mustahil selama belum ada aturan keselamatan `Active` (`RAD-OPEN-011`).

**Layar ini karena itu akan selalu menampilkan keadaan kosong saat ini** — dan justru itu yang
membuat pembedaan gangguan versus kekosongan pada bagian 1 menjadi penting sejak hari pertama,
bukan nanti.

---

## 8. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Penyajian pada layar `medical-record-management` | Layar itu tidak punya `encounterId`, sedangkan endpointnya beralamat pada kunjungan. Lihat bagian 5 — **perlu keputusan pemilik modul** |
| Penyajian di dalam tab CPPT | Berkasnya 1.256 baris; tab tersendiri lebih bersih dan tidak menyentuh kode yang tidak perlu disentuh |
| Mengambil isi seluruh bacaan di muka | Seluruh riwayat klinis pasien tersaji begitu tab dibuka, dan sebagian besar tidak akan dibaca |
| Menyimpan hasil di Redux | Dilarang tegas `RAD-INT-001` bagian 2 |
| Menyentuh `PatientClinicalDocumentSource.Radiology` | Slot itu untuk berkas unggahan dari luar, bukan hasil bacaan modul ini |
| Base component baru | Dilarang `AGENTS.md` |

---

## 9. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **Penyajian pada rekam medis belum ada** | Bagian 5 dan 8. Yang dikerjakan baru workspace dokter. **Perlu keputusan pemilik modul** |
| **`by-encounter` tanpa isi bacaan** | Temuan bagian 3. **Perlu keputusan pemilik modul** |
| **`RadReport : ActAsRadiologist` tidak dapat diberikan** | Temuan `FE-RAD-11`. Tidak akan ada bacaan dirilis, sehingga layar ini selalu kosong |
| **`EnsurePendingReportAsync` tanpa pemanggil** | Temuan `FE-RAD-10`, masih terbuka |
| **Empat transisi study tanpa endpoint** | Temuan `FE-RAD-09`, masih terbuka |
| **Tiga enum tidak terbit pada metadata study** | Temuan `FE-RAD-09`, masih terbuka |
| **Study terkunci saat aturan keselamatan berubah** | Temuan `FE-RAD-08`, masih terbuka |
| **Kontrak `409` vs jalur API `422`** | Temuan `FE-RAD-08`, masih terbuka |
| **`HoldAsync` menerima status terminal** | Temuan `FE-RAD-06`, masih terbuka |
| **Daftar kerja tanpa identitas pasien** | Temuan `FE-RAD-07`, masih terbuka |
| **Tidak ada aturan keselamatan `Active`** | `RAD-OPEN-011` terbuka. Menahan seluruh gelombang |

---

## 10. Keadaan akhir roadmap frontend Radiologi

**Ketiga belas task selesai.** `FE-RAD-01` sampai `FE-RAD-13`, seluruhnya terlaporkan.

| Gelombang | Task | Keadaan |
| --- | --- | --- |
| `MVP-1` | `FE-RAD-01` – `FE-RAD-04` | Selesai 2026-09-11 |
| `MVP-3` | `FE-RAD-05` – `FE-RAD-12` | Selesai 2026-09-11 s/d 2026-09-14; `FE-RAD-09` dan `FE-RAD-10` **selesai sebagian** karena penghalang backend |
| `MVP-4` | `FE-RAD-13` | Selesai 2026-09-14 |

**920 unit test lulus, 0 gagal. Lint 0 error. Build lulus.**

**Yang menahan modul dipakai bukan lagi frontend.** Tiga penghalang backend bertumpuk, dan
ketiganya di luar kewenangan task frontend mana pun:

1. **`RAD-OPEN-011`** — belum ada aturan keselamatan `Active`, sehingga tidak satu pun
   pemeriksaan melewati gerbang keselamatan.
2. **`RadReport : ActAsRadiologist` tidak dapat diberikan** — sehingga tidak satu pun bacaan
   dapat disahkan maupun dirilis.
3. **`EnsurePendingReportAsync` tanpa pemanggil** — sehingga daftar bacaan menunggu tidak pernah
   terisi.

Selama ketiganya terbuka, **seluruh alur MVP-3 sudah terbangun tetapi belum dapat dijalankan
untuk satu pasien pun**. Layarnya ada, jalurnya lengkap, dan penolakannya benar — yang belum ada
adalah konfigurasi yang membuat pasien pertama dapat melewatinya.

---

## 11. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 4 berkas baru, 4 diubah. 16 test baru lulus; 920 test repository lulus; lint 0 error tanpa warning baru; build lulus. Ketentuan mengikat butir 4 dipenuhi dengan empat keadaan yang urutan pemeriksaannya eksplisit — kegagalan diperiksa sebelum kekosongan. Butir 2 dan 3 dipenuhi dengan menampilkan **hanya `currentVersion`** dan menolak versi yang belum `Released`, karena `GET /{id}` membawa draf koreksi beserta isinya sementara `currentVersionNumber` sengaja tidak dinaikkan. Dua temuan: `by-encounter` tidak membawa isi bacaan, dan layar rekam medis tidak punya `encounterId` sehingga penyajian di sana belum dikerjakan. **Task terakhir roadmap frontend Radiologi.** | `draft` |
