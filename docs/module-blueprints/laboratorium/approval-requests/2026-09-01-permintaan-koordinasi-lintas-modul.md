# Permintaan Koordinasi Lintas Modul — Modul Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-001` |
| `tanggal` | 2026-09-01 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium |
| `rujukan` | `blueprint-manifest.md` revision `9`; `04-prd-to-mvp.md` bagian 15 |
| `status` | `dijawab sebagian` — lihat bagian 0 |
| `disetujui oleh` | `andryzainhome` (`andryzain01@gmail.com`) dan `sukmagp` — Sukma Giri Pratama (`sukmagiri11@gmail.com`), selaku pemilik repository |
| `tanggal persetujuan` | 2026-09-01 |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

Dokumen ini dapat diteruskan apa adanya. Setiap bagian berdiri sendiri: penerima cukup membaca
bagian yang menyebut modulnya.

---

## 0. Hasil — Apa yang Sudah Terjawab dan Apa yang Belum

Persetujuan diberikan `andryzainhome` dan `sukmagp` pada 2026-09-01, disampaikan lewat pemilik
modul Laboratorium.

### 0.1 Disetujui — lima butir

| No | Yang diminta | Akibat |
|---:|---|---|
| 1 | Kolom disiplin pada `MstProcedure` | `LAB-COORD-005` **ditutup**. `MVP-0` tidak lagi terhalang |
| 2 | Dua data induk perujuk | `LAB-COORD-004` bagian data induk **ditutup** |
| 3 | Kolom penunjuk perujuk pada kunjungan + kontrak pemanggilan Registrasi | `LAB-COORD-004` bagian kunjungan dan `LAB-COORD-003` **ditutup**. `MVP-1` tidak lagi terhalang |
| 7 | Pemberitahuan sebagai kemampuan platform | `LAB-COORD-001` **ditutup** |
| 8 | Satu jenis dokumen klinis baru pada `rekam-medis` | `LAB-COORD-002` **ditutup** |

### 0.2 Belum terjawab — dua butir yang membutuhkan **jawaban**, bukan persetujuan

| No | Yang dibutuhkan | Kenapa persetujuan belum cukup |
|---:|---|---|
| 4 | Jumlah baris `TrxLabSpecimen` di basis data produksi | Yang diminta adalah **satu angka**, bukan izin. Menyetujui permintaan tidak memberi tahu berapa barisnya. Migration `MVP-2` tetap tidak boleh dijalankan sebelum angkanya diketahui |
| 5 | ~~Lokasi `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`~~ | **TERJAWAB 2026-09-01** — lihat 3.2. Keduanya **masih berlaku** dan berada di `QuilvianEngineeringSkills/agents/rules/backend/engineering/`. `LAB-OPEN-002` ditutup oleh `LAB-FACT-007` |

`LAB-OPEN-012` tetap terbuka. `LAB-OPEN-002` ditutup, tetapi menurunkan dua penghambat baru
`LAB-OPEN-018` dan `LAB-OPEN-019` yang keduanya masih memerlukan tindakan pemilik repository
backend — lihat 3.2.

### 0.3 Di luar wewenang pemberi persetujuan — satu butir

| No | Yang diminta | Kenapa tidak dapat ditutup |
|---:|---|---|
| 6 | Tanda tangan klinis atas `LAB-DEC-003`, `LAB-DEC-004`, `LAB-DEC-007` | `LAB-DEC-011` — yang **disetujui pemilik modul Laboratorium sendiri** — menyatakan bahwa wewenang klinis berada di pihak lain, dan ketiganya memerlukan tanda tangan **dokter penanggung jawab laboratorium atau Komite Medis** sebelum desain final |

`andryzainhome` dan `sukmagp` adalah pemilik repository, bukan wewenang klinis. Menutup
`LAB-SIGN-001` atas persetujuan mereka justru melanggar keputusan yang dibuat pemilik modul
Laboratorium sendiri.

**Yang membuat ini penting, bukan sekadar formalitas.** Ketiga keputusan itu menentukan siapa
yang boleh menyatakan sebuah angka hasil benar, apa yang terjadi ketika pasien dalam bahaya,
dan apa yang terjadi ketika hasil yang sudah dipakai ternyata salah. Bila kelak terjadi
insiden, rumah sakit perlu menunjukkan bahwa pihak klinis ikut memutuskan.

`LAB-SIGN-001` tetap terbuka. Seluruh slice hasil pemeriksaan tetap tertahan.

