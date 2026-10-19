# PLAN-REPAIR-022 — Cegah admisi RANAP ganda dan tambahkan idempotency key

```yaml
plan_id: PLAN-REPAIR-EPS-022
issue: ../issue/issue-022-admisi-ranap-ganda-tanpa-guard.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-10-16"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "e70f5abf (Yasmina)"
basis_source_frontend: "Tidak relevan - temuan backend murni"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-022-01` | `ISS-EPS-022-01` | Guard mencegah/menangani admisi RANAP ganda untuk pasien yang sama | BE | 1 | — | ⛔ TERBLOKIR — menunggu K-01 dan K-02 | — |
| `FIX-EPS-022-02` | `ISS-EPS-022-02` | Tambahkan idempotency key ke `OpenAdmissionRequest` | BE | 2 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 0 dari 2 perbaikan selesai.** `FIX-EPS-022-01` menunggu keputusan bentuk guard dari
pemilik modul (K-01, K-02); `FIX-EPS-022-02` tidak tertahan keputusan apa pun dan bisa dikerjakan
lebih dulu secara independen bila pemilik modul menyetujui rencana ini.

## 2. Solusi terpilih per temuan

### ISS-EPS-022-01 — Tidak ada guard yang mencegah >1 kunjungan/episode RANAP aktif atau Draft bersamaan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tolak admisi baru (`OpenAdmissionAsync`) bila pasien masih punya episode `Draft` lain yang belum ditutup/dibatalkan untuk `ServiceUnit`/`PatientClass` yang sama, mirip pola "sedang hadir" yang sudah ada untuk status `Admitted`/`DischargePending` tapi diperluas mencakup `Draft` | Konsisten dengan pola guard yang sudah ada (`FindPresentEpisodeAsync`); perubahan terlokalisasi di satu titik validasi | Petugas yang memang perlu mengulang admisi setelah gagal booking bed (skenario nyata pelapor) akan terblokir sampai Draft lama dibatalkan secara eksplisit — butuh UX/alur pembatalan yang jelas, bukan sekadar penolakan mentah |
| B | Biarkan admisi baru boleh dibuat, tapi tambahkan mekanisme auto-expire/auto-batal untuk episode `Draft` yang tidak dilanjutkan dalam rentang waktu tertentu (mis. job terjadwal atau pengecekan saat admisi baru dibuka) | Tidak memblokir petugas yang sedang mencoba ulang; "sampah" Draft lama otomatis bersih | Butuh keputusan bisnis tambahan (berapa lama rentang waktunya, siapa yang berwenang menimpa), kompleksitas lebih tinggi, berpotensi membatalkan Draft yang sebenarnya masih diproses petugas lain secara sah |
| C | Kombinasi: izinkan admisi baru, tapi WAJIBkan konfirmasi eksplisit dari petugas ("Pasien ini masih punya admisi Draft lain, lanjutkan / batalkan yang lama dulu?") sebelum admisi baru benar-benar dibuat — mirip pola "peringatan" yang sudah ada tapi dengan keputusan sadar, bukan hanya `warning` pasif di response | Memberi petugas kendali dan visibilitas penuh tanpa auto-expire yang berisiko salah menutup data aktif; tidak kehilangan skenario legit (re-admisi setelah gagal booking) | Butuh perubahan kontrak API (field konfirmasi) dan kemungkinan perubahan UI frontend — radius dampak lebih luas dari opsi A/B murni backend |

**Belum ada solusi terpilih** — ketiga opsi di atas punya trade-off alur kerja klinis/operasional
yang hanya bisa diputuskan pemilik modul ini (K-01, K-02 pada bagian 6). Opsi A paling dekat
dengan pola yang sudah ada di source (perluasan `FindPresentEpisodeAsync`), dan karena itu
ditandai sebagai **kandidat default** bila pemilik memilih arah "tolak admisi baru" pada K-02 —
tapi tidak ditetapkan sebagai solusi final sebelum K-01/K-02 dijawab, agar tidak mengarang
keputusan alur kerja yang bukan wewenang skill ini.

### ISS-EPS-022-02 — Endpoint admisi rawat inap tidak punya idempotency key

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambahkan field idempotency key opsional ke `OpenAdmissionRequest`, dengan pemeriksaan sebelum create meniru pola `EncounterIntakeService.FindByIdempotencyKeyAsync`/`RegistrationIdempotencyKey` yang sudah terbukti bekerja di jalur INT-05 | Reuse pola existing yang sudah terbukti; tidak mengarang mekanisme baru; radius dampak kecil (satu field request + satu query pemeriksaan sebelum create) | Perlu migration untuk kolom idempotency key baru pada `InpEpisode` atau `RegPatientEncounter` bila belum ada kolom yang bisa dipakai ulang — perlu dicek dulu apakah `RegistrationIdempotencyKey` pada `RegPatientEncounter` bisa dipakai ulang langsung tanpa kolom baru |
| B | Tidak menambahkan idempotency key di level data; cukup proteksi sisi klien (disable tombol submit saat request berjalan) | Tidak ada perubahan backend/database | Tidak melindungi dari retry jaringan di luar kendali klien (timeout lalu auto-retry), hanya mengurangi risiko klik ganda manual — perlindungan parsial, bukan solusi struktural |

