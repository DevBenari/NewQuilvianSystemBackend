# Laporan Perubahan Backend — `BE-LAB-59`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-59` (backend) |
| Judul | Pilihan dokter konfirmator beserta jalur jatuhnya — gelombang `MVP-7`, slice `S4b` |
| Trace | `LAB-DEC-111`; menggantikan butir 2 `LAB-DEC-108` |
| Kontrak | `LAB-API-v1` `r26` bagian 21.7 |
| Klasifikasi | `MEDIUM` — satu resolver, satu endpoint baca, dua DTO. **Nol tabel baru, nol migration** |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-22 |
| Dependency | Nol |
| Status | ✅ **`SELESAI`** — `AC-173` dan `AC-174` terbukti, beserta dua pembuktian tambahan |

---

## 1. Yang dibangun

| Berkas | Isi |
|---|---|
| `DTOs/LabConfirmingDoctorDtos.cs` | `LabDoctorOption`, `LabConfirmingDoctorOptionsResponse` |
| `Services/LabConfirmingDoctorResolver.cs` | Penyusun pilihan beserta jalur jatuhnya |
| `Controllers/LabExaminationController.cs` | `GET /{id}/confirming-doctor-options` |
| `Program.cs` | Satu pendaftaran `Scoped` |

**Nol tabel baru dan nol jembatan baru.** Rantainya memang sudah lengkap pada satu
`ApplicationDbContext`, dan itu diverifikasi langsung di codebase sebelum satu baris pun
ditulis:

```
TrxOnCallAssignment.WorkforceProfileId
    -> MstDoctor.WorkforceProfileId
    -> MstDoctor.FullName + MstDoctor.WhatsAppNumber
```

### `MstDoctorSchedule` sengaja tidak dipakai

`LAB-DEC-111` butir 4 menolaknya, dan alasannya tetap berlaku: ia jadwal **praktik
poliklinik** — ber-`ClinicId` wajib, berkuota pasien, berpenanda kiosk — sedangkan
`ScheduleType` nol memuat nilai yang berarti *sedang jaga*. Memakainya berarti menawarkan
dokter yang sedang praktik poli sebagai penerima kabar hasil kritis pukul dua pagi.

### Satu ruas yang sengaja TIDAK digabungkan

`LabOrder.InstructingDoctorId` (`BE-RWI-104`) **tidak** ikut dibaca sebagai
`attendingDoctor`. Ia dokter pemberi instruksi pada pesanan rawat inap — pertanyaan yang
berbeda dari siapa DPJP pasien. Menggabungkannya diam-diam akan memberi satu nama label yang
bukan miliknya. `attendingDoctor` dibaca dari `RegPatientEncounter.DoctorId` saja.

### Dua ruas di luar kontrak, keduanya penambahan yang tidak merusak

| Ruas / parameter | Kenapa ada |
|---|---|
| `?search=` | `LAB-DEC-111` butir 2 menuntut daftar yang **dapat dicari**. Kontrak nol menyebut parameter; parameter query opsional nol mengubah bentuk respons |
| `fallbackTruncated` | Daftar dipotong pada 50 baris. Tanpa penanda ini, layar nol dapat tahu bahwa mengetik pencarian akan memunculkan dokter lain |

Batasnya dipasang **sekarang, ketika dokternya baru belasan** — justru karena daftar tanpa
batas nol terlihat bermasalah sampai jumlahnya berubah.

---

## 2. Verifikasi

Seluruhnya panggilan sungguhan terhadap aplikasi yang berjalan dan database `QuilvianNewDevYoga`.

### 2.1 `AC-174` — jadwal jaga KOSONG

`TrxOnCallAssignment` benar-benar **nol baris** pada saat diuji — bukan dikosongkan untuk
pengujian. Itu keadaan sesungguhnya yang diramalkan `LAB-DEC-111`.

| Ruas | Nilai |
|---|---|
| `onDutyDoctors` | `[]` |
| `onDutyScheduleAvailable` | **`false`** ✅ |
| `fallbackDoctors` | **terisi** — seluruh dokter aktif beserta nomor WhatsApp ✅ |
| `fallbackNote` | *"Jadwal jaga belum tersedia untuk jam ini…"* ✅ |
| Pesan respons | *"Jadwal jaga belum tersedia; seluruh dokter aktif ditampilkan."* |

### 2.2 `AC-173` — jadwal jaga TERISI

Satu penugasan jaga aktif disisipkan atas izin pemilik modul (lihat bagian 4).

| Ruas | Nilai |
|---|---|
| `onDutyDoctors` | **tepat satu** — `dr. Nabila Rahmawati, Sp.MK`, WhatsApp `080000000102` ✅ |
| `onDutyScheduleAvailable` | `true` ✅ |
| `fallbackDoctors` | `[]` — **jalur jatuh padam sendiri** ✅ |

### 2.3 Penyaringan waktu benar-benar bekerja

Menguji hanya "ada baris" akan meloloskan resolver yang **mengabaikan jam**. Jendela
penugasan karena itu digeser ke masa lalu (`-20 jam` … `-8 jam`), lalu dibaca ulang:

| Keadaan jendela | `onDutyScheduleAvailable` |
|---|---|
| Aktif sekarang | `true` |
| **Sudah lewat** | **`false`**, dan jalur jatuh menyala kembali ✅ |

Jendelanya dikembalikan aktif sesudah pembuktian.

