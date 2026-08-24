# Laporan Perubahan Backend — `BE-IGD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-002` |
| Judul | "Target waktu belum diatur" dapat dibedakan dari "0 menit" |
| Slice | S0 — Modul benar-benar hidup |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `IGD-DEC-027`, `IGD-DEC-035`, validation matrix bagian 2, `AT-IGD-012` |
| Contract version | Validation `0.2.0` — aturannya tidak berubah, **bentuk data berubah** (lihat bagian 6) |
| Dependency | `BE-IGD-001` — sudah dikerjakan |
| Commit backend saat dikerjakan | `d2682c3ca045d95d293564dd6f4bdad9d6df8f6c` |
| Tanggal | 14 Agustus 2026 |
| Jenis perubahan | Perbaikan makna data, ditambah satu migration |

---

## 1. Masalah yang diperbaiki

### 1.1 Dua keadaan yang tercampur menjadi satu

Setiap level triase punya target waktu respons, yaitu berapa menit pasien boleh menunggu
sebelum ditangani. Nilainya disimpan di kolom `MaxWaitingMinutes`.

Sebelum perubahan ini, kolom tersebut bertipe angka biasa yang **wajib terisi**. Akibatnya dua
keadaan yang sangat berbeda tersimpan dengan nilai yang sama persis, yaitu angka 0:

| Keadaan nyata | Yang tersimpan sebelumnya | Yang seharusnya |
| --- | --- | --- |
| "Pasien ini harus dilayani seketika" (level 1, Merah) | `0` | `0` — memang benar |
| "SOP rumah sakit belum menetapkan targetnya" (level 3 sampai 5) | `0` | Kosong |

Sistem lalu memperlakukan keduanya sama: batas waktu respons dihitung `StartedAt + 0 menit`,
sehingga jatuh tempo pada detik yang sama dengan saat penilaian dibuat.

### 1.2 Akibatnya bagi perawat

> **Contoh:** SOP MMC belum menetapkan target untuk level 3. Petugas mengisi master dengan
> mengosongkan kolom target, tetapi karena kolomnya angka biasa, yang tersimpan adalah 0.
> Pasien level 3 yang dinilai pukul 08.00 langsung memperoleh batas waktu pukul 08.00 juga.
> Satu menit kemudian sistem menandainya melampaui batas.
>
> Bila dalam satu jam ada 12 pasien level 3 sampai 5, layar perawat berisi 12 peringatan yang
> seluruhnya palsu. Ketika kemudian ada satu pasien Merah yang benar-benar terlambat ditangani,
> peringatannya menjadi baris ke-13 yang tenggelam di antara 12 peringatan palsu.

Peringatan yang selalu menyala sama saja dengan tidak ada peringatan. Itulah kerugian nyata
yang diperbaiki task ini.

### 1.3 Mengapa tidak diisi angka perkiraan saja

Menebak target adalah pilihan yang dilarang keputusan yang sudah diambil, yaitu `IGD-DEC-027`
dan `IGD-DEC-035`. Alasannya: target waktu triase adalah aturan klinis. Angka yang ditebak
programmer akan menjadi tolok ukur mutu yang dipakai laporan rumah sakit, padahal tidak pernah
disahkan siapa pun.

---

## 2. Proses bisnis

### 2.1 Tujuan

Perawat hanya menerima peringatan untuk pasien yang benar-benar melewati batas waktu yang sudah
ditetapkan SOP.

### 2.2 Pelaku

| Pelaku | Kewenangan |
| --- | --- |
| Admin IGD | Mengisi dan mengubah master level triase, termasuk mengosongkan target waktu |
| Perawat IGD | Membuat penilaian triase; tidak dapat mengubah target waktu |
| Kepala Instalasi Gawat Darurat | Pemilik SOP triase; menetapkan angka target yang sah |
| Product/Domain Owner | Mengesahkan penyimpangan rencana migration pada bagian 7 |

### 2.3 Pemicu

Perawat menyimpan penilaian triase untuk seorang pasien.

### 2.4 Prasyarat

Level triase yang dipilih sudah terdaftar dan berstatus aktif di master.