> **Bila keliru:** apabila salah satu dari `andryzainhome` atau `sukmagp` memang memegang
> wewenang klinis — misalnya merangkap dokter penanggung jawab laboratorium — cukup nyatakan
> hal itu, dan `LAB-SIGN-001` akan ditutup dengan nama yang bersangkutan sebagai penanda tangan
> klinis.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Modul Laboratorium sudah punya blueprint lengkap — 36 keputusan disetujui, arsitektur domain
siap, dan seluruh kontrak tersusun. **Pekerjaan tetap tidak bisa dimulai** karena tujuh hal
berada di luar wewenang modul Laboratorium: tiga menyentuh tabel milik modul lain, satu
memerlukan tanda tangan klinis, satu memerlukan pemeriksaan basis data, satu memerlukan
kesepakatan platform, dan satu berupa dokumen tata kelola yang hilang dari repository.

Yang diminta **bukan** persetujuan desain. Desainnya sudah selesai. Yang diminta adalah izin
menyentuh milik orang lain, dan jawaban atas hal yang memang bukan urusan Laboratorium.

---

## 2. Prioritas 1 — memblokir gelombang `MVP-0` dan `MVP-1`

Ketiganya menyentuh tabel yang **bukan milik Laboratorium**. Tanpa izin, Laboratorium tidak
dapat memulai gelombang pertama sama sekali.

### 2.1 Pemilik `master-data` — kolom disiplin pada `MstProcedure`

| Butir | Isi |
|---|---|
| **Yang diminta** | Izin menambah **satu kolom** klasifikasi disiplin pada `MstProcedure`, dan pengisian nilainya untuk pemeriksaan berpenanda `IsLaboratory` yang sudah ada |
| **ID koordinasi** | `LAB-COORD-005` |
| **Yang diblokir** | Gelombang `MVP-0`, `EPIC-LAB-09` katalog dan harga |
| **Akibat nyata bila tidak ada** | Sistem tidak dapat memeriksa apakah pemeriksaan yang dipilih sesuai disiplin pesanannya. Petugas dapat memasukkan Hemoglobin ke pesanan Mikrobiologi, dan sistem tidak akan menolaknya |
| **Keputusan yang mendasari** | `LAB-DEC-036` |
| **Tabel terdampak** | `MstProcedure` |

**Kenapa satu kolom ini boleh, sementara kolom lain tidak.** `MstProcedure` sudah punya
`IsLaboratory`, `IsRadiology`, `IsSurgery`, dan `IsTherapy` — seluruhnya **klasifikasi** jenis
tindakan. Yang diminta sejenis dengan itu: pembeda Patologi Klinik, Patologi Anatomi, dan
Mikrobiologi.

Yang **tidak** diminta dan memang tidak boleh masuk: satuan hasil, batas nilai, jenis wadah.
Seluruhnya berada di tabel milik Laboratorium sendiri.

### 2.2 Pemilik `master-data` — dua data induk perujuk

| Butir | Isi |
|---|---|
| **Yang diminta** | Dua data induk baru: **instansi perujuk** dan **dokter perujuk** |
| **ID koordinasi** | `LAB-COORD-004` |
| **Yang diblokir** | Gelombang `MVP-1`, `EPIC-LAB-08` pendaftaran pasien rujukan luar |
| **Akibat nyata bila tidak ada** | Nama klinik perujuk hanya dapat diketik bebas. "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan "sehat sentosa" akan terhitung sebagai tiga institusi berbeda. Laporan dokter pengirim tidak akan pernah dapat dipercaya |
| **Keputusan yang mendasari** | `LAB-DEC-035` |

**Isi minimum yang dibutuhkan:**

| Data induk | Isi |
|---|---|
| Instansi perujuk | Nama klinik atau rumah sakit, alamat, telepon, penanda aktif |
| Dokter perujuk | Nama dokter, tertaut ke instansinya, penanda aktif |

**Kenapa global, bukan milik Laboratorium.** Rujukan bukan hal khusus laboratorium. Kunjungan
pasien sudah punya penanda `IsReferral` sejak awal, dan Rawat Jalan maupun IGD juga menerima
pasien rujukan. Menaruhnya di Laboratorium berarti modul lain kelak membuat daftar tandingan.

### 2.3 Pemilik `registration-management` — dua hal sekaligus

