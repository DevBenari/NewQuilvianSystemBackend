# PLAN-REPAIR-009 — Langkah Pilih Bed kembali dapat dilanjutkan, pembacaan bed lebih ringan, dan galat server terbaca

```yaml
plan_id: PLAN-REPAIR-EPS-009
issue: ../issue/issue-009-pilih-bed-buntu-dan-galat-server.md
status_rencana: DISETUJUI_SEBAGIAN  # FIX-01 s.d. 06 disetujui dan selesai; FIX-07 menunggu K-02
tanggal_rencana: "2026-10-13"
diputuskan_oleh: "Pemilik sistem (Muhammad Hamzah) — lewat laporan yang sama: 'tolong bantu saya mengatasi masalah', 'coba cek… agar query tidak berat'"
tanggal_keputusan: "2026-10-13"
basis_source_backend: "86cb8ef8d3660e97776096fb33fcaf7dec7c9006 (MHamzah)"
basis_source_frontend: "c2f42e4ce3af5e4d07ec2c963bcec3e97a4031aa (HamzahV2)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-009-01` | `ISS-EPS-009-01` | Unit admisi mengikuti bed terpilih; unit bawaan tidak hilang saat pasien dipilih | FE | 1 | `FE-RWI-243` | ✅ SELESAI | [`FE-RWI-243.md`](../../task/report/frontend/FE-RWI-243.md) — unit 6/6 PASS; ESLint, build, uji layar `NOT RUN` (`node_modules` tidak ada) |
| `FIX-EPS-009-02` | `ISS-EPS-009-02`, `ISS-EPS-009-T4` | Alasan asli kegagalan titik tulis tampil di langkah Pilih Bed; kode galat dibaca dari `errors.code` | FE | 1 | `FE-RWI-243` | ✅ SELESAI | [`FE-RWI-243.md`](../../task/report/frontend/FE-RWI-243.md) |
| `FIX-EPS-009-03` | `ISS-EPS-009-03`, `ISS-EPS-009-T1` | Kurangi bolak-balik database pada `available-beds`, `bed-board`, dan monitoring; pesan 409 tanpa nama exception | BE | 2 | `BE-RWI-228` | ✅ SELESAI | [`BE-RWI-228.md`](../../task/report/backend/BE-RWI-228.md) — build 0 error, QBE strict 7 berkas PASS; hitung SQL runtime `NOT RUN` |
| `FIX-EPS-009-04` | `ISS-EPS-009-04` | `bed-board` menerapkan penyaring `roomId`, `patientClassId`, `search` | BE | 2 | `BE-RWI-228` | ✅ SELESAI | [`BE-RWI-228.md`](../../task/report/backend/BE-RWI-228.md) |
| `FIX-EPS-009-05` | `ISS-EPS-009-05` | Kunci baris episode saat menggugurkan Draft, lalu periksa ulang | BE | 1 | `BE-RWI-229` | ✅ SELESAI | [`BE-RWI-229.md`](../../task/report/backend/BE-RWI-229.md) — build 0 error, QBE strict PASS; uji PostgreSQL `NOT RUN` |
| `FIX-EPS-009-06` | `ISS-EPS-009-01`, `ISS-EPS-009-04` | Amandemen dokumen: kontrak `bed-board`, skema 3.8–3.9, matriks validasi `VAL-ADM-03` | Dokumen | 2 | — (perbaikan dokumen) | ✅ SELESAI | Bagian "Amandemen PLAN-REPAIR-EPS-009" pada [`api-contract.md`](../../contracts/api-contract.md), [`validation-matrix.md`](../../contracts/validation-matrix.md), [`05-skema-tampilan.md`](../../05-skema-tampilan.md) |
| `FIX-EPS-009-07` | `ISS-EPS-009-T3` | Pembatalan episode berpenempatan memindahkan bed ke siklus pembersihan | BE | 3 | — | BELUM DIKERJAKAN | Di luar permintaan pemilik; menunggu persetujuan tersendiri |

**Ringkasan: 6 dari 7 perbaikan selesai pada tingkat source, build, dan dokumen. `FIX-EPS-009-07` menunggu persetujuan tersendiri. Uji runtime oleh pemilik (bagian 8) belum dijalankan.**

