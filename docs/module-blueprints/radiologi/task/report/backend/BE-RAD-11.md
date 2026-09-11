# Laporan Perubahan Backend — `BE-RAD-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-11` |
| Judul | Penyajian hasil ke rekam medis |
| Slice | `S14` — Penyajian hasil ke rekam medis |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 4, gelombang `MVP-4` |
| Trace | `FR-RAD-030`, `FR-RAD-032`; `RAD-DEC-006`; `RAD-INT-001` bagian 2; `RAD-API-001` endpoint `GET /by-encounter/{encounterId}` |
| Contract version | `RAD-API-001` rev 7 saat dikerjakan. Diamandemen menjadi **rev 8** oleh task ini, **menunggu konfirmasi pemilik modul** |
| Dependency | `BE-RAD-10` — **selesai** |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan/auth 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai untuk sisi backend.** Build lulus 0 error; 245 uji radiologi lulus, 12 di antaranya baru; seluruh 1.450 uji in-memory lulus. Definition of Done terpenuhi lewat empat uji arsitektur. **Penghalang `ActAsRadiologist` masih terbuka** |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`; `QBE-API-001`; `QBE-DTO-001`; **`QBE-MOD-001`** — capability tinggal di Area/Module pemiliknya, dan uji arsitektur pada task ini adalah penegakannya; `QBE-PERM-001`; `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-DB-001`, `QBE-DB-002` — tidak ada entity, configuration, maupun migration; `QBE-CODE-*`; `QBE-TXN-001` — task ini hanya membaca; `QBE-DEL-001` |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Endpoint `GET /by-encounter/{encounterId}` sudah ada sejak `BE-RAD-09`, tetapi ia mengembalikan
**seluruh** bacaan pada satu kunjungan — termasuk yang masih draf.

Endpoint itu dipakai rekam medis dan layar dokter pengirim. Artinya, sampai task ini, seorang
dokter jaga yang membuka rekam medis pasien dapat membaca kesimpulan yang **belum diperiksa
dokter radiolog mana pun**.

> **Contoh nyata.** Residen menulis draf pukul 22.00: "tidak tampak perdarahan". Ia belum
> mengesahkannya, dan memang tidak boleh — drafnya wajib disahkan dokter radiolog.
>
> Pukul 22.15 dokter jaga membuka rekam medis dan melihat kesimpulan itu. Ia memulangkan pasien.
>
> Pukul 23.00 dokter radiolog memeriksa draf itu dan menemukan perdarahan kecil. Bacaan yang
> akhirnya sah berbunyi sebaliknya — tetapi keputusan sudah diambil berdasarkan tulisan yang
> belum menjadi pernyataan siapa pun.

Yang kedua: **janji "rekam medis membaca, tidak menyalin" belum dijaga apa pun.** Ia tertulis di
`RAD-DEC-006` dan `RAD-INT-001`, tetapi tidak ada satu pun mekanisme yang akan berbunyi ketika
suatu hari seseorang menambahkan kolom salinan di modul lain.

---

## 2. Proses bisnis

**Tujuan.** Dokter pengirim membaca hasil radiologi tanpa berpindah modul, selalu versi yang
berlaku, dan tidak pernah melihat yang belum sah.

### 2.1 Yang muncul dan yang tidak

| Keadaan bacaan | Muncul di rekam medis? | Alasannya |
| --- | :---: | --- |
| `Pending` — menunggu draf | **Tidak** | Belum ada isinya sama sekali |
| `Drafted` — draf, belum disahkan | **Tidak** | Belum menjadi pernyataan siapa pun |
| `Validated` — sudah disahkan, belum dirilis | **Tidak** | Perilisan adalah tindakan yang menyerahkannya kepada dokter pengirim |
| `Released` | **Ya** | Sah dipakai |
| `AmendmentDrafted` — koreksi sedang disusun | **Ya**, menampilkan **versi rilis sebelumnya** | Versi lama masih berlaku selama koreksinya belum sah |
| `AmendmentValidated` | **Ya**, masih versi rilis sebelumnya | Sama |
| `AmendmentReleased` | **Ya**, menampilkan versi koreksi | Koreksinya sudah sah |

Keadaan `Validated` adalah yang paling mudah terlewat. Bacaan itu sudah diperiksa dokter
radiolog, sehingga terasa "sudah jadi" — tetapi perilisan adalah tindakan tersendiri, dan
sebelum tindakan itu terjadi, tidak ada yang pernah menyatakan bacaan itu siap dipakai dokter
lain. Dibuktikan `FR032_BacaanYangSudahDisahkanTetapiBelumDirilisTidakMuncul`.

### 2.2 Mengapa disembunyikan seluruhnya, bukan ditandai

Pilihan lain yang terlihat masuk akal adalah menampilkan draf dengan label "belum disahkan".
Itu tidak dikerjakan, dan alasannya:

> **Penanda hanya menolong orang yang membacanya.** Dokter jaga pukul 02.00 yang sedang menangani
> tiga pasien membaca **kesimpulannya**, bukan labelnya. Begitu isinya terlihat, ia sudah masuk
> ke dalam pertimbangan — dan tidak ada cara menariknya kembali.
>
> Menyembunyikan seluruhnya menutup jalan itu sepenuhnya. Biayanya kecil dan jelas: dokter
> pengirim menunggu sampai bacaan dirilis, yang memang seharusnya.

### 2.3 Pekerjaan Radiologi tidak ikut tersembunyi

Penyaringan hanya berlaku pada jalur pembaca hasil. Petugas Radiologi yang perlu melihat draf
satu kunjungan memakai `GET /?encounterId=…`, yang **tidak** disaring.

Pemisahan itu disengaja. Kalau penyaringan dipasang di lapisan data, draf akan hilang juga dari
layar orang yang justru harus menyelesaikannya — dan pekerjaan yang tidak terlihat adalah
pekerjaan yang tidak dikerjakan. Dibuktikan
`DaftarKerjaRadiologiTetapMelihatDrafYangSamaLewatJalurLain`.

### 2.4 Koreksi terlihat tanpa satu pun langkah penyalinan

> **Contoh berangka, langsung dari `FR-RAD-030`.** dr. Andi membaca hasil pukul 08.00: versi 1.
> Pukul 09.00 dr. Sinta merilis koreksi. Pukul 10.00 dr. Andi membuka lagi — dan melihat
> **versi 2**.
>
> Tidak ada pekerjaan sinkronisasi di antaranya, tidak ada kejadian yang dikirim, dan tidak ada
> cache yang perlu dibersihkan. Rekam medis membaca tabel yang sama dengan yang ditulis
> Radiologi, jadi tidak ada apa pun yang dapat tertinggal.

Dibuktikan `AC19_SetelahKoreksiDirilis_RekamMedisLangsungMelihatVersiTerbaru`. Dan selama
koreksi masih disusun, yang terbaca tetap versi 1 —
`AC19_SelamaKoreksiDisusun_RekamMedisMasihMelihatVersiRilisSebelumnya`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md`, `docs/engineering/` beserta registry | Governance canonical dan preflight QBE |
| `rules/backend/` — `TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE` | Aturan operasional task |
| `contracts/integration-contract.md` bagian 2 | Yang wajib dan yang dilarang pada penyajian hasil; slot dokumen rekam medis |
| `04-prd-to-mvp.md` `FR-RAD-030` s/d `FR-RAD-032` | Contoh berangka yang menentukan perilaku |
| `testing/acceptance-test-matrix.md` bagian 5 | AC-19 dan AC-21 beserta bukti yang diharapkan |
| `Areas/HealthServices/ClinicalManagement/Enums/PatientClinicalDocumentSource.cs` | Slot `Radiology` yang tidak boleh diisi alur internal |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientClinicalDocument.cs` | Tabel yang menjadi sasaran larangan salinan |
| `Tests/.../LabSpecimenDecisionTests.cs` | Pola uji yang membaca source sungguhan, dipakai ulang untuk uji arsitektur |
| `Services/RadReportService.cs`, `Controllers/RadReportController.cs` | Yang sudah ada dari `BE-RAD-08` s/d `BE-RAD-10` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Services/RadReportService.cs` | `GetByEncounterAsync` kini menyaring bacaan yang belum pernah dirilis |
| `Areas/.../Controllers/RadReportController.cs` | Keterangan endpoint diperjelas; pesan daftar kosong menyebut "yang sudah dirilis" |
| `Tests/.../RadReportMedicalRecordTests.cs` | **Baru.** 12 uji — delapan perilaku, empat arsitektur |

