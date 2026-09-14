# Laporan Perubahan Frontend — `FE-RAD-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-11` |
| Judul | Pengesahan dan perilisan bacaan |
| Epic | `EPIC RAD-02` |
| Requirement | `FR-RAD-011`, `FR-RAD-012` (penomoran blueprint) |
| Decision | `RAD-DEC-003`, `RAD-DEC-015` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-STATE-001` bagian 3; `RAD-PERM-001` bagian 5.1 |
| Acceptance criteria | `AC-1`, `AC-2` |
| Test yang diminta roadmap | `UAT-04` radiolog mengesahkan sendiri berhasil; `UAT-05` residen mengesahkan sendiri ditolak; `UAT-11` dua radiolog bersamaan |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 5 butir 1 — tombol Sahkan wajib disembunyikan atau dinonaktifkan bagi penulis draf yang bukan radiolog |
| Dependency | `FE-RAD-10` **selesai**; `BE-RAD-08` **selesai dengan penghalang terbuka** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini dan baris status roadmap |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-14 |
| Status | **Selesai.** 14 unit test baru lulus; 890 unit test repository lulus, 0 gagal. **Tidak dapat dipakai siapa pun** selama penghalang bagian 3 terbuka |

---

## 1. Ketentuan mengikat: tombol Sahkan bagi penulis bukan-radiolog

`RAD-ARCH-FE-001` bagian 5 butir 1, menegakkan `RAD-DEC-003` dan `RAD-PERM-001` bagian 5.1.

**Dipenuhi tanpa menyentuh daftar kewenangan sama sekali.** Itu keputusan yang menentukan.
`isSelfValidationBlocked` hanya perlu dua hal yang sudah ada pada balasan:

```js
penulisVersi === pelaku && authorRoleSnapshot !== "Radiologist"
```

**Mengapa bukan lewat `usePermission`.** Hook itu mengembalikan `allowed` bernilai **benar
selama daftarnya belum diketahui** — sengaja, supaya satu permintaan yang gagal tidak menutup
jalan bagi petugas yang berhak. Menggantungkan ketentuan ini padanya berarti tombol Sahkan
sempat tampil justru pada saat yang dilarang, yaitu pada render pertama sebelum daftar
kewenangan tiba. Aturan pengesahan sendiri tidak bergantung pada kewenangan siapa pun; ia
bergantung pada **siapa yang menulis draf ini dan dengan peran apa**, dan keduanya sudah terbawa
`RadReportVersionResponse`.

**Yang dinilai adalah peran saat draf ditulis, bukan kewenangan hari ini.** Residen yang kemudian
menjadi dokter radiolog tetap terhalang atas draf lamanya, karena `AuthorRoleSnapshot`
dibekukan pada versinya. Layar mengikuti aturan yang sama supaya tombolnya tidak pernah
menawarkan sesuatu yang pasti ditolak server. `S5` menguncinya.

**Disembunyikan, dan sebabnya dinyatakan.** Tombol yang hilang tanpa keterangan membuat petugas
mengira layarnya rusak. Yang tampil adalah kalimat yang **sama persis** dengan jawaban server —
*"Draf yang Anda tulis harus disahkan dokter radiolog."* — sehingga bunyinya tidak berubah
antara sebelum dan sesudah tombol ditekan.

**Penyembunyian ini bukan authorization, dan tidak diperlakukan begitu di mana pun.** Setiap
tindakan tetap dikirim ke server yang menilai ulang seluruhnya. Uji `S6` mengunci sikap itu
secara tegas: ketika identitas pelaku tidak diketahui, layar **tidak menebak** dan tombolnya
tetap ditawarkan — karena menyembunyikan atas dasar tebakan justru menutup jalan bagi radiolog
yang berhak, sementara membiarkannya berakhir pada `403` yang terlihat dan dapat dilaporkan.

---

## 2. `UAT-11` — dua radiolog menekan Sahkan bersamaan

Yang terjadi bukan yang biasa disangka. `ValidateAsync` dibungkus:

```csharp
BeginTransactionAsync(IsolationLevel.Serializable)
ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey])
```

Kunci penasihat itu membuat permintaan kedua **menunggu**, bukan berjalan bersamaan. Setelah
gilirannya tiba, versinya sudah berstatus `Validated`, sehingga pemeriksaan pertama
`ValidateAsync` menolaknya — `409 RAD_INVALID_TRANSITION`, *"Hanya draf yang belum disahkan yang
dapat disahkan; bacaan ini berstatus sudah disahkan."*

Jadi **tidak ada pengesahan ganda, dan tidak ada galat konkurensi basis data**. Yang perlu
ditangani layar adalah penolakan yang menyebut keadaan terbaru. Layar menampilkannya sebagai
bentrok — pesan server apa adanya, ditambah satu kalimat langkah berikutnya, plus tombol Muat
ulang. Tidak ada pengiriman ulang otomatis; itu keputusan petugas.

`S10` mengunci perilakunya dari sisi layar.

---

## 3. Temuan yang menentukan: tidak ada yang dapat mengesahkan apa pun

> **KOREKSI 2026-09-14 — temuan bagian ini KELIRU.** Penanda `RadReport : ActAsRadiologist`
> **sudah dapat diberikan** sejak 2026-09-11: `AccessMenuSeeder` mendaftarkannya lewat
> `PenandaTanpaEndpointYangDidaftarkan`, dipanggil `EnsurePenandaTanpaEndpoint` baris 137.
> Penutupannya tercatat pada `approval-requests/2026-09-11-keputusan-empat-penghalang.md`
> bagian 1, **tiga hari sebelum laporan ini ditulis.**
>
> **Sebab kekeliruannya:** keadaan penghalang dibaca dari dokumen blueprint yang belum
> disegarkan, bukan dari source. Yang berlaku adalah source — dan governance memang sudah
> menyatakannya demikian.
>
> Yang tersisa bukan pekerjaan kode melainkan pekerjaan Administrator: mencentang penanda itu
> pada posisi yang memang dokter radiolog. Ditetapkan `RAD-DEC-017` pada 2026-09-14 —
> **posisi Dokter Radiologi**.
>
> Keterangan di bawah ini dibiarkan apa adanya sebagai catatan keadaan yang dipercaya saat
> `FE-RAD-11` dikerjakan. Rancangan layarnya **tidak terpengaruh**: ia memang tidak pernah
> bergantung pada daftar kewenangan, dan itu masih benar.

`HasRadiologistAuthorityAsync` dijawab penanda `RadReport : ActAsRadiologist`, yang menurut
keterangannya sendiri **sengaja tidak menempel pada satu endpoint pun**:

> *Penanda ini sengaja tidak menempel pada satu endpoint pun: ia menjawab "dihitung sebagai
> radiolog", sedangkan `RadReport : Validate` menjawab "boleh mencoba mengesahkan".*

Pemisahan itu benar secara desain. Persoalannya ada pada penyemaian: `AccessMenuSeeder` hanya
mendaftarkan pasangan kewenangan yang menempel pada endpoint. Penanda tanpa endpoint karena itu
**tidak pernah terdaftar, dan tidak dapat diberikan kepada peran mana pun.**

Akibatnya `HasRadiologistAuthorityAsync` mengembalikan `false` untuk **setiap akun**, sehingga:

- **Sahkan selalu ditolak** `403 RAD_VALIDATOR_NOT_RADIOLOGIST`.
- **Rilis selalu ditolak** `403` dengan pesan yang setara.
- **Menulis draf sebagai `Radiologist` selalu ditolak** `403 RAD_AUTHOR_ROLE_NOT_PERMITTED`.
- **Mengosongkan peran penulis selalu ditolak** `400 RAD_AUTHOR_ROLE_REQUIRED`, karena "ikuti
  kewenangan saya" hanya diterima bagi yang terdaftar radiolog.

Ini penghalang yang sama yang dicatat `BE-RAD-08` dan `BE-RAD-09`, dan ia **masih terbuka**.
Task ini membangun layarnya dengan benar, tetapi **tidak ada seorang pun yang dapat memakainya**
sampai penandanya dapat diberikan.

### Yang dikerjakan menghadapi itu

**Dinyatakan, bukan disembunyikan.** `usePermission("RadReport", "ActAsRadiologist")` dipakai
**bukan untuk menyembunyikan tombol**, melainkan untuk menerangkan: ketika daftar kewenangan
sudah diketahui dan penandanya memang tidak ada, layar mengatakannya lebih dulu — *"Penanda
`RadReport : ActAsRadiologist` belum terdaftar pada sistem kewenangan... Ini persoalan
konfigurasi, bukan kekeliruan Anda."*

Tanpa itu, seorang radiolog akan menekan Sahkan, menerima "Hanya dokter radiolog yang boleh
mengesahkan hasil bacaan", dan menyimpulkan akunnya salah — padahal yang kurang ada di sisi
konfigurasi. Penanda sengaja dituntut `loaded` lebih dulu: selama daftarnya belum diketahui,
`allowed` bernilai benar dan tidak membuktikan apa pun.

**Perbaikannya milik backend** dan **perlu keputusan pemilik modul**: mendaftarkan penanda tanpa
endpoint ke `AccessMenuSeeder`, atau menyediakan jalur penyemaian tersendiri baginya.

---

## 4. Koreksi atas `FE-RAD-10` yang dikerjakan di sini

Bawaan **"Ikuti kewenangan saya"** pada pemilih peran penulis akan **selalu gagal** selama
penanda radiolog belum dapat diberikan — `CreateDraftAsync` menolaknya `400
RAD_AUTHOR_ROLE_REQUIRED`. Keterangan isiannya diperbaiki supaya penolakan itu tidak datang
sebagai kejutan: pilih peran Anda; kosongkan **hanya** bila Anda terdaftar sebagai dokter
radiolog.

Ditambah pula **keterangan langkah berikutnya atas penolakan simpan**, dipilih menurut **kode
galat**, bukan dengan mengurai kalimat. `RadReportController` menyertakan `{ Code }` pada setiap
penolakan justru untuk itu; mengurai kalimat akan patah begitu satu huruf pada pesannya
disunting. `S12` dan `S13` mengunci pemetaannya, termasuk sikap untuk kode yang tidak dikenal:
**tidak menghasilkan tebakan**, hanya pesan server saja.

Satu koreksi kecil lain: kode galat ternyata berada pada ruas **`errors`**, bukan `data` —
`ApiResponse<T>.Fail(statusCode, message, errors)` menaruhnya di sana. Dibaca dari tempat yang
benar setelah `ApiResponse.cs` diperiksa.

---

## 5. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `tests/unit/rad-report-validation-rules.test.mjs` | Baru — 14 test; `UAT-04`, `UAT-05`, `UAT-11`, dan ketentuan mengikat |
| `src/lib/hooks/health-services/radiology-management/rad-report-rules.js` | Diubah — `isSelfValidationBlocked`, `canAttemptValidate`, `canAttemptRelease`, `describeValidateBlock`, `describeVersionStatus`, `describeActionFailure`, `getAuthorRoleSnapshot`, `RAD_REPORT_ERROR_CODE` |
| `src/lib/hooks/health-services/radiology-management/use-rad-report-draft.jsx` | Diubah — tindakan `sahkan` dan `rilis`, pembacaan kode galat, tiga pemeriksaan kewenangan |
| `src/components/view/…/rad-reports/rad-report-draft-view.jsx` | Diubah — bagian pengesahan dan perilisan beserta dua konfirmasi |
| `src/lib/constants/…/rad-report-constants.jsx` | Diubah — `RAD_REPORT_ACTION_COPY`; keterangan peran penulis diperbaiki |
| `tests/unit/rad-report-storage-guard.test.mjs` | Diubah — `rad-report-constants.jsx` ditambahkan ke berkas yang dijaga |

**Tidak ada route baru** — layar pengesahan tumbuh di layar draf `FE-RAD-10`, karena yang
mengesahkan perlu membaca isinya lebih dulu. Tidak ada potongan Redux baru, tidak ada base
component baru, tidak ada entri menu baru, dan tidak ada pemanggilan Axios baru:
`validateRadReport` dan `releaseRadReport` sudah ada sejak `FE-RAD-01`.

---

## 6. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-report-validation-rules.test.mjs` | **14 lulus, 0 gagal** |
| `node --test tests/unit/rad-report-storage-guard.test.mjs` | **5 lulus** — ketentuan mengikat `FE-RAD-10` tetap terjaga setelah berkasnya disunting |
| `node --test tests/unit/` (seluruh repository) | **890 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 3 warning** — seluruhnya `react-hooks/set-state-in-effect` yang sudah ada sebelumnya pada `use-rad-worklist`, `use-rad-order-form`, dan `use-rad-report-draft`. **Tidak ada warning baru** |
| `npm run build` | **Compiled successfully in 38.3s.** Tidak ada route baru, sesuai rancangan |

