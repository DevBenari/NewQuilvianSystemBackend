# Rekam Medis Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RM-BP-001` |
| `revision` | **11 November 2026: revision `6` `approved` (`RM-DEC-108`)** (indikator ringkasan pulang dan titik sentuh HR, `RM-DEC-097`–`107`) — lihat bagian "Revision 6"; **10 November 2026: revision `5` `approved` (`RM-DEC-088`)** (Optik dan Rehab Medik, `RM-DEC-078`–`087`) — lihat bagian "Revision 5"; revision `4` `approved`. Catatan lama: `3` — diperbarui 10 September 2026. **28 Oktober 2026:** keputusan amandemen sambungan seluruh modul disahkan (`RM-DEC-037`–`050`); desainnya belum dimulai, sehingga revision belum naik. Lihat bagian "Amandemen 28 Oktober 2026" |
| `status` | **`approved`** — disahkan Yoga Aji Pratama 26 Agustus 2026 (`RM-DEC-027`); kontrak API menyusul 27 Agustus 2026 (`RM-DEC-028`); amandemen sambungan Laboratorium 10 September 2026 (`RM-DEC-030`–`RM-DEC-036`). **Tidak ada lagi yang menunggu pengesahan** |
| `module` | `rekam-medis` |
| `design_snapshot_at` | `2026-08-24`; amandemen sambungan Laboratorium `2026-09-10` |
| `backend_commit_sha` | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` (snapshot desain); **`0e2eb105a0754085c5dbc5f1f68b1eec42a3773c`** (audit 10 September 2026) |
| `frontend_commit_sha` | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` (snapshot desain); **`0e5d3c36d8f8c48e81cd3aa8f9714b16448ea55c`** (audit 10 September 2026) |
| `owners` | Seluruh peran dipegang **Yoga Aji Pratama**: product/domain, clinical governance, security/privacy (`RM-DEC-027`), serta API authority dan frontend authority (`RM-DEC-028`). **Tidak ada peran yang masih `OPEN`** |
| `approved_by` | **Yoga Aji Pratama** — penanggung jawab modul |
| `approved_at` | **26 Agustus 2026** — kepemilikan API dan frontend menyusul **27 Agustus 2026** |
| `input_revisions` | `00-interview-decisions.md` revision **`10`** — pengesahan revision 4, 28 Oktober 2026 (revision `8` dan `9` closure pass, revision `7` amandemen sambungan seluruh modul — ketiganya hari yang sama; revision `6`, 10 September 2026); `01-existing-capability-map.md` revision **`3`** — impact scan 28 Oktober 2026 |
| `input_hashes` | Decisions **`sha256:d7ccef575e6fae8f4e6f6b99015e4264ec080b8e6ac533175a67cf69d53a6924`** — revision 10 (pengesahan; sebelumnya `1d788ecd…` revision 9), dihitung 28 Oktober 2026 dengan `sha256sum` atas berkas di disk (berakhiran LF). Sebelumnya `6a8d3f49…` (revision 8), `59e46deb…` (revision 7), dan `914e6cc7…` (revision 6). Requirement completeness `RM-RCG-001` revision 2: `sha256:a82ce13ad01ac05a3420b2ff9daf2a227846546d2c1d78df5bda3a6300c4fb79`. Capability map revision 3: `sha256:b840e22b3a780ba180c340c2dcdc44416c14a5809913a8b01cf1175088d61dc7`. Capability map: hash tercatat **tidak lagi cocok** dengan berkasnya; lihat "Catatan penyimpangan hash" |
| `contract_versions` | API **`0.1.1`** — `approved` 27 Agustus 2026, hash terkunci. `0.1.0` disahkan pada `RM-DEC-028`; `0.1.1` hanya menyegarkan kolom status, **bentuk kontrak tidak berubah** sehingga klien `0.1.0` tetap sah. State `0.1.0`; validation `0.1.0`; integration `0.1.0`; permission/audit `0.1.0` — keempatnya masih `draft` |
| `compatibility_impact` | **Aditif pada skema.** Lima tabel baru; **nol perubahan kolom** pada tabel yang sedang dipakai. Tiga perubahan **perilaku** pada endpoint berjalan, dirinci pada bagian "Dampak kompatibilitas" |

---

## Design gate

**Blueprint ini sudah disahkan, dan rilis pertamanya sudah tuntas dikerjakan di backend maupun
frontend.** Pengesahan dilakukan Yoga Aji Pratama selaku penanggung jawab modul pada 26 Agustus
2026, memegang ketiga peran owner sekaligus (`RM-DEC-027`), dilengkapi kepemilikan API dan
frontend pada 27 Agustus 2026 (`RM-DEC-028`). Amandemen sambungan Laboratorium menyusul pada
10 September 2026 (`RM-DEC-030` sampai `RM-DEC-036`).

Keadaan gate per 27 Agustus 2026 — **tujuh dari sembilan terpenuhi**:

| Gate | Keadaan | Yang tertahan bila belum terpenuhi |
|---|---|---|
| Product/domain owner ditunjuk | **Terpenuhi** — Yoga Aji Pratama | — |
| Clinical governance owner ditunjuk | **Terpenuhi** — Yoga Aji Pratama | Tinjauan komite medik atas `RM-DEC-003`, `004`, `020` **belum** dilakukan; lihat `RM-DEC-027` |
| Security/privacy owner ditunjuk | **Terpenuhi** — Yoga Aji Pratama | Tinjauan pihak perlindungan data atas `RM-DEC-017`, `021`, `022`, `024` **belum** dilakukan; lihat `RM-DEC-027` |
| API authority menyetujui kontrak | **Terpenuhi** 27 Agustus 2026 — Yoga Aji Pratama (`RM-DEC-028`). Kontrak `0.1.0` naik menjadi `approved`, hash dikunci | Tidak ada lagi yang tertahan. **Gerbang paralel frontend terbuka** |
| Angka masa simpan jejak akses ditetapkan | **Terpenuhi** 24 Agustus 2026 — **25 tahun** (`RM-DEC-024`) | Tidak ada lagi yang tertahan. Tabel `TrxMedicalRecordAccessLog` dirancang terbagi per tahun berdasarkan `AccessedAt`, 25 bagian pada keadaan penuh. Dasar regulasi wajib dilampirkan owner saat pengesahan |
| SOP rekam medis rumah sakit tersedia | **`OPEN`** | Isi awal `MstMedicalRecordAccessPurpose` |
| Penelusuran alur penggabungan pasien | **Selesai** 24 Agustus 2026. Hasilnya: penggabungan hanya penandaan, tidak memindahkan data klinis. Status `RM-CAP-007` naik menjadi `Conflict`. Perilaku layarnya ditetapkan `409` dan sudah dijaga `BE-16` pada empat pintu masuk | Tidak ada lagi yang tertahan |
| Jumlah pasien bernomor rekam medis ganda di data nyata | **`OPEN`** | Tidak lagi memblokir apa pun. `BE-16` sudah selesai 27 Agustus 2026, sehingga pengamannya terpasang apa pun jawabannya. Angka ini kini hanya menentukan prioritas pembersihan data, bukan prioritas kode |
| Project test backend tersedia | **Terpenuhi** 24 Agustus 2026 — `BE-00` selesai, project `QuilvianSystemBackend.Tests` berjalan | Tidak ada lagi yang tertahan. `RM-CAP-032` tertutup |

Dua gate yang paling menentukan:

**`RM-DEC-017` mengubah perilaku di luar modul ini.** Kewenangan `SuperAdmin` menyangkut seluruh
aplikasi termasuk IGD. Desain sudah membatasi penerapannya hanya di dalam service modul rekam
medis agar dampaknya tidak menyebar sebelum ada persetujuan, tetapi keputusan yang lebih luas
tetap milik security/privacy owner.

