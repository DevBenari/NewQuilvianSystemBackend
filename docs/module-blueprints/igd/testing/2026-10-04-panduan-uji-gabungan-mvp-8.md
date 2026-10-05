# Panduan Uji Gabungan `MVP-8` — Observasi Dieskalasi, Kunjungan yang Sudah Berakhir, dan Penanda Menunggu Penutupan

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 4 Oktober 2026 |
| Untuk | Agen penguji (Antigravity) yang dijalankan pemilik modul (Rizki) |
| Tujuan | Membuktikan pengerjaan ulang tiga task `MVP-8` dalam satu siklus: `BE-IGD-061` (uji API S13–S19), `FE-IGD-041` (uji layar U2, U6, U7, R), dan `FE-IGD-042` (uji layar acceptance 10–13 beserta regresi singkat U1–U8) |
| Jumlah skenario | 29 — 13 uji API, 16 uji layar (rincian pada bagian 8) |
| Source yang diuji | Backend `rizkiG` `c1f79f79`, dibuild Rizki 3 Oktober 2026 (0 error, 0 warning). Frontend `RizkiV2` `19ba512de` + dua berkas `FE-IGD-042` yang belum di-commit; hasil build agent 4 Oktober 2026 11.46 |
| Kontrak | validation `0.11.0` §11.1–11.2; state `0.8.0` §9.5–9.6; API `0.14.0` §9.1–9.4 — `approved` (`IGD-DEC-186`) |
| Sumber skenario | Laporan [`BE-IGD-061`](../task/report/backend/BE-IGD-061.md) (*Pengerjaan ulang 3 Oktober 2026* dan bagian 5.1), [`FE-IGD-041`](../task/report/frontend/FE-IGD-041.md) (*Pengerjaan ulang 3 Oktober 2026*), [`FE-IGD-042`](../task/report/frontend/FE-IGD-042.md) (bagian 6.1 dan *Pengerjaan ulang 4 Oktober 2026*). Bila berbeda, **laporan task yang berlaku**, kecuali satu koreksi pada `061-S18` (bagian 5) |
| Status | Panduan. Belum ada skenario yang dijalankan |

Berkas ini tidak diisi hasil. Hasil ditulis pada laporan baru — lihat bagian 9.

---

## 1. Aturan yang mengikat agen

Putaran 1–3 Oktober sebagian ditolak karena melanggar aturan di bawah. Patuhi seluruhnya; pelanggaran membuat
skenario yang bersangkutan **tidak sah** walaupun hasilnya tampak lulus.

| No | Aturan | Contoh pelanggaran yang pernah terjadi |
| ---: | --- | --- |
| A1 | **Layar dilayani hasil build** (`node .next/standalone/server.js` atau `npm run start`), bukan `npm run dev`. Bukti: `document.querySelector('nextjs-portal') === null` pada setiap skenario layar, dicatat di JSON. Lencana "N" di pojok kiri bawah tangkapan layar berarti `next dev` → skenario tidak sah | 2 Oktober seluruh uji layar dilayani `next dev` |
| A2 | **Viewport 1440 × 900** dengan sidebar terbuka untuk **semua** skenario layar | — |
| A3 | **Akun peran nyata** (bagian 2.2). SuperAdmin **tidak** dipakai untuk satu langkah pun | — |
| A4 | **Akses Role tidak boleh diubah** — tidak lewat layar, tidak lewat API, tidak lewat SQL. Agen tidak membuka tombol Simpan pada halaman Akses Role. Bila sebuah langkah ditolak `403` karena izin kurang, **berhenti**, catat `NOT RUN — izin <nama izin> tidak ada pada peran <peran>`, dan minta pemilik. Skrip yang memanggil endpoint peran/izin selain `GET` atau menjalankan `INSERT`/`UPDATE`/`DELETE` membuat seluruh putaran tidak sah | 3 Oktober agen mengubah izin Petugas Pendaftaran dan Admin Rekam Medis lewat SuperAdmin (`configure-role-access.mjs`) |
| A5 | **Sandi tidak ditulis di skrip maupun laporan.** Kredensial dibaca dari variabel lingkungan (bagian 2.2). Badan `POST /auth/login` pada JSON bukti disamarkan (`"password": "***"`). Laporan tidak memuat host, port basis data, token, atau connection string | 3 Oktober `check-test-data.mjs` dan JSON bukti memuat kredensial dalam teks biasa |
| A6 | **Skrip wajib memeriksa setiap harapan.** Setiap baris *Yang diharapkan* menjadi satu entri `pemeriksaan` berisi `harapan`, `teramati` (diambil dari respons, DOM, atau kueri yang benar-benar terjadi), dan `lulus` (hasil perbandingan). `putusan` dihitung `Object.values(pemeriksaan).every(p => p.lulus)`. Menulis `pass = true`, `putusan: "PASS"`, atau kalimat harapan sebagai nilai teramati **tidak diterima** | 2 Oktober `041-U6` ditulis lulus tanpa pemeriksaan (`pass = true`) |
| A7 | **Kalimat diperiksa huruf demi huruf** terhadap tabel bagian 3.3 (`===` untuk pesan API; `includes` untuk teks modal yang memuat kalimat itu). Kalimat yang mirip bukan lulus | — |
| A8 | **Catat semua permintaan ke backend** (`https://localhost:7184/api/**`) per skenario: method, URL, badan permintaan, kode status, badan respons, waktu. Beberapa pemeriksaan justru membuktikan *tidak adanya* permintaan | `036-U7` 2 Oktober hanya merekam satu endpoint |
| A9 | **Tangkapan layar viewport** (bukan halaman penuh) diambil sesudah respons yang diuji tiba + 1000 ms | — |
| A10 | **Tidak ada penulisan langsung ke basis data.** Kueri hanya `SELECT`. Data uji dibuat lewat API atau layar dengan akun peran nyata | — |
| A11 | **Setiap percobaan dilaporkan**, termasuk yang gagal atau diulang. Bukti lama tidak ditimpa — beri akhiran `-try2`, `-try3` | Percobaan pertama `040-U6` tertimpa dan tidak dilaporkan |
| A12 | **Narasi laporan sama dengan bukti mentah.** Tulis "berurutan" bila berurutan, "kunjungan lain" bila memakai data lain | `036-U8` ditulis "bersamaan" padahal berjeda 2 detik |
| A13 | **Source tidak diubah**, frontend maupun backend. **Backend tidak di-build** oleh agen. Skenario yang gagal dilaporkan `FAIL` apa adanya | — |
| A14 | Skenario yang tidak dapat dijalankan ditulis **`NOT RUN` beserta alasannya**, bukan `PASS` | — |
| A15 | **Urutan pada kunjungan uji S6 mengikat** (bagian 6, blok B): `061-S17` langkah 1–3 → `061-S18` → `042-U12` → `061-S17` langkah 4. Langkah 4 membatalkan observasinya, sesudah itu S18 dan `042-U12` tidak dapat dijalankan lagi | — |
| A16 | Ini verifikasi pengembang. **Tidak ada klaim UAT** | — |

