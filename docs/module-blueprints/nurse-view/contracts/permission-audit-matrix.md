# Permission & Audit Matrix — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.0` · status `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-10-28 |
| Traceability | `NV-DEC-009`, `016`, `022`; `NV-AC-015`, `016`, `018`, `022` |

Pemetaan endpoint ke hak akses ada di kolom **Hak akses** pada `contracts/api-contract.md` dan
tidak diulang di sini. String atribut diturunkan: `[AccessPermission("<Resource>", "<Action>")]`.
Logger mengikuti konvensi: `GET` tidak dicatat, selain `GET` dicatat.

## 1. Cara kerja hak akses di repository ini

| Lapisan | Dipakai | Rujukan |
|---|---|---|
| Akun manusia | `[Authorize]` + `[AccessController(moduleCode: "HEALTH_SERVICE_NURSE_VIEW", moduleName: "Health Service Nurse View", ...)]` di kelas; `[AccessAction]` + `[AccessPermission]` di setiap endpoint | Pola `NursingShiftController.cs` |
| Akun perangkat | `[Authorize(Policy = NurseViewDisplayRead)]`, tanpa `[AccessPermission]`; policy terdaftar di `ApprovedAlternativeAuthorization` | Pola `QueueDisplayRuntimeController.cs`, `Constants/AuthorizationPolicies.cs` |
| Pemberian hak ke peran | Layar Akses Role | POST role-access **menimpa** seluruh policy peran; tambahkan butir, jangan ganti |

Resource baru: `NurseViewDisplayDevice` (`Read`, `Create`, `Update`) dan
`NurseViewVitalSignInterval` (`Read`, `Update`).

## 2. Peta peran ke butir hak akses

| Peran | `NurseViewDisplayDevice` | `NurseViewVitalSignInterval` | Papan TV |
|---|---|---|---|
| Admin IT / Administrator | Read, Create, Update | Read, Update | Tidak (akun manusia ditolak policy perangkat) |
| Kepala ruang, perawat | — | Read (opsional, agar bisa melihat angka yang berlaku) | Hanya menonton TV |
| Komite Keperawatan | — | — (mengesahkan di luar sistem; angka dimasukkan admin) | — |
| Akun perangkat Nurse View | — | — | `GET current`, `GET board`, `JoinNurseViewFloor` |

## 3. Kewenangan yang tidak dijaga mesin hak akses

| Penjaga aturan bisnis | Yang dijaga | Yang **tidak** dijaganya | Risiko |
|---|---|---|---|
| Lantai diambil dari perangkat (`NvwDeviceContextService`) | Perangkat lantai 3 tidak bisa membaca lantai 4 (`NV-AC-015`) | Admin yang mengubah lantai perangkat ke lantai lain | Rendah; perubahan tercatat log dan `UpdateBy` |
| Pemeriksaan status setiap permintaan | Perangkat `Inactive` langsung ditolak | Token yang dicuri dan dipakai sebelum perangkat dinonaktifkan | Sedang; mitigasi: nonaktifkan memperbarui security stamp + kunci akun |
| Penyamaran di backend | Nama lengkap, No. RM, alergen tidak keluar | Pengunjung yang mengenali pasien dari inisial + nomor bed | Diterima owner (`NV-DEC-008`) |
| Rujukan pengesahan wajib | Angka interval tidak masuk tanpa nomor SK | Kebenaran isi SK | Tanggung jawab admin & Komite |

## 4. Audit

| Kejadian | Jejak tahan lama |
|---|---|
| Daftar/ubah perangkat | `IdentityModel` + `LoggerService` (`EntityId`, `DeviceCode`, aksi) |
| Buat/atur ulang login | `LoginIssuedAt`, `LoginIssuedBy` + log. **Kata sandi tidak pernah dicatat** |
| Nonaktifkan | `DeactivatedAt`, `DeactivatedBy`, `DeactivationReason` + log |
| Ganti interval | Baris lama ditandai `IsDelete` (dengan `DeleteBy`), baris baru dengan `CreateBy`, `RatificationReference`, `RatifiedOn` + log |
| Perangkat membaca papan | Tidak dicatat (GET); hanya `LastOnlineAt` |

## 5. Kolom sensitif dan masa simpan

| Kolom | Perlakuan |
|---|---|
| `MstPatient.FullName`, `BirthDate` | Dibaca di memori, tidak keluar dari service; **MUST NOT** masuk log |
| `NvwDisplayDeviceLoginResponse.Password` | Hanya di response pembuatan/atur ulang login; tidak disimpan, tidak dicatat |
| Nilai TTV di kartu | Tampil di TV (`NV-DEC-007`); tidak masuk log dan tidak masuk sinyal realtime |

Tidak ada data baru yang menuntut masa simpan khusus; data perangkat dan interval disimpan
selama sistem berjalan.

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.2.0` · status `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-11-05 |
| Traceability | `NV-DEC-034`, `035`, `040`, `041`, `043`, `044`, `049`, `053`; `AD-02`, `AD-06` |

Endpoint tetap dipetakan di kolom **Hak akses** `contracts/api-contract.md` R2 dan tidak diulang.

### 6. Cara kerja hak akses — tambahan

| Lapisan | Dipakai | Rujukan |
|---|---|---|
| Akun manusia | `[AccessController]` modul `HEALTH_SERVICE_NURSE_VIEW` yang sama; `[AccessAction]` + `[AccessPermission]` di setiap endpoint. Baris Akses Role terbit otomatis lewat `AccessMenuSeeder` | `Seeders/AccessMenuSeeder.cs`, `Services/Security/PermissionRegistryValidator.cs` |
| Akun tablet | `[Authorize(Policy = NurseViewBedTabletAccess)]`, tanpa `[AccessPermission]`; policy wajib masuk `ApprovedAlternativeAuthorization` karena `KnownUnenforcedBusinessEndpoints` kosong | `Constants/AuthorizationPolicies.cs` |
| Hub konsol | `HasAccessAsync(user, "NurseViewPatientCall", "Read")` di `JoinNurseViewCallConsole` | Pola `QueueHub.JoinAdmissionQueueBoard` |

