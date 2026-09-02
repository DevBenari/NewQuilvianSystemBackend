# Tiga Pekerjaan untuk Master Data dan Registrasi — Modul Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-003` |
| `tanggal` | 2026-09-02 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium |
| `kepada` | Pemilik `master-data` (dua pekerjaan) dan pemilik `registration-management` (satu pekerjaan) |
| `induk` | `LAB-REQ-001` butir 1, 2, dan 3 — **sudah disetujui** `andryzainhome` dan `sukmagp` pada 2026-09-01 |
| `sifat` | Operasional. Bukan permintaan izin — izinnya sudah ada |

**Ini bukan permintaan persetujuan.** Ketiganya sudah disetujui pemilik repository pada
2026-09-01. Dokumen ini menjelaskan **apa persisnya yang perlu dibuat**, supaya tidak perlu
menafsirkan ulang dari blueprint.

## Kenapa ketiganya bisa dimulai lebih dulu

Modul Laboratorium sedang tertahan karena baris registry `LaboratoryManagement / Laboratory`
masih berstatus `PLANNED`. **Ketiga pekerjaan di bawah tidak tersentuh penahan itu**, karena
baris registry pemiliknya sudah aktif:

| Pekerjaan | Pemilik | Baris registry | Lifecycle |
|---|---|---|---|
| `BE-EXT-01` | `master-data` | `Master / Reference` — `Mst` | **ACTIVE** |
| `BE-EXT-02` | `master-data` | `Master / Reference` — `Mst` | **ACTIVE** |
| `BE-EXT-03` | `registration-management` | `RegistrationManagement / Registration` — `Reg` | **ACTIVE / LEGACY** |

Karena `BE-LAB-07` katalog dan `BE-LAB-08` pendaftaran sama-sama menunggu ketiganya, memulai
sekarang tidak membuang waktu.

---

## 1. `BE-EXT-01` — Kolom disiplin pada `MstProcedure`

**Untuk pemilik `master-data`. Menahan gelombang `MVP-0`.**

| Butir | Isi |
|---|---|
| **Berkas model** | `Areas/HealthServices/MasterData/Models/MstProcedure.cs` |
| **Yang ditambahkan** | Satu kolom `LabDiscipline` bertipe enum, **boleh kosong**, ber-index |
| **Nilainya** | Patologi Klinik, Patologi Anatomi, Mikrobiologi. Hanya bermakna bila `IsLaboratory` bernilai benar |
| **Pengisian data lama** | Isi untuk seluruh pemeriksaan berpenanda `IsLaboratory` yang sudah ada |
| **Dasar** | `LAB-DEC-036`, `LAB-COORD-005` |

```sql
ALTER TABLE public."MstProcedure" ADD COLUMN "LabDiscipline" integer;
CREATE INDEX "IX_MstProcedure_LabDiscipline" ON public."MstProcedure" ("LabDiscipline");
```

**Kenapa satu kolom ini boleh, sementara kolom lain tidak.** `MstProcedure` sudah punya
`IsLaboratory`, `IsRadiology`, `IsSurgery`, dan `IsTherapy` — seluruhnya **klasifikasi** jenis
tindakan. Yang diminta sejenis dengan itu.

Yang **tidak** diminta dan memang tidak boleh masuk: satuan hasil, batas nilai, jenis wadah.
Ketiganya berada di tabel milik Laboratorium sendiri.

**Akibat nyata bila tidak ada.** Sistem tidak dapat memeriksa apakah pemeriksaan yang dipilih
sesuai disiplin pesanannya. Petugas dapat memasukkan Hemoglobin ke pesanan Mikrobiologi, dan
sistem tidak akan menolaknya.

**Bukti selesai.** Kolom ada, terisi untuk seluruh pemeriksaan `IsLaboratory`, dan penyaringan
katalog per disiplin bekerja.

---

## 2. `BE-EXT-02` — Dua data induk perujuk

**Untuk pemilik `master-data`. Menahan gelombang `MVP-1`.**

| Butir | Isi |
|---|---|
| **Berkas model** | `Areas/HealthServices/MasterData/Models/MstReferralInstitution.cs` dan `MstReferralDoctor.cs` |
| **Dasar** | `LAB-DEC-035`, `LAB-COORD-004` |

**Isi minimum:**

| Data induk | Kolom |
|---|---|
| `MstReferralInstitution` | `InstitutionCode` (unik), `InstitutionName`, alamat, telepon, penanda aktif |
| `MstReferralDoctor` | `ReferralInstitutionId` (FK ke instansinya), `DoctorName`, penanda aktif |

```sql
CREATE TABLE public."MstReferralInstitution" ( … );
CREATE UNIQUE INDEX "IX_MstReferralInstitution_InstitutionCode"
    ON public."MstReferralInstitution" ("InstitutionCode") WHERE "IsDelete" = false;

CREATE TABLE public."MstReferralDoctor" ( … );
ALTER TABLE public."MstReferralDoctor"
    ADD CONSTRAINT "FK_MstReferralDoctor_MstReferralInstitution_ReferralInstitutionId"
    FOREIGN KEY ("ReferralInstitutionId")
    REFERENCES public."MstReferralInstitution" ("Id") ON DELETE RESTRICT;
```

