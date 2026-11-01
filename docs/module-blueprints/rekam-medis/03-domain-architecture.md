# Rekam Medis — Hospital Domain Architecture (Amandemen 28 Oktober 2026)

## A. Identitas arsitektur

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` (revision blueprint `3`; naik ke `4` pada `/design-business-module`) |
| Architecture ID | `RM-DA-001` revision `1` |
| Tanggal | 28 Oktober 2026 |
| Scope | Amandemen `RM-DEC-037`: sambungan berkas ke dokumen final seluruh modul, dan kelengkapan berkas |
| Kesiapan requirement | `RM-RCG-001` revision 2 — `PARTIALLY_READY`; sepuluh slice siap, S-A1b berhenti |
| Input | Decision log revision `9` (`sha256:1d788ecd…`, `RM-DEC-037`–`061`); capability map revision `3` (`sha256:b840e22b…`, `RM-CAP-034`–`070`); `RM-RCG-001` revision 2 (`sha256:a82ce13a…`) |
| Source SHA | Backend `363de34f` (branch `yoga`); frontend `f0a3b07b8` (branch `YogaV2`) |
| Baseline rujukan | **Tidak dipakai.** Tidak ada observasi `REFERENCE_ONLY` pada dokumen ini |
| Sifat | Arsitektur **logis**. Tidak ada tabel, kolom, endpoint, migration, maupun komponen UI yang ditetapkan di sini. Nama class di kolom bukti hanya menunjuk keadaan source saat ini |

Rilis pertama (penelusuran, keutuhan, jejak akses) dirancang di `02-backend-architecture.md` dan
**tidak** dirancang ulang. Dokumen ini hanya menambah apa yang dituntut amandemen.

### Slice yang dirancang

| Slice | Isi | Keputusan |
|---|---|---|
| S-A1 | Hasil Lab Patologi Klinik dan Mikrobiologi | `RM-DEC-030`–`035`, `057` |
| S-A2 | Laporan Radiologi | `RM-DEC-038`, `041`, `042` |
| S-A3 | IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah | `RM-DEC-039`, `059` |
| S-A4 | Farmasi, model `ClinicalManagement` baru, ringkasan pencatatan berkala | `RM-DEC-039`, `052` |
| S-A5 | Hasil dan resume MCU | `RM-DEC-040` |
| S-A6 | Penanda `Susulan` dan "pernah diralat" | `RM-DEC-042`, `043`, `051` |
| S-A7 | Perbaikan kekosongan diam-diam jenis 14/15 dan pemberitahuan cakupan | `RM-CAP-035`, `041` |
| S-B1 | Master dokumen wajib | `RM-DEC-045`, `047`, `053`–`056`, `060`, `061` |
| S-B2 | Penilaian kelengkapan kunjungan dan daftar kerja | `RM-DEC-045`, `046`, `050`, `051` |
| S-B3 | Addendum pengesahan | `RM-DEC-049` |
| ~~S-A1b~~ | ~~Hasil Patologi Anatomi~~ | **Tidak dirancang** — menunggu open question 18 (`RM-DEC-058`). Hanya keadaan "belum tersedia di berkas" yang dirancang, sebagai bagian S-A7 |

---

## B. Ubiquitous language

| Istilah | Makna dalam modul ini | Bukan |
|---|---|---|
| **Berkas rekam medis** | Satu kesatuan seluruh dokumen klinis final milik satu pasien, lintas kunjungan dan lintas modul, dibaca langsung dari pemiliknya setiap kali dibuka | Bukan salinan; bukan tabel penampung isi dokumen |
| **Dokumen sumber** | Dokumen klinis yang dibuat dan dimiliki modul pelayanan atau penunjang, misalnya hasil lab, laporan operasi, ringkasan pulang | Bukan milik rekam medis |
| **Final** | Keadaan dokumen yang menurut **modul pemiliknya** sudah tidak lagi konsep. Arti tepatnya berbeda per modul (`RM-DEC-041`) | Bukan status keutuhan rekam medis (`Signed`) — dua hal berbeda |
| **Waktu final** | Saat dokumen menjadi final menurut pemiliknya, misalnya `ReleasedAt` hasil lab | Bukan waktu kejadian klinis (`BirthDateTime`, `VisitAt`) — kecuali sumbernya memang tak punya waktu final (`RM-DEC-059`) |
| **Tutup klinis** | Saat pelayanan kunjungan selesai dan catatan terbuka dikunci otomatis (`RM-DEC-051`) | Bukan tutup administratif (`CompletedAt` saat finalisasi tagihan) |
| **Susulan** | Dokumen yang waktu finalnya **sesudah** tutup klinis kunjungannya | Bukan kunjungan yang dibuka kembali — kunjungan tidak pernah dibuka kembali (`RM-DEC-006`) |
| **Pernah diralat** | Dokumen final yang isinya kemudian dikoreksi lewat mekanisme modul pemiliknya | Bukan addendum rekam medis — addendum punya jejaknya sendiri |
| **Dapat berubah** | Label untuk sumber yang isinya dapat diganti tanpa riwayat (asuhan gizi, `RM-DEC-059`) | Bukan konsep |
| **Kelengkapan berkas** | Penilaian apakah berkas satu kunjungan sudah memuat semua yang seharusnya, menurut tiga syarat `RM-DEC-045` | Bukan peringatan "RIWAYAT INI TIDAK LENGKAP" pada linimasa — itu **gangguan sumber** (`RM-CAP-040`) |
| **Kekurangan** | Satu alasan konkret berkas belum lengkap, misalnya "catatan anestesi belum ada" | — |
| **Dokumen wajib** | Butir yang dituntut menurut jenis kunjungan, jenis unit layanan, dan kejadian di dalam kunjungan | — |
| **Kriteria** | Cara mengenali bahwa satu butir dokumen wajib terpenuhi, misalnya "asesmen awal medis berstatus selesai" | Kriteria ditulis di program; **pemilihan** kriteria per jenis kunjungan diatur master |
| **Gangguan sumber** | Keadaan ketika satu modul tidak dapat dibaca saat berkas dibuka | Bukan "tidak ada dokumen" |
| **Addendum pengesahan** | Addendum yang menyatakan isi catatan `LockedUnsigned` benar, sehingga kekurangan tanda tangannya tuntas (`RM-DEC-049`) | Bukan tanda tangan susulan — catatan tetap `LockedUnsigned` |
| **Kunjungan penunjang langsung** | Kunjungan Rawat Jalan di unit berjenis Laboratorium, Radiologi, atau Hemodialisa (`RM-DEC-060`) | — |

---

## C. Peta bounded context

| ID | Context | Tanggung jawab dalam amandemen | Konsep yang dimiliki | Hubungan |
|---|---|---|---|---|
| `BC-RM-01` | **Berkas Rekam Medis** (`MedicalRecordManagement`) | Menyatukan dokumen final semua sumber, menurunkan penanda `Susulan`, menyajikan ringkasan berkala, mencatat jejak akses | `DC-RM-01`, `04`, `10` | **Downstream** dari seluruh context sumber, lewat kontrak `DC-RM-03`. Pola *customer–supplier*: rekam medis menetapkan bentuk kontrak, pemilik sumber mengisinya |
| `BC-RM-02` | **Keutuhan Dokumen** (`MedicalRecordManagement`, sudah ada) | Penguncian, tanda tangan, addendum, **penutupan klinis**, addendum pengesahan | `DC-RM-05`, `09`; existing `MrcClinicalDocumentIntegrity`, `MrcClinicalNoteAddendum` | Dipanggil context pelayanan saat kunjungan ditutup (sudah berjalan di lima jalur) |
| `BC-RM-03` | **Kelengkapan Berkas** (`MedicalRecordManagement`, baru) | Master dokumen wajib, penilaian per kunjungan, daftar kerja | `DC-RM-06`, `07`, `08`, `12` | Downstream dari `BC-RM-01` (dokumen), `BC-RM-02` (penutupan, tanda tangan), dan kontrak `DC-RM-13` (pemeriksaan tertunda) |
| `BC-LAB` | Laboratorium | Hasil, rilis, ralat (`S6`), pemeriksaan tertunda | Hasil lab | Upstream; mengisi `DC-RM-03` dan `DC-RM-13` |
| `BC-RAD` | Radiologi | Laporan, rilis, amandemen, pemeriksaan tertunda | Laporan radiologi | Upstream; `DC-RM-03` dan `DC-RM-13` |
| `BC-IGD` | IGD | Triase, observasi, resusitasi, disposisi; tutup visit | Dokumen IGD | Upstream; `DC-RM-03`; pemanggil penutupan klinis |
| `BC-RWI` | Rawat Inap | Dokumen admisi, ringkasan pulang; tutup episode | Dokumen rawat inap | Upstream; `DC-RM-03`; pemanggil penutupan klinis |
| `BC-OK` | Kamar Operasi | Laporan operasi, anestesi, checklist, kelahiran | Dokumen operasi | Upstream; `DC-RM-03`; penanda kejadian "kunjungan memuat operasi" |
| `BC-HMD` | Hemodialisa | Catatan sesi (sudah ikut `BC-RM-02`) | Sesi | Upstream; `DC-RM-03` |
| `BC-GZI`, `BC-BDR`, `BC-PHR`, `BC-MCU` | Gizi, Bank Darah, Farmasi, MCU | Dokumen masing-masing | Dokumen masing-masing | Upstream; `DC-RM-03` |
| `BC-CLI` | `ClinicalManagement` | Tiga belas jenis lama + model baru | Catatan klinis | Upstream. Karena seluruh modelnya punya `PatientId`, pembacaannya dapat tetap di `BC-RM-01` lewat penarik generik (`RM-CAP-042`) |
| `BC-REG` | Registrasi | Kunjungan, jenis kunjungan, status, `CompletedAt` | `RegPatientEncounter` | Upstream, **Existing** — sumber `EncounterType`, `ServiceUnitId`, `IsReferral` |
| `BC-MST` | Master Data | Unit layanan dan jenisnya | `MstServiceUnit.ServiceUnitType` | Upstream, **Existing** (`RM-FACT-021`) |
| `BC-PAT` | Patient Management | Identitas pasien | `MstPatient` | Upstream, **Existing** |
| `BC-AUTH` | Platform Authorization | Izin per departemen-posisi | `SysAccessPolicies` | Upstream, **Existing** |

---

## D. Katalog konsep domain

| ID | Konsep | Klasifikasi | Context | Ownership | Identitas | Invariant penting | Bukti |
|---|---|---|---|---|---|---|---|
| `DC-RM-01` | Berkas Rekam Medis Pasien | `AGGREGATE_ROOT` baca (read model) | `BC-RM-01` | **Extend** (layar penelusuran yang ada) | Id pasien | Tidak menyimpan isi dokumen sumber (`RM-DEC-038`); satu pembukaan = satu jejak akses (`RM-DEC-040`); sumber gagal tampil sebagai gangguan (`RM-DEC-044`) | `RM-CAP-034`, `042` |
| `DC-RM-02` | Deskriptor Dokumen Final | `VALUE_OBJECT` | Kontrak `BC-RM-01` ↔ sumber | **New** (bentuk kontrak) | Jenis dokumen + id dokumen di sumbernya | Hanya dokumen final (`RM-DEC-041`); wajib membawa waktu final (`RM-DEC-043`) | `RM-CAP-043`–`057` |
| `DC-RM-03` | Kontrak Sumber Dokumen Berkas | `EXTERNAL_CONTRACT` (internal antarcontext) | Diisi tiap context sumber | **Extend** setiap pemilik sumber | Satu per jenis dokumen | Masukan **id pasien**, keluaran deskriptor (`RM-DEC-038`); dipanggil dalam proses, tidak lewat izin modul asal (`RM-DEC-040`) | `RM-CAP-058`, `069`, `070` |
| `DC-RM-04` | Ringkasan Pencatatan Berkala | `VALUE_OBJECT` turunan | `BC-RM-01` | **Adapter/View** | Pasien + jenis + tanggal | Satu baris per jenis per hari; jumlah wajib sama dengan jumlah pengukuran yang tidak dibatalkan (`RM-DEC-052`, `AC-42`) | `RM-CAP-066` |
| `DC-RM-05` | **Penutupan Klinis Kunjungan** | `AGGREGATE_ROOT` | `BC-RM-02` | **New** | Satu per kunjungan | Dicatat **sekali**, saat penguncian otomatis pertama kali dijalankan untuk kunjungan itu; **tidak pernah berubah** sesudahnya, termasuk oleh rekonsiliasi IGD (`RM-DEC-051`) | `RM-CAP-036`; `ClinicalDocumentIntegrityService.cs:551-586` |
| `DC-RM-06` | Aturan Dokumen Wajib | `REFERENCE_DATA` | `BC-RM-03` | **New** | Jenis kunjungan + jenis unit layanan (boleh "semua unit") + pemicu bersyarat (boleh kosong) + kriteria | Isi awal = `RM-DEC-047` diperluas `RM-DEC-060`; dapat diubah tanpa rilis program (`RM-DEC-045`, `AC-33`); tidak memuat butir rujukan (`RM-DEC-061`) | `RM-CAP-038` |
| `DC-RM-06a` | Katalog Kriteria Dokumen | `REFERENCE_DATA` versi program | `BC-RM-03` | **New** | Kode kriteria | Setiap kriteria menunjuk satu sumber dan satu definisi "terpenuhi" yang sudah disahkan (`RM-DEC-053`–`056`, `059`, `060`); master hanya **memilih** kriteria, tidak mendefinisikannya | `RM-CAP` kelompok I |
| `DC-RM-07` | **Penilaian Kelengkapan Kunjungan** | `AGGREGATE_ROOT` turunan | `BC-RM-03` | **New** | Satu per kunjungan yang sudah tutup klinis | Keadaan utama mengikuti urutan prioritas `RM-DEC-046`; tidak pernah `Lengkap` bila ada sumber gagal (`RM-DEC-044`); tidak memuat isi klinis | — |
| `DC-RM-08` | Kekurangan | `VALUE_OBJECT` | `BC-RM-03` | **New** | Jenis kekurangan + rujukan dokumen atau kriteria | Seluruh kekurangan ditampilkan, tidak ada yang disembunyikan di balik keadaan utama (`RM-FE-023`) | — |
| `DC-RM-09` | Addendum Pengesahan | `ENTITY` | `BC-RM-02` | **Extend** `MrcClinicalNoteAddendum` | Id addendum | Hanya atas catatan `LockedUnsigned`; pembuat mengikuti `RM-DEC-004`/`020`; menuntaskan kekurangan hanya setelah ditandatangani; status catatan tidak berubah (`RM-DEC-049`) | `RM-CAP-039` |
| `DC-RM-10` | Penanda Susulan | `VALUE_OBJECT` turunan | `BC-RM-01` | **Adapter/View** | — | Waktu final dokumen > waktu `DC-RM-05` (`RM-DEC-043`, `051`) | `RM-CAP-037` |
| `DC-RM-11` | Penanda Pernah Diralat | `VALUE_OBJECT` | Dibawa `DC-RM-02` | **Existing** di tiap sumber | — | Dibaca dari pemilik, tidak dihitung rekam medis (`RM-DEC-042`) | `RM-CAP` kelompok F |
| `DC-RM-12` | Tanggal Mulai Daftar Kerja | `REFERENCE_DATA` (satu nilai) | `BC-RM-03` | **New** | — | Kunjungan yang tutup klinis sebelum tanggal ini tidak masuk daftar kerja, tetapi tetap dapat dinilai bila diminta (`RM-DEC-050`) | — |
| `DC-RM-13` | Kontrak Pemeriksaan Tertunda | `EXTERNAL_CONTRACT` | Diisi `BC-LAB`, `BC-RAD` | **Extend** | Kunjungan | Mengembalikan pemeriksaan yang dipesan tetapi belum final, **termasuk Patologi Anatomi** walaupun hasil PA belum tampil di berkas | `RM-DEC-035` |

### Konsep bersama yang dipakai ulang lewat pemiliknya

| Konsep | Pemilik | Ownership | Dipakai untuk |
|---|---|---|---|
| Pasien | `BC-PAT` | **Existing** | Kunci berkas |
| Kunjungan, jenis kunjungan, status, rujukan | `BC-REG` | **Existing** | Kunci penilaian, jenis kunjungan, pemicu daftar kerja |
| Jenis unit layanan | `BC-MST` | **Existing** | Kunci aturan `RM-DEC-060` |
| Episode rawat inap, visit IGD, kasus operasi, sesi HD | Context masing-masing | **Existing** | Penerjemah id pasien dan pemicu bersyarat |
| Pengguna dan izin | `BC-AUTH` | **Existing** | Akses berkas, daftar kerja, master |
| Status keutuhan dan addendum | `BC-RM-02` | **Existing** | Syarat 1 |

---

## E. Model aggregate

### `AGG-RM-01` — Berkas Rekam Medis Pasien (`DC-RM-01`)

| Aspek | Isi |
|---|---|
| Root | Pasien |
| Batas | Satu pembukaan berkas: daftar deskriptor dari semua sumber, daftar gangguan sumber, satu jejak akses |
| Invariant | (1) Nol isi dokumen sumber tersimpan di rekam medis. (2) Setiap sumber yang gagal masuk daftar gangguan dan tidak pernah tampil sebagai daftar kosong. (3) Jenis yang belum tersambung — termasuk PA — tampil sebagai "belum tersedia", bukan kosong (`RM-DEC-058`, `AC-39`, `AC-49`). (4) Satu jejak akses per pembukaan |
| Tindakan | Buka berkas; saring per jenis/kunjungan/rentang; buka rincian satu dokumen (diteruskan ke pemiliknya) |
| Event | Tidak menerbitkan event baru. Jejak akses tetap seperti rilis pertama |

### `AGG-RM-02` — Penutupan Klinis Kunjungan (`DC-RM-05`)

| Aspek | Isi |
|---|---|
| Root | Kunjungan |
| Batas | Satu catatan per kunjungan: waktu tutup klinis, context yang menutup (Registrasi, Rawat Inap, IGD, Billing), pelaku |
| Invariant | (1) Paling banyak satu per kunjungan. (2) Ditulis pada panggilan penguncian **pertama**; panggilan berikutnya tidak mengubahnya — idempoten. (3) Tidak diubah oleh rekonsiliasi IGD maupun perubahan `CompletedAt` |
| Mengapa konsep baru | Hari ini waktu tutup klinis hanya tersimpan **pada dokumen yang kebetulan masih konsep** (`MrcClinicalDocumentIntegrity.LockedEncounterClosedAt`). Kunjungan tanpa satu pun konsep tidak meninggalkan jejak waktu tutup sama sekali, padahal `Susulan` dan daftar kerja membutuhkannya untuk **setiap** kunjungan |
| Tindakan | Catat penutupan (dipanggil dari jalur penguncian yang sudah ada) |
| Event | `EVT-RM-01` Kunjungan tutup klinis — memicu penilaian kelengkapan pertama |
| Data lama | Kunjungan yang tutup sebelum konsep ini ada: waktunya **diturunkan** saat diminta, dari sumber yang tersedia (waktu kunci dokumen paling awal, penutupan episode rawat inap, selesai visit IGD, `CompletedAt`). Hanya dipakai untuk penilaian atas permintaan (`RM-DEC-050`). Urutan sumbernya ditetapkan desain |

### `AGG-RM-03` — Penilaian Kelengkapan Kunjungan (`DC-RM-07`)

| Aspek | Isi |
|---|---|
| Root | Kunjungan yang sudah tutup klinis dan tidak batal |
| Batas | Keadaan utama, daftar kekurangan, waktu penilaian, versi aturan yang dipakai, **waktu pertama kali lengkap** |
| Invariant | (1) Keadaan utama = kekurangan berprioritas tertinggi: `Tidak dapat dipastikan` → `Menunggu hasil` → `Belum lengkap` → `Lengkap`. (2) `Lengkap` hanya bila nol kekurangan dan nol gangguan sumber. (3) Tidak menyimpan isi klinis — hanya jenis kekurangan dan rujukan. (4) Tidak menahan apa pun di context lain (`RM-DEC-046`) |
| Tindakan | Nilai ulang satu kunjungan; nilai ulang banyak kunjungan (setelah master berubah atau pemeriksaan rekonsiliasi) |
| Event | `EVT-RM-02` Penilaian berubah keadaan — hanya untuk daftar kerja; tidak ada konsumen di luar rekam medis |
| **Usulan arsitektur — bukan keputusan bisnis** | Penilaian **disimpan sebagai hasil turunan** dan dinilai ulang pada pemicu tertentu, alih-alih dihitung penuh setiap daftar kerja dibuka. Alasan: daftar kerja menyaring dan mengurutkan ribuan kunjungan, sedangkan setiap penilaian memanggil belasan sumber. Ini **tidak** melanggar `RM-DEC-038`: yang disimpan adalah kesimpulan rekam medis sendiri, bukan isi dokumen modul lain. Akibatnya penilaian dapat tertinggal dari sumbernya; karena itu setiap penilaian memuat waktunya, dan membuka satu kunjungan selalu menilai ulang kunjungan itu |
| Pemicu nilai ulang | Tutup klinis (`EVT-RM-01`); addendum pengesahan ditandatangani; catatan ditandatangani; master berubah; dibuka dari daftar kerja; pemeriksaan berkala untuk kunjungan yang belum `Lengkap`. **Modul sumber tidak menerbitkan event "dokumen final"** hari ini, sehingga pemeriksaan berkala adalah satu-satunya cara menangkap dokumen yang final belakangan — lihat `GAP-ARCH-RM-04` |
| Waktu pertama kali lengkap | Disimpan supaya indikator mutu kelengkapan (`NON_BLOCKING_STANDARD`, `RM-RCG-001`) dapat disusun kelak tanpa mengubah struktur |

### `AGG-RM-04` — Aturan Dokumen Wajib (`DC-RM-06`)

| Aspek | Isi |
|---|---|
| Root | Satu aturan |
| Batas | Jenis kunjungan, jenis unit layanan, pemicu bersyarat, kriteria, aktif/nonaktif |
| Invariant | (1) Kriteria wajib berasal dari katalog `DC-RM-06a`. (2) Aturan yang lebih khusus (berunit) **menggantikan** aturan umum untuk jenis kunjungan yang sama — contoh: Rawat Jalan unit Laboratorium memakai aturannya sendiri, bukan daftar Rawat Jalan biasa (`RM-DEC-060`). (3) Pemicu bersyarat berlaku **di atas** aturan jenis kunjungan, bukan menggantikannya. (4) Perubahan tercatat: siapa, kapan, sebelum dan sesudah |
| Tindakan | Tambah, ubah, nonaktifkan aturan |
| Event | `EVT-RM-03` Aturan berubah — memicu penilaian ulang kunjungan yang belum `Lengkap` |

### Addendum pengesahan (`DC-RM-09`) — perluasan aggregate addendum yang ada

Bukan aggregate baru. Addendum yang ada mendapat **jenis**: koreksi atau pengesahan. Seluruh
aturan addendum yang ada tetap berlaku; yang ditambahkan hanya dua invariant: pengesahan hanya
atas catatan `LockedUnsigned`, dan satu catatan cukup satu addendum pengesahan yang
ditandatangani untuk menuntaskan kekurangannya.

---

## F. Model relasi

| Dari | Ke | Makna | Kardinalitas | Wajib | Arah ownership |
|---|---|---|---|---|---|
| Berkas (`DC-RM-01`) | Pasien | Berkas milik satu pasien | 1 : 1 | Ya | Pasien di `BC-PAT` |
| Berkas | Deskriptor (`DC-RM-02`) | Berkas memuat dokumen final | 1 : 0..n | Tidak | Dokumen di context sumber |
| Deskriptor | Kunjungan | Dokumen tertaut kunjungan | n : 0..1 | **Tidak** — golongan darah Bank Darah tidak berkunjungan (`RM-CAP-055`) | Kunjungan di `BC-REG` |
| Penutupan klinis (`DC-RM-05`) | Kunjungan | Kunjungan ditutup klinis | 0..1 : 1 | — | `BC-RM-02` |
| Penilaian (`DC-RM-07`) | Kunjungan | Penilaian atas satu kunjungan | 0..1 : 1 | — | `BC-RM-03` |
| Penilaian | Penutupan klinis | Penilaian hanya ada setelah tutup klinis | 1 : 1 | Ya | — |
| Penilaian | Kekurangan (`DC-RM-08`) | Rincian | 1 : 0..n | — | Bagian dari penilaian |
| Kekurangan | Dokumen atau kriteria | Apa yang kurang | n : 1 | Ya, salah satu | Dokumen di sumber; kriteria di `DC-RM-06a` |
| Aturan (`DC-RM-06`) | Jenis unit layanan | Kunci khusus | n : 0..1 | Tidak — kosong = semua unit | `BC-MST` |
| Aturan | Kriteria | Butir yang dituntut | n : 1 | Ya | `DC-RM-06a` |
| Addendum pengesahan | Catatan `LockedUnsigned` | Menyatakan isi benar | 0..1 bertanda tangan : 1 | — | `BC-RM-02` |

**Penghapusan.** Tidak ada konsep amandemen yang dihapus. Aturan dinonaktifkan, tidak dihapus,
supaya penilaian lama tetap dapat dijelaskan. Penilaian kunjungan yang kemudian batal
dikeluarkan dari daftar kerja, tidak dihapus.

---

## G. Model lifecycle

### Penutupan klinis (`DC-RM-05`)

| Dari | Ke | Pemicu | Pelaku | Catatan |
|---|---|---|---|---|
| (tidak ada) | Tercatat | Panggilan penguncian pertama untuk kunjungan | Context yang menutup | Terminal. Tidak ada pembatalan maupun koreksi (`RM-DEC-051`) |

### Penilaian kelengkapan (`DC-RM-07`)

Keadaan **bukan alur kerja** — ia kesimpulan yang dapat bergerak ke arah mana pun setiap kali
dinilai ulang. Contoh: `Lengkap` dapat turun ke `Tidak dapat dipastikan` bila saat dinilai ulang
satu sumber gangguan, lalu kembali `Lengkap` setelah sumber pulih.

| Keadaan | Kapan |
|---|---|
| `Tidak dapat dipastikan` | Satu atau lebih sumber yang dibutuhkan gagal dibaca |
| `Menunggu hasil` | Ada pemeriksaan tertunda (`DC-RM-13`) |
| `Belum lengkap` | Ada catatan tidak ditandatangani yang belum disahkan, atau ada dokumen wajib yang tidak ada |
| `Lengkap` | Nol kekurangan, nol gangguan |

Kunjungan yang batal **sesudah** dinilai keluar dari daftar kerja. "Waktu pertama kali lengkap"
tidak dihapus walaupun keadaan kemudian turun.

### Addendum pengesahan (`DC-RM-09`)

Mengikuti lifecycle addendum yang ada. Satu-satunya tambahan: begitu addendum pengesahan
**ditandatangani**, kekurangan "tidak ditandatangani" catatan asalnya tuntas. Addendum yang
belum ditandatangani tidak menuntaskan apa pun.

### Aturan dokumen wajib (`DC-RM-06`)

`Aktif` ⇄ `Nonaktif`. Diubah oleh pemegang izin master. Setiap perubahan tercatat dan memicu
`EVT-RM-03`.

---

## H. Tanggung jawab authorization

| Tindakan | Syarat | Dasar |
|---|---|---|
| Membuka berkas dan seluruh isi lintas modul | `MedicalRecord : Read` + pemeriksaan keperluan akses rilis pertama | `RM-DEC-040` (`Existing`) |
| Membaca sumber modul lain dari dalam berkas | **Tanpa** izin modul asal. Karena itu kontrak `DC-RM-03` dipanggil dalam proses dan **tidak boleh** dibuka sebagai jalur HTTP berlingkup pasien yang hanya dijaga izin rekam medis | `RM-DEC-040`, `RM-CAP-058` |
| Melihat daftar kerja kelengkapan | Izin tersendiri — daftar kerja memuat nama pasien dan nama penulis | `CONFIGURABLE_DEFAULT` (`RM-RCG-001`) |
| Mengubah master dokumen wajib | Izin tersendiri | `CONFIGURABLE_DEFAULT` |
| Membuat addendum pengesahan | Penulis asli; atau kepala unit/DPJP bila penulis berhalangan | `RM-DEC-004`, `020`, `049` (`Existing`) |
| Mencatat penutupan klinis | Tidak ada izin pengguna — dicatat oleh context yang sudah berwenang menutup kunjungan | `AGG-RM-02` |

Siapa pemegang izin daftar kerja dan master **tidak ditetapkan** di sini; diberikan lewat
kebijakan akses per departemen-posisi oleh pengelola akses rumah sakit.

---

## I. Audit dan histori

| Kejadian | Yang tercatat | Dasar |
|---|---|---|
| Pembukaan berkas | Satu baris jejak akses (sudah ada), mencakup seluruh sumber | `RM-DEC-040` |
| Penutupan klinis | Kunjungan, waktu, context penutup, pelaku | `AGG-RM-02` |
| Perubahan master dokumen wajib | Pelaku, waktu, nilai sebelum dan sesudah | `NON_BLOCKING_STANDARD` usulan `RM-RCG-001`, dijadikan invariant `AGG-RM-04` |
| Penilaian | Waktu penilaian, versi aturan, waktu pertama kali lengkap | `AGG-RM-03` |
| Addendum pengesahan | Mengikuti jejak addendum yang ada: pembuat, alasan, tanda tangan | `RM-DEC-049` |
| Ralat dokumen sumber | **Tidak** dicatat rekam medis — milik pemiliknya | `RM-DEC-042` |

Histori setiap perubahan keadaan penilaian **tidak** disimpan. Yang disimpan hanya keadaan
terakhir dan waktu pertama kali lengkap — cukup untuk daftar kerja dan indikator mutu kelak.

---

## J. Model integrasi

| ID | Produsen | Konsumen | Tujuan | Sumber kebenaran | Sifat | Saat gagal |
|---|---|---|---|---|---|---|
| `INT-RM-10` | Setiap context sumber (`DC-RM-03`) | `BC-RM-01` | Deskriptor dokumen final per pasien | Context sumber | Sinkron, dalam proses, baca saja | Sumber masuk daftar gangguan (`RM-CAP-034`); bagian lain tetap tampil |
| `INT-RM-11` | `BC-LAB`, `BC-RAD` (`DC-RM-13`) | `BC-RM-03` | Pemeriksaan tertunda per kunjungan | Context sumber | Sinkron, dalam proses | Penilaian `Tidak dapat dipastikan` |
| `INT-RM-12` | Context penutup kunjungan: Registrasi, Rawat Inap, IGD, Billing | `BC-RM-02` | Penguncian + penutupan klinis | `BC-RM-02` mencatat; context penutup memicu | Sinkron, **di dalam transaksi penutupan** — mengikuti pola penguncian yang sudah ada (`InpDischargeService.Closure.cs:835-858`) | Penutupan kunjungan gagal bila pencatatan gagal, sama seperti penguncian hari ini |
| `INT-RM-13` | `BC-OK`, `BC-HMD`, `BC-CLI` | `BC-RM-03` | Pemicu bersyarat: kunjungan memuat operasi/sesi HD/transfusi | Context sumber | Sinkron, dalam proses | `Tidak dapat dipastikan` |
| `INT-RM-14` | `BC-REG`, `BC-MST` | `BC-RM-03` | Jenis kunjungan, unit, jenis unit, status batal | Existing | Baca saja | `Tidak dapat dipastikan` |

**Idempotensi.** Penutupan klinis idempoten per kunjungan. Penilaian ulang idempoten: menilai
dua kali tanpa perubahan sumber menghasilkan keadaan yang sama. Kontrak sumber baca saja.

**Rekonsiliasi.** Pemeriksaan berkala menilai ulang kunjungan yang belum `Lengkap`. Kunjungan
yang sudah `Lengkap` hanya dinilai ulang bila master berubah atau dibuka.

**Kewajiban yang mengikat context lain** (dibawa ke kontrak integrasi masing-masing):

| Context | Kewajiban | Dasar |
|---|---|---|
| Laboratorium | `S6` tidak dirilis sebelum penanda "pernah diralat" tersambung | `RM-DEC-057` |
| Laboratorium | Pemeriksaan PA tertunda tetap dilaporkan lewat `DC-RM-13`, walaupun hasil PA belum tampil | `RM-DEC-058`, `035` |
| Setiap sumber | Menyediakan waktu final; tidak mengembalikan dokumen yang belum final | `RM-DEC-041`, `043` |
| IGD | Jalur disposisi rawat inap memanggil penguncian | `GAP-ARCH-RM-01` |

---

## K. Dampak billing

**Tidak ada dampak charge yang diketahui.** Membaca dokumen, menilai kelengkapan, dan membuat
addendum pengesahan tidak membuat, mengubah, atau membatalkan tagihan. Kelengkapan tidak
menahan finalisasi tagihan (`RM-DEC-046`, `AC-34`). Billing hanya terlibat sebagai **salah
satu pemicu** penguncian dan penutupan klinis yang sudah berjalan (`BillingFinalizationService.cs:898`);
amandemen tidak mengubah perilaku billing.

## L. Dampak keselamatan klinis

**Relevan terhadap keselamatan.** Berkas adalah tempat dokter mengambil keputusan dari riwayat
pasien. Batasnya dibuat eksplisit:

| Risiko | Batas arsitektur |
|---|---|
| Dokter membaca "tidak ada hasil" padahal sumbernya gagal | Invariant (2) dan (3) `AGG-RM-01` |
| Dokter membaca konsep sebagai hasil resmi | Kontrak `DC-RM-03` hanya mengembalikan dokumen final |
| Hasil berubah diam-diam sesudah dibaca | Penanda pernah diralat dari pemilik; PA ditahan; label "dapat berubah" untuk asuhan gizi |
| Hasil yang datang belakangan tidak terlihat sebagai hal baru | Penanda `Susulan` |
| Berkas dinyatakan lengkap padahal satu sumber tidak terbaca | Invariant (2) `AGG-RM-03` |
| Catatan IGD pasien yang lanjut rawat inap tidak pernah dikunci | `GAP-ARCH-RM-01` |

Tidak ada wewenang klinis baru yang dibuat. Wewenang addendum dan tanda tangan tetap milik
keputusan rilis pertama.

---

## M. Gap arsitektur

| ID | Gap | Bukti | Dampak | Pemilik | Menahan |
|---|---|---|---|---|---|
| `GAP-ARCH-RM-01` | **Disposisi IGD ke rawat inap menutup kunjungan IGD tanpa memanggil penguncian.** Lima jalur penutupan lain memanggil `LockOpenDocumentsForEncounterAsync`; jalur ini tidak | `EmergencyDispositionService.AdmissionRequest.cs:1585-1598` (menyetel `Completed` dan `CompletedAt`); nol pemanggilan penguncian di berkas `EmergencyDispositionService*.cs`. Jalur yang memanggil: `BillingFinalizationService.cs:898`, `EmergencyVisitService.cs:689`, `InpDischargeService.Closure.cs:853`, `DoctorQueueController.cs:610`, `NurseStationQueueController.cs:356`, `PatientEncounterController.cs:1674` | Catatan IGD pasien yang dirawat inap tetap `Draft` selamanya — melanggar `RM-DEC-003` hari ini. Kunjungan itu juga tidak akan punya penutupan klinis, sehingga tidak pernah masuk daftar kerja | Blueprint IGD, berkoordinasi dengan rekam medis | **Implementasi** S-A6 dan S-B2 untuk kunjungan IGD yang lanjut rawat inap. Tidak menahan desain |
| `GAP-ARCH-RM-02` | Hasil Patologi Anatomi | `RM-FACT-020` | — | Blueprint Laboratorium (open question 18) | Desain S-A1b |
| `GAP-ARCH-RM-03` | Arti final disposisi IGD, asuhan gizi, catatan kelahiran masih sementara | `RM-DEC-059` | Bila berubah, cukup mengganti satu kriteria di `DC-RM-06a` | Pemilik IGD, Gizi, Kamar Operasi | Tidak menahan |
| `GAP-ARCH-RM-04` | Modul sumber tidak menerbitkan event "dokumen final" | Nol event semacam itu ditemukan pada audit revision 3 | Penilaian dapat tertinggal sampai pemeriksaan berkala berikutnya. Desain wajib menetapkan jarak pemeriksaan dan menampilkan waktu penilaian | Rekam medis (desain) | Tidak menahan |
| `GAP-ARCH-RM-05` | Beban baca: satu pembukaan berkas memanggil sekitar lima belas sumber | `RM-DEC-038` | Desain wajib menetapkan batas waktu per sumber dan pembacaan serentak; sumber yang lewat batas waktu diperlakukan sebagai gangguan | Rekam medis (desain) | Tidak menahan |
| `GAP-ARCH-RM-06` | Jalur baca berlingkup pasien belum ada di Laboratorium dan sebagian besar modul | Blocker 7 dan 9; `RM-CAP-069`, `070` | Arsitektur ini menetapkan bentuknya (`DC-RM-03`); pengisiannya pekerjaan tiap context | Blueprint masing-masing modul | **Implementasi** per sumber, satu per satu |
| `GAP-ARCH-RM-07` | Dua penanda diagnosis utama | `RM-DEC-054` | Kriteria membaca keduanya | `ClinicalManagement` | Tidak menahan |

---

## N. Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_PARTIAL`.**