**Solusi terpilih: Opsi A**, tunduk pada pengecekan teknis saat implementasi (apakah kolom
`RegistrationIdempotencyKey` pada `RegPatientEncounter` bisa dipakai ulang tanpa migration baru,
mengingat `BuildInpatientEncounter` sudah membuat `RegPatientEncounter` di transaksi yang sama).

1. Reuse murni pola yang sudah terbukti bekerja (`EncounterIntakeService`), bukan arsitektur baru.
2. Radius dampak kecil, tidak mengubah kontrak respons endpoint.
3. Menutup `ISS-EPS-022-02` secara independen dari keputusan K-01/K-02 pada `ISS-EPS-022-01` —
   idempotency key tetap berguna apa pun bentuk guard yang akhirnya dipilih untuk duplikasi
   admisi yang disengaja (idempotency mencegah duplikasi yang **tidak disengaja**, guard mencegah
   duplikasi yang **disengaja tapi tidak diinginkan** — dua masalah berbeda, saling melengkapi).

**Kenapa bukan opsi lain.** Opsi B tidak melindungi dari retry jaringan di luar kendali klien,
berbeda dengan pola idempotency key yang sudah terbukti menangani kasus itu di modul lain.

## 3. Skema tampilan sebelum → sesudah

Tidak ada perubahan tampilan yang ditetapkan di sini — `ISS-EPS-022-01` menunggu keputusan bentuk
guard (K-02) yang baru menentukan apakah ada perubahan UI (opsi C pada solusi di atas butuh UI
konfirmasi; opsi A/B tidak). `ISS-EPS-022-02` murni backend, tidak ada perubahan tampilan.

## 4. Rincian perbaikan

### FIX-EPS-022-01 — Guard mencegah/menangani admisi RANAP ganda untuk pasien yang sama

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-022-01` |
| **Area** | Backend (dan Frontend bila K-02 memilih opsi C) |
| **Jenis perubahan** | Source backend; berpotensi `DESIGN_CHANGE` bila K-02 memilih opsi C (butuh kontrak API baru) |
| **Berkas yang diubah** | Belum ditetapkan — bergantung K-02. Kandidat: `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` (method `OpenAdmissionAsync`, sekitar baris 180-192 dan 321-338) |
| **Radius dampak** | Seluruh alur admisi RANAP, termasuk pemanggil yang mengirim `EncounterId` existing maupun yang membuat encounter baru |
| **Bergantung pada** | K-01, K-02 (bagian 6) |

**Langkah.** Belum dirinci — menunggu K-01/K-02. Begitu dijawab, dokumen ini diperbarui dengan
langkah konkret mengikuti opsi yang dipilih (A/B/C pada bagian 2).

**Acceptance criteria.** Belum dirinci — menunggu keputusan bentuk guard.

**Verifikasi.** Belum dirinci.

**Risiko.** Opsi A berisiko memblokir skenario re-admisi yang sah tanpa jalur pembatalan Draft
lama yang jelas — perlu dipastikan ada cara bagi petugas membatalkan Draft lama sebelum guard ini
aktif, atau petugas akan terjebak tanpa jalan keluar.

### FIX-EPS-022-02 — Tambahkan idempotency key ke OpenAdmissionRequest

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-022-02` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend; migration database bila kolom idempotency key belum bisa dipakai ulang |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` (`OpenAdmissionRequest`, baris 29-69); `InpEpisodeService.cs` (`OpenAdmissionAsync`, baris 180-192 dan sekitar `BuildInpatientEncounter` baris 346-352) |
| **Radius dampak** | Seluruh pemanggil endpoint admisi rawat inap (frontend layar Admisi Rawat Inap) |
| **Bergantung pada** | Tidak ada — independen dari `FIX-EPS-022-01` |

**Langkah.**

1. Cek apakah kolom `RegistrationIdempotencyKey` pada `RegPatientEncounter.cs:195-209` bisa
   dipakai ulang untuk alur admisi (encounter yang dibuat `BuildInpatientEncounter` sudah
   menyertakan properti ini) tanpa migration baru.
2. Tambahkan field idempotency key opsional ke `OpenAdmissionRequest`.
3. Di awal `OpenAdmissionAsync`, sebelum validasi lain (meniru urutan
   `EncounterIntakeService.RegisterAsync:81-92` — idempotensi diperiksa **sebelum** apa pun yang
   lain), bila idempotency key dikirim dan sudah pernah dipakai sebelumnya, kembalikan hasil
   admisi yang sudah ada (replay), bukan membuat admisi baru.
4. Pastikan pemanggil frontend (layar Admisi Rawat Inap) mengirim idempotency key yang baru untuk
   setiap percobaan admisi yang benar-benar berbeda (bukan retry), mengikuti pola
   `generateUuid()` yang sudah dipakai layar Billing testing tool sebagai referensi pola client-side
   (`QuilvianSystemFrontendDev` `create-manual/use-create-manual-invoice.js`).

**Acceptance criteria.**

1. Mengirim `OpenAdmissionRequest` dua kali dengan idempotency key yang sama menghasilkan satu
   admisi (percobaan kedua mengembalikan hasil admisi pertama, bukan membuat admisi baru).
2. Mengirim dua `OpenAdmissionRequest` dengan idempotency key berbeda untuk pasien yang sama tetap
   menghasilkan dua admisi terpisah (idempotency key tidak menjadi guard duplikasi yang disengaja
   — itu tanggung jawab `FIX-EPS-022-01`).
3. Pemanggil existing yang tidak mengirim idempotency key (bila field dibuat opsional untuk
   backward compatibility) tetap berfungsi seperti sebelumnya.

**Verifikasi.** `dotnet build` (dijalankan manual sesuai instruksi kerja repository ini, tidak
otomatis); test terarah bila project test untuk `InpEpisodeService` tersedia; verifikasi manual
mengirim request admisi dua kali dengan idempotency key sama lewat Swagger/Postman.

**Risiko.** Bila kolom idempotency key butuh migration baru, ikuti batasan migration pada
`AGENTS.md` backend (file migration boleh disiapkan sebagai bagian hasil implementasi, tapi
`dotnet ef database update` tetap wewenang terpisah pemilik, tidak dijalankan otomatis).

## 5. Urutan pengerjaan

```text
K-01, K-02 (keputusan pemilik modul rawat-inap)
└── FIX-EPS-022-01   TERBLOKIR sampai K-01/K-02 dijawab

