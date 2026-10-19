# Bukti — Uji langkah mundur migration `AddEmergencyAdmissionRequest` (`BE-IGD-072` acceptance 8)

| Field | Nilai |
| --- | --- |
| Tanggal | 18 Oktober 2026, sekitar 13.10–13.41 WIB |
| Task | `BE-IGD-072` acceptance 8 — _"Migration 1 diterapkan pemilik; `Down()` berpenjaga diuji atau dikecualikan pemilik"_; panduan uji sisa §7 R1-01 |
| Migration yang diuji | `20261012080102_AddEmergencyAdmissionRequest` — `Down()` diawali penjaga `RAISE EXCEPTION` bila `EmgAdmissionRequest` berisi data, lalu `DropTable` `EmgAdmissionRequestEvent`, `DropTable` `EmgAdmissionRequest`, `DropColumn` `EmgDispositionType.RequiresAdmissionRequest` |
| Wewenang | Rizki Gunawan, 18 Oktober 2026: _"mari kita lanjut ke tahap 2"_ (prompt A.6), lalu dua jawaban di chat — uji **kosong dan berisi data**, agent menyalakan Docker Desktop di laptop ini, dan **unduh `postgres:15` lalu hapus lagi di akhir** |
| Source | Backend `rizkiG` `6bb18fe6` + working tree `BE-IGD-083` (tidak menyentuh migration); DLL hasil build Rizki 18 Oktober 2026 10.51.30, dipakai dengan `--no-build` dan tidak dibuild ulang |
| Klasifikasi bukti | Uji migration pada **basis data sekali pakai di kontainer lokal**. Bukan UAT, bukan uji HTTP, bukan uji layar. Basis data dev **tidak disentuh** |

## 1. Apa yang diuji, dengan bahasa sederhana

Form Permintaan Rawat Inap disimpan di dua tabel baru, `EmgAdmissionRequest` dan `EmgAdmissionRequestEvent`. Bila migration
ini suatu hari harus dibatalkan, ada dua hal yang harus benar:

1. **Bila sudah ada form tersimpan, pembatalan harus ditolak.** Menghapus tabel berarti menghapus dokumen klinis pasien.
   Itulah tugas penjaga di awal `Down()`.
2. **Bila tabelnya masih kosong, pembatalan harus bersih.** Skema kembali persis seperti sebelum migration ini, dan bila
   migration dijalankan lagi, semuanya kembali identik.

_Contoh._ Pemilik membatalkan fitur form di sebuah lingkungan uji. Bila perawat sudah pernah membuat satu form saja, rollback
berhenti dengan pesan yang jelas dan tidak ada yang terhapus. Bila belum ada form, rollback berjalan dan tabel kunjungan,
tindak lanjut, dan modul lain tetap utuh.

## 2. Alat dan batas

| Butir | Isi |
| --- | --- |
| Basis data | Kontainer PostgreSQL **15.19** dari image `postgres:15` (diunduh atas izin pemilik; versi mayor sama dengan dev 15.15), port `127.0.0.1:55432`, dua basis data kosong: `quilvian_down072` (uji) dan `quilvian_ref041938` (pembanding) |
| Konfigurasi kontainer | `max_locks_per_transaction=4096`. Percobaan pertama dengan nilai bawaan (64) gagal `53200: out of shared memory` pada migration ke-200: EF Core 9 menerapkan seluruh rantai dalam **satu transaksi**, sehingga ratusan tabel baru menghabiskan tabel kunci. Itu batas konfigurasi kontainer, bukan cacat migration; kontainer dibuat ulang dan rantai diulang dari kosong |
| Cara menerapkan | `dotnet ef database update [target] --project QuilvianSystemBackend.csproj --no-build --connection <kontainer>` (dotnet-ef 10.0.12, runtime EF Core 9.0.18) |
| Pengaman dev | Selain `--connection`, variabel lingkungan `ConnectionStrings__DefaultConnection` proses `dotnet ef` diarahkan ke kontainer. `ApplicationDbContextFactory` membaca variabel lingkungan paling akhir, jadi basis data dev tidak pernah dialamatkan |
| Pengukuran | Skrip SQL baca-saja: jumlah tabel `public`; riwayat migration; keberadaan dua tabel form dan kolom `RequiresAdmissionRequest`; sidik jari `md5` **seluruh** kolom, index, dan constraint skema `public`; sidik jari definisi kedua tabel form; jumlah FK dari tabel lain ke tabel form |
| Baris uji | Satu baris disisipkan ke `EmgAdmissionRequest` (FK dimatikan hanya untuk sesi itu dengan `session_replication_role = replica`; kolom wajib diisi nilai dummy), lalu dihapus sebelum rollback kedua |
| Pembersihan | Kontainer dan volumenya dihapus (`docker rm -f -v`), image `postgres:15` dihapus (`docker rmi`), Docker Desktop dimatikan. Sesudahnya `docker system df` menunjukkan 0 image, 0 kontainer, 0 volume — sama dengan keadaan awal |

## 3. Hasil

