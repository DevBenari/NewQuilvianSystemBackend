# Laporan Perubahan Backend — `BE-NV-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-009` |
| Judul | API admin tablet bed |
| Slice | `MVP-6` — `EPIC NV-05` pengelolaan tablet bed (gelombang backend 2, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-009` |
| Trace | `FR-NV-033`, `034`, `035`; `NV-DEC-035`, `056`; `AD-01`, `AD-03`, `AD-04`; `G-17`; `VAL-NV-17`–`25`; `contracts/api-contract.md` R2 §5; `contracts/state-transition-matrix.md` §4; flowchart `06`; `02-backend-architecture.md` R2.4.1, R2.4.5, R2.4.6, R2.9, R2.10 |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`) |
| Dependency | `BE-NV-008` ✅ (6 Nov 2026; migration `20261106015006_AddNurseViewPatientCall` sudah diterapkan Rizki ke DB dev) |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa > 20 termasuk governance dan blueprint (2), berkas diubah 4–8 (1), logika sedang (1), endpoint baru sesuai kontrak terkunci (1), persistence existing tanpa schema baru (1), auth berkaitan — membuat akun Identity per tablet (1), UI tidak ada (0) |
| Task mode | `BACKEND` — prompt Rizki 6 Nov 2026, target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/{Constants,Controllers,DTOs,Services}`, `Program.cs` (satu `AddScoped`, restu `NV-DEC-056`), laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `0c6eab34` (branch `rizkiG`); perubahan belum di-commit |
| Tanggal | 2026-11-06 |
| Status | 🟡 **SEBAGIAN** — source lengkap untuk AC-1–5, QBE Strict `PASS` (9 berkas, 0 violation, 0 review), pasangan atribut akses 11/11 cocok. **Belum:** `dotnet build` oleh Rizki dan uji HTTP oleh Rizki/Antigravity (§5). Dikerjakan bersama `BE-NV-016` supaya cukup satu kali build |

---

## 1. Masalah yang diperbaiki

Tabel tablet bed sudah ada sejak `BE-NV-008`, tetapi masih kosong dan belum ada cara mengisinya.
Admin IT belum bisa mendaftarkan tablet untuk sebuah bed, membuat akun login tablet, memindahkan
tablet ke bed lain, maupun mematikan tablet yang hilang atau rusak. Tanpa itu, tombol "Panggil
perawat" di samping bed (`BE-NV-011`) tidak punya perangkat untuk menekannya.

**Contoh.** Tablet bed 305-A jatuh dan layarnya pecah. Tanpa API ini, tidak ada cara mencabut akun
tablet itu lalu memasang tablet pengganti untuk bed yang sama, selain mengubah database langsung,
yang dilarang.

---

## 2. Proses bisnis

**Tujuan:** Admin IT mengelola tablet bed dari pemasangan sampai dinonaktifkan, dengan aturan satu
bed paling banyak satu tablet yang masih dipakai.

**Pelaku:** Admin IT dengan hak `NurseViewBedTablet : Read/Create/Update`, diberikan lewat layar
Pengaturan → Manajemen Role → Akses Role.

**Langkah normal:**

1. Admin membuka pilihan lantai (`GET /floors`), misalnya Lantai 3, lalu pilihan bed lantai itu
   (`GET /bed-options?floorCode=3`). Setiap bed membawa tanda `HasTabletInUse`; bed yang sudah
   punya tablet tampil bertanda.
2. Admin mendaftarkan "Tablet Bed 305-A" untuk bed 305-A. Sistem menerbitkan kode dari number
   series deret `NVW_BED_TABLET`, misalnya `NVT-000001` (`AD-03`), dengan status `PendingLogin`
   ("Belum ada login") dan `Version = 0`.
3. Admin menekan Buat Login dan mengirim `version = 0`. Sistem membuat akun `nvt-000001` dengan
   kata sandi acak yang **hanya tampil di response ini**. Status berubah menjadi `Active` dan
   `Version = 1`.
4. Bila tablet dipindah ke bed 318-B, admin mengubah bed-nya lewat `PUT /{id}`. Sistem hanya
   mengizinkannya bila bed lama tidak punya panggilan aktif dan bed baru belum punya tablet yang
   masih dipakai (`AD-04`, `AD-01`).
5. Bila tablet hilang, admin menonaktifkan dengan alasan, misalnya "Tablet pecah, diganti unit
   cadangan". Status menjadi `Inactive`, akun dikunci 100 tahun, dan security stamp diganti.
6. Bila tablet yang sama dipasang lagi, admin menekan Atur Ulang Login. Sistem memberi kata sandi
   baru, membuka kunci akun, dan status kembali `Active` — asalkan bed-nya belum dipasangi tablet
   lain sementara itu (`VAL-NV-19`).

**Status tablet** (`contracts/state-transition-matrix.md` §4): `PendingLogin = 1` → `Active = 2`
↔ `Inactive = 3`. Tablet tidak bisa dihapus; penggantinya nonaktifkan.

**Jalur tidak normal:**

| Kejadian | Pesan | Kode |
| --- | --- | --- |
| Nama tablet kosong atau > 100 karakter | "Nama tablet wajib diisi, paling banyak 100 karakter." (`VAL-NV-17`) | `400` |
| Bed kosong, tidak dikenal, tidak aktif, atau kamarnya tanpa lantai | "Bed tidak dikenal atau kamarnya belum punya lantai. Pilih bed dari daftar." (`VAL-NV-18`) | `422` |
| Tablet kedua untuk bed 305-A yang tablet pertamanya belum nonaktif | "Bed ini sudah punya tablet. Nonaktifkan tablet lama lebih dulu." (`VAL-NV-19`) | `409` |
| Memindah tablet dari bed yang masih punya panggilan `Waiting`/`Claimed` | "Bed ini masih punya panggilan aktif. Selesaikan panggilannya dulu." (`VAL-NV-20`) | `409` |
| Dua admin mengubah tablet yang sama; yang kedua mengirim versi lama | "Data tablet sudah diubah orang lain. Muat ulang lalu coba lagi." (`VAL-NV-21`) | `409` |
| Buat Login pada tablet yang sudah punya login | "Tablet ini sudah punya login. Gunakan Atur Ulang Login." (`VAL-NV-22`) | `409` |
| Atur Ulang Login pada tablet `PendingLogin` | "Tablet belum punya login. Gunakan Buat Login." (`VAL-NV-23`) | `409` |
| Alasan nonaktif kurang dari 5 atau lebih dari 500 karakter | "Alasan menonaktifkan wajib diisi (5–500 karakter)." (`VAL-NV-24`) | `400` |
| Menonaktifkan tablet yang sudah nonaktif | "Tablet sudah nonaktif." (`VAL-NV-25`) | `409` |
| Mengaktifkan kembali tablet lama padahal bed-nya sudah dipasangi tablet pengganti | Pesan `VAL-NV-19` | `409` |
| Pilihan bed tanpa `floorCode` atau lantai tidak dikenal | "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar." (`VAL-NV-02`) | `422` |
| Akun Identity `nvt-…` sudah ada | "Akun login untuk kode perangkat ini sudah ada. Hubungi tim IT." | `409` |
| Number series gagal menerbitkan kode | "Kode perangkat gagal diterbitkan. Coba lagi." | `409` |

**Contoh satu bed satu tablet.** Bed 305-A punya `NVT-000001` yang `Active`. Admin mencoba
mendaftarkan `NVT-000002` untuk 305-A → ditolak `409`. Admin menonaktifkan `NVT-000001`, lalu
mendaftarkan tablet pengganti untuk 305-A → diterima. Bila kemudian admin mencoba mengaktifkan
kembali `NVT-000001` lewat Atur Ulang Login → ditolak `409`, karena 305-A sudah dipakai tablet
pengganti.

**Transaksi.** Buat login, atur ulang login, dan nonaktifkan dijalankan dalam satu transaksi
database bersama perubahan akun Identity. Bila penyimpanan tablet gagal — misalnya kalah balapan
pada index `IX_NvwBedTablet_BedId_InUse` — akun dan tablet sama-sama tidak berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `NewQuilvianSystemBackend/AGENTS.md`, `CLAUDE.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`
  (sama persis dengan salinan suite skill), `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
  baris 45 `Nvw` `ACTIVE` (salinan repository memuat baris `Nvw` dan `Opt` yang belum ada di suite;
  salinan repository yang berlaku); suite skill `TASK_RULES.md`, `TASK_CLASSIFICATION.md`,
  `API_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `role-access-rules.md`,
  `rule-output/status-task-roadmap.md`
- Blueprint: `roadmap/backend-roadmap.md` Revision 2 (kartu `BE-NV-009`, "Berkas bersama per
  task"); `contracts/api-contract.md` R2 §5; `contracts/validation-matrix.md` R2;
  `contracts/state-transition-matrix.md` §4; `02-backend-architecture.md` R2.1–R2.13;
  `data/data-dictionary.md` R2; laporan `BE-NV-002.md`, `BE-NV-004.md`, `BE-NV-008.md`
- Pola terdekat: `Services/NvwDisplayDeviceService.cs`, `Controllers/NvwDisplayDeviceController.cs`,
  `Controllers/NvwHttp.cs`, `Services/NvwResult.cs`, `DTOs/NvwDisplayDeviceDtos.cs`,
  `Services/NvwFloorBoardQueryService.cs` (pembacaan kamar dan bed per lantai)
- Model dan configuration `BE-NV-008`: `Models/NvwBedTablet.cs`, `NvwPatientCall.cs`,
  `Repositories/Configurations/HealthServices/NurseViewManagement/NvwBedTabletConfiguration.cs`
- Rujukan bersama: `Areas/HealthServices/MasterData/Models/MstBed.cs`, `MstRoom.cs`,
  `Repositories/Configurations/HealthServices/MstBedConfiguration.cs` (relasi `Room`),
  `Services/Security/PermissionRegistryValidator.cs` (gerbang integritas akses saat startup),
  `Areas/Corporate/FinanceManagement/MasterData/Services/DirectPaymentThresholdService.cs`
  (pola deteksi unique violation `23505`), `NumberSeriesAllocator.cs` (format `PREFIX-000001`),
  `InpatientBedManagementDtos.cs` (preseden `[JsonIgnore]` pada DTO), `Program.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Services/NvwBedTabletService.cs` | **Baru.** Daftar (saring lantai, status, kata kunci; urut kode, nama, bed, lantai, status, terakhir online), pilihan lantai (memakai `NvwDisplayDeviceService.GetFloorsAsync`), pilihan bed satu lantai beserta `HasTabletInUse`, rincian beserta `HasActiveCall` dan `AvailableActions`, buat, ubah termasuk pindah bed, buat login, atur ulang login (termasuk aktifkan kembali), nonaktifkan. Kode dari `NumberSeriesAllocator` deret `NVW_BED_TABLET`, awalan `NVT`, 6 digit, `NEVER`. Benturan index `IX_NvwBedTablet_BedId_InUse` (`23505`) diterjemahkan menjadi `VAL-NV-19`; galat database lain tetap naik |
| `…/Controllers/NvwBedTabletController.cs` | **Baru.** 9 endpoint api §5; `[Tags("Health Services / Nurse View Management / Bed Tablet")]`; `[AccessController]` modul `HEALTH_SERVICE_NURSE_VIEW`, `ControllerName = NurseViewBedTablet`; tiap action `[AccessAction]` + `[AccessPermission]`. Log aksi admin ditulis di teks pesan |
| `…/DTOs/NvwBedTabletDtos.cs` | **Baru.** Query, empat request, tiga response, dan `NvwBedOptionResponse` sesuai api §5 |
| `…/Services/NvwResult.cs` | +21 baris gabungan; bagian task ini 19: kode `VAL-NV-17`–`25` dan pesannya persis matriks validasi, ditambah `TabletNotFound`. Dua baris sisanya `VAL-NV-40` milik `BE-NV-016` |
| `…/Constants/NurseViewPermissions.cs` | +10 baris gabungan; bagian task ini 5: resource `NurseViewBedTablet`. Sisanya resource milik `BE-NV-016` |
| `Program.cs` | +1 baris `builder.Services.AddScoped<NvwBedTabletService>();` sesudah `NvwRealtimeService` (restu `NV-DEC-056`). Tidak ada baris lain berubah |

Nol baris komentar pada berkas baru, sesuai konvensi modul.

**Diff ringkas berkas bersama `Program.cs`** (gabungan dengan `BE-NV-016`, +2/−0):

```diff
     builder.Services.AddScoped<NvwRealtimeService>();
