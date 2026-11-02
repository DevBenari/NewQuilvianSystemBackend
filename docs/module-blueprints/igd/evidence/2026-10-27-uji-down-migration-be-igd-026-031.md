# Bukti — Uji langkah mundur migration `ImplementIgdFullPatientJourney` (`BE-IGD-026` acceptance 1, `BE-IGD-031` acceptance 2)

| Field | Nilai |
| --- | --- |
| Tanggal | 27 Oktober 2026, pagi |
| Task | `BE-IGD-026` acceptance 1 — _"Migration punya langkah mundur tertulis dan sudah diuji di basis data terpisah"_; `BE-IGD-031` acceptance 2 — _"Langkah mundur berupa `RENAME` balik, diuji"_ |
| Migration yang diuji | `20260826090500_ImplementIgdFullPatientJourney` — satu migration memuat kedua langkah: `QueueId` pengkajian dan konsultasi menjadi nullable (`BE-IGD-026`, `BE-IGD-028`) serta `TrxEmergencyTransfer` → `TrxEmergencyDeparture` beserta empat kolom yang berganti nama (`BE-IGD-031`) |
| Wewenang | Rizki Gunawan, 26 Oktober 2026: prompt langkah 5 butir T5 — kontainer `postgres:16` sekali pakai, `--connection <kontainer> --no-build`, sidik jari md5, **basis data dev tidak disentuh**. 27 Oktober 2026: *"lanjutkan pekerjaan"* sesudah commit dan build pemilik |
| Source | Backend `rizkiG` `cdffb114`; DLL hasil build Rizki 27 Oktober 2026 08.49.36, dipakai dengan `--no-build` dan tidak dibuild ulang |
| Klasifikasi bukti | Uji migration pada **basis data sekali pakai di kontainer lokal**. Bukan UAT, bukan uji HTTP, bukan uji layar. Basis data dev **tidak disentuh** |
| Bukti mentah | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-down-migration-be-igd-026-031-20261027/` (di-gitignore): log tiap langkah, `sidik-*.txt`, `objek-*.txt`, `baris-*.txt`, `diff-*.txt`, skrip, dan `hasil.json` yang dirakit `skrip/rakit-hasil.mjs` dari berkas tersebut |

## 1. Apa yang diuji, dengan bahasa sederhana

Migration 26 Agustus 2026 melakukan dua hal yang menjadi pokok kedua task: kolom antrean pada pengkajian
boleh kosong supaya pasien IGD dapat dikaji tanpa antrean, dan tabel perpindahan pasien IGD berganti nama
menjadi tabel kepergian. Kedua kartu menuntut langkah mundurnya **dicoba sungguhan**, bukan hanya
tertulis. Tiga pertanyaan dijawab:

1. Bila migration dibatalkan, apakah tabel dan kolom kembali ke nama lamanya dan **datanya tetap ada**?
2. Bila migration diterapkan lagi sesudahnya, apakah hasilnya **persis sama** dengan penerapan pertama?
3. Bila sudah ada pengkajian IGD tanpa antrean, apa yang terjadi saat pembatalan dicoba?

_Contoh._ Satu kepergian berstatus *Selesai Diserahterimakan* (`TransferStatus` 4) dengan kamar asal
tercatat. Sesudah Up ia menjadi baris `TrxEmergencyDeparture` berstatus fisik 3 dan serah terima 3,
kamar asalnya diarsipkan. Sesudah Down ia kembali menjadi baris `TrxEmergencyTransfer` berstatus 4
dengan kamar asal yang sama — tetapi waktu dan petugas penerimanya hilang (bagian 4).

## 2. Alat dan batas

| Butir | Isi |
| --- | --- |
| Basis data | Kontainer PostgreSQL **16.15** dari image `postgres:16` yang sudah ada di mesin (nol unduhan), port `127.0.0.1:55432`, satu basis data kosong `quilvian_down026`. **Selisih versi:** dev memakai PostgreSQL 15; seluruh perintah migration ini (`ALTER … RENAME`, `SET/DROP NOT NULL`, `ADD CONSTRAINT`) berperilaku sama pada kedua versi mayor |
| Konfigurasi kontainer | `max_locks_per_transaction=4096` (`IGD-FACT-136`) |
| Cara menerapkan | `dotnet ef database update <target> --project QuilvianSystemBackend.csproj --no-build --connection <kontainer>` (dotnet-ef 9.0.18) |
| Pengaman dev | Selain `--connection`, `ConnectionStrings__DefaultConnection` proses `dotnet ef` diarahkan ke kontainer, sehingga basis data dev tidak pernah dialamatkan — termasuk saat `migrations list` |
| Urutan | `migrations list` dari kontainer: 361 migration, `20260826025451_AddTableBillingKasirPArt3` tepat sebelum migration sasaran. Rantai hanya diterapkan **sampai pendahulu itu**, lalu Up/Down satu langkah — tidak membatalkan ±250 migration modul lain di atasnya |
| Pengukuran | `skrip/sidik-jari.sql` baca-saja: jumlah tabel `public`; riwayat migration; md5 **seluruh** kolom, index, dan constraint skema `public`; md5 yang sama **di luar** sembilan tabel sasaran; keberadaan tabel; nullability `QueueId`; md5 baris pengkajian. Ditambah daftar objek sembilan tabel sasaran (`objek-*.txt`) dan baris uji (`baris-*.txt`) |
| Baris uji | Enam baris `TrxEmergencyTransfer`, satu per `TransferStatus` 1–6, dengan alasan, ringkasan, penolakan (status 5), penempatan kamar/tempat tidur (status 2 dan 4), serta `AcceptedAt`/`AcceptedByUserId` (status 4); satu `TrxPatientAssessment` dengan `QueueId` terisi; satu `TrxEmergencyVisit` dan satu `MstServiceUnit` induk. FK dimatikan hanya untuk sesi sisip (`session_replication_role = replica`); kolom wajib lain diisi nilai dummy |
| Pembersihan | `docker rm -f -v` kontainer uji; Docker Desktop dimatikan lagi (semula mati). Image `postgres:16` dibiarkan karena memang sudah ada sebelumnya. Satu kontainer dan dua volume lain yang ada di Docker bukan milik uji ini dan tidak disentuh |

## 3. Hasil

| Langkah | Perintah | Hasil |
| --- | --- | --- |
| B0 | `update 20260826025451_AddTableBillingKasirPArt3` pada basis data kosong | `Done.`, exit `0`, 105 migration, 46 detik |
| B1 percobaan 1 | Sisip baris uji, `update 20260826090500_…` | **Gagal karena rancangan baris uji**, bukan migration: `23503` — keenam baris merujuk `EmergencyVisitId` fiktif, dan `ADD CONSTRAINT FK_TrxEmergencyDeparture_TrxEmergencyVisit_EmergencyVisitId` memvalidasi baris yang ada. Sidik jari sesudahnya **identik dengan B0** — Up berjalan dalam satu transaksi dan tidak meninggalkan objek setengah jadi. Dicatat apa adanya; diukur ulang sesudah induk ditambahkan |
| B1 | Tambah induk sah, `update 20260826090500_…` | `Done.`, exit `0`, 22 detik |
| B2 — Down | `update 20260826025451_…` | `Done.`, exit `0`, `Reverting migration '20260826090500_ImplementIgdFullPatientJourney'`, 23 detik |
| B3 — naik lagi | `update 20260826090500_…` | `Done.`, exit `0`, 21 detik |
| G percobaan 1 | Sisip pengkajian `QueueId` NULL, lalu Down | **Gagal karena rancangan baris uji**: `AssessmentNumber` bawaan kembar ditolak indeks unik, sehingga baris NULL tidak pernah tercipta dan Down yang menyusul hanyalah pengulangan B2 — hasilnya **identik dengan B2**, membuktikan Down berulang dengan hasil yang sama. Dicatat apa adanya |
| B4 | `update 20260826090500_…` | `Done.`, exit `0`; objek identik dengan B1 |
| G — Down dengan pengkajian IGD tanpa antrean | Sisip pengkajian `QueueId` NULL bernomor unik, lalu `update 20260826025451_…` | **Ditolak**: exit `1`, `23502: column "QueueId" of relation "TrxPatientAssessment" contains null values` (21 detik) |

| Pengukuran | B0 | B1 (Up) | B2 (Down) | B3 (Up lagi) | G sesudah penolakan |
| --- | --- | --- | --- | --- | --- |
| Tabel `public` | 524 | 527 | 525 | 527 | 527 |
| Riwayat migration | 105 | 106 | **105** | 106 | **106** |
| `TrxEmergencyTransfer` / `TrxEmergencyDeparture` | ada / tidak | tidak / ada | **ada / tidak** | tidak / ada | tidak / ada |
| `QueueId` pengkajian, konsultasi nullable | NO, NO | YES, YES | **NO, NO** | YES, YES | YES, YES |
| md5 seluruh kolom | `242f0cab…` | `e10121f3…` | `2a2a76c3…` | **`e10121f3…`** | sama dengan sebelum Down |
| md5 seluruh index | `eb681a09…` | `d646b70b…` | `7fbeaf47…` | **`d646b70b…`** | sama dengan sebelum Down |
| md5 seluruh constraint | `4e5af186…` | `c598c9c3…` | `c7dc3327…` | **`c598c9c3…`** | sama dengan sebelum Down |
| md5 kolom / index / constraint **di luar sasaran** | `463f23e3…` / `0c505908…` / `c017d3d8…` | **sama** | **sama** | **sama** | **sama** |
| md5 baris pengkajian | `a61d68ce…` | `a61d68ce…` | `a61d68ce…` | `a61d68ce…` | (2 baris) tidak berubah |

**Pemetaan status bolak-balik**, dibaca dari `baris-B1.txt` dan `baris-B2.txt`:

| `TransferStatus` semula | Sesudah Up (fisik / serah terima) | Peristiwa yang terbit | Sesudah Down |
| :---: | :---: | :---: | :---: |
| 1 | 1 / 1 | 1 | **1** |
| 2 | 1 / 3 | 1 | **2** |
| 3 | 2 / 1 | 2 | **3** |
| 4 | 3 / 3 | 3 | **4** |
| 5 | 1 / 4 | 1 | **5** |
| 6 | 9 / 9 | 9 | **6** |

Nomor, alasan, ringkasan, penolakan, dan penempatan keenam baris kembali sama persis (`DN-4`).

`hasil.json` — 19 pemeriksaan, seluruhnya lulus, setiap verdict dihitung dari nilai yang diukur:
`up` `PASS` (5/5), `down` `PASS` (7/7), `upLagi` `PASS` (4/4), `negatif` `PASS` (3/3);
`BE-IGD-026#1` `PASS`, `BE-IGD-031#2` `PASS`.