| Langkah | Perintah | Hasil |
| --- | --- | --- |
| B0 — rantai penuh | `update` ke head pada basis data kosong | `Done.`, exit `0`, **332** migration dalam 191 detik |
| G1 — rollback dengan 1 form | Sisip 1 baris, lalu `update 20261012041938_AddAdmissionTransferOrigin` | **Ditolak**: exit `1`, `P0001: Down BE-IGD-072 dihentikan: EmgAdmissionRequest sudah berisi Form Permintaan Rawat Inap. Menghapus tabel ini menghilangkan dokumen klinis pasien.` (106 detik) |
| B1 — rollback tabel kosong | Hapus baris uji, lalu `update 20261012041938_AddAdmissionTransferOrigin` | `Done.`, exit `0`, **23** migration dibatalkan (91 detik) |
| R — pembanding | `update 20261012041938_AddAdmissionTransferOrigin` pada basis data kosong kedua | `Done.`, exit `0`, 309 migration (126 detik) |
| B2 — naik kembali | `update` ke head | `Done.`, exit `0`, 23 migration (34 detik) |

| Pengukuran | B0 | G1 (sesudah penolakan) | B1 | R (pembanding) | B2 |
| --- | --- | --- | --- | --- | --- |
| Tabel `public` | 832 | 832 | **819** | 819 | 832 |
| Riwayat migration | 332 | 332 | **309** | 309 | 332 |
| Migration terakhir | `20261016110000_AddRadOrderPhotoNumber` | sama dengan B0 | `20261012041938_AddAdmissionTransferOrigin` | sama dengan B1 | sama dengan B0 |
| `EmgAdmissionRequest` / `…Event` ada | ya / ya | ya / ya (1 baris tetap ada) | **tidak / tidak** | tidak / tidak | ya / ya |
| Kolom `EmgDispositionType.RequiresAdmissionRequest` | ada | ada | **tidak ada** | tidak ada | ada |
| Sidik jari seluruh kolom | `da9a6b80…` | **sama dengan B0** | `fbfb03c3…` | **sama dengan B1** | **sama dengan B0** |
| Sidik jari seluruh index | `adaf9826…` | sama dengan B0 | `ac806289…` | sama dengan B1 | sama dengan B0 |
| Sidik jari seluruh constraint | `fec721a8…` | sama dengan B0 | `fbe01fe6…` | sama dengan B1 | sama dengan B0 |
| Sidik jari definisi dua tabel form | `157452d8…` | sama dengan B0 | (tabel tidak ada) | (tabel tidak ada) | **`157452d8…` — identik** |
| FK dari tabel lain ke tabel form | 0 | 0 | — | — | 0 |

Perbandingan berkas hasil B1 dengan R dan B0 dengan B2 memakai `diff`: keduanya **identik**.

Yang dibuktikan:

1. **Penjaga menahan rollback bila ada form.** Dengan satu form tersimpan, rollback berhenti di `Down()` migration ini dengan
   pesan penjaga persis. Karena EF Core 9 menjalankan seluruh rangkaian rollback dalam satu transaksi, 22 migration modul lain
   yang sudah dibatalkan lebih dulu ikut dikembalikan: skema G1 identik dengan B0 dan form uji tidak hilang. Tidak ada
   keadaan setengah jalan.
2. **Rollback tabel kosong bersih dan tepat.** 23 migration (22 milik modul lain sesudah 12 Oktober, ditambah migration ini)
   dibatalkan tanpa galat. Skema hasilnya identik dengan basis data yang sejak awal hanya dinaikkan sampai `20261012041938`,
   untuk seluruh kolom, index, dan constraint di 819 tabel.
3. **Naik kembali identik.** Kedua tabel form kembali dengan definisi yang sama persis (kolom, tipe, nullability, default,
   index unik bersaring, check constraint, FK), dan skema seluruh 832 tabel sama dengan sebelum uji.
4. Tidak ada FK dari tabel lain ke tabel form, jadi `DropTable` tidak dapat menjatuhkan objek modul lain.

## 4. Yang **tidak** dicakup

| Hal | Keterangan |
| --- | --- |
| Isi kolom `RequiresAdmissionRequest` | Kolom ditambahkan dengan bawaan `false`; `Up()` tidak mengubah data. Penandaan jenis tindak lanjut rawat inap dilakukan lewat master data per lingkungan (syarat rilis R3.18.5). Basis data uji kosong, jadi yang diuji hanya skema |
| Baris `EmgAdmissionRequestEvent` saat rollback | Penjaga memeriksa `EmgAdmissionRequest` saja. Kejadian selalu milik sebuah form, jadi kejadian tanpa form tidak terjadi lewat aplikasi |
| Versi mesin | Uji memakai 15.19; dev 15.15. Versi mayor sama |
| Batas kunci dev | Penerapan seluruh rantai ke basis data kosong memerlukan `max_locks_per_transaction` lebih dari 64 di EF Core 9. Basis data dev yang naik bertahap tidak terdampak, tetapi lingkungan baru yang dibangun dari kosong perlu menaikkannya — dicatat sebagai catatan rilis |

## 5. Dampak status

`BE-IGD-072` acceptance 8 **terpenuhi**: migration diterapkan pemilik (12 Oktober 2026), dan `Down()` berpenjaga diuji pada
kedua keadaan. Bersama acceptance 1–7, 9, dan 10 yang sudah terbukti, seluruh **10 dari 10** acceptance terpenuhi.
**UAT belum dan tidak diklaim.**
