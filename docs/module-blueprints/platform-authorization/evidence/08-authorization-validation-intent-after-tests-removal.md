# Intent Validasi Otorisasi yang Hilang Saat Tests Dihapus

> **Mode:** catatan pelestarian. Ditulis **sebelum** merge `origin/QuilvianIntegrationBackend`
> menghapus seluruh backend test project, supaya invarian yang dijaga `BE-SEC-001` sampai
> `BE-SEC-003` tidak lenyap tanpa jejak.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Preservation evidence** — bukan laporan task |
| Sebab | `origin/QuilvianIntegrationBackend` menghapus **193 berkas test** (71.831 baris) secara sengaja |
| Ditulis pada | `AndryZain` @ `40b6636a`, sebelum merge |
| Tanggal | 11 September 2026 |
| Status invarian | **Tidak lagi dieksekusi otomatis** setelah merge. Isinya tetap berlaku sebagai kontrak |

---

## A. Kenapa dokumen ini ada

Menghapus test **tidak** menghapus aturan yang dijaganya. Enam berkas di bawah memuat **73 invarian**
otorisasi — sebagian di antaranya adalah satu-satunya hal yang membuktikan bahwa
`AccessMenuSeeder` tidak diam-diam memberi hak, bahwa registry usang tidak lagi mengotorisasi, dan
bahwa daftar kewenangan yang dikirim ke frontend tidak lebih longgar daripada penjaganya.

Bila intent-nya tidak dicatat sekarang, yang tersisa sesudah merge hanyalah kode tanpa alasan, dan
pelanggaran berikutnya tidak akan tertangkap oleh apa pun.

**Dokumen ini bukan pengganti validasi.** Ia daftar tuntutan yang harus dipenuhi ulang oleh strategi
validasi baru — lihat bagian G.

---

## B. `PermissionSplitPreparationTests` — invarian pemecahan `BE-SEC-003`

Paling penting bagi Fase B, karena inilah yang mengunci himpunan identitas.

| # | Invarian | Tuntutan |
|---|---|---|
| 1 | `EveryNewSplitIdentityIsRegistered` | Seluruh **23** identitas hasil pemecahan terdaftar di registry, sehingga dapat dicentang admin. Jumlahnya dikunci sebagai angka, bukan sekadar "ada" |
| 2 | `EveryNewSplitIdentityGuardsExactlyOneEndpoint` | Setiap identitas baru menjaga **tepat satu** endpoint. Bila satu identitas menjaga dua endpoint, pemecahannya belum benar-benar memisahkan kemampuan |
| 3 | `NoResourceIsDeclaredInMoreThanOneModule` | Satu resource tidak boleh terdaftar pada lebih dari satu modul — aturan kanonik `BE-SEC-001` nomor 5 |
| 4 | `SplitEndpointsUseTheirNewIdentity` | Pemetaan endpoint → identitas diperiksa **langsung dari atribut pada method**, bukan dari snapshot, supaya salah pasang tidak tertutupi endpoint saudaranya |
| 5 | `AccessPermissionActionMatchesAccessActionName` | Argumen ke-2 `[AccessPermission]` sama persis dengan argumen ke-1 `[AccessAction]`. Pelanggarannya menghasilkan **403 permanen yang tidak dapat diperbaiki dari layar Akses Role** |
| 6 | `SurvivingLegacyIdentitiesRemainDeclared` | Lima identitas lama yang bertahan tetap dideklarasikan, sehingga policy lamanya tidak menjadi yatim |
| 7 | `RetiredLegacyIdentitiesAreClosedSoftlyAndKeepTheirPolicies` | Identitas pensiun ditutup **lifecycle**, bukan hard delete, dan baris policy-nya tetap utuh sebagai jaring rollback |
| 8 | `SplitPreparationNeverCreatesAccessPolicy` | Fase A **tidak memindahkan satu pun hak** |
| 9 | `SensitiveSplitCapabilitiesAreRegisteredButNeverGranted` | Kemampuan sensitif (`Approve`, `Execute`, `Cancel`, `WriteSoap`, `Complete`, `FinishConsultation`) terdaftar tetapi **nol** pemegang — *fail closed* |

