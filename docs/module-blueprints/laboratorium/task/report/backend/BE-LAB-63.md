# Laporan Perubahan Backend — `BE-LAB-63`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-63` (backend) |
| Judul | Pengaturan disiplin dan nomor cetak — gelombang `MVP-7b`, slice `S4b` |
| Trace | `LAB-DEC-117`, `LAB-DEC-119`, `LAB-DEC-127`; bukti `LAB-EVD-005` |
| Kontrak | `LAB-API-v1` `r27` bagian 22.3 dan 22.7 |
| Klasifikasi | `MEDIUM` — satu tabel, satu kolom, satu layanan alokasi, satu controller |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-22 |
| Dependency | `BE-LAB-53` ✅ |
| Status | ✅ **`SELESAI`** — `AC-180` dan `AC-182` terbukti terhadap database sungguhan. **Satu batas diketahui diangkat sebagai `LAB-OPEN-043`** |

---

## 1. Yang dibangun

| Berkas | Isi |
|---|---|
| `Models/LabDisciplineSetting.cs` | Label, nama konsultan, kalimat baku, awalan nomor |
| `Repositories/Configurations/.../LabDisciplineSettingConfiguration.cs` | Index unik **parsial** atas `Discipline`; nol foreign key |
| `Models/LabOrder.cs` | Kolom `LabReportNumber`, **nullable** |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Index unik **parsial** atas `(Discipline, LabReportNumber)` |
| `Services/LabReportNumberService.cs` | Alokasi per disiplin per tahun |
| `Services/LabDisciplineSettingService.cs` | Baca dan ubah tiga baris |
| `Controllers/LabDisciplineSettingController.cs` | 4 endpoint — **nol `POST`, nol `DELETE`** |
| `Seeders/LabDisciplineSettingSeeder.cs` | Tiga baris dari `LAB-EVD-005`, **hanya menyisipkan** |
| `Services/LabOrderService.cs` | Alokasi disambungkan ke **kedua** jalur pembuatan pesanan |
| `DTOs/LabOrderDtos.cs` | `labReportNumber` pada jalur baca |
| `Migrations/20260921063308_AddLabDisciplineSettingAndReportNumber.cs` | Dibangkitkan |

### Kenapa alokasinya disambungkan, padahal roadmap nol memintanya

Cakupan roadmap berhenti pada tabel, kolom, layanan, dan controller. Tetapi `AC-180`
menuntut **satu pesanan memiliki dua nomor yang berbeda dan keduanya terbaca** — dan itu
mustahil dibuktikan oleh kolom yang nol pernah diisi siapa pun. Layanan alokasi yang tidak
pernah dipanggil sama bahayanya dengan aturan yang tercatat tetapi tidak berlaku: ia
**terlihat** sudah selesai.

### Kenapa nomor cetak dialokasikan SESUDAH disiplin diketahui

Ia per disiplin per tahun, sehingga urutannya mustahil ditentukan sebelum disiplinnya ada.
Pada jalur `POST /lab-orders`, disiplin baru terisi di dalam penyusun entity — diturunkan dari
katalog bila permintaan tidak membawanya (`BE-LAB-29`). Pesanan berdisiplin **kosong** nol
memperoleh nomor cetak, dan itu sah (`AC-85`): lembar hasilnya pun belum dapat dicetak.

### Satu perbedaan halus antara kedua jalur pembuatan

| Jalur | Panggilan | Kenapa |
|---|---|---|
| `POST /lab-orders` | `AllocateOneAsync` sekali | Satu pesanan |
| `POST /lab-orders/by-examinations` | `AllocateOneAsync` **per kelompok** | Kelompoknya dibentuk **per disiplin**, sehingga setiap iterasi menyentuh penghitung yang **berbeda** |

