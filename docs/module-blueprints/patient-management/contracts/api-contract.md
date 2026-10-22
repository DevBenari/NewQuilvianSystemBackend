# Kontrak API — Migrasi Data Pasien RSMMC

Dokumen ini memuat dua bagian. **Bagian A** (bagian 1–9) adalah kontrak `1.1.0` untuk
`BE-PAT-MIG-001`; isinya dibiarkan apa adanya sebagai bukti sejarah. **Bagian B** (bagian B1–B15 di
akhir dokumen) adalah kontrak `BE-PAT-MIG-002` beserta perubahan pendaftaran pasien normal milik
task prasyarat `BE-PAT-MIG-003` (B14) dan pendaftaran bayi milik `BE-PAT-MIG-003A` (B14.4). Bagian B
pertama kali `approved` sebagai `1.2.0` pada 15 Oktober 2026 (`PAT-DEC-033`); versi yang **berlaku**
kini `1.3.0` (19 Oktober 2026). Endpoint worker Bagian B tetap **belum ada** di source.

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `last_changed_in` | `1.3.0` — sinkronisasi bukti runtime 22 Oktober 2026 tanpa perubahan versi: nilai `PATIENT_BASELINE = 741` dan rumus lengkapnya (B4.2), baris B9 `public."MstPatient"` diselaraskan dengan `PAT-DEC-075`, label versi B2, contoh B4.2, traceability B13; bentuk API tidak berubah. Isi `1.3.0` 19 Oktober 2026: guard reservasi pada jalur pendaftaran bayi (B14.4 baru), pemeriksa B14.2 melaporkan kedua jalur, dan amandemen desain worker — `PERSIST_IS_MEMBER` dihapus, lima kolom baru `NULL`, `PATIENT_BASELINE` dengan baseline tetap (`PAT-DEC-069`–`077`; klarifikasi non-API B15 19 Oktober 2026 — prasyarat konfigurasi server bukti sekali pakai, temuan `F-14` — dan 20 Oktober 2026 — eksekusi serial suite provider-real, tanpa perubahan versi). `1.2.2` (`PAT-DEC-068`), `1.2.1` (`PAT-DEC-067`), dan `1.2.0` sebelumnya. Bagian A terakhir berubah di `1.1.0` |
| Versi kontrak berlaku | **`1.3.0`** — `approved` 19 Oktober 2026 (`PAT-DEC-069`–`077`, instruksi pemilik `TASK: BE-PAT-MIG-003A`). Bagian A tetap berisi `1.1.0` tanpa perubahan. *Riwayat: `1.2.2` `approved` 16 Oktober 2026* |
| Versi kontrak diusulkan | — (tidak ada). Riwayat: `1.2.0` diusulkan 14 Oktober 2026 sebagai `draft`, disetujui 15 Oktober 2026; `1.2.1` dan `1.2.2` disetujui 16 Oktober 2026 |
| Status kontrak | Bagian A: **`approved`** — final 8 Oktober 2026. Bagian B: **`approved`** — `1.2.0` 15 Oktober 2026 (`PAT-DEC-033`) sesudah `requirement-completeness-gate` `PASS`; `1.2.1` 16 Oktober 2026 (`PAT-DEC-067`); `1.2.2` 16 Oktober 2026 (`PAT-DEC-068`); **`1.3.0` 19 Oktober 2026 (`PAT-DEC-069`–`077`) — berlaku**. Sinkronisasi bukti runtime 22 Oktober 2026 tanpa perubahan versi (catatan di bawah amandemen `1.3.0`). Pertanyaan `BLOCKING`: 0 |
| Riwayat versi | `1.0.0` (8 Oktober 2026): bentuk awal; `PAT-OQ-002`–`004` masih `PROPOSED`. `1.1.0` (8 Oktober 2026): `PAT-OQ-002`–`004` final, ditambah `PAT-OQ-005`–`007`; nama status konflik QR dikunci `CONFLICT`. **Bentuk request dan route tidak berubah**. `1.2.0` (14 Oktober 2026, draft; diamandemen dua kali 15 Oktober 2026; **`approved` 15 Oktober 2026**): menambah Bagian B untuk `BE-PAT-MIG-002` dan B14 untuk `BE-PAT-MIG-003`. **Aditif** untuk API: endpoint, request, response, dan status Bagian A tidak berubah; bentuk request/response pendaftaran normal tidak berubah, ditambah satu kode penolakan `503` khusus Staging (B14.3). `1.2.1` (16 Oktober 2026, **`approved`**, `PAT-DEC-067`): `Enabled = false` di Staging persis kini gagal tertutup, bukan mematikan guard; menambah endpoint baca-saja `GET /admin/migration/rsmmc-reservation-guard` (B14.2). Aditif untuk API; satu perubahan perilaku hanya di Staging persis. `1.2.2` (16 Oktober 2026, **`approved`**, `PAT-DEC-068`): kunci serialisasi allocator dua bagian tetap, tanpa `hashtext()`; syarat endpoint pemeriksa dikonfirmasi. Tanpa perubahan bentuk API. `1.3.0` (19 Oktober 2026, **`approved`**, `PAT-DEC-069`–`077`): guard reservasi pada `POST /newborns` (B14.4); empat field aditif pemeriksa B14.2 dan `overallGuardReady` mensyaratkan kedua jalur; `503` khusus Staging persis pada jalur bayi; amandemen desain worker B4.2 `PATIENT_BASELINE`, B4.3 langkah 8 dihapus, B10 lima kolom `NULL`, B15. Aditif untuk API |
| Task | Bagian A: `BE-PAT-MIG-001`. Bagian B: `BE-PAT-MIG-002`; B14: `BE-PAT-MIG-003`; B14.4 dan perluasan B14.2: `BE-PAT-MIG-003A` |
| Snapshot source | Bagian A: branch `QuilvianStaDeploy`, commit `103b45ccd5f0d9e2cbacff588540d8fe3d52e706` (selisih baris ke Integration dicatat pada laporan `BE-PAT-MIG-001` bagian 9.5). Bagian B: `QuilvianIntegrationBackend` @ `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923` — kutipan baris B2–B11 berasal dari snapshot ini. Impact scan final read-only 22 Oktober 2026: Integration `201d367ed7722320069c69e266133fbabdaf0fc3` (`PAT-FACT-096`); sinkronisasi dokumen di atas Integration `8bccaa034db955c9acb719fc1cb476ed9c71f1c3`, yang menambah perbaikan F-01 hanya di `Tests/` (`PAT-FACT-097`). Agent build tetap memeriksa ulang kutipan baris terhadap HEAD saat build |
| Status endpoint | Pemeriksa B14.2: **tersedia** sejak `BE-PAT-MIG-003` (`ab45dd3c`), keluaran `1.3.0` dibangun `BE-PAT-MIG-003A`; `POST /newborns` sudah ada (upstream `3a0189c2`), guard B14.4 dibangun `BE-PAT-MIG-003A`; endpoint worker Bagian B: **Rencana (belum tersedia)**. Bagian A: **tersedia** di source sejak commit `ef3adbe4` (catatan di bawah tidak diubah karena bukti sejarah). Bagian B: **Rencana (belum tersedia)** |

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

---
---

# Bagian B — Migrasi Massal Pasien RSMMC (`BE-PAT-MIG-002`) — `1.2.0`

> **Status: `approved`** — 15 Oktober 2026, sesi ketiga (`PAT-DEC-033`). Persetujuan ini mengunci
> **desain**. Ia **tidak** memberi wewenang menulis source, *commit*, deployment, eksekusi database,
> pemanggilan endpoint, pembuatan QR, atau menjalankan Canary-A. Endpoint pada bagian ini **belum
> ada** di source.
>
> *Teks status lama — jejak:* "Seluruh bagian B adalah **usulan** untuk disetujui pemilik
> (`PAT-DEC-033`). Butir bertanda `PROPOSED` atau `BLOCKING` merujuk pertanyaan pada
> `00-interview-decisions.md` bagian 10."
>
> **Amandemen 15 Oktober 2026.** Bukti database Step 16 dan keputusan pemilik `PAT-DEC-034`–`043`
> dimasukkan: strategi path QR (`PAT-OQ-028`), persistensi `IsMember = false` (`PAT-OQ-029`),
> gerbang `NORMAL_ALLOCATION_GUARD`, manifest eksekusi Canary-A yang dibekukan ulang
> (`PAT-OQ-033`), dan bagian B14 tentang perlindungan reservasi.
>
> **Amandemen kedua 15 Oktober 2026.** Keputusan pemilik `PAT-DEC-044`–`062` dimasukkan: route
> dan bentuk request disetujui; urutan `CANARY_A` mengikuti manifest; identifier/versi/sidik jari
> manifest pada respons; simulasi membaca filesystem; kolom crosswalk dari preview; kunci dua
> bagian; path QR `rsmmc-bulk`; persistensi `IsMember`; dan B14 tentang task prasyarat
> `BE-PAT-MIG-003`. Yang masih `BLOCKING`: `PAT-OQ-019`, `031`, `036`, `037`.
>
> **Finalisasi sesi ketiga 15 Oktober 2026.** Keempat butir `BLOCKING` ditutup pemilik: skema fisik
> dan SQL bertipe (`PAT-DEC-063`), lantai MRN 890.188 (`PAT-DEC-064`), bukti provider-real dengan
> PostgreSQL 15 sekali pakai (`PAT-DEC-065`), serta aturan environment dan konfigurasi guard
> (`PAT-DEC-066`). Usulan standar agent `PAT-GAP-002`–`016` dari
> `requirement-completeness-assessment.md` ikut dikunci: gerbang `PREVIEW_SCHEMA_SIGNATURE`
> (B4.2), perlakuan NULL dan nilai enum tidak dikenal (B10), aturan environment dan konfigurasi
> (B14.3), keluaran pemeriksa (B14.2), dan bukti provider-real (B15).
>
> **Amandemen `1.2.1` — 16 Oktober 2026 (`PAT-DEC-067`), instruksi pemilik sebelum build
> `BE-PAT-MIG-003`.** Di `Staging` persis, `RsmmcMigrationReservationGuard.Enabled = false` kini
> **gagal tertutup** seperti konfigurasi yang hilang atau salah bentuk; tidak ada lagi cara mematikan
> perlindungan reservasi lewat konfigurasi di Staging. Pemeriksa B14.2 kini juga tersedia sebagai
> endpoint admin baca-saja khusus Staging dengan izin `Patient : Update`, dengan nama field sesuai
> instruksi pemilik.
>
> **Amandemen `1.2.2` — 16 Oktober 2026 (`PAT-DEC-068`).** Kunci serialisasi allocator nomor pasien
> normal kini `pg_advisory_xact_lock(1346458697, 1313821261)` — dua kunci Int32 tetap: namespace `0x50415449` (`"PATI"`, PATIENT_IDENTIFIER_ALLOCATION) dan kunci `0x4E4F524D` (`"NORM"`, NORMAL_PATIENT_CREATE). Endpoint pemeriksa B14.2
> dikonfirmasi pemilik dengan syarat baca-saja, nol mutasi, tanpa data pribadi.
>
> **Amandemen `1.3.0` — 19 Oktober 2026 (`PAT-DEC-069`–`077`), instruksi pemilik
> `TASK: BE-PAT-MIG-003A`.** Impact scan sebelum build `BE-PAT-MIG-002` menemukan jalur pendaftaran
> bayi `POST /newborns` yang mengalokasikan nomor di luar guard (`PAT-SRC-031`). Task prasyarat
> `BE-PAT-MIG-003A` memasang lantai, konfigurasi, dan kunci yang sama pada jalur itu di `Staging` persis
> (B14.4); pemeriksa B14.2 kini melaporkan kedua jalur dan `overallGuardReady` mensyaratkan keduanya.
> Desain worker diamandemen: langkah `PERSIST_IS_MEMBER` dihapus (B4.3, `PAT-DEC-075`), lima kolom
> `MstPatient` baru dibiarkan `NULL` (B10, `PAT-DEC-076`), dan `PATIENT_BASELINE` memakai baseline tetap
> yang diukur ulang sebelum Canary-A (B4.2, `PAT-DEC-077`). `BE-PAT-MIG-002` tetap belum boleh dibangun
> (`PAT-DEC-069`).
>
> **Sinkronisasi bukti runtime — 22 Oktober 2026, tanpa perubahan versi.** Prasyarat yang menahan
> `BE-PAT-MIG-002` kini terbukti (`00-interview-decisions.md` bagian 16): `BE-PAT-MIG-003A` di-*merge*
> dan dideploy ke Staging (`PAT-FACT-092`), `PAT-GATE-005` generasi 2 `PASS` (`PAT-FACT-093`),
> `PAT-RSK-042` 0 baris (`PAT-FACT-094`), dan baseline tetap `PATIENT_BASELINE = 741` (`PAT-FACT-095`).
> Implementasi source `BE-PAT-MIG-002` **boleh dimulai**; runtime Canary-A tetap milik `PAT-GATE-002`
> dan belum berjalan. Teks B4.2 `PATIENT_BASELINE`, contoh B4.2, label versi B2, baris B9
> `public."MstPatient"`, dan traceability B13 diselaraskan dengan `PAT-DEC-075`–`077` yang sudah
> `approved`. Bentuk endpoint, request, response, dan status tidak berubah.

