# Blueprint Output Contract

## 1. Struktur keluaran

Kedua belas file berikut **MUST** ada. Daftarnya pasti, tidak bebas.

```text
docs/module-blueprints/<module>/
├── blueprint-manifest.md
├── 00-interview-decisions.md
├── 01-existing-capability-map.md
├── 02-backend-architecture.md
├── 03-frontend-architecture.md
├── erd/
│   ├── 00-context-erd.md
│   ├── <bounded-context>.md
│   └── data-dictionary.md
├── contracts/
│   ├── api-contract.md
│   ├── state-transition-matrix.md
│   ├── validation-matrix.md
│   ├── integration-contract.md
│   └── permission-audit-matrix.md
└── testing/
    └── acceptance-test-matrix.md
```

`00-interview-decisions.md` dan `01-existing-capability-map.md` adalah keluaran skill
sebelumnya. Skill ini membacanya, tidak menulisnya.

### File yang tidak relevan bagi modul tertentu

File tetap dibuat, berisi **satu baris alasan**. Ini menutup pertentangan antara kewajiban
struktur dan larangan membuat dokumen kosong.

Bentuk yang benar:

```markdown
# Integration Contract — Modul IGD

Tidak berlaku untuk modul ini karena IGD tidak memanggil sistem luar.
Ditinjau ulang bila kebutuhan integrasi muncul.
```

Bentuk yang salah:

- file dihapus tanpa jejak, sehingga pembaca tidak dapat membedakan "memang tidak perlu" dari
  "terlupa ditulis";
- file berisi judul dan tabel kosong tanpa keterangan apa pun.

Alasan **MUST** menyebut sebabnya, bukan hanya "tidak relevan".

## 2. Manifest minimum

| Field | Requirement |
| --- | --- |
| `blueprint_id` | Stabil lintas revisi |
| `revision` | Naik pada setiap perubahan desain yang material |
| `status` | `draft`, `approved`, `superseded` |
| `owners` | Product/domain, API, security, dan frontend authority |
| `approved_by`, `approved_at` | Bukti approval manusia |
| `backend_commit_sha`, `frontend_commit_sha` | Snapshot discovery dan desain |
| `contract_versions` | Versi kontrak API, integrasi, dan state |
| `artifact_hashes` | Deteksi drift |
| `input_revisions`, `input_hashes` | Asal-usul dokumen hulu |

## 3. Isi wajib per file

### 3.1 `02-backend-architecture.md`

Bagian yang **MUST** ada:

| Bagian | Isi |
| --- | --- |
| Bounded context dan ownership | Batas konteks, aggregate root, invariant, transaction boundary |
| **Tabel kepemilikan data** | Kelompok data, modul pemilik, dipakai modul ini atau tidak, dibuat ulang atau tidak |
| **Class diagram** | Mermaid `classDiagram`, dipecah per konteks; lihat [class-and-erd-template.md](class-and-erd-template.md) |
| **Penjelasan setiap class** | Tabel per class, wajib memuat Status dan Lokasi file |
| **Arsitektur folder** | Pohon folder beserta status setiap file; lihat [backend-structure-rules.md](backend-structure-rules.md) |
| **Status model** | Tabel ringkas Baru / Diperbarui / Sudah ada beserta dampak migration |
| **Rencana migration** | Urutan, dapat dijalankan tanpa mematikan layanan atau tidak, pengisian data lama, langkah mundur |
| **Rencana data master awal** | Isi minimum setiap tabel master agar modul dapat dipakai |
| **Yang sengaja tidak dibuat** | Class yang dipertimbangkan lalu ditolak, beserta alasannya |

Tabel kepemilikan data adalah pertahanan paling langsung terhadap duplikasi entity. Bentuknya:

| Kelompok data | Modul pemilik | Dipakai modul ini | Dibuat ulang di modul ini |
| --- | --- | :---: | --- |
| Pasien | Patient Management | Ya | Tidak |
| Triage | Emergency Installation | Ya | Ya, karena khusus IGD |
| Resep | Pharmacy Management | Ya | Tidak |

Rencana data master awal wajib karena modul dengan tabel master kosong tidak dapat dipakai
sama sekali. Bentuknya:

| Master | Isi minimum | Sumber nilai |
| --- | --- | --- |
| `MstEmergencyTriageLevel` | Lima level ATS/ESI beserta warna dan target waktu respons | SOP triage rumah sakit |

