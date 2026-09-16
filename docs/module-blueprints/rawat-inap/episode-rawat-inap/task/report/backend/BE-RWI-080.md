# Laporan Perubahan Backend — `BE-RWI-080`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-080` |
| Judul | Penugasan konsulen, dokter jaga, dan penugasan singkat |
| Slice | Gelombang 2 — `RI-V2-1`, `EPIC RI-39` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-080` |
| Trace | `FR-RI-193`, `FR-RI-194`, `FR-RI-195`; `RWI-DEC-099`, `RWI-DEC-130`; `VAL-INP-01`, `VAL-INP-08`, `VAL-INP-09`; `INV-INP-12`; `contracts/api-contract.md` `0.9.0` bagian 10.2; `02-backend-architecture.md` 11.5.3 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-079` — **selesai di source** pada sesi yang sama |
| Klasifikasi | `HEAVY` — dua endpoint baru, dua metode service, tujuh penjaga aturan bisnis, satu penjaga kewenangan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **Selesai di source.** `dotnet build` dan verifikasi proses bisnis `UAT-46` s.d. `UAT-48` **`NOT RUN`** atas permintaan pemilik pekerjaan — lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Sampai hari ini, satu-satunya cara menambahkan dokter pada sebuah episode rawat inap adalah
**mengalihkan DPJP** — `POST /{id}/doctor-assignments`. Jalur itu menutup penugasan DPJP yang sedang
berlaku dan membuka penggantinya.

Akibatnya, kebutuhan yang sangat lazim tidak punya jalan sama sekali: melibatkan dokter **kedua**
tanpa mengganti dokter penanggung jawabnya.

**Contoh nyata.** Budi dirawat dr. Ahmad. Kondisi jantungnya memburuk, dan kepala ruangan perlu
melibatkan dr. Sari sebagai konsulen kardiologi. Dengan jalur yang ada, satu-satunya cara adalah
mengalihkan DPJP ke dr. Sari — yang berarti dr. Ahmad berhenti bertanggung jawab atas pasiennya
sendiri, dan sejak saat itu tidak lagi dapat memutuskan Budi boleh pulang.

Task ini membuka jalur tulis kedua yang **tidak pernah** menyentuh DPJP.

---

## 2. Proses bisnis

**Tujuan.** Kepala ruangan dapat melibatkan konsulen, memanggil dokter jaga, dan membuat penugasan
singkat penulisan catatan terlambat — tanpa satu pun di antaranya menggeser DPJP.

**Pelaku.** Kepala ruangan atau supervisor. **Bukan** dokter yang bersangkutan — `RWI-DEC-130` (4).

**Pemicu.** Kondisi pasien menuntut pendapat dokter lain, shift jaga berganti, atau ada catatan
klinis yang tertinggal.

**Langkah yang berurutan — pelibatan konsulen.**

1. Kepala ruangan membuka episode Budi.
2. Ia memilih dr. Sari, peran **konsulen**, dan mengisi alasan "konsultasi kardiologi".
3. Sistem memeriksa: pemanggil kepala ruangan atau supervisor; alasan berisi kalimat yang dapat
   dibaca; peran bukan DPJP; episode berstatus `Admitted` atau `DischargePending`; dokter aktif
   pada master dokter; tidak ada penugasan aktif dengan peran yang sama pada periode yang
   bertindih.
4. Baris penugasan baru tersimpan. **dr. Ahmad tetap DPJP** — barisnya tidak disentuh sama sekali.
5. Besoknya konsultasi selesai. Kepala ruangan menekan "Akhiri", dan baris dr. Sari diberi waktu
   selesai. Barisnya **tetap ada** beserta seluruh periodenya.

**Langkah yang berurutan — penugasan singkat penulisan catatan terlambat.**

1. Kepala ruangan memilih dr. Yoga, tujuan **`LateDocumentation`**, dan mengisi waktu selesai
   beserta alasannya.
2. Sistem **mengabaikan** waktu mulai yang dikirim pemanggil dan menetapkannya "sekarang".
3. Sistem memaksa perannya menjadi dokter jaga; permintaan yang mengirim peran lain ditolak.
4. Baris tersimpan, dan check constraint `CK_InpDoctorAssignment_LateDocumentation` dari
   `BE-RWI-079` ikut memeriksanya di tingkat database.

