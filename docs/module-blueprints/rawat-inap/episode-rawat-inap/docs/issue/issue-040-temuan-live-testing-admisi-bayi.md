# ISSUE-040 — Temuan live testing Admisi Bayi Baru Lahir: gelang bayi salah nama, bed Perinatologi bocor ke kamar ibu lain, dan validasi waktu lahir

```yaml
issue_id: ISSUE-EPS-040
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Admisi Bayi Baru Lahir, Langkah 1–8 (route /health-services/inpatient-management/admissions/newborn)"
sumber_laporan: "Laporan live browser testing 25-10-2026 (testing/test-by-agy/laporan-live-testing-admisi-bayi-baru-lahir.md): 10 isu, ISU-10 berisi 10 catatan; 70 screenshot dan 1 log jaringan"
tanggal_issue: "2026-10-25"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "985a1fc75c2bb69163251bb7744bead949ae7285 (MHamzah)"
source_sha_frontend: "9cad353ba926ca3750ca03791b2ff4d5b6fddf5a (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-040-temuan-live-testing-admisi-bayi.md
```

---

## 1. Ringkasan

Issue ini menganalisis temuan live browser testing pada layar **Admisi Bayi Baru Lahir** tanggal 25
Oktober 2026. Ketiga jalur perawatan (Rawat Gabung, Perinatologi, NICU) berhasil sampai Kartu
Pasien, sehingga tidak ada butir `Blocker`. Laporan dipecah menjadi **20 butir** ditambah **3 temuan
tambahan** dari penelusuran source.

Dua butir paling berbahaya menyangkut keselamatan pasien:

1. **Gelang bayi mencetak nama yang salah.** Awalan "NY." tercetak dua kali, dan nama bayi yang diisi
   petugas tidak dipakai, sehingga gelang dan label bayi yang sama memuat nama berbeda.
2. **Bayi Perinatologi bisa dipesankan boks di kamar ibu lain atau bed NICU.**

Satu butir lagi membuat bayi yang lahir pukul 00.00–06.59 WIB **tidak bisa didaftarkan** karena server
membandingkan tanggal lahir dengan tanggal UTC.

Dua butir ternyata **sesuai desain yang berlaku** dan ditulis jujur sebagai `DESIGN_CHANGE`: daftar
DPJP yang tidak dibatasi dokter anak, dan klausul persetujuan yang tercentang sejak awal. Keduanya
membutuhkan keputusan pemilik.

Satu butir (CSP foto profil) berada di luar modul ini dan diteruskan ke pemilik konfigurasi platform
frontend.

---

## 2. Laporan asli

Pelapor adalah sesi live browser testing oleh agent. Nomor laporan mengikuti nomor isu pada laporan
live testing; ISU-10 dipecah per catatan.

| No. laporan | Keluhan (ringkas, kata-kata laporan) | Lampiran yang merujuk |
| --- | --- | --- |
| ISU-01 | Gelang bayi tercetak "BY. NY. NY. …" di 3/3 jalur; nama khusus bayi tidak dipakai di gelang, sedangkan label memakainya | `37`, `38`, `57`, `58`, `79` |
| ISU-02 | Bayi Perinatologi ditawari NICU-1-B, 304-BOKS, dan 305-BOKS (kamar ibu lain); semuanya bisa dipesan | `45`, `46` |
| ISU-03 | Backend memeriksa "masa depan" dengan tanggal UTC; bayi lahir 00.00–06.59 WIB akan ditolak (bukti kode) | — |
| ISU-04 | Jam lahir yang belum terjadi (23.50 saat jam 11.15) dan tanggal 01-01-1990 diterima | `18`, `20` |
| ISU-05 | NIK penanda tangan menerima `ABCDE<script>999…`; backend hanya membatasi 50 karakter | `33` |
| ISU-06 | Daftar DPJP bayi memuat 11 dokter semua spesialisasi | `23`, `25` |
| ISU-07 | 12 klausul persetujuan sudah tercentang sejak awal | `29` |
| ISU-08 | Picker tanggal lahir terpotong di bawah layar 1920×945 | `19` |
| ISU-09 | "Tempat Tidur Pasien Ibu: Belum terbaca" pada konfirmasi rawat gabung | `26` |
| ISU-10.1 | Status kasus tampil "InProgress"/"Completed" | `10` |
| ISU-10.2 | Kartu Pasien menulis "menunggu konfirmasi kedatangan" padahal sudah Admitted dan bed Terisi | `39` |
| ISU-10.3 | Ringkasan "Perawatan" hanya "Neonatal terpisah" untuk Perinatologi dan NICU | `55`, `77` |
| ISU-10.4 | Bed NICU-1-A menampilkan dua alasan sekaligus | `74` |
| ISU-10.5 | Tombol "Pesan Tempat Tidur" mati ±6 detik tanpa keterangan | `49` |
| ISU-10.6 | Nama bed berbeda antarlangkah ("303-BOKS" vs "BD-RSMMC-BOKS-303"); BAYI-1-A berkode umum | `26`, `35`, `55` |
| ISU-10.7 | `surgical-booking-lookup` 404 untuk bayi; `general-consent/print-data` 409 sebelum admisi dikonfirmasi | log jaringan |
| ISU-10.8 | Nama bayi menerima tag HTML | `21` |
| ISU-10.9 | Kolom NIK bayi menerima huruf saat diketik | `17` |
| ISU-10.10 | Foto profil diblokir Content Security Policy | log konsol |

