# Audit Orphan Otorisasi Pasca-Rekonsiliasi — dan penemuan *naked endpoint*

> **Mode:** audit baca-saja untuk bagian A–E. Tidak ada aplikasi dijalankan, tidak ada koneksi
> database, tidak ada `AccessMenuSeeder`, tidak ada `SysAccessPolicy` yang disentuh, tidak ada
> Integration yang di-fetch atau di-merge, tidak ada commit/push.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Audit evidence** — bukan laporan task |
| Sebab | Rekonsiliasi registry terhadap DEV menyisakan policy efektif yang menunjuk identitas registry **tertutup** |
| Kandidat beku yang diaudit | `4ff7b9987cda7d11df22080ff1b2f849bbc97319` |
| Branch | `wip/be-sec-003b-verifier-transfer` |
| Otoritas database | Ekspor DEV yang disediakan pemilik sistem, dikutip apa adanya pada bagian A |
| Tanggal | 16 September 2026 |
| Hasil | **`AUTH_ORPHAN_AUDIT_BLOCKS_BE_SEC_003B`** — ditindaklanjuti `BE-SEC-012` |

---

## A. Bukti database yang dipakai

| Metrik | Nilai |
|---|---:|
| `SysAccessPolicy` fisik / efektif | 498 / 469 |
| `SysActionAccess` aktif | 1.286 |
| `SysControllerAccess` aktif | 339 |
| `SysApplicationModule` aktif | 48 |
| Identitas target `BE-SEC-003` | 24 / 24 siap |
| Identitas registry aktif ganda | 0 |

Policy efektif yang menunjuk identitas registry **tertutup** — 17 baris:

| Identitas tertutup | Policy efektif | Pemegang |
|---|---:|---|
| `KioskScanSession.Cancel` | 1 | Keperawatan × Perawat Rawat Inap |
| `Queue.Read` | 6 | Keperawatan × Perawat Rawat Jalan; Medis × Dokter IGD, Dokter Spesialis, Dokter Umum |
| `Queue.Update` | 6 | populasi pemegang yang sama |
| `WorkSchedule.Delete` | 2 | Finance × Manajer Finance; Human Resource × Manajer HR |
| `WorkSchedule.Update` | 2 | Finance × Manajer Finance; Human Resource × Manajer HR |

Jumlah 17 itu cocok persis dengan yang sudah tercatat pada
[`07-global-registry-drift-audit.md`](07-global-registry-drift-audit.md) bagian B.1, sehingga kedua
pengukuran berbicara tentang himpunan yang sama.

`BillingItemCategory.*`, `DoctorQueue.Update`, dan `PatientProcedure.Update` **tidak** dibahas ulang
di sini; ketiganya sudah ditutup `evidence/07` dan `evidence/10`.

---

## B. Cara membaca identitas kanonik

Sejak Fase A0, identitas sebuah kemampuan adalah pasangan `(resource, action)` yang ditulis pada
`[AccessPermission]` — persis nilai yang dicari `HasAccessAsync` saat request masuk.

| Atribut | Perannya |
|---|---|
| `[AccessPermission(resource, action)]` | **Satu-satunya penegakan.** Memasang `AccessPermissionFilter` |
| `[AccessAction(...)]` | **Metadata saja.** Menentukan nama tampil, urutan, dan kolom pada layar Akses Role |
| `[Authorize]` | Hanya menuntut pengguna sudah login |

Konsekuensinya sederhana dan menentukan seluruh audit ini: **endpoint yang hanya membawa
`[Authorize]` dapat dipanggil siapa pun yang punya login.**

---

## C. Hasil per identitas

### C.1 `KioskScanSession.Cancel` — artefak seeder pra-A0

Endpoint admin `PATCH /kiosk-scan-sessions/{id}/cancel` masih ada dan masih dapat dijangkau. Ia
membawa `[AccessAction("Cancel", ...)]` tetapi `[AccessPermission("KioskScanSession", "Update")]`.

Sebelum A0, seeder mendaftarkan argumen pertama `[AccessAction]` sementara filter mencari argumen
kedua `[AccessPermission]`. `KioskScanSession.Cancel` lahir dari seeder versi lama itu dan **tidak
pernah ditegakkan siapa pun**. Identitas kanonik `KioskScanSession` hari ini adalah `Create`,
`Read`, `Update`, `Delete` — tidak ada `Cancel`.

| Kesimpulan | Nilai |
|---|---|
| Klasifikasi | **B** — normalisasi identitas, bukan kemampuan yang pensiun |
| Penerus | `KioskScanSession.Update` — method dan route yang sama |
| Catatan | `Update` lebih luas: ia juga membuka `mark-used-for-registration` |

Jalur perangkat kiosk (`kiosk/{id}/cancel`) terpisah dan dijaga policy `KioskRead`. Itu desain yang
disengaja untuk akun perangkat, bukan kelalaian.

