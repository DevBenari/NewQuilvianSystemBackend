# Permintaan Keputusan Owner — Aturan Member pada Antrean Rawat Jalan

| Field | Nilai |
|---|---|
| Dokumen ID | `CRM-DECREQ-001` |
| Revisi | `1` (draf) |
| Tanggal | `2026-10-15` |
| Dari | Yoga Aji Pratama, owner `crm-membership` |
| Kepada | Sukma Giri, owner Rawat Jalan (Product/Domain) |
| Pokok | Amendment `AG-03`: antrean RJ memakai aturan member tunggal milik CRM |
| Sifat dokumen | Permintaan keputusan. Dokumen ini **tidak** mengubah status blueprint dan **tidak** memberi wewenang implementasi |
| Task yang tertahan | `BE-CRM-022` (roadmap `crm-membership`) |
| Task yang tidak tertahan | Semua task CRM lain. Antrean RJ tetap berjalan dengan aturannya sendiri |
| Catatan 2026-10-16 | Sejak handover `CRM-DEC-031`, sisi CRM untuk antrean RJ juga dipegang Sukma Giri, sehingga penanya dan penjawab dokumen ini sekarang orang yang sama. Pertanyaan 1 dan 2 tetap perlu dijawab, dan dijawab dalam sesi `grill-me` Amendment RP di `rawat-jalan`. Pertanyaan 3: penyetujunya Sukma Giri. Pelaksana ditentukan saat `BE-CRM-022` dibuka |

## Untuk siapa dokumen ini

Dokumen ini ditujukan kepada pemilik proses Rawat Jalan. Setiap pertanyaan disertai contoh
kasus pasien dan akibat tiap pilihan, sehingga bisa dijawab tanpa membaca kode.

Ringkasnya, kami meminta izin mengubah **satu hal saja** di antrean RJ: cara sistem menentukan
apakah pasien terhitung member. Hal lain dari antrean, seperti prioritas, nomor, dan tampilan
layar publik, tidak berubah.

## Cara mengisi