`ISS-EPS-009-06` dan `ISS-EPS-009-07` tidak punya perbaikan baru: yang pertama sudah ditangani
`6b8a8b20`, yang kedua sudah teratasi dengan `dotnet ef database update`. `ISS-EPS-009-T2` adalah
catatan konfigurasi rilis (bagian 8).

---

## 2. Solusi terpilih per temuan

### ISS-EPS-009-01 — Unit layanan admisi kosong setelah pasien dipilih

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Unit admisi diambil dari bed terpilih (`bed.serviceUnitId`); unit bawaan hanya menyemai penyaring papan dan dipulihkan setelah reset | Sama dengan alur transfer (`inpatient-admission-transfer-utils.js:29-32`) dan kontrak `RWI-KELAS-006-1.0` ("server membaca kelas/unit dari kamar bed"); bed di unit lain tetap dapat dipakai; tidak ada isian tersembunyi yang bisa basi | Bila papan menampilkan bed dari unit bukan rawat inap, penolakannya datang dari server (bukan dari layar) |
| B | Hanya pulihkan unit bawaan setelah reset, perbandingan unit tetap | Perubahan paling kecil | Bed di unit lain tetap tampil dapat dipilih lalu ditolak; unit tersembunyi tidak dapat diperbaiki petugas |
| C | Kunci penyaring unit papan ke unit bawaan | Tidak ada bed unit lain yang dapat dipilih | Membatasi admisi langsung ke unit rawat inap lain (ICU, Perinatologi) — kebijakan baru yang belum diputuskan |

**Solusi terpilih: Opsi A.**

1. Satu aturan untuk dua jalur admisi (biasa dan transfer): unit dan kelas mengikuti bed.
2. Server tetap penjaga terakhir: `OpenAdmissionAsync` menolak bed yang unitnya tidak cocok
   (`InpEpisodeService.cs:197-209`) dan unit yang bukan rawat inap
   (`ValidateServiceUnitAndPatientClassAsync`, `:1266-1270`); kunjungan rawat inap juga menolak unit
   bukan rawat inap (`BE-RWI-204`).
3. Tidak mengubah `RWI-DEC-268`: petugas tetap tidak memilih unit secara manual.

**Kenapa bukan opsi lain.** Opsi B membiarkan jebakan "bed tampil bisa dipilih lalu ditolak".
Opsi C membuat keputusan bisnis baru tentang unit mana yang boleh menerima admisi.

### ISS-EPS-009-02 — Alasan asli ditimpa kalimat umum

Hanya ada satu cara yang wajar: fungsi titik tulis (`doctor.submit`) mengembalikan `message`
bersama `success: false`, dan langkah Pilih Bed menampilkannya. Kalimat umum hanya dipakai bila
tidak ada alasan sama sekali. Isi kalimat server tidak diubah layar.

### ISS-EPS-009-03 — Pembacaan bed boros

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Kurangi bolak-balik tanpa mengubah jawaban: pakai ulang data proyeksi, cek identitas di memori, hak aksi pembersihan dimuat hanya bila ada bed dalam keadaan itu, daftar unit SuperAdmin disimpan sekali per permintaan, kamar diambil dari `Include` | Isi jawaban identik; tanpa migration; tanpa mengubah kontrak | Tetap dua permintaan per perubahan penyaring |
| B | Gabungkan `bed-board` dan `available-beds` menjadi satu endpoint | Satu permintaan | Mengubah kontrak dan dua layar sekaligus; risiko lebih besar |
| C | Cache hasil papan beberapa detik | Paling cepat | Papan bisa menampilkan bed yang baru saja dipesan petugas lain — melanggar 3A.6 (server penentu keadaan) |

**Solusi terpilih: Opsi A.** Menyasar sumber waktu yang terbukti (jumlah bolak-balik), radius
dampak hanya tiga service bed, dan jawabannya tetap sama byte demi byte untuk data yang sama.