| Slice | Kesiapan | Catatan |
|---|---|---|
| S-A1, S-A2, S-A3, S-A4, S-A5, S-A6, S-A7, S-B1, S-B2, S-B3 | **Siap** untuk `/design-business-module` | Seluruh ownership terselesaikan; tidak ada kebijakan yang dikarang. `GAP-ARCH-RM-01` dan `06` menahan **implementasi** sebagian sumber, bukan desain |
| S-A1b | **Berhenti** | Open question 18 |

Kesepuluh slice siap **independen** dari S-A1b: kontrak sumber, penilaian, dan master tidak
bergantung pada tampil-tidaknya hasil PA. Satu-satunya titik sentuhnya — pemeriksaan PA
tertunda tetap dihitung pada syarat 2 — sudah dirancang (`DC-RM-13`).

### Handoff ke `/design-business-module`

| Field | Nilai |
|---|---|
| `blueprint_id` / revision | `RM-BP-001` / `3` → naik ke `4` |
| `domain_architecture_revision` | `RM-DA-001` revision 1 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL` |
| `domain_architecture_scope` | Sepuluh slice di atas; S-A1b dikecualikan |
| `requirement_readiness` | `RM-RCG-001` revision 2 |
| `decision_revision` | `00-interview-decisions.md` revision 9 |
| `contract_versions` | API `0.1.1` (rilis pertama, tetap sah); amandemen akan menaikkannya |
| `backend_source_sha` / `frontend_source_sha` | `363de34f` / `f0a3b07b8` |
| Yang wajib dipertajam desain | Kontrak `DC-RM-03` per sumber; katalog kriteria `DC-RM-06a`; urutan sumber waktu tutup klinis untuk data lama; jarak pemeriksaan berkala (`GAP-ARCH-RM-04`); batas waktu per sumber (`GAP-ARCH-RM-05`); bentuk izin baru |
| Yang **tidak** boleh diubah desain | Makna final milik pemilik sumber; larangan menyalin isi; satu izin berkas; tutup klinis tidak berubah; kelengkapan tidak menahan apa pun |

## Riwayat

| Revision | Tanggal | Ringkasan |
|---:|---|---|
| 1 | 28 Oktober 2026 | Arsitektur domain pertama untuk amandemen. Tiga bounded context rekam medis (satu baru: Kelengkapan Berkas), empat aggregate, empat belas konsep. Dua konsep baru yang paling menentukan: **Penutupan Klinis Kunjungan** (satu-satunya sumber waktu tutup klinis, karena hari ini waktu itu hanya tersimpan pada dokumen konsep) dan **Penilaian Kelengkapan** sebagai hasil turunan tersimpan. Gap baru `GAP-ARCH-RM-01`: disposisi IGD ke rawat inap tidak mengunci catatan IGD |
