# Rekam Medis Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RM-BP-001` |
| `revision` | `1` |
| `status` | **`approved`** — disahkan Yoga Aji Pratama 26 Agustus 2026, kecuali kontrak API yang masih menunggu pemilik API |
| `module` | `rekam-medis` |
| `design_snapshot_at` | `2026-08-24` |
| `backend_commit_sha` | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |
| `frontend_commit_sha` | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| `owners` | Seluruh peran dipegang **Yoga Aji Pratama**: product/domain, clinical governance, security/privacy (`RM-DEC-027`), serta API authority dan frontend authority (`RM-DEC-028`). **Tidak ada peran yang masih `OPEN`** |
| `approved_by` | **Yoga Aji Pratama** — penanggung jawab modul |
| `approved_at` | **26 Agustus 2026** |
| `input_revisions` | `00-interview-decisions.md` revision `4`; `01-existing-capability-map.md` revision `2` |
| `input_hashes` | Decisions `sha256:2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c`; capability map `sha256:9cacecf803c0d552623a5f1ce5841af7bea7da5fc49aaf1b3142a076dd4416ae` |
| `contract_versions` | API `0.1.0`; state `0.1.0`; validation `0.1.0`; integration `0.1.0`; permission/audit `0.1.0` — seluruhnya `draft` |
| `compatibility_impact` | **Aditif pada skema.** Lima tabel baru; **nol perubahan kolom** pada tabel yang sedang dipakai. Tiga perubahan **perilaku** pada endpoint berjalan, dirinci pada bagian "Dampak kompatibilitas" |

---

## Design gate

**Blueprint ini sudah disahkan dan implementasi backend boleh dimulai.** Pengesahan dilakukan
Yoga Aji Pratama selaku penanggung jawab modul pada 26 Agustus 2026, memegang ketiga peran
owner sekaligus. Rinciannya pada `RM-DEC-027`.

Keadaan gate setelah pengesahan:

| Gate | Keadaan | Yang tertahan bila belum terpenuhi |
|---|---|---|
| Product/domain owner ditunjuk | **Terpenuhi** — Yoga Aji Pratama | — |
| Clinical governance owner ditunjuk | **Terpenuhi** — Yoga Aji Pratama | Tinjauan komite medik atas `RM-DEC-003`, `004`, `020` **belum** dilakukan; lihat `RM-DEC-027` |
| Security/privacy owner ditunjuk | **Terpenuhi** — Yoga Aji Pratama | Tinjauan pihak perlindungan data atas `RM-DEC-017`, `021`, `022`, `024` **belum** dilakukan; lihat `RM-DEC-027` |
| API authority menyetujui kontrak | **Terpenuhi** 27 Agustus 2026 — Yoga Aji Pratama (`RM-DEC-028`). Kontrak `0.1.0` naik menjadi `approved`, hash dikunci | Tidak ada lagi yang tertahan. **Gerbang paralel frontend terbuka** |
| Angka masa simpan jejak akses ditetapkan | **Terpenuhi** 24 Agustus 2026 — **25 tahun** (`RM-DEC-024`) | Tidak ada lagi yang tertahan. Tabel `TrxMedicalRecordAccessLog` dirancang terbagi per tahun berdasarkan `AccessedAt`, 25 bagian pada keadaan penuh. Dasar regulasi wajib dilampirkan owner saat pengesahan |
| SOP rekam medis rumah sakit tersedia | **`OPEN`** | Isi awal `MstMedicalRecordAccessPurpose` |
| Penelusuran alur penggabungan pasien | **Selesai** 24 Agustus 2026. Hasilnya: penggabungan hanya penandaan, tidak memindahkan data klinis. Status `RM-CAP-007` naik menjadi `Conflict` | Yang tersisa: keputusan closure question nomor 8 tentang perilaku layar penelusuran untuk pasien bernomor ganda |
| Jumlah pasien bernomor rekam medis ganda di data nyata | **`OPEN`** | Menentukan apakah `BE-16` mendesak atau sekadar pengaman. Tidak dapat dijawab dari source |
| Project test backend tersedia | **`OPEN`** (`RM-CAP-032`) | Perubahan pada `RM-CAP-011`, `012`, `013` yang menyentuh kode berjalan |

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
| `03-frontend-architecture.md` | `3c2662e502668f3b57c92921d38e7f5b57ff79ac48f9a781b7b85b775c349b1f` — **revisi 2**, dihitung ulang 27 Agustus 2026 |
| `erd/00-context-erd.md` | `292c301a70004a730a1be7b93f99de9088ad5c9c62714afa52a43787202380f7` |
| `erd/keutuhan-dokumen.md` | `09cc9588dec0f464ab4a9e3a46a66d0a2a6df9d9c82f9b66c7acff827c41469d` |
| `erd/jejak-akses.md` | `f78ea5013e8ef516ea8ece969f4f95e9203212cc0a00f787e9d5e90ac1b9ec7f` |
| `erd/data-dictionary.md` | `f90106beda3faa6c2f6c635c92c11475bbf91e0a4c8223d1d7b4a10480e9f062` |
| `contracts/api-contract.md` | `a20372c4b3a6b05842e733206d13b7599895b127a2c638f5533b2004e626bed8` |
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

| Grup | Jumlah endpoint | Status |
|---|---:|---|
| Medical Record | 5 | Rencana |
| Clinical Document Integrity | 4 | Rencana |
| Clinical Note Addendum | 3 | Rencana |
| Clinical Note Author Delegation | 3 | Rencana |
| Medical Record Access Log | 4 | Rencana |
| Medical Record Access Purpose | 6 | Rencana |
| **Total endpoint baru** | **25** | Seluruhnya `Rencana (belum tersedia)` |

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

Menurut tabel skill, langkah setelah desain adalah `/plan-module-delivery` — **dengan syarat
owner sudah menyetujui blueprint dan kontraknya.** Syarat itu belum terpenuhi.

Yang tertahan dan yang tidak:

| Bagian | Dapat dilanjutkan? |
|---|---|
| Penyusunan urutan kerja | Ya, tetapi hasilnya ikut berstatus `draft` |
| Implementasi tabel baru | Seluruhnya tertahan approval owner. `RM-DEC-024` sudah tertutup, sehingga `TrxMedicalRecordAccessLog` tidak lagi punya hambatan teknis |
| Perubahan pada kode berjalan | Tertahan sampai project test backend tersedia dan owner menyetujui |
| Migration pengisian data lama | Tertahan approval clinical governance owner dan penelusuran data nyata |
