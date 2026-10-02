# Verifikasi Runtime `FE-IGD-027` — dijalankan pemilik

| Field | Nilai |
| --- | --- |
| Tanggal | 18 September 2026 |
| Jenis | **Evidence pemilik.** Uji lewat layar dijalankan Product/Domain Owner, bukan agent |
| Pelapor | Rizki Gunawan, Product/Domain Owner IGD |
| Task | `FE-IGD-027` |
| Frontend | branch `RizkiV2` |
| Cakupan | Penetapan dan pengalihan dokter selama episode IGD. **Bukan UAT** |

---

## A. Tiga belas pemeriksaan — seluruhnya `PASS`

| # | Yang diuji | Hasil |
| ---: | --- | :-: |
| 1 | Visit baru tanpa assignment | **PASS** |
| 2 | Section dokter muncul sesudah triage | **PASS** |
| 3 | Empty state *"Belum ada dokter penanggung jawab pada kunjungan ini."* | **PASS** |
| 4 | Tetapkan dokter pertama | **PASS** |
| 5 | Nama dokter tampil, bukan GUID | **PASS** |
| 6 | Nama penugas tampil, bukan GUID | **PASS** |
| 7 | Assignment aktif tampil *"Sedang berjalan"* | **PASS** |
| 8 | UI berubah dari mode Tetapkan ke mode Alihkan | **PASS** |
| 9 | Alasan handover wajib | **PASS** |
| 10 | Handover dokter | **PASS** |
| 11 | Histori dokter lama tetap tersimpan | **PASS** |
| 12 | Dokter baru menjadi assignment aktif | **PASS** |
| 13 | Alasan pengalihan tampil | **PASS** |

Contoh penetapan awal yang berhasil:

```
dr. Maya Permata Sari
Sedang berjalan
Sejak 18 Sep 2026, 14.48
Ditetapkan SuperAdmin
```

## B. Skenario yang **tidak dapat** diuji pada basis data dev saat ini

| Skenario | Hasil | Sebab |
| --- | :-: | --- |
| Tampilan baris hasil pengisian data lama (`assignedByName` kosong → *"Data historis"*) | **NOT FEASIBLE** | **Nol** data yang cocok. Kueri kandidat backfill pemilik mengembalikan **0 baris** — tidak ada kunjungan yang `RegPatientEncounter.DoctorId`-nya terisi sementara `EmgDoctorAssignment`-nya kosong |

**Ini bukan `FAIL`.** Tidak ada yang rusak; yang tidak ada adalah datanya. Jalur tampilannya
sudah ditulis dan menunggu `BE-IGD-048` berjalan pada environment yang memang punya data lama.

## C. Bukti basis data dari pemilik

Kueri silang `EmgVisit` × `RegPatientEncounter` × `EmgDoctorAssignment` menunjukkan tiga pola,
dan ketiganya **sehat**:

| Pola | Arti |
| --- | --- |
| `EncounterDoctorId` ada + assignment ada | Sudah punya riwayat penugasan |
| `EncounterDoctorId` `NULL` + assignment `NULL` | Memang belum punya dokter |
| Satu `EmergencyVisitId` punya lebih dari satu baris assignment | **Riwayat handover, bukan cacat** — memang bentuk yang dituju `IGD-DEC-082` |

Kueri kandidat backfill:

```
WHERE e."DoctorId" IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" a
                  WHERE a."EmergencyVisitId" = v."Id")
```

**Hasil: 0 baris.**

**Nol kandidat tidak berarti `BE-IGD-048` tidak diperlukan.** Task itu tetap relevan sebagai
jalur migrasi bagi environment dan basis data lain yang memang memuat data lama. Pada dev saat
ini, migration-nya akan menyisipkan 0 baris — dan itu hasil yang benar, bukan kegagalan.

## D. Batas bisnis yang ditegaskan pemilik

| Pernyataan | Isi |
| --- | --- |
| `EmgDoctorAssignment` **adalah** | Riwayat tanggung jawab dokter **selama episode IGD**, yang memang berganti mengikuti shift, jam jaga, dan handover operasional |
| `EmgDoctorAssignment` **bukan** | DPJP Rawat Inap |
| Bila pasien dirawat inap | Episode IGD → keputusan rawat inap → admisi → **DPJP Rawat Inap ditetapkan domain Rawat Inap**. Penugasan IGD **tidak** diteruskan otomatis |
| Dokter aktif IGD | **Tidak** otomatis menjadi author seluruh SOAP, order, dan tindakan. Aksi klinis tetap memakai pelaku yang benar-benar melakukannya |

## E. `RegPatientEncounter.DoctorId` dipertahankan

Keduanya punya peran berbeda dan **tidak** saling menggantikan:

| Kolom | Peran |
| --- | --- |
| `RegPatientEncounter.DoctorId` | **Pointer** dokter efektif pada encounter |
| `EmgDoctorAssignment` | **Riwayat temporal** dokter IGD |

Contoh: dr. A pukul 09.00, handover ke dr. B pukul 14.00.

| Tempat | Isi |
| --- | --- |
| `RegPatientEncounter.DoctorId` | dr. B |
| `EmgDoctorAssignment` | dr. A 09.00 → 14.00; dr. B 14.00 → `NULL` |

`RegPatientEncounter.DoctorId` **tidak dihapus**.

## F. Terminologi diperketat

Banner sempat menulis *"dokter pemeriksa"* sementara section menulis *"Dokter Penanggung
Jawab"*. Atas permintaan pemilik keduanya diselaraskan menjadi **"Dokter Penanggung Jawab
IGD"**, supaya tidak rancu dengan DPJP Rawat Inap.

Perubahan ini **hanya kata**, dan **tidak** memperluas lingkup ke DPJP Rawat Inap.