**Kenapa bukan opsi lain.** B adalah perubahan kontrak yang tidak dibutuhkan untuk memenuhi
keluhan. C menukar kebenaran data dengan kecepatan pada layar yang justru berisiko bentrok.

### ISS-EPS-009-04 — Papan bed mengabaikan penyaring

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | `bed-board` menerima `roomId`, `patientClassId`, `search` opsional, diteruskan ke `MonitoringAsync` (tambah `PatientClassId`) | Aditif, pemanggil tanpa penyaring tidak berubah; ringkasan dan kartu konsisten dengan daftar yang dapat dipilih; papan memproyeksikan lebih sedikit bed | Kontrak API perlu diamandemen |
| B | Layar menandai sendiri bed di luar penyaring | Tanpa perubahan backend | Layar mulai menyaring ulang data server — bertentangan dengan prinsip "kelayakan ditentukan server" pada layar yang sama |

**Solusi terpilih: Opsi A.**

### ISS-EPS-009-05 — 500 pada daftar episode

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Di dalam transaksi pengguguran: `SELECT … FROM "InpEpisode" WHERE "Id" = … FOR UPDATE`, baca ulang episode, periksa ulang status dan batas waktu | Pola yang sama dengan `InpBedMutationCoordinator.LockAsync`; pembaca kedua menunggu lalu melihat `Cancelled`; tidak pernah ada baris riwayat ganda | Pembaca kedua menunggu sesaat (milidetik) |
| B | Tangkap `DbUpdateException` di jalur baca lalu lanjutkan | Perubahan kecil | Menyembunyikan galat lain yang sejenis; transaksi yang gagal tetap terjadi |

**Solusi terpilih: Opsi A.** Penguncian hanya dijalankan pada penyedia relasional, seperti
`LockBedRowAsync` (`InpBedOccupancyService.cs:1817-1828`), supaya test InMemory tetap berjalan.

### ISS-EPS-009-T3 — Pembatalan episode berpenempatan

Satu cara yang wajar: jalur pembatalan memakai pelepasan yang sama dengan jalur pulang
(`InvalidateAsync` → `WaitingCleaning`) di bawah kunci hierarki bed. Tidak dikerjakan pada rencana
ini karena di luar permintaan pemilik; menunggu persetujuan tersendiri.

---

## 3. Skema tampilan sebelum → sesudah

Data contoh: pasien Tn. Budi Santoso (pseudonim), DPJP dr. Andi Wijaya, unit "Rawat Inap",
kamar RI-201 Kelas II.

### 3.1 SEBELUM — langkah Pilih Bed setelah menekan Lanjut ke Pemesanan

```text
LANGKAH PILIH BED
Pilih Tempat Tidur
ⓘ Kelayakan penempatan ditentukan server …
⊗ Admisi belum dapat disimpan. Periksa data kamar dan isian Dokter sebelum melanjutkan.   ← sebab asli hilang
┌ 34 Total ┐ ┌ 28 Tersedia ┐ ┌ 6 Terisi ┐ ┌ 0 Dipesan ┐ ┌ 0 Ditutup ┐                       ← angka seluruh unit
[ Rawat Inap ▾ ] [ Kamar ▾ ] [ Cari… ] [ KELAS II ▾ ]
┌ RI-101 · Kelas I ───────────────────────────────┐
│ (×) RI-101-A  Tidak lolos kelayakan — Server tidak memasukkan …   ← padahal hanya di luar penyaring
└──────────────────────────────────────────────────┘
┌ RI-201 · Kelas II ──────────────────────────────┐
│ (•) RI-201-A  Tersedia                           │
└──────────────────────────────────────────────────┘
                                   [ Kembali ]  [ Lanjut ke Pemesanan ]  → tetap di langkah ini
```

### 3.2 SESUDAH — langkah Pilih Bed