| Field | Nilai |
| --- | --- |
| Task | `BE-PAT-MIG-002` — RSMMC Resumable Bulk Patient Migration Worker |
| Status | **`approved`** — owner: pemilik/leader PatientManagement; `approved_by`: pemilik (akun Git `devbenari`), lewat instruksi tertulis sesi ketiga yang menyetujui finalisasi bila `requirement-completeness-gate` lulus; `approved_at`: 15 Oktober 2026. *Riwayat: `draft` 14 Oktober 2026, diamandemen dua kali 15 Oktober 2026* |
| `input_revision` | `00-interview-decisions.md` revision `3` (`approved` untuk bagian 6–14, 15 Oktober 2026) dan `requirement-completeness-assessment.md` revision `1`; diamandemen revision `6` (`PAT-DEC-069`–`077`, 19 Oktober 2026) dan bukti runtime bagian 16 (22 Oktober 2026); hash pada manifest |
| Snapshot source | `QuilvianIntegrationBackend` @ `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923` (kutipan baris). Impact scan final 22 Oktober 2026: `201d367e` (`PAT-FACT-096`) |
| Dampak kompatibilitas | Aditif untuk API: endpoint Bagian A dan kontrak response bersama tidak berubah. Alur pendaftaran pasien normal tidak diubah **oleh endpoint ini**; perlindungan reservasi yang menyentuh alur itu milik task prasyarat `BE-PAT-MIG-003` (B14, `PAT-DEC-057`); bentuk request/response endpoint pendaftaran normal tidak berubah |
| Traceability | `PAT-DEC-017`–`PAT-DEC-077`; `PAT-MIG3A-AC-01`–`PAT-MIG3A-AC-12` untuk B14.2 (`1.3.0`) dan B14.4; `PAT-OQ-008`–`PAT-OQ-037` (seluruhnya diputuskan atau `SUPERSEDED`); `PAT-GAP-002`–`016`; `PAT-MIG2-AC-01`–`PAT-MIG2-AC-51` (`AC-44`/`45` dipindah ke `BE-PAT-MIG-003`); `PAT-MIG3-AC-01`–`PAT-MIG3-AC-14` untuk B14 dan B15 |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — alasan pada `00-interview-decisions.md` bagian 6 |

---

## B1. Jenis endpoint

Endpoint ini adalah **aksi admin khusus Staging** pada resource Patient yang sudah ada. Ia bukan
master data baru dan bukan transaksi yang punya siklus status sendiri. Karena itu sembilan
endpoint baseline master data tidak berlaku, dan tidak ada `GET /options`, `PATCH /{id}/status`,
atau `DELETE /{id}` yang menyertainya.

Build pertama hanya mengenal **satu mode pemilihan**: `CANARY_A` (`PAT-DEC-045`). Manifest
eksekusinya adalah `RSMMC-CANARY-A` versi `1`: 25 `legacy_pid` kandidat pada
`00-interview-decisions.md` bagian 11, **dalam urutan yang sama**, yang menjadi manifest eksekusi
begitu path QR `rsmmc-bulk` dan reservation guard `BE-PAT-MIG-003` selesai (`PAT-DEC-056`).
Sampai saat itu daftar tersebut kandidat (`PAT-DEC-040`). Identifier dan versi manifest dikunci
lewat `PAT-DEC-033`. Mode untuk Canary-B dan gelombang besar ditambahkan lewat revisi kontrak
terpisah sesudah `PAT-GATE-003`/`PAT-GATE-004`.

---

## B2. Endpoint

### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`
Contract version: `1.3.0` — status `approved` *(riwayat: `1.2.0` `approved` 15 Oktober 2026; route dan bentuk request tidak berubah sejak itu)*