FIX-EPS-022-02   tanpa dependency — boleh dikerjakan sekarang, tidak menunggu K-01/K-02
```

## 6. Keputusan pemilik yang dibutuhkan

Disalin dari `issue-022` bagian 7:

| No | Keputusan | Pilihan | Rekomendasi diteruskan dari sesi diagnosis Billing (K-02 pada `issue-003` modul Billing) | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah satu pasien boleh punya >1 kunjungan/episode RANAP aktif atau Draft bersamaan? | (a) Boleh, disengaja / (b) Tidak boleh, perlu guard baru | **(b)** | `FIX-EPS-022-01` |
| K-02 | Bila (b): guard-nya berbentuk apa? | (a) Tolak admisi baru / (b) Auto-batalkan Draft lama / (c) Kombinasi dengan konfirmasi eksplisit petugas | Tidak direkomendasikan — keputusan alur kerja klinis/operasional murni | `FIX-EPS-022-01` |

Rekomendasi K-01 (b) berasal dari sesi diagnosis modul Billing (16-10-2026), diputuskan oleh
pemilik yang menangani sesi itu — **bukan** pemilik modul rawat-inap. Dicatat sebagai rekomendasi
kuat, bukan keputusan final.

## 7. Dokumen hulu yang ikut direvisi

Belum dapat ditentukan sampai K-02 dijawab. Bila K-02 memilih opsi C (konfirmasi eksplisit
petugas), kemungkinan perlu merevisi dokumen skema tampilan/kontrak API layar Admisi Rawat Inap —
dicatat ulang begitu K-02 terjawab, bukan diputuskan sepihak di sini.

## 8. Verifikasi menyeluruh

Setelah `FIX-EPS-022-01` dan `FIX-EPS-022-02` selesai:

1. Coba buka admisi RANAP baru untuk pasien yang masih punya episode Draft/aktif lain, pastikan
   perilakunya sesuai keputusan K-02 (ditolak / auto-batal Draft lama / diminta konfirmasi).
2. Kirim request admisi yang sama dua kali dengan idempotency key sama, pastikan hanya satu
   admisi yang benar-benar dibuat.
3. Pastikan skenario re-admisi yang sah (percobaan pertama gagal booking bed karena kelas pasien
   salah, lalu dicoba ulang dengan kelas yang benar) tetap bisa diselesaikan petugas tanpa jalan
   buntu.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Rencana dibuat, 2 perbaikan. `FIX-EPS-022-01` terblokir menunggu K-01/K-02; `FIX-EPS-022-02` siap dikerjakan begitu rencana ini disetujui pemilik modul rawat-inap | `diagnose-module-issue` |