```text
LANGKAH PILIH BED
Pilih Tempat Tidur
ⓘ Kelayakan penempatan ditentukan server …
┌ 12 Total ┐ ┌ 10 Tersedia ┐ ┌ 2 Terisi ┐ ┌ 0 Dipesan ┐ ┌ 0 Ditutup ┐                       ← angka Rawat Inap · Kelas II
[ Rawat Inap ▾ ] [ Kamar ▾ ] [ Cari… ] [ KELAS II ▾ ]
┌ RI-201 · Kelas II ──────────────────────────────┐
│ (•) RI-201-A  Tersedia                           │
│ (×) RI-201-B  Terisi — EP-2026-000214            │
└──────────────────────────────────────────────────┘
                                   [ Kembali ]  [ Menyimpan admisi… ] → Booking Bed
```

Bila gagal, alert merah memuat alasan asli, misalnya:

```text
⊗ Unit tujuan untuk kunjungan rawat inap wajib merupakan unit layanan bertipe rawat inap.
⊗ Pasien memiliki permintaan admisi dari kamar pulih; buka admisi dari permintaan itu.
⊗ DPJP wajib dipilih. Episode rawat inap tidak boleh tersimpan tanpa dokter penanggung jawab.
```

**Wilayah**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Alert galat | Alasan dari `doctor.submit` (layar atau server); kalimat umum hanya bila alasan kosong | `use-inpatient-admission-bed.jsx` `actionError` | `InformationAlert` (tidak berubah) |
| Ringkasan | Angka bed sesuai unit, kamar, kelas, dan pencarian yang sedang dipakai | `GET bed-board` dengan penyaring | Tidak berubah |
| Kartu bed | Hanya bed yang cocok penyaring; redup hanya bila ditolak aturan atau sedang terpakai | `GET bed-board` + `GET available-beds` | Tidak berubah |
| Penyaring unit | Disemai unit rawat inap bawaan; boleh diganti petugas | `doctor.form.serviceUnitId` (bawaan) | Tidak berubah |

**Tombol**

| Label | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Kembali | kedua | selalu | Kembali ke langkah Dokter |
| Lanjut ke Pemesanan | utama | Satu bed dapat dipilih terpilih; mati selama menyimpan | Titik tulis 1 dengan unit dan kelas **dari bed**: `POST /patient-encounters/admin` → `POST /episodes` (`selectedBedId`) → isolasi bila ada → maju ke Booking Bed |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Label tombol "Menyimpan admisi…"; papan tidak dapat diklik |
| Gagal di layar | Alert merah berisi alasan; isian Dokter dan bed terpilih tetap |
| Gagal di server | Alert merah berisi pesan server; kunjungan yang sudah terbentuk dipakai ulang saat mencoba lagi |
| Kosong karena penyaring | Papan kosong dengan keterangan penyaring tidak cocok (perilaku lama `hasActiveBedBoardFilter`) |
| Tidak berhak | Tidak berubah |

Langkah Dokter, Booking Bed, dan Konfirmasi: tidak ada perubahan tampilan.

---

## 4. Rincian perbaikan