---

## 2. Persiapan

### 2.1 Langkah persiapan

| Langkah | Tindakan | Syarat lanjut |
| --- | --- | --- |
| P1 | Di `QuilvianSystemFrontendDev`: catat `git rev-parse --short HEAD`, `git status --short`, dan waktu ubah `src/utils/health-services/emergency-installation-management/emergency-assessment-status-action.utils.js` serta `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx`. Di `NewQuilvianSystemBackend`: `git rev-parse --short HEAD` | Frontend `19ba512de` dengan kedua berkas berstatus `M`; backend `c1f79f79` atau sesudahnya |
| P2 | Bandingkan waktu ubah `.next/BUILD_ID` dengan kedua berkas pada P1 | `BUILD_ID` **lebih baru** (hasil build 4 Oktober 2026 11.46). Bila lebih tua: berhenti dan minta pemilik — agen tidak menjalankan build |
| P3 | Periksa port 3000 | Bila dipakai `node .next/standalone/server.js`: lanjut. Bila kosong: jalankan `npm run start` dari akar frontend. Bila dipakai `next dev` atau proses lain: **berhenti dan minta pemilik menghentikannya** — jangan mematikan proses yang bukan milik agen |
| P4 | Buka `http://localhost:3000/login` | Halaman login terbuka; `nextjs-portal` bernilai `null` |
| P5 | Backend pemilik berjalan di `https://localhost:7184`; login dengan akun `KLINIS` | Login berhasil |
| P6 | Untuk setiap akun pada 2.2: login lewat layar, simpan respons `GET /api/v1/auth/permissions` yang dipanggil layar sendiri sebagai `P6-<akun>.json` | Izin pada tabel 2.2 ada. Bila kurang: **berhenti** (aturan A4) |
| P7 | Jalankan kueri D-S6 (bagian 4.1) dan simpan keluarannya | Lihat 4.1 |

### 2.2 Akun uji — diberikan pemilik, tidak diubah agen

| Akun | Peran nyata | Izin yang wajib ada | Variabel lingkungan |
| --- | --- | --- | --- |
| `KLINIS` | Perawat IGD (Departemen Keperawatan) | `EmergencyVisit : Read`, `Create`, `Update`; `EmergencyObservation : Read`, `Create`, `Update`; `EmergencyObservationDetail : Read`, `Create`, `Update`; `EmergencyDisposition : Read`, `Create`, `Update`; `EmergencyDeparture : Read`, `Create`, `Update` | `QUILVIAN_PERAWAT_EMAIL`, `QUILVIAN_PERAWAT_PASSWORD` |
| `DOKTER` *(hanya bila tindak lanjut pada konfigurasi peran Anda milik dokter)* | Dokter IGD | `EmergencyDisposition : Read`, `Create`, `Update`; `EmergencyVisit : Read` | `QUILVIAN_DOKTER_EMAIL`, `QUILVIAN_DOKTER_PASSWORD` |
| `LOKET` | Petugas Pendaftaran (Departemen Pendaftaran) | `PatientEncounter : Create`, `Read`; `EmergencyVisit : Create` | `QUILVIAN_LOKET_EMAIL`, `QUILVIAN_LOKET_PASSWORD` |
| Basis data dev | — | `SELECT` saja | `QUILVIAN_DEV_DB_URL` |

Bila `DOKTER` dipakai, langkah membuat, mengonfirmasi, dan menjalankan tindak lanjut (API maupun layar `042-U1`,
`U2`) memakai akun itu; catat di JSON akun mana yang menjalankan tiap langkah. Bila `KLINIS` sudah memegang izin
tindak lanjut, `DOKTER` tidak diperlukan.

---

## 3. Alamat, nilai enum, dan kalimat

### 3.1 Alamat