### 3.3 Penyaring memakai dua syarat, bukan satu

```csharp
!BelumDirilisStatuses.Contains(x.ReportStatus) && x.FirstReleasedAt != null
```

Syarat pertama membaca **status**; syarat kedua membaca **waktu perilisan**. Keduanya seharusnya
selalu sepakat.

Justru karena seharusnya sepakat, keduanya dipakai bersama. Baris yang kedua kolomnya berselisih
— entah karena data lama, entah karena jalur yang belum terpikirkan — **ditahan, bukan
diloloskan**. Pada penyaring yang menentukan apa yang dibaca dokter, arah kesalahan yang dipilih
harus yang menahan.

### 3.4 Definition of Done dijaga empat uji arsitektur

Butir DoD berbunyi: *"Uji arsitektur membuktikan tidak ada tabel di luar Radiologi yang menyimpan
isi bacaan."* Itu dibuktikan dari empat arah sekaligus, karena satu arah saja mudah dilewati.

| Uji | Yang diperiksa | Bentuk pelanggaran yang ditangkapnya |
| --- | --- | --- |
| `TidakAdaEntityDiLuarRadiologiYangMenyimpanRujukanHasilBacaan` | Seluruh entity di luar namespace Radiologi, lewat reflection | Modul lain menambahkan kolom `RadReportId` beserta salinan kesimpulannya |
| `TidakAdaModulLainYangMenyebutHasilBacaanRadiologiPadaSourcenya` | Seluruh berkas `.cs` di bawah `Areas/` di luar modul Radiologi | Modul lain menyalin isi bacaan ke kolom bernama netral — yang menandainya adalah penyebutan nama tipenya |
| `AC21_AlurHasilBacaanTidakMenyentuhSlotDokumenRekamMedis` | Seluruh berkas modul Radiologi | Alur internal ikut mengisi `PatientClinicalDocumentSource.Radiology` |
| `ServiceHasilBacaanTidakBergantungPadaModulLainSelainInfrastrukturBersama` | Constructor `RadReportService` | Ketergantungan baru pada modul lain — tanda paling awal sebuah modul mulai menulis ke wilayah modul lain |

