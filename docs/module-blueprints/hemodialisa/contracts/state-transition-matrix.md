# Hemodialisa — Matriks Perpindahan Status

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `approved` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| `input_revision` | `02-backend-architecture.md` r1 |

Dokumen ini adalah **satu-satunya** tempat daftar perpindahan status hidup. Flowchart memakai
nama status yang sama persis, tetapi tidak mengulang daftarnya.

Perpindahan yang **tidak** tercantum di sini berarti **tidak sah**. Setiap upaya menjalankannya
ditolak server dengan kode dan pesan yang ada di `contracts/validation-matrix.md`.

---

## 1. Permintaan HD — `HmdOrderStatus`

Enam status: `Requested`, `Accepted`, `OnHold`, `Rejected`, `Cancelled`, `Fulfilled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat permintaan | `Requested` | Dokter atau perawat unit peminta | Pasien dan kunjungan sah; alasan klinis terisi | `HMD-VAL-001` |
| `Requested` | Terima | `Accepted` | Koordinator unit HD | Unit HD menerima pasien pada periode itu | `HMD-VAL-002` |
| `Requested` | Tahan | `OnHold` | Koordinator unit HD | Alasan operasional terisi | `HMD-VAL-003` |
| `Accepted` | Tahan | `OnHold` | Koordinator unit HD | Alasan operasional terisi | `HMD-VAL-003` |
| `OnHold` | Lepas tahanan | status sebelumnya | Koordinator unit HD | — | `HMD-VAL-004` |
| `Requested` | Tolak | `Rejected` | **Dokter** | Alasan klinis terisi | `HMD-VAL-005` |
| `OnHold` | Tolak | `Rejected` | **Dokter** | Alasan klinis terisi | `HMD-VAL-005` |
| `Requested` | Batalkan | `Cancelled` | Pembuat permintaan | Belum diterima unit HD | `HMD-VAL-006` |
| `Accepted` | Tautkan ke sesi | `Fulfilled` | Koordinator unit HD | Sesi terbentuk dan menunjuk permintaan ini | — |

**Tidak sah, dan sengaja disebut:**

| Perpindahan yang dilarang | Sebabnya |
|---|---|
| `Rejected` → status mana pun | Penolakan klinis bersifat akhir. Bila kondisi berubah, dibuat permintaan baru agar riwayat penolakan tetap terbaca |
| `Fulfilled` → `Cancelled` | Membatalkan permintaan yang sudah menjadi sesi akan meninggalkan sesi tanpa asal-usul. Yang dibatalkan adalah sesinya |
| `Cancelled` → `Requested` | Sama alasannya dengan penolakan |
| Koordinator menolak permintaan | Menolak adalah keputusan klinis, bukan operasional (`HMD-ASM-003`) |

---

## 2. Episode HD — `HmdEpisodeStatus`

Empat status: `Draft`, `Active`, `Suspended`, `Closed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat episode | `Draft` | Petugas administrasi HD | Pasien sah; DPJP ditetapkan | `HMD-VAL-010` |
| `Draft` | Aktifkan | `Active` | Petugas administrasi HD | Pasien belum punya episode `Active` lain, kecuali pengaturan mengizinkan | `HMD-VAL-011` |
| `Active` | Tangguhkan | `Suspended` | Petugas administrasi HD | Alasan penangguhan terisi | `HMD-VAL-012` |
| `Suspended` | Aktifkan kembali | `Active` | Petugas administrasi HD | Tidak ada episode `Active` lain pada pasien yang sama | `HMD-VAL-011` |
| `Active` | Tutup | `Closed` | Petugas administrasi HD | Tidak ada sesi yang belum difinalisasi; alasan penutupan terisi | `HMD-VAL-013` |
| `Suspended` | Tutup | `Closed` | Petugas administrasi HD | Sama seperti di atas | `HMD-VAL-013` |
| `Draft` | Tutup | `Closed` | Petugas administrasi HD | Belum pernah ada sesi | `HMD-VAL-013` |

**Tidak sah:** `Closed` → status mana pun. Pasien yang kembali menjalani program HD mendapat
episode **baru**, supaya riwayat program lamanya utuh.

