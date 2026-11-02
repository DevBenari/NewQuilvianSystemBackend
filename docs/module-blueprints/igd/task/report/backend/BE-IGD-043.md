# Laporan Susulan Backend — `BE-IGD-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-043` |
| Judul | Laporan susulan pengaturan IGD tersirat (`f76ebaab`) |
| Slice | `IGD-S08` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.8 |
| Requirement | — (laporan atas perilaku pendaftaran dan disposisi yang sudah berjalan) |
| Keputusan | `IGD-DEC-112`; bukti `IGD-EV-118` |
| Contract version | **Nol perubahan** |
| Dependency | — |
| Klasifikasi | `REPORT` — **nol perubahan source** |
| Target tulis | Laporan ini; baris status roadmap; traceability |
| Source yang dibaca | `rizkiG` `ed5cb292` (sesudah merge integration `56a19904`) |
| Tanggal | 26 Oktober 2026 |
| Status | ✅ **SELESAI — acceptance 1–5 terpenuhi; `git diff` source kosong** |

---

## 1. Apa yang dilaporkan

Commit `f76ebaab` (28 Agustus 2026) memperkenalkan **pengaturan IGD tersirat** tanpa laporan
tracked. Perilakunya disahkan belakangan lewat `IGD-DEC-112` pada 15 September 2026. Laporan ini
memetakan setiap aturan keputusan itu ke baris source yang berlaku hari ini, dan mencatat akibat
sampingnya pada disposisi.

## 2. Acceptance 1 — setiap aturan `IGD-DEC-112` dipetakan ke baris source

Seluruhnya berada di `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs`,
metode statis `ResolveActiveSettingAsync` (`:133`).

| # | Aturan `IGD-DEC-112` | Baris source | Isi |
| ---: | --- | --- | --- |
| 1 | Satu baris `EmgSetting` sungguhan **selalu menang** | `:137`–`:145` | Kueri `IsActive && !IsDelete`, diurutkan `IsDefault` lalu `CreateDateTime` menurun; `if (tersimpan != null) return tersimpan;` |
| 2 | Bila tabel kosong **dan** master Unit Pelayanan memuat **tepat satu** unit aktif bertipe `Emergency`, unit itu dipakai | `:149`–`:157` | Kueri `ServiceUnitType == ServiceUnitType.Emergency` dengan `.Take(2)` |
| 3 | **Nol atau lebih dari satu** unit → pendaftaran tetap ditolak | `:159`–`:160` | `if (unitGawatDarurat.Count != 1) return null;` |
| 4 | Pengaturan sementara **tidak disimpan**, `Id = Guid.Empty`, `Code = "IMPLICIT"` | `:162`–`:170` | Objek `EmgSetting` dibentuk di memori, nol `Add`, nol `SaveChangesAsync` |

`.Take(2)` pada aturan 2 adalah pilihan yang disengaja dan dijelaskan komentar di `:146`–`:147`:
dua baris sudah cukup membedakan *tepat satu* dari *lebih dari satu*, sehingga tabel unit tidak
perlu dihitung seluruhnya.

### Pesan penolakannya

Satu konstanta dipakai bersama supaya penolakan berbunyi sama dari endpoint mana pun —
`EmergencyVisitService.PesanPengaturanTidakTersedia` (`:58`):

> *"Pengaturan IGD aktif belum tersedia dan unit IGD tidak dapat disimpulkan sendiri. Pastikan ada
> tepat satu unit pelayanan aktif bertipe gawat darurat pada master Unit Pelayanan, atau isi master
> Pengaturan IGD lebih dulu."*

Pemakainya: `EmergencyVisitController.cs:985` dan `EmergencyVisitService.cs:189`.

## 3. Acceptance 2 — akibat samping pada disposisi, beserta contohnya

Rantainya tiga langkah, dan ketiganya terbaca di source:

| Langkah | Baris source | Isi |
| --- | --- | --- |
| 1 | `EmgSetting.cs:40` | `public bool RequireRegistrationCompletionBeforeDisposition { get; set; } = true;` — **nilai bawaan `true` ada di entity-nya** |
| 2 | `EmergencyVisitService.cs:162`–`:170` | Objek `IMPLICIT` dibentuk lewat object initializer yang **tidak menyebut** properti itu, sehingga ia mewarisi bawaan `true` |
| 3 | `EmergencyDispositionService.cs:99`–`:108` | Saat `DispositionStatus == Executed`, pengaturan di-*resolve*; bila `RequireRegistrationCompletionBeforeDisposition == true` dan `visit.RegistrationStatus != Completed` → *"Registrasi IGD harus diselesaikan sebelum disposition dieksekusi."* |

**Contoh yang diminta kartu.** Tabel `EmgSetting` kosong, master Unit Pelayanan memuat tepat satu
unit IGD aktif. Pasien terdaftar berstatus registrasi `Provisional`. Perawat menjalankan tindak
lanjut (`DispositionStatus = Executed`):

- **Sebelum `f76ebaab`:** `ResolveActiveSettingAsync` menjawab `null`, sehingga `setting?.Require…`
  bernilai `null` dan perbandingan `== true` gagal — penjagaan **mati diam-diam** dan disposisi
  lolos.
