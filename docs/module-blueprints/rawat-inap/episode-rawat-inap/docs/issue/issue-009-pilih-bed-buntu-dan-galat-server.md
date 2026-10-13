# ISSUE-009 — Langkah Pilih Bed admisi buntu, pembacaan bed boros, dan galat server yang tidak terbaca

```yaml
issue_id: ISSUE-EPS-009
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah Pilih Bed (FE-INP-03 bagian 3.9) dan Booking Bed (3.10); papan bed yang sama dipakai Papan Tempat Tidur (FE-INP-02) dan Bed Management"
sumber_laporan: "Permintaan pemilik sistem 13-10-2026: 3 butir, 1 screenshot langkah Pilih Bed, 1 potongan log konsol backend 13:54–13:58"
tanggal_issue: "2026-10-13"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Blocker
source_sha_backend: "86cb8ef8d3660e97776096fb33fcaf7dec7c9006 (MHamzah); folder InPatientManagement identik dengan 11b8d8fd yang sedang berjalan (DLL 13:47, proses 13:54)"
source_sha_frontend: "c2f42e4ce3af5e4d07ec2c963bcec3e97a4031aa (HamzahV2); working tree bersih"
bukti_log: "NewQuilvianSystemBackend/Logs/quilvian-backend-20261009.json, -20261012.json, -20261013.json"
rencana_perbaikan: ../plan-repair/plan-repair-009-pilih-bed-buntu-dan-galat-server.md
```

## 1. Ringkasan

Pada langkah **Pilih Bed** admisi rawat inap, petugas sudah memilih tempat tidur yang tersedia,
tetapi tombol **Lanjut ke Pemesanan** selalu berhenti dengan pesan merah "Admisi belum dapat
disimpan. Periksa data kamar dan isian Dokter sebelum melanjutkan." Ini butir paling berbahaya
(**Blocker**): tidak ada pasien yang bisa diadmisikan lewat jalur Pasien Baru maupun Pasien Lama.
Penyebabnya ada di layar, bukan di server: unit layanan admisi yang disembunyikan dari petugas
ikut terhapus saat pasien dipilih, lalu setiap tempat tidur dianggap "berbeda unit". Pesan yang
tampil pun menutupi sebab aslinya.

Audit juga menemukan bahwa satu kali membuka atau menyaring papan bed memicu dua permintaan
yang masing-masing bolak-balik ke database ±30–45 kali, papan mengabaikan penyaring Kamar/Kelas
sehingga bed di luar penyaring tampil "Tidak lolos kelayakan", dan ada satu galat 500 nyata pada
daftar episode yang lahir dari dua pembacaan bersamaan. Galat 500 lain di log 9–13 Oktober
ternyata permintaan yang dibatalkan peramban (sudah ditangani 13-10 13:23) dan kolom database
yang belum dimigrasi (sudah teratasi).

## 2. Laporan asli

| No. laporan | Keluhan pelapor (diringkas) | Lampiran |
| ---: | --- | --- |
| 1 | "Pada backend coba cek pada pemilihan bed agar query tidak berat dan bisa berjalan dengan lancar." | Log konsol: `available-beds` 529 ms / 294 ms, `bed-board` 414 ms / 311 ms, `bed-management/monitoring` 1.057 ms |
| 2 | "Bantu saya mengatasi masalah kenapa tidak bisa dilakukan lanjut pemesanan bed." | Screenshot langkah 5 dari 9 "Pilih Bed": penyaring Unit "Rawat Inap", Kelas "KELAS II", 34 bed / 28 tersedia, alert merah "Admisi belum dapat disimpan…" |
| 3 | "Audit kesalahan pada bed management dan modul admisi rawat inap, seperti forward error yang belum jelas dan kesalahan yang mengakibatkan error 500." | Tidak ada lampiran khusus; dijawab dari source dan log backend |

