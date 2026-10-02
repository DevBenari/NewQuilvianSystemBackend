# Laporan Perubahan Backend — `BE-LAB-13`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-13` |
| Judul | Fakta kelayakan tagih per pemeriksaan |
| Slice | `S10` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 5, gelombang `MVP-2` |
| Trace | `FR-05.1` .. `FR-05.4`; `LAB-INH-013`; `AC-12`, `AC-13`, `AC-37`; `LAB-INT-v1` r3 `INT-01` |
| Contract version | `LAB-INT-v1` r3 `INT-01` — satuan fakta berubah dari wadah menjadi pemeriksaan |
| Dependency | `BE-LAB-12` — **`SELESAI`**. Lihat bagian 3.4 butir 1 soal `BE-LAB-11` |
| Klasifikasi | `HEAVY` — skor 10 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f103fff`, branch `yoga` |
| Tanggal | 2026-09-03; ditutup 2026-09-04 |
| Status | **`SELESAI`** — seluruh butir DoD terbukti. `AC-13` dan `LaboratoryAuthorityTests.cs` ditutup 2026-09-04 setelah penghalang sesungguhnya ditemukan. Lihat bagian 5.2 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabSpecimenService` dan `LabOrderService` sudah ada. `LabFactEmission` adalah `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-AUD-001`, `QBE-MOD-001` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*` — tidak ada entity, configuration, maupun migration |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Fakta kelayakan tagih diterbitkan **per wadah**, padahal yang ditagihkan adalah pemeriksaan.

> Satu tabung darah ungu menopang hemoglobin, leukosit, dan trombosit. Ketika petugas
> menyatakan tabung itu layak, Billing menerima **satu** fakta dengan satu salinan tarif —
> tarif hemoglobin saja, karena itulah yang kebetulan tersimpan pada wadahnya. Leukosit dan
> trombosit dikerjakan, tetapi tidak pernah sampai ke tagihan.

Rumah sakit kehilangan pendapatan atas pekerjaan yang benar-benar dilakukan, dan tidak ada yang
menyadarinya karena jumlah faktanya tetap terlihat wajar — satu per tabung.

---

## 2. Proses bisnis

### 2.1 Contoh berangka — `AC-37`

Satu wadah menopang dua pemeriksaan:

| | Sebelum | Sesudah |
| --- | ---: | ---: |
| Fakta terbit | 1 | **2** |
| Salinan tarif | 35.000 (hemoglobin saja) | 35.000 **dan** 30.000 |
| Yang sampai ke Billing | Rp35.000 | **Rp65.000** |
| Leukosit | Dikerjakan, tidak tertagih | Tertagih |

### 2.2 Idempotensi

Menekan tombol layak dua kali menerbitkan **dua** fakta, bukan empat. Identitas faktanya
menunjuk pemeriksaan yang sama, sehingga producer mengenalinya sebagai pengiriman ulang.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Wadah ditolak | **Tidak ada fakta yang terbit sama sekali** |
| Pemeriksaan sudah dibatalkan tersendiri | Tidak ikut menerbitkan fakta — ia memang tidak dikerjakan |
| Wadah tanpa baris pemeriksaan (data peninggalan sebelum `LAB-DEC-024`) | Fakta tetap terbit atas wadah, supaya jejaknya tidak hilang |

---

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Services/LabSpecimenService.cs` | `EmitChargeEligibilityAsync` dan `EmitClinicalCancellationAsync` menerbitkan **satu fakta per pemeriksaan**; `BuildFactRequest` beroleh kelebihan-beban yang menerima `LabExamination` dan mengisi `SourceItemId` dengan identitas pemeriksaan; `LabFactEmission` baru mengumpulkan hasilnya |
| `.../Services/LabOrderService.cs` | `MapHandoff` beroleh kelebihan-beban untuk `LabFactEmission` |
| `.../DTOs/LabSpecimenDtos.cs` | `LabBillingHandoffResponse` bertambah `MilestoneFactIds` dan `MilestoneFactCount` — **aditif** |
| `Tests/.../LabSpecimenDecisionTests.cs` | Tiga uji baru |
| `Tests/.../IntegrationTests.Postgres/Laboratory/LaboratorySpecimenLifecycleTests.cs` | Sebelas pemanggilan anggota `Handoff` disesuaikan menjadi `Handoff.Perwakilan`. Tanpa ini project ujinya **tidak dapat dikompilasi**, sehingga tidak satu pun uji di dalamnya dapat berjalan — termasuk `LaboratoryAuthorityTests.cs` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `LabBillingHandoffResponse` bertambah dua ruas, **aditif**. `MilestoneFactId` tetap diisi identitas fakta pertama supaya pemanggil lama tidak putus |
| Kontrak integrasi | **`INT-01` berubah satuannya.** `SourceItemId` kini menunjuk `LabExamination.Id`, bukan `LabSpecimen.Id`. Producer, enum, dan jalur dispatch **tidak disentuh** — hanya pemanggilannya |
| Database | **Tidak ada dampak schema.** Yang berubah adalah jumlah baris fakta yang terbit |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada hak akses yang berubah |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Dependency melingkar pada roadmap** | Kartu `BE-LAB-13` menyebut dependency `BE-LAB-11`, sementara `BE-LAB-11` tidak dapat menghapus keenam kolom selama muatan fakta masih membacanya. Keduanya saling menunggu. Diselesaikan dengan urutan **pembaca dulu, schema terakhir**: `BE-LAB-13` memindahkan sumber muatan ke `LabExamination`, lalu `BE-LAB-11` menghapus kolom yang sudah tidak dibaca. **Penyelarasan kedua kartu menjadi utang pemilik blueprint** |
| 2 | **`MilestoneFactId`, bukan `ClinicalMilestoneFactId`** | Keduanya ada pada `ClinicalFactEmissionResult` dan perbedaannya menentukan: yang kedua adalah identitas **baris**, yang berganti setiap fakta memperoleh versi baru; yang pertama adalah identitas **fakta**, yang bertahan lintas versi. Putaran pertama memakai yang keliru, dan uji idempotensi menangkapnya — lihat bagian 5.1 |
| 3 | Wadah tanpa baris pemeriksaan tetap menerbitkan fakta atas wadah | Data peninggalan sebelum `LAB-DEC-024` tidak punya baris pemeriksaan. Membiarkannya tidak menerbitkan apa pun akan menghilangkan jejak tagihan yang sah |
| 4 | Penerbitan dilakukan berurutan, bukan sekaligus | Producer menerima satu permintaan per panggilan. Tiga pemeriksaan berarti tiga panggilan berurutan; mengubah producer agar menerima kumpulan adalah pekerjaan pemilik `ClinicalManagement`, bukan Laboratorium |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Tidak ada endpoint yang ditambah atau diubah route-nya. Yang berubah adalah
isi jawaban `POST /lab-specimens/{id}/accept`, yang kini membawa `MilestoneFactIds` dan
`MilestoneFactCount`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 205, Total: 205` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan Billing yang terbuka sejak sebelum seluruh pekerjaan Laboratorium |
| Checker QBE atas 39 berkas modul | `VIOLATION: 0`, `Final result: PASS` | `PASS` | Keluaran perintah |
| **`FR-05.1`** — wadah dua pemeriksaan menerbitkan dua fakta | Dua fakta dengan identitas berbeda; kedua pemeriksaan `ChargeEligible` pada waktu yang sama; salinan tarif 35.000 dan 30.000 | `PASS` | `FR0501_WadahDuaPemeriksaan_MenerbitkanDuaFaktaDenganTarifMasingMasing` |
| **Idempotensi** — menekan layak dua kali | Tetap dua fakta, dan identitasnya **sama persis** | `PASS` | `FR0501_MenekanLayakDuaKali_TetapMenghasilkanDuaFakta` |
| Wadah ditolak | Tidak menerbitkan fakta apa pun | `PASS` | `WadahDitolak_TidakMenerbitkanFaktaApaPun` |
| `dotnet build` project `IntegrationTests.Postgres` | `0 Error(s)` sesudah perbaikan; **11 `CS1061`** sebelumnya | `PASS` | Keluaran perintah, 2026-09-04 |
| **`AC-13`** — nol properti dan nol method finansial | `Failed: 0, Passed: 18, Total: 18` | `PASS` | Seluruh `LaboratoryAuthorityTests` hijau, termasuk `ModelLaboratorium_TidakMemilikiPropertiFinansialApaPun` dan `ServiceLaboratorium_TidakMemilikiMethodKewenanganFinansial` — lihat bagian 5.2 |
| Seluruh project `IntegrationTests.Postgres` | `Failed: 52, Passed: 34, Total: 86` | `PASS sebagian / ENVIRONMENT` | Ke-34 yang lulus adalah uji yang tidak menyentuh database. Ke-52 yang gagal memakai satu pesan yang sama, `BLOCKED_BY_TEST_DB_CONFIGURATION` |
| `Tests/QuilvianSystemBackend.Tests` sesudah perbaikan | `Failed: 0, Passed: 205, Total: 205` | `PASS` | Keluaran perintah, 2026-09-04 |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `LaboratorySpecimenLifecycleTests.cs` — 18 uji yang memakai database | Terhalang `QUILVIAN_BILLING_TEST_DB`. Pengisiannya **dicoba** pada 2026-09-04 dan ditolak server: akun aplikasi tidak memiliki hak `CREATEDB`, jawabannya `42501: permission denied to create database`. Menyediakan database test adalah wewenang DBA, bukan wewenang sesi ini |
| Uji integrasi total rujukan Rp270.000 | Kartu task menyebut angka itu pada suite integrasi Postgres yang terhalang. Perilaku setaranya dibuktikan pada provider InMemory dengan tarif 35.000 dan 30.000 |
| Perintah database apa pun | Task ini tidak menyentuh schema |

### 5.1 Uji yang menangkap kesalahan sungguhan

Uji idempotensi gagal pada percobaan pertama: dua panggilan menghasilkan empat identitas
berbeda. Penyebabnya bukan producer, melainkan kode ini — ia mengumpulkan
`ClinicalMilestoneFactId`, identitas **baris**, yang memang berganti setiap fakta memperoleh
versi baru. Yang seharusnya dikumpulkan adalah `MilestoneFactId`, identitas **fakta** yang
bertahan lintas versi.

Kesalahan itu tidak akan terlihat dari membaca kode, dan tidak akan terlihat dari jumlah fakta
yang terbit — keduanya benar. Ia hanya terlihat karena ada uji yang membandingkan identitas
antara dua pemanggilan.

### 5.2 Penghalang yang ternyata bukan penghalang

Laporan putaran pertama menyebut `AC-13` terhalang `QUILVIAN_BILLING_TEST_DB`. **Itu keliru**, dan
keliru dengan cara yang perlu dicatat.

`LaboratoryAuthorityTests` tidak memakai `IClassFixture<BillingTestDatabaseFixture>` sama sekali.
Ia bekerja lewat refleksi atas tipe dan atribut — nol koneksi database. Yang sesungguhnya
menghalanginya adalah **project ujinya tidak dapat dikompilasi**: `LaboratorySpecimenLifecycleTests.cs`
masih memanggil `Kind`, `MilestoneFactId`, dan `MilestoneFactVersion` langsung pada `Handoff`,
padahal `BE-LAB-13` mengubah tipe kembaliannya menjadi `LabFactEmission` yang menaruh ketiganya di
bawah `Perwakilan`. Sebelas kesalahan `CS1061`, dan satu project yang gagal build tidak menjalankan
satu pun ujinya.

Kekeliruan itu lolos karena putaran pertama membaca pesan kegagalan `BLOCKED_BY_TEST_DB_CONFIGURATION`
milik uji lain, lalu menyimpulkan seluruh project terhalang hal yang sama — **tanpa pernah menjalankan
`dotnet build` atas project itu**. Pesan yang benar untuk uji yang berbeda dipakai menjelaskan uji yang
tidak pernah dicoba.

Perbaikannya sebelas baris. Sesudahnya `LaboratoryAuthorityTests` lulus 18 dari 18 tanpa database apa
pun, dan kedua butir DoD yang tertunda tertutup.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-37` — kelayakan tagih terbit per pemeriksaan | **Terpenuhi** | `FR0501_WadahDuaPemeriksaan_...` |
| `AC-12` — pengiriman ulang tidak menggandakan | **Terpenuhi** | `FR0501_MenekanLayakDuaKali_...` |
| `AC-13` — nol properti dan method finansial pada Laboratorium | **Terpenuhi** | `LaboratoryAuthorityTests` 18/18 — lihat bagian 5.2 |

