# Bukti `BE-ACC-002` — Mekanisme hak akses badan hukum

| Field | Isi |
|---|---|
| Task | `BE-ACC-002` — Audit mekanisme hak akses badan hukum |
| Sifat | **Read-only.** Tidak ada satu baris source aplikasi yang dibuat atau diubah |
| Blueprint | `ACC-BP-001` revisi 5 `approved` |
| Kontrak yang diuji | `ACC-PERMISSION-0.1` bagian 5, aturan kedua |
| Keputusan asal | `ACC-DEC-037` — pembukuan dipisah per badan hukum |
| Gap yang ditutup | `GAP-ACC-005` |
| Backend source SHA | `ca6b7e0ef3af4454cae11709739b1f36657352e2` (branch `rizkiG`) |
| Catatan SHA | `ca6b7e0` hanya menambah 28 berkas dokumentasi blueprint. Source aplikasi identik dengan baseline `aa837d7` yang tercatat di manifest |
| Tanggal | 2 September 2026 |

---

## 1. Pertanyaan yang dijawab

Kontrak `ACC-PERMISSION-0.1` bagian 5 menuliskan satu aturan yang tidak dapat diwakili matriks
peran:

> Pengguna hanya boleh menyentuh badan hukum yang menjadi haknya.

Kontrak itu sendiri menutup bagiannya dengan kalimat yang mengikat audit ini:

> Bagaimana hak atas badan hukum diberikan kepada pengguna mengikuti mekanisme yang sudah
> berlaku di repository, dan **wajib diperiksa saat implementasi** — bukan diasumsikan.

Audit ini menjalankan pemeriksaan tersebut. Pertanyaannya persis satu:

**Bagaimana sistem yang sudah berjalan menentukan badan hukum mana yang menjadi hak seorang
pengguna?**

---

## 2. Jawaban

**Mekanisme itu tidak ada.**

Ini bukan kesimpulan "belum ditemukan". Seluruh jalur yang mungkin sudah ditelusuri satu per
satu sampai habis, dan tidak satu pun memuat hubungan antara pengguna dan badan hukum.
Rinciannya ada di bagian 4 sampai 8.

Akibatnya untuk Accounting:

| Hal | Keadaan |
|---|---|
| Pola yang dapat dipakai ulang Accounting | **Tidak ada** |
| Klasifikasi temuan | **`MISSING` — dependency keamanan platform** |
| Pemilik | Owner keamanan platform, bukan owner Accounting |
| Yang tertahan | Penegakan penyaringan `LegalEntityId`, yaitu `BE-ACC-007` sampai `BE-ACC-014` |
| Yang **tidak** tertahan | `BE-ACC-003`, `BE-ACC-004`, `BE-ACC-005` — entity dan kolom tetap boleh dibuat |

Bagian 9 menjelaskan mengapa pembuatan entity tetap aman meski penegakannya belum ada.

---

## 3. Cara kerja hak akses yang sebenarnya berlaku sekarang

Supaya temuan di bagian berikutnya dapat dinilai orang lain, alur hak akses yang nyata ditulis
lengkap lebih dahulu.

### Alur satu permintaan

```text
Permintaan masuk
   |
   +- 1. [Authorize]                    -> sudah login? kalau belum: 401
   |
   +- 2. [AccessPermission("CostCenter", "Read")]
   |        `- AccessPermissionFilter.OnAuthorizationAsync
   |              `- AccessPermissionService.HasAccessAsync(user, controller, action)
   |                    |
   |                    +- SuperAdmin?  -> lolos
   |                    |
   |                    `- cari kecocokan:
   |                         AspNetUserOrganization  (UserId -> DepartmentId + PositionId)
   |                                    X
   |                         SysAccessPolicy         (DepartmentId + PositionId -> Controller + Action)
   |                         -> ketemu dan IsAllowed? lolos : 403
   |
   `- 3. Query data dijalankan  <- TIDAK ADA penyaringan badan hukum di sini
```

Langkah 3 itulah masalahnya. Keputusan hak akses berhenti pada pertanyaan *"boleh membuka menu
ini?"* dan tidak pernah sampai pada pertanyaan *"boleh melihat baris yang mana?"*

### Berkas yang menyusun alur di atas

| Peran | Berkas | Simbol |
|---|---|---|
| Penanda modul pada controller | `Attributes/AccessControllerAttribute.cs` | `AccessControllerAttribute` |
| Penanda hak per aksi | `Attributes/AccessPermissionAttribute.cs` | `AccessPermissionAttribute` |
| Empat jenis hak | `Constants/AccessTypes.cs` | `Read`, `Create`, `Update`, `Delete` |
| Penyaring permintaan | `Filters/AccessPermissionFilter.cs` | `AccessPermissionFilter.OnAuthorizationAsync` |
| Keputusan hak akses | `Services/Security/AccessPermissionService.cs` | `HasAccessAsync`, baris 26 |

