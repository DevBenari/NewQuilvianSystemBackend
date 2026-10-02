# Laporan Perubahan Backend — `BE-RWI-049`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-049` |
| Judul | Kunjungan yang salah catat dapat dibatalkan tanpa menghilangkan jejaknya |
| Slice | `DOK-MVP-4` — visite |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-049` |
| Trace | `FR-DOK-040`, `FR-DOK-041`; `INV-DOK-08`, `INV-DOK-09`; `RWI-AC-156`; `RWI-DEC-085`; `contracts/state-transition-matrix.md` §5.2; `VAL-DOK-28`, `VAL-DOK-29` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-048` 🟡 **sebagian** ([laporan](BE-RWI-048.md)) — permukaan visite sudah ada; yang tertunda hanya uji PostgreSQL |
| Klasifikasi | `MEDIUM`, skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 2, kontrak API 2, database 0, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Ketujuh acceptance criteria terbukti, termasuk tiga uji arsitektur. Nol migration |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY` |
| Applicability | `NEW CODE`. Endpoint pembatalan dan penautan baru |
| QBE berlaku | `QBE-API-001`, `QBE-SVC-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-DEL-001`, `QBE-LOG-001` |
| Entity operasional baru | `NONE`. Kolom pembatalan dan penunjuk kejadian pengganti sudah dibuat `BE-RWI-041` |
| Archetype | Transaksi. **Nol** endpoint penghapusan, **nol** endpoint penyuntingan waktu maupun peran — ketiadaan itu diuji, bukan sekadar dinyatakan |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Salah ketik jam pada catatan visite tidak punya jalan keluar yang jujur.**

Dokter yang visite pukul 07.40 tetapi mengisi 17.40 menghadapi dua pilihan buruk bila tidak ada
mekanisme koreksi:

1. Membiarkannya. Riwayat menampilkan kunjungan yang tidak pernah terjadi pada jam itu, dan
   laporan kunjungan dokter menjadi salah.
2. Menyunting jamnya. Fakta lama tergantikan fakta baru **tanpa jejak**, dan auditor kehilangan
   kemampuan melihat bahwa pernah ada catatan yang keliru.

`RWI-DEC-085` menutup pilihan kedua: penyuntingan di tempat dilarang. Yang benar adalah
membatalkan beralasan, lalu mencatat ulang.

---

## 2. Proses bisnis

### 2.1 Cara koreksi yang benar

Mengikuti `state-transition-matrix.md` bagian 5.2:

1. Batalkan kejadian yang salah **beserta alasannya**. Ia tetap tampil pada riwayat dengan
   penanda batal.
2. Catat kejadian baru dengan kunci permintaan **baru**, menunjuk kejadian yang digantikannya.
3. Hitungan visite hanya menghitung kejadian berstatus tercatat.

### 2.2 Contoh berangka

> dr. Andi visite Tn. Budi pukul 07.40 tetapi mengisi 17.40. Ia membatalkan dengan alasan
> "salah ketik jam", lalu mencatat kejadian baru pukul 07.40 yang menunjuk kejadian yang
> digantikannya.
>
> Riwayat Tn. Budi menampilkan **dua baris**: satu batal beserta alasannya, satu berlaku.
> Hitungan visite hari itu tetap **satu**.

### 2.3 Kenapa kejadian batal tetap tersimpan

`INV-DOK-08`. Kejadian yang dibatalkan adalah bagian dari riwayat, bukan sampah. Auditor perlu
melihat bahwa pernah ada catatan yang keliru beserta alasan pembatalannya; menghapusnya berarti
menghapus fakta bahwa kekeliruan itu pernah ada.

Karena itu:

- Tidak ada satu pun endpoint penghapusan pada permukaan visite.
- Riwayat menampilkan kejadian batal **secara bawaan**; yang tidak menghitungnya adalah
  ringkasan, bukan riwayat.

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Alasan pembatalan kosong | "Alasan pembatalan wajib diisi." | `400` |
| Kejadian sudah dibatalkan sebelumnya | "Kejadian visite ini sudah dibatalkan." | `409` |
| Kejadian tidak ditemukan | Ditolak | `404` |
| Penautan dokumen pada kejadian yang sudah batal | Ditolak | `409` |

### 2.5 Kenapa pembatalan kedua ditolak, bukan didiamkan

`Cancelled` adalah status terminal. Pembatalan kedua akan **menimpa** alasan dan waktu
pembatalan pertama — dan itu menghapus jejak yang justru sedang dijaga.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD |
| `contracts/state-transition-matrix.md` §5.1, §5.2, §5.3 | Transisi yang tidak sah dan cara koreksi yang benar |
| `contracts/validation-matrix.md` §4 | `VAL-DOK-28`, `VAL-DOK-29` |
| `contracts/api-contract.md` §4, §11 | Bentuk endpoint pembatalan dan daftar endpoint yang **sengaja tidak ada** |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitService.cs` | Perintah pembatalan yang sudah tersedia dari `BE-RWI-041` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs` | Endpoint `PATCH /{id}/cancel` dan `PATCH /{id}/links`; riwayat menampilkan kejadian batal beserta alasannya; ringkasan tidak menghitungnya |
| `Areas/HealthServices/ClinicalManagement/DTOs/PhysicianVisitDtos.cs` | Permintaan pembatalan beralasan; permintaan penautan yang **sengaja tidak memuat** waktu maupun peran |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitService.cs` | Penautan dokumen beserta penjagaan kepemilikan kunjungan; hitungan kejadian batal |
| `Tests/.../ClinicalManagement/PhysicianVisitCancellationTests.cs` | **Baru.** Tujuh uji, tiga di antaranya uji arsitektur |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru sesuai `api-contract.md` §4: `PATCH /{id}/cancel` dan `PATCH /{id}/links`. Nol endpoint penyuntingan waktu maupun peran — dan ketiadaannya diuji |
| Database | Nol perubahan schema, nol migration, nol eksekusi database |
| Keamanan/Auth | Action `Cancel` pada Resource `PhysicianVisit`, nama sama persis pada kedua penanda. Diberikan juga kepada supervisor klinis lewat layar Akses Role, karena kejadian salah catat bisa ditemukan setelah dokternya pulang |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Physician Visit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/cancel` | Membatalkan kejadian yang salah catat beserta alasannya. Baris **tidak dihapus** | `PhysicianVisit : Cancel` |
| `PATCH` | `/{id}/links` | Menautkan catatan dokter, catatan terpadu, atau tindakan. **Tidak menerima waktu maupun peran** | `PhysicianVisit : Update` |
| `GET` | `/episodes/{episodeId}` | Riwayat; kejadian batal ikut tampil beserta alasannya. Penyaring `includeCancelled` tersedia, bawaannya menampilkan | `PhysicianVisit : Read` |
| `GET` | `/summary` | Hitungan; kejadian batal **tidak** ikut dihitung, tetapi jumlahnya tetap dilaporkan terpisah | `PhysicianVisit : Read` |

**Kode status pembatalan:** `200` berhasil; `400` alasan kosong; `404` kejadian tidak ditemukan;
`409` kejadian sudah dibatalkan sebelumnya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Pembatalan tanpa alasan ditolak | `400`, pesan memuat "Alasan pembatalan"; status kejadian tetap tercatat | `PASS` | `PembatalanTanpaAlasan_Ditolak400` |
| Kejadian batal tetap tersimpan, tetap tampil, tidak ikut dihitung | `IsDelete` salah; alasan, waktu, dan pembatal tersimpan; riwayat menampilkannya dengan label "Dibatalkan"; `RecordedCount` 0, `CancelledCount` 1, `TotalCount` 1 | `PASS` | `KejadianDibatalkan_TetapTampilPadaRiwayatDanTidakIkutDihitung` |
| Membatalkan kejadian yang sudah batal | `409`; alasan pembatalan pertama **tidak tertimpa** | `PASS` | `MembatalkanKejadianYangSudahBatal_Ditolak409` |
| Pencatatan ulang menunjuk kejadian yang digantikan | Dua baris; kejadian berlaku menunjuk kejadian batal; `RecordedCount` 1, `CancelledCount` 1, `TotalCount` 2 | `PASS` | `PencatatanUlangSetelahPembatalan_MenunjukKejadianYangDigantikan` |
| **Architecture test** — nol endpoint penyunting waktu maupun peran | Seluruh badan permintaan pada controller visite selain pencatatan tidak memuat `VisitDateTime` maupun `VisitRole` | `PASS` | `TidakAdaEndpointYangMenyuntingWaktuMaupunPeranVisite` |
| **Architecture test** — nol endpoint penghapusan | Tidak ada satu pun method beratribut `HttpDelete` | `PASS` | `ControllerVisiteTidakMenyediakanEndpointPenghapusan` |
| **Architecture test** — agregasi tagihan tidak menyentuh kejadian klinis | Nol tipe di luar `ClinicalManagement` yang memegang `CliPhysicianVisit` | `PASS` | `HanyaClinicalManagementYangMenyentuhKejadianVisite` |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, berkas tidak disentuh task ini |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** migration dan perintah basis data apa pun.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pembatalan tanpa alasan ditolak `400` | Terpenuhi | `PembatalanTanpaAlasan_Ditolak400` |
| 2. Kejadian yang dibatalkan **tetap tersimpan** dan tetap tampil pada riwayat beserta alasannya | Terpenuhi | `KejadianDibatalkan_TetapTampilPadaRiwayatDanTidakIkutDihitung` |
| 3. Hitungan hanya menghitung kejadian yang tidak dibatalkan | Terpenuhi | Uji yang sama — `RecordedCount` 0 dari `TotalCount` 1 |
| 4. Membatalkan kejadian yang sudah batal ditolak `409` | Terpenuhi | `MembatalkanKejadianYangSudahBatal_Ditolak409` |
| 5. Pencatatan ulang setelah pembatalan menunjuk kejadian yang digantikannya | Terpenuhi | `PencatatanUlangSetelahPembatalan_MenunjukKejadianYangDigantikan` |
| 6. **Tidak ada** jalur yang menyunting waktu atau peran kejadian | Terpenuhi | `TidakAdaEndpointYangMenyuntingWaktuMaupunPeranVisite` dan `ControllerVisiteTidakMenyediakanEndpointPenghapusan` |
| 7. Agregasi tagihan tidak mengubah, menggabungkan, maupun menghapus kejadian klinis | Terpenuhi | `HanyaClinicalManagementYangMenyentuhKejadianVisite` |

### Definition of Done

| Butir | Status |
| --- | --- |
| Ketujuh acceptance criteria terbukti | ✅ |
| Architecture test hijau | ✅ Tiga uji arsitektur |
| Laporan menunjukkan riwayat berisi baris batal beserta alasannya | ✅ Bagian 2.2 dan tabel verifikasi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan build baru |
| **Cara kriteria 7 dibuktikan** | `RWI-AC-156` menuntut riwayat klinis tidak berubah setelah agregasi tagihan. Kebijakan agregasi tarif visite **belum ada**, sehingga tidak ada perilaku yang dapat diuji hari ini. Yang diuji karena itu adalah batasnya pada tingkat arsitektur: selama tidak satu pun tipe di luar `ClinicalManagement` dapat menyentuh kejadian visite, Billing tidak dapat menggabungkan dua kejadian menjadi satu walaupun tagihannya digabung. Ketika kebijakan agregasinya kelak dibuat, ia harus menggabungkan pada sisi tagihan |
| Masalah yang diketahui | Penautan dokumen memperlakukan nilai kosong sebagai "jangan ubah", bukan "lepaskan tautan". Belum ada permukaan untuk **melepas** tautan yang sudah terpasang. Keadaannya belum diminta kontrak mana pun; bila kelak dibutuhkan, ia perlu bentuk permintaan yang membedakan "kosong" dari "hapus" |
| Risiko tersisa | Pembatalan tidak dibatasi hanya kepada pencatat kejadian; siapa pun pemegang butir `Cancel` dapat membatalkannya. Itu **disengaja** dan sesuai `permission-audit-matrix.md` bagian 2: kejadian salah catat bisa ditemukan setelah dokternya pulang, sehingga supervisor klinis juga memegang butir itu. Jejaknya tetap utuh karena pembatal dan alasannya tersimpan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; tidak ada stage, commit, maupun push |
| Langkah berikutnya | `BE-RWI-051` memakai kejadian visite ini sebagai tautan opsional pada tindakan dokter |
