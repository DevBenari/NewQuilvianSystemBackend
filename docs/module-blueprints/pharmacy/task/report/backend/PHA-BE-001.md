# PHA-BE-001 — Resolver Routing Depo Farmasi

## Ringkasan untuk Pembaca Umum

Satu rumah sakit punya banyak tempat penyimpanan obat: gudang utama, ruang karantina, depo
poliklinik, depo IGD, depo rawat inap. Ketika sebuah resep muncul, seseorang harus memutuskan
depo mana yang melayaninya. Kalau keputusan itu diambil manual, dua hal buruk mungkin terjadi:
obat diambil dari gudang utama yang seharusnya tidak melayani pasien, atau dua depo merasa sama
berhak dan stoknya dihitung dua kali.

Task ini membangun penentunya. Diberi satu kunjungan pasien, ia menjawab **tepat satu** depo,
atau menolak dengan alasan yang jelas. Aturannya mengikuti jenis layanan: rawat jalan mencari
depo kliniknya dulu, baru depo unit layanannya; IGD mencari depo bertipe darurat pada unit yang
sama; rawat inap mencari depo bertipe farmasi pada unit yang sama. Gudang utama, ruang karantina,
lokasi non-farmasi, lokasi nonaktif, dan lokasi yang memang tidak boleh menyerahkan obat selalu
dikeluarkan lebih dulu.

Dua sikap disengaja. Pertama, ketika kandidatnya lebih dari satu, ia **menolak** alih-alih
memilih yang pertama — konfigurasi ganda adalah kesalahan administrator yang harus terlihat,
bukan ditutupi dengan tebakan yang kebetulan benar. Kedua, ia **tidak mengubah data apa pun**:
ia hanya membaca. Jadi memanggilnya berkali-kali tidak pernah mengambil stok, tidak pernah
mengunci baris, dan tidak pernah mengubah keadaan resep.

---

- TASK ID: PHA-BE-001
- TASK TYPE: Fitur (resolver routing Depo Farmasi; baca saja, tanpa mutation)
- COMPLEXITY: LOW
- MODEL: Claude Opus 5
- TASK MODE: BACKEND
- SIFAT LAPORAN: **Retroaktif.** Source `PHA-BE-001` sudah berdiri sejak sebelum laporan ini
  ditulis, tetapi laporan task-nya tidak pernah dibuat, sehingga bukti acceptance-nya tidak
  tercatat — tercatat sebagai `stale evidence` pada `MODULE-STATUS.md`. Laporan ini **memeriksa
  source yang sudah ada** terhadap sembilan acceptance criteria, dan **tidak mengubah satu baris
  source pun**.
- TANGGAL VERIFIKASI: 5 Oktober 2026
- BACKEND SHA: `38142748a4d6b5e1c84156acf93681c593b0a06d` (branch `Ikbal`, isi identik dengan
  `QuilvianIntegrationBackend` `7787318d`)
- FILES INSPECTED:
  - `docs/module-blueprints/pharmacy/roadmap/README.md` (`PHA-BE-001` beserta sembilan acceptance
    criteria, bukti verifikasi, dan Definition of Done)
  - `Areas/HealthServices/PharmacyManagement/Services/PharmacyDepotRoutingService.cs` (142 baris)
  - `Areas/HealthServices/PharmacyManagement/DTOs/PharmacyDepotRoutingDtos.cs`
  - `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs`
  - `Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs`
  - `Program.cs` (registrasi DI, baris 547)
- FILES CHANGED: **nol berkas source.** Hanya laporan ini
  (`docs/module-blueprints/pharmacy/task/report/backend/PHA-BE-001.md`) dan pembaruan rujukan
  pada `MODULE-STATUS.md`.

## Acceptance criteria — pemeriksaan satu per satu

| # | Kriteria | Hasil | Bukti |
|---|---|---|---|
| 1 | Rawat Jalan memilih berdasarkan `ClinicId`, lalu `ServiceUnitId` bila tidak ada Clinic match | ✅ | `ResolveOutpatientAsync` baris 95–110: cabang klinik dijalankan lebih dulu, fallback hanya terjadi bila hasilnya `PHA_ROUTE_NOT_FOUND` |
| 2 | IGD memilih `ServiceUnitId` sama dan `StorageLocationType = Emergency` | ✅ | baris 73–77 |
| 3 | Rawat Inap memilih `ServiceUnitId` sama dan `StorageLocationType = Pharmacy` | ✅ | baris 78–82 |
| 4 | Kandidat nonaktif, dihapus, Gudang Utama, karantina, non-Farmasi, atau tidak boleh dispensing dikeluarkan | ✅ | baris 55–64: `IsActive`, `!IsDelete`, `!IsCancel`, `IsPharmacyLocation`, `IsAllowDispensing`, `!IsMainWarehouse`, `!IsQuarantineLocation` — enam syarat yang diminta, plus `!IsCancel` sebagai tambahan |
| 5 | Nol kandidat menghasilkan `PHA_ROUTE_NOT_FOUND` | ✅ | baris 135–139 |
| 6 | Lebih dari satu kandidat pada prioritas sama menghasilkan `PHA_ROUTE_AMBIGUOUS`; **tidak memilih baris pertama** | ✅ | baris 118–133: `.Take(2)` lalu `Count > 1` → gagal. Karena hanya dua id diambil dan keduanya tidak pernah dipilih, mustahil baris pertama lolos diam-diam |
| 7 | Jenis encounter selain tiga layanan menghasilkan `PHA_ROUTE_SERVICE_UNSUPPORTED` | ✅ | baris 83–85, arm `_` pada `switch` |
| 8 | Memakai `AsNoTracking`, cancellation token, dan tidak melakukan `SaveChanges` | ✅ | `AsNoTracking` baris 34 dan 56; `cancellationToken` diteruskan ke seluruh `SingleOrDefaultAsync`/`ToListAsync`; pencarian `SaveChanges` pada berkas bernilai **0** |
| 9 | Build backend berhasil tanpa migration baru | ✅ | build `0 error`; nol migration bernuansa routing/depot pada `Migrations/` |