### 2.5 Langkah utama

1. Perawat memilih level triase pasien, misalnya level 3.
2. Sistem mengambil data level itu dari master, termasuk target waktunya.
3. Sistem menyalin target tersebut ke penilaian sebagai *snapshot* — salinan beku, supaya
   perubahan master di kemudian hari tidak mengubah riwayat penilaian lama.
4. Bila target terisi, sistem menghitung batas waktu respons: waktu mulai ditambah target.
5. Bila target kosong, sistem **mengosongkan** batas waktu respons. Pasien tetap tercatat dan
   tetap terlihat di antrean, hanya saja tidak punya batas yang bisa dilanggar.
6. Perawat melihat antrean. Yang bertanda "melampaui batas" hanya pasien yang batas waktunya
   memang ada dan sudah lewat.

### 2.6 Aturan bisnis

**Aturan A — Kosong bukan nol.**

| Isi target di master | Arti | Batas waktu respons |
| --- | --- | --- |
| Kosong | SOP belum menetapkan | Kosong. Pasien tidak pernah dihitung melampaui batas |
| `0` | Harus dilayani seketika | Sama persis dengan waktu penilaian dimulai |
| `30` | Boleh menunggu paling lama 30 menit | Waktu penilaian dimulai + 30 menit |

> **Contoh berangka, ketiganya pada pukul 08.00:**
>
> - Level 1 (target `0`): batas waktu 08.00. Pukul 08.05 pasien belum ditangani → melampaui
>   batas, peringatan muncul, dan memang seharusnya muncul.
> - Level 3 (target kosong): batas waktu kosong. Pukul 09.30 pasien masih menunggu → tidak ada
>   peringatan, karena tidak ada janji waktu yang dilanggar.
> - Level 4 (target `60`): batas waktu 09.00. Pukul 08.55 → belum melampaui. Pukul 09.05 →
>   melampaui.

**Aturan B — Angka negatif tetap ditolak.**

Mengosongkan target diperbolehkan, tetapi mengisi angka negatif tetap ditolak dengan pesan
"MaxWaitingMinutes tidak boleh negatif." Target minus tidak punya arti klinis apa pun.

**Aturan C — Riwayat tidak boleh berubah.**

Penilaian lama yang sudah menyimpan angka target tetap menyimpan angka itu apa adanya.
Perubahan ini tidak menyentuh satu pun baris data yang sudah ada.

> **Contoh:** penilaian tanggal 1 Agustus menyimpan target 30 menit. Bila pada 20 Agustus admin
> mengubah master level itu menjadi 45 menit, penilaian 1 Agustus **tetap** 30 menit. Auditor
> yang memeriksa kemudian melihat angka yang benar-benar berlaku pada hari kejadian.

### 2.7 Perubahan status

Tidak ada. Perubahan ini tidak menyentuh status kunjungan maupun status penilaian triase.

### 2.8 Jalur tidak normal

| Kejadian | Yang terjadi | Yang dilihat pengguna |
| --- | --- | --- |
| Admin mengosongkan target level 3 | Diterima dan disimpan sebagai kosong | Tersimpan; kolom target tampil kosong, bukan 0 |
| Admin mengisi target `-5` | Ditolak | "MaxWaitingMinutes tidak boleh negatif." (kode 400) |
| Admin mengisi target `0` untuk level 1 | Diterima | Tersimpan sebagai 0, artinya seketika |
| Perawat menilai pasien pada level yang targetnya kosong | Penilaian tersimpan | Batas waktu tampil kosong; pasien tidak pernah ditandai terlambat |
| Penilaian lama dibuka kembali | Nilai lama tampil apa adanya | Tidak ada perubahan angka |

### 2.9 Hasil akhir

Sistem menyimpan tiga keadaan yang sebelumnya hanya dua: "belum diatur", "seketika", dan
"sekian menit". Peringatan keterlambatan hanya muncul untuk keadaan kedua dan ketiga.

---

## 3. File yang diubah