## C. `AccessPermissionEnforcementTests` — penegakan runtime

32 invarian. Yang mengikat Fase B:

| Kelompok | Tuntutan |
|---|---|
| **Saklar pengembangan** | `Security:Authorization:Enabled=false` meloloskan pengguna **hanya di luar produksi**, **tetap menolak di produksi**, dan **tidak pernah** meloloskan yang belum login |
| **Kelayakan penempatan** | Menolak penempatan kedaluwarsa, pengguna nonaktif, principal tanpa login, dan policy `IsAllowed=false` |
| **SuperAdmin** | Tiga cabang: bypass saat kebijakan klinis tidak ditegakkan; jatuh ke policy normal bila aksinya bukan system-only; tetap boleh bila system-only |
| **Pemisahan kewenangan nyata** | Kasir tanpa peran finance tidak dapat approve write-off; kasir biasa tidak dapat reopen shift; kepala kasir bisa **hanya bila diberikan eksplisit**; petty cash approve/top-up/read masing-masing terpisah |
| **Filter** | `401` bila belum login, `403` bila login tetapi tidak berwenang, lolos bila berwenang |
| **Paritas daftar kewenangan** | **10 invarian.** `GetEffectivePermissionsAsync` wajib menjawab **sama persis** dengan `HasAccessAsync`: menolak policy tidak diizinkan, controller/action nonaktif, pasangan system-only bagi pengguna biasa, penempatan **kedaluwarsa**, penempatan **dibatalkan**, principal tanpa login, pengguna nonaktif; SuperAdmin mendapat seluruh pasangan terdaftar; pasangan yang dipegang lewat dua penempatan **tidak digandakan** |

> `GetEffectivePermissions_ExcludesCancelledOrganizationAssignment` adalah test yang ditambahkan
> Fase A′ untuk menutup cacat paritas `IsCancel`. Ia belum pernah dieksekusi — proyek test sudah
> tidak dapat dikompilasi sebelum merge, dan sesudah merge berkasnya hilang.

## D. `CanonicalSecurityContractTests` — kontrak identitas

| Invarian | Tuntutan |
|---|---|
| `AuthorizationIdentityAlwaysComesFromAccessPermission` | Tidak ada sumber identitas kedua yang menyelinap dari argumen pertama `[AccessAction]` |
| `CompatibilityFallbackMatchesApprovedLegacySetExactly` | Himpunan fallback dibandingkan sebagai **himpunan**, bukan jumlah — supaya pertukaran diam-diam tertangkap. Nilai terkunci: **69** |
| `NoProtectedEndpointIsLeftUnregisterable` | Nol endpoint terproteksi yang kuncinya tidak terdaftar |
| `CompatibilityFallbackDoesNotCreateAmbiguousIdentity` | Fallback tidak melahirkan identitas ganda |
| `CompatibilityFallbackDoesNotGrantAnything` | Fallback tidak memberi hak |

## E. `PermissionRegistryInvariantTests` — perilaku seeder

| Invarian | Tuntutan |
|---|---|
| `EveryProtectedEndpointIsRegisterableInRoleAccess` | Setiap endpoint terproteksi muncul di layar Akses Role |
| `SeederIdentityMatchesRuntimeIdentity` | Identitas yang didaftarkan seeder = identitas yang dicari runtime |
| `RegistrySnapshotIsNotVacuous` | Snapshot tidak boleh kosong — menangkap kegagalan diam refleksi |
| `NoDuplicateCanonicalPermissionIdentity` | Tidak ada identitas kanonik ganda |
| `EveryActionUsesAnAllowedAccessType` | `AccessType` hanya `Read`/`Create`/`Update`/`Delete` |
| **`ReconcileNeverCreatesAccessPolicy`** | **Seeder tidak pernah membuat `SysAccessPolicy`.** Acceptance criteria `BE-SEC-003` nomor 7 |
| `ReconcileClosesStaleRegistryRowsWithoutHardDelete` | Penutupan memakai lifecycle |
| `ReconcileIsIdempotent` | Dijalankan dua kali menghasilkan keadaan sama |