**`RM-DEC-014` menyentuh data klinis nyata.** Migration ketiga memberi status awal pada seluruh
CPPT yang sudah tersimpan. Jumlah barisnya belum diketahui karena data produksi tidak diaudit.
Ini satu-satunya bagian desain yang menyentuh data pasien yang sudah ada.

Gate yang belum terpenuhi berarti menahan implementasi bagian yang bergantung padanya. Gate
**tidak pernah** memblokir pelayanan klinis yang sedang berjalan — seluruh perubahan bersifat
menambah, dan dapat dibatalkan tanpa menyentuh data klinis.

---

## Artifact hashes

Dihitung pada 24 Agustus 2026.

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | ~~`2d4c37bc…`~~ → ~~`914e6cc7…`~~ (revision `6`) → ~~`59e46deb…`~~ (7) → ~~`6a8d3f49…`~~ (8) → ~~`1d788ecd…`~~ (9) → **`d7ccef575e6fae8f4e6f6b99015e4264ec080b8e6ac533175a67cf69d53a6924`** — revision `10`, dihitung 28 Oktober 2026 |
| `01-existing-capability-map.md` | `9cacecf803c0d552623a5f1ce5841af7bea7da5fc49aaf1b3142a076dd4416ae` |
| `02-backend-architecture.md` | `32ab3711e9203bedf2838cdadbbeb1ab6400c20d49b1b1497eaed9efaa5243a1` |
| `03-frontend-architecture.md` | `6c5f875a2e2e005919d34816fee3e3ed7341d97a110e92a961afbe3686f42931` — **revisi 2**, dihitung ulang 27 Agustus 2026 |
| `erd/00-context-erd.md` | `292c301a70004a730a1be7b93f99de9088ad5c9c62714afa52a43787202380f7` |
| `erd/keutuhan-dokumen.md` | `09cc9588dec0f464ab4a9e3a46a66d0a2a6df9d9c82f9b66c7acff827c41469d` |
| `erd/jejak-akses.md` | `f78ea5013e8ef516ea8ece969f4f95e9203212cc0a00f787e9d5e90ac1b9ec7f` |
| `erd/data-dictionary.md` | `f90106beda3faa6c2f6c635c92c11475bbf91e0a4c8223d1d7b4a10480e9f062` |
| `contracts/api-contract.md` | `f057ab9531458383f067b3b79308adb0414593a52a82dc84d607cc5fc3c5a2a2` — **`0.1.1`**, dihitung ulang 27 Agustus 2026 |
| `contracts/state-transition-matrix.md` | `ef23b7a6c4c443ec6dad8c97466d73a4a672f5b565b4c89c3e17983cddcf1325` |
| `contracts/validation-matrix.md` | `c47601f7f1e62822f13ded3b532e33f1c9d6f0305fe870dd30a458c2bc4bb4ca` |
| `contracts/integration-contract.md` | `c73d66096e2e1ec1ec7f6949238530d83e72ef136c38cc777a78bc7adff8a037` |
| `contracts/permission-audit-matrix.md` | `119678d4383421de76aac6aa73b7f91432d04df3dc5ad798bcdb2b51a785e4ee` |
| `testing/acceptance-test-matrix.md` | `4f1c51aab14048cd6aab0133f80431c286ec37a400107069d81f777ada16c5dd` |

Hash `00-interview-decisions.md` dan `01-existing-capability-map.md` juga tercatat sebagai
`input_hashes`. Bila salah satunya berubah, seluruh artefak desain menjadi **stale** dan wajib
ditinjau ulang sebelum dipakai.

### Catatan penyimpangan hash — dicatat 10 September 2026

Pemeriksaan `sha256sum` pada 10 September 2026 menemukan bahwa **tiga hash pada tabel di atas
tidak lagi cocok dengan berkasnya**, terlepas dari amandemen hari ini:

| Artifact | Hash tercatat | Hash sebenarnya |
|---|---|---|
| `01-existing-capability-map.md` | `9cacecf803c0…` | `76b8b9b58915…` |
| `03-frontend-architecture.md` | `6c5f875a2e2e…` | `b983ef71cc1d…` |
| `contracts/api-contract.md` | `f057ab953145…` | `a3b9d3272faa…` |

Ketiganya **sengaja tidak ditimpa** pada pembaruan ini. Menuliskan hash baru begitu saja akan
menghapus satu-satunya bukti bahwa penguncian hash sudah melenceng, dan membuat masalahnya
tidak terlihat oleh pembaca berikutnya.

**Penyebabnya sudah ditelusuri lewat riwayat commit, dan bukan soal teknis.** Bukti tanggal di
bawah berdiri sendiri: dua dari tiga berkas terbukti disunting setelah hash-nya dicatat,
apa pun perlakuan akhir barisnya.

Yang sebenarnya terjadi terbaca dari riwayat commit:

| Artifact | Terakhir berubah | Hash dicatat | Kesimpulan |
|---|---|---|---|
| `03-frontend-architecture.md` | 31 Agustus 2026 (`9124900f`) | 27 Agustus 2026 | Berkas berubah **setelah** hash dicatat, dan hash-nya tidak pernah diperbarui |
| `contracts/api-contract.md` | 31 Agustus 2026 (`9124900f`) | 27 Agustus 2026 | Sama seperti di atas |
| `01-existing-capability-map.md` | 27 Agustus 2026 (`b35f28dc`) | 27 Agustus 2026 | Berubah pada hari yang sama; hash dicatat sebelum perubahan terakhir, atau tidak pernah diverifikasi |

Artinya penguncian hash pada blueprint ini **pernah gagal dijalankan sebagaimana mestinya**:
artefak disunting tanpa hash-nya ikut dihitung ulang. Selama kebiasaan itu tidak diperbaiki,
tabel hash di atas **tidak dapat dipakai sebagai bukti keutuhan** — ia hanya menunjukkan kapan
seseorang terakhir kali ingat memperbaruinya.

Yang **tidak** ikut terpengaruh: hash `00-interview-decisions.md` dihitung ulang hari ini dari
berkas yang baru saja disunting, sehingga baris itu sahih pada saat ditulis.

**Cara menghitung ulang hash supaya hasilnya dapat dibandingkan.** Repository ini memakai
`core.autocrlf=true` pada mesin Windows, sehingga berkas di disk berakhiran CRLF sedangkan
`.gitattributes` menormalkan isinya menjadi LF. Dua orang yang menghitung hash berkas yang sama
karena itu bisa memperoleh angka berbeda tanpa ada yang keliru. Seluruh hash pada dokumen ini
dihitung dengan `sha256sum` terhadap **berkas apa adanya di disk**. Siapa pun yang
memverifikasinya wajib memakai cara yang sama, atau menyebutkan cara lain yang dipakainya.

`03-frontend-architecture.md` naik ke **revisi 2** pada 27 Agustus 2026: brief UI ditambahkan —
entri menu (bagian 10) dan skema tampilan per menu (bagian 11) — dan status kepemilikannya
disesuaikan dengan `RM-DEC-027` serta `RM-DEC-028`. Hash-nya dihitung ulang di atas, dan
`roadmap/frontend-roadmap.md` sudah disesuaikan mengikutinya (`roadmap_revision: 2`).

Perubahan ini **tidak** membuat artefak lain stale. Ia tidak menyentuh input hash mana pun,
tidak mengubah kontrak API, dan tidak mengubah satu pun keputusan pada `00-interview-decisions.md`.

---

## Struktur keluaran

Kedua belas file wajib tersedia. Modul ini memiliki empat belas karena ERD dipecah menjadi dua
konteks agar setiap diagram muat dibaca dalam satu layar.