| Resource | Alamat |
| --- | --- |
| Kunjungan IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-visits` |
| Observasi IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-observations` |
| Pemantauan observasi | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-observation-details` |
| Tindak lanjut IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-dispositions` |
| Kepergian IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-departures` |
| Encounter (petugas) | `https://localhost:7184/api/v1/health-services/registration-management/patient-encounters/admin` |
| Layar Pengkajian Pasien IGD | `http://localhost:3000/health-services/emergency-installation-management/emergency-assessment` |
| Layar loket IGD | `http://localhost:3000/health-services/registration-management/emergency-registration` |

Ruang Kerja Pemeriksaan dibuka dari daftar Pengkajian: cari **nomor kunjungan lengkap**, lalu tombol *Pemeriksaan*
pada barisnya. Daftar tanpa saringan memuat semua status, termasuk *Selesai* dan *Dibatalkan*.

### 3.2 Nilai enum

| Enum | Nilai |
| --- | --- |
| `visitStatus` | `InTreatment` 4 (*Sedang ditangani*), `UnderObservation` 5 (*Dalam observasi*), `AwaitingDisposition` 6 (*Menunggu keputusan*), `Disposed` 7 (*Tindak lanjut ditetapkan*), `Cancelled` 8 (*Dibatalkan*), `Completed` 9 (*Selesai*) |
| `observationStatus` | `Active` 1 (*Sedang berjalan*), `Completed` 2 (*Selesai*), `Escalated` 3 (*Dieskalasi*), `Cancelled` 4 (*Dibatalkan*) |
| `dispositionStatus` | `Draft` 1, `Confirmed` 2, `Executed` 3, `Cancelled` 4 |
| `encounterStatus` | `Completed` 9 |

### 3.3 Kalimat yang diperiksa huruf demi huruf

| Kode | Kalimat | Sumber |
| --- | --- | --- |
| `K-R18` | Kunjungan IGD ini sudah berakhir; observasinya tidak dapat diselesaikan, dieskalasi, atau diaktifkan kembali. | validation §11.2 aturan 18 — pesan API **dan** keterangan layar |
| `K-R21` | Kunjungan IGD ini sudah berakhir; pemantauan observasi tidak dapat ditambahkan lagi. | validation §11.2 aturan 21 |
| `K-R14` | Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini. | validation §11.1 aturan 14 — pesan API dan keterangan layar |
| `K-OBS` | Masih ada observasi yang belum diselesaikan. | validation §6 aturan 2 — pesan `409` dan `awaitingClosureReason` |
| `K-CATATAN` | Catatan paling banyak 1000 karakter. | validation §8 |
| `K-SELESAI` | Kunjungan pasien berpindah ke Menunggu Tindak Lanjut, dan periode ini tidak dapat dibuka kembali dari layar ini. | Modal *Selesaikan*, periode Aktif, kunjungan berjalan |
| `K-SELESAI-D` | Tindak lanjut pasien sudah dilaksanakan, sehingga status kunjungan tidak berpindah. Bila tidak ada kewajiban lain yang tersisa, kunjungan langsung selesai. Periode ini tidak dapat dibuka kembali dari layar ini. | Modal *Selesaikan*, periode Aktif, kunjungan `Disposed` |
| `K-SELESAI-ESK-D` | Menutup periode yang sudah dieskalasi. Tindak lanjut pasien sudah dilaksanakan, sehingga status kunjungan tidak berpindah. Bila tidak ada kewajiban lain yang tersisa, kunjungan langsung selesai. | Modal *Selesaikan*, periode Dieskalasi, kunjungan `Disposed` |
| `K-ESKALASI` | Pakai ini ketika keadaan pasien memburuk sehingga pemantauan berkala tidak lagi memadai. Kunjungan kembali ke status Sedang Ditangani, dan alasannya tersimpan pada periode observasi. | Modal *Eskalasi*, kunjungan berjalan |
| `K-BATAL` | Membatalkan berarti periode ini dianggap tidak pernah berjalan. Pakai hanya untuk periode yang salah dibuka, bukan untuk periode yang sudah selesai dipantau. Status kunjungan tidak ikut berubah. | Modal *Batalkan*, periode Aktif |
| `K-JALANKAN` | Kunjungan langsung selesai bila tidak ada kewajiban yang tersisa. Bila masih ada observasi, perpindahan, atau pesanan yang belum tuntas, kunjungan menunggu penutupan dan tertutup sendiri begitu kewajiban terakhir dibereskan. | Modal *Jalankan* tindak lanjut |
| `K-041-HINT` | Tindak lanjutnya sudah dilaksanakan. Kunjungan tertutup sendiri begitu penahan yang tertulis di bawah status kunjungannya dibereskan. | Petunjuk di bawah baris jumlah daftar Pengkajian |
| `K-041-KOSONG` | Tidak ada kunjungan yang menunggu penutupan. | Keadaan kosong saringan |

---

## 4. Data uji

### 4.1 Kunjungan uji S6 (`D-S6`) — data lama, dicari dengan `SELECT`

Kunjungan uji `061-S6` 2 Oktober 2026: kunjungan **Selesai** yang observasinya masih **Dieskalasi** — keadaan yang sejak
`IGD-DEC-183` tidak dapat dibuat lagi lewat alur normal. Bukti mentah 2 Oktober sudah tidak ada di repository, jadi
kunjungannya dicari ulang:

```sql
-- D-S6: observasi Dieskalasi yang tertinggal pada kunjungan berakhir
SELECT v."EmergencyVisitNumber", v."Id" AS visit_id, v."VisitStatus",
       o."Id" AS observation_id, o."ObservationNumber", o."ObservationStatus", o."EscalationReason",
       (SELECT COUNT(*) FROM public."EmgObservationDetail" d
         WHERE d."EmergencyObservationId" = o."Id" AND NOT d."IsDelete") AS jumlah_pemantauan
FROM public."EmgObservation" o
JOIN public."EmgVisit" v ON v."Id" = o."EmergencyVisitId"
WHERE o."ObservationStatus" = 3
  AND v."VisitStatus" IN (8, 9)
  AND NOT o."IsDelete" AND NOT v."IsDelete"
ORDER BY v."VisitStatus" DESC, o."UpdateDateTime" DESC NULLS LAST;
```

| Hasil | Tindakan |
| --- | --- |
| Ada baris dengan `VisitStatus` 9 | Pakai baris teratas sebagai `D-S6`. Utamakan yang `jumlah_pemantauan` > 0 (dipakai kaki `PUT` pada `061-S18`) |
| Hanya ada baris `VisitStatus` 8 | Pakai sebagai `D-S6` dan catat bahwa kunjungannya *Dibatalkan*, bukan *Selesai* |
| Kosong | `061-S17`, `061-S18` (kaki `D-S6`), dan `042-U12` → `NOT RUN — data D-S6 tidak ada`. Jangan membuat data pengganti lewat SQL |

**Simpan jumlah baris kueri ini sebelum `061-S17` dijalankan** (`P7.json`). Angka itu sekaligus jumlah observasi
Dieskalasi yang tertinggal pada kunjungan berakhir di dev (`IGD-OQ-112`) sebelum putaran ini menyentuhnya.

### 4.2 Kunjungan baru `K1`…`K10` — dibuat lewat API atau layar dengan akun peran nyata

Setiap kunjungan baru butuh **pasien bersih**: tanpa kunjungan IGD berjalan dan tanpa encounter `Emergency` yang belum
berakhir. Pasien boleh dipakai ulang bila kunjungan sebelumnya sudah *Selesai* atau *Dibatalkan*.

**Resep dasar.**

| Kode | Langkah | Hasil |
| --- | --- | --- |
| `R0` | Akun `LOKET`: daftarkan pasien bersih di layar loket IGD sampai layar Selesai (alur `C4-01` 3 Oktober), **atau** `POST /patient-encounters/admin` dengan badan yang sama dengan permintaan layar loket. Catat caranya | `encounterId` |
| `R1` | `R0`, lalu akun `KLINIS`: `POST /emergency-visits/start-triage` `{ "encounterId": "<R0>", "mode": "ImmediateCare" }` | `201`; `visitStatus` 4 |
| `RA` | `R1`, lalu `POST /emergency-observations` `{ "emergencyVisitId": "<id>", "indication": "Uji MVP-8 <kode kunjungan>", "observationPlan": "Pantau tanda vital tiap 30 menit" }` | `200`; observasi `Active`; kunjungan tetap 4 |
| `RB` | `RA`, lalu `PATCH /emergency-observations/{id}/observation-status` `{ "observationStatus": 3, "notes": "Saturasi turun, uji MVP-8" }` | `200`; observasi `Escalated`; kunjungan 4 |
| `RD` | Tindak lanjut jenis **Pulang**: ambil `id` jenis berkode `PULANG` (`SELECT "Id" FROM public."EmgDispositionType" WHERE "Code" = 'PULANG' AND NOT "IsDelete"`), lalu `POST /emergency-dispositions` `{ "emergencyVisitId", "dispositionTypeId", "dispositionReason": "Uji MVP-8" }` → `PATCH /emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 2 }` | Tindak lanjut `Confirmed` |
| `RX` | `PATCH /emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 3 }` | Tindak lanjut `Executed` |

**Kunjungan yang dibuat.**

| Kode | Resep | Dipakai pada |
| --- | --- | --- |
| `K1` | `RB` → `RD` → `RX` | `061-S13`, `061-S14` |
| `K2` | `RB` → `RD` → `RX` | `061-S15` |
| `K3` | `RB` → `RD` → `RX` | `061-S16`, `041-U2` (baris tertahan), `042-U13` |
| `K4` | `RA` (tanpa tindak lanjut) | Pembanding `061-S18`, `042-U7`, `042-U9`, `042-U10`, `042-U11` |
| `K5` | `RA` → `RD` (dikonfirmasi, **belum** dijalankan) | `042-U1`…`U5` |
| `K6` | `RA` → `RD` (dikonfirmasi, **belum** dijalankan) | `042-U8`, `061-S19-S7`, `S8`, `S9`, `S1` |
| `K7` | `RA` → `POST /emergency-departures` `{ "emergencyVisitId", "toServiceUnitId": "<unit tujuan aktif>", "departureReason": "Uji MVP-8" }` → `RD` → `RX` | `061-S19-S5`, `061-S19-S11` kaki `cancel` |
| `K8` | `RA` → `PATCH /emergency-visits/{id}/visit-status` `{ "visitStatus": 5 }` | `061-S19-S10` kaki pertama |
| `K9` | `RA` | `061-S19-S10` kaki kedua |
| `K10` *(opsional)* | `RA` → kepergian seperti `K7` → `POST /emergency-departures/{id}/submit-handover` → `RD` → `RX` | `061-S19-S11` kaki `reject-handover` |