Bukti grup dan base URL: `PatientController.cs` baris 43 (`[Route]`) dan 53 (`[Tags]`) pada
`a52b3c98`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/admin/migration/rsmmc-pilot-mrn/reconcile` | Rekonsiliasi MRN Pilot (Bagian A) | `Patient : Update` | `RsmmcPilotMrnReconcileRequest` | `ApiResponse<RsmmcPilotMrnReconcileResponse>` | Sudah ada — **tidak berubah** |
| `POST` | `/admin/migration/rsmmc-bulk-patients/migrate` | Mensimulasikan atau menjalankan pembuatan pasien RSMMC dari target preview untuk satu mode pemilihan yang disetujui, paling banyak `limit` pasien per panggilan | `Patient : Update` | Body `RsmmcBulkPatientMigrationRequest` | `ApiResponse<RsmmcBulkPatientMigrationResponse>` | **Rencana (belum tersedia)** — route dan bentuk request disetujui (`PAT-DEC-044`) |

Nama class DTO di atas adalah usulan. Isi field dan perilakunya dikunci kontrak ini sesudah
disetujui; ejaan nama class boleh disesuaikan saat build dan dicatat pada laporan task.

### B2.1 Masukan

| Field | Tipe | Wajib | Aturan | Contoh |
| --- | --- | --- | --- | --- |
| `batchId` | `Guid` | Ya | Harus batch kanonik `45eeefba-dd39-6831-1bd2-d986851f7c1f` untuk eksekusi | `45eeefba-dd39-6831-1bd2-d986851f7c1f` |
| `selectionMode` | `string` | Ya | Hanya `CANARY_A` pada build pertama. Nilai lain ditolak `400` | `CANARY_A` |
| `dryRun` | `bool?` | Tidak | Bila tidak dikirim → `true`. **Tidak boleh ada default implisit `false`** — sama dengan `PAT-OQ-003` | `true` |
| `limit` | `int?` | Tidak | Bawaan **25**, rentang **1–25** (`PAT-DEC-045`). Hanya pasien yang akan dibuat yang menghabiskan `limit` | `5` |

Daftar `legacy_pid` **tidak** dikirim klien. Server memakai manifest eksekusi Canary-A yang disimpan
di server (`PAT-DEC-044`, `PAT-DEC-056`), dan memprosesnya dalam urutan manifest (`PAT-DEC-045`).

| Body yang dikirim | Hasil |
| --- | --- |
| `{ "batchId": "45eeefba-…", "selectionMode": "CANARY_A" }` | Simulasi, `limit` 25 |
| `{ "batchId": "45eeefba-…", "selectionMode": "CANARY_A", "dryRun": false, "limit": 5 }` | Eksekusi paling banyak 5 pasien, hanya di `Staging` dan hanya sesudah `PAT-GATE-002` (b) |
| `{ "batchId": "45eeefba-…", "selectionMode": "WAVE" }` | Ditolak `400` — mode belum didukung |
| `{ "batchId": "45eeefba-…", "selectionMode": "CANARY_A", "limit": 26 }` | Ditolak `400` — di atas batas 25 |
| `{ "selectionMode": "CANARY_A" }` | Ditolak `400` — `batchId` wajib |

### B2.2 Keluaran

**Ringkasan panggilan.**

| Field | Isi | Contoh |
| --- | --- | --- |
| `batchId`, `selectionMode`, `dryRun`, `limit`, `environmentName` | Gema masukan dan nama environment | `CANARY_A`, `true`, `25`, `Staging` |
| `manifestId`, `manifestVersion` | Identifier dan versi manifest eksekusi (`PAT-DEC-044`) | `RSMMC-CANARY-A`, `1` |
| `manifestSha256` | Sidik jari manifest eksekusi **berurutan** — `legacy_pid` dalam urutan manifest, dipisah koma, tanpa spasi, UTF-8 | `c43207eb…79ad` untuk `RSMMC-CANARY-A` v1 |
| `manifestSize` | Jumlah `legacy_pid` dalam manifest | `25` |
| `totalInManifest`, `alreadyMigrated`, `eligibleBeforeRun`, `selected` | Keadaan awal dan jumlah terpilih | `25`, `0`, `25`, `25` |
| `ready`, `migrated`, `conflict`, `failed`, `notProcessed`, `remaining` | Hasil panggilan | `25`, `0`, `0`, `0`, `0`, `25` |
| `stopped`, `stopReason` | Apakah proses berhenti, dan pada `legacy_pid` mana | `false`, `null` |
| `gates[]` | Hasil gerbang tingkat panggilan (B4.2): `code`, `description`, `expected`, `actual`, `passed` | — |
| `items[]` | Satu baris per pasien manifest, **dalam urutan manifest** (`PAT-DEC-045`). *Teks lama: "urut `legacy_pid` naik secara angka" — dikoreksi 15 Oktober 2026, sesi ketiga* | — |

**Isi setiap baris `items[]`.**

| Field | Isi | Contoh samaran |
| --- | --- | --- |
| `legacyPid` | Teks angka, supaya nilai `bigint` tidak terpotong di klien JavaScript | `"-667995"` |
| `sourceGroup` | Kelompok sumber dari kolom preview `source_group` (`PAT-FACT-083`) | `NOREG_WITHOUT_RM` |
| `crosswalkStateBefore` | `MISSING`, `EXISTS_WITHOUT_TARGET`, atau `EXISTS_WITH_TARGET` | `MISSING` |
| `patientId` | `Id` pasien; kosong bila belum dibuat | `null` |
| `patientCode` | `PatientCode` dari preview | `PAT-RSMMC-12345` |
| `medicalRecordNumber` | MRN akhir perencana | `00-12-34-56` |
| `plannedQrCodePath` | Path publik QR rencana di namespace `rsmmc-bulk` (`PAT-DEC-052`) | `/uploads/patient-qrcodes/rsmmc-bulk/00-12-34-56/qrcode.png`. *Contoh 14 Oktober 2026 memakai pola lama `/uploads/patient-qrcodes/00-12-34-56/qrcode.png`* |
| `fullName` | Nama dari preview (`PAT-DEC-049`). Tidak pernah masuk log aplikasi | `Siti Contoh` |
| `hasIdentityNumber`, `hasPhoneNumber` | Hanya tanda ada atau tidak; nilainya **tidak** ditampilkan | `true`, `false` |
| `status` | Lihat B3.2 | `READY` |
| `historicalQrAtDefaultPath` | Simulasi (`PAT-DEC-046`): apakah berkas QR historis ada di path bawaan `patient-qrcodes/<MRN>/qrcode.png`. Informasi saja, bukan konflik (`PAT-DEC-052`) | `true` |
| `historicalQrLength`, `historicalQrSha256` | Simulasi: panjang dan SHA-256 berkas historis itu, dibaca tanpa menulis — bukti awal bahwa berkas itu tidak berubah sesudah eksekusi (`PAT-MIG2-AC-43`) | `4812`, `9f2c…` |
| `plannedQrExists` | Simulasi: apakah path rencana `rsmmc-bulk` sudah terisi. `true` berarti eksekusi akan berakhir `CONFLICT` | `false` |
| `conflictType`, `failureStage`, `message` | Rincian bila bukan jalur normal | `null` |

### B2.3 Kode status

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Simulasi atau eksekusi selesai, termasuk bila berhenti di tengah karena konflik atau kegagalan. Ringkasan, gerbang, dan seluruh baris dikembalikan |
| `400` | Isian tidak sah: `batchId` kosong, `selectionMode` tidak dikenal, `limit` di luar 1–25, atau pada eksekusi aktor tidak dikenali. Kode: `RSMMC_BULK_BATCH_ID_REQUIRED`, `RSMMC_BULK_SELECTION_MODE_NOT_SUPPORTED`, `RSMMC_BULK_LIMIT_OUT_OF_RANGE`, `RSMMC_BULK_ACTOR_UNKNOWN` |
| `401` | Pengguna belum login |
| `403` | Pengguna tidak punya izin `Patient : Update`, **atau** environment bukan `Staging` persis (kode `RSMMC_BULK_ENVIRONMENT_NOT_ALLOWED`) — ditolak sebelum satu tabel pun dibaca |
| `409` | Eksekusi ditolak karena satu atau lebih gerbang tingkat panggilan gagal (kode `RSMMC_BULK_GATE_FAILED`). Tidak ada data yang diubah; daftar gerbang ada di `errors.gates` |

Pola kode `403` untuk environment dan `409` untuk gerbang sama dengan endpoint Bagian A, sehingga
operator tidak perlu mempelajari dua perilaku.

---

## B3. Aturan memilih dan mengklasifikasikan pasien

### B3.1 Siapa yang diproses

Hanya `legacy_pid` pada manifest eksekusi `RSMMC-CANARY-A` v1 (`PAT-DEC-056`). Urutan eksekusi
**persis urutan manifest** yang disetujui — -667992, -667991, -667986, 797423, -667995, … , 620 —
dan manifest **tidak** disort ulang berdasarkan `legacy_pid` (`PAT-DEC-045`). `legacy_pid` tetap
diperlakukan sebagai bilangan bulat bertanda Int64 dan tidak pernah dibandingkan sebagai teks
(`PAT-DEC-038`); urutan naik Int64 hanya akan berlaku bagi mode pemilihan umum di task kemudian.
Pembanding urutan versi Pilot tidak dipakai (`PAT-SRC-011`).

**Contoh dengan `limit` 5.** Panggilan pertama memproses -667992, -667991, -667986, 797423, dan
-667995. Panggilan kedua melewati kelimanya sebagai `ALREADY_MIGRATED`, lalu memproses -667984,
797554, 1, 888, dan 2907.

*Teks lama, dicabut oleh `PAT-DEC-045`:* "Urutannya `legacy_pid` naik secara angka Int64 … -667995,
-667992, …, 797554 pada kandidat."

Pasien manifest yang keadaannya berubah **tidak pernah** diganti pasien lain. Ia diklasifikasikan
pada tabel B3.2.

### B3.2 Klasifikasi per pasien

| Keadaan saat dibaca | Status | Menghabiskan `limit`? | Data berubah? | Eksekusi berhenti? |
| --- | --- | --- | --- | --- |
| Preview ada pada batch kanonik dan `PENDING`; perencana `FINALIZED` dengan MRN sama; crosswalk **tidak ada**; kode, MRN, identitas, dan telepon bebas; QR tujuan bebas | `READY` (simulasi) / `MIGRATED` (eksekusi) | **Ya** | Hanya pada eksekusi | Tidak |
| Crosswalk punya target; `MstPatient` dengan `Id` itu ada; `PatientCode` = preview; MRN = MRN akhir; `QrCodePath` = path rencana | `ALREADY_MIGRATED` | Tidak | Tidak | Tidak |
| Seperti baris di atas, tetapi `QrCodePath` berbeda, atau berkas QR tidak ada (dibaca, `PAT-DEC-046`) | `MIGRATED_QR_INCONSISTENT` | Tidak | Tidak — dilaporkan, tidak diperbaiki | Tidak |
| Crosswalk ada, target NULL | `CONFLICT` / `CROSSWALK_EXISTS_WITHOUT_TARGET` | Tidak | Tidak — baris historis tidak disentuh | **Ya** |
| Crosswalk punya target, `MstPatient` tidak ada | `CONFLICT` / `ORPHANED_CROSSWALK` | Tidak | Tidak — pasien tidak dibuat ulang | **Ya** |
| Crosswalk punya target, pasien ada tetapi kode atau MRN berbeda | `CONFLICT` / `TARGET_MAPPING_MISMATCH` | Tidak | Tidak | **Ya** |
| `quilvian_patient_id` yang sama dipetakan lebih dari satu `legacy_pid` | `CONFLICT` / `DUPLICATE_TARGET_MAPPING` | Tidak | Tidak | **Ya** |
| Preview hilang, bukan batch kanonik, atau `validation_status` bukan `PENDING` | `CONFLICT` / `PREVIEW_STATE_CHANGED` | Tidak | Tidak | **Ya** |
| Perencana bukan `FINALIZED`, MRN akhir berbeda dari MRN usulan preview, atau formatnya bukan `00-00-00-00` | `CONFLICT` / `PLANNER_MISMATCH` | Tidak | Tidak | **Ya** |
| `PatientCode` preview sudah dipakai `MstPatient` mana pun | `CONFLICT` / `PATIENT_CODE_ALREADY_USED` | Tidak | Tidak | **Ya** |
| MRN akhir sudah dipakai `MstPatient` mana pun | `CONFLICT` / `MRN_ALREADY_USED` | Tidak | Tidak | **Ya** |
| `identity_number` preview sudah dipakai pasien yang tidak dihapus | `CONFLICT` / `IDENTITY_NUMBER_ALREADY_USED` — **tidak pernah** digabung ke pasien itu | Tidak | Tidak | **Ya** |
| `phone_number` preview berklasifikasi `SAFE_TO_STORE` sudah dipakai `MstPatient` lain — telepon itu bergeser sesudah audit | `CONFLICT` / `PHONE_ALREADY_USED` (`PAT-DEC-050`). Nilai preview tidak dibuang atau diubah diam-diam | Tidak | Tidak | **Ya** |
| Berkas tak dikenal sudah ada di path `rsmmc-bulk` pasien itu | `CONFLICT` / `QR_ARTIFACT_ALREADY_EXISTS` — gagal tertutup (`PAT-DEC-052`). QR historis Pilot di path bawaan **bukan** konflik; ia dilaporkan sebagai `historicalQrAtDefaultPath` | Tidak | Tidak — berkas tidak ditimpa dan tidak diadopsi | **Ya** |
| `full_name` kosong; kolom enum wajib atau boolean bernilai NULL; nilai yang bukan anggota enum — termasuk 0 untuk `patient_type`/`patient_status`; atau panjang melebihi batas kolom (B10) | `CONFLICT` / `PREVIEW_VALUE_INVALID` — **tidak ada** nama pengganti dan **tidak ada** pemetaan otomatis ke `Unknown` | Tidak | Tidak | **Ya** |
| `legacy_pid` tertunda (`280143`, `805596`) | `CONFLICT` / `DEFERRED_LEGACY_PID` — penjaga defensif; keduanya tidak ada di manifest | Tidak | Tidak | **Ya** |
| Kunci per pasien sedang dipegang proses lain | `CONFLICT` / `CONCURRENT_RUN` (`PAT-DEC-048`) — tanpa menunggu | Tidak | Tidak | **Ya** |
| `legacy_pid` tidak aman dikonversi ke Int32 untuk kunci dua bagian | `FAILED` / `LOCK_KEY_OUT_OF_RANGE` — gagal tertutup. Tidak terjadi pada `RSMMC-CANARY-A` v1, yang seluruhnya di rentang Int32 | Tidak | Tidak | **Ya** |
| Gagal teknis pada salah satu tahap B4.3 | `FAILED` + `failureStage` | Tidak | Tidak — transaksi dibatalkan | **Ya** |
| Belum diproses karena eksekusi sudah berhenti | `NOT_PROCESSED` | Tidak | Tidak | — |

Pada simulasi, seluruh pasien manifest diklasifikasikan dan proses **tidak pernah** berhenti
(`PAT-DEC-036`; perilaku simulasi ini bagian dari kontrak yang dikunci lewat `PAT-DEC-033`).

Ejaan nama status dan jenis konflik di atas boleh disesuaikan saat build; perilakunya dikunci (`PAT-DEC-033`);
perubahan ejaan pada build wajib dicatat di laporan task.

---

## B4. Proses bisnis

### B4.1 Ringkasan

| Unsur | Isi |
| --- | --- |
| **Tujuan** | Pasien RSMMC kanonik yang belum ada di Quilvian dibuat persis sesuai target preview yang sudah diaudit, tanpa nomor baru, tanpa pasien ganda, dan dapat dilanjutkan bila terhenti |
| **Pelaku** | Admin migrasi yang sudah login dan punya `Patient : Update`. Yang mengizinkan simulasi dan eksekusi: pemilik/leader lewat `PAT-GATE-002` (a) dan (b) |
| **Pemicu** | Admin memanggil endpoint — **bukan** bagian `BE-PAT-MIG-002`; butuh otorisasi runtime terpisah |
| **Prasyarat** | Environment `Staging` persis; seluruh gerbang B4.2 lulus; `PAT-GATE-001` sudah `CLOSED/PASS`; `PAT-GATE-005` `PASS` sebelum eksekusi |
| **Hasil akhir** | Untuk setiap pasien yang berhasil: satu `MstPatient` baru, satu baris crosswalk baru, satu QR baru. Preview, perencana, cadangan, crosswalk historis, Pilot 715, dan QR lama tidak berubah |

### B4.2 Gerbang tingkat panggilan

Dihitung ulang pada **setiap** panggilan, simulasi maupun eksekusi. Pada eksekusi, satu gerbang
gagal saja menolak seluruh permintaan (`409`) sebelum satu pasien pun diproses. Pada simulasi,
hasil gerbang dilaporkan dan seluruh pasien tetap diklasifikasikan.

| Kode | Yang diperiksa | Nilai yang diharapkan |
| --- | --- | --- |
| `PREVIEW_SCHEMA_SIGNATURE` | Kolom tiga tabel migrasi yang dipakai worker ada dengan tipe fisik yang disetujui (`PAT-DEC-063`), dibaca dari `information_schema.columns`: preview — `batch_id` uuid, `legacy_pid` bigint, `source_group` varchar, `proposed_patient_code`/`proposed_medical_record_number` text, tujuh kolom enum integer, empat boolean, `identity_type`/`identity_number`/`phone_number`/`address`/`legacy_old_rm`/`legacy_normalized_mrn`/`legacy_normalized_nik` text, `validation_status` varchar, `full_name` text atau varchar, `birth_date` date; perencana dan crosswalk sesuai `PAT-FACT-079`, `082`. Gerbang ini dievaluasi **pertama**, sebelum query bertipe lain dijalankan (`PAT-GAP-009`) | Seluruh kolom cocok |
| `APPROVED_BATCH` | `batchId` sama dengan batch kanonik | `45eeefba-dd39-6831-1bd2-d986851f7c1f` |
| `BATCH_ROW_EXISTS` | Batch kanonik ada di `migration.rsmmc_patient_batch` (sasaran foreign key crosswalk) | 1 baris |
| `PLANNER_FINALIZED_ROWS` | Baris perencana `FINALIZED` batch kanonik | 705.223 |
| `PREVIEW_ROWS` | Baris preview batch kanonik | 704.506 |
| `PREVIEW_UNIQUE` | `legacy_pid` / `PatientCode` / MRN unik pada preview kanonik | 704.506 / 704.506 / 704.506 |
| `DEFERRED_SET` | `legacy_pid` perencana `FINALIZED` yang tidak ada di preview kanonik dan bukan Pilot | Tepat `{280143, 805596}` |
| `DEFERRED_RESERVATIONS_UNUSED` | `PAT-RSMMC-101416`, `PAT-RSMMC-619077`, dan MRN akhir kedua pasien tertunda tidak dipakai `MstPatient` | 0 |
| `PATIENT_BASELINE` | **Berlaku sejak 22 Oktober 2026:** jumlah **seluruh** baris `public."MstPatient"` — tanpa filter `IsDelete`, sama dengan query pengukuran baseline — wajib sama dengan **741** + jumlah baris `migration.rsmmc_patient_crosswalk` dengan `last_batch_id` = batch kanonik dan `migration_status = 'MIGRATED_BULK_WORKER'` (`PAT-DEC-034`, `PAT-DEC-077`; nilai 741 = `PAT-FACT-095`, diukur read-only 22 Oktober 2026 sesudah `PAT-GATE-005` generasi 2 `PASS`). Yang dihitung `migration_status`, **bukan** `match_status`: pemetaan Pilot juga ber-`match_status = TARGET_CREATED` (`PAT-FACT-062`) tetapi ber-`migration_status` `MIGRATED_API_AUTO`/`MIGRATED_API_PILOT` (`PAT-FACT-063`), sehingga tidak ikut; 285 baris historis tanpa target juga tidak ikut. Nilai 741 ditanam pada profil batch, tidak dapat diubah lewat konfigurasi. *Teks `1.3.0` sebelum 22 Oktober 2026 — jejak:* Jumlah `MstPatient` = **baseline tetap yang disetujui** + jumlah crosswalk `MIGRATED_BULK_WORKER` (`PAT-DEC-034`, `PAT-DEC-077`). Baseline diukur **read-only** sesudah seluruh prasyarat source dan migration dideploy dan pemeriksa B14.2 lulus lagi, dicatat sebagai bukti, lalu ditanam pada profil batch seperti manifest — **bukan** dihitung dari jumlah yang ada saat eksekusi. Selama persiapan dan jendela Canary-A seluruh pembuatan pasien Staging dibekukan (`PAT-DEC-073`). Seeder demo Operating Room menambah satu pasien bila diaktifkan (`PAT-SRC-023`). *Teks `1.2.2`: "Jumlah `MstPatient` = 741 + …"; 741 (`PAT-FACT-038`) bukan nilai permanen* | Sama |
| `NORMAL_ALLOCATION_GUARD` | `BE-PAT-MIG-003` dan `BE-PAT-MIG-003A` **aktif dan terverifikasi** (`PAT-DEC-055`, `057`, `069`, `072`): pemeriksa baca-saja (B14.2) melaporkan `overallGuardReady = true` — yaitu guard aktif dengan konfigurasi sah di `Staging` persis (`PAT-DEC-066`, `067`), **seluruh** jalur pembuatan pasien non-migrasi yang berlaku aman reservasi (`normalPatientCreateProtected = true` dan `newbornPatientCreateProtected = true`, keduanya `RESERVED_FLOOR`), `PatientCode` berikutnya ≥ `PAT-RSMMC-705250` (`PAT-DEC-054`), dan MRN berikutnya ≥ `00-89-01-88` (`PAT-DEC-064`). Pembekuan operasional saja **tidak** meluluskan gerbang ini. Pada source `a52b3c98` gerbang ini gagal (`PAT-FACT-072`, `073`); pada build `BE-PAT-MIG-003` saja (generasi 1) jalur bayi belum terlindungi (`PAT-SRC-031`) | Lulus |
| `PREVIEW_CODE_UNMAPPED` | `MstPatient` yang memakai `PatientCode` preview tetapi tidak dipetakan crosswalk ke `legacy_pid` pemilik kode itu | 0 |
| `PREVIEW_MRN_UNMAPPED` | Sama, untuk MRN | 0 |
| `CROSSWALK_DUPLICATE_TARGET` | Grup `quilvian_patient_id` yang dipetakan lebih dari satu `legacy_pid` | 0 |
| `PILOT_CROSSWALK_INTACT` | Pemetaan Pilot: 715 target, 710 `MIGRATED_API_AUTO`, 5 `MIGRATED_API_PILOT` | Sama dengan `PAT-FACT-060`–`063` |
| `PILOT_PREVIEW_ROWS` | Preview Pilot historis | 715 |
| `BACKUP_ROWS` | Tabel cadangan Pilot | 715 |
| `MANIFEST_INTEGRITY` | Manifest eksekusi `RSMMC-CANARY-A` v1 (`PAT-DEC-056`): 25 nilai unik dalam urutan yang disetujui, sidik jari berurutan sama dengan `c43207eb…79ad`, seluruhnya ada di preview kanonik, sebaran kelompok sumber 7/6/6/6 | Lulus |

**Contoh.** Sesudah 25 pasien Canary-A berhasil, Staging berisi 766 pasien dan 25 baris crosswalk
worker: 766 = 741 + 25, gerbang lulus. Seorang petugas kiosk lalu mendaftarkan pasien uji di Staging,
melanggar pembekuan. Karena guard reservasi aktif (`PAT-FACT-093`), pasien itu memperoleh
`PAT-RSMMC-705250` / `00-89-01-88`, bukan nomor cadangan, sehingga `PREVIEW_CODE_UNMAPPED` tetap 0.
Namun panggilan worker berikutnya menghitung `PATIENT_BASELINE` 767 ≠ 766, lalu menolak eksekusi `409`.
Tidak ada pasien yang diproses sampai pemilik memutuskan penanganannya.

*Contoh lama — jejak, sebelum guard `BE-PAT-MIG-003` aktif:* pembangkit normal memberi
`PAT-RSMMC-00742` (`PAT-FACT-072`), sehingga `PATIENT_BASELINE` 767 ≠ 766 **dan**
`PREVIEW_CODE_UNMAPPED` 1 ≠ 0.

### B4.3 Langkah per pasien saat `dryRun=false`

Satu pasien satu transaksi. Pasien yang sudah *commit* tidak ikut dibatalkan oleh kegagalan
pasien sesudahnya.

1. **Buka transaksi** untuk pasien ini saja.
2. **Ambil kunci database per pasien tanpa menunggu** — `pg_try_advisory_xact_lock(key1, key2)`
   dua kunci (`PAT-DEC-048`). Nilai: key1 = `1173286842`, yaitu 32 bit pertama UUID batch
   kanonik (`0x45EEEFBA`), dicadangkan khusus untuk worker `BE-PAT-MIG-002` pada batch itu; key2 =
   `legacy_pid` yang dikonversi ke Int32 dengan pemeriksaan rentang. Di luar Int32 → `FAILED` /
   `LOCK_KEY_OUT_OF_RANGE`. Kunci sedang dipegang → `CONFLICT` / `CONCURRENT_RUN`. Kunci lepas
   sendiri saat transaksi selesai. Bentuk dua kunci tidak berbagi ruang kunci dengan
   `pg_advisory_xact_lock(hashtext(…))` satu kunci yang dipakai modul lain (`PAT-SRC-026`).
   *Contoh:* `legacy_pid` -667992 → kunci `(1173286842, -667992)`.
3. **Baca ulang preview** batch kanonik untuk `legacy_pid` ini, **sesudah** kunci didapat, dengan SQL berparameter bertipe (`batch_id` uuid, `legacy_pid` bigint — B9.1).
4. **Baca ulang perencana**: wajib `FINALIZED` dan MRN akhir = MRN usulan preview.
5. **Baca ulang crosswalk** berdasarkan kunci utama `legacy_pid`, lalu klasifikasikan (B3.2):
   bila sudah punya target dan cocok → `ALREADY_MIGRATED`, transaksi ditutup tanpa perubahan.
6. **Periksa kepemilikan** di `MstPatient`: `PatientCode`, MRN, identitas, dan telepon tidak dipakai
   pasien lain. Pasien yang ditandai terhapus ikut diperiksa untuk kode dan MRN, karena indeks
   uniknya tanpa syarat (`PAT-SRC-007`).
7. **Bentuk entity `MstPatient`** dari preview sesuai pemetaan B10, dengan `QrCodePath` = path
   `rsmmc-bulk` (`PAT-DEC-052`), lalu simpan (`INSERT_PATIENT`).
8. ~~**Tulis `IsMember = false` secara eksplisit** hanya atas baris yang baru di-insert
   (`PERSIST_IS_MEMBER`)~~ — **dihapus `1.3.0`** (`PAT-DEC-075`). Entity langkah 7 membawa
   `IsMember = false`, dan konfigurasi EF saat ini (`HasDefaultValue(true).HasSentinel(true)`)
   mengirim nilai itu eksplisit pada INSERT (`PAT-SRC-032`). Worker **tidak** mengeluarkan UPDATE
   `IsMember`, karena proyeksi membership milik crm-membership. Nomor langkah dipertahankan supaya
   rujukan "langkah 6–10" tetap terbaca. *Teks `1.2.2`: "Langkah ini ada karena EF tidak mengirim
   nilai `false` untuk kolom ber-default `true` (`PAT-SRC-008`)".*
9. **Tulis baris crosswalk baru** dengan SQL berparameter di transaksi yang sama
   (`INSERT_CROSSWALK`; `PAT-DEC-047`): `legacy_pid`, `legacy_old_rm` = `preview.legacy_old_rm`,
   `normalized_mrn` = `preview.legacy_normalized_mrn`, `normalized_nik` = `preview.legacy_normalized_nik`,
   `quilvian_patient_id` = `Id` pasien baru, `match_status = TARGET_CREATED`,
   `migration_status = MIGRATED_BULK_WORKER`, `last_batch_id` = batch kanonik, `created_at` = waktu
   database; `canonical_legacy_pid`, `review_reason`, `updated_at` = NULL.
10. **Baca ulang pasien dari database** tanpa *tracking* di dalam transaksi yang sama dan bandingkan
    setiap kolom asal preview, termasuk `IsMember = false` dan `IsActive = true` (`VERIFY_PERSISTED`;
    `PAT-DEC-053` bagian 2–5, `PAT-DEC-075`). Selisih → batal dan *rollback*. Sesudah transaksi,
    `IsMember` adalah proyeksi membership yang boleh berubah oleh lifecycle membership; audit
    pasca-Canary membandingkannya dengan status membership pasien, bukan dengan preview.
11. **Buat QR** untuk MRN akhir di path `rsmmc-bulk` (`PAT-DEC-052`) dengan `CreateNew` (`CREATE_QR`).
    Konflik → batal, berkas yang ada tidak disentuh.
12. **Commit** (`COMMIT`).
13. **Catat log** dengan aktor, lalu lanjut ke pasien berikutnya sampai `limit` tercapai.

**Mengapa QR dibuat sesudah data database disiapkan, tidak sebelumnya seperti Pilot.** Tabrakan
kode atau MRN, kegagalan crosswalk, dan selisih baca ulang tertangkap di langkah 6–10 **sebelum**
ada berkas yang ditulis. Akibatnya QR yatim yang harus dibersihkan jauh lebih jarang. QR tetap
sudah ada sebelum *commit*, sehingga tidak pernah ada pasien tersimpan yang menunjuk QR yang
belum dibuat. Urutan ini bagian dari kontrak yang disetujui lewat `PAT-DEC-033`.

### B4.4 Simulasi (`dryRun=true`)

Langkah 3–6 dijalankan sebagai **pembacaan saja**, tanpa transaksi tulis dan tanpa kunci. Gerbang
`NORMAL_ALLOCATION_GUARD` tetap dihitung dan dilaporkan.
Sesuai `PAT-DEC-046`, simulasi membaca filesystem tanpa menulis: keberadaan dan metadata berkas di
path rencana `rsmmc-bulk`, serta keberadaan, panjang, dan SHA-256 berkas di path bawaan historis
`patient-qrcodes/<MRN>/qrcode.png` sebagai informasi dan bukti awal `PAT-MIG2-AC-43`. Penentu
lokasinya tidak membuat folder. Langkah 7–12 **tidak** dijalankan: tidak ada insert, tidak ada baris
crosswalk, dan tidak ada folder atau berkas yang dibuat, diubah, atau dihapus.

### B4.5 Jalur tidak normal — kasus A sampai J

| Kasus | Keadaan | Status | `MstPatient` | Crosswalk | QR baru | QR lama | Lanjut ke pasien berikutnya? |
| --- | --- | --- | --- | --- | --- | --- | --- |
| A | Crosswalk belum ada | Jalur normal → `MIGRATED` | Dibuat | Baris baru | Dibuat | Tidak disentuh | Ya |
| B | Crosswalk ada, target NULL | `CONFLICT` / `CROSSWALK_EXISTS_WITHOUT_TARGET` | Tidak dibuat | Tidak disentuh | Tidak dibuat | Tidak disentuh | Tidak — jalur ini milik Canary-B |
| C | Crosswalk ada, target terisi | Lanjut ke D, E, atau F | — | — | — | — | — |
| D | Pasien target ada dan cocok | `ALREADY_MIGRATED` | Tidak berubah | Tidak berubah | Tidak dibuat | Tidak disentuh | Ya |
| E | Pasien target tidak ada | `CONFLICT` / `ORPHANED_CROSSWALK` | **Tidak** dibuat ulang | Tidak disentuh | Tidak dibuat | Tidak disentuh | Tidak |
| F | Pasien target ada, kode/MRN berbeda | `CONFLICT` / `TARGET_MAPPING_MISMATCH` | Tidak berubah | Tidak disentuh | Tidak dibuat | Tidak disentuh | Tidak |
| G | Berkas tak dikenal sudah ada di path `rsmmc-bulk` — misalnya QR yatim dari percobaan sebelumnya | `CONFLICT` / `QR_ARTIFACT_ALREADY_EXISTS` | Transaksi dibatalkan — tidak tersimpan | Dibatalkan | **Tidak** ditimpa | Tidak disentuh | Tidak |
| H | Data database sudah disiapkan, tetapi hasil penulisan QR tidak pasti (galat sesudah berkas dibuka) | `FAILED` / `CREATE_QR` | Transaksi dibatalkan — **tidak pernah commit bila QR tidak pasti** | Dibatalkan | Berkas sebagian **dipertahankan**, tidak dihapus otomatis | Tidak disentuh | Tidak |
| I | QR berhasil dibuat, tetapi transaksi dibatalkan sebelum *commit* dicoba | `FAILED` + tahap | Dibatalkan | Dibatalkan | Dihapus **hanya** bila kepemilikan terbukti (penanda + panjang + SHA-256); selain itu dipertahankan dan dilaporkan | Tidak disentuh | Tidak |
| I′ | Proses mati sesudah QR dibuat, sebelum *commit* | Tidak ada respons | Dibatalkan otomatis oleh database | Dibatalkan | Tertinggal sebagai QR yatim | Tidak disentuh | Panggilan berikutnya melihat kasus G |
| J | `CommitAsync` melempar galat | `FAILED` / `COMMIT_OUTCOME_UNCERTAIN` | Tidak dapat dipastikan | Tidak dapat dipastikan | **Dipertahankan** — tidak pernah dihapus otomatis | Tidak disentuh | Tidak; tanpa *retry* otomatis |

**Bagaimana panggilan berikutnya menyelesaikan kasus J.** Panggilan berikutnya membaca crosswalk
sesudah kunci didapat. Bila *commit* ternyata berhasil, crosswalk punya target yang cocok →
`ALREADY_MIGRATED`. Bila ternyata gagal, crosswalk tidak ada tetapi berkas QR ada → kasus G,
dan operator memeriksa berkas itu. Dalam kedua cara, **tidak ada pasien kedua**.

**Contoh.** Canary-A `limit` 25, eksekusi, dengan path `rsmmc-bulk` (`PAT-DEC-052`). Pasien 1–3 `MIGRATED`.
Pasien 4 kasus G karena path namespace-nya ternyata berisi QR yatim dari percobaan sebelumnya yang
prosesnya mati sebelum *commit*. Hasil: 3 `MIGRATED`, 1 `CONFLICT`, 21 `NOT_PROCESSED`;
`stopped = true`. Database: tiga pasien baru dan tiga baris crosswalk baru; pasien 4 tidak ada.
QR historis Pilot di path bawaan MRN pasien mana pun tetap utuh.

*Contoh lama — dipertahankan sebagai jejak:* pada draft 14 Oktober 2026, contoh ini memakai pasien
4 yang folder QR bawaannya berisi QR lama Pilot. Bukti Step 16 menunjukkan keadaan itu berlaku
untuk 715 pasien, sehingga diperlakukan sebagai isu P0 `PAT-OQ-028`, bukan konflik biasa.

---

## B5. Idempotensi dan konkurensi

Pasien ganda dicegah oleh **tiga lapis**, dari yang paling depan:

| Lapis | Mekanisme | Yang dijaganya |
| --- | --- | --- |
| 1 | Kunci database per `batchId` + `legacy_pid`, lalu seluruh pembacaan diulang sesudah kunci didapat | Dua proses tidak pernah mengerjakan pasien yang sama pada saat yang sama |
| 2 | Kunci utama crosswalk `legacy_pid` | Satu `legacy_pid` tidak dapat punya dua baris crosswalk |
| 3 | Indeks unik `MstPatient.PatientCode` dan `MstPatient.MedicalRecordNumber` (`PAT-SRC-007`) | Insert kedua dengan kode atau MRN yang sama ditolak database, walau lapis 1 dan 2 terlewati |

Karena `PatientCode` dan MRN ditentukan preview dan perencana — bukan dibangkitkan — pasien yang
sama selalu mendapat kode dan MRN yang sama. Itulah sebabnya lapis 3 efektif: percobaan kedua
pasti bertabrakan.

Kunci yang hanya hidup di memori proses, Count+1, dan Max+1 **tidak** dipakai (`PAT-DEC-023`,
`QBE-CODE-003`).

**Contoh.** Admin A dan admin B menekan eksekusi hampir bersamaan. A mendapat kunci untuk
`legacy_pid` -667995 lebih dulu. B mencoba kunci yang sama: dengan try-lock dua kunci `PAT-DEC-048`, B
langsung melaporkan `CONFLICT` / `CONCURRENT_RUN` dan berhenti. Sesudah A selesai, panggilan ulang
B melaporkan -667995 sebagai `ALREADY_MIGRATED`.

---

## B6. Kontrak QR

| Hal | Ketentuan |
| --- | --- |
| Isi QR dan nama berkas | Sama dengan Bagian A bagian 5: isi = MRN akhir berformat lewat `PatientQrPayloadBuilder`; nama berkas `qrcode.png` |
| Folder dan path publik | Disetujui `PAT-DEC-052` (A1): `patient-qrcodes/rsmmc-bulk/<MRN>/qrcode.png` untuk **seluruh** pasien bulk, sehingga path publiknya `/uploads/patient-qrcodes/rsmmc-bulk/<MRN>/qrcode.png`. Pola `patient-qrcodes/<MRN>/qrcode.png` hanya dipakai pasien normal dan QR historis |
| Bukti bahwa path baru aman | Konsumen memakai `QrCodePath` tersimpan, tidak ada yang menyusun path dari MRN (`PAT-SRC-019`, `020`); seluruh folder upload disajikan (`PAT-SRC-021`) |
| Penulis berkas | `RsmmcPilotQrArtifactStore` dipakai **apa adanya**, dengan folder dasar `patient-qrcodes/rsmmc-bulk` lewat layout terpisah (`PAT-SRC-022`): pemeriksaan awal, `FileMode.CreateNew`, bukti kepemilikan penanda + panjang + SHA-256 (`PAT-DEC-024`) |
| Berkas tujuan sudah ada | Tidak ditimpa, tidak diadopsi; `CONFLICT` — gagal tertutup |
| QR historis 715 tabrakan | Dibiarkan byte demi byte (`PAT-DEC-039`) dan **tidak pernah** diadopsi (`PAT-DEC-052`) |
| `MIGRATED_QR_INCONSISTENT` | `QrCodePath` dibandingkan dengan path `rsmmc-bulk`, bukan dengan pola lama |
| Simulasi | Membaca path rencana `rsmmc-bulk` dan path bawaan historis — keberadaan, metadata, dan SHA-256 historis — tanpa membuat folder atau mengubah berkas apa pun (`PAT-DEC-046`) |
| QR lama Pilot | Tidak pernah dikirim ke jalur pembersihan; tidak pernah dihapus, dipindah, atau ditimpa |
| Git | Berkas QR adalah artefak runtime dan **tidak boleh** masuk Git |

---

## B7. Gerbang environment

Sama dengan Bagian A bagian 6. `RsmmcPilotEnvironmentGate` dipakai apa adanya: hanya `Staging`
persis, perbandingan ordinal, untuk simulasi **dan** eksekusi, dievaluasi sebelum tabel apa pun
dibaca. Tidak ada pengaturan konfigurasi yang dapat melonggarkannya, sehingga eksekusi di
produksi **tidak mungkin** lewat jalur kode ini.

---

## B8. Hak akses

| Ketentuan | Isi |
| --- | --- |
| Login | `[Authorize]` tingkat controller (`PatientController.cs` baris 42) |
| Izin | `[AccessAction("Update", "Migrate RSMMC Bulk Patients", AccessType = AccessTypes.Update, …)]` beserta `[AccessPermission("Patient", "Update")]`. Argumen kedua `[AccessPermission]` sama persis dengan argumen pertama `[AccessAction]` |
| Letak aksi | Sesudah aksi `Update` yang sudah ada, supaya label layar Akses Role tidak berubah — sama dengan alasan pada `BE-PAT-MIG-001` |
| Aktor | Eksekusi menolak pengguna yang `Id`-nya tidak dapat dibaca (`400` / `RSMMC_BULK_ACTOR_UNKNOWN`), karena `CreateBy` dan log wajib berisi aktor |
| Dilarang | `IsInRole`, daftar nama peran, atau `UserType` sebagai penentu izin; `[AllowAnonymous]` |

Risiko yang sama dengan `PAT-RSK-005` berlaku: setiap pemegang `Patient : Update` di Staging dapat
memanggil endpoint ini. Penjaga sesungguhnya adalah gerbang runtime `PAT-GATE-002` dan manifest
beku. Izin tetap `Patient : Update` sesuai `PAT-DEC-028`; `PAT-DEC-044` tidak mengubahnya.

---

## B9. Data yang dibaca, diubah, dan dilarang diubah

| Tabel | Dibaca | Diubah | Dasar |
| --- | --- | --- | --- |
| `migration.rsmmc_patient_target_preview` | Ya | **Tidak pernah** | `PAT-DEC-019`, `PAT-DEC-035` |
| `migration.rsmmc_patient_mrn_plan` | Ya | **Tidak pernah** | `PAT-DEC-019`, `PAT-DEC-021` |
| `migration.rsmmc_patient_crosswalk` | Ya | **Hanya `INSERT`** untuk `legacy_pid` yang belum punya baris, dengan kolom sesuai `PAT-DEC-047` (B4.3 langkah 9). Tidak ada `UPDATE` atau `DELETE` pada build pertama. `normalized_nik` disimpan sebagai bukti saja dan tidak pernah dibaca untuk mencocokkan pasien | `PAT-DEC-027`, `PAT-DEC-031`, `PAT-DEC-047` |
| `migration.rsmmc_patient_batch` | Ya — gerbang | **Tidak pernah** | — |
| `migration.rsmmc_patient_pilot_reconcile_backup` | Ya — hitungan dan penanda QR lama | **Tidak pernah** | `PAT-DEC-009`, `PAT-DEC-019` |
| `public."MstPatient"` | Ya | **Hanya `INSERT`** pasien baru. `IsMember = false` ikut terkirim pada INSERT itu, lalu dibaca ulang `AsNoTracking` di transaksi yang sama dan diverifikasi; selisih → rollback (`PAT-DEC-075`). **Tidak ada** UPDATE khusus untuk memaksa `IsMember = false`. Pasien yang sudah ada — termasuk Pilot 715 — tidak pernah di-*update* atau dihapus. *Teks `1.2.2` — jejak: "ditambah satu penulisan `IsMember = false` atas baris yang baru di-insert di transaksi yang sama (`PAT-DEC-053`)"; dicabut `PAT-DEC-075`* | `PAT-DEC-019`, `PAT-DEC-025`, `PAT-DEC-041`, `PAT-DEC-075` |
| Tabel lain | — | Tidak | — |

Tabel `migration.rsmmc_*` tetap **tidak** dipetakan ke model EF. Pembacaan dan satu-satunya
penulisan crosswalk memakai SQL berparameter **bertipe** sesuai skema fisik `PAT-FACT-079`–`083`,
di koneksi dan transaksi `ApplicationDbContext` yang sama, tanpa EF migration dan tanpa perubahan
skema (`PAT-DEC-019`, `PAT-DEC-063`). Cast `::text` seperti Pilot **tidak** dipakai. Model dan
konfigurasi `MstPatient` tidak berubah (`PAT-DEC-053`). *Teks lama: "bentuk SQL final menunggu
`PAT-OQ-019`"; juga merujuk `PAT-OQ-017` pilihan B, yang kini `SUPERSEDED`.*

### B9.1 Tipe parameter dan kolom

| Kolom | Tipe PostgreSQL | Tipe C# | Keterangan |
| --- | --- | --- | --- |
| `batch_id` (ketiga tabel), `last_batch_id` | `uuid` | `Guid` | Batch kanonik `45eeefba-dd39-6831-1bd2-d986851f7c1f` |
| `legacy_pid` | `bigint` | `long` | Tidak pernah dibandingkan sebagai teks (`PAT-DEC-038`) |
| `quilvian_patient_id` | `uuid` | `Guid?` | NULL berarti crosswalk tanpa target |
| `match_status`, `migration_status` | `varchar(30)` | `string` | `TARGET_CREATED` 14 karakter, `MIGRATED_BULK_WORKER` 20 karakter (`PAT-FACT-080`) |
| `plan_status`, `final_medical_record_number` | `varchar(30)`, `varchar(50)` | `string` | `FINALIZED`; format `00-00-00-00` |
| `source_group`, `validation_status` | `varchar(40)` | `string` | `validation_status` NULL diperlakukan bukan `PENDING` |
| Tujuh kolom enum preview | `integer` | `int?` | Dipetakan ketat ke enum (B10) |
| `is_member`, `is_newborn`, `is_deceased`, `is_active` | `boolean` | `bool?` | NULL → `PREVIEW_VALUE_INVALID` |
| `full_name` | `text` (atau `varchar`) | `string?` | Dijaga `PREVIEW_SCHEMA_SIGNATURE` |
| `birth_date` | `date` | `DateTime?` / `DateOnly?` | Dijaga `PREVIEW_SCHEMA_SIGNATURE` |
| Kolom teks lain | `text` | `string?` | Apa adanya, tanpa perapian |
| `created_at` crosswalk | `timestamptz` | — | Diisi waktu database |

**Contoh.** Pembacaan preview untuk satu pasien berbentuk `… WHERE batch_id = @batchId AND
legacy_pid = @legacyPid` dengan `@batchId` bertipe `uuid` dan `@legacyPid` bertipe `bigint`,
sehingga primary key `(batch_id, legacy_pid)` terpakai. Bentuk Pilot `batch_id::text = …` tidak
dipakai.

---

## B10. Pemetaan preview → `MstPatient`

| Field `MstPatient` | Sumber | Catatan |
| --- | --- | --- |
| `Id` | `Guid` baru dari service | Identifier teknis, bukan nomor bisnis |
| `PatientCode` | `proposed_patient_code` apa adanya | Pembangkit normal dilarang (`PAT-DEC-020`) |
| `MedicalRecordNumber` | `mrn_plan.final_medical_record_number` | Wajib sama dengan `proposed_medical_record_number` (`PAT-DEC-021`) |
| `PatientType`, `PatientStatus`, `RegistrationSource`, `Religion`, `MaritalStatus`, `BloodType` | Kolom preview bernama sama, `integer` (`PAT-FACT-083`) | Dipetakan ketat: nilai harus anggota enum. NULL atau bukan anggota enum → `PREVIEW_VALUE_INVALID`. `PatientType` dan `PatientStatus` tidak punya anggota bernilai 0, sehingga 0 juga tidak sah. Default database kolom-kolom ini tidak menghilangkan nilai yang dikirim (`PAT-SRC-029`) |
| `Gender` | `gender`, `integer` | Kolom `MstPatient` *nullable* tanpa default: NULL → NULL; nilai lain harus anggota enum (`PAT-GAP-010`) |
| `FullName` | `full_name` apa adanya | Kosong → konflik; **tidak ada** nama pengganti (`PAT-DEC-030`) |
| `BirthDate` | `birth_date` | Kolom `date` di kedua sisi; boleh NULL. Tipenya dijaga `PREVIEW_SCHEMA_SIGNATURE` (`PAT-GAP-009`) |
| `IdentityType`, `IdentityNumber`, `PhoneNumber`, `Address` | Kolom preview bernama sama, apa adanya | Boleh kosong; **sensitif** — tidak masuk log |
| `IsMember` | `is_member` — seluruh preview `false` (`PAT-FACT-074`) | Dikirim eksplisit pada INSERT karena sentinel `true` (`PAT-SRC-032`), lalu diverifikasi ulang di transaksi yang sama (`PAT-DEC-041`, `PAT-DEC-075`). Tanpa UPDATE khusus. Preview tidak boleh diubah menjadi `true` |
| `IsActive` | `is_active` — seluruh preview `true` (`PAT-FACT-074`) | Diverifikasi ulang. `IsActive = false` bukan kebutuhan bulk |
| `IsNewborn`, `IsDeceased` | Kolom preview bernama sama | Diverifikasi ulang; default database `false` sehingga nilai `false` tidak hilang. NULL → `PREVIEW_VALUE_INVALID` |
| `QrCodePath` | Path publik QR yang dibuat | B6 |
| `NickName`, `BirthPlace`, `WhatsAppNumber`, `Email`, `CountryId`, `ProvinceId`, `CityId`, `DistrictId`, `PostalCodeId`, `PhotoPath`, `DefaultMembershipTierId`, `ActivePatientMembershipId`, `MotherPatientId`, `BirthOrder`, `BirthWeightGram`, `BirthLengthCm`, `BirthTime`, `DeliveryMethod`, `DeceasedDate`, `MergedToPatientId`, `MergeReason`, `Notes` | **Kosong (NULL)** | Seluruhnya *nullable* tanpa default database (`MstPatient.cs`, `MstPatientConfiguration.cs`), sehingga NULL adalah default model yang terbukti aman. Tidak ada isi yang dikarang |
| `Citizenship`, `NationalityCountryId`, `DeliveryCaseId`, `NewbornRegistrationKeyHash`, `NewbornRegistrationFingerprint` | **Kosong (NULL)** | Kolom baru sejak desain `1.2.0` (`PAT-SRC-033`) tanpa pemetaan legacy yang disetujui (`PAT-DEC-076`); nilai tidak dikarang. Indeks unik berfilter `IS NOT NULL` tidak terpengaruh. Migration `20261016080745_ReviseNewbornAdmissionFlow` wajib sudah diterapkan di Staging sebelum runtime |
| `CreateDateTime`, `CreateBy` | Waktu UTC saat itu; aktor | `PAT-DEC-025` |
| `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy` | Kosong / `Guid.Empty` | Default `IdentityModel` |
| `IsDelete`, `IsCancel` | `false` | Default `IdentityModel` |

Tidak ada perapian teks — misalnya *trim* atau huruf kecil — karena preview adalah snapshot yang
sudah disetujui dan audit kelebihan panjangnya nol (`PAT-FACT-047`).

**Contoh.** Preview: `is_deceased = true`, tanpa tanggal wafat. Pasien dibuat dengan
`IsDeceased = true` dan `DeceasedDate` kosong, karena preview tidak membawa tanggal itu dan
worker tidak boleh mengarangnya.

---

## B11. Log dan audit

| Kejadian | Level | Isi minimum |
| --- | --- | --- |
| Environment ditolak | Peringatan | `batchId`, `dryRun`, nama environment, aktor |
| Gerbang tingkat panggilan gagal pada eksekusi | Peringatan | `batchId`, kode gerbang yang gagal, aktor |
| Simulasi selesai | Info | Ringkasan jumlah, sidik jari manifest, aktor |
| Satu pasien `MIGRATED` | Info | `batchId`, `legacyPid`, `PatientId`, `PatientCode`, MRN, path QR, aktor |
| Satu pasien `CONFLICT` | Peringatan | Sama, ditambah `conflictType` |
| Satu pasien `FAILED` | Error | Sama, ditambah `failureStage` dan galat |
| Pembersihan QR tidak terbukti | Peringatan atau Error | Path, panjang dan SHA-256 yang diharapkan, hasil pembersihan |

Nama (`fullName`), NIK, telepon, dan alamat **tidak** masuk log (`PAT-DEC-049`). Nama properti log
tidak memakai `Id`, `UserId`, atau `Name`, karena `LoggerService` membacanya sebagai identitas
pengguna log — sama dengan catatan pada service Pilot. Jejak tahan lama yang sesungguhnya ada di
database: `CreateBy`/`CreateDateTime` pada `MstPatient` dan `created_at` pada crosswalk; log
aplikasi tidak menggantikannya (`QBE-AUD-001`).

---

## B12. Yang tidak termasuk Bagian B

- Mode `CANARY_B`, gelombang, rentang, atau pemrosesan latar belakang otomatis.
- `UPDATE` atau `DELETE` pada crosswalk, preview, perencana, cadangan, atau `MstPatient` yang sudah
  ada.
- Perubahan endpoint Bagian A atau konfigurasi EF `MstPatient` (`PAT-DEC-053`).
- Perubahan pembangkit nomor dan alur pendaftaran normal — milik task prasyarat `BE-PAT-MIG-003`
  (`PAT-DEC-057`); dampaknya dicatat pada B14.
- EF migration, perubahan skema, perintah database, pemanggilan endpoint, pembuatan QR, dan
  deployment selama implementasi.
- Pembuatan membership, kontak darurat, dokumen identitas, atau data turunan pasien lain.
- Layar frontend (`PAT-DEC-060`).

---

## B13. Traceability Bagian B

| Bagian | Keputusan | Acceptance criteria |
| --- | --- | --- |
| B2, B2.1–B2.3 | `PAT-DEC-028`, `044`, `045`, `049` | `PAT-MIG2-AC-01`, `02`, `21`, `23`, `37`–`40` |
| B3 | `PAT-DEC-022`, `026`, `030`, `036`, `045`, `048`, `050`, `056` | `PAT-MIG2-AC-09`, `12`–`16`, `26`–`28`, `48`, `50` |
| B4.2 | `PAT-DEC-019`, `021`, `034`, `042`, `055`, `056`, `063`, `064`, `066`, `072`, `077` | `PAT-MIG2-AC-03`–`08`, `29`, `46`, `51` |
| B4.3–B4.5 | `PAT-DEC-023`, `024`, `046`, `047`, `048`, `052`, `053`, `075` | `PAT-MIG2-AC-10`–`22`, `33`, `49` |
| B5 | `PAT-DEC-023`, `048` | `PAT-MIG2-AC-10`, `11`, `35` |
| B6 | `PAT-DEC-024`, `039`, `043`, `046`, `052`, `062` | `PAT-MIG2-AC-17`–`20`, `30`, `43` |
| B7 | `PAT-DEC-019`, `028` | `PAT-MIG2-AC-01`, `31` |
| B8 | `PAT-DEC-028` | `PAT-MIG2-AC-39` |
| B9, B9.1 | `PAT-DEC-019`, `027`, `031`, `035`, `047`, `053`, `063`, `075` | `PAT-MIG2-AC-29`, `32`, `34`, `42`, `49`, `51` |
| B10 | `PAT-DEC-020`, `025`, `030`, `041`, `053`, `063`, `075`, `076` | `PAT-MIG2-AC-24`, `25`, `27`, `33`, `47`, `51` |
| B11 | `PAT-DEC-025`, `049` | `PAT-MIG2-AC-23` |
| B14–B14.3 | `PAT-DEC-042`, `054`, `055`, `057`, `064`, `066`, `067`, `068` | `PAT-MIG2-AC-46`; `PAT-MIG3-AC-01`–`PAT-MIG3-AC-12`, `14` |
| B15 | `PAT-DEC-053`, `065`, `075` | `PAT-MIG2-AC-47`; `PAT-MIG3-AC-13` |

---

## B14. Perlindungan reservasi `PatientCode` dan MRN — task prasyarat `BE-PAT-MIG-003`

**Masalahnya, dalam bahasa sederhana.** Target preview sudah "memesan" `PatientCode` 742 sampai
705.249 dan seluruh MRN akhir perencana untuk pasien RSMMC. Pemesanan itu hanya tercatat di tabel
migrasi; pembangkit nomor pendaftaran pasien normal tidak mengetahuinya. Ia memilih nomor kosong
terkecil di `MstPatient`, sehingga pendaftaran berikutnya di Staging langsung mengambil
`PAT-RSMMC-00742` dan `00-00-00-27` (`PAT-FACT-072`, `073`). Pasien bulk pemilik nomor itu
kelak tidak dapat dibuat lagi: indeks unik menolaknya.

**Contoh.** Hari Senin petugas mendaftarkan pasien uji "Rina Contoh" di Staging. Ia mendapat
`PAT-RSMMC-00742` dan `00-00-00-27`. Hari Selasa worker mencoba membuat pasien bulk yang
memesan kode dan MRN itu, lalu berhenti dengan `PATIENT_CODE_ALREADY_USED`. Karena `PAT-INF-001`
terkonfirmasi (`PAT-FACT-089`), pendaftaran Senin itu juga sudah menimpa QR historis Pilot di folder `00-00-00-27`.

| Lapis | Mekanisme | Status | Keputusan / task |
| --- | --- | --- | --- |
| 1 — pencegahan final | Allocator `PatientCode` normal dengan lantai tahan lama: suffix pertama yang belum dipakai ≥ 705.250, diserialkan di database, indeks unik sebagai pengaman terakhir | `OWNER_APPROVED` | `PAT-DEC-054`, `066`; `BE-PAT-MIG-003` |
| 1 — pencegahan final | Allocator MRN normal dengan lantai: MRN numerik pertama yang belum dipakai ≥ 890.188 (`00-89-01-88`). MRN akhir `FINALIZED` tertinggi 890.187 (`PAT-FACT-084`), sehingga lantai ini tidak pernah memberi MRN cadangan, termasuk MRN dua pasien tertunda. Sisa kapasitas 99.109.812 nomor | `OWNER_APPROVED` | `PAT-DEC-064`, `066`; `BE-PAT-MIG-003` |
| 1 — pencegahan final | Jalur pendaftaran bayi memakai allocator berlantai dan kunci yang sama di `Staging` persis; di luar itu `NumberSeries` bayi tidak berubah (B14.4) | `OWNER_APPROVED` | `PAT-DEC-070`, `071`; `BE-PAT-MIG-003A` |
| 2 — pencegahan sementara | Pembekuan pembuatan pasien di Staging — normal, admin, kiosk, dan bayi — sampai jendela Canary-A selesai. Kontrol operasional, bukan route yang dimatikan. Bukan strategi final | `OWNER_APPROVED`; dinyatakan berlaku (`PAT-FACT-078`); diperluas 19 Oktober 2026 | `PAT-DEC-055`, `PAT-DEC-073` |
| 3 — deteksi | Gerbang worker `NORMAL_ALLOCATION_GUARD`, `PATIENT_BASELINE`, `PREVIEW_CODE_UNMAPPED`, `PREVIEW_MRN_UNMAPPED`, `DEFERRED_RESERVATIONS_UNUSED` (B4.2) | Bagian kontrak ini | `PAT-DEC-042`, `PAT-DEC-055` |

**Catatan pembekuan (`PAT-SRC-027`).** `POST /` dan `POST /kiosk` hanya dijaga policy
`KioskRead`; `[AccessAction("Create", …)]` pada keduanya hanya metadata. Hanya `POST /admin` dijaga
`[AccessPermission("Patient", "Create")]`. Mencabut izin `Patient : Create` lewat layar Akses Role
karena itu **tidak** menutup dua dari tiga jalur. Bukti pembekuan untuk `PAT-GATE-002` wajib
menunjukkan ketiga jalur tertutup.

### B14.1 Dampak pada endpoint pendaftaran pasien normal — milik `BE-PAT-MIG-003`

### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat pasien baru | Login + policy `KioskRead`; `[AccessAction("Create")]` tanpa `[AccessPermission]` | `CreatePatientRequest` | `ApiResponse<PatientCreateResponse>` | Sudah ada — bentuk **tidak** berubah; cara memilih `PatientCode` dan MRN berubah (**Rencana**) |
| `POST` | `/kiosk` | Membuat pasien baru dari kiosk | Sama dengan `POST /` | `CreatePatientRequest` | `ApiResponse<PatientCreateResponse>` | Sama |
| `POST` | `/admin` | Membuat pasien baru dari layar admin | `Patient : Create` | `CreatePatientRequest` | `ApiResponse<PatientCreateResponse>` | Sama; meneruskan ke `CreatePatient` |

Ketiganya bermuara pada satu method `CreatePatient` (`PAT-SRC-023`), sehingga allocator baru hanya
dipasang di satu titik **untuk pendaftaran normal**.

> **Koreksi `1.3.0` (`PAT-DEC-072`).** Pernyataan lama bahwa seluruh insert `MstPatient` bermuara pada
> `CreatePatient` tidak berlaku lagi. Sejak commit upstream `3a0189c2` ada jalur kedua: pendaftaran bayi
> `POST /newborns` → `NewbornPatientRegistrationService` (`PAT-SRC-031`), yang dilindungi lewat B14.4.
> Jalur ketiga, `OperatingRoomDemoSeeder`, tetap memakai kode `DEMO-OPR-*` dan hanya aktif bila
> `Seeders:RunOperatingRoomDemoSeed` dinyalakan di luar Produksi.

| Hal | Sekarang | Sesudah `BE-PAT-MIG-003` |
| --- | --- | --- |
| `PatientCode` berikutnya | Nomor kosong terkecil — saat ini `PAT-RSMMC-00742`, yang dicadangkan | Saat guard aktif: suffix pertama yang belum dipakai ≥ 705.250 — misalnya `PAT-RSMMC-705250`; bila 705.250 dan 705.251 sudah dipakai, `PAT-RSMMC-705252` |
| Letak allocator | Method `private` di controller | Module service (`QBE-SVC-001`, `QBE-CODE-001`); controller tidak lagi mengalokasikan nomor (`QBE-CODE-002`) |
| Serialisasi | Tidak ada; dua pendaftaran bersamaan dapat bertabrakan di indeks unik | Saat guard aktif: `pg_advisory_xact_lock(1346458697, 1313821261)` — dua kunci Int32 tetap: namespace `0x50415449` (`"PATI"`, PATIENT_IDENTIFIER_ALLOCATION) dan kunci `0x4E4F524D` (`"NORM"`, NORMAL_PATIENT_CREATE), di dalam transaksi `CreatePatient` yang sudah ada (`PAT-SRC-028`) — pendaftaran kedua **menunggu**, tidak ditolak. Indeks unik `PatientCode` dan MRN tetap jaring terakhir (`PAT-DEC-068`; *teks `1.2.1`: `hashtext('quilvian.patient.identifier-allocation')`*) |
| MRN berikutnya | Nomor kosong terkecil — saat ini `00-00-00-27`, yang dicadangkan | Saat guard aktif: MRN numerik pertama yang belum dipakai ≥ 890.188 — misalnya `00-89-01-88`; bila `00-89-01-88` sudah dipakai, `00-89-01-89` (`PAT-DEC-064`) |
| Environment dan konfigurasi | Semua | Guard dipakai **wajib** di `EnvironmentName == "Staging"` persis dan hanya berjalan bila `RsmmcMigrationReservationGuard.Enabled == true`; di Staging persis, nilai lain — `false`, hilang, salah bentuk, tidak terbaca — gagal tertutup. Di luar Staging persis tidak berubah. Rinciannya B14.3 (`PAT-DEC-066`, `067`) |
| Saat guard tidak aktif | — | Algoritma nomor kosong terkecil yang sama dengan sekarang, dipindah ke module service **tanpa perubahan hasil** dan **tanpa** kunci baru — penomoran Produksi tidak berubah (`PAT-GAP-005`) |
| Kode status baru | — | Hanya di Staging persis bila `Enabled` bukan `true` — `false`, hilang, salah bentuk, atau tidak terbaca: `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID`, sebelum insert pasien, penulisan QR, dan penulisan foto (B14.3). Respons sukses dan galat lain tidak berubah |
| QR pendaftaran normal | `patient-qrcodes/<MRN>/qrcode.png` lewat `File.WriteAllBytes` | Tidak diubah task ini. Saat guard aktif, MRN cadangan tidak lagi dipakai, sehingga folder 715 QR historis Pilot tidak lagi menjadi tujuan pendaftaran normal |
| `IsMember` pendaftaran normal | Tersimpan `true` bila request `false` (`PAT-SRC-024`) — **tidak berlaku lagi** sejak `0c6e3145` (`PAT-SRC-030`, `032`) | Tidak diubah task ini (`PAT-DEC-053`) |

### B14.2 Pemeriksa baca-saja untuk `NORMAL_ALLOCATION_GUARD`

`BE-PAT-MIG-003` menyediakan pemeriksa di tingkat service yang menghitung `PatientCode` dan MRN normal
berikutnya **tanpa** mengalokasikan, tanpa mengambil kunci, dan tanpa menulis apa pun (`PAT-GAP-008`).
Sejak `1.2.1` (`PAT-DEC-067`) pemeriksa yang sama juga dapat dipanggil pemilik untuk `PAT-GATE-005`
lewat endpoint admin baca-saja khusus `Staging` persis.

### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/admin/migration/rsmmc-reservation-guard` | Melaporkan keadaan guard reservasi nomor pasien, perlindungan jalur pendaftaran normal **dan** bayi, serta nomor yang **akan** diberikan pendaftaran berikutnya, tanpa mencadangkan apa pun | `Patient : Update` | — | `ApiResponse<RsmmcReservationGuardStatusResponse>` | **Tersedia** sejak `BE-PAT-MIG-003` (generasi 1); keluaran diperluas `BE-PAT-MIG-003A` (`1.3.0`, generasi 2) — aditif |

