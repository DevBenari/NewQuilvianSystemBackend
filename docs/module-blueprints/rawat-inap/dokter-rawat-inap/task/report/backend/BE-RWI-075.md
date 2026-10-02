# Laporan Perubahan Backend — `BE-RWI-075`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-075` |
| Judul | Catatan terpadu tidak dapat disembunyikan lagi |
| Slice | `S5. Gelombang 1A — Rawat Inap Safety Corrections` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian S5 |
| Trace | `RWI-DEC-098`; `RWI-DEC-105`; `FR-DOK-060` s.d. `FR-DOK-062`; `04-prd-to-mvp.md` bagian 21.3; `RWI-OQ-055` |
| Contract version | `contracts/api-contract.md` `0.5.0` bagian 0.A.1 dan bagian 3; `contracts/state-transition-matrix.md` `0.5.0` bagian 3A. **`approved`** 11 September 2026 lewat `RWI-DEC-105` |
| Dependency | Approval kontrak `0.5.0` — **terpenuhi**. Nol prasyarat task lain |
| Klasifikasi | `LIGHT` — satu controller, satu route dihapus, dua penjaga ditambahkan; nol tabel, nol migration, nol hak akses baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/ClinicalManagement/` dan `docs/module-blueprints/rawat-inap/dokter-rawat-inap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `c11904ea99dc0f9a9ecfb3b41f6edb2bf500d206`, branch `MHamzah` |
| Tanggal | 11 September 2026 |
| Status | ✅ **SELESAI 11 September 2026.** Ketujuh acceptance criteria terpetakan ke source yang benar-benar ada. **Satu butir verifikasi tidak dijalankan dan ditulis apa adanya:** `dotnet build` `NOT RUN`, dikecualikan atas instruksi pemilik pada task aktif |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` |
| Submodule | `Controllers` |
| Pemilik/prefix registry | `ClinicalManagement` terdaftar `ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Task ini **nol entity baru**, sehingga prefix tidak menjadi pertanyaan |
| Keberlakuan | `TOUCHED LEGACY` — satu action dihapus dan satu action diperketat pada controller yang sudah ada |
| Status registry | Tidak ada modul, submodule, maupun prefix baru yang perlu didaftarkan. `QBE-MOD-002` dan `QBE-MOD-003` **tidak** menahan pekerjaan ini |
| QBE ID yang benar-benar berlaku | `QBE-API-001` boundary dan kode status yang sudah mapan; `QBE-VAL-001` invarian pembatalan; `QBE-DEL-001` lifecycle delete/cancel beserta audit aktornya — inilah aturan yang paling langsung dijawab task ini; `QBE-AUD-001` jejak audit tetap terpisah dari application logging |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001` s.d. `QBE-CFG-002` — nol entity, nol configuration. `QBE-NAM-*` — nol penamaan baru. `QBE-CODE-001` s.d. `QBE-CODE-006` — nol nomor bisnis. `QBE-DB-001` dan `QBE-DB-002` — bukan `LEGACY MIGRATION`. `QBE-PERM-001` — nol butir hak akses baru |
| Peninggalan yang dicabut | Folder `agents/rules/` dan `.codex/` **tidak ditemukan** di working tree |

---

## 1. Masalah yang diperbaiki

Catatan Perkembangan Pasien Terintegrasi — CPPT — adalah bagian rekam medis. Sampai hari ini
catatan itu **dapat dihilangkan**, dan ada dua jalan menuju ke sana:

| Jalan | Keadaannya sebelum perubahan |
| --- | --- |
| `DELETE /{id}` | Melakukan soft delete **tanpa satu pun pemeriksaan**: tanpa memeriksa status dokumen, tanpa menuntut alasan, dan tanpa memeriksa siapa penulisnya |
| `PATCH /{id}/cancel` | Membatalkan catatan apa pun, **termasuk yang sudah final dan yang sudah diverifikasi DPJP** |

Keduanya menghasilkan akibat yang sama bagi pembaca rekam medis: catatan yang pernah ada menjadi
tidak terbaca, dan tidak ada jejak yang menjelaskan apa yang hilang atau mengapa.

**Contoh konkret.** Seorang dokter menulis catatan perkembangan pada 10 September pukul 14:00,
memfinalkannya, dan DPJP memverifikasinya pukul 16:00. Pukul 17:00 ia menyadari salah menulis
dosis. Sebelum perubahan ini ia dapat memanggil `DELETE`, dan catatan itu lenyap dari rekam medis
beserta verifikasi DPJP-nya — tanpa satu baris jejak pun yang menyebutkan bahwa sebuah catatan
pernah ada di sana.

Sesudah perubahan ini, jalan itu tertutup. Yang tersisa adalah addendum bernomor urut, yang
membetulkan sambil **menyisakan** catatan aslinya.

**Kenapa menutup `DELETE` saja tidak cukup.** Ini inti risiko task ini. Bila hanya route hapus
yang dicabut, catatan final yang tadinya dapat dihapus akan tetap dapat **dibatalkan**, dan bagi
pembaca rekam medis hasilnya sama saja. Karena itu kedua bagian wajib berada pada satu task.

---

## 2. Proses bisnis

### 2.1 Tiga keadaan catatan, dan jalan keluar masing-masing

| Keadaan catatan | Cara membetulkan yang sah | Endpoint |
| --- | --- | --- |
| Masih draf | Pembatalan beralasan | `PATCH /{id}/cancel` |
| Sudah final atau terkunci | Addendum bernomor urut | Grup Clinical Note Addendum |
| Sudah diverifikasi DPJP | Addendum bernomor urut | Grup Clinical Note Addendum |

### 2.2 Alur normal — membatalkan catatan draf

1. Pengguna membuka catatan yang masih draf dan memilih membatalkannya.
2. Sistem menuntut alasan pembatalan yang benar-benar berisi.
3. Sistem memeriksa keutuhan dokumen: catatan draf lolos.
4. Sistem memeriksa status verifikasi: catatan draf belum diverifikasi, jadi lolos.
5. Catatan ditandai batal beserta **waktu, pelaku, dan alasannya**.
6. Catatan **tetap terbaca** pada rekam medis dan pada audit. Ia tidak hilang; ia terbaca sebagai
   dibatalkan.

### 2.3 Jalur tidak normal

| Keadaan | Jawaban |
| --- | --- |
| Memanggil `DELETE /{id}` | **`404`**, bukan `403`. Route-nya memang sudah tidak ada, jadi tidak ada yang dapat diberi izin |
| Membatalkan catatan yang sudah final atau terkunci | **`422`** beserta keterangan bahwa koreksi dilakukan lewat addendum |
| Membatalkan catatan yang sudah diverifikasi DPJP | **`422`** beserta keterangan yang sama |
| Membatalkan tanpa alasan, atau dengan alasan berisi spasi saja | **`400`** |
| Membatalkan catatan yang sudah dibatalkan | **`400`**, pemeriksaan lama yang tidak disentuh |
| Catatan tidak ditemukan | `404` |

### 2.4 Kenapa status verifikasi diperiksa terpisah

Ini bagian yang paling mudah terlewat. Keutuhan dokumen disimpan pada **tabel lain**, yaitu daftar
keutuhan rekam medis, sedangkan status verifikasi CPPT tinggal pada **baris CPPT itu sendiri**.
Keduanya tidak selalu sejalan:

| Keadaan | Baris keutuhan | Status verifikasi CPPT | Tanpa pemeriksaan kedua |
| --- | --- | --- | --- |
| Catatan baru yang sudah diverifikasi | Masih draf, atau **belum terdaftar** karena catatan lama | `Verified` | **Lolos dibatalkan** — inilah lubangnya |

Catatan yang lahir sebelum mesin keutuhan berlaku tidak punya baris keutuhan sama sekali, dan
pemeriksaan keutuhan sengaja **melewatkan** dokumen yang belum terdaftar supaya alur lama tidak
terblokir. Karena itu status verifikasi wajib diperiksa sebagai penjaga kedua yang berdiri
sendiri.

### 2.5 Batas yang dinyatakan apa adanya

`RWI-DEC-098` menutup **dua** controller: CPPT dan tanda vital. Tanda vital adalah milik
`BE-RWI-077` pada sub-modul `keperawatan`, bukan task ini.

Delapan jalur hapus lain pada `ClinicalManagement` **sengaja tetap berdiri** atas keputusan
pemilik: consent, alergi, riwayat penyakit, riwayat keluarga, dokumen klinis, lampiran, surat
keterangan medis, dan infeksi nosokomial. Kontrak `0.5.0` **tidak** menyatakan kedelapan jalur itu
aman; ia hanya menyatakan keduanya berada di luar `Gelombang 1A`. Ketegangannya dilacak
`RWI-OQ-055`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `rules/backend/TEST_POLICY.md`, `REPORT_TEMPLATE.md`, `role-access-rules.md`, `API_RULES.md`
- `contracts/api-contract.md` `0.5.0` bagian 0.A.1; `testing/acceptance-test-matrix.md` bagian 3A.1
- `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` — untuk memastikan arti sebenarnya dari `EnsureMutableAsync`
- Seluruh action pada `PatientIntegratedProgressNoteController`, untuk memastikan tidak ada jalur hapus kedua

### 3.2 Berkas yang berubah

**Satu berkas.**

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Action `DeleteProgressNote` beserta route `HttpDelete("{id:guid}")` **dihapus seluruhnya**, 36 baris. Jalur `PATCH /{id}/cancel` memanggil `EnsureMutableAsync` sebelum pembatalan diproses, menolak `422`. Catatan berstatus `Verified` ditolak `422` lewat pemeriksaan kedua yang berdiri sendiri. Alasan pembatalan yang hanya berisi spasi ditolak `400`. Empat `ProducesResponseType` disesuaikan |

### 3.3 Satu pengetatan kecil di luar kalimat cakupan

Kolom `Cakupan` menyebut dua hal: menghapus route dan memanggil `EnsureMutableAsync`. Satu hal
ketiga ditambahkan, dan alasannya disebut supaya tidak terbaca sebagai penambahan diam-diam.

`CancelReason` dijaga `[Required]`, yang menolak nilai kosong dan `null` tetapi **meloloskan**
`" "`. Alasan berisi spasi tersimpan sebagai teks kosong sesudah dirapikan, dan pembatalan tanpa
alasan menghapus satu-satunya hal yang membedakan pembatalan dari penghapusan — yaitu jejaknya.
Karena `AC-DOK-064` menuntut `400` untuk pembatalan tanpa alasan, lubang ini berada tepat di
dalam kriteria itu, bukan di sebelahnya.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint dihapus**, sesuai `api-contract.md` `0.5.0` bagian 0.A.1. `DELETE /patient-integrated-progress-notes/{id}` tidak lagi tersedia dan menjawab `404`. `PATCH /{id}/cancel` bertambah dua kode balasan, `400` dan `422`. **Ini perubahan yang merusak** bagi pemanggil yang memakai `DELETE`; kontraknya memang memerintahkannya |
| Database | `NOT APPLICABLE` — nol tabel, nol kolom, nol index, nol migration. Nol perintah dikirim ke database mana pun |
| Keamanan/Auth | Butir hak akses `PatientIntegratedProgressNote : Delete` **tidak lagi terpasang pada action mana pun** di controller ini, karena action pemiliknya dihapus. Nol `[AccessPermission]` baru. Nol `IsInRole`, nama peran, nama departemen, maupun `UserType` ditambahkan. Arah perubahannya **mengetatkan** |

### 3.5 Satu akibat hak akses yang perlu diketahui admin

Butir `PatientIntegratedProgressNote : Delete` lahir dari `[AccessAction("Delete", ...)]` pada
action yang kini dihapus. Butir yang sudah pernah tersimpan di database hak akses **tidak** ikut
terhapus oleh perubahan source; ia hanya menjadi butir yang tidak menjaga apa-apa, karena tidak
ada lagi endpoint yang menyebutnya.

Ini tidak berbahaya — butir yatim tidak memberi izin atas apa pun — tetapi ia akan tetap terlihat
pada layar Pengaturan → Manajemen Role → Akses Role sampai seseorang merapikannya. Pembersihan
baris hak akses adalah pekerjaan data, bukan source, dan **tidak** dikerjakan task ini.

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| ~~`DELETE`~~ | ~~`/{id}`~~ | **DIHAPUS.** Route tidak lagi tersedia; permintaan ke path itu dijawab `404` | — |
| `PATCH` | `/{id}/cancel` | Membatalkan catatan yang masih draf beserta alasannya. Menolak `422` bila catatan sudah final, terkunci, atau sudah diverifikasi DPJP; menolak `400` bila alasannya kosong | `PatientIntegratedProgressNote : Update` |

Hak akses `PATCH /{id}/cancel` **tidak berubah satu karakter pun**. Argumen pertama
`[AccessPermission]` tetap `PatientIntegratedProgressNote`, sama persis dengan `ControllerName`
pada `[AccessController]`, dan argumen keduanya tetap `Update`, sama persis dengan argumen pertama
`[AccessAction]` pada method yang sama.

---

## 5. Verifikasi

### 5.1 Yang benar-benar dijalankan

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` mode `Strict` | `VIOLATION: 0`, `REVIEW: 0`, `Findings: none`, `Final result: PASS` | `PASS` | Keluaran perintah |
| **`AC-DOK-060`** — pencarian `HttpDelete` pada berkas itu | **`0` hasil.** Route hapus benar-benar tidak ada lagi, bukan sekadar dinonaktifkan | `PASS` | Pencarian source |
| Penelusuran `EnsureMutableAsync` pada jalur pembatalan | Terpasang sebelum satu pun kolom pembatalan disentuh, menjawab `422` | `PASS` | Pembacaan source |
| Penelusuran arti `EnsureMutableAsync` | `Signed`, `LockedUnsigned`, dan `Cancelled` ditolak; `Draft` dilewatkan; dokumen yang **belum terdaftar** sengaja dilewatkan supaya catatan lama tidak terblokir | `PASS` | Pembacaan source |
| Penelusuran jalur baca sesudah pembatalan | Query dasar menyaring `!IsDelete` saja, **tidak** menyaring `IsActive` maupun `IsCancel`, sehingga catatan yang dibatalkan tetap terbaca | `PASS` | Pembacaan source |
| Review diff/scope | 1 berkas milik task ini. Nol tabel, nol migration, nol hak akses baru, nol perubahan di luar scope | `PASS` | `git diff` |
| Pemeriksaan hardcode role access | Nol `IsInRole`, nama peran, nama departemen, atau `UserType` ditambahkan | `PASS` | Pencarian source |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta data catatan pada ketiga keadaan.