+    builder.Services.AddScoped<NvwBedTabletService>();
+    builder.Services.AddScoped<NvwPatientCallWaitThresholdService>();
```

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 9 endpoint baru sesuai api §5. **Delta yang dicatat**, semuanya mengikuti implementasi `0.1.x` perangkat display yang sudah lulus `BE-NV-002`: (1) parameter halaman bernama `pageNumber`/`pageSize`, ditambah `sortBy`/`sortDirection`, seperti `NvwDisplayDevicePagedQuery` (kontrak menulis `page`); (2) rincian memuat `DeactivatedByName` untuk memenuhi `G-17` "beserta pelakunya" (kontrak §5 tidak mendaftarkannya); (3) `GET /bed-options` tanpa `floorCode` atau dengan lantai tidak dikenal dijawab `422` `VAL-NV-02`, karena kontrak tidak menyebut kode galatnya. Tidak ada endpoint baseline master data (`summary`, `options`, `PATCH status`, `DELETE`): kontrak memakai aksi bernama dan state matrix §4 melarang hapus tablet |
| Database | Tidak ada schema baru dan tidak ada migration. Menulis `NvwBedTablet`, `AspNetUsers` (lewat `UserManager`), dan number series `NVW_BED_TABLET` (lahir pada alokasi pertama). Membaca `MstBed`, `MstRoom`, `NvwPatientCall` |
| Keamanan/Auth | Satu akun Identity per tablet (`nvt-000001@nurse-view.local`). Kata sandi `Nvt@<16 hex>aA1!` hanya ada di `NvwBedTabletLoginResponse`, tidak disimpan di tabel modul, dan tidak masuk log. **Sampai `BE-NV-010` selesai,** akun tablet login sebagai akun biasa tanpa hak akses apa pun dan tanpa klaim tablet. Pemeriksaan kewenangan seluruhnya lewat `[AccessPermission]`; tidak ada nama peran, departemen, atau posisi di kode |

**Dua properti server-saja.** `NvwBedTabletLoginResponse.BedCode`, `.DeviceStatusLabel`, dan
`NvwBedTabletDetailResponse.PreviousBedCode` bertanda `[System.Text.Json.Serialization.JsonIgnore]`
(preseden `InpatientBedManagementDtos.cs` baris 50–52). Gunanya hanya menyusun teks log
yang diwajibkan kartu (`EntityId`, `DeviceCode`, `BedCode`, aksi, status) tanpa query tambahan dan
tanpa menambah field ke JSON. Bentuk JSON response tetap sama dengan kontrak.

**Teks log per aksi** (`LoggerService` hanya menyimpan teks pesan, lihat `BE-NV-004` no. 13):

| Aksi | Contoh teks pesan |
| --- | --- |
| Daftarkan | "Tablet bed NVT-000001 (Id …) didaftarkan untuk bed 305-A. Aksi: Create. Status: Belum ada login." |
| Ubah | "Tablet bed NVT-000001 (Id …) diperbarui; bed 305-A. Aksi: Update. Status: Aktif." |
| Pindah bed | "Tablet bed NVT-000001 (Id …) dipindah dari bed 305-A ke bed 318-B. Aksi: Update. Status: Aktif." |
| Buat login | "Login tablet bed NVT-000001 (Id …) dibuat untuk bed 305-A; akun nvt-000001. Aksi: CreateLogin. Status: Aktif." |
| Atur ulang login | "Login tablet bed NVT-000001 (Id …) diatur ulang untuk bed 305-A; akun nvt-000001. Aksi: ResetLogin. Status: Aktif." |
| Nonaktifkan | "Tablet bed NVT-000001 (Id …) untuk bed 305-A dinonaktifkan. Aksi: Deactivate. Status: Nonaktif." |

`BedCode` di log dan response adalah `MstBed.BedCode`. Di DB dev nilainya berbentuk
`BD-RSMMC-00169`; label yang dibaca manusia seperti "305-A" ada di `BedName`.

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Bed Tablet

Base URL: `api/v1/health-services/nurse-view-management/bed-tablets`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar tablet; saring `floorCode`, `status`, `search`; urut `sortBy`/`sortDirection`; halaman `pageNumber`/`pageSize` | `NurseViewBedTablet : Read` |
| `GET` | `/floors` | Pilihan lantai dari `FloorCode` kamar aktif, beserta jumlah kamar dan bed | `NurseViewBedTablet : Read` |
| `GET` | `/bed-options` | Pilihan bed aktif satu lantai (`floorCode` wajib) beserta `HasTabletInUse` | `NurseViewBedTablet : Read` |
| `GET` | `/{id}` | Rincian, jejak login/nonaktif beserta pelakunya, `HasActiveCall`, `Version`, `AvailableActions` | `NurseViewBedTablet : Read` |
| `POST` | `/` | Daftarkan tablet untuk satu bed (`201`) | `NurseViewBedTablet : Create` |
| `PUT` | `/{id}` | Ubah nama, bed, lokasi, keterangan | `NurseViewBedTablet : Update` |
| `POST` | `/{id}/login` | Buat login; kata sandi tampil sekali | `NurseViewBedTablet : Update` |
| `POST` | `/{id}/login/reset` | Atur ulang login; juga mengaktifkan kembali tablet `Inactive` | `NurseViewBedTablet : Update` |
| `PATCH` | `/{id}/deactivate` | Nonaktifkan; akun dikunci seketika | `NurseViewBedTablet : Update` |

Kode status: `200`/`201` berhasil · `400` isian salah (`VAL-NV-17`, `24`) · `403` tanpa hak akses ·
`404` tablet tidak ada · `409` `VAL-NV-19`–`23`, `25` · `422` bed tidak sah (`VAL-NV-18`) atau lantai
tidak dikenal pada pilihan bed.

**Contoh body buat.**
`{ "deviceName": "Tablet Bed 305-A", "bedId": "…", "locationNote": "Dinding kanan kepala bed" }`

**Contoh response buat login.**
`{ "tabletId": "…", "deviceCode": "NVT-000001", "loginUserName": "nvt-000001", "password": "<hanya di sini>", "version": 1 }`

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <9 berkas>` (dipanggil `& .\tooling\qbe\…` dengan array path; 9 berkas gabungan `BE-NV-009` + `BE-NV-016`) | Scope `ExplicitFiles` · Files evaluated **9** (= 9 berkas) · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-06 |
| Pasangan `ControllerName` ↔ `[AccessPermission]` arg-1, `[AccessAction]` arg-1 ↔ `[AccessPermission]` arg-2, `AccessType` | 9/9 cocok pada `NvwBedTabletController` (11/11 bersama `BE-NV-016`); `AccessType` hanya `Read`, `Create`, `Update` | `PASS` | Skrip pemeriksa di scratchpad, 2026-11-06 |
| Kata sandi tidak masuk log | Lambda `logMessage` keenam aksi tidak merujuk `Password`; `payload` tidak dikirim | `PASS` (source) | `grep Password` pada controller: nol hasil |
| Nol komentar baris | Nol `//` dan `/*` pada 6 berkas baru | `PASS` | `grep` |
| Nama `NurseCall*` (`NV-FCT-004`) | Nol hasil | `PASS` | `grep` |
| Akhiran baris | 9 berkas CRLF, nol LF tunggal, tanpa BOM; `Program.cs` tetap tanpa baris baru di akhir | `PASS` | Hitungan byte lewat Node |
| Review diff berkas bersama | `Program.cs` +2/−0; `NvwResult.cs` dan `NurseViewPermissions.cs` hanya tambahan | `PASS` | `git diff` |
| `SELECT` read-only DB dev (`default_transaction_read_only=on`) | `NvwBedTablet` 0 baris; 189 bed aktif seluruhnya di kamar berlantai (lantai 2, 3, 4, 5, 8, 9); 5 bed nonaktif/terhapus tersedia untuk uji `VAL-NV-18` | `PASS` | Keluaran `psql`; tidak ada penulisan |
| `dotnet build` | Belum | `NOT RUN` | Dijalankan Rizki (aturan task); perintah di bawah |
| Uji HTTP | Belum | `NOT RUN` | Skenario di bawah, oleh Rizki/Antigravity sesudah build |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