Lampiran berupa berkas `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-bayi-baru-lahir/<nomor>-*.png`.
Seluruh lampiran dirujuk butir di atas, kecuali screenshot jalur normal yang menjadi bukti kelulusan.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-040-01` | ISU-01 | Gelang bayi "BY. NY. NY." dan mengabaikan nama resmi bayi | `BUG` | Backend | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-01`, `-02` |
| `ISS-EPS-040-02` | ISU-02 | Bed Perinatologi menawarkan boks kamar ibu lain dan bed NICU | `BUG` | Backend | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-03`, `-04` |
| `ISS-EPS-040-03` | ISU-03 | Tanggal lahir dibandingkan dengan tanggal UTC | `BUG` | Backend (area Manajemen Pasien) | High | SUDAH-VERIFIKASI di source; perilaku runtime DUGAAN | `FIX-EPS-040-05` |
| `ISS-EPS-040-04` | ISU-04 | Waktu lahir yang belum terjadi dan tanggal absurd diterima | `BUG`, `RULE_VIOLATION` | Frontend + Backend | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-06`, `-07` |
| `ISS-EPS-040-05` | ISU-05 | Identitas wali tidak divalidasi sesuai skema | `BUG` | Frontend + Backend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-08` |
| `ISS-EPS-040-06` | ISU-06 | DPJP bayi tidak dibatasi dokter anak | `DESIGN_CHANGE` | Frontend + Backend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-040-09` |
| `ISS-EPS-040-07` | ISU-07 | Klausul persetujuan tercentang sejak awal | `DESIGN_CHANGE` | Frontend + dokumen | Medium | SUDAH-VERIFIKASI | `FIX-EPS-040-10` |
| `ISS-EPS-040-08` | ISU-08 | Picker tanggal membuka ke bawah lalu terpotong | `BUG` | Frontend (base component) | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-11` |
| `ISS-EPS-040-09` | ISU-09 | Tempat tidur dan kelas ibu "Belum terbaca" | `CONTRACT_MISMATCH` | Backend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-040-12` |
| `ISS-EPS-040-10` | ISU-10.1 | Status kasus berbahasa Inggris mentah | `RULE_VIOLATION` | Backend | Low | SUDAH-VERIFIKASI | `FIX-EPS-040-13` |
| `ISS-EPS-040-11` | ISU-10.2 | Pesan Kartu Pasien tidak sesuai status episode | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-040-14` |
| `ISS-EPS-040-12` | ISU-10.3 | Ringkasan perawatan tidak membedakan Perinatologi dan NICU | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-040-15` |
| `ISS-EPS-040-13` | ISU-10.4 | Bed terisi juga diberi alasan "belum siap atau masih dipegang" | `BUG` | Backend | Low | SUDAH-VERIFIKASI | `FIX-EPS-040-16` |
| `ISS-EPS-040-14` | ISU-10.5 | Tombol "Pesan Tempat Tidur" mati tanpa penjelasan | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-040-17` |
| `ISS-EPS-040-15` | ISU-10.6 | Nama bed tidak konsisten antarlangkah | `BUG`, `DATA` | Frontend + data master | Low | SUDAH-VERIFIKASI | `FIX-EPS-040-18` |
| `ISS-EPS-040-16` | ISU-10.7 | Lookup booking operasi dipanggil untuk pasien bayi | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-040-19` |
| `ISS-EPS-040-17` | ISU-10.7 | Data calon penanda tangan ditolak 409 selama admisi | `CONTRACT_MISMATCH` | Frontend + Backend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-040-20` |
| `ISS-EPS-040-18` | ISU-10.8 | Nama bayi menerima tag HTML | `RULE_VIOLATION` | Frontend + Backend | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-21` |
| `ISS-EPS-040-19` | ISU-10.9 | NIK bayi menerima huruf saat diketik | `RULE_VIOLATION` | Frontend | Low | DARI-CAPTURE | `FIX-EPS-040-21` |
| `ISS-EPS-040-20` | ISU-10.10 | Foto profil diblokir CSP | `BUG` | Frontend (konfigurasi platform) | Low | DARI-CAPTURE | Di luar modul |
| `ISS-EPS-040-T1` | — | Nama ibu/wali tidak terisi otomatis pada Persetujuan Wali | `CONTRACT_MISMATCH` | Frontend + Backend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-20` |
| `ISS-EPS-040-T2` | — | Nomor episode dan encounter memakai jam UTC | `BUG` | Backend | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-040-22` |
| `ISS-EPS-040-T3` | — | Tag Swagger persetujuan tidak mengikuti pola modul | `RULE_VIOLATION` | Backend | Low | SUDAH-VERIFIKASI | `FIX-EPS-040-23` |

Keterangan keparahan: skala issue (`Blocker`/`High`/`Medium`/`Low`) berbeda dari skala risiko live
testing (P0–P5). Contoh: ISU-04 berlabel "P2 · Sedang" pada laporan live testing, tetapi di sini `High`
karena data lahir yang salah tersimpan tanpa disadari.

---

## 4. Rincian per temuan

### ISS-EPS-040-01 — Gelang bayi "BY. NY. NY." dan mengabaikan nama resmi bayi

| | |
| --- | --- |
| **No. laporan** | ISU-01 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; tampilan DARI-CAPTURE (`37`, `57`, `58`, `79`) |
| **Perbaikan** | `FIX-EPS-040-01`, `FIX-EPS-040-02` |

**Apa yang terjadi.** Di langkah 7 Dokumen Bayi, gelang selalu bertuliskan "BY. NY. NY. [NAMA IBU] /
BAYI 1". Bila petugas memberi nama bayi di langkah 2 (contoh uji: "Bagas Pratama"), kartu pasien dan
label memakai nama itu, tetapi gelang tetap memakai nama ibu. Gelang dan label untuk bayi yang sama
jadi memuat dua nama berbeda.

**Kenapa terjadi.**

```text
gelang "BY. NY. NY. SITI AMINAH / BAYI 1"
  ← sebab langsung: aturan gelang selalu merangkai "BY. NY. " + nama ibu mentah
    ← akar: nama ibu di master pasien sudah berawalan "Ny.", dan aturan gelang ditulis terpisah
       dari aturan nama pasien bayi — tidak memakai pembersih awalan maupun nama resmi pasien
```

`NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpWristbandRules.cs:123-126`
(SUDAH-VERIFIKASI):

```csharp
if (isNewborn && !string.IsNullOrWhiteSpace(motherName))
{
    return $"BY. NY. {motherName.Trim().ToUpperInvariant()}{(birthOrder > 0 ? $" / BAYI {birthOrder}" : string.Empty)}";
}
```

Pendaftaran bayi sudah punya pembersih awalan di
`.../PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs:407-416`
(`NormalizeMotherName` membuang "Ny. ", "Nyonya ", "Ibu "). Label pasien pada respons yang sama sudah
memakai nama resmi pasien:
`.../InPatientManagement/Services/InpAdmissionWorkspaceQueryService.cs:386-387`
(`patient.FullName` + "/ BAYI n"). Gelang memakai jalur berbeda di baris 409-410 dari berkas yang sama.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. Skema BAYI-S09 pada
`plan-repair-016-alur-admisi-episode-bayi.md:654` menetapkan isi gelang "Nama bayi awal/resmi **dan**
nama ibu". Keputusan rapat MTG-BAYI-04 (`issue-016`, baris 75) mewajibkan nama ibu ada di gelang,
tetapi "tidak menyatakan nama ibu sebagai satu-satunya pengenal". Layar langkah 2 juga menjanjikan
"nama ini akan digunakan pada kartu dan gelang pasien". Detail format gelang masih berstatus keputusan
terbuka K-02 pada issue-016.

**Dampak nyata.** Perawat ruang bayi mencocokkan gelang Bayi B dengan label botol ASI. Gelang
bertuliskan "BY. NY. NY. IBU B", label bertuliskan "BAGAS PRATAMA". Perawat ragu, atau menganggap
keduanya bayi berbeda. Ketidakcocokan ini persis yang ingin dicegah Sasaran Keselamatan Pasien 1.

**Rekomendasi.** Pisahkan dua hal:
- Awalan ganda dibersihkan sekarang, tanpa menunggu keputusan apa pun.
- Format gelang (nama resmi bayi plus baris nama ibu) menunggu keputusan K-01.

Rinciannya di plan repair.

---

### ISS-EPS-040-02 — Bed Perinatologi menawarkan boks kamar ibu lain dan bed NICU

| | |
| --- | --- |
| **No. laporan** | ISU-02 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; respons server DARI-CAPTURE (`45`, `46`, log jaringan) |
| **Perbaikan** | `FIX-EPS-040-03`, `FIX-EPS-040-04` |

