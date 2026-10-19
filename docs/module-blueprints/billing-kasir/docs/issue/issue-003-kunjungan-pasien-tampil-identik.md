# ISSUE-BKC-003 — Dropdown Pasien/Kunjungan pada Invoice Manual menampilkan beberapa baris berlabel identik

```yaml
issue_id: ISSUE-BKC-003
module_id: billing-kasir
submodule: null  # blueprint berbentuk SINGLE; <blueprint-root> = folder modul
layar: "Tambah Invoice Manual — field Pasien/Kunjungan (create-manual-invoice-view.jsx, route /health-services/billing-management/billing/invoices/create-manual)"
sumber_laporan: "Laporan pengguna 16-10-2026 disertai 1 screenshot (1 butir). Dilanjutkan 16-10-2026 dengan hasil query SELECT (menjawab P-01) disertai 4 screenshot, memunculkan 1 butir baru. Total 2 butir, 5 lampiran"
tanggal_issue: "2026-10-16"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "e70f5abf (Yasmina)"
source_sha_frontend: "8ad45262b (yasmina)"
rencana_perbaikan: ../plan-repair/plan-repair-003-kunjungan-pasien-tampil-identik.md
```

## 1. Ringkasan

> **Status 16-10-2026: `SELESAI` untuk cakupan modul Billing.** Satu-satunya perbaikan yang
> disetujui dan direncanakan di blueprint `billing-kasir` ini (`FIX-BKC-003-01`) sudah dikerjakan
> (`FE-BKC-FIX-014`) dan tervalidasi lint/build. **Tiga temuan lain (`ISS-BKC-003-T1`,
> `ISS-BKC-003-T2`, `ISS-BKC-003-02`) masih terbuka** — keputusannya sudah diambil pemilik
> (K-02, K-03), tapi implementasinya milik modul Registrasi/Rawat Inap dan belum didaftarkan
> sebagai task di sana. `SELESAI` di sini berarti selesai untuk bagian yang menjadi wewenang tulis
> Billing, bukan bahwa seluruh akar masalah sudah tertutup di semua modul.

Pada layar testing **Tambah Invoice Manual**, dropdown pencarian "Pasien / Kunjungan" menampilkan
tiga baris dengan teks yang terlihat sama persis untuk pasien Jesse Pinkman. Setelah ditelusuri
sampai ke query backend dan logika dropdown frontend, **tidak ditemukan bug duplikasi data**:
baik query backend maupun komponen dropdown frontend sudah benar dan tidak pernah menggandakan
satu baris data menjadi beberapa. Yang sebenarnya terjadi adalah tiga kunjungan (encounter) yang
**benar-benar berbeda** milik pasien yang sama, yang semuanya lolos kriteria "masih berjalan" —
tetapi label dropdown tidak menampilkan satu pun data pembeda (No. Kunjungan atau tanggal/jam),
sehingga ketiganya terlihat seperti data ganda padahal bukan. **Dugaan ini sudah dikonfirmasi
dengan data nyata**: pelapor menjalankan query yang diusulkan (P-01) dan hasilnya menunjukkan
4 baris `RegPatientEncounter` untuk pasien yang sama, tiga di antaranya (`ENC-RSMMC-00173`,
`00174`, `00175`) berstatus `Registered` aktif — persis tiga baris di dropdown — dengan dua di
antaranya (`00174`, `00175`) dibuat pada **tanggal yang sama**. ⚠ Ada satu temuan tambahan yang
lebih mendasar dan berada di luar kepemilikan modul Billing: tidak ditemukan aturan yang mencegah
satu pasien memiliki lebih dari satu kunjungan rawat inap (RANAP) yang aktif/berjalan secara
bersamaan — ini kemungkinan besar alasan ketiga kunjungan itu bisa ada sekaligus. Dari data yang
sama, pelapor juga menemukan satu kejanggalan baru pada baris keempat (`00172`, sudah selesai
10 hari lalu): statusnya `Completed` tetapi `CompletedAt` kosong — ditelusuri sampai ke bug nyata
di endpoint lain milik modul Registrasi (`ISS-BKC-003-02`), tidak berdampak pada dropdown Billing
ini tapi tetap dicatat dan diteruskan ke pemiliknya.

## 2. Laporan asli

