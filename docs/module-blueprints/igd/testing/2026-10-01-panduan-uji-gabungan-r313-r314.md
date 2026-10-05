# Panduan Uji Gabungan IGD — R3.13 dan R3.14

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 1 Oktober 2026 |
| Untuk | Pemilik modul (Rizki) dan agen penguji |
| Cakupan | 11 task berstatus 🟡: 6 backend (uji API) dan 5 frontend (uji layar) |
| Jumlah skenario | 82 — 40 uji API, 42 uji layar |
| Source yang diuji | Backend `rizkiG` `b9076c71` + working tree; frontend `RizkiV2` `aa0b1168b` + working tree — belum di-commit |
| Sumber skenario | Laporan task di `docs/module-blueprints/igd/task/report/`. Bila berkas ini dan laporan task berbeda, **laporan task yang berlaku** |
| Status | Panduan. Belum ada skenario yang dijalankan |

Berkas ini hanya mengumpulkan skenario dari sebelas laporan task supaya dapat dijalankan dalam satu putaran. Hasil uji
**tidak** ditulis di sini — lihat bagian 8.

---

## 1. Ringkasan cakupan

| Tahap | Task | Jenis | Skenario | Jumlah | Laporan asal |
| ---: | --- | --- | --- | ---: | --- |
| 1 | `BE-IGD-055` (delta `IGD-DEC-179`) | API | D1–D6 | 6 | [BE-IGD-055](../task/report/backend/BE-IGD-055.md) bagian 8.6 |
| 1 | `BE-IGD-057` | API | S8 ulang | 1 | [BE-IGD-057](../task/report/backend/BE-IGD-057.md) bagian 5.3 |
| 1 | `BE-IGD-059` | API | S1–S8 | 8 | [BE-IGD-059](../task/report/backend/BE-IGD-059.md) bagian 5.1 |
| 2 | `BE-IGD-061` | API | S1–S12 | 12 | [BE-IGD-061](../task/report/backend/BE-IGD-061.md) bagian 5.1 |
| 2 | `BE-IGD-063` | API | S1–S8 | 8 | [BE-IGD-063](../task/report/backend/BE-IGD-063.md) bagian 5.1 |
| 2 | `BE-IGD-062` | API | S1–S5 | 5 | [BE-IGD-062](../task/report/backend/BE-IGD-062.md) bagian 5.1 |
| 3 | `FE-IGD-036` | Layar | U1–U14 | 14 | [FE-IGD-036](../task/report/frontend/FE-IGD-036.md) bagian 6.1 |
| 3 | `FE-IGD-039` | Layar | U1–U6 | 6 | [FE-IGD-039](../task/report/frontend/FE-IGD-039.md) bagian 6.1 |
| 3 | `FE-IGD-040` | Layar | U1–U7 | 7 | [FE-IGD-040](../task/report/frontend/FE-IGD-040.md) bagian 6.1 |
| 4 | `FE-IGD-042` | Layar | U1–U8 | 8 | [FE-IGD-042](../task/report/frontend/FE-IGD-042.md) bagian 6.1 |
| 4 | `FE-IGD-041` | Layar | U1–U7 | 7 | [FE-IGD-041](../task/report/frontend/FE-IGD-041.md) bagian 6.1 |

Pada berkas ini tiap skenario diberi awalan nomor task supaya tidak tertukar: `055-D1`, `061-S7`, `036-U10`.

---

## 2. Aturan uji

Aturan ini mengikat siapa pun yang menjalankan uji, termasuk agen penguji.

1. **Hasil ditulis dari eksekusi.** Kode status dan badan respons diambil dari permintaan yang benar-benar dikirim.
   Teks hasil yang ditulis tetap di dalam skrip bukan bukti.
2. **Bukti mentah disimpan per skenario.**
   - Uji API: satu entri JSON berisi method, URL, badan permintaan, kode status, badan respons, dan waktu.
   - Uji layar: tangkapan layar PNG, ditambah catatan jaringan (method, URL, kode status, badan) untuk skenario yang
     menyebut permintaan.
3. **Uji layar dijalankan pada hasil build**, bukan `npm run dev`.
4. **Tidak ada penulisan langsung ke basis data.** Kueri baca boleh dipakai untuk memeriksa hasil. Sandi, hash sandi,
   dan data pengguna tidak diubah lewat SQL.