Kode status: `200` laporan dikembalikan, termasuk bila guard belum siap; `401` belum login; `403` tidak
punya izin, **atau** environment bukan `Staging` persis (kode `RSMMC_RESERVATION_GUARD_ENVIRONMENT_NOT_ALLOWED`).

| Keluaran | Isi | Contoh di Staging sesudah deploy dengan `Enabled = true` |
| --- | --- | --- |
| `environment` | Nama environment | `Staging` |
| `guardState` | `ACTIVE`, `DISABLED` (`Enabled = false`), `CONFIG_MISSING`, `CONFIG_MALFORMED`, atau `NOT_APPLICABLE` (di luar Staging persis) | `ACTIVE` |
| `guardEnabled` | Guard benar-benar berjalan | `true` |
| `guardConfigurationValid` | Konfigurasi sah untuk environment ini; di Staging persis hanya `Enabled = true` yang sah | `true` |
| `patientCodeFloor`, `patientCodeFloorFormatted` | Lantai kode | `705250`, `PAT-RSMMC-705250` |
| `mrnFloor`, `mrnFloorFormatted` | Lantai MRN | `890188`, `00-89-01-88` |
| `nextPatientCodeNumeric`, `nextPatientCodeFormatted` | Kode yang **akan** diberikan pendaftaran berikutnya | `705250`, `PAT-RSMMC-705250` |
| `nextMrnNumeric`, `nextMrnFormatted` | MRN yang **akan** diberikan pendaftaran berikutnya | `890188`, `00-89-01-88` |
| `patientCodeFloorSatisfied`, `mrnFloorSatisfied` | Nomor berikutnya ≥ lantai | `true`, `true` |
| `normalPatientCreateAllocationMode` | Cara alokasi pendaftaran normal (`POST /`, `/kiosk`, `/admin`): `RESERVED_FLOOR` (lantai + kunci), `FAIL_CLOSED` (ditolak sebelum alokasi), atau `LEGACY` (penomoran lama, di luar Staging persis). **Baru `1.3.0`** | `RESERVED_FLOOR` |
| `normalPatientCreateProtected` | Pendaftaran normal tidak dapat memakai nomor cadangan migrasi — `RESERVED_FLOOR` atau `FAIL_CLOSED`. **Baru `1.3.0`** | `true` |
| `newbornPatientCreateAllocationMode` | Cara alokasi pendaftaran bayi (`POST /newborns`), nilai sama. **Baru `1.3.0`** | `RESERVED_FLOOR` |
| `newbornPatientCreateProtected` | Pendaftaran bayi tidak dapat memakai nomor cadangan migrasi. **Baru `1.3.0`** | `true` |
| `overallGuardReady` | `guardEnabled` dan `guardConfigurationValid`, **kedua** jalur `RESERVED_FLOOR`, dan kedua lantai terpenuhi. Tidak pernah `true` bila jalur bayi tidak terlindungi (`PAT-DEC-072`). *`1.2.2`: tanpa syarat jalur* | `true` |
| `checkedAtUtc` | Waktu pemeriksaan | — |