Seluruh lampiran dirujuk butir di atas. Log konsol yang dilampirkan berhenti pada
`GET bed-board` pukul 13:58:25 dan tidak memuat `POST /patient-encounters/admin` maupun
`POST /episodes`.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-009-01` | 2 | Unit layanan admisi kosong setelah pasien dipilih, sehingga setiap bed ditolak "berbeda unit" | `BUG` | FE | Blocker | SUDAH-VERIFIKASI di source; DARI-CAPTURE (screenshot, log tanpa POST) | `FIX-EPS-009-01` |
| `ISS-EPS-009-02` | 2, 3 | Alasan asli kegagalan ditimpa kalimat umum di langkah Pilih Bed | `BUG`, `RULE_VIOLATION` | FE | High | SUDAH-VERIFIKASI | `FIX-EPS-009-02` |
| `ISS-EPS-009-03` | 1 | Pembacaan bed bolak-balik ke database ±30–45 kali per permintaan | `BUG` | BE | Medium | SUDAH-VERIFIKASI di source; DARI-CAPTURE (waktu respons) | `FIX-EPS-009-03` |
| `ISS-EPS-009-04` | 1, 3 | Papan bed mengabaikan penyaring Kamar, Kelas, dan Pencarian | `CONTRACT_MISMATCH` | BE + dokumen | Medium | SUDAH-VERIFIKASI | `FIX-EPS-009-04`, `FIX-EPS-009-06` |
| `ISS-EPS-009-05` | 3 | HTTP 500 pada daftar episode: dua pembacaan bersamaan menggugurkan admisi Draft yang sama | `BUG` | BE | High | SUDAH-VERIFIKASI di log 12-10 dan source | `FIX-EPS-009-05` |
| `ISS-EPS-009-06` | 3 | HTTP 500 dari permintaan bed yang dibatalkan peramban karena terlalu lama (10–38 detik) | `BUG` | BE | Medium | SUDAH-VERIFIKASI di log 9–13 Okt | — sudah ditangani `6b8a8b20`; pemicunya dikurangi `FIX-EPS-009-03` |
| `ISS-EPS-009-07` | 3 | HTTP 500 "column AdmissionTransferId does not exist" | `DATA` | Database | High (pada saat itu) | SUDAH-VERIFIKASI di log 12-10; tidak muncul lagi 13-10 | — migration sudah diterapkan |
| `ISS-EPS-009-T1` | — | Pesan 409 bed memuat nama exception teknis `DbUpdateException` | `RULE_VIOLATION` | BE | Low | SUDAH-VERIFIKASI | `FIX-EPS-009-03` |
| `ISS-EPS-009-T2` | — | Papan bed menjadi 503 bila akun sistem expiry tidak dikonfigurasi di lingkungan lain | `DATA` | Konfigurasi | Medium | SUDAH-VERIFIKASI di source | — catatan rilis |
| `ISS-EPS-009-T3` | — | Pembatalan episode yang sudah ditempatkan mengembalikan bed ke "Tersedia" tanpa siklus pembersihan | `BUG` | BE | Medium | SUDAH-VERIFIKASI di source | `FIX-EPS-009-07` |
| `ISS-EPS-009-T4` | — | Kode galat server dibaca dari tempat yang salah, sehingga setiap 409 tampil sebagai "permintaan kamar pulih" | `CONTRACT_MISMATCH` | FE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-009-02` |

## 4. Rincian per temuan

### ISS-EPS-009-01 — Unit layanan admisi kosong setelah pasien dipilih, sehingga setiap bed ditolak "berbeda unit"

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE pada screenshot dan log |
| **Perbaikan** | `FIX-EPS-009-01` |

**Apa yang terjadi.** Petugas mencari pasien lama, memilih cara bayar, memilih DPJP, lalu memilih
tempat tidur kosong di "Rawat Inap · Kelas II". Saat menekan **Lanjut ke Pemesanan**, layar menolak
dan petugas tidak bisa maju ke Booking Bed. Mengulang dengan bed lain hasilnya sama.

**Kenapa terjadi.** Sejak 13-10-2026 (`FE-RWI-240`, `PLAN-REPAIR-DOK-006`) kunjungan dan episode
baru dibuat setelah bed dipilih. Unit layanan admisi disembunyikan dari petugas (`RWI-DEC-268`)
dan diisi otomatis dengan unit rawat inap pertama. Rantai sebabnya:

```text
Lanjut ke Pemesanan ditolak, tanpa POST apa pun ke server
  ← resolveInpatientAdmissionRoom(bed, form.serviceUnitId) melempar
    "Kamar yang dipilih berbeda dari unit layanan admisi", karena form.serviceUnitId = ""
    ← efek reset di hook Dokter mengosongkan seluruh isian, termasuk serviceUnitId,
      setiap kali pasien berganti (dari "" menjadi pasien terpilih)
      ← pengisian otomatis unit hanya berjalan SEKALI saat halaman dibuka, sebelum pasien dipilih
         dan tidak ada yang mengisinya kembali (dropdown-nya tersembunyi)
```

- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx:114-126`
  — efek reset `setForm({ ...DEFAULT_INPATIENT_DOCTOR_STEP_VALUES })` saat `patientId` berubah;
  `DEFAULT_INPATIENT_DOCTOR_STEP_VALUES.serviceUnitId` bernilai `""`
  (`inpatient-admission-flow-constants.jsx:665-671`).
- `use-inpatient-admission-doctor.jsx:218-224` dan `:244-249` — unit diisi dari opsi pertama hanya
  saat opsi dimuat, dan pemuatan hanya sekali saat hook dipasang.
- `inpatient-admission-doctor-step.jsx:92-100` — langkah Dokter hanya memuat ulang opsi bila
  daftarnya kosong; daftar sudah terisi, jadi unit tidak pernah diisi ulang.
- `inpatient-admission-encounter-utils.jsx:302-323` — validasi langkah Dokter sengaja tidak
  memeriksa unit, sehingga tombol tetap aktif dengan unit kosong.
- `inpatient-admission-encounter-utils.jsx:282-290`:

```js
export const resolveInpatientAdmissionRoom = (bed, serviceUnitId) => {
  if (!trimmed(bed?.bedId) || !trimmed(bed?.patientClassId)) { throw new Error("…kelas perawatan…"); }
  if (trimmed(bed.serviceUnitId) !== trimmed(serviceUnitId)) {
    throw new Error("Kamar yang dipilih berbeda dari unit layanan admisi. …");
  }
  return { patientClassId: trimmed(bed.patientClassId), selectedBedId: trimmed(bed.bedId) };
};
```

Penyaring papan tetap memperlihatkan "Rawat Inap" karena efek penyemaian di
`use-inpatient-admission-bed.jsx:67-72` berjalan pada commit yang sama dengan efek reset dan masih
membaca nilai unit lama. Akibatnya layar terlihat benar padahal isian unit admisi sudah kosong.

Seandainya unit tidak terhapus pun, jebakan yang sama tetap ada: petugas boleh mengganti penyaring
unit di papan, bed di unit lain tampil dapat dipilih, lalu ditolak dengan alasan yang sama. Alur
transfer IGD/Poli sudah memakai aturan yang benar — unit dan kelas diambil dari bed terpilih
(`inpatient-admission-transfer-utils.js:29-32`) — dan kontrak `RWI-KELAS-006-1.0` menyatakan
"server membaca kelas/unit dari kamar bed tersebut".

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `05-skema-tampilan.md` 3.9 menyatakan
"Lanjut ke Pemesanan" aktif dan maju ke Booking Bed bila satu bed `Tersedia` dipilih; amandemen
`PLAN-REPAIR-DOK-006` (bagian akhir dokumen yang sama) meminta kunjungan/episode dibuka dengan
kelas kamar setelah bed dipilih.

**Dampak nyata.** Petugas admisi tidak dapat menyelesaikan satu admisi pun lewat jalur Pasien
Baru/Lama sejak `FE-RWI-240` berjalan. Contoh: Tn. Budi (pseudonim), DPJP dr. Andi, bed
`RI-201-A` Kelas II — berhenti di Pilih Bed.

**Rekomendasi.** Unit admisi mengikuti bed terpilih (sama dengan alur transfer); unit bawaan
hanya dipakai untuk menyemai penyaring papan dan tidak hilang ketika pasien dipilih. Rincian di
plan repair `FIX-EPS-009-01`.

### ISS-EPS-009-02 — Alasan asli kegagalan ditimpa kalimat umum di langkah Pilih Bed

| | |
| --- | --- |
| **No. laporan** | 2, 3 |
| **Jenis** | `BUG`, `RULE_VIOLATION` (pesan tidak menunjuk sebab) |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-009-02` |

