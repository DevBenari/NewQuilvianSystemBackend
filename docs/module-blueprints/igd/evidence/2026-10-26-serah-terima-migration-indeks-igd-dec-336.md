# Serah terima migration indeks `IX_TrxDoctorConsultation_EncounterId` — `IGD-DEC-336`

| Field | Nilai |
| --- | --- |
| Dasar | `IGD-DEC-336` (`IGD-OQ-139` dijawab): indeks unik **dipersempit**, bukan dilepas uniknya — filternya ditambah `ClinicId IS NOT NULL` |
| Penghalang | **Sudah hilang** — merge `MHamzah` terjadi 23 Oktober 2026 |
| Yang sudah saya kerjakan | `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` sudah disunting ke bentuk yang disepakati (satu baris, nol komentar baru) |
| Yang menunggu Rizki | (1) dua kueri pra-cek di bawah, (2) `dotnet ef migrations add`, (3) `dotnet build` |
| Koordinasi | **Muhammad Hamzah** — indeks ini lahir dari rantai keputusan Rawat Inap (`IGD-FACT-178`) |

---

## 1. Keadaan yang sedang disatukan

Sebelum pass ini keadaannya bercabang tiga, dan itulah yang membuat `IGD-DEC-336` menahan commit:

| Lapisan | Keadaan sebelum | Akibat |
| --- | --- | --- |
| Kode controller | Mengizinkan lebih dari satu catatan dokter per kunjungan IGD berjalan, dengan penjaga **satu draf terbuka per dokter** (`IGD-DEC-337`) | — |
| Konfigurasi EF dan snapshot | Indeks masih **UNIK** tanpa penyaring `ClinicId` | Scaffolding migration siapa pun akan membawa perubahan indeks liar |
| Basis data dev | btree biasa — unik dilepas **manual**, tanpa migration | Dev tidak mewakili lingkungan lain |
| Staging dan produksi | **MASIH UNIK** | Catatan dokter kedua pada kunjungan IGD ditolak **basis data** sebagai `500`, bukan `400` yang rapi |

Sesudah migration ini diterapkan, keempat lapisan sepakat.

## 2. Perubahan konfigurasi yang sudah dikerjakan

`Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` baris 294–296:

```csharp
entity.HasIndex(x => x.EncounterId)
    .IsUnique()
    .HasFilter("\"IsDelete\" = false AND \"InpEpisodeId\" IS NULL AND \"ClinicId\" IS NOT NULL");
```

Yang berubah hanya ruas `HasFilter` — penambahan `AND "ClinicId" IS NOT NULL`. Nol komentar
ditambah, nol komentar lama disunting, nol baris lain tersentuh.

### 2.1 Dua hal yang perlu Anda putuskan sebelum commit

**(a) Komentar `BE-RWI-043` di atas indeks itu menjadi usang.** Bunyinya sekarang:

> *"Batas satu catatan dokter per kunjungan tetap dijaga database untuk rawat jalan, medical
> check-up, dan IGD; ia hanya dilepas bagi catatan yang menempel pada perawatan rawat inap."*

Sesudah perubahan ini, IGD **tidak lagi** dijaga database. Saya **tidak menyuntingnya** karena
komentar lama tidak boleh disentuh. Keputusannya milik Anda bersama Hamzah: biarkan apa adanya,
atau izinkan satu suntingan komentar pada commit yang sama.

**(b) Medical check-up dan telemedicine kemungkinan ikut terbebas.** `ClinicId` diisi
`queue?.ClinicId ?? encounter.ClinicId` (`DoctorConsultationController.cs:604`). Penyaring
`ClinicId IS NOT NULL` karena itu membebaskan **setiap** jenis kunjungan yang `ClinicId`-nya
kosong, bukan hanya `Emergency`. Lapisan aplikasi tetap menjaga mereka — cabang terakhir
`ValidateSingleConsultationRuleAsync` menolak dengan *"Konsultasi dokter untuk encounter ini sudah
ada."* — tetapi lapisan basis datanya hilang. Kueri 2 di bawah mengukur berapa banyak yang
terdampak, supaya keputusannya berdasar angka, bukan dugaan.

## 3. Kueri pra-cek — mohon dijalankan, saya dilarang

Keduanya **read-only**. Jalankan di setiap lingkungan sebelum migration diterapkan di sana
(dev, staging, produksi), sesuai syarat `IGD-DEC-336`.

### Kueri 1 — penghalang migration: duplikat terhadap indeks yang BARU

Harus menjawab **0 baris**. Bila ada baris, migration akan gagal saat membuat indeks.

```sql
SELECT c."EncounterId", COUNT(*) AS jumlah_catatan
FROM public."TrxDoctorConsultation" c
WHERE c."IsDelete" = false
  AND c."InpEpisodeId" IS NULL
  AND c."ClinicId" IS NOT NULL
GROUP BY c."EncounterId"
HAVING COUNT(*) > 1
ORDER BY jumlah_catatan DESC;
```

### Kueri 2 — cakupan pembebasan: siapa saja yang kehilangan penjagaan basis data