---

## 3. Resep HD — `HmdPrescriptionStatus`

Empat status: `Draft`, `Active`, `Superseded`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat draf | `Draft` | Dokter dialisis | Episode berstatus `Active` | `HMD-VAL-020` |
| `Draft` | Aktifkan | `Active` | Dokter dialisis | Akses vaskular yang dirujuk masih layak; seluruh parameter wajib terisi | `HMD-VAL-021` |
| `Active` | Gantikan | `Superseded` | Dokter dialisis | Resep pengganti berhasil diaktifkan pada transaksi yang sama | `HMD-VAL-022` |
| `Draft` | Batalkan | `Cancelled` | Dokter dialisis | — | — |
| `Active` | Batalkan | `Cancelled` | Dokter dialisis | Tidak ada sesi berjalan yang memakai resep ini | `HMD-VAL-023` |

**Tidak sah, dan inilah aturan terpenting pada bagian ini:**

| Perpindahan yang dilarang | Sebabnya |
|---|---|
| Menyunting resep berstatus `Active` | Instruksi dokter yang sudah berlaku tidak boleh berubah diam-diam. Perubahan membuat resep **baru** yang menggantikan yang lama |
| `Superseded` → `Active` | Menghidupkan kembali resep lama membuat dua resep merasa berlaku. Bila instruksi lama ingin dipakai lagi, dibuat resep baru yang isinya sama |
| Perawat mengubah resep agar cocok dengan tindakan yang sudah terjadi | Penyimpangan pelaksanaan dicatat pada sesi sebagai deviasi, bukan dengan mengubah instruksi dokter |

---

## 4. Sesi HD — `HmdSessionStatus`

Dua belas status: `Planned`, `Scheduled`, `CheckedIn`, `PreCheck`, `Held`, `Ready`,
`InProgress`, `Stopped`, `Completed`, `AwaitingFinalization`, `Finalized`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Bentuk sesi | `Planned` | Koordinator unit HD | Episode `Active`; resep `Active` | `HMD-VAL-030` |
| `Planned` | Tetapkan jadwal dan sumber daya | `Scheduled` | Koordinator unit HD | Mesin, station, dan pasien bebas tabrakan; mesin `Ready` dan memenuhi kebutuhan isolasi | `HMD-VAL-031` s/d `HMD-VAL-035` |
| `Scheduled` | Pasien datang | `CheckedIn` | Perawat dialisis | Kunjungan sah tersedia | `HMD-VAL-036` |
| `CheckedIn` | Mulai isi Pra-HD | `PreCheck` | Perawat dialisis | — | — |
| `PreCheck` | Nyatakan siap | `Ready` | Perawat dialisis | Seluruh butir checklist wajib terpenuhi atau dilewati secara sah; penilaian Pra-HD terisi; kesiapan unit `Ready`; dokter penanggung jawab ditetapkan | `HMD-VAL-040` s/d `HMD-VAL-044` |
| `PreCheck` | Tahan | `Held` | Perawat dialisis | Alasan penahanan terisi | `HMD-VAL-045` |
| `Ready` | Tahan | `Held` | Perawat dialisis | Alasan penahanan terisi | `HMD-VAL-045` |
| `Held` | Lanjutkan | `PreCheck` | Perawat dialisis | — | — |
| `Ready` | Mulai | `InProgress` | Perawat dialisis | Seluruh syarat `Ready` **diperiksa ulang** saat ini juga; kunci idempotency menyertai permintaan | `HMD-VAL-050` s/d `HMD-VAL-053` |
| `InProgress` | Selesai | `Completed` | Perawat dialisis | Penilaian Pasca-HD dan disposisi terisi | `HMD-VAL-060` |
| `InProgress` | Hentikan | `Stopped` | Perawat dialisis | Alasan penghentian terisi | `HMD-VAL-061` |
| `Completed` | Selesaikan dokumentasi | `AwaitingFinalization` | Perawat dialisis | Isian minimum final lengkap | `HMD-VAL-070` |
| `Stopped` | Selesaikan dokumentasi | `AwaitingFinalization` | Perawat dialisis | Isian minimum final lengkap | `HMD-VAL-070` |
| `AwaitingFinalization` | Kembalikan untuk dilengkapi | `Completed` atau `Stopped` | Dokter penanggung jawab | Alasan pengembalian terisi | `HMD-VAL-071` |
| `AwaitingFinalization` | Sahkan dan kunci | `Finalized` | **Dokter penanggung jawab sesi** | Isian minimum lengkap; dokumen berhasil didaftarkan dan ditandatangani di daftar keutuhan Rekam Medis | `HMD-VAL-072` s/d `HMD-VAL-074` |
| `Planned` | Batalkan | `Cancelled` | Koordinator unit HD | Alasan pembatalan terisi | `HMD-VAL-080` |
| `Scheduled` | Batalkan | `Cancelled` | Koordinator unit HD | Alasan pembatalan terisi | `HMD-VAL-080` |
| `CheckedIn` | Batalkan | `Cancelled` | Koordinator unit HD | Alasan pembatalan terisi | `HMD-VAL-080` |
| `PreCheck` | Batalkan | `Cancelled` | Koordinator unit HD | Alasan pembatalan terisi | `HMD-VAL-080` |
| `Held` | Batalkan | `Cancelled` | Koordinator unit HD | Alasan pembatalan terisi | `HMD-VAL-080` |
| `Ready` | Batalkan | `Cancelled` | Koordinator unit HD | Alasan pembatalan terisi | `HMD-VAL-080` |

