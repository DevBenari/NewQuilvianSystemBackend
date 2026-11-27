# Pemilihan Penyedia — Matriks Uji Penerimaan

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PS-TEST` · `last_changed_in: 1.1` · status `approved` (`PBJ-DEC-095`, Pengguna sesi, 2026-11-26; 1.1 `PBJ-DEC-097`, 2026-11-27) |
| Masukan | `PBJ-AC-006`, `011`, `013`, `022`..`036`; `PBJ-PS-STATE`, `-VAL`, `-PERM`, `-INT` 1.0 |
| Lingkungan uji | `QuilvianNewDevSukma`; aktor non-SuperAdmin dibuat lewat API; matriks `Wfl` uji dipasang lalu dibersihkan (pola `AT-PP`/`AT-VD`) |

Jenis test: **API** = panggilan endpoint + pemeriksaan DB baca-saja; **Layar** = uji layar dengan aktor berizin; **Build** =
build/QBE/EF.

## 1. Pembentukan paket

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PS-01` | `PBJ-DEC-071` | Buat paket dari baris PR IGD 10 dan Rawat Inap 5 kursi roda | API | Satu item 15 unit, dua sumber; baris PR ber-`PackageId` |
| `AT-PS-02` | `PBJ-AC-026` | Baris yang sama dimasukkan ke paket kedua | API | 409 `PBJ-VAL-121`; paket kedua tidak terbentuk |
| `AT-PS-03` | `PBJ-AC-027` | PR berisi dua baris; satu dipaketkan | API | PR tetap `APPROVED`; setelah baris kedua dipaketkan → `CONVERTED_TO_PACKAGE` |
| `AT-PS-04` | `PBJ-DEC-090`, `091` | Tambah baris jenis atau tahun berbeda | API | 422 `PBJ-VAL-122` / `PBJ-VAL-123` |
| `AT-PS-05` | `PBJ-DEC-082` | Paket dengan rujukan rencana paket | API | `PackagePlanId` tersimpan; isi tetap baris PR |
| `AT-PS-06` | `PBJ-DEC-020` | Pengguna tanpa `ReadAllUnits` membuka paket unit lain | API | 404 |

## 2. Persiapan dan persetujuan

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PS-10` | `PBJ-AC-022` | Ajukan persiapan dengan HPS satu barang 0 | API | 422 `PBJ-VAL-124` |
| `AT-PS-11` | `PBJ-DEC-068` | HPS Rp 42.750.000 → saran metode | API | Saran "Penunjukan Langsung (contoh)" |
| `AT-PS-12` | `PBJ-AC-023` | Metode di luar rentang tanpa alasan, lalu dengan alasan | API | 422 `PBJ-VAL-126`; dengan alasan: kondisi `PBJ_METHOD_OUT_OF_RANGE` aktif, langkah tambahan dibuat |
| `AT-PS-13` | `PBJ-ASM-007` | Jasa tanpa KAK/TOR | API | 422 `PBJ-VAL-127` menyebut KAK/TOR |
| `AT-PS-14` | `PBJ-DEC-038` | Ajukan saat definisi tidak aktif | API | 409 `PBJ-VAL-091` (1.1, `PBJ-DEC-097`); paket tetap `DRAFT` |
| `AT-PS-15` | `PBJ-DEC-072` | Disetujui `Wfl`, dikembalikan, ditolak | API + Layar | `READY_FOR_RFQ` + `ApprovedHpsTotal` + nomor `PKT`; `PREPARATION_RETURNED`; `REJECTED` + baris PR dilepas |
| `AT-PS-16` | `PBJ-AC-002` | Pengaju menyetujui persiapannya sendiri | API | Ditolak; tidak ada tugas bagi pengaju |
| `AT-PS-17` | `PBJ-AC-023` | Ubah rentang HPS metode di master setelah paket memilihnya | API | Salinan metode di paket tidak berubah |
| `AT-PS-18` | `PBJ-AC-022` | Ubah HPS paket `READY_FOR_RFQ` | API | 409 `PBJ-VAL-149`; lewat buka ulang → `DRAFT` |

## 3. RFQ dan penawaran

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PS-20` | `PBJ-AC-024` | Kirim RFQ saat persiapan belum disetujui | API | 409 `PBJ-VAL-129` |
| `AT-PS-21` | `PBJ-ASM-006`, `PBJ-AC-011` | Undang vendor tidak layak untuk salah satu kategori | API | Tidak muncul di `eligible-vendors`; dipaksa → 422 `PBJ-VAL-130` |
| `AT-PS-22` | `PBJ-AC-013` | Undang vendor Dibatasi tanpa alasan | API | 422 `PBJ-VAL-131` |
| `AT-PS-23` | `PBJ-DEC-081`, `PBJ-AC-030` | Penawaran diterima 17.10, batas 16.00 | API | `IsLate = true`; tidak dihitung sah |
| `AT-PS-24` | `PBJ-DEC-081` | Perpanjang sebelum dan sesudah batas lewat | API | Sebelum: batas baru untuk semua undangan + riwayat; sesudah: 422 `PBJ-VAL-132` |
| `AT-PS-25` | `PBJ-AC-035` | Catat Rp 2.500.000 + PPN 11% | API | `FinalUnitPrice` 2.775.000,00 |
| `AT-PS-26` | `PBJ-DEC-067` | Catat penawaran vendor tidak diundang; catat ganda | API | 422 `PBJ-VAL-133`; 409 `PBJ-VAL-134` |

