# Rekam Medis Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RM-BP-001` |
| `revision` | `2` — diperbarui 27 Agustus 2026 |
| `status` | **`approved`** — disahkan Yoga Aji Pratama 26 Agustus 2026 (`RM-DEC-027`); kontrak API menyusul 27 Agustus 2026 (`RM-DEC-028`). **Tidak ada lagi yang menunggu pengesahan** |
| `module` | `rekam-medis` |
| `design_snapshot_at` | `2026-08-24` |
| `backend_commit_sha` | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |
| `frontend_commit_sha` | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| `owners` | Seluruh peran dipegang **Yoga Aji Pratama**: product/domain, clinical governance, security/privacy (`RM-DEC-027`), serta API authority dan frontend authority (`RM-DEC-028`). **Tidak ada peran yang masih `OPEN`** |
| `approved_by` | **Yoga Aji Pratama** — penanggung jawab modul |
| `approved_at` | **26 Agustus 2026** — kepemilikan API dan frontend menyusul **27 Agustus 2026** |
| `input_revisions` | `00-interview-decisions.md` revision `4`; `01-existing-capability-map.md` revision `2` |
| `input_hashes` | Decisions `sha256:2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c`; capability map `sha256:9cacecf803c0d552623a5f1ce5841af7bea7da5fc49aaf1b3142a076dd4416ae` |
| `contract_versions` | API **`0.1.1`** — `approved` 27 Agustus 2026, hash terkunci. `0.1.0` disahkan pada `RM-DEC-028`; `0.1.1` hanya menyegarkan kolom status, **bentuk kontrak tidak berubah** sehingga klien `0.1.0` tetap sah. State `0.1.0`; validation `0.1.0`; integration `0.1.0`; permission/audit `0.1.0` — keempatnya masih `draft` |
| `compatibility_impact` | **Aditif pada skema.** Lima tabel baru; **nol perubahan kolom** pada tabel yang sedang dipakai. Tiga perubahan **perilaku** pada endpoint berjalan, dirinci pada bagian "Dampak kompatibilitas" |

---

## Design gate

**Blueprint ini sudah disahkan, dan implementasi backend sudah berjalan sampai hampir tuntas.**
Pengesahan dilakukan Yoga Aji Pratama selaku penanggung jawab modul pada 26 Agustus 2026,
memegang ketiga peran owner sekaligus (`RM-DEC-027`), dilengkapi kepemilikan API dan frontend
pada 27 Agustus 2026 (`RM-DEC-028`).

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
| `00-interview-decisions.md` | `2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c` |
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
| Medical Record Access Purpose | 6 | 0 | **Belum ada.** Hanya model dan configuration; controller-nya tidak pernah dibuat |
| **Total** | **25** | **20** | 20 hidup, 6 belum ada — selisihnya karena addendum bertambah satu |

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

Bila commit SHA kedua repository berubah sebelum implementasi dimulai, jalankan
`/trace-existing-capabilities` mode impact scan pada berkas di atas sebelum blueprint dipakai.

---

## Langkah berikutnya

**Diperbarui 27 Agustus 2026.** Bagian ini pada revisi sebelumnya masih berbunyi "owner belum
menyetujui" dan menandai hampir semuanya tertahan. Itu sudah tidak benar: approval turun
26–27 Agustus (`RM-DEC-027`, `RM-DEC-028`), `/plan-module-delivery` sudah dijalankan, dan
sembilan belas task backend sudah dikerjakan.

### Yang sudah selesai

| Tahap | Keadaan |
|---|---|
| Desain dan pengesahan | Selesai. Seluruh peran owner terisi; kontrak API `0.1.1` `approved` dengan hash terkunci |
| Perencanaan | Selesai. `roadmap/backend-roadmap.md` (`BE-00`–`BE-18`) dan `roadmap/frontend-roadmap.md` (`FE-00`–`FE-09`) |
| Brief UI | Selesai 27 Agustus 2026. Entri menu dan skema tampilan per layar pada `03-frontend-architecture.md` bagian 10 dan 11 |
| Kode backend | 20 endpoint hidup di `Areas/HealthServices/MedicalRecordManagement/Controllers/`. Enam belas task `SELESAI` penuh |

### Yang tersisa di backend

| Sisa | Task | Sifatnya |
|---|---|---|
| Controller master keperluan akses | `BE-09` bagian pertama | **Pekerjaan kode yang terlewat.** Model, configuration, dan migration ada; DTO, controller, dan keenam endpoint api-contract bagian 7 **tidak pernah dibuat**. Ini yang menahan `FE-06` |
| Isi awal master keperluan akses | `BE-09` bagian kedua | Menunggu SOP rekam medis rumah sakit. Bukan pekerjaan kode |
| Penelaahan data lama | `BE-08` | Alat dan panduannya selesai; yang tersisa menjalankannya pada data nyata lalu memberi tahu unit rekam medis |
| Pemberitahuan ke penulis CPPT | `BE-15` | Bukan pekerjaan kode. Lihat `BE-15-pemberitahuan-penulis-cppt.md` |

Baris pertama perlu dibaca dua kali, karena akibatnya berantai dan tidak kelihatan dari status
`SELESAI sebagian` yang tertulis sebelumnya. Selama master keperluan akses kosong, **pembukaan
berkas pasien di luar rawatan selalu ditolak** — dan tanpa controller-nya, tidak ada cara
mengisinya lewat antarmuka. Dua penahan bertumpuk di satu tempat: SOP yang belum ada, dan
endpoint yang belum dibuat. Yang kedua dapat dikerjakan sekarang juga tanpa menunggu yang pertama.

### Yang tersisa di frontend

**Nol dari sepuluh task dikerjakan.** Tidak ada direktori `medical-record-management` di
`src/app/health-services/` maupun di `src/components/view/health-services/`.

| Urutan | Task | Keadaan |
|---|---|---|
| 1 | `FE-00` | **Pintunya.** Lapisan service dan hook; seluruh task lain bergantung padanya |
| 2 | `FE-01`, `FE-02` | Backend-nya sudah tersedia |
| 3 | `FE-03`, `FE-04`, `FE-05` | Backend-nya sudah tersedia |
| 4 | `FE-06` | **Tertahan backend** — menunggu controller master keperluan akses |
| 5 | `FE-07`, `FE-08`, `FE-09` | Menunggu task pendahulunya |

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