## F. `StaleRegistryAuthorizationTests` dan `OrganizationAuthorizationProjectionTests`

| Invarian | Tuntutan |
|---|---|
| `InactiveActionRegistryDeniesEvenWhenPolicyStillExists` | Registry mati menolak walau policy masih ada — **dasar keselamatan Fase B** |
| `DeletedActionRegistryDeniesEvenWhenPolicyStillExists` | idem untuk `IsDelete` |
| `ClosedControllerRegistryDeniesEvenWhenPolicyStillExists` | idem untuk controller |
| `CancelledOrganizationAssignmentDeniesAccess` | Penempatan dibatalkan tidak memberi akses — invarian A0 nomor 6 |
| `NonPrimaryAssignmentStillGrantsAccess` | `IsPrimary` **bukan** penyaring kelayakan — invarian A0 nomor 4 |
| `EffectivePermissionsAreUnionOfActiveAssignments` | Izin efektif adalah **gabungan** seluruh penempatan sah |
| `EveryAssignmentTypeParticipates` | `AssignmentType` **bukan** penyaring kelayakan — invarian A0 nomor 5 |
| `DepartmentTransferRevokesStaleProjection` | Perpindahan departemen mencabut proyeksi lama |
| `InvalidAssignmentNeverProjects`, `ReconcileDoesNotResurrectInvalidAssignment` | Penempatan tidak sah tidak pernah diproyeksikan, juga tidak dihidupkan ulang |
| `LegacyRowWithoutProvableSourceIsPreservedAndReported` | Baris legacy tanpa sumber terbukti dipertahankan dan dilaporkan, bukan dibuang |
| `DryRunReportsWithoutWriting` | Mode laporan tidak menulis |

---

## G. Tuntutan minimum bagi strategi validasi pengganti

Strategi baru **tidak perlu** menghidupkan kembali seluruh pohon `Tests/`. Tetapi ia wajib menutup
tiga hal yang tanpa otomasi akan lolos begitu saja, dan ketiganya **dapat diperiksa tanpa database
dan tanpa menyalakan aplikasi** — cukup refleksi atas assembly:

| Prioritas | Tuntutan | Sumbernya |
|---|---|---|
| **1** | Argumen ke-2 `[AccessPermission]` = argumen ke-1 `[AccessAction]` pada setiap method | B-5 |
| **2** | Nol endpoint terproteksi tanpa kunci terdaftar; fallback tetap himpunan yang disetujui | D |
| **3** | Nol identitas kanonik ganda; satu resource satu modul; `AccessType` hanya empat nilai | E, B-3 |

Ketiganya sudah terbukti dapat dijalankan: runner baca-saja pada `evidence/07` menghitung
`45/329/1246/69/0` hanya dengan `PermissionRegistryDescriptor.BuildFromAssembly`, tanpa host dan
tanpa koneksi database.

Dua tuntutan berikut **menuntut database** dan karena itu lebih mahal, tetapi tidak boleh dianggap
hilang:

| Prioritas | Tuntutan | Sumbernya |
|---|---|---|
| **4** | `ReconcileNeverCreatesAccessPolicy` | E |
| **5** | Paritas `HasAccessAsync` ↔ `GetEffectivePermissionsAsync`, termasuk penempatan **dibatalkan** dan **kedaluwarsa** | C |

Usulan bentuknya ada pada laporan task `BE-SEC-003`; keputusan bentuk akhir ada pada pemilik sistem.

---

## Lampiran

| Berkas yang dihapus merge | Invarian |
|---|---:|
| `Security/PermissionSplitPreparationTests.cs` | 9 |
| `Security/CanonicalSecurityContractTests.cs` | 5 |
| `Security/PermissionRegistryInvariantTests.cs` | 8 |
| `Security/StaleRegistryAuthorizationTests.cs` | 7 |
| `Security/OrganizationAuthorizationProjectionTests.cs` | 12 |
| `BillingManagement/AccessPermissionEnforcementTests.cs` | 32 |
| **Total** | **73** |

Tidak ada database write. Tidak ada aplikasi dijalankan.