**Ketiga uji pemindaian dijaga terhadap lulus semu.** Masing-masing menegaskan lebih dulu bahwa
ia benar-benar memindai sesuatu — lebih dari 100 berkas, lebih dari 100 entity, lebih dari 10
berkas radiologi. Uji pemindaian yang kehilangan jalur sourcenya akan memeriksa nol berkas lalu
melaporkan "tidak ada pelanggaran" selamanya, dan itu bentuk kegagalan paling berbahaya: ia
terlihat persis seperti keberhasilan.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu perubahan perilaku** pada endpoint yang sudah berjalan: `GET /by-encounter/{encounterId}` kini mengembalikan lebih sedikit baris. **Tidak ada bentuk data yang berubah** dan tidak ada field yang hilang. Dicatat sebagai `RAD-API-001` revision 8, **menunggu konfirmasi pemilik modul** |
| Database | **`NOT APPLICABLE`.** Tidak ada entity, configuration, migration, maupun perubahan snapshot. Migration `AddRadReport` tetap **belum dijalankan** |
| Keamanan/Auth | Tidak ada string hak akses baru. **Dampak privasi positif**: kesimpulan klinis yang belum sah berhenti tampil pada layar pembaca hasil. Empat uji arsitektur menjaga isi bacaan tidak berpindah ke tabel yang aturan aksesnya berbeda |

---

## 4. Dokumentasi endpoint

#### Health Services / Radiology Management / Rad Report