```text
docs/module-blueprints/rekam-medis/
├── blueprint-manifest.md
├── 00-interview-decisions.md              # keluaran /grill-me
├── 01-existing-capability-map.md          # keluaran /trace-existing-capabilities
├── 02-backend-architecture.md
├── 03-frontend-architecture.md
├── erd/
│   ├── 00-context-erd.md
│   ├── keutuhan-dokumen.md
│   ├── jejak-akses.md
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

---

## Ringkasan desain

### Gagasan yang menentukan bentuk seluruh desain

Status keutuhan dokumen **tidak** ditempelkan ke tiga belas tabel klinis, melainkan disimpan
pada satu tabel daftar tersendiri, `TrxClinicalDocumentIntegrity`.

Akibatnya, dan inilah alasan pilihan itu diambil:

| Sifat | Nilainya |
|---|---|
| Perubahan kolom pada tabel berjalan | **Nol** |
| Tempat aturan penguncian ditulis | **Satu**, bukan tiga belas |
| Cara pengisian data lama | Menyisipkan baris baru, bukan memutakhirkan tabel klinis |
| Cara membatalkan bila gagal | Menghapus tabel baru. Tidak pernah menyentuh data klinis |

Harga yang dibayar: penegakan aturan bergantung pada service benar-benar dipanggil. Ditutup
dengan membatasi cakupan rilis pertama pada **satu jenis dokumen saja**, yaitu CPPT.

### Tabel baru

| Tabel | Kegunaan |
|---|---|
| `TrxClinicalDocumentIntegrity` | Status keutuhan per dokumen |
| `TrxClinicalNoteAddendum` | Koreksi yang tidak menimpa isi lama |
| `TrxClinicalNoteAuthorDelegation` | Penetapan penulis berhalangan |
| `TrxMedicalRecordAccessLog` | Jejak siapa membuka rekam medis siapa |
| `MstMedicalRecordAccessPurpose` | Daftar keperluan akses |

### Endpoint

Status per 27 Agustus 2026, dihitung dari atribut rute pada source — bukan dari kolom status
api-contract, yang sebagiannya sudah usang.

| Grup | Direncanakan | Hidup | Status |
|---|---:|---:|---|
| Medical Record | 5 | 5 | **Tersedia** |
| Clinical Document Integrity | 4 | 4 | **Tersedia** |
| Clinical Note Addendum | 3 | 4 | **Tersedia.** Bertambah satu: `/authority`, dipakai frontend untuk memutuskan menggambar tombol addendum atau tidak |
| Clinical Note Author Delegation | 3 | 3 | **Tersedia.** Belum ada layarnya — lihat `03-frontend-architecture.md` bagian 10.6 |
| Medical Record Access Log | 4 | 4 | **Tersedia** |
| Medical Record Access Purpose | 6 | 6 | **Tersedia** sejak `BE-20`, 31 Agustus 2026. Baris ini sebelumnya berbunyi "Belum ada; controller-nya tidak pernah dibuat" — dikoreksi 10 September 2026 terhadap `Areas/HealthServices/MasterData/Controllers/MedicalRecordAccessPurposeController.cs` |
| **Total** | **25** | **26** | **Seluruh grup tersedia.** Angka hidup melampaui rencana karena addendum bertambah satu endpoint `/authority` |

Di luar kontrak terdapat `MedicalRecordBackfillController` dengan 2 endpoint, alat penelaahan
data lama milik `BE-08`. Ia tidak masuk hitungan karena bukan endpoint pelayanan.

### Uji penerimaan

43 uji, mencakup 14 jalur gagal. Lima hal dinyatakan **tidak dapat diuji otomatis** dan
tercantum terbuka pada acceptance test matrix bagian 4.

---

## Dampak kompatibilitas

### Aditif — tidak berisiko

| Perubahan | Keterangan |
|---|---|
| Lima tabel baru | Belum ada yang memakainya |
| 25 endpoint baru | Tidak menyentuh endpoint yang ada |
| Izin baru | Terdaftar otomatis lewat `AccessMenuSeeder`; peran yang belum diberi izin tidak melihat perubahan apa pun |

### Mengubah perilaku — perlu perhatian

| Perubahan | Dampak | Yang harus dilakukan |
|---|---|---|
| `PUT` CPPT menolak dokumen terkunci | Klien yang mengubah catatan terkunci kini menerima `400`. Sebelumnya berhasil | Disebut pada catatan rilis |
| `ProviderUserId` diabaikan pada permintaan ubah CPPT | Klien tidak menerima galat, tetapi nilainya tidak berpengaruh | **Wajib** disebut pada catatan rilis dan Swagger |
| `IsReadOnlyGenerated` diabaikan pada permintaan ubah CPPT | Sama seperti di atas | Sama seperti di atas |
| `PATCH` status kunjungan mengunci dokumen terbuka | Penutupan kunjungan kini dapat gagal bila penguncian gagal | Disebut pada catatan rilis |

Pilihan mengabaikan alih-alih menolak dua nilai tersebut diambil supaya frontend yang sedang
berjalan tidak putus. Namun mengabaikan kiriman klien tanpa pemberitahuan bukan praktik yang
baik, sehingga pemberitahuannya wajib.

---

## Impact scan trigger

Blueprint ini menjadi **stale** bila salah satu berkas berikut berubah:

| Berkas | Bagian desain yang terdampak |
|---|---|
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Tiga perubahan perilaku; pendaftaran keutuhan |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Pemicu penguncian saat kunjungan ditutup |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs` | Pemetaan `AuthorUserId` dan `PrivateNote` |
| `Areas/HealthServices/RegistrationManagement/Enums/EncounterStatus.cs` | Penentuan kunjungan aktif |
| `Services/Security/AccessPermissionService.cs` | Perlakuan `SuperAdmin` |
| `Services/Logging/LoggerService.cs` | Pemisahan log dan jejak akses |
| `Models/IdentityModel.cs` | Kolom audit warisan |
| `Repositories/ApplicationDbContext.cs` | Pendaftaran configuration |
| `Seeders/AccessMenuSeeder.cs` | Pendaftaran izin otomatis |
| `src/lib/hooks/health-services/clinical-management/use-doctor-cppt.js` | Pola pemanggilan riwayat |
| `src/utils/menu-sidebar/menu-items.jsx` | Penambahan menu |

Ditambahkan 10 September 2026 untuk amandemen sambungan Laboratorium:

| Berkas | Bagian desain yang terdampak |
|---|---|
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | `RM-FACT-009`; prasyarat `RM-DEC-036` |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | `RM-FACT-010`; cara pemanggilan `RM-DEC-031` |
| `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs` | Sama seperti di atas |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Endpoint berlingkup pasien yang dituntut `RM-DEC-031` |
| `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Penambahan sumber; `RM-FACT-012` |
| `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | Penambahan jenis dokumen. **Perhatikan:** daftar jenis diturunkan langsung dari enum ini, sehingga menambah nilai tanpa melengkapi cabang pengambilannya akan membuat jenis itu ikut diminta pada **setiap** berkas pasien |
| `Areas/HealthServices/ClinicalManagement/Models/` | Jumlah berkas di sini menentukan `RM-FACT-012`; bertambahnya tabel melebarkan jarak tanpa pemberitahuan |
| `docs/module-blueprints/radiologi/contracts/integration-contract.md` | `RAD-DEC-006`, pasangan `RM-DEC-030`. **Sedang berubah dan belum masuk Git** per 10 September 2026 |

Bila commit SHA kedua repository berubah sebelum implementasi dimulai, jalankan
`/trace-existing-capabilities` mode impact scan pada berkas di atas sebelum blueprint dipakai.

---

## Amandemen 28 Oktober 2026 — sambungan seluruh modul dan kelengkapan berkas

**Keadaan: keputusan disahkan, desain belum dimulai.** Revision blueprint **tetap `3`**
karena arsitektur target belum disentuh; yang berubah baru masukannya.

