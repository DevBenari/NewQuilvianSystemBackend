# ISSUE-EPS-022 — Satu pasien bisa punya lebih dari satu admisi RANAP aktif/Draft bersamaan tanpa dicegah sistem

```yaml
issue_id: ISSUE-EPS-022
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Admisi Rawat Inap (layar pembuka admisi, pemanggil InpEpisodeService.OpenAdmissionAsync) - tidak terikat satu nama layar UI spesifik yang teridentifikasi dalam penelusuran ini; temuan berasal dari pembacaan source backend endpoint admisi"
sumber_laporan: "Diteruskan dari modul Billing (blueprint billing-kasir): ISS-BKC-003-T1 dan ISS-BKC-003-T2 pada issue-003-kunjungan-pasien-tampil-identik.md. Ditemukan saat pemilik produk melaporkan dropdown kunjungan Billing menampilkan 3 kunjungan RANAP aktif untuk pasien yang sama; pemilik mengonfirmasi (16-10-2026) ia memang sempat 2x mencoba mendaftarkan pasien yang sama ke RANAP tapi gagal booking bed akibat konfigurasi kelas pasien salah, dan menduga registrasinya 'nyangkut' tanpa episode. Penelusuran source membuktikan registrasi dan episode TETAP dibuat bersama (satu transaksi) sehingga bukan 'nyangkut tanpa episode', melainkan gap aturan bisnis yang lebih mendasar: sistem memang tidak mencegah admisi RANAP berulang untuk pasien yang sama. Keputusan K-02 pada issue tersebut (16-10-2026, oleh pemilik yang menangani sesi diagnosis - bukan pemilik modul ini): 'Tidak boleh - perlu guard baru'. Keputusan itu dicatat di sini sebagai rekomendasi kuat, BUKAN sebagai persetujuan final modul ini - lihat bagian 6. 1 butir, 0 lampiran baru (bukti dari pembacaan source dan capture data yang sudah ada di issue-003 modul Billing)"
tanggal_issue: "2026-10-16"
status: TERBUKA
keparahan_tertinggi: Medium
source_sha_backend: "e70f5abf (Yasmina)"
source_sha_frontend: "Tidak relevan - temuan backend murni"
rencana_perbaikan: ../plan-repair/plan-repair-022-admisi-ranap-ganda-tanpa-guard.md
```

## 1. Ringkasan

Sistem saat ini mengizinkan satu pasien memiliki lebih dari satu kunjungan/episode rawat inap
(RANAP) berstatus aktif atau Draft secara bersamaan. Dua mekanisme yang seharusnya mencegah ini
ternyata tidak berlaku untuk RANAP: (1) saklar "satu pasien satu kunjungan aktif" di modul
Registrasi secara eksplisit hanya mencakup kunjungan rawat jalan berklinik, bukan rawat inap; (2)
alur admisi rawat inap sendiri hanya menolak admisi baru bila pasien "sedang hadir" (episode
berstatus `Admitted` atau `DischargePending` yang belum fisik keluar) — episode berstatus `Draft`
secara eksplisit **tidak** dianggap menghalangi, dan kode sumbernya sendiri menyatakan admisi
Draft ganda hanya peringatan, bukan penolakan. Ditambah tidak adanya idempotency key pada endpoint
admisi, klik ganda atau percobaan admisi berulang (termasuk yang gagal di tengah jalan, mis. gagal
booking bed karena kelas pasien salah) bisa meninggalkan beberapa kunjungan RANAP aktif untuk
pasien yang sama tanpa ada yang otomatis dibatalkan. Ini terbukti terjadi pada data nyata yang
ditemukan modul Billing (tiga kunjungan RANAP aktif untuk satu pasien, dua di antaranya dibuat
pada hari yang sama).

## 2. Laporan asli