### 2.4 `attendingDoctor` bukan selalu kosong

Seluruh pemeriksaan yang ada bernaung pada kunjungan tanpa dokter, sehingga ruas ini
sempat kosong pada setiap pembacaan — keadaan yang **terlihat sama** dengan ruas yang
tidak pernah bekerja. Satu pesanan baru karena itu dibuat lewat API pada kunjungan
`e3338713-…` yang berdokter:

| Ruas | Nilai |
|---|---|
| `attendingDoctor` | `dr. Rendy Pangalila` ✅ |
| `onDutyDoctors` | `dr. Nabila Rahmawati, Sp.MK` ✅ |

**Dua dokter berbeda dari dua sumber berbeda pada satu respons** — persis *"dua cara memilih,
bukan dua jabatan"* (`LAB-DEC-108`, dipertahankan `LAB-DEC-111` butir 5).

### 2.5 Pencarian dan penolakan

| Uji | Hasil |
|---|---|
| `?search=nabila` | ✅ satu baris, `fallbackTruncated: false` |
| `?search=zzzzz` | ✅ `fallbackDoctors: []`, keterangan tetap terkirim |
| Pemeriksaan tidak ada | ✅ `404` |
| Tanpa kredensial | ✅ `401` |

---

## 3. Tiga kerusakan merge yang ditemukan dan diperbaiki

Merge `4ba789b2` (`QuilvianIntegrationBackend` → `yoga`, 2026-09-22 10:16) meninggalkan HEAD
dalam keadaan **tidak dapat dibuild dan tidak dapat dinyalakan**. Ketiganya ditemukan berurutan
saat mencoba menguji task ini.

| # | Kerusakan | Perbaikan |
|---|---|---|
| 1 | `Program.cs` memanggil `LabDummyDataSeeder` yang **berkasnya tidak ada pada kedua sisi merge** — dicabut 2026-09-17 atas instruksi pemilik modul, tetapi cabang integrasi belum menerimanya | Pemanggilnya dicabut ulang; komentarnya menyimpan jejaknya |
| 2 | `BilConsumerHandoffService` dan sekerabatnya **tidak terdaftar** di DI — tiga layanan Billing menuntutnya, dan aplikasi gagal pada validasi DI, **bukan saat dibuild** | `builder.Services.AddBillingManagement();` dipanggil. Extension itu ada pada cabang integrasi tetapi **nol pernah dipanggil dari mana pun** |
| 3 | `LabOrderConfiguration` **kehilangan foreign key `ExaminerDoctorId` → `MstDoctor`** — ada pada sisi `yoga`, tidak ada pada sisi integrasi, dan merge mengambil sisi kedua | Dipulihkan beserta index-nya |

> **Kerusakan ketiga yang paling berbahaya, dan hampir lolos.** Ia membuat
> `dotnet ef database update` menolak jalan dengan `PendingModelChangesWarning`. Membangkitkan
> migration dari drift itu — langkah yang paling wajar diambil — akan **MENGHAPUS foreign
> key-nya dari database**, mencabut integritas referensial atas dokter pemeriksa tanpa seorang
> pun memutuskannya. Migration sementara itu dibuat, dibaca, lalu **dihapus**, dan sebabnya
> ditelusuri ke kedua sisi merge sebelum apa pun diterapkan.

### Satu kerusakan merge yang TIDAK diperbaiki

`ApplicationDbContextModelSnapshot` terbawa dari sisi `yoga`, sehingga ia nol memuat perubahan
model ke-21 migration cabang lain. Akibatnya setiap `dotnet ef database update` tanpa target
eksplisit akan menolak jalan dengan drift setebal **182 KB**.

Ke-21 migration diterapkan dengan menyebut targetnya secara tegas
(`dotnet ef database update 20260922015126_AddBillCollectionPrescriptionHandoff`), dan
**snapshot-nya sengaja dibiarkan apa adanya**: menyegarkannya menyentuh model seluruh modul
dan itu milik yang melakukan merge, bukan milik task ini.

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Endpoint pengisi `TrxOnCallAssignment` | Milik `human-resource` (`LAB-COORD-014`); `r26` 21.8 menolaknya secara tegas |
| Penetapan konfirmator | Sudah ada pada jalur konsultasi `BE-LAB-54`. Endpoint ini **membaca**, nol menyimpan |
| Menyegarkan snapshot EF | Lihat bagian 3 |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji tertinggal di dev, atas izin pemilik modul yang memilih membiarkannya** supaya
> `FE-LAB-33` punya bahan:
>
> | Tabel | `Id` | Isi |
> |---|---|---|
> | `TrxRosterPeriod` | `aa000059-…-0001` | `UJI-LAB59` |
> | `MstOnCallType` | `aa000059-…-0002` | `UJI-JAGA` |
> | `TrxOnCallAssignment` | `aa000059-…-0003` | `dr. Nabila Rahmawati`, aktif ±10 jam ke depan |
>
> **Ketiganya milik Human Resource, bukan Laboratorium, dan seluruhnya berawalan `UJI-`.**
> Jendela penugasannya akan lewat dengan sendirinya; sesudah itu jalur jatuh menyala kembali.
>
> Ditambah satu pesanan uji `14344c44-…` beserta wadah dan pemeriksaannya pada kunjungan
> `e3338713-…`, dipakai membuktikan bagian 2.4.