| Butir | Isi |
|---|---|
| **Yang diminta** | **(a)** Kolom penunjuk instansi dan dokter perujuk pada `TrxPatientEncounter`. **(b)** Kesepakatan kontrak pemanggilan: Laboratorium meminta Registrasi membuat kunjungan |
| **ID koordinasi** | `LAB-COORD-004` untuk (a), `LAB-COORD-003` untuk (b) |
| **Yang diblokir** | Gelombang `MVP-1`, `EPIC-LAB-08` seluruhnya |
| **Akibat nyata bila tidak ada** | Pasien yang datang langsung ke laboratorium **tidak dapat dilayani sama sekali**. Ia harus mengantre di loket pendaftaran lebih dulu, padahal ia hanya perlu satu pemeriksaan darah |
| **Keputusan yang mendasari** | `LAB-DEC-032`, `LAB-DEC-035` |
| **Tabel terdampak** | `TrxPatientEncounter` |

**Yang perlu ditegaskan: Laboratorium tidak akan menulis ke tabel kunjungan.** Rancangannya
justru sebaliknya — layar pendaftaran berada di modul Laboratorium supaya petugas tidak
berpindah aplikasi, tetapi **Registrasi yang membuat kunjungannya**. Laboratorium mengirim
isian, menunggu jawaban, lalu menyimpan penunjuk kunjungan yang dikembalikan.

**Kabar baiknya: sebagian besar sudah ada.** Pemeriksaan pada `9124900` menemukan Registrasi
sudah memiliki `EncounterRegistrationSource.WalkIn`, kolom `IsWalkIn`, penanda `IsReferral`,
`ReferralNumber`, `IsReferralRequired`, `IsReferralVerified`, dan `PatientEncounterController`
yang sudah menangani pembuatan kunjungan datang langsung. Yang belum ada hanya kolom penunjuk
perujuk dan kesepakatan bentuk pemanggilannya.

**Yang perlu disepakati pada kontrak pemanggilan:**

| Aspek | Catatan |
|---|---|
| Bentuk permintaan dan jawaban | Isian pendaftaran masuk, penunjuk kunjungan keluar |
| Idempotensi | **Wajib.** Petugas menekan Simpan dua kali tidak boleh menghasilkan dua kunjungan untuk satu pasien pada hari yang sama |
| Perilaku saat ditolak | Penolakan diteruskan apa adanya. Laboratorium tidak menyimpan data setengah jadi |

---

## 3. Prioritas 2 — memblokir gelombang `MVP-2`

### 3.1 Pemilik repository backend atau DBA — jumlah data laboratorium

| Butir | Isi |
|---|---|
| **Yang diminta** | Jawaban satu angka: **berapa baris `TrxLabSpecimen` yang ada di basis data produksi?** |
| **ID koordinasi** | `LAB-OPEN-012` |
| **Yang diblokir** | Gelombang `MVP-2`, migration pemisahan wadah dan pemeriksaan |
| **Akibat nyata bila tidak dijawab** | Migration yang mengubah struktur tabel berjalan tanpa mengetahui berapa banyak data pasien yang terdampak |

**Kenapa ini penting dan mudah.** Frontend Laboratorium tidak ada sama sekali pada
`688daff90`, sehingga besar kemungkinan belum ada data pasien sungguhan. Bila benar nol,
seluruh kerumitan pemindahan data gugur dan migration menjadi biasa. Tetapi itu **dugaan,
bukan bukti** — dan mengubah struktur tabel berdasarkan dugaan tidak dapat diterima.

Yang dibutuhkan hanya satu perhitungan baris.

### 3.2 Pemilik repository backend — dokumen tata kelola yang hilang