### FIX-EPS-009-01 — Unit admisi mengikuti bed terpilih

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-009-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx` (`resolveInpatientAdmissionRoom`); `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` (reset dan `submit`) |
| **Radius dampak** | Alur admisi Pasien Baru, Pasien Lama, dan Kamar Pulih (`inpatient-admission-view.jsx`). Alur transfer tidak memakai `resolveInpatientAdmissionRoom` dan tidak berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. `resolveInpatientAdmissionRoom(bed)` mengembalikan `{ serviceUnitId, patientClassId, selectedBedId }`
   dari bed dan hanya menolak bila bed, kelas, atau unit bed kosong.
2. `submit({ selectedBed })` pada `deferCreation` memakai unit dari bed untuk kunjungan dan episode,
   dan menyelaraskan `form.serviceUnitId` dengan unit itu.
3. Simpan unit bawaan (opsi pertama pencarian kosong) di ref; efek reset saat pasien berganti
   memulihkan unit bawaan, bukan mengosongkannya.

**Acceptance criteria.**

1. Pasien Lama → Pembayaran Tunai → DPJP dr. Andi → bed RI-201-A (Rawat Inap, Kelas II) → Lanjut ke
   Pemesanan menghasilkan `POST /patient-encounters/admin` dan `POST /episodes` dengan `serviceUnitId`
   unit Rawat Inap, `patientClassId` Kelas II, `selectedBedId` RI-201-A, lalu layar maju ke Booking Bed.
2. Mengganti penyaring papan ke unit rawat inap lain lalu memilih bednya menghasilkan admisi pada
   unit bed itu, bukan penolakan "berbeda unit".
3. Setelah pasien dipilih, `doctor.form.serviceUnitId` tetap berisi unit bawaan (papan tersemai
   unit yang sama setiap kali).
4. Bed tanpa kelas atau tanpa unit tetap ditolak di layar dengan kalimat yang menyebut kelas/unit.

**Verifikasi.** `npx eslint <berkas yang diubah> --quiet`; test unit terarah untuk
`resolveInpatientAdmissionRoom` bila memungkinkan; `npm run build` dijalankan pemilik bila build
lokal tidak memungkinkan. Uji layar oleh pemilik (bagian 8).

**Risiko.** Bila master bed memuat bed pada unit bukan rawat inap dan petugas memilihnya, penolakan
datang dari server ("Unit tujuan untuk kunjungan rawat inap wajib…"). Terlihat jelas setelah
`FIX-EPS-009-02`.

### FIX-EPS-009-02 — Alasan asli tampil di langkah Pilih Bed

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-009-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `use-inpatient-admission-doctor.jsx` (`submit` mengembalikan `message`); `use-inpatient-admission-bed.jsx` (`prepareBooking`) |
| **Radius dampak** | Langkah Dokter (alur tanpa `deferCreation`) tetap membaca `formError` seperti sebelumnya; langkah Pilih Bed membaca `message` |
| **Bergantung pada** | Dikerjakan bersama `FIX-EPS-009-01` (berkas sama) |

**Langkah.**

1. Setiap jalur gagal `submit` mengembalikan `{ success: false, message }` dengan kalimat yang sama
   dengan yang ditulis ke `formError`.
2. `prepareBooking` menampilkan `result.message`; kalimat umum dipakai hanya bila `message` kosong.

**Acceptance criteria.**

1. Bila server menolak kunjungan dengan "Unit tujuan untuk kunjungan rawat inap wajib merupakan unit
   layanan bertipe rawat inap.", kalimat itu yang tampil di langkah Pilih Bed.
2. Pasien yang punya permintaan kamar pulih menampilkan "Pasien memiliki permintaan admisi dari kamar
   pulih; buka admisi dari permintaan itu." di langkah Pilih Bed.
3. Kalimat "Admisi belum dapat disimpan…" hanya muncul bila tidak ada alasan apa pun.

**Verifikasi.** Sama dengan `FIX-EPS-009-01`.

**Risiko.** Rendah; hanya jalur tampilan pesan.

### FIX-EPS-009-03 — Kurangi bolak-balik database pada pembacaan bed

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-009-03`, `ISS-EPS-009-T1` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend, tanpa migration, tanpa perubahan kontrak |
| **Berkas yang diubah** | `InpBedManagementQueryService.cs`, `InpBedAccessService.cs`, `InpBedOccupancyService.cs`, `InpBedMutationCoordinator.cs` (pesan 409) |
| **Radius dampak** | `GET available-beds`, `GET bed-board`, `GET bed-management/monitoring`, dan setiap pemanggil `ProjectAsync` (kelayakan pemesanan/penempatan, kesiapan bed, konteks transfer). Bentuk dan isi jawaban tidak berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. `ProjectManyAsync` dipecah: inti proyeksi mengembalikan baris beserta data yang dimuatnya
   (kesiapan, penempatan, pemesanan, unit, kelas); `ProjectManyAsync` publik tetap sama.
2. Kamar diambil dari navigasi `Room` bila sudah dimuat; query kamar hanya untuk yang belum.
3. Cek boleh-baca identitas dihitung di memori dari episode yang sudah dimuat (ikut membaca
   `ServiceUnitId` dan `IsDelete`), dengan aturan yang sama persis dengan `CanReadIdentityAsync`.
4. Unit hak aksi `StartCleaning`/`CompleteCleaning`/`VerifyReadiness` dimuat hanya bila ada bed
   bebas dalam keadaan kesiapan yang relevan.
