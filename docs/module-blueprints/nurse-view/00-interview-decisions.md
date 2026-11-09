# Nurse View — Interview Decisions

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` |
| Revision | `5` — **Amendment pass, draft** (2026-11-05): tablet bed dan panggilan pasien, ditambah `NV-DEC-034`–`056`; lihat [Amendment revision 5](#amendment-revision-5--tablet-bed-dan-panggilan-pasien). Sebelumnya `4` — Delta kontrak `0.1.2` (2026-11-03), ditambah `NV-DEC-033`; `3` — Delta kontrak `0.1.1` (2026-11-02), ditambah `NV-DEC-031`, `032`; `2` — Closure pass (2026-10-28), ditambah `NV-DEC-027`, `028`; `1` — Scope pass |
| Status | `draft` |
| Product/domain owner | **Rizki Gunawan** |
| Owner modul tetangga | Rawat Inap: Muhammad Hamzah · Gizi: Kepala Instalasi Gizi / Kepala Unit Gizi (`GIZ-DEC-014`) |
| Backend SHA | `a6f977a3` (branch `rizkiG`) |
| Frontend SHA | `76b0c6e4b` (branch `RizkiV2`) |
| Masukan | [`00-analisa-awal.md`](./00-analisa-awal.md) revision `0` |
| Tanggal | 2026-10-28 |

> **Catatan audit.** Scope dikunci **sebelum** `trace-existing-capabilities` dijalankan. Audit
> formal sudah dijalankan pada 2026-10-28 di SHA yang sama:
> [`01-existing-capability-map.md`](./01-existing-capability-map.md). Dua keputusan bertumbukan
> dengan source (`NV-DEC-012` header perawat jaga, `NV-DEC-009`/`016` pola pairing) dan menunggu
> closure pass. **Closure pass selesai 2026-10-28** (`NV-DEC-021` s.d. `025`); kedua conflict
> tertutup dari sisi owner. Tiga restu eksternal (`NV-OQ-015`–`017`) seluruhnya tertutup per 2026-10-28 (`NV-DEC-027`, `029`, `030`).

> **Cara membaca rekomendasi.** Setiap pertanyaan disertai rekomendasi yang merujuk praktik umum
> sistem rumah sakit lain, misalnya papan status bangsal elektronik (*electronic patient
> whiteboard* / *ward dashboard*) dan sistem panggil perawat (*nurse call*). Rujukan itu
> **pembanding**, bukan kebijakan rumah sakit ini. Keputusannya tetap milik owner.

> **Perubahan scope di tengah wawancara.** Pada pertanyaan 4, owner menyederhanakan modul:
> **tidak ada interaksi pasien**. Nurse View menjadi display pemantauan perawat, dibedakan per
> lantai. `NV-DEC-001`, `002`, dan `003` karena itu `superseded`; penggantinya `NV-DEC-004` dan
> `005`. Keputusan lama tidak dihapus.

> **Amendment pass revision 5 (2026-11-05, disetujui `NV-DEC-055`).** Owner membuka kembali dua kandidat slice:
> tablet pasien per bed dan panggilan pasien ke perawat. Selama amendment belum disetujui, scope,
> keputusan, dan kontrak `0.1.2` di bawah tetap menjadi acuan 14 task MVP yang sudah selesai.
> Rinciannya di [Amendment revision 5](#amendment-revision-5--tablet-bed-dan-panggilan-pasien).

## Scope dan Outcome

**Batas scope.** Nurse View adalah display pemantauan untuk perawat, satu tampilan per lantai.
Display ini hanya **membaca** data pasien rawat inap per bed dari modul yang sudah ada. Pasien
tidak berinteraksi dengan sistem ini.

**Contoh.** Display di nurse station lantai 3 menampilkan seluruh bed lantai 3. Bed 301-A
terisi pasien dengan EWS terakhir 6 (risiko sedang) dan perawat penanggung jawab Sari. Perawat
melihatnya dari jauh tanpa membuka rekam medis satu per satu.

| Di dalam scope | Di luar scope — milik modul lain |
|---|---|
| Display pemantauan per lantai: bed, penghuninya, perawat, ringkasan TTV/EWS dan peringatan (baca saja) | Pencatatan TTV dan pengkajian — Clinical Management / `rawat-inap/keperawatan` (Muhammad Hamzah) |
| Pengaturan perangkat display per lantai | Episode, penempatan bed, penanggung jawab — `rawat-inap/episode-rawat-inap` (Muhammad Hamzah) |
| Isi kartu bed dan header perawat jaga — `NV-DEC-007`, `012`, `015`, `018`–`020` | Roster perawat — HR Scheduling |
| Tanda TTV lewat jadwal dan pengaturan intervalnya — `NV-DEC-010` | Diet, menu, produksi, distribusi — Gizi |
| | Integrasi monitor pasien (alat) — kandidat slice berikutnya, `NV-DEC-011` |

## Glossary

| Istilah | Arti di modul ini |
|---|---|
| Display Nurse View | TV di nurse station yang menampilkan papan pemantauan satu lantai. Tidak bisa disentuh atau diklik |
| Lantai | Nilai `MstRoom.FloorCode`. Satu lantai bisa berisi beberapa bangsal |
| Bangsal | Unit pelayanan rawat inap (`MstRoom.ServiceUnitId`) |
| Kartu bed | Kotak informasi satu bed di papan |
| EWS | *Early Warning Score* — skor peringatan dini dari TTV, dihitung modul Clinical. Tingkatnya `Low`, `Medium`, `High`, `Critical` |
| Perawat PJ | Perawat penanggung jawab episode (`InpNurseAssignment` aktif) |
| Perawat jaga | Perawat yang terjadwal di roster HR pada shift berjalan |
| Lewat jadwal | TTV terakhir lebih tua daripada interval yang disahkan untuk tingkat EWS-nya |
| Data basi | Keadaan ketika display tidak tersambung lebih dari 2 menit, sehingga isi layar mungkin tidak terbaru |
| Pairing | Memasangkan TV ke data perangkat lewat kode yang tampil di layar TV |

## Aktor dan Tanggung Jawab

| Aktor | Yang dikerjakan | Wewenang |
|---|---|---|
| Admin IT / Administrator | Mendaftarkan, memasangkan, menonaktifkan perangkat display; memasukkan angka interval yang sudah disahkan | Satu-satunya pengelola data Nurse View |
| Komite Keperawatan | Mengesahkan angka interval ukur ulang TTV per tingkat EWS | Pemilik kebijakan klinis `NV-OQ-010` |
| Perawat, kepala ruang | Melihat papan | Penonton; tidak mengubah apa pun lewat Nurse View |
| Akun perangkat display | Membaca data papan untuk satu lantai | Hanya lantai yang dipasangkan; tidak bisa membaca lantai lain |
| Pasien | — | Tidak berinteraksi (`NV-DEC-004`) |

## Business Rules dan Invariants

1. **Nurse View tidak menulis data klinis maupun data Rawat Inap.** Satu-satunya data miliknya:
   perangkat display dan pengaturan interval.
2. **Penghuni bed selalu dibaca dari penempatan aktif Rawat Inap.** Saat pasien pindah atau pulang,
   kartu ikut berubah. Display tidak menyimpan salinan pasien.
3. **Data minimal di layar publik, dijaga di backend.** Jawaban API untuk akun perangkat **tidak
   memuat** nama lengkap, No. RM, nama alergen, atau isi klinis selain yang tampil. Penyamaran
   tidak boleh hanya dilakukan di frontend.
4. **Satu perangkat, satu lantai.** Akun perangkat lantai 3 ditolak bila meminta data lantai 4.
5. **Aman-gagal:** tanpa interval yang disahkan, tidak ada tanda "lewat jadwal"; tanpa koneksi,
   ada pita data basi.

## State dan Transition

Hanya perangkat display yang punya status sendiri. Status kartu bed diturunkan dari modul lain.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
Diperbarui closure pass (`NV-DEC-022`): alur kode pairing diganti login yang dibuat admin.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Daftarkan perangkat | `BelumAdaLogin` | Admin IT | Kode perangkat unik, lantai dipilih |
| `BelumAdaLogin` | Buat login (kata sandi tampil sekali) | `Aktif` | Admin IT | Perangkat aktif |
| `Aktif` | Atur ulang login | `Aktif` | Admin IT | Kata sandi lama langsung tidak berlaku |
| `Aktif` | Nonaktifkan | `Nonaktif` | Admin IT | Login langsung ditolak |
| `Nonaktif` | Aktifkan kembali | `Aktif` | Admin IT | Wajib atur ulang login; kata sandi lama tetap tidak berlaku |

<details><summary>Tabel lama revision 1 (superseded oleh `NV-DEC-022`)</summary>

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Daftarkan perangkat | `BelumDipasangkan` | Admin IT | Kode perangkat unik, lantai dipilih |
| `BelumDipasangkan` | Pasangkan dengan kode pairing | `Aktif` | Admin IT | Kode pairing cocok dan belum kedaluwarsa |
| `Aktif` | Nonaktifkan | `Nonaktif` | Admin IT | — ; token langsung tidak berlaku |
| `Nonaktif` | Aktifkan & pasangkan ulang | `Aktif` | Admin IT | Pairing baru; token lama tetap tidak berlaku |

</details>

Status *online*/*offline* adalah hasil pengamatan (waktu terakhir tersambung), bukan status yang
diubah orang.

## Skenario Normal dan Exception

| Skenario | Perilaku |
|---|---|
| Perawat menyimpan TTV baru | Kartu bed diperbarui lewat push dalam hitungan detik |
| Perawat membatalkan TTV | Kartu kembali ke TTV sah sebelumnya |
| Pasien pindah 301-A (lantai 3) ke 402-B (lantai 4) | Kartu pasien hilang dari display lantai 3 dan muncul di lantai 4; 301-A tampil dengan status barunya |
| Pasien pulang | Bed tampil "Dibersihkan" atau "Kosong" sesuai status Rawat Inap |
| Koneksi putus > 2 menit | Pita abu-abu "Data terakhir HH:MM — koneksi terputus" |
| Perangkat dinonaktifkan saat sedang tampil | Permintaan berikutnya ditolak (kode 401/403); TV kembali ke layar pairing |
| Roster shift berjalan belum terbit | Header bangsal menampilkan "Roster shift ini belum tersedia" (**Assumption**, perlu dikonfirmasi) |
| Kamar tanpa `FloorCode` | Tidak tampil di display mana pun; perlu dilaporkan ke admin master data |

## Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `NV-FE-001` | Kelompok per bangsal, kartu kritis di atas, ganti halaman otomatis | Rizki Gunawan | `approved` | Tidak boleh diubah developer | `NV-DEC-017` |
| `NV-FE-002` | Keterbacaan ±3 meter | Rizki Gunawan | `approved` | Ukuran huruf/kartu bebas selama terbaca dari ±3 m | `NV-DEC-017` |
| `NV-FE-003` | Merah berkedip untuk kritis, tanpa suara | Rizki Gunawan | `approved` | Intensitas dan ritme kedip `DEV_DISCRETION` | `NV-DEC-013` |
| `NV-FE-004` | Warna, ikon, jarak, durasi tiap halaman, tata letak header | Developer | `DEV_DISCRETION` | Mengikuti design token dan komponen Quilvian yang sudah ada | `NV-DEC-017` |
| `NV-FE-005` | Route halaman display | Developer | `DEV_DISCRETION` | Pola route tanpa sidebar seperti `app/queue-display` | `NV-DEC-009` |
| `NV-FE-006` | Kapasitas strip Panggilan Aktif di TV (amendment revision 5) | Developer | `DEV_DISCRETION` | Jumlah item bebas, tetapi panggilan yang tidak muat wajib ditandai "+k panggilan lain", dan urutan mengikuti `NV-DEC-038` | `NV-DEC-044` |
| `NV-FE-007` | Jeda pengingat bunyi dan lama tunda "Matikan bunyi" di Konsol Panggilan (amendment revision 5) | Developer | `DEV_DISCRETION` | Jeda pengingat 15–60 detik. Lama tunda bebas, tetapi konsol wajib menampilkan sampai kapan pengingat ditunda, dan panggilan baru tetap berbunyi | `NV-DEC-045` |

### Di luar scope — kandidat slice berikutnya

Dicatat agar tidak hilang, **bukan** janji pengerjaan.

| Kandidat | Asal | Catatan bila kelak dibuka |
|---|---|---|
| Tablet pasien per bed | Kebutuhan awal 2026-10-28 | Pola akun perangkat Kiosk / `MstQueueDisplayDevice` sudah ada. **Dibuka kembali 2026-11-05** — Amendment revision 5, disetujui `NV-DEC-055` |
| Panggilan pasien ke perawat | Kebutuhan awal | Hanya sebagai **pelengkap** bel kabel (pernah diputuskan di `NV-DEC-003`, kini `superseded`). Nama `NurseCall` sudah dipakai antrean rawat jalan dengan arti terbalik. **Dibuka kembali 2026-11-05** — Amendment revision 5, disetujui `NV-DEC-055` |
| Pemesanan makan oleh pasien | Kebutuhan awal | Terhalang `GIZ-DEC-013`; butuh keputusan Kepala Instalasi Gizi |
| Jadwal visite dokter untuk pasien | Kebutuhan awal | Rencana visite belum ada sebagai data; `CliPhysicianVisit` hanya riwayat |

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `NV-DEC-001` | Decision | Batas scope awal (panggilan pasien, display perawat, tablet pasien, pemesanan makan, perangkat, laporan waktu tanggap) disetujui | Rizki Gunawan | `superseded` oleh `NV-DEC-004` | Rizki Gunawan, 2026-10-28 | Pertanyaan 1 |
| `NV-DEC-002` | Decision | `blueprint_shape: SINGLE` atas scope awal | Rizki Gunawan | `superseded` oleh `NV-DEC-005` | Rizki Gunawan, 2026-10-28 | Pertanyaan 2 |
| `NV-DEC-003` | Decision | Menutup `NV-OQ-001`: tablet melengkapi bel kabel, tanpa integrasi hardware di MVP | Rizki Gunawan | `superseded` oleh `NV-DEC-004` — tablet keluar scope | Rizki Gunawan, 2026-10-28 | Pertanyaan 3; pembanding UL 1069, DIN VDE 0834 |
| `NV-DEC-004` | Decision | **Scope disederhanakan.** Tidak ada interaksi pasien. Nurse View = display pemantauan perawat per lantai, baca saja. Tablet pasien, panggilan pasien, pemesanan makan, dan jadwal visite untuk pasien keluar scope dan dicatat sebagai kandidat slice berikutnya | Rizki Gunawan | `approved` untuk revision 1–4; bagian tablet pasien dan panggilan pasien `superseded` oleh `NV-DEC-035` (2026-11-05). Pemesanan makan dan jadwal visite tetap di luar scope | Rizki Gunawan, 2026-10-28 | Pertanyaan 4 dan konfirmasinya; pembanding *electronic patient whiteboard* |
| `NV-DEC-005` | Decision | `blueprint_shape: SINGLE`, `shape_decided_by: USER_CONFIRMED`, tetap berlaku untuk scope baru. Hanya satu rumpun yang tersisa (display pemantauan), dan hampir seluruh datanya milik modul lain; tidak ada dasar pemecahan. Kemampuan tanpa rumpun: tidak ada | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Turunan `NV-DEC-002` + `NV-DEC-004` |
| `NV-DEC-006` | Decision | **Satu display = satu lantai = semua bed rawat inap di lantai itu.** Lantai ditentukan dari `MstRoom.FloorCode`. Bila satu lantai berisi beberapa bangsal (`ServiceUnitId`), layar mengelompokkannya per bangsal. **Contoh:** lantai 3 berisi Bangsal Anak (8 bed) dan Bangsal Bedah (12 bed); display lantai 3 menampilkan 20 bed dalam dua kelompok. **Prasyarat:** `FloorCode` setiap kamar rawat inap aktif terisi; kamar tanpa `FloorCode` tidak muncul di display mana pun dan perlu dilaporkan | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 5 |
| `NV-DEC-007` | Decision | **Isi kartu tiap bed di MVP:** (1) status bed, pasien penghuni, perawat penanggung jawab episode; (2) TTV dan skor EWS terakhir berwarna, jam pengukuran, tanda lewat jadwal ukur ulang, peringatan kritis; (3) ikon keselamatan: risiko jatuh, alergi, isolasi. **Tidak masuk MVP:** DPJP, diet aktif, rencana pulang. **Contoh kartu:** "301-A · Tn. B. · Perawat Sari · EWS 6 (kuning) pukul 08.10 · ⚠ risiko jatuh · alergi" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 6; pembanding *ward dashboard* |
| `NV-DEC-008` | Decision | Menutup `NV-OQ-009`. **Identitas pasien di display disamarkan:** inisial nama + jenis kelamin + umur. Nama lengkap dan No. RM **tidak** tampil di display. **Contoh:** pasien "Budi Santoso", laki-laki, 64 tahun, tampil "B.S. · L · 64 th". Mengikuti pola `ShowPatientName = false` pada display antrean, dan prinsip data minimal (UU Pelindungan Data Pribadi) | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 7 |
| `NV-DEC-009` | Decision | **Display adalah TV pasif yang masuk dengan akun perangkat per lantai**, mengikuti pola display antrean (`MstQueueDisplayDevice`: kode pairing + token perangkat). Tidak ada klik, sentuh, atau halaman detail. Perawat yang butuh detail membuka rekam medis lewat menu yang sudah ada. **Contoh:** TV di nurse station lantai 3 dinyalakan, menampilkan kode pairing, admin memasangkannya ke "Lantai 3", lalu papan tampil tanpa login perawat | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 8 |
| `NV-DEC-010` | Decision | **Tanda "TTV lewat jadwal" memakai interval per tingkat risiko EWS** (`Low`/`Medium`/`High`/`Critical`, enum `EwsRiskLevel`), disimpan sebagai pengaturan. **Angkanya kebijakan klinis** dan wajib disahkan Komite Keperawatan. **Aman-gagal:** selama angka belum disahkan, tanda tidak tampil dan kartu hanya menampilkan umur pengukuran. **Contoh:** interval `Medium` = 4 jam; TTV terakhir 08.00 dengan EWS `Medium`; pukul 12.00 kartu ditandai merah "lewat jadwal 0 menit", pukul 12.30 "lewat 30 menit". Pembanding: frekuensi pemantauan NEWS2 (Royal College of Physicians) | Rizki Gunawan (bentuk aturan); Komite Keperawatan (angka) | `approved` untuk bentuk; angka `NV-OQ-010` | Rizki Gunawan, 2026-10-28 | Pertanyaan 9. **Fact:** source saat ini tidak menyimpan jadwal ukur ulang TTV; `EwsMonitoringRecommendation` hanya teks |
| `NV-DEC-011` | Decision | Menutup `NV-OQ-008`. **Sumber TTV hanya TTV yang diinput perawat** ke `PatientVitalSign` lewat form yang sudah ada. Tidak ada integrasi monitor pasien di MVP; dicatat sebagai kandidat slice berikutnya. TTV yang dibatalkan (`cancel`) tidak ditampilkan. **Contoh:** perawat mengisi TTV bed 301-A pukul 10.05 di pengawasan harian; display lantai 3 menampilkannya, tanpa ada data dari alat monitor | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 10 |
| `NV-DEC-012` | Decision | **Perawat di display.** Kartu bed menampilkan perawat penanggung jawab episode (`InpNurseAssignment` aktif). Header display menampilkan daftar perawat jaga shift berjalan **per bangsal**, dibaca dari roster HR (pola `AmbilDokterJagaAktifAsync` IGD). Episode tanpa penanggung jawab tetap tampil, dengan tanda berwarna "Belum ada PJ" (sejalan `RWI-DEC-032`: tidak menahan, tapi terlihat). **Contoh:** shift pagi Bangsal Anak dijaga Sari, Dewi, dan Rina; header menampilkan ketiganya; bed 302-B yang belum punya PJ tampil "Belum ada PJ" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 11 |
| `NV-DEC-013` | Decision | **Peringatan kritis hanya visual, tanpa suara.** Kartu dengan EWS `Critical` atau peringatan kritis tampil merah, berkedip, dan naik ke urutan teratas di kelompok bangsalnya. Display tidak membunyikan apa pun karena TV pasif tidak bisa disentuh untuk mematikan bunyi. Jalur peringatan yang sudah ada (`critical-alerts`, `notify-doctor`) tetap berlaku. **Contoh:** TTV bed 305-A pukul 14.20 menghasilkan EWS 8 (`Critical`); dalam beberapa detik kartu 305-A merah berkedip dan pindah ke baris pertama Bangsal Bedah, tanpa bunyi | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 12; pembanding ECRI tentang *alarm fatigue* |
| `NV-DEC-014` | Decision | **Pembaruan didorong (*push*) lewat SignalR di hub yang sudah ada (`QueueHub`), dengan muat ulang penuh tiap 60 detik sebagai cadangan.** Bila koneksi putus lebih dari 2 menit, seluruh layar diberi pita abu-abu berisi jam data terakhir. Papan **tidak boleh** tampak normal saat datanya basi. **Contoh:** jaringan putus pukul 10.42; pukul 10.44 muncul pita "Data terakhir 10.42 — koneksi terputus"; begitu tersambung lagi, papan dimuat ulang penuh dan pita hilang | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 13 |
| `NV-DEC-015` | Decision | **Semua bed di lantai tampil, termasuk yang tidak berpenghuni**, sebagai kartu ringkas abu-abu dengan salah satu status: "Kosong", "Dipesan — pasien akan datang", "Dibersihkan", "Tidak tersedia". Statusnya dibaca dari Rawat Inap (`InpBedPlacement`, `InpBedReservation`, pembersihan bed, `BedStatus`), tidak dihitung ulang. **Contoh:** bed 303-B baru ditinggal pasien pulang dan sedang dibersihkan → kartu "Dibersihkan"; bed 303-C dipesan untuk pasien IGD → kartu "Dipesan — pasien akan datang" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 14 |
| `NV-DEC-016` | Decision | **Nurse View punya master perangkat display sendiri**, meniru pola display antrean (kode, nama, lantai, kode pairing, token perangkat, aktif/nonaktif, waktu terakhir online), **tanpa** mengubah `MstQueueDisplayDevice`. Dikelola **Administrator/IT**; kepala ruang dan perawat hanya penonton. Ini satu-satunya kelompok data milik Nurse View. **Contoh:** admin IT membuat perangkat "NV-L3-01 · TV Nurse Station Lantai 3 · lantai 3", menyalakan TV, memasukkan kode pairing yang tampil, dan TV mulai menampilkan papan lantai 3. Saat TV dicuri atau rusak, admin menonaktifkannya dan tokennya tidak berlaku lagi | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 15. **Fact:** `MstQueueDisplayDevice.NurseStationClusterId` wajib dan merujuk cluster poli rawat jalan |
| `NV-DEC-017` | Decision | **Tata letak: grid kartu per bangsal.** Yang ditetapkan owner: (1) kartu dikelompokkan per bangsal; (2) kartu kritis di urutan teratas kelompoknya; (3) terbaca dari ±3 meter; (4) bila tidak muat satu layar, berganti halaman otomatis. Selebihnya `DEV_DISCRETION` (lihat Frontend Decision Authority). **Contoh:** lantai 4 berisi 36 bed; TV memuat 24 kartu; halaman 1 (Bangsal Penyakit Dalam, 24 bed) dan halaman 2 (Bangsal Saraf, 12 bed) bergantian otomatis. Denah lantai tidak dipakai di papan ini | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 16 |
| `NV-DEC-018` | Decision | **Ikon risiko jatuh hanya untuk tingkat Tinggi**, dari hasil pengkajian risiko jatuh terakhir yang sah di modul keperawatan, sama dengan penanda kuning fisik di pasien/bed. Tingkat rendah dan sedang tidak diberi ikon. **Contoh:** pasien dewasa dengan skor Morse 50 (tinggi) → ikon kuning; skor 30 (sedang) → tanpa ikon. Skala anak mengikuti instrumen yang dipakai pengkajian, dengan aturan yang sama: hanya tingkat tinggi | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 17 |
| `NV-DEC-019` | Decision | **Ikon alergi hanya penanda "ada alergi"**, berwarna merah seperti gelang alergi, tanpa jenis maupun nama zat alergen. Sumbernya data alergi pasien yang aktif (`PatientAllergy`). **Contoh:** pasien alergi Amoxicillin → kartu hanya menampilkan ikon merah "Alergi"; rinciannya dibaca perawat di rekam medis atau gelang pasien | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 18 |
| `NV-DEC-020` | Decision | **Ikon isolasi dari kebutuhan pasien, ditambah tanda tidak cocok.** Ikon tampil bila episode `RequiresIsolation = true`. Bila pasien butuh isolasi tetapi menempati bed biasa, atau sebaliknya, kartu diberi tanda peringatan "Bed tidak sesuai isolasi", memakai logika laporan `isolation-mismatch` Rawat Inap yang sudah ada. Jenis isolasi (kontak/droplet/udara) **tidak** ditampilkan karena datanya belum ada. **Contoh:** pasien bed 304-A butuh isolasi tetapi 304-A bukan bed isolasi → kartu menampilkan ikon isolasi dan tanda "Bed tidak sesuai isolasi" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 19. **Fact:** `InpEpisode.RequiresIsolation` bertipe ya/tidak; `MstBed.IsIsolationBed`, `MstRoom.IsIsolationRoom` |
| `NV-DEC-021` | Decision | **Closure pass — menutup `NV-CQ-001`/`NV-CON-001` dan `NV-OQ-014`; memperjelas `NV-DEC-012`.** Header perawat jaga tetap dibaca dari roster HR: `TrxShiftAssignment` yang shift-nya sedang berjalan, dicocokkan ke bangsal lewat `MstServiceUnit.OrganizationUnitId`. **Aman-gagal:** bila tidak ada baris roster untuk bangsal dan shift itu, header menampilkan "Roster shift ini belum tersedia". Tidak ada penebakan dari penempatan organisasi atau sumber lain. Bila `OrganizationUnitId` bangsal kosong, header menampilkan tulisan yang sama. **Contoh:** Rabu 08.00, roster Bangsal Anak belum diisi (keadaan saat ini menurut `RWI-DEC-100`) → header "Roster shift ini belum tersedia"; kartu bed tetap menampilkan perawat PJ. Bulan depan HR mulai menerbitkan roster perawat → header otomatis berisi nama, tanpa perubahan desain | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Closure pass pertanyaan 1; `01-existing-capability-map.md` `NV-CAP-05` |
| `NV-DEC-022` | Decision | **Closure pass — menutup `NV-CQ-002`/`NV-CON-002` dari sisi owner. Mengganti alur kode pairing pada `NV-DEC-009` dan `NV-DEC-016`**; inti kedua keputusan itu (TV pasif, akun perangkat, master sendiri dikelola admin IT) tetap berlaku. TV masuk dengan **login per perangkat yang dibuat admin**, meniru display antrean: admin membuat perangkat → menekan "Buat Login" → nama pengguna dan kata sandi tampil **sekali** → diketik di TV. Admin bisa mengatur ulang login dan menonaktifkan perangkat. **Contoh:** admin membuat "NV-L3-01", menekan Buat Login, mencatat kata sandi yang tampil sekali, lalu mengetiknya di TV lantai 3. Bila TV dicuri, admin menonaktifkan NV-L3-01 dan login itu langsung ditolak. **Konsekuensi:** perubahan minimal pada berkas bersama — klaim perangkat di `AuthController`, satu policy di `Program.cs` + `ApprovedAlternativeAuthorization`, satu redirect di `proxy.js` — **wajib direstui pemilik governance/QBE** (`NV-OQ-015`) sebelum implementasi | Rizki Gunawan | `approved` (bagian owner); berkas bersama menunggu `NV-OQ-015` | Rizki Gunawan, 2026-10-28 | Closure pass pertanyaan 2; `NV-CAP-13` |
| `NV-DEC-023` | Decision | **Closure pass — menutup `NV-CQ-003`; mempersempit `NV-DEC-014`.** Push hanya untuk dua jenis kejadian: (1) **TTV disimpan atau dibatalkan**, lewat **satu** titik terbit baru yang diminta ke pemilik kode TTV (`NV-OQ-016`); (2) event bed yang sudah ada (`InpatientBedSelectionChanged`, saat reservasi/penempatan). Pulang, pindah, pembersihan, dan penunjukan perawat PJ cukup tertangkap **muat ulang penuh tiap 60 detik**. Pita data basi dan jedanya (2 menit) tetap seperti `NV-DEC-014`. **Contoh:** pukul 14.20 TTV bed 305-A disimpan dengan EWS `Critical` → kartu merah dalam hitungan detik. Pukul 14.25 pasien 303-B dipulangkan → kartu 303-B berubah paling lambat 14.26 | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Closure pass pertanyaan 3; `NV-CAP-14` |
| `NV-DEC-024` | Decision | **Closure pass — menutup `NV-CQ-004` dari sisi owner.** Nurse View diusulkan masuk registry sebagai `HealthServices` / `NurseViewManagement` / Nurse View, jenis `BUSINESS DOMAIN / MODULE`, **prefix `Nvw`** (Nurse View), owner Rizki Gunawan. Folder backend: `Areas/HealthServices/NurseViewManagement/`. **Contoh nama entity:** `NvwDisplayDevice` (perangkat display), `NvwVitalSignInterval` (interval ukur ulang TTV). Pendaftarannya ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` menunggu restu pemilik registry (`NV-OQ-017`); sebelum itu `QBE-MOD-002` tetap menghalangi pembuatan entity | Rizki Gunawan | `approved` (usulan owner); registry menunggu `NV-OQ-017` | Rizki Gunawan, 2026-10-28 | Closure pass pertanyaan 4; `NV-CAP-12`. **Fact:** prefix `Nvw` belum dipakai di registry |
| `NV-DEC-025` | Decision | **Closure pass — menutup `NV-CQ-005`; mempertegas `NV-DEC-006`.** Kamar tampil di papan cukup bila aktif dan punya `FloorCode`. `FloorPlanRoomKey` (posisi di denah) **tidak** disyaratkan, karena papan berbentuk grid. Karena itu Nurse View **tidak** memakai saringan kamar milik `InpFloorPlanQueryService.LoadAsync` apa adanya. **Contoh:** kamar 310 di lantai 3 punya `FloorCode = 3` tetapi belum digambar di denah. Pasien di 310-A tetap tampil di papan lantai 3, walaupun tidak muncul di Monitoring Bed versi denah | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Closure pass pertanyaan 5; `NV-CAP-01` |
| `NV-DEC-026` | Decision | **Mengganti butir (2) `NV-DEC-023`.** Event bed yang sudah ada **tidak** dipakai TV, karena payload `InpatientBedSelectionChanged` memuat nama lengkap pasien (`PatientName`). Akun perangkat Nurse View **dilarang** bergabung ke grup `inpatient-bed-selection:{unit}`. Perubahan bed (reservasi, penempatan, pulang, pindah, pembersihan) tertangkap muat ulang ≤ 60 detik. Push tinggal **satu**: TTV disimpan/dibatalkan (`NV-OQ-016`), dengan sinyal tanpa identitas pasien. Tidak ada permintaan perubahan kode ke `episode-rawat-inap`. **Contoh:** pukul 09.00 pasien IGD ditempatkan di 302-C; layar Pilih Bed perawat admisi menerima event seketika, sedangkan TV lantai 3 menampilkan 302-C "Terisi" paling lambat 09.01 | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan lanjutan closure pass. **Fact:** `InpBedRealtimeService.cs` baris 57 dan 78 (`PatientName = patient.FullName`) |
| `NV-DEC-027` | Decision | **Persetujuan owner Rawat Inap atas dokumen lintas modul.** Muhammad Hamzah menyetujui: (1) `TLM-NV-001` keperawatan permintaan 1 — satu titik terbit TTV di `PatientVitalSignController` (menutup `NV-OQ-016`); (2) permintaan 2 — `MapCpptProfessionType` menjadi `internal static`; (3) `TLM-NV-001` episode — Nurse View memanggil `ProjectBatchAsync` yang `internal` secara langsung. Hamzah **bukan** lead, sehingga persetujuan ini **tidak** menutup `NV-OQ-015` maupun `NV-OQ-017` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-10-28, disampaikan Rizki Gunawan | Pernyataan Rizki Gunawan di sesi 2026-10-28 |
| `NV-DEC-028` | Decision | **Blueprint Nurse View revision `1` dan kontrak `0.1.0` disetujui owner**, termasuk jawaban `NV-DEC-027`. Perubahan sesudah ini wajib menjadi revision/versi baru | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Jawaban pertanyaan approval sesi 2026-10-28 |
| `NV-DEC-029` | Decision | **Menutup `NV-OQ-017`.** Baris registry `HealthServices` / `NurseViewManagement` / Nurse View, `BUSINESS DOMAIN / MODULE`, prefix **`Nvw`** = *Nurse View*, lifecycle `ACTIVE`, disetujui **pemilik modul**, bukan lead. Ini ketentuan yang berlaku untuk modul baru, sama seperti `Emg` (IGD), `Acc` (`ACC-DEC-038`), `Inp` (`RWI-DEC-068`), dan `Bbk`. Penulisan barisnya ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` menjadi **langkah pertama task MVP-0**, karena berkas itu di luar batas tulis mode blueprint. Catatan lifecycle-nya menulis framing "propagasi persetujuan yang sudah ada (`NV-DEC-029`), bukan persetujuan baru". `NV-OQ-015` tetap milik lead | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pernyataan owner sesi 2026-10-28; preseden catatan lifecycle registry (2026-08-24 `Inp`, 2026-09-03 `Bbk`, 2026-09-08 `Acc`) |
| `NV-DEC-030` | Decision | **Menutup `NV-OQ-015`. Lead merestui perubahan enam berkas bersama** (`AuthController`, `AuthorizationPolicies`, `Program.cs`, `QueueHub`, `use-base-login.jsx`, `proxy.js`) dengan dua syarat mengikat: **(1) tidak mengganggu login secara keseluruhan** — pengguna biasa, akun Kiosk, akun display antrean, dan akun lain tetap berperilaku persis seperti sebelumnya; **(2) pola disamakan dengan display antrean** — resolver login, klaim `is_*_account`, policy alternatif, cookie, dan redirect `proxy.js` meniru alur `QueueDisplay*`. Batas tafsir syarat (2): yang ditiru adalah **alur login/runtime**, **bukan** utang teknis `QueueDisplayDeviceController` (akses DbContext langsung, kode `Max+1`) yang dilarang untuk kode baru (`QBE-SVC-001`, `QBE-CODE-002/003`). Syarat (1) dibuktikan `NV-AC-023` s.d. `027`. **Contoh:** sesudah perubahan, perawat Sari login seperti biasa dan masuk dashboard; akun Kiosk tetap ke `/kiosk`; TV antrean tetap ke `/queue-display`; hanya akun `NVW-*` yang ke `/nurse-view-display` | Lead | `approved` dengan syarat | Lead, 2026-10-28, secara lisan, disampaikan Rizki Gunawan | Pernyataan Rizki Gunawan sesi 2026-10-28 |
| `NV-DEC-031` | Decision | **Mengubah `VAL-NV-16`; kontrak naik ke `0.1.1`.** Login akun perangkat Nurse View `Inactive` dengan **kata sandi benar** dijawab `401` "Perangkat display dinonaktifkan. Hubungi IT.". Kata sandi salah tetap dijawab pesan login terkunci standar, sehingga status perangkat tidak terbaca orang yang tidak tahu sandinya. Akun lain tanpa perubahan. Ini satu-satunya pengecualian `NV-AC-027`. **Contoh:** TV lantai 3 yang dinonaktifkan admin dicoba login dengan sandi yang tersimpan; petugas membaca alasan sebenarnya, bukan "terlalu banyak percobaan login gagal" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-02 | Jawaban pertanyaan pesan login nonaktif, sesi 2026-11-02; baseline `test-with-agy/nurse-view/uji-nv-003-20261102/` |
| `NV-DEC-032` | Decision | **Memperluas daftar berkas bersama frontend `NV-DEC-030`.** Selain `use-base-login.jsx` dan `proxy.js`, route `/nurse-view-display` didaftarkan pada `root-layout-shell.jsx`, `auth-wrapper.jsx`, dan `session-activity-manager.jsx`, masing-masing tepat satu baris daftar prefix, persis seperti `/queue-display`. Fakta: tanpa `root-layout-shell` TV tampil dengan sidebar admin; tanpa `auth-wrapper` TV yang menyala saat jaringan putus terhapus sesinya dan jatuh ke layar login (`checkAuthStatus` memanggil `clearIdentity`). Tidak mengubah perilaku rute lain, karena hanya entri daftar yang dicocokkan dengan path | Rizki Gunawan | `approved` pemilik modul; penyampaian ke lead oleh Rizki | Rizki Gunawan, 2026-11-02 | Jawaban pertanyaan berkas tambahan, sesi 2026-11-02 |
| `NV-DEC-033` | Decision | **Memperjelas langkah 9 roster (`NV-DEC-021`); kontrak naik ke `0.1.2`.** Fakta source: `RosterPeriodService.AddAssignmentAsync` milik HR tidak pernah mengisi `TrxShiftAssignment.OrganizationUnitId` maupun `TrxRosterAssignment.ProfessionId`; unit tersimpan di `TrxRosterPeriod.OrganizationUnitId`, profesi di `MstEmployee.ProfessionId`. Header memakai kolom kontrak bila terisi, dan bila kosong memakai **unit periode roster** dan **profesi pegawai**. Keduanya tetap data roster dan data orang yang dirostering, bukan penebakan dari penempatan organisasi. Profesi lolos bila `IsClinicalProfession` dan `MapCpptProfessionType` = `Nurse`/`Midwife`. `RosterAvailable = true` hanya bila ada minimal satu perawat/bidan jaga; selain itu "Roster shift ini belum tersedia". `ShiftLabel` = `MstShift.ShiftName`, atau jam mulai–selesai Asia/Jakarta bila nama kosong. **Contoh:** HR menerbitkan roster Instalasi Rawat Inap untuk Sari (Perawat) dan Budi (Rekam Medis), shift Pagi 07.00–14.00; pukul 09.00 header Rawat Inap menampilkan "Sari · Pagi", tanpa Budi | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-03 | Jawaban pertanyaan sumber data roster, sesi 2026-11-03 |
| `NV-DEC-034` | Decision | **Amendment revision 5 — perangkat di nurse station: gabungan.** (1) TV tetap papan pasif berakun perangkat lantai (`NV-DEC-009` tetap), ditambah strip Panggilan Aktif. (2) **Konsol Panggilan** di PC nurse station, dibuka perawat dengan **akunnya sendiri** dan izin baru (namanya ditetapkan `design-business-module`): daftar panggilan lantai, tombol "Saya tangani", bunyi beserta tombol matikan bunyi. (3) Panggilan **ditutup di tablet bed** lewat tombol "Perawat hadir". Tidak ada akun perangkat yang bisa diklik, dan TV tetap tanpa suara (`NV-DEC-013`). Keputusan ini sekaligus menjawab siapa yang menutup panggilan. **Contoh:** konsol lantai 3 berbunyi; Sari mengklik "Saya tangani" untuk 305-A; TV menampilkan "305-A · Sari menuju"; Dewi melihat TV lalu mengambil 318-B; Sari tiba dan menekan "Perawat hadir"; 305-A hilang dari strip dan waktu tanggapnya tercatat atas nama Sari. **Alasan:** layar tablet 10 inci sekitar 4 kali lebih kecil daripada TV 43 inci sehingga tidak terbaca dari ±3 m (`NV-FE-002`); klik di akun perangkat tidak mencatat nama perawat; perangkat berakun perangkat yang bisa diklik keluar dari restu `NV-DEC-030`. **Konsekuensi:** ada layar dan izin baru; konsol harus terbuka dan login di PC; tanpa konsol, TV tetap menampilkan panggilan tetapi tanpa bunyi dan tanpa klaim | Rizki Gunawan | `approved` — berlaku sejak batas scope amendment dikunci (`NV-DEC-035`) | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan perangkat nurse station (opsi A), sesi 2026-11-05 |
| `NV-DEC-035` | Decision | **Amendment revision 5 — batas scope dikunci; menutup `NV-OQ-018`.** Di dalam scope: (1) perangkat tablet per bed, didaftarkan admin IT, login per perangkat, terikat ke satu bed; (2) pasien di tablet dibaca dari penempatan aktif Rawat Inap, dan bed kosong membuat tablet terkunci; (3) tombol panggil dan status panggilan di tablet; (4) siklus panggilan pasien beserta waktu tanggapnya; (5) strip "Panggilan Aktif" di Display Nurse View lantai yang sama; (6) Konsol Panggilan (`NV-DEC-034`); (7) aturan banyak panggilan. Di luar scope: pemesanan makan (Gizi), jadwal visite, data klinis di tablet, integrasi hardware bel kabel, integrasi monitor pasien, notifikasi ke HP perawat. Menggantikan bagian tablet dan panggilan pada `NV-DEC-004`. **Contoh:** pasien bed 305-A bisa memanggil perawat dari tablet, tetapi tidak bisa memesan makan atau melihat hasil TTV-nya di tablet | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban "pilihan saya A" atas pertanyaan 1–3 amendment, dibaca sebagai A untuk ketiganya, sesi 2026-11-05 |
| `NV-DEC-036` | Decision | **Menutup `NV-OQ-019`. `blueprint_shape: SINGLE`, `shape_decided_by: USER_CONFIRMED`.** Amendment menjadi blueprint revision 2 dengan gelombang MVP baru; 14 task yang sudah selesai tetap di tempatnya. Rumpun kemampuan: (A) papan pemantauan per lantai, sudah berjalan; (B) panggilan pasien: perangkat tablet bed, siklus panggilan, strip di TV, dan Konsol Panggilan. Hasil uji: rumpun B memenuhi 5/5 syarat pemecahan; rumpun A memenuhi 3/5 (Resource sendiri, gelombang MVP sendiri, master data dan pemilik approval sendiri), tetapi tidak punya kosakata status sendiri dan bounded context-nya dipakai bersama karena strip panggilan tampil di TV yang sama. Uji aturan condong ke `COMPOSITE`; owner memilih `SINGLE` karena kedua rumpun berbagi TV, grup hub, pola login perangkat, dan pemilik yang sama, serta `NV-DEC-002` dulu memilih `SINGLE` untuk scope yang memuat panggilan. Kemampuan tanpa rumpun: tidak ada | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban yang sama dengan `NV-DEC-035` |
| `NV-DEC-037` | Decision | **Menutup `NV-OQ-020`. Tablet melengkapi bel kabel, tidak menggantikannya** — sama dengan `NV-DEC-003` yang dulu gugur karena tablet keluar scope. Bel kabel tetap jalur utama yang bersuara; tablet, TV, dan konsol adalah tambahan. Tidak ada integrasi hardware dengan bel kabel. **Aman-gagal:** bila panggilan dari tablet tidak terkirim, tablet menampilkan "Panggilan belum terkirim — tekan bel di dinding". **Contoh:** Wi-Fi lantai 3 mati pukul 02.00; pasien 312-B menekan tombol di tablet; tablet menampilkan pesan itu; pasien menekan bel kabel yang berbunyi di nurse station seperti biasa | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban yang sama dengan `NV-DEC-035`; pembanding UL 1069, DIN VDE 0834 |
| `NV-DEC-038` | Decision | **Menutup `NV-OQ-021`. Tablet punya dua tombol: "Panggil perawat" dan "Darurat".** Urutan di strip TV dan Konsol Panggilan: semua panggilan Darurat lebih dulu, lalu panggilan biasa. Di dalam kelompok yang sama, yang paling lama menunggu berada di atas, dihitung dari jam server. Tidak ada jenis panggilan lain di MVP amendment. Arti "Darurat" bagi sistem belum dipertegas (`NV-OQ-030`). **Contoh:** 305-A memanggil biasa pukul 10.00.00, 318-B memanggil biasa pukul 10.00.20, dan 331-C menekan Darurat pukul 10.00.40. Pukul 10.01.00 urutannya: (1) 331-C, Darurat, menunggu 0:20; (2) 305-A, menunggu 1:00; (3) 318-B, menunggu 0:40. **Konsekuensi:** pasien bisa menekan Darurat untuk hal ringan (`NV-OQ-031`) | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan prioritas panggilan, opsi "Dua tombol", sesi 2026-11-05 |
| `NV-DEC-039` | Decision | **Menutup `NV-OQ-030`. "Darurat" hanya menaikkan prioritas.** Panggilan Darurat naik ke kelompok teratas dengan penanda dan bunyi yang berbeda di Konsol Panggilan. Darurat **tidak** memanggil tim lain dan **tidak** menggantikan prosedur kegawatan rumah sakit yang sudah ada (misalnya Code Blue); prosedur itu tetap berjalan lewat jalurnya sendiri. **Satu bed hanya punya satu panggilan aktif.** Bila bed sudah punya panggilan biasa lalu pasien menekan Darurat, panggilan yang sama berubah menjadi Darurat, dan jam awalnya tetap sehingga lama menunggu sebenarnya tetap terlihat. Menekan tombol yang sama berulang kali juga tidak membuat panggilan baru. **Contoh:** 305-A menekan "Panggil perawat" pukul 10.00.00, lalu "Darurat" pukul 10.00.50. Pukul 10.01.00 strip menampilkan satu item "305-A · DARURAT · menunggu 1:00 (sejak 10.00.00)", bukan dua item | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan arti Darurat, opsi "Naik prioritas", sesi 2026-11-05 |
| `NV-DEC-040` | Decision | **Menutup `NV-OQ-024`. Klaim tunggal.** Satu panggilan hanya bisa diklaim satu perawat lewat "Saya tangani" di Konsol Panggilan. Perawat lain boleh **"Ambil alih"**; jejaknya mencatat siapa dan kapan. Perawat yang mengklaim boleh **"Lepas"**, dan panggilan kembali `Menunggu` dengan jam awal tetap. **Tidak ada kedaluwarsa klaim otomatis.** Lama menunggu selalu dihitung dari saat pasien pertama menekan tombol, bukan dari saat diklaim, sehingga panggilan yang diklaim tetapi tak kunjung didatangi tetap terlihat makin lama. **Contoh:** Sari mengklaim 305-A pukul 10.01.00, lalu tertahan menolong pasien lain. Pukul 10.04.00 strip menampilkan "305-A · Sari menuju · menunggu 4:00". Dewi mengklik "Ambil alih" pukul 10.04.10; strip berubah menjadi "305-A · Dewi menuju · menunggu 4:10", dan jejak mencatat "Sari klaim 10.01.00; Dewi ambil alih 10.04.10" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan aturan klaim, opsi "Klaim tunggal", sesi 2026-11-05 |
| `NV-DEC-041` | Decision | **Menutup `NV-OQ-023`. "Perawat hadir" berada di area khusus perawat pada tablet dan harus ditekan-tahan 3 detik.** Tidak ada PIN, kartu, atau kredensial baru. Nama perawat diambil dari klaim di Konsol Panggilan: catatannya berbunyi "diklaim Sari, kehadiran dikonfirmasi di tablet". Bila panggilan belum pernah diklaim, tercatat "perawat, tanpa nama". **Batas yang diterima owner:** tablet tidak membuktikan siapa yang hadir. Orang yang sengaja menahan tombol bisa menutup panggilan; dampaknya setara dengan pasien membatalkan panggilan, dan bel kabel tetap tersedia (`NV-DEC-037`). Laporan waktu tanggap wajib menyebut batas ini. **Contoh:** Sari mengklaim 305-A pukul 10.01.00 dan tiba pukul 10.02.30. Ia menahan tombol 3 detik. Catatan: "Ditangani 10.02.30 · waktu tanggap 2:30 · diklaim Sari · kehadiran dikonfirmasi di tablet tanpa verifikasi identitas". Bila sentuhan dilepas sebelum 3 detik, panggilan tetap aktif | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan perlindungan "Perawat hadir", opsi "Tekan-tahan 3 detik", sesi 2026-11-05 |
| `NV-DEC-042` | Decision | **Menutup `NV-OQ-031`. Darurat yang ditekan untuk hal ringan tidak dinilai di MVP amendment.** Perawat tidak diminta menilai kesesuaian, dan tablet tidak meminta konfirmasi sebelum Darurat terkirim. Penyalahgunaan ditangani di luar sistem lewat edukasi pasien saat orientasi kamar, sebagai tanggung jawab operasional ruangan. **Alasan:** bila banyak pasien menyalahgunakan Darurat, urutan paling buruk kembali menjadi "siapa duluan" di dalam kelompok Darurat, sehingga tidak lebih buruk daripada satu tombol. Langkah tambahan saat perawat sedang menangani pasien justru menambah beban. **Contoh:** pasien 305-A menekan Darurat untuk minta air pukul 10.00.00; Sari datang dan menahan "Perawat hadir"; catatan hanya berbunyi "Darurat · ditangani 10.02.30", tanpa penilaian. **Konsekuensi:** belum ada data seberapa sering Darurat disalahgunakan; penilaian kesesuaian bisa ditambahkan di slice berikutnya bila diperlukan | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan Darurat untuk hal ringan, opsi "Tidak dicatat", sesi 2026-11-05 |
| `NV-DEC-043` | Decision | **Menutup `NV-OQ-025`. Konsol Panggilan: perawat memilih lantai, klaim berdasarkan izin.** Saat membuka konsol, perawat memilih satu lantai, dan pilihan itu diingat di PC tersebut. Siapa pun yang memegang izin baru Panggilan Pasien (diberikan admin lewat layar Akses Role; nama izinnya ditetapkan `design-business-module`) boleh melihat dan mengklaim panggilan lantai mana pun. Klaim **tidak** bergantung pada roster HR, karena roster perawat belum rutin terbit (`NV-UNK-003`). Nama lantai wajib tampil jelas di konsol supaya konsol tidak dipakai untuk lantai yang salah. Tidak ada master perangkat baru untuk PC. **Contoh:** Sari bertugas di lantai 3 dan membuka konsol "Lantai 3". Pukul 13.00 ia diminta membantu lantai 4, lalu mengganti pilihan ke "Lantai 4" dan mengklaim panggilan 402-B. Klaimnya diterima walaupun roster shift itu belum terbit | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan cakupan konsol, opsi "Pilih lantai + izin", sesi 2026-11-05 |
| `NV-DEC-044` | Decision | **Menutup `NV-OQ-026`. Isi item panggilan.** **Strip TV:** nomor bed, Darurat atau Panggilan, lama menunggu, dan nama perawat yang mengklaim. Tidak ada identitas pasien sama sekali. **Konsol Panggilan:** isi strip ditambah inisial, jenis kelamin, dan umur, sama dengan kartu bed (`NV-DEC-008`), supaya perawat yakin pasiennya benar. Nama lengkap dan No. RM **tidak** tampil di keduanya; tetap hanya di rekam medis. Penyamaran wajib dilakukan di backend, bukan hanya di frontend (Business rule 3). **Kapasitas strip** `DEV_DISCRETION` (`NV-FE-006`), dengan syarat panggilan yang tidak muat selalu ditandai "+k panggilan lain". **Contoh:** strip TV "331-C · DARURAT · 0:20"; konsol "331-C · R.A. · P · 72 th · DARURAT · 0:20". Pasien "Rina Astuti" tidak pernah tampil dengan nama lengkap | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan privasi item panggilan, opsi "TV bed saja, konsol inisial", sesi 2026-11-05 |
| `NV-DEC-045` | Decision | **Menutup `NV-OQ-032`. Bunyi Konsol Panggilan: per panggilan baru, ditambah pengingat sampai diklaim.** (1) Setiap panggilan baru berbunyi pendek satu kali; panggilan Darurat, termasuk panggilan biasa yang naik menjadi Darurat, memakai nada yang berbeda. (2) Selama masih ada panggilan `Menunggu` yang belum diklaim, konsol mengulang bunyi pengingat secara berkala. (3) Bunyi berhenti begitu semua panggilan sudah diklaim; klaim berfungsi sebagai tanda "panggilan diterima". (4) "Matikan bunyi" hanya **menunda pengingat**, dan konsol menampilkan sampai kapan pengingat ditunda. Panggilan baru tetap berbunyi walaupun pengingat sedang ditunda. TV tetap tanpa suara (`NV-DEC-013`). Jeda pengingat dan lama tunda `DEV_DISCRETION` (`NV-FE-007`). **Contoh:** 10.00.00 panggilan 305-A masuk dan konsol berbunyi "ting"; 10.00.20 331-C menekan Darurat dan konsol berbunyi dengan nada Darurat; 10.00.50 belum ada klaim sehingga pengingat berbunyi; 10.01.10 Sari mengklaim 331-C; 10.01.20 pengingat berbunyi lagi untuk 305-A; 10.01.30 Dewi mengklaim 305-A dan konsol senyap. **Alasan:** bunyi terus-menerus memicu kelelahan alarm (*alarm fatigue*, ECRI), sedangkan bunyi sekali saja mudah terlewat | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan bunyi konsol, opsi "Bunyi + pengingat", sesi 2026-11-05 |
| `NV-DEC-046` | Decision | **Menutup `NV-OQ-029`. Bed kosong mengunci tablet; pindah atau pulang menutup panggilan.** (1) Bila bed tidak punya penempatan aktif, tablet menampilkan "Bed belum berpenghuni" dan tombol panggil tidak aktif. (2) Bila pasien pindah atau pulang saat panggilannya masih `Menunggu` atau `Diklaim`, sistem menutup panggilan itu menjadi `DitutupSistem` dengan alasan "pasien pindah" atau "pasien pulang". (3) Panggilan **tidak** ikut pindah ke bed baru. (4) Tablet berganti ke pasien baru begitu penempatan baru tercatat, dan pasien baru tidak melihat sisa panggilan pasien lama. Nurse View hanya **membaca** penempatan Rawat Inap (Business rule 2); cara mendeteksi perpindahan ditetapkan `design-business-module`, dan perubahan kode di modul Rawat Inap tetap butuh persetujuan pemiliknya. **Contoh:** 331-C memanggil pukul 10.00; pukul 10.05 pasien dipindah ke 402-B. Panggilan 331-C menjadi `DitutupSistem` dengan alasan "pasien pindah", tablet 331-C menampilkan "Bed belum berpenghuni", dan strip lantai 4 tidak menerima panggilan apa pun dari kejadian itu | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan bed kosong/pindah/pulang, opsi "Kunci + tutup sistem", sesi 2026-11-05 |
| `NV-DEC-047` | Decision | **Menutup `NV-OQ-033`. Strip Panggilan Aktif selalu tampil di atas papan TV dan tidak ikut berganti halaman.** Kartu bed yang sedang memanggil juga diberi penanda. Saat tidak ada panggilan, strip tidak memakan tempat. Ini menutup titik gagal no. 1 (`NV-FCT-001`). **Contoh:** lantai 3 berisi 40 bed dalam 3 halaman; halaman 1 sedang tampil saat 331-C (halaman 3) menekan Darurat. Strip langsung menampilkan "331-C · DARURAT · 0:00" tanpa menunggu halaman 3, dan kartu 331-C ikut ditandai saat halaman 3 tampil | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan posisi strip TV, opsi "Strip tetap di atas", sesi 2026-11-05 |
| `NV-DEC-048` | Decision | **Bentuk aturan warna lama menunggu; angkanya tetap `NV-OQ-022`.** Item panggilan di strip TV dan Konsol Panggilan berubah warna menurut lama menunggu, dihitung dari saat pasien pertama menekan tombol. **Angka ambangnya kebijakan pelayanan keperawatan** dan wajib disahkan Komite Keperawatan, lalu diisi admin di pengaturan, dengan pola yang sama dengan interval TTV (`NV-DEC-010`). **Aman-gagal:** selama angka belum disahkan, item hanya menampilkan lama menunggu tanpa perubahan warna. Penanda Darurat tidak bergantung pada ambang ini. **Contoh:** sebelum pengesahan, 305-A tampil "menunggu 7:00" tanpa warna. Bila Komite kelak mengesahkan kuning 3 menit dan merah 5 menit (angka contoh, bukan keputusan), item yang sama tampil merah | Rizki Gunawan (bentuk aturan); Komite Keperawatan (angka) | `approved` untuk bentuk; angka `NV-OQ-022` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan ambang warna, opsi "Komite + aman-gagal", sesi 2026-11-05 |
| `NV-DEC-049` | Decision | **Menutup `NV-OQ-027`. Data waktu tanggap dicatat lengkap di MVP amendment; layar laporan menyusul di slice berikutnya.** Yang dicatat untuk setiap panggilan: jam tekan pertama, jam naik menjadi Darurat, setiap klaim, ambil alih, dan lepas beserta pelakunya, jam "Perawat hadir", serta jam dan alasan penutupan. Laporan apa pun yang kelak dibuat wajib menyebut bahwa kehadiran tidak terverifikasi identitasnya (`NV-DEC-041`), sehingga angka per perawat tidak dipakai sebagai bukti kinerja tanpa keputusan baru. **Contoh:** panggilan 305-A tercatat "tekan 10.00.00 · Darurat 10.00.50 · klaim Sari 10.01.00 · ambil alih Dewi 10.04.10 · hadir 10.05.00 · waktu tanggap 5:00"; belum ada layar yang merangkumnya | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban pertanyaan laporan waktu tanggap, opsi "Data dulu, laporan nanti", sesi 2026-11-05 |
| `NV-DEC-050` | Decision | **Closure pass — menutup `NV-CQ-006`; memperjelas `NV-DEC-046`.** Panggilan hanya ditutup otomatis bila Rawat Inap mencatat pasien **meninggalkan bed**: `EndReason = Transfer` → alasan "pasien pindah"; `EpisodeClosed` atau `PatientDeparted` → alasan "pasien pulang". Akhir penempatan yang bersifat administratif, yaitu `AdmissionCancelled` (penempatan dibatalkan) dan `CorrectedEntry` (koreksi salah catat), **tidak** menutup panggilan. Panggilan tetap aktif di strip dan konsol sampai perawat menekan "Perawat hadir", karena seseorang benar-benar memanggil dari bed itu. Panggilan melekat ke **bed**; penempatan saat panggilan dibuat disimpan sebagai riwayat. Bila tablet terkunci karena bed kini tidak punya penempatan aktif, area "Perawat hadir" tetap bisa dipakai selama masih ada panggilan aktif. **Contoh:** 331-C memanggil pukul 10.00. Pukul 10.03 penempatan 331-C dikoreksi karena pasien sebenarnya di 331-D. Panggilan 331-C tetap aktif, dan tablet 331-C terkunci untuk panggilan baru. Pukul 10.05 Sari tiba dan menahan "Perawat hadir"; panggilan menjadi `Ditangani` | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban closure pass `NV-CQ-006`, opsi "Tidak ditutup otomatis", sesi 2026-11-05; fakta `NV-CAP-19`. Butir `AdmissionCancelled` dan koreksi ke bed lain **diubah `NV-DEC-057`** (2026-11-09) |
| `NV-DEC-051` | Decision | **Closure pass — menutup `NV-CQ-007` dan `NV-CON-003`.** Prototipe `src/lib/helpers/nurseCallContext.jsx` (Oktober 2025) **dihapus** sebagai bagian task frontend amendment panggilan pasien. Tidak ada halaman yang memakainya, dan riwayatnya tetap ada di git. Sebelum dihapus, berkas ini tidak boleh dijadikan dasar implementasi. **Contoh:** task frontend yang membuat Konsol Panggilan sekaligus menghapus berkas itu, dan laporannya mencatat bahwa tidak ada `import` yang tersisa | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban closure pass `NV-CQ-007`, opsi "Hapus di task amendment"; fakta `NV-CAP-29` |
| `NV-DEC-052` | Decision | **Closure pass — menutup `NV-CQ-008`.** Panggilan yang harus ditutup karena pasien pindah atau pulang (`NV-DEC-050`) tertutup **paling lambat 60 detik** sesudah penempatan berakhir, sama dengan batas muat ulang papan (`NV-DEC-023`). Pemeriksaannya milik Nurse View sendiri, **tanpa** titik terbit baru di kode Rawat Inap. Selama jeda itu panggilan masih tampil; bel kabel tetap menjadi cadangan (`NV-DEC-037`). **Contoh:** 331-C dipindah ke 402-B pukul 10.05.00. Panggilan 331-C masih tampil di strip lantai 3 sampai paling lambat 10.06.00, lalu menjadi `DitutupSistem` dengan alasan "pasien pindah" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban closure pass `NV-CQ-008`, opsi "Paling lambat 60 detik" |
| `NV-DEC-053` | Decision | **Menutup `NV-OQ-034`. Penutupan cadangan dari Konsol Panggilan dengan alasan wajib.** Pemegang izin Panggilan Pasien (`NV-DEC-043`) boleh menekan **"Tutup panggilan"** di konsol untuk panggilan `Menunggu` atau `Diklaim`. Alasan wajib dipilih dari daftar tetap: (1) "Tablet tidak dapat dipakai"; (2) "Sudah ditangani, lupa konfirmasi di tablet"; (3) "Lainnya", disertai keterangan wajib. Penutupan tercatat atas nama perawat yang menutup, beserta jam dan alasannya, dan **ditandai "tidak dikonfirmasi di bed"**, sehingga laporan waktu tanggap kelak bisa memisahkannya dari "Perawat hadir" di tablet. **Tidak ada kedaluwarsa otomatis** (`NV-DEC-040` tetap). Jalur normal tetap "Perawat hadir" di tablet (`NV-DEC-041`). **Contoh:** Sari menangani 305-A pukul 10.02 tetapi lupa menahan tombol. Pukul 13.00 ia melihat strip masih menampilkan "305-A · Sari menuju · 3:00:00", lalu menekan "Tutup panggilan" di konsol dan memilih "Sudah ditangani, lupa konfirmasi di tablet". Catatan: "ditutup dari konsol oleh Sari, 13.00.10, tidak dikonfirmasi di bed". Contoh kedua: tablet 312-B mati pukul 02.00 sesudah pasien memanggil; Dewi datang menangani, lalu menutup dari konsol dengan alasan "Tablet tidak dapat dipakai" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban `NV-OQ-034`, opsi "Tutup dari konsol + alasan", sesi 2026-11-05 |
| `NV-DEC-054` | Decision | **Menutup `NV-OQ-035`. Pasien boleh membatalkan panggilan biasa selama belum diklaim.** Tombol "Batal" di tablet hanya tersedia bila panggilan berjenis **biasa** dan masih **`Menunggu`**. Panggilan **Darurat** dan panggilan yang sudah **`Diklaim`** tidak bisa dibatalkan pasien; perawat tetap datang dan menutupnya. Pembatalan tercatat dengan jamnya, dan panggilan hilang dari strip dan konsol. **Contoh:** pasien 318-B memanggil biasa pukul 10.00.20 karena infus habis. Pukul 10.01.00 perawat lain kebetulan lewat dan menggantinya, lalu pasien menekan "Batal"; panggilan menjadi `Dibatalkan`. Bila Dewi sudah mengklaim 318-B pukul 10.00.50, tombol "Batal" tidak ada lagi di tablet. Pasien 331-C yang menekan Darurat tidak pernah melihat tombol "Batal". Usulan `G-13` (Darurat tidak bisa diturunkan) **tetap usulan** dan disahkan terpisah saat approval | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Jawaban `NV-OQ-035`, opsi "Biasa & belum diklaim", sesi 2026-11-05 |
| `NV-DEC-055` | Decision | **Blueprint Nurse View revision 2 dan kontrak `0.2.0` disetujui owner.** Mencakup bagian "Revision 2" pada 11 berkas desain dan flowchart `04`–`06`, serta seluruh usulan bertanda sebagaimana tertulis: `AD-01`–`AD-06`; `G-09`–`G-14`, `G-16`, `G-17` (termasuk `G-13`: Darurat tidak bisa diturunkan pasien); `NV-FE-008` (Konsol Panggilan di grup menu Rawat Inap, dikabarkan ke Muhammad Hamzah), `NV-FE-009`, `NV-FE-010`. `G-15` tetap `CONFIGURABLE_DEFAULT`: angka ambang menunggu Komite Keperawatan (`NV-OQ-022`). Perubahan sesudah ini wajib menjadi revision/versi baru. Restu lead `NV-OQ-028` **tetap** dibutuhkan sebelum implementasi `MVP-6`–`MVP-8` | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-05 | Pernyataan "saya menyetujui nyaa" atas desain revision 2, sesi 2026-11-05 |
| `NV-DEC-056` | Decision | **Menutup `NV-OQ-028`. Lead merestui perubahan sebelas berkas bersama revision 2**, persis sebagaimana tertulis di `02-backend-architecture.md` R2.5 dan `03-frontend-architecture.md` R2.7. Backend: `AuthController`, `AuthorizationPolicies`, `Program.cs`, `QueueHub`. Frontend: `proxy.js`, `use-base-login.jsx`, `root-layout-shell.jsx`, `auth-wrapper.jsx`, `session-activity-manager.jsx`, `queue-realtime-client.js`, `menu-items.jsx`. **Kedua syarat `NV-DEC-030` berlaku penuh:** (1) login secara keseluruhan tidak terganggu — pengguna biasa, akun Kiosk, akun display antrean, TV Nurse View, dan akun lain tetap berperilaku persis seperti sebelumnya; (2) pola disamakan dengan display antrean dan TV Nurse View — resolver login, klaim `is_*_account`, policy alternatif, cookie, dan redirect `proxy.js` — dengan batas tafsir yang sama: yang ditiru alur login/runtime, **bukan** utang teknis `QueueDisplayDeviceController`. Tidak ada syarat lain yang dilaporkan. Perubahan berkas bersama di luar R2.5/R2.7 tetap butuh restu baru. Syarat (1) dibuktikan `NV-AC-041` dan `UAT-26`. Butir menu Konsol Panggilan di grup Rawat Inap tetap dikabarkan ke Muhammad Hamzah (`NV-FE-008`, `TLM-NV-002`); restu ini tidak menggantikan kabar itu. **Contoh:** sesudah tablet bed ditambahkan, perawat Sari tetap masuk dashboard, akun Kiosk tetap ke `/kiosk`, TV antrean tetap ke `/queue-display`, TV `NVW-*` tetap ke `/nurse-view-display`, dan hanya akun `NVT-*` yang dialihkan ke `/nurse-view-bed-tablet` | Lead | `approved` dengan syarat `NV-DEC-030` | Lead, secara lisan, dilaporkan Rizki Gunawan 2026-11-05 (tanggal restu tidak disebut) | Jawaban Rizki Gunawan atas pertanyaan gerbang `NV-OQ-028` di awal `plan-module-delivery` revision 2, opsi "Sudah, sesuai R2.5/R2.7", sesi 2026-11-05 |
| `NV-DEC-057` | Decision | **Mengubah sebagian `NV-DEC-050` atas jawaban pemilik Rawat Inap (`TLM-NV-002` bagian 8).** Pemeriksaan berkala selalu mengikuti baris pengganti koreksi (`SupersededByCorrectionId`) sampai penempatan terkini, lalu: (1) `AdmissionCancelled` berarti admisi dibatalkan dan pasien tidak lagi menempati bed, sehingga panggilan **ditutup sistem** dengan alasan baru "admisi dibatalkan" (`NvwPatientCallCloseReason.AdmissionCancelled = 8`); (2) koreksi salah pilih bed (baris pengganti di bed lain) berarti pasien secara fisik dipindah, sehingga panggilan **ditutup sistem** dengan alasan "pasien pindah"; (3) koreksi administratif di bed yang sama (kelas atau jam) membiarkan panggilan **tetap aktif**, dan bila penempatan pengganti kemudian berakhir karena pindah, pulang, atau batal admisi, panggilan ditutup seperti biasa. `Transfer`, `EpisodeClosed`, dan `PatientDeparted` tidak berubah; "Perawat hadir" tetap tidak memeriksa penempatan; batas 60 detik `NV-DEC-052` berlaku untuk ketiganya. **Contoh:** 331-C memanggil pukul 10.00. (a) Pukul 10.03 supervisor membatalkan admisi → paling lambat 10.04 panggilan `DitutupSistem` "admisi dibatalkan". (b) Pukul 10.03 penempatan dikoreksi ke 331-D → paling lambat 10.04 panggilan 331-C `DitutupSistem` "pasien pindah". (c) Pukul 10.02 kelasnya dikoreksi, panggilan tetap aktif; pukul 10.05 pasien dipindah ke 402-B → paling lambat 10.06 panggilan 331-C `DitutupSistem` "pasien pindah" | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-11-09 | Jawaban Muhammad Hamzah 2026-11-09 (`TLM-NV-002` bagian 8 no. 1 dan 4; dicocokkan dengan `InpEpisodeService.CancelAdmissionAsync`/`ReleaseBedHoldsAsync` dan `ApplyPlacementCorrectionCoreAsync`); pilihan Rizki Gunawan "Tutup, alasan baru", "Tutup sebagai pasien pindah", "Tambal sekarang", sesi `BE-NV-013` 2026-11-09 |

## Acceptance Criteria

| ID | Kriteria (Given / When / Then) | Keputusan |
|---|---|---|
| `NV-AC-001` | **Given** TV dipasangkan ke lantai 3 yang berisi Bangsal Anak (8 bed) dan Bangsal Bedah (12 bed) **When** papan dibuka **Then** tampil 20 kartu dalam dua kelompok bangsal, dan kamar tanpa `FloorCode` tidak tampil | `NV-DEC-006`, `017` |
| `NV-AC-002` | **Given** pasien "Budi Santoso", L, 64 th, di bed 301-A **When** papan lantai 3 tampil **Then** kartu menampilkan "B.S. · L · 64 th" **And** jawaban API untuk perangkat tidak memuat nama lengkap maupun No. RM | `NV-DEC-008` |
| `NV-AC-003` | **Given** TTV baru bed 305-A menghasilkan EWS `Critical` **When** TTV disimpan **Then** kartu 305-A menjadi merah berkedip di urutan teratas Bangsal Bedah tanpa bunyi | `NV-DEC-013`, `014` |
| `NV-AC-004` | **Given** interval belum disahkan **When** TTV terakhir berumur 9 jam **Then** tidak ada tanda "lewat jadwal", hanya "diukur 9 jam lalu" | `NV-DEC-010` |
| `NV-AC-005` | **Given** interval `Medium` = 4 jam dan TTV terakhir 08.00 dengan EWS `Medium` **When** jam menunjukkan 12.30 **Then** kartu ditandai "lewat jadwal 30 menit" | `NV-DEC-010` |
| `NV-AC-006` | **Given** display tersambung **When** koneksi putus 2 menit lebih **Then** tampil pita abu-abu berisi jam data terakhir **And** saat tersambung kembali papan dimuat ulang penuh dan pita hilang | `NV-DEC-014` |
| `NV-AC-007` | **Given** TTV 10.05 dibatalkan **When** papan diperbarui **Then** kartu menampilkan TTV sah sebelumnya | `NV-DEC-011` |
| `NV-AC-008` | **Given** episode tanpa perawat PJ **When** papan tampil **Then** kartu menampilkan tanda "Belum ada PJ" | `NV-DEC-012` |
| `NV-AC-009` | **Given** shift pagi Bangsal Anak dijaga Sari, Dewi, Rina menurut roster **When** papan tampil pada shift pagi **Then** header Bangsal Anak menampilkan ketiganya | `NV-DEC-012` |
| `NV-AC-010` | **Given** bed 303-B sedang dibersihkan dan 303-C dipesan **When** papan tampil **Then** kartunya abu-abu bertuliskan "Dibersihkan" dan "Dipesan — pasien akan datang" | `NV-DEC-015` |
| `NV-AC-011` | **Given** pasien dengan skor Morse 30 dan pasien dengan skor 50 **When** papan tampil **Then** hanya pasien skor 50 yang diberi ikon risiko jatuh | `NV-DEC-018` |
| `NV-AC-012` | **Given** pasien alergi Amoxicillin **When** papan tampil **Then** kartu menampilkan ikon "Alergi" tanpa nama zat, **And** nama zat tidak ada di jawaban API perangkat | `NV-DEC-019` |
| `NV-AC-013` | **Given** pasien butuh isolasi di bed biasa **When** papan tampil **Then** kartu menampilkan ikon isolasi dan tanda "Bed tidak sesuai isolasi" | `NV-DEC-020` |
| `NV-AC-014` | **Given** pasien dipindah dari 301-A (lantai 3) ke 402-B (lantai 4) **When** pemindahan disimpan di Rawat Inap **Then** kartu pasien hilang dari display lantai 3 dan muncul di display lantai 4 | Business rule 2 |
| `NV-AC-015` | **Given** perangkat lantai 3 aktif **When** perangkat meminta data lantai 4 **Then** ditolak (kode 403) | Business rule 4 |
| `NV-AC-016` | **Given** perangkat aktif sedang tampil **When** admin menonaktifkannya **Then** permintaan berikutnya ditolak dan TV kembali ke layar login | `NV-DEC-016`, `022` |
| `NV-AC-017` | **Given** roster Bangsal Anak shift berjalan kosong, atau `OrganizationUnitId` bangsal kosong **When** papan tampil **Then** header Bangsal Anak menampilkan "Roster shift ini belum tersedia" dan tidak ada nama perawat yang ditebak | `NV-DEC-021` |
| `NV-AC-018` | **Given** admin menekan Buat Login untuk NV-L3-01 **When** login dibuat **Then** kata sandi tampil satu kali saja **And** membuka ulang detail perangkat tidak menampilkan kata sandi itu lagi | `NV-DEC-022` |
| `NV-AC-019` | **Given** TV lantai 3 tersambung **When** TTV bed 305-A disimpan dengan EWS `Critical` **Then** kartu berubah tanpa menunggu muat ulang 60 detik | `NV-DEC-023` |
| `NV-AC-020` | **Given** pasien 303-B dipulangkan pukul 14.25 **When** tidak ada event push **Then** kartu 303-B sudah berubah paling lambat pada muat ulang berikutnya (≤ 60 detik) | `NV-DEC-023` |
| `NV-AC-022` | **Given** akun perangkat Nurse View tersambung ke hub **When** ia mencoba bergabung ke grup `inpatient-bed-selection:{unit}` **Then** ditolak, **And** sinyal TTV yang diterima TV tidak memuat nama, No. RM, maupun nilai TTV | `NV-DEC-026`, Business rule 3 |
| `NV-AC-021` | **Given** kamar 310 aktif dengan `FloorCode = 3` tanpa `FloorPlanRoomKey` **When** papan lantai 3 tampil **Then** bed kamar 310 ikut tampil | `NV-DEC-025` |
| `NV-AC-023` | **Given** akun pegawai biasa (perawat Sari) **When** login sesudah perubahan Nurse View **Then** masuk dashboard seperti sebelumnya, tanpa klaim `nurse_view_device_id` dan tanpa cookie `isNurseViewAccount` | `NV-DEC-030` (1) |
| `NV-AC-024` | **Given** akun perangkat Kiosk **When** login **Then** tetap diarahkan ke `/kiosk` dan policy `KioskRead` tetap lolos | `NV-DEC-030` (1) |
| `NV-AC-025` | **Given** akun display antrean **When** login **Then** tetap diarahkan ke `/queue-display`, klaim `is_queue_display_account` tetap terbit, dan papan antrean tetap tampil | `NV-DEC-030` (1) |
| `NV-AC-026` | **Given** akun SuperAdmin/Administrator **When** login **Then** perilaku tidak berubah, **And** membuka papan Nurse View tetap ditolak (`403`) | `NV-DEC-030` (1), `VAL-NV-14` |
| `NV-AC-027` | **Given** login gagal (kata sandi salah, akun terkunci) untuk akun mana pun **When** dicoba **Then** pesan dan kode tetap sama dengan sebelum perubahan. Pengecualian tunggal: perangkat Nurse View nonaktif dengan kata sandi benar (`NV-DEC-031`) | `NV-DEC-030` (1), `NV-DEC-031` |

## Open Questions dan Blocker

| ID | Pertanyaan | Penjawab | Memblokir |
|---|---|---|---|
| `NV-OQ-010` | Angka interval ukur ulang TTV per tingkat EWS (`Low`/`Medium`/`High`/`Critical`) | Komite Keperawatan | Hanya **runtime** tanda "lewat jadwal". Desain dan rilis tidak terhalang karena aman-gagal |
| `NV-OQ-011` | Apakah roster HR mencatat perawat sampai tingkat bangsal (unit), sehingga header per bangsal bisa diisi? | `trace-existing-capabilities` (fakta source) | **Terjawab: Conflict** — struktur roster bisa dipetakan ke bangsal, tetapi `RWI-DEC-100` menyatakan shift perawat belum dijalankan. Lihat `01-existing-capability-map.md` `NV-CON-001`; diputuskan lewat `NV-CQ-001` |
| `NV-CQ-001` s.d. `005` | Pertanyaan penutup hasil audit kemampuan: header perawat, login perangkat, push lintas modul, registry modul, kamar tanpa `FloorPlanRoomKey` | Rizki Gunawan | Ditutup — `NV-DEC-021` s.d. `025` |
| `NV-OQ-012` | Apakah `FloorCode` semua kamar rawat inap aktif sudah terisi? | Admin master data (cek operasional) | Runtime saja |
| `NV-OQ-013` | Jenis isolasi (kontak/droplet/udara) belum ada datanya | Muhammad Hamzah (Rawat Inap) — di luar sesi ini | `LATER SLICE` |
| `NV-OQ-014` | Konfirmasi perilaku header saat roster shift belum terbit | Rizki Gunawan | Ditutup — `NV-DEC-021` |
| `NV-OQ-015` | Restu perubahan berkas bersama untuk login perangkat: klaim di `AuthController`, satu policy di `Program.cs` + `ApprovedAlternativeAuthorization`, metode `JoinNurseViewFloor` di `Hubs/QueueHub.cs`, registrasi service di `Program.cs`, serta di frontend `proxy.js` dan `lib/hooks/auth/use-base-login.jsx` (daftar diperinci `design-business-module` 2026-10-28, `02-backend-architecture.md` §4.10) | Lead (pemilik governance/QBE) | **Ditutup 2026-10-28 — `NV-DEC-030`**, dengan syarat lead: (1) login pengguna biasa, akun Kiosk, akun display antrean, dan akun lain **tidak boleh terganggu**; (2) pola **disamakan dengan display antrean** |
| `NV-OQ-016` | Satu titik terbit realtime saat TTV disimpan/dibatalkan, di `PatientVitalSignController` (Clinical Management) | Pemilik kode TTV — Muhammad Hamzah lewat `rawat-inap/keperawatan` (dikonfirmasi saat permintaan diajukan) | **Ditutup 2026-10-28 — disetujui Hamzah (`NV-DEC-027`).** Diajukan 2026-10-28 lewat [`rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001`](../rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001-sinyal-realtime-saat-ttv-disimpan.md); pemberitahuan ketergantungan baca ke [`episode-rawat-inap`](../rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-001-pembacaan-data-bed-dan-episode-oleh-nurse-view.md) |
| `NV-OQ-017` | Pendaftaran `HealthServices / NurseViewManagement / Nvw` ke registry | Rizki Gunawan (pemilik modul) | **Ditutup 2026-10-28 — `NV-DEC-029`.** Tidak butuh restu lead; baris ditulis pemilik modul sebagai langkah pertama MVP-0 |

### Unknown runtime (dari capability map)

| ID | Hal | Dampak bila belum beres saat go-live |
|---|---|---|
| `NV-UNK-001` | `FloorCode` setiap kamar | Kamar tidak tampil di papan mana pun |
| `NV-UNK-002` | Versi instrumen risiko jatuh sudah aktif | Ikon risiko jatuh tidak pernah tampil |
| `NV-UNK-003` | Roster berisi perawat | Header selalu "Roster shift ini belum tersedia" (sudah diterima, `NV-DEC-021`) |
| `NV-UNK-004` | `OrganizationUnitId` bangsal | Sama dengan `NV-UNK-003` |

### Pertanyaan dari analisa awal

| ID | Pertanyaan | Penjawab | Status |
|---|---|---|---|
| `NV-OQ-001` | Tablet melengkapi atau menggantikan bel kabel | Rizki Gunawan | Gugur — tablet keluar scope (`NV-DEC-004`) |
| `NV-OQ-002` | Rute panggilan pasien | Rizki Gunawan | Gugur — panggilan keluar scope |
| `NV-OQ-003` | Display per unit atau per nurse station | Rizki Gunawan | Ditutup — per lantai, semua bed di lantai itu (`NV-DEC-006`); TV pasif (`NV-DEC-009`) |
| `NV-OQ-004` | Kepemilikan tablet | Rizki Gunawan | Gugur — tablet keluar scope |
| `NV-OQ-005` | Pemesanan makan | Kepala Instalasi Gizi | Gugur untuk scope ini — kandidat slice berikutnya |
| `NV-OQ-006` | Jadwal visite untuk pasien | Rizki Gunawan | Gugur — kandidat slice berikutnya |
| `NV-OQ-007` | Data klinis yang boleh dilihat pasien | Rizki Gunawan | Gugur — pasien tidak melihat apa pun |
| `NV-OQ-008` | Pemantauan TTV: EWS di display atau integrasi monitor pasien | Rizki Gunawan | Ditutup — TTV input perawat saja (`NV-DEC-011`) |
| `NV-OQ-009` | Nama pasien di display | Rizki Gunawan | Ditutup — inisial + jenis kelamin + umur (`NV-DEC-008`) |

## Amendment revision 5 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| Mode | Amendment pass `grill-me` |
| Dibuka | 2026-11-05 oleh Rizki Gunawan |
| Status | `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`). Seluruh keputusan owner tertutup (`NV-DEC-034`–`055`); impact scan, gerbang kelengkapan, closure pass, dan desain revision 2 selesai. Restu lead `NV-OQ-028` diberikan dengan syarat `NV-DEC-030` (`NV-DEC-056`, dilaporkan 2026-11-05); implementasi `MVP-6`–`MVP-8` tidak lagi tertahan gerbang ini |
| Backend SHA | `0c6eab34` (branch `rizkiG`) |
| Frontend SHA | `91a11b08f` (branch `RizkiV2`) |
| Dampak ke 14 task MVP | Tidak ada. Blueprint revision `1` dan kontrak `0.1.2` tetap acuan sampai amendment disetujui |

> **Catatan audit.** Capability map dibuat di SHA `a6f977a3` / `76b0c6e4b` dan belum memuat
> panggilan pasien maupun perangkat per bed. Untuk amendment ini map dianggap basi;
> `trace-existing-capabilities` mode impact scan dijalankan sesudah scope dikunci.
> **Impact scan selesai 2026-11-05** pada `0c6eab34` / `91a11b08f`: `NV-CAP-17`–`29`, conflict
> `NV-CON-003`, unknown `NV-UNK-006`–`007`, dan pertanyaan penutup `NV-CQ-006`–`008`.

### Kebutuhan dari owner

Dikutip dari Rizki Gunawan, 2026-11-05:

1. Tablet pasien dipasang **per bed**. Bed berada di kamar, dan kamar berada di lantai.
2. Setiap tablet **login sesuai bed-nya** dan dipakai pasien untuk **memanggil perawat**.
3. Display Nurse View menampilkan **alert panggilan**.
4. Perlu langkah yang jelas ketika **banyak pasien memanggil bersamaan**.
5. Pertanyaan lanjutan: bagaimana bila TV diganti tablet atau PC. Dijawab `NV-DEC-034`.

### Glossary amendment

| Istilah | Arti di modul ini |
|---|---|
| Tablet bed | Tablet milik rumah sakit yang terpasang di satu bed dan masuk dengan akun perangkat bed itu |
| Panggilan | Permintaan pasien agar perawat datang. Satu bed hanya punya satu panggilan aktif (`NV-DEC-039`) |
| Darurat | Panggilan yang dinaikkan ke kelompok teratas. Tidak memanggil tim lain dan tidak menggantikan prosedur kegawatan rumah sakit (`NV-DEC-039`) |
| Strip Panggilan Aktif | Baris di atas papan TV yang selalu menampilkan panggilan aktif lantai itu dan tidak ikut berganti halaman (`NV-DEC-047`) |
| Konsol Panggilan | Layar di PC nurse station yang dibuka perawat dengan akunnya sendiri untuk mengklaim panggilan dan mendengar bunyinya (`NV-DEC-034`) |
| Klaim | Tanda bahwa seorang perawat menangani panggilan ("Saya tangani"). Satu panggilan hanya punya satu pengklaim (`NV-DEC-040`) |
| Perawat hadir | Tombol di area khusus tablet yang ditekan-tahan 3 detik saat perawat tiba, untuk menutup panggilan (`NV-DEC-041`) |
| Lama menunggu | Waktu sejak pasien pertama menekan tombol, dihitung dari jam server |
| Waktu tanggap | Waktu dari tekan pertama sampai "Perawat hadir" |

### Batas scope amendment — dikunci `NV-DEC-035`

| Di dalam scope | Di luar scope — milik modul lain atau slice lain |
|---|---|
| Perangkat tablet per bed: didaftarkan admin IT, login per perangkat, terikat ke satu bed | Pemesanan makan — Gizi (`GIZ-DEC-013`) |
| Pasien di tablet ditentukan dari penempatan aktif Rawat Inap; bed kosong membuat tablet terkunci | Jadwal visite untuk pasien — `dokter-rawat-inap` |
| Tombol panggil dan status panggilan di tablet | Data klinis untuk pasien di tablet |
| Siklus panggilan pasien beserta waktu tanggapnya | Integrasi hardware bel kabel |
| Strip "Panggilan Aktif" di Display Nurse View lantai yang sama | Integrasi monitor pasien (`NV-DEC-011`) |
| Konsol Panggilan di PC nurse station (`NV-DEC-034`) | Notifikasi ke HP perawat — kandidat slice berikutnya |
| Aturan banyak panggilan: urutan, tekan berulang, kebanjiran, gagal kirim | Layar laporan waktu tanggap — slice berikutnya (`NV-DEC-049`) |

### Fakta source

| ID | Fakta | Bukti | Akibatnya |
|---|---|---|---|
| `NV-FCT-001` | TV menampilkan 15 kartu per halaman (5 kolom × 3 baris) dan berganti halaman tiap 15 detik | `nurse-view-display-constants.jsx`: `NURSE_VIEW_BOARD_COLUMNS`, `NURSE_VIEW_BOARD_ROWS`, `NURSE_VIEW_PAGE_DURATION_SECONDS` | Lantai 40 bed tampil dalam 3 halaman. Penanda panggilan yang hanya ada di kartu bed bisa tidak terlihat sampai 30 detik |
| `NV-FCT-002` | Setiap sinyal realtime memuat ulang seluruh papan, paling sering sekali per 5 detik | `use-nurse-view-floor-board.jsx`: `scheduleRealtimeLoad`, `NURSE_VIEW_REALTIME_MIN_GAP_SECONDS = 5` | Panggilan yang menumpang jalur ini bisa terlambat sampai 5 detik |
| `NV-FCT-003` | Belum ada entity panggilan pasien maupun perangkat per bed. `NvwDisplayDevice` terikat ke `FloorCode` | `Areas/HealthServices/NurseViewManagement/Models/` hanya berisi `NvwDisplayDevice` dan `NvwVitalSignInterval` | Panggilan dan tablet adalah data baru milik Nurse View |
| `NV-FCT-004` | Nama `NurseCall` sudah dipakai antrean rawat jalan untuk perawat yang memanggil pasien | `NurseStationQueueController.cs`: `NurseCallExpiresAt`, `NurseCallAttemptCount` | Entity baru memakai nama lain, misalnya `NvwPatientCall` |
| `NV-FCT-005` | Papan runtime hanya bisa dibuka akun perangkat Nurse View | Policy `NurseViewDisplayRead` di `NvwDisplayRuntimeController`; `NV-AC-026` (SuperAdmin ditolak `403`) | Konsol yang dibuka dengan login perawat butuh layar dan endpoint sendiri, bukan papan TV yang sama |

### Analisa banyak panggilan — penanganannya dikunci keputusan owner

**Contoh yang dipakai.** Lantai 3 berisi 40 bed (3 halaman). Pukul 10.00.00 bed 305-A
(halaman 1) memanggil, pukul 10.00.20 bed 318-B (halaman 2), dan pukul 10.00.40 bed 331-C
(halaman 3). Pukul 10.00.45 pasien 305-A menekan lagi.

| No | Titik gagal | Akibat bila tidak diatur | Usulan penanganan |
|---|---|---|---|
| 1 | Panggilan berada di halaman lain | 331-C baru terlihat saat halaman 3 tampil | Strip "Panggilan Aktif" tetap di atas papan dan tidak ikut berganti halaman (`NV-DEC-047`) |
| 2 | Urutan tidak jelas | Perawat tidak tahu siapa yang menunggu paling lama | Panggilan Darurat dulu, lalu yang paling lama menunggu (`NV-DEC-038`) |
| 3 | Tekan berulang | 305-A tampak baru memanggil, padahal ia menunggu paling lama | Satu bed hanya punya satu panggilan aktif. Tekan ulang tidak membuat panggilan baru dan tidak mengubah jam awal: "305-A · menunggu sejak 10.00.00 · ditekan 2×" (`NV-DEC-039`) |
| 4 | TV tidak bisa disentuh | Dua perawat ke bed yang sama, atau tidak ada yang datang | Klaim "Saya tangani" di Konsol Panggilan; TV menampilkan "305-A · Sari menuju" (`NV-DEC-034`) |
| 5 | Jalur realtime papan | Terlambat sampai 5 detik (`NV-FCT-002`) | Sinyal panggilan sendiri yang ringan: nomor bed, jenis, jam, tanpa identitas pasien. Syaratnya `NV-AC-028` dan `NV-DEC-044`; mekanismenya ditetapkan `design-business-module` |
| 6 | Kebanjiran | 12 panggilan tidak muat dalam satu baris | N panggilan teratas ditambah tulisan "+k panggilan lain" (`NV-DEC-044`, `NV-FE-006`) |
| 7 | Tablet atau Wi-Fi mati | Pasien mengira panggilannya terkirim | Tablet menampilkan "Panggilan belum terkirim — tekan bel di dinding" (`NV-DEC-037`), lalu mengirim ulang otomatis tanpa membuat panggilan ganda |
| 8 | Pasien pindah atau pulang saat panggilan aktif | Panggilan menggantung selamanya | Sistem menutupnya dengan alasan "pasien pindah" atau "pasien pulang" (`NV-DEC-046`) |

Jam yang dipakai selalu jam server, bukan jam tablet. Saat TV terputus, panggilan tetap
tersimpan dan muncul lagi begitu TV tersambung; pita data basi `NV-DEC-014` tetap berlaku.

### Siklus panggilan — dari keputusan owner

| Dari status | Tindakan | Ke status | Siapa | Contoh |
|---|---|---|---|---|
| — | Pasien menekan tombol panggil | `Menunggu` | Tablet bed | 305-A memanggil pukul 10.00.00 |
| `Menunggu` | "Saya tangani" | `Diklaim` | Perawat, di Konsol Panggilan | Sari mengklaim 305-A pukul 10.01.10 |
| `Diklaim` | "Ambil alih" | `Diklaim` (pengklaim berganti) | Perawat lain, di Konsol Panggilan | Dewi mengambil alih 305-A dari Sari pukul 10.04.10 (`NV-DEC-040`) |
| `Diklaim` | "Lepas" | `Menunggu`, jam awal tetap | Perawat yang mengklaim | Sari melepas 305-A; strip kembali menampilkan "305-A · Panggilan · menunggu 3:20" (`NV-DEC-040`) |
| `Menunggu` atau `Diklaim` | "Perawat hadir", ditekan-tahan 3 detik (`NV-DEC-041`) | `Ditangani` | Perawat, di tablet bed | Sari tiba pukul 10.02.30; waktu tanggap 2 menit 30 detik |
| `Menunggu`, hanya panggilan biasa | Pasien membatalkan (`NV-DEC-054`) | `Dibatalkan` | Pasien, di tablet bed | Pasien 318-B menekan Batal pukul 10.01.00. Tidak tersedia untuk Darurat maupun panggilan yang sudah diklaim |
| `Menunggu` atau `Diklaim` | "Tutup panggilan" dengan alasan wajib (`NV-DEC-053`) | Selesai, ditandai "tidak dikonfirmasi di bed" | Pemegang izin, di Konsol Panggilan | Sari menutup 305-A pukul 13.00.10 dengan alasan "Sudah ditangani, lupa konfirmasi di tablet" |
| `Menunggu` atau `Diklaim` | Pasien pindah atau pulang | `DitutupSistem` | Sistem | 331-C dipindah ke lantai 4 |

Lama menunggu dan warnanya hanya tampilan, bukan status. Aturan klaim dikunci `NV-DEC-040`:
klaim tunggal, boleh diambil alih dan dilepas, tanpa kedaluwarsa otomatis. Penutupan karena
pindah atau pulang dikunci `NV-DEC-046`. Nama status di kode ditetapkan `design-business-module`.

### Keputusan lama yang tersentuh

| Keputusan | Isi yang berlaku | Dampak bila amendment disetujui |
|---|---|---|
| `NV-DEC-004` | Tanpa interaksi pasien | Bagian tablet dan panggilan digantikan `NV-DEC-035` |
| `NV-DEC-009` | TV pasif | Tetap — dikukuhkan `NV-DEC-034` |
| `NV-DEC-013` | Display tanpa suara | Tetap untuk TV. Bunyi hanya di Konsol Panggilan (`NV-DEC-034`) |
| `NV-DEC-023`, `026` | Push hanya untuk TTV | Ditambah satu sinyal panggilan tanpa identitas pasien |
| Business rule 1 | Data milik Nurse View hanya perangkat display dan interval | Ditambah perangkat tablet bed dan data panggilan |
| `NV-DEC-030` | Restu lead untuk login perangkat display | Konsol tidak menyentuhnya karena memakai login biasa. Akun perangkat tablet bed adalah jenis perangkat kedua dan butuh restu lead (`NV-OQ-028`) |

### Acceptance criteria amendment — draft

Kriteria berikut sudah bisa diuji dari keputusan owner. Rinciannya, termasuk kode status dan
bentuk jawaban API, ditetapkan `design-business-module`.

| ID | Kriteria (Given / When / Then) | Keputusan |
|---|---|---|
| `NV-AC-028` | **Given** tablet bed 305-A dengan penempatan aktif dan TV lantai 3 tersambung **When** pasien menekan "Panggil perawat" **Then** strip TV dan Konsol Panggilan lantai 3 menampilkan 305-A tanpa menunggu muat ulang papan 60 detik | `NV-DEC-035`, `NV-FCT-002` |
| `NV-AC-029` | **Given** 305-A sudah memanggil biasa pukul 10.00.00 **When** pasien menekan "Panggil perawat" lagi, lalu "Darurat" pukul 10.00.50 **Then** hanya ada satu item 305-A, berstatus Darurat, dengan lama menunggu dihitung sejak 10.00.00 | `NV-DEC-039` |
| `NV-AC-030` | **Given** 305-A memanggil biasa pukul 10.00.00, 318-B biasa pukul 10.00.20, dan 331-C Darurat pukul 10.00.40 **When** strip tampil pukul 10.01.00 **Then** urutannya 331-C, 305-A, 318-B | `NV-DEC-038` |
| `NV-AC-031` | **Given** TV lantai 3 (3 halaman) sedang menampilkan halaman 1 **When** 331-C di halaman 3 memanggil **Then** strip langsung menampilkan 331-C, dan strip tidak ikut berganti halaman | `NV-DEC-047` |
| `NV-AC-032` | **Given** Sari sudah mengklaim 305-A **When** Dewi membuka panggilan itu di konsol **Then** pilihan yang tersedia "Ambil alih", bukan klaim kedua; sesudah Dewi mengambil alih, strip menampilkan "Dewi menuju", jejak mencatat Sari dan Dewi, dan lama menunggu tetap dihitung dari jam tekan pertama | `NV-DEC-040` |
| `NV-AC-033` | **Given** Sari mengklaim 305-A **When** Sari menekan "Lepas" **Then** panggilan kembali `Menunggu` dengan jam awal tetap | `NV-DEC-040` |
| `NV-AC-034` | **Given** panggilan 305-A diklaim Sari **When** area "Perawat hadir" disentuh kurang dari 3 detik **Then** panggilan tetap aktif; **When** ditahan 3 detik **Then** panggilan menjadi `Ditangani`, hilang dari strip, dan tercatat "diklaim Sari, kehadiran dikonfirmasi di tablet tanpa verifikasi identitas" | `NV-DEC-041` |
| `NV-AC-035` | **Given** dua panggilan `Menunggu` belum diklaim **When** jeda pengingat berlalu **Then** konsol berbunyi pengingat; **When** keduanya diklaim **Then** konsol senyap; **Given** pengingat sedang ditunda **When** panggilan baru masuk **Then** konsol tetap berbunyi | `NV-DEC-045` |
| `NV-AC-036` | **Given** pengguna tanpa izin Panggilan Pasien **When** membuka konsol atau mengklaim **Then** ditolak (kode 403); **Given** Sari berizin dan roster shift itu kosong **When** ia mengklaim panggilan lantai 4 **Then** klaim diterima | `NV-DEC-043` |
| `NV-AC-037` | **Given** pasien "Rina Astuti", P, 72 th, di 331-C memanggil **When** strip TV dan konsol tampil **Then** TV hanya menampilkan "331-C", konsol menampilkan "R.A. · P · 72 th", jawaban API untuk akun perangkat TV tidak memuat identitas pasien, dan jawaban API konsol tidak memuat nama lengkap maupun No. RM | `NV-DEC-044` |
| `NV-AC-038` | **Given** 331-C memanggil pukul 10.00.00 **When** pasien dipindah ke 402-B pukul 10.05.00 **Then** paling lambat 10.06.00 panggilan menjadi `DitutupSistem` dengan alasan "pasien pindah", tablet 331-C menampilkan "Bed belum berpenghuni", dan strip lantai 4 tidak menerima panggilan dari kejadian itu | `NV-DEC-046`, `050`, `052` |
| `NV-AC-042` | **Given** 331-C memanggil pukul 10.00 **When** penempatan 331-C dikoreksi salah catat **di bed yang sama** (kelas atau jam) **Then** panggilan tetap aktif di strip dan konsol dan "Perawat hadir" tetap bisa ditahan untuk menutupnya. *Diubah `NV-DEC-057` (2026-11-09):* pembatalan admisi dan koreksi ke bed lain kini menutup panggilan; bunyi lama "koreksi salah catat atau pembatalan penempatan → tetap aktif, tablet terkunci" tidak berlaku lagi | `NV-DEC-050`, `057` |
| `NV-AC-043` | **Given** cabang frontend task amendment selesai **When** source diperiksa **Then** berkas `src/lib/helpers/nurseCallContext.jsx` tidak ada, dan tidak ada `import` yang merujuknya | `NV-DEC-051` |
| `NV-AC-044` | **Given** panggilan 305-A `Diklaim` Sari sejak 10.01 **When** Sari menekan "Tutup panggilan" di konsol tanpa memilih alasan **Then** ditolak; **When** ia memilih "Sudah ditangani, lupa konfirmasi di tablet" **Then** panggilan selesai, hilang dari strip, dan tercatat "ditutup dari konsol oleh Sari, tidak dikonfirmasi di bed"; **And** alasan "Lainnya" tanpa keterangan ditolak | `NV-DEC-053` |
| `NV-AC-045` | **Given** 318-B memanggil biasa dan masih `Menunggu` **When** pasien menekan "Batal" **Then** panggilan menjadi `Dibatalkan` dan hilang dari strip; **Given** 318-B sudah diklaim, atau 331-C memanggil Darurat **When** tablet tampil **Then** tombol "Batal" tidak ada, dan permintaan batal yang tetap dikirim ditolak backend | `NV-DEC-054` |
| `NV-AC-039` | **Given** tablet 312-B tidak tersambung **When** pasien menekan "Panggil perawat" **Then** tablet menampilkan "Panggilan belum terkirim — tekan bel di dinding"; **When** koneksi pulih **Then** panggilan terkirim tepat satu kali | `NV-DEC-037`, `039` |
| `NV-AC-040` | **Given** angka ambang warna belum disahkan **When** panggilan menunggu 10 menit **Then** item tampil "menunggu 10:00" tanpa perubahan warna | `NV-DEC-048` |
| `NV-AC-041` | **Given** akun pegawai biasa, Kiosk, display antrean, dan TV Nurse View **When** login sesudah akun perangkat tablet bed ditambahkan **Then** perilakunya sama dengan `NV-AC-023`–`027` | `NV-DEC-030`, `NV-OQ-028` |

### Pertanyaan terbuka amendment

| ID | Pertanyaan | Penjawab | Memblokir |
|---|---|---|---|
| `NV-OQ-018` | Konfirmasi batas scope amendment | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-035`** |
| `NV-OQ-019` | Bentuk blueprint `SINGLE` atau `COMPOSITE` | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-036`, `SINGLE`** |
| `NV-OQ-020` | Tablet melengkapi atau menggantikan bel kabel | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-037`, melengkapi** |
| `NV-OQ-021` | Jenis panggilan dan urutan prioritasnya | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-038`, dua tombol** |
| `NV-OQ-022` | Angka ambang warna lama menunggu. Bentuk aturan dan aman-gagalnya sudah dikunci `NV-DEC-048` | Komite Keperawatan | Hanya **runtime** perubahan warna. Desain dan rilis tidak terhalang |
| `NV-OQ-023` | Perlindungan tombol "Perawat hadir" | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-041`, tekan-tahan 3 detik** |
| `NV-OQ-024` | Aturan klaim | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-040`, klaim tunggal** |
| `NV-OQ-025` | Konsol: lantai yang dilihat dan siapa yang boleh mengklaim | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-043`, pilih lantai + izin** |
| `NV-OQ-026` | Kapasitas strip dan isi setiap item | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-044`; kapasitas `NV-FE-006`** |
| `NV-OQ-027` | Laporan waktu tanggap | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-049`, data dulu, laporan slice berikutnya** |
| `NV-OQ-028` | Restu lead untuk akun perangkat tablet bed di berkas bersama login | Lead | **Ditutup 2026-11-05 — `NV-DEC-056`, restu lead dengan syarat `NV-DEC-030`** (sebelumnya `IMPLEMENTATION`) |
| `NV-OQ-029` | Bed kosong, pasien pindah, dan pasien pulang | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-046`** |
| `NV-OQ-030` | Arti "Darurat" bagi sistem | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-039`, naik prioritas** |
| `NV-OQ-031` | Penanganan Darurat yang ditekan untuk hal ringan | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-042`, tidak dicatat di MVP** |
| `NV-OQ-032` | Perilaku bunyi Konsol Panggilan saat banyak panggilan | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-045`, bunyi + pengingat** |
| `NV-OQ-033` | Posisi panggilan aktif di TV yang berganti halaman | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-047`, strip tetap di atas** |
| `NV-UNK-005` | Apakah setiap bed rawat inap punya bel kabel yang berfungsi. Ini prasyarat aman-gagal `NV-DEC-037` | Pengelola sarana rumah sakit (cek operasional) | Runtime dan go-live |
| `NV-OQ-034` | Penutupan cadangan bila "Perawat hadir" tidak bisa ditekan atau lupa ditekan | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-053`, tutup dari konsol dengan alasan wajib** |
| `NV-OQ-035` | Pembatalan oleh pasien | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-054`, hanya panggilan biasa yang belum diklaim** |
| `NV-CQ-006` | Alasan penutupan untuk penempatan dibatalkan dan koreksi salah catat | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-050`, tidak ditutup otomatis** |
| `NV-CQ-007` | Prototipe `nurseCallContext.jsx` | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-051`, dihapus di task amendment** |
| `NV-CQ-008` | Jeda terlama penutupan saat pindah atau pulang | Rizki Gunawan | **Ditutup 2026-11-05 — `NV-DEC-052`, paling lambat 60 detik** |
| `NV-UNK-006` | Apakah pindah bed dicatat tepat saat pasien benar-benar berpindah (`01-existing-capability-map.md`) | Kepala ruang (praktik operasional) | Runtime; bel kabel tetap cadangan |
| `NV-UNK-007` | Kemampuan tablet contoh: browser, layar selalu menyala, mode kiosk, izin bunyi | Admin IT (uji satu tablet contoh) | Pengadaan dan go-live |