Cakupan test: `UAT-04` pada `S1`; `UAT-05` pada `S2` dan `S3`; `UAT-11` pada `S10`; ketentuan
mengikat pada `S2` sampai `S7`, termasuk peran yang dibekukan (`S5`), identitas tak dikenal
(`S6`), dan perbandingan GUID lintas kapitalisasi (`S7`).

**`MANUAL TEST: NOT FEASIBLE`.** Dua penghalang, dan keduanya mutlak. **Pertama**, tidak ada satu
pun akun yang dapat memegang `RadReport : ActAsRadiologist` — bagian 3 — sehingga Sahkan dan
Rilis pasti ditolak `403` di lingkungan mana pun. **Kedua**, agar ada bacaan untuk disahkan,
sebuah study harus mencapai `QualityAccepted`; itu mustahil selama belum ada aturan keselamatan
`Active` (`RAD-OPEN-011`). Seluruh jalur karena itu dikunci unit test terhadap perilaku yang
dibaca langsung dari `RadReportService`, `RadReportController`, dan `ApiResponse.cs`.

---

## 7. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Menyembunyikan tombol berdasarkan `ActAsRadiologist` | Akan menyembunyikan tombol bagi **semua orang** tanpa menerangkan apa pun. Dipakai menerangkan, bukan menyembunyikan — lihat bagian 3 |
| Menonaktifkan tombol Sahkan alih-alih menyembunyikannya | Ketentuan mengikat mengizinkan keduanya. Disembunyikan karena tombol mati yang menetap di layar tetap mengundang percobaan berulang saat pasien menunggu |
| Layar koreksi dan riwayat versi lengkap | Milik `FE-RAD-12` |
| Tombol Sahkan/Rilis pada daftar bacaan | Yang mengesahkan wajib membaca isinya lebih dulu. Daftar sengaja tidak membawa isi bacaan |
| Pengiriman ulang otomatis saat `409` | Keputusan petugas, bukan layar |

