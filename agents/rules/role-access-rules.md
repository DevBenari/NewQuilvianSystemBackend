# Aturan Hak Akses (Role Access)

| Field | Nilai |
| --- | --- |
| Status | Mengikat untuk setiap endpoint backend yang dibuat atau disentuh |
| Layar pemilik | Pengaturan → Manajemen Role → **Akses Role** |
| Diverifikasi terhadap | `Attributes/`, `Filters/AccessPermissionFilter.cs`, `Services/Security/AccessPermissionService.cs`, `Seeders/AccessMenuSeeder.cs`, `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` |
| Presedensi | `AGENTS.md` > `docs/engineering/` > `agents/rules/` > dokumen ini |

Dua aturan pokok, dan sisanya hanya penjelasan cara memenuhinya:

1. **Hak akses ditentukan admin lewat layar Akses Role, bukan oleh kode.** Kode mendeklarasikan
   kemampuan apa yang ada; kode tidak pernah memutuskan siapa yang boleh memakainya.
2. **Dilarang hardcode role access.** Tidak ada nama peran, nama departemen, atau nama posisi
   yang ditulis di dalam kode sebagai penentu boleh atau tidak boleh.

---

## 1. Cara kerja yang harus dipahami dulu

```text
[AccessController] + [AccessAction]  ──(refleksi saat startup)──►  SysControllerAccess / SysActionAccess
                                                                        │
                                        layar Akses Role membaca daftar ini
                                                                        ▼
                                          admin mencentang  ──►  SysAccessPolicy (Departemen × Posisi)
                                                                        │
[AccessPermission] ──► AccessPermissionFilter ──► AccessPermissionService.HasAccessAsync
```

Yang menentukan akses akhirnya adalah baris `SysAccessPolicy` yang dibuat admin lewat layar
Akses Role, dipasangkan dengan penempatan pengguna pada `ApplicationUserOrganization`
(departemen dan posisi, beserta masa berlakunya).

Konsekuensinya: **sebuah kemampuan hanya bisa diberikan bila ia muncul di layar Akses Role.**
Endpoint yang permission-nya tidak terdaftar tidak bisa diberikan kepada siapa pun, dan akan
menolak semua orang dengan 403 selamanya.

---

## 2. Tiga atribut wajib pada setiap endpoint

| Atribut | Letak | Fungsi |
| --- | --- | --- |
| `[Authorize]` | Class | Menuntut pengguna sudah login |
| `[AccessController(...)]` | Class | Mendaftarkan controller sebagai menu di layar Akses Role |
| `[AccessAction(...)]` | Setiap action | Mendaftarkan kemampuan sebagai baris yang bisa dicentang |
| `[AccessPermission(...)]` | Setiap action | Menegakkan pemeriksaan saat request masuk |

Endpoint tanpa `[AccessPermission]` hanya terlindungi `[Authorize]`. Artinya **siapa pun yang
punya login** bisa memanggilnya, termasuk petugas dari unit yang sama sekali tidak berkepentingan.
Itu bukan pengamanan.

Satu-satunya pengecualian adalah endpoint yang memang sengaja `[AllowAnonymous]`, misalnya login
perangkat kiosk. Pengecualian itu wajib disebut alasannya di laporan task.

---

## 3. Kontrak penamaan yang mengikat

Ini bagian yang paling mudah salah dan paling mahal akibatnya.

| Yang ditulis | Harus sama persis dengan |
| --- | --- |
| Argumen ke-1 `[AccessPermission]` | Nilai `ControllerName` pada `[AccessController]` |
| Argumen ke-2 `[AccessPermission]` | Argumen ke-1 `[AccessAction]` pada **method yang sama** |

Alasannya ada di kode. Seeder menyimpan `SysActionAccess.ActionName = attribute.ActionName`,
yaitu argumen pertama `[AccessAction]`. Sementara filter mencari baris registry memakai argumen
kedua `[AccessPermission]`. Bila keduanya berbeda, pencarian tidak menemukan apa pun,
`HasAccessAsync` mengembalikan `false`, dan hasilnya **403 permanen yang tidak bisa diperbaiki
dari layar Akses Role** — karena baris untuk dicentang itu memang tidak pernah dibuat.

Contoh salah yang benar-benar ada di source, pada `LabSpecimenController.cs`:

```csharp
[AccessAction("Update", "Collect Lab Specimen", ..., AccessType = AccessTypes.Update)]
[AccessPermission("LabSpecimen", "Collect")]   // ← mencari "Collect", registry berisi "Update"
```

Bentuk yang benar — nama action sama di kedua atribut:

```csharp
[AccessAction("Update", "Collect Lab Specimen", ..., AccessType = AccessTypes.Update)]
[AccessPermission("LabSpecimen", "Update")]
```

Cara memeriksa sebelum menyatakan task selesai: untuk setiap action, sandingkan ketiga nilai itu
dan pastikan cocok huruf demi huruf. Kesalahan ini tidak terlihat saat pengujian memakai akun
SuperAdmin, karena SuperAdmin melewati seluruh pemeriksaan.

---

## 4. `AccessType` hanya boleh empat nilai

Layar Akses Role menampilkan matriks berkolom **Read, Create, Update, Delete**, ditambah tombol
Full Access sebagai pintasan. Daftar aksi yang ditampilkan disaring dengan
`AccessTypes.AllowedForRoleAccess`, yang isinya persis keempat nilai itu.

| Aturan | Konsekuensi bila dilanggar |
| --- | --- |
| `AccessType` wajib `AccessTypes.Read`, `Create`, `Update`, atau `Delete` | Nilai lain membuat kemampuan tidak muncul di layar, jadi tidak bisa diberikan |
| `VisibleInRoleAccess` dibiarkan `true` | Disetel `false` menyembunyikannya dari admin |
| `IsSystemOnly` dibiarkan `false` | Disetel `true` membuatnya hanya bisa dipakai SuperAdmin |

