# Nurse View — Interview Decisions

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` |
| Revision | `2` — Closure pass (2026-10-28), ditambah `NV-DEC-027`, `028`. Sebelumnya `1` — Scope pass |
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

### Di luar scope — kandidat slice berikutnya

Dicatat agar tidak hilang, **bukan** janji pengerjaan.

| Kandidat | Asal | Catatan bila kelak dibuka |
|---|---|---|
| Tablet pasien per bed | Kebutuhan awal 2026-10-28 | Pola akun perangkat Kiosk / `MstQueueDisplayDevice` sudah ada |
| Panggilan pasien ke perawat | Kebutuhan awal | Hanya sebagai **pelengkap** bel kabel (pernah diputuskan di `NV-DEC-003`, kini `superseded`). Nama `NurseCall` sudah dipakai antrean rawat jalan dengan arti terbalik |
| Pemesanan makan oleh pasien | Kebutuhan awal | Terhalang `GIZ-DEC-013`; butuh keputusan Kepala Instalasi Gizi |
| Jadwal visite dokter untuk pasien | Kebutuhan awal | Rencana visite belum ada sebagai data; `CliPhysicianVisit` hanya riwayat |

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `NV-DEC-001` | Decision | Batas scope awal (panggilan pasien, display perawat, tablet pasien, pemesanan makan, perangkat, laporan waktu tanggap) disetujui | Rizki Gunawan | `superseded` oleh `NV-DEC-004` | Rizki Gunawan, 2026-10-28 | Pertanyaan 1 |
| `NV-DEC-002` | Decision | `blueprint_shape: SINGLE` atas scope awal | Rizki Gunawan | `superseded` oleh `NV-DEC-005` | Rizki Gunawan, 2026-10-28 | Pertanyaan 2 |
| `NV-DEC-003` | Decision | Menutup `NV-OQ-001`: tablet melengkapi bel kabel, tanpa integrasi hardware di MVP | Rizki Gunawan | `superseded` oleh `NV-DEC-004` — tablet keluar scope | Rizki Gunawan, 2026-10-28 | Pertanyaan 3; pembanding UL 1069, DIN VDE 0834 |
| `NV-DEC-004` | Decision | **Scope disederhanakan.** Tidak ada interaksi pasien. Nurse View = display pemantauan perawat per lantai, baca saja. Tablet pasien, panggilan pasien, pemesanan makan, dan jadwal visite untuk pasien keluar scope dan dicatat sebagai kandidat slice berikutnya | Rizki Gunawan | `approved` | Rizki Gunawan, 2026-10-28 | Pertanyaan 4 dan konfirmasinya; pembanding *electronic patient whiteboard* |
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
| `NV-AC-027` | **Given** login gagal (kata sandi salah, akun terkunci) untuk akun mana pun **When** dicoba **Then** pesan dan kode tetap sama dengan sebelum perubahan | `NV-DEC-030` (1) |

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
