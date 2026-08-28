# Pemberitahuan — Snapshot Model Kehilangan Tiga Tabel Master

| Field | Value |
| --- | --- |
| `notice_id` | `RJ-BIL-NOTICE-001` |
| `tanggal` | 2026-08-27 |
| `pengaju` | Sukma Giri — Product/Domain Owner Rawat Jalan (`RJ-BIL-BP-001`) |
| `ditemukan pada` | `RJ-BIL-BE-007`, saat membangkitkan `20260827040349_AddBillingReconciliationCase` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |
| `status` | `dikirim` / pemberitahuan, bukan permintaan persetujuan |

Dokumen ini dapat diteruskan apa adanya kepada pemilik modul terkait.

---

## 1. Satu paragraf untuk yang tidak punya waktu

`ApplicationDbContextModelSnapshot.cs` kehilangan tiga entity master — `MstRegister`,
`MstRoomChargePolicy`, dan `MstTaxRule` — padahal ketiga tabelnya **sudah ada di database**.
Akibatnya EF menyangka ketiganya belum pernah dibuat, lalu menyisipkan `CreATE TABLE` untuk
ketiganya ke dalam migration siapa pun yang kebetulan dibangkitkan berikutnya. Migration itu lalu
gagal di setiap database yang sudah berjalan. Selama keadaan ini belum diperbaiki,
`dotnet ef database update` dan `Database.Migrate()` **gagal untuk seluruh tim**.

---

## 2. Bukti

### 2.1 Snapshot memang kehilangan ketiganya

| Entity | Ada di snapshot pada `HEAD` | Punya migration | Ada di database |
| --- | --- | --- | --- |
| `MstRoomChargePolicy` | **Tidak** | Ya — `20260820084721_AddTaxAndRoomChargePolicies` | Ya |
| `MstTaxRule` | **Tidak** | Ya — `20260820084721_AddTaxAndRoomChargePolicies` | Ya |
| `MstRegister` | **Tidak** | **Tidak ada, di migration mana pun** | Ya |

### 2.2 Tabelnya benar-benar sudah ada

Bukan kesimpulan, melainkan jawaban PostgreSQL ketika migration dijalankan apa adanya:

```
Npgsql.PostgresException : 42P07: relation "MstRegister" already exists
```

### 2.3 Penyebabnya: snapshot berayun antarbranch

Setiap migration menyimpan salinan penuh snapshot model pada `Designer.cs`-nya. Bila seorang
developer membangkitkan migration di branch yang belum memuat entity rekannya, snapshot itu
merekam model yang **tidak lengkap** — dan snapshot terakhirlah yang menjadi acuan berikutnya.

Jejaknya terbaca jelas pada `MstRoomChargePolicy`:

| Migration | Memuat entity itu? |
| --- | --- |
| `20260820084721_AddTaxAndRoomChargePolicies` | Ya |
| snapshot pada `HEAD` hari ini | **Tidak** |

Entity-nya tidak pernah dihapus siapa pun. Ia hanya tidak ikut tersalin ketika snapshot ditulis
ulang oleh migration dari branch lain.

---

## 3. Apa yang sudah diperbaiki dari sisi Rawat Jalan

Migration `20260827040349_AddBillingReconciliationCase` **hanya membuat dua tabel milik Billing**:
`BilReconciliationCase` dan `MstBillingReconciliationPolicy`.

Ketiga `CreateTable` untuk tabel master di atas **dibuang** dari migration, tetapi ketiga entity
tetap ada pada snapshot. Justru itulah perbaikannya: snapshot kembali merekam bahwa ketiga tabel
memang ada, sehingga migration berikutnya — milik siapa pun — tidak lagi mencoba membuatnya ulang.

Empat puluh lima pasang `DropForeignKey`/`AddForeignKey` pada tabel `Bil*` juga dibuang setelah
diverifikasi tidak mengubah perilaku apa pun:

| Pemeriksaan | Hasil |
| --- | --- |
| Foreign key yang di-drop tetapi tidak ditambahkan kembali | **Nihil** |
| Foreign key yang ditambahkan tetapi tidak pernah di-drop | **Nihil** |
| `onDelete` pada seluruh 45 `AddForeignKey` | `Restrict` |
| `onDelete` pada definisi aslinya di `20260824080052` | `Restrict` |

---

## 4. Yang masih terbuka, dan pemiliknya

### 4.1 `MstRegister` tidak punya migration sama sekali

Tabelnya ada di database pengembangan, tetapi **tidak ada satu pun dari 206 migration yang pernah
membuatnya**. Artinya tabel itu lahir di luar jalur migration.

Akibatnya nyata: **database yang benar-benar baru tidak akan memiliki `MstRegister`**, dan kode
yang membacanya akan gagal di runtime.

Menambalnya dari sisi Rawat Jalan berarti mengambil alih schema modul orang lain, dan itu justru
kesalahan yang sedang diperbaiki di seluruh dokumen ini. Perbaikannya milik pemilik modul:
terbitkan migration yang membuat `MstRegister` secara idempoten, misalnya memakai
`CREATE TABLE IF NOT EXISTS` sehingga aman terhadap database yang sudah memilikinya.

### 4.2 Yang perlu diperiksa pemilik ketiga entity

1. Apakah definisi entity yang ter-commit memang yang dikehendaki. Snapshot sekarang mengikuti
   definisi itu apa adanya.
2. Apakah tabel-tabel itu membutuhkan pengisian data awal. `RJ-BIL-BE-007` **tidak** mengisi satu
   baris pun ke ketiganya, karena isi master data adalah keputusan pemiliknya.

---

## 5. Yang sudah diterapkan ke database

Migration `20260827040349_AddBillingReconciliationCase` diterapkan ke database pengembangan
bersama `QuilvianNewDevTim01` atas keputusan `RJ-BIL-DEC-009`, melalui test integrasi Billing.

Penerapan terbatas pada database pengembangan. Staging, UAT, dan production **tidak** tersentuh:
penjagaan pada `BillingTestDatabaseFixture` menolak ketiganya secara mutlak dan tidak mengenal
opt-in apa pun, dan hal itu dikunci oleh test tersendiri.

---

## 6. Pencegahan yang murah

Keadaan ini menghentikan seluruh tim, bukan hanya modul pemiliknya. Satu perintah menutup seluruh
kelas masalah ini, dan layak dijalankan sebelum commit yang menyentuh entity mana pun:

```
dotnet ef migrations has-pending-model-changes
```

Bila jawabannya *"Changes have been made to the model since the last migration"*, berarti ada
entity yang belum sepadan dengan migration — dan siapa pun yang menjalankan
`dotnet ef database update` hari itu akan gagal.

Pencegahan kedua, untuk akar masalahnya: setelah merge dari branch lain, periksa apakah snapshot
kehilangan entity milik orang lain sebelum membangkitkan migration baru.