## 4. Evaluasi dan negosiasi

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PS-30` | `PBJ-AC-028` | Pencatat mengevaluasi penawaran yang ia catat | API + Layar | 403 `PBJ-VAL-135`; tombol tidak tampil |
| `AT-PS-31` | `PBJ-AC-029` | Evaluator membuka paket, penawaran, dan lampiran | API | Tidak ada field harga di respons; lampiran Surat Penawaran Harga tidak terdaftar dan unduhannya 404 |
| `AT-PS-32` | `PBJ-DEC-074` | Evaluator membuka paket yang tidak ditugaskan | API | 404 |
| `AT-PS-33` | `PBJ-DEC-074` | `FAIL` tanpa alasan | API | 422 `PBJ-VAL-136` |
| `AT-PS-34` | `PBJ-ASM-005` | Negosiasi menaikkan harga; negosiasi penawaran terlambat | API | 422 `PBJ-VAL-137`; 422 `PBJ-VAL-138` |
| `AT-PS-35` | `PBJ-DEC-075` | Penawaran sah 2 dari minimum 3 → RFQ gagal → RFQ ulang | API | RFQ lama `FAILED` tersimpan; paket `READY_FOR_RFQ`; RFQ baru bernomor baru |

## 5. Penetapan dan komitmen

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PS-40` | `PBJ-AC-025` | Harga akhir Rp 44.000.000 > HPS Rp 42.750.000 tanpa alasan, lalu dengan alasan | API | 422 `PBJ-VAL-141`; dengan alasan: kondisi `PBJ_OVER_HPS` aktif |
| `AT-PS-41` | `PBJ-AC-036` | Pilih bukan terendah tanpa alasan | API | 422 `PBJ-VAL-142`; dengan alasan: peringkat tersimpan di `RankingSnapshotJson` dan tampil di `-10` |
| `AT-PS-42` | `PBJ-AC-031` | Ajukan penetapan vendor yang diblokir | API | 422 `PBJ-VAL-143` |
| `AT-PS-43` | `PBJ-DEC-076`, `092` | Vendor menjadi tidak layak setelah diajukan, lalu disetujui | API | Award `NOT_APPLIED`; tanpa komitmen; paket `EVALUATION` |
| `AT-PS-44` | `PBJ-AC-032` | Penetapan Rp 41.250.000 disetujui untuk sumber IGD 10 + Rawat Inap 5 | API + DB | Dua komitmen aktif `PBJ_PACKAGE_AWARD`: Rp 27.500.000 dan Rp 13.750.000; jumlah = Rp 41.250.000 |
| `AT-PS-45` | `PBJ-AC-026` | Ajukan penetapan kedua untuk paket yang sama | API | 409 (index unik penetapan hidup) |
| `AT-PS-46` | `PBJ-DEC-094` | Penetapan disetujui tetapi garis pagu tidak ada | API | Tidak diterapkan; galat sinkron tercatat; sinkron ulang berhasil setelah pagu dibuat |

## 6. PO, batal, pecah

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PS-50` | `PBJ-AC-006` | Catat PO-2026-0412 di dua paket | API | Paket kedua 409 `PBJ-VAL-145` |
| `AT-PS-51` | `PBJ-ASM-009` | Catat nomor PO sebelum penetapan disetujui | API | 409 `PBJ-VAL-146` |
| `AT-PS-52` | `PBJ-AC-033` | Batal paket `AWARDED`; batal paket `ORDERED` | API + DB | Komitmen dilepas, baris PR bebas, PR kembali `APPROVED`; `ORDERED` → 409 `PBJ-VAL-147` |
| `AT-PS-53` | `PBJ-AC-034` | Pecah paket `EVALUATION` berisi kursi roda dan tongkat | API + DB | Paket baru membawa item tongkat, sumber, penawaran, hasil evaluasi, metode asal; baris PR pindah `PackageId` |
| `AT-PS-54` | `PBJ-AC-034` | Pecah saat penetapan diajukan; pecah semua item | API | 409 `PBJ-VAL-144` |
| `AT-PS-55` | `PBJ-DEC-093` | Cabut penetapan disetujui sebelum PO | API + DB | Award `REVOKED`; komitmen dilepas; paket `EVALUATION` |

## 7. Fondasi dan konformitas

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PS-60` | `PBJ-DEC-096` | Baris `ProcurementManagement / Sourcing` `ACTIVE` sebelum model pertama | Dokumen | Baris registry dan catatan perubahan (didaftarkan 2026-11-26) |
| `AT-PS-61` | QBE | Build Release 0 error; QBE Strict `PASS` | Build | Log build dan QBE |
| `AT-PS-62` | Kamus data | Skema sesuai `data-dictionary.md`; tanpa perubahan model tertunda | Build + DB | `dotnet ef migrations has-pending-model-changes` bersih |
| `AT-PS-63` | `03-frontend-architecture.md` bagian 3 | Setiap layar terjangkau dari menu atau layar induk | Layar | Tangkapan layar per butir menu dan layar anak |
| `AT-PS-64` | Data master awal | Seeder idempoten; metode contoh bertanda `IsSample` | API + DB | Jalankan dua kali, jumlah baris tetap |
| `AT-PS-65` | `PBJ-FE-004` | Daftar `-01`, `-04`, `-06`, `-09` memakai baris filter satu baris | Layar | Tangkapan layar |