**Apa yang dibuktikan kedua field jalur.** Pemeriksa tidak membuat pasien dan tidak mengambil kunci,
sehingga ia tidak menjalankan alokasi. Ia melaporkan **keputusan cabang** yang dipakai masing-masing
jalur saat mendaftarkan pasien untuk konfigurasi yang sedang berlaku:
`PatientIdentifierAllocationService.ResolveAllocationMode` untuk pendaftaran normal dan
`NewbornPatientRegistrationService.ResolveIdentifierAllocationMode` untuk pendaftaran bayi. Kedua jalur
yang `RESERVED_FLOOR` memakai satu method alokasi yang sama (`AllocateReservedAsync`), sehingga nomor
berikutnya pada laporan berlaku untuk keduanya. Build yang dideploy tanpa `BE-PAT-MIG-003A` tidak
mengembalikan field ini sama sekali — bukti generasi 2 wajib menunjukkan keduanya `true`.

*Contoh di Staging dengan `Enabled = false`:* `guardState = DISABLED`, kedua mode `FAIL_CLOSED`, kedua
`…Protected = true` (keduanya menolak sebelum alokasi), tetapi `overallGuardReady = false` karena
pendaftaran tidak tersedia.

Di Staging persis, nomor berikutnya dihitung dengan aturan lantai walau guard belum aktif, supaya
pemilik melihat nomor yang akan dipakai begitu guard dinyalakan; `overallGuardReady` tetap `false`.
Tidak ada data pribadi pasien pada keluaran.