### C.2 `Queue.Read` dan `Queue.Update` — pensiun karena dipecah, lalu pindah modul

Tidak ada satu pun `AccessPermission("Queue", ...)` di source, dan tidak ada controller yang
`ControllerName`-nya `"Queue"`. Kemampuannya sudah dipecah menjadi resource khusus:

| Resource sekarang | Action kanonik |
|---|---|
| `DoctorQueue` | `Read`, `Call`, `StartConsultation`, `FinishConsultation`, `Skip`, `NoShow`, `Requeue` |
| `NurseStationQueue` | `Read`, `Update` |
| `QueueVoice` | `Read`, `Create`, `Update` |
| `QueueDisplayRuntime` | dijaga policy perangkat `QueueDisplayRuntimeRead` |

`moduleCode: "HEALTH_SERVICE_REGISTRATION"` **tidak ada lagi** di source; seluruh resource antrean
operasional kini berada di `HEALTH_SERVICE_REGISTRATION_MANAGEMENT`. Baris `Queue.*` ganda pada dua
modul di database adalah sisa perpindahan itu.

**Tidak ada pemegang yang kehilangan jangkauan.** Diturunkan dari
[`03-be-sec-003-pre-implementation-impact.md`](03-be-sec-003-pre-implementation-impact.md)
bagian 14.1.1:

| Pemegang `Queue.*` historis | Resource penerus yang **sudah** dipegang |
|---|---|
| Keperawatan × Perawat Rawat Jalan | `NurseStationQueue.*` |
| Medis × Dokter Umum | `DoctorQueue.*`, `NurseStationQueue.*`, `QueueVoice.*` |
| Medis × Dokter Spesialis | `DoctorQueue.*` |
| Medis × Dokter IGD | `DoctorQueue.*`, `QueueVoice.*` |

| Kesimpulan | Nilai |
|---|---|
| Klasifikasi | **A/B** — pensiun karena dipecah, ditambah perpindahan modul |
| Migrasi policy | **Tidak diperlukan** |

`Queue.Update` **tidak** dipetakan ke seluruh action tulis khusus. Pemetaan semacam itu tidak
dibutuhkan justru karena setiap pemegangnya sudah memegang penerusnya secara mandiri.

### C.3 `WorkSchedule.Update` dan `WorkSchedule.Delete` — **bukan kemampuan yang pensiun**

Inilah temuan yang mengubah kesimpulan audit.

Argumen keselamatan `evidence/07` berbunyi: *setiap kunci yang ditutup adalah kemampuan yang tidak
lagi punya endpoint, dan tanpa endpoint izinnya tidak pernah ditanyakan.* Argumen itu benar untuk
C.1 dan C.2. Untuk `WorkSchedule` ia **tidak berlaku**: endpoint-nya ada, dapat dipanggil, dan
justru tidak pernah bertanya.

| Verb | Route | `AccessAction` | `AccessPermission` | Penegakan efektif |
|---|---|---|---|---|
| `GET` | `filters/metadata`, `summary`, `/`, `options` | `Read` | `WorkSchedule.Read` | Akses Role |
| `GET` | `{id:guid}` | — | — | **`[Authorize]` saja** |
| `POST` | `/` | `Create` | `WorkSchedule.Create` | Akses Role |
| `PUT` | `{id:guid}` | — | — | **`[Authorize]` saja** |
| `PATCH` | `{id:guid}/status` | — | — | **`[Authorize]` saja** |
| `DELETE` | `{id:guid}` | — | — | **`[Authorize]` saja** |

Isi method-nya dibaca seluruhnya: tidak ada pemeriksaan izin di dalam badan method. `Actor()` hanya
membaca id pengguna untuk mengisi kolom audit.

**Akibat nyatanya:** siapa pun yang punya login — termasuk petugas dari unit yang sama sekali tidak
berkepentingan — dapat mengubah, menonaktifkan, dan menghapus jadwal kerja yang menjadi dasar
perhitungan kehadiran dan penggajian.

| Kesimpulan | Nilai |
|---|---|
| Klasifikasi | **C** — regresi otorisasi pada source |
| Migrasi policy | **DITAHAN** sampai source diperbaiki |

**Asal-usulnya, supaya tidak salah dikutip.** `git log -S 'AccessPermission("WorkSchedule","Update")' --all`
tidak menghasilkan apa pun, dan seluruh riwayat berkas ini (3 commit) memperlihatkan `PUT`/`PATCH`/`DELETE`
tanpa penegakan sejak commit pertamanya. Jadi ini **cacat sejak lahir**, bukan penegakan yang dicabut
belakangan. Kondisi keamanannya tetap sama; asal-usulnya dicatat supaya remediasinya tidak dijelaskan
secara keliru.

---

## D. Cacat yang sama pada empat controller lain

