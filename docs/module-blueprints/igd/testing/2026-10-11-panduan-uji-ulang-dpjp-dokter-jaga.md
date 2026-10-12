# Panduan Uji Ulang DPJP Dokter Jaga IGD — `BE-IGD-056` dan `FE-IGD-037`

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 11 Oktober 2026 |
| Untuk | Agen penguji Antigravity yang dijalankan pemilik modul (Rizki) |
| Dasar | Pemeriksaan bukti putaran 11 Oktober 2026 (laporan [`BE-IGD-056` §8](../task/report/backend/BE-IGD-056.md), [`FE-IGD-037` §9](../task/report/frontend/FE-IGD-037.md)); keputusan `IGD-DEC-255`…`259` |
| Cakupan | Hanya acceptance yang belum terbukti. Skenario yang sudah lulus (`AT-IGD-214`, `215`, `217`, `218`, `220` a) **tidak** diulang |
| Jumlah skenario | 15 — 7 API (R-01…R-07), 6 layar (R-08…R-13), 2 roster kosong (R-14, R-15) |
| Backend | `rizkiG` `c7e523fd` + working tree `BE-IGD-056`, dijalankan pemilik dari build pemilik. Agen tidak menjalankan `dotnet build` |
| Frontend | `RizkiV2` `3335f4b53` + working tree `FE-IGD-037`, termasuk delta `IGD-DEC-256` (penanda override pada DPJP aktif). Wajib hasil build produksi dengan `.next/BUILD_ID` bertanggal 11 Oktober 2026 20.51 WIB atau lebih baru; server port 3000 dinyalakan pemilik dari build itu |
| Folder bukti | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-dpjp-dokter-jaga-<YYYY-MM-DD>/` |
| Laporan | `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-ulang-antigravity-dpjp-dokter-jaga.md` |

---

## 1. Aturan yang mengikat

Putaran sebelumnya diterima dengan penyimpangan (`IGD-DEC-255`). Aturan 6–9 di bawah menutup penyimpangan itu dan
diperiksa terhadap log backend.

| No | Aturan |
| ---: | --- |
| 1 | Layar dilayani hasil build produksi (`node .next/standalone/server.js`), bukan `next dev`. Setiap sesi layar mencatat `document.querySelector('nextjs-portal') === null` |
| 2 | Viewport 1440 × 900, menu samping terbuka. Tangkapan layar diambil minimal 1000 ms sesudah respons terakhir |
| 3 | Hanya akun perawat IGD dari `QUILVIAN_PERAWAT_EMAIL` dan `QUILVIAN_PERAWAT_PASSWORD`. Tanpa SuperAdmin, tanpa permintaan ke `/api/v1/administrator/**` |
| 4 | Sandi dan token tidak pernah ditulis di skrip, JSON, atau laporan. Header `Authorization` dan `Cookie` disamarkan |
| 5 | Tidak mengubah jadwal dokter, data master dokter, unit layanan, Akses Role, atau penugasan HR. **Tidak ada kueri basis data sama sekali, termasuk `SELECT`** — semua pemeriksaan lewat API |
| 6 | Penulisan yang boleh hanya: R-05 (dua `POST` yang harus ditolak), R-06, R-07, R-13 (`POST` yang harus ditolak), dan R-15. `POST`, `PUT`, `PATCH`, atau `DELETE` lain adalah pelanggaran |
| 7 | **Setiap** permintaan dicatat, termasuk persiapan, percobaan gagal, dan pengulangan. Tidak ada pemindaian `?at=` dan tidak ada probe endpoint di luar panduan ini |
| 8 | Setiap skrip yang dijalankan disimpan di folder bukti dan tidak ditimpa: `run-r08-p1.mjs`, `run-r08-p2.mjs`, dan seterusnya. Catatan jaringan merekam **semua** URL `/api/v1/`, bukan hanya satu path |
| 9 | Laporan memuat daftar semua percobaan dengan jam mulai dan selesai (WIB), dan jumlah permintaannya cocok dengan log backend. Nama pasien diambil dari layar atau respons, bukan ditebak |
| 10 | `page.route` hanya dipakai pada R-12 dan R-13, persis seperti tertulis; kode handlernya ikut tersimpan di skrip |
| 11 | Kegagalan skrip (selector, timeout) dibedakan dari kegagalan perilaku. Hasil ditulis `PASS`, `FAIL`, atau `NOT RUN` beserta alasannya. Jangan menulis "UAT lulus" |

---

## 2. Data uji

| Kode | Isi | Keadaan awal yang diharapkan |
| --- | --- | --- |
| D1 | `de72b03c-bf0c-46c2-a283-8d54de7e43f3` — dr. Farhan Malik | Dokter jaga IGD di dev |
| D2 | `8ec5dfff-ba70-4fdb-b0c4-f72aeae461ab` — dr. Ranger Biru | Umum, di luar jadwal jaga |
| D3 | `55733b7f-e35c-4c7c-87df-b18ae138354b` — dr. Maya Permata Sari | Tidak ditawarkan pemilih |
| V1 | Kunjungan `792039a6-b8de-4918-b61d-53e15566471c`, encounter `7b4a5e54-d9ec-49b2-8249-eca1769588cb`, `IGD-261003021124-4C83AE` | DPJP aktif D2, `isRosterOverride=true`, alasan *"Dua dokter jaga sedang resusitasi"*; dua baris riwayat |
| V2 | Kunjungan `b0aa0950-2b65-47d5-b738-f6126f023e22`, encounter `e61307aa-cfb8-4292-aca6-1068c1b1dffe`, `IGD-261001033716-45CD48` | DPJP aktif D2, `isRosterOverride=true`; satu baris riwayat |
| V3 | Kunjungan `63f12a1a-4a67-460b-8880-0a863937e87c`, encounter `8051077d-162b-4ba0-9e3e-24c44aa2896e`, `IGD-261001034358-B98C47` | Tanpa DPJP; nol baris riwayat |

Base URL API: `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-doctor-assignments`.

**P0 — Pemeriksaan awal.** `GET …?emergencyVisitId=` untuk V1, V2, dan V3, lalu cocokkan dengan kolom terakhir. Bila
berbeda, hentikan putaran dan laporkan keadaan yang ditemukan.

URL layar:

- Pintu A (panel riwayat triage): `http://localhost:3000/health-services/emergency-installation-management/emergency-triage/{encounterId}`
- Pintu B (Ruang Kerja Pemeriksaan IGD): `http://localhost:3000/health-services/emergency-installation-management/emergency-assessment/{emergencyVisitId}`

---

## 3. Skenario

Jalankan **berurutan**. R-09 wajib di antara R-06 dan R-07. R-11, R-12, dan R-13 wajib sebelum R-15, karena ketiganya
butuh V3 tanpa DPJP.

### 3.1 API

| ID | Langkah | Harapan | Acceptance |
| --- | --- | --- | --- |
| R-01 | `GET …/on-duty-doctors?onlyOnDuty=true` | `200`; setiap baris `isOnDuty=true`; tidak ada baris `isOnDuty=false` | BE 2 |
| R-02 | `GET …/on-duty-doctors?search=Ranger`, lalu `?search=Farhan`, lalu `?search=Maya` | *Ranger*: D2 ada (`isOnDuty=false`), D1 tidak ada. *Farhan*: D1 ada (`isOnDuty=true`), tanpa baris `isOnDuty=false`. *Maya*: D3 tidak ada | BE 2 |
| R-03 | `GET …/active?emergencyVisitId={V1}` | `200`; `doctorId` = D2, `isRosterOverride=true`, `rosterOverrideReason` = *"Dua dokter jaga sedang resusitasi"* | BE 6 |
| R-04 | Cari satu kunjungan yang punya baris penugasan dengan `effectiveFrom` sebelum `2026-10-11T00:00:00Z`: `GET …/emergency-visits` (halaman pertama), lalu `GET …?emergencyVisitId=` untuk paling banyak 20 kunjungan. Catat setiap kunjungan yang diperiksa | Baris lama itu `isRosterOverride=false` dan `rosterOverrideReason=null`. Bila tidak ditemukan dalam 20 kunjungan: `NOT RUN` beserta daftar kunjungannya | BE 6 |
| R-05 | (a) `POST …` body `{emergencyVisitId: V1, doctorId: D2}` tanpa `rosterOverrideReason`. (b) `POST …` body `{emergencyVisitId: V1, doctorId: D1}` | (a) `400`, pesan persis *"Dokter ini tidak sedang jaga IGD pada waktu penugasan. Isi alasan memilih dokter di luar jadwal jaga."* — **bukan** `409`. (b) `409`, pesan persis *"Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter."*. Riwayat V1 tetap dua baris | BE 4 (urutan) |
| R-06 | `POST …/{id penugasan aktif V2}/handover` body `{doctorId: D1, assignmentReason: "Uji aturan 11", rosterOverrideReason: "  alasan ini harus diabaikan  "}` | `200`; baris baru `doctorId` D1, `isRosterOverride=false`, `rosterOverrideReason=null`, `assignmentReason` *"Uji aturan 11"* | BE 4 (aturan 11) |
| R-07 | **Sesudah R-09.** `POST …/{id penugasan aktif V2 dari R-06}/handover` body `{doctorId: D2, assignmentReason: "Uji pemangkasan server", rosterOverrideReason: "   Uji pemangkasan alasan di server   "}`. Wajib lewat API, bukan layar, karena layar sudah memangkas alasan sebelum dikirim | `200`; `isRosterOverride=true`; `rosterOverrideReason` persis *"Uji pemangkasan alasan di server"* tanpa spasi di ujung | BE 5 |

### 3.2 Layar

| ID | Langkah | Harapan | Acceptance |
| --- | --- | --- | --- |
| R-08 | Buka pintu A untuk V1, lalu pintu B untuk V1. Tangkap layar banner DPJP (pintu A) dan kartu *Informasi Pasien* (pintu B) | Pintu A: di bawah *Bertugas sejak … • Ditugaskan oleh …* ada baris persis *"Di luar jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"*. Pintu B: ruas *Dokter PJ IGD* berisi *"dr. Ranger Biru • Di luar jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"* | FE 6; `AT-IGD-216` layar |
| R-09 | **Sesudah R-06, sebelum R-07.** Buka pintu A dan pintu B untuk V2 | Pintu A: banner dr. Farhan Malik **tanpa** baris *Di luar jadwal jaga*. Pintu B: ruas *Dokter PJ IGD* hanya *"dr. Farhan Malik"* | FE 6; `AT-IGD-215` layar |
| R-10 | **Sesudah R-07.** Buka pintu A untuk V2 | Baris persis *"Di luar jadwal jaga — alasan: Uji pemangkasan alasan di server"* | FE 6 |
| R-11 | Pintu A untuk V3, tombol *Tetapkan Dokter Sekarang*. Pilih D2. Tangkap layar modal saat isian alasan masih kosong. Ketik 501 huruf `A` di isian *Alasan Memilih Dokter di Luar Jadwal Jaga*. Catat panjang nilai dan atribut `maxlength`. Tangkap layar lagi, lalu tekan *Batal* | Isian alasan tampil; saat kosong tombol simpan nonaktif dan pesan *"Alasan memilih dokter di luar jadwal jaga wajib diisi."* tampil; panjang nilai 500, `maxlength="500"`. Nol `POST` | FE 4 |
| R-12 | Sebelum membuka pintu A untuk V3, pasang `page.route('**/emergency-doctor-assignments/on-duty-doctors**', (route) => route.fulfill({ status: 500, contentType: 'application/json', body: JSON.stringify({ success: false, statusCode: 500, message: 'Uji: daftar dokter gagal dimuat', data: null }) }))`. Buka modal *Tetapkan Dokter Sekarang* dan tangkap layar | Modal menampilkan pesan galat (*"Uji: daftar dokter gagal dimuat"* atau *"Gagal mengambil daftar dokter jaga IGD."*); tidak ada dokter yang dapat dipilih; catatan jaringan memuat nol permintaan ke `/corporate/human-resource/master-data/doctors` | FE 1 |
| R-13 | Tanpa intersepsi R-12. Pintu A untuk V3, buka modal, pilih D2, isi alasan *"Uji penolakan 400"*. Sebelum menekan simpan, pasang `page.route('**/emergency-doctor-assignments', (route) => { const r = route.request(); if (r.method() !== 'POST') return route.continue(); const body = JSON.parse(r.postData()); return route.continue({ postData: JSON.stringify({ ...body, doctorId: '55733b7f-e35c-4c7c-87df-b18ae138354b' }) }); })`. Tekan simpan, tunggu respons, tangkap layar, lalu `GET` riwayat V3 lewat API | Backend `400` dengan pesan persis *"Dokter ini tidak dapat dipilih sebagai dokter penanggung jawab IGD. Pilih dokter jaga IGD, atau dokter umum/emergency dengan alasan."*; modal tetap terbuka dan menampilkan pesan itu apa adanya; sesudah `400` ada satu `GET …/on-duty-doctors` baru (daftar dimuat ulang); riwayat V3 tetap nol baris | FE 5 |