**Konfirmasi pemilik (`PAT-DEC-068`).** Endpoint ini dipertahankan dengan syarat: khusus `Staging`
persis, dijaga `Patient : Update`, baca-saja, nol mutasi database, nol mutasi filesystem, tanpa
mencadangkan nomor, tanpa data pribadi, dan hanya mengembalikan metadata guard, kesiapan, dan
alokasi yang dibutuhkan `PAT-GATE-005`.

Gerbang worker `NORMAL_ALLOCATION_GUARD` lulus hanya bila `overallGuardReady = true`. Karena gerbang
ini memakai pemeriksa milik `BE-PAT-MIG-003`, `BE-PAT-MIG-002`
dibangun sesudah `BE-PAT-MIG-003` (grafik roadmap).

*Nama field `1.2.0` — jejak:* `environmentName`, `guardActive`, `configurationValid`, `nextPatientCode`,
`nextMedicalRecordNumber`; pemeriksa hanya service internal, bukan endpoint HTTP. Diganti `1.2.1` atas instruksi
pemilik sebelum build (`PAT-DEC-067`).

**Contoh.** Sebelum `BE-PAT-MIG-003`: pemeriksa tidak tersedia → gerbang gagal → eksekusi `409`.
Sesudah deploy dengan `Enabled = true`: pemeriksa melaporkan `PAT-RSMMC-705250`, `00-89-01-88`, dan `overallGuardReady = true` → gerbang lulus. Sesudah deploy **tanpa** konfigurasi, atau dengan `Enabled = false`: `guardConfigurationValid = false`, `overallGuardReady = false` → gerbang gagal.