- **Sesudah `f76ebaab`:** pengaturan `IMPLICIT` terbentuk dan membawa `true`, sehingga disposisi
  yang sama kini **ditolak**.

Jadi commit itu tidak hanya menambah kemudahan pendaftaran; ia **menghidupkan** satu penjagaan yang
selama ini tidak pernah berjalan pada lingkungan bertabel pengaturan kosong. Inilah yang
`IGD-DEC-112` sebut sebagai *"akibat samping yang ikut disahkan"*.

## 4. Acceptance 3 — penjaga seeder level triase, dan seeder tanpa pemanggil

`Areas/HealthServices/EmergencyInstallationManagement/MasterData/Seeders/EmergencyMasterDataSeeder.cs`.

| Butir | Keadaan |
| --- | --- |
| Penjaga kepemilikan | `IsOwnedByAnotherSource` (`:52`–`:58`): bila tabel memuat **satu saja** kode yang tidak dikenal daftar seeder, seeder **tidak menambah apa pun** |
| Penerapannya pada level triase | `SeedTriageLevelsAsync` (`:70`), pemeriksaan di `:112`–`:121`; bila terpicu, ia mengisi `result.TriageLevelSkippedReason` dan `return` |
| Alasan penjaga itu paling keras di sini | Komentar `:105`–`:111`: rumah sakit berskala warna (MERAH/KUNING/HIJAU/HITAM) punya kode yang seluruhnya asing, sehingga pemeriksaan per `Code` menganggap `L1`…`L5` belum ada. Slot yang bentrok memang dilewati pemeriksaan `(TriageSystem, Level)`, tetapi slot yang kosong tetap terisi — hasilnya **satu tabel memuat dua skala triase sekaligus** |
| Bahaya yang dicegah | Komentar `:95`–`:100`: tanpa penjaga ini, penyisipan ulang `L1`…`L5` menabrak index `(TriageSystem, Level)`, `SaveChanges` gagal, dan karena pemanggilnya tidak menangkap exception **aplikasi berhenti sebelum sempat melayani permintaan** |

**Fakta yang kartu minta dicatat tegas: seeder ini masih tanpa pemanggil.** Penelusuran
`EmergencyMasterDataSeeder` pada seluruh repository menjawab **nol hasil** di luar definisinya
sendiri. Artinya keenam `SeedXxxAsync` — level triase, indikator, cara kedatangan, jenis kasus,
jenis tindak lanjut, dan pengaturan bawaan — **tidak pernah dijalankan** oleh siapa pun. Penjaga
yang dirancang hati-hati itu hari ini melindungi kode yang tidak aktif.

Konsekuensi langsungnya: master Pengaturan IGD tetap kosong di lingkungan mana pun yang tidak
diisi manual, dan justru karena itulah jalur `IMPLICIT` pada bagian 2 menjadi jalur yang
sesungguhnya dipakai, bukan jalur cadangan.

## 5. Acceptance 4 — uji API

**`NOT FEASIBLE`**, dengan alasan yang dapat diperiksa:

| Alasan | Rincian |
| --- | --- |
| Menuntut tabel `EmgSetting` kosong | Jalur `IMPLICIT` hanya aktif ketika nol baris aktif. Mengosongkannya di dev berarti menghapus master yang dipakai putaran uji lain yang sedang berjalan |
| Menuntut jumlah unit IGD diatur | Aturan 2 dan 3 menuntut **tepat satu** dan **lebih dari satu** unit aktif bertipe `Emergency` secara bergantian — perubahan master bersama |
| Agent dilarang menjalankan kueri basis data | Memverifikasi jumlah baris sebelum dan sesudah menuntut kueri, yang bukan wewenang agent |

Pemetaan source pada bagian 2 sampai 4 **tidak** bergantung pada uji API: ketiganya pernyataan
tentang jalur kode, dan jalur kode itu dapat dibaca langsung. Bila pemilik ingin pembuktian
runtime, bentuk tersingkatnya satu putaran pada lingkungan terpisah — bukan dev — dengan urutan:
tabel pengaturan dikosongkan → daftarkan pasien → jalankan tindak lanjut pada registrasi
`Provisional` → harapkan penolakan bagian 3.

## 6. Acceptance 5 — nol perubahan source

| Pemeriksaan | Hasil |
| --- | --- |
| `git diff` pada `Areas/` dan `Repositories/` | **kosong** |
| Berkas yang ditulis task ini | Laporan ini, baris status roadmap, traceability — seluruhnya dokumen |
| Komentar baru pada source | **nol** |

---

## 7. Yang ditemukan dan sengaja tidak dikerjakan

| Temuan | Sifat | Tindakan |
| --- | --- | --- |
| `EmergencyMasterDataSeeder` nol pemanggil | Bukan cacat task ini; seeder memang belum pernah dipasang | **Dicatat saja.** Memasangnya menuntut keputusan tentang kapan dan di lingkungan mana seeder boleh berjalan — bukan wewenang laporan susulan |
| Master Pengaturan IGD kosong di dev | Konsekuensi butir di atas | Dicatat; jalur `IMPLICIT` menutupinya untuk pendaftaran, tetapi membuat `RequireRegistrationCompletionBeforeDisposition` selalu `true` tanpa ada yang pernah memilihnya |