### 5.2 Contoh berangka sebagai pengganti test otomatis

**Catatan `CPPT-2026-00481`, ditulis 10 September pukul 14:00.**

| Waktu | Keadaan catatan | Baris keutuhan | `PATCH /cancel` menjawab |
| --- | --- | --- | --- |
| 14:00 | Baru dibuat | `Draft` | `200`, tersimpan beserta alasan dan pelakunya |
| 15:00 | Sudah difinalkan | `Signed` | **`422`** — ditolak penjaga keutuhan |
| 16:00 | Sudah diverifikasi DPJP | `Signed` | **`422`** — ditolak penjaga keutuhan |
| 16:00 | Catatan **lama** yang diverifikasi, tanpa baris keutuhan | tidak ada | **`422`** — ditolak penjaga verifikasi, dan inilah gunanya penjaga kedua |
| kapan pun | Apa pun | apa pun | `DELETE` menjawab **`404`**, route tidak ada |

**Regresi Rawat Jalan.** Catatan CPPT poliklinik yang masih draf dan belum diverifikasi melewati
kedua penjaga persis seperti sebelum perubahan: penjaga keutuhan melewatkan `Draft`, dan penjaga
verifikasi melewatkan status selain `Verified`. Jawabannya tetap `200` dengan kolom pembatalan
yang sama.