**Apa yang terjadi.** Bayi B didaftarkan untuk Ruang Perinatologi (non-intensif). Daftar bed yang
layak dari server berisi 10 bed yang semuanya bisa dipesan:
- 7 bed R. Bayi;
- NICU-1-B;
- 304-BOKS (boks di kamar ibunya sendiri);
- **305-BOKS di kamar VIP 305 yang sedang ditempati Ibu C**.

Di denah, dialog VIP 305 menampilkan "305-A Terisi · [Ibu C]" dan tepat di bawahnya "305-BOKS
Tersedia · Pilih bed ini".

**Kenapa terjadi.**

```text
305-BOKS dapat dipesan untuk bayi Perinatologi
  ← sebab langsung: aturan kelayakan bayi hanya memeriksa "bed khusus bayi", "rawat gabung wajib
     kamar ibu", dan "intensif wajib bed intensif"
    ← akar: mode Neonatal non-intensif tidak punya aturan tempatnya sendiri; boks rawat gabung
       (bed bayi di kamar dewasa) dan bed NICU sama-sama lolos karena sama-sama IsForNewborn
```

`.../InPatientManagement/Services/InpNewbornAdmissionRules.cs:83-101` (SUDAH-VERIFIKASI):

```csharp
if (patient.IsNewborn && !bed.IsForNewborn)
    failures.Add(("NEWBORN_BED_REQUIRED", ...));
...
if (isAdmission && patient.IsNewborn && episode?.NewbornCareMode == "RoomingIn" &&
    (!motherRoomId.HasValue || motherRoomId.Value != room.Id))
    failures.Add(("NEWBORN_ROOMING_IN_MISMATCH", ...));
if (isAdmission && patient.IsNewborn && episode?.RequiresNeonatalIntensiveCare == true &&
    !bed.IsIntensiveCareBed && !room.IsIntensiveCare)
    failures.Add(("NEWBORN_INTENSIVE_REQUIRED", ...));
```

Master kamar sudah punya penanda yang cukup untuk membedakan: `MstRoom.IsForNewborn` dan
`MstRoom.IsIntensiveCare` (`.../MasterData/Models/MstRoom.cs:39-43`). Nilai penanda itu pada kamar
R. Bayi, Ruang NICU, dan VIP 301–305 belum diperiksa (DUGAAN). Cara membuktikannya: jalankan query
baca-saja pada bagian 7 (P-01).

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `issue-031` (disetujui 21-10-2026) memetakan
setiap kartu ke jenis ruangnya:
- Kartu 2 Perinatologi → "Kamar BAYI-1 & BAYI-2 Lantai 2";
- Kartu 3 NICU → kamar NICU;
- Rawat Gabung → boks di kamar ibu.

Kartu Perinatologi di layar (`inpatient-newborn-admission-constants.js`) juga menyebut "Kamar BAYI-1 &
BAYI-2 Lantai 2". Satu hal belum diputuskan: boleh tidaknya bayi non-intensif memakai bed NICU saat
ruang bayi penuh (K-02).

**Dampak nyata.** Petugas yang terburu-buru mengklik VIP 305 dan memesan 305-BOKS. Bayi B dirawat di
samping Ibu C, dan Ibu C bisa saja menyusui bayi yang bukan bayinya. Skenario lain: satu-satunya bed
NICU kosong terpakai bayi sehat, lalu bayi kritis berikutnya tidak mendapat inkubator.

**Rekomendasi.** Mode Neonatal hanya menerima bed di kamar khusus bayi; boks di kamar dewasa hanya
untuk rawat gabung dengan ibunya sendiri. Penolakan NICU untuk bayi non-intensif menunggu K-02.

---

### ISS-EPS-040-03 — Tanggal lahir dibandingkan dengan tanggal UTC

| | |
| --- | --- |
| **No. laporan** | ISU-03 |
| **Jenis** | `BUG` |
| **Area** | Backend — berkas di area Manajemen Pasien (dibangun `BE-RWI-231`, diubah juga `BE-PAT-MIG-003A`) |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; perilaku runtime DUGAAN (uji live dilakukan ±11.15 WIB) |
| **Perbaikan** | `FIX-EPS-040-05` |

**Apa yang terjadi.** Bayi yang lahir antara pukul 00.00 dan 06.59 WIB akan ditolak server dengan pesan
"Tanggal lahir bayi wajib valid dan tidak boleh di masa depan." Layar sendiri menerima tanggal itu.

**Kenapa terjadi.**

```text
pendaftaran bayi 26-10 pukul 01.15 WIB ditolak sebagai "masa depan"
  ← sebab langsung: tanggal lahir (tanggal WIB) dibandingkan dengan DateTime.UtcNow.Date
    ← akar: tanggal kalender rumah sakit diperlakukan sama dengan tanggal UTC; layar memakai
       Asia/Jakarta (inpatient-newborn-admission-utils.js:72-75), server tidak
```

`.../PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs:93-94` (SUDAH-VERIFIKASI):

```csharp
if (!request.BirthDate.HasValue || request.BirthDate.Value.Date > DateTime.UtcNow.Date)
    return Invalid("Tanggal lahir bayi wajib valid dan tidak boleh di masa depan.");
```

Contoh hitungan: pukul 01.15 WIB tanggal 26-10-2026 sama dengan 25-10-2026 pukul 18.15 UTC. Server
membandingkan 26-10 > 25-10, lalu menolak. Pola tanggal lokal rumah sakit sudah ada di area Rawat Inap:
`InpAdmissionText.LocalToday(InpAdmissionSourceReader.ResolveTimeZone(hospital))`
(`InpAdmissionWorkspaceQueryService.cs:380`).

Cara membuktikan perilaku runtime: uji unit dengan jam tiruan 2026-10-26T01:15+07:00, atau uji layar
antara pukul 00.00 dan 06.59 WIB.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `RWI-RULE-014` pada `00-interview-decisions.md`
memakai contoh "Ibu Rina melahirkan pukul **03:20**" sebagai alur normal. Kontrak 022 (baris 116) hanya
menolak tanggal/jam lahir yang memang tidak valid.

**Dampak nyata.** Bidan jaga malam selesai SC pukul 02.10 WIB dan mendaftarkan bayi. Server menolak.
Petugas mencoba berulang, lalu menunggu sampai pukul 07.00 atau mengisi tanggal kemarin. Pilihan kedua
membuat tanggal lahir di rekam medis salah.

**Rekomendasi.** Bandingkan gabungan tanggal dan jam lahir dengan waktu sekarang dalam zona waktu rumah
sakit. Karena berkasnya berada di area Manajemen Pasien, pemilik modul `patient-management` perlu
diberi tahu.

---

### ISS-EPS-040-04 — Waktu lahir yang belum terjadi dan tanggal absurd diterima

| | |
| --- | --- |
| **No. laporan** | ISU-04 |
| **Jenis** | `BUG`, `RULE_VIOLATION` |
| **Area** | Frontend + Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; request terkirim DARI-CAPTURE (`18`, `20`, dicegat agent) |
| **Perbaikan** | `FIX-EPS-040-06`, `FIX-EPS-040-07` |