5. **Izin diberikan pemilik lewat layar Akses Role**, bukan lewat skrip.
6. **Skenario paralel dikirim benar-benar serentak.** Simpan kedua respons dan sebutkan urutan mana yang teramati.
7. **Skenario yang tidak dapat dijalankan ditulis `NOT RUN` beserta alasannya**, bukan `PASS`.
8. **Source tidak diubah untuk meloloskan skenario.** Kegagalan dilaporkan apa adanya, lengkap dengan badan respons.
9. **Laporan tidak memuat sandi, token, atau connection string.**
10. **Data uji dicatat**: pasien, nomor encounter, dan nomor kunjungan yang dibuat selama uji.
11. Uji ini verifikasi pengembang. Tidak ada klaim UAT.

---

## 3. Persiapan

### 3.1 Build

| Repository | Perintah | Yang diharapkan |
| --- | --- | --- |
| `NewQuilvianSystemBackend` | `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false` | 0 error. Tidak ada migration baru pada putaran ini |
| `QuilvianSystemFrontendDev` | `npm run build`, lalu `npm run start` | Build selesai; layar dilayani hasil build |

Galat kompilasi pada berkas IGD atau `PatientEncounterController.cs` dikembalikan ke pengembang sebelum uji dimulai.

### 3.2 Izin yang dibutuhkan akun uji

| Izin | Dipakai oleh |
| --- | --- |
| `EmergencyVisit : Read`, `Create`, `Update` | Hampir semua skenario |
| `EmergencyVisit : NoShow` | `057-S8`, `FE-IGD-039` |
| `PatientEncounter : Create`, serta izin ubah status dan batal encounter | Pendaftaran, `BE-IGD-059` |
| Izin observasi, tindak lanjut, dan kepergian IGD | `BE-IGD-061`, `062`, `063`, `FE-IGD-041`, `042` |

### 3.3 Alamat dasar

| Resource | Alamat dasar |
| --- | --- |
| Kunjungan IGD | `api/v1/health-services/emergency-installation-management/emergency-visits` |
| Observasi IGD | `api/v1/health-services/emergency-installation-management/emergency-observations` |
| Tindak lanjut IGD | `api/v1/health-services/emergency-installation-management/emergency-dispositions` |
| Kepergian IGD | `api/v1/health-services/emergency-installation-management/emergency-departures` |
| Encounter | `api/v1/health-services/registration-management/patient-encounters` |

### 3.4 Nilai enum

| Enum | Nilai |
| --- | --- |
| `visitStatus` | `Arrived` 1, `WaitingForTriage` 2, `Triaged` 3, `InTreatment` 4, `UnderObservation` 5, `AwaitingDisposition` 6, `Disposed` 7, `Cancelled` 8, `Completed` 9 |
| `observationStatus` | `Active` 1, `Completed` 2, `Escalated` 3, `Cancelled` 4 |
| `dispositionStatus` | `Draft` 1, `Confirmed` 2, `Executed` 3, `Cancelled` 4 |
| `encounterStatus` | `Completed` 9, `NoShow` 11 |
| `arrivalTimeSource` | `Unverified` 0, `Fallback` 1, `Confirmed` 2 |

### 3.5 Istilah data uji

| Istilah | Arti |
| --- | --- |
| Encounter baru | Encounter `Emergency` hasil `POST /patient-encounters` yang **belum** punya kunjungan IGD |
| Pasien bersih | Pasien tanpa kunjungan IGD lain yang berjalan dan tanpa encounter IGD lain yang terbuka. Wajib untuk skenario paralel; tanpa itu Mulai Triage ditolak aturan lain dan ujinya tidak berarti |
| Kunjungan menunggu penutupan | Kunjungan `Disposed` yang tindak lanjutnya `Executed` tetapi masih punya penahan: observasi aktif, kepergian belum tuntas, atau pesanan belum bersikap |

**Cara membuat kunjungan menunggu penutupan.** Mulai kunjungan sampai *Sedang ditangani*, buat penahannya (misalnya
buka satu periode observasi), buat tindak lanjut, konfirmasi, lalu jalankan. Kunjungan menjadi `Disposed` dan tidak
tertutup selama penahannya ada. Langkah serupa pernah dijalankan pada
[laporan uji `BE-IGD-060`](2026-09-30-laporan-uji-s1-s6-be-igd-060.md).