---

## 8. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **~~`RadReport : ActAsRadiologist` tidak dapat diberikan~~** | **KELIRU — dikoreksi 2026-09-14.** Penandanya sudah dapat diberikan sejak 2026-09-11; sisanya pekerjaan Administrator. Ditetapkan pada posisi **Dokter Radiologi** oleh `RAD-DEC-017` |
| **Tidak ada aturan keselamatan `Active`** | `RAD-OPEN-011` terbuka. Tidak satu pun study mencapai `QualityAccepted`, sehingga tidak ada bacaan yang lahir sama sekali |
| **`EnsurePendingReportAsync` tanpa pemanggil** | Temuan `FE-RAD-10`, masih terbuka |
| **Empat transisi study tanpa endpoint** | Temuan `FE-RAD-09`, masih terbuka |
| **Tiga enum tidak terbit pada metadata study** | Temuan `FE-RAD-09`, masih terbuka |
| **Study terkunci saat aturan keselamatan berubah** | Temuan `FE-RAD-08`, masih terbuka |
| **Kontrak `409` vs jalur API `422`** pada `rad-studies` | Temuan `FE-RAD-08`, masih terbuka |
| **`HoldAsync` menerima status terminal** | Temuan `FE-RAD-06`, masih terbuka |
| **Daftar kerja tanpa identitas pasien** | Temuan `FE-RAD-07`, masih terbuka |

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 1 berkas baru, 5 diubah. 14 test baru lulus; 890 test repository lulus; lint 0 error, tanpa warning baru; build lulus. Ketentuan mengikat bagian 5 butir 1 dipenuhi **tanpa bergantung pada daftar kewenangan**, karena `usePermission` sengaja membolehkan selama daftarnya belum diketahui. `UAT-11` ternyata diselesaikan kunci penasihat Postgres, bukan galat konkurensi. Temuan utama: `RadReport : ActAsRadiologist` tidak dapat diberikan kepada peran mana pun, sehingga pengesahan dan perilisan pasti ditolak untuk setiap akun. | `draft` |