**Apa yang terjadi.** Layar menerima jam lahir 23.50 untuk hari ini, padahal saat itu baru pukul 11.15.
Layar juga menerima tanggal lahir 1 Januari 1990 untuk bayi baru lahir. Kedua isian dikirim ke server.
Dari pembacaan kode, server juga akan menerimanya.

**Kenapa terjadi.**

```text
waktu lahir mustahil lolos
  ← sebab langsung: frontend hanya membandingkan tanggal (tanpa jam) dan tidak punya batas bawah;
     backend hanya memeriksa tanggal > hari ini
    ← akar: aturan "waktu lahir harus masuk akal untuk kasus persalinan ini" belum pernah
       dirumuskan; validasi berhenti pada format
```

`QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js:67-80`
(SUDAH-VERIFIKASI): pemeriksaan `birthDate > today` hanya membandingkan string tanggal, dan jam hanya
dicek formatnya. Backend `NewbornPatientRegistrationService.cs:93-96` hanya menolak tanggal di masa
depan dan jam yang kosong. Picker tanggal tidak punya dukungan batas `min`/`max`
(`filter-date-picker.jsx`, tidak ada `minDate`/`maxDate`), sehingga tanggal masa depan pun tetap bisa
diklik.

**Apakah ini menyimpang dari desain?** `BUG`. Kontrak 022 (baris 116) menyatakan server menolak
"tanggal/jam lahir tidak valid", sedangkan jam yang belum terjadi tidak valid. Batas bawah tanggal belum
ditetapkan dokumen mana pun, sehingga batas itu menjadi keputusan K-03. Unsur `RULE_VIOLATION`: layar
menawarkan waktu yang nanti tetap harus ditolak bila server diperbaiki.

**Dampak nyata.** Petugas salah memilih tahun di picker. Bayi tercatat lahir tahun 1990, gelang
menampilkan umur 36 tahun, dan aturan neonatal (dosis, laporan) membaca umur yang salah.

**Rekomendasi.** Tolak waktu lahir yang belum terjadi (gabungan tanggal dan jam, zona WIB) di layar dan
server. Batas bawah mengikuti K-03.

---

### ISS-EPS-040-05 — Identitas wali tidak divalidasi sesuai skema

| | |
| --- | --- |
| **No. laporan** | ISU-05 |
| **Jenis** | `BUG` |
| **Area** | Frontend + Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; request DARI-CAPTURE (`33`, dicegat agent) |
| **Perbaikan** | `FIX-EPS-040-08` |

**Apa yang terjadi.** Kolom "Nomor Identitas (NIK / Paspor)" menerima huruf, simbol, dan teks seperti
`ABCDE<script>99999999999999` (27 karakter), lalu mengirimnya ke server. Tombol simpan juga sudah aktif
sebelum NIK diisi.

**Kenapa terjadi.**

- `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-consent-form-step.jsx:260-266`:
  `canSubmit` hanya mensyaratkan nama ≥ 3 karakter, 12 klausul, dan tanda tangan. NIK, No. HP, dan
  hubungan tidak disyaratkan.
- Baris 342 mengirim `signerIdentityNumber` apa adanya.
- `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionConsentDtos.cs:35-36`
  hanya membatasi `MaxLength(50)`.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `05-skema-tampilan.md` bagian 3.12 (baris 654-659)
menandai **NIK \***, **No. HP \***, dan **Hubungan \*** sebagai wajib, dengan contoh NIK 16 digit.

**Dampak nyata.** Dokumen General Consent bertanda tangan menyimpan nomor identitas "ABCDE…". Saat
terjadi sengketa, rumah sakit tidak bisa menunjukkan siapa yang sebenarnya menandatangani.

**Rekomendasi.** Wajibkan NIK, No. HP, dan hubungan sesuai skema. Validasi formatnya di layar dan
server. Penerimaan paspor mengikuti K-04; sampai diputuskan, pakai rekomendasinya.

---

### ISS-EPS-040-06 — DPJP bayi tidak dibatasi dokter anak

| | |
| --- | --- |
| **No. laporan** | ISU-06 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend + Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-09` (menunggu K-05) |

**Apa yang terjadi.** Langkah 3 menampilkan 11 dokter dari semua spesialisasi (Gigi, Ortopedi, Jantung,
dan lainnya) tanpa penanda atau peringatan. Padahal deskripsi halaman berbunyi "tentukan DPJP Anak".

**Kenapa terjadi.** Daftar dokter diambil `fetchInpatientDoctorOptions` tanpa saringan spesialisasi
(`use-inpatient-admission-doctor.jsx:410-420`). Aturan konteks bayi di backend
(`InpNewbornAdmissionRules.cs:15-34`) juga tidak memeriksa DPJP.

**Apakah ini menyimpang dari desain?** Tidak, sehingga jenisnya `DESIGN_CHANGE`. Kontrak 022 hanya
menyebut "Rencana perawatan dan DPJP terverifikasi" (baris 41). `plan-repair-016` baris 768 menegaskan
"tidak dianggap ada persetujuan baru terhadap … jumlah DPJP bayi tertentu". Teks "DPJP Anak" hanya ada
pada metadata halaman, bukan dokumen desain.

**Dampak nyata.** Petugas memilih dokter gigi sebagai DPJP bayi NICU karena namanya ada di urutan atas
daftar. Notifikasi visite dan tanggung jawab klinis jatuh ke dokter yang tidak semestinya.

**Rekomendasi.** Pemilik menetapkan aturan DPJP bayi (K-05). Setelah itu, layar dan server
menerapkannya, dan skema tampilan direvisi.

---

### ISS-EPS-040-07 — Klausul persetujuan tercentang sejak awal

| | |
| --- | --- |
| **No. laporan** | ISU-07 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend + dokumen |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-10` (menunggu K-06) |

**Apa yang terjadi.** Saat langkah Persetujuan Wali dibuka, penghitung sudah menunjukkan "12 Dari 12
Disetujui". Wali bisa menandatangani tanpa pernah mencentang satu klausul pun.

**Kenapa terjadi.** `inpatient-admission-consent-form-step.jsx:189-190`:

```javascript
// 2. State 12 Klausul Persetujuan (Default Semua Tercentang)
const [agreedClauses, setAgreedClauses] = useState(ALL_CLAUSE_NUMBERS);
```

**Apakah ini menyimpang dari desain?** Tidak secara tegas, sehingga jenisnya `DESIGN_CHANGE`. Rangka
`05-skema-tampilan.md` bagian 3.12 menggambar butir persetujuan dalam keadaan `[✓]`, dan tidak ada
kalimat yang menyatakan butir harus kosong saat dibuka. Bila pemilik memutuskan butir dimulai kosong,
skema itu wajib direvisi agar source dan desain tidak saling membantah.

**Dampak nyata.** Dalam audit akreditasi, auditor menanyakan bukti bahwa wali benar-benar menyetujui
klausul pelepasan informasi ke penjamin. Sistem hanya bisa menunjukkan bahwa kotak itu tercentang dari
awal.