| Butir | Keadaan |
|---|---|
| Keputusan | `00-interview-decisions.md` revision `7`: `RM-DEC-037` sampai `RM-DEC-050`, seluruhnya `approved` oleh Yoga Aji Pratama 28 Oktober 2026 |
| Isi singkat | Aturan sambungan Laboratorium (`RM-DEC-030`–`035`) berlaku untuk seluruh modul: baca langsung tanpa salinan, satu izin `MedicalRecord : Read`, satu baris jejak akses, hanya dokumen final, ralat milik modul pemilik, gangguan per bagian. Ditambah **kelengkapan berkas** (cakupan 4): tiga syarat, master dokumen wajib, dan daftar kerja yang tidak menahan pelayanan |
| Modul yang disambungkan | Laboratorium, Radiologi; IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah; Farmasi dan `ClinicalManagement` baru; MCU (`RM-DEC-039`) |
| Snapshot source amandemen | Backend **`363de34f`** (branch `yoga`), frontend **`f0a3b07b8`** (branch `YogaV2`) |
| Capability map | **Revision 3 — impact scan 28 Oktober 2026** pada snapshot di atas, `sha256:b840e22b3a780ba180c340c2dcdc44416c14a5809913a8b01cf1175088d61dc7`. `RM-CAP-034` sampai `RM-CAP-070`: 3 conflict, 7 unknown, 8 closure question. Blocker Lab 5 diusulkan tertutup; 6 dan 7 tetap terbuka |

### Artefak yang menjadi `STALE` terhadap amandemen ini

Input hash decisions berubah, sehingga menurut aturan bagian "Artifact hashes" seluruh artefak
desain wajib ditinjau ulang sebelum dipakai **untuk cakupan amandemen**. Bagian rilis pertama
yang sudah dibangun tetap sah dan tidak dibongkar.

| Artefak | Bagian yang terdampak |
|---|---|
| `01-existing-capability-map.md` | Seluruh modul pada `RM-FACT-013` belum dipetakan |
| `02-backend-architecture.md`, `erd/*` | Jalur baca per modul, master dokumen wajib, penilaian kelengkapan |
| `03-frontend-architecture.md` | Linimasa jenis baru, pesan gangguan per bagian, menu daftar kerja (`RM-FE-020`–`024`) |
| `contracts/api-contract.md` | Endpoint kelengkapan dan master dokumen wajib. Kontrak `0.1.1` tetap sah untuk 26 endpoint yang ada |
| `contracts/integration-contract.md` | Satu bagian per modul tersambung |
| `contracts/state-transition-matrix.md`, `validation-matrix.md`, `permission-audit-matrix.md` | Keadaan kelengkapan, addendum pengesahan, izin daftar kerja |
| `testing/acceptance-test-matrix.md` | Acceptance criteria 25 sampai 39 belum dipetakan |
| `roadmap/*` | Belum ada task untuk amandemen |

### Impact scan trigger tambahan

Area backend yang kini menjadi sumber berkas: `BloodBankManagement`, `EmergencyInstallationManagement`,
`HemodialysisManagement`, `InPatientManagement`, `LaboratoryManagement`, `MedicalCheckupManagement`,
`NutritionManagement`, `OperatingRoomManagement`, `PharmacyManagement`, `RadiologyManagement`,
`RegistrationManagement`, dan `ClinicalManagement`. Kontrak integrasi blueprint modul-modul itu
ikut menjadi pemicu.

### Urutan berikutnya

1. ~~`/trace-existing-capabilities` mode impact scan atas dua belas area di atas~~ —
   **selesai 28 Oktober 2026** (capability map revision 3). Menghasilkan delapan closure
   question; ~~`/grill-me` closure pass~~ **selesai 28 Oktober 2026** (decision log revision 8,
   `RM-DEC-051`–`059`). **Nol blocker desain tersisa.** Hasil Patologi Anatomi ditahan
   (`RM-DEC-058`); tiga aturan final sementara menunggu konfirmasi pemilik IGD, Gizi, dan
   Kamar Operasi tanpa menahan desain (`RM-DEC-059`).
2. ~~`/requirement-completeness-gate` untuk cakupan amandemen~~ — **selesai 28 Oktober 2026**:
   `02-requirement-completeness-assessment.md` (`RM-RCG-001` revision 1,
   `sha256:3c75dd76492ebd4540a4702d1f6871a3b068831c23dc03590379088b1d8c745e`). Kesiapan
   `PARTIALLY_READY`: delapan slice siap; S-B1 sebagian; S-B2 dan S-A1b menunggu keputusan.
   Pemblokir baru `DEC-RM-001` (dokumen wajib kunjungan penunjang langsung) — **ditutup hari
   yang sama** oleh `RM-DEC-060`/`061` (decision log revision 9). `RM-RCG-001` revision 2:
   **sepuluh dari sebelas slice `READY_FOR_DOMAIN_DESIGN`**; hanya S-A1b (hasil PA) berhenti.
2c. ~~`/plan-module-delivery`~~ — **selesai 28 Oktober 2026**: roadmap revision 2 pada ketiga berkas
   `roadmap/`. `BE-21`–`BE-41`, `FE-10`–`FE-15`. Siap dimulai: `BE-21`, `BE-25`, `BE-37`, `BE-38`, dan
   `FE-10`–`FE-14` paralel. Sembilan task tertahan persetujuan pemilik modul. Radiologi disetujui
   `RM-DEC-064`.
2b. ~~`/design-business-module`~~ — **selesai 28 Oktober 2026**, **disahkan hari yang sama**
   (`RM-DEC-062`). Lihat bagian "Revision 4" di bawah.
2a. ~~`/hospital-domain-architect` untuk sepuluh slice siap~~ — **selesai 28 Oktober 2026**:
   `03-domain-architecture.md` (`RM-DA-001` revision 1,
   `sha256:4497a3a138194883b83ac182d47f4ea1940c97602368715fcedb86b2d5e9c0e2`),
   `DOMAIN_ARCHITECTURE_PARTIAL` — sepuluh slice siap, S-A1b berhenti. Gap baru
   `GAP-ARCH-RM-01`: disposisi IGD ke rawat inap menutup kunjungan tanpa mengunci catatan IGD
   (menahan implementasi, bukan desain; milik blueprint IGD).
3. `/design-business-module` — revision blueprint naik ke `4`.
4. `/plan-module-delivery` mengikuti urutan `RM-DEC-048`.

Dua utang modul ini sendiri boleh direncanakan lebih awal tanpa menunggu langkah 1: jenis
`HemodialysisSession`/`LaboratoryResult` yang kosong diam-diam di linimasa (`RM-FACT-014`) dan
penanda `Susulan` untuk 13 jenis lama (`RM-DEC-043`).

### Revision 4 — approved (28 Oktober 2026)

| Field | Value |
|---|---|
| `revision` | **`4`** — **`approved`** |
| `status` | **`approved`** — Yoga Aji Pratama, 28 Oktober 2026 (`RM-DEC-062`). Persetujuan pemilik Laboratorium dan MCU atas pembaca di modulnya: `RM-DEC-063` |
| Scope | Sepuluh slice `RM-DA-001`; S-A1b (PA) dikecualikan |
| Snapshot | Backend `363de34f`; frontend `f0a3b07b8` — diverifikasi sama saat desain |
| `contract_versions` | API **`0.2.0` approved** (aditif terhadap `0.1.1`); state, validation, integration, permission/audit **`0.2.0` approved** |
| `compatibility_impact` | **Aditif.** Empat tabel baru, satu kolom baru ber-bawaan (`MrcClinicalNoteAddendum.AddendumPurpose`), nol perubahan pada tabel modul lain. Satu perubahan perilaku: jenis 14/15 tidak lagi kosong diam-diam |
| Struktur | `04-prd-to-mvp.md` **baru**; `erd/kelengkapan-berkas.md` **baru**; `03-domain-architecture.md` (`RM-DA-001`) dan `02-requirement-completeness-assessment.md` (`RM-RCG-001`) baru sejak revision 3. Artefak lain menerima bagian "Amandemen revision 4" di akhir berkas, tanpa menimpa isi rilis pertama |