---

## 4. Tahap 1 — uji API jalur pendaftaran–triage

### 4.1 `BE-IGD-055` — lima ruas kunjungan opsional pada `POST /start-triage`

Token pemegang `EmergencyVisit : Create`.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `055-D1` | Encounter baru; `mode: "Triage"`, `arrivalDateTime` sah, ditambah `arrivalLocation: "  Pintu ambulans  "`, `foundLocation`, `traumaLocation`, `traumaDateTime` = 1 jam lalu, `notes` | `201`; respons membawa kelima ruas; `arrivalLocation` = `"Pintu ambulans"` (spasi tepi hilang); `GET /{id}` menampilkan nilai yang sama |
| `055-D2` | Encounter baru; `mode: "Triage"` tanpa kelima ruas | `201`; kelima ruas `null` — perilaku lama tidak berubah |
| `055-D3` | Encounter baru; `traumaDateTime` = sekarang + 1 jam | `400` *"Waktu trauma tidak boleh melewati waktu sekarang."*; encounter itu tetap tanpa kunjungan |
| `055-D4` | Encounter baru; `arrivalLocation` 251 karakter; lalu `notes` 1001 karakter | Keduanya `400`; encounter itu tetap tanpa kunjungan |
| `055-D5` | Ulangi `start-triage` pada encounter `055-D1` dengan `traumaLocation` berbeda | `200`; `traumaLocation` **tetap** nilai `055-D1` |
| `055-D6` | Encounter baru; `mode: "ImmediateCare"` ditambah `traumaLocation` dan `notes` | `201`; `visitStatus` 4; kedua ruas tersimpan; `arrivalTimeSource` tetap 1 |

### 4.2 `BE-IGD-057` — S8 ulang (NoShow lawan Mulai Triage serentak)

S1–S7, S9, dan S10 sudah terbukti. Yang tersisa hanya S8, dan hanya sah pada **pasien bersih**.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `057-S8` | (1) `POST /patient-encounters` (Emergency) untuk pasien bersih, catat `encounterId`. (2) Kirim serentak `POST /emergency-visits/no-show` `{ "encounterId", "reason" }` dan `POST /emergency-visits/start-triage` `{ "encounterId", "mode": "Triage", "arrivalDateTime" }`. (3) `GET /triage-queue`. (4) Ulangi beberapa kali dengan encounter baru | Tiap ulangan tepat satu berhasil: `no-show` `200` + `start-triage` `409` *"Encounter ini sudah berakhir (NoShow)…"*, **atau** `start-triage` `201` + `no-show` `409` *"Pasien ini sudah memiliki kunjungan IGD IGD-…"*. Tidak pernah keduanya berhasil. Pada antrean: baris kunjungan bila Mulai Triage menang, tidak tampil bila NoShow menang |