### Perpindahan yang tidak sah pada sesi

| Perpindahan yang dilarang | Sebabnya |
|---|---|
| `InProgress` → `Cancelled` | Cuci darah yang sudah berjalan tidak dapat dianggap tidak pernah terjadi. Yang tersedia adalah `Stopped` |
| `Finalized` → status mana pun | Catatan klinis final bersifat tetap. Koreksi memakai *addendum*, bukan membuka kunci |
| `Completed` atau `Stopped` → `InProgress` | Sesi yang sudah berakhir secara fisik tidak dapat dilanjutkan. Bila pasien perlu dicuci darah lagi, dibuat sesi baru |
| `Scheduled` → `InProgress` tanpa melewati `Ready` | Melompati pemeriksaan Pra-HD menghilangkan seluruh pengaman keselamatan sebelum tindakan |
| `AwaitingFinalization` → `Finalized` oleh perawat | Pengesahan adalah kewenangan dokter penanggung jawab sesi (`HMD-ASM-002`) |
| Dokter yang sama menyelesaikan dokumentasi **dan** mengesahkannya | Ditolak bila pengaturan menghendaki dua orang berbeda. Lihat `HMD-GATE-006` |

### Dua pelaku pada finalisasi

Inilah wujud syarat 2. Dua kolom disimpan terpisah, dan keduanya wajib terisi pada sesi yang
`Finalized`:

| Kolom | Diisi saat | Oleh |
|---|---|---|
| `DocumentedByUserId`, `DocumentedAt` | Perpindahan ke `AwaitingFinalization` | Perawat dialisis |
| `SignedByUserId`, `SignedAt` | Perpindahan ke `Finalized` | Dokter penanggung jawab sesi |

Bila badan klinis kelak memilih alur satu langkah, kolom kedua tinggal diisi orang yang sama —
tanpa perubahan tabel.

---

## 5. Status mesin — `HmdMachineStatus`