| Butir DoD | Status |
| --- | --- |
| Fakta terbit per pemeriksaan | **Terpenuhi** |
| Idempotensi terbukti | **Terpenuhi** |
| `LaboratoryAuthorityTests.cs` tetap hijau | **Terpenuhi** — 18/18 |
| `AC-13` terbukti | **Terpenuhi** |

**Keempat butir DoD terpenuhi.** Dua di antaranya sempat dilaporkan terhalang lingkungan; bagian 5.2
menjelaskan mengapa laporan itu keliru dan apa penghalang sesungguhnya.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | **Tujuh assertion pada `LaboratorySpecimenLifecycleTests.cs` masih memakai satuan lama.** Uji-uji itu mencari baris tagihan dengan `SourceItemId == specimen.Id`, padahal sejak task ini `SourceItemId` menunjuk `LabExamination.Id`. Baris `126`, `168` .. `171`, `280`, `359` .. `360`, `425`, dan `449`. Uji tersebut **tidak dapat dijalankan tanpa database**, sehingga tidak diubah dari sesi ini: menyunting assertion yang tidak dapat dibuktikan justru melemahkannya sebagai bukti. Yang benar adalah menyelesaikannya bersama penyediaan database test. Baris `210` tetap sah karena ia menuntut ketiadaan |
| Risiko tersisa | **Sedang.** Perubahan ini menaikkan jumlah fakta yang dikirim ke Billing dari satu per wadah menjadi satu per pemeriksaan. Bila ada lingkungan yang sudah memuat fakta lama ber-`SourceItemId` wadah, keduanya akan hidup berdampingan dan Billing melihat dua jenis satuan. Pada dev pemilik hal ini tidak terjadi karena datanya nol |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. **`BE-LAB-11`** — aman dikerjakan; tidak ada lagi yang membaca keenam kolom itu. 2. Meminta DBA menyediakan database test dan hak `CREATEDB`, lalu menyelesaikan ketujuh assertion satuan lama pada `LaboratorySpecimenLifecycleTests.cs`. 3. `BE-LAB-10` — penandaan cito dan duplo. 4. Menyelaraskan dependency melingkar `BE-LAB-11` ↔ `BE-LAB-13` pada roadmap |
