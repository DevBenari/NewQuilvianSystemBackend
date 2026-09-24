# Audit `BE-IGD-044` terhadap `IGD-OQ-092` — sesudah migration diterapkan

| Field | Nilai |
| --- | --- |
| Tanggal | 17 September 2026 |
| Jenis | **Audit read-only.** Nol perubahan source, nol perubahan database, nol corrective migration, nol migration disunting |
| Pemicu | Perintah pemilik sesudah `AddEmergencyDoctorAssignment` diterapkan ke basis data development |
| Migration diaudit | `20260917072515_AddEmergencyDoctorAssignment` — **applied**, tidak disentuh |
| Backend | branch `rizkiG` `a5f4f4b8` + working tree (migration dan snapshot belum di-commit) |
| Batasan dipatuhi | Migration yang sudah applied **tidak** dijalankan, dihapus, disunting, atau diregenerasi. Nol kueri basis data oleh agent. `BE-IGD-045` **tidak** dimulai |

---

## A. Kesimpulan eksekutif

**Schema-nya benar. Datanya kosong.**

Migration yang diterapkan membuat tabel, kolom, index, dan foreign key persis seperti kamus data
§4 dan acceptance 1–3. Yang **tidak** ada di dalamnya adalah **pengisian data lama**: `Up()`
hanya memuat `CreateTable` dan lima `CreateIndex`, dengan **nol** `InsertData` dan **nol**
`migrationBuilder.Sql`.

Akibatnya `IGD-OQ-092` **tidak pernah dieksekusi**. Tidak ada pelaku yang dipilih, tidak ada
`UpdateBy`/`CreateBy` yang dipakai sebagai pelaku historis, dan tidak ada baris yang dilewati —
karena tidak ada satu pun baris yang pernah dicoba dimasukkan.

Itu kabar baik dan kabar buruk sekaligus. Baiknya: **nol data salah tersimpan**, dan risiko
terburuk `IGD-OQ-092` — mengarang pelaku, atau migration gagal di tengah karena foreign key —
tidak terjadi. Buruknya: **acceptance 4 belum terpenuhi sama sekali**, dan tabelnya kosong.

## B. Schema sebagaimana benar-benar diterapkan

| Kolom | Tipe terapan | Nullable | Sesuai kamus data §4 |
| --- | --- | :-: | :-: |
| `Id` | `uuid` PK | tidak | ✅ |
| `EmergencyVisitId` | `uuid` | tidak | ✅ |
| `DoctorId` | `uuid` | tidak | ✅ |
| `EffectiveFrom` | `timestamp with time zone` | tidak | ⚠️ lihat F.1 |
| `EffectiveTo` | `timestamp with time zone` | **ya** | ✅ |
| `AssignedByUserId` | `uuid` | **tidak** | ✅ |
| `AssignmentReason` | `character varying(500)` | ya | ✅ |
| `IsActive` | — | — | ✅ **tidak ada**, sesuai `IGD-DEC-130` |

Ditambah delapan kolom audit bawaan `IdentityModel`.

**Foreign key:** `AspNetUsers.Id` (`AssignedByUserId`), `EmgVisit.Id` (`EmergencyVisitId`),
`MstDoctor.Id` (`DoctorId`) — ketiganya `Restrict`.

**Index:**

| Nama | Bentuk | Asal |
| --- | --- | --- |
| `IX_EmgDoctorAssignment_EmergencyVisitId_Active` | **unique**, filter `"EffectiveTo" IS NULL` | Kamus data §4 |
| `IX_EmgDoctorAssignment_EmergencyVisitId_EffectiveFrom` | komposit | Kamus data §4 |
| `IX_EmgDoctorAssignment_DoctorId` | tunggal | Ditambahkan `BE-IGD-044` |
| `IX_EmgDoctorAssignment_EffectiveTo` | tunggal | Ditambahkan `BE-IGD-044` |
| `IX_EmgDoctorAssignment_AssignedByUserId` | tunggal | **Dibuat EF otomatis** untuk FK |

## C. Jawaban atas ketujuh pertanyaan audit