**Kenapa waktu mulai tidak dapat dimundurkan.** Jendela inilah yang nanti dibaca penjaga kewenangan
menulis catatan klinis. Jendela yang dapat dimundurkan pemanggil memungkinkan sebuah catatan ditulis
seolah-olah dibuat pada masa yang sudah lewat — pada rekam medis, itu bukan kemudahan melainkan
pemalsuan.

**Contoh berangka.** Pemanggil mengirim `startDateTime = 2026-09-15 08:00` bersama
`assignmentPurpose = 1` pada 16 September pukul 10.00. Yang tersimpan adalah
`StartDateTime = 2026-09-16 10:00`. Nilai kiriman diabaikan, bukan ditolak — menolaknya hanya
memindahkan masalah ke layar tanpa membuat jendelanya lebih aman.

**Status yang dihasilkan.** Tidak ada perubahan status episode. `INV-INP-12` ditegakkan: siapa DPJP
tidak berubah oleh penugasan mana pun yang lahir dari jalur ini.

**Jalur tidak normal.**

| Keadaan | Kode | Pesan bagi pengguna |
| --- | :---: | --- |
| Dokter belum dipilih | `400` | "Dokter yang dilibatkan belum dipilih." |
| Alasan kosong atau hanya tanda baca | `400` | "Alasan pelibatan dokter wajib diisi dengan kalimat yang dapat dibaca." |
| Penugasan singkat tanpa waktu selesai | `400` | "VAL-INP-08 — penugasan singkat penulisan catatan terlambat wajib punya waktu selesai." |
| Waktu selesai tidak lebih besar dari waktu mulai | `400` | "Waktu selesai penugasan harus lebih besar dari waktu mulainya." |
| Peran atau tujuan di luar enum | `400` | "Peran/Tujuan penugasan yang dipilih tidak dikenal." |
| Pemanggil bukan kepala ruangan atau supervisor | `403` | "Hanya kepala ruangan atau supervisor yang dapat melibatkan dokter pendukung pada episode ini." |
| Episode tidak ditemukan | `404` | "Episode rawat inap tidak ditemukan." |
| Admisi sudah gugur kedaluwarsa | `409` | "Admisi ini sudah gugur karena ditinggalkan melewati batas waktu." |
| Dokter sudah punya penugasan aktif berperan sama pada periode itu | `409` | "Dokter ini sudah punya penugasan aktif dengan peran yang sama pada periode tersebut." |
| Peran DPJP dikirim ke jalur ini | `422` | "VAL-INP-09 — penugasan DPJP tidak dibuat lewat jalur ini. Gunakan pengalihan DPJP." |
| Episode bukan `Admitted` atau `DischargePending` | `422` | "Dokter pendukung hanya dapat dilibatkan pada episode yang sedang dirawat atau sedang menunggu pulang." |
| Penugasan singkat berperan selain dokter jaga | `422` | "Penugasan singkat penulisan catatan terlambat selalu berperan dokter jaga." |
| Dokter tidak ditemukan atau tidak aktif | `422` | "Dokter yang dipilih tidak ditemukan atau tidak aktif." |
| Penugasan DPJP dicoba diakhiri | `409` | "VAL-INP-09 — penugasan DPJP tidak dapat diakhiri lewat jalur ini. Gunakan pengalihan DPJP." |
| Penugasan sudah berakhir | `409` | "Penugasan ini sudah berakhir." |

**Kenapa mengakhiri DPJP ditolak.** Mengakhirinya lewat jalur ini akan meninggalkan episode berjalan
**tanpa DPJP sama sekali**. Keempat penjaga `GUARD-INP-01` sampai `GUARD-INP-04` berbunyi
`AssignmentRole = Dpjp`; tanpa satu pun baris DPJP aktif, seluruh keputusan klinis ditolak sejak
saat itu — termasuk keputusan pasien boleh pulang.