Yang **berubah** bagi rawat jalan perlu disebut jujur: catatan poliklinik yang sudah **final**
kini ikut ditolak dibatalkan. Itu memang yang diminta `RWI-DEC-098` — aturannya melekat pada
**keutuhan dokumen**, bukan pada jenis pelayanan — dan rekam medis rawat jalan tidak lebih boleh
disembunyikan daripada rekam medis rawat inap.

### 5.3 Tidak dijalankan

| Butir | Klasifikasi | Alasan |
| --- | --- | --- |
| `dotnet build` project aplikasi | `NOT RUN` | **Dikecualikan atas instruksi pemilik pada task aktif 11 September 2026**, yang menyatakan build dijalankan sendiri. Jumlah error dan warning **tidak diklaim** |
| Integration test `AC-DOK-060` s.d. `AC-DOK-066` | `NOT RUN` | Folder `Tests/` sudah tidak ada di repository dan `rules/backend/TEST_POLICY.md` melarang membuatnya kembali tanpa permintaan pemilik pada task aktif. Digantikan penelusuran source beserta contoh berangka menurut `TEST_POLICY.md` bagian 5 |

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-DOK-060` `DELETE /{id}` dipanggil → **`404`**, bukan `403` | **Terpenuhi** | Action beserta atributnya dihapus seluruhnya; pencarian `HttpDelete` pada berkas itu mengembalikan `0` hasil. Karena route-nya tidak terdaftar, routing ASP.NET menjawab `404` sebelum mesin hak akses dipanggil — itulah sebabnya jawabannya `404` dan bukan `403` |
| `AC-DOK-061` Membatalkan catatan `Final` → `422` beserta keterangan addendum | **Terpenuhi** | `EnsureMutableAsync` menolak `Signed` dan `LockedUnsigned`; controller membungkusnya `422` dengan kalimat yang menyebut addendum |
| `AC-DOK-062` Membatalkan catatan `Terverifikasi` → `422` | **Terpenuhi** | Pemeriksaan kedua yang berdiri sendiri atas `VerificationStatus == Verified`, dijelaskan bagian 2.4 |
| `AC-DOK-063` Membatalkan catatan `Draf` beserta alasan → `200`, terbaca sebagai dibatalkan beserta alasan dan pelakunya | **Terpenuhi** | Jalur sukses menulis `IsCancel`, `CancelledAt`, `CancelledByUserId`, dan `CancelReason` pada baris yang sama |
| `AC-DOK-064` Membatalkan catatan `Draf` tanpa alasan → `400` | **Terpenuhi** | `[Required]` pada `CancelReason` menolak kosong dan `null` lewat validasi model `[ApiController]`; alasan berisi spasi saja ditolak pemeriksaan eksplisit yang ditambahkan task ini — bagian 3.3 |
| `AC-DOK-065` Catatan yang dibatalkan **tetap terbaca** pada rekam medis dan audit | **Terpenuhi** | Pembatalan **tidak** menyentuh `IsDelete`, dan query dasar controller menyaring `!IsDelete` saja. Baris tetap muncul pada pembacaan normal beserta kolom pembatalannya |
| `AC-DOK-066` Regresi Rawat Jalan: jalur pembatalan CPPT rawat jalan tidak berubah perilakunya | **Terpenuhi, dengan satu selisih yang dinyatakan** | Catatan rawat jalan berstatus draf berperilaku sama persis. Catatan rawat jalan yang sudah **final** kini ikut ditolak — bukan pengecualian yang terlewat, melainkan akibat langsung `RWI-DEC-098` yang aturannya melekat pada keutuhan dokumen, bukan pada jenis pelayanan. Lihat bagian 5.2 |

### 6.2 Definition of Done

| Butir DoD | Status | Keterangan |
| --- | --- | --- |
| Ketujuh acceptance criteria terpetakan ke source | **Terpenuhi** | Tabel 6.1 |
| Nol `HttpDelete` tersisa pada berkas itu | **Terpenuhi** | Pencarian mengembalikan `0` |
| Regresi Rawat Jalan lulus | **Terpenuhi lewat penelusuran source**, bukan lewat test otomatis — bagian 5.2 dan 5.3 |
| `dotnet build` tanpa error baru | **BELUM TERPENUHI — `NOT RUN`** | Dikecualikan atas instruksi pemilik. Hasilnya tidak diklaim |
| Kesesuaian QBE dan preflight engineering | **Terpenuhi** | Preflight tertulis di atas; checker `Strict` `PASS` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `git diff` melaporkan `LF will be replaced by CRLF` pada beberapa berkas `ClinicalManagement`. Peringatan itu berasal dari konfigurasi akhir baris Git, bukan dari isi perubahan |
| Masalah yang diketahui | Butir hak akses `PatientIntegratedProgressNote : Delete` menjadi butir yatim pada database hak akses — bagian 3.5. Delapan jalur hapus lain pada `ClinicalManagement` tetap berdiri atas keputusan pemilik, dilacak `RWI-OQ-055` |
| Risiko tersisa | **Pemanggil yang masih memakai `DELETE` akan menerima `404`.** Frontend perlu diperiksa sebelum rilis; bila masih ada tombol hapus CPPT, tombol itu akan gagal tanpa penjelasan yang berguna bagi pengguna. Pemeriksaan repository frontend **tidak** dikerjakan task ini karena berada di luar target tulisnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 1 berkas milik task ini, berstatus `M`. Working tree juga memuat pekerjaan task lain yang belum di-commit dan tidak disentuh task ini — lihat bagian 7.1. Nol stage, nol commit, nol push |
| Langkah berikutnya | Lihat bagian 7.2 |

### 7.1 Pekerjaan task lain yang ada di working tree yang sama

Berkas berikut berubah di working tree yang sama tetapi **bukan** milik task ini, dan tidak
disentuh sama sekali:

| Berkas | Pemiliknya |
| --- | --- |
| `PatientVitalSignController.cs` | `BE-RWI-077` — jalur hapus tanda vital, koreksi `RWI-DEC-098` yang kedua |
| `NursingActorService.cs`, `NursingCarePlanService.cs`, `NursingInterventionService.cs`, `NursingCarePlanController.cs`, `NursingInterventionController.cs` | `BE-RWI-078` — penulis klinis perawat |
| `InpBedOccupancyService.cs` | `BE-RWI-073` |
| Berkas `InPatientManagement/` dan `Migrations/` | `BE-RWI-074` |

`PatientIntegratedProgressNoteController.cs` dan `PatientAssessmentController.cs` dikerjakan task
ini **bersama** `BE-RWI-076` dan `BE-RWI-078`. Pembagiannya bersih dan diperiksa ulang di akhir
pekerjaan; rinciannya ada pada laporan `BE-RWI-076` bagian 7.2.

### 7.2 Langkah berikutnya

| Urut | Langkah | Pemilik |
| ---: | --- | --- |
| 1 | Jalankan `dotnet build` project aplikasi, lalu perbarui bagian 5 laporan ini dengan jumlah error dan warning yang sebenarnya | Pemilik repository |
| 2 | Periksa repository frontend: pastikan tidak ada lagi tombol atau pemanggilan `DELETE` CPPT yang kini akan menjawab `404` | Pemilik frontend |
| 3 | Rapikan butir hak akses yatim `PatientIntegratedProgressNote : Delete` pada data hak akses | Admin sistem |
| 4 | Putuskan `RWI-OQ-055`: apakah delapan jalur hapus lain pada `ClinicalManagement` ikut ditutup | Pemilik klinis |