| Artifact | SHA-256 revision 4 |
|---|---|
| `02-backend-architecture.md` | `f0025568bbb14b443252b5e87a10a4efa3440e2102dafe8276c9b7abc0ed0b48` |
| `03-frontend-architecture.md` | `df567cee5c850ac4bf1a63ba591ddddc507e46ecc88a75f48839a6ced9e11af2` |
| `03-domain-architecture.md` | `4497a3a138194883b83ac182d47f4ea1940c97602368715fcedb86b2d5e9c0e2` |
| `04-prd-to-mvp.md` | `7adae7a669f7afba0962361c6c84288ed012e48922c6d68be3febccc1d1dbf7b` |
| `erd/00-context-erd.md` | `bea9146918073d93b0cd3ca67b61ae56f2d872f7717dc24782230955542fb686` |
| `erd/kelengkapan-berkas.md` | `bcc5fd43647fce38c065a6ef1e9c9c3f39452863d5bc39e24b85c6b9ea369733` |
| `erd/data-dictionary.md` | `c50ff66e4882a38e7e3978ef98b7563a68b7cdcf3edb481bed15b100a5258ed9` |
| `contracts/api-contract.md` | `479cab0dbd1ceb224a705fd574937d1a793774196a51a8f5b5288a551697c28f` |
| `contracts/state-transition-matrix.md` | `161d8a76d9f9563f07b3dc0dd619187235103bcd282590dbd24618606245617f` |
| `contracts/validation-matrix.md` | `b41d423fe09b8b943565a60f0f30718c27e2af266f0c1a057682282df3600063` |
| `contracts/integration-contract.md` | `395ca8ba3eef116501cefcb811a18739b29bd30644a25007588e88e9f9920711` |
| `contracts/permission-audit-matrix.md` | `dcb18e3d01346583c98bb2772939b06acdda3d2ef8b0a18143b0a21bcc46f72d` |
| `testing/acceptance-test-matrix.md` | `ce4a991ddaa2df6ad7b0b462ef9f41bb14340534c42545200f7ed80c4db5ab79` |

Dihitung ulang sesudah penanda status `approved` dipasang, `sha256sum` atas berkas di disk (LF). Mengubah satu artefak di atas tanpa menghitung ulang
barisnya mengulang penyimpangan yang dicatat pada "Catatan penyimpangan hash".

**Pengesahan revision 4 dan kontrak `0.2.0` sudah diberikan** (`RM-DEC-062`). Persetujuan
pemilik IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah, Farmasi, dan Radiologi untuk
berkas pembaca di modul mereka masih menahan **gelombang** yang memuat modul itu saja; Laboratorium dan MCU sudah disetujui (`RM-DEC-063`).
**Tidak berlaku lagi sejak 29 Oktober 2026** — seluruh persetujuan turun (`RM-DEC-064`, `067`–`075`); lihat "Keadaan delivery amandemen" di bawah.

**Catatan untuk blueprint MCU.** `RM-DEC-040` menutup `MCU-GAP-04` dari sisi rekam medis. Berkas
`mcu/02-requirement-completeness-assessment.md` dan `mcu/04-prd-to-mvp.md` masih menyebutnya
ditunda; penyelarasannya milik blueprint MCU dan **tidak** disunting pada amandemen ini.

### Revision 5 — `approved` (10 November 2026) — Optik dan Rehab Medik

| Field | Value |
|---|---|
| `revision` | **`5`** — **`approved`**, Yoga Aji Pratama, 10 November 2026 (`RM-DEC-088`). Revision 4 tetap `approved` dan berlaku untuk cakupannya |
| Scope | `RM-DEC-078`–`087`: sambungan Optik (2 jenis) dan Rehab Medik (6 jenis) dicatat; profil `RJ-OPT` tanpa butir; kunci jenis poli dan profil `RJ-REHAB`; probe Rehab; addendum koreksi Optik ditolak |
| Snapshot | Backend **`c7686f07`** (branch `yoga`); frontend **`3397211dd`** (branch `YogaV2`) |
| `input_revisions` | Decision log revision **`15`**; capability map revision **`4`**; `RM-DA-001` revision 1 dan `RM-RCG-001` revision 2 **tanpa pass baru** — amandemen hanya memperluas slice S-A3, S-A6, S-B1, S-B2, S-B3 yang sudah siap (alasan di `02-backend-architecture.md` "Gerbang masuk" revision 5) |
| `input_hashes` | Decisions `sha256:90662bd2fa02e66ffe1fbcd8878515a9f74173344bf2879877da9a02e0e5dbab`; capability map `sha256:73a74962185a991e87a5dadf1984e505d74f3347f612d666a084887035dbf995` |
| `contract_versions` | API, integration, state, validation, permission/audit **`0.3.0` `approved`** (`RM-DEC-088`). `0.2.0` tetap `approved`; `0.3.0` sekaligus melebur penyegaran status `0.2.1` yang tertunda |
| `compatibility_impact` | **Aditif pada skema:** satu kolom boleh kosong (`MstMedicalRecordCompletenessProfile.ClinicType`), index unik kunci diganti empat kolom, satu CHECK. **Satu perubahan perilaku:** addendum koreksi pada dokumen Optik ditolak `400` (`VAL-RM-B01`). Nol perubahan tabel modul lain |
| Usulan desain yang ikut disahkan | `DS-R5-04` — penanda "pernah diralat" dokumen Rehab hanya menghitung addendum koreksi, bukan pengesahan. **Disahkan** `RM-DEC-088` |
| Decision log sesudah pengesahan | `sha256:83ea0cc895849f21786a517e0e56172b02f377782f806caf248d2411dbf25c58` (revision 16). `input_hashes` di atas tetap menunjuk masukan desain |

Setiap artefak menerima bagian "Amandemen revision 5" di akhir berkas tanpa menimpa isi lama.
Hash dihitung ulang `sha256sum` atas berkas di disk (LF) **sesudah penanda `approved` dipasang**, 10 November 2026:

| Artifact | SHA-256 revision 5 |
|---|---|
| `02-backend-architecture.md` | `a223c93c173b60e94fb4c78f47d94fc7f04f1c4fd5becc96a9b2712293406b96` |
| `03-frontend-architecture.md` | `758406852f1b8f184e8f469417bcf1da1307b89043fcc62ab46ddbd36c1fbc5a` |
| `04-prd-to-mvp.md` | `85ead610a3b367c16b130334731543c6f9a7c20dfdea927ec2badf4d8ab9e417` |
| `erd/00-context-erd.md` | `f9f3e7765b793180aa39fb5edff1451d323f13e2149c74e101cbeb15fc72bb8a` |
| `erd/kelengkapan-berkas.md` | `42f4b7aa8428c30b2d88dac71124292cff605128e629e1363844daf4d9628db8` |
| `erd/data-dictionary.md` | `314e883ebdb5dc380e9cf60df83ac84e4cc1077a715d77022beaa8ac651e9408` |
| `contracts/api-contract.md` | `56dc74c9fcc587f5731cf84635b93d84a9b496ff8a5c68022df4b270a123bd34` |
| `contracts/integration-contract.md` | `5c133720311e2d807523f31c4addcc0838379b2ba0846a1cf63d690fdaa3b2fc` |
| `contracts/state-transition-matrix.md` | `2823e6bdbdc1f4add2cbdd4fa90f68fbc28ccf4054194ed65b85b04081863b5a` |
| `contracts/validation-matrix.md` | `f2dd1f9e89fd0200f59ee00602630fe025af02e7219c8b998b42515ebf9edf8a` |
| `contracts/permission-audit-matrix.md` | `c19290420a33b0fe0f46808ddc7b988d2fbc31fdc4943788039de130e5e80611` |
| `testing/acceptance-test-matrix.md` | `11b93d795db0f2061c9fb892d14f54d7c3980c34a7b3db9b4f0007b2b75d33c4` |