Uji manual: `REQUIRED`.

**Tidak dijalankan:** `dotnet build`, `dotnet run`, dan uji HTTP — sesuai aturan task, build dan
eksekusi runtime milik Rizki. Tidak ada `dotnet ef` karena task ini tanpa migration.

### Perintah build untuk Rizki

Satu kali build untuk `BE-NV-009` dan `BE-NV-016`, dari folder `NewQuilvianSystemBackend`. Task ini
tanpa migration, jadi `-p:SkipMigrationMetadata=true` aman dipakai. DLL hasil flag itu **tidak boleh**
dipakai `dotnet ef --no-build`.

```bash
# PC kantor
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false -p:SkipMigrationMetadata=true

# laptop
dotnet build-server shutdown
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false -p:SkipMigrationMetadata=true -m:1 -nodeReuse:false

# menjalankan backend sesudah build
dotnet run --no-build
```

Saat startup, `PermissionRegistryValidator` memeriksa atribut akses. Bila gagal, startup berhenti
dengan pesan yang menyebut endpoint-nya; bila lolos, resource `NurseViewBedTablet` dan
`NurseViewPatientCallWaitThreshold` muncul di layar Akses Role lewat `AccessMenuSeeder`.

### Skenario uji HTTP untuk Antigravity — urut