**Rekomendasi.** Pemilik bersama Komite Etik/Hukum memutuskan K-06.

---

### ISS-EPS-040-08 — Picker tanggal membuka ke bawah lalu terpotong

| | |
| --- | --- |
| **No. laporan** | ISU-08 |
| **Jenis** | `BUG` |
| **Area** | Frontend — base component `FilterDatePicker` (dipakai lintas modul) |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; ukuran DARI-CAPTURE (`19`) |
| **Perbaikan** | `FIX-EPS-040-11` |

**Apa yang terjadi.** Pada layar 1920×945, picker "Tanggal lahir" terbuka ke bawah dan melewati batas
layar. Baris tanggal 25–31 serta tombol "Hari ini" dan "Hapus" tidak bisa diklik sebelum halaman
digulir.

**Kenapa terjadi.** `QuilvianSystemFrontendDev/src/components/features/base-features/filter-date-picker.jsx:25-26, 184-188`:

```javascript
const POPOVER_ESTIMATED_HEIGHT = 360;
const POPOVER_MIN_BOTTOM_SPACE = 240;
...
const placement =
  availableBelow >= POPOVER_ESTIMATED_HEIGHT ||
  availableBelow >= POPOVER_MIN_BOTTOM_SPACE ||
  availableBelow >= availableAbove
    ? "bottom" : "top";
```

Hasil ukur live test: bawah pemicu ±683 px, tinggi layar 945 px, sehingga ruang bawah = 945 − 683 − 16 =
246 px. Nilai ini ≥ 240, jadi popover setinggi ±352 px dibuka ke bawah dan terpotong ±106 px.

**Apakah ini menyimpang dari desain?** `BUG`. `rules/frontend/ui-consistency-checklist.md` mewajibkan
kontrol dapat dipakai pada layar standar. `FilterTimePicker` di form yang sama sudah membalik ke atas
dengan benar.

**Dampak nyata.** Tanggal hari ini adalah pilihan yang paling sering untuk bayi baru lahir, dan justru
itu yang tersembunyi. Petugas mengira tombol "Hari ini" tidak ada.

**Rekomendasi.** Turunkan keputusan arah dari tinggi popover yang sebenarnya, bukan ambang 240 px.
Komponen ini dipakai sekitar 222 berkas, jadi perubahannya harus diuji pada beberapa layar.

---

### ISS-EPS-040-09 — Tempat tidur dan kelas ibu "Belum terbaca"

| | |
| --- | --- |
| **No. laporan** | ISU-09 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-12` |

**Apa yang terjadi.** Kartu "Konfirmasi Penempatan Boks Bayi di Kamar Ibu" menampilkan "Tempat Tidur
Pasien Ibu: Belum terbaca", padahal Ibu A menempati 303-A.

**Kenapa terjadi.** Layar membaca `currentBedCode`, `currentBedName`, dan `currentPatientClassName`
(`inpatient-newborn-admission-steps.jsx:300-302`), sedangkan DTO
`NewbornMotherCandidateResponse` (`.../InPatientManagement/DTOs/InpatientNewbornDtos.cs:38-61`) hanya
punya `CurrentServiceUnit*` dan `CurrentRoom*`. Kelas kamar tampil benar karena layar mengambil
cadangannya dari papan boks.

**Apakah ini menyimpang dari desain?** `CONTRACT_MISMATCH`: perbaikan `FIX-EPS-038-07` menetapkan
keadaan "Belum terbaca" hanya untuk data yang memang tidak tersedia, sedangkan data bed ibu sebenarnya
ada di server.

**Dampak nyata.** Petugas ingin memastikan boks dipesan di kamar ibu yang benar dengan mencocokkan bed
ibu, tetapi informasinya kosong.

**Rekomendasi.** Tambahkan bed dan kelas penempatan ibu ke respons kandidat.

---

### ISS-EPS-040-10 — Status kasus berbahasa Inggris mentah

| | |
| --- | --- |
| **No. laporan** | ISU-10.1 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-13` |

**Apa yang terjadi.** Kartu "Kasus persalinan terpilih" menampilkan "OPR-OBS-…-068 · InProgress" dan
"… · Completed".

**Kenapa terjadi.** `.../InPatientManagement/Services/InpEpisodeService.NewbornCandidates.cs:248`:
`DeliveryCaseStatusName = item.Status.ToString()`.

**Apakah ini menyimpang dari desain?** `RULE_VIOLATION` terhadap daftar periksa "teks Inggris atau pesan
teknis mentah" dan `AGENTS.md` frontend bagian Bahasa.

**Dampak nyata.** Petugas admisi yang tidak terbiasa istilah Inggris tidak yakin apakah kasus
"InProgress" boleh dipakai.

**Rekomendasi.** Kirim label Indonesia ("Sedang berlangsung", "Selesai").

---

### ISS-EPS-040-11 — Pesan Kartu Pasien tidak sesuai status episode

| | |
| --- | --- |
| **No. laporan** | ISU-10.2 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-14` |

**Apa yang terjadi.** Setelah admisi bayi diaktifkan (episode Admitted, bed Terisi), langkah 8 menulis
"Episode … menunggu konfirmasi kedatangan di Papan Tempat Tidur."

**Kenapa terjadi.** `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx:281`
memakai teks tetap milik alur dewasa, yang selesai pada tahap pemesanan. Alur bayi berbeda: aktivasi
langsung mencatat penempatan (`use-inpatient-newborn-admission.js:314`).

**Apakah ini menyimpang dari desain?** `BUG`: kontrak 022 menyatakan bayi sudah aktif setelah langkah 6.

**Dampak nyata.** Petugas membuka Papan Tempat Tidur untuk "mengonfirmasi kedatangan" yang sebenarnya
sudah tercatat, lalu bingung karena tidak ada tombol konfirmasi.

**Rekomendasi.** Teks mengikuti status episode yang terbaca.

---

### ISS-EPS-040-12 — Ringkasan perawatan tidak membedakan Perinatologi dan NICU

| | |
| --- | --- |
| **No. laporan** | ISU-10.3 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-15` |

**Apa yang terjadi.** Langkah 6 menulis "Perawatan: Neonatal terpisah", baik untuk Perinatologi maupun
NICU.

**Kenapa terjadi.** `inpatient-newborn-admission-steps.jsx:662` membaca label dari `NEWBORN_CARE_OPTIONS`
yang hanya punya dua nilai, bukan dari tiga kartu `NEWBORN_CARE_CARD_OPTIONS`.

**Apakah ini menyimpang dari desain?** `BUG` terhadap `issue-031` yang menetapkan tiga jenis perawatan
terlihat oleh petugas.

**Dampak nyata.** Pada tinjauan akhir, petugas tidak bisa memastikan bayi kritis benar-benar dicatat
sebagai NICU sebelum menekan Aktifkan.

**Rekomendasi.** Pakai label kartu: "Rawat Gabung dengan Ibu", "Ruang Perinatologi", "Ruang NICU".