| File | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs` | `MaxWaitingMinutes` menjadi `int?` beserta keterangan artinya |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs` | `MaxWaitingMinutesSnapshot` menjadi `int?` beserta keterangan artinya |
| `Areas/HealthServices/MasterData/DTOs/EmergencyTriageLevelDtos.cs` | `MaxWaitingMinutes` menjadi `int?` pada response dan request |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyTriageDtos.cs` | `MaxWaitingMinutesSnapshot` menjadi `int?` pada response dan request |
| `Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyTriageController.cs` | Perhitungan batas waktu hanya dijalankan bila target terisi |
| `Areas/HealthServices/MasterData/Controllers/EmergencyTriageLevelController.cs` | Pemeriksaan negatif hanya berlaku bila target diisi |
| `Migrations/20260814073820_MakeTriageMaxWaitingMinutesNullable.cs` | Migration baru |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Ikut diperbarui otomatis, tepat 2 baris |

### 3.1 Inti perubahan perhitungan

Sebelum:

```csharp
entity.ResponseDueAt = entity.StartedAt.AddMinutes(triageLevel.MaxWaitingMinutes);
```

Sesudah:

```csharp
// Target waktu yang belum ditetapkan SOP dibiarkan kosong, bukan dianggap 0 menit.
// Level dengan target 0 menit tetap menghasilkan batas waktu sama dengan StartedAt.
entity.ResponseDueAt = triageLevel.MaxWaitingMinutes.HasValue
    ? entity.StartedAt.AddMinutes(triageLevel.MaxWaitingMinutes.Value)
    : null;