## 4. Yang tidak dipulihkan Down — terukur, bukan penghalang acceptance

`diff-objek-B0-vs-B2.txt`, dihitung `hasil.json` bagian `hilangDiDown`:

| Butir | Ukuran | Sifat |
| --- | --- | --- |
| Index lama `TrxEmergencyTransfer` | **12 tidak dipulihkan**, termasuk `UNIQUE` `IX_TrxEmergencyTransfer_TransferNumber` | Up membuangnya (`:75`–`:86`); Down tidak membuatnya kembali. Sesudah Down, nomor transfer kembar tidak lagi ditolak basis data |
| FK lama `TrxEmergencyTransfer` | **7 tidak dipulihkan** (empat ke `AspNetUsers`, dua ke `MstServiceUnit`, satu ke `TrxEmergencyVisit`) | Up membuangnya (`:43`–`:49`); Down hanya membuang FK baru (`:224`–`:226`) |
| `TransferStatus` | kini `DEFAULT 1` | Down menambah kolomnya dengan bawaan (`:234`); semula tanpa bawaan |
| `AcceptedAt`, `AcceptedByUserId` | 1 baris terisi sebelum Up → **0** sesudah Down | Up membuang kedua kolom tanpa arsip (`:117`–`:118`), berbeda dengan empat kolom penempatan yang diarsipkan lebih dulu. **Kehilangan permanen** bila kolomnya berisi |
| `TrxEmergencyDepartureLegacyPlacement` | tertinggal sesudah Down | **Disengaja** — komentar `:244`–`:246`: tabel arsip satu-satunya salinan riwayat penempatan |