| No. laporan | Keluhan | Lampiran |
| ---: | --- | --- |
| 1 | "Mengapa ketika saya memilih kunjungan, untuk Jesse Pinkman terdapat 3 kunjungan yg sama? Coba cek querynya? Harusnya query tersebut menampilkan data kunjungan yang masih berjalan dan tidak duplicate." | Screenshot 1: dropdown terbuka menampilkan 3 baris "Jesse Pinkman · 00-00-00-18 · RANAP · Tunai", baris ketiga bertanda "· sudah ada invoice" |
| 2 | "Seharusnya ketika pasien membayar dan hasilnya lunas, encounter status menjadi selesai dan nilai completedAt langsung terisi otomatis. Mengapa bisa menjadi 3 baris yang berbeda?" — dikirim bersama hasil query SELECT yang diusulkan pada P-01 | Screenshot 2–5: hasil query `RegPatientEncounter` 4 baris (Row #1–#4) untuk pasien yang sama — tiga baris `EncounterStatus=1` (Registered) aktif dengan `Id`/`EncounterNumber` berbeda (`00173`, `00174`, `00175`), satu baris `EncounterStatus=9` (Completed) dengan `CompletedAt` kosong (`00172`) |

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-BKC-003-01` | 1 | Label dropdown kunjungan tidak menyertakan field pembeda (No. Kunjungan/tanggal) sehingga kunjungan berbeda terlihat identik | `BUG` | Frontend | High | `SUDAH-VERIFIKASI` di source **dan** di basis data (P-01 dijawab 16-10-2026: 3 `Id`/`EncounterNumber` berbeda dikonfirmasi) | `FIX-BKC-003-01` |
| `ISS-BKC-003-02` | 2 | Encounter berstatus `Completed` tapi `CompletedAt` kosong — endpoint admin ubah status tidak mengisi `CompletedAt` | `BUG` (modul lain, bukan milik Billing) | Backend — modul Registrasi (`RegistrationManagement`) | Medium | `SUDAH-VERIFIKASI` di source untuk jalur kode penyebab; `DARI-CAPTURE` untuk baris data spesifik | Tidak direncanakan di sini — lihat bagian 6 |
| `ISS-BKC-003-T1` | — | Tidak ada guard yang mencegah satu pasien punya lebih dari satu kunjungan/admisi RANAP aktif/Draft secara bersamaan | `RULE_VIOLATION` (dugaan gap aturan bisnis, bukan milik modul Billing) | Backend — modul Registrasi/Rawat Inap | Medium | `SUDAH-VERIFIKASI` di source **dan** diperkuat data P-01 (dua kunjungan dibuat tanggal sama) | Tidak direncanakan di sini — lihat bagian 6 |
| `ISS-BKC-003-T2` | — | Endpoint admisi rawat inap (`OpenAdmissionAsync`) tidak punya idempotency key, berbeda dari jalur pendaftaran INT-05 yang sudah punya | `BUG` (dugaan, modul Rawat Inap) | Backend — modul Rawat Inap | Low | `SUDAH-VERIFIKASI` di source | Tidak direncanakan di sini — lihat bagian 6 |

## 4. Rincian per temuan

### ISS-BKC-003-01 — Label dropdown kunjungan tidak menyertakan field pembeda

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source; data dari capture pelapor |
| **Perbaikan** | `FIX-BKC-003-01` |

**Apa yang terjadi.** Saat kasir mencari kunjungan pasien di field "Pasien / Kunjungan", bila
pasien yang sama punya lebih dari satu kunjungan rawat inap yang masih berjalan, semua kunjungan
itu muncul dengan teks persis sama: nama pasien, No. RM, jenis layanan, dan cara bayar. Tidak ada
nomor kunjungan atau tanggal yang membedakan, sehingga kasir tidak bisa tahu kunjungan mana yang
benar untuk dipilih — berisiko memilih kunjungan yang keliru dan menagihkan biaya ke kunjungan
yang salah.

**Kenapa terjadi.** Rantai sebab, dari gejala sampai akar:

1. Backend `GetActiveEncounterOptionsAsync`
   (`Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs:1512-1613`)
   benar — join 1:1 `RegPatientEncounters` ke `MstPatients` (baris 1519-1536) tanpa fan-out, dan
   mengembalikan **satu baris per kunjungan yang benar-benar berbeda** di database. Query ini
   **tidak** menggandakan data.
2. DTO `ActiveEncounterOptionResponse`
   (`Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs:305-328`) sudah
   menyertakan `EncounterNumber` (baris 308) dan `EncounterDate` (baris 313), dan keduanya diisi
   penuh oleh service (`BillingInvoiceService.cs:1596`, `:1601`). Data pembedanya **sudah
   tersedia** dari backend.
3. Frontend `FilterSelect`
   (`src/components/features/base-features/filter-select.jsx:136-179`, fungsi `normalizeOptions`)
   melakukan dedup berdasar `option.value` (yaitu `encounter.id`) lewat `Map` — bukan berdasar
   label. Karena tiap kunjungan punya `id` (GUID) berbeda, dedup ini **tidak** menyatukan ketiganya
   menjadi satu; ketiganya tetap tiga opsi terpisah.
4. Redux reducer `getActiveEncounterOptions.fulfilled`
   (`QuilvianSystemFrontendDev/src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx:1258-1262`)
   **mengganti** `state.activeEncounterOptions` dengan array baru setiap fetch — bukan
   menambahkan (append) ke array lama. Tidak ada duplikasi dari sisi state management.
5. **Akar masalahnya**: konstruksi label opsi di
   `QuilvianSystemFrontendDev/src/components/view/health-services/billing-management/billing-invoices/create-manual/create-manual-invoice-view.jsx:52-86`
   hanya memakai `patientName`, `medicalRecordNumber`, `serviceType`, dan `paymentDisplay`:

   ```javascript
   label: `${patientName} · ${medicalRecordNumber} · ${serviceType} · ${paymentDisplay}${
     hasInvoice ? " · sudah ada invoice" : ""
   }`,
   ```

   Field `encounterNumber` dan `encounterDate` yang sudah dikirim backend **tidak pernah dibaca**
   di sini. Begitu dua kunjungan berbeda punya kombinasi nama+RM+jenis layanan+cara bayar yang
   sama (hal yang wajar terjadi untuk pasien rawat inap berulang), labelnya otomatis identik.
   Sebagai perbandingan, kartu konteks di bawah dropdown
   (`encounter-context-card.jsx:54,59,89`) justru **sudah** menampilkan `encounterNumber` dan
   `encounterDate` — tapi hanya setelah satu opsi dipilih, bukan di dalam daftar pilihan itu
   sendiri, sehingga tidak membantu kasir memilih kunjungan yang benar di awal.

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ada keputusan desain yang disetujui
(`00-interview-decisions.md`, `03-frontend-architecture.md`) yang secara eksplisit menetapkan
komposisi label dropdown ini sehingga field pembeda sengaja dihilangkan. Justru ada preseden
berlaku untuk masalah yang sama persis: `BKC-DEC-061`
(`docs/module-blueprints/billing-kasir/00-interview-decisions.md:677`) memutuskan untuk dropdown
Tarif Layanan pada layar yang sama — "Bila masih tersisa >1 baris untuk nama yang sama setelah
difilter, tampilkan semua sebagai opsi terpisah berlabel scope ... — tidak memilih diam-diam."
Prinsip itu (opsi yang tampak sama harus didisambiguasi secara eksplisit di label, bukan
dibiarkan ambigu) berlaku analog untuk dropdown kunjungan, tetapi belum diterapkan di sana.

**Dampak nyata.** Seorang kasir yang membuka dropdown untuk pasien pseudonim "Jesse Pinkman"
melihat tiga baris "Jesse Pinkman · 00-00-00-18 · RANAP · Tunai" tanpa cara membedakan mana
kunjungan hari ini dan mana kunjungan lama. Bila salah pilih, biaya tertagih ke kunjungan yang
keliru — berdampak pada invoice yang salah dan perlu dikoreksi manual.

**Rekomendasi.** Tambahkan No. Kunjungan (dan idealnya tanggal/jam) ke label opsi dropdown, karena
datanya sudah tersedia penuh dari backend dan tidak membutuhkan perubahan kontrak API. Rincian
solusi di `FIX-BKC-003-01` pada plan repair.

**Verifikasi P-01 (16-10-2026, `DARI-CAPTURE`).** Pelapor menjalankan query SELECT yang diusulkan
pada bagian 7 dan membagikan 4 baris hasil untuk pasien yang sama (No. RM `00-00-00-18`):

| Row | `Id` | `EncounterNumber` | `EncounterStatus` | `IsActive` | `CompletedAt` | `EncounterDate` |
| --- | --- | --- | --- | --- | --- | --- |
| #1 | `3aa95598-c88d-4a9a-8525-23c440d102ba` | `ENC-RSMMC-00175` | `1` (Registered) | `true` | `NULL` | 2026-10-16 |
| #2 | `164bb832-ac3b-4284-81fa-8e868f4d4a09` | `ENC-RSMMC-00174` | `1` (Registered) | `true` | `NULL` | 2026-10-16 |
| #3 | `17f3c906-c964-4974-b031-f1b4b049c28d` | `ENC-RSMMC-00173` | `1` (Registered) | `true` | `NULL` | 2026-10-15 |
| #4 | `820460d2-a52f-4b16-9926-6b580a0a57b6` | `ENC-RSMMC-00172` | `9` (Completed) | `true` | `NULL` | 2026-10-06 |

Baris #1–#3 **mengonfirmasi dugaan di atas secara langsung**: tiga `Id` (GUID) yang benar-benar
berbeda, semuanya `EncounterStatus = Registered` (nilai enum `1` pada
`Areas/HealthServices/RegistrationManagement/Enums/EncounterStatus.cs:10-11` — status valid,
bukan Draft/terminal) dan `IsActive = true` — persis kriteria yang dipakai
`ManualChargeEligibleEncounterStatuses` (`BillingInvoiceService.cs:1496-1506`, memuat
`Registered`). Ketiganya inilah tiga baris yang muncul di dropdown pelapor. Baris #4 (`Completed`)
**bukan** salah satu dari tiga baris dropdown — `Completed` tidak termasuk
`ManualChargeEligibleEncounterStatuses`, jadi baris ini sudah tersaring oleh pemeriksaan status
saja, terlepas dari nilai `CompletedAt`-nya (lihat `ISS-BKC-003-02` untuk kejanggalan baris #4
itu sendiri).

Pola nomor `EncounterNumber` berurutan (`00173` → `00175`) dengan dua di antaranya (`00174`,
`00175`) dibuat pada **tanggal yang sama** (2026-10-16) memperkuat dugaan `ISS-BKC-003-T1`/`T2`:
kemungkinan besar tiga kunjungan ini lahir dari pendaftaran/admisi berulang untuk pasien yang sama
dalam rentang waktu singkat, yang tidak dicegah oleh aturan bisnis saat ini.

### ISS-BKC-003-02 — Encounter berstatus Completed tapi CompletedAt kosong

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Backend — modul Registrasi (`RegistrationManagement`), **bukan milik Billing** |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source untuk jalur kode penyebab; `DARI-CAPTURE` untuk baris data spesifik (`ENC-RSMMC-00172`) |
| **Perbaikan** | Tidak direncanakan di sini — lihat bagian 6 |

**Apa yang terjadi.** Satu kunjungan pasien (`ENC-RSMMC-00172`, tanggal 2026-10-06) berstatus
"Selesai" (`EncounterStatus = Completed`), tetapi kolom `CompletedAt` yang seharusnya mencatat
kapan kunjungan itu selesai justru kosong (`NULL`). Pelapor benar: jalur resmi pelunasan tagihan
seharusnya mengisi keduanya sekaligus.

**Kenapa terjadi.** Ditemukan dua jalur kode berbeda yang bisa mengubah status jadi `Completed`,
dan hanya satu yang benar:

1. Jalur resmi pelunasan billing — `BillingFinalizationService.TryCompleteEncounterForInvoiceAsync`
   (`Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs:632-633`,
   dipicu `BillingSettlementService.cs:779` setelah invoice lunas) — **mengisi `CompletedAt` dengan
   benar**, bersamaan dengan mengubah status:
   ```csharp
   encounter.EncounterStatus = EncounterStatus.Completed;
   encounter.CompletedAt = occurredAt.UtcDateTime;
   ```
2. Jalur admin generik — endpoint `PATCH /patient-encounters/{id}/status` dan alias
   `PATCH /admin/{id}/status`, method `UpdateEncounterStatus`
   (`Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:2003`) —
   menerima `EncounterStatus` apa pun (termasuk `Completed`) dari pemanggil lalu langsung
   menyimpannya **tanpa pernah mengisi `CompletedAt`** di method ini. Bahkan ada komentar pada
   modul lain (`EmergencyVisitService.cs:636-637`) yang secara eksplisit menyebut endpoint inilah
   yang dipakai sebagai jalur paralel ke `Completed` — menandakan ini bukan jalur tersembunyi,
   tapi memang dipakai, hanya lupa mengisi `CompletedAt`.

**Apakah ini menyimpang dari desain?** `BUG`. Baris data (#4 pada verifikasi P-01 di
`ISS-BKC-003-01`) konsisten dengan kunjungan yang statusnya diubah lewat jalur admin
(`UpdateEncounterStatus`), bukan lewat pelunasan tagihan — jalur pelunasan terbukti selalu
mengisi `CompletedAt`. Endpoint ini juga tidak mencatat audit log, sehingga tidak ada jejak siapa
yang mengubah status dan kapan untuk baris ini.

**Dampak nyata.** Untuk layar Billing ini **tidak berdampak**: `Completed` sudah tersaring oleh
pemeriksaan status saja pada dropdown kunjungan (lihat verifikasi P-01), terlepas dari
`CompletedAt`. Dampaknya ada di tempat lain yang mungkin bergantung pada `CompletedAt` sebagai
jejak waktu penyelesaian kunjungan (pelaporan, rekonsiliasi) — berpotensi menampilkan kunjungan
"selesai" tanpa waktu selesai yang valid.

**Rekomendasi.** Diteruskan ke pemilik modul Registrasi: isi `CompletedAt` di
`PatientEncounterController.cs:2003` setiap kali `EncounterStatus` diubah menjadi `Completed`
lewat endpoint tersebut, konsisten dengan jalur pelunasan billing. Di luar wewenang tulis dan
kepemilikan modul Billing, sehingga tidak masuk `plan-repair-003`.

## 5. Tanya-jawab pelapor

> **T:** Mengapa ketika saya memilih kunjungan, untuk Jesse Pinkman terdapat 3 kunjungan yang sama?
>
> **J:** Ketiganya **bukan** data yang sama digandakan — ketiganya adalah tiga kunjungan rawat
> inap (RANAP) yang benar-benar berbeda milik pasien yang sama, yang semuanya masih berstatus
> aktif/berjalan sehingga lolos kriteria "kunjungan aktif" yang dipakai dropdown ini. Masalahnya
> ada di tampilan: label dropdown hanya menulis nama pasien, No. RM, jenis layanan, dan cara
> bayar — tanpa No. Kunjungan atau tanggal yang sebenarnya membedakan ketiganya — sehingga tiga
> kunjungan yang berbeda itu terlihat seperti satu data yang digandakan. Lihat `ISS-BKC-003-01`.

> **T:** Coba cek querynya?
>
> **J:** Sudah dicek sampai ke baris kode. Query backend `GetActiveEncounterOptionsAsync`
> (`BillingInvoiceService.cs:1512-1613`) sudah benar: join 1:1 ke tabel pasien, tanpa operasi yang
> bisa menggandakan baris, dan tidak butuh `Distinct()` tambahan karena memang tidak ada sumber
> penggandaan. Komponen dropdown dan Redux di frontend juga sudah diperiksa dan sama-sama tidak
> menggandakan data (lihat rantai sebab di `ISS-BKC-003-01`). Bukan query yang salah.

> **T:** Harusnya query tersebut menampilkan data kunjungan yang masih berjalan dan tidak
> duplicate.
>
> **J:** Query **sudah** hanya menampilkan kunjungan yang masih berjalan (filter `IsActive`,
> bukan status terminal/Draft, belum ada invoice yang closed — `BillingInvoiceService.cs:1523-1535`),
> dan datanya **memang tidak duplicate** di level baris — tiga `Id` (GUID) kunjungan itu berbeda.
> Yang terlihat seperti "duplicate" adalah teksnya, bukan datanya (lihat `ISS-BKC-003-01`). Ada
> pertanyaan lanjutan yang lebih penting di baliknya: apakah memang wajar satu pasien punya tiga
> kunjungan RANAP aktif sekaligus? Itu temuan tambahan `ISS-BKC-003-T1` di bagian 6 — jawabannya
> ada di modul Registrasi/Rawat Inap, bukan di modul Billing.

> **T:** Seharusnya ketika pasien membayar dan hasilnya lunas, encounter status menjadi selesai
> dan nilai completedAt langsung terisi otomatis. Mengapa bisa menjadi 3 baris yang berbeda?
>
> **J:** Dua pertanyaan berbeda, dijawab terpisah. Soal **CompletedAt otomatis**: benar, dan jalur
> resminya (pelunasan tagihan lewat `BillingFinalizationService`) memang sudah mengisi `CompletedAt`
> dengan benar setiap kali status diubah ke `Completed`. Baris yang Anda temukan dengan
> `CompletedAt` kosong (`ENC-RSMMC-00172`) kemungkinan besar bukan berasal dari jalur pelunasan
> itu, melainkan dari endpoint admin terpisah yang memang punya bug: tidak mengisi `CompletedAt`
> sama sekali. Ini bug nyata, tapi milik modul Registrasi, bukan Billing — lihat `ISS-BKC-003-02`.
> Soal **3 baris berbeda**: sudah dikonfirmasi dari data yang Anda bagikan — bukan satu kunjungan
> yang digandakan, melainkan tiga `RegPatientEncounter` dengan `Id` dan `EncounterNumber` berbeda
> (`00173`, `00174`, `00175`), semuanya berstatus `Registered` (aktif, bukan Completed/Draft).
> Baris `Completed` (`00172`) yang Anda tanyakan soal `CompletedAt`-nya itu adalah kunjungan lama
> yang berbeda (10 hari sebelumnya) dan **bukan** bagian dari tiga baris di dropdown — lihat
> verifikasi P-01 pada `ISS-BKC-003-01`.

## 6. Temuan tambahan

### ISS-BKC-003-T1 — Tidak ada guard yang mencegah satu pasien punya lebih dari satu kunjungan RANAP aktif/Draft bersamaan

| | |
| --- | --- |
| **Jenis** | `RULE_VIOLATION` (dugaan gap aturan bisnis) |
| **Area** | Backend — modul Registrasi (`RegistrationManagement`) dan Rawat Inap (`InPatientManagement`), **bukan milik Billing** |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source |
| **Kenapa dicantumkan** | Ini kemungkinan besar alasan sesungguhnya kenapa tiga kunjungan RANAP aktif bisa ada sekaligus untuk satu pasien pada `ISS-BKC-003-01` — akar masalah yang lebih dalam dari sekadar tampilan dropdown |

**Apa yang terjadi.** Sistem saat ini mengizinkan satu pasien memiliki lebih dari satu kunjungan
rawat inap (RANAP) berstatus aktif/belum selesai secara bersamaan, termasuk kunjungan berstatus
Draft.

**Kenapa terjadi.**

1. Pada jalur pendaftaran/registrasi, ada saklar "satu pasien satu kunjungan aktif"
   (`OutpatientEncounterRules.cs:19-99`, konstanta `BlockActiveEncounterConfigKey` baris 71-73),
   tapi (a) defaultnya **nonaktif** (`RJ-DOC-DEC-026`, baris 66-70), dan (b)
   `WhereOutpatientClinicEncounter` (baris 26-33) secara eksplisit **hanya** mencakup
   `EncounterType.Outpatient` berklinik — dokumentasinya sendiri menegaskan "Rawat Inap tidak
   menghalangi (`RJ-DOC-DEC-022`)". `PatientEncounterController.cs:1792-1800` memanggil aturan ini
   lewat `FindActiveEncounterAsync` (baris 1713-1714), sehingga walau saklar dinyalakan, RANAP
   tetap tidak pernah diblokir di titik ini.
2. Pada jalur admisi rawat inap, `InpEpisodeService.OpenAdmissionAsync`
   (`InpEpisodeService.cs:180-192`) hanya menolak bila pasien "sedang hadir" — didefinisikan oleh
   `FindPresentEpisodeAsync` (`InpEpisodeService.Assignments.cs:69-77`) sebagai episode berstatus
   `Admitted`, atau `DischargePending` dengan `PhysicallyLeftAt == null`. Episode berstatus
   `Draft` **tidak** termasuk "hadir". `InpEpisodeService.cs:321-338` secara eksplisit menyatakan
   admisi Draft kedua (dan seterusnya) untuk pasien yang sama hanya menghasilkan **peringatan**
   ("admisi Draft ganda adalah peringatan, bukan penolakan. Petugas boleh melanjutkan"), bukan
   penolakan.

**Apakah ini menyimpang dari desain?** Tidak dapat ditentukan di sini — ini keputusan aturan
bisnis milik modul Registrasi/Rawat Inap, bukan Billing, dan skill ini tidak berwenang mengarang
atau mengubah aturan bisnis modul lain.

**Dampak nyata.** Selain memperburuk `ISS-BKC-003-01` (dropdown Billing jadi ambigu), berpotensi
membingungkan alur rawat inap itu sendiri — dua admisi Draft aktif untuk pasien yang sama di modul
asalnya.

**Rekomendasi.** Diteruskan ke pemilik modul Registrasi/Rawat Inap untuk diputuskan: apakah
kondisi ini memang disengaja (mis. untuk skenario re-admisi cepat) atau perlu guard tambahan.
Tidak termasuk cakupan perbaikan `plan-repair-003` ini.

### ISS-BKC-003-T2 — Endpoint admisi rawat inap tidak punya idempotency key

| | |
| --- | --- |
| **Jenis** | `BUG` (dugaan) |
| **Area** | Backend — modul Rawat Inap (`InPatientManagement`), **bukan milik Billing** |
| **Keparahan** | Low |
| **Status bukti** | `SUDAH-VERIFIKASI` di source |
| **Kenapa dicantumkan** | Memperkuat kemungkinan penyebab `ISS-BKC-003-T1`: tanpa idempotency key, klik ganda pada tombol admisi bisa menghasilkan lebih dari satu kunjungan/episode Draft untuk kunjungan yang sama |

**Apa yang terjadi.** DTO `OpenAdmissionRequest`
(`Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs:29-69`) tidak memiliki
field idempotency key. Backend **sudah** punya mekanisme idempotency key yang terbukti bekerja
(`RegistrationIdempotencyKey` pada `RegPatientEncounter.cs:195-209`, dipakai
`EncounterIntakeService.cs:81-116,484-498`), tetapi mekanisme itu khusus jalur pendaftaran
modul penunjang (INT-05, mis. Laboratorium) — bukan jalur admisi rawat inap utama. `OpenAdmissionAsync`
tidak memanggil `EncounterIntakeService` sama sekali.

**Kenapa terjadi.** Jalur admisi RANAP dan jalur INT-05 dikembangkan terpisah; pola idempotency
key dari INT-05 belum diterapkan ulang (reuse) ke jalur admisi.

**Apakah ini menyimpang dari desain?** `DUGAAN` — perlu dikonfirmasi pemilik modul Rawat Inap
apakah idempotency key memang direncanakan untuk jalur admisi dan belum sempat dikerjakan, atau
memang belum pernah diputuskan.

**Dampak nyata.** Klik ganda/retry jaringan pada tombol admisi berpotensi membuat lebih dari satu
`RegPatientEncounter`/`InpEpisode` Draft untuk niat admisi yang sama — berkontribusi pada
`ISS-BKC-003-T1`.

**Rekomendasi.** Diteruskan ke pemilik modul Rawat Inap. Tidak termasuk cakupan perbaikan
`plan-repair-003` ini.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Status |
| ---: | --- | --- | --- |
| P-01 | ~~Untuk memastikan ketiga baris memang tiga kunjungan berbeda, tolong jalankan query SELECT dan bagikan hasilnya.~~ | Analisis source menyimpulkan 3 `Id` berbeda, perlu dikonfirmasi dari data nyata | **Terjawab 16-10-2026.** Hasil query (4 baris) mengonfirmasi tiga `Id`/`EncounterNumber` berbeda — lihat verifikasi P-01 pada `ISS-BKC-003-01` |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Keputusan diambil | Status |
| ---: | --- | --- | --- | --- |
| K-01 | Format tambahan pada label dropdown kunjungan: cukup No. Kunjungan saja, atau No. Kunjungan + tanggal/jam? | No. Kunjungan saja / No. Kunjungan + tanggal+jam | **No. Kunjungan + tanggal+jam** | **Diputuskan 16-10-2026** oleh pemilik modul Billing. `FIX-BKC-003-01` tidak lagi tertahan — lihat `plan-repair-003` (status `DISETUJUI`) |
| K-02 | (Lintas modul, untuk pemilik Registrasi/Rawat Inap — bukan pemilik Billing) Apakah satu pasien memang boleh punya >1 kunjungan RANAP aktif/Draft bersamaan? | Boleh (disengaja) / Tidak boleh (perlu guard baru) | **Tidak boleh — perlu guard baru** | **Diputuskan 16-10-2026** oleh pemilik yang sama di sesi ini. Keputusan dicatat di sini, tapi implementasinya (guard baru pada `OutpatientEncounterRules`/`InpEpisodeService` untuk RANAP) tetap wewenang dan tanggung jawab pemilik modul Registrasi/Rawat Inap — belum ada `FIX` terdaftar di blueprint `billing-kasir` ini untuk butir ini. **Diteruskan 16-10-2026** sebagai [`ISSUE-EPS-022`](../../../rawat-inap/episode-rawat-inap/docs/issue/issue-022-admisi-ranap-ganda-tanpa-guard.md) pada blueprint `rawat-inap/episode-rawat-inap` (menunggu persetujuan pemilik modul tersebut) |
| K-03 | (Lintas modul, untuk pemilik Registrasi — bukan pemilik Billing) Apakah `CompletedAt` perlu diisi di endpoint `UpdateEncounterStatus` (`PatientEncounterController.cs:2003`) setiap kali status diubah jadi `Completed`? | Ya, perbaiki endpoint tersebut / Tidak, endpoint ini memang tidak boleh dipakai untuk status `Completed` (perlu validasi penolakan sebagai gantinya) | **Ya — perbaiki endpoint agar mengisi `CompletedAt`**, konsisten dengan jalur pelunasan billing | **Diputuskan 16-10-2026** oleh pemilik yang sama di sesi ini. Sama seperti K-02: implementasinya milik modul Registrasi, belum ada `FIX` di blueprint ini. **Diteruskan 16-10-2026** sebagai [`ISSUE-RJ-DOC-001`](../../../rawat-jalan/docs/issue/issue-001-encounter-completed-tanpa-completed-at.md) pada blueprint `rawat-jalan` (menunggu persetujuan pemilik modul tersebut) |

## 8. Catatan pola

Pola yang sama baru saja diputuskan secara eksplisit untuk dropdown lain di layar yang persis
sama: `BKC-DEC-061` (`00-interview-decisions.md:677`) untuk dropdown Tarif Layanan — "opsi yang
tampak sama untuk nama yang sama harus didisambiguasi dengan label scope, tidak dibiarkan ambigu
atau dipilih diam-diam". `ISS-BKC-003-01` adalah masalah yang sama persis pada dropdown kunjungan
di form yang sama, hanya belum diterapkan di sana. Disarankan agar prinsip ini dicatat sebagai
pola umum untuk seluruh dropdown pencarian di modul Billing: setiap field yang menampilkan opsi
hasil pencarian wajib menyertakan field pengenal unik yang terlihat (nomor/kode), bukan hanya
atribut deskriptif yang bisa berulang antar baris.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Issue dibuat dari laporan 1 butir disertai 1 screenshot | `diagnose-module-issue` |
| 2026-10-16 | Pelapor menjawab P-01 dengan hasil query SELECT (4 baris). Menambahkan verifikasi data pada `ISS-BKC-003-01` (status bukti naik ke `SUDAH-VERIFIKASI` di source dan basis data), menambahkan butir baru `ISS-BKC-003-02` (Completed tanpa CompletedAt, milik modul Registrasi), dan keputusan pemilik baru `K-03` | `diagnose-module-issue` |
| 2026-10-16 | Pemilik modul Billing memutuskan K-01 (No. Kunjungan + tanggal+jam pada label dropdown), K-02 (satu pasien tidak boleh punya >1 kunjungan RANAP aktif/Draft bersamaan — perlu guard baru di modul lain), dan K-03 (endpoint `UpdateEncounterStatus` perlu diperbaiki agar mengisi `CompletedAt` — di modul lain). `status` berubah `TERBUKA` → `DALAM_PERBAIKAN`; `plan-repair-003` berubah `MENUNGGU_PERSETUJUAN` → `DISETUJUI` | `diagnose-module-issue` atas perintah pemilik |
| 2026-10-16 | `FIX-BKC-003-01` dikerjakan lewat `build-module-frontend` sebagai `FE-BKC-FIX-014`: label dropdown kunjungan kini menyertakan No. Kunjungan + tanggal/jam. Lint 0 error, `npm run build` lulus; uji manual live browser `NOT FEASIBLE` (backend tidak dijalankan otomatis). `status` berubah `DALAM_PERBAIKAN` → `SELESAI` (cakupan modul Billing saja — lihat catatan pada bagian 1). `ISS-BKC-003-T1`, `ISS-BKC-003-T2`, `ISS-BKC-003-02` tetap terbuka, menunggu pendaftaran task di modul Registrasi/Rawat Inap. Laporan: [FE-BKC-FIX-014.md](../../task/report/frontend/FE-BKC-FIX-014.md) | `build-module-frontend` atas perintah pemilik |
| 2026-10-16 | `ISS-BKC-003-T1`/`T2` (K-02) diteruskan sebagai [`ISSUE-EPS-022`](../../../rawat-inap/episode-rawat-inap/docs/issue/issue-022-admisi-ranap-ganda-tanpa-guard.md) pada blueprint `rawat-inap/episode-rawat-inap`; `ISS-BKC-003-02` (K-03) diteruskan sebagai [`ISSUE-RJ-DOC-001`](../../../rawat-jalan/docs/issue/issue-001-encounter-completed-tanpa-completed-at.md) pada blueprint `rawat-jalan`. Keduanya `MENUNGGU_PERSETUJUAN` pemilik modul masing-masing | `diagnose-module-issue` atas perintah pemilik |