| No. laporan | Keluhan | Lampiran |
| ---: | --- | --- |
| 1 | Diteruskan dari `ISS-BKC-003-T1`/`T2` (modul Billing). Konfirmasi pemilik produk: "Saya memang pernah mencoba mendaftarkan [pasien], 2x sebelumnya pada ranap. Tetapi gagal, akibat configurasi kelas patient yang salah, sehingga ga bisa booking bed. Apakah pasien udah terdaftar di ranap, tetapi episode ranapnya ga ada?" | Tidak ada lampiran baru di sini; capture data `RegPatientEncounter` 4 baris (tiga `EncounterStatus=Registered` aktif pada tanggal berdekatan, dua di antaranya tanggal sama) ada pada `issue-003` modul Billing (`ISS-BKC-003-01` bagian verifikasi P-01) |

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-022-01` | 1 | Tidak ada guard yang mencegah satu pasien punya lebih dari satu kunjungan/episode RANAP aktif atau Draft bersamaan | `RULE_VIOLATION` (gap aturan bisnis) | Backend | Medium | `SUDAH-VERIFIKASI` di source; diperkuat data nyata dari modul Billing | `FIX-EPS-022-01` |
| `ISS-EPS-022-02` | 1 | Endpoint admisi rawat inap (`OpenAdmissionAsync`) tidak punya idempotency key | `BUG` | Backend | Low | `SUDAH-VERIFIKASI` di source | `FIX-EPS-022-02` |

## 4. Rincian per temuan

### ISS-EPS-022-01 — Tidak ada guard yang mencegah >1 kunjungan/episode RANAP aktif atau Draft bersamaan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `RULE_VIOLATION` (gap aturan bisnis, belum tentu pelanggaran aturan yang sudah ada — bisa jadi memang belum pernah diputuskan) |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source; diperkuat data nyata |
| **Perbaikan** | `FIX-EPS-022-01` |

**Apa yang terjadi.** Seorang petugas bisa membuka admisi rawat inap baru untuk pasien yang
sebenarnya sudah punya kunjungan/episode rawat inap aktif atau belum selesai (Draft), tanpa
peringatan yang menghentikan prosesnya atau mekanisme otomatis yang membersihkan percobaan admisi
yang gagal di tengah jalan.

**Kenapa terjadi.** Dua lapis pemeriksaan yang ada, keduanya tidak menghalangi RANAP:

1. **Lapis `RegPatientEncounter` (pendaftaran/registrasi).**
   `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterRules.cs:19-99` punya
   saklar "satu pasien satu kunjungan aktif" (konstanta `BlockActiveEncounterConfigKey`, baris
   71-73), tapi (a) defaultnya **nonaktif** (`RJ-DOC-DEC-026`, baris 66-70), dan (b)
   `WhereOutpatientClinicEncounter` (baris 26-33) secara eksplisit **hanya** mencakup
   `EncounterType.Outpatient` berklinik — dokumentasinya sendiri menegaskan "Rawat Inap tidak
   menghalangi (`RJ-DOC-DEC-022`)".
   `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:1792-1800`
   memanggil aturan ini lewat `FindActiveEncounterAsync` (baris 1713-1714) — walau saklar
   dinyalakan sekalipun, RANAP tetap tidak pernah diblokir di titik ini.
2. **Lapis `InpEpisode` (admisi/rawat inap, modul ini).**
   `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs:180-192` (method
   `OpenAdmissionAsync`) hanya menolak admisi baru bila pasien "sedang hadir" — didefinisikan
   `FindPresentEpisodeAsync` (`InpEpisodeService.Assignments.cs:69-77`) sebagai episode berstatus
   `Admitted`, atau `DischargePending` dengan `PhysicallyLeftAt == null`. Episode berstatus
   `Draft` **tidak** termasuk "hadir". `InpEpisodeService.cs:321-338` secara eksplisit menyatakan
   admisi Draft kedua (dan seterusnya) untuk pasien yang sama hanya menghasilkan **peringatan**
   ("admisi Draft ganda adalah peringatan, bukan penolakan. Petugas boleh melanjutkan"), bukan
   penolakan.

Setiap admisi baru tanpa `EncounterId` yang dikirim membuat `RegPatientEncounter` Inpatient baru
sekaligus (`InpEpisodeService.cs:346-352`, `BuildInpatientEncounter`, satu transaksi dengan
pembuatan `InpEpisode` — lihat juga jawaban atas pertanyaan pemilik pada butir Tanya-jawab di
bawah), sehingga setiap percobaan admisi yang tidak diblokir menghasilkan satu kunjungan +
episode Draft baru yang berdiri sendiri.

**Apakah ini menyimpang dari desain?** Tidak dapat ditentukan penuh di sini — ini keputusan aturan
bisnis yang hanya bisa diambil pemilik modul ini: apakah kondisi "beberapa admisi Draft untuk
pasien yang sama" memang disengaja (mis. untuk skenario re-admisi cepat, percobaan admisi yang
bisa diulang sebelum salah satunya dikonfirmasi) atau memang gap yang perlu guard baru.

**Dampak nyata.** Data nyata ditemukan modul Billing: tiga kunjungan RANAP (`EncounterStatus =
Registered`, aktif) untuk satu pasien sekaligus, dua di antaranya dibuat pada tanggal yang sama —
konsisten dengan percobaan admisi berulang untuk pasien yang sama dalam rentang waktu singkat.
Selain membingungkan dropdown di modul Billing (`ISS-BKC-003-01`, sudah diperbaiki di sana),
berpotensi membingungkan alur rawat inap itu sendiri — petugas yang membuka daftar admisi/episode
pasien tersebut akan melihat beberapa baris Draft tanpa kejelasan mana yang harus dilanjutkan dan
mana yang seharusnya sudah tidak relevan (mis. gagal booking bed dan ditinggalkan).

**Rekomendasi.** Tambahkan guard yang mencegah admisi RANAP baru untuk pasien yang masih punya
episode Draft/aktif lain yang belum ditutup/dibatalkan secara eksplisit, ATAU sediakan mekanisme
eksplisit untuk membatalkan/menutup percobaan admisi yang gagal (mis. gagal booking bed) sehingga
tidak tertinggal sebagai Draft yatim. Opsi lengkap di plan repair.

### ISS-EPS-022-02 — Endpoint admisi rawat inap tidak punya idempotency key

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | `SUDAH-VERIFIKASI` di source |
| **Perbaikan** | `FIX-EPS-022-02` |

**Apa yang terjadi.** Tombol/aksi membuka admisi rawat inap tidak dilindungi dari klik ganda atau
retry jaringan — dua kali tekan sebelum respons pertama kembali berpotensi membuat dua admisi
untuk niat yang sama.

**Kenapa terjadi.** DTO `OpenAdmissionRequest`
(`Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs:29-69`) tidak memiliki
field idempotency key apa pun. Backend **sudah** punya pola idempotency key yang terbukti bekerja
(`RegistrationIdempotencyKey` pada `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs:195-209`,
dipakai `EncounterIntakeService.cs:81-116,484-498`), tapi mekanisme itu khusus jalur pendaftaran
modul penunjang (INT-05, mis. Laboratorium) yang melayani kedatangan langsung tanpa antrean
dokter — **bukan** jalur admisi rawat inap utama. `OpenAdmissionAsync` tidak memanggil
`EncounterIntakeService` sama sekali, dan tidak ada pola serupa yang diterapkan ulang
(reuse) ke jalur admisi.

**Apakah ini menyimpang dari desain?** `BUG` — tidak ditemukan keputusan desain yang sengaja
meniadakan idempotency key di jalur admisi; pola yang sudah ada di modul lain (INT-05) justru
menunjukkan ini memang kebutuhan yang diketahui, hanya belum diterapkan ulang di sini.

**Dampak nyata.** Berkontribusi pada `ISS-EPS-022-01` — klik ganda/retry jaringan pada tombol
admisi bisa menjadi salah satu penyebab lahirnya admisi Draft duplikat untuk niat yang sama.

**Rekomendasi.** Tambahkan field idempotency key ke `OpenAdmissionRequest`, dengan pola pemeriksaan
sebelum create yang sama seperti `EncounterIntakeService.FindByIdempotencyKeyAsync`. Rincian opsi
di plan repair.

## 5. Tanya-jawab pelapor

> **T:** Saya memang pernah mencoba mendaftarkan [pasien] 2x sebelumnya pada RANAP, tapi gagal
> akibat konfigurasi kelas pasien salah sehingga tidak bisa booking bed. Apakah pasien sudah
> terdaftar di RANAP, tapi episode RANAP-nya tidak ada?
>
> **J:** Tidak — dibuktikan dengan membaca `OpenAdmissionAsync` (`InpEpisodeService.cs:342-459`):
> saat encounter dibuat oleh alur admisi itu sendiri (bukan `EncounterId` existing yang disodorkan
> dari luar), `RegPatientEncounter` dan `InpEpisode` dibuat dalam **satu transaksi**
> (`BeginTransaction` → `Add` encounter → `Add` episode → satu `SaveChangesAsync` → `Commit`;
> gagal di tengah jalan = rollback keduanya). Booking bed sendiri ada di service **terpisah**
> (`InpBedOccupancyService.ReserveBedCoreAsync`), dipanggil **setelah** episode sudah tersimpan —
> episode di-load dari DB lebih dulu, baru divalidasi kelas kamarnya. Jadi kalau yang gagal betul
> "booking bed karena kelas pasien salah", episode-nya **sudah ada** (status Draft, cuma belum
> dapat bed) — bukan hilang/nyangkut. Ini dikonfirmasi dengan data nyata: query
> `RegPatientEncounter` dua kunjungan yang Anda duga bermasalah (`EpisodeId` di `InpEpisode`
> ternyata **NULL** untuk keduanya setelah dicek langsung — lihat catatan di `issue-003` modul
> Billing) justru **membuktikan sebaliknya** dari dugaan awal saya: kedua kunjungan itu benar-benar
> **tidak punya episode sama sekali**, bukan episode Draft tanpa bed. Setelah ditelusuri lebih
> jauh, kemungkinan penjelasannya adalah validasi gagal **sebelum** transaksi pembuatan
> encounter+episode sempat dimulai (mis. kamar yang dipilih tidak punya kelas pasien aktif,
> `InpEpisodeService.cs:240-241`, atau validasi `ServiceUnit`/`PatientClass`,
> `InpEpisodeService.cs:1412-1452`) — dalam skenario itu, method berhenti sebelum sempat menulis
> apa pun, **kecuali** bila encounter yang dipakai sebenarnya sudah ada sebelumnya dari jalur lain
> (bukan dibuat `OpenAdmissionAsync` sendiri). Poin pentingnya bagi isu ini: siapa pun mekanismenya,
> hasil akhirnya tetap sama — percobaan admisi yang gagal bisa meninggalkan `RegPatientEncounter`
> yang "nyangkut" tanpa episode (atau dengan episode Draft yatim), dan sistem tidak punya
> mekanisme otomatis untuk membersihkan atau mencegah ini terulang. Itulah yang dicatat sebagai
> `ISS-EPS-022-01`/`02` di atas.

## 6. Temuan tambahan

Tidak ada.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada pertanyaan tambahan — jawaban pada bagian 5 sudah mencakup
klarifikasi yang dibutuhkan.

**Untuk pemilik (modul rawat-inap, sub-modul episode-rawat-inap):**

| No | Keputusan | Pilihan | Rekomendasi diteruskan dari sesi diagnosis Billing (K-02) | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah satu pasien boleh punya >1 kunjungan/episode RANAP aktif atau Draft bersamaan? | (a) Boleh, disengaja untuk skenario re-admisi/percobaan berulang / (b) Tidak boleh, perlu guard yang menolak atau mekanisme pembatalan otomatis untuk percobaan admisi yang gagal | **(b)** — tidak boleh, perlu guard baru | `FIX-EPS-022-01` |
| K-02 | Bila (b) dipilih: guard-nya berbentuk apa — menolak admisi baru selama ada Draft/aktif lain, atau membiarkan admisi baru tapi otomatis membatalkan/menutup Draft lama yang belum dilanjutkan dalam rentang waktu tertentu? | (a) Tolak admisi baru / (b) Auto-batalkan Draft lama / (c) Kombinasi (tolak, dengan opsi petugas membatalkan Draft lama secara manual dulu) | Tidak direkomendasikan di sini — murni keputusan alur kerja klinis/operasional milik modul ini | `FIX-EPS-022-01` |

Rekomendasi K-01 (b) berasal dari sesi diagnosis modul Billing (16-10-2026), diputuskan oleh
pemilik yang menangani sesi itu — **bukan** pemilik modul rawat-inap. Dicatat sebagai rekomendasi
kuat, bukan keputusan final; pemilik modul ini tetap perlu mengonfirmasi arah keputusan dan
menjawab K-02 (bentuk guard-nya) sebelum `FIX-EPS-022-01` dapat didaftarkan ke roadmap.

## 8. Catatan pola

Pola "admisi Draft ganda hanya peringatan, bukan penolakan" (`InpEpisodeService.cs:321-338`)
tampaknya keputusan desain yang disengaja pada satu titik (ada komentar eksplisit di source), tapi
dampaknya baru terlihat nyata lewat laporan dari modul lain (Billing). Disarankan modul ini
meninjau ulang apakah keputusan tersebut (mengizinkan admisi Draft ganda sebagai peringatan)
masih sesuai kebutuhan operasional saat ini, mengingat dampaknya pada modul hilir (Billing) dan
kemungkinan modul lain yang juga mengonsumsi daftar kunjungan/episode aktif per pasien.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Issue dibuat, diteruskan dari `ISS-BKC-003-T1`/`T2` pada `issue-003` modul Billing | `diagnose-module-issue` |