---

### ISS-EPS-040-13 — Bed terisi juga diberi alasan "belum siap atau masih dipegang"

| | |
| --- | --- |
| **No. laporan** | ISU-10.4 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-16` |

**Apa yang terjadi.** NICU-1-A yang ditempati pasien lain menampilkan dua alasan: "Bed belum siap atau
masih dipegang. Muat ulang kesiapan bed." dan "Tempat tidur ini sedang ditempati pasien lain."

**Kenapa terjadi.** `.../InPatientManagement/Services/InpBedOccupancyService.cs:1670-1671` menambahkan
`BED_NOT_READY` setiap kali `!operational.IsBookable`, termasuk saat bed terisi. Alasan terisi kemudian
ditambahkan lagi oleh aturan di bawahnya.

**Apakah ini menyimpang dari desain?** `BUG`: `PLAN-REPAIR-EPS-027` (skema tampilan, baris 1896)
meminta alasan yang tepat untuk bed yang belum layak.

**Dampak nyata.** Petugas menekan "Muat ulang" berkali-kali, menunggu bed yang sebenarnya sedang
dirawati pasien.

**Rekomendasi.** Jangan tambahkan `BED_NOT_READY` bila alasan yang lebih spesifik (terisi atau dipesan)
sudah berlaku.

---

### ISS-EPS-040-14 — Tombol "Pesan Tempat Tidur" mati tanpa penjelasan

| | |
| --- | --- |
| **No. laporan** | ISU-10.5 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-17` |

**Apa yang terjadi.** Setelah "Lanjut ke Pemesanan" pada jalur Perinatologi dan NICU, tombol "Pesan
Tempat Tidur" abu-abu ±6 detik tanpa keterangan.

**Kenapa terjadi.** `use-inpatient-admission-bed.jsx:1161-1166`: `canReserve` mensyaratkan
`episodeId`, dan episode Draft baru dibuat di belakang layar setelah encounter tersimpan. Layar tidak
menampilkan keadaan "menyiapkan admisi".

**Apakah ini menyimpang dari desain?** `RULE_VIOLATION` terhadap prinsip 3 Skill diagnosa dan
`ui-consistency-checklist.md`: jangan mematikan tombol tanpa memberi tahu sebabnya.

**Dampak nyata.** Petugas mengira layar macet, lalu menekan Kembali dan mengulang langkah.

**Rekomendasi.** Tampilkan "Menyiapkan admisi bayi…" selama `preparing`.

---

### ISS-EPS-040-15 — Nama bed tidak konsisten antarlangkah

| | |
| --- | --- |
| **No. laporan** | ISU-10.6 |
| **Jenis** | `BUG`, `DATA` |
| **Area** | Frontend + data master bed |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-18` (frontend); koreksi kode bed lewat pemilik master data |

**Apa yang terjadi.**
- Langkah 4 rawat gabung menulis "303-BOKS", tetapi langkah 6 menulis "BD-RSMMC-BOKS-303".
- Bed BAYI-1-A memakai kode umum "BD-RSMMC-00001", sedangkan bed lain berkode bermakna
  (BD-RSMMC-NICU-02).

**Kenapa terjadi.** `inpatient-newborn-admission-steps.jsx:665`
(`bed.selectedBedLocation || bed.reservation?.bedCode`). Alur rawat gabung tidak mengisi
`selectedBedLocation`, sehingga layar jatuh ke kode teknis. Kode BAYI-1-A adalah isi master data
(`DATA`).

**Apakah ini menyimpang dari desain?** `BUG` untuk tampilan; `DATA` untuk kode bed.

**Dampak nyata.** Petugas mencocokkan tinjauan dengan papan dan menemukan dua nama berbeda untuk boks
yang sama.

**Rekomendasi.** Ringkasan memakai nama kamar dan nama bed yang sama dengan langkah 4. Kode bed
BAYI-1/BAYI-2 diselaraskan oleh pemilik master data.

---

### ISS-EPS-040-16 — Lookup booking operasi dipanggil untuk pasien bayi

| | |
| --- | --- |
| **No. laporan** | ISU-10.7 (bagian pertama) |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-19` |

**Apa yang terjadi.** Setelah identitas bayi tersimpan, layar memanggil
`GET /admissions/surgical-booking-lookup?patientId=<bayi>` dan mendapat 404 di setiap jalur.

**Kenapa terjadi.** `use-inpatient-admission-patient.jsx:690-705` memanggil lookup jadwal IBS
(`FE-RWI-280`) untuk setiap pasien terpilih, tanpa memeriksa apakah pasiennya bayi. Lookup itu bersifat
*fail-open* (gagal tidak mengganggu alur), tetapi tetap tercatat sebagai galat di konsol.

**Apakah ini menyimpang dari desain?** `BUG`: booking operasi tidak relevan untuk pasien bayi baru
lahir.

**Dampak nyata.** Galat di konsol menutupi galat yang sungguhan saat tim mendiagnosis masalah lain.

**Rekomendasi.** Lewati lookup bila pasien adalah bayi baru lahir.

---

### ISS-EPS-040-17 — Data calon penanda tangan ditolak 409 selama admisi

| | |
| --- | --- |
| **No. laporan** | ISU-10.7 (bagian kedua) |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend + Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-20` |

**Apa yang terjadi.** Saat langkah Persetujuan Wali dibuka, layar memanggil
`GET …/admission-workspace/general-consent/print-data` dan selalu ditolak `409` dengan pesan "Admisi
belum dikonfirmasi".

**Kenapa terjadi.**

```text
409 INP-ADM-DOC-002 pada langkah 5
  ← sebab langsung: consent-form-step.jsx:77 meminta print-data untuk mengambil calon penanda tangan
    ← akar: guard backend menolak bacaan dokumen untuk episode Draft (InpAdmissionWriteGuard.cs:51),
       padahal langkah Persetujuan Wali memang berjalan saat episode masih Draft