### 4.3 `BE-IGD-059` — jalur umum Registrasi dibatasi untuk encounter `Emergency`

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `059-S1` | `PATCH /patient-encounters/{id}/status` `{ "encounterStatus": 9 }` pada encounter Emergency yang masih menunggu triage | `409` *"Status kunjungan gawat darurat tidak dapat diubah dari sini. Tutup lewat layar IGD: tandai pasien pergi sebelum ditriage, atau selesaikan/batalkan kunjungan IGD-nya."*; `GET` encounter sesudahnya: status tidak berubah |
| `059-S2` | Sama dengan `059-S1` lewat `/admin/{id}/status` | `409` kalimat yang sama |
| `059-S3` | `PATCH /patient-encounters/{id}/cancel` `{ "cancelReason": "uji" }` pada encounter Emergency yang **sudah punya kunjungan** berjalan | `409` *"Encounter ini sudah memiliki kunjungan IGD IGD-…. Batalkan lewat kunjungan IGD tersebut; encounter akan ikut dibatalkan."*; encounter tidak berubah |
| `059-S4` | `PATCH …/cancel` pada encounter Emergency **tanpa** kunjungan | `200` *"Patient encounter berhasil dibatalkan."*; `GET` encounter: `isCancel` benar, `cancelledAt`, `cancelReason`, pelaku terisi, `isActive` salah; baris hilang dari `GET /emergency-visits/triage-queue` |
| `059-S5` | Daftarkan ulang pasien `059-S4` (Emergency, tanpa alasan pendaftaran ganda) | `200` — tidak tertolak penjaga |
| `059-S6` | Pasien bersih: daftarkan (Emergency), lalu kirim serentak `PATCH …/cancel` dan `POST /emergency-visits/start-triage` (`mode: "Triage"`, `arrivalDateTime`). Ulangi beberapa kali dengan encounter baru | Tiap ulangan tepat satu berhasil: `cancel` `200` + `start-triage` `409` *"Encounter ini sudah berakhir…"*, **atau** `start-triage` `201` + `cancel` `409` menyebut nomor kunjungan. Tidak pernah keduanya berhasil |
| `059-S7` | `PATCH …/status` dan `PATCH …/cancel` pada encounter **rawat jalan** | Keduanya `200` seperti sebelumnya; antrean rawat jalan ikut dibatalkan pada `cancel` |
| `059-S8` | `PATCH …/cancel` pada encounter Emergency yang kunjungannya sudah **selesai** | `400` *"Kunjungan yang sudah selesai tidak dapat dibatalkan."* |

`059-S6` dan `057-S8` sama-sama butuh pasien bersih dan dapat dijalankan berurutan pada sesi yang sama.

---

## 5. Tahap 2 — uji API penutupan kunjungan

### 5.1 `BE-IGD-061` — penutupan menyusul dari observasi dan kepergian

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `061-S1` | Kunjungan menunggu penutupan dengan **satu observasi aktif** sebagai satu-satunya penahan. `PATCH /emergency-observations/{id}/observation-status` `{ "observationStatus": 2, "notes": "tanda vital stabil" }` | `200`; observasi `Completed`, `completionSummary` terisi. `GET` kunjungan: `Completed`, `closedByDispositionId` terisi tindak lanjut itu, pelaku pembaruan = pengguna yang menyelesaikan observasi; encounter tertutup |
| `061-S2` | Kunjungan menunggu penutupan karena kepergian yang belum tuntas: `POST /emergency-departures/{id}/arrive`, lalu `POST …/accept-handover` | Sesudah `accept-handover`: `200`; kunjungan `Completed` |
| `061-S3` | Kunjungan menunggu penutupan karena satu pesanan belum bersikap: `PATCH /emergency-departures/{id}/order-items/{itemId}/action` | `200`; kunjungan `Completed` |
| `061-S4` | Aksi yang sama dengan `061-S1`…`S3` pada kunjungan yang **tidak** menunggu penutupan (misalnya masih `InTreatment`) | Aksinya berhasil seperti biasa; status kunjungan mengikuti aturan lama; tidak tertutup |
| `061-S5` | Kunjungan menunggu penutupan dengan **dua** penahan (observasi aktif + pesanan belum bersikap): selesaikan observasinya | `200`; observasi `Completed`; kunjungan tetap `Disposed` |
| `061-S6` | Observasi berstatus `Escalated` pada kunjungan `Disposed`: selesaikan | `200`; kunjungan tidak dipindahkan ke `AwaitingDisposition`; tertutup bila tak ada penahan lain |
| `061-S7` | Kunjungan `Disposed` dengan observasi aktif: `{ "observationStatus": 3, "notes": "pasien memburuk" }` | `409` *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."*; observasi tetap `Active`, `escalationReason` kosong, kunjungan tetap `Disposed` |
| `061-S8` | `061-S7` dengan catatan 1.200 karakter | `409` kalimat yang sama, **bukan** `400` |
| `061-S9` | Kunjungan `Disposed`: selesaikan observasi dengan catatan 1.200 karakter | `400` *"Catatan paling banyak 1000 karakter."*; observasi dan kunjungan tidak berubah |
| `061-S10` | Kunjungan `UnderObservation`: selesaikan observasi; kunjungan `InTreatment` lain: eskalasi observasi | Kunjungan pertama → `AwaitingDisposition`; kedua → tetap lewat penjaga seperti semula |
| `061-S11` | Kunjungan menunggu penutupan: `POST …/reject-handover` (beralasan), dan pada kunjungan lain `PATCH /emergency-departures/{id}/cancel` | Aksi `200`; kunjungan tertutup bila itu penahan terakhir |
| `061-S12` | Kunjungan menunggu penutupan: `POST …/order-items/{itemId}/accept`, dan pada pesanan lain `…/reject` | Aksi `200`; kunjungan tertutup bila itu penahan terakhir |