Bila pembuatan kepergian (`K7`, `K10`) ditolak `403` kewenangan unit (*"Unit … belum dipetakan ke simpul organisasi…"*
atau *"Unit asal belum tercatat…"*), skenario yang bergantung padanya → `NOT RUN — tertahan BE-IGD-039`. Itu bukan
kegagalan task ini (laporan `BE-IGD-061`, *Pemeriksaan bukti uji gabungan*).

---

## 5. Kueri pemeriksaan

```sql
-- Q-VISIT: status, penanda asal penutupan, dan pelaku pembaruan
SELECT v."EmergencyVisitNumber", v."VisitStatus", v."ClosedByDispositionId", v."UpdateBy", v."VisitCompletedAt",
       e."EncounterStatus"
FROM public."EmgVisit" v
LEFT JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
WHERE v."Id" = '<visit_id>';

-- Q-PEMANTAUAN: jumlah baris pemantauan satu periode
SELECT COUNT(*) AS jumlah FROM public."EmgObservationDetail"
WHERE "EmergencyObservationId" = '<observation_id>' AND NOT "IsDelete";
```

`UpdateBy` dibandingkan dengan `id` pengguna pada respons `GET /api/v1/auth/me` akun `KLINIS` (tidak perlu kueri tabel
pengguna).

**Koreksi terhadap laporan `BE-IGD-061`.** Skenario S18 menulis pembanding *"`201`"*. Source
`EmergencyObservationDetailController.Create` menjawab `Ok(...)`, jadi kode yang diharapkan untuk pemantauan yang
berhasil adalah **`200`**.

---

## 6. Skenario — kerjakan berurutan per blok

### Blok A — `BE-IGD-061` S13–S16 (API, akun `KLINIS`)

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `061-S13` | Siapkan `K1` (`RB` → `RD` → `RX`). Lalu `GET /emergency-visits?awaitingClosure=true&search=<nomor kunjungan K1>` | `RX` `200`; tindak lanjut `Executed`; `GET /emergency-visits/{K1}` → `visitStatus` **7** (tidak 9). Daftar memuat tepat satu baris `K1` dengan `isAwaitingClosure: true` dan `awaitingClosureReason` === `K-OBS` |
| `061-S14` | `K1`: `PATCH /emergency-observations/{observasi K1}/observation-status` `{ "observationStatus": 2, "notes": "membaik sesudah penanganan" }`; lalu `GET /emergency-observations/{id}`, `GET /emergency-visits/{K1}`, dan Q-VISIT | `200`; observasi `observationStatus` 2, `completionSummary` === `"membaik sesudah penanganan"`; kunjungan `visitStatus` 9; Q-VISIT: `ClosedByDispositionId` = id tindak lanjut `K1`, `UpdateBy` = id akun `KLINIS`, `EncounterStatus` 9 |
| `061-S15` | Siapkan `K2`. `PATCH /emergency-observations/{observasi K2}/observation-status` `{ "observationStatus": 4 }`; lalu `GET /emergency-visits/{K2}` dan Q-VISIT | `200`; observasi 4; kunjungan 9; `ClosedByDispositionId` = id tindak lanjut `K2` |
| `061-S16` | Siapkan `K3`. `PATCH /emergency-visits/{K3}/complete` `{ "notes": "Uji S16" }`; lalu `GET /emergency-visits/{K3}` | `409`, `message` === `K-OBS`; kunjungan tetap 7; observasi `K3` tetap 3. **`K3` jangan diselesaikan** — dipakai blok C dan D |

### Blok B — kunjungan uji S6 (`D-S6`), urutan mengikat (aturan A15)

Siapkan dulu `K4` (`RA`) — kakinya dipakai sebagai pembanding S18 selagi kunjungannya masih berjalan.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `061-S17` langkah 1–3 | Akun `KLINIS`, observasi `D-S6`, berurutan: (1) `{ "observationStatus": 2 }`; (2) `{ "observationStatus": 3 }`; (3) `{ "observationStatus": 2, "notes": "<1.200 karakter>" }` pada `PATCH /emergency-observations/{id}/observation-status`. Sesudah tiap permintaan: `GET /emergency-observations/{id}` | Ketiganya `409`, `message` === `K-R18` (permintaan 3 **bukan** `400`). Sesudah tiap permintaan: observasi tetap 3, `escalationReason` dan `completionSummary` sama dengan sebelum langkah 1; kunjungan `D-S6` tetap 9 (atau 8) |
| `061-S18` | (a) Q-PEMANTAUAN observasi `D-S6` → `POST /emergency-observation-details` `{ "emergencyObservationId": "<observasi D-S6>", "clinicalConditionSummary": "Uji S18" }` → Q-PEMANTAUAN lagi. (b) Pembanding: `POST` yang sama pada observasi Aktif `K4`. (c) Bila observasi `D-S6` punya baris pemantauan: `GET /emergency-observation-details/{id baris}`, lalu `PUT /emergency-observation-details/{id baris}` dengan nilai yang **sama persis** dengan hasil `GET` | (a) `409`, `message` === `K-R21`; jumlah sesudah = jumlah sebelum. (b) `200`; satu baris baru pada observasi `K4`. (c) **Bukan** `409` `K-R21` — `PUT` berperilaku seperti sebelum perubahan ini (catat kode dan pesan apa adanya). Bila tak ada baris: kaki (c) `NOT RUN` beserta alasannya |
| `042-U12` | **Layar, 1440 × 900.** Ruang Kerja Pemeriksaan `D-S6` → tab Observasi → pilih periode *Dieskalasi*. Ukur, lalu klik paksa (`force: true`) tombol *Selesaikan*; rekam jaringan sejak klik sampai 1500 ms sesudahnya. **Jangan menekan *Batalkan*** | Kartu pasien *Selesai* (atau *Dibatalkan*). Tombol yang tampil: *Selesaikan* dan *Batalkan*; tidak ada *Eskalasi*. *Selesaikan* `disabled`; *Batalkan* tidak `disabled`. Elemen keterangan di bawah deret tombol: `innerText` === `K-R18`, terlihat (`offsetParent !== null`, tinggi > 0) — bukan atribut `title`. Klik paksa: tidak ada modal terbuka, **nol** permintaan `PATCH …/observation-status` |
| `061-S17` langkah 4 | API: `{ "observationStatus": 4 }`; lalu `GET /emergency-observations/{id}`, `GET /emergency-visits/{D-S6}` | `200`; observasi 4; kunjungan tetap 9 (atau 8) — status kunjungan tidak berubah dan tidak ada penutupan baru |