> Jalur kedua sengaja **tidak** memakai alokasi berblok seperti `OrderNumber`. Yang menjebak
> pada nomor order adalah penghitung **tunggal** yang dibaca berulang di dalam satu transaksi —
> entity yang belum tersimpan nol terlihat oleh kueri SQL mentah, sehingga nomor yang sama
> kembali berulang kali. Di sini penghitungnya terpisah per disiplin, dan kelompoknya pun satu
> per disiplin, sehingga masalah itu nol dapat terjadi.

---

## 2. Satu batas yang diketahui, dan kenapa ia TIDAK ditutup diam-diam

Bukti `LAB-EVD-005` memperlihatkan **tiga bentuk nomor yang berbeda**:

| Disiplin | Bukti | Pemisah | Lebar urut |
|---|---|---|---|
| Mikrobiologi | `26-1129` | `-` | 4 |
| Patologi Anatomi | `26.0919` | `.` | 4 |
| Patologi Klinik | `25039254` | **nol** | 6 |

`r27` bagian 22.7 menyetujui **satu** ruas `reportNumberPrefix`, dan satu ruas awalan tidak
dapat menyatakan pemisah maupun lebar. Yang dibangun karena itu memakai `-` dan empat digit
untuk ketiganya — **cocok dengan Mikrobiologi, dan tidak dengan dua lainnya.**

Tiga jalan ditolak, masing-masing dengan alasannya:

| Jalan | Kenapa ditolak |
|---|---|
| Menambah kolom pemisah dan lebar | Mengubah kontrak yang sudah `approved` secara sepihak |
| Menjadikan `ReportNumberPrefix` template berisi `{yy}` dan `{0000}` | Itu bukan awalan lagi; namanya akan berbohong tentang isinya |
| Menaruh tahun di dalam awalan lalu meminta kepala instalasi menggantinya tiap Januari | Nomor akan salah pada setiap awal tahun yang terlewat, dan salahnya baru terlihat pada lembar yang sudah tercetak |

**Diangkat sebagai `LAB-OPEN-043`.** Membangun format yang salah untuk dua dari tiga disiplin
sambil berdiam diri adalah cara termurah membuat cacat ini ditemukan oleh pasien yang memegang
lembarnya.

---

## 3. Verifikasi

### 3.1 Terhadap database

| Pemeriksaan | Hasil |
|---|---|
| `IX_LabDisciplineSetting_Discipline` | ✅ `UNIQUE ... WHERE ("IsDelete" = false)` — **parsial** |
| `IX_LabOrder_Discipline_LabReportNumber` | ✅ `UNIQUE ("Discipline","LabReportNumber") WHERE (("IsDelete" = false) AND ("LabReportNumber" IS NOT NULL))` |
| `LabDisciplineSetting` | ✅ 3 baris, nol foreign key |
| `Down` migration | ✅ `DropTable` + `DropIndex` + `DropColumn` |

### 3.2 Seeder — ketiga baris terbaca APA ADANYA dari `LAB-EVD-005`

| Disiplin | `ConsultantLabel` | `ConsultantName` | `StandingNote` |
|---|---|---|---|
| Patologi Klinik | `Konsultan` | `Prof.Dr.Riadi Wirawan SpPK(K)` | kosong |
| Patologi Anatomi | `Spesialis Patologi Anatomi` | `Ening Krisnuhoni, SpPA-K, dr.` | kosong |
| Mikrobiologi | `Konsultan Mikrobiologi Klinik` | `Usman Chatib Warsa, PhD, SpMK-K, Prof. dr.` | `LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI TINGKAT KEPEKAAN BAKTERI.` |

> Kekosongan `StandingNote` pada dua disiplin adalah **pembacaan**, bukan kelalaian: footer
> keduanya pada `LAB-EVD-005` memang nol memuat kalimat baku.

### 3.3 `AC-180` — dua nomor berbeda pada satu pesanan, dan penghitung yang terpisah

Empat pesanan dibuat berturut-turut terhadap aplikasi yang berjalan:

| `OrderNumber` | Disiplin | `LabReportNumber` |
|---|---|---|
| `LAB-RSMMC-000010` | Mikrobiologi | **`26-0001`** |
| `LAB-RSMMC-000011` | Mikrobiologi | **`26-0002`** |
| `LAB-RSMMC-000012` | Patologi Anatomi | **`26-0001`** |
| `LAB-RSMMC-000013` | Patologi Klinik | **`26-0001`** |

Dua hal terbukti sekaligus:

1. **Keduanya terbaca dan keduanya berbeda** — `AC-180`.
2. `26-0001` muncul **tiga kali**, sekali per disiplin. Itulah arti *per disiplin per tahun*,
   dan index unik gabungan memang mengizinkannya.

`OrderNumber` **tetap utuh** dan tetap berderet global `000010` → `000013`. Nol satu pun
dibongkar menjadi format cetak (`LAB-DEC-072` utuh).

### 3.4 `AC-182` — footer berubah, wewenang klinis TIDAK

Nama konsultan Mikrobiologi diubah dari `Usman Chatib Warsa` menjadi
`dr. Hana Suryani, Sp.MK-K, Prof.`, lalu sidik jari dua tabel dibandingkan sebelum dan sesudah:

| Yang diperiksa | Sebelum | Sesudah |
|---|---|---|
| `LabDisciplineSetting` Mikrobiologi | `Usman Chatib Warsa…` | **berubah** ✅ |
| `SysAccessPolicy` (md5 seluruh baris) | `b3505b1a5fe74023f34768cce09b1cfa` | **identik** ✅ |
| `MstDoctor` (17 baris, md5) | `71847e019b21f5b5f2c06c7685c2e164` | **identik** ✅ |

Nama konsultan yang tercetak dan pemegang wewenang klinis adalah dua peran berbeda, dan nol
endpoint pada controller ini menyentuh yang kedua.

Nama dikembalikan ke nilai `LAB-EVD-005` sesudah pembuktian.

### 3.5 Idempotensi seeder — aplikasi dinyalakan ulang

Sesudah nama diubah manual, aplikasi **dimatikan lalu dinyalakan kembali**:

| Pemeriksaan | Hasil |
|---|---|
| Nama hasil suntingan | ✅ **`dr. Hana Suryani, Sp.MK-K, Prof.` bertahan** |
| Jumlah baris | ✅ tetap **3**, nol duplikat |

> Ini pengujian yang paling penting pada task ini. Seeder yang menyegarkan isinya setiap
> aplikasi menyala akan mengembalikan nama pejabat lama pada setiap penempatan ulang — pada
> dokumen yang dipegang pasien.

### 3.6 Keempat endpoint dan penolakannya

| Uji | Hasil |
|---|---|
| `GET /` ketiga pengaturan | ✅ `200` |
| `GET /options` | ✅ `200`, ketiganya `isConfigured: true` |
| `GET /{discipline}` | ✅ `200` |
| `PUT /{discipline}` | ✅ `200` |
| Label konsultan kosong | ✅ `400` *"Label konsultan wajib diisi."* |
| Disiplin tidak dikenal (`Hematology`) | ✅ `400` |
| Tanpa kredensial | ✅ `401` |

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Delapan ruas turunan `r27` 22.3 pada pembacaan hasil | Milik `BE-LAB-58`; task ini hanya menyediakan **sumbernya** |
| Pembubuhan nomor cetak pada pesanan lama | Akan memberi nomor tahun ini kepada pesanan tahun lalu |
| `POST` dan `DELETE` pengaturan | `r27` 22.7 menolaknya — barisnya tetap tiga |
| Pemisah dan lebar per disiplin | `LAB-OPEN-043` |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji tertinggal di dev:** empat pesanan uji `LAB-RSMMC-000010`..`000013` pada
> encounter `d6fdf9f0-…`, masing-masing bernomor cetak. Pengaturan disiplin sudah dikembalikan
> ke nilai `LAB-EVD-005`.