**Apa yang terjadi.** Apa pun sebab kegagalannya — unit tidak cocok, kunjungan ditolak server,
penjamin tidak terbaca, pasien punya permintaan dari kamar pulih — petugas hanya melihat
"Admisi belum dapat disimpan. Periksa data kamar dan isian Dokter sebelum melanjutkan." Petugas
kembali ke langkah Dokter, tidak menemukan apa-apa yang salah, lalu meminta bantuan TI.

**Kenapa terjadi.**

```text
alert umum di langkah Pilih Bed
  ← use-inpatient-admission-bed.jsx:495-498: bila admission.prepare() gagal, layar menulis
    kalimat tetap, bukan alasan yang dikembalikan
    ← doctor.submit() menaruh alasan aslinya di doctor.formError (…-doctor.jsx:479, :549, :620-631)
      dan hanya mengembalikan { success: false }
      ← formError hanya dirender di langkah Dokter (inpatient-admission-doctor-step.jsx:450);
         langkah Pilih Bed (inpatient-admission-bed-step.jsx:79-80) tidak membacanya
```

**Apakah ini menyimpang dari desain?** Ya. Komentar hook `use-inpatient-admission-doctor.jsx:613-631`
sengaja menyusun pesan berbeda per kode server (`INP-ADM-REF-001`, `INP-ADM-REF-002`, pesan
episode), dan daftar periksa skill menuntut pesan yang menunjuk isian atau sebab yang salah.
Sejak titik tulis 1 dipindah ke langkah Pilih Bed, pesan-pesan itu tidak pernah terlihat.