| Butir | Isi |
|---|---|
| **Yang diminta** | Lokasi `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| **ID koordinasi** | `LAB-OPEN-002` |
| **Yang diblokir** | **Seluruh implementasi backend**, gelombang mana pun |
| **Akibat nyata bila tidak ada** | `AGENTS.md` menyatakan kedua dokumen itu berwenang atas implementasi backend, tetapi folder `docs/engineering/` dan `.codex/` tidak ditemukan pada `9124900`. Implementer tidak punya kontrak yang harus diikuti |

Bila kedua dokumen memang sudah tidak berlaku, yang diminta adalah pernyataan itu secara
tertulis, agar `AGENTS.md` dapat diperbarui.

#### Jawaban — 2026-09-01

| Butir | Isi |
|---|---|
| **Status** | **TERJAWAB.** `LAB-OPEN-002` ditutup oleh `LAB-FACT-007` |
| **Keduanya masih berlaku?** | **Ya.** Tidak dicabut. `AGENTS.md` tetap menempatkannya pada urutan wewenang ke-2 dan ke-3, dan `QBE-MOD-002`/`QBE-MOD-003`/`QBE-NAM-004` masih dikutip aktif oleh blueprint lain |
| **Lokasi canonical** | `QuilvianEngineeringSkills/agents/rules/backend/engineering/` — sumber lintas vendor |
| **Lokasi edisi Claude** | `QuilvianEngineeringSkills/Claude/.claude/rules/backend/engineering/` — identik byte-per-byte |
| **Kenapa dulu tidak ketemu** | Kedua dokumen dipindahkan keluar dari repository backend ke repository suite Skill. Path `docs/engineering/` yang masih disebut `AGENTS.md` baris 11 dan 20 sudah usang dan bertentangan dengan baris 40 pada berkas yang sama |

**Dua penghambat baru yang muncul dari jawaban ini — masih memerlukan tindakan:**

| ID | Isi | Tindakan yang diminta |
|---|---|---|
| `LAB-OPEN-018` | Rules root yang **terpasang** (`${CLAUDE_PLUGIN_ROOT}/.claude/rules/`) tidak memuat subfolder `engineering/`. Plugin terpasang berasal dari `MHamzah1/QuilvianEngineeringSkillsClaude@f0136df`, bukan dari sumber canonical `DevBenari/QuilvianEngineeringSkills@59bd3e2` | Publikasikan/perbarui suite Skill sehingga rules root runtime memuat kedua dokumen. Selama belum, gerbang `AGENTS.md` sendiri memaksa setiap task backend berhenti dengan `BLOCKED — canonical governance unavailable` |
| `LAB-OPEN-019` | Registry mencatat `HealthServices / LaboratoryManagement / Laboratory`, prefix `Lab`, lifecycle `PLANNED`. Hak penamaan sudah ada, izin implementasi belum | Naikkan lifecycle `PLANNED` → `ACTIVE`, dengan preseden `RWI-DEC-068` untuk `InPatientManagement` |

**Rapikan juga:** `AGENTS.md` backend perlu diperbarui agar baris 11 dan 20 tidak lagi menunjuk
`docs/engineering/`, dan folder peninggalan `agents/rules/` (7 berkas) yang sudah dinyatakan
dicabut oleh `AGENTS.md` baris 53 perlu dibereskan.

---

## 4. Prioritas 3 — memblokir seluruh slice hasil pemeriksaan

Bagian ini tidak memblokir `MVP-0` sampai `MVP-4`, tetapi memblokir kelanjutannya. Diajukan
sekarang agar dapat berjalan paralel.

### 4.1 Dokter penanggung jawab laboratorium atau Komite Medis

| Butir | Isi |
|---|---|
| **Yang diminta** | Tanda tangan atas tiga keputusan keselamatan pasien |
| **ID koordinasi** | `LAB-SIGN-001` |
| **Yang diblokir** | Seluruh pengisian, validasi, rilis, nilai kritis, dan koreksi hasil |

| Keputusan | Isi | Kenapa perlu tanda tangan klinis |
|---|---|---|
| `LAB-DEC-003` | Pengisi hasil tidak boleh memvalidasi hasil yang sama, dengan jalur pengecualian bertanda permanen | Menentukan siapa yang boleh menyatakan sebuah angka hasil benar |
| `LAB-DEC-004` | Nilai kritis tetap dirilis, pelaporan wajib tercatat | Menentukan apa yang terjadi ketika pasien dalam bahaya |
| `LAB-DEC-007` | Koreksi hasil hanya oleh petugas berwenang validasi, dokter otomatis diberi tahu | Menentukan apa yang terjadi ketika hasil yang sudah dipakai ternyata salah |

Pemilik modul sudah menyetujui ketiganya dari sisi produk dan operasional lewat `LAB-DEC-011`,
sekaligus menyatakan bahwa wewenang klinis berada di pihak lain.

### 4.2 Pemilik platform — kemampuan pemberitahuan bersama

| Butir | Isi |
|---|---|
| **Yang diminta** | Kesepakatan bahwa pemberitahuan tersimpan dibangun sebagai kemampuan platform, bukan milik Laboratorium |
| **ID koordinasi** | `LAB-COORD-001` |
| **Yang diblokir** | Nilai kritis dan pemberitahuan koreksi hasil |
| **Akibat nyata bila dibangun di Laboratorium** | Ketika Farmasi dan Radiologi kelak membutuhkan hal serupa, dokter harus memeriksa tiga kotak pemberitahuan berbeda. Untuk nilai kritis, itu berbahaya |

Pemeriksaan pada `9124900` menemukan platform **belum punya** sarana pemberitahuan apa pun —
tidak ada tabel notifikasi, tidak ada surel, tidak ada pesan singkat. Yang ada hanya
`Hubs/QueueHub.cs` yang khusus melayani antrean nurse station.

### 4.3 Pemilik `rekam-medis` — jenis dokumen klinis baru

| Butir | Isi |
|---|---|
| **Yang diminta** | Izin menambah satu nilai pada daftar jenis dokumen klinis, untuk hasil laboratorium |
| **ID koordinasi** | `LAB-COORD-002` |
| **Yang diblokir** | Pendaftaran hasil ke rekam medis, koreksi hasil setelah kunjungan ditutup, penautan berkas hasil eksternal |
| **Akibat nyata bila tidak ada** | Hasil laboratorium tidak terlihat sebagai bagian berkas rekam medis pasien. Saat akreditasi atau sengketa, hasil yang tidak tercatat di rekam medis sulit dipertanggungjawabkan |

**Yang tidak diminta:** menyalin isi hasil ke tabel rekam medis. Angka hasil tetap disimpan
Laboratorium. Yang didaftarkan hanya keutuhannya — siapa penulisnya, kapan ditandatangani, dan
kapan terkunci.

Mekanisme koreksi untuk dokumen terkunci **sudah tersedia** lewat addendum, sehingga tidak ada
kemampuan baru yang perlu dibangun di modul `rekam-medis`.

---

## 5. Ringkasan yang Diminta

| No | Kepada | Yang diminta | Memblokir |
|---:|---|---|---|
| 1 | Pemilik `master-data` | Satu kolom disiplin pada `MstProcedure` | `MVP-0` |
| 2 | Pemilik `master-data` | Dua data induk perujuk | `MVP-1` |
| 3 | Pemilik `registration-management` | Kolom penunjuk perujuk + kontrak pemanggilan | `MVP-1` |
| 4 | Pemilik repository backend / DBA | Jumlah baris `TrxLabSpecimen` di produksi | `MVP-2` |
| 5 | Pemilik repository backend | Lokasi dua dokumen tata kelola | Seluruh implementasi |
| 6 | Dokter PJ laboratorium / Komite Medis | Tanda tangan tiga keputusan keselamatan | Seluruh slice hasil |
| 7 | Pemilik platform | Kesepakatan pemberitahuan sebagai kemampuan platform | Nilai kritis |
| 8 | Pemilik `rekam-medis` | Satu jenis dokumen klinis baru | Hasil ke rekam medis |

**Nomor 1 sampai 3 adalah yang paling mendesak.** Tanpa ketiganya, gelombang pertama tidak
dapat dimulai sama sekali. Ketiganya juga yang paling ringan — satu kolom, dua daftar, dan satu
kesepakatan bentuk pemanggilan.

**Nomor 4 dan 5 murah dijawab.** Yang satu satu perhitungan baris, yang satu lagi keterangan
lokasi berkas.

---

## 6. Yang Tidak Diminta

Agar tidak disalahpahami, berikut yang **bukan** bagian permintaan ini:

| Bukan yang diminta | Keterangan |
|---|---|
| Persetujuan atas rancangan Laboratorium | Sudah disetujui pemilik modulnya |
| Izin menulis ke tabel modul lain | Laboratorium **tidak akan** menulis. Ia memanggil, membaca, dan menyajikan |
| Perubahan cara kerja modul lain | Tidak ada alur modul lain yang berubah |
| Penambahan kolom operasional pada `MstProcedure` | Hanya satu kolom klasifikasi. Satuan, batas nilai, dan jenis wadah tetap di tabel Laboratorium |
| Pemindahan data tarif | Tarif tetap milik Master Data. Laboratorium hanya menyajikannya, baca saja |

---

## 7. Riwayat

| Tanggal | Perubahan | Status |
|---|---|---|
| 2026-09-01 | Permintaan disusun dari `blueprint-manifest.md` revision 9 | `draft`, belum dikirim |
| 2026-09-01 | Butir 5 terjawab lewat penelusuran repository: kedua dokumen tata kelola ditemukan di `QuilvianEngineeringSkills/agents/rules/backend/engineering/` dan **masih berlaku**. `LAB-OPEN-002` ditutup oleh `LAB-FACT-007`; `LAB-OPEN-018` dan `LAB-OPEN-019` dibuka sebagai penghambat implementasi penggantinya | `draft` |