### 3.3 Roster kosong — `AT-IGD-219`

Hanya dijalankan pada jendela tanpa jadwal IGD yang disiapkan pemilik (`IGD-DEC-257`). Agen tidak mengubah jadwal. Catat
jam mulai dan selesai jendela dari pemilik.

| ID | Langkah | Harapan | Acceptance |
| --- | --- | --- | --- |
| R-14 | Di dalam jendela: `GET …/on-duty-doctors` tanpa `at` | `200`; nol baris `isOnDuty=true`; kelompok di luar jadwal tidak kosong; tidak ada `displayLabel` yang memuat *"Dokter Jaga IGD"*. Bila masih ada baris `isOnDuty=true`: R-14 dan R-15 `NOT RUN` beserta jamnya | BE 1; `AT-IGD-219` |
| R-15 | Di dalam jendela: pintu A untuk V3, buka modal. Tangkap layar. Pilih D2, isi alasan *"Uji roster kosong"*, simpan. Tangkap layar banner | Sebelum simpan: keterangan persis *"Tidak ada dokter berjadwal jaga IGD pada jam ini. Pilih dokter di luar jadwal jaga dan isi alasannya."*; hanya judul *Di luar jadwal jaga — wajib alasan* yang tampil. Sesudah simpan: `POST` `201`, `isRosterOverride=true`; banner menampilkan *"Di luar jadwal jaga — alasan: Uji roster kosong"* | FE 7; `AT-IGD-219` |