### 5.2 `BE-IGD-063` — saringan "menunggu penutupan" pada `GET /emergency-visits`

Jalankan `063-S1`…`S7` **sebelum** penahan pada data uji dibereskan; `063-S8` memakai hasil `BE-IGD-061`.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `063-S1` | `GET /?awaitingClosure=true` | Hanya kunjungan yang punya tindak lanjut `Executed` dan belum `Completed`/`Cancelled`; tiap baris `isAwaitingClosure: true` |
| `063-S2` | Kunjungan tertahan oleh observasi aktif, lalu oleh kepergian belum tiba, lalu oleh pesanan belum disikapi | `awaitingClosureReason` berturut-turut: *"Masih ada observasi yang belum diselesaikan."*, *"Masih ada proses kepergian pasien yang belum selesai."*, *"Masih ada pesanan yang belum ditentukan sikapnya: …."* |
| `063-S3` | `GET /` tanpa `awaitingClosure`, dibandingkan dengan hasil sebelum build ini | Baris, urutan, `totalData`, dan ruas lama sama; tiap baris bertambah dua ruas baru |
| `063-S4` | `GET /?awaitingClosure=true&pageSize=1` | `totalData` tetap jumlah seluruh kunjungan yang menunggu penutupan; `items` berisi satu baris |
| `063-S5` | `GET /?awaitingClosure=false` | `totalData`-nya ditambah `totalData` `063-S1` sama dengan `totalData` tanpa saringan; nol baris ber-`isAwaitingClosure: true` |
| `063-S6` | `GET /{id}` untuk kunjungan tertahan, lalu untuk kunjungan `Completed` | Yang pertama `isAwaitingClosure: true` beserta alasannya; yang kedua `false` dan `null` |
| `063-S7` | `awaitingClosure=true` digabung `search` atau `startDate`/`endDate` | Kedua saringan berlaku bersama |
| `063-S8` | Selesaikan penahan sebuah kunjungan tertahan, lalu ulangi `063-S1` | Kunjungan itu tertutup dan tidak lagi muncul; `totalData` berkurang satu |

### 5.3 `BE-IGD-062` — pembatalan tindak lanjut pada kunjungan selesai

Endpoint: `PATCH /emergency-dispositions/{id}/disposition-status`.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `062-S1` | Tindak lanjut `Executed` pada kunjungan `Completed`: ke `Cancelled` **dengan** `notes` | `409` *"Kunjungan IGD ini sudah selesai, sehingga disposisinya tidak dapat dibatalkan. Daftarkan pasien sebagai episode baru bila ia kembali."*; tindak lanjut dan kunjungan tidak berubah |
| `062-S2` | Sama dengan `062-S1` **tanpa** `notes` | `409` kalimat yang sama, bukan `400` alasan wajib |
| `062-S3` | Tindak lanjut `Executed` pada kunjungan yang masih menunggu penutupan: ke `Cancelled` beralasan | `400` *"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."* |
| `062-S4` | Tindak lanjut `Draft` atau `Confirmed` pada kunjungan berjalan: ke `Cancelled` beralasan | `200` |
| `062-S5` | Tindak lanjut `Draft` pada kunjungan berjalan: ke `Cancelled` tanpa alasan | `400` *"Alasan pembatalan wajib diisi ketika tindak lanjut dibatalkan."* |

---

## 6. Tahap 3 — uji layar jalur pendaftaran–triage

### 6.1 `FE-IGD-036` — loket hanya membuat encounter; dialog Mulai Triage