### Blok C — `FE-IGD-041` U2, U6, U7, R (layar, 1440 × 900, akun `KLINIS`)

Prasyarat: minimal 11 kunjungan menunggu penutupan di dev, termasuk `K3`. **Jangan memakai pencarian nama pasien atau
nomor rekam medis** — backend tidak mencari kedua ruas itu.

| ID | Langkah | Yang diharapkan | Bukti wajib |
| --- | --- | --- | --- |
| `041-U2` | Daftar Pengkajian → saringan *Penutupan Kunjungan* = *Menunggu penutupan* | Permintaan `GET /emergency-visits` membawa `awaitingClosure=true`. Tidak ada kolom berjudul PENUTUPAN. Tiap baris memuat lencana *Tindak lanjut ditetapkan* dan, di bawahnya, kotak *"Menunggu penutupan — <alasan>"*; baris `K3` (bila di halaman 1, atau cari nomornya) beralasan `K-OBS`. Pembungkus tabel: `scrollLeft` = 0, `scrollWidth` − `clientWidth` ≤ 0; tepi kanan setiap kotak penanda ≤ tepi kanan pembungkus | PNG; jaringan; JSON ukuran (`clientWidth`, `scrollWidth`, `scrollLeft`, `getBoundingClientRect().right` tiap kotak dan pembungkus) |
| `041-U6` | (a) Saringan aktif, cari nomor kunjungan **lengkap** `K3`. (b) Ganti pencarian menjadi `IGD`, lalu pindah ke halaman 2. (c) Di halaman 2, ubah *Status Kunjungan* | (a) Satu baris, berpenanda; permintaan membawa `search` dan `awaitingClosure=true`. (b) Lebih dari sepuluh baris; permintaan halaman 2 membawa `pageNumber=2`, `awaitingClosure=true`, `search=IGD`; semua baris halaman 2 berpenanda. (c) Permintaan berikutnya `pageNumber=1` | Jaringan ketiga langkah; PNG (a) dan (b) |
| `041-U7` | Pilih *Semua kunjungan*. Cari nomor `K3` tanpa saringan; lalu kosongkan pencarian dan buka halaman yang **tidak** memuat baris tertahan | Baris `K3` berkotak penanda di bawah lencana; baris lain hanya lencana. Tidak ada kolom PENUTUPAN. `scrollWidth` pembungkus tabel sama pada kedua tampilan | PNG kedua tampilan; JSON `scrollWidth` |
| `041-R` | Ulang singkat U1, U3, U4, U5 laporan `FE-IGD-041` §6.1 | U1: pilihan *Semua kunjungan* dan *Menunggu penutupan*. U3: *"N kunjungan menunggu penutupan"* dengan N = jumlah pada baris paginasi, dan petunjuk === `K-041-HINT`. U4: kembali ke *Semua kunjungan* lalu Reset — permintaan tanpa `awaitingClosure`, baris jumlah hilang. U5: *Menunggu penutupan* + rentang tanggal tanpa data → judul === `K-041-KOSONG` | PNG tiap butir; jaringan U4 |

### Blok D — `FE-IGD-042` acceptance 10–13 (layar, 1440 × 900, akun `KLINIS`)

Pada setiap skenario layar: sebelum aksi, pasang penanda `window.__ujiMvp8 = Date.now()`; "tanpa memuat ulang
halaman" berarti penanda itu masih ada sesudah aksi.

