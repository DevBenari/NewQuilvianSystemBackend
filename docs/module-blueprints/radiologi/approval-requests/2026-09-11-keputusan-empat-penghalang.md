# Keputusan atas Empat Penghalang Akhir Backend — Modul Radiologi

| Field | Value |
|---|---|
| `request_id` | `RAD-REQ-002` |
| `tanggal` | 2026-09-11 |
| `pengaju` | Pelaksana backend, menutup `BE-RAD-08` s/d `BE-RAD-13` |
| `pemutus` | Yoga Aji Pratama — Product/Domain Owner Radiologi (`RAD-DEC-014`) |
| `rujukan` | `task/report/backend/BE-RAD-08.md` bagian 7.1; `BE-RAD-10.md` bagian 2.3; `BE-RAD-13.md` bagian 3.4 dan 7.1 |
| `status` | **`selesai`** — tiga butir dikerjakan penuh, satu butir diserahkan sebagai skrip SQL |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

Empat hal menggantung setelah seluruh 15 task backend selesai. Tidak satu pun dapat diselesaikan
backend sendiri: ketiganya menuntut keputusan pemilik modul, dan satu menuntut wewenang database.

**Keputusan pemilik modul, 2026-09-11: menyetujui rekomendasi pelaksana untuk keempatnya.**

---

## 1. Penanda `RadReport : ActAsRadiologist` — **DITUTUP**

### Keadaan sebelumnya

`RAD-PERM-001` bagian 6 menetapkan penanda ini **tidak menempel pada satu endpoint pun** — ia
hanya dibaca service. Tetapi `AccessMenuSeeder` mendaftarkan pasangan hak akses semata-mata
dengan menelusuri action MVC yang punya `[AccessController]` **dan** `[AccessAction]`.

Akibatnya pasangan itu tidak pernah masuk `SysActionAccess`. `HasAccessAsync` mencarinya, tidak
menemukannya, lalu menjawab `false` — untuk semua orang kecuali SuperAdmin.

> **Apa artinya bagi pengguna.** Sejak `BE-RAD-08`, aturan `RAD-DEC-003` ada di kode dan terbukti
> lewat 41 uji — tetapi **tidak berjalan sama sekali di sistem sebenarnya**. Tidak ada satu pun
> dokter radiolog yang dapat mengesahkan atau merilis hasil bacaan, dan tidak ada satu pun layar
> yang dapat memperbaikinya, karena baris untuk dicentangnya memang tidak ada.

### Yang dikerjakan

**Pilihan A dijalankan** — mendaftarkan penanda tanpa endpoint, setia pada `RAD-PERM-001`
bagian 6. Pilihan B ditolak karena bertentangan dengan bagian itu.

| Berkas | Perubahan |
| --- | --- |
| `Seeders/AccessMenuSeeder.cs` | Daftar `PenandaTanpaEndpointYangDidaftarkan` beserta `EnsurePenandaTanpaEndpoint` |
| `Tests/.../RadiologyRoleAccessContractTests.cs` | Butir kelima ditambahkan, lihat bagian 1.2 |
| `contracts/permission-audit-matrix.md` | Amandemen revision 9 |

**Bentuknya sengaja begini:**

| Keputusan | Alasan |
| --- | --- |
| Daftarnya **ditulis tangan dan pendek** | Setiap barisnya kewenangan yang tidak dapat ditelusuri dari source dengan cara biasa, jadi harus terbaca di satu tempat yang mudah diperiksa saat review |
| `HttpMethod` dan `RoutePath` **dibiarkan kosong** | Penanda ini memang tidak punya route. Mengisinya dengan alamat karangan membuat layar Akses Role menampilkan jalur yang tidak dapat dipanggil siapa pun |
| **Gagal keras** bila controllernya tidak ditemukan | Penanda yang gagal didaftarkan menghasilkan `403` permanen tanpa galat yang terlihat — persis bentuk kegagalan yang sedang ditutup. Melewatinya diam-diam berarti mengulangi cacatnya dengan kode yang terlihat seperti sudah menanganinya |
| Tetap **terpisah** dari `RadReport : Validate` | Menggabungkannya menghapus `RAD-DEC-003`: residen dapat diberi `Validate` untuk mengesahkan draf radiografer, dan tanpa penanda ini draf yang ia tulis sendiri tetap ditolak |

