# Kontrak API — Rekonsiliasi MRN Pilot RSMMC

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| Versi kontrak | **`1.1.0`** |
| Status kontrak | **`approved`** — final 8 Oktober 2026. Tidak ada butir `PROPOSED` atau terbuka |
| Riwayat versi | `1.0.0` (8 Oktober 2026): bentuk awal; `PAT-OQ-002`–`004` masih `PROPOSED`. `1.1.0` (8 Oktober 2026): `PAT-OQ-002`–`004` final, ditambah `PAT-OQ-005`–`007`; nama status konflik QR dikunci `CONFLICT`. **Bentuk request dan route tidak berubah** |
| Task | `BE-PAT-MIG-001` |
| Snapshot source | Branch `QuilvianStaDeploy`, commit `103b45ccd5f0d9e2cbacff588540d8fe3d52e706` |
| Status endpoint | **Rencana (belum tersedia)** — belum ada di source |

Dokumen ini mengunci bentuk endpoint, aturan pemilihan pasien, kontrak QR, dan batas data yang
boleh berubah. Kutipan baris source di bawah diambil dari snapshot di atas. Bila snapshot
berubah sebelum implementasi dimulai, baris-baris itu wajib diperiksa ulang.

---

## 1. Jenis endpoint

Endpoint ini **bukan** master data baru dan **bukan** transaksi yang punya siklus status. Ia aksi
admin sementara pada resource Patient yang sudah ada, dan hanya dipakai sekali untuk membereskan
715 pasien Pilot.

Akibatnya:

- sembilan endpoint baseline master data **tidak berlaku** untuk aksi ini;
- tidak ada `GET /options`, `PATCH /{id}/status`, atau `DELETE /{id}` yang menyertainya;
- endpoint ini dirancang untuk dihapus atau dinonaktifkan sesudah rekonsiliasi selesai. Kapan dan
  bagaimana itu dilakukan **belum diputuskan**, dan bukan bagian `BE-PAT-MIG-001`.

---

## 2. Endpoint

### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