`erd/keutuhan-dokumen.md`, `erd/jejak-akses.md`, dan `03-domain-architecture.md` **tidak disunting**:
tidak ada perubahan pada tabel keutuhan, jejak akses, maupun konsep domain.

**Impact scan trigger tambahan:** `Areas/HealthServices/OpticalManagement/`,
`Areas/HealthServices/RehabilitationManagement/`, `MasterData/Enums/ServiceUnitType.cs`,
`MasterData/Enums/ClinicType.cs`, `MasterData/Models/MstClinic.cs`, serta berkas pada "Impact scan
trigger revision 4" di `01-existing-capability-map.md`.

**Yang menahan delivery:**

| Butir | Pemilik | Menahan |
|---|---|---|
| ~~Pengesahan revision 5 dan kontrak `0.3.0` (termasuk `DS-R5-04`)~~ | Yoga Aji Pratama | **Selesai** 10 November 2026 (`RM-DEC-088`) |
| Persetujuan perubahan di folder Optik (penanda "pernah diralat") | Yoga Aji Pratama selaku pemilik Optik | Bagian Optik `MVP-7` |
| Persetujuan perubahan di folder Rehab (probe, penanda) | Sukma Giri Pratama | `MVP-8` |
| Keberadaan poli rehab di master data (`RM-UN-R4-01`) | Admin data | `UAT-23` saja |

**Roadmap revision 3 (10 November 2026)** — `roadmap/backend-roadmap.md` (`BE-42`–`BE-46`,
`sha256:5a5fb311…`), `roadmap/frontend-roadmap.md` (`FE-16`–`FE-18`, `sha256:dd88fd06…`),
`roadmap/requirement-traceability.md` (T3, `sha256:3d484bcd…`). Siap dimulai: `BE-42`, `BE-43`,
`FE-16`, `FE-17`. Tertahan: `BE-44` (persetujuan pemilik Optik), `BE-45` (persetujuan pemilik Rehab
Medik), `FE-18` (task backend dan data uji).

**Keadaan delivery 10 November 2026:** seluruh backend revision 5 `SELESAI` — `BE-42`–`BE-46`. `MedicalRecordTests` 268 lulus, `OpticalTests` 297 lulus. Migration `20261110043756_AddClinicTypeToMedicalRecordCompletenessProfile` dibuat, **belum diterapkan**. Persetujuan folder: `RM-DEC-089` (Optik), `RM-DEC-090` (Rehab Medik). Catatan rilis `0.3.0` menunggu pengesahan. `FE-16`, `FE-17` `SELESAI`. **Migration `BE-42` diterapkan ke devYoga** 10 November 2026 (instruksi Yoga Aji Pratama; riwayat 387); `RJ-OPT`/`RJ-REHAB` terpasang lewat seeder. `FE-18`: `UAT-22` lulus; `UAT-20`/`21`/`23` menunggu migration Rehab (`AddRehabilitationManagement`, masih tertunda — tanpanya backend hanya dapat dinyalakan dengan `Seeders__ContinueOnFailure=true`) dan data Optik — `testing/uat-amandemen-0.3.0.md`.

**Pembaruan 10 November 2026 sore (decision log revision 18, `RM-DEC-091`–`096`):** migration Rehab diterapkan ke devYoga (`RM-DEC-092`; riwayat 388, **nol tertunda**, backend start normal). Opsi jenis poli `Unknown` dikeluarkan dan ditolak (`RM-DEC-095`; penyegaran kontrak `0.3.1` untuk pass blueprint berikutnya). **`FE-18` `SELESAI`** pola `FE-15` (`RM-DEC-096`): `UAT-22` lulus nyata; `UAT-20`, `21`, `23` terbuka dan terlacak sampai master Optik dan Rehab disiapkan pemiliknya. **Seluruh task roadmap revision 3 kini `SELESAI`.**

### Revision 6 — `approved` (11 November 2026) — indikator ringkasan pulang dan titik sentuh HR

| Field | Value |
|---|---|
| `revision` | **`6`** — **`approved`**, Yoga Aji Pratama, 11 November 2026 (`RM-DEC-108`). Revision 4 dan 5 tetap `approved` untuk cakupannya |
| Scope | `RM-DEC-097`–`107`: rencana kontrol di berkas; indikator ketepatan ringkasan pulang untuk OPPE HR (read model, endpoint, izin, audit); permintaan penolakan tanda tangan ulang ke Rawat Inap; pernyataan non-integrasi gerbang HR, log override, K3RS |
| Snapshot | Backend **`46a57def`** (branch `yoga`); frontend **`75b97ecad`** (branch `YogaV2`) |
| `input_revisions` | Decision log revision **`20`**; capability map revision **`5`**; `RM-DA-001` revision 1 dan `RM-RCG-001` revision 2 **tanpa pass baru**. **Penyimpangan yang wajib disadari pengesah:** indikator adalah read model baru di luar daftar slice `RM-DA-001` (alasan di `02-backend-architecture.md` "Gerbang masuk" revision 6) |
| `input_hashes` | Decisions `sha256:0e8fe5f0dd17861a4d43a71279791c12b33c9726783b042800a5b355bad05f7e`; capability map `sha256:c69ed2f603f8a32e4d5b78190c77b974b5ef67802ffd461c3fb8452fed7e5b26` |
| `contract_versions` | API, integration, state, validation, permission/audit **`0.4.0` `approved`** (`RM-DEC-108`). Melebur penyegaran `0.3.1` (`RM-DEC-095`). `0.3.0` tetap `approved` |
| `compatibility_impact` | **Nol perubahan skema, nol migration.** Aditif di rekam medis: satu grup endpoint, satu izin, satu bagian tampilan. **Satu perubahan perilaku di Rawat Inap** (bila disetujui pemiliknya): tanda tangan ulang ringkasan tanpa sesi koreksi → `409` |
| Usulan desain yang ikut pengesahan | `DS-R6-01`–`07` (`02-backend-architecture.md` A6.13) — **disahkan** `RM-DEC-108`, bersama penerimaan read model tanpa pass gerbang baru |
| Decision log sesudah pengesahan | `sha256:1f9aa1155d02b30c895c74fa86fca6254d406cdaae5c24a3f3cd3c119b9c37f6` (revision 21). `input_hashes` di atas tetap menunjuk masukan desain |

Setiap artefak menerima bagian "Amandemen revision 6" di akhir berkas tanpa menimpa isi lama. Hash
dihitung ulang `sha256sum` atas berkas di disk (LF) **sesudah penanda `approved` dipasang**, 11 November 2026:

| Artifact | SHA-256 revision 6 |
|---|---|
| `02-backend-architecture.md` | `8dbf6eca20363b40b92d9191bc85b08075df1f4547493216aa4482c53d9e7315` |
| `03-frontend-architecture.md` | `f069110dd55075c9bacbd542187cf807d514c60f906b12936db56855525a29c2` |
| `04-prd-to-mvp.md` | `97a77f397050b095e336ae4a397e5d5889f710dfe666204c3f1ac58464cc6833` |
| `erd/00-context-erd.md` | `fdf9c1a2f937a3866dd0ee688aff4247524078ef8a60eebc2f1a6a88ff6aeb60` |
| `erd/kelengkapan-berkas.md` | `04e8da02a673b7a8498846d2e7c13578184ec3968649e381e621c5e173ac6730` |
| `erd/data-dictionary.md` | `8cd984ea3fe6ce79858ae26056dd021f8351c14eaa1624d0a19da92c92e6fc58` |
| `contracts/api-contract.md` | `0a51a38946ae80daf248976492468f06cde4add070cbced1f3d109ebccbfd4d4` |
| `contracts/integration-contract.md` | `068988bd88e8730db94f0946c08c82ff0d3a8b899dcf268b838c30f664b15b11` |
| `contracts/permission-audit-matrix.md` | `540954d743065f5fa22c9120c31e4551af987635d8c21f7ed501fc1a54d012c1` |
| `contracts/validation-matrix.md` | `536ff10406fe6e78abf8337ad7c8d1a3a97613a16a289ffec84a04ba3a3a9ade` |
| `contracts/state-transition-matrix.md` | `e3a4a5b6f216daee6dc189a8d3660f9d37fabe3fd8787d26bffff082d8e16709` |
| `testing/acceptance-test-matrix.md` | `8bdfc390e7ace180f0a4b0bb8cb03f2750494107db2a3407461349401780cd03` |