```

### 3.2 Yang sengaja tidak diubah

| Yang tidak disentuh | Alasan |
| --- | --- |
| Aturan pembatas `"MaxWaitingMinutes" >= 0` di basis data | Pada PostgreSQL, pembatas semacam ini otomatis melewatkan baris yang nilainya kosong. Nilai kosong lolos, nilai negatif tetap ditolak. Jadi pembatasnya sudah benar apa adanya |
| Endpoint pengubahan penilaian (`PUT`) | Endpoint itu memang tidak pernah menghitung ulang snapshot maupun batas waktu. Perilakunya dibiarkan sama, sesuai aturan C pada bagian 2.6 |
| Index `IX_TrxEmergencyTriage_ResponseDueAt` | Tetap berlaku dan tetap berguna; nilai kosong tidak mengganggu |
| Pengisian data master | Itu task `BE-IGD-003` |

---

## 4. Verifikasi

### 4.1 Yang sudah dijalankan

| Pemeriksaan | Cara | Hasil |
| --- | --- | --- |
| Seluruh pemakaian kolom sudah ikut disesuaikan | Penelusuran `MaxWaitingMinutes` ke seluruh berkas `.cs`; 15 titik pemakaian ditemukan, seluruhnya diperiksa satu per satu | Lulus |
| Build project | `dotnet build` | **Lulus** — 0 galat, 125 peringatan lama yang tidak berkaitan |
| Migration terbentuk benar | Pembacaan berkas migration | Lulus — hanya `DROP NOT NULL` pada dua kolom, tanpa pengisian nilai apa pun |
| Migration tidak menyentuh basis data | Log `dotnet ef` menunjukkan aplikasi berhenti pada `builder.Build()` baris 621, jauh sebelum seeder pada baris 799 | Lulus — tidak ada satu pun perintah tulis ke basis data |
| Snapshot model ikut benar | `git diff` pada `ApplicationDbContextModelSnapshot.cs` | Lulus — tepat 2 baris berubah, keduanya kolom yang dimaksud, tidak ada perubahan lain yang ikut terbawa |

Pemeriksaan terakhir penting: snapshot model adalah berkas sepanjang puluhan ribu baris yang
mudah ikut membawa perubahan orang lain tanpa disadari. Diff dua baris membuktikan tidak ada
yang menumpang.

### 4.2 Acceptance criteria

| No | Kriteria | Status | Dasar |
| ---: | --- | --- | --- |
| 1 | Target kosong menghasilkan batas waktu kosong dan snapshot kosong | **Terpenuhi di kode, belum diuji berjalan** | Percabangan `HasValue` pada `EmergencyTriageController`; snapshot disalin langsung dari master |
| 2 | Target `0` tetap menghasilkan batas waktu sama dengan waktu mulai | **Terpenuhi di kode, belum diuji berjalan** | `HasValue` bernilai benar untuk angka 0, sehingga `AddMinutes(0)` tetap dijalankan |
| 3 | Penilaian lama yang sudah punya angka tidak berubah | **Terpenuhi** | Migration tidak menulis nilai; endpoint pengubahan tidak menyentuh kedua kolom |
| 4 | Baris master lama terisi apa adanya, tanpa menebak | **Terpenuhi** | Migration hanya melepas kewajiban terisi; nilai lama tidak disentuh |

Kriteria 1 dan 2 sengaja tidak ditulis "lulus". Kodenya sudah benar dan sudah dibaca ulang,
tetapi belum pernah dijalankan sungguhan — lihat bagian 4.3.

### 4.3 Yang belum dijalankan, beserta alasannya

| Yang belum diuji | Alasan | Cara menutupnya |
| --- | --- | --- |
| Tiga unit test (target kosong, target 0, target 30) | Repository belum punya project test sama sekali. Membuatnya adalah keputusan arsitektur tersendiri, sama seperti yang sudah dicatat pada `BE-IGD-001` | Perlu keputusan pemilik: buat project test, atau catat sebagai bukti manual pada `BE-IGD-014` |
| Integration test `AT-IGD-011` dan `AT-IGD-012` | Alasan yang sama | Sama seperti di atas |
| Uji migration maju dan mundur | Tidak tersedia basis data lokal. `appsettings.Development.json` mengarah ke basis data pengembangan **bersama** di `160.22.250.77`. Menjalankan migration di sana mengubah skema yang dipakai orang lain, dan itu memerlukan izin eksplisit | Siapkan PostgreSQL lokal, lalu `dotnet ef database update` diikuti `dotnet ef database update <migration-sebelumnya>` |

Karena itu task ini **belum tuntas**. Yang terbukti: kode, migration, dan dokumen sudah benar
dan project berhasil dibangun. Yang belum terbukti: perilakunya saat benar-benar dijalankan.

---

## 5. Peringatan tentang pembatalan migration

Migration ini dapat dibatalkan, tetapi pembatalannya **tidak sepenuhnya aman**.

Mengembalikan kolom menjadi wajib terisi memaksa setiap nilai kosong diisi angka 0. Level 3
yang sengaja dibiarkan tanpa target akan berubah artinya menjadi "harus dilayani seketika" —
yaitu persis kesalahan yang diperbaiki task ini.

Selama data master IGD belum diisi (`BE-IGD-003` belum dikerjakan), tidak ada satu pun baris
yang berisiko, karena tabelnya masih kosong. Setelah master terisi, pembatalan migration wajib
didahului pencatatan level mana saja yang targetnya kosong.

---

## 6. Dampak ke kontrak dan frontend

Aturan validasi tidak berubah; yang berubah adalah **bentuk data yang dikirim**.

| Field | Sebelum | Sesudah | Siapa yang terdampak |
| --- | --- | --- | --- |
| `MaxWaitingMinutes` pada level triase | Selalu angka | Angka **atau** `null` | Layar master level triase |
| `MaxWaitingMinutesSnapshot` pada penilaian triase | Selalu angka | Angka **atau** `null` | Layar antrean dan detail triase |

Contoh balasan untuk level yang targetnya belum ditetapkan:

```json
{
  "statusCode": 200,
  "success": true,
  "message": "Data master level triage IGD berhasil diambil.",
  "data": {
    "level": 3,
    "code": "L3",
    "name": "Urgent",
    "colorName": "Kuning",
    "maxWaitingMinutes": null,
    "isActive": true
  }
}
```

Frontend perlu menampilkan `null` sebagai keterangan yang dapat dibaca perawat, misalnya
"Target belum ditetapkan", **bukan** sebagai angka 0 dan bukan sebagai kolom kosong tanpa
keterangan. Ini menjadi bahan `FE-IGD-003`.

Tidak ada endpoint yang ditambah, dihapus, atau berubah alamatnya. Grup Swagger yang isinya
terpengaruh:

### Health Services / Master Data / Emergency Installation Management / Emergency Triage Level

Base URL: `api/v1/health-services/master-data/emergency-installation-management/emergency-triage-levels`

Bentuk `maxWaitingMinutes` pada seluruh endpoint grup ini kini boleh `null`.

### Health Services / Emergency Installation Management / Emergency Triage

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triages`