**Hasil akhirnya.** Satu pasien dapat ditangani DPJP, beberapa konsulen, dan beberapa dokter jaga
sekaligus, dan riwayat berperiodenya tetap dapat menjawab "siapa yang berwenang pada tanggal
tertentu".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` `0.9.0` bagian 10.2 | Bentuk route, request, response, dan tabel kode status yang mengikat |
| `.../02-backend-architecture.md` 11.5.3, 11.5.7 | Nama metode service dan pemetaan controller |
| `Areas/.../Services/InpEpisodeService.Assignments.cs` — `HandoverDoctorAsync` | Pola transaksi, penjaga kewenangan, dan penanganan `DbUpdateException` |
| `Areas/.../Controllers/InpatientEpisodeController.cs` — `HandoverDoctor` | Pola atribut akses, logger, dan `FromFailure` |
| `Areas/.../Helpers/InpatientActorClaims.cs` | Cara repository ini menilai kepala ruangan/supervisor |
| `Areas/.../DTOs/InpatientEpisodeAssignmentDtos.cs` | Pola penamaan request/response penugasan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../DTOs/InpatientEpisodeAssignmentDtos.cs` | `AssignSupportingDoctorRequest` dan `EndSupportingAssignmentRequest` **baru**; `InpatientDoctorAssignmentResponse` bertambah `AssignmentPurpose` |
| `Areas/.../Services/InpEpisodeService.Assignments.cs` | `AssignSupportingDoctorAsync` dan `EndSupportingAssignmentAsync` **baru**; dua proyeksi riwayat penugasan membawa `AssignmentPurpose` |
| `Areas/.../Controllers/InpatientEpisodeController.cs` | `POST /{id}/doctor-assignments/supporting` dan `PATCH /{id}/doctor-assignments/{assignmentId}/end` **baru** |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Dua endpoint baru; satu field baru pada response riwayat penugasan. Tidak ada endpoint, field, atau nilai enum existing yang berubah atau hilang. `POST /{id}/doctor-assignments` untuk pengalihan DPJP **tidak disentuh** |
| Database | Tidak ada perubahan schema pada task ini. Task ini **memakai** kolom dan check constraint yang dibuat `BE-RWI-079`; sampai migration `E1` dijalankan, kedua endpoint ini akan gagal pada runtime |
| Keamanan/Auth | **Ada.** Kedua endpoint memakai `[AccessAction("Update", ...)]` dan `[AccessPermission("InpatientEpisode", "Update")]` yang sudah ada, ditambah penjaga kepala ruangan/supervisor di dalam service. Lihat temuan hardcode role pada bagian 7 |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/supporting` | Kepala ruangan atau supervisor melibatkan konsulen, memanggil dokter jaga, atau membuat penugasan singkat penulisan catatan terlambat | `InpatientEpisode : Update` + penjaga kepala ruangan/supervisor |
| `PATCH` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/{assignmentId}/end` | Mengakhiri penugasan konsulen atau dokter jaga | `InpatientEpisode : Update` + penjaga kepala ruangan/supervisor |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments` | Riwayat penugasan. **Berubah:** bertambah `assignmentPurpose` | `InpatientEpisode : Read` |

**Bentuk permintaan `POST .../doctor-assignments/supporting`.**

```yaml
POST /api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/supporting:
  summary: Melibatkan dokter pendukung pada satu episode
  security: [ { bearer: [] } ]
  x-permission: "InpatientEpisode : Update — hanya kepala ruangan atau supervisor"
  requestBody:
    required: true
    content:
      application/json:
        schema:
          type: object
          required: [doctorId, assignmentRole, reason]
          properties:
            doctorId:          { type: string, format: uuid }
            assignmentRole:    { type: integer, enum: [2, 3], description: "2 Consultant, 3 OnCallDoctor. Nilai 1 Dpjp ditolak" }
            assignmentPurpose: { type: integer, enum: [0, 1], default: 0, description: "0 Regular, 1 LateDocumentation" }
            startDateTime:     { type: string, format: date-time, description: "DIABAIKAN untuk LateDocumentation; selalu waktu sekarang" }
            endDateTime:       { type: string, format: date-time, description: "WAJIB untuk LateDocumentation" }
            reason:            { type: string, minLength: 1, maxLength: 500 }
  responses:
    "200": { description: Penugasan dibuat; badan balasan berisi baris penugasan terbaru }
    "400": { description: "Isian kurang atau tidak masuk akal; termasuk VAL-INP-08" }
    "403": { description: Pemanggil bukan kepala ruangan atau supervisor }
    "404": { description: Episode tidak ditemukan }
    "409": { description: Penugasan aktif berperan sama pada periode yang bertindih }
    "422": { description: "VAL-INP-09; episode tidak berstatus Admitted/DischargePending; dokter tidak aktif" }

PATCH /api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/{assignmentId}/end:
  summary: Mengakhiri penugasan konsulen atau dokter jaga
  x-permission: "InpatientEpisode : Update — hanya kepala ruangan atau supervisor"
  requestBody:
    required: false
    content:
      application/json:
        schema:
          type: object
          properties:
            endDateTime: { type: string, format: date-time, description: "Kosong berarti sekarang" }
            reason:      { type: string, maxLength: 500 }
  responses:
    "200": { description: Penugasan diakhiri }
    "400": { description: Waktu berakhir melewati sekarang atau mendahului waktu mulai }
    "403": { description: Pemanggil bukan kepala ruangan atau supervisor }
    "404": { description: Episode atau penugasan tidak ditemukan }
    "409": { description: "VAL-INP-09 — penugasan DPJP; atau penugasan sudah berakhir" }
```