Worker sendiri **tidak pernah** membangkitkan `PatientCode` maupun MRN (`PAT-DEC-020`, `021`).
Kunci yang hanya hidup di memori proses tidak diterima sebagai perlindungan (`PAT-DEC-042`).

### B14.3 Aturan environment dan konfigurasi guard — `PAT-DEC-066`, `PAT-DEC-067`

Di `EnvironmentName == "Staging"` (persis, ordinal, membedakan huruf besar-kecil) allocator yang aman
reservasi **wajib** dipakai, dan ia hanya berjalan bila `RsmmcMigrationReservationGuard.Enabled == true`.
Nilai lain di Staging persis — `false`, hilang, salah bentuk, atau tidak terbaca — **gagal tertutup**;
pendaftaran normal tidak pernah jatuh ke allocator lama (`PAT-DEC-067`). Di luar Staging persis,
perilaku allocator normal yang sekarang tidak berubah dan konfigurasi guard tidak dibutuhkan. Lantai
705.250 dan 890.188 adalah konstanta profil reservasi, **bukan** konfigurasi, sehingga tidak dapat
diturunkan lewat konfigurasi (`PAT-GAP-003`). Agent dan source tidak mengubah `appsettings*`; nilai
konfigurasi diatur pemilik saat deployment, misalnya lewat environment variable
`RsmmcMigrationReservationGuard__Enabled=true`.

| Environment | Konfigurasi `RsmmcMigrationReservationGuard` | Perilaku pendaftaran normal | `guardEnabled` |
| --- | --- | --- | --- |
| `Staging` persis | `Enabled = true` | Lantai 705.250 dan 890.188 dengan kunci database | `true` |
| `Staging` persis | `Enabled = false` ditulis eksplisit | **Gagal tertutup**: pendaftaran ditolak `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` (`guardState = DISABLED`); tidak jatuh ke penomoran lama (`PAT-DEC-067`). *`1.2.0`: "penomoran seperti sekarang — pemilik sengaja mematikan guard"* | `false` |
| `Staging` persis | Bagian tidak ada, `Enabled` tidak ada (`CONFIG_MISSING`), atau nilainya bukan boolean / tidak terbaca (`CONFIG_MALFORMED`) | **Gagal tertutup**: pendaftaran ditolak `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID`, log peringatan tanpa data pribadi; tidak jatuh ke penomoran lama (`PAT-GAP-002`) | `false` |
| `Production` | Apa pun | Penomoran seperti sekarang; kampanye reservasi **tidak** mengubahnya | `false` |
| `Development`, `Test`, `staging` (huruf kecil), atau nama lain | Apa pun | Penomoran seperti sekarang. Test integrasi yang sengaja di-*host* sebagai `Staging` dengan `Enabled = true` menguji guard | `false` |

