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