5. `UnitsAsync` menyimpan daftar unit aktif SuperAdmin sekali per permintaan.
6. `SearchAvailableBedsAsync` memakai data proyeksi untuk kelayakan dan nama unit/kelas, tanpa memuat
   ulang. `MonitoringAsync` memakai nama unit dari data proyeksi.
7. Pesan 409 `BED_HOLD_CONFLICT` pada coordinator tanpa nama tipe exception.

**Acceptance criteria.**

1. Untuk data yang sama, JSON `available-beds`, `bed-board`, dan `monitoring` sebelum dan sesudah
   identik (urutan, status, alasan, `AvailableActions`, `HoldingContext`, penyamaran identitas).
2. Jumlah perintah SQL satu `available-beds` (34 bed, 6 terisi, SuperAdmin, tanpa bed sedang
   dibersihkan) turun dari ±35–45 menjadi ≤ 20; tidak ada query per bed.
3. Pengguna `OperationalOnly` tetap tidak melihat nama pasien dan nomor episode.
4. Konflik simpan bersamaan menjawab 409 "Data bed berubah bersamaan. Muat ulang data." tanpa
   "DbUpdateException".

**Verifikasi.** Build backend (`dotnet build` ke folder keluaran terpisah karena bin terkunci
proses yang berjalan); perbandingan jumlah perintah SQL lewat log EF `Information`
(`Microsoft.EntityFrameworkCore.Database.Command`) oleh pemilik; uji layar papan.

**Risiko.** Kesalahan menyalin aturan penyamaran identitas akan membocorkan nama pasien ke petugas
kebersihan. Dideteksi dengan acceptance criteria 3.

### FIX-EPS-009-04 — `bed-board` menerapkan penyaring

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-009-04` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend; kontrak aditif |
| **Berkas yang diubah** | `InpatientBedOccupancyController.cs` (`GetBedBoard`), `InpBedOccupancyService.BedManagement.cs` (`GetBedBoardAsync`), `InpatientBedManagementDtos.cs` (`BedMonitoringQuery.PatientClassId`), `InpBedManagementQueryService.cs` (`MonitoringAsync`) |
| **Radius dampak** | Papan admisi dan Papan Tempat Tidur (mengirim penyaring) — kini ikut tersaring sesuai rancangan frontend. Daftar kerja episode memanggil tanpa penyaring — tidak berubah. Monitoring Bed Management menerima `patientClassId` opsional baru |
| **Bergantung pada** | Dikerjakan bersama `FIX-EPS-009-03` (berkas sama) |

**Langkah.**

1. `GET bed-board` menerima `roomId`, `patientClassId`, `search` opsional di samping `serviceUnitId`.
2. `GetBedBoardAsync` meneruskan keempatnya ke `MonitoringAsync`; `MonitoringAsync` menyaring kelas
   lewat `Room.PatientClassId`.

**Acceptance criteria.**

1. `GET bed-board?serviceUnitId=<Rawat Inap>&patientClassId=<Kelas II>` hanya memuat kamar Kelas II
   dan angka ringkasannya menghitung bed Kelas II saja.
2. `GET bed-board` tanpa parameter menjawab sama seperti sebelumnya.
3. Pencarian "RI-201" hanya memuat bed yang kode/nama bed atau nama kamarnya memuat kata itu, sama
   dengan `available-beds`.

**Verifikasi.** Build backend; uji papan oleh pemilik.

**Risiko.** Layar lain yang ternyata mengirim penyaring tetapi mengharapkan papan utuh. Pencarian
source frontend hanya menemukan `use-inpatient-bed-board.jsx` (mengharapkan tersaring) dan
`use-inpatient-episode-worklist.jsx` (tanpa penyaring).

### FIX-EPS-009-05 — Kunci baris episode saat menggugurkan Draft

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-009-05` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend, tanpa migration |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` (`ExpireDraftIfDueAsync`) |
| **Radius dampak** | Semua pemanggil `ExpireDraftIfDueAsync`: daftar dan ringkasan episode, detail episode, ubah/batal admisi, penugasan dokter/perawat. Perilaku bisnis tidak berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Di dalam transaksi pengguguran, kunci baris `InpEpisode` dengan `FOR UPDATE` (hanya relasional).
2. Muat ulang nilai episode dari database, lalu periksa ulang `Draft` dan batas waktunya.
3. Bila episode sudah tidak `Draft` atau belum lewat batas, akhiri transaksi tanpa menulis apa pun
   dan kembalikan `false`.

**Acceptance criteria.**

1. Dua `GET /episodes` dan `GET /episodes/summary` bersamaan atas satu Draft yang lewat batas
   menghasilkan dua jawaban 200, satu baris riwayat `ExpireDraft`, dan episode `Cancelled`.
2. Pengguguran tunggal tetap menulis satu baris riwayat bertanda sistem dan melepas pemesanan,
   seperti sebelumnya.
3. Provider InMemory tidak menjalankan perintah kunci.

**Verifikasi.** Build backend; uji PostgreSQL dua permintaan bersamaan oleh pemilik (bagian 8).

**Risiko.** Kunci yang dipegang terlalu lama menahan pembaca lain; transaksi pengguguran pendek
(beberapa query), sehingga penundaan dalam orde milidetik.

### FIX-EPS-009-06 — Amandemen dokumen

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-009-01`, `ISS-EPS-009-04` (sisi dokumen) |
| **Area** | Dokumen blueprint |
| **Jenis perubahan** | Revisi `contracts/api-contract.md`, `05-skema-tampilan.md`, `contracts/validation-matrix.md` |
| **Radius dampak** | Hanya dokumen sub-modul `episode-rawat-inap` |
| **Bergantung pada** | `FIX-EPS-009-01`, `FIX-EPS-009-04` |