| # | Pertanyaan | Temuan |
| ---: | --- | --- |
| 1 | Nullability `AssignedByUserId` pada model, configuration, migration | ✅ **Konsisten bertiga.** Model `[Required] Guid` (value type, non-nullable); configuration tidak menimpanya; migration `nullable: false`. Sesuai kamus data §4 "Wajib" |
| 2 | Perlakuan backfill terhadap `RegPatientEncounter.DoctorId` | ⛔ **Tidak ada perlakuan apa pun.** Backfill tidak ada di migration |
| 3 | Apakah seluruh encounter lama ber-`DoctorId` memperoleh baris | ⛔ **Tidak ada satu pun.** Nol `InsertData`, nol `Sql` di `Up()` — tabelnya kosong sejak dibuat |
| 4 | Apakah ada encounter dilewati karena `CreateBy`/`UpdateBy` tidak valid | ➖ **Tidak berlaku.** Tidak ada baris yang pernah dicoba dimasukkan, jadi tidak ada yang dilewati |
| 5 | Apakah `UpdateBy`/`CreateBy` dipakai sebagai pelaku historis tanpa bukti | ✅ **Tidak.** Risiko ini **tidak terjadi**. Nol pelaku dikarang |
| 6 | Apakah constraint/index `EffectiveTo IS NULL` sudah sesuai | ✅ **Sesuai.** `unique: true` dengan `filter: "EffectiveTo" IS NULL`, persis acceptance 3 dan kamus data §4. Penyaringnya **tidak** menyertakan `IsDelete` — disengaja, sesuai pelajaran `BE-IGD-047` |
| 7 | Konsistensi source, kamus data, roadmap, kontrak, laporan terhadap schema terapan | 🟡 **Sebagian.** Tiga delta dokumentasi di bagian F; nol delta perilaku |

## D. Acceptance `BE-IGD-044` sesudah migration diterapkan

| # | Kriteria | Sebelum audit | Sesudah audit |
| ---: | --- | --- | --- |
| 1 | Penamaan `EmgDoctorAssignment` | ✅ | ✅ |
| 2 | Kolom sesuai §4, nol `IsActive` | ✅ | ✅ |
| 3 | Unique bersyarat `EffectiveTo IS NULL` | ✅ | ✅ **dikuatkan** — kini ada di basis data, bukan hanya di configuration |
| 4 | Pengisian data lama | 🟡 | ⛔ **TIDAK TERPENUHI** — nol baris |
| 5 | Langkah mundur diuji di basis data terpisah | 🟡 | 🟡 **belum dikonfirmasi** — lihat G.2 |
| 6 | Snapshot hanya bertambah blok `EmgDoctorAssignment` | 🟡 | ✅ **TERPENUHI** — diff snapshot 102 baris tambahan, **nol penghapusan**; seluruh entity yang disebut adalah `EmgDoctorAssignment` beserta tiga navigasinya |

Acceptance 6 patut dicatat: modul ini pernah kehilangan blok modul lain lewat snapshot. Kali ini
tidak terjadi, walaupun ada merge dari `QuilvianIntegrationBackend` (`a5f4f4b8`) yang menyentuh
snapshot 77 baris sebelum migration dibuat.

## E. Data lama yang terdampak

Agent **tidak menjalankan kueri basis data**. Yang pasti dari pembacaan migration: **jumlah baris
`EmgDoctorAssignment` hari ini adalah nol**, karena tidak ada jalur mana pun yang pernah
menulisinya — `BE-IGD-045` belum dibuat, dan nol kode lain menyentuh `DbSet` itu.

Yang **belum** diketahui hanyalah besar celahnya, dan itu menuntut satu kueri milik pemilik.
Kueri disediakan di bagian G.1.

### E.1 Celahnya bertambah setiap hari, bukan tetap

Penetapan dokter lewat layar hari ini memanggil `PATCH .../{encounterId}/doctor` milik
Registrasi, dan itu **hanya** mengisi `RegPatientEncounter.DoctorId`. Nol baris
`EmgDoctorAssignment` dibuat.

Artinya penetapan dr. Rendy Pangalila pada uji layar pagi ini **juga tidak punya baris riwayat**.
Himpunan yang perlu diisi ulang **terus bertambah** sampai `FE-IGD-027` mengalihkan layar ke
endpoint baru. Semakin lama jaraknya, semakin besar pekerjaan pengisian data lamanya.

## F. Tiga delta dokumentasi — tidak satu pun mengubah perilaku

| # | Delta | Keadaan |
| ---: | --- | --- |
| 1 | Kamus data §4 menulis `EffectiveFrom` bertipe `timestamp`; terapannya `timestamp with time zone` | Bawaan Npgsql untuk `DateTime`, dan sama dengan seluruh kolom waktu modul IGD lain. **Bukan cacat** — kamus datanya yang perlu menyebut zona waktu agar tidak menyesatkan |
| 2 | Kamus data §4 menyebut dua index; terapannya lima | Dua tambahan (`DoctorId`, `EffectiveTo`) sengaja dibuat `BE-IGD-044`, satu (`AssignedByUserId`) dibuat EF otomatis untuk FK. Perlu dicatat di kamus data supaya audit berikutnya tidak menganggapnya liar |
| 3 | Laporan `BE-IGD-044` dan roadmap menulis acceptance 4–6 "menunggu migration Rizki" | Sudah usang: acceptance 6 kini ✅, acceptance 4 ⛔, acceptance 5 belum dikonfirmasi |