Pemindaian seluruh repository memakai kriteria "endpoint tanpa `[AccessPermission]`, tanpa
`[AccessAction]`, tanpa `[AllowAnonymous]`, tanpa `[Authorize(Policy = ...)]`" menemukan lima
controller dengan pola template yang identik:

| Controller | Endpoint tanpa penegakan |
|---|---|
| `WorkScheduleController` | `GET {id}`, `PUT`, `PATCH {id}/status`, `DELETE` |
| `ShiftController` | sama |
| `ShiftGroupController` | sama |
| `ShiftPatternController` | sama |
| `WorkCalendarController` | sama |

**20 endpoint.** Kelimanya memiliki tepat 5 `[AccessPermission]` (4× `Read`, 1× `Create`) dan
seluruhnya berada di modul `HUMAN_RESOURCE_MASTER_DATA`.

---

## E. Kenapa cacat ini lolos dari seluruh audit sebelumnya

`PermissionRegistryDescriptor.BuildCore` hanya mencatat endpoint yang membawa **salah satu** dari
dua atribut. Endpoint yang tidak membawa **keduanya** jatuh ke `continue` dan hilang sepenuhnya.

`PermissionRegistryValidator` karena itu hanya memeriksa empat hal: metadata gap
(`[AccessPermission]` tanpa `[AccessAction]`), resource ganda, `AccessType` tidak sah, dan
`UnenforcedActions` (`[AccessAction]` tanpa `[AccessPermission]`).

> Tidak ada pemeriksaan untuk endpoint yang tidak membawa atribut apa pun.

Inilah sebabnya `evidence/07` dapat melaporkan **nol metadata gap** dengan jujur sementara 20
endpoint tulis berjalan tanpa penegakan: keduanya mengukur hal yang berbeda, dan yang satu tidak
pernah dirancang untuk melihat yang lain.

Titik buta ini ditutup pada `BE-SEC-012` sebagai invarian kelima, memakai jalur klasifikasi
`BuildCore` yang sama — bukan aturan `grep` terpisah di CI.

---

## F. Dampak pada `BE-SEC-003B`

| Hal | Temuan |
|---|---|
| Irisan scope | **Tidak ada.** Pilot `BE-SEC-003B` adalah enam controller klinis; `WorkSchedule` berada di `HUMAN_RESOURCE_MASTER_DATA` |
| Apakah Tahap 1/Tahap 2 memperburuk lubangnya | **Tidak.** Paparan itu murni ada di source dan tidak bergantung pada keadaan registry |
| Apakah audit ini menahan `BE-SEC-003B` | **Ya** — lihat alasan di bawah |

Dua dari lima identitas yang diaudit tidak dapat diselesaikan tanpa memperbaiki source lebih dulu,
dan kandidat beku yang akan dibawa masuk ke jendela maintenance memuat lubang otorisasi yang sudah
terkonfirmasi. Karena irisan scope-nya nol, pemilik sistem **boleh** memutuskan untuk tetap
melanjutkan Tahap 1/Tahap 2 atas dasar scope — keputusan itu milik pemilik, dan dicatat di sini
sebagai pilihan yang sadar, bukan sebagai kesimpulan audit.

**`AUTH_ORPHAN_AUDIT_BLOCKS_BE_SEC_003B`**

---

## G. Tindak lanjut

| Tindakan | Task | Status |
|---|---|---|
| Perbaikan source 20 endpoint + invarian naked endpoint | `BE-SEC-012` | Selesai — lihat [laporan](../task/report/backend/BE-SEC-012.md) |
| Keputusan pemberian `WorkSchedule.Update`/`Delete` | Pemilik modul HR | **TERBUKA** — lihat [`13-workschedule-dormant-grant-deployment-blocker.md`](13-workschedule-dormant-grant-deployment-blocker.md) |
| Keputusan `KioskScanSession.Update` bagi Perawat Rawat Inap | Pemilik modul Registration | **TERBUKA** |
| Pensiunkan policy `Queue.Read` / `Queue.Update` tanpa penggantian | Migrasi policy terpisah | **TERBUKA**, tidak mendesak |
| Delapan endpoint `WorkScheduleAssignment` tanpa penegakan | Task lanjutan, pemilik modul HR Scheduling | **TERBUKA** — tercatat pada baseline `BE-SEC-012` |

---

## Lampiran — batas audit ini

| Hal | Status |
|---|---|
| Database | **Tidak disentuh.** Seluruh angka database dikutip dari ekspor yang disediakan pemilik |
| `AccessMenuSeeder` | **Tidak dijalankan** |
| Aplikasi | **Tidak dinyalakan** |
| `SysAccessPolicy` | **Tidak diubah** |
| Integration | **Tidak di-fetch, tidak di-merge** |
| Bagian A–E | Murni pembacaan source pada kandidat beku |
