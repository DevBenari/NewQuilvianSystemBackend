# Laporan Perubahan Backend — `BE-EXT-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-EXT-04` |
| Judul | [Registrasi] Kiosk mengetahui tujuan layanan |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6c |
| Trace | `FR-11.11`; `LAB-DEC-051`, `LAB-DEC-052`; `AC-93`; `T-93a`..`T-93d` |
| Contract version | Dikontrakkan di sisi `registration-management`. **`LAB-API-v1` tidak bertambah endpoint** |
| Wewenang lintas modul | [`LAB-REQ-006`](../../../approval-requests/2026-09-15-persetujuan-bagian-lab-di-kiosk.md) — Andry Zain, 2026-09-15 |
| Dependency | — |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source `RegistrationManagement`, configuration, migration, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — dua kolom nullable berdiri dan diterapkan; **ke-16 sesi kiosk yang sudah ada terbukti utuh**; jalur `Down` lalu `Up` dibuktikan |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | **`RegistrationManagement / Registration`** — bukan Laboratorium |
| Pemilik dan prefix registry | Prefix **`Reg`**, lifecycle **`ACTIVE / LEGACY`** — registry baris 16 |
| Keberlakuan | **`TOUCHED LEGACY`**. `TrxKioskScanSession` memakai prefix `Trx*` yang dilarang `QBE-NAM-001` untuk `NEW CODE`, tetapi ia tabel yang sudah ada. **Legacy ratchet berlaku: tidak dinamai ulang**, dan penamaan ulangnya bukan cakupan task ini |
| Status gerbang | Tidak menahan — nol entity baru |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-CFG-002`, `QBE-API-001`, `QBE-DTO-001`, `QBE-ENUM-001` |

**Task ini menyentuh milik modul lain, dan itu disengaja.** Polanya mengikuti `BE-EXT-01`
sampai `BE-EXT-03`: task berada pada roadmap Laboratorium dan dikerjakan atas wewenang
`LAB-REQ-006`, bukan atas asumsi kepemilikan.

### Dua temuan gate yang ditutup lebih dulu

`AC-92` dan `AC-93` **tidak punya satu pun baris uji**. `T-92a`..`T-92d` dan `T-93a`..`T-93d`
ditambahkan pada matriks uji bagian 11.5c sebelum kode ditulis.

> **Ini kejadian kedua berturut-turut.** `AC-83` mengalami hal yang sama pada gate `BE-LAB-29`
> kemarin. Dua kali berturut-turut menunjukkan ini **bukan kelalaian sekali**, melainkan celah
> proses: acceptance criteria yang ditulis pada decision log tidak otomatis memperoleh baris uji,
> dan tidak ada langkah yang memeriksanya. Dicatat sebagai pelajaran pada 7.2.

---

## 1. Masalah yang diperbaiki

Kiosk sudah ada dan sudah dipakai — **16 sesi nyata, 15 di antaranya cocok ke pasien**. Tetapi
kerjanya cuma satu: memindai identitas lalu mencocokkannya ke `PatientId`.

**Ia tidak tahu pasien itu hendak ke mana.** Akibatnya petugas di setiap unit tidak dapat
melihat siapa yang sedang menunggunya, dan pasien yang sudah memindai identitasnya di kiosk
tetap harus mengulang seluruh identifikasi di meja unit tujuan.

Untuk Laboratorium, akibat keduanya lebih spesifik: tidak ada cara membedakan pasien yang datang
membawa **permintaan dokter** dari pasien yang **memeriksakan diri sendiri** — padahal keduanya
berbeda perlakuan sampai ke penjamin dan tagihan (`LAB-DEC-052`).

---

## 2. Proses bisnis

**Pelaku.** Pasien di kiosk, lalu petugas unit tujuan.

**Alur sesudah perubahan:**

1. Pasien memindai identitasnya di kiosk — **tidak berubah sama sekali**.
2. Pasien menyatakan **layanan yang dituju**; Laboratorium salah satu pilihannya.
3. Bila tujuannya Laboratorium, ia menyatakan apakah **membawa permintaan dokter**.
4. Petugas lab membuka daftar sesi bertujuan Laboratorium yang belum diproses, dan menariknya.

Langkah 2 dan 3 adalah layar, cakupan `FE-LAB-13`. Task ini menyediakan tempat penyimpanannya
dan jalur bacanya.

**Tiga keadaan `HasPhysicianRequest`, dan ketiganya bermakna:**

| Nilai | Artinya |
| --- | --- |
| `true` | Pasien membawa permintaan dokter |
| `false` | Pasien memeriksakan diri sendiri |
| `null` | **Belum ditanyakan** — bukan sama dengan `false` |

Bedanya penting. Sesi yang dibuat sebelum ruas ini ada bernilai `null`, dan memperlakukannya
sebagai "memeriksakan diri sendiri" berarti mengarang jawaban atas pertanyaan yang tidak pernah
diajukan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Temuan yang menghapus separuh pekerjaan

Kartu task menyebut *"satu jalur baca sesi bertujuan Laboratorium yang belum diproses"*.
Pemeriksaan source menemukan jalur itu **sudah ada**:

`GET /kiosk-scan-sessions/options?onlyUsableForRegistration=true` sudah mengembalikan sesi yang
**berhasil dipindai, tidak dibatalkan, dan belum dipakai registrasi** — persis definisi "belum
diproses". Modelnya bahkan sudah punya `IsUsedForRegistration`.

**Jalur baca baru karena itu tidak dibuat.** Yang ditambahkan hanya satu penyaring `targetService`
pada endpoint yang sudah ada. Membuat endpoint kedua akan melahirkan **dua definisi "belum
diproses"** yang dapat menyimpang diam-diam — dan yang menyimpang biasanya baru ketahuan ketika
dua layar menampilkan angka berbeda untuk hal yang sama.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Enums/KioskServiceTarget.cs` | **Baru.** Empat nilai: `Unknown`, `OutpatientClinic`, `Laboratory`, `Other` |
| `Areas/HealthServices/RegistrationManagement/Models/TrxKioskScanSession.cs` | Dua kolom nullable: `TargetService`, `HasPhysicianRequest` |
| `Repositories/Configurations/HealthServices/TrxKioskScanSessionConfiguration.cs` | Konversi enum dan satu index |
| `Areas/HealthServices/RegistrationManagement/DTOs/KioskScanSessionDtos.cs` | Dua ruas pada `KioskScanSessionOptionResponse` |
| `Areas/HealthServices/RegistrationManagement/Controllers/KioskScanSessionController.cs` | Penyaring `targetService` pada `options` dan `admin/options`; pemetaan respons; **pemanggilan diubah menjadi argumen bernama** |
| `Migrations/20260915083316_AddKioskServiceTargetAndPhysicianRequest.cs` | **Baru.** Dua kolom nullable dan satu index |