```

**Apakah ini menyimpang dari desain?** `CONTRACT_MISMATCH`. Skema 3.12 menempatkan Persetujuan sebelum
aktivasi, sedangkan endpoint yang dipakai hanya melayani episode yang sudah Admitted. Akibat lanjutannya
dicatat sebagai `ISS-EPS-040-T1`.

**Dampak nyata.** Lihat T1: nama ibu tidak pernah terisi otomatis.

**Rekomendasi.** Ambil identitas ibu dari konteks admisi bayi yang sudah dimuat layar, dan jangan
memanggil print-data selama episode Draft.

---

### ISS-EPS-040-18 — Nama bayi menerima tag HTML

| | |
| --- | --- |
| **No. laporan** | ISU-10.8 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend + Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI; request DARI-CAPTURE (`21`) |
| **Perbaikan** | `FIX-EPS-040-21` |

**Apa yang terjadi.** Nama bayi `<img src=x onerror="alert(1)">Bayi` diterima layar dan dikirim ke
server. Skrip tidak dieksekusi di layar, karena React menampilkannya sebagai teks.

**Kenapa terjadi.** Layar hanya membatasi `maxLength: 100` (`inpatient-newborn-admission-steps.jsx:189-193`).
Backend hanya `MaxLength(100)` (`PatientDtos.cs:495-496`) dan `Trim()`.

**Apakah ini menyimpang dari desain?** `RULE_VIOLATION` terhadap kebiasaan sanitasi teks pada
`rules/testing/` (sanitasi teks, keamanan injeksi skrip).

**Dampak nyata.** Nama berisi tag tercetak di kartu, label, dan dokumen PDF. Bila suatu template cetak
tidak meng-escape nilai, skrip bisa berjalan.

**Rekomendasi.** Batasi karakter nama ke huruf, spasi, titik, koma, apostrof, dan tanda hubung.

---

### ISS-EPS-040-19 — NIK bayi menerima huruf saat diketik

| | |
| --- | --- |
| **No. laporan** | ISU-10.9 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | DARI-CAPTURE (`17`) |
| **Perbaikan** | `FIX-EPS-040-21` |

**Apa yang terjadi.** Kolom "NIK bayi, bila tersedia" menerima ketikan `32abcd…`. Isian baru ditolak
saat Simpan.

**Kenapa terjadi.** Kolom memakai `inputMode: "numeric"` (hanya petunjuk keyboard di ponsel) tanpa
penyaring karakter (`inpatient-newborn-admission-steps.jsx:202`). Kolom No. HP wali di langkah 5 sudah
menyaring huruf (`consent-form-step.jsx:124, 157`), sehingga polanya bisa dipakai ulang.

**Apakah ini menyimpang dari desain?** `RULE_VIOLATION` terhadap prinsip 4: validasi layar mencerminkan
aturan server.

**Dampak nyata.** Petugas baru tahu kesalahannya setelah menekan Simpan.

**Rekomendasi.** Saring karakter non-angka saat diketik.

---

### ISS-EPS-040-20 — Foto profil diblokir Content Security Policy

| | |
| --- | --- |
| **No. laporan** | ISU-10.10 |
| **Jenis** | `BUG` |
| **Area** | Frontend — konfigurasi platform (`QuilvianSystemFrontendDev/next.config.js`, directive `img-src`) |
| **Keparahan** | Low |
| **Status bukti** | DARI-CAPTURE (log konsol) |
| **Perbaikan** | Di luar modul — diteruskan ke pemilik konfigurasi platform frontend |

**Apa yang terjadi.** Foto profil pengguna dari `api-dev.quilvian-mmchospital.com` diblokir karena
`img-src` hanya mengizinkan `'self' data: blob: https://localhost:7184 http://103.153.60.136:5050`.
Konsol terisi galat berulang di setiap halaman.

**Kenapa terjadi.** Domain aset API dev belum masuk daftar CSP. Konfigurasi ini milik platform, bukan
modul Rawat Inap.

**Apakah ini menyimpang dari desain?** Tidak ada dokumen desain modul ini yang mengaturnya.

**Dampak nyata.** Galat konsol berulang mempersulit diagnosis, dan foto profil tidak tampil.

**Rekomendasi.** Diteruskan ke pemilik konfigurasi platform. Tidak dimasukkan ke rencana modul ini.

---

## 5. Tanya-jawab pelapor

Laporan live testing tidak memuat pertanyaan eksplisit. Satu pertanyaan tersirat dijawab:

> **T:** Apakah 409 dari `general-consent/print-data` dan 404 dari `surgical-booking-lookup` hanya
> gangguan kecil yang boleh diabaikan?
>
> **J:** Tidak keduanya. 404 lookup booking memang hanya panggilan yang tidak perlu untuk bayi
> (`ISS-EPS-040-16`). Sebaliknya, 409 print-data adalah sebab langsung nama ibu tidak terisi otomatis
> pada Persetujuan Wali (`ISS-EPS-040-17`, `ISS-EPS-040-T1`). Layar tetap berjalan karena galatnya
> ditelan diam-diam (`consent-form-step.jsx:85`), tetapi fungsi yang dirancang skema 3.12 ikut hilang.

---

## 6. Temuan tambahan

### ISS-EPS-040-T1 — Nama ibu/wali tidak terisi otomatis pada Persetujuan Wali

| | |
| --- | --- |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend + Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE (`29`: kolom nama kosong) |
| **Perbaikan** | `FIX-EPS-040-20` |
| **Kenapa dicantumkan** | Terlihat pada lampiran 29 tetapi tidak dikeluhkan; akar masalahnya sama dengan `ISS-EPS-040-17` |

**Apa yang terjadi.** Pada ketiga jalur, kolom "Nama Lengkap Penanda Tangan" kosong. Petugas harus
mengetik ulang nama ibu yang sudah diketahui sistem sejak langkah 1.

**Kenapa terjadi.** `consent-form-step.jsx:134-167` mengisi data wali dari `candidates`. Daftar itu
berasal dari print-data yang selalu ditolak 409 selama Draft (`ISS-EPS-040-17`), sehingga cabang "tidak
ada kandidat" mengosongkan semua isian. Identitas ibu sebenarnya sudah dimuat
`use-inpatient-newborn-admission.js:156-171` (`newborn.mother`: nama, NIK, telepon, alamat).

**Apakah ini menyimpang dari desain?** Ya. Skema 3.12 aturan 1 (baris 702): "Sistem secara otomatis
mengaktifkan opsi Penanggung Jawab/Keluarga dan **mengisi nama ibu / wali dari data admisi**."

**Dampak nyata.** Petugas mengetik ulang nama, dan salah ketik pada dokumen hukum bisa terjadi
(misalnya "Rahmawaty" ditulis untuk "Rahmawati").

**Rekomendasi.** Isi otomatis dari `newborn.mother` saat penanda tangan "Orang Tua".

---

### ISS-EPS-040-T2 — Nomor episode dan encounter memakai jam UTC

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE (episode admisi pukul 11.15 WIB bernomor `RI-261025041519-…`) |
| **Perbaikan** | `FIX-EPS-040-22` |
| **Kenapa dicantumkan** | Terlihat pada nomor episode di lampiran `36`, `56`, `78` tetapi tidak dikeluhkan; akar masalahnya serupa `ISS-EPS-040-03` |

**Apa yang terjadi.** Nomor episode bayi yang diaktifkan pukul 11.15 WIB berbunyi `RI-261025041519-…`,
yaitu jam 04.15 UTC. Admisi pukul 00.00–06.59 WIB akan bernomor dengan tanggal kemarin.

**Kenapa terjadi.** `.../InPatientManagement/Services/InpEpisodeNumberService.cs:47`:
`Generate(prefix, DateTime.UtcNow)`. Pola yang sama dipakai nomor encounter darurat di
`InpEpisodeService.cs:1613`.

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ada keputusan yang meminta nomor berbasis UTC, dan
petugas membaca nomor itu sebagai waktu lokal.

**Dampak nyata.** Petugas rekam medis mencari episode "26 Oktober dini hari" dengan awalan `RI-261026`,
tetapi nomornya berawalan `RI-261025`.