---

## 4. Temuan 1 — Kebijakan hak akses tidak punya dimensi badan hukum

Tabel kebijakan hak akses hanya memuat empat kunci, dan tidak satu pun berhubungan dengan badan
hukum.

**Berkas:** `Models/SysAccessPolicy.cs`

| Kolom | Baris | Keterangan |
|---|---:|---|
| `DepartmentId` | 13 | Departemen |
| `PositionId` | 16 | Jabatan |
| `ControllerAccessId` | 19 | Controller yang diatur |
| `ActionAccessId` | 22 | Aksi yang diatur |
| `IsAllowed` | 24 | Boleh atau tidak |

Tidak ada `LegalEntityId`, `HospitalSiteId`, maupun kolom cakupan lain.

**Artinya:** kebijakan hak akses menjawab *"jabatan X di departemen Y boleh menekan tombol Z"*.
Ia tidak punya tempat untuk menyimpan *"…dan hanya untuk badan hukum tertentu"*.

Kueri yang menegakkannya ada di `Services/Security/AccessPermissionService.cs` baris 122–133.
Ia menggabungkan `ApplicationUserOrganizations` dengan `SysAccessPolicies` hanya pada pasangan
`(DepartmentId, PositionId)`:

```csharp
from organization in _dbContext.ApplicationUserOrganizations.AsNoTracking()
join policy in _dbContext.SysAccessPolicies.AsNoTracking()
    on new { organization.DepartmentId, organization.PositionId }
    equals new { policy.DepartmentId, policy.PositionId }
```

---

## 5. Temuan 2 — Tidak ada jalur apa pun dari pengguna ke badan hukum

Enam tabel yang paling mungkin menyimpan hubungan itu diperiksa seluruh kolomnya.

| # | Berkas | Kelas | Ada `LegalEntityId`? |
|---:|---|---|:---:|
| 1 | `Models/ApplicationUser.cs` | `ApplicationUser` | **Tidak** |
| 2 | `Models/ApplicationUserOrganization.cs` | `ApplicationUserOrganization` | **Tidak** |
| 3 | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstDepartment.cs` | `MstDepartment` | **Tidak** |
| 4 | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstPosition.cs` | `MstPosition` | **Tidak** |
| 5 | `MstEmployee.cs` | `MstEmployee` | **Tidak** |
| 6 | `MstWorkforceProfile.cs` | `MstWorkforceProfile` | **Tidak** |

`ApplicationUserOrganization` adalah tabel penugasan pengguna — tempat paling wajar bagi hubungan
semacam ini. Isinya hanya `UserId`, `DepartmentId`, `PositionId`, `IsPrimary`, dan masa berlaku.

Arah sebaliknya juga diperiksa. `MstLegalEntity`
(`Areas/Corporate/HumanResource/MasterData/Organization/Models/MstLegalEntity.cs`) memiliki empat
koleksi anak — `HospitalSites`, `OrganizationUnits`, `CostCenters`, `WorkLocations` — dan
**tidak satu pun** menunjuk pengguna.

**Artinya:** rantai `Pengguna -> Departemen -> Jabatan` berhenti sebelum mencapai badan hukum.
Tidak ada jalan memutar lewat pegawai atau profil tenaga kerja, karena keduanya juga tidak
menyimpan badan hukum.

---

## 6. Temuan 3 — Token login tidak membawa badan hukum

Token JWT dibuat di `Controllers/AuthController.cs` baris 1873–1897. Isinya 20 klaim:

`user_id`, `username`, `email`, `full_name`, `user_type`, `user_type_id`, `user_code`,
`is_kiosk`, `department_id`, `position_id`, `primary_department_id`, `primary_position_id`,
`workforce_profile_id`, `employee_id`, `doctor_id`, `external_user_id`, ditambah klaim standar
JWT.

**Tidak ada klaim badan hukum.** Pencarian menyeluruh atas `legal_entity` di seluruh berkas `.cs`
menghasilkan nol kecocokan di luar folder `Migrations/`.

Ditelusuri lebih jauh, klaim yang benar-benar dipakai kode untuk **menyaring data** hanya ini:

| Klaim | Jumlah pemakaian | Dipakai untuk |
|---|---:|---|
| `user_id` | 193 | Stempel audit `CreateBy`/`UpdateBy`, dan kepemilikan baris sendiri |
| `doctor_id` | 2 | Dokter melihat datanya sendiri |
| `queue_display_device_id`, `kiosk_device_id` | 4 | Perangkat terikat pada satu unit fisik |