| ID | Kriteria | Langkah | Yang diharapkan |
| --- | ---: | --- | --- |
| `042-U13` | 13 (dan regresi U6) | `K3` (masih `Disposed`, tertahan observasi Dieskalasi). Ruang Kerja → tab Observasi → periode *Dieskalasi* → *Selesaikan* → baca modal → isi kesimpulan *"membaik sesudah penanganan, layak pulang"* → tombol *Selesaikan* di modal | Sebelum aksi: kartu pasien *Tindak lanjut ditetapkan*; *Selesaikan* dan *Batalkan* tidak `disabled`; tidak ada keterangan di bawah deret tombol. Modal memuat `K-SELESAI-ESK-D` dan **tidak** memuat *"Menunggu Tindak Lanjut"*. `PATCH …/observation-status` `200`; periode *Selesai* dengan kesimpulan itu; ada `GET /emergency-visits/{K3}` sesudah `PATCH`; kartu pasien *Selesai*; penanda `window` masih ada. Q-VISIT: `VisitStatus` 9, `ClosedByDispositionId` = tindak lanjut `K3` |
| `042-U7` | 6 (regresi) | `K4` (*Sedang ditangani*, observasi Aktif). Tab Observasi → periode *Sedang berjalan*. Buka modal *Selesaikan* lalu *Batal*; buka modal *Eskalasi* lalu *Batal* | Ketiga tombol tidak `disabled`; tidak ada keterangan di bawah deret tombol. Modal *Selesaikan* memuat `K-SELESAI`; modal *Eskalasi* memuat `K-ESKALASI`. Nol `PATCH` |
| `042-U9` | 12 | Masih pada layar `K4` dari `042-U7` (**jangan dimuat ulang**). Dari skrip Node dengan token `KLINIS`: `PATCH /emergency-visits/{K4}/visit-status` `{ "visitStatus": 8, "notes": "Uji 042-U9" }`. Lalu di layar: *Selesaikan* → isi kesimpulan *"uji 042-U9"* → *Selesaikan* di modal | `PATCH visit-status` `200`, `visitStatus` 8. `PATCH …/observation-status` `409`, `message` === `K-R18`. Modal **tetap terbuka**; kotak pesan merah di dalam modal `innerText` === `K-R18`; isian kesimpulan masih *"uji 042-U9"*. Catat kalimat modal yang tampil (layar masih memakai status lama — diharapkan `K-SELESAI`) |
| `042-U10` | 10 | Tutup modal, **muat ulang** halaman `K4` → tab Observasi → periode *Sedang berjalan*. Klik paksa *Selesaikan* lalu *Eskalasi*; rekam jaringan sejak klik pertama sampai 1500 ms sesudah klik kedua | Kartu pasien *Dibatalkan*. *Selesaikan* dan *Eskalasi* `disabled`; *Batalkan* tidak `disabled`. Satu elemen keterangan di bawah deret tombol, `innerText` === `K-R18`, terlihat tanpa kursor (bukan `title`). Klik paksa: nol modal, nol permintaan ke `/api/**` |
| `042-U11` | 11 | Masih `K4`: *Batalkan* → baca modal → *Batalkan* di modal; lalu `GET /emergency-visits/{K4}` | Modal memuat `K-BATAL`. `PATCH …/observation-status` badan `observationStatus` 4 → `200`; periode *Dibatalkan*; kartu pasien tetap *Dibatalkan*; `visitStatus` tetap 8 |

### Blok E — regresi singkat `FE-IGD-042` U1–U8 dan `BE-IGD-061` S19 pada `K6`

U6 dibuktikan oleh langkah modal `042-U13`; U7 oleh `042-U7`.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `042-U1` | `K5` (tindak lanjut `Confirmed`). Layar → tab Tindak Lanjut → *Jalankan* | Modal memuat `K-JALANKAN`; tidak memuat *"belum menyelesaikan kunjungan"* |
| `042-U2` | Konfirmasi *Jalankan* | `PATCH …/disposition-status` `200`; kartu pasien *Tindak lanjut ditetapkan*; penanda `window` masih ada |
| `042-U3` | Pindah ke tab Observasi → periode *Sedang berjalan*; klik paksa *Eskalasi* | *Eskalasi* `disabled`, keterangan `innerText` === `K-R14`; *Selesaikan* dan *Batalkan* tidak `disabled`; nol modal dan nol permintaan sesudah klik |
| `042-U4` | *Selesaikan* | Modal memuat `K-SELESAI-D`; tidak memuat *"Menunggu Tindak Lanjut"* |
| `042-U5` | Isi kesimpulan *"tanda vital stabil"* → *Selesaikan* | `200`; periode *Selesai* dengan kesimpulan itu; kartu pasien *Selesai*; penanda `window` masih ada |
| `042-U8` | `K6` (tindak lanjut `Confirmed`). Buka tab Observasi (kartu *Sedang ditangani*). Dari skrip Node: `RX` pada tindak lanjut `K6`. Di layar tanpa memuat ulang: *Eskalasi* → isi alasan *"uji 042-U8"* → *Eskalasi* di modal | `RX` `200`. `PATCH …/observation-status` `409`, `message` === `K-R14`; kotak pesan di dalam modal === `K-R14`; alasan masih *"uji 042-U8"* |
| `061-S19-S7` | API `K6`: `{ "observationStatus": 3, "notes": "pasien memburuk" }`; lalu `GET` observasi dan kunjungan | `409`, `message` === `K-R14`; observasi tetap 1, `escalationReason` kosong; kunjungan 7 |
| `061-S19-S8` | `K6`: eskalasi dengan catatan 1.200 karakter | `409`, `message` === `K-R14` — **bukan** `400` |
| `061-S19-S9` | `K6`: `{ "observationStatus": 2, "notes": "<1.200 karakter>" }` | `400`, `message` === `K-CATATAN`; observasi tetap 1; kunjungan 7 |
| `061-S19-S1` | `K6`: `{ "observationStatus": 2, "notes": "tanda vital stabil" }`; lalu `GET` dan Q-VISIT | `200`; observasi 2, `completionSummary` terisi; kunjungan 9; `ClosedByDispositionId` = tindak lanjut `K6`; `UpdateBy` = akun `KLINIS`; `EncounterStatus` 9 |