Prasyarat: backend hasil build tahap ini (memuat `BE-IGD-059` dan delta `BE-IGD-055`).

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `036-U1` | Loket: daftarkan pasien tanpa episode sampai selesai | Langkah Emergency Visit hanya berisi kategori kunjungan, keluhan, dan alasan pendaftaran ganda. Panel jaringan: tepat **satu** `POST /patient-encounters`, **nol** `POST /emergency-visits`. Layar Selesai memuat nomor encounter dan *Menunggu Triage* |
| `036-U2` | Buka Triage Pasien | Pasien `036-U1` tampil *Menunggu Triage — Terdaftar hh.mm* dengan tombol **Mulai Triage** dan **Tangani Segera** |
| `036-U3` | Tekan Mulai Triage; mundurkan waktu tiba 15 menit; tekan *Mulai Triage* | Dialog terisi awal waktu terdaftar. `POST /start-triage` membawa `mode: "Triage"` dan `arrivalDateTime` yang dimundurkan → `201`. Detail triage pasien terbuka. Kembali ke daftar: baris menjadi *Tiba hh.mm* dengan *Isi Triage* |
| `036-U4` | Pasien baru: Mulai Triage, kosongkan waktu tiba; lalu isi waktu satu jam ke depan | Kosong: pesan wajib, nol permintaan. Masa depan: `400` *"Waktu tiba tidak boleh melewati waktu sekarang."* tampil di dialog |
| `036-U5` | Pasien baru: Tangani Segera pada baris tanpa kunjungan | Konfirmasi **tanpa isian**. `POST /start-triage` berisi hanya `encounterId` dan `mode: "ImmediateCare"`. Pengkajian IGD terbuka; kunjungan *Sedang ditangani* |
| `036-U6` | Pasien rekam pengganti: Mulai Triage, centang *Pasien tanpa identitas*, isi nama sementara | Payload `isUnknownPatient: true` + nama sementara. Baris daftar menampilkan keterangan tanpa identitas. Tanda vital, SOAP, dan pesanan lab dapat dibuat |
| `036-U7` | Pasien hasil pendaftaran ganda beralasan yang kunjungan lamanya masih berjalan: Mulai Triage pada encounter keduanya | `409` dengan kalimat server tampil di dialog; daftar dimuat ulang |
| `036-U8` | Dua tab: tekan Mulai Triage untuk pasien yang sama di keduanya | Tab kedua mendapat `200` dan membuka kunjungan yang sama; tidak ada kunjungan kedua |
| `036-U9` | Loket: pasien yang masih menunggu triage, isi alasan pendaftaran ganda, selesaikan | Encounter kedua tersimpan lewat satu `POST`; nol `POST /emergency-visits` |
| `036-U10` | Pasien baru: Mulai Triage; isi lokasi kedatangan, lokasi pasien ditemukan, lokasi trauma, waktu trauma 30 menit lalu, dan catatan kunjungan; tekan *Mulai Triage* | Badan `POST /start-triage` membawa `arrivalLocation`, `foundLocation`, `traumaLocation`, `traumaDateTime`, `notes` → `201`; respons memuat kelima nilai yang sama; Detail triage terbuka |
| `036-U11` | Pasien baru: Mulai Triage tanpa mengisi kelima isian baru | Badan permintaan **tidak** memuat kelima kunci itu → `201` |
| `036-U12` | Pasien baru: Mulai Triage dengan waktu trauma satu jam ke depan | `400` *"Waktu trauma tidak boleh melewati waktu sekarang."* tampil di dialog; dialog tetap terbuka dan isian lain tidak hilang |
| `036-U13` | Ketik lebih dari 250 karakter pada salah satu isian lokasi, dan lebih dari 1000 pada catatan | Isian berhenti menerima ketikan pada 250 dan 1000 karakter |
| `036-U14` | Tangani Segera pada baris tanpa kunjungan | Tetap konfirmasi **tanpa isian**; badan permintaan hanya `encounterId` dan `mode` |

Kelima nilai pada `036-U10` belum ditampilkan layar mana pun; buktinya diambil dari panel jaringan.

### 6.2 `FE-IGD-039` — aksi *Pergi sebelum ditriage*