**Nol perubahan pada source Laboratorium.** Task ini seluruhnya berada di `RegistrationManagement`.

### 3.3 Keputusan penamaan yang saya ambil, dan pemiliknya boleh mengubahnya

`02-backend-architecture.md` bagian 12.6.1 menyatakan **bentuk persisnya ditetapkan pemilik
`registration-management`**, dan yang dikunci `LAB-REQ-006` adalah kebutuhannya, bukan nama
kolomnya. Karena implementasinya dikerjakan sekarang, penamaan itu harus diputuskan:

| Yang dinamai | Pilihan saya | Dasarnya |
| --- | --- | --- |
| Enum | `KioskServiceTarget` | Mengikuti bentuk `KioskScanSource` dan `KioskDeviceType` yang sudah ada |
| Nilai enum | `Unknown = 0` .. `Other = 99` | Persis pola `KioskDeviceType` |
| Kolom tujuan | `TargetService` | — |
| Kolom jalur | `HasPhysicianRequest` | — |

**Pemilik `registration-management` boleh menamai ulang keempatnya**, dan itu bukan pekerjaan
ulang yang besar selama belum ada data terisi — hari ini nol baris memilikinya. Dicatat supaya
pilihan ini tidak terbaca sebagai kesepakatan yang pernah terjadi.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /options` dan `GET /admin/options` menerima satu parameter **opsional** baru, dan responsnya bertambah dua ruas. **Aditif** — dibiarkan kosong berarti seluruh tujuan ikut, sehingga pemanggil lama tidak berubah perilakunya. `LAB-API-v1` tidak disentuh |
| Database | Dua kolom **nullable tanpa nilai bawaan** beserta satu index pada `TrxKioskScanSession`. Diterapkan ke `QuilvianNewDevYoga`; jalur `Down` dibuktikan |
| Keamanan/Auth | **`NOT APPLICABLE`.** `KioskScanSession : Read` dan `KioskReadPolicy` tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Kiosk Scan Session

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options`, `/kiosk/options` | Daftar pilihan sesi kiosk, kini **dapat disaring menurut tujuan layanan** | `KioskReadPolicy` |
| `GET` | `/admin/options` | Sama, jalur admin | `KioskScanSession : Read` |