Keempat butir pertama tidak menggugurkan acceptance kedua kartu. `BE-IGD-026` menuntut langkah mundur
yang diuji, dan bagian `QueueId`-nya terbalik sempurna. `BE-IGD-031` menuntut `RENAME` balik yang
diuji, dan nama tabel serta keempat kolomnya kembali dengan data utuh. Dampak nyatanya juga kecil:
`TrxEmergencyTransfer` berisi **0 baris** di dev saat Up diterapkan 27 Agustus 2026 (`IGD-UNK-03`),
sehingga tidak ada `AcceptedAt` yang hilang. Lagi pula, membatalkan migration ini di lingkungan nyata
berarti membatalkan ±250 migration modul lain lebih dulu. Dicatat sebagai `IGD-FACT-246`, bukan kartu baru.

## 5. Down ditolak begitu ada pengkajian IGD tanpa antrean

Uji `G` membuktikan Down **tidak dapat** dijalankan di lingkungan mana pun yang sudah menyimpan satu saja
pengkajian IGD tanpa antrean — dan itu justru tujuan `BE-IGD-026`. Penolakannya aman:

- Pernyataan yang gagal adalah `SET NOT NULL` di **akhir** Down (`:287`), sesudah seluruh pembalikan
  tabel kepergian. Sidik jari skema, baris, dan riwayat migration sesudah penolakan **identik** dengan
  sebelumnya — transaksi EF menggulung balik semuanya.
- Tidak ada pesan penjaga yang ramah seperti `BE-IGD-072`; yang muncul galat PostgreSQL apa adanya.

Konsekuensi operasionalnya: sebelum Down migration ini dapat dijalankan, pengkajian dan konsultasi
IGD tanpa antrean harus dipindahkan atau dihapus lebih dulu. Itu keputusan data klinis, bukan langkah
teknis. Dicatat sebagai `IGD-FACT-247`.

## 6. Putusan

| Task | Acceptance | Sebelum | Sesudah |
| --- | --- | :---: | :---: |
| `BE-IGD-026` | 1. Langkah mundur tertulis dan diuji di basis data terpisah | 🟡 belum diuji | ✅ `DN-1`…`DN-7`, `RU-1`…`RU-4`, `NG-1`…`NG-3` |
| `BE-IGD-026` | 3. Nol baris lama berubah nilainya | ✅ dipetakan 15 September | ✅ kini juga **terukur**: md5 baris pengkajian `a61d68ce…` sama di B0, B1, B2, B3 |
| `BE-IGD-031` | 2. Langkah mundur `RENAME` balik, diuji | 🟡 belum diuji | ✅ `DN-3`, `DN-4`, `RU-2`, `RU-3` |
| `BE-IGD-031` | 1. Nol baris data hilang | ✅ dipetakan 15 September | ✅ kini juga **terukur** untuk Up: 6 dari 6 baris pindah utuh (`UP-3`) |