**Kenapa global, bukan milik Laboratorium.** Rujukan bukan hal khusus laboratorium. Kunjungan
pasien sudah punya penanda `IsReferral` sejak awal, dan Rawat Jalan maupun IGD juga menerima
pasien rujukan. Menaruhnya di Laboratorium berarti modul lain kelak membuat daftar tandingan.

**Kenapa dokter perujuk tidak memakai data induk dokter yang sudah ada.** Dokter pada
`master-data` adalah dokter **rumah sakit ini**. Dokter perujuk adalah dokter **di luar** rumah
sakit — dua populasi yang berbeda.

**Akibat nyata bila tidak ada.** Nama klinik perujuk hanya dapat diketik bebas. "Klinik Sehat
Sentosa", "Kl. Sehat Sentosa", dan "sehat sentosa" akan terhitung sebagai tiga institusi
berbeda, dan laporan dokter pengirim tidak akan pernah dapat dipercaya.

**Bukti selesai.** Kedua tabel ada, dokter tertaut ke instansinya, keduanya dapat dipilih dari
daftar oleh modul mana pun.

---

## 3. `BE-EXT-03` — Penunjuk perujuk pada kunjungan dan kontrak pemanggilan

**Untuk pemilik `registration-management`. Menahan gelombang `MVP-1` seluruhnya.**

Dua hal sekaligus.

### 3a. Dua kolom pada `TrxPatientEncounter`

| Kolom | Tipe | Wajib | Keterangan |
|---|---|---|---|
| `ReferralInstitutionId` | `Guid?` | Tidak | FK ke `MstReferralInstitution` |
| `ReferralDoctorId` | `Guid?` | Tidak | FK ke `MstReferralDoctor` |

Bergantung pada `BE-EXT-02` selesai lebih dulu.

### 3b. Kontrak pemanggilan `INT-05`

**Laboratorium tidak akan menulis ke tabel kunjungan.** Rancangannya justru sebaliknya: layar
pendaftaran berada di modul Laboratorium supaya petugas tidak berpindah aplikasi, tetapi
**Registrasi yang membuat kunjungannya**. Laboratorium mengirim isian, menunggu jawaban, lalu
menyimpan penunjuk kunjungan yang dikembalikan.

| Aspek | Yang perlu disepakati |
|---|---|
| Bentuk permintaan dan jawaban | Isian pendaftaran masuk, penunjuk kunjungan keluar — cukup memuat penunjuk kunjungan, nomor kunjungan, dan identitas pasien seadanya |
| Idempotensi | **Wajib.** Petugas menekan Simpan dua kali tidak boleh menghasilkan dua kunjungan untuk satu pasien pada hari yang sama |
| Perilaku saat ditolak | Penolakan diteruskan apa adanya. Laboratorium tidak menyimpan data setengah jadi |

**Kabar baiknya: sebagian besar sudah ada.** Pemeriksaan pada `c87d9c0` menemukan Registrasi
sudah memiliki `EncounterRegistrationSource.WalkIn`, kolom `IsWalkIn`, penanda `IsReferral`,
`ReferralNumber`, `IsReferralRequired`, `IsReferralVerified`, dan `PatientEncounterController`
yang sudah menangani pembuatan kunjungan datang langsung. Yang belum ada hanya dua kolom
penunjuk perujuk dan kesepakatan bentuk pemanggilannya.

**Akibat nyata bila tidak ada.** Pasien yang datang langsung ke laboratorium tidak dapat
dilayani sama sekali. Ia harus mengantre di loket pendaftaran lebih dulu, padahal ia hanya
perlu satu pemeriksaan darah.

**Bukti selesai.** Dua kolom ada, kontrak `INT-05` disepakati tertulis, dan idempotensi terbukti
lewat uji: menekan Simpan dua kali menghasilkan satu kunjungan, bukan dua.

---

## 4. Ringkasan

| Task | Kepada | Yang dibuat | Menahan |
|---|---|---|---|
| `BE-EXT-01` | `master-data` | Satu kolom `LabDiscipline` pada `MstProcedure` | `MVP-0`, `BE-LAB-07` |
| `BE-EXT-02` | `master-data` | `MstReferralInstitution`, `MstReferralDoctor` | `MVP-1`, `BE-EXT-03` |
| `BE-EXT-03` | `registration-management` | Dua kolom FK pada `TrxPatientEncounter` + kontrak `INT-05` | `MVP-1`, `BE-LAB-08` |

Urutannya: `BE-EXT-01` berdiri sendiri; `BE-EXT-02` mendahului `BE-EXT-03`.

**Yang tidak diminta:** memindahkan data tarif (tarif tetap milik Master Data, Laboratorium
hanya menyajikannya baca-saja), mengubah cara kerja modul mana pun, dan menambah kolom
operasional pada `MstProcedure` di luar satu kolom klasifikasi itu.

---

## 5. Rujukan

| Dokumen | Isi |
|---|---|
| `roadmap/backend-roadmap.md` | `BE-EXT-01` sampai `BE-EXT-03` lengkap dengan acceptance criteria dan Definition of Done |
| `erd/data-dictionary.md` bagian 9b | Definisi kolom lengkap beserta DDL |
| `contracts/integration-contract.md` `INT-05` | Kontrak pemanggilan Registrasi |
| `approval-requests/2026-09-01-permintaan-koordinasi-lintas-modul.md` bagian 2 | Permintaan asli beserta persetujuannya |