Nilai seperti warna dan target waktu **MUST** berasal dari master, **MUST NOT** di-hardcode di
controller maupun frontend.

Bagian "Yang sengaja tidak dibuat" mencegah orang berikutnya mengusulkan ulang hal yang sama:

| Yang ditolak | Alasan |
| --- | --- |
| `PatientIGD` | Pasien sudah dimiliki Patient Management; dipakai lewat `EncounterId` |
| SOAP versi IGD | Sudah ada di Clinical Management dan dipakai lintas pelayanan |

### 3.2 `03-frontend-architecture.md`

Revisi ini memfokuskan kedalaman pada backend. Frontend cukup memuat kontrak fungsional:
kebutuhan layar, aksi per peran, data dan status yang dikonsumsi, penanganan state, serta
matriks kewenangan UI dan ruang `DEV_DISCRETION`.

### 3.3 `erd/`

Mengikuti [class-and-erd-template.md](class-and-erd-template.md).

| Yang wajib ada | Keterangan |
| --- | --- |
| Kolom di dalam kotak entity | ERD yang hanya memuat nama tabel dan garis relasi **tidak** memenuhi kontrak |
| Penanda `PK`, `FK`, `UK` | Agar kunci terbaca tanpa legenda terpisah |
| Kamus data bertingkat | Tabel `Baru` dan `Diperbarui` seluruh kolom; `Sudah ada` cukup kolom kunci ditambah rujukan file model |
| Skema DDL | Untuk tabel `Baru` dan `Diperbarui`, diambil dari file configuration EF Core |
| Peringatan DDL | Bagian DDL dibuka dengan pernyataan bahwa isinya dokumentasi bentuk, bukan skrip yang dijalankan |

Kolom audit `IdentityModel` tidak digambar pada ERD dan tidak ditulis ulang pada DDL.

### 3.4 `contracts/api-contract.md`

Setiap grup endpoint memakai judul persis nilai `[Tags(...)]` pada controller, diikuti base URL
dan tabel API.

```markdown
### Health Services / Emergency Installation Management / Emergency Triage

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triages`
Contract version: `v2` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/retriage` | Menilai ulang pasien | `EmergencyTriage : Update` | `RetriageRequest` | `ApiResponse<TriageResponse>` | **Rencana (belum tersedia)** |
```

Endpoint yang belum ada di kode **MUST** diberi label `Rencana (belum tersedia)`.
Kode status beserta artinya bagi pengguna ditulis di bawah tabel.

### 3.5 `contracts/permission-audit-matrix.md`

Memuat string `[AccessPermission(...)]` apa adanya, agar implementer menyalin dan tidak
menerjemahkan.

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `POST /{id}/retriage` | `EmergencyTriage` | `Update` | `[AccessPermission("EmergencyTriage", "Update")]` | Ya |
| `GET /` | `EmergencyTriage` | `Read` | `[AccessPermission("EmergencyTriage", "Read")]` | Tidak |

Kolom terakhir mengikuti konvensi project: GET tidak dicatat logger. Payload log **MUST NOT**
memuat kolom yang bertanda sensitif pada kamus data.

### 3.6 `contracts/state-transition-matrix.md`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |

Transisi yang tidak sah **MUST** disebutkan, bukan hanya yang sah.

### 3.7 `contracts/validation-matrix.md`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |

Pesan ditulis dalam Bahasa Indonesia yang dipahami pengguna, bukan istilah teknis.

### 3.8 `testing/acceptance-test-matrix.md`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |

Wajib memuat jalur gagal, bukan hanya jalur berhasil.

## 4. Versioning

Setiap contract menyimpan:

- `contract_version` dan status `draft` / `approved` / `superseded`;
- owner, `approved_by`, `approved_at`;
- `input_revision`, `input_hash`, dan dampak kompatibilitas;
- traceability ke requirement dan decision ID.

Setiap architecture dan contract **MUST** menyebut requirement ID, decision ID, owner,
exception path, dampak security dan privacy, serta acceptance test yang membuktikannya.

Perubahan setelah approval membuat revision atau version baru dan memicu impact scan dua
repository.

Sumber keputusan kontrak ini: `docs/agency/update-skills/03-revisi-design-business-module.md`.