**Rekomendasi.** Bentuk nomor dari waktu rumah sakit (WIB). Radius dampaknya seluruh admisi rawat inap.

---

### ISS-EPS-040-T3 — Tag Swagger persetujuan tidak mengikuti pola modul

| | |
| --- | --- |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-040-23` |
| **Kenapa dicantumkan** | Ditemukan saat menyusun tabel endpoint laporan live testing |

**Apa yang terjadi.** Di halaman Swagger, endpoint persetujuan muncul di grup `InpatientAdmissionConsent`,
terpisah dari grup "Health Services / Inpatient Management / …".

**Kenapa terjadi.** `.../InPatientManagement/Controllers/InpatientAdmissionConsentController.cs` memakai
`[Tags("InpatientAdmissionConsent")]`, sedangkan controller lain memakai pola
`[Tags("Health Services / Inpatient Management / <Nama>")]`.

**Apakah ini menyimpang dari desain?** `RULE_VIOLATION` terhadap aturan output 5.1 dan standar endpoint
`rules/backend/`.

**Dampak nyata.** Tim QA dan integrator tidak menemukan endpoint persetujuan di grup Rawat Inap.

**Rekomendasi.** Ganti menjadi `Health Services / Inpatient Management / Inpatient Admission Consent`.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Mohon jalankan query baca-saja ini dan lampirkan hasilnya: `SELECT "RoomName", "IsForNewborn", "IsIntensiveCare" FROM "MstRoom" WHERE "RoomName" IN ('R. Bayi','Ruang NICU','VIP 301','VIP 302','VIP 303','VIP 304','VIP 305');` | Aturan `FIX-EPS-040-03` bergantung pada penanda kamar bayi. Bila R. Bayi belum bertanda `IsForNewborn`, aturan baru justru akan menolak bed yang benar. | `FIX-EPS-040-03` tetap bisa dikerjakan, tetapi butuh langkah koreksi master data lebih dulu bila penandanya kosong |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Format gelang bayi | (a) "BY. NY. [IBU]" saja / (b) nama resmi bayi + baris "IBU: [NAMA IBU]" / (c) nama resmi bayi saja | (b): baris 1 sama dengan label ("BAGAS PRATAMA / BAYI 1"; bila belum bernama, "BY. NY. SITI AMINAH / BAYI 1"), baris 2 "IBU: SITI AMINAH". Memenuhi MTG-BAYI-04 dan menutup K-02 issue-016 bagian format gelang | `FIX-EPS-040-02` |
| K-02 | Bolehkah bayi non-intensif memakai bed NICU? | Tidak pernah / boleh sebagai limpahan dengan alasan tercatat / boleh bebas | Tidak, di alur admisi. Limpahan, bila dibutuhkan, dibuat sebagai fitur terpisah yang mencatat alasan dan penyetuju | `FIX-EPS-040-04` |
| K-03 | Batas bawah tanggal lahir bayi pada admisi | Tidak dibatasi / tidak lebih awal dari tanggal kasus persalinan diminta dikurangi 1 hari / maksimal 28 hari ke belakang | Tidak lebih awal dari tanggal kasus persalinan diminta dikurangi 1 hari. Contoh: kasus diminta 23-10-2026, maka tanggal lahir paling awal 22-10-2026 | `FIX-EPS-040-07` |
| K-04 | Jenis identitas wali yang diterima | NIK saja / NIK atau paspor | NIK 16 digit **atau** paspor 6–9 karakter huruf/angka, sesuai label layar | Tidak — `FIX-EPS-040-08` memakai rekomendasi sampai diputuskan lain |
| K-05 | Siapa yang boleh menjadi DPJP bayi | Semua dokter / dokter Anak saja / dokter Anak diprioritaskan, dokter lain wajib alasan | Dokter Anak tampil teratas dan diberi penanda; memilih spesialisasi lain wajib mengisi alasan | `FIX-EPS-040-09` |
| K-06 | Keadaan awal 12 klausul persetujuan | Tercentang (sekarang) / kosong, dicentang satu per satu / kosong + satu tombol "Saya telah membaca dan menyetujui seluruh klausul" | Kosong + satu tombol setuju-semua setelah klausul ditampilkan | `FIX-EPS-040-10` |
| K-07 | Data uji live testing (3 bayi Admitted, bed 303-BOKS, BAYI-1-A, NICU-1-B Terisi) | Dibiarkan / dipulangkan dan ditutup lewat alur berwenang | Ditutup lewat alur pulang berwenang sebelum uji ulang, karena Ruang NICU (2 bed) kini penuh | Tidak menahan perbaikan; menahan uji ulang jalur NICU |

---

## 8. Catatan pola

1. **Aturan zona waktu berulang.** `ISS-EPS-040-03` dan `ISS-EPS-040-T2` sama-sama memakai
   `DateTime.UtcNow` untuk hal yang dibaca petugas sebagai tanggal lokal. Repository sudah punya dua
   pola tanggal lokal (`InpAdmissionText.LocalToday` di Rawat Inap dan `FinanceBusinessDate` di
   Finance). **Usulan pencegahan:** aturan backend global "tanggal bisnis dan nomor dokumen yang dibaca
   manusia dibentuk dari zona waktu rumah sakit", beserta satu helper bersama.
2. **Dua jalur penyusun nama untuk satu pasien.** `ISS-EPS-040-01` terjadi karena gelang dan label
   punya penyusun nama sendiri-sendiri. **Usulan pencegahan:** satu fungsi penyusun nama tampilan pasien
   bayi, dipakai gelang, label, dan kartu.
3. **Validasi berhenti pada format.** `ISS-EPS-040-04`, `-05`, `-18`, dan `-19` memeriksa bentuk isian,
   bukan masuk akal tidaknya. Polanya sama dengan temuan validasi pada `issue-003`.
4. **Bukti untuk `PLAN-REPAIR-EPS-038`.** Live testing 25-10-2026 menjalankan skenario yang relevan
   dengan uji layar S1–S10 plan repair 038:
   - "Lanjut ke Pemesanan" sekali klik (jalur Perinatologi dan NICU);
   - klik ganda "Pesan Boks Bayi & Lanjut" hanya menghasilkan satu encounter, satu episode, dan satu
     reservasi;
   - boks rawat gabung terbaca dari kamar ibu (lampiran `26`–`28`, `48`–`53`, `73`–`75`).

   Register 038 **tidak** diubah oleh issue ini. Pemilik atau builder 038 dapat memakai bukti tersebut
   saat memperbarui registernya.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-25 | Issue dibuat dari laporan live testing (10 isu; ISU-10 dipecah 10 catatan) menjadi 20 butir + 3 temuan tambahan | `diagnose-module-issue` |
| 2026-10-25 | Pemilik memutuskan K-01–K-06 = rekomendasi dan menyetujui seluruh rencana. Status → `DALAM_PERBAIKAN`; source 23 perbaikan selesai (`FIX-EPS-040-BE`, `FIX-EPS-040-FE`), build dan uji layar belum. K-07 dan P-01 masih terbuka | `build-module-backend`, `build-module-frontend` |