**Artinya:** satu-satunya cakupan data yang diturunkan dari identitas pengguna adalah "milik saya
sendiri" dan "perangkat ini". Tidak pernah ada cakupan organisasi.

---

## 7. Temuan 4 — `LegalEntityId` adalah isian dari pengirim permintaan, bukan batas kewenangan

Ini temuan yang paling penting, dan yang paling mudah disalahpahami.

`LegalEntityId` memang dipakai luas — 228 berkas `.cs` menyebutnya, 40 model menyimpannya. Tetapi
di **seluruh** pemakaian itu, nilainya datang dari pengirim permintaan, bukan dari identitas
pengguna.

### Contoh nyata: `CostCenterController`

Berkas: `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/CostCenterController.cs`

| Baris | Kode | Sifat |
|---:|---|---|
| 102 | `[FromQuery] Guid? legalEntityId` | Isian **opsional** dari URL |
| 114 | `ApplyFilter(BaseQuery(), legalEntityId, …)` | Dipakai apa adanya |
| 387 | `if (legalEntityId.HasValue && …) query = query.Where(x => x.LegalEntityId == legalEntityId.Value)` | Kalau kosong, **tidak menyaring apa pun** |
| 260 | `LegalEntityId = request.LegalEntityId` | Saat membuat data, diambil dari isian |
| 302 | `entity.LegalEntityId = request.LegalEntityId` | Saat mengubah data, ditimpa dari isian |
| 435 | `.AnyAsync(x => x.Id == request.LegalEntityId && x.IsActive && !x.IsDelete)` | Validasi hanya memastikan badan hukumnya **ada dan aktif** |

Baris 435 itu inti persoalannya. Validasi bertanya *"badan hukum ini ada?"*. Ia **tidak pernah**
bertanya *"pengguna ini berhak atas badan hukum ini?"*

### Contoh dengan angka

Misalkan ada dua badan hukum: `RS Quilvian Pusat` dan `RS Quilvian Cabang`. Seorang staf keuangan
cabang punya hak `CostCenter : Read`.

| Yang dikirim staf cabang | Yang terjadi sekarang | Yang seharusnya |
|---|---|---|
| `?legalEntityId=<Cabang>` | Data cabang tampil | Benar |
| `?legalEntityId=<Pusat>` | **Data pusat tampil** | Seharusnya ditolak dengan kode 403 |
| tanpa parameter sama sekali | **Data kedua badan hukum tampil** | Seharusnya hanya cabang |

Baris ketiga adalah perilaku bawaan — parameternya opsional, jadi menghilangkannya justru
membuka semuanya.

### Pola ini berlaku umum, bukan kekhususan satu controller

| Pemeriksaan | Hasil |
|---|---:|
| Controller menerima `legalEntityId` dari URL | 9 |
| Controller mengambil `LegalEntityId` dari isian permintaan | 17 |
| Controller menurunkan badan hukum dari identitas pengguna | **0** |

Satu-satunya tempat identitas pengguna dibaca di area Corporate adalah
`LegalEntityController.CurrentUserId()` baris 494–498, dan hasilnya hanya dipakai sebagai stempel
audit `CreateBy`/`UpdateBy` pada baris 243, 293, 330, dan 364 — bukan untuk menentukan
kewenangan.

---

## 8. Temuan 5 — Tidak ada penyaringan tingkat baris di lapisan database

Entity Framework menyediakan `HasQueryFilter`, yaitu penyaring yang menempel otomatis pada setiap
kueri sebuah tabel. Kalau mekanisme itu dipakai, penyaringan badan hukum bisa saja terjadi
diam-diam di lapisan bawah tanpa terlihat di controller.

Pencarian `HasQueryFilter` di seluruh berkas `.cs` repository: **nol kecocokan.**

**Artinya:** tidak ada lapisan tersembunyi yang menyelamatkan keadaan. Apa yang terlihat di
controller memang seluruh yang terjadi.

---

## 9. Mengapa `BE-ACC-003` tetap aman dikerjakan

Perlu dibedakan dua hal yang mudah tercampur:

| Hal | Keadaan | Terhalang temuan ini? |
|---|---|:---:|
| **Menyimpan** `LegalEntityId` sebagai kolom pada tabel Accounting | Sudah diputuskan `ACC-DEC-037`, sudah ada di kamus data | **Tidak** |
| **Menegakkan** bahwa pengguna hanya menyentuh badan hukum haknya | Belum ada mekanismenya | **Ya** |

`BE-ACC-003` sampai `BE-ACC-005` hanya melakukan hal pertama. Kolom `LegalEntityId` pada
`AccChartOfAccount` dan kawan-kawannya tetap benar dan tetap dibutuhkan — justru kolom itulah
yang nanti menjadi sasaran penegakan begitu mekanismenya turun.