Bentuk `maxWaitingMinutesSnapshot` pada seluruh endpoint grup ini kini boleh `null`, dan
`responseDueAt` kini benar-benar dapat kosong untuk penilaian baru.

---

## 7. Penyimpangan dari blueprint yang perlu disahkan

Roadmap sudah memperingatkan hal ini, dan peringatannya terbukti benar.

| Aspek | Rencana pada blueprint | Kenyataan setelah task ini |
| --- | --- | --- |
| Jumlah migration modul IGD | Satu, yaitu `AddTriageSlaBreachMarker` | Dua. `MakeTriageMaxWaitingMinutesNullable` mendahuluinya |

Alasan penambahan: aturan `TargetUnconfigured` pada validation matrix bagian 2 mengharuskan
sistem membedakan "belum diatur" dari "0 menit". Tipe angka biasa tidak mampu menyatakan
keadaan kosong. Satu-satunya alternatif adalah menebak angka target, dan itu dilarang
`IGD-DEC-027` serta `IGD-DEC-035`.

Dokumen yang sudah diperbarui agar cocok dengan kenyataan:

| Dokumen | Yang diperbarui |
| --- | --- |
| `02-backend-architecture.md` bagian 6 | Rencana migration menjadi dua baris, beserta peringatan cara mundur |
| `02-backend-architecture.md` bagian 5 | Status `TrxEmergencyTriage` dan `MstEmergencyTriageLevel` |
| `02-backend-architecture.md` diagram kelas | `MaxWaitingMinutes` menjadi `int?` |
| `erd/data-dictionary.md` | Tipe kolom, kewajiban terisi, dan definisi tabel |

Yang **memerlukan pengesahan Product/Domain Owner**: bertambahnya satu migration di luar
rencana. Bila owner menolak, satu-satunya jalan tersisa adalah menebak angka target, yang
bertentangan dengan keputusan yang sudah tercatat.

---

## 8. Risiko tersisa

| No | Risiko | Akibat nyata bila diabaikan |
| ---: | --- | --- |
| 1 | Belum ada test otomatis maupun uji migration | Perhitungan batas waktu bisa rusak lagi tanpa ketahuan pada perubahan berikutnya |
| 2 | Frontend belum menangani nilai kosong (`FE-IGD-003`) | Layar dapat menampilkan target kosong sebagai "0", sehingga kesalahan yang diperbaiki di backend muncul kembali di layar perawat |
| 3 | Data master IGD masih kosong (`BE-IGD-003`) | Perubahan ini belum dapat dirasakan siapa pun, karena belum ada level triase yang bisa dipilih |
| 4 | Pemantau keterlambatan belum ada (`BE-IGD-006`) | Batas waktu sudah dihitung benar, tetapi belum ada yang memantaunya |

---

## 9. Bukti penelusuran

| Klaim | Bukti |
| --- | --- |
| Target sebelumnya bertipe angka wajib terisi | `NewQuilvianSystemBackend` + `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs` baris 35 (sebelum diubah) + `d2682c3` |
| Batas waktu dihitung tanpa memeriksa kekosongan | `Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyTriageController.cs` baris 220 (sebelum diubah) + `d2682c3` |
| Aturan `TargetUnconfigured` mengikat | `docs/module-blueprints/igd/contracts/validation-matrix.md` baris 46-48 |
| Larangan menebak angka target | `docs/module-blueprints/igd/blueprint-manifest.md` baris 99 |
| Migration tidak mengisi nilai apa pun | `Migrations/20260814073820_MakeTriageMaxWaitingMinutesNullable.cs` baris 13-29 |
| Snapshot model hanya berubah 2 baris | `git diff Migrations/ApplicationDbContextModelSnapshot.cs` |