**Aturan uji.** Aktor berhak = akun peran yang diberi `NurseViewBedTablet : Read/Create/Update`
lewat layar Akses Role memakai pola salin → tambah → pulihkan (DB dev dipakai bersama; jangan
menimpa seluruh policy). SuperAdmin boleh untuk persiapan lewat aplikasi, tetapi **bukan** aktor
skenario, karena SuperAdmin melewati pemeriksaan akses sehingga salah pasang atribut tidak
terlihat. Aktor tanpa hak = akun yang tidak memegang `NurseViewBedTablet`, misalnya
`QUILVIAN_LOKET_*` (periksa dulu, seperti `BE-NV-002` no. 11). Tanpa SQL tulis. Kata sandi hanya di
memori skrip; di JSON bukti, setiap ruas `password` ditulis `<disamarkan>`. Simpan bukti di
`QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-009-<tanggal>/` beserta skrip dan
`percobaan.json`. Berhenti bila langkah prasyarat gagal.

Pilih dua bed aktif di Lantai 3 yang `HasTabletInUse = false` dari langkah 2: **bed A** dan
**bed B**. Untuk `VAL-NV-18` pakai bed nonaktif `71950186-9e7e-4368-abe4-1457dc2e8d7a`
(`BD-RSMMC-00169`, Lantai 3) dan bed terhapus tanpa lantai `0dbc6876-5920-4c65-9f68-c64bfa45d839`.

