# Laporan Perubahan Backend — `BE-RWI-087`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-087` |
| Judul | Penutupan membatalkan dosis obat berjadwal |
| Slice | Gelombang 4 — `RI-V2-3`, `EPIC RI-41`, langkah penutupan 6. Dirilis bersama `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-087` |
| Trace | `FR-RI-200`; `INT-INP-10`, `INT-KEP-15`; `INV-INP-11`; `NFR-025`; `02-backend-architecture.md` 11.5.4 langkah 6, 11.8; `data/data-dictionary.md` 18.5 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-084` — **sebagian** (tidak menahan); `BE-RWI-114` [BE-KEP] — **belum mendarat, dan inilah yang menahan** |
| Klasifikasi | `MEDIUM` sebagaimana direncanakan; tidak dikerjakan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — tidak ada berkas yang ditulis pada task ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **⛔ TERBLOKIR `BE-RWI-114`** — tabel MAR `PharmacyManagement` belum ada sama sekali di repository. **Nol berkas source diubah** |

---

## 1. Masalah yang diperbaiki

Belum ada yang diperbaiki. Laporan ini mencatat **kenapa** dan **berdasarkan bukti apa**.

**Masalah yang seharusnya ditutup task ini.** MAR — catatan pemberian obat — membentuk dosis `Due`
ke depan berdasarkan jadwal resep. Bila pasien pulang pukul 14.00, dosis pukul 20.00 tetap ada di
daftar perawat. Dosis itu dapat tercatat diberikan kepada pasien yang sudah tidak ada di ruangan.

**Contoh yang dimaksud.** Joko pulang 16 September 2026 pukul 14.00.

| Dosis | Status sebelum penutupan | Seharusnya sesudah penutupan | Kenapa |
| --- | --- | --- | --- |
| 08.00 | `Administered` | Tetap `Administered` | Rekam medis pemberian obat; tidak pernah disentuh |
| 12.00 | `Missed` | Tetap `Missed` | Fakta yang sudah terjadi |
| 20.00 | `Due` | Menjadi `Cancelled` beralasan tetap | Dijadwalkan setelah waktu tutup; tidak akan pernah diberikan |

---

## 2. Proses bisnis

Proses bisnisnya tetap sebagaimana dirancang, dan dicatat di sini supaya pelaksana berikutnya tidak
perlu menelusurinya ulang.

**Tujuan.** Tidak ada dosis obat yang menunggu diberikan kepada pasien yang sudah pulang.

**Pelaku.** Petugas yang menutup episode. Ia tidak perlu tahu ada dosis berjadwal atau tidak.

**Pemicu.** Episode ditutup.

**Langkah yang seharusnya dipasang — langkah 6 penutupan.**

1. Di dalam transaksi penutupan yang sama, panggil
   `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync(episode.Id, now)`.
2. Dosis berstatus `Due` yang jadwalnya **setelah** waktu tutup menjadi `Cancelled` beralasan tetap
   "perawatan ditutup".
3. Dosis `Administered`, `Held`, `Refused`, dan `Missed` **tidak disentuh sama sekali**.
4. Jumlah dosis yang dibatalkan dikembalikan pada `sideEffects.cancelledFutureDoseCount`.

**Kenapa hanya dosis setelah waktu tutup.** Dosis yang jadwalnya sudah lewat tetapi belum dicatat
adalah **pencatatan yang tertinggal**, bukan dosis yang tidak akan diberikan. Membatalkannya
menghapus jejak bahwa pemberian obat pernah terlewat — dan itu justru informasi yang paling perlu
terlihat. Keadaan itu ditangani sebagai **peringatan** `UnrecordedPastDoses` pada `BE-RWI-084`, bukan
sebagai pembatalan.

**Kenapa dosis `Administered` tidak boleh tersentuh.** Ia adalah rekam medis pemberian obat. Obatnya
benar-benar masuk ke tubuh pasien, dan catatan itu tidak dapat dibatalkan oleh peristiwa administrasi
mana pun.

**Kenapa harus di dalam transaksi penutupan.** `INV-INP-11` dan `NFR-025`: satu galat pada langkah
mana pun membuat **nol** perubahan tersimpan, dan episode tetap `DischargePending`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Hasil pemeriksaan |
| --- | --- |
| `.../02-backend-architecture.md` 11.5.4 langkah 6 | Langkah 6 memanggil `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync` |
| `.../02-backend-architecture.md` 11.8 | **"Selama tabel dosis belum ada, langkah 6 tidak dipasang."** Kalimat itu tertulis apa adanya pada bagian urutan terhadap sub-modul lain |
| `data/data-dictionary.md` 18.5 | `PhmMedicationAdministration` milik `PharmacyManagement`, ditulis lewat `MedicationAdministrationService` yang dirancang sub-modul `keperawatan` data 11.12 |
| Pencarian berkas `*MedicationAdministration*` di seluruh `Areas/**` | **Nol hasil** |
| `keperawatan/roadmap/backend-roadmap-v2.md` kartu `BE-RWI-114` | Tabel MAR, revisinya, jadwalnya, pengaturannya, dan hosted service pembentukan dosis seluruhnya dibuat di sana — berstatus `MISSING / NEW`, migration `K4` |
| `keperawatan/task/report/backend/` | Laporan terakhir `BE-RWI-078`; **tidak ada laporan `BE-RWI-114`** |

### 3.2 Berkas yang berubah

**Nol berkas.** Tidak ada satu pun source yang diubah pada task ini.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada perubahan. Field `sideEffects.cancelledFutureDoseCount` sudah ada bentuknya (dipasang `BE-RWI-084`) dan bernilai `0` beserta keterangan pada `notYetWiredSteps` |
| Database | `NOT APPLICABLE` — tidak ada perubahan schema, tidak ada migration |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint. Langkah 6 adalah perilaku di dalam
transaksi penutupan, bukan endpoint tersendiri.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | **`0 Error(s)`, `211 Warning(s)`, `Time Elapsed 00:04:31.02`** | `PASS` | Dijalankan 16 September 2026 untuk rangkaian task ini secara keseluruhan. **Task ini sendiri tidak menyumbang satu berkas pun** |
| Verifikasi proses bisnis `UAT-50` | Tidak dijalankan | `NOT RUN` | Tidak ada perilaku baru untuk diuji |
| Uji galat buatan | Tidak dijalankan | `NOT RUN` | Tidak ada langkah baru di dalam transaksi |
| Pencarian keberadaan tabel dan service MAR | `MedicationAdministrationService` dan `PhmMedicationAdministration` **tidak ada** di repository ini | `PASS` sebagai temuan | Pencarian nama berkas dan nama kelas pada seluruh `Areas/**` — nol hasil |
| Pemeriksaan wewenang | Tabel MAR milik `PharmacyManagement` dan dirancang sub-modul `keperawatan`. Membuatnya dari task ini adalah pekerjaan `BE-RWI-114`, yang tidak diberi wewenang pada task aktif | `PASS` | `data-dictionary.md` 18.5; `keperawatan/roadmap/backend-roadmap-v2.md` kartu `BE-RWI-114` |
| Pemeriksaan instruksi arsitektur | `02-backend-architecture.md` 11.8 menuliskan secara eksplisit bahwa langkah 6 **tidak dipasang** selama tabel dosis belum ada | `PASS` | Bagian "Urutan terhadap sub-modul lain" pada 11.8 |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — tidak ada perubahan perilaku.

**Tidak dijalankan:** seluruh butir verifikasi kartu task, karena tidak ada perubahan source untuk
diverifikasi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Dosis `Due` berjadwal **setelah** waktu tutup menjadi `Cancelled` beralasan tetap | **Belum terpenuhi — terblokir** | Tabel dosis belum ada |
| AC-2 — Dosis `Administered`, `Held`, `Refused`, dan `Missed` tidak disentuh sama sekali | **Belum terpenuhi — terblokir** | Sama |
| AC-3 — Pembatalan terjadi di dalam transaksi penutupan yang sama — `INT-KEP-15` | **Belum terpenuhi — terblokir** | Titik pemasangannya sudah ditandai di dalam `CloseEpisodeInternalAsync` beserta alasannya, tetapi belum ada yang dipanggil |
| AC-4 — Galat buatan → nol perubahan tersimpan | **Belum terpenuhi — terblokir** | Belum ada perubahan yang perlu dibatalkan |

**Nama blocker.** `BE-RWI-114` pada [`keperawatan/roadmap/backend-roadmap-v2.md`](../../../../keperawatan/roadmap/backend-roadmap-v2.md)
— "MAR dan pembentukan dosis (migration `K4`)". Sampai task itu mendarat, **tidak ada tabel dosis
yang dapat dibatalkan.**

**Kenapa tidak dikerjakan sebagian.** Tiga hal menutup pintunya sekaligus, dan ketiganya berdiri
sendiri:

| No | Alasan |
| ---: | --- |
| 1 | **Tabelnya tidak ada.** Bukan "servicenya belum ada tetapi tabelnya sudah" — `PhmMedicationAdministration` sama sekali belum lahir. Tidak ada satu baris pun yang dapat dibaca maupun ditulis |
| 2 | **Bukan wewenang task ini.** Tabel MAR milik `PharmacyManagement` dan dirancang sub-modul `keperawatan`. Membuatnya dari sini berarti mengerjakan `BE-RWI-114` tanpa wewenang, lengkap dengan hosted service pembentukan dosis dan penjagaan idempotennya |
| 3 | **Arsitekturnya memerintahkan menunggu.** `02-backend-architecture.md` 11.8: "Selama tabel dosis belum ada, langkah 6 tidak dipasang." Ini bukan tafsiran; kalimatnya tertulis apa adanya |

Kartu roadmap `BE-RWI-087` sendiri menuliskannya pada Definition of Done: **"Task ini tidak boleh
dimulai sebelum `BE-RWI-114` [BE-KEP] mendarat, karena tabel MAR-nya belum ada."**

**Yang sudah disiapkan untuk pelaksana berikutnya.**

| Hal | Keadaan |
| --- | --- |
| Titik pemasangan langkah 6 | Sudah ditandai di dalam `CloseEpisodeInternalAsync` beserta alasan dan rujukannya |
| Field `sideEffects.cancelledFutureDoseCount` | Sudah ada bentuknya, bernilai `0` |
| Keterangan "belum terpasang" | Konstanta `LangkahEnamBelumTerpasang`, ikut pada `sideEffects.notYetWiredSteps` dan pada peringatan `UnrecordedPastDoses` |
| Peringatan dosis pada kesiapan penutupan | Sudah ada bentuknya, ditandai `isMeasured = false` |

Pemasangannya kelak adalah **satu pemanggilan** pada titik yang sudah ditandai, ditambah menghapus
dua keterangan "belum terpasang" — bukan penelusuran ulang.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Task tidak boleh dimulai sebelum `BE-RWI-114` mendarat | **Dipatuhi** — task tidak dimulai |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi, ditandai `⛔` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` — task ini tidak mengubah satu berkas pun |
| Masalah yang diketahui | `NONE` pada task ini |
| Risiko tersisa | **Nyata dan perlu diketahui pemilik.** Sampai langkah 6 dipasang, dosis obat berjadwal setelah waktu tutup **tetap muncul di daftar perawat** untuk pasien yang sudah pulang. Risiko keselamatan pasiennya — obat tercatat diberikan kepada orang yang tidak ada di ruangan — **belum tertutup**. Ia hanya tertutup ketika `BE-RWI-114` mendarat dan langkah 6 dipasang. Perlu diketahui bahwa tabel MAR-nya sendiri juga belum ada, sehingga dosis berjadwal saat ini belum dibentuk sistem sama sekali; risiko ini **menjadi aktif** begitu MAR berjalan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-086`. **Tidak satu berkas pun berubah karena task ini.** Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada operasi Git yang dilakukan |
| Langkah berikutnya | Kerjakan `BE-RWI-114` pada roadmap `keperawatan`. Setelah tabel MAR dan `MedicationAdministrationService` ada, pasang pemanggilannya pada titik yang sudah ditandai di `CloseEpisodeInternalAsync`, isi `cancelledFutureDoseCount` dari nilai kembaliannya, hapus `LangkahEnamBelumTerpasang` dari `notYetWiredSteps` dan dari peringatan `UnrecordedPastDoses`, lalu perbarui laporan ini beserta `BE-RWI-084` |