### 1.2 Celah pada uji kontrak hak akses ikut ditutup

`RAD-PERM-001` bagian 9 punya empat butir, dan **keempatnya buta terhadap penghalang ini selama
empat task** — karena keempatnya hanya memeriksa pasangan yang dipakai `[AccessPermission]` pada
endpoint.

Butir kelima ditambahkan: **setiap pasangan yang dibaca service lewat `HasAccessAsync` wajib
terdaftar sebagai baris yang dapat dicentang.** Pasangannya dipindai langsung dari source modul,
karena pemanggilan semacam itu terjadi di dalam badan method dan tidak meninggalkan jejak pada
metadata tipe mana pun.

Uji itu juga menolak lulus secara semu: ia gagal bila tidak menemukan satu pun pemanggilan
`HasAccessAsync`, yang berarti pola pemindaiannya sudah tidak cocok.

### 1.3 Yang masih perlu dikerjakan Administrator

Penandanya kini **dapat diberikan**, tetapi **belum diberikan kepada siapa pun**. Administrator
perlu mencentang `RadReport : ActAsRadiologist` pada peran yang memang dokter radiolog.

`RAD-PERM-001` bagian 6 menyebut ini eksplisit: nama peran Quilvian yang memegangnya belum
ditetapkan, dan itu pekerjaan Administrator saat menyusun peran. Disiplin pemberiannya juga
ditetapkan di sana — penanda ini **tidak boleh** masuk paket hak akses umum.

---

## 2. Menjalankan dua migration — **DISERAHKAN SEBAGAI SKRIP SQL**

### Keputusan

Connection string aplikasi mengarah ke database **`QuilvianNewDevYoga` pada host remote**, bukan
database lokal. Aturan pemanggilan yang mengikat pelaksana berbunyi tegas: *"jangan menerapkan
migration ke database non-lokal"*.

**Keputusan pemilik modul: skrip SQL diserahkan, penerapannya dilakukan pemilik modul atau DBA.**

Alasannya bukan sekadar kepatuhan. Server itu remote dan mungkin dipakai orang lain; skrip yang
dapat dibaca lebih dulu memberi kesempatan memeriksanya sebelum satu baris pun berubah.

### Yang diserahkan

`Migrations/scripts/20260911_AddRadReport_AddRadOrderUrgency.sql`

| Sifat | Keadaan |
| --- | --- |
| Migration yang tercakup | `20260911025734_AddRadReport` dan `20260911045053_AddRadOrderUrgency` |
| Tabel dibuat | `public."RadReport"`, `public."RadReportVersion"` |
| Tabel diubah | `public."RadOrder"` — tiga kolom penanda cito |
| Index dibuat | Sembilan; tiga unik, dua difilter `"IsDelete" = false` |
| **`DROP`, `TRUNCATE`, `DELETE`** | **Tidak ada satu pun** |
| Tabel lain yang tersentuh | **Tidak ada** |
| Idempoten | Ya — 18 penjaga `__EFMigrationsHistory`; dijalankan dua kali tidak mengubah apa pun pada kali kedua |
| Transaksi | Seluruhnya dalam satu `START TRANSACTION` … `COMMIT`; satu perintah gagal membatalkan semuanya |

**Satu skrip untuk dua migration, dan itu disengaja.** Keduanya diterapkan bersamaan: daftar
kerja Radiologi membaca kolom yang dibuat `AddRadOrderUrgency`, sehingga `GET /worklist` akan
gagal bila hanya `AddRadReport` yang diterapkan — gagal tanpa ada yang menyadari sebabnya.

### Cara menjalankan

```bash
psql -h <host> -U <user> -d <database> \
  -f Migrations/scripts/20260911_AddRadReport_AddRadOrderUrgency.sql
```

### Yang perlu diperiksa sesudahnya

Empat butir tercantum pada `Migrations/scripts/README.md`. Yang paling cepat dan paling menjawab:

```sql
SELECT COUNT(*) FROM public."RadOrder" WHERE "IsUrgent" IS NOT FALSE;
```