## G. Yang dibutuhkan untuk menutup `IGD-OQ-092`

### G.1 Kueri untuk pemilik — mengukur celah dan menguji ketiga pilihan sekaligus

Dijalankan pemilik di DBeaver pada `QuilvianNewDevRizki`. **Read-only.**

```sql
SELECT
    COUNT(*)                                              AS encounter_berdokter,
    COUNT(*) FILTER (WHERE u_upd."Id" IS NOT NULL)        AS pelaku_dari_updateby,
    COUNT(*) FILTER (WHERE u_upd."Id" IS NULL
                       AND u_crt."Id" IS NOT NULL)        AS pelaku_dari_createby,
    COUNT(*) FILTER (WHERE u_upd."Id" IS NULL
                       AND u_crt."Id" IS NULL)            AS tanpa_pelaku_sah
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
LEFT JOIN public."AspNetUsers" u_upd ON u_upd."Id" = NULLIF(e."UpdateBy", '00000000-0000-0000-0000-000000000000'::uuid)
LEFT JOIN public."AspNetUsers" u_crt ON u_crt."Id" = NULLIF(e."CreateBy", '00000000-0000-0000-0000-000000000000'::uuid)
WHERE v."EncounterId" IS NOT NULL
  AND e."DoctorId" IS NOT NULL
  AND NOT v."IsDelete"
  AND NOT e."IsDelete";
```

Cara membacanya:

| Kolom | Artinya |
| --- | --- |
| `encounter_berdokter` | Besar himpunan yang seharusnya punya baris menurut acceptance 4 |
| `pelaku_dari_updateby` + `pelaku_dari_createby` | Baris yang pilihan **(b)** dapat isi tanpa mengarang apa pun |
| `tanpa_pelaku_sah` | **Baris yang akan dilewati pilihan (b)** — dan justru inilah angka yang menentukan apakah (b) cukup |

Bila `tanpa_pelaku_sah` bernilai **0**, ketiga pilihan `IGD-OQ-092` menghasilkan data yang sama,
dan keputusannya menjadi sepele: pakai **(b)**, acceptance 4 terpenuhi penuh tanpa pelaku
dikarang, dan pertanyaannya ditutup.

Sanity check tambahan, wajib bernilai nol sebelum pengisian dijalankan:

```sql
SELECT COUNT(*) AS baris_sekarang FROM public."EmgDoctorAssignment";
```

### G.2 Yang perlu dikonfirmasi pemilik, bukan diaudit dari berkas

| Hal | Pertanyaan |
| --- | --- |
| Acceptance 5 | Apakah langkah mundur (`Down()` = `DropTable`) sudah **diuji di basis data terpisah**, bukan hanya diterapkan maju di development? Ini syarat selesai yang sama dengan yang menahan `BE-IGD-026` dan `BE-IGD-031` |
| `IGD-OQ-092` | Pilihan (a), (b), atau (c) — lihat laporan `BE-IGD-044` bagian 6 |

## H. Klasifikasi perbaikan

Pertanyaannya: apakah menutup celah ini butuh corrective migration, atau cukup source/kontrak?

| Lapisan | Perlu diubah? | Alasan |
| --- | :-: | --- |
| **Schema tabel** | **TIDAK** | Kolom, tipe, nullability, FK, dan index sudah benar dan sesuai kontrak. Tidak ada yang perlu di-`ALTER` |
| **Source aplikasi** | TIDAK untuk celah ini | Model dan configuration sudah benar. `BE-IGD-045` tetap dibutuhkan, tetapi itu task tersendiri, bukan perbaikan |
| **Kontrak / kamus data** | **YA, kecil** | Tiga delta dokumentasi bagian F. Nol perubahan perilaku, nol kenaikan versi kontrak yang memutus |
| **Data** | **YA** | Nol baris; acceptance 4 menuntut pengisian data lama |

**Kesimpulan: dibutuhkan corrective migration baru, dan sifatnya data-only.** Bukan karena
schema-nya salah — schema-nya benar — melainkan karena pengisian data lama tidak pernah ikut
terbawa pada migration pertama.