Rincian kalimat lama dan baru pada bagian 7.

### FIX-EPS-009-07 — Pembatalan episode berpenempatan memasuki siklus pembersihan

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-009-T3` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `InpEpisodeService.cs` (`ReleaseBedHoldsAsync`/`RestoreBedStatusCopyAsync`) |
| **Radius dampak** | Batal admisi `Admitted` oleh supervisor; gugur Draft (tanpa penempatan, tidak berubah) |
| **Bergantung pada** | Persetujuan pemilik tersendiri |

**Acceptance criteria.** Supervisor membatalkan episode yang sudah menempati RI-201-A → RI-201-A
tampil "Menunggu Pembersihan", bukan "Tersedia".

---

## 5. Urutan pengerjaan

```text
✅ FIX-EPS-009-01   tanpa dependency — prioritas 1, membuka kembali admisi
└── ✅ FIX-EPS-009-02   berkas sama, satu task frontend
✅ FIX-EPS-009-05   tanpa dependency — prioritas 1, menutup 500 daftar episode
✅ FIX-EPS-009-03   tanpa dependency — prioritas 2
└── ✅ FIX-EPS-009-04   berkas sama, satu task backend
FIX-EPS-009-01, FIX-EPS-009-04
└── ✅ FIX-EPS-009-06   dokumen
persetujuan pemilik tersendiri
└── FIX-EPS-009-07
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Unit admisi mengikuti bed terpilih | Ikut bed / kunci papan ke unit bawaan | Ikut bed (bagian 2) | Tidak; dipakai sebagai bawaan rekomendasi |
| K-02 | Kerjakan `FIX-EPS-009-07` | Kerjakan / tunda | Kerjakan sebagai task tersendiri | `FIX-EPS-009-07` |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `contracts/api-contract.md` | baris `GET /bed-board` (adapter Bed Management) | Query `serviceUnitId?` | Query `serviceUnitId?`, `roomId?`, `patientClassId?`, `search?` — penyaring sama dengan `available-beds`; tanpa parameter jawabannya tidak berubah (PLAN-REPAIR-EPS-009) |
| `05-skema-tampilan.md` | 3.8 wilayah "Unit Tujuan (Tersembunyi)" | "`serviceUnitId` otomatis mengambil default rawat inap" | "Unit bawaan rawat inap hanya menyemai penyaring papan; unit admisi yang disimpan mengikuti kamar bed terpilih, sama dengan kelas" |
| `05-skema-tampilan.md` | 3.9 tombol "Lanjut ke Pemesanan" | "Maju ke langkah Booking Bed" | "Titik tulis 1 dengan unit dan kelas dari bed; bila gagal, alasan aslinya tampil di langkah ini; berhasil → Booking Bed" |
| `contracts/validation-matrix.md` | `VAL-ADM-03` | "backend payload otomatis mengisi default Service Unit Rawat Inap aktif" | "Unit admisi = unit kamar bed terpilih (`SelectedBedId`); unit bawaan hanya untuk penyaring awal papan" |