Hasilnya harus `0` — seluruh pesanan lama menjadi tidak-cito, sesuai Definition of Done
`BE-RAD-12`.

### Yang belum terbukti, dan perlu diketahui

`Down()` kedua migration ada, terbaca benar, dan terkompilasi — tetapi **belum pernah dijalankan
sama sekali**. `Down()` `AddRadReport` menjatuhkan kedua tabel hasil bacaan **beserta seluruh
isinya**, sehingga pemulihan hanya aman selama belum ada bacaan yang ditulis.

---

## 3. Lima amandemen kontrak — **DISETUJUI**

| Kontrak | Revisi | Isi pokok |
| --- | --- | --- |
| `RAD-API-001` | 6 | Grup *Rad Report* berjalan untuk delapan endpoint; dua endpoint baseline ditambahkan; `RadReportValidateRequest` tidak dibuat; `POST .../draft` menjawab `200` bukan `201` |
| `RAD-API-001` | 7 | `GET /{id}/versions` dan `POST /{id}/amendments` berjalan; `WorkingVersionNumber` ditambahkan |
| `RAD-API-001` | 8 | **Perubahan perilaku**: `GET /by-encounter` hanya mengembalikan bacaan yang sudah dirilis |
| `RAD-API-001` | 9 | `POST /` menerima `IsUrgent`; rincian memuat jejak penandaannya |
| `RAD-API-001` | 10 | `GET /worklist` dan `PUT /{id}/urgency` berjalan; bawaan `date` adalah hari ini menurut WIB |
| `RAD-PERM-001` | 6, 7, 8 | Grup *Rad Report* dan dua baris terakhir *Rad Order* berjalan; tidak ada string hak akses baru |

### Satu keputusan bentuk yang ikut disetujui

`transaction-endpoint-standard.md` menyatakan `PUT /{id}/<aksi>` **dilarang**, sedangkan kontrak
menuliskan `PUT /{id}/urgency`.

**Keputusan: `PUT` dipertahankan.** Menyeragamkan `urgency` saja akan membuat satu endpoint
berbeda sendiri dari sebelas endpoint aksi lain pada controller yang sama. Permukaan yang tidak
dapat ditebak lebih mahal bagi pembuat layar daripada selisih terhadap standar yang sudah
dicatat terang-terangan pada `RAD-API-001` revision 10.

Penyeragaman seluruh `RadOrderController`, bila kelak diinginkan, menjadi task tersendiri yang
menyentuh dua belas endpoint beserta konsumennya sekaligus.

---

## 4. Kalimat `RAD-STATE-001` bagian 3 — **DIPERBAIKI**

### Yang keliru

Baris "Tulis draf koreksi" berbunyi *"Versi lama menjadi `Superseded`"*, seolah perpindahan itu
terjadi saat draf koreksi **ditulis**. Bagian 4 dokumen yang sama, contoh `FR-RAD-020`, dan
matriks uji penerimaan seluruhnya menyatakan sebaliknya: saat koreksi **dirilis**.

### Mengapa ini bukan perkara redaksi

> Antara "koreksi mulai ditulis" dan "koreksi dirilis" bisa ada jeda berjam-jam. Selama jeda itu
> versi lama adalah **satu-satunya bacaan yang sah** — sudah diperiksa dokter radiolog dan sudah
> dirilis. Draf koreksi belum diperiksa siapa pun.
>
> Implementer yang membaca bagian 3 apa adanya akan memensiunkan versi lama begitu draf koreksi
> ditulis. Dokter jaga yang membuka hasil pukul 10 malam kemudian melihat salah satu dari dua
> hal: draf yang belum disahkan, atau tidak ada bacaan berlaku sama sekali.

### Yang diperbaiki

