# Laporan Perubahan Backend — `BE-RWI-065`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-065` |
| Judul | Pengkajian keperawatan yang selesai ikut terkunci seperti dokumen dokter |
| Slice | Gelombang `KEP-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-008`, `FR-KEP-009`; `RWI-DEC-091`, `RWI-DEC-086`, `RWI-DEC-087`, `RWI-FACT-016`; `RWI-AC-175`; `INT-KEP-06` |
| Contract version | Integration `0.3.0` `INT-KEP-06` bagian 7.1; state transition `0.3.0` bagian 1 |
| Dependency | `BE-RWI-056` — 🟡 sebagian, tidak menahan task ini |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah ≤ 3 (0), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), hanya perilaku persistence yang sudah ada (1), keamanan berkaitan tetapi bukan intinya (1), workflow terbatas (1). Total **5** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962`, branch `MHamzah`; commit `0a63358` dibuat pemilik repository di tengah pengerjaan |
| Tanggal | 6 September 2026 |
| Status | **Selesai.** Keenam acceptance criteria terbukti. **Nol perubahan bentuk data**; migration task ini kosong |

---

## 1. Masalah yang diperbaiki

Mesin keutuhan dokumen klinis milik `MedicalRecordManagement` sudah ada, sudah menegakkan jenis
`Assessment`, dan sudah dipakai kajian medis milik DPJP. Yang belum ada adalah **pendaftaran
pengkajian keperawatan** ke mesin itu. Source menyatakannya sendiri, apa adanya, sebelum task ini:

> *"Sengaja hanya untuk kajian medis. Pendaftaran pengkajian keperawatan adalah pekerjaan sub-modul
> keperawatan; menyalakannya dari sini akan mengubah perilaku jalur poliklinik dan IGD yang tidak
> diminta task ini."*
> — `PatientAssessmentController.cs`

Akibatnya, pengkajian perawat yang sudah diselesaikan berada dalam keadaan yang paling buruk dari
dua dunia:

1. **Tidak terkunci.** Isinya masih dapat disunting langsung lewat `PUT /{id}`, tanpa jejak alasan
   dan tanpa nama pengubahnya tercatat sebagai koreksi.
2. **Tidak dapat dikoreksi.** Mesin koreksi menolak dokumen yang belum terdaftar, sehingga jalur
   koreksi beralasan pun tertutup.

> **Contoh nyatanya.** Ns. Sari menyelesaikan pengkajian awal Tn. Budi pukul 09.00. Pukul 14.00
> seseorang membuka pengkajian itu dan mengubah skala nyerinya dari 7 menjadi 3. Rekam medis tidak
> memuat satu pun jejak bahwa angka itu pernah 7, siapa yang mengubahnya, maupun alasannya. Itulah
> keadaan yang ditemukan `RWI-FACT-014` pada dokumen dokter dan yang ditutup task ini untuk perawat.

---

## 2. Proses bisnis

**Tujuan.** Satu lembar rekam medis tidak memuat dua bentuk penguncian. Pengkajian perawat terkunci
pada mesin yang sama dengan dokumen dokter.

**Pelaku.** Perawat pelaksana sebagai penulis dan penanda tangan pengkajiannya sendiri.

**Langkah yang berurutan.**

1. Perawat mengisi pengkajian. Selama masih dikerjakan, ia dapat menyuntingnya seperti biasa.
2. Perawat menyelesaikan pengkajian.
3. Pada **penyimpanan yang sama**, sistem mendaftarkan pengkajian itu ke mesin keutuhan sebagai
   dokumen berjenis `Assessment` yang **sudah tertanda tangan**, dengan penulis pengkajian sebagai
   penanda tangannya.
4. Sejak saat itu, penyuntingan langsung ditolak dan diarahkan memakai koreksi.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Pendaftaran ke mesin keutuhan gagal | **Penyelesaian ikut batal.** Pengkajian tetap belum selesai; tidak ada baris keutuhan yang menggantung |
| Penulis pengkajian tidak dapat ditentukan | Pendaftaran ditolak — dokumen bertanda tangan tanpa penanda tangan bukan bukti apa pun — sehingga penyelesaiannya ikut batal |
| Menyunting langsung pengkajian yang sudah terkunci | Ditolak `400`: *"Catatan ini sudah ditandatangani dan tidak dapat diubah. Gunakan addendum untuk membetulkan."* |
| Pengkajian masih `Draft` atau `InProgress` | Tetap dapat disunting seperti biasa |
| Pengkajian poliklinik, medical check-up, atau IGD | **Tidak didaftarkan**, dan penolakan penyuntingannya tetap kalimat lama |

> **Kenapa pendaftaran dan penyelesaian wajib satu transaksi.** Bila dipisah menjadi dua langkah,
> akan lahir jendela waktu ketika pengkajian sudah `Completed` tetapi belum punya baris keutuhan.
> Dokumen seperti itu **tidak dapat dikoreksi selamanya** — persis keadaan yang sedang ditutup.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | `RegisterSignedAsync`, `EnsureMutableAsync`, dan daftar jenis yang ditegakkan |
| `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | Nilai `Assessment` yang sudah ada |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Jalur `PATCH /{id}/complete` dan `PUT /{id}` beserta pola transaksi milik `BE-RWI-038` |
| `contracts/state-transition-matrix.md` bagian 1 dan bagian 4 | Syarat teknis yang wajib dibaca sebelum dibangun |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/ClinicalDocumentFinalizationIntegrityTests.cs` | Pola uji keutuhan milik `BE-RWI-038` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Syarat pendaftaran melebar dari `isKajianMedis` menjadi `isKajianMedis \|\| entity.InpEpisodeId.HasValue`; kalimat penolakan kegagalan pendaftaran menyesuaikan jenis dokumennya; `EnsureMutableAsync` dipasang pada `PUT /{id}` **sebelum** pemeriksaan status lama |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentIntegrityTests.cs` | **Berkas baru**, dipakai bersama `BE-RWI-057`. Enam uji di antaranya milik task ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol endpoint baru, nol field baru. Satu perubahan perilaku pada `PUT /{id}`: pengkajian rawat inap yang sudah terkunci kini ditolak beserta arahan memakai koreksi, bukan kalimat teknis. `PATCH /{id}/complete` kini mengembalikan `IsRegisteredToIntegrity` bernilai benar untuk pengkajian keperawatan rawat inap |
| Database | **`NOT APPLICABLE`.** Nol tabel, nol kolom, nol index, nol nilai enum. **Migration task ini kosong** — tidak ada berkas migration yang dibuat, dan itu memang yang benar |
| Keamanan/Auth | `NOT APPLICABLE` — nol atribut hak akses ditambah atau diubah. Penguncian dokumen bukan hak akses melainkan kelayakan status, dan itu memang tugas kode (`role-access-rules.md` bagian 6) |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/complete` | Menyelesaikan pengkajian. Untuk pengkajian rawat inap, sekaligus mendaftarkannya sebagai dokumen `Assessment` tertanda tangan dalam penyimpanan yang sama | `PatientAssessment : Update` |
| `PUT` | `/{id}` | Menyunting isi pengkajian. Kini menolak dokumen yang sudah terkunci beserta arahan memakai koreksi | `PatientAssessment : Update` |

**Nol endpoint baru dibuat task ini.**

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| `PengkajianKeperawatanSelesai_TerdaftarSebagaiDokumenTertandaTangan` | Jenis `Assessment`, status `Signed`, penulis = penanda tangan, `LockTrigger` = `AuthorSigned` | `PASS` | Uji |
| **Test yang memaksa pendaftaran gagal**: `PendaftaranKeutuhanGagal_PenyelesaianIkutBatal` | `400`; pengkajian **tetap belum selesai**; `CompletedAt` kosong; nol baris keutuhan | `PASS` | Uji |
| `MenyuntingPengkajianTerkunci_Ditolak400DenganArahanKoreksi` | `400`; pesannya menyebut addendum; isi pengkajian tidak berubah | `PASS` | Uji |
| `PengkajianBelumSelesai_TetapDapatDisunting` | `200`; isinya berubah seperti biasa | `PASS` | Uji |
| **Test yang menghitung nilai enum**: `NolNilaiEnumBaru_PadaJenisDokumenKlinis` | `ClinicalDocumentKind` tetap 13 nilai; `Assessment` dan `Procedure` sudah ada sebelumnya | `PASS` | Uji |
| **Regresi poliklinik**: `PengkajianPoliklinikSelesai_TidakDidaftarkanDanTetapDapatDisunting` | Nol baris keutuhan; penolakan penyuntingannya tetap kalimat lama persis | `PASS` | Uji |
| **Regresi kajian medis**: seluruh `MedicalAssessmentTests` dan `ClinicalDocumentFinalizationIntegrityTests` | Lulus tanpa perubahan perilaku | `PASS` | Bagian dari 394 uji yang lulus |
| Pemeriksaan migration task ini kosong | Nol berkas migration dibuat untuk task ini | `PASS` | Bagian 3.3; daftar migration pada bagian 7 |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Menyelesaikan pengkajian keperawatan mendaftarkannya pada mesin keutuhan dengan jenis `Assessment`, penulis sebagai penanda tangan | Terpenuhi | Uji `PengkajianKeperawatanSelesai_TerdaftarSebagaiDokumenTertandaTangan` |
| 2. Bila pendaftaran gagal, penyelesaian ikut batal | Terpenuhi | Uji `PendaftaranKeutuhanGagal_PenyelesaianIkutBatal` — membuktikan pengkajian tetap belum selesai **dan** nol baris keutuhan terbentuk |
| 3. Menyunting langsung pengkajian `Completed` ditolak `400` beserta arahan memakai koreksi | Terpenuhi | Uji `MenyuntingPengkajianTerkunci_Ditolak400DenganArahanKoreksi` |
| 4. Nol nilai enum baru pada `ClinicalDocumentKind` | Terpenuhi | Uji `NolNilaiEnumBaru_PadaJenisDokumenKlinis` |
| 5. Perilaku kajian medis, poliklinik, dan IGD tidak berubah | Terpenuhi | Uji `PengkajianPoliklinikSelesai_TidakDidaftarkanDanTetapDapatDisunting`; seluruh uji kajian medis yang sudah ada tetap lulus |
| 6. Pengkajian `Draft` atau `InProgress` tetap dapat disunting | Terpenuhi | Uji `PengkajianBelumSelesai_TetapDapatDisunting` |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Pendaftaran menyala untuk pengkajian keperawatan | Terpenuhi |
| Penjaga penyuntingan terpasang | Terpenuhi |
| Keenam acceptance criteria terbukti | Terpenuhi |
| Test transaksi hijau | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |
| Laporan menyebut nol perubahan bentuk data | Terpenuhi — bagian 3.3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Batas yang disengaja | Pendaftaran dinyalakan hanya untuk pengkajian yang **menempel pada perawatan rawat inap** (`InpEpisodeId` terisi), bukan untuk seluruh pengkajian keperawatan. Menyalakannya tanpa batas akan mengunci pengkajian poliklinik dan IGD — perubahan perilaku yang tidak diminta task mana pun dan yang dilarang kriteria 5 |
| Keputusan urutan pemeriksaan | `EnsureMutableAsync` diperiksa **sebelum** pemeriksaan status lama pada `PUT /{id}`, supaya pengguna menerima arahan yang benar ("gunakan addendum") alih-alih kalimat teknis ("tidak dapat diubah"). Dokumen yang belum terdaftar dilewatkan apa adanya oleh mesin keutuhan, sehingga jalur lama tetap menerima kalimat lamanya persis |
| Migration | **Nol.** Task ini tidak membuat satu pun berkas migration, dan itu memang yang benar: `RegisterSignedAsync` menulis ke tabel `MrcClinicalDocumentIntegrity` yang sudah ada |
| Temuan di luar cakupan | `PATCH /{id}/cancel` hari ini menerima pengkajian berstatus `Completed`. Sejak task ini, pengkajian rawat inap yang `Completed` sudah terkunci pada mesin keutuhan — tetapi jalur pembatalan **tidak** memeriksa keutuhan, sehingga dokumen terkunci masih dapat dipindahkan ke `Cancelled`. `state-transition-matrix.md` bagian 1 hanya membolehkan pembatalan dari `Draft` dan `InProgress`. Perilaku itu **tidak diubah** task ini karena menyentuh jalur poliklinik dan IGD dan tidak disebut satu pun acceptance criteria. **Dicatat sebagai temuan**, dan disarankan menjadi task tersendiri |
| Risiko tersisa | Pengkajian rawat inap yang sudah `Completed` **sebelum** task ini diterapkan tidak punya baris keutuhan. `EnsureMutableAsync` melewatkan dokumen yang belum terdaftar, sehingga baris lama itu tetap dapat disunting. Pengisian data lama adalah pekerjaan `MedicalRecordBackfillService` milik `MedicalRecordManagement`, di luar cakupan task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sempat terputus; pemulihan mengikuti `TASK_RULES.md`. Nol penyuntingan ganda |
| Status Git | Perubahan task ini masih di working tree; **agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment |
| Langkah berikutnya | `BE-RWI-057` memakai baris keutuhan yang lahir di sini sebagai sasaran koreksi |