---

## 4. Bukti dan laporan

| Berkas | Isi |
| --- | --- |
| `data-uji.json` | Hasil P0, jam jendela R-14/R-15 dari pemilik, nama pasien V1–V3 seperti tampil di layar |
| `R-01-api.json` … `R-07-api.json`, `R-14-api.json` | Request (header disamarkan) dan response utuh; `pemeriksaan` per harapan; `hasil` |
| `R-08-*.png` … `R-15-*.png` | Satu PNG per titik tangkap layar yang disebut di skenario |
| `R-08-*-network.json` … `R-15-*-network.json` | Semua permintaan `/api/v1/` sesi layar itu, termasuk status dan body response |
| `run-*.mjs` | Setiap versi skrip yang dijalankan (aturan 8) |
| `percobaan.json` | Daftar seluruh percobaan: skrip, jam mulai dan selesai WIB, jumlah permintaan, hasil, dan alasan bila diulang |

Laporan memuat tabel hasil R-01…R-15, daftar percobaan dari `percobaan.json`, dan pernyataan aturan 1–11. Keputusan
akhir per acceptance diambil pengembang dari bukti mentah dan log backend.

---

## 5. Tambahan 11 Oktober 2026 malam — R-16, roster kosong dengan daftar tiruan (`IGD-DEC-263`)

