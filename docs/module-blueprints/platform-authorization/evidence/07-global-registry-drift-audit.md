# Global Registry Drift Audit — pra-jendela `BE-SEC-003`

> **Mode:** audit baca-saja. Tidak ada database write, tidak ada aplikasi yang dijalankan, tidak ada
> `AccessMenuSeeder` yang dieksekusi, tidak ada policy expansion, tidak ada commit/push.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Audit evidence** — bukan laporan task |
| Tujuan | Mengetahui **seluruh** perubahan registry yang akan dilakukan `AccessMenuSeeder` saat HEAD pertama kali start terhadap database ini — bukan hanya yang menyangkut `BE-SEC-003` |
| HEAD backend | `8131913380063aee7f2ebe2bda54e888d88b2b02` (`AndryZain`) + koreksi Fase A′ yang belum di-commit |
| Database | `QuilvianNewDevAndryZain` — ekspor 11 September 2026 |
| Tanggal | 11 September 2026 |
| Hasil | **`GLOBAL_REGISTRY_RECONCILIATION_SAFE_FOR_BE_SEC_003_WINDOW`** — dengan satu temuan non-blocking milik modul lain |

---

## A. Metode

**Sisi source — kanonik, bukan algoritma alternatif.**
`PermissionRegistryDescriptor.BuildFromAssembly(typeof(AccessPermissionService).Assembly)`
([`PermissionRegistryDescriptor.cs:136`](../../../../Services/Security/PermissionRegistryDescriptor.cs#L136)) —
metode yang sama persis dengan yang dipakai `AccessMenuSeeder` dan test invarian. Dijalankan lewat
runner baca-saja sekali pakai di direktori scratchpad yang mereferensikan
`bin/Debug/net9.0/QuilvianSystemBackend.dll`.

Runner itu **tidak menyentuh repository**: nol berkas source ditambahkan, dan artefak build-nya sudah
dihapus kembali dari `bin/`. `BuildFromAssembly` murni refleksi atas atribut — ia **tidak** menyalakan
host ASP.NET, **tidak** membuka koneksi database, dan **tidak** memanggil `ReconcileAsync`.

**Sisi database.** Ekspor `diagnose-global-registry-drift-dbeaver.sql` yang dijalankan pemilik sistem.

---

## B. Hasil pengukuran

| Sisi | Metrik | Nilai |
|---|---|---:|
| **Source** | Module | 45 |
| | Resource | 329 |
| | Action (kunci runtime) | **1.246** |
| | Fallback kompatibilitas | **69** |
| | Metadata gap | **0** |
| **Database** | `SysActionAccess` aktif | **1.076** |
| | `SysAccessPolicy` fisik / efektif | 498 / **469** |
| | Pasangan Departemen × Posisi | 11 |

Angka fallback **69** cocok persis dengan himpunan yang dikunci
`CompatibilityFallbackMatchesApprovedLegacySetExactly`, dan **0 metadata gap** berarti tidak ada
endpoint terproteksi yang kuncinya tidak terdaftar. Kedua invarian `BE-SEC-001` itu utuh di HEAD.

### B.1 Validasi kelengkapan transkripsi

Baris database yang **berstatus `OPEN` dan memegang policy efektif** berjumlah **299 kunci** dengan
total **452** policy efektif. Baris yang **sudah tertutup namun policy-nya masih hidup** menyumbang
**17** lagi (`KioskScanSession.Cancel` 1; `Queue.Read` 4+2; `Queue.Update` 4+2; `WorkSchedule.Delete` 2;
`WorkSchedule.Update` 2).

```
452 + 17 = 469
```

Sama persis dengan total policy efektif yang diukur. **Tidak ada satu pun baris pemegang hak yang
terlewat** dari himpunan yang dibandingkan.

---

## C. Klasifikasi

Perbandingan dilakukan atas **299 kunci kritis** — yaitu setiap baris registry yang hidup *dan*
memegang hak. Baris `OPEN` tanpa policy tidak dibandingkan satu per satu karena penutupannya tidak
dapat merugikan siapa pun.

| Kategori | Jumlah | Keterangan |
|---|---:|---|
| `MATCH` | **293** | Kunci hidup yang masih dideklarasikan source |
| `DB_ONLY_ACTIVE_WITH_EFFECTIVE_POLICY` | **6** | Lihat bagian D |
| `DB_ONLY_CLOSED` | — | Sudah tertutup; rekonsiliasi tidak mengubahnya |
| `SOURCE_ONLY` | **≈170 bersih** | Registry aktif 1.076 → 1.246. Kemampuan baru, lahir **tanpa** pemegang (*fail closed*) |
| `METADATA_CHANGED` — `ModuleCode` | **0** pada resource hidup | Lihat C.1 |

### C.1 Tidak ada module drift — diperiksa khusus

Identitas `SysControllerAccess` adalah pasangan **(`ModuleId`, `ResourceName`)**, bukan
`ResourceName` saja. Resource yang **pindah modul** karena itu menutup baris controller lama dan
membuat baris baru — dan seluruh policy yang menunjuk baris lama menjadi yatim. Ini jalur kerusakan
yang tidak terlihat dari nama resource saja, sehingga diperiksa terpisah.

Tiga puluh resource kritis diperiksa module-nya. **Seluruhnya cocok antara source dan baris `OPEN`
di database.**

Yang justru terlihat: perpindahan modul yang memang pernah terjadi **sudah selesai direkonsiliasi**
pada masa lalu — baris lama berstatus `CONTROLLER_CLOSED` dengan **nol** policy efektif, sementara
baris baru `OPEN` memegang policy-nya:

| Resource | Modul lama (tertutup, 0 policy) | Modul sekarang (hidup, memegang policy) |
|---|---|---|
| `CompanyGuarantor` | `HEALTH_SERVICE_MASTER_DATA` | `ADMINISTRATOR_MASTER_DATA` |
| `IdentityScannerProfile` | `HEALTH_SERVICE_MASTER_DATA` | `ADMINISTRATOR_MASTER_DATA` |
| `InsuranceProvider` | `HEALTH_SERVICE_MASTER_DATA` | `ADMINISTRATOR_MASTER_DATA` |
| `KioskDevice` | `HEALTH_SERVICE_MASTER_DATA` | `ADMINISTRATOR_MASTER_DATA` |
| `MembershipTier` | `HEALTH_SERVICE_MASTER_DATA` | `ADMINISTRATOR_MASTER_DATA` |
| `Supplier` | `HEALTH_SERVICE_MASTER_DATA` | `ADMINISTRATOR_MASTER_DATA` |
| `QueueVoiceProfile` | `HEALTH_SERVICE_REGISTRATION_MANAGEMENT` | `ADMINISTRATOR_MASTER_DATA` |
| `PatientEmergencyContact`, `PatientIdentityDocument`, `PatientMembership`, `PatientRelationship` | `HEALTH_SERVICE_PATIENT_MANAGEMENT` | `HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA` |
| `PatientEncounter`, `KioskScanSession` | `HEALTH_SERVICE_REGISTRATION` | `HEALTH_SERVICE_REGISTRATION_MANAGEMENT` |

**Tidak ada perpindahan modul yang masih menunggu.** Rekonsiliasi berikutnya tidak akan memindahkan
satu pun resource yang sedang memegang hak.

---

## D. `DB_ONLY_ACTIVE_WITH_EFFECTIVE_POLICY` — enam kunci

Inilah kategori yang ditakutkan: registry hidup, memegang hak, tetapi **tidak lagi dideklarasikan
source**, sehingga `AccessMenuSeeder` akan menutupnya.

| # | Kunci runtime | Policy efektif | Pengguna aktif | Pemegang | Pemilik |
|---|---|---:|---:|---|---|
| 1 | `BillingItemCategory.Create` | 3 | 14 | Finance × Manajer Finance; Human Resource × Manajer HR; Medis × Dokter Umum | **Billing** |
| 2 | `BillingItemCategory.Delete` | 3 | 14 | sama | **Billing** |
| 3 | `BillingItemCategory.Read` | 3 | 14 | sama | **Billing** |
| 4 | `BillingItemCategory.Update` | 3 | 14 | sama | **Billing** |
| 5 | `DoctorQueue.Update` | 3 | 6 | Medis × {Dokter Umum, Dokter Spesialis, Dokter IGD} | `BE-SEC-003` |
| 6 | `PatientProcedure.Update` | 1 | 2 | Medis × Dokter Umum | `BE-SEC-003` |

Nomor 5 dan 6 **sudah direncanakan** — keduanya identitas pensiun Fase A, dan Fase B membuat
penggantinya. Tidak ada kejutan di sana.

### D.1 `BillingItemCategory` — temuan di luar `BE-SEC-003`

Akar masalahnya terbaca langsung dari source
([`MstTariffCategory.cs:34`](../../../../Areas/HealthServices/MasterData/Models/MstTariffCategory.cs#L34)):

> *"`MstBillingItemCategory` saat kategori billing digabung ke kategori tarif."*

Kategori billing **digabungkan ke `TariffCategory`**. Controller-nya hilang dari source; tabelnya
dihapus migration `20260902072756_DropTableMstBillingCategory`.

**Apakah penutupannya merugikan? Tidak secara fungsional.** Tidak ada satu pun endpoint di HEAD yang
mendeklarasikan `BillingItemCategory.*`. `HasAccessAsync` hanya dipanggil oleh filter pada endpoint;
tanpa endpoint, izin itu **tidak pernah ditanyakan**. Kedua belas baris policy tersebut karena itu
**sudah mati secara praktis hari ini**, terlepas dari flag registry-nya. Menutup baris registry hanya
membuat kematian itu terlihat.

**Yang tetap perlu diputuskan pemilik sistem — dan ini bukan akibat seeder.** Penerus kemampuannya
adalah `TariffCategory`, yang di database hanya dipegang **Medis × Dokter Umum (1 policy, 2
pengguna)**. Dua pasangan yang dulu memegang `BillingItemCategory` — **Finance × Manajer Finance**
dan **Human Resource × Manajer HR** — **tidak memegang `TariffCategory` sama sekali**.

Jadi konsolidasi billing→tarif meninggalkan celah kepemilikan: dua jabatan kehilangan kemampuan
mengelola kategori, dan celah itu **sudah terjadi saat source digabungkan**, bukan saat seeder
berjalan. Audit ini hanya menemukannya sebelum jejaknya hilang dari layar Akses Role.

| Field | Usulan |
|---|---|
| Pemilik | Tim Billing / master data tarif |
| Pertanyaan | Apakah Finance × Manajer Finance dan Human Resource × Manajer HR seharusnya memegang `TariffCategory`? |
| Sifat | **Tidak menahan** jendela `BE-SEC-003`; tidak ada kemampuan terjangkau yang hilang karena rekonsiliasi |
| Mendesak | Catat **sebelum** seeder berjalan — sesudahnya bukti siapa yang dulu memegangnya tidak lagi tampil di layar Akses Role |

---

## E. Kesimpulan keselamatan

Pertanyaan yang menentukan bukan *"berapa baris yang ditutup seeder"*, melainkan
**"adakah Departemen × Posisi yang kehilangan kemampuan yang hari ini benar-benar dapat dijangkau?"**

Jawabannya **tidak ada**, dan alasannya struktural, bukan kebetulan:

> Setiap kunci `DB_ONLY_ACTIVE` adalah kemampuan yang **tidak lagi punya endpoint di source**.
> Tanpa endpoint, izinnya tidak pernah diperiksa. Hak itu sudah tidak dapat dipakai hari ini —
> baik registry-nya ditutup maupun tidak.

Ditambah tiga hal yang sudah terverifikasi:

- **293 dari 299** kunci hidup tetap dideklarasikan source.
- **Nol** module drift pada resource yang sedang memegang hak.
- **Nol** metadata gap, dan fallback tetap **69** sesuai himpunan terkunci.

Pertambahan **≈170** identitas baru seluruhnya lahir **tanpa pemegang**, sesuai invarian
`AccessMenuSeeder` yang tidak pernah membuat `SysAccessPolicy`. Tidak ada privilege broadening.

---

## F. Catatan untuk jendela penerapan

1. Rekonsiliasi global **aman** dijalankan di dalam jendela `BE-SEC-003` — tidak ada kerugian
   terjangkau di luar yang sudah direncanakan.
2. Jendela tetap wajib tertutup dari traffic pengguna antara startup seeder dan selesainya Fase B
   Tahap 1 — bukan karena drift global, melainkan karena 22 identitas `BE-SEC-003` baru belum
   dipegang siapa pun sampai Tahap 1 selesai.
3. Catat kepemilikan `BillingItemCategory` (bagian D.1) **sebelum** seeder berjalan.
4. Sesudah seeder, `SysActionAccess` aktif seharusnya menjadi **1.246**. Angka lain berarti ada
   sesuatu yang tidak terduga dan Fase B harus ditahan.

---

## Lampiran — batas audit ini

| Hal | Status |
|---|---|
| Snapshot source lewat `BuildFromAssembly` | **Dijalankan** — 45/329/1246/69/0 |
| Kelengkapan himpunan kritis database | **Terbukti** — 452 + 17 = 469 |
| Perbandingan 299 kunci kritis | **Terverifikasi** |
| Module drift pada resource hidup | **Terverifikasi nol** (30 resource diperiksa) |
| Baris `OPEN` tanpa policy | **Tidak dibandingkan satu per satu** — penutupannya tidak dapat merugikan siapa pun |
| `METADATA_CHANGED` selain `ModuleCode` (`AccessType`, `IsSystemOnly`, `DisplayName`) | **Tidak diaudit menyeluruh** — seeder memperbaruinya di tempat tanpa menutup baris, sehingga tidak dapat menghilangkan hak |

Tidak ada database write. Tidak ada aplikasi dijalankan. Tidak ada `AccessMenuSeeder` dieksekusi.
Tidak ada policy expansion. Tidak ada commit. Tidak ada push. Repository tidak bertambah satu pun
berkas source.