Satu pengecualian yang mengubah kesimpulan ini: **bila pemilik memilih agar baris lama boleh
tidak punya pelaku sama sekali**, `AssignedByUserId` harus menjadi nullable. Itu **mengubah
schema** dan **mengubah kamus data §4** yang hari ini menyatakannya wajib — jadi bukan lagi
data-only, dan menuntut keputusan tersendiri. Pilihan itu **tidak** ada di antara (a), (b), (c)
`IGD-OQ-092` dan hanya disebut di sini supaya batasnya jelas.

## I. Bentuk corrective migration yang dibutuhkan — **JANGAN dijalankan**

Disusun sebagai rancangan, bukan perintah. Menunggu jawaban `IGD-OQ-092` dan hasil kueri G.1.

| Field | Isi |
| --- | --- |
| Nama usulan | `BackfillEmergencyDoctorAssignment` |
| Sifat | **Data-only.** Nol `AddColumn`, nol `AlterColumn`, nol `DropColumn`, nol perubahan index |
| `Up()` | Satu `migrationBuilder.Sql(...)` berisi `INSERT ... SELECT` sesuai pilihan `IGD-OQ-092` |
| `Down()` | Satu `migrationBuilder.Sql(...)` berisi `DELETE` yang **hanya** menghapus baris hasil pengisian ini |
| Idempotensi | Wajib. `WHERE NOT EXISTS` terhadap baris berjalan pada kunjungan yang sama, supaya menjalankannya dua kali tidak melanggar unique bersyarat |
| Prasyarat | Jawaban `IGD-OQ-092`; hasil kueri G.1; `baris_sekarang` = 0 |

### I.1 Bentuk `Up()` untuk pilihan (b) — rekomendasi

```sql
INSERT INTO public."EmgDoctorAssignment"
    ("Id", "EmergencyVisitId", "DoctorId", "EffectiveFrom", "EffectiveTo",
     "AssignedByUserId", "AssignmentReason",
     "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
     "IsCancel", "IsDelete")
SELECT
    gen_random_uuid(),
    v."Id",
    e."DoctorId",
    COALESCE(e."UpdateDateTime", e."CreateDateTime"),
    NULL,
    COALESCE(u_upd."Id", u_crt."Id"),
    NULL,
    NOW(),
    COALESCE(u_upd."Id", u_crt."Id"),
    '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000',
    FALSE, FALSE
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
LEFT JOIN public."AspNetUsers" u_upd ON u_upd."Id" = NULLIF(e."UpdateBy", '00000000-0000-0000-0000-000000000000'::uuid)
LEFT JOIN public."AspNetUsers" u_crt ON u_crt."Id" = NULLIF(e."CreateBy", '00000000-0000-0000-0000-000000000000'::uuid)
WHERE v."EncounterId" IS NOT NULL
  AND e."DoctorId" IS NOT NULL
  AND NOT v."IsDelete"
  AND NOT e."IsDelete"
  AND COALESCE(u_upd."Id", u_crt."Id") IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM public."EmgDoctorAssignment" a
      WHERE a."EmergencyVisitId" = v."Id" AND a."EffectiveTo" IS NULL
  );
```

`COALESCE(...) IS NOT NULL` inilah yang **melewati** baris tanpa pelaku sah — persis perilaku
pilihan (b), dan alasannya wajib dicatat pada laporan task beserta jumlah yang terlewati dari
kueri G.1.

### I.2 Bentuk `Down()`

`DELETE` yang dibatasi pada baris berjalan yang `AssignmentReason`-nya kosong **dan** waktu
pembuatannya sama dengan jendela migration ini. Menghapus seluruh baris berjalan tanpa pembatas
berbahaya: sesudah `BE-IGD-045` jalan, sebagian di antaranya adalah penugasan sungguhan yang
dibuat petugas, bukan hasil pengisian data lama.

Bentuk pastinya ditentukan bersama jawaban `IGD-OQ-092`, karena pembatasnya bergantung pada
nilai `AssignedByUserId` yang dipilih.

## J. Rekomendasi urutan

1. Pemilik menjalankan kueri **G.1** dan mengonfirmasi **G.2**.
2. Pemilik menjawab `IGD-OQ-092`.
3. Baru corrective migration `BackfillEmergencyDoctorAssignment` disusun — **oleh agent**,
   dijalankan **oleh Rizki**, mengikuti batas eksekusi yang sama dengan `BE-IGD-044`.
4. `BE-IGD-045` **boleh dimulai lebih dulu** dan tidak menunggu butir 1–3: tabelnya sudah ada,
   schema-nya sudah benar, dan endpoint-nya tidak bergantung pada data lama. Menundanya tidak
   memberi keuntungan apa pun.

Butir 4 adalah satu-satunya rekomendasi yang mengubah urutan kerja; sisanya menutup celah data.