R-01…R-13 sudah diputus (laporan `BE-IGD-056` §9, `FE-IGD-037` §11) dan **tidak diulang**. R-14 dan R-15 diganti R-16:
layar diberi daftar dokter yang disaring sehingga hanya dokter di luar jadwal yang sampai, tanpa mengubah jadwal.
Aturan 1–11 bagian 1 tetap berlaku. `page.route` hanya untuk R-16, persis seperti tertulis di bawah.

| Butir | Nilai |
| --- | --- |
| Folder bukti | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-r16-dpjp-<YYYY-MM-DD>/` |
| Laporan | `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-r16-dpjp-dokter-jaga.md` |
| Kunjungan | V3 — `63f12a1a-4a67-460b-8880-0a863937e87c`, encounter `8051077d-162b-4ba0-9e3e-24c44aa2896e`; pasien di layar *ANISA PRAMESTI* |
| Penulisan yang boleh | Tepat satu `POST …/emergency-doctor-assignments` untuk V3 |

**P0.** `GET …?emergencyVisitId={V3}` harus nol baris. Bila tidak, hentikan dan laporkan.

**R-16.** Sebelum membuka pintu A untuk V3, pasang:

```js
await page.route('**/emergency-doctor-assignments/on-duty-doctors**', async (route) => {
  const response = await route.fetch();
  const json = await response.json();
  json.data = (json.data || []).filter((d) => d.isOnDuty === false);
  return route.fulfill({ response, json });
});
```

| Langkah | Harapan | Bukti |
| --- | --- | --- |
| 1. Buka pintu A untuk V3, tekan *Tetapkan Dokter Sekarang*, buka pemilih dokter. Tangkap layar | Keterangan persis *"Tidak ada dokter berjadwal jaga IGD pada jam ini. Pilih dokter di luar jadwal jaga dan isi alasannya."*; judul *Di luar jadwal jaga — wajib alasan* tampil; judul *Dokter jaga IGD saat ini* **tidak** tampil; tidak ada label opsi yang memuat *"Dokter Jaga IGD"* | `R-16-kosong.png`; teks label opsi di JSON |
| 2. Pilih D2, isi alasan *"Uji roster kosong"*, tekan *Tetapkan Dokter*. **Tunggu respons `POST` dengan `page.waitForResponse`** sebelum langkah berikutnya; jangan menutup browser sebelum respons datang | `POST` membawa `rosterOverrideReason` *"Uji roster kosong"*; respons `201`, `isRosterOverride=true` | `R-16-network.json` |
| 3. Tunggu 1500 ms, tangkap layar banner DPJP | Banner menampilkan dr. Ranger Biru dan baris persis *"Di luar jadwal jaga — alasan: Uji roster kosong"* | `R-16-banner.png` |
| 4. `GET …?emergencyVisitId={V3}` lewat API | Satu baris D2, `isRosterOverride=true`, `rosterOverrideReason` *"Uji roster kosong"* | `R-16-api.json` |

Bila percobaan gagal sebelum `POST`, ulangi dengan skrip versi baru dan catat di `percobaan.json`. Bila `POST` sudah
terkirim, **jangan** mengirim ulang sebelum P0 diulang dan V3 terbukti masih nol baris.