Endpoint `BE-PAT-MIG-002` tetap mustahil di luar `Staging` persis (B7) — terlepas dari konfigurasi
ini.

**Contoh.** Hari pertama sesudah deploy `BE-PAT-MIG-003` ke Staging, pemilik lupa mengisi
konfigurasi. Petugas kiosk mencoba mendaftarkan pasien dan ditolak `503`; tidak ada nomor cadangan
yang terpakai. Sesudah pemilik mengisi `RsmmcMigrationReservationGuard__Enabled=true` dan
me-*restart* layanan, pendaftaran berikutnya mendapat `PAT-RSMMC-705250` dan `00-89-01-88`.
Selama pembekuan `PAT-DEC-055` masih berlaku, kejadian pertama itu memang tidak terjadi.

Nomor yang sudah diberikan saat guard aktif tidak ditarik atau diberi ulang bila konfigurasi guard kemudian
dimatikan (`PAT-GAP-007`).

**Kunci serialisasi (`PAT-DEC-068`).** Saat guard aktif, alokasi diawali
`SELECT pg_advisory_xact_lock(1346458697, 1313821261)` di dalam transaksi `CreatePatient`.

| Kunci | Nilai | Teks ASCII | Arti |
| --- | --- | --- | --- |
| Pertama (namespace) | `0x50415449` = 1.346.458.697 | `"PATI"` | PATIENT_IDENTIFIER_ALLOCATION |
| Kedua | `0x4E4F524D` = 1.313.821.261 | `"NORM"` | NORMAL_PATIENT_CREATE — satu kunci global untuk seluruh pendaftaran pasien normal |

Keduanya konstanta tetap di `RsmmcIdentifierReservationProfile`, sama bagi seluruh instance
aplikasi, tanpa `hashtext()`. Kunci lepas sendiri saat transaksi pendaftaran berakhir. Impact scan
16 Oktober 2026: seluruh kunci lain di repository berbentuk **satu** kunci 64-bit (`hashtext(...)` atau
bigint), yang menurut PostgreSQL tidak berbagi ruang kunci dengan bentuk dua kunci 32-bit; satu-satunya
kunci dua bagian lain yang direncanakan — worker `BE-PAT-MIG-002` — memakai namespace `1173286842`.
*Teks `1.2.1`: `pg_advisory_xact_lock(hashtext('quilvian.patient.identifier-allocation'))`.*

### B14.4 Dampak pada pendaftaran bayi — milik `BE-PAT-MIG-003A` (`1.3.0`)

**Masalahnya, dalam bahasa sederhana.** Pendaftaran bayi dari kasus persalinan membagikan nomornya
sendiri lewat deret nomor platform (`NumberSeries`). Deret itu dimulai dari nomor tertinggi yang sudah
ada, sehingga di Staging bayi pertama akan mendapat `PAT-RSMMC-00742` — nomor yang dipesan pasien lama
RSMMC (`PAT-SRC-031`). Bagian ini memasang perlindungan `BE-PAT-MIG-003` pada jalur itu juga.

### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/newborns` | Mendaftarkan satu bayi dari kasus persalinan obstetri; header `Idempotency-Key` wajib | `Patient : Create` | `CreateNewbornPatientRequest` | `ApiResponse<NewbornPatientCreateResponse>` | Sudah ada (upstream `3a0189c2`) — bentuk request/response **tidak** berubah; cara memilih `PatientCode` dan MRN di `Staging` persis berubah; ditambah `503` khusus Staging persis |

| Environment | Konfigurasi `RsmmcMigrationReservationGuard` | Perilaku pendaftaran bayi |
| --- | --- | --- |
| `Staging` persis | `Enabled = true` | `PatientCode` pertama yang belum dipakai ≥ 705.250 dan MRN pertama yang belum dipakai ≥ 890.188, lewat allocator yang **sama** dengan pendaftaran normal (`AllocateReservedAsync`) di bawah kunci yang sama. Deret `NumberSeries` `PATIENT_CODE` / `PATIENT_MEDICAL_RECORD` **tidak** dibaca maupun dinaikkan |
| `Staging` persis | `false`, hilang, salah bentuk, atau tidak terbaca | **Gagal tertutup** sebelum basis data dibaca, dikunci, atau ditulis: `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` dengan `guardState`, log peringatan `Patient.CreateNewbornPatient.ReservationGuard` tanpa data pribadi. Tidak pernah jatuh ke `NumberSeries` |
| `Production`, `Development`, `Test`, `staging`, nama lain | Apa pun — tidak dibaca | Perilaku sebelum task **tidak berubah**: `NumberSeries` `PATIENT_CODE` (awalan `PAT-RSMMC`, 5 digit) dan `PATIENT_MEDICAL_RECORD` (8 digit, format `00-00-00-00`), batas bawah dari nomor tertinggi bila deret belum ada; tanpa kunci `(PATI, NORM)` (`PAT-DEC-070` b) |

Guard dinilai **satu kali** di awal pendaftaran, sesudah validasi isian, lalu keputusan yang sama
dipakai sampai alokasi — tidak ada celah konfigurasi berubah di tengah pendaftaran. Bentuk respons
sukses, `400`, `404`, `409`, dan `422` tidak berubah. Pemutaran ulang `Idempotency-Key` yang sama di
Staging yang gagal tertutup juga mendapat `503`, karena penolakan terjadi sebelum basis data dibaca.

**Urutan kunci di dalam satu pendaftaran bayi (`Staging` persis, guard aktif).** Seluruhnya kunci
transaksi (`xact`) pada transaksi pendaftaran bayi dan lepas sendiri saat *commit* atau *rollback*.

| No | Kunci | Bentuk | Keterangan |
| ---: | --- | --- | --- |
| 1 | `pg_advisory_xact_lock(hashtext('PAT_NEWBORN_KEY_<hash Idempotency-Key>'))` | Satu kunci 64-bit | Sudah ada; menyerialkan permintaan dengan kunci idempotency yang sama |
| 2 | `pg_advisory_xact_lock(hashtext('PAT_NEWBORN_CASE_<DeliveryCaseId>'))` | Satu kunci 64-bit | Sudah ada; menyerialkan urutan kelahiran per kasus |
| 3 | `pg_advisory_xact_lock(1346458697, 1313821261)` | Dua kunci Int32 | **Baru**; kunci allocator yang sama dengan pendaftaran normal (`PAT-DEC-071`), diambil sesudah validasi ibu, kasus, dan NIK |
| — | Baris `PatNewbornBirthSequence` dan indeks unik `MstPatient` | Kunci baris/indeks | Terjadi saat `SaveChanges`, sesudah kunci 3 |

Pendaftaran normal hanya mengambil kunci 3 dan tidak pernah mengambil kunci 1 atau 2; tidak ada jalur
lain yang mengambil kunci 1 atau 2. Karena itu urutan kunci tidak pernah terbalik dan tidak ada siklus
*deadlock*. Di luar Staging persis, kunci 3 tidak diambil; allocator `NumberSeries` memakai koneksinya
sendiri dengan kunci `hashtext('NUM_SERIES_<deret>_GLOBAL')` yang lepas sebelum kembali ke transaksi bayi.

**Contoh.** Di Staging dengan guard aktif, pasien normal terakhir `PAT-RSMMC-705250` / `00-89-01-88`.
Bidan mendaftarkan bayi "By. Ny. Sari" bersamaan dengan petugas kiosk yang mendaftarkan pasien dewasa.
Yang lebih dulu mendapat kunci memperoleh `PAT-RSMMC-705251` / `00-89-01-89`; yang lain menunggu,
lalu memperoleh `PAT-RSMMC-705252` / `00-89-01-90`. Tidak ada yang pernah mendapat `PAT-RSMMC-00742`.

---

## B15. Bukti PostgreSQL provider-real — `PAT-DEC-065`

Bukti SQLite saja **tidak** cukup untuk kedua task. Test provider-real berjalan terhadap server
**PostgreSQL 15 sekali pakai**: terisolasi, bukan Staging bersama, bukan Produksi, memakai provider
Npgsql dan konfigurasi EF aplikasi yang sebenarnya, lalu dihancurkan sesudah test.

| Hal | Ketentuan |
| --- | --- |
| Server | Disediakan pemilik atau developer saat build; versi mayor 15, sama dengan Staging 15.15 (`PAT-FACT-085`) |
| Konfigurasi server bukti | `max_locks_per_transaction = 2048`. Model `ApplicationDbContext` kini cukup besar sehingga `EnsureCreatedAsync` gagal pada nilai bawaan PostgreSQL 15 (`64`) dengan SQLSTATE `53200` *out of shared memory* dan petunjuk server *increase max_locks_per_transaction*. Server bukti yang lulus `23`/`23` pada 19 Oktober 2026 memakai `2048`. **Hanya syarat server uji sekali pakai** — **bukan** syarat maupun anjuran mengubah PostgreSQL DEV, STAGING, atau PRODUCTION, dan tidak menyentuh `appsettings*` maupun infrastruktur deployment (temuan review `F-14`) |
| Eksekusi suite | Pada server bukti sekali pakai saat ini, seluruh suite `Kategori=ProviderReal` dijalankan **serial**: `dotnet test … --filter "Kategori=ProviderReal" -- xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=1`. Run paralel 20 Oktober 2026 menghasilkan `21`/`23`: dua kasus gagal saat `PostgresSekaliPakai.BuatAsync` menyiapkan basis data (soket transport terputus; server tidak crash, `OOMKilled = false`, `RestartCount = 0`), lulus saat diulang tersendiri; run serial lulus `23`/`23`. Temuan operasional harness bukti — **bukan** syarat PostgreSQL DEV, STAGING, maupun PRODUCTION |
| Koneksi | Connection string dari environment variable; tidak ditulis ke `appsettings*` maupun Git |
| Basis data | Dibuat test dengan nama acak, dihapus sesudah test — juga saat test gagal |
| Skema | `MstPatient` dibentuk dari model EF aplikasi, sehingga default `IsMember = true` ikut ada; test memastikan default itu ada sebelum menguji. Tabel `migration.*` dibuat test sesuai `PAT-FACT-079`–`083` |
| Server tidak tersedia | Saat bukti diminta, test **gagal**, tidak dilewati diam-diam |
| Dependency | Tanpa paket baru seperti Testcontainers; Npgsql sudah dipakai aplikasi |
| Staging dan Produksi | Tidak pernah disentuh oleh test |

| Task | Yang wajib dibuktikan |
| --- | --- |
| `BE-PAT-MIG-003` (`PAT-MIG3-AC-13`) | Lantai `PatientCode` dan MRN; kode 742–705.249 dan MRN ≤ 890.187 tidak pernah diberikan; dua pendaftaran bersamaan memperoleh nomor berbeda lewat kunci; mode tanpa guard memberi hasil yang sama dengan algoritma lama; konfigurasi hilang di `Staging` persis → gagal tertutup |
| `BE-PAT-MIG-003A` (`PAT-MIG3A-AC-09`) | Alokasi berlantai pendaftaran bayi; pendaftaran bayi di luar Staging persis sama dengan perilaku `NumberSeries` sebelum task dan tidak memakai kunci `(PATI, NORM)`; gagal tertutup tanpa mutasi dan tanpa kunci; pendaftaran bayi menunggu kunci `(1346458697, 1313821261)` yang dipegang sesi lain dan melepasnya saat *commit*; bayi↔bayi dan normal↔bayi bersamaan tanpa nomor ganda dan tidak di bawah lantai; idempotency dan urutan kelahiran tetap; pemeriksa baca-saja melaporkan kedua jalur |
| `BE-PAT-MIG-002` (`PAT-MIG2-AC-47`) | `IsMember = false` tersimpan fisik `false` lewat INSERT; `IsActive = true` tetap `true`; baca ulang `AsNoTracking` di transaksi yang sama cocok dengan preview; rollback tidak meninggalkan pasien maupun crosswalk; percobaan ulang idempoten tidak membuat target ganda; kegagalan sebelum *commit* tidak meninggalkan pemetaan pasien yang ter-*commit*. *`1.2.2`: "koreksi eksplisit `IsMember` di transaksi yang sama berhasil" — dihapus `PAT-DEC-075`* |

**Contoh.** Developer menyalakan server PostgreSQL 15 lokal, mengisi environment variable connection
string, lalu menjalankan test provider-real. Test membuat basis data `pat_proof_7f3a…`, memasukkan
satu pasien preview dengan `is_member = false`, lalu membaca `SELECT "IsMember" FROM "MstPatient"`
dan mendapat `false`. Basis data dihapus. Laporan task mencatat perintah dan jumlah lulus/gagal apa
adanya.