**Parameter baru:** `targetService` (opsional). Dibiarkan kosong berarti seluruh tujuan ikut.

**Ruas baru pada respons:** `targetService`, `hasPhysicianRequest`.

Petugas lab memakainya sebagai `?onlyUsableForRegistration=true&targetService=Laboratory`.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `Reg` `ACTIVE / LEGACY`; `TOUCHED LEGACY`, tidak dinamai ulang | `PASS` | Bagian preflight |
| Review diff/scope | 1 berkas baru, 4 disentuh, 2 migration, 1 snapshot | `PASS` | Bagian 3.2 |
| `dotnet build -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Lihat 5.1 |
| Eksekusi migration | Dua kolom nullable dan index berdiri | `PASS` | Lihat 5.2 |
| **`T-93a`** — ke-16 sesi lama tetap terbaca, kedua ruas kosong | **16 sesi utuh, 15 masih cocok ke pasien, 0 kolom baru terisi** | `PASS` | Lihat 5.2 |
| **Jalur `Down` lalu `Up`** | Terbukti | `PASS` | Lihat 5.2 |
| `T-93b` — alur kiosk lama tidak berubah perilakunya | Ditegakkan secara struktural | `PASS` | Lihat 5.3 |
| `T-93c`, `T-93d` — perilaku penyaring tujuan | Menunggu aplikasi dijalankan | `NOT RUN` | Lihat 5.4 |

### 5.1 Satu kesalahan yang nyaris lolos, ditangkap compiler

Menyisipkan parameter `targetService` di **tengah** tanda tangan `GetSessionOptionsForKiosk`
memutus pemanggilnya — `GetSessionOptionsForAdmin` memanggilnya dengan **argumen posisional**:

```
error CS1503: Argument 6: cannot convert from 'string' to 'KioskServiceTarget?'
error CS1503: Argument 7: cannot convert from 'int' to 'string?'
```

**Yang menyelamatkannya hanya kebetulan.** Compiler menangkapnya karena tipenya tidak cocok.
Seandainya parameter yang disisipkan bertipe sama dengan tetangganya, pemanggilan itu akan
**tetap terkompilasi** dan diam-diam mengirim nilai ke parameter yang salah — `search` menjadi
`targetService`, `pageNumber` menjadi `search`, dan seterusnya.

Diperbaiki dengan mengubah pemanggilannya menjadi **argumen bernama**, beserta komentar yang
menjelaskan kenapa. Penyisipan parameter berikutnya tidak akan dapat memutusnya diam-diam.

Build akhir: **0 Error, 207 Warning**, nol warning dari berkas task ini.

### 5.2 Bukti terhadap database

| Butir | Hasil |
| --- | --- |
| `TargetService` | `integer`, **`is_nullable = YES`**, `column_default = NULL` |
| `HasPhysicianRequest` | `boolean`, **`is_nullable = YES`**, `column_default = NULL` |
| `IX_TrxKioskScanSession_TargetService` | **Terbaca dari `pg_indexes`** |
| **`T-93a`** | **16 sesi utuh**; **15 masih cocok ke pasien**; **0** terisi tujuan; **0** terisi jalur permintaan; 15 sudah dipakai registrasi |
| Sesi yang memenuhi syarat jalur baca | **1** — `Success`, tidak dibatalkan, belum dipakai registrasi |
| **Jalur `Down` lalu `Up`** | `database update 20260915081317_AddLabOrderedProcedure` → `Done.`; `database update` → `Done.`; nol `Pending` sesudahnya |

**Nol nilai bawaan pada kedua kolom, dan itu disengaja.** Bandingkan dengan `ScanSource` dan
`ScanStatus` pada tabel yang sama, yang memang punya nilai bawaan karena setiap sesi selalu
memilikinya sejak awal. Kedua kolom baru ini tidak: sesi yang sudah terjadi memang tidak
menyatakannya, dan `column_default = NULL` adalah cara menuliskan kenyataan itu.

**Hanya migration task ini yang `Pending` sebelum penerapan** — diperiksa lebih dulu, karena
`BE-LAB-26` kemarin menemukan enam migration modul lain ikut menggantung.

### 5.3 `T-93b` ditegakkan secara struktural

Alur kiosk lama tidak berubah perilakunya, dan itu terjamin oleh bentuk perubahannya sendiri:

| Bukti | Isi |
| --- | --- |
| Kedua kolom nullable tanpa nilai bawaan | Tidak ada penulisan yang dipaksakan pada alur lama |
| Penyaring `targetService` opsional | Dibiarkan kosong berarti seluruh tujuan ikut — pemanggil lama tidak menyebutnya, dan hasilnya sama persis |
| Nol validasi baru | Tidak satu pun jalur yang sebelumnya diterima kini ditolak |

### 5.4 Yang belum dijalankan

| Pemeriksaan | Kenapa belum |
| --- | --- |
| `T-93c` — sesi bertujuan Laboratorium yang belum dipakai muncul, yang sudah dipakai tidak | Memerlukan aplikasi dijalankan. Penyaringnya satu klausa `Where`; datanya siap — 1 sesi memenuhi syarat |
| `T-93d` — sesi bertujuan selain Laboratorium tidak ikut muncul | Sama. **Belum dapat diuji bermakna**: hari ini nol sesi memiliki tujuan, karena layar yang mengisinya adalah `FE-LAB-13` |

Uji manual: `NOT FEASIBLE` pada sesi ini.

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kedua ruas nullable | **✅ Terbukti di database** | 5.2 — `is_nullable = YES`, `column_default = NULL` |
| **Nol perilaku sesi kiosk yang sudah ada berubah** | **✅ Terpenuhi** | 5.3 secara struktural; 5.2 membuktikan 16 sesi utuh |
| Data lama utuh | **✅ Terbukti** | `T-93a` — 16 sesi, 15 masih cocok ke pasien |
| Migration jalan maju dan mundur | **✅ Terbukti** | 5.2 |

| AC | Status | Bukti |
| --- | --- | --- |
| `AC-93` — penambahan ruas tidak mengubah perilaku sesi kiosk yang sudah ada | **✅ Terpenuhi dan terbukti** | `T-93a` dan 5.3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `HostAbortedException` pada `migrations add` — perilaku normal EF Tools |
| Masalah yang diketahui | Penamaan pada 3.3 diputuskan tanpa spesifikasi pemilik `registration-management`; boleh diubah |
| Risiko tersisa | Rendah. Kedua kolom belum diisi siapa pun sampai `FE-LAB-13` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 1 berkas baru, 4 disentuh, 2 migration, snapshot, laporan ini. Tidak ada `git add`, `commit`, maupun `push` |
| Langkah berikutnya | Lihat 7.1 |

### 7.1 Langkah berikutnya

1. **Gelombang 1 `MVP-5b` selesai.** `BE-LAB-27` dan `BE-EXT-05` kini terbuka.
2. **Konfirmasikan penamaan pada 3.3** ke pemilik `registration-management`. Sekarang murah
   diubah; sesudah `FE-LAB-13` mengisi datanya, tidak lagi.
3. `FE-LAB-13` mengisi kedua kolom itu — sampai saat itu, `T-93d` belum dapat diuji bermakna.

### 7.2 Pelajaran proses, kejadian kedua berturut-turut

`AC-92` dan `AC-93` tidak punya baris uji, persis seperti `AC-83` kemarin. Polanya sama: AC
ditulis pada decision log oleh `grill-me`, lalu matriks uji diperbarui **terpisah** oleh
`design-business-module` — dan tidak ada langkah yang memeriksa bahwa setiap AC baru memperoleh
barisnya.

Akibatnya sudah terbukti sekali: `AC-83` bertahan dilanggar sejak 2026-09-14 tanpa seorang pun
menyadarinya, karena memang tidak ada cara memeriksanya. Selama celah ini terbuka, setiap
amandemen berikutnya berisiko melahirkan AC yang tidak dapat ditagih.

**Saran perbaikannya satu langkah:** jadikan "setiap AC baru punya sekurang-kurangnya satu baris
uji" sebagai gerbang tetap pada `design-business-module`, bukan pemeriksaan yang kebetulan
dilakukan saat task dikerjakan.