`erd/keutuhan-dokumen.md`, `erd/jejak-akses.md`, dan `03-domain-architecture.md` **tidak disunting**.

**Impact scan trigger tambahan:** `Areas/HealthServices/InPatientManagement/Services/InpDischargeService*.cs`,
`InpMedicalRecordDocumentSource.cs`, model `InpEpisode`, `InpDischargeSummary`,
`InpDischargeSummaryRevision`, `InpDoctorAssignment`; `Areas/Corporate/HumanResource/CredentialingManagement/`,
`OccupationalHealthManagement/`, `PerformanceManagement/` (terutama munculnya
`IOppeSimrsMetricExtractorService`); `MstDoctorConfiguration.cs`; `Services/Security/AccessPermissionService.cs`.

**Yang menahan delivery:**

| Butir | Pemilik | Menahan |
|---|---|---|
| ~~Pengesahan revision 6 dan kontrak `0.4.0`, termasuk `DS-R6-01`–`07` dan pemakaian read model tanpa pass gerbang baru~~ | Yoga Aji Pratama | **Selesai** 11 November 2026 (`RM-DEC-108`) |

**Roadmap revision 4 (11 November 2026)** — `roadmap/backend-roadmap.md` (`BE-47`–`BE-51`,
`sha256:6eba3396…`), `roadmap/frontend-roadmap.md` (`FE-19`, `sha256:cedd7af7…`),
`roadmap/requirement-traceability.md` (T4, `sha256:4148bfed…`). **Siap dimulai:** `BE-49`, uji unit
`FE-19`, lalu `BE-51` sesudah `BE-49`. ~~**Tertahan persetujuan Muhammad Hamzah:** `BE-47`, `BE-48`,
`BE-50`.~~ **Persetujuan folder Rawat Inap turun 11 November 2026** (`RM-DEC-109`, disampaikan Yoga
Aji Pratama): `BE-47`, `BE-48` `READY`; `BE-50` `READY` sesudah `BE-49`. Roadmap sesudah pembaruan
status: backend `sha256:d700b2d2…`, traceability `sha256:aa2a6ddf…`. Tidak ada migration.

**Keadaan delivery 11 November 2026:** seluruh backend roadmap revision 4 **`SELESAI`** — `BE-47`
(rencana kontrol), `BE-48` (tanda tangan ulang `409`), `BE-49` (endpoint indikator), `BE-50` (pembaca
Rawat Inap), `BE-51` (catatan rilis `0.4.0`, **menunggu pengesahan**). `MedicalRecordTests` **308 lulus**,
0 gagal. Nol migration. Laporan per task di `task/report/backend/`. Roadmap sesudah delivery: backend
`sha256:3af7fead…`, traceability `sha256:e0260fe6…`, frontend `sha256:cedd7af7…` (tidak berubah). Yang
tersisa: `FE-19`/`UAT-24`; UAT `MVP-10` di devYoga (`403` dengan akun tanpa izin, kinerja periode
panjang); ~~pengesahan catatan rilis `0.4.0` dan `0.3.0`~~ (**disahkan** 11 November 2026, `RM-DEC-110`; rilis
`0.2.0` tetap menunggu); penerimaan `INT-RM-20` oleh pemilik HR;
**penyegaran kontrak `0.4.1`** (contoh paging C1 mengikuti `PagedResult`) pada pass blueprint berikutnya.

**Pembaruan 11 November 2026 sore:** `FE-19` **`SELESAI`** (3 uji unit + 2 E2E API tiruan; `UAT-24` nyata
terbuka karena `InpDischargeSummary` devYoga kosong). **UAT `MVP-10`** terhadap backend lokal + devYoga
(`testing/uat-amandemen-0.4.0.md`): `401` tanpa sesi, `200` metadata dan tarikan (pembaca `BE-50` berjalan di
PostgreSQL, bukan `503`), `400` keempat validasi, **`403` akun non-superadmin tanpa izin** (menutup `AT-RM-94`),
audit satu baris per tarikan. Terbuka dan terlacak: angka berisi, kinerja volume nyata, `UAT-24` nyata, dan
`409` tanda tangan ulang lewat HTTP — seluruhnya menunggu data Rawat Inap (devYoga: 0 episode).
| Persetujuan perubahan di folder Rawat Inap (`INT-RM-18`, `10m`, `19`) | Muhammad Hamzah | `MVP-9`, `MVP-10` |
| Penerimaan `INT-RM-20` dan temuan K3RS (`RM-CAP-093`) | Pemilik HR | `POST-MVP` saja |

---

## Langkah berikutnya

### Keadaan delivery amandemen — 2 November 2026

**Seluruh 21 task backend amandemen (`BE-21`–`BE-41`) `SELESAI`.** `dotnet test
Tests/QuilvianSystemBackend.MedicalRecordTests` → 225 lulus. Laporan per task di
`task/report/backend/`.

| Tahap | Keadaan |
|---|---|
| Persetujuan pemilik modul sumber | **Lengkap** 29 Oktober 2026 — `RM-DEC-063`, `064`, `067`–`075`. Kalimat "masih menahan gelombang" pada bagian Revision 4 di atas sudah tidak berlaku |
| Pembaca sumber | Seluruhnya tersambung: Laboratorium, Radiologi, IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah, Farmasi, MCU. Satu-satunya jenis "belum tersedia": hasil Patologi Anatomi (`RM-DEC-058`) |
| Kelengkapan berkas | Penilaian, daftar kerja, master profil, dan pemeriksaan berkala hidup. Asumsi sementara "kondisi tanpa pembaca dianggap tidak ada" sudah tidak berlaku untuk kondisi mana pun |
| Keputusan baru selama delivery | `RM-DEC-076` (kriteria Kamar Operasi per kasus, checklist tiga fase) — decision log Riwayat Pass revisi 12 |
| Skema | Migration `20261028070658_AddMedicalRecordCompletenessFoundation` diterapkan di devYoga 28 Oktober 2026; lingkungan lain belum |
| Catatan rilis | Bagian "Rilis amandemen `0.2.0`" di `catatan-rilis.md` — **menunggu pengesahan** pemilik API |
| Frontend | `FE-10`–`FE-14` `SIAP` dan kini dapat diuji terhadap backend nyata; `FE-15` menunggu |

**Yang tersisa — menunggu tindakan manusia:**

| Sisa | Pemilik |
|---|---|
| Mengesahkan catatan rilis amandemen (`catatan-rilis.md` bagian A9) | Yoga Aji Pratama |
| Menerapkan migration amandemen di luar devYoga | User, dengan instruksi terpisah |
| Menyelaraskan sebelas profil dokumen wajib dengan SOP rumah sakit (`RM-DEC-047`) | Pemilik proses rekam medis |
| Menilai ulang kunjungan beroperasi atau ber-HD yang dulu `Complete` karena asumsi sementara (`catatan-rilis.md` A7 butir 3) | Unit rekam medis |
| Layar `FE-10`–`FE-15` | Frontend |

**Catatan untuk pass blueprint berikutnya (`manage-module-blueprint`).** Butir di bawah ini
**sengaja tidak disunting** selama delivery, karena artefaknya terkunci hash di bagian
"Revision 4 — approved" dan menyuntingnya di luar pass membuat desain *stale* tanpa jejak:

| Butir | Keadaan |
|---|---|
| Field `revision`, `input_revisions`, `input_hashes`, `contract_versions` pada tabel atas | Masih menyebut revision `3` dan kontrak `0.1.1`; bagian Revision 4 di atas yang benar |
| Decision log | Sudah bertambah sesudah revision 10: persetujuan modul (Riwayat Pass 11, `RM-DEC-067`–`075`) dan `RM-DEC-076` (Riwayat Pass 12). Hash `d7ccef57…` tidak lagi cocok |
| `02-backend-architecture.md` | Hash di disk (`f73af022…`) berbeda dari yang tercatat (`f0025568…`); perubahan berasal dari commit `ef73ca75` hari yang sama dengan penguncian. Perlu diperiksa, lalu dihitung ulang |
| Kolom status `contracts/api-contract.md` A1–A4, `04-prd-to-mvp.md`, `03-frontend-architecture.md` | Masih `Rencana (belum tersedia)` padahal seluruh endpoint hidup. Penyegaran status `0.2.1` (bentuk tidak berubah, preseden `0.1.1`) |
| Selisih kontrak yang terkumpul selama delivery | `unavailableKinds` juga pada `/summary` (`BE-21`); `items[].sortOrder` tidak diterima dan bentuk `/criteria/options` (`BE-37`); bentuk daftar kerja dan `404` "belum dinilai" (`BE-39`); calon `isUnderCorrection` MCU (`BE-36`). Seluruhnya dirinci di laporan task dan `catatan-rilis.md` A2.3 |
| `A4.11` Kamar Operasi dan Hemodialisa | Dipertajam `RM-DEC-076` (per kasus, tiga fase) dan tafsir "`Completed` ke atas" = sesi berakhir termasuk `Stopped` (`BE-32`) |

### Keadaan rilis pertama — 10 September 2026

**Diperbarui 10 September 2026.** Rilis pertama modul rekam medis **sudah tuntas dikerjakan** —
21 task backend dan 10 task frontend seluruhnya `SELESAI`. Yang tersisa dari rilis pertama
bukan lagi pekerjaan kode, melainkan tiga butir yang menunggu tindakan manusia.

Pada revisi 2, bagian ini masih menandai frontend belum dimulai dan satu controller belum
dibuat. Koreksinya ada di bawah, ditulis terbuka supaya jelas apa yang keliru dan sejak kapan.

### Yang sudah selesai

| Tahap | Keadaan |
|---|---|
| Desain dan pengesahan | Selesai. Seluruh peran owner terisi; kontrak API `0.1.1` `approved` dengan hash terkunci |
| Perencanaan | Selesai. `roadmap/backend-roadmap.md` (`BE-00`–`BE-18`) dan `roadmap/frontend-roadmap.md` (`FE-00`–`FE-09`) |
| Brief UI | Selesai 27 Agustus 2026. Entri menu dan skema tampilan per layar pada `03-frontend-architecture.md` bagian 10 dan 11 |
| Kode backend | **Seluruh 21 task selesai.** 26 endpoint hidup. `dotnet test` → 132 lulus |
| Kode frontend | **Sepuluh dari sepuluh task `SELESAI`** per 31 Agustus 2026 |

### Koreksi besar — dicatat 10 September 2026

Bagian ini pada revisi 2 menyatakan **"nol dari sepuluh task frontend dikerjakan"** dan
**"controller master keperluan akses tidak pernah dibuat"**. Keduanya **sudah tidak benar**
sejak 31 Agustus 2026, dan manifest ini tertinggal sepuluh hari.

Yang sebenarnya terjadi:

| Yang tertulis pada revisi 2 | Keadaan sebenarnya |
|---|---|
| Nol dari sepuluh task frontend dikerjakan | **Sepuluh dari sepuluh `SELESAI`.** Empat layar, empat entri menu, dan empat uji antarmuka berjalan |
| Tidak ada direktori `medical-record-management` di frontend | Ada di `src/app/health-services/` **dan** `src/components/view/health-services/`, lengkap dengan service, hook, util, dan style |
| Controller master keperluan akses tidak pernah dibuat | Dibangun `BE-20` pada 31 Agustus 2026 di `Areas/HealthServices/MasterData/Controllers/`, enam endpoint, 9 uji lulus |
| `FE-06` tertahan backend | Terbuka oleh `BE-20`, lalu `SELESAI` pada hari yang sama |

Kekeliruan seperti ini berakibat nyata. Manifest adalah dokumen yang dibaca orang untuk
memutuskan pekerjaan berikutnya — pembaca revisi 2 akan menyiapkan sepuluh task frontend yang
sebenarnya sudah selesai, dan menugaskan orang membangun controller yang sudah ada.

### Yang tersisa — seluruhnya bukan pekerjaan kode

| Sisa | Task | Sifatnya |
|---|---|---|
| Isi awal master keperluan akses | `BE-09` bagian kedua | Menunggu SOP rekam medis rumah sakit. Tabelnya ada, layar pengisinya ada, **isinya nol** — dan selama nol, pembukaan berkas pasien di luar rawatan selalu ditolak |
| ~~Penelaahan data lama~~ | `BE-08` | **Selesai di devYoga 2 November 2026** — 10 CPPT terkunci tanpa tanda tangan, nol tersisa. Di produksi: penelaahan ulang dan pemberitahuan unit rekam medis menjadi syarat pemasangan |
| Pemberitahuan ke penulis CPPT | `BE-15` | Bukan pekerjaan kode. Lihat `BE-15-pemberitahuan-penulis-cppt.md` |

### Utang kode yang ditemukan audit 10 September 2026

Ketiganya **tidak** terlihat dari status `SELESAI` pada roadmap, karena semuanya soal yang
tidak pernah masuk daftar task:

| Utang | Bukti | Sifatnya |
|---|---|---|
| ~~`RM-DEC-006` disahkan tetapi nol implementasi~~ — **ditutup** `BE-26`, 29 Oktober 2026 | Penanda `Susulan` tidak ditemukan di backend maupun frontend | Menahan `RM-DEC-035` dan `RM-FE-001`. **Milik modul ini sendiri, tidak menunggu siapa pun** |
| ~~Layar penelusuran hanya membaca 13 dari 20 model `ClinicalManagement`~~ — **ditutup di backend** `BE-22`, 28 Oktober 2026; layar `FE-10`/`FE-11` | Tujuh model tidak terbaca, termasuk rencana asuhan keperawatan dan visite dokter | Jarak ini **melebar sendiri** setiap `ClinicalManagement` bertambah tabel |
| ~~Sambungan ke modul penunjang belum ada~~ — **ditutup** `BE-23`, `BE-24`, 28 Oktober 2026 | Nol jalur Laboratorium → Rekam Medis pada kedua kontrak integrasi | Aturannya sudah diputuskan `RM-DEC-030`–`RM-DEC-036`; implementasinya menunggu entri hasil lab |

### Utang yang tercatat terbuka

| Utang | Tercatat di |
|---|---|
| Penyaringan menu per izin belum ada di frontend; `RM-FE-013` belum ditegakkan | `03-frontend-architecture.md` bagian 10.5 |
| Layar penetapan penulis berhalangan tidak dibuat pada rilis pertama | `03-frontend-architecture.md` bagian 7 dan 10.6 |
| ~~Kolom status api-contract bagian 3, 5, dan 6 usang~~ | **Ditutup** 27 Agustus 2026 lewat kontrak `0.1.1`. Sebelas baris status dikoreksi; bentuk kontrak tidak berubah |
| Tinjauan komite medik dan pihak perlindungan data | `RM-DEC-027` |

Tiga baris pertama tidak memblokir pekerjaan mana pun. Ia dicatat supaya tidak hilang — utang
yang tercatat terbuka dapat dijadwalkan, utang yang tidak tercatat akan ditemukan orang lain
sebagai kejutan.