Yang tertahan adalah `BE-ACC-007` ke atas, yaitu saat endpoint pertama dibuat. Di titik itu
pertanyaan "pengguna ini boleh melihat badan hukum yang mana" tidak lagi dapat dihindari.

---

## 10. Yang dibutuhkan Accounting dari owner keamanan platform

Ditulis sebagai kebutuhan, **bukan** sebagai rancangan. Merancangnya adalah wewenang owner
keamanan platform, dan Accounting tidak boleh mendahuluinya.

| # | Kebutuhan | Alasan |
|---:|---|---|
| 1 | Satu cara resmi menyatakan badan hukum mana yang menjadi hak seorang pengguna | Sekarang tidak ada tempat menyimpannya |
| 2 | Cara membacanya saat permintaan diproses | Sekarang token tidak membawanya |
| 3 | Ketentuan untuk pengguna berhak atas lebih dari satu badan hukum | Umum terjadi pada grup rumah sakit |
| 4 | Ketentuan untuk `SuperAdmin` | Sekarang `SuperAdmin` melewati seluruh pemeriksaan |
| 5 | Perilaku saat parameter badan hukum tidak dikirim | Sekarang bawaannya "tampilkan semua" — kebalikan dari yang aman |

Butir 5 perlu ditegaskan. Bawaan yang aman adalah **menyaring ke hak pengguna**, bukan membuka
semuanya. Selama bawaannya belum diubah, menambahkan penyaringan di Accounting saja tidak
menyelesaikan apa pun, karena data yang sama tetap terbuka lewat endpoint modul lain.

---

## 11. Dampak ke luar Accounting

Temuan ini **tidak** terbatas pada Accounting. Ia berlaku pada seluruh 40 model yang menyimpan
`LegalEntityId`, sebagian besar milik Human Resource — termasuk struktur gaji
(`MstSalaryStructure`), periode penggajian (`MstPayrollPeriod`), dan klaim biaya
(`TrxExpenseClaim`).

Accounting hanya modul yang kebetulan menemukannya lebih dahulu, karena `ACC-DEC-037` memaksa
pertanyaannya diajukan secara eksplisit.

Ini disampaikan sebagai laporan, bukan sebagai klaim tingkat keparahan. Menilai risikonya dan
menentukan penanganannya adalah wewenang owner keamanan platform.

---

## 12. Cara memeriksa ulang temuan ini

Seluruh perintah bersifat baca saja dan dijalankan dari akar `NewQuilvianSystemBackend`.

```bash
# Temuan 1 — kebijakan hak akses tanpa dimensi badan hukum
grep -n "public " Models/SysAccessPolicy.cs

# Temuan 2 — tidak ada jalur pengguna ke badan hukum
grep -n "LegalEntity" Models/ApplicationUser.cs Models/ApplicationUserOrganization.cs

# Temuan 3 — token tidak membawa badan hukum
grep -rn "legal_entity" --include=*.cs . | grep -v obj | grep -v Migrations

# Temuan 4 — badan hukum datang dari pengirim permintaan
grep -n "legalEntityId\|LegalEntityId" \
  Areas/Corporate/HumanResource/MasterData/Organization/Controllers/CostCenterController.cs

# Temuan 5 — tidak ada penyaringan tingkat baris
grep -rn "HasQueryFilter" --include=*.cs . | grep -v obj
```

Perintah ketiga dan kelima menghasilkan keluaran kosong. Kosong di sini **adalah** buktinya.

---

## 13. Kesimpulan

| Pertanyaan | Jawaban |
|---|---|
| Mekanismenya ada? | **Tidak** |
| Ada pola yang dapat dipakai ulang Accounting? | **Tidak ada** |
| Klasifikasi | `MISSING` — dependency keamanan platform |
| Pemilik | Owner keamanan platform |
| `GAP-ACC-005` | **Tertutup sebagai audit.** Pertanyaannya terjawab; jawabannya "belum ada" |
| Dependency baru | `ACC-DEP-008`, dicatat di [05-prerequisite-readiness.md](../05-prerequisite-readiness.md) |
| Task yang tertahan | `BE-ACC-007` sampai `BE-ACC-014` |
| Task yang tetap jalan | `BE-ACC-003`, `BE-ACC-004`, `BE-ACC-005` |

Accounting **tidak** membuat mekanismenya sendiri. Membuat penyaringan khusus Accounting akan
menghasilkan cara kedua yang berbeda dari cara platform nanti, dan menutupi persoalan yang
sebenarnya berlaku sistem-luas.