| No | Langkah | Hasil yang diharapkan | AC |
| ---: | --- | --- | --- |
| 1 | `GET /floors` | `200`; lantai `2`,`3`,`4`,`5`,`8`,`9` | AC-1 |
| 2 | `GET /bed-options?floorCode=3` | `200`; daftar bed Lantai 3 dengan `hasTabletInUse` | AC-1 |
| 3 | `GET /bed-options` tanpa `floorCode`, lalu `?floorCode=99` | Keduanya `422` "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar." | AC-1 |
| 4 | `POST /` `{ "deviceName": "Tablet Bed A (uji)", "bedId": "<A>" }` → **T1** | `201`; `deviceCode` `NVT-000001` (nomor pertama deret); `deviceStatus` 1; `version` 0; `hasActiveCall` false; `availableActions` `Update`, `CreateLogin`, `Deactivate` | AC-1, AC-2 |
| 5 | `POST /` dengan `deviceName` `""`, lalu 101 karakter | `400` "Nama tablet wajib diisi, paling banyak 100 karakter." | AC-2 (`VAL-NV-17`) |
| 6 | `POST /` dengan `bedId` acak, tanpa `bedId`, bed nonaktif, dan bed terhapus tanpa lantai | Keempatnya `422` "Bed tidak dikenal atau kamarnya belum punya lantai. Pilih bed dari daftar." | AC-2 (`VAL-NV-18`) |
| 7 | `POST /` tablet kedua untuk bed A | `409` "Bed ini sudah punya tablet. Nonaktifkan tablet lama lebih dulu." | AC-2 (`VAL-NV-19`, `UAT-13`) |
| 8 | `POST /` untuk bed B → **T2** | `201`; kode berikutnya `NVT-000002` — no. 5–7 tidak memakan nomor | AC-2 |
| 9 | `PUT /{T2}` pindah ke bed A, `version` 0 | `409` pesan `VAL-NV-19` | AC-2 |
| 10 | `GET /bed-options?floorCode=3` | Bed A dan B `hasTabletInUse` true | AC-1 |
| 11 | `POST /{T1}/login` `{ "version": 0 }` | `200`; `password` terisi; `loginUserName` `nvt-000001`; `version` 1 | AC-3 |
| 12 | Ulangi no. 11 dengan `version` 1 | `409` "Tablet ini sudah punya login. Gunakan Atur Ulang Login." | AC-2 (`VAL-NV-22`) |
| 13 | `POST /{T2}/login/reset` `{ "version": 0 }` | `409` "Tablet belum punya login. Gunakan Buat Login." | AC-2 (`VAL-NV-23`) |
| 14 | `GET /{T1}` | Nol ruas `password`; `deviceStatus` 2; `loginUserName`, `loginIssuedAt`, `loginIssuedByName` terisi; `availableActions` memuat `ResetLogin` | AC-3, AC-5 |
| 15 | `PUT /{T1}` dengan `version` 0 | `409` "Data tablet sudah diubah orang lain. Muat ulang lalu coba lagi." | AC-2 (`VAL-NV-21`) |
| 16 | Login `nvt-000001` dengan sandi no. 11 (`POST /api/v1/auth/login`) | `200` — akun biasa tanpa hak; klaim tablet baru ada di `BE-NV-010` | AC-3 |
| 17 | `PATCH /{T1}/deactivate` `{ "reason": "abc", "version": 1 }`, lalu tanpa `reason` | Keduanya `400` "Alasan menonaktifkan wajib diisi (5–500 karakter)." | AC-3 (`VAL-NV-24`) |
| 18 | `PATCH /{T1}/deactivate` `{ "reason": "Uji BE-NV-009 nonaktif", "version": 1 }` | `200`; `deviceStatus` 3; `deactivatedAt`, `deactivatedByName`, `deactivationReason` terisi | AC-3, AC-5 |
| 19 | Login `nvt-000001` dengan sandi no. 11 | `401` (akun terkunci) | AC-3 |
| 20 | Ulangi no. 18 dengan `version` 2 | `409` "Tablet sudah nonaktif." | AC-2 (`VAL-NV-25`) |
| 21 | `POST /{T1}/login/reset` `{ "version": 2 }` | `200`; sandi baru berbeda; `GET /{T1}` → `deviceStatus` 2, `version` 3, jejak nonaktif tetap tersimpan | AC-3 |
| 22 | Login `nvt-000001` dengan sandi lama, lalu sandi baru | Sandi lama `401`; sandi baru `200` | AC-3 |
| 23 | `PATCH /{T1}/deactivate` `{ "reason": "Uji BE-NV-009 tablet diganti", "version": 3 }` | `200`; bed A kini bebas | AC-3 |
| 24 | `PUT /{T2}` pindah ke bed A, `version` 0 | `200`; `bedCode` bed A; `version` 1. Log memuat "dipindah dari bed … ke bed …" | AC-1, AC-4 jalur sukses |
| 25 | `POST /{T1}/login/reset` `{ "version": 4 }` | `409` pesan `VAL-NV-19` — T1 tidak bisa aktif kembali karena bed A dipakai T2 | AC-2 (`VAL-NV-19` aktifkan kembali) |
| 26 | `GET /?floorCode=3&search=NVT&status=3` dan `GET /?floorCode=3` | `200`; status 3 hanya T1; tanpa saring status memuat T1 dan T2 | AC-1 |
| 27 | Akun tanpa hak memanggil `GET /`, `GET /bed-options?floorCode=3`, `POST /` | Ketiganya `403` | AC-1 |
| 28 | Bersih-bersih: `PATCH /{T2}/deactivate` `{ "reason": "Uji BE-NV-009 selesai", "version": 1 }` | `200`; kedua tablet uji nonaktif sehingga bed A dan B bebas untuk pemakaian nyata | — |
| 29 | Pindai `Logs/quilvian-backend-20261106.json` (atau tanggal uji) | Nol kemunculan sandi no. 11 dan no. 21 (dalam bentuk asli dan ter-encode), nol pola `Nvt@[0-9A-F]{16}aA1!`; enam jenis teks log di §3.3 muncul dengan kode tablet dan `BedCode` | AC-5 |