Bukti grup dan base URL: `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs`
baris 42 (`[Route]`) dan baris 52 (`[Tags]`).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/admin/migration/rsmmc-pilot-mrn/reconcile` | **Rencana (belum tersedia).** Mensimulasikan atau menjalankan pemindahan MRN pasien Pilot ke nomor kanonik, paling banyak `limit` pasien per panggilan | `Patient : Update`, atau izin lain yang lebih ketat — lihat bagian 7 | Body: `batchId`, `dryRun`, `limit` | `ApiResponse<…>` berisi ringkasan dan daftar baris |

Path lengkapnya sama persis dengan rencana yang disetujui, sehingga **tidak ada selisih rute**.
Bila implementasi terpaksa memakai path lain, selisihnya wajib dicatat pada laporan task.

### 2.1 Masukan

| Field | Tipe | Wajib | Aturan | Contoh |
| --- | --- | --- | --- | --- |
| `batchId` | `Guid` | Ya | Harus sama dengan `batch_id` pada tabel perencana | `45eeefba-dd39-6831-1bd2-d986851f7c1f` |
| `dryRun` | `bool` | Tidak | Bila tidak dikirim, dianggap `true` (`PAT-OQ-003`). **Tidak boleh ada default implisit `false`** | `true` |
| `limit` | `int` | Tidak | Bawaan **25**. Hanya boleh **1 sampai 100**. Hanya menghitung pasien yang layak direkonsiliasi (`PAT-OQ-004`) | `25` |

**Jebakan default `dryRun`.** Pada C#, properti `bool DryRun { get; set; }` tanpa nilai awal
bernilai `false` ketika kolomnya tidak dikirim. Bentuk itu **dilarang**, karena lupa mengisi
kolom akan langsung mengubah data. Properti wajib bernilai `true` ketika kolomnya tidak dikirim.

| Body yang dikirim | `dryRun` yang berlaku |
| --- | --- |
| `{ "batchId": "…", "limit": 25 }` | `true` — simulasi |
| `{ "batchId": "…", "dryRun": true }` | `true` — simulasi |
| `{ "batchId": "…", "dryRun": false }` | `false` — eksekusi, hanya di `Staging` |

Contoh penolakan `limit`:

| `limit` yang dikirim | Hasil |
| ---: | --- |
| 0 | Ditolak — di bawah batas bawah 1 |
| 1 | Diterima |
| 25 | Diterima — nilai bawaan |
| 100 | Diterima — batas atas |
| 101 | Ditolak — di atas batas atas 100 |

### 2.2 Keluaran per baris

Setiap baris mewakili satu pasien, diurutkan menurut `legacy_pid` naik. Isi minimal yang
diwajibkan `AC-04`:

| Isi | Contoh dengan data samaran |
| --- | --- |
| `legacy_pid` | `100001` |
| `MstPatient.Id` | `3f0c…` (UUID pasien) |
| `PatientCode` | `PAT-RSMMC-00042` |
| Nama lengkap | `Budi Contoh` |
| MRN sekarang | `01-23-45-67` |
| MRN kanonik | `00-98-76-54` |
| Path QR sekarang | `/uploads/patient-qrcodes/01-23-45-67/qrcode.png` |
| Path QR rencana | `/uploads/patient-qrcodes/00-98-76-54/qrcode.png` |
| Status baris | lihat bagian 4.3 |

Nama field JSON mengikuti konvensi camelCase repository. Isi wajibnya terkunci; ejaan nama field
diputuskan saat implementasi dan dicatat pada laporan task.

### 2.3 Kode status

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Simulasi atau eksekusi selesai. Ringkasan dan daftar baris dikembalikan, termasuk pasien yang gagal bila proses berhenti di tengah |
| `400` | Isian tidak sah. Contoh: `limit` 0 atau 101, atau `batchId` kosong |
| `401` | Pengguna belum login |
| `403` | Pengguna tidak punya hak akses untuk aksi ini |
| Kode `4xx` yang jelas | Environment tidak diizinkan (bagian 6). Kode pastinya `DEV_DISCRETION`, dipilih saat implementasi dan dicatat pada laporan task |

---

## 3. Aturan memilih pasien

Pasien diproses hanya bila **seluruh** syarat `AC-01` terpenuhi:

1. `batch_id` pada tabel perencana sama dengan `batchId` pada permintaan;
2. `plan_status` pada tabel perencana bernilai `FINALIZED`;
3. `legacy_pid` pada crosswalk sama dengan `legacy_pid` pada tabel perencana;
4. `quilvian_patient_id` pada crosswalk terisi;
5. `MstPatient.Id` sama dengan `quilvian_patient_id`;
6. `MstPatient.MedicalRecordNumber` sekarang **berbeda** dengan `final_medical_record_number`.

MRN tujuan **hanya** diambil dari `final_medical_record_number`. Kolom
`crosswalk.normalized_mrn` tidak pernah dipakai sebagai tujuan (`AC-02`).

Pasien yang lolos syarat 1–5 disebut **set Pilot**. Setiap panggilan, simulasi maupun eksekusi,
memeriksa status **seluruh** set Pilot (`PAT-OQ-004`):

| Keadaan pasien pada set Pilot | Status | Dihitung dalam `limit`? | Diubah? |
| --- | --- | --- | --- |
| MRN masih berbeda dari MRN kanonik (lolos syarat 6) | Layak direkonsiliasi | **Ya** | Ya, pada `dryRun=false` |
| MRN kanonik, `QrCodePath` sesuai MRN kanonik | `ALREADY_RECONCILED` | Tidak | Tidak |
| MRN kanonik, `QrCodePath` **tidak** sesuai MRN kanonik | `QR_INCONSISTENT` | Tidak | **Tidak** — dilaporkan, tidak diperbaiki diam-diam |

`QR_INCONSISTENT` bukan `ALREADY_RECONCILED`. Rekonsiliasi umum ini tidak memperbaikinya;
perbaikannya, bila diperlukan, adalah keputusan terpisah.

Ringkasan keluaran memuat jumlah per status untuk seluruh set Pilot, dan daftar lengkap pasien
berstatus `QR_INCONSISTENT`.

**Contoh melanjutkan proses.** Ada 715 pasien dengan `limit` 25:

| Panggilan ke- | Yang diproses | Sisa yang belum kanonik |
| ---: | --- | ---: |
| 1 | 25 pasien dengan `legacy_pid` terkecil | 690 |
| 2 | 25 pasien berikutnya, karena 25 pertama sudah tidak lolos syarat 6 | 665 |
| … | … | … |
| 28 | 25 pasien | 15 |
| 29 | 15 pasien terakhir | 0 |
| 30 | Tidak ada; seluruhnya dilaporkan `ALREADY_RECONCILED` | 0 |

---

## 4. Proses bisnis

### 4.1 Ringkasan

| Unsur | Isi |
| --- | --- |
| **Tujuan** | 715 pasien Pilot berpindah ke MRN kanonik RSMMC tanpa kehilangan identitas, relasi, atau QR lama |
| **Pelaku** | Admin migrasi yang sudah login dan punya hak akses bagian 7. Yang menyetujui eksekusi: pemilik/leader, lewat otorisasi terpisah (`PAT-GATE-001`) |
| **Pemicu** | Admin memanggil endpoint — **bukan** bagian `BE-PAT-MIG-001`, butuh otorisasi terpisah |
| **Prasyarat** | Berjalan di environment yang diizinkan (bagian 6); tabel `migration.rsmmc_*` tersedia; tabel cadangan sudah berisi 715 baris (`PAT-FACT-025`) |
| **Hasil akhir** | Untuk setiap pasien yang berhasil: `MedicalRecordNumber` dan `QrCodePath` menunjuk nomor kanonik, QR baru tersedia, QR lama tetap ada |

### 4.2 Langkah per pasien saat `dryRun=false`

1. Sistem memastikan environment bernama persis `Staging` (bagian 6). Bila tidak, seluruh
   permintaan ditolak sebelum satu pasien pun dibaca.
2. Sistem memeriksa status seluruh set Pilot (bagian 3), lalu mengambil pasien layak berikutnya
   menurut `legacy_pid` naik.
3. Sistem memastikan MRN tujuan tidak sedang dipakai pasien lain. Bila dipakai, pasien itu
   dicatat sebagai konflik dan proses berhenti.
4. Sistem memeriksa path QR tujuan untuk MRN kanonik **sebelum** helper QR mana pun dipanggil
   (`PAT-OQ-005`). Bila artefaknya sudah ada: helper yang dapat menimpa **tidak dipanggil**, file
   yang ada tidak ditimpa, QR lama tidak disentuh, pasien itu dilaporkan berstatus `CONFLICT`, dan
   proses berhenti (`AC-13`).
5. Sistem membuat QR baru di folder MRN tujuan. Bila gagal, data pasien tidak diubah dan proses
   berhenti (`AC-11`).
6. Sistem membuka transaksi database untuk **satu pasien ini saja**, lalu mengubah empat field:
   `MedicalRecordNumber`, `QrCodePath`, `UpdateDateTime`, dan `UpdateBy`. Sesudah itu
   `SaveChanges` dan *commit*.
7. Bila langkah 6 gagal, sistem menghapus **hanya** QR baru dari langkah 5 secara *best-effort*
   (dicoba sebisanya; kegagalan menghapus dicatat, tidak menggagalkan laporan). QR lama tidak
   disentuh. Proses berhenti (`AC-12`).
8. Bila berhasil, sistem lanjut ke pasien berikutnya sampai `limit` tercapai.

Pada `dryRun=true`, langkah 1–4 dijalankan sebagai pemeriksaan saja. Langkah 5–7 **tidak
dijalankan**: tidak ada perubahan database dan tidak ada file yang ditulis (`AC-03`).

Langkah 3 dan 4 dianggap kegagalan tak terduga yang menghentikan proses. Ini tafsiran dari
`AC-13` dan `AC-26`, ditambah fakta audit bahwa konflik MRN tujuan berjumlah nol
(`PAT-FACT-016`). Konflik yang muncul berarti keadaan data sudah berbeda dari hasil audit, dan
harus diperiksa manusia lebih dulu.

### 4.3 Status baris

| Status | Kapan | Data berubah? | Terkunci? |
| --- | --- | --- | --- |
| `ALREADY_RECONCILED` | MRN sudah kanonik dan `QrCodePath` sesuai nomor itu. Tidak dihitung dalam `limit` | Tidak | **Ya** — `AC-14`, `PAT-OQ-004` |
| `QR_INCONSISTENT` | MRN sudah kanonik tetapi `QrCodePath` tidak sesuai nomor itu. Sistem tidak menebak perbaikannya. Tidak dihitung dalam `limit` | Tidak | **Ya** — `AC-15`, `PAT-OQ-004` |
| `CONFLICT` | Langkah 4: artefak QR tujuan sudah ada | Tidak | **Ya** — `PAT-OQ-005` |
| Akan diubah (simulasi) | `dryRun=true`, pasien lolos seluruh pemeriksaan | Tidak | Nama `DEV_DISCRETION` |
| Berhasil diubah | `dryRun=false`, langkah 6 berhasil | Ya — empat field | Nama `DEV_DISCRETION` |
| Konflik MRN tujuan | Langkah 3 | Tidak | Nama `DEV_DISCRETION`; perilakunya terkunci. Boleh memakai `CONFLICT` yang sama asalkan keterangan baris menyebut jenis konfliknya |
| Gagal | Langkah 5 atau 7 | Tidak — atau dipulihkan oleh *rollback* transaksi | Nama `DEV_DISCRETION`; perilakunya terkunci |

`DEV_DISCRETION` berarti nama persisnya dipilih developer saat implementasi dan wajib dicatat
pada laporan task. Perilakunya tidak boleh menyimpang dari tabel ini.

### 4.4 Jalur tidak normal

| Kejadian | Akibat | Data pasien | QR lama | QR baru |
| --- | --- | --- | --- | --- |
| Environment tidak diizinkan | Seluruh permintaan ditolak | Tetap | Tetap | Tidak dibuat |
| Pembuatan QR baru gagal | Berhenti pada pasien itu | Tetap | Tetap | Tidak ada |
| `SaveChanges`/*commit* gagal | Berhenti pada pasien itu | Tetap, karena transaksi dibatalkan | Tetap | Dihapus *best-effort* |
| Pasien ke-10 gagal pada panggilan `limit` 25 | Pasien 1–9 **tetap** berhasil; pasien 10–25 belum diproses | 1–9 berubah, 10–25 tetap | Tetap | Ada untuk 1–9 |

Baris terakhir adalah inti `AC-25`: satu pasien satu transaksi, sehingga kegagalan tidak
membatalkan pasien yang sudah selesai.

---

## 5. Kontrak QR

Bukti dari `PatientController.cs` pada snapshot:

| Hal | Perilaku saat ini | Baris |
| --- | --- | --- |
| Isi QR | `BuildPatientQrPayload(MRN)` mengembalikan MRN berformat, misalnya `00-98-76-54` | 1386–1401 |
| Format MRN | Digit diambil lalu diberi tanda hubung: 8 digit → `xx-xx-xx-xx`, 6 digit → `xx-xx-xx` | 1606–1636 |
| Folder | `patient-qrcodes/<MRN yang dibersihkan SanitizePathSegment>/` | 59, 1076–1077 |
| Nama file | `qrcode.png` | 1103 |
| Path publik | `<PublicRequestPath>/patient-qrcodes/<MRN>/qrcode.png`; bawaannya `/uploads` | 60, 1153–1157 |
| Penulisan file | `System.IO.File.WriteAllBytes` — **menimpa diam-diam** bila file sudah ada | 1144 |

Konsekuensi untuk `BE-PAT-MIG-001`:

- QR baru wajib memakai isi, folder, dan path yang sama polanya dengan perilaku di atas, tetapi
  dari **MRN kanonik** (`AC-09`).
- Method `SavePatientQrCodeFile` tidak boleh dipakai apa adanya, karena baris 1144 akan menimpa
  artefak yang sudah ada. Keberadaan artefak tujuan wajib diperiksa **sebelum** helper itu
  dipanggil (`PAT-OQ-005`). Bila artefaknya sudah ada, helper tidak dipanggil sama sekali dan
  pasien dilaporkan `CONFLICT` (`AC-13`).
- Antara pemeriksaan dan penulisan masih ada jeda singkat. Bila ada proses lain membuat file yang
  sama di jeda itu, penulisan tetap dapat menimpa. Cara menutup jeda ini — misalnya penulisan
  yang gagal bila file sudah ada — `DEV_DISCRETION`, dan pilihannya dicatat pada laporan task.
- Folder QR lama tidak pernah dihapus, dipindah, atau diganti nama (`AC-10`).
- File QR adalah artefak runtime di folder penyimpanan, bukan source. File QR **tidak boleh**
  masuk Git.

**Contoh.** Pasien dengan MRN lama `01-23-45-67` dan MRN kanonik `00-98-76-54`:

| | Sebelum | Sesudah |
| --- | --- | --- |
| Folder QR lama | `patient-qrcodes/01-23-45-67/qrcode.png` ada | **Tetap ada** |
| Folder QR baru | tidak ada | `patient-qrcodes/00-98-76-54/qrcode.png` dibuat |
| Isi QR baru | — | `00-98-76-54` |
| `QrCodePath` | `/uploads/patient-qrcodes/01-23-45-67/qrcode.png` | `/uploads/patient-qrcodes/00-98-76-54/qrcode.png` |

---

## 6. Gerbang environment

`AC-05`: `dryRun=false` hanya boleh di environment yang **terbukti** bukan produksi. Produksi
selalu ditolak, dan environment yang tidak dikenal juga ditolak (*fail closed* — bila ragu,
tolak).

Pola yang sudah ada di source **tidak memenuhi** syarat ini dan **tidak boleh ditiru**:

| Berkas | Baris | Pola | Masalahnya |
| --- | ---: | --- | --- |
| `Services/Security/AccessPermissionService.cs` | 38 | `!environment.IsProduction()` | Nama environment yang salah ketik atau kosong dianggap "bukan produksi", lalu diizinkan |
| `Areas/HealthServices/OperatingRoomManagement/Options/OperatingRoomRuleRelaxation.cs` | 41 | `!environment.IsProduction()` | Sama seperti di atas |

Yang diwajibkan adalah **daftar izin positif** (*positive allowlist*) yang gagal tertutup
(`PAT-OQ-006`). Isinya hanya `Staging` (`PAT-OQ-002`). `!environment.IsProduction()` tidak boleh
menjadi satu-satunya pemeriksaan.

| Ketentuan | Isi |
| --- | --- |
| Berlaku untuk | `dryRun=true` **dan** `dryRun=false` |
| Cara mencocokkan | Persis sama huruf demi huruf, termasuk huruf besar-kecil. `IHostEnvironment.IsStaging()` membandingkan tanpa membedakan huruf besar-kecil, sehingga ia saja tidak cukup |
| Login dan hak akses | Tetap wajib (bagian 7). Gerbang environment adalah lapisan tambahan |
| Test | Test harness boleh mensimulasikan `Staging`. Aturan runtime pada kode produksi tidak diubah untuk keperluan test |

| Nama environment | Hasil |
| --- | --- |
| `Staging` | Diizinkan |
| `staging` | Ditolak — huruf besar-kecil berbeda |
| `Stagging` | Ditolak — salah ketik |
| `Production` | Ditolak |
| `Development` | Ditolak |
| `Staging-Pilot` | Ditolak — nama khusus yang tidak dikenal |
| kosong atau tidak diset | Ditolak |

Ejaan `Staging` sama dengan yang dipakai `.github/workflows/generate-migration-artifact.yml`
baris 105. Nama environment pada server staging yang sedang berjalan tidak diperiksa. Bila
ternyata berbeda ejaannya, endpoint menolak — gagal tertutup, dan terlihat pada simulasi pertama.

---

## 7. Hak akses

Ketentuan `AC-06`: wajib login, ditambah izin `Patient : Update` yang sudah ada, atau izin lain
yang lebih ketat.

| Bukti | Baris |
| --- | --- |
| `[Authorize]` pada tingkat controller | 41 |
| `[AccessController(ControllerName = "Patient")]` | 43–51 |
| Konvensi izin ubah: `[AccessAction("Update", …, AccessType = AccessTypes.Update)]` beserta `[AccessPermission("Patient", "Update")]` | 771–778 dan 912–919 |

Dua pilihan yang sama-sama memenuhi `AC-06`:

| Pilihan | Bentuk | Akibatnya |
| --- | --- | --- |
| A | Memakai ulang `Update` | Setiap peran yang sudah boleh mengubah data pasien otomatis boleh menjalankan rekonsiliasi di staging |
| B | Aksi khusus yang lebih ketat, dengan `AccessType = AccessTypes.Update` | Admin harus mencentang izinnya secara khusus pada layar Akses Role |

Pilihannya `DEV_DISCRETION` di dalam batas `AC-06`, dan wajib dicatat pada laporan task.
Rekomendasinya pilihan **B**, supaya kemampuan mengubah MRN massal tidak ikut terbuka bagi setiap
petugas pendaftaran. Apa pun pilihannya:

- argumen pertama `[AccessPermission]` wajib persis `Patient`;
- argumen kedua `[AccessPermission]` wajib persis sama dengan argumen pertama `[AccessAction]` pada
  method yang sama;
- dilarang memakai `IsInRole`, daftar nama peran, atau `UserType` sebagai penentu izin.

---

## 8. Data yang dibaca, diubah, dan dilarang diubah

| Tabel | Dibaca | Diubah | Dasar |
| --- | --- | --- | --- |
| `migration.rsmmc_patient_mrn_plan` | Ya | **Tidak pernah** | `AC-21` |
| `migration.rsmmc_patient_crosswalk` | Ya | **Tidak pernah** | `AC-20` |
| `migration.rsmmc_patient_pilot_reconcile_backup` | Tidak diwajibkan | **Tidak pernah** — tidak dibuat ulang, di-*update*, dikosongkan, atau dihapus | `AC-22`, `PAT-DEC-009` |
| `public."MstPatient"` | Ya | Hanya empat field pada bagian 9 | `AC-08` |
| Tabel lain | — | Tidak | `AC-07` |

Ketiga tabel `migration.rsmmc_*` **belum dipetakan** di source pada snapshot ini: pencarian
`rsmmc_` pada seluruh berkas `*.cs` tidak menemukan hasil. Implementasi wajib membaca tabel-tabel
itu **tanpa** menimbulkan kebutuhan EF migration baru (`PAT-DEC-012`, `PAT-OQ-007`):

- tidak ada pembuatan atau penerapan EF migration;
- tidak ada perubahan skema;
- tidak ada perubahan data tabel migrasi secara langsung selama implementasi;
- mekanisme baca dipilih saat build, mengikuti kontrak engineering canonical dan infrastruktur
  yang ada. Contoh mekanisme berparameter yang sudah dipakai source tercatat pada
  `00-interview-decisions.md` bagian 4.4;
- nilai masukan selalu dikirim sebagai parameter, tidak disambung ke teks SQL;
- bukti bahwa model EF tidak berubah dicatat pada laporan task.

---

## 9. Field yang tetap dan yang berubah pada `MstPatient`

`MstPatient` mewarisi `IdentityModel` (`Models/IdentityModel.cs` baris 17–37).

| Field | Boleh berubah? | Dasar |
| --- | --- | --- |
| `MedicalRecordNumber` | **Ya** — menjadi MRN kanonik | `AC-08` |
| `QrCodePath` | **Ya** — menjadi path QR baru | `AC-08` |
| `UpdateDateTime` | **Ya** | `AC-08` |
| `UpdateBy` | **Ya** | `AC-08` |
| `Id` | Tidak | `AC-07` |
| `PatientCode` | Tidak — dan tidak boleh dibangkitkan ulang | `AC-07`, `AC-17` |
| `CreateDateTime`, `CreateBy` | Tidak | `AC-07` |
| Seluruh data demografi dan relasi | Tidak | `AC-07` |

Pembangkit nomor yang **dilarang** dipanggil untuk pasien Pilot:

| Method | Baris | Larangan |
| --- | ---: | --- |
| `GenerateMedicalRecordNumberAsync` | 1403 | `AC-16` |
| `GeneratePatientCodeAsync` | 2468 | `AC-17` |

Endpoint ini juga tidak boleh membuat `MstPatient` baru (`AC-18`), dan tidak boleh menghapus lalu
membuat ulang `MstPatient` (`AC-19`).