Base URL: `api/v1/health-services/radiology-management/rad-reports`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/by-encounter/{encounterId}` | Melihat bacaan satu kunjungan yang **sudah dirilis** — dipakai rekam medis dan layar dokter pengirim | `RadReport : Read` |

**Tidak ada endpoint baru pada task ini.** Yang berubah adalah perilaku endpoint yang sudah ada.
Sebelas endpoint lain grup ini didokumentasikan pada `BE-RAD-09.md` dan `BE-RAD-10.md` bagian 4.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, **0 error**, 188 warning, 4 menit 46 detik | `PASS` | Keluaran perintah |
| Warning baru dari berkas radiologi | **Tidak ada satu pun**; project uji tetap 10 warning | `PASS` | Penyaringan warning build |
| `dotnet build` project uji in-memory | Berhasil, **0 error**, 10 warning | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadReportMedicalRecordTests` | **12 lulus, 0 gagal**, 21 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadiologyManagement` | **245 lulus, 0 gagal**, 37 detik | `PASS` | 233 sebelumnya + 12 baru |
| `dotnet test --no-build` seluruh project in-memory | **1.450 lulus, 0 gagal**, 43 detik | `PASS` | 1.438 sebelumnya + 12 |

### 5.1 Bukti per butir yang diminta roadmap

| Yang wajib dibuktikan | Uji | Hasil |
| --- | --- | --- |
| Bacaan berstatus `Drafted` tidak muncul | `FR032_BacaanBerstatusDraftedTidakMuncul` | `PASS` |
| Bacaan `Validated` tetapi belum dirilis tidak muncul | `FR032_BacaanYangSudahDisahkanTetapiBelumDirilisTidakMuncul` | `PASS` |
| Bacaan yang sudah dirilis muncul | `BacaanYangSudahDirilisMuncul` | `PASS` |
| Penyaring bekerja per baris, bukan per kunjungan | `HanyaBacaanYangDirilisYangIkutTerbawaKetikaKunjunganPunyaKeduanya` | `PASS` |
| Draf tetap terlihat pada jalur kerja Radiologi | `DaftarKerjaRadiologiTetapMelihatDrafYangSamaLewatJalurLain` | `PASS` |
| **Koreksi langsung terlihat tanpa penyalinan** | `AC19_SetelahKoreksiDirilis_RekamMedisLangsungMelihatVersiTerbaru` | `PASS` |
| Selama koreksi disusun, versi rilis sebelumnya yang terbaca | `AC19_SelamaKoreksiDisusun_RekamMedisMasihMelihatVersiRilisSebelumnya` | `PASS` |
| Kunjungan tanpa bacaan dirilis mengembalikan daftar kosong, bukan galat | `KunjunganTanpaBacaanDirilisMengembalikanDaftarKosong` | `PASS` |
| **DoD — tidak ada tabel di luar Radiologi yang menyimpan isi bacaan** | Empat uji arsitektur pada bagian 3.4 | `PASS` |

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database yang tabelnya belum
dibuat.

### 5.2 Batas verifikasi yang perlu diketahui

| Yang belum terbukti | Sebabnya |
| --- | --- |
| Perilaku terhadap tabel sungguhan | Migration `AddRadReport` belum dijalankan ke database mana pun |
| **`FR-RAD-031` — gangguan dibedakan dari kekosongan** | Kriteria layar, bukan backend. Backend menjawab `200` dengan daftar kosong ketika memang kosong, dan gagal dengan galat ketika memang gagal; **membedakan keduanya di layar adalah pekerjaan `FE-RAD-13`** |
| Uji arsitektur hanya memindai `Areas/` | Penyalinan yang terjadi di `Repositories/`, `Services/`, atau modul di luar `Areas/` tidak tertangkap. Cakupan itu dipilih karena `Repositories/` memang berisi configuration `RadReport` yang sah; memperluas pemindaian menuntut daftar pengecualian yang justru mudah dilonggarkan diam-diam |
| Pipeline HTTP sesungguhnya | Uji memanggil service dan controller secara langsung |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| `dotnet ef migrations add` maupun `database update` | Tidak ada perubahan model; eksekusi database **wewenang terpisah yang belum diberikan** |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi; tabelnya juga belum ada |
| Analyzer build penuh | Dimatikan atas permintaan pemilik modul. Warning compiler tetap dihitung dan disaring |
| Perubahan pada modul Clinical Management | Di luar target tulis task ini. Slot `PatientClinicalDocumentSource.Radiology` **tidak disentuh** — memang tidak boleh, karena ia tetap dipakai untuk berkas unggahan dari luar |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-19 — bacaan dikoreksi lalu rekam medis dibuka, menampilkan versi terbaru tanpa penyalinan | **Terpenuhi di backend** | Dua uji AC-19 pada tabel 5.1. Bukti dari sisi layar milik `FE-RAD-13` |
| AC-21 — slot `PatientClinicalDocumentSource.Radiology` tidak diisi alur hasil bacaan internal | **Terpenuhi** | `AC21_AlurHasilBacaanTidakMenyentuhSlotDokumenRekamMedis` |
| Test — bacaan berstatus `Drafted` tidak muncul | **Terpenuhi** | Dua uji `FR-RAD-032` |
| Test — koreksi langsung terlihat tanpa penyalinan | **Terpenuhi** | `AC19_SetelahKoreksiDirilis_RekamMedisLangsungMelihatVersiTerbaru` |
| DoD — uji arsitektur membuktikan tidak ada tabel di luar Radiologi yang menyimpan isi bacaan | **Terpenuhi** | Empat uji arsitektur, ketiganya dijaga terhadap lulus semu |
| Risiko — jangan menyediakan jalur apa pun yang memungkinkan modul lain menyimpan salinan | **Terpenuhi** | Tidak ada endpoint baru; endpoint yang ada hanya membaca. Uji arsitektur menjaga keadaan itu tidak bergeser diam-diam |

**Butir yang belum terpenuhi, disebut apa adanya:**

1. **`FR-RAD-031` belum tersentuh sama sekali** — ia kriteria layar (`FE-RAD-13`), dan backend
   memang tidak dapat membuktikannya. Disebut di sini supaya tidak dianggap selesai hanya karena
   `EPIC RAD-04` terlihat rampung di backend.
2. **Penanda `RadReport : ActAsRadiologist` masih belum dapat diberikan** — tidak berubah sejak
   `BE-RAD-08`.
3. **Belum ada bukti terhadap database sungguhan.**
4. **`RadReportListResponse` tidak membawa kesimpulan bacaan.** Rekam medis perlu membuka
   `GET /{id}` untuk membacanya. Ini keputusan sadar dari `BE-RAD-09` — daftar sering terbuka
   pada layar yang terlihat banyak orang — tetapi **`FE-RAD-13` mungkin ingin meninjaunya**, dan
   itu keputusan pemilik modul, bukan keputusan yang boleh diambil diam-diam di backend.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | Penghalang `ActAsRadiologist` — `BE-RAD-08.md` bagian 7.1. Selisih kalimat `RAD-STATE-001` bagian 3 — `BE-RAD-10.md` bagian 2.3. `RadReport.Version` masih belum dideklarasikan `IsConcurrencyToken()` |
| Risiko tersisa | **Pertama**, frontend yang sudah dibangun terhadap `RAD-API-001` revision 6 atau 7 akan melihat lebih sedikit baris pada `GET /by-encounter` — perubahan yang disengaja, tetapi perlu diketahui pembuat layar. **Kedua**, migration belum dijalankan. **Ketiga**, uji arsitektur hanya memindai `Areas/` — lihat batas verifikasi 5.2 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 7.1 |
| Langkah berikutnya | Gelombang `MVP-4` menyisakan `BE-RAD-12` (penanda cito) dan `BE-RAD-13` (daftar kerja per alat). Sebelum itu, dua hal yang menunggu keputusan Anda: penghalang `ActAsRadiologist`, dan konfirmasi amandemen kontrak revision 6 sampai 8 |

### 7.1 Status Git pada akhir pekerjaan

Berkas hasil task ini:

```text
 M docs/module-blueprints/radiologi/contracts/api-contract.md
 M docs/module-blueprints/radiologi/roadmap/backend-roadmap.md
 M docs/module-blueprints/radiologi/roadmap/requirement-traceability.md
?? Areas/HealthServices/RadiologyManagement/Controllers/RadReportController.cs
?? Areas/HealthServices/RadiologyManagement/Services/RadReportService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadReportMedicalRecordTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-11.md
```

`RadReportController.cs` dan `RadReportService.cs` **disunting** task ini tetapi tetap tampil
`??` karena keduanya belum pernah di-commit sejak `BE-RAD-08` dan `BE-RAD-09` — Git belum
melacaknya, sehingga tidak ada yang dapat ditandai berubah.

Berkas lain yang tampak pada `git status` — Laboratorium, laporan `BE-RAD-04` sampai `BE-RAD-10`,
migration `AddRadReport`, serta model dan configuration hasil `BE-RAD-07` — sudah ada sebelum
task ini dimulai dan **bukan** hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan. **Tidak ada perintah database yang
dijalankan.**