## Bukti verifikasi tambahan yang diminta task

| Yang diminta | Hasil |
|---|---|
| diff hanya menyentuh DTO/service/DI yang disetujui | ✅ tiga titik saja: service, DTO, `Program.cs:547` `AddScoped<PharmacyDepotRoutingService>()` |
| build berhasil | ✅ `0 error` |
| pemeriksaan query membuktikan tidak ada mutation | ✅ nol `SaveChanges`, nol `Add`/`Update`/`Remove`, kedua query `AsNoTracking` |
| tidak ada endpoint, tabel, migration, atau frontend baru | ✅ resolver **tidak dipanggil dari mana pun** — sesuai cakupan task, yang melarang mutation workflow. Pencarian pemakaiannya di luar berkasnya sendiri hanya menemukan resolver Human Resource yang tidak berkaitan |

## Dua penyimpangan dari teks task, keduanya dicatat apa adanya

**1. Model yang dipakai `RegPatientEncounter`, bukan `TrxPatientEncounter`.** Field `Reuse` pada
task menyebut `TrxPatientEncounter`. Kelas dengan nama itu **tidak ada** di repositori; yang ada
`Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs`. Implementasinya
memakai yang benar-benar ada. Yang keliru teks task-nya, bukan source-nya, dan prefix `Reg`
memang milik Registration.

**2. Ada satu error code di luar sembilan kriteria: `PHA_ROUTE_ENCOUNTER_INVALID`.** Dipakai dua
kali — ketika `encounterId` kosong (baris 26–31) dan ketika encounter-nya tidak ditemukan atau
sudah tidak aktif (baris 48–53). Sembilan kriteria tidak menyebut keadaan ini sama sekali.
Sifatnya **menambah**, bukan mengubah: tidak satu pun dari tujuh code yang diminta berubah arti.
Tanpa code ini, encounter yang tidak sah akan jatuh ke `PHA_ROUTE_SERVICE_UNSUPPORTED` dan
menyalahkan jenis layanan atas masalah yang sebenarnya pada kunjungannya. Dicatat sebagai
penambahan yang perlu diakui pemilik kontrak, bukan diselundupkan.

## Yang laporan ini TIDAK mengklaim

`PHA-BE-002` — **pengujian otomatis resolver** — **masih terbuka**. Resolver ini belum punya satu
pun uji: pencarian `DepotRouting` pada `Tests/` hanya menemukan berkas biner hasil build, bukan
berkas uji. Itu sesuai Definition of Done `PHA-BE-001` sendiri, yang menyatakan "bukti perilaku
otomatis tetap menjadi DoD `PHA-BE-002`, bukan diklaim selesai oleh task ini".

Jadi yang ditutup laporan ini adalah **bukti acceptance by inspection**, bukan bukti perilaku
otomatis. Keduanya berbeda dan sengaja tidak dicampur.

- API CONTRACT IMPACT: **Nol endpoint**. Resolver adalah service internal.
- DATABASE IMPACT: **Nol**. Baca saja, dua query `AsNoTracking`, nol migration.
- SECURITY IMPACT: Menutup kemungkinan obat dilayani dari Gudang Utama atau ruang karantina
  lewat jalur routing, dan menolak konfigurasi depo ganda alih-alih memilih sembarang.
- VALIDATION: sembilan acceptance criteria terpenuhi by inspection; build `0 error`; suite
  Farmasi 173/173 lulus (tidak satu pun menyentuh resolver — lihat bagian di atas).
- WARNINGS: resolver sudah terdaftar di DI tetapi belum dikonsumsi siapa pun. Itu **benar** untuk
  sekarang, karena `PHA-BE-003` yang menyambungkannya ke workflow masih `BLOCKED` oleh
  `PHA-OQ-014`/`PHA-OQ-015`. Perlu diingat agar tidak dibaca sebagai kode mati.
- KNOWN ISSUES / OPEN QUESTION:
  1. pemilik kontrak perlu mengakui `PHA_ROUTE_ENCOUNTER_INVALID` sebagai code kedelapan;
  2. teks `Reuse` pada task perlu dikoreksi dari `TrxPatientEncounter` ke `RegPatientEncounter`;
  3. `PHA-BE-002` belum dikerjakan.
- NEXT TASKS: `PHA-BE-002` (uji otomatis resolver — dapat dikerjakan sekarang, tanpa dependency),
  lalu `PHA-BE-003` setelah `PHA-OQ-014`/`PHA-OQ-015` terkunci.