`IsSystemOnly = true` adalah pintu darurat untuk kemampuan yang memang tidak boleh diberikan ke
siapa pun selain SuperAdmin, misalnya perawatan sistem. Jangan memakainya untuk menghindari
kerepotan mendaftarkan permission.

Bila sebuah aksi terasa tidak muat ke dalam empat kolom itu — misalnya "menyetujui" berbeda
kewenangan dari "menyunting" — jangan mengarang `AccessType` baru. Naikkan sebagai pertanyaan
terbuka, karena menambah kolom berarti mengubah layar Akses Role, dan itu keputusan pemilik
sistem.

---

## 5. Larangan hardcode

Yang **tidak boleh** ditulis di dalam kode sebagai penentu kewenangan:

| Anti-pola | Contoh | Kenapa dilarang |
| --- | --- | --- |
| Memeriksa nama peran | `User.IsInRole("SuperAdmin")` | Nama peran di rumah sakit bisa berbeda; admin tidak bisa mengubahnya tanpa deploy ulang |
| Daftar nama peran di kode | `string[] { "Supervisor", "KepalaRuangan" }` | Sama seperti di atas, dan diam-diam menolak petugas yang sah |
| Nama departemen atau posisi sebagai teks | `if (department == "Farmasi")` | Departemen dan posisi adalah master data, bukan konstanta |
| Menyimpulkan kewenangan dari `UserType` | `if (user.UserType == 1)` | Melewati seluruh matriks Akses Role |
| Menambah pemeriksaan sendiri di controller | `if (!IsAllowedManually(user)) return Forbid();` | Kewenangan jadi punya dua sumber yang bisa saling menyimpang |

Contoh nyata anti-pola ini ada di
`Areas/HealthServices/InPatientManagement/Helpers/InpatientActorClaims.cs`, yang menyimpan
`SupervisorOrWardHeadRoles`, `SupervisorRoles`, dan `CashierOrBillingRoles` sebagai daftar teks
tetap. Komentarnya sendiri sudah mengakui itu asumsi dan mencatatnya sebagai risiko terbuka:
bila nama peran kasir di rumah sakit berbeda, penandaan kelayakan keuangan ditolak 403 untuk
petugas yang sesungguhnya berwenang — dan karena kelayakan keuangan menggerbang penutupan
episode, pasien ikut tertahan.

**Yang benar:** setiap kewenangan baru dideklarasikan sebagai `[AccessAction]` supaya muncul di
layar Akses Role, lalu ditegakkan lewat `[AccessPermission]`. Admin yang memutuskan departemen
dan posisi mana yang mendapatkannya.

Menemukan hardcode pada kode lama **tidak** dengan sendirinya memberi wewenang merapikannya.
Catat sebagai temuan di laporan task, karena menggantinya mengubah siapa yang bisa memakai
fitur dan itu menuntut keputusan pemilik proses.

---

## 6. Yang tetap diputuskan kode, dan bukan hardcode

Jangan salah paham: tidak semua pemeriksaan adalah role access. Tiga hal berikut memang tugas
kode, dan tetap wajib ada **di samping** permission.

| Pemeriksaan | Contoh | Kenapa bukan permission |
| --- | --- | --- |
| Kepemilikan data | Pegawai hanya boleh melihat pengajuan cutinya sendiri | Diturunkan dari pengguna yang sedang login, bukan dari nama peran |
| Kelayakan status | Episode yang sudah ditutup tidak bisa dibatalkan | Aturan bisnis, berlaku untuk semua orang termasuk yang berwenang |
| Kewenangan yang melekat pada datanya | Hanya dokter penanggung jawab episode itu yang boleh menandatangani | Diturunkan dari relasi data, bukan dari daftar peran |

Ketiganya diturunkan dari data, bukan dari teks yang ditulis di kode. Itulah pembedanya.

Punya permission tidak berarti boleh melakukannya pada data tertentu. Permission menjawab
"boleh memakai fitur ini", ketiga pemeriksaan di atas menjawab "boleh pada data yang ini".

---

## 7. Checklist sebelum task dianggap selesai

1. Setiap action punya `[AccessAction]` dan `[AccessPermission]`; controller punya `[Authorize]`
   dan `[AccessController]`.
2. Argumen ke-1 `[AccessPermission]` sama persis dengan `ControllerName` pada
   `[AccessController]`.
3. Argumen ke-2 `[AccessPermission]` sama persis dengan argumen ke-1 `[AccessAction]` pada
   method yang sama.
4. `AccessType` memakai salah satu dari `AccessTypes.Read`, `Create`, `Update`, `Delete`.
5. `VisibleInRoleAccess` tetap `true` dan `IsSystemOnly` tetap `false`, kecuali ada alasan yang
   ditulis di laporan.
6. Tidak ada `IsInRole`, daftar nama peran, nama departemen, nama posisi, atau `UserType` yang
   dipakai untuk menentukan kewenangan.
7. Kepemilikan data, kelayakan status, dan kewenangan yang melekat pada data tetap diperiksa di
   backend.
8. Kemampuan baru sudah dipastikan muncul di layar Akses Role, sehingga admin benar-benar bisa
   memberikannya. Bila tidak muncul, permission-nya belum selesai.
9. Endpoint `[AllowAnonymous]` disebut satu per satu di laporan beserta alasannya.
10. Hardcode yang ditemukan pada kode lama dicatat sebagai temuan, bukan diperbaiki tanpa
    wewenang.