`RAD-STATE-001` naik ke revision 2. Barisnya kini berbunyi *"Versi lama tetap `Released` dan
tetap berlaku sampai koreksinya dirilis"*, dengan catatan di bawah tabel yang menjelaskan
sebabnya dan menyebut bahwa penerapannya sudah benar sejak `BE-RAD-10`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, **0 error**, 188 warning, 2 menit 2 detik | `PASS` |
| `dotnet build` project uji in-memory | Berhasil, **0 error** | `PASS` |
| `dotnet test --no-build --filter RadiologyRoleAccessContractTests` | **27 lulus, 0 gagal** — 23 sebelumnya + 4 baru | `PASS` |
| `dotnet test --no-build --filter RadiologyManagement` | **287 lulus, 0 gagal** | `PASS` |
| `dotnet test --no-build` seluruh project in-memory | **1.492 lulus, 0 gagal** | `PASS` |
| Isi skrip SQL — tabel yang disentuh | Hanya `RadReport`, `RadReportVersion`, `RadOrder` | `PASS` |
| Isi skrip SQL — `DROP`, `TRUNCATE`, `DELETE` | **Nol** | `PASS` |
| Isi skrip SQL — penjaga idempotent dan transaksi | 18 penjaga; satu `START TRANSACTION` … `COMMIT` | `PASS` |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| `dotnet ef database update` | **Keputusan bagian 2** — targetnya database non-lokal; skrip SQL diserahkan sebagai gantinya |
| Uji integrasi Postgres radiologi | Tabelnya belum ada sampai skrip dijalankan |
| Pemberian penanda `ActAsRadiologist` kepada peran | Pekerjaan Administrator, bukan backend — bagian 1.3 |
| Analyzer build penuh | Dimatikan atas permintaan pemilik modul; warning compiler tetap disaring |

---

## 6. Keadaan setelah dokumen ini

| Yang dulu menggantung | Sekarang |
| --- | --- |
| Penanda `ActAsRadiologist` tidak dapat diberikan | **Ditutup.** Terdaftar dan dapat dicentang; tinggal diberikan Administrator |
| Dua migration belum dijalankan | **Skrip SQL siap.** Penerapannya di tangan pemilik modul atau DBA |
| Lima amandemen kontrak menunggu konfirmasi | **Disetujui** dan tercatat pada masing-masing kontrak |
| Kalimat `RAD-STATE-001` bagian 3 menyesatkan | **Diperbaiki**, revision 2 |

**Yang tersisa di luar backend:**

| Butir | Pemiliknya |
| --- | --- |
| Menjalankan skrip SQL | Pemilik modul atau DBA |
| Memberikan `RadReport : ActAsRadiologist` kepada peran | Administrator |
| Mengesahkan dua belas draf aturan keselamatan — `DEC-RAD-005` | Penanggung jawab klinis. Sampai itu, gerbang keselamatan menolak seluruh pemeriksaan |
| Tiga belas task frontend `FE-RAD-01` s/d `FE-RAD-13` | Frontend |

**Dua usulan yang sudah berulang kali dilaporkan dan masih terbuka:**

1. **Squash migration.** Folder `Migrations` kini 404 MB. Setiap migration menyalin ulang model
   1.309 entity ke satu Designer ±4,4 MB, dan `dotnet ef migrations add` karena itu berjalan
   lebih dari sepuluh menit.
2. **`RadReport.Version` dan `RadReportVersion.Version`** didokumentasikan sebagai token
   konkurensi pada modelnya, tetapi configuration `BE-RAD-07` **tidak** memanggil
   `IsConcurrencyToken()`. Memperbaikinya mengubah model EF, sehingga menuntut migration baru —
   dan itu sebabnya belum dikerjakan.

---

## 7. Status Git

Berkas hasil pekerjaan ini:

```text
 M Seeders/AccessMenuSeeder.cs
 M Migrations/scripts/README.md
 M docs/module-blueprints/radiologi/contracts/api-contract.md
 M docs/module-blueprints/radiologi/contracts/permission-audit-matrix.md
 M docs/module-blueprints/radiologi/contracts/state-transition-matrix.md
?? Migrations/scripts/20260911_AddRadReport_AddRadOrderUrgency.sql
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadiologyRoleAccessContractTests.cs
?? docs/module-blueprints/radiologi/approval-requests/2026-09-11-keputusan-empat-penghalang.md
```

`RadiologyRoleAccessContractTests.cs` **disunting** pekerjaan ini tetapi tetap tampil `??` karena
belum pernah di-commit sejak `BE-RAD-14`.

Tidak ada `git add`, commit, maupun push yang dilakukan. **Tidak ada perintah database yang
dijalankan.**