---

## 8. Verifikasi menyeluruh

Dijalankan pemilik setelah backend di-build ulang dan dijalankan ulang (server lokal dijalankan
`dotnet run --no-build`, sehingga perubahan backend belum aktif sebelum build).

1. **Admisi Pasien Lama sampai Booking.** Cari pasien → Tunai → DPJP → Pilih Bed (Rawat Inap,
   Kelas II) → Lanjut ke Pemesanan → layar maju ke Booking Bed; log backend memuat
   `POST /patient-encounters/admin` dan `POST /episodes` 200.
2. **Penyaring papan.** Ganti Kelas ke VIP → kartu dan angka ringkasan hanya VIP; kembali Kelas II →
   angka berubah lagi.
3. **Pesan galat.** Pilih pasien yang punya permintaan kamar pulih tanpa membuka dari permintaannya
   → kalimat kamar pulih tampil di langkah Pilih Bed.
4. **Kinerja.** Nyalakan log `Microsoft.EntityFrameworkCore.Database.Command` pada `Information`,
   buka langkah Pilih Bed → hitung perintah SQL per permintaan (target ≤ 20 untuk `available-beds`).
5. **500 daftar episode.** Siapkan satu Draft dengan `UpdateDateTime` lewat `DraftEpisodeExpiryHours`,
   buka halaman daftar episode (memanggil daftar dan ringkasan bersamaan) → keduanya 200, satu baris
   riwayat `ExpireDraft`.
6. **Catatan rilis (`ISS-EPS-009-T2`).** Lingkungan UAT/produksi wajib mengisi
   `BedManagement:ReservationExpiryActorUserId` (akun sistem aktif) dan
   `BedManagement:ReservationExpiryAssignmentReference`; tanpa itu papan bed menjawab 503 setiap kali
   ada pemesanan kedaluwarsa.
7. **Migration (`ISS-EPS-009-07`).** Setelah menarik commit bermigration, jalankan
   `dotnet ef database update` sebelum `dotnet run`.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Rencana dibuat, 7 perbaikan. `FIX-01` s.d. `FIX-06` disetujui langsung lewat laporan pemilik; K-01 memakai rekomendasi sebagai bawaan. `FIX-07` menunggu persetujuan tersendiri | `diagnose-module-issue` |
| 2026-10-13 | `FIX-EPS-009-01` dan `-02` ✅ sebagai `FE-RWI-243`. Delta: `ISS-EPS-009-T4` (kode galat di `errors.code`) ikut ditutup `FIX-02`; test unit lama "unit berbeda ditolak" diganti aturan unit dari bed. ESLint/build tidak dapat dijalankan karena `node_modules` hilang dari repository frontend | `build-module-frontend` |
| 2026-10-13 | `FIX-EPS-009-05` ✅ sebagai `BE-RWI-229`. Tanpa delta terhadap rencana | `build-module-backend` |
| 2026-10-13 | `FIX-EPS-009-03` dan `-04` ✅ sebagai `BE-RWI-228`. Delta: `EvaluatePlacementEligibilityAsync` (pemesanan/penempatan) ikut memakai proyeksi yang sama; `bed-management/monitoring` ikut menerima `patientClassId` opsional; `FillServiceUnitAndClassNamesAsync` dihapus | `build-module-backend` |
| 2026-10-13 | `FIX-EPS-009-06` ✅: amandemen ditambahkan pada `contracts/api-contract.md`, `contracts/validation-matrix.md`, dan `05-skema-tampilan.md` | `diagnose-module-issue` (MODULE BLUEPRINT MODE) |