**Delta kontrak yang dicatat.**

| Hal | Kontrak `0.9.0` 10.2 | Yang dibuat | Alasan |
| --- | --- | --- | --- |
| Kode status penugasan singkat tanpa waktu selesai | `400` | `400` | Mengikuti kontrak. **Kartu roadmap `BE-RWI-080` acceptance criteria 2 menuliskan `422`.** Kontrak `0.9.0` yang berlaku, dan selisihnya dilaporkan di sini |
| Kode status `VAL-INP-09` pada pengakhiran | Kontrak menyebut `409` | `409` | Mengikuti kontrak. Kartu roadmap menuliskan `422` pada blok Swagger ilustratifnya |
| `Idempotency-Key` | Disebut kontrak | **Belum dipasang** | Repository ini menyimpan `IdempotencyKey` pada entity yang memilikinya, dan `InpDoctorAssignment` tidak punya kolom itu. Menambahkannya menuntut kolom baru di luar cakupan migration `E1`. Penjaga periode bertindih (`409`) sudah menutup pengiriman ganda yang tidak disengaja. **Dicatat sebagai butir terbuka**, bukan didiamkan |
| Balasan sukses | Kontrak menyebut `201` pada contoh | `200` | Mengikuti pola seluruh endpoint modul ini, yang tidak pernah mengembalikan `201`. Tabel kontrak sendiri hanya menyebut `ApiResponse<InpatientDoctorAssignmentResponse>` tanpa mengunci kodenya |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan pemilik pekerjaan pada permintaan task ini |
| Verifikasi proses bisnis `UAT-46`, `UAT-47`, `UAT-48` | Tidak dijalankan | `NOT RUN` | Menuntut aplikasi berjalan beserta database yang sudah dimigrasi; keduanya bersandar pada build yang dikecualikan |
| Verifikasi kontrak API terhadap `api-contract.md` `0.9.0` 10.2 | Route, verb, bentuk request, dan tabel kode status dibandingkan baris per baris. Empat selisih ditemukan dan seluruhnya dicatat pada bagian 4 | `PASS` dengan delta tercatat | Tabel "Delta kontrak yang dicatat" |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE`. Keberlakuan `NEW CODE` untuk dua metode dan dua endpoint baru | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Pemeriksaan QBE yang berlaku | `QBE-MOD-002` tidak menahan; tidak ada `Trx*` baru; tidak ada akses `ApplicationDbContext` dari controller; tidak ada Count/Max/Last+1 sebagai identitas — `SequenceNumber` memakai pola `Max + 1` yang **sudah** dipakai `HandoverDoctorAsync` pada baris yang sama tabelnya, dan dipertahankan agar deret nomor urut per episode tidak pecah; tidak ada `SortOrder` generik yang dipersistensi; tidak ada generic repository | `PASS` dengan satu catatan | Bagian 7 baris Masalah yang diketahui |
| Review diff dan scope | Tiga berkas disentuh, seluruhnya di dalam `InPatientManagement`. Jalur pengalihan DPJP tidak berubah satu baris pun | `PASS` | `git diff` pada ketiga berkas |
| Keseimbangan sintaks | Seimbang | `PASS` | Pemeriksaan statis; **bukan pengganti `dotnet build`** |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database yang sudah dimigrasi.

**Tidak dijalankan:** `dotnet build` serta `UAT-46` s.d. `UAT-48`, keduanya dikecualikan pemilik
pekerjaan yang menyatakan akan menjalankan build sendiri setelah implementasi source selesai.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Kepala ruangan membuat penugasan konsulen beralasan → sukses, baris bertujuan `Consultant` | Terpenuhi di source | `AssignSupportingDoctorAsync`; peran `Consultant` diterima, baris disimpan tanpa menutup baris lain |
| AC-2 — Penugasan singkat tanpa `endedAt` → ditolak menyebut `VAL-INP-08` | Terpenuhi di source, kode status mengikuti kontrak `400` | Penjaga `isLateDocumentation && !request.EndDateTime.HasValue`; pesan memuat teks `VAL-INP-08`. Selisih `422`→`400` terhadap kartu roadmap dicatat pada bagian 4 |
| AC-3 — Penugasan singkat selalu berperan dokter jaga dan berwaktu mulai "sekarang", walau pemanggil mengirim `startedAt` lain | Terpenuhi di source | Penolakan peran selain `OnCallDoctor`; `startDateTime = isLateDocumentation ? now : (request.StartDateTime ?? now)` |
| AC-4 — Penugasan baru apa pun tidak mengubah siapa DPJP episode — `INV-INP-12` | Terpenuhi di source | Peran `Dpjp` ditolak masuk jalur ini; tidak ada satu pun baris existing yang disentuh method ini; unique index `IX_InpDoctorAssignment_EpisodeId_ActiveDpjp` tidak pernah tersentuh |
| AC-5 — Perawat pelaksana memanggil endpoint ini → `403` | Terpenuhi di source | `actorIsWardHeadOrSupervisor` bernilai salah untuk peran perawat → `InpEpisodeOperationResult.Forbidden` → `403` |
| AC-6 — Penugasan DPJP dicoba diakhiri → ditolak menyebut `VAL-INP-09` | Terpenuhi di source, kode status mengikuti kontrak `409` | `EndSupportingAssignmentAsync`, penjaga `assignment.AssignmentRole == Dpjp`; pesan memuat teks `VAL-INP-09` |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Penjagaan penugasan singkat ada di **service dan database** | Terpenuhi di source — penjaga service pada `AssignSupportingDoctorAsync`, check constraint `CK_InpDoctorAssignment_LateDocumentation` dari `BE-RWI-079` |
| Check constraint dari `BE-RWI-079` ikut diuji | **Belum terpenuhi** — `NOT RUN`, menuntut database yang sudah dimigrasi |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` yang dapat dipastikan — compiler tidak dijalankan |
| Masalah yang diketahui | **Satu.** `SequenceNumber` penugasan baru diambil dengan `Max(SequenceNumber) + 1` pada episode yang sama — pola yang biasanya ditolak QBE. Pola itu **dipertahankan di sini dengan sengaja**: `HandoverDoctorAsync` sudah memakainya pada tabel yang sama, deret nomor urut berlaku satu per episode, dan unique index `(EpisodeId, SequenceNumber)` menolak tabrakan sehingga dua permintaan bersamaan tidak dapat menghasilkan nomor kembar. Mengganti polanya hanya di satu metode akan memecah deret yang sama menjadi dua cara penomoran |
| Temuan pada kode lama | **Hardcode role access.** `Areas/.../Helpers/InpatientActorClaims.cs` menilai kepala ruangan, supervisor, dan kasir dari **daftar nama peran** lewat `IsInRole` — `SupervisorOrWardHeadRoles`, `SupervisorRoles`, `CashierOrBillingRoles`. Ini melanggar aturan "jangan pernah hardcode role access". Temuan ini **sudah ada sebelum task ini** (dicatat sejak laporan `BE-RWI-008` bagian 5.3) dan **tidak diperbaiki di sini** karena perbaikannya menuntut keputusan pemilik tentang cara menyatakan kewenangan kepala ruangan lewat layar Akses Role. Task ini memakai helper yang sudah ada, bukan menambah daftar nama peran baru. **Dilaporkan, bukan diperbaiki tanpa wewenang** |
| Risiko tersisa | Kedua endpoint bersandar pada kolom `AssignmentPurpose` yang **belum ada di database mana pun**. Sampai migration `E1` dijalankan, keduanya gagal pada runtime |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-086`. Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada operasi Git yang dilakukan |
| Langkah berikutnya | Pemilik menjalankan build; setelah migration `E1` dijalankan, jalankan `UAT-46` s.d. `UAT-48` lalu tempelkan hasilnya ke bagian 5. Putuskan juga nasib `Idempotency-Key` pada jalur ini |