1. Baca **Keputusan 1** sampai **Keputusan 3**.
2. Isi kolom `Jawaban owner` pada [formulir jawaban](#formulir-jawaban) di bagian akhir.
3. Bila tidak ada pilihan yang cocok, pilih `Other` dan tuliskan penjelasannya.

Bila waktu terbatas, cukup jawab **Keputusan 1**. Jawaban itu saja sudah membuka `BE-CRM-022`.

---

## Latar belakang

Modul CRM Membership menetapkan **satu aturan "member aktif"** untuk semua titik layanan
(`CRM-DEC-005`, aturan `J1`):

> Pada tanggal X (kalender WIB), pasien adalah member bila punya membership berstatus **Aktif**
> dengan `tanggal mulai periode ≤ X ≤ tanggal berakhir`, dan tingkatnya masih aktif.
> Selain itu pasien Reguler. Tidak ada masa tenggang.

Aturan ini sudah dipakai oleh layar data pasien, pencarian member, kiosk, dan proses terjadwal
yang mengubah status menjadi Kedaluwarsa.

Antrean RJ belum memakainya. Antrean punya aturan sendiri di
`OutpatientQueueClassificationService` (keputusan `RJ-DOC-DEC-055`). Aturan itu sudah
memeriksa status dan tanggal, jadi hasilnya sering sama. Tetapi pada beberapa kasus hasilnya
berbeda, sehingga satu pasien bisa **Reguler di satu layar tetapi masuk antrean prioritas member
di antrean RJ**. Itulah yang ingin ditutup oleh `AG-03`.

---

## Keputusan 1 — Apakah antrean RJ boleh memakai aturan member CRM?

### Perbedaan kedua aturan

| # | Kasus | Antrean RJ sekarang | Aturan CRM (`J1`) |
|---|---|---|---|
| 1 | Membership Aktif **tanpa tanggal berakhir** | Member, dianggap berlaku tanpa batas | **Reguler** (data tidak lengkap, sistem memilih aman) |
| 2 | Membership Aktif **tanpa tanggal mulai periode** | Member, karena yang dicek tanggal bergabung | **Reguler** |
| 3 | Tanggal bergabung berbeda dari tanggal mulai periode | Dihitung dari tanggal bergabung | Dihitung dari tanggal mulai periode |
| 4 | Pergantian hari | Tanggal kunjungan versi UTC | Tanggal kalender WIB |
| 5 | Membership Aktif yang flag `IsActive`-nya dimatikan | Reguler | Member |

Contoh kasus 1. Pak Andi punya membership Gold hasil migrasi data lama yang tidak memiliki
tanggal berakhir. Hari ini perawat di nurse station melihat Pak Andi sebagai **Reguler**, tetapi
antrean RJ memberinya **nomor prioritas Gold**. Petugas tidak bisa menjelaskan mana yang benar.

Catatan kasus 5: flag ini ada di data membership, tetapi aturan CRM belum memeriksanya. Kasus ini
dibahas di sisi CRM dan tidak perlu dijawab di sini.

### Yang **tidak** berubah

| Hal | Tetap seperti sekarang |
|---|---|
| Pemisahan Member, Prioritas, dan Privasi layar publik (`RJ-DOC-DEC-055`) | Ya. Prioritas tetap dari `PriorityQueue`/`PriorityLevel` tingkat, privasi dari `PublicDisplayMode` |
| Snapshot klasifikasi di antrean saat nomor terbit | Ya |
| Nomor antrean, audience, dan payload realtime | Ya |
| Data induk tingkat (`RJ-DOC-DEC-059`) | Ya, tidak di-seed |
| Pasien yang memenuhi aturan CRM | Klasifikasinya sama persis dengan sekarang |

Yang berubah hanya **siapa yang terhitung member**: pasien pada kasus 1 sampai 4 di atas.

### Seberapa banyak pasien terdampak

Belum bisa diukur. Database pengembangan (`QuilvianNewDevYoga`) saat ini **tidak punya satu pun
membership berstatus Aktif**: 3 Batal, 2 Menunggu Pembayaran, 1 Kedaluwarsa (kueri baca-saja,
15 Okt 2026). Angka nyata hanya bisa diambil dari data produksi atau staging. Kuerinya kami
siapkan di [Lampiran A](#lampiran-a--kueri-dampak-baca-saja).

```text
Pertanyaan 1:
Apakah antrean RJ boleh memakai aturan member CRM (J1)?

Pilihan A (rekomendasi CRM):
Boleh, langsung berlaku penuh.
Akibat: satu aturan member di semua layar. Pasien kasus 1–4 menjadi Reguler di antrean
mulai rilis tersebut. Petugas perlu diberi tahu sebelumnya.

Pilihan B:
Boleh, tetapi setelah data dibereskan.
Membership Aktif yang tanggalnya tidak lengkap diverifikasi dulu oleh petugas CRM,
baru antrean dialihkan.
Akibat: tidak ada pasien yang tiba-tiba turun ke Reguler. Butuh waktu verifikasi dan
hasil Lampiran A untuk tahu seberapa banyak datanya.

Pilihan C:
Belum boleh.
Akibat: antrean tetap memakai aturannya sendiri. Perbedaan kasus 1–4 tetap ada dan
dicatat sebagai gap (GAP-07). BE-CRM-022 tetap tertahan.

Pilihan D:
Other.
```

---

## Keputusan 2 — Status member dinilai pada tanggal daftar atau tanggal kunjungan?

Bagian ini muncul saat kami menelusuri kode. Ia berlaku apa pun jawaban Keputusan 1.

`CRM-DEC-005` menyatakan: *kunjungan yang sudah terdaftar sebelum membership habis tetap memakai
tingkat saat didaftarkan.* Contoh resmi dari keputusan itu:

> Gold berlaku 14 Okt 2025 s.d. 13 Okt 2026. Reservasi yang **didaftarkan 13 Okt** dan
> **dilayani 14 Okt** tetap Gold. Pendaftaran baru pada 14 Okt sudah Reguler.

Antrean RJ sekarang menilai status member pada **tanggal kunjungan**, bukan tanggal
pendaftaran. Pada contoh di atas, antrean memberi pasien itu **Reguler**. Hasil ini bertentangan
dengan `CRM-DEC-005`.

Perlu diketahui juga akibat sebaliknya bila tanggal pendaftaran yang dipakai. Pasien yang membuat
booking pada 1 Okt untuk kunjungan 20 Okt tetap mendapat prioritas Gold pada 20 Okt, walaupun
membershipnya sudah habis sejak 13 Okt.

```text
Pertanyaan 2:
Untuk booking ke tanggal mendatang, status member dinilai pada tanggal apa?

Pilihan A:
Tanggal pendaftaran booking (sesuai CRM-DEC-005).
Akibat: janji kepada member dihormati. Pasien bisa tetap prioritas walau membership
sudah habis pada hari kunjungan.

Pilihan B:
Tanggal kunjungan (seperti antrean sekarang).
Akibat: hanya member yang masih berlaku pada hari itu yang mendapat prioritas.
CRM-DEC-005 perlu diamandemen oleh owner CRM supaya kedua modul sejalan.

Pilihan C:
Other.
```

Pendaftaran langsung pada hari yang sama tidak terpengaruh, karena tanggal daftar sama dengan
tanggal kunjungan.

---

## Keputusan 3 — Siapa yang mengerjakan dan menyetujui perubahan?

Perubahan teknisnya kecil dan hanya menyentuh modul Registration/Rawat Jalan:

| Bagian | Isi |
|---|---|
| `OutpatientQueueClassificationService.cs` | Kueri member sendiri (baris 58–69) diganti dengan pemanggilan aturan CRM. Data prioritas tingkat tetap dibaca seperti sekarang |
| `PatientEncounterController.cs` baris 103 | Cara membuat service disesuaikan karena service butuh komponen baru |
| Uji | Uji klasifikasi sebelum dan sesudah. Pasien yang memenuhi aturan CRM harus mendapat hasil identik |

```text
Pertanyaan 3:
Siapa yang mengerjakan dan siapa yang menyetujui hasilnya?

Pilihan A (rekomendasi CRM):
Dikerjakan tim CRM (Yoga), direview dan disetujui Sukma sebelum digabung.

Pilihan B:
Dikerjakan tim Rawat Jalan (Sukma).

Pilihan C:
Other.
```

---

## Formulir jawaban

| ID | Pertanyaan singkat | Pemilik | Jawaban owner | Catatan |
|---|---|---|---|---|
| `1` | Antrean RJ memakai aturan member CRM (`J1`)? | Rawat Jalan | **A** — boleh, berlaku penuh (Sukma Giri, 16 Okt 2026) | `RJ-PRI-DEC-006` |
| `2` | Status member dinilai pada tanggal daftar atau tanggal kunjungan? | Rawat Jalan + CRM | **A** — tanggal pendaftaran/reservasi dibuat, sesuai `CRM-DEC-005` (Sukma Giri, 16 Okt 2026) | `RJ-PRI-DEC-007`. Pengecualian: membership Dibatalkan sebelum hari H membuat pasien turun ke Reguler (`RJ-PRI-DEC-024`) |
| `3` | Siapa yang mengerjakan dan menyetujui? | Rawat Jalan | Penyetuju Sukma Giri (handover `CRM-DEC-031`). Pelaksana ditentukan saat task dibuka | Catatan header 2026-10-16 |

Permintaan tambahan, bila owner bersedia:

| ID | Permintaan | Jawaban owner |
|---|---|---|
| `T-01` | Siapa yang bisa menjalankan [Lampiran A](#lampiran-a--kueri-dampak-baca-saja) pada data produksi/staging dan mengirim angkanya? | |

---

## Apa yang terbuka setelah dijawab

| Jawaban yang masuk | Yang menjadi mungkin |
|---|---|
| Pertanyaan 1 dijawab `A` | `BE-CRM-022` keluar dari status ⛔ dan dapat direncanakan untuk dikerjakan |
| Pertanyaan 1 dijawab `B` | `BE-CRM-022` dibuka setelah verifikasi data selesai. Butuh hasil `T-01` |
| Pertanyaan 1 dijawab `C` | `BE-CRM-022` tetap ⛔. `GAP-07` dan `AT-36` tetap terbuka |
| Pertanyaan 2 dijawab | Tanggal penilaian masuk acceptance criteria `BE-CRM-022`. Jawaban `B` diikuti amandemen `CRM-DEC-005` |
| Pertanyaan 3 dijawab | Pelaksana dan pemberi persetujuan tercatat di DoD `BE-CRM-022` |

Keputusan akan dicatat di dua tempat: `rawat-jalan/00-interview-decisions.md` (sebagai revisi
definisi "Member" di `RJ-DOC-DEC-055`) dan `crm-membership/00-interview-decisions.md`.

## Batas dokumen ini

Mengisi dokumen ini tidak dengan sendirinya:

- mengubah status task pada roadmap;
- memberi wewenang mengubah source code;
- memberi wewenang menjalankan kueri pada database produksi atau mengubah data.

## Rujukan

| Dokumen | Isi |
|---|---|
| [roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) | Kartu task `BE-CRM-022` |
| [00-interview-decisions.md](00-interview-decisions.md) | `CRM-DEC-005`: aturan kedaluwarsa dan contoh reservasi |
| [03-domain-architecture.md](03-domain-architecture.md) | `AG-03` |
| [contracts/integration-contract.md](contracts/integration-contract.md) | `INT-02`: resolver ke antrean RJ |
| [testing/acceptance-test-matrix.md](testing/acceptance-test-matrix.md) | `AT-36`: pasien kedaluwarsa terbaca Reguler di nurse station dan antrean RJ |
| [../rawat-jalan/00-interview-decisions.md](../rawat-jalan/00-interview-decisions.md) | `RJ-DOC-DEC-055` dan `RJ-DOC-DEC-059` |

---

## Lampiran A — Kueri dampak (baca-saja)

PostgreSQL. Hanya `SELECT`. Sebaiknya dijalankan dengan sesi baca-saja
(`SET default_transaction_read_only = on`). Tanggal acuan adalah hari ini menurut WIB.

```sql
WITH hari AS (SELECT (now() AT TIME ZONE 'Asia/Jakarta')::date AS d),
aktif AS (
  SELECT m.*, t."PriorityQueue"
  FROM "MstPatientMembership" m
  JOIN "MstMembershipTier" t ON t."Id" = m."MembershipTierId"
  WHERE NOT m."IsDelete" AND m."MembershipStatus" = 1          -- 1 = Aktif
    AND t."IsActive" AND NOT t."IsDelete"
),
lama AS (   -- aturan antrean RJ sekarang
  SELECT DISTINCT a."PatientId", a."PriorityQueue" FROM aktif a, hari
  WHERE a."IsActive" AND a."JoinDate" <= hari.d
    AND (a."ExpiredDate" IS NULL OR a."ExpiredDate" >= hari.d)
),
j1 AS (     -- aturan CRM
  SELECT DISTINCT a."PatientId" FROM aktif a, hari
  WHERE a."PeriodStartDate" IS NOT NULL AND a."ExpiredDate" IS NOT NULL
    AND a."PeriodStartDate" <= hari.d AND a."ExpiredDate" >= hari.d
)
SELECT
  (SELECT count(*) FROM aktif)                                   AS membership_aktif,
  (SELECT count(*) FROM aktif WHERE "ExpiredDate" IS NULL)       AS tanpa_tanggal_berakhir,
  (SELECT count(*) FROM aktif WHERE "PeriodStartDate" IS NULL)   AS tanpa_tanggal_mulai,
  (SELECT count(*) FROM lama)                                    AS member_aturan_lama,
  (SELECT count(*) FROM j1)                                      AS member_aturan_crm,
  (SELECT count(*) FROM lama WHERE "PatientId" NOT IN (SELECT "PatientId" FROM j1))
                                                                 AS turun_ke_reguler,
  (SELECT count(*) FROM lama WHERE "PriorityQueue"
     AND "PatientId" NOT IN (SELECT "PatientId" FROM j1))        AS kehilangan_prioritas,
  (SELECT count(*) FROM j1 WHERE "PatientId" NOT IN (SELECT "PatientId" FROM lama))
                                                                 AS naik_ke_member;
```

Arti kolom yang paling penting:

| Kolom | Arti |
|---|---|
| `turun_ke_reguler` | Pasien yang hari ini member di antrean RJ tetapi Reguler menurut aturan CRM |
| `kehilangan_prioritas` | Bagian dari kolom di atas yang tingkatnya memberi nomor prioritas |
| `naik_ke_member` | Pasien yang hari ini Reguler di antrean tetapi member menurut aturan CRM (kasus 5) |