Resource baru: `NurseViewBedTablet` (`Read`, `Create`, `Update`), `NurseViewPatientCall`
(`Read`, `Update`), `NurseViewPatientCallWaitThreshold` (`Read`, `Update`).

### 7. Peta peran ke butir hak akses — tambahan

| Peran | `NurseViewBedTablet` | `NurseViewPatientCall` | `NurseViewPatientCallWaitThreshold` | Runtime |
|---|---|---|---|---|
| Admin IT / Administrator | Read, Create, Update | — | Read, Update | Tidak |
| Perawat rawat inap | — | Read, Update | — | — |
| Kepala ruang | — | Read, Update | Read (opsional) | — |
| Peran pantau-saja (bila dibuat) | — | Read (`AD-06`) | — | — |
| Komite Keperawatan | — | — | — (mengesahkan di luar sistem) | — |
| Akun tablet bed | — | — | — | `bed-tablet-runtime/*`, `JoinNurseViewBedTablet` |
| Akun TV Nurse View | — | — | — | `display-runtime/active-calls` (tambahan), `JoinNurseViewFloor` |

### 8. Kewenangan yang tidak dijaga mesin hak akses — tambahan

| Penjaga aturan bisnis | Yang dijaga | Yang **tidak** dijaganya | Risiko |
|---|---|---|---|
| Bed diambil dari tablet (`NvwBedTabletContextService`) | Tablet 305-A tidak bisa memanggil atas nama bed lain | — | Rendah |
| Pemisahan klaim TV dan tablet | Tablet tidak bisa membaca papan lantai; TV tidak bisa membuat panggilan | — | Tinggi bila keliru; diuji silang `VAL-NV-14` (tablet membuka papan) dan `VAL-NV-26` (TV membuka runtime tablet) |
| Tekan-tahan 3 detik "Perawat hadir" (`NV-DEC-041`) | Sentuhan tak sengaja | **Siapa** yang hadir. Orang yang sengaja menahan tombol bisa menutup panggilan | Diterima owner; dampaknya setara pasien membatalkan, bel kabel tetap ada. Laporan wajib menyebut batas ini |
| Lepas hanya oleh pengklaim | Perawat lain tidak bisa menghapus klaim tanpa jejak | Ambil alih tetap boleh oleh pemegang izin mana pun | Diterima (`NV-DEC-040`); jejak mencatat keduanya |
| Alasan wajib "Tutup panggilan" | Penutupan dari konsol selalu beralasan dan bernama | Kebenaran alasan; perawat bisa menutup tanpa datang | Diterima (`NV-DEC-053`); ditandai "tidak dikonfirmasi di bed" |
| Izin konsol tanpa roster (`NV-DEC-043`) | Konsol tetap jalan walau roster kosong | Perawat lantai lain bisa mengklaim lantai ini | Diterima; nama lantai tampil jelas di konsol |

### 9. Audit — tambahan

| Kejadian | Jejak tahan lama |
|---|---|
| Daftar, ubah, buat/atur ulang login, nonaktifkan tablet | `IdentityModel`, kolom login dan nonaktif pada `NvwBedTablet` (`G-17`) + `LoggerService`. **Kata sandi tidak pernah dicatat** |
| Tekan, tekan ulang, naik Darurat | `NvwPatientCallEvent` (`ActorKind = BedTablet`, `IdempotencyKey`) |
| Klaim, ambil alih, lepas | `NvwPatientCallEvent` (`ActorUserId`, `PreviousClaimerUserId`) + log |
| Perawat hadir | `NvwPatientCallEvent` `NurseArrived`; panggilan `IsBedsideConfirmed = true`, `ClosedByUserId` = pengklaim saat itu |
| Tutup dari konsol | `NvwPatientCallEvent` `ClosedFromConsole` dengan alasan; `IsBedsideConfirmed = false` |
| Batal oleh pasien | `NvwPatientCallEvent` `CancelledByPatient` |
| Ditutup sistem | `NvwPatientCallEvent` `ClosedBySystem`, alasan pindah atau pulang |
| Ganti ambang | Pola interval TTV: baris lama `IsDelete` + `DeleteBy`, baris baru `CreateBy` + rujukan pengesahan + log |
| Pembacaan tablet, konsol, strip | Tidak dicatat (GET); hanya `LastOnlineAt` tablet |

Pesan log ditulis lengkap di **teks pesan** (`EntityId`, `BedCode`, aksi, status), karena argumen
`data` `LoggerService` tidak tersimpan.

### 10. Kolom sensitif dan masa simpan — tambahan

| Kolom | Perlakuan |
|---|---|
| `NvwPatientCall.CloseNote`, `NvwPatientCallEvent.Note` | Bertanda **Sensitif**: bisa memuat keterangan kondisi pasien. Tampil hanya di konsol bagi pemegang izin; **MUST NOT** masuk log maupun sinyal realtime |
| `NvwBedTabletLoginResponse.Password` | Sama dengan perangkat display: hanya di response, tidak disimpan, tidak dicatat |
| Inisial di konsol | Dibaca dari `MstPatient` di memori dan disamarkan di backend (`NV-DEC-044`) |
| Masa simpan data panggilan | Tidak ada penghapusan otomatis (`G-10`). Bila kelak ada kebijakan masa simpan, penghapusan dirancang sebagai task tersendiri |