**Dampak nyata.** Butir `ISS-EPS-009-01` sebenarnya menulis alasan yang cukup jelas ("Kamar yang
dipilih berbeda dari unit layanan admisi…"), tetapi alasan itu tidak pernah sampai ke petugas.

**Rekomendasi.** `submit` mengembalikan `message`; langkah Pilih Bed menampilkannya apa adanya
dan memakai kalimat umum hanya bila tidak ada alasan sama sekali.

### ISS-EPS-009-03 — Pembacaan bed bolak-balik ke database ±30–45 kali per permintaan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` (kinerja) |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; waktu respons DARI-CAPTURE dan log |
| **Perbaikan** | `FIX-EPS-009-03` |

**Apa yang terjadi.** Setiap kali langkah Pilih Bed dibuka atau penyaring diganti, layar memanggil
`GET bed-board` dan `GET available-beds` bersamaan (`use-inpatient-bed-board.jsx:123-132`). Hari ini
keduanya 300–530 ms dan monitoring 1.057 ms. Sebelum `6b8a8b20` (13-10 13:23) permintaan yang sama
tercatat 2–38 detik di log; yang terlama `available-beds` 37,9 detik pada 13-10 11:46 WIB.

**Kenapa terjadi.** Waktu habis di jumlah bolak-balik ke database, bukan di query yang lambat —
indeks `BedId` pada penempatan, pemesanan, dan kesiapan sudah ada. Satu `available-beds` saat ini:

| Bagian | Bolak-balik | Sumber |
| --- | ---: | --- |
| Cek hak "InpatientBedOccupancy:Read" + daftar unit | 3–6 | `InpBedAccessService.cs:54-83`; `AccessPermissionService.cs:41-` membaca user dan role setiap cek |
| Gugurkan pemesanan kedaluwarsa | 1 | `InpBedMutationCoordinator.cs:156-162` |
| Kandidat bed | 1 | `InpBedOccupancyService.cs:170-174` |
| Proyeksi status (kamar, unit, kelas, kesiapan, penempatan, pemesanan, episode) | 7 | `InpBedManagementQueryService.cs:33-73` |
| Tiga cek hak aksi pembersihan + daftar unit masing-masing, walaupun tidak ada bed yang sedang dibersihkan | 9–15 | `InpBedManagementQueryService.cs:75-77`; `InpBedAccessService.cs:68-75` memuat ulang seluruh unit per operasi |
| Cek boleh-baca identitas pasien, **satu query per bed terisi/dipesan** | 3–4 + N | `InpBedManagementQueryService.cs:127-133`; `InpBedAccessService.cs:100-108` |
| Kesiapan, pemesanan, penempatan dimuat **ulang** | 3 | `InpBedOccupancyService.cs:187-200` |
| Nama unit dan kelas dimuat **ulang** | 2 | `InpBedOccupancyService.cs:274`, `:1947-1991` |

Contoh berangka pada screenshot: 34 bed, 6 terisi → ±35–45 bolak-balik untuk `available-beds`,
±25–30 untuk `bed-board`, dua-duanya sekaligus.

**Apakah ini menyimpang dari desain?** Kontrak tidak menetapkan batas waktu, tetapi
`03-frontend-architecture.md` 5.2 meminta papan dibaca ulang saat berubah; pembacaan yang boros
membuat pembacaan ulang itu mahal. Diperlakukan sebagai `BUG` kinerja.

**Rekomendasi.** Pakai ulang data yang sudah dimuat proyeksi, satukan cek identitas menjadi
pemeriksaan di memori, muat hak aksi pembersihan hanya bila ada bed dalam keadaan itu, dan simpan
daftar unit SuperAdmin sekali per permintaan. Perkiraan: ±15–18 bolak-balik untuk
`available-beds`, ±12–15 untuk `bed-board`, tanpa mengubah isi jawaban.

### ISS-EPS-009-04 — Papan bed mengabaikan penyaring Kamar, Kelas, dan Pencarian

| | |
| --- | --- |
| **No. laporan** | 1, 3 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend + dokumen kontrak |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-009-04`, `FIX-EPS-009-06` |

**Apa yang terjadi.** Petugas memilih Kelas II. Daftar bed yang dapat dipilih memang hanya Kelas II,
tetapi kartu bed Kelas I dan VIP di unit yang sama tetap tampil redup dengan label
"Tidak lolos kelayakan — Server tidak memasukkan tempat tidur ini ke hasil pencarian untuk pasien
tersebut". Angka ringkasan 34/28/6 tetap angka seluruh unit, bukan Kelas II.

**Kenapa terjadi.** Layar mengirim `serviceUnitId`, `roomId`, `patientClassId`, dan `search` ke
kedua endpoint (`inpatient-bed-utils.jsx:360-371`). `available-beds` memakai keempatnya
(`InpBedOccupancyService.cs:133-168`), tetapi `bed-board` hanya menerima `serviceUnitId`
(`InpatientBedOccupancyController.cs:94-98`, `InpBedOccupancyService.BedManagement.cs:98-100`),
padahal `MonitoringAsync` di bawahnya sudah mendukung `RoomId` dan `Search`. Bed yang tersaring
keluar dari `available-beds` tetapi masih ada di papan lalu dijelaskan sebagai "tidak lolos
kelayakan" (`inpatient-bed-utils.jsx:352-357`) — alasan yang salah.

**Apakah ini menyimpang dari desain?** `contracts/api-contract.md:1013` hanya mencantumkan
`serviceUnitId?` untuk `bed-board`, sedangkan desain frontend menganggap papan ikut tersaring
(`inpatient-bed-utils.jsx:593-610`, `inpatient-bed-board-constants.jsx:25-30` "kontraknya
menyediakan serviceUnitId dan pencarian"). Frontend dan kontrak backend tidak sepakat →
`CONTRACT_MISMATCH`. Perbaikan di sisi backend bersifat aditif (parameter opsional) dan pemanggil
lain tanpa penyaring (`use-inpatient-episode-worklist.jsx:290`) tidak berubah.

**Rekomendasi.** `bed-board` menerima `roomId`, `patientClassId`, `search` opsional dan memakai
penyaring yang sama dengan `available-beds`; kontrak API diamandemen.

### ISS-EPS-009-05 — HTTP 500 pada daftar episode: dua pembacaan bersamaan menggugurkan admisi Draft yang sama

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di log dan source |
| **Perbaikan** | `FIX-EPS-009-05` |

**Apa yang terjadi.** 12-10-2026 pukul 14:12:30 WIB, halaman daftar episode memanggil
`GET /episodes/summary` (200, 746 ms) dan `GET /episodes` (**500**, 778 ms) pada detik yang sama.
Petugas melihat "Terjadi kesalahan pada server."; memuat ulang 2 detik kemudian berhasil.

**Kenapa terjadi.**

```text
GET /episodes → 500
  ← DbUpdateException: 23505 duplicate key value violates unique constraint
    "IX_InpStatusHistory_EpisodeId_SequenceNumber"  (log 2026-10-12T07:12:30Z)
    ← ExpireDraftIfDueAsync (InpEpisodeService.cs:747-789) menggugurkan Draft yang lewat batas
      di dalam transaksi TANPA mengunci baris episode
      ← GetEpisodeListAsync dan GetEpisodeSummaryAsync (InpEpisodeService.Reads.cs:47, :155)
        sama-sama menjalankan ExpireDueDraftEpisodesAsync; keduanya membaca Draft yang sama,
        keduanya menghitung SequenceNumber = terakhir + 1 (InpEpisodeService.cs:664-677),
        yang kalah ditolak indeks unik dan tidak ditangkap
```

Pola yang sama untuk pemesanan bed sudah aman karena memakai `FOR UPDATE`
(`InpBedMutationCoordinator.cs:143-147`); pengguguran Draft belum.

**Apakah ini menyimpang dari desain?** Ya. `RWI-DEC-030` memilih pengguguran saat dibaca, dan
indeks unik `(EpisodeId, SequenceNumber)` adalah jaring pengaman terakhir — bukan cara normal
memberi tahu petugas.

**Dampak nyata.** Muncul setiap kali ada Draft yang lewat batas dan dua layar (atau dua panel satu
halaman) membaca daftar episode bersamaan. Detail episode dan langkah admisi yang memanggil
`ExpireDraftIfDueAsync` punya peluang yang sama.

**Rekomendasi.** Kunci baris episode di dalam transaksi pengguguran, baca ulang, lalu periksa ulang
status dan batas waktunya; pembaca kedua cukup melihat episode yang sudah `Cancelled`.

### ISS-EPS-009-06 — HTTP 500 dari permintaan bed yang dibatalkan peramban

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di log |
| **Perbaikan** | — sudah ditangani `6b8a8b20` (13-10 13:23); `FIX-EPS-009-03` mengurangi pemicunya |

**Apa yang terjadi.** Log 9, 12, dan 13 Oktober memuat 191 galat tak tertangani (semuanya 500);
136 di antaranya `OperationCanceledException`, terbanyak pada `bed-management/monitoring`,
`bed-board`, `available-beds`, dan `billing-status`. Sisanya 54 kolom belum dimigrasi
(`ISS-EPS-009-07`) dan 1 duplicate key (`ISS-EPS-009-05`). Permintaan berjalan 2–38 detik, petugas mengganti
penyaring atau pindah halaman, peramban membatalkan, lalu server mencatatnya sebagai galat 500.

**Kenapa terjadi.** Proyeksi lama memanggil `ProjectAsync` per bed (`InpBedManagementQueryService.cs`
baris 63 versi lama, terbaca pada stack trace), dan middleware lama memperlakukan pembatalan sebagai
crash. Commit `6b8a8b20` mengganti proyeksi menjadi `ProjectManyAsync` dan menambah penanganan
pembatalan menjadi 499 (`Middlewares/GlobalExceptionMiddleware.cs:25-33`). Server yang berjalan sejak
13:54 sudah memuat keduanya; log 13:54–14:09 tidak memuat 500 lagi.

**Rekomendasi.** Tidak ada perbaikan source baru. Dicatat supaya 500 lama di log tidak dikira
masalah yang masih terbuka.

### ISS-EPS-009-07 — HTTP 500 "column AdmissionTransferId does not exist"

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `DATA` |
| **Area** | Database |
| **Keparahan** | High pada saat itu |
| **Status bukti** | SUDAH-VERIFIKASI di log |
| **Perbaikan** | — |

**Apa yang terjadi.** 12-10-2026 11:38–12:29 WIB, 54 permintaan ke `admission-transfers`,
`episodes`, dan `patient-encounters` gagal 500 dengan `42703: column r0.AdmissionTransferId does not
exist`. Kode sudah membaca kolom baru, tetapi database belum di-`update`. Sejak 13-10 tidak muncul
lagi dan `admission-transfers` menjawab 200 pada log pelapor.

**Rekomendasi.** Tidak ada perbaikan source. Pencegahan: setiap kali menarik commit yang membawa
migration, jalankan `dotnet ef database update` sebelum menjalankan backend.

## 5. Tanya-jawab pelapor

> **T:** Kenapa tidak bisa lanjut ke pemesanan bed?
>
> **J:** Karena layar mengira bed yang dipilih berada di unit yang berbeda dari unit admisi —
> padahal unit admisi sudah terhapus kosong saat pasien dipilih. Kegagalan terjadi di layar
> sebelum permintaan apa pun dikirim (itulah sebabnya log tidak memuat `POST`), dan alasan
> sebenarnya tertutup kalimat umum. Perbaikannya: unit admisi mengikuti bed yang dipilih, dan
> alasan apa pun ditampilkan apa adanya.

> **T:** Apakah query pemilihan bed memang berat?
>
> **J:** Query-nya sendiri ringan dan sudah berindeks; yang berat adalah jumlah bolak-baliknya —
> ±35–45 kali untuk satu pencarian bed, dua pencarian sekaligus setiap penyaring berganti. Rencana
> perbaikan menurunkannya kira-kira separuh tanpa mengubah isi jawaban.

> **T:** Kesalahan apa saja yang menyebabkan error 500?
>
> **J:** Dari log 9–13 Oktober ada tiga sumber: (1) permintaan yang dibatalkan peramban karena
> terlalu lama — sudah ditangani hari ini 13:23; (2) kolom database yang belum dimigrasi —
> sudah teratasi; (3) dua pembacaan daftar episode bersamaan yang menggugurkan admisi Draft yang
> sama — masih terbuka, diperbaiki `FIX-EPS-009-05`.

## 6. Temuan tambahan

### ISS-EPS-009-T1 — Pesan 409 bed memuat nama exception teknis

| | |
| --- | --- |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-009-03` |
| **Kenapa dicantumkan** | Termasuk "forward error belum jelas" pada butir 3 |

`InpBedMutationCoordinator.cs:107-111` menyusun "Data bed berubah bersamaan. Muat ulang data: " +
`ex.GetType().Name`, sehingga petugas membaca "…: DbUpdateException". Kalimat petugas cukup
"Data bed berubah bersamaan. Muat ulang data."; detail teknis milik log.

### ISS-EPS-009-T2 — Papan bed menjadi 503 bila akun sistem expiry tidak dikonfigurasi

| | |
| --- | --- |
| **Jenis** | `DATA` (konfigurasi lingkungan) |
| **Area** | Konfigurasi backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | — catatan rilis pada plan repair bagian 8 |
| **Kenapa dicantumkan** | Gejala "papan tidak bisa dibuka" yang akan muncul di lingkungan lain |

`InpBedMutationCoordinator.cs:178-184` menolak dengan 503 "Akun sistem expiry dan bukti
penugasannya belum dapat diverifikasi" bila ada pemesanan kedaluwarsa dan
`BedManagement:ReservationExpiryActorUserId` atau `ReservationExpiryAssignmentReference` kosong.
Karena pengguguran berjalan di setiap pembacaan papan, seluruh papan ikut gagal. `appsettings.json`
lokal sudah mengisinya; lingkungan UAT/produksi wajib mengisinya juga.

### ISS-EPS-009-T3 — Pembatalan episode yang sudah ditempatkan melewati siklus pembersihan bed

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-009-07` |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri `ISS-EPS-009-05`; menyentuh bed management |

Pembatalan episode `Admitted` oleh supervisor (`CancelAdmissionAsync` → `ReleaseBedHoldsAsync`,
`InpEpisodeService.cs:854-978`) menutup penempatan lalu menyetel `MstBed.BedStatus = Available`
langsung, tanpa `InvalidateAsync` pada akar kesiapan. Jalur pulang normal
(`ReleaseBedStatusCopyAsync`, `InpBedOccupancyService.cs:1865-1920`) memindahkan bed bekas pakai ke
`WaitingCleaning`. Akibatnya bed yang baru saja dipakai pasien dapat langsung tampil "Tersedia" dan
dipesan pasien berikutnya tanpa dibersihkan. Jarang terjadi, tetapi berisiko klinis.

### ISS-EPS-009-T4 — Kode galat server dibaca dari tempat yang salah

| | |
| --- | --- |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-009-02` (dikerjakan `FE-RWI-243`) |
| **Kenapa dicantumkan** | Ditemukan saat mengerjakan `FIX-EPS-009-02`; termasuk "forward error belum jelas" butir 3 |

`use-inpatient-admission-doctor.jsx` membaca kode penolakan dari `data.code`, padahal `InpatientEpisodeController.FromStatus` mengirimnya di `data.errors.code` (`Responses/ApiResponse.cs:13`). Kodenya selalu kosong, dan cabang cadangan `status === 409` membuat setiap 409 — misalnya "Kunjungan ini sudah punya episode rawat inap." — tampil sebagai "Pasien memiliki permintaan admisi dari kamar pulih". Petugas lalu mencari permintaan kamar pulih yang tidak ada.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada. Ketiga butir dapat dibuktikan dari source, screenshot, dan log.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| — | Perbaikan butir 1–3 | — | **Sudah diputuskan** lewat permintaan pemilik 13-10-2026 ("tolong bantu saya mengatasi masalah", "coba cek… agar query tidak berat") | Tidak ada |
| K-01 | Unit admisi mengikuti bed terpilih (unit bawaan hanya menyemai penyaring papan) | Ikut bed / kunci papan ke unit bawaan | Ikut bed — sama dengan alur transfer dan kontrak `RWI-KELAS-006-1.0`; tidak menambah kebijakan baru | Tidak; dipakai sebagai bawaan rekomendasi dan dapat dibalik pemilik |

## 8. Catatan pola

1. **Validasi yang berjalan di langkah lain kehilangan tempat tampilnya.** Ketika titik tulis
   dipindah dari langkah Dokter ke langkah Pilih Bed (`FE-RWI-240`), pesan galatnya tetap ditulis ke
   state langkah Dokter. Pola pencegahan: fungsi titik tulis mengembalikan `message`, dan setiap
   pemanggil menampilkannya di langkah tempat tombol ditekan.
2. **Isian tersembunyi yang diisi otomatis harus tahan reset.** `RWI-DEC-268` menyembunyikan unit;
   sejak itu tidak ada lagi jalan bagi petugas memperbaiki nilai yang kosong. Isian tersembunyi
   sebaiknya diturunkan dari data yang dipilih (bed), bukan disalin sekali dari daftar opsi.
3. **Pengguguran saat dibaca butuh kunci baris.** Pemesanan bed sudah dikunci; Draft episode belum.
   Setiap pembaca yang juga menulis wajib memakai `FOR UPDATE` + baca ulang.
4. **Log backend adalah bukti 500 terbaik.** `Logs/quilvian-backend-*.json` memuat stack trace
   lengkap; tiga sumber 500 pada issue ini ditemukan di sana, bukan di layar.
5. Issue serupa: `ISSUE-EPS-006` (transfer, pola unit/kelas dari bed) dan `ISSUE-DOK-006`
   (kelas mengikuti kamar).

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Issue dibuat dari laporan 3 butir, 1 screenshot, 1 potongan log; ditambah audit log backend 9–13 Oktober | `diagnose-module-issue` |
| 2026-10-13 | Status `DALAM_PERBAIKAN`: pemilik meminta langsung diperbaiki pada laporan yang sama | `diagnose-module-issue` |
| 2026-10-13 | Temuan tambahan `ISS-EPS-009-T4` dicatat saat `FE-RWI-243` dikerjakan; ditutup oleh `FIX-EPS-009-02` | `build-module-frontend` |
| 2026-10-13 | Perbaikan `FIX-EPS-009-01` s.d. `06` selesai pada tingkat source (`FE-RWI-243`, `BE-RWI-228`, `BE-RWI-229`) dan dokumen; status tetap `DALAM_PERBAIKAN` sampai uji runtime pemilik dan keputusan `FIX-EPS-009-07` | `build-module-frontend`, `build-module-backend` |