AC-4 (`VAL-NV-20`, tolak pindah bed yang masih punya panggilan aktif) **tidak bisa** dibuktikan
runtime pada task ini, karena panggilan baru bisa dibuat di `BE-NV-011`. Bukti task ini adalah
penelusuran source (§6); bukti runtime-nya diambil saat verifikasi `BE-NV-011`, sesuai kartu.

Sesudah uji, agent memeriksa lewat `SELECT` read-only: dua baris `NvwBedTablet` `DeviceStatus = 3`
dengan `DeactivatedBy`, `LoginIssuedBy` terisi pada T1, dan satu akun `nvt-000001` terkunci.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 sembilan endpoint sama dengan api §5: method, path, hak akses, request, response | Terpetakan ke source; runtime belum | `NvwBedTabletController.cs` 9 action; DTO `NvwBedTabletDtos.cs`; tiga delta kecil di §3.3. Uji no. 1–4, 10, 24, 26, 27 |
| AC-2 `VAL-NV-17`–`25` menolak dengan kode dan pesan matriks | Terpetakan ke source; runtime belum | `NvwResult.cs` (pesan persis matriks); service: `CreateAsync`/`UpdateAsync` (`17`, `18`, `19`, `20`, `21`), `CreateLoginAsync` (`21`, `22`), `ResetLoginAsync` (`19`, `21`, `23`), `DeactivateAsync` (`21`, `24`, `25`); index unik `23505` → `19`. Uji no. 5–9, 12, 13, 15, 17, 20, 25 |
| AC-3 buat login → `Active`, sandi hanya di response login; atur ulang → sandi dan sesi lama gugur; nonaktif tanpa alasan → `400` | Terpetakan ke source; runtime belum | `CreateLoginAsync`, `ResetLoginAsync` (sandi baru, buka kunci, security stamp baru, `LoginIssuedAt` baru sebagai dasar stempel sesi `BE-NV-010`), `DeactivateAsync`. "Sesi lama gugur" pada sisi runtime tablet ditegakkan `BE-NV-010`; yang dibuktikan di sini sandi lama ditolak. Uji no. 11, 14, 16–19, 21–23 |
| AC-4 pindah tablet dari bed dengan panggilan aktif → `409` (`VAL-NV-20`) | Terpetakan ke source; runtime menunggu `BE-NV-011` | `UpdateAsync`: bila `BedId` berubah, `BedHasActiveCallAsync(tablet.BedId)` memeriksa `NvwPatientCall` `Waiting`/`Claimed` yang tidak terhapus pada bed lama sebelum pemeriksaan bed baru |
| AC-5 kolom login dan nonaktif terisi beserta pelakunya; sandi tidak ada di response rincian maupun log | Terpetakan ke source; runtime belum | `LoginIssuedAt/By`, `DeactivatedAt/By/Reason` diisi; rincian memuat `LoginIssuedByName`, `DeactivatedByName`; `NvwBedTabletDetailResponse` tidak punya properti `Password`; teks log tanpa sandi. Uji no. 14, 18, 29 |
| DoD — QBE `PASS` | Terpenuhi | §5: 9 berkas, 0 violation, 0 review |
| DoD — build Rizki lulus | **Belum terpenuhi** | Menunggu build |
| DoD — uji HTTP tercatat | **Belum terpenuhi** | Menunggu uji no. 1–29 |
| DoD — laporan ada | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (controller → `NvwBedTabletService`, tanpa
`ApplicationDbContext` di controller), `QBE-API-001` (`ApiResponse<T>`, `PagedResult<T>`, kode
status mapan lewat `NvwHttp`), `QBE-PERM-001` (`[AccessController]`/`[AccessAction]`/`[AccessPermission]`),
`QBE-LOG-001` (log aksi dengan aktor dan teks jejak), `QBE-DTO-001`, `QBE-VAL-001` (anotasi DTO +
pemeriksaan service), `QBE-TXN-001` (akun + tablet satu transaksi), `QBE-CODE-001`/`002`/`003`/`005`/`006`
(kode milik service, deret dan awalan sendiri, provider bersama atomik, bukan `Max+1`, bukan di
controller), `QBE-CODE-004` (unique `DeviceCode` dari `BE-NV-008`), `QBE-PAGE-001`, `QBE-OPT-001`
(`/floors` dan `/bed-options` dipakai form `FE-NV-008`), `QBE-DEL-001` (tanpa hapus; nonaktif beraudit),
`QBE-ENUM-001`. Tidak berlaku: `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-003`, `QBE-DB-*` (tanpa entity,
configuration, maupun migration baru).

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` / Nurse View ·
Submodule — · pemilik Rizki Gunawan · prefix `Nvw` · registry `ACTIVE` (baris 45, 2026-10-28,
`NV-DEC-029`) · applicability `NEW CODE`; `Program.cs` = `TOUCHED LEGACY` (hanya tambahan, restu
`NV-DEC-056` dengan syarat `NV-DEC-030`) · branch `rizkiG` sesuai blueprint · sisa folder
`agents/rules/` dan `.codex/` tidak ditemukan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sampai `BE-NV-010` selesai, akun tablet yang dibuat di sini login sebagai akun biasa tanpa hak akses dan tanpa diarahkan ke layar tablet. Jangan bagikan kredensialnya di luar uji |
| Masalah yang diketahui | (1) Pindah bed membaca panggilan aktif lalu menyimpan tanpa kunci baris. Bila `BE-NV-011` membuat panggilan di antara dua langkah itu, panggilan bisa lahir pada bed lama sementara tablet sudah pindah. Celahnya beberapa milidetik dan butuh admin memindah tablet tepat saat pasien menekan; cukup dicegah di `BE-NV-011` dengan mengambil bed dari tablet di dalam transaksi panggilan, bukan dengan mengubah task ini. (2) `VAL-NV-19` diterapkan literal pada pindah bed, termasuk untuk tablet `Inactive`; index database sendiri hanya menahan tablet yang belum nonaktif. (3) Login tablet nonaktif masih dijawab pesan "Akun terkunci sementara…" milik `AuthController`; pesan `AD-02` baru ada di `BE-NV-010` |
| Risiko tersisa | Opsi password Identity dipenuhi pola `Nvt@<16 hex>aA1!`; bila kebijakan password berubah, pembuatan login bisa gagal dengan pesan Identity. Akun `nvt-…` hasil uji tetap ada di `AspNetUsers` (terkunci bila tabletnya nonaktif) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan dua task ini (`BE-NV-009` + `BE-NV-016`): `M Program.cs`, `M Areas/HealthServices/NurseViewManagement/Services/NvwResult.cs`, `M …/Constants/NurseViewPermissions.cs`; `?? …/Controllers/NvwBedTabletController.cs`, `NvwPatientCallWaitThresholdController.cs`; `?? …/DTOs/NvwBedTabletDtos.cs`, `NvwPatientCallWaitThresholdDtos.cs`; `?? …/Services/NvwBedTabletService.cs`, `NvwPatientCallWaitThresholdService.cs`; `?? docs/…/task/report/backend/BE-NV-009.md`, `BE-NV-016.md`; `M docs/…/roadmap/backend-roadmap.md`, `frontend-roadmap.md`, `requirement-traceability.md` (baris status saja). Sudah ada sebelum task ini dan tidak disentuh: berkas `BE-NV-008` (6 enum, 4 model, 4 configuration, `ApplicationDbContext.cs`, migration `20261106015006` + snapshot), dokumen blueprint revision 2 lain, `TLM-NV-002`, dan berkas `docs/module-blueprints/igd/**` milik pengguna |
| Langkah berikutnya | Rizki build sekali untuk `BE-NV-009` + `BE-NV-016`; uji HTTP no. 1–29; lalu `BE-NV-010` (login dan runtime akun tablet — butuh regresi login seluruh sistem) |