Prasyarat: akun memegang `EmergencyVisit : NoShow`.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `039-U1` | Daftar Triage Pasien: baris tanpa kunjungan | Tiga tombol: Mulai Triage, Tangani Segera, **Pergi** |
| `039-U2` | Tekan Pergi, biarkan alasan kosong | Tombol *Tandai Pergi* tidak aktif; nol permintaan |
| `039-U3` | Isi alasan, tekan *Tandai Pergi* | `POST /no-show` `200`; baris hilang; spanduk hijau *"… ditandai pergi sebelum ditriage."*; isian berhenti di 250 karakter |
| `039-U4` | Baris yang **sudah** punya kunjungan | Tidak ada tombol Pergi |
| `039-U5` | Dua tab: tab A menekan Mulai Triage, tab B (belum dimuat ulang) menekan Pergi pada pasien yang sama | Tab B: `409` *"Pasien ini sudah memiliki kunjungan IGD …"* pada spanduk merah; daftar dimuat ulang dan barisnya menjadi baris kunjungan |
| `039-U6` | Periksa seluruh teks pada tombol, dialog, dan spanduk | Tidak ada kata "Tidak Hadir" |

### 6.3 `FE-IGD-040` — panel waktu tiba pada Detail triage

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `040-U1` | Pasien Tangani Segera (sumber `1`): buka Detail triage dari daftar (*Lihat Riwayat*) | Panel kuning **Waktu tiba sementara**, kalimat menyebut "(waktu terdaftar)", isian terisi nilai tercatat |
| `040-U2` | Kunjungan lama (sumber `0`) yang masih Menunggu Triage: buka *Isi Triage* | Panel kuning **Waktu tiba belum dikonfirmasi** di bawah ringkasan pasien |
| `040-U3` | Dari `040-U1`: mundurkan beberapa menit, tekan Konfirmasi | `PATCH …/arrival-time` `200`; panel menjadi baris hijau *"Tiba … · dikonfirmasi <nama>"* — nama, bukan GUID |
| `040-U4` | Pasien Tangani Segera lain: isi waktu **sesudah** mulai penanganan, tekan Konfirmasi | `409` dengan kalimat server yang menyebut peristiwa dan jamnya tampil di bawah isian; panel tetap kuning |
| `040-U5` | Dari `040-U2`: isi formulir triage, tekan Simpan Pemeriksaan **tanpa** mengonfirmasi | Nol permintaan simpan; kalimat *"Konfirmasi waktu tiba lebih dulu…"* tampil di atas tombol |
| `040-U6` | Dari `040-U5`: tekan Konfirmasi, lalu Simpan Pemeriksaan | Panel hijau; penyimpanan triage berjalan seperti biasa |
| `040-U7` | Pasien yang lahir lewat Mulai Triage (sumber `2`): buka *Isi Triage* | Hanya baris hijau ringkas; simpan tidak ditahan |

---

## 7. Tahap 4 — uji layar penutupan kunjungan

### 7.1 `FE-IGD-042` — kalimat konfirmasi dan tombol Eskalasi

Satu siklus dengan `BE-IGD-061`. Layar: Pengkajian Pasien IGD, detail kunjungan.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `042-U1` | Kunjungan dengan tindak lanjut `Dikonfirmasi`: tab Tindak Lanjut, tekan Jalankan | Modal memuat *"Kunjungan langsung selesai bila tidak ada kewajiban yang tersisa…menunggu penutupan dan tertutup sendiri…"*; tidak ada *"belum menyelesaikan kunjungan"* |
| `042-U2` | Lanjutkan `042-U1` pada kunjungan yang observasinya masih aktif: konfirmasi Jalankan | Kartu pasien berubah menjadi *Tindak lanjut ditetapkan* tanpa memuat ulang halaman |
| `042-U3` | Dari `042-U2`, pindah ke tab Observasi | Tombol Eskalasi nonaktif; keterangan *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."* terbaca di bawah deret tombol; menekannya tidak memunculkan modal dan nol permintaan pada tab jaringan |
| `042-U4` | Dari `042-U3`, tekan Selesaikan | Modal memuat *"…status kunjungan tidak berpindah…kunjungan langsung selesai"*; tidak ada *"Menunggu Tindak Lanjut"* |
| `042-U5` | Dari `042-U4`, isi kesimpulan, konfirmasi | Periode menjadi Selesai beserta kesimpulannya; kartu pasien terbaca *Selesai* tanpa memuat ulang halaman |
| `042-U6` | Ulangi `042-U3`…`U4` pada periode berstatus Dieskalasi di kunjungan `Disposed` | Modal Selesaikan memuat isi yang sama dengan awalan *"Menutup periode yang sudah dieskalasi."* |
| `042-U7` | Kunjungan *Dalam observasi* (bukan `Disposed`): buka tab Observasi | Eskalasi aktif; kalimat Selesaikan dan Eskalasi sama persis seperti sebelumnya; tidak ada keterangan di bawah tombol |
| `042-U8` | Buka tab Observasi pada kunjungan yang belum `Disposed`, jalankan tindak lanjutnya dari tab peramban lain, lalu tekan Eskalasi dan isi alasan | `409` *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."* tampil di dalam modal; alasan yang diketik tetap ada |