Empat status: `Ready`, `Blocked`, `Maintenance`, `NotEligible`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Ready` | Blokir | `Blocked` | Koordinator unit HD | Alasan terisi; mesin tidak sedang dipakai sesi berjalan | `HMD-VAL-090` |
| `Ready` | Masukkan perawatan | `Maintenance` | Koordinator unit HD | Sama seperti di atas | `HMD-VAL-090` |
| `Ready` | Nyatakan tidak laik | `NotEligible` | Koordinator unit HD | Sama seperti di atas | `HMD-VAL-090` |
| `Blocked` | Nyatakan siap | `Ready` | Koordinator unit HD | Alasan pemulihan terisi | — |
| `Maintenance` | Nyatakan siap | `Ready` | Koordinator unit HD | Alasan pemulihan terisi | — |
| `NotEligible` | Nyatakan siap | `Ready` | Koordinator unit HD | Alasan pemulihan terisi | — |
| `Blocked` | Masukkan perawatan | `Maintenance` | Koordinator unit HD | — | — |
| `Maintenance` | Blokir | `Blocked` | Koordinator unit HD | — | — |

Setiap perpindahan **wajib** menulis satu baris riwayat. Tidak ada perpindahan status mesin yang
boleh terjadi tanpa jejak.

**Tidak sah:** menjadwalkan atau memulai sesi pada mesin berstatus `Blocked`, `Maintenance`,
atau `NotEligible`.

---

## 6. Status station — `HmdStationStatus`

Tiga status: `Available`, `Blocked`, `Maintenance`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| `Available` | Blokir | `Blocked` | Koordinator unit HD | Alasan terisi; tidak sedang dipakai sesi berjalan |
| `Available` | Masukkan perawatan | `Maintenance` | Koordinator unit HD | Sama |
| `Blocked` | Buka | `Available` | Koordinator unit HD | — |
| `Maintenance` | Buka | `Available` | Koordinator unit HD | — |

---

## 7. Kesiapan unit — `HmdReadinessStatus`

Tiga status: `Draft`, `Ready`, `NotReady`. Status `ConditionallyReady` **sengaja tidak dipakai**,
karena ia melahirkan kelonggaran yang belum disahkan siapa pun.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Bentuk lembar pemeriksaan | `Draft` | Koordinator unit HD | Belum ada lembar untuk tanggal dan shift yang sama | `HMD-VAL-100` |
| `Draft` | Nyatakan siap | `Ready` | Koordinator unit HD | Seluruh butir wajib terpenuhi; hasil pemeriksaan air masih dalam masa berlaku | `HMD-VAL-101`, `HMD-VAL-102` |
| `Draft` | Nyatakan tidak siap | `NotReady` | Koordinator unit HD | Alasan terisi | `HMD-VAL-103` |
| `Ready` | Nyatakan tidak siap | `NotReady` | Koordinator unit HD | Alasan terisi | `HMD-VAL-103` |
| `NotReady` | Nyatakan siap | `Ready` | Koordinator unit HD | Seluruh butir wajib terpenuhi | `HMD-VAL-101` |

Ketika kesiapan unit berpindah ke `NotReady`, sesi yang **belum** dimulai pada shift itu tidak
dapat dinyatakan `Ready`. Sesi yang **sudah** berjalan tidak dihentikan otomatis — penghentian
adalah keputusan klinis, bukan akibat perubahan status administratif.

---

## 8. Status verifikasi kewenangan — `HmdCompetencyVerificationStatus`

Tiga nilai: `Verified`, `NotAuthorized`, `NotVerifiable`.

| Nilai | Kapan dipakai | Akibatnya pada penjadwalan |
|---|---|---|
| `Verified` | Pembacaan kewenangan berhasil dan petugas memang berwenang | Penugasan diterima |
| `NotAuthorized` | Pembacaan berhasil dan petugas tidak berwenang | Penugasan ditolak bila pengaturan penegakan menyala; bila mati, diterima dengan peringatan |
| `NotVerifiable` | Pembacaan kewenangan **belum tersedia** (`HMD-DEP-002`) | Penugasan diterima dengan peringatan, apa pun nilai pengaturan penegakan |

Nilai `NotVerifiable` **tidak boleh** ditulis sebagai `Verified`. Perbedaan keduanya adalah
perbedaan antara "sudah diperiksa dan aman" dan "belum pernah diperiksa", dan justru itulah yang
dicari auditor.

Ini adalah satu-satunya tempat status berubah karena **pengaturan**, bukan karena tindakan
manusia. Ketika `HmdSetting.EnforceCompetencyCheck` dinyalakan, tidak ada tabel yang berubah —
yang berubah hanya apakah `NotAuthorized` menolak penugasan atau sekadar memperingatkan.