Bukan penghalang; angkanya menentukan butir 2.1(b).

```sql
SELECT e."EncounterType",
       COUNT(*) AS baris_catatan,
       COUNT(DISTINCT c."EncounterId") AS jumlah_kunjungan
FROM public."TrxDoctorConsultation" c
JOIN public."RegPatientEncounter" e ON e."Id" = c."EncounterId"
WHERE c."IsDelete" = false
  AND c."InpEpisodeId" IS NULL
  AND c."ClinicId" IS NULL
GROUP BY e."EncounterType"
ORDER BY e."EncounterType";
```

`EncounterType`: `1` Outpatient, `2` Emergency, `3` Inpatient, `4` MedicalCheckup,
`5` Telemedicine. Yang diharapkan muncul hanya `2`. Bila `1` muncul, ada kunjungan rawat jalan
tanpa `ClinicId` dan penyaring ini **tidak** cukup menjaga rawat jalan — hentikan migration dan
kabari saya. Bila `4` atau `5` muncul, butir 2.1(b) perlu dijawab Hamzah lebih dulu.

### Kueri 3 — memastikan keadaan indeks nyata per lingkungan

Dev pernah dilepas uniknya secara manual; kueri ini memperlihatkan apa yang benar-benar ada.

```sql
SELECT indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename = 'TrxDoctorConsultation'
  AND indexname LIKE '%EncounterId%';
```

## 4. Perintah migration — mohon dijalankan

Dijalankan dari dalam `NewQuilvianSystemBackend/`, **sesudah** Kueri 1 menjawab 0 baris.

```bash
dotnet ef migrations add PersempitIndeksCatatanDokterPerKunjungan --context ApplicationDbContext
```

Lalu build, dengan analyzer dimatikan supaya tidak menembus batas RAM:

```bash
dotnet build -p:RunAnalyzers=false
```

### Yang wajib diperiksa pada migration hasil scaffolding

| # | Pemeriksaan | Yang diharapkan |
| ---: | --- | --- |
| 1 | Isi `Up()` | **Hanya** `DropIndex` + `CreateIndex` untuk `IX_TrxDoctorConsultation_EncounterId`, dengan filter baru memuat `AND "ClinicId" IS NOT NULL` |
| 2 | Isi `Down()` | Mengembalikan filter lama `"IsDelete" = false AND "InpEpisodeId" IS NULL` — bukan menghapus indeksnya |
| 3 | Berkas lain yang ikut berubah | `ApplicationDbContextModelSnapshot.cs` **wajib** ikut berubah pada indeks yang sama |
| 4 | Perubahan liar | **Nol** tabel atau kolom lain di `Up()`. Bila ada, hentikan — artinya snapshot menyimpan selisih dari modul lain, dan itu persoalan terpisah |

Bila butir 4 memunculkan perubahan asing, jangan dipaksakan: itu gejala yang pernah kita temui
pada snapshot, dan menumpanginya akan menyeret entitas modul lain ke dalam commit IGD.

## 5. Commit sepaket

`IGD-DEC-336` syarat (2): migration berangkat **bersamaan** dengan perubahan controller. Merilis
controller lebih dulu ke lingkungan yang indeks uniknya masih utuh akan mengubah penolakan rapi
`400` menjadi `500`. Satu commit memuat empat hal:

| # | Berkas |
| ---: | --- |
| 1 | `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` — penjaga per dokter (`IGD-DEC-337`), **sudah ada di `3eac45aa`** |
| 2 | `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` — sudah disunting pass ini |
| 3 | `Migrations/<stempel>_PersempitIndeksCatatanDokterPerKunjungan.cs` — hasil perintah bagian 4 |
| 4 | `Migrations/ApplicationDbContextModelSnapshot.cs` — ikut berubah oleh perintah yang sama |

Karena butir 1 sudah ter-commit lebih dulu di `3eac45aa`, yang penting sekarang adalah **urutan
rilis**, bukan urutan commit: migration ini harus **sudah** diterapkan di staging dan produksi
sebelum atau bersamaan dengan rilis yang membawa `3eac45aa`.

## 6. Urutan penerapan per lingkungan

| Urutan | Lingkungan | Langkah |
| ---: | --- | --- |
| 1 | dev | Kueri 1–3 → `migrations add` → `dotnet build` → `dotnet ef database update`. Dev sudah btree biasa, jadi `DropIndex` atas indeks unik mungkin tidak menemukan sasarannya — periksa hasil Kueri 3 lebih dulu dan laporkan bila migration perlu dibuat idempoten |
| 2 | staging | Kueri 1 ulang → `database update` → Kueri 3 untuk memastikan filternya terpasang |
| 3 | produksi | Kueri 1 ulang → `database update` pada jendela rilis yang sama dengan `3eac45aa` |

Catatan dev pada urutan 1 bukan formalitas: dev-nya sudah dilepas manual tanpa migration, sehingga
`Up()` hasil scaffolding menggambarkan keadaan **snapshot**, bukan keadaan dev yang sebenarnya.
Itulah satu-satunya tempat perintah ini berpeluang gagal.