### Blok F — sisa regresi `BE-IGD-061` S19 (API)

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `061-S19-S5` | `K7` (dua penahan: observasi Aktif + kepergian berjalan). Selesaikan observasinya `{ "observationStatus": 2, "notes": "uji S5" }`; lalu `GET /emergency-visits?awaitingClosure=true&search=<K7>` | `200`; observasi 2; kunjungan tetap 7; `awaitingClosureReason` === *"Masih ada proses kepergian pasien yang belum selesai."* |
| `061-S19-S11` kaki `cancel` | `K7`: `PATCH /emergency-departures/{id}/cancel` (beralasan) | `200`; kunjungan 9 |
| `061-S19-S11` kaki `reject-handover` *(opsional)* | `K10`: `POST /emergency-departures/{id}/reject-handover` `{ "reason": "Uji S11" }` | `200`; kunjungan tertutup bila itu penahan terakhir. `403` kewenangan unit → `NOT RUN — tertahan BE-IGD-039` |
| `061-S19-S10` | `K8` (*Dalam observasi*): `{ "observationStatus": 2 }`. `K9` (*Sedang ditangani*): `{ "observationStatus": 3, "notes": "uji S10" }`. Sesudah masing-masing `GET` kunjungan | `K8`: `200`, kunjungan 6. `K9`: `200`, kunjungan 4 — perilaku lama, tanpa `K-R14` maupun `K-R18` |

**Sisa data uji.** `K8` (*Menunggu keputusan*) dan `K9` (*Sedang ditangani*) tetap berjalan sesudah putaran ini;
`K10` bila dibuat. Catat nomornya pada laporan.

---

## 7. Bentuk bukti mentah

Folder **baru**: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-gabungan-mvp-8-2026-10-04/`

| Berkas | Isi |
| --- | --- |
| `<ID>.json` | `{ id, akun (peran, bukan email), percobaan, mulai, selesai, viewport, nextjsPortalNull, data{}, langkah[], jaringan[], kueri[], pemeriksaan{ <nama>: { harapan, teramati, lulus } }, putusan, alasan }` — `jaringan[]` berisi seluruh permintaan ke `/api/**`; badan login disamarkan |
| `<ID>.png` | Tangkapan layar viewport menurut aturan A9 (skenario layar) |
| `persiapan.json` | Hasil P1–P5: SHA, `git status --short`, waktu ubah berkas, waktu `BUILD_ID`, proses pada port 3000 |
| `P6-<akun>.json` | Respons `GET /auth/permissions` tiap akun (aturan A4) |
| `P7.json` | Keluaran kueri D-S6 sebelum `061-S17` |
| `data-uji.json` | Pasien (nama dan RM), nomor encounter, nomor kunjungan, dan id observasi/tindak lanjut `K1`…`K10` serta `D-S6`, beserta resep yang dipakai |
| Skrip | Semua skrip yang dijalankan, apa adanya. Kredensial hanya lewat `process.env` |

---

## 8. Rekap skenario

| Task | Skenario | Jumlah |
| --- | --- | ---: |
| `BE-IGD-061` | S13, S14, S15, S16, S17, S18 | 6 |
| `BE-IGD-061` | S19: S1, S5, S7, S8, S9, S10, S11 | 7 |
| `FE-IGD-041` | U2, U6, U7, R | 4 |
| `FE-IGD-042` | U9, U10, U11, U12, U13 (acceptance 10–13) | 5 |
| `FE-IGD-042` | Regresi U1, U2, U3, U4, U5, U7, U8 (U6 lewat `042-U13`) | 7 |
| **Jumlah** | | **29** |

---

## 9. Pelaporan

Tulis laporan baru di folder ini: `2026-10-04-laporan-uji-gabungan-mvp-8.md`. Berkas panduan ini, laporan task,
roadmap, dan dokumen blueprint lain **tidak disunting** — penandaan status dilakukan pengembang sesudah memeriksa bukti
mentah.

Isi laporan:

1. Metadata: tanggal, pelaksana, SHA frontend dan backend, `git status --short` frontend dari P1, waktu `BUILD_ID`,
   bukti layar dilayani hasil build (`nextjsPortalNull` pada semua skenario layar), viewport, basis data dev **tanpa**
   alamat host.
2. Akun: peran yang dipakai tiap langkah (bukan email), dan pernyataan bahwa Akses Role tidak diubah.
3. Tabel per skenario: ID, percobaan ke berapa, `PASS` / `FAIL` / `NOT RUN`, kode status dan kalimat yang teramati,
   nama berkas bukti.
4. Data uji: isi `data-uji.json` dalam bentuk tabel, termasuk jumlah baris `P7`.
5. Setiap penyimpangan dari panduan ini: data pengganti, percobaan ulang, langkah yang dilewati.

| Task | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| --- | --- | ---: | ---: | ---: | ---: |
| `BE-IGD-061` | S13–S18 | 6 | | | |
| `BE-IGD-061` | S19 (7 butir) | 7 | | | |
| `FE-IGD-041` | U2, U6, U7, R | 4 | | | |
| `FE-IGD-042` | U9–U13 | 5 | | | |
| `FE-IGD-042` | Regresi U1–U5, U7, U8 | 7 | | | |
| **Jumlah** | | **29** | | | |