### 7.2 `FE-IGD-041` — saringan dan kolom "Menunggu penutupan"

Butuh `BE-IGD-063`. Layar: daftar Pengkajian Pasien IGD. Jalankan selagi masih ada kunjungan tertahan.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `041-U1` | Buka Pengkajian Pasien IGD, lihat panel Filter Pasien | Ada pilihan **Penutupan Kunjungan** berisi *Semua kunjungan* dan *Menunggu penutupan* |
| `041-U2` | Pilih *Menunggu penutupan* | Permintaan membawa `awaitingClosure=true`; hanya kunjungan tertahan yang tampil; kolom PENUTUPAN memuat penanda dan alasan tiap baris, terbaca utuh tanpa membuka detail |
| `041-U3` | Masih pada `041-U2` | Di atas tabel terbaca *"N kunjungan menunggu penutupan"*, sama dengan jumlah pada baris paginasi |
| `041-U4` | Pilih *Semua kunjungan*, lalu ulangi dengan tombol Reset | Daftar kembali seperti semula; permintaan tanpa `awaitingClosure`; baris jumlah hilang |
| `041-U5` | Pilih *Menunggu penutupan* saat tidak ada kunjungan tertahan (atau gabungkan dengan tanggal yang kosong) | *"Tidak ada kunjungan yang menunggu penutupan."* beserta penjelasannya |
| `041-U6` | Gabungkan *Menunggu penutupan* dengan pencarian nama, lalu pindah halaman bila baris lebih dari sepuluh | Kedua saringan berlaku bersama; pindah halaman tetap tersaring; mengubah saringan kembali ke halaman 1 |
| `041-U7` | Tanpa saringan, buka halaman yang memuat kunjungan tertahan | Kolom PENUTUPAN tampil; baris tertahan berpenanda, baris lain bertanda hubung |

---

## 8. Pelaporan hasil

| Hal | Ketentuan |
| --- | --- |
| Bukti mentah | `QuilvianSystemFrontendDev/test-with-agy/igd/` — JSON dan PNG, dinamai menurut ID skenario pada berkas ini |
| Laporan hasil | Berkas baru di folder ini (`docs/module-blueprints/igd/testing/`), mengikuti pola nama laporan uji yang sudah ada. Berkas panduan ini tidak disunting |
| Isi laporan | Per skenario: ID, hasil (`PASS` / `FAIL` / `NOT RUN`), kode status dan kalimat yang teramati, nama berkas bukti |
| Penandaan roadmap | Dilakukan pengembang **sesudah** bukti mentah diperiksa, bukan dari ringkasan laporan |

Rekap untuk diisi pada laporan hasil:

| Task | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| --- | --- | ---: | ---: | ---: | ---: |
| `BE-IGD-055` | D1–D6 | 6 | | | |
| `BE-IGD-057` | S8 ulang | 1 | | | |
| `BE-IGD-059` | S1–S8 | 8 | | | |
| `BE-IGD-061` | S1–S12 | 12 | | | |
| `BE-IGD-063` | S1–S8 | 8 | | | |
| `BE-IGD-062` | S1–S5 | 5 | | | |
| `FE-IGD-036` | U1–U14 | 14 | | | |
| `FE-IGD-039` | U1–U6 | 6 | | | |
| `FE-IGD-040` | U1–U7 | 7 | | | |
| `FE-IGD-042` | U1–U8 | 8 | | | |
| `FE-IGD-041` | U1–U7 | 7 | | | |
| **Jumlah** | | **82** | | | |
